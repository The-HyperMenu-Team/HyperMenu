using Hazel;

namespace MalumMenu.anticheat.rpc
{
	internal class SetColor : RpcCheck
	{
		public override void Validate(PlayerControl player, MessageReader reader, ref bool blockRpc)
		{
			uint netId = reader.ReadUInt32();
			byte color = reader.ReadByte();

			if(netId != player.Data.NetId)
			{
				blockRpc = true;
				Anticheat.Flag(player, $"SetColor RPC sent for {player.Data.PlayerName} contains invalid net id, expected {player.Data.NetId}, received {netId}", false);
			}

			if(color >= Palette.ColorNames.Length)
			{
				blockRpc = true;
				Anticheat.Flag(player, $"SetColor RPC sent for {player.Data.PlayerName} contains an invalid color: {color}", false);
			}

			if(blockRpc)
			{
				player.SetColor((byte)CrewmateColor.Red);
			}
		}

		public override RpcCalls GetRpcCall()
		{
			return RpcCalls.SetColor;
		}

		public override bool IsHostOnly()
		{
			return true;
		}
	}
}
