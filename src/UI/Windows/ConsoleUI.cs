using Il2CppSystem;
using UnityEngine;
using System.Collections.Generic;

namespace MalumMenu;

public class ConsoleUI : MonoBehaviour
{
    private const int HandlingId = 10003;
    public static int windowHeight = 380;
    public static int windowWidth = 600;
    public static Rect windowRect;

    private GUIStyle _logStyle;
    private static Vector2 _scrollPosition = Vector2.zero;
    private static List<string> _logEntries = new();
    private const int MaxLogEntries = 300;
    private static readonly string _logFilePath = BuildLogFilePath();

    private static string BuildLogFilePath()
    {
        try { return System.IO.Path.Combine(ErrorReporter.ConsoleLogsDir, $"Console.{System.DateTime.Now:MM_dd_yyyy.HH_mm_ss}.log"); }
        catch { return $"HyperMenu/ConsoleLogs/Console.{System.DateTime.Now:MM_dd_yyyy.HH_mm_ss}.log"; }
    }

    private void Start()
    {
        try
        {
            // Instantiate 2D area of ConsoleUI
            windowRect = new(
                Screen.width / 2f - windowWidth / 2f,
                Screen.height / 2f - windowHeight / 2f,
                windowWidth,
                windowHeight
            );
        }
        catch (System.Exception ex) { ErrorReporter.Report(ex, HandlingId, "ConsoleUI.Start: init window rect"); }
    }

    private void OnGUI()
    {
        try
        {
            if (!CheatToggles.showConsole || !(MenuUI.isGUIActive || MalumMenu.menuKeepSubwindowsOpen.Value) || MalumMenu.isPanicked) return;

            _logStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                padding = new RectOffset { left = 4, right = 4, top = 2, bottom = 2 },
                normal = { textColor = new Color(0.95f, 0.95f, 0.95f) }
            };

            UIHelpers.ApplyUIColor();

            windowRect = GUI.Window((int)WindowId.ConsoleUI, windowRect, (GUI.WindowFunction)ConsoleWindow, "Console");
        }
        catch (System.Exception ex) { ErrorReporter.Report(ex, HandlingId, "ConsoleUI.OnGUI: draw console window"); }
    }

    private void ConsoleWindow(int windowID)
    {
        GUILayout.BeginVertical(GUI.skin.box);

        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, false);

        foreach (var log in _logEntries)
        {
            GUILayout.Label(log, _logStyle);
        }

        GUILayout.EndScrollView();

        GUILayout.EndVertical();

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Clear Log", GUILayout.Width(285)))
        {
            _logEntries.Clear();
        }

        if (GUILayout.Button("Copy to Clipboard", GUILayout.Height(30)))
        {
            GUIUtility.systemCopyBuffer = String.Join("\n", _logEntries.ToArray());
        }

        GUILayout.EndHorizontal();

        GUI.DragWindow();
    }

    private static bool _fileErrorReported; // one error report per session if console file IO stays broken

    public static void Log(string message)
    {
        if (_logEntries.Count >= MaxLogEntries) // Limit the number of logs to keep memory usage in check
        {
            _logEntries.RemoveAt(0); // Remove the oldest log entry
        }
        var currentTime = DateTime.Now.ToString("HH:mm:ss");

        // In-memory history first so reports always have console context even if file IO fails.
        _logEntries.Add($"[{currentTime}] {message}");

        try
        {
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(_logFilePath) ?? ErrorReporter.ConsoleLogsDir);
            System.IO.File.AppendAllText(_logFilePath, message + "\n");
        }
        catch (System.Exception fileEx)
        {
            // A broken console file must not stay silent: BepInEx log every time,
            // full error report once per session. Safe from infinite recursion —
            // ErrorReporter's re-entrancy guard suppresses the nested Report when
            // Log() is running inside its notify path.
            try { MalumMenu.Log?.LogError("[HyperMenu] Console log file write failed: " + fileEx.Message); } catch { }
            if (!_fileErrorReported)
            {
                _fileErrorReported = true;
                try { ErrorReporter.Report(fileEx, HandlingId, "ConsoleUI.Log: write to " + _logFilePath); } catch { }
            }
        }

        // Scroll to the bottom
        _scrollPosition.y = float.MaxValue;
    }

    /// <summary>Returns up to the last <paramref name="count"/> console entries (oldest first). Never throws.</summary>
    public static string[] GetRecentEntries(int count)
    {
        try
        {
            if (_logEntries == null || _logEntries.Count == 0) return System.Array.Empty<string>();
            int take = System.Math.Min(count, _logEntries.Count);
            return _logEntries.GetRange(_logEntries.Count - take, take).ToArray();
        }
        catch
        {
            return System.Array.Empty<string>();
        }
    }
}
