using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;
using BugSplatDotNetStandard;

namespace MalumMenu;

/// <summary>
/// Uploads this session's retained errors to a BugSplat database, one report per distinct
/// error, so they can be reviewed
/// together instead of one local .txt file at a time.
/// </summary>
public static class BugReporter
{
    internal const int HandlingId = 10013;

    private const string Database = "HyperMenu";
    private const string Application = "HyperMenu";

    // The upload runs on the calling (Unity main) thread via Post(...).Wait(timeout), so the
    // bound is what stops a stalled upload from freezing the game indefinitely.
    private const int PostTimeoutSeconds = 20;

    // Closing the game is a much tighter window than a button press, so the automatic
    // on-quit upload gets a shorter bound: a stalled request must not hold up shutdown.
    private const int QuitPostTimeoutSeconds = 6;

    private static bool _posting;

    /// <summary>When the last successful upload happened. Used so closing the game does not
    /// re-send errors that were already reported by hand.</summary>
    // Which errors have already been shipped is tracked by ErrorReporter in
    // HyperMenu/UploadedErrors.json, not by a timestamp here, so it survives a relaunch.

    /// <summary>Errors recorded this session and not yet uploaded.</summary>
    public static int PendingCount
    {
        get
        {
            try { return ErrorReporter.RetainedErrorCount; }
            catch { return 0; }
        }
    }

    public static bool Posting => _posting;

    /// <summary>True when there is at least one error that has not been uploaded yet.</summary>
    public static bool HasUnuploadedErrors
    {
        get { return ErrorReporter.HasUnuploadedErrors; }
    }

    /// <summary>
    /// Uploads this session's errors as a single BugSplat report with the session rollup
    /// attached, and records the upload so it is not repeated.
    /// </summary>
    public static string PostAll() => RunUpload(PostTimeoutSeconds);

    /// <summary>
    /// Called from OnApplicationQuit. Silently uploads anything not already sent. No toast is
    /// shown (the window is going away) and failures are only written to the console log.
    /// </summary>
    public static void AutoReportOnQuit()
    {
        try
        {
            if (!CheatToggles.autoReportErrors) return;
            if (_posting) return;
            if (!HasUnuploadedErrors) return;

            string outcome = RunUpload(QuitPostTimeoutSeconds);
            try { ConsoleUI.Log("[HyperMenu] On-quit BugSplat report: " + outcome); }
            catch { }
        }
        catch { }
    }

    private static string RunUpload(int timeoutSeconds)
    {
        try
        {
            if (_posting) return "An upload is already running.";

            ErrorReporter.RetainedError[] pending;
            try { pending = ErrorReporter.SnapshotRetainedErrors(); }
            catch (Exception ex) { return "Could not read the error list: " + ex.Message; }

            if (pending == null || pending.Length == 0)
                return "No errors recorded this session, so there is nothing to upload.";

            _posting = true;
            try
            {
                return Upload(pending, timeoutSeconds);
            }
            finally
            {
                _posting = false;
            }
        }
        catch (Exception ex)
        {
            return "Upload failed: " + ex.GetType().Name + ": " + ex.Message;
        }
    }

    private static string Upload(ErrorReporter.RetainedError[] pending, int timeoutSeconds)
    {
        BugSplat bugSplat;
        try
        {
            bugSplat = new BugSplat(Database, Application, MalumMenu.hyperVersion);
            bugSplat.User = "<@" + Application + ">";
        }
        catch (Exception ex)
        {
            return "Could not initialise BugSplat (" + ex.GetType().Name + "). "
                 + "If this says FileNotFound or TypeLoad, the BugSplatDotNetStandard.dll "
                 + "dependency is missing from BepInEx/plugins.";
        }

        // One report per distinct error. The retained list is already deduplicated by signature
        // (with a repeat tally in the description), so these are genuinely separate faults rather
        // than N copies of the same one.
        int sent = 0;
        string firstFailure = null;

        for (int i = 0; i < pending.Length; i++)
        {
            ErrorReporter.RetainedError item = pending[i];
            if (item == null) continue;

            string detail = Describe(item, i, pending.Length);

            ExceptionPostOptions options = new ExceptionPostOptions
            {
                Description = "HyperMenu error " + (i + 1) + " of " + pending.Length + ": " + detail,
                Email = "<@" + Application + ">",
                User = "<@" + Application + ">",
                Notes = "HyperMenu " + MalumMenu.hyperVersion
                      + " | " + DescribeEnvironment()
            };

            // Attach this error's own HyperError_*.txt rather than the whole session rollup, so
            // each BugSplat report carries exactly the file for the fault it is about.
            string reportFile = ErrorReporter.ReportPathFor(item);
            if (!string.IsNullOrEmpty(reportFile) && File.Exists(reportFile))
            {
                try { options.Attachments.Add(new FileInfo(reportFile)); }
                catch { /* attachment is optional */ }
            }

            // The real exception is posted, so BugSplat groups and de-duplicates by its own
            // signature rather than everything landing as one synthetic blob.
            string outcome = Post(bugSplat, item.Exception ?? new Exception(detail), options, timeoutSeconds);
            if (outcome == null)
            {
                sent++;

                // Marked one at a time so a mid-way failure does not record errors that were
                // never actually sent.
                ErrorReporter.MarkUploaded(new[] { item });
            }
            else if (firstFailure == null)
            {
                firstFailure = outcome;
            }
        }

        if (sent > 0)
        {
            return "Uploaded " + sent + " of " + pending.Length + " error report"
                 + (pending.Length == 1 ? "" : "s") + " to BugSplat.";
        }

        if (firstFailure != null) return "Upload failed: " + firstFailure;

        return "Nothing was uploaded.";
    }

    /// <summary>Returns null on success, or a failure reason.</summary>
    private static string Post(BugSplat bugSplat, Exception ex, ExceptionPostOptions options,
        int timeoutSeconds = PostTimeoutSeconds)
    {
        Task<HttpResponseMessage> task;
        try
        {
            task = bugSplat.Post(ex, options);
        }
        catch (Exception postEx)
        {
            return "Post threw: " + postEx.GetType().Name;
        }

        bool finished;
        try
        {
            finished = task.Wait(TimeSpan.FromSeconds(timeoutSeconds));
        }
        catch (AggregateException agg)
        {
            return "Upload failed: " + Flatten(agg);
        }
        catch (Exception waitEx)
        {
            return "Upload failed: " + waitEx.GetType().Name;
        }

        if (!finished)
        {
            // The task is still running. Observe its eventual fault: ErrorReporter subscribes to
            // TaskScheduler.UnobservedTaskException, so an unobserved fault here would generate a
            // fresh error report about the uploader, which is a silly loop.
            task.ContinueWith(t => { AggregateException ignored = t.Exception; },
                TaskContinuationOptions.OnlyOnFaulted);
            return "timed out after " + timeoutSeconds + "s";
        }

        try
        {
            HttpResponseMessage response = task.Result;
            if (response == null) return "no response";
            if (!response.IsSuccessStatusCode)
                return "server said " + (int)response.StatusCode + " " + response.ReasonPhrase;
            return null;
        }
        catch (Exception resultEx)
        {
            return "could not read result: " + resultEx.GetType().Name;
        }
    }

    /// <summary>Unity invokes this on the way out. Kept deliberately tiny: all it does is hand
    /// over to <see cref="AutoReportOnQuit"/>, which owns every failure path.</summary>
    public sealed class QuitHook : MonoBehaviour
    {
        private void OnApplicationQuit() { AutoReportOnQuit(); }
    }

    private static string Describe(ErrorReporter.RetainedError item, int index, int total)
    {
        string id;
        try
        {
            string file = HandlingIds.FileFor(item.HandlingId);
            id = string.IsNullOrEmpty(file) ? "ID " + item.HandlingId : file;
        }
        catch { id = "ID " + item.HandlingId; }

        string what = item.PlainMessage;
        if (string.IsNullOrEmpty(what) && item.Exception != null) what = item.Exception.Message;

        return "HyperMenu error " + (index + 1) + " of " + total
             + " - " + id
             + (string.IsNullOrEmpty(item.Context) ? "" : " (" + item.Context + ")")
             + " at " + item.When.ToString("yyyy-MM-dd HH:mm:ss")
             + (item.Count > 1
                 ? " (seen " + item.Count + "x, last at " + item.LastSeen.ToString("HH:mm:ss") + ")"
                 : "")
             + "\n" + what;
    }

    private static string DescribeEnvironment()
    {
        try
        {
            return "Unity " + UnityEngine.Application.unityVersion
                 + " | " + UnityEngine.SystemInfo.operatingSystem
                 + " | " + UnityEngine.SystemInfo.processorType;
        }
        catch { return "environment unknown"; }
    }

    private static string Flatten(AggregateException agg)
    {
        try
        {
            AggregateException inner = agg.Flatten();
            if (inner.InnerExceptions.Count > 0) return inner.InnerExceptions[0].GetType().Name;
            return agg.GetType().Name;
        }
        catch { return "unknown error"; }
    }
}
