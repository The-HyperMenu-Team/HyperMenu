using AmongUs.GameOptions;
using Hazel;
using MalumMenu.features;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MalumMenu
{
    internal class Utilities
    {
        private static readonly Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<SkinData> allSkins = HatManager.Instance.allSkins;
        private static readonly Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<HatData> allHats = HatManager.Instance.allHats;
        private static readonly Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<VisorData> allVisors = HatManager.Instance.allVisors;
        private static readonly Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<PetData> allPets = HatManager.Instance.allPets;
        private static readonly Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<NamePlateData> allNameplates = HatManager.Instance.allNamePlates;

        public static int GetRandomUnusedColor()
        {
            List<int> colors = Enumerable.Range(0, 18).ToList();

            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                colors.Remove(player.Data.DefaultOutfit.ColorId);
            }

            System.Random rnd = new System.Random();

            if (colors.Count == 0)
            {
                return rnd.Next(0, 18);
            }

            return colors[rnd.Next(0, colors.Count)];
        }

        public static void RandomizePlayer(bool ingame = false)
        {
            System.Random rnd = new System.Random();

            if (ingame)
            {
                PlayerControl.LocalPlayer.CmdCheckColor((byte)GetRandomUnusedColor());

                PlayerControl.LocalPlayer.RpcSetHat(allHats[rnd.Next(0, allHats.Length)].ProductId);
                PlayerControl.LocalPlayer.RpcSetVisor(allVisors[rnd.Next(0, allVisors.Length)].ProductId);
                PlayerControl.LocalPlayer.RpcSetSkin(allSkins[rnd.Next(0, allSkins.Length)].ProductId);
                PlayerControl.LocalPlayer.RpcSetPet(allPets[rnd.Next(0, allPets.Length)].ProductId);
            }
            else
            {
                PlayerCustomization.EquipSkin(allSkins[rnd.Next(0, allSkins.Length)]);
                PlayerCustomization.EquipHat(allHats[rnd.Next(0, allHats.Length)]);
                PlayerCustomization.EquipVisor(allVisors[rnd.Next(0, allVisors.Length)]);
                PlayerCustomization.EquipPet(allPets[rnd.Next(0, allPets.Length)]);
                PlayerCustomization.EquipNameplate(allNameplates[rnd.Next(0, allNameplates.Length)]);

                AccountManager.Instance.RandomizeName();
            }
        }

        public static PlayerControl GetRandomPlayer(bool excludeHost = false, bool excludeDead = false, bool excludeImposters = false, bool excludeSelf = true)
        {
            Il2CppSystem.Collections.Generic.List<PlayerControl> allPlayers = PlayerControl.AllPlayerControls;
            List<PlayerControl> validPlayers = new List<PlayerControl>();

            foreach (PlayerControl player in allPlayers)
            {
                if (
                    (excludeSelf && AmongUsClient.Instance.ClientId == player.OwnerId) ||
                    (excludeHost && AmongUsClient.Instance.HostId == player.OwnerId) ||
                    (excludeDead && player.Data.IsDead) ||
                    (excludeImposters && player.Data.Role.CanUseKillButton)
                ) continue;

                validPlayers.Add(player);
            }

            if (validPlayers.Count == 0) return null;

            System.Random rnd = new System.Random();
            return validPlayers[rnd.Next(validPlayers.Count)];
        }

        public static void CopyPlayer(PlayerControl player)
        {
            NetworkedPlayerInfo.PlayerOutfit outfit = player.CurrentOutfit;

            bool hasAnticheat = IsAnticheatPresent();

            Network.BatchedMessage batch = new Network.BatchedMessage();

            if (!hasAnticheat)
            {
                batch.QueueSetName(PlayerControl.LocalPlayer, outfit.PlayerName);
            }

            if (!hasAnticheat || AmongUsClient.Instance.AmHost)
            {
                batch.QueueSetColor(PlayerControl.LocalPlayer, (byte)outfit.ColorId);
            }

            batch.QueueSetNameplateStr(PlayerControl.LocalPlayer, outfit.NamePlateId, ++outfit.NamePlateSequenceId);
            batch.QueueSetHatStr(PlayerControl.LocalPlayer, outfit.HatId, ++outfit.HatSequenceId);
            batch.QueueSetVisorStr(PlayerControl.LocalPlayer, outfit.VisorId, ++outfit.VisorSequenceId);
            batch.QueueSetSkinStr(PlayerControl.LocalPlayer, outfit.SkinId, ++outfit.SkinSequenceId);
            batch.QueueSetPetStr(PlayerControl.LocalPlayer, outfit.PetId, ++outfit.PetSequenceId);

            batch.FinishBatch();
        }

        public static void AttemptStartMeeting(PlayerControl reporter, NetworkedPlayerInfo target)
        {
            MalumMenu.Log.LogInfo($"Attempting to start a meeting for {reporter.Data.PlayerName}");

            bool hasAnticheat = IsAnticheatPresent();

            if (hasAnticheat && AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started)
            {
                MalumMenu.notifications.Send("Start Meeting", "The game must have started in order for this feature to work.");
                return;
            }

            if (AmongUsClient.Instance.AmHost)
            {
                MalumMenu.Log.LogInfo($"We are the host so we can directly use the StartMeeting RPC");

                if (ShipStatus.Instance == null)
                {
                    MalumMenu.notifications.Send("Start Meeting", "There must be a valid instance of ShipStatus for this feature to work.");
                }
                else
                {
                    OpenMeeting(reporter, target);
                }

                return;
            }

            MalumMenu.Log.LogInfo("We are not the host so we have to use the ReportDeadBody RPC");

            if (hasAnticheat && reporter != PlayerControl.LocalPlayer)
            {
                MalumMenu.notifications.Send("Start Meeting", "You must be the host of the lobby to make another player start a meeting.");
                return;
            }

            if (reporter.Data.IsDead)
            {
                MalumMenu.notifications.Send("Start Meeting", "You can only call meetings or report bodies if you are alive.");
                return;
            }

            if (hasAnticheat && target != null)
            {
                if (!target.IsDead)
                {
                    MalumMenu.notifications.Send("Start Meeting", "You can only report bodies of players who have died in this round.");
                    return;
                }

                if (!DoesDeadBodyExist(target.PlayerId))
                {
                    MalumMenu.notifications.Send("Start Meeting", "Unable to find a dead body for this player, you can only report a player's body if they have died this round and their body has not dissolved.");
                    return;
                }
            }

            Network.BatchedMessage batch = new Network.BatchedMessage();
            batch.QueueReportDeadBody(reporter, target);
            batch.FinishBatch();
        }

        public static void OpenMeeting(PlayerControl reporter, NetworkedPlayerInfo target)
        {
            MeetingRoomManager.Instance.AssignSelf(reporter, target);
            reporter.RpcStartMeeting(target);
            HudManager.Instance.OpenMeetingRoom(reporter);
        }

        public static bool DoesDeadBodyExist(byte playerId)
        {
            foreach (Collider2D collider in Physics2D.OverlapCircleAll(new Vector2(0, 0), 99999f, Constants.PlayersOnlyMask))
            {
                if (collider.tag != "DeadBody") continue;

                DeadBody bodyComponent = collider.GetComponent<DeadBody>();
                if (bodyComponent && bodyComponent.ParentId == playerId)
                {
                    return true;
                }
            }

            return false;
        }

        public static void ShapeshiftPlayer(PlayerControl victim, PlayerControl target, bool shouldAnimate = true)
        {
            bool hasAnticheat = IsAnticheatPresent();

            if (hasAnticheat && !AmongUsClient.Instance.AmHost)
            {
                MalumMenu.notifications.Send("Shapeshift Player", "You must be the host of the lobby in order to use this feature.");
                return;
            }

            if (hasAnticheat && AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started)
            {
                MalumMenu.notifications.Send("Shapeshift Player", "The game must have started for this option to work.");
                return;
            }

            Network.BatchedMessage batch = new Network.BatchedMessage();

            if (hasAnticheat && victim.Data.RoleType != RoleTypes.Shapeshifter)
            {
                RoleTypes currentRole = victim.Data.RoleType;

                batch.QueueSetRole(victim, RoleTypes.Shapeshifter, true);
                batch.QueueShapeshift(victim, target, shouldAnimate);
                batch.QueueSetRole(victim, currentRole, true);
            }
            else
            {
                batch.QueueShapeshift(victim, target, shouldAnimate);
            }

            batch.FinishBatch();
        }

        public static MapNames GetCurrentMap()
        {
            if(ShipStatus.Instance == null)
            {
                if(AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay)
                {
                    return (MapNames)AmongUsClient.Instance.TutorialMapId;
                }
                else
                {
                    return (MapNames)GameOptionsManager.Instance.CurrentGameOptions.MapId;
                }
            }

            return (Network.Constants.SpawnType)ShipStatus.Instance.SpawnId switch
            {
                Network.Constants.SpawnType.SkeldShipStatus => MapNames.Skeld,
                Network.Constants.SpawnType.DleksShipStatus => MapNames.Dleks,
                Network.Constants.SpawnType.MiraShipStatus => MapNames.MiraHQ,
                Network.Constants.SpawnType.PolusShipStatus => MapNames.Polus,
                Network.Constants.SpawnType.AirshipShipStatus => MapNames.Airship,
                Network.Constants.SpawnType.FungleShipStatus => MapNames.Fungle,
                _ => MapNames.Skeld
            };
        }

        public static bool IsAnticheatPresent()
        {
            if (Constants.IsVersionModded() || PlayerControl.LocalPlayer == null || PlayerControl.LocalPlayer.Data == null) return false;

            return PlayerControl.LocalPlayer.Data.OwnerId != (int)Network.Constants.OwnerIds.Host;
        }

        public static string GetPlayerColor(NetworkedPlayerInfo player)
        {
            int colorId = player.DefaultOutfit.ColorId;

            if (colorId < 0 || colorId >= Palette.ColorNames.Length)
            {
                return "Fortegreen";
            }

            return player.GetPlayerColorString();
        }
    }
}
