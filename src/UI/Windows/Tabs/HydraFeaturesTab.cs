using UnityEngine;
using HydraMenu.features;
using HydraMenu.routines;
using HydraMenu;
using System.Collections.Generic;

namespace MalumMenu;

public class HydraFeaturesTab : ITab
{
    public string name => "Hydra";

    private int _subTab = 0;

    public void Draw()
    {
        // Sub-tab selection
        GUILayout.BeginHorizontal();
        _subTab = GUILayout.SelectionGrid(_subTab, new[] { "Troll", "Visuals", "Self", "Host", "Movement", "Roles", "Protect", "Sabotage" }, 8);
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        switch (_subTab)
        {
            case 0:
                DrawTroll();
                break;
            case 1:
                DrawVisuals();
                break;
            case 2:
                DrawSelf();
                break;
            case 3:
                DrawHost();
                break;
            case 4:
                DrawMovement();
                break;
            case 5:
                DrawRoles();
                break;
            case 6:
                DrawProtections();
                break;
            case 7:
                DrawSabotage();
                break;
        }
    }

    private void DrawTroll()
    {
        GUILayout.Label("Troll Features", GUIStylePreset.TabSubtitle);

        if (PlayerControl.LocalPlayer == null)
        {
            GUILayout.Label("You are not currently in a game, these options will not work.");
        }

        Troll.AutoReportBodies.Enabled = GUILayout.Toggle(Troll.AutoReportBodies.Enabled, " Auto Report Bodies");
        MalumMenu.routineManager.autoTriggerSpores.Enabled = GUILayout.Toggle(MalumMenu.routineManager.autoTriggerSpores.Enabled, " Auto Trigger Spores");
        Troll.BlockSabotages.Enabled = GUILayout.Toggle(Troll.BlockSabotages.Enabled, " Block Sabotages");
        Troll.BlockVenting.Enabled = GUILayout.Toggle(Troll.BlockVenting.Enabled, " Disable Vents");

        if (GUILayout.Button("Fuck Start Timer"))
        {
            System.Random rnd = new System.Random();
            int counter = rnd.Next(-128, 127);
            PlayerControl.LocalPlayer.RpcSetStartCounter(counter);
            GameStartManager.Instance.SetStartCounter((sbyte)counter);
        }

        if (GUILayout.Button("Trigger All Spores"))
        {
            if (HydraMenu.Utilities.GetCurrentMap() != MapNames.Fungle)
            {
                MalumMenu.notificationManager.Send("Trigger Spores", "This option only works on the Fungle map.");
            }
            else
            {
                FungleShipStatus shipStatus = ShipStatus.Instance.Cast<FungleShipStatus>();
                foreach (Mushroom mushroom in shipStatus.sporeMushrooms.Values)
                {
                    PlayerControl.LocalPlayer.RpcTriggerSpores(mushroom);
                }
                MalumMenu.notificationManager.Send("Trigger Spores", "All spores have been triggered.", 5);
            }
        }

        if (GUILayout.Button("Copy Random Player"))
        {
            PlayerControl randomPl = HydraMenu.Utilities.GetRandomPlayer();
            HydraMenu.Utilities.CopyPlayer(randomPl);
        }

        GUILayout.Space(5);

        GUILayout.Label("Door Troller:", GUIStylePreset.TabSubtitle);
        MalumMenu.routineManager.doorTroller.Enabled = GUILayout.Toggle(MalumMenu.routineManager.doorTroller.Enabled, " Enabled");

        GUILayout.Label($"Lock and Unlock Delay: {MalumMenu.routineManager.doorTroller.lockAndUnlockDelay:F2}s");
        MalumMenu.routineManager.doorTroller.lockAndUnlockDelay = GUILayout.HorizontalSlider(MalumMenu.routineManager.doorTroller.lockAndUnlockDelay, 0.1f, 2.0f);
    }

    private void DrawVisuals()
    {
        GUILayout.Label("Visual Features", GUIStylePreset.TabSubtitle);

        Visuals.SkipShhhAnimation.Enabled = GUILayout.Toggle(Visuals.SkipShhhAnimation.Enabled, " Skip Shhh Animation");
        Visuals.AccurateDisconnectReasons.Enabled = GUILayout.Toggle(Visuals.AccurateDisconnectReasons.Enabled, " Use More Accurate Disconnection Reasons");
        Visuals.Fullbright.Enabled = GUILayout.Toggle(Visuals.Fullbright.Enabled, " Fullbright");
        Visuals.ShowProtections.Enabled = GUILayout.Toggle(Visuals.ShowProtections.Enabled, " Show Guardian Angel Protections");
        Chat.AlwaysVisibleChat.Enabled = GUILayout.Toggle(Chat.AlwaysVisibleChat.Enabled, " Always Visible Chat");
        Visuals.ShowGhosts.Enabled = GUILayout.Toggle(Visuals.ShowGhosts.Enabled, " Show Ghosts");
        Chat.OnChat.ShowMessagesByGhosts = GUILayout.Toggle(Chat.OnChat.ShowMessagesByGhosts, " Show Messages By Ghosts");
    }

    private void DrawSelf()
    {
        GUILayout.Label("Self Features", GUIStylePreset.TabSubtitle);

        if (PlayerControl.LocalPlayer == null || PlayerControl.LocalPlayer.Data == null)
        {
            GUILayout.Label("You are not currently in a game, these options will not work.");
        }
        else
        {
            GUILayout.Label($"Role: {PlayerControl.LocalPlayer.Data.RoleType}");
        }

        Self.UpdateStatsFreeplay.Enabled = GUILayout.Toggle(Self.UpdateStatsFreeplay.Enabled, " Update Stats in Freeplay");
        Self.AlwaysShowTaskAnimations = GUILayout.Toggle(Self.AlwaysShowTaskAnimations, " Always Show Task Animations");
        Self.NoLadderCooldown.Enabled = GUILayout.Toggle(Self.NoLadderCooldown.Enabled, " No Ladder Cooldown");
        Self.UnlimitedMeetings.enabled = GUILayout.Toggle(Self.UnlimitedMeetings.enabled, " Unlimited Meetings");

        if (GUILayout.Button("Call Meeting"))
        {
            if (AmongUsClient.Instance.AmHost)
            {
                HydraMenu.Utilities.OpenMeeting(PlayerControl.LocalPlayer, null);
            }
            else
            {
                PlayerControl.LocalPlayer.CmdReportDeadBody(null);
            }
        }

        GUILayout.Label($"Speed Modifier: {Self.PlayerSpeedModifier.Multiplier:F2}x");
        Self.PlayerSpeedModifier.Multiplier = GUILayout.HorizontalSlider(Self.PlayerSpeedModifier.Multiplier, 0f, 5f);
    }

    private void DrawHost()
    {
        GUILayout.Label("Host Features", GUIStylePreset.TabSubtitle);

        if (PlayerControl.LocalPlayer == null)
        {
            GUILayout.Label("You are not currently in a game, these options will not work.");
        }
        else if (!AmongUsClient.Instance.AmHost)
        {
            GUILayout.Label("You are not the host of the current lobby.");
        }

        Host.BanMidGame.Enabled = GUILayout.Toggle(Host.BanMidGame.Enabled, " Be able to ban players mid-game");
        Host.FlippedSkeld = GUILayout.Toggle(Host.FlippedSkeld, " Use Flipped Skeld Map");
        Host.DisableMeetings.Enabled = GUILayout.Toggle(Host.DisableMeetings.Enabled, " Disable Meetings");
        Host.DisableSabotages.Enabled = GUILayout.Toggle(Host.DisableSabotages.Enabled, " Disable Sabotages");
        Host.DisableCloseDoors.Enabled = GUILayout.Toggle(Host.DisableCloseDoors.Enabled, " Disable Close Doors");
        Host.DisableCameras.Enabled = GUILayout.Toggle(Host.DisableCameras.Enabled, " Disable Security Cameras");
        Host.DisableGameEnd.Enabled = GUILayout.Toggle(Host.DisableGameEnd.Enabled, " Disable Game End");

        GUILayout.BeginHorizontal();
        Host.BlockLowLevels.Enabled = GUILayout.Toggle(Host.BlockLowLevels.Enabled, $" Kick players with less than {Host.BlockLowLevels.MinLevel} levels");
        Host.BlockLowLevels.MinLevel = (uint)GUILayout.HorizontalSlider(Host.BlockLowLevels.MinLevel, 0, 100);
        GUILayout.EndHorizontal();

        MalumMenu.routineManager.reportBodySpam.Enabled = GUILayout.Toggle(MalumMenu.routineManager.reportBodySpam.Enabled, " Spam Report Bodies");

        if (GUILayout.Button("Force Start Game"))
        {
            AmongUsClient.Instance.StartGame();
        }

        if (GUILayout.Button("Kill Everyone"))
        {
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                PlayerControl.LocalPlayer.RpcMurderPlayer(player, true);
            }
        }

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Force Crewmate Victory"))
        {
            Host.DisableGameEnd.Enabled = false;
            GameManager.Instance.RpcEndGame(GameOverReason.CrewmatesByTask, false);
            MalumMenu.notificationManager.Send("Game Finished", "You ended the game with a crewmate victory.", 5);
        }

        if (GUILayout.Button("Force Imposter Victory"))
        {
            Host.DisableGameEnd.Enabled = false;
            GameManager.Instance.RpcEndGame(GameOverReason.ImpostorsByKill, false);
            MalumMenu.notificationManager.Send("Game Finished", "You ended the game with an imposter victory.", 5);
        }
        GUILayout.EndHorizontal();
    }

    private void DrawMovement()
    {
        GUILayout.Label("Movement Features", GUIStylePreset.TabSubtitle);

        if (PlayerControl.LocalPlayer == null)
        {
            GUILayout.Label("You are not currently in a game, these options will not work.");
        }
        else
        {
            Vector2 position = PlayerControl.LocalPlayer.transform.position;
            GUILayout.Label($"Current Map: {HydraMenu.Utilities.GetCurrentMap()}\nCurrent Position:\nX: {position.x:F2}\nY: {position.y:F2}");
            PlayerControl.LocalPlayer.Collider.enabled = !GUILayout.Toggle(!PlayerControl.LocalPlayer.Collider.enabled, " Noclip");
        }

        GUILayout.Label($"Speed Modifier: {Self.PlayerSpeedModifier.Multiplier:F2}x");
        Self.PlayerSpeedModifier.Multiplier = GUILayout.HorizontalSlider(Self.PlayerSpeedModifier.Multiplier, 0f, 5f);

        GUILayout.Space(10);
        GUILayout.Label("Teleportation", GUIStylePreset.TabSubtitle);

        Teleporter.UseSnapToRPC = GUILayout.Toggle(Teleporter.UseSnapToRPC, " Use SnapTo RPC For Teleports");
        GUILayout.Label("Teleport To Location:");

        Dictionary<string, Vector2> teleportLocations = Teleporter.GetTeleportLocations();

        byte i = 0;
        foreach (var (key, value) in teleportLocations)
        {
            if (i % 2 == 0)
            {
                GUILayout.BeginHorizontal();
            }

            if (GUILayout.Button(key))
            {
                Teleporter.TeleportTo(value);
            }

            if (i % 2 != 0)
            {
                GUILayout.EndHorizontal();
            }

            i++;
        }

        // If the amount of teleport locations is an odd number then we won't be ending the horizontal layout, so we check if we need to end it here
        if (i % 2 != 0)
        {
            GUILayout.EndHorizontal();
        }
    }

    private void DrawRoles()
    {
        GUILayout.Label("Role Features", GUIStylePreset.TabSubtitle);

        Roles.AllowVentingForCrewmates = GUILayout.Toggle(Roles.AllowVentingForCrewmates, " Vent As Crewmate");
        Roles.MoveModifier.MoveInVents = GUILayout.Toggle(Roles.MoveModifier.MoveInVents, " Move In Vents");
        Roles.SkipSabotageChecks.SabotageAsCrewmate = GUILayout.Toggle(Roles.SkipSabotageChecks.SabotageAsCrewmate, " Sabotage As Crewmate");
        Roles.SkipSabotageChecks.SabotageInVents = GUILayout.Toggle(Roles.SkipSabotageChecks.SabotageInVents, " Allow Sabotaging In Vents As Imposter");
        Roles.NoKillCooldown.Enabled = GUILayout.Toggle(Roles.NoKillCooldown.Enabled, " No Kill Cooldown");
        Roles.DisableShapeshiftAnimation = GUILayout.Toggle(Roles.DisableShapeshiftAnimation, " Disable Shapeshift Animation");
    }

    private void DrawProtections()
    {
        GUILayout.Label("Protection Features", GUIStylePreset.TabSubtitle);

        Protections.ForceDTLS.Enabled = GUILayout.Toggle(Protections.ForceDTLS.Enabled, " Force enable DTLS to encrypt network data");
        Protections.BlockServerTeleports.Enabled = GUILayout.Toggle(Protections.BlockServerTeleports.Enabled, " Block position updates from server");
        Protections.HardenedReadPackedUInt.Enabled = GUILayout.Toggle(Protections.HardenedReadPackedUInt.Enabled, " Use hardened packed int deserializer");
        Protections.BlockInvalidVentOverload = GUILayout.Toggle(Protections.BlockInvalidVentOverload, " Protect against invalid vent overload");
        Protections.BlockInvalidLadderOverload = GUILayout.Toggle(Protections.BlockInvalidLadderOverload, " Protect against invalid ladder overload");

        GUILayout.Label("Spoofer Features");
        Spoofer.shouldSpoofVersion = GUILayout.Toggle(Spoofer.shouldSpoofVersion, " Enable Version Spoofing");
        Spoofer.useModdedProtocol = GUILayout.Toggle(Spoofer.useModdedProtocol, " Use Modded Protocol");
    }

    private void DrawSabotage()
    {
        GUILayout.Label("Sabotage Features", GUIStylePreset.TabSubtitle);

        if (ShipStatus.Instance == null)
        {
            GUILayout.Label("You are not currently in a game, or the game has not started yet. These options will not work.");
        }

        Sabotage.UpdateSystemsDirectly = GUILayout.Toggle(Sabotage.UpdateSystemsDirectly, " Update Sabotage Systems Directly");

        Dictionary<string, SystemTypes> sabotages = Sabotage.GetSabotages();
        Dictionary<string, SystemTypes> doors = Sabotage.GetDoors();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Sabotage All"))
        {
            Sabotage.SabotageAll();
            MalumMenu.notificationManager.Send("Sabotage", "All sabotages have been enabled.", 5);
        }

        if (GUILayout.Button("Close All Doors"))
        {
            Sabotage.LockAll();
            MalumMenu.notificationManager.Send("Sabotage", "All doors have been closed.", 5);
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Fix All Sabotages"))
        {
            Sabotage.FixAllSabotages();
            MalumMenu.notificationManager.Send("Sabotage", "All sabotages have been repaired.", 5);
        }

        if (GUILayout.Button("Unlock All Doors"))
        {
            if (Sabotage.CanUnlockDoors())
            {
                Sabotage.UnlockAll();
                MalumMenu.notificationManager.Send("Sabotage", "All doors have been unlocked.", 5);
            }
            else
            {
                MalumMenu.notificationManager.Send("Sabotage", "The map you are currently on does not support unlocking doors.", 10);
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(5);
        GUILayout.Label("Sabotages:");
        foreach (var (key, value) in sabotages)
        {
            if (GUILayout.Button(key))
            {
                Sabotage.SabotageSystem(value);
                MalumMenu.notificationManager.Send("Sabotage", $"{key} has been sabotaged.", 5);
            }
        }

        GUILayout.Label("Close Doors:");
        if (doors.Count == 0)
        {
            GUILayout.Label("This map has no doors that can be closed.");
            return;
        }

        byte i = 0;
        foreach (var (key, value) in doors)
        {
            if (i % 2 == 0)
            {
                GUILayout.BeginHorizontal();
            }

            if (GUILayout.Button(key))
            {
                Sabotage.LockDoor(value);
            }

            if (i % 2 != 0)
            {
                GUILayout.EndHorizontal();
            }

            i++;
        }

        // If the amount of door sabotages is an odd number then we won't be ending the horizontal layout, so we check if we need to end it here
        if (i % 2 != 0)
        {
            GUILayout.EndHorizontal();
        }
    }
}

