using System;
using HarmonyLib;

namespace MalumMenu;

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.FixedUpdate))]
public static class ShipStatus_FixedUpdate
{
    private const int HandlingId = 30018;
    public static void Postfix(ShipStatus __instance)
    {
        try
        {
            MalumSabotageCheats.Process(__instance);
            MalumCheats.OpenSabotageMapCheat();

            MalumCheats.CloseMeetingCheat();
            MalumCheats.SkipMeetingCheat();
            MalumCheats.CallMeetingCheat();
            MalumCheats.WalkInVentCheat();
            MalumCheats.KickVentsCheat();

            MalumCheats.DoAnyTaskCheat();

            MalumPPMCheats.ReportBodyPPM();
        }
        catch (Exception ex) { ErrorReporter.Report(ex, HandlingId, "ShipStatus_FixedUpdate.Postfix: run ship cheats"); }
    }
}

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.FixedUpdate))]
public static class FungleShipStatus_FixedUpdate
{
    private const int HandlingId = 30018;
    public static void Postfix(FungleShipStatus __instance)
    {
        try
        {
            MalumSabotageCheats.ProcessFungle(__instance);
        }
        catch (Exception ex) { ErrorReporter.Report(ex, HandlingId, "FungleShipStatus_FixedUpdate.Postfix: run fungle sabotage"); }
    }
}
