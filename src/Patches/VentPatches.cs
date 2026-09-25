using System;
using HarmonyLib;
using UnityEngine;

namespace MalumMenu;

[HarmonyPatch(typeof(Vent), nameof(Vent.CanUse))]
public static class Vent_CanUse
{
    private const int HandlingId = 30020;
    // Postfix patch of Vent.CanUse to allow usage of vents when useVents cheat is enabled
    public static void Postfix(Vent __instance, NetworkedPlayerInfo pc, ref bool canUse, ref bool couldUse, ref float __result)
    {
        try
        {
            if (!PlayerControl.LocalPlayer || !PlayerControl.LocalPlayer.Data) return;
            if (PlayerControl.LocalPlayer.Data.Role.CanVent || PlayerControl.LocalPlayer.Data.IsDead) return;
            if (!CheatToggles.unlockVents) return;

            var @object = pc.Object;

            var center = @object.Collider.bounds.center;
            var position = __instance.transform.position;
            var num = Vector2.Distance(center, position);

            // Allow usage of vents unless the vent is too far or there are objects blocking the player's path
            canUse = num <= __instance.UsableDistance && !PhysicsHelpers.AnythingBetween(@object.Collider, center, position, Constants.ShipOnlyMask, false);
            couldUse = true;
            __result = num;
        }
        catch (Exception ex) { ErrorReporter.Report(ex, HandlingId, "Vent_CanUse.Postfix: unlock vents"); }
    }
}

[HarmonyPatch(typeof(Vent), nameof(Vent.EnterVent))]
public static class Vent_EnterVent
{
    private const int HandlingId = 30020;
    // Postfix patch of Vent.EnterVent to log on ConsoleUI when a player enters a vent
    // along with the room they entered it in
    public static void Postfix(Vent __instance, PlayerControl pc)
    {
        try
        {
            if (!CheatToggles.logVents || !Utils.isShip) return;

            var (realPlayerName, displayPlayerName, isDisguised) = Utils.GetPlayerIdentity(pc);
            var room = Utils.GetRoomFromPosition(__instance.transform.position); //- (Vector3) pc.Collider.offset);
            var roomName = room != null ? room.RoomId.ToString() : "an unknown location";

            ConsoleUI.Log(isDisguised
                ? $"{realPlayerName} (as {displayPlayerName}) entered a vent in {roomName}"
                : $"{realPlayerName} entered a vent in {roomName}");
        }
        catch (Exception ex) { ErrorReporter.Report(ex, HandlingId, "Vent_EnterVent.Postfix: log vent enter"); }
    }
}

[HarmonyPatch(typeof(Vent), nameof(Vent.ExitVent))]
public static class Vent_ExitVent
{
    private const int HandlingId = 30020;
    // Postfix patch of Vent.ExitVent to log on ConsoleUI when a player exits a vent
    // along with the room they exited it in
    public static void Postfix(Vent __instance, PlayerControl pc)
    {
        try
        {
            if (!CheatToggles.logVents || !Utils.isShip) return;

            var (realPlayerName, displayPlayerName, isDisguised) = Utils.GetPlayerIdentity(pc);

            var room = Utils.GetRoomFromPosition(__instance.transform.position); //- (Vector3) pc.Collider.offset);
            var roomName = room != null ? room.RoomId.ToString() : "an unknown location";

            ConsoleUI.Log(isDisguised
                ? $"{realPlayerName} (as {displayPlayerName}) exited a vent in {roomName}"
                : $"{realPlayerName} exited a vent in {roomName}");
        }
        catch (Exception ex) { ErrorReporter.Report(ex, HandlingId, "Vent_ExitVent.Postfix: log vent exit"); }
    }
}
