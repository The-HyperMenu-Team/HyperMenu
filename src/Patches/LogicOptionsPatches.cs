using System;
using HarmonyLib;

namespace MalumMenu;

[HarmonyPatch(typeof(LogicOptions), nameof(LogicOptions.GetAnonymousVotes))]
public static class LogicOptions_GetAnonymousVotes
{
    private const int HandlingId = 30007;
    // Postfix patch of LogicOptions.GetAnonymousVotes to disable anonymous votes for revealVotes cheat
    public static void Postfix(ref bool __result)
    {
        try
        {
            if (CheatToggles.revealVotes)
            {
                __result = false;
            }
        }
        catch (Exception ex) { ErrorReporter.Report(ex, HandlingId, "LogicOptions_GetAnonymousVotes.Postfix: disable anonymous votes"); }
    }
}

[HarmonyPatch(typeof(LogicOptionsNormal), nameof(LogicOptionsNormal.GetAnonymousVotes))]
public static class LogicOptionsNormal_GetAnonymousVotes
{
    private const int HandlingId = 30007;
    // Postfix patch of LogicOptionsNormal.GetAnonymousVotes to disable anonymous votes for revealVotes cheat
    public static void Postfix(ref bool __result)
    {
        try
        {
            if (CheatToggles.revealVotes)
            {
                __result = false;
            }
        }
        catch (Exception ex) { ErrorReporter.Report(ex, HandlingId, "LogicOptionsNormal_GetAnonymousVotes.Postfix: disable anonymous votes"); }
    }
}
