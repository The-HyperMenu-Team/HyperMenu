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
    /// Rollup of every report written this run, as
    /// <c>&lt;GameRoot&gt;/HyperMenu/SessionErr_MM_dd_yyyy_HH_mm_ss.txt</c> - one file per
    /// game launch. <see cref="SessionStart"/> is a static field initialiser, so it is captured
    /// the first time ErrorReporter is touched (at mod startup), which is what makes the
    /// filename the launch time rather than the time of the first error. The file itself is
    /// only created once there is something to write, so a clean run leaves no file behind.
    /// </summary>
    private static readonly DateTime SessionStart = DateTime.Now;

    private static readonly object _sessionLock = new();
    private static string _sessionErrPath;

    /// <summary>Full path of this session's rollup file, or null if it cannot be resolved.</summary>
    public static string SessionErrPath
    {
        get
        {
            lock (_sessionLock)
            {
                if (_sessionErrPath == null)
                {
                    try
                    {
                        _sessionErrPath = Path.Combine(BaseDir,
                            "SessionErr_" + SessionStart.ToString("MM_dd_yyyy_HH_mm_ss") + ".txt");
                    }
                    catch { return null; }
                }
                return _sessionErrPath;
            }
        }
    }

    private const string SessionDivider =
        "--------------------------------------------------------------------------------";

    /// <summary>
    /// Appends one report plus a divider to this session's SessionErr_*.txt rollup.
    /// Never throws: failing here must not cost us the report we just built, and must never
    /// propagate back out of the catch block that called <see cref="Report(Exception, int, string, string)"/>.
    /// </summary>
    private static readonly System.Collections.Generic.HashSet<string> _sessionSeen = new();

    // ---------------------------------------------------------------- uploaded-error ledger
    // Signatures of errors already shipped to BugSplat. Persisted to UploadedErrors.json so that
    // quitting and relaunching does not re-send them. Hand-rolled JSON on purpose: the project
    // has no JSON dependency and UpdateCheck.cs already parses JSON by hand, so this keeps
    // that pattern rather than pulling in another assembly under IL2CPP.
    private static readonly System.Collections.Generic.HashSet<string> _uploadedSignatures =
        new System.Collections.Generic.HashSet<string>(StringComparer.Ordinal);
    private static bool _uploadedDirty;

    /// <summary>Path of the JSON ledger of already-uploaded error signatures.</summary>
    public static string UploadedLogPath => Path.Combine(BaseDir, "UploadedErrors.json");

    /// <summary>How many distinct error signatures have been uploaded to BugSplat.</summary>
    public static int UploadedCount
    {
        get { lock (_lock) { return _uploadedSignatures.Count; } }
    }

    /// <summary>True when at least one retained error has not been uploaded yet.</summary>
    public static bool HasUnuploadedErrors
    {
        get
        {
            try
            {
                RetainedError[] pending = SnapshotRetainedErrors();
                if (pending == null) return false;
                lock (_lock)
                {
                    for (int i = 0; i < pending.Length; i++)
                    {
                        if (pending[i] == null) continue;
                        if (!_uploadedSignatures.Contains(pending[i].Signature ?? string.Empty)) return true;
                    }
                }
            }
            catch { }
            return false;
        }
    }

    /// <summary>
    /// Records the given errors as uploaded and persists the ledger. The file is only rewritten
    /// when something was actually added, so quitting with no new errors leaves it untouched.
    /// </summary>
    public static void MarkUploaded(System.Collections.Generic.IEnumerable<RetainedError> items)
    {
        try
        {
            if (items == null) return;
            bool added = false;
            lock (_lock)
            {
                foreach (RetainedError item in items)
                {
                    if (item == null) continue;
                    string sig = item.Signature ?? string.Empty;
                    if (sig.Length == 0) continue;
                    if (_uploadedSignatures.Add(sig)) added = true;
                }
                if (added) _uploadedDirty = true;
            }
            if (added) SaveUploadedLog();
        }
        catch { }
    }

    private static void LoadUploadedLog()
    {
        try
        {
            string path = UploadedLogPath;
            if (!File.Exists(path)) return;
            string[] lines = File.ReadAllLines(path);
            lock (_lock)
            {
                foreach (string raw in lines)
                {
                    string line = (raw ?? string.Empty).Trim();
                    if (line.Length < 2) continue;
                    if (line[0] != '"' || line[line.Length - 1] != '"') continue;
                    string decoded = UnescapeJson(line.Substring(1, line.Length - 2));
                    if (decoded.Length > 0) _uploadedSignatures.Add(decoded);
                }
                _uploadedDirty = false;
            }
        }
        catch { }
    }

    private static void SaveUploadedLog()
    {
        try
        {
            System.Collections.Generic.List<string> ordered;
            lock (_lock)
            {
                if (!_uploadedDirty) return;   // nothing new - leave the file exactly as it is
                ordered = new System.Collections.Generic.List<string>(_uploadedSignatures);
                _uploadedDirty = false;
            }
            StringBuilder sb = new StringBuilder();
            sb.Append("{\n  \"version\": 1,\n");
            sb.Append("  \"updatedUtc\": \"").Append(DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")).Append("\",\n");
            sb.Append("  \"signatures\": [\n");
            for (int i = 0; i < ordered.Count; i++)
            {
                sb.Append("    \"").Append(EscapeJson(ordered[i])).Append('"');
                if (i < ordered.Count - 1) sb.Append(',');
                sb.Append('\n');
            }
            sb.Append("  ]\n}\n");
            File.WriteAllText(UploadedLogPath, sb.ToString());
        }
        catch { }
    }

    private static string EscapeJson(string value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }

    private static string UnescapeJson(string value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        return value.Replace("\\\"", "\"").Replace("\\\\", "\\");
    }

    /// <summary>Report file already written for a given signature this session, and how many
    /// times it has been seen since. Keeps ErrorReports/ at one file per distinct fault.</summary>
    private static readonly System.Collections.Generic.Dictionary<string, string> _fileBySignature = new();
    private static readonly System.Collections.Generic.Dictionary<string, int> _occurrencesBySignature = new();

    private static void AppendToSessionLog(string body, string signature)
    {
        try
        {
            string target = SessionErrPath;
            if (string.IsNullOrEmpty(target)) return;

            bool repeat;
            lock (_lock) { repeat = signature != null && !_sessionSeen.Add(signature); }

            if (repeat)
            {
                // Already written once this session. The file stays chronological, but a
                // repeat costs one line instead of another full copy of the same report.
                File.AppendAllText(target,
                    "   ... repeat of the identical error above" + Environment.NewLine);
                return;
            }

            File.AppendAllText(target,
                (body ?? "") + Environment.NewLine + SessionDivider + Environment.NewLine);
        }
        catch { }
    }

    /// <summary>
    /// Creates the HyperMenu/ErrorReports/ConsoleLogs folders. Call from MalumMenu.Load
    /// at mod startup, before <see cref="Initialize"/>. Never throws.
    /// </summary>
    /// <summary>One error kept in memory so it can be uploaded to BugSplat later.</summary>
    public sealed class RetainedError
    {
        public Exception Exception;
        public string PlainMessage;
        public int HandlingId;
        public string Context;
        public DateTime When;
        public DateTime LastSeen;
        /// <summary>How many times this exact fault was reported this session.</summary>
        public int Count;
        /// <summary>Identity of the fault, used to collapse repeats. See <see cref="Signature"/>.</summary>
        public string Signature;
    }

    private const int MaxRetainedErrors = 50;
    private static readonly System.Collections.Generic.List<RetainedError> _retained = new();
    private static readonly System.Collections.Generic.Dictionary<string, RetainedError> _retainedBySignature = new();

    /// <summary>How many DISTINCT errors are waiting to be uploaded this session.</summary>
    public static int RetainedErrorCount
    {
        get { lock (_lock) { return _retained.Count; } }
    }

    /// <summary>Total occurrences this session, counting repeats of an already-seen fault.</summary>
    public static int RetainedOccurrenceCount
    {
        get
        {
            lock (_lock)
            {
                int n = 0;
                foreach (var e in _retained) n += e.Count;
                return n;
            }
        }
    }

    /// <summary>Snapshot of the DISTINCT retained errors, noisiest first.</summary>
    /// <summary>
    /// Returns the ErrorReports/HyperError_*.txt file written for this specific retained
    /// error, or null when none was written (the report file could not be created, or this
    /// error's signature is unknown).
    /// </summary>
    public static string ReportPathFor(RetainedError item)
    {
        if (item == null || string.IsNullOrEmpty(item.Signature)) return null;
        lock (_lock)
        {
            return _fileBySignature.TryGetValue(item.Signature, out string path) ? path : null;
        }
    }

    public static RetainedError[] SnapshotRetainedErrors()
    {
        lock (_lock)
        {
            var copy = new System.Collections.Generic.List<RetainedError>(_retained);
            copy.Sort(delegate (RetainedError a, RetainedError b)
            {
                int c = b.Count.CompareTo(a.Count);
                if (c != 0) return c;
                return a.When.CompareTo(b.When);
            });
            return copy.ToArray();
        }
    }

    /// <summary>
    /// Identity of a fault, so that the same thing reported every frame collapses into one
    /// entry instead of one entry per frame. Built from the exception type, the handling id,
    /// the call site context, the innermost stack frame, and the message with all digit runs
    /// collapsed - ids, counts, coordinates and timestamps vary between occurrences of one
    /// bug and would otherwise split it into many.
    /// </summary>
    private static string Signature(Exception ex, string plainMessage, int handlingId, string context)
    {
        try
        {
            string type = ex != null ? ex.GetType().FullName : "System.Exception";

            string where = "";
            try
            {
                string st = ex != null ? ex.StackTrace : null;
                if (!string.IsNullOrEmpty(st))
                {
                    int nl = st.IndexOf('\n');
                    where = (nl < 0 ? st : st.Substring(0, nl)).Trim();
                }
            }
            catch { }

            return type + "|" + handlingId + "|" + (context ?? "") + "|"
                 + where + "|" + CollapseDigits(ex != null ? ex.Message : plainMessage);
        }
        catch { return "fallback|" + handlingId; }
    }

    private static string CollapseDigits(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        var sb = new System.Text.StringBuilder(s.Length);
        bool inRun = false;
        for (int i = 0; i < s.Length; i++)
        {
            if (char.IsDigit(s[i]))
            {
                if (!inRun) { sb.Append('#'); inRun = true; }
            }
            else { sb.Append(s[i]); inRun = false; }
        }
        return sb.ToString();
    }

    // Keeps the Exception object itself, not just its text, because BugSplat is posted the
    // live exception so the server sees the real type, message and stack trace. Bounded so a
    // long session with a recurring fault cannot grow this without limit.
    private static void Retain(Exception ex, string plainMessage, int handlingId, string context,
                               string signature)
    {
        try
        {
            lock (_lock)
            {
                // One entry per distinct fault. A fault that fires every frame must not turn
                // into dozens of identical uploads and dozens of rollup entries; repeats only
                // bump the counter and the last-seen time.
                if (signature != null && _retainedBySignature.TryGetValue(signature, out var seen))
                {
                    seen.Count++;
                    seen.LastSeen = DateTime.Now;
                    return;
                }

                var entry = new RetainedError
                {
                    Exception = ex ?? new Exception(plainMessage ?? "Unknown error"),
                    PlainMessage = plainMessage,
                    HandlingId = handlingId,
                    Context = context,
                    When = DateTime.Now,
                    LastSeen = DateTime.Now,
                    Count = 1,
                    Signature = signature
                };
                _retained.Add(entry);
                if (signature != null) _retainedBySignature[signature] = entry;

                // Bounded: drop the oldest distinct fault once we exceed the cap.
                while (_retained.Count > MaxRetainedErrors)
                {
                    var oldest = _retained[0];
                    _retained.RemoveAt(0);
                    if (oldest.Signature != null) _retainedBySignature.Remove(oldest.Signature);
                }
            }
        }
        catch { }
    }

    public static void EnsureDirectories()
    {
        try { Directory.CreateDirectory(BaseDir); } catch { }
        try { LoadUploadedLog(); } catch { }
        try { Directory.CreateDirectory(ReportDir); } catch { }
        try { Directory.CreateDirectory(ConsoleLogsDir); } catch { }
        // Resolving the path here triggers the SessionStart initialiser, so the rollup
        // filename reflects the launch time even if the first error is much later.
        try { string pinned = SessionErrPath; } catch { }
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

        // Rollup happens before the per-report file is attempted, so the session log still
        // captures the report if ErrorReports/ itself cannot be written.
        string signature = Signature(ex, plainMessage, handlingId, context);

        AppendToSessionLog(body, signature);

        // Retained after the storm throttle above, so only errors that produced a real report
        // are ever queued for upload.
        Retain(ex, plainMessage, handlingId, context, signature);

        string path = null;
        try
        {
            string existing = null;
            lock (_lock) { _fileBySignature.TryGetValue(signature, out existing); }

            if (!string.IsNullOrEmpty(existing) && File.Exists(existing))
            {
                // The same fault again this session. Add a single occurrence line to the file
                // that already describes it rather than writing a byte-identical copy, and
                // stay quiet so a per-frame fault cannot spam notifications either.
                int seen;
                lock (_lock) { _occurrencesBySignature.TryGetValue(signature, out seen); seen++; _occurrencesBySignature[signature] = seen; }
                File.AppendAllText(existing,
                    "   ... occurrence " + seen + " (identical error, full report above)" + Environment.NewLine);
                return existing;
            }

            Directory.CreateDirectory(ReportDir);
            string stamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string unique = Guid.NewGuid().ToString("N").Substring(0, 4);
            path = Path.Combine(ReportDir, "HyperError_" + stamp + "_" + FormatId(handlingId) + "_" + unique + ".txt");
            File.WriteAllText(path, body);
            lock (_lock) { _fileBySignature[signature] = path; _occurrencesBySignature[signature] = 1; }
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
            var viaScan = GetInstalledPluginDlls();
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

    /// <summary>
    /// Lists the plugin DLLs sitting in the BepInEx plugins folder, which is the thing an
    /// error report actually wants to show: which mod files are installed on this machine.
    ///
    /// It deliberately does NOT enumerate the types inside those assemblies. Under BepInEx
    /// IL2CPP that path reaches RuntimeModule type enumeration, which aborts the CLR outright
    /// with "Fatal error. Internal CLR error. (0x80131506)". That is a runtime abort rather than
    /// a managed exception, so neither a try/catch nor the Safe() wrapper can contain it, and a
    /// single stray exception anywhere in the UI would take the whole game down. Reading the
    /// directory and reading an assembly's name are both safe. Never throws.
    /// </summary>
    private static System.Collections.Generic.List<string> GetInstalledPluginDlls()
    {
        var lines = new System.Collections.Generic.List<string>();

        // Names of the assemblies BepInEx actually loaded, so each file can be marked.
        var loaded = new System.Collections.Generic.HashSet<string>(StringComparer.OrdinalIgnoreCase);
        try
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    string n = asm.GetName().Name;
                    if (!string.IsNullOrEmpty(n))
                    {
                        loaded.Add(n);
                    }
                }
                catch { }
            }
        }
        catch { }

        try
        {
            string dir = Path.Combine(Paths.GameRootPath, "BepInEx", "plugins");
            if (Directory.Exists(dir))
            {
                string[] files = Directory.GetFiles(dir, "*.dll", SearchOption.TopDirectoryOnly);
                System.Array.Sort(files, StringComparer.OrdinalIgnoreCase);
                foreach (string f in files)
                {
                    string file = Path.GetFileNameWithoutExtension(f);
                    lines.Add("      - " + file + (loaded.Contains(file) ? "  (loaded)" : "  (not loaded)"));
                }

                if (lines.Count > 0)
                {
                    return lines;
                }
            }
        }
        catch { }

        // The plugins folder was unreadable or empty - fall back to whatever is loaded.
        foreach (string n in loaded)
        {
            lines.Add("      - " + n + "  (loaded)");
        }

        if (lines.Count == 0)
        {
            lines.Add("      (none found)");
        }

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
