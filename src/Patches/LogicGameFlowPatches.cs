using System;
using HarmonyLib;

namespace MalumMenu;

[HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.CheckEndCriteria))]
public static class LogicGameFlowNormal_CheckEndCriteria
{
    private const int HandlingId = 30006;
    // Prefix patch of LogicGameFlowNormal.CheckEndCriteria to prevent a running game from ending
    public static bool Prefix()
    {
        try
        {
            return !CheatToggles.noGameEnd;
        }
        catch (Exception ex) { ErrorReporter.Report(ex, HandlingId, "LogicGameFlowNormal_CheckEndCriteria.Prefix: check end criteria"); return true; }
    }
}

[HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.IsGameOverDueToDeath))]
public static class LogicGameFlowNormal_IsGameOverDueToDeath
{
    private const int HandlingId = 30006;
    // Postfix patch of LogicGameFlowNormal.IsGameOverDueToDeath to ensure the game does not stall
    // after an exile that should have triggered game over
    public static void Postfix(ref bool __result)
    {
        try
        {
            if (CheatToggles.noGameEnd)
            {
                __result = false;
            }

        }
        catch (Exception ex) { ErrorReporter.Report(ex, HandlingId, "LogicGameFlowNormal_IsGameOverDueToDeath.Postfix: suppress game over"); }
    }
}

[HarmonyPatch(typeof(LogicGameFlowHnS), nameof(LogicGameFlowHnS.CheckEndCriteria))]
public static class LogicGameFlowHnS_CheckEndCriteria
{
    private const int HandlingId = 30006;
    // Prefix patch of LogicGameFlowHnS.CheckEndCriteria to prevent a running HnS game from ending
    public static bool Prefix()
    {
        try
        {
            return !CheatToggles.noGameEnd;
        }
        catch (Exception ex) { ErrorReporter.Report(ex, HandlingId, "LogicGameFlowHnS_CheckEndCriteria.Prefix: check HnS end criteria"); return true; }
    }
}

[HarmonyPatch(typeof(LogicGameFlowHnS), nameof(LogicGameFlowHnS.IsGameOverDueToDeath))]
public static class LogicGameFlowHnS_IsGameOverDueToDeath
{
    private const int HandlingId = 30006;
    // Postfix patch of LogicGameFlowNormal.IsGameOverDueToDeath to ensure the HnS game does not stall
    // after an exile that should have triggered game over
    public static void Postfix(ref bool __result)
    {
        try
        {
            if (CheatToggles.noGameEnd)
            {
                __result = false;
            }

        }
        catch (Exception ex) { ErrorReporter.Report(ex, HandlingId, "LogicGameFlowHnS_IsGameOverDueToDeath.Postfix: suppress HnS game over"); }
    }
}
