using System;
using HarmonyLib;
using Il2CppSystem;
using UnityEngine;

namespace MalumMenu;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetKillTimer))]
public static class PlayerControl_SetKillTimer
{
    private const int HandlingId = 30014;
    // Prefix patch of PlayerControl.SetKillTimer to remove kill cooldown
    public static void Prefix(PlayerControl __instance, ref float time)
    {
        try
        {
            if (!__instance.AmOwner || !Utils.isHost || !CheatToggles.noKillCd) return;

            time = 0f;
        }
        catch (System.Exception ex) { ErrorReporter.Report(ex, HandlingId, "PlayerControl_SetKillTimer.Prefix: remove kill cooldown"); }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdCheckMurder))]
public static class PlayerControl_CmdCheckMurder
{
    private const int HandlingId = 30014;
    // Prefix patch of PlayerControl.CmdCheckMurder to always bypass checks when killing players
    public static bool Prefix(PlayerControl __instance, PlayerControl target)
    {
        try
        {
            /*if (Utils.isLobby){
                HudManager.Instance.Notifier.AddDisconnectMessage("Killing in lobby disabled for being too buggy");
                return false;
            }

            // Direct kill RPC should only be used when absolutely necessary as to avoid detection from anticheat mods
            if (!CheatToggles.killAnyone && !CheatToggles.zeroKillCd && !Utils.isVanished(__instance.Data) &&
                !Utils.isMeeting &&
                (MalumPPMCheats.oldRole == null ||
                 Utils.getBehaviourByRoleType((AmongUs.GameOptions.RoleTypes)MalumPPMCheats.oldRole).IsImpostor))
                return true;
            if (!__instance.Data.Role.IsValidTarget(target.Data))
            {
                return true;
            }

            if (target.protectedByGuardianId > -1 && !CheatToggles.killAnyone){
                return true;
            }

            Utils.murderPlayer(target, MurderResultFlags.Succeeded);

            return false;*/

            if (!Utils.isHost) return true;

            // __instance.isKilling = true;
            PlayerControl.LocalPlayer.RpcMurderPlayer(target, true);

            return false;
        }
        catch (System.Exception ex) { ErrorReporter.Report(ex, HandlingId, "PlayerControl_CmdCheckMurder.Prefix: bypass murder checks"); return true; }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
public static class PlayerControl_MurderPlayer
{
    private const int HandlingId = 30014;
    // Prefix patch of PlayerControl.MurderPlayer to log on ConsoleUI when a player tries to kill another player,
    // along with who the killer and target are, and where the kill happened.
    // Also logs when a kill gets saved by a guardian angel.
    public static void Prefix(PlayerControl __instance, PlayerControl target)
    {
        try
        {
            if (!CheatToggles.logDeaths || target == null) return;

            var (realKillerName, displayKillerName, isDisguised) = Utils.GetPlayerIdentity(__instance);
            var targetName = $"<color=#{ColorUtility.ToHtmlStringRGB(target.Data.Color)}>{target.CurrentOutfit.PlayerName}</color>";

            var room = Utils.GetRoomFromPosition(target.GetTruePosition());
            var roomName = room != null ? room.RoomId.ToString() : "an unknown location";

            if (target.protectedByGuardianId != -1)
            {
                ConsoleUI.Log(isDisguised ? $"{realKillerName} (as {displayKillerName}) tried to kill {targetName} in {roomName} (Protected)"
                    : $"{realKillerName} tried to kill {targetName} in {roomName} (Protected)");
            }
            else
            {
                ConsoleUI.Log(isDisguised ? $"{realKillerName} (as {displayKillerName}) killed {targetName} in {roomName}"
                    : $"{realKillerName} killed {targetName} in {roomName}");
            }
        }
        catch (System.Exception ex) { ErrorReporter.Report(ex, HandlingId, "PlayerControl_MurderPlayer.Prefix: log kills"); }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.TurnOnProtection))]
public static class PlayerControl_TurnOnProtection
{
    private const int HandlingId = 30014;
    // Prefix patch of PlayerControl.TurnOnProtection to make all protections visible
    public static void Prefix(ref bool visible)
    {
        try
        {
            if (CheatToggles.seeGhosts)
            {
                visible = true;
            }
        }
        catch (System.Exception ex) { ErrorReporter.Report(ex, HandlingId, "PlayerControl_TurnOnProtection.Prefix: reveal protections"); }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdCheckShapeshift))]
public static class PlayerControl_CmdCheckShapeshift
{
    private const int HandlingId = 30014;
    // Prefix patch of PlayerControl.CmdCheckShapeshift to prevent SS animation
    public static void Prefix(ref bool shouldAnimate)
    {
        try
        {
            if (shouldAnimate && CheatToggles.noShapeshiftAnim)
            {
                shouldAnimate = false;
            }
        }
        catch (System.Exception ex) { ErrorReporter.Report(ex, HandlingId, "PlayerControl_CmdCheckShapeshift.Prefix: skip shapeshift anim"); }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdCheckRevertShapeshift))]
public static class PlayerControl_CmdCheckRevertShapeshift
{
    private const int HandlingId = 30014;
    // Prefix patch of PlayerControl.CmdCheckRevertShapeshift to prevent SS animation
    public static void Prefix(ref bool shouldAnimate){

        try
        {
            if (shouldAnimate && CheatToggles.noShapeshiftAnim)
            {
                shouldAnimate = false;
            }
        }
        catch (System.Exception ex) { ErrorReporter.Report(ex, HandlingId, "PlayerControl_CmdCheckRevertShapeshift.Prefix: skip revert anim"); }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Shapeshift))]
public static class PlayerControl_Shapeshift
{
    private const int HandlingId = 30014;
    // Postfix patch of PlayerControl.Shapeshift to log on ConsoleUI when a player shapeshifts into another player,
    // and who they shapeshifted into. Also logs when a shapeshift gets reverted.
    public static void Postfix(PlayerControl __instance, PlayerControl targetPlayer, bool animate)
    {
        try
        {
            if (!CheatToggles.logShapeshifts) return;

            if (__instance.CurrentOutfitType == PlayerOutfitType.MushroomMixup) return;

            var targetPlayerInfo = targetPlayer.Data;

            var room = Utils.GetRoomFromPosition(__instance.GetTruePosition());
            var roomName = room != null ? room.RoomId.ToString() : "an unknown location";

            if (targetPlayerInfo.PlayerId == __instance.Data.PlayerId)
            {
                ConsoleUI.Log($"<color=#{ColorUtility.ToHtmlStringRGB(GameData.Instance.GetPlayerById(__instance.PlayerId).Color)}>" +
                              $"{GameData.Instance.GetPlayerById(__instance.PlayerId)._object.Data.PlayerName}</color> unshapeshifted in {roomName}");
            }
            else
            {
                ConsoleUI.Log($"<color=#{ColorUtility.ToHtmlStringRGB(GameData.Instance.GetPlayerById(__instance.PlayerId).Color)}>" +
                              $"{GameData.Instance.GetPlayerById(__instance.PlayerId)._object.Data.PlayerName}</color> shapeshifted into " +
                              $"<color=#{ColorUtility.ToHtmlStringRGB(GameData.Instance.GetPlayerById(targetPlayerInfo.PlayerId).Color)}>" +
                              $"{GameData.Instance.GetPlayerById(targetPlayerInfo.PlayerId)._object.Data.PlayerName}</color> in {roomName}");
            }
        }
        catch (System.Exception ex) { ErrorReporter.Report(ex, HandlingId, "PlayerControl_Shapeshift.Postfix: log shapeshifts"); }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CompleteTask))]
public static class PlayerControl_CompleteTask
{
    private const int HandlingId = 30014;
    public static void Postfix(PlayerControl __instance, uint idx)
    {
        try
        {
            if (!CheatToggles.logTasks) return;

            var task = __instance.myTasks.Find((Il2CppSystem.Predicate<PlayerTask>)(p => (int)p.Id == (int)idx));
            var room = Utils.GetRoomFromPosition(__instance.GetTruePosition());
            var roomName = room != null ? room.RoomId.ToString() : "an unknown location";

            if (task)
            {
                ConsoleUI.Log(
                    $"<color=#{ColorUtility.ToHtmlStringRGB(__instance.Data.Color)}>{__instance.Data.PlayerName}</color> completed task {task.TaskType} in {roomName}");
            }
        }
        catch (System.Exception ex) { ErrorReporter.Report(ex, HandlingId, "PlayerControl_CompleteTask.Postfix: log task completion"); }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSyncSettings))]
public static class PlayerControl_RpcSyncSettings
{
    private const int HandlingId = 30014;
    // Prefix patch of PlayerControl.RpcSyncSettings to prevent the anti-cheat from kicking you
    // for some settings that are out of the "original" valid range
    public static bool Prefix(PlayerControl __instance, byte[] optionsByteArray)
    {
        try
        {
            return !CheatToggles.noOptionsLimits;
        }
        catch (System.Exception ex) { ErrorReporter.Report(ex, HandlingId, "PlayerControl_RpcSyncSettings.Prefix: bypass sync check"); return true; }
    }
}
