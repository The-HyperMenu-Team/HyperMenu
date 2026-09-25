using Il2CppSystem.Collections.Generic;
using BepInEx.Unity.IL2CPP.Utils;
using System;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

namespace MalumMenu;
public static class MalumPPMCheats
{
    private const int HandlingId = 20009;
    private static bool _telekillPlayerActive;
    private static bool _killPlayerActive;
    private static bool _spectateActive;
    private static bool _teleportPlayerActive;
    private static bool _reportBodyActive;
    private static bool _ejectPlayerActive;
    private static bool _setFakeRoleActive;
    private static bool _setFakeAliveActive;
    private static bool _forceRoleActive;
    private static RoleTypes? _oldRole = null;

    public static void ReportBodyPPM()
    {
        try
        {
            if (CheatToggles.reportBody)
        {

            if (!_reportBodyActive)
            {
                // Close any player pick menus already open & their cheats
                if (PlayerPickMenu.playerpickMenu != null)
                {
                    PlayerPickMenu.playerpickMenu.Close();
                    CheatToggles.DisablePPMCheats("reportBody");
                }

                // Player pick menu to choose any body (alive or dead) and report it
                PlayerPickMenu.OpenPlayerPickMenu(Utils.GetAllPlayerData(), (Action) (() =>
                {
                    try
                    {
                        PlayerControl.LocalPlayer.CmdReportDeadBody(PlayerPickMenu.targetPlayerData);
                    }
                    catch (Exception ex) { ErrorReporter.Report(ex, HandlingId, "MalumPPMCheats.ReportBodyPPM callback: reporting body"); }
                }));

                _reportBodyActive = true;
            }

            // Deactivate cheat if menu is closed
            if (PlayerPickMenu.playerpickMenu == null)
            {
                CheatToggles.reportBody = false;
            }

        }
        else
        {
            if (_reportBodyActive)
            {
                _reportBodyActive = false;
            }
        }
        }
        catch (Exception ex) { ErrorReporter.Report(ex, HandlingId, "MalumPPMCheats.ReportBodyPPM: opening report menu"); }
    }

    public static void EjectPlayerPPM()
    {
        try
        {
            if (CheatToggles.ejectPlayer)
        {
            if (!_ejectPlayerActive)
            {
                // Close any player pick menus already open & their cheats
                if (PlayerPickMenu.playerpickMenu != null)
                {
                    PlayerPickMenu.playerpickMenu.Close();
                    CheatToggles.DisablePPMCheats("ejectPlayer");
                }

                if (!Utils.isMeeting)
                {
                    CheatToggles.ejectPlayer = false;
                    return;
                }

                List<NetworkedPlayerInfo> playerInfo = new List<NetworkedPlayerInfo>();
                foreach (var player in PlayerControl.AllPlayerControls)
                {
                    if (!player.Data.IsDead && !player.Data.Disconnected)
                    {
                        playerInfo.Add(player.Data);
                    }
                }

                // Player pick menu to choose any living player and eject them during meeting
                PlayerPickMenu.OpenPlayerPickMenu(playerInfo, (Action)(() =>
                {
                    try
                    {
                        NetworkedPlayerInfo playerToEject = PlayerPickMenu.targetPlayerData;
                        MeetingHud.Instance.RpcVotingComplete(new Il2CppStructArray<MeetingHud.VoterState>(0L), playerToEject, false, false, ushort.MinValue);
                    }
                    catch (Exception ex) { ErrorReporter.Report(ex, HandlingId, "MalumPPMCheats.EjectPlayerPPM callback: ejecting player"); }
                }));

                _ejectPlayerActive = true;
            }

            // Deactivate cheat if menu is closed
            if (PlayerPickMenu.playerpickMenu == null)
            {
                CheatToggles.ejectPlayer = false;
            }
        }
        else if (_ejectPlayerActive)
        {
            _ejectPlayerActive = false;
        }
        }
        catch (Exception ex) { ErrorReporter.Report(ex, HandlingId, "MalumPPMCheats.EjectPlayerPPM: opening eject menu"); }
    }

    public static void KillPlayerPPM()
    {
        try
        {
            if (CheatToggles.killPlayer)
        {
            if (!_killPlayerActive)
            {
                // Close any player pick menus already open & their cheats
                if (PlayerPickMenu.playerpickMenu != null)
                {
                    PlayerPickMenu.playerpickMenu.Close();
                    CheatToggles.DisablePPMCheats("killPlayer");
                }

                if (Utils.isLobby)
                {
                    HudManager.Instance.Notifier.AddDisconnectMessage("Killing in lobby disabled for being too buggy");
                    CheatToggles.killPlayer = false;
                    return;
                }

                // Player pick menu made for killing any player by sending a successful MurderPlayer RPC call
                PlayerPickMenu.OpenPlayerPickMenu(Utils.GetAllPlayerData(), (Action)(() =>
                {
                    try
                    {
                        Utils.MurderPlayer(PlayerPickMenu.targetPlayerData.Object, MurderResultFlags.Succeeded);
                    }
                    catch (Exception ex) { ErrorReporter.Report(ex, HandlingId, "MalumPPMCheats.KillPlayerPPM callback: killing player"); }
                }));

                _killPlayerActive = true;
            }

            // Deactivate cheat if menu is closed
            if (PlayerPickMenu.playerpickMenu == null)
            {
                CheatToggles.killPlayer = false;
            }
        }
        else if (_killPlayerActive)
        {
            _killPlayerActive = false;
        }
        }
        catch (Exception ex) { ErrorReporter.Report(ex, HandlingId, "MalumPPMCheats.KillPlayerPPM: opening kill menu"); }
    }

    public static void TelekillPlayerPPM()
    {
        try
        {
            if (CheatToggles.telekillPlayer)
        {
            if (!_telekillPlayerActive)
            {
                // Close any player pick menus already open & their cheats
                if (PlayerPickMenu.playerpickMenu != null)
                {
                    PlayerPickMenu.playerpickMenu.Close();
                    CheatToggles.DisablePPMCheats("telekillPlayer");
                }

                if (Utils.isLobby)
                {
                    HudManager.Instance.Notifier.AddDisconnectMessage("Killing in lobby disabled for being too buggy");
                    CheatToggles.telekillPlayer = false;
                    return;
                }

                // Player pick menu made for killing any player by sending a successful MurderPlayer RPC call
                // and immediatly teleporting back to original position
                PlayerPickMenu.OpenPlayerPickMenu(Utils.GetAllPlayerData(), (Action)(() =>
                {
                    try
                    {
                        var oldPos = PlayerControl.LocalPlayer.GetTruePosition();
                        Utils.MurderPlayer(PlayerPickMenu.targetPlayerData.Object, MurderResultFlags.Succeeded);
                        AmongUsClient.Instance.StartCoroutine(ErrorReporter.GuardCoroutine(Utils.DelayedSnapTo(oldPos), HandlingId, "DelayedSnapTo"));
                    }
                    catch (Exception ex) { ErrorReporter.Report(ex, HandlingId, "MalumPPMCheats.TelekillPlayerPPM callback: telekilling player"); }
                }));

                _telekillPlayerActive = true;
            }

            // Deactivate cheat if menu is closed
            if (PlayerPickMenu.playerpickMenu == null)
            {
                CheatToggles.telekillPlayer = false;
            }
        }
        else if (_telekillPlayerActive)
        {
            _telekillPlayerActive = false;
        }
        }
        catch (Exception ex) { ErrorReporter.Report(ex, HandlingId, "MalumPPMCheats.TelekillPlayerPPM: opening telekill menu"); }
    }

    public static void TeleportPlayerPPM()
    {
        try
        {
            if (CheatToggles.teleportPlayer)
        {
            if (!_teleportPlayerActive)
            {
                // Close any player pick menus already open & their cheats
                if (PlayerPickMenu.playerpickMenu != null)
                {
                    PlayerPickMenu.playerpickMenu.Close();
                    CheatToggles.DisablePPMCheats("teleportPlayer");
                }

                List<NetworkedPlayerInfo> playerDataList = new List<NetworkedPlayerInfo>();

                // All players are saved to playerList apart from LocalPlayer
                foreach (var player in PlayerControl.AllPlayerControls)
                {
                    if (!player.AmOwner)
                    {
                        playerDataList.Add(player.Data);
                    }
                }

                // Player pick menu made for teleporting LocalPlayer to any player's position
                PlayerPickMenu.OpenPlayerPickMenu(playerDataList, (Action)(() =>
                {
                    try
                    {
                        PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(PlayerPickMenu.targetPlayerData.Object.transform.position);
                    }
                    catch (Exception ex) { ErrorReporter.Report(ex, HandlingId, "MalumPPMCheats.TeleportPlayerPPM callback: teleporting to player"); }
                }));

                _teleportPlayerActive = true;
            }

            // Deactivate cheat if menu is closed
            if (PlayerPickMenu.playerpickMenu == null)
            {
                CheatToggles.teleportPlayer = false;
            }
        }
        else if (_teleportPlayerActive)
        {
            _teleportPlayerActive = false;
        }
        }
        catch (Exception ex) { ErrorReporter.Report(ex, HandlingId, "MalumPPMCheats.TeleportPlayerPPM: opening teleport menu"); }
    }

    public static void SetFakeRolePPM()
    {
        try
        {
            if (CheatToggles.setFakeRole)
        {

            if (!_setFakeRoleActive)
            {

                // Close any player pick menus already open & their cheats
                if (PlayerPickMenu.playerpickMenu != null)
                {
                    PlayerPickMenu.playerpickMenu.Close();
                    CheatToggles.DisablePPMCheats("setFakeRole");
                }

                List<NetworkedPlayerInfo> playerDataList = new List<NetworkedPlayerInfo>();

                // Shapeshifter role can only be used if it was already assigned at the start of the game
                // This is done to prevent the anticheat from kicking players
                if (_oldRole == RoleTypes.Shapeshifter || Utils.isFreePlay)
                {
                    playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Shapeshifter", OutfitPreset.Shapeshifter, Utils.GetBehaviourByRoleType(RoleTypes.Shapeshifter)));
                }

                // Phantom role can only be used if it was already assigned at the start of the game
                // This is done to prevent the anticheat from kicking players
                if (_oldRole == RoleTypes.Phantom || Utils.isFreePlay)
                {
                    playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Phantom", OutfitPreset.Phantom, Utils.GetBehaviourByRoleType(RoleTypes.Phantom)));
                }

                // Viper role can only be used if it was already assigned at the start of the game
                // This is done to prevent the anticheat from kicking players
                if (_oldRole == RoleTypes.Viper || Utils.isFreePlay)
                {
                    playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Viper", OutfitPreset.Viper, Utils.GetBehaviourByRoleType(RoleTypes.Viper)));
                }

                // Impostor role can only be used if it was already assigned at the start of the game or as host
                // This is done to prevent the anticheat from kicking players
                if ((_oldRole != null && Utils.GetBehaviourByRoleType((RoleTypes)_oldRole).TeamType == RoleTeamTypes.Impostor) || Utils.isFreePlay || Utils.isHost)
                {
                    playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Impostor", OutfitPreset.Impostor, Utils.GetBehaviourByRoleType(RoleTypes.Impostor)));
                }

                // Judge role can only be used if it was already assigned at the start of the game
                // This is done to prevent the anticheat from kicking players
                if (_oldRole == RoleTypes.Judge || Utils.isFreePlay)
                {
                    playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Judge", OutfitPreset.Judge, Utils.GetBehaviourByRoleType(RoleTypes.Judge)));
                }

                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Tracker", OutfitPreset.Tracker, Utils.GetBehaviourByRoleType(RoleTypes.Tracker)));
                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Noisemaker", OutfitPreset.Noisemaker, Utils.GetBehaviourByRoleType(RoleTypes.Noisemaker)));
                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Engineer", OutfitPreset.Engineer, Utils.GetBehaviourByRoleType(RoleTypes.Engineer)));
                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Scientist", OutfitPreset.Scientist, Utils.GetBehaviourByRoleType(RoleTypes.Scientist)));
                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Detective", OutfitPreset.Detective, Utils.GetBehaviourByRoleType(RoleTypes.Detective)));
                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Crewmate", OutfitPreset.Crewmate, Utils.GetBehaviourByRoleType(RoleTypes.Crewmate)));

                // Player pick menu made for changing your roles with a custom choice list
                PlayerPickMenu.OpenPlayerPickMenu(playerDataList, (Action) (() =>
                {
                    try
                    {
                    // Log the originally assigned role before it gets changed by setFakeRole cheat
                    if (!Utils.isLobby && !Utils.isFreePlay && _oldRole == null)
                    {
                        _oldRole = PlayerControl.LocalPlayer.Data.RoleType;
                    }

                    if (PlayerControl.LocalPlayer.Data.IsDead) // Prevent accidential revives
                    {
                        if (PlayerPickMenu.targetPlayerData.Role.TeamType == RoleTeamTypes.Impostor)
                        {
                            RoleManager.Instance.SetRole(PlayerControl.LocalPlayer, RoleTypes.ImpostorGhost);
                        }
                        else
                        {
                            RoleManager.Instance.SetRole(PlayerControl.LocalPlayer, RoleTypes.CrewmateGhost);
                        }
                    }
                    else
                    {
                        /* if (PlayerPickMenu.targetPlayerData.Role.Role == RoleTypes.Shapeshifter && oldRole != RoleTypes.Shapeshifter){

                            Utils.showPopup("\n<size=125%>Changing into the Shapeshifter role is not recommended\nsince shapeshifting will get you kicked by the anticheat");

                        } else if (PlayerPickMenu.targetPlayerData.Role.Role == RoleTypes.Noisemaker && oldRole != RoleTypes.Noisemaker){

                            Utils.showPopup("\n<size=125%>Changing into the Noisemaker role is not recommended\nsince dying won't trigger the alert for other players");

                        } else if (oldRole == RoleTypes.Noisemaker){

                            Utils.showPopup("\n<size=125%>Your \"real\" role is still Noisemaker\nso other players will still see the alert when you die");

                        } */

                        RoleManager.Instance.SetRole(PlayerControl.LocalPlayer, PlayerPickMenu.targetPlayerData.Role.Role);
                    }
                    }
                    catch (Exception ex) { ErrorReporter.Report(ex, HandlingId, "MalumPPMCheats.SetFakeRolePPM callback: setting fake role"); }
                }));

                _setFakeRoleActive = true;
            }

            // Deactivate cheat if menu is closed
            if (PlayerPickMenu.playerpickMenu == null)
            {
                CheatToggles.setFakeRole = false;
            }

        }
        else
        {
            if (_setFakeRoleActive)
            {
                _setFakeRoleActive = false;
            }
        }
        }
        catch (Exception ex) { ErrorReporter.Report(ex, HandlingId, "MalumPPMCheats.SetFakeRolePPM: opening role menu"); }
    }

    public static void SetFakeAlivePPM()
    {
        try
        {
            if (CheatToggles.setFakeAlive)
        {

            if (!_setFakeAliveActive)
            {

                // Close any player pick menus already open & their cheats
                if (PlayerPickMenu.playerpickMenu != null)
                {
                    PlayerPickMenu.playerpickMenu.Close();
                    CheatToggles.DisablePPMCheats("setFakeAlive");
                }

                List<NetworkedPlayerInfo> playerDataList = new List<NetworkedPlayerInfo>();

                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Alive", OutfitPreset.Crewmate, Utils.GetBehaviourByRoleType(RoleTypes.Crewmate)));
                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Dead", OutfitPreset.Dead, Utils.GetBehaviourByRoleType(RoleTypes.CrewmateGhost)));

                // Player pick menu made for changing your alive state with a custom choice list
                PlayerPickMenu.OpenPlayerPickMenu(playerDataList, (Action) (() =>
                {
                    try
                    {
                        if (PlayerPickMenu.targetPlayerData.Role.IsDead)
                        {
                            PlayerControl.LocalPlayer.Die(DeathReason.Exile, true);
                        }
                        else
                        {
                            PlayerControl.LocalPlayer.Revive();
                        }
                    }
                    catch (Exception ex) { ErrorReporter.Report(ex, HandlingId, "MalumPPMCheats.SetFakeAlivePPM callback: setting alive state"); }
                }));

                _setFakeAliveActive = true;
            }

            // Deactivate cheat if menu is closed
            if (PlayerPickMenu.playerpickMenu == null)
            {
                CheatToggles.setFakeAlive = false;
            }

        }
        else
        {
            if (_setFakeAliveActive)
            {
                _setFakeAliveActive = false;
            }
        }
        }
        catch (Exception ex) { ErrorReporter.Report(ex, HandlingId, "MalumPPMCheats.SetFakeAlivePPM: opening alive menu"); }
    }

    public static void ForceRolePPM()
    {
        try
        {
            if (CheatToggles.forceRole)
        {
            if (!_forceRoleActive)
            {
                if (PlayerPickMenu.playerpickMenu != null)
                {
                    PlayerPickMenu.playerpickMenu.Close();
                    CheatToggles.DisablePPMCheats("forceRole");
                }

                List<NetworkedPlayerInfo> playerDataList = new List<NetworkedPlayerInfo>();

                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Shapeshifter", OutfitPreset.Shapeshifter, Utils.GetBehaviourByRoleType(RoleTypes.Shapeshifter)));
                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Phantom", OutfitPreset.Phantom, Utils.GetBehaviourByRoleType(RoleTypes.Phantom)));
                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Viper", OutfitPreset.Viper, Utils.GetBehaviourByRoleType(RoleTypes.Viper)));
                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Impostor", OutfitPreset.Impostor, Utils.GetBehaviourByRoleType(RoleTypes.Impostor)));
                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Tracker", OutfitPreset.Tracker, Utils.GetBehaviourByRoleType(RoleTypes.Tracker)));
                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Noisemaker", OutfitPreset.Noisemaker, Utils.GetBehaviourByRoleType(RoleTypes.Noisemaker)));
                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Engineer", OutfitPreset.Engineer, Utils.GetBehaviourByRoleType(RoleTypes.Engineer)));
                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Scientist", OutfitPreset.Scientist, Utils.GetBehaviourByRoleType(RoleTypes.Scientist)));
                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Detective", OutfitPreset.Detective, Utils.GetBehaviourByRoleType(RoleTypes.Detective)));
                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Crewmate", OutfitPreset.Crewmate, Utils.GetBehaviourByRoleType(RoleTypes.Crewmate)));

                // Player pick menu made for forcing a role onto another player
                PlayerPickMenu.OpenPlayerPickMenu(playerDataList, (Action)(() =>
                {
                    try
                    {
                        CheatToggles.forcedRole = PlayerPickMenu.targetPlayerData.Role.Role;
                    }
                    catch (Exception ex) { ErrorReporter.Report(ex, HandlingId, "MalumPPMCheats.ForceRolePPM callback: forcing role"); }
                }));

                _forceRoleActive = true;
            }

            // Deactivate cheat if menu is closed
            if (PlayerPickMenu.playerpickMenu == null)
            {
                CheatToggles.forceRole = false;
            }

        }
        else
        {
            if (_forceRoleActive)
            {
                _forceRoleActive = false;
            }
        }
        }
        catch (Exception ex) { ErrorReporter.Report(ex, HandlingId, "MalumPPMCheats.ForceRolePPM: opening force-role menu"); }
    }

    public static void SpectatePPM()
    {
        try
        {
            if (CheatToggles.spectate)
        {

            if (!_spectateActive)
            {

                // Close any player pick menus already open & their cheats
                if (PlayerPickMenu.playerpickMenu != null)
                {
                    PlayerPickMenu.playerpickMenu.Close();
                    CheatToggles.DisablePPMCheats("spectate");
                }

                List<NetworkedPlayerInfo> playerDataList = new List<NetworkedPlayerInfo>();

                // All players are saved to playerList apart from LocalPlayer
                foreach (var player in PlayerControl.AllPlayerControls)
                {
                    if (!player.AmOwner)
                    {
                        playerDataList.Add(player.Data);
                    }
                }

                // Player pick menu made for spectating the targeted player
                PlayerPickMenu.OpenPlayerPickMenu(playerDataList, (Action) (() =>
                {
                    try
                    {
                        Camera.main.gameObject.GetComponent<FollowerCamera>().SetTarget(PlayerPickMenu.targetPlayerData.Object);
                    }
                    catch (Exception ex) { ErrorReporter.Report(ex, HandlingId, "MalumPPMCheats.SpectatePPM callback: setting spectate target"); }
                }));

                _spectateActive = true;

                PlayerControl.LocalPlayer.moveable = false; // Can't move while spectating

                CheatToggles.freecam = false; // Disable incompatible cheats while spectating

            }

            // Deactivate cheat if menu is closed and no one is getting spectated
            if (PlayerPickMenu.playerpickMenu == null && Camera.main.gameObject.GetComponent<FollowerCamera>().Target == PlayerControl.LocalPlayer)
            {
                CheatToggles.spectate = false;
                PlayerControl.LocalPlayer.moveable = true;
            }
        }
        else
        {
            // Deactivate cheat when it is disabled from the Malum GUI
            if (_spectateActive)
            {
                _spectateActive = false;
                PlayerControl.LocalPlayer.moveable = true;
                Camera.main.gameObject.GetComponent<FollowerCamera>().SetTarget(PlayerControl.LocalPlayer);
            }
        }
        }
        catch (Exception ex) { ErrorReporter.Report(ex, HandlingId, "MalumPPMCheats.SpectatePPM: opening spectate menu"); }
    }
}
