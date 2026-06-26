using System.Collections.Generic;
using UnityEngine;

namespace MalumMenu.routines
{
	public class JailPlayerRoutine : IRoutine
	{
		public JailPlayerRoutine()
		{
			RoutineName = "JailPlayer";
		}

		public bool _enabled = false;
		public HashSet<uint> targets = new HashSet<uint>();

		public float delay = 0.5f;
		private float timeElapsed = 0f;

		public override bool Enabled
		{
			get { return _enabled; }
			set
			{
				if(value == _enabled) return;

				if(value)
				{
					if(PlayerControl.LocalPlayer == null || ShipStatus.Instance == null)
					{
						MalumMenu.notifications.Send("Jail Player", "Jail Player can only be used inside of a game.", 10);
						return;
					}

					if(Utilities.IsAnticheatPresent() && !AmongUsClient.Instance.AmHost)
					{
						MalumMenu.notifications.Send("Jail Player", "Jail Player can only be used if you are the host of the lobby.", 10);
						return;
					}
				}
				else
				{
					targets.Clear();
				}

				_enabled = value;
			}
		}

		public override void Run()
		{
			timeElapsed += Time.deltaTime;
			if(timeElapsed < delay) return;
			timeElapsed = 0f;

			if(PlayerControl.LocalPlayer == null || ShipStatus.Instance == null)
			{
				MalumMenu.notifications.Send("Jail Player", "Jail Player has been disabled as you left the game.", 10);
				_enabled = false;
				return;
			}

			GetMapData(out SystemTypes jailRoom, out int ventId);

			foreach(PlayerControl player in PlayerControl.AllPlayerControls)
			{
				if(!targets.Contains(player.NetId)) continue;

				SystemTypes room = GetRoomForPlayer(player);
				if(room != jailRoom)
				{
					player.MyPhysics.RpcBootFromVent(ventId);
				}
			}
		}

		private SystemTypes GetRoomForPlayer(PlayerControl player)
		{
			foreach(PlainShipRoom room in ShipStatus.Instance.AllRooms)
			{
				if(room.roomArea == null) continue;

				int collisions = room.roomArea.OverlapCollider(HudManager.Instance.roomTracker.filter, HudManager.Instance.roomTracker.detectiveBuffer);
				if(RoomTracker.CheckHitsForPlayer(HudManager.Instance.roomTracker.detectiveBuffer, collisions, player))
				{
					return room.RoomId;
				}
			}

			return (SystemTypes)255;
		}

		private void GetMapData(out SystemTypes room, out int ventId)
		{
			MapNames currentMap = Utilities.GetCurrentMap();

			switch(currentMap)
			{
				case MapNames.Skeld:
				case MapNames.Dleks:
					room = SystemTypes.Nav;
					ventId = 12;
					break;

				case MapNames.MiraHQ:
					room = SystemTypes.Decontamination;
					ventId = 9;
					break;

				case MapNames.Polus:
					room = SystemTypes.Storage;
					ventId = 8;
					break;

				case MapNames.Airship:
					room = SystemTypes.GapRoom;
					ventId = 7;
					break;

				case MapNames.Fungle:
					room = SystemTypes.Laboratory;
					ventId = 4;
					break;

				default:
					room = SystemTypes.Nav;
					ventId = 12;
					break;
			}
		}
	}
}
