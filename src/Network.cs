using AmongUs.GameOptions;
using AmongUs.InnerNet.GameDataMessages;
using HarmonyLib;
using Hazel;
using InnerNet;
using UnityEngine;

namespace MalumMenu
{
    internal class Network
    {
        internal class Constants
        {
            public enum OwnerIds
            {
                Everyone = -1,
                Host = -2,
                Self = -3,
                Server = -4
            }

            public enum SpawnType
            {
                SkeldShipStatus = 0,
                MeetingHud = 1,
                LobbyBehavior = 2,
                PlayerControl = 4,
                MiraShipStatus = 5,
                PolusShipStatus = 6,
                DleksShipStatus = 7,
                AirshipShipStatus = 8,
                GameManager = 10,
                NetworkedPlayerInfo = 11,
                VoteBanSysten = 12,
                FungleShipStatus = 13
            }
        }

        internal class RPCEmitter
        {
            public static void SendSetScanner(bool scanning)
            {
                byte scanCount = ++PlayerControl.LocalPlayer.scannerCount;

                PlayerControl.LocalPlayer.SetScanner(scanning, scanCount);

                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
                    PlayerControl.LocalPlayer.NetId,
                    (byte)RpcCalls.SetScanner,
                    SendOption.Reliable,
                    -1
                );

                writer.Write(scanning);
                writer.Write(scanCount);

                AmongUsClient.Instance.FinishRpcImmediately(writer);
            }

            public static void SendPlayAnimation(byte animation)
            {
                PlayerControl.LocalPlayer.PlayAnimation(animation);

                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
                    PlayerControl.LocalPlayer.NetId,
                    (byte)RpcCalls.PlayAnimation,
                    SendOption.None,
                    -1
                );

                writer.Write(animation);

                AmongUsClient.Instance.FinishRpcImmediately(writer);
            }
        }

        public class BatchedMessage
        {
            public readonly MessageWriter writer;
            public readonly int targetClientId;

            public BatchedMessage(int targetClientId = -1)
            {
                writer = MessageWriter.Get(SendOption.Reliable);
                this.targetClientId = targetClientId;

                if (targetClientId == -1)
                {
                    writer.StartMessage(InnerNet.Tags.GameData);
                    writer.Write(AmongUsClient.Instance.GameId);
                }
                else
                {
                    writer.StartMessage(InnerNet.Tags.GameDataTo);
                    writer.Write(AmongUsClient.Instance.GameId);
                    writer.WritePacked(targetClientId);
                }
            }

            private bool AmTarget
            {
                get { return targetClientId == -1 || targetClientId == AmongUsClient.Instance.ClientId; }
            }

            public void QueueDataFlag(InnerNetObject netObj)
            {
                writer.StartMessage((byte)GameDataTypes.DataFlag);
                writer.WritePacked(netObj.NetId);
                netObj.Serialize(writer, false);
                writer.EndMessage();
            }

            public void QueueDataFlag(uint netId, MessageWriter msg)
            {
                writer.StartMessage((byte)GameDataTypes.DataFlag);
                writer.WritePacked(netId);
                writer.Write(msg, false);
                writer.EndMessage();
            }

            public void QueueSpawn(InnerNetObject netObject, int ownerId = -2, SpawnFlags flags = SpawnFlags.None)
            {
                SpawnGameDataMessage spawn = AmongUsClient.Instance.CreateSpawnMessage(netObject, ownerId, flags);
                spawn.Serialize(writer);
            }

            public void QueueDespawn(uint netId)
            {
                writer.StartMessage((byte)GameDataTypes.DespawnFlag);
                writer.WritePacked(netId);
                writer.EndMessage();
            }

            public void QueueSceneChange(int clientId, string scene)
            {
                writer.StartMessage((byte)GameDataTypes.SceneChangeFlag);
                writer.WritePacked(clientId);
                writer.Write(scene);
                writer.EndMessage();
            }

            public void QueueCompleteTask(PlayerControl source, byte taskId)
            {
                source.CompleteTask(taskId);

                writer.StartMessage((byte)GameDataTypes.RpcFlag);
                writer.WritePacked(source.NetId);
                writer.Write((byte)RpcCalls.CompleteTask);
                writer.Write(taskId);
                writer.EndMessage();
            }

            public void QueueCheckName(PlayerControl source, string name)
            {
                writer.StartMessage((byte)GameDataTypes.RpcFlag);
                writer.WritePacked(source.NetId);
                writer.Write((byte)RpcCalls.CheckName);
                writer.Write(name);
                writer.EndMessage();
            }

            public void QueueSetName(PlayerControl source, string name)
            {
                if(AmTarget)
                {
                    source.SetName(name);
                }

                writer.StartMessage((byte)GameDataTypes.RpcFlag);
                writer.WritePacked(source.NetId);
                writer.Write((byte)RpcCalls.SetName);
                writer.Write(source.NetId);
                writer.Write(name);
                writer.EndMessage();
            }

            public void QueueSetColor(PlayerControl source, byte color)
            {
                if(AmTarget)
                {
                    source.SetColor(color);
                }

                writer.StartMessage((byte)GameDataTypes.RpcFlag);
                writer.WritePacked(source.NetId);
                writer.Write((byte)RpcCalls.SetColor);
                writer.Write(source.Data.NetId);
                writer.Write(color);
                writer.EndMessage();
            }

            public void QueueReportDeadBody(PlayerControl source, NetworkedPlayerInfo target)
            {
                if (AmongUsClient.Instance.AmHost)
                {
                    source.ReportDeadBody(target);
                    return;
                }

                writer.StartMessage((byte)GameDataTypes.RpcFlag);
                writer.WritePacked(source.NetId);
                writer.Write((byte)RpcCalls.ReportDeadBody);
                writer.Write(target != null ? target.PlayerId : 255);
                writer.EndMessage();
            }

            public void QueueMurderPlayer(PlayerControl source, PlayerControl target, MurderResultFlags result)
            {
                if(AmTarget)
                {
                    source.MurderPlayer(target, result);
                }

                writer.StartMessage((byte)GameDataTypes.RpcFlag);
                writer.WritePacked(source.NetId);
                writer.Write((byte)RpcCalls.MurderPlayer);
                writer.WritePacked(target.NetId);
                writer.Write((int)result);
                writer.EndMessage();
            }

            public void QueueSendChat(PlayerControl source, string text)
            {
                if (HudManager.Instance != null)
                {
                    HudManager.Instance.Chat.AddChat(source, text, true);
                }

                writer.StartMessage((byte)GameDataTypes.RpcFlag);
                writer.WritePacked(source.NetId);
                writer.Write((byte)RpcCalls.SendChat);
                writer.Write(text);
                writer.EndMessage();
            }

            public void QueueSetScanner(PlayerControl source, bool scanning, byte seq)
            {
                source.SetScanner(scanning, seq);

                writer.StartMessage((byte)GameDataTypes.RpcFlag);
                writer.WritePacked(source.NetId);
                writer.Write((byte)RpcCalls.SetScanner);
                writer.Write(scanning);
                writer.Write(seq);
                writer.EndMessage();
            }

            public void QueueSetStartCounter(PlayerControl source, sbyte counter, int seq)
            {
                writer.StartMessage((byte)GameDataTypes.RpcFlag);
                writer.WritePacked(source.NetId);
                writer.Write((byte)RpcCalls.SetStartCounter);
                writer.WritePacked(seq);
                writer.Write(counter);
                writer.EndMessage();
            }

            public void QueueSnapTo(PlayerControl source, Vector2 position)
            {
                if(AmTarget)
                {
                    source.NetTransform.SnapTo(position, (ushort)(source.NetTransform.lastSequenceId + 1));
                }

                ushort seqId = (ushort)(source.NetTransform.lastSequenceId + 2);

                writer.StartMessage((byte)GameDataTypes.RpcFlag);
                writer.WritePacked(source.NetTransform.NetId);
                writer.Write((byte)RpcCalls.SnapTo);
                NetHelpers.WriteVector2(position, writer);
                writer.Write(seqId);
                writer.EndMessage();
            }

            public void QueueCloseMeeting()
            {
                if(AmTarget)
                {
                    MeetingHud.Instance.Close();
                }

                writer.StartMessage((byte)GameDataTypes.RpcFlag);
                writer.WritePacked(MeetingHud.Instance.NetId);
                writer.Write((byte)RpcCalls.CloseMeeting);
                writer.EndMessage();
            }

            public void QueueVotingComplete(MeetingHud.VoterState[] voteStates, NetworkedPlayerInfo ejectedPlayer, bool isTie)
            {
                if(AmTarget)
                {
                    MeetingHud.Instance.VotingComplete(voteStates, ejectedPlayer, isTie);
                }

                writer.StartMessage((byte)GameDataTypes.RpcFlag);
                writer.WritePacked(MeetingHud.Instance.NetId);
                writer.Write((byte)RpcCalls.VotingComplete);

                writer.WritePacked(voteStates.Length);

                foreach (MeetingHud.VoterState state in voteStates)
                {
                    state.Serialize(writer);
                }

                writer.Write(ejectedPlayer.PlayerId);
                writer.Write(isTie);

                writer.EndMessage();
            }

            public void QueueAddVote(int sourceId, int targetId)
            {
                if(AmTarget)
                {
                    VoteBanSystem.Instance.AddVote(sourceId, targetId);
                }

                writer.StartMessage((byte)GameDataTypes.RpcFlag);
                writer.WritePacked(VoteBanSystem.Instance.NetId);
                writer.Write((byte)RpcCalls.AddVote);
                writer.Write(sourceId);
                writer.Write(targetId);
                writer.EndMessage();
            }

            public void QueueSetTasks(NetworkedPlayerInfo player, byte[] tasks)
            {
                player.SetTasks(tasks);

                writer.StartMessage((byte)GameDataTypes.RpcFlag);
                writer.WritePacked(player.NetId);
                writer.Write((byte)RpcCalls.SetTasks);
                writer.WriteBytesAndSize(tasks);
                writer.EndMessage();
            }

            public void QueueSetLevel(PlayerControl source, uint level)
            {
                source.SetLevel(level);

                writer.StartMessage((byte)GameDataTypes.RpcFlag);
                writer.WritePacked(source.NetId);
                writer.Write((byte)RpcCalls.SetLevel);
                writer.WritePacked(level);
                writer.EndMessage();
            }

            public void QueueSetHatStr(PlayerControl source, string hat, byte seqid)
            {
                if(AmTarget)
                {
                    source.SetHat(hat, source.Data.DefaultOutfit.ColorId);
                }

                writer.StartMessage((byte)GameDataTypes.RpcFlag);
                writer.WritePacked(source.NetId);
                writer.Write((byte)RpcCalls.SetHatStr);
                writer.Write(hat);
                writer.Write(seqid);
                writer.EndMessage();
            }

            public void QueueSetSkinStr(PlayerControl source, string skin, byte seqid)
            {
                if(AmTarget)
                {
                    source.SetSkin(skin, source.Data.DefaultOutfit.ColorId);
                }

                writer.StartMessage((byte)GameDataTypes.RpcFlag);
                writer.WritePacked(source.NetId);
                writer.Write((byte)RpcCalls.SetSkinStr);
                writer.Write(skin);
                writer.Write(seqid);
                writer.EndMessage();
            }

            public void QueueSetPetStr(PlayerControl source, string pet, byte seqid)
            {
                if(AmTarget)
                {
                    source.SetPet(pet, source.Data.DefaultOutfit.ColorId);
                }

                writer.StartMessage((byte)GameDataTypes.RpcFlag);
                writer.WritePacked(source.NetId);
                writer.Write((byte)RpcCalls.SetPetStr);
                writer.Write(pet);
                writer.Write(seqid);
                writer.EndMessage();
            }

            public void QueueSetVisorStr(PlayerControl source, string visor, byte seqid)
            {
                if(AmTarget)
                {
                    source.SetVisor(visor, source.Data.DefaultOutfit.ColorId);
                }

                writer.StartMessage((byte)GameDataTypes.RpcFlag);
                writer.WritePacked(source.NetId);
                writer.Write((byte)RpcCalls.SetVisorStr);
                writer.Write(visor);
                writer.Write(seqid);
                writer.EndMessage();
            }

            public void QueueSetNameplateStr(PlayerControl source, string nameplate, byte seqid)
            {
                if(AmTarget)
                {
                    source.SetNamePlate(nameplate);
                }

                writer.StartMessage((byte)GameDataTypes.RpcFlag);
                writer.WritePacked(source.NetId);
                writer.Write((byte)RpcCalls.SetNamePlateStr);
                writer.Write(nameplate);
                writer.Write(seqid);
                writer.EndMessage();
            }

            public void QueueSetRole(PlayerControl source, RoleTypes role, bool canOverride = false)
            {
                if(AmTarget)
                {
                    source.StartCoroutine(source.CoSetRole(role, canOverride));
                }

                writer.StartMessage((byte)GameDataTypes.RpcFlag);
                writer.WritePacked(source.NetId);
                writer.Write((byte)RpcCalls.SetRole);
                writer.Write((ushort)role);
                writer.Write(canOverride);
                writer.EndMessage();
            }

            public void QueueShapeshift(PlayerControl source, PlayerControl target, bool shouldAnimate)
            {
                if(AmTarget)
                {
                    source.Shapeshift(target, shouldAnimate);
                }

                writer.StartMessage((byte)GameDataTypes.RpcFlag);
                writer.WritePacked(source.NetId);
                writer.Write((byte)RpcCalls.Shapeshift);
                writer.WriteNetObject(target);
                writer.Write(shouldAnimate);
                writer.EndMessage();
            }

            public void QueueCheckMurder(PlayerControl source, PlayerControl target)
            {
                if (AmongUsClient.Instance.AmHost)
                {
                    source.CheckMurder(target);
                }

                writer.StartMessage((byte)GameDataTypes.RpcFlag);
                writer.WritePacked(source.NetId);
                writer.Write((byte)RpcCalls.CheckMurder);
                writer.WriteNetObject(target);
                writer.EndMessage();
            }

            public void QueueUpdateSystem(PlayerControl source, SystemTypes system, MessageWriter msg)
            {
                writer.StartMessage((byte)GameDataTypes.RpcFlag);
                writer.WritePacked(ShipStatus.Instance.NetId);
                writer.Write((byte)RpcCalls.UpdateSystem);
                writer.Write((byte)system);
                writer.WriteNetObject(source);
                writer.Write(msg, false);
                writer.EndMessage();
            }

            public void QueueLobbyTimeExpiring(int timer)
            {
                LobbyBehaviour.Instance.HandleLobbyTimerExtensionRequest(69420, false, 255, 0, 0);

                writer.StartMessage((byte)GameDataTypes.RpcFlag);
                writer.WritePacked(LobbyBehaviour.Instance.NetId);
                writer.Write((byte)RpcCalls.LobbyTimeExpiring);
                writer.WritePacked(timer);
                writer.Write(false);
                writer.EndMessage();
            }

            public void FinishBatch()
            {
                writer.EndMessage();
                AmongUsClient.Instance.SendOrDisconnect(writer);
                writer.Recycle();
            }
        }

        internal class DataHandler
        {
            public static readonly int MAX_MESSAGE_LENGTH = 1400;

            [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.HandleGameData))]
            class HandleGameData
            {
                static bool Prefix(InnerNetClient __instance, MessageReader parentReader)
                {
                    if(features.Protections.BlockLargeGameMessages && parentReader.Length > MAX_MESSAGE_LENGTH)
                    {
                        parentReader.Recycle();
                        return false;
                    }

                    try
                    {
                        while(parentReader.BytesRemaining > 0)
                        {
                            MessageReader reader = parentReader.ReadMessageAsNewBuffer();
                            HandleGameDataInner(__instance, reader, ++__instance.msgNum);
                        }
                    }
                    finally
                    {
                        parentReader.Recycle();
                    }

                    return false;
                }
            }

            public static void HandleGameDataInner(InnerNetClient innerNetClient, MessageReader reader, int msgNum)
            {
                GameDataTypes type = (GameDataTypes)reader.Tag;

                if(features.Protections.BlockInvalidGameDataMessages && (type == GameDataTypes.Invalid || type == (GameDataTypes)3 || type > GameDataTypes.ReadyFlag))
                {
                    reader.Recycle();
                    return;
                }

                bool isValid = anticheat.Anticheat.HandleGameData(type, reader);
                if(!isValid)
                {
                    reader.Recycle();
                    return;
                }

                innerNetClient.StartCoroutine(innerNetClient.HandleGameDataInner(reader, msgNum));
            }
        }
    }
}
