using System;
using HarmonyLib;
using InnerNet;

namespace MalumMenu.features
{
	internal class PlayerLogger
	{
		private const int HandlingId = 40004;

		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Start))]
		class OnJoin
		{
			static void Postfix(PlayerControl __instance)
			{
				try
				{
					if(__instance == PlayerControl.LocalPlayer || AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay) return;

					ClientData clientData = AmongUsClient.Instance.GetClientFromCharacter(__instance);
					if(clientData == null) return;

					PlatformSpecificData platformData = clientData.PlatformData;

					MalumMenu.Log.LogMessage($"[PlayerLogger] {clientData.PlayerName} ({__instance.NetId}) joined on {platformData.Platform}. friendcode {clientData.FriendCode}, puid {clientData.ProductUserId}");
				}
				catch (Exception ex)
				{
					ErrorReporter.Report(ex, HandlingId, "OnJoin.Postfix: logging joining player info");
				}

			}
		}
	}
}
