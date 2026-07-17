using Hazel;
using InnerNet;
using System;

namespace MalumMenu.anticheat.rpc
{
    internal class AddVote : RpcCheck
    {
        public override void Validate(PlayerControl player, MessageReader reader, ref bool blockRpc)
        {
            int source = reader.ReadInt32();
            int target = reader.ReadInt32();

            ClientData client = AmongUsClient.Instance.FindClientById(source);
            if(client == null || client.Character == null)
            {
                MalumMenu.Log.LogInfo($"An unknown client id ({source}) attempted to votekick {target}");
                blockRpc = true;
                return;
            }

            player = client.Character;

            if(player.Data.IsDead)
            {
                Anticheat.Flag(player, $"{player.Data.PlayerName} attempted to votekick a player while dead.");
                blockRpc = true;
                return;
            }

            if(MeetingHud.Instance == null)
            {
                Anticheat.Flag(player, $"{player.Data.PlayerName} attempted to votekick a player outside of a meeting.");
                blockRpc = true;
                return;
            }
        }

        public override RpcCalls GetRpcCall()
        {
            return RpcCalls.AddVote;
        }

        public override Type GetExpectedNetObject()
        {
            return typeof(VoteBanSystem);
        }
    }
}
