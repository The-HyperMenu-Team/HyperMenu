using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MalumMenu;

/// <summary>
/// Central error-reporting system for HyperMenu.
/// Call <see cref="Report(Exception, int, string, string)"/> from any catch block,
/// passing the owning file's 5-digit handling ID. Reports are saved as text files
/// under <c>&lt;GameRoot&gt;/HyperMenu/ErrorReports</c>. This class never throws.
/// </summary>
public static class ErrorReporter
{
    /// <summary>Reserved handling ID for unhandled/global errors with no owning file.</summary>
    public const int GlobalHandlingId = 0;

    private static readonly object _lock = new();
    private static bool _initialized;
    private static bool _isReporting; // re-entrancy guard (e.g. log callbacks firing while we report)

    // Rate limiter: caps file-writing during exception storms (e.g. a per-frame
    // callback throwing every frame) so ErrorReports can't fill the disk.
    private static readonly System.Collections.Generic.Queue<DateTime> _reportTimes = new();
    private const int MaxReportsPerMinute = 20;

    private static bool RateLimitExceeded()
    {
        DateTime now = DateTime.UtcNow;
        while (_reportTimes.Count > 0 && (now - _reportTimes.Peek()).TotalMinutes >= 1)
            _reportTimes.Dequeue();
        if (_reportTimes.Count >= MaxReportsPerMinute) return true;
        _reportTimes.Enqueue(now);
        return false;
    }

    private static string BaseDir
    {
        get
        {
            try { return Path.Combine(Paths.GameRootPath, "HyperMenu"); }
            catch { return Path.Combine(Directory.GetCurrentDirectory(), "HyperMenu"); }
        }
    }

    private static string ReportDir => Path.Combine(BaseDir, "ErrorReports");

    /// <summary>Console-log directory: &lt;GameRoot&gt;/HyperMenu/ConsoleLogs. Never throws.</summary>
    public static string ConsoleLogsDir
    {
        get
        {
            try { return Path.Combine(BaseDir, "ConsoleLogs"); }
            catch { return Path.Combine(Directory.GetCurrentDirectory(), "HyperMenu", "ConsoleLogs"); }
        }
    }

    /// <summary>
    /// Creates the HyperMenu/ErrorReports/ConsoleLogs folders. Call from MalumMenu.Load
    /// at mod startup, before <see cref="Initialize"/>. Never throws.
    /// </summary>
    public static void EnsureDirectories()
    {
        try { Directory.CreateDirectory(BaseDir); } catch { }
        try { Directory.CreateDirectory(ReportDir); } catch { }
        try { Directory.CreateDirectory(ConsoleLogsDir); } catch { }
    }

    /// <summary>
    /// Subscribes global handlers. Call from MalumMenu.Load at mod startup
    /// (after <see cref="EnsureDirectories"/>). Safe to call multiple times.
    /// Must be called from the main thread. Never throws.
    /// </summary>
    public static void Initialize()
    {
        lock (_lock)
        {
            if (_initialized) return;
            _initialized = true;
        }

        try { AppDomain.CurrentDomain.UnhandledException += OnUnhandledException; } catch { }
        try { TaskScheduler.UnobservedTaskException += OnUnobservedTaskException; } catch { }
        // Note: no Unity Application.logMessageReceived hook — the Unity Scripting
        // surface exposed to this mod does not expose it, so Unity-thread exceptions
        // are covered by AppDomain.UnhandledException plus explicit Report() calls.
    }

    /// <summary>Saves an error report for an exception. Returns the file path, or null on failure.</summary>
    public static string Report(Exception ex, int handlingId, string context = null,
        [CallerFilePath] string callerFilePath = null)
    {
        if (ex == null) return null;
        lock (_lock)
        {
            if (_isReporting) return null; // avoid recursion (e.g. Unity log callback -> Report -> log -> ...)
            _isReporting = true;
            try { return WriteReport(ex, null, handlingId, context, callerFilePath); }
            catch { return null; }
            finally { _isReporting = false; }
        }
    }

    /// <summary>Saves an error report for a non-exception error message. Returns the file path, or null on failure.</summary>
    public static string Report(string message, int handlingId, string context = null,
        [CallerFilePath] string callerFilePath = null)
    {
        if (string.IsNullOrEmpty(message)) return null;
        lock (_lock)
        {
            if (_isReporting) return null;
            _isReporting = true;
            try { return WriteReport(null, message, handlingId, context, callerFilePath); }
            catch { return null; }
            finally { _isReporting = false; }
        }
    }

    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        try
        {
            var ex = e.ExceptionObject as Exception
                ?? new Exception("Unhandled non-Exception object: " + (e.ExceptionObject?.ToString() ?? "null"));
            Report(ex, GlobalHandlingId, "AppDomain.UnhandledException (runtimeTerminating=" + e.IsTerminating + ")");
        }
        catch { }
    }

    private static void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
    {
        try
        {
            Report(e.Exception, GlobalHandlingId, "TaskScheduler.UnobservedTaskException");
            e.SetObserved();
        }
        catch { }
    }

    /// <summary>Runs an action, saving an error report if it throws. Never throws itself.</summary>
    public static void Guard(Action action, int handlingId, string context = null,
        [CallerFilePath] string callerFilePath = null)
    {
        try { action(); }
        catch (Exception ex) { Report(ex, handlingId, context, callerFilePath); }
    }

    /// <summary>Runs a function, saving an error report and returning <paramref name="fallback"/> if it throws. Never throws itself.</summary>
    public static T Guard<T>(Func<T> func, T fallback, int handlingId, string context = null,
        [CallerFilePath] string callerFilePath = null)
    {
        try { return func(); }
        catch (Exception ex) { Report(ex, handlingId, context, callerFilePath); return fallback; }
    }

    /// <summary>
    /// Wraps a mod-owned coroutine so exceptions at any yield step are reported.
    /// Use as <c>StartCoroutine(ErrorReporter.GuardCoroutine(MyRoutine(), HandlingId, "MyRoutine"))</c>.
    /// </summary>
    public static IEnumerator GuardCoroutine(IEnumerator inner, int handlingId, string context = null,
        [CallerFilePath] string callerFilePath = null)
    {
        if (inner == null) yield break;
        while (true)
        {
            bool moved;
            try { moved = inner.MoveNext(); }
            catch (Exception ex) { Report(ex, handlingId, "coroutine: " + context, callerFilePath); yield break; }
            if (!moved) yield break;
            yield return inner.Current;
        }
    }

    private static string WriteReport(Exception ex, string plainMessage, int handlingId, string context, string callerFilePath)
    {
        // Storm protection: still log a one-liner, but skip the file + popup.
        if (RateLimitExceeded())
        {
            try
            {
                string oneLine = ex != null ? ex.GetType().Name + ": " + ex.Message : plainMessage;
                MalumMenu.Log?.LogError("[HyperMenu] Error storm — report throttled (ID " + FormatId(handlingId) + "): " + oneLine);
            }
            catch { }
            return null;
        }

        string body;
        try { body = BuildBody(ex, plainMessage, handlingId, context, callerFilePath); }
        catch (Exception buildEx)
        {
            // Last-resort minimal body if context gathering itself failed.
            body = "HyperMenu Error Report (context gathering failed: " + buildEx.Message + ")\n"
                + "Timestamp: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\n"
                + "Handling ID: " + FormatId(handlingId) + "\n"
                + "Error: " + (ex != null ? ex.ToString() : plainMessage) + "\n";
        }

        string path = null;
        try
        {
            Directory.CreateDirectory(ReportDir);
            string stamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string unique = Guid.NewGuid().ToString("N").Substring(0, 4);
            path = Path.Combine(ReportDir, "HyperError_" + stamp + "_" + FormatId(handlingId) + "_" + unique + ".txt");
            File.WriteAllText(path, body);
        }
        catch { return null; }

        Notify(handlingId, ex, plainMessage, path);
        return path;
    }

    private static void Notify(int handlingId, Exception ex, string plainMessage, string path)
    {
        string oneLine = ex != null
            ? ex.GetType().Name + ": " + Truncate(ex.Message, 160)
            : Truncate(plainMessage, 160);
        try { MalumMenu.Log?.LogError("[HyperMenu] Error (ID " + FormatId(handlingId) + ") saved to " + path + " :: " + oneLine); }
        catch { }
        try { ConsoleUI.Log("[HyperMenu] Error (ID " + FormatId(handlingId) + ") saved to " + path + " :: " + oneLine); }
        catch { }
        try
        {
            if (MalumMenu.notifications != null)
                MalumMenu.notifications.Send("HyperMenu Error",
                    "Something went wrong (ID " + FormatId(handlingId) + ").\nA report was saved to HyperMenu/ErrorReports.\n" + oneLine);
        }
        catch { }
    }

    private static string FormatId(int id)
    {
        if (id < 0 || id > 99999) return "INVALID-" + id;
        return id.ToString("D5");
    }

    private static string Truncate(string s, int max)
    {
        if (string.IsNullOrEmpty(s)) return s ?? "";
        return s.Length <= max ? s : s.Substring(0, max) + "...";
    }

    private static string Safe(Func<string> fn)
    {
        try
        {
            string v = fn();
            return string.IsNullOrEmpty(v) ? "unknown" : v;
        }
        catch { return "unknown"; }
    }

    private static string BuildBody(Exception ex, string plainMessage, int handlingId, string context, string callerFilePath)
    {
        var sb = new StringBuilder();
        DateTime now = DateTime.Now;

        sb.AppendLine("===== HyperMenu Error Report =====");
        sb.AppendLine();

        // 1. Time and date
        sb.AppendLine("1. Timestamp (local): " + now.ToString("yyyy-MM-dd HH:mm:ss.fff zzz"));
        sb.AppendLine("   Timestamp (UTC)  : " + now.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
        sb.AppendLine();

        // 2. Handling ID
        sb.Append("2. Handling ID: " + FormatId(handlingId));
        string fileName = null;
        try { fileName = !string.IsNullOrEmpty(callerFilePath) ? Path.GetFileName(callerFilePath) : null; } catch { }
        if (!string.IsNullOrEmpty(fileName)) sb.Append(" (reported from: " + fileName + ")");
        sb.AppendLine();
        sb.AppendLine("   Registry: see src/Utilities/HandlingIds.cs (" +
            (HandlingIds.IsValid(handlingId) ? "registered" : "UNREGISTERED ID — add it to HandlingIds.cs") + ")");
        sb.AppendLine();

        // 3. Traceback
        sb.AppendLine("3. Traceback:");
        if (ex != null)
        {
            sb.AppendLine(Safe(() => ex.ToString()));
            sb.AppendLine("--- Current-thread stack (at report time) ---");
            sb.AppendLine(Safe(() => Environment.StackTrace));
        }
        else
        {
            sb.AppendLine("(no exception object — message-only report)");
            sb.AppendLine(Safe(() => Environment.StackTrace));
        }
        sb.AppendLine();

        // 4. Exact error
        sb.AppendLine("4. Exact error:");
        if (ex != null)
        {
            sb.AppendLine("   Type: " + Safe(() => ex.GetType().FullName));
            sb.AppendLine("   Message: " + Safe(() => ex.Message));
            sb.AppendLine("   HResult: " + Safe(() => "0x" + ex.HResult.ToString("X8")));
            sb.AppendLine("   Source: " + Safe(() => ex.Source));
            sb.AppendLine("   TargetSite: " + Safe(() => ex.TargetSite?.ToString()));
            if (ex.InnerException != null)
            {
                sb.AppendLine("   InnerException Type: " + Safe(() => ex.InnerException.GetType().FullName));
                sb.AppendLine("   InnerException Message: " + Safe(() => ex.InnerException.Message));
            }
        }
        else
        {
            sb.AppendLine("   Message: " + plainMessage);
        }
        sb.AppendLine();

        // 5. Among Us version
        sb.AppendLine("5. Among Us version: " + Safe(() => Application.version));
        sb.AppendLine("   Unity version: " + Safe(() => Application.unityVersion));
        sb.AppendLine();

        // 6. Mod version (from MalumMenu.cs)
        sb.AppendLine("6. Mod version: Hyper " + Safe(() => MalumMenu.hyperVersion)
            + " (" + Safe(() => MalumMenu.hyperBuild) + "), Malum base " + Safe(() => MalumMenu.malumVersion));
        sb.AppendLine();

        // 7. Other important info (full context bundle)
        sb.AppendLine("7. Other info:");
        sb.AppendLine("   Context: " + (string.IsNullOrEmpty(context) ? "(none provided)" : context));
        sb.AppendLine("   Scene: " + Safe(() => SceneManager.GetActiveScene().name));
        sb.AppendLine("   Map ID: " + Safe(() => Utils.GetCurrentMapID().ToString()));
        sb.AppendLine("   GameState: " + Safe(() => AmongUsClient.Instance.GameState.ToString()));
        sb.AppendLine("   NetworkMode: " + Safe(() => AmongUsClient.Instance.NetworkMode.ToString()));
        sb.AppendLine("   AmHost: " + Safe(() => AmongUsClient.Instance.AmHost.ToString()));
        sb.AppendLine("   FPS: " + Safe(() => Utils.GetFps().ToString()));
        sb.AppendLine("   Ping: " + Safe(() => Utils.GetPing().ToString()) + " ms");
        sb.AppendLine("   Supported AU: [" + Safe(() => string.Join(", ", MalumMenu.supportedAU)) + "]");
        sb.AppendLine("   Tolerated AU: [" + Safe(() => string.Join(", ", MalumMenu.toleratedAU)) + "]");
        sb.AppendLine("   OS: " + Safe(GetOSInfo));
        sb.AppendLine("   BepInEx plugins:");
        sb.AppendLine(Safe(GetPluginList));
        sb.AppendLine("   Recent console log (last 20):");
        sb.AppendLine(Safe(GetRecentConsole));

        return sb.ToString();
    }

    private static string GetOSInfo()
    {
        // Unity reports the emulated OS under Wine/Proton (e.g. "Windows 10 ..."),
        // so also capture the .NET view plus best-effort Wine/host indicators.
        string unityOS = Safe(() => SystemInfo.operatingSystem);
        string dotnetOS = Safe(() => Environment.OSVersion.ToString());
        string frameworkOS = Safe(() => System.Runtime.InteropServices.RuntimeInformation.OSDescription?.Trim());
        string wine = "no";
        try
        {
            string prefix = Environment.GetEnvironmentVariable("WINEPREFIX");
            string loader = Environment.GetEnvironmentVariable("WINELOADER");
            string arch = Environment.GetEnvironmentVariable("WINEARCH");
            string server = Environment.GetEnvironmentVariable("WINESERVER");
            if (!string.IsNullOrEmpty(prefix) || !string.IsNullOrEmpty(loader)
                || !string.IsNullOrEmpty(arch) || !string.IsNullOrEmpty(server))
            {
                wine = "yes (WINEPREFIX=" + (prefix ?? "?") + " WINEARCH=" + (arch ?? "?") + ")";
            }
            else
            {
                // When the Unix root is visible through Wine, /etc/os-release reveals the host distro.
                string host = TryReadFirstLine("/etc/os-release") ?? TryReadFirstLine(@"Z:\etc\os-release");
                if (host != null) wine = "likely (host: " + host + ")";
            }
        }
        catch { }
        return unityOS + " | .NET: " + dotnetOS + " | Runtime: " + frameworkOS + " | Wine: " + wine;
    }

    private static string TryReadFirstLine(string path)
    {
        try
        {
            if (!File.Exists(path)) return null;
            using (var sr = new StreamReader(path))
            {
                string line = sr.ReadLine();
                // Prefer PRETTY_NAME= if present.
                if (line != null && line.StartsWith("PRETTY_NAME="))
                    return line.Trim();
                string name = null;
                try
                {
                    sr.BaseStream.Seek(0, SeekOrigin.Begin);
                    sr.DiscardBufferedData();
                    string content = sr.ReadToEnd();
                    foreach (var l in content.Split('\n'))
                    {
                        if (l.TrimStart().StartsWith("PRETTY_NAME=")) { name = l.Trim(); break; }
                    }
                }
                catch { }
                return name ?? line?.Trim();
            }
        }
        catch { return null; }
    }

    private static string GetPluginList()
    {
        // Primary: BepInEx.Bootstrap.Chainloader.PluginInfos via reflection (no hard
        // compile dependency). Fallback: scan loaded types for [BepInPlugin] classes
        // (the attribute sits on the plugin class, not the assembly).
        try
        {
            var viaChainloader = TryGetPluginListFromChainloader();
            if (viaChainloader != null && viaChainloader.Count > 0)
            {
                viaChainloader.Sort();
                return string.Join("\n", viaChainloader);
            }
            var viaScan = TryGetPluginListFromTypeScan();
            if (viaScan != null && viaScan.Count > 0)
            {
                viaScan.Sort();
                return string.Join("\n", viaScan);
            }
            return "      (none / unavailable)";
        }
        catch (Exception e) { return "(unavailable: " + e.Message + ")"; }
    }

    private static System.Collections.Generic.List<string> TryGetPluginListFromChainloader()
    {
        var lines = new System.Collections.Generic.List<string>();
        try
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                System.Type chainloaderType = null;
                try { chainloaderType = asm.GetType("BepInEx.Bootstrap.Chainloader"); } catch { }
                if (chainloaderType == null) continue;
                var prop = chainloaderType.GetProperty("PluginInfos",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                if (prop == null) continue;
                var dict = prop.GetValue(null) as System.Collections.IDictionary;
                if (dict == null) continue;
                foreach (System.Collections.DictionaryEntry entry in dict)
                {
                    try
                    {
                        object pluginInfo = entry.Value;
                        object metadata = pluginInfo?.GetType().GetProperty("Metadata")?.GetValue(pluginInfo);
                        string name = metadata?.GetType().GetProperty("Name")?.GetValue(metadata)?.ToString() ?? "?";
                        string guid = metadata?.GetType().GetProperty("GUID")?.GetValue(metadata)?.ToString()
                            ?? entry.Key?.ToString() ?? "?";
                        string version = metadata?.GetType().GetProperty("Version")?.GetValue(metadata)?.ToString() ?? "?";
                        lines.Add("      - " + name + " [" + guid + "] v" + version);
                    }
                    catch { }
                }
                if (lines.Count > 0) return lines;
            }
        }
        catch { }
        return lines;
    }

    private static System.Collections.Generic.List<string> TryGetPluginListFromTypeScan()
    {
        var lines = new System.Collections.Generic.List<string>();
        var seen = new System.Collections.Generic.HashSet<string>();
        try
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                System.Type[] types;
                try { types = asm.GetTypes(); }
                catch (System.Reflection.ReflectionTypeLoadException rtle)
                {
                    try { types = rtle.Types; } catch { continue; }
                }
                catch { continue; }
                if (types == null) continue;
                foreach (var t in types)
                {
                    try
                    {
                        if (t == null) continue;
                        foreach (var attr in t.GetCustomAttributes(false))
                        {
                            if (attr == null || !attr.GetType().Name.StartsWith("BepInPlugin")) continue;
                            string guid = Safe(() => attr.GetType().GetProperty("GUID")?.GetValue(attr)?.ToString());
                            string name = Safe(() => attr.GetType().GetProperty("Name")?.GetValue(attr)?.ToString());
                            string version = Safe(() => attr.GetType().GetProperty("Version")?.GetValue(attr)?.ToString());
                            string key = asm.GetName().Name + "|" + guid;
                            if (seen.Add(key))
                                lines.Add("      - " + (string.IsNullOrEmpty(name) || name == "unknown" ? asm.GetName().Name : name)
                                    + " [" + guid + "] v" + version);
                            break;
                        }
                    }
                    catch { }
                }
            }
        }
        catch { }
        return lines;
    }

    private static string GetRecentConsole()
    {
        try
        {
            var lines = ConsoleUI.GetRecentEntries(20);
            if (lines == null || lines.Length == 0) return "      (empty)";
            return string.Join("\n", lines.Select(l => "      | " + l));
        }
        catch (Exception e) { return "(unavailable: " + e.Message + ")"; }
    }
}
