using System;
using HarmonyLib;
using System.Linq;
using Sentry.Internal.Extensions;

namespace MalumMenu;

[HarmonyPatch(typeof(EngineerRole), nameof(EngineerRole.FixedUpdate))]
public static class EngineerRole_FixedUpdate
{
    private const int HandlingId = 30016;
    public static void Postfix(EngineerRole __instance)
    {
        try
        {
            if(__instance.Player.AmOwner)
            {
                MalumCheats.HandleEngineerCheats(__instance);
            }
        }
        catch (Exception ex) { ErrorReporter.Report(ex, HandlingId, "EngineerRole_FixedUpdate.Postfix: handle engineer cheats"); }
    }
}

[HarmonyPatch(typeof(ShapeshifterRole), nameof(ShapeshifterRole.FixedUpdate))]
public static class ShapeshifterRole_FixedUpdate
{
    private const int HandlingId = 30016;
    public static void Postfix(ShapeshifterRole __instance)
    {
        try
        {
            if(__instance.Player.AmOwner)
            {
                MalumCheats.HandleShapeshifterCheats(__instance);
            }
        }
        catch (Exception ex) { ErrorReporter.Report(ex, HandlingId, "ShapeshifterRole_FixedUpdate.Postfix: handle shapeshifter cheats"); }
    }
}

[HarmonyPatch(typeof(ScientistRole), nameof(ScientistRole.Update))]
public static class ScientistRole_Update
{
    private const int HandlingId = 30016;
    public static void Postfix(ScientistRole __instance)
    {
        try
        {
            if(__instance.Player.AmOwner)
            {
                MalumCheats.HandleScientistCheats(__instance);
            }
        }
        catch (Exception ex) { ErrorReporter.Report(ex, HandlingId, "ScientistRole_Update.Postfix: handle scientist cheats"); }
    }
}

[HarmonyPatch(typeof(TrackerRole), nameof(TrackerRole.FixedUpdate))]
public static class TrackerRole_FixedUpdate
{
    private const int HandlingId = 30016;
    public static void Postfix(TrackerRole __instance)
    {
        try
        {
            if(__instance.Player.AmOwner)
            {
                MalumCheats.HandleTrackerCheats(__instance);
            }
        }
        catch (Exception ex) { ErrorReporter.Report(ex, HandlingId, "TrackerRole_FixedUpdate.Postfix: handle tracker cheats"); }
    }
}

[HarmonyPatch(typeof(PhantomRole), nameof(PhantomRole.IsValidTarget))]
public static class PhantomRole_IsValidTarget
{
    private const int HandlingId = 30016;
    // Postfix patch of PhantomRole.IsValidTarget to allow killing while invisible
    public static void Postfix(NetworkedPlayerInfo target, ref bool __result)
    {
        try
        {
            if (CheatToggles.killVanished)
            {
                __result = Utils.IsValidTarget(target);
            }
        }
        catch (Exception ex) { ErrorReporter.Report(ex, HandlingId, "PhantomRole_IsValidTarget.Postfix: allow vanished kills"); }
    }
}

[HarmonyPatch(typeof(ImpostorRole), nameof(ImpostorRole.IsValidTarget))]
public static class ImpostorRole_IsValidTarget
{
    private const int HandlingId = 30016;
    // Postfix patch of ImpostorRole.IsValidTarget to allow forbidden kill targets for killAnyone cheat
    // Allows killing ghosts (with seeGhosts), impostors, players in vents, etc...
    public static void Postfix(NetworkedPlayerInfo target, ref bool __result)
    {
        try
        {
            if (CheatToggles.killAnyone)
            {
               __result = Utils.IsValidTarget(target);
            }
        }
        catch (Exception ex) { ErrorReporter.Report(ex, HandlingId, "ImpostorRole_IsValidTarget.Postfix: allow any kill target"); }
    }
}

[HarmonyPatch(typeof(ImpostorRole), nameof(ImpostorRole.FindClosestTarget))]
public static class ImpostorRole_FindClosestTarget
{
    private const int HandlingId = 30016;
    // Prefix patch of ImpostorRole.FindClosestTarget to allow for infinite kill reach
    public static bool Prefix(ImpostorRole __instance, ref PlayerControl __result)
    {
        try
        {
            if (!CheatToggles.killReach) return true;

            var playerList = Utils.GetPlayersSortedByDistance().Where(player => !player.IsNull() && __instance.IsValidTarget(player.Data) && player.Collider.enabled).ToList();

            __result = playerList[0];

            return false;
        }
        catch (Exception ex) { ErrorReporter.Report(ex, HandlingId, "ImpostorRole_FindClosestTarget.Prefix: extend kill reach"); return true; }
    }
}

[HarmonyPatch(typeof(DetectiveRole), nameof(DetectiveRole.FindClosestTarget))]
public static class DetectiveRole_FindClosestTarget
{
    private const int HandlingId = 30016;
    // Prefix patch of DetectiveRole.FindClosestTarget to allow for infinite interrogate reach
    public static bool Prefix(DetectiveRole __instance, ref PlayerControl __result)
    {
        try
        {
            if (!CheatToggles.interrogateReach) return true;

            var playerList = Utils.GetPlayersSortedByDistance().Where(player => !player.IsNull() && __instance.IsValidTarget(player.Data) && player.Collider.enabled).ToList();

            __result = playerList[0];

            return false;
        }
        catch (Exception ex) { ErrorReporter.Report(ex, HandlingId, "DetectiveRole_FindClosestTarget.Prefix: extend interrogate reach"); return true; }
    }
}

[HarmonyPatch(typeof(TrackerRole), nameof(TrackerRole.FindClosestTarget))]
public static class TrackerRole_FindClosestTarget
{
    private const int HandlingId = 30016;
    // Prefix patch of TrackerRole.FindClosestTarget to allow for infinite track reach
    public static bool Prefix(TrackerRole __instance, ref PlayerControl __result)
    {
        try
        {
            if (!CheatToggles.trackReach) return true;

            var playerList = Utils.GetPlayersSortedByDistance().Where(player => !player.IsNull() && __instance.IsValidTarget(player.Data) && player.Collider.enabled).ToList();

            __result = playerList[0];

            return false;
        }
        catch (Exception ex) { ErrorReporter.Report(ex, HandlingId, "TrackerRole_FindClosestTarget.Prefix: extend track reach"); return true; }
    }
}
