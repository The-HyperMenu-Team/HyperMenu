using Hazel;

namespace MalumMenu.anticheat.rpc
{
	internal class UsePlatform : RpcCheck
	{
		public override void Validate(PlayerControl player, MessageReader reader, ref bool blockRpc)
		{
			MapNames map = Utilities.GetCurrentMap();
			if(map != MapNames.Airship)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} tried to use a platform outside of the proper map.");
				blockRpc = true;
				return;
			}

			if(ShipStatus.Instance == null)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} tried to use a platform when there is no instance of ShipStatus.");
				blockRpc = true;
				return;
			}

			if(GameManager.Instance.IsHideAndSeek())
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} tried to use a platform while in Hide and Seek.");
				blockRpc = true;
				return;
			}
		}

		public override RpcCalls GetRpcCall()
		{
			return RpcCalls.UsePlatform;
		}
	}
}
