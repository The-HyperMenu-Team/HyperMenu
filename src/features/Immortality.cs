using HarmonyLib;
using InnerNet;

namespace MalumMenu.features
{
	internal class Immortality
	{
		private static readonly int CUSTOM_VENT_ID = 50;

		private static bool _enabled = false;

		public static bool Enabled
		{
			get
			{
				return _enabled;
			}
			set
			{
				if(value == _enabled) return;

				if(ShipStatus.Instance == null)
				{
					MalumMenu.notifications.Send("Immortality", "This option can only be enabled when you are inside a game.");
					return;
				}

				if(value && !PlayerControl.LocalPlayer.inVent)
				{
					MalumMenu.Log.LogInfo("Immortality was enabled, sending a VentilationSystem update with operation Enter");
					VentilationSystem.Update(VentilationSystem.Operation.Enter, CUSTOM_VENT_ID);
				}

				if(!value && !PlayerControl.LocalPlayer.inVent)
				{
					MalumMenu.Log.LogInfo("Immortality was disabled, sending a VentilationSystem update with operation Exit");
					VentilationSystem.Update(VentilationSystem.Operation.Exit, CUSTOM_VENT_ID);
				}

				_enabled = value;
			}
		}

		[HarmonyPatch(typeof(VentilationSystem), nameof(VentilationSystem.Update))]
		class BlockSendingUpdates
		{
			static bool Prefix(VentilationSystem.Operation op, int ventId)
			{
				if(ventId != CUSTOM_VENT_ID && Enabled && (op == VentilationSystem.Operation.Enter || op == VentilationSystem.Operation.Exit || op == VentilationSystem.Operation.Move))
				{
					MalumMenu.Log.LogInfo($"Our client sent VentilationSystem operation {op} for vent {ventId}, cancelling..");
					return false;
				}

				return true;
			}
		}

		[HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.DisconnectInternal))]
		class OnDisconnect
		{
			static void Prefix()
			{
				_enabled = false;
			}
		}

		[HarmonyPatch(typeof(GameManager), nameof(GameManager.StartGame))]
		class OnGameStart
		{
			static void Postfix()
			{
				if(!Enabled) return;

				MalumMenu.Log.LogMessage($"A new instance of ShipStatus has spawned, sending the immortality RPC");
				VentilationSystem.Update(VentilationSystem.Operation.Enter, CUSTOM_VENT_ID);
			}
		}

		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
		class OnMurder
		{
			static void Postfix(PlayerControl __instance, PlayerControl target)
			{
				if(Enabled && target == PlayerControl.LocalPlayer)
				{
					MalumMenu.notifications.Send("Immortality", $"{__instance.Data.PlayerName} attempted to kill you!", 5);
				}
			}
		}

		[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Close))]
		class OnMeetingEnd
		{
			static void Postfix()
			{
				if(!Enabled || PlayerControl.LocalPlayer.Data.IsDead) return;

				MalumMenu.Log.LogInfo("Meeting has ended, resending Immortality RPC to retain immortal status");
				VentilationSystem.Update(VentilationSystem.Operation.Enter, CUSTOM_VENT_ID);
			}
		}
	}
}