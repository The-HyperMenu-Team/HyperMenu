using System;
using HarmonyLib;

namespace MalumMenu.features
{
	internal class Chat
	{
		private const int HandlingId = 40001;

		[HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChat))]
		public static class OnChat
		{
			public static bool LogChatMessages { get; set; } = true;
			public static bool ShowMessagesByGhosts { get; set; } = true;

			static void Postfix(ChatController __instance, PlayerControl sourcePlayer, string chatText)
			{
				try
				{
					if(sourcePlayer == null) return;

					if(LogChatMessages) MalumMenu.Log.LogMessage($"[ChatLogger] {sourcePlayer.Data.PlayerName}: {chatText}");

					if(ShowMessagesByGhosts && !PlayerControl.LocalPlayer.Data.IsDead && sourcePlayer.Data.IsDead)
					{
						__instance.AddChatWarning($"{sourcePlayer.Data.PlayerName}\n{chatText}");
					}
				}
				catch (Exception ex)
				{
					ErrorReporter.Report(ex, HandlingId, "OnChat.Postfix: logging chat and showing ghost messages");
				}
			}
		}

		[HarmonyPatch(typeof(ChatController), nameof(ChatController.SetVisible))]
		public static class AlwaysVisibleChat
		{
			public static bool Enabled { get; set; } = true;

			static void Prefix(ref bool visible)
			{
				try
				{
					if(Enabled) visible = true;
				}
				catch (Exception ex)
				{
					ErrorReporter.Report(ex, HandlingId, "AlwaysVisibleChat.Prefix: forcing chat visible");
				}
			}
		}
	}
}
