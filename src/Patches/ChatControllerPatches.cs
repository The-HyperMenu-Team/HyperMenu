using AmongUs.QuickChat;
using HarmonyLib;
using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace MalumMenu;


[HarmonyPatch(typeof(QuickChatMenu))]
public static class QuickChatMenuOpenPatch
{
    private const int HandlingId = 30003;
    [HarmonyPatch(nameof(QuickChatMenu.Open))]
    [HarmonyPostfix]
    public static void Postfix(QuickChatMenu __instance)
    {
        try
        {
            // Safeguard against null instances when the side panel triggers
            if (__instance == null) return;

            // In recent versions, QuickChatMenu populates a list of controller elements dynamically.
            // We use reflection or explicit properties to access the text container if direct references break.
            // Let's look for the active category controller items that the game renders on screen
            // If direct class types are missing, they are typically bound within an internal array or list.
            try
            {
                // We fetch the items array/list reflecting the visual elements
                foreach (var field in typeof(QuickChatMenu).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance))
                {
                    // Check if the field is an array or collection containing items
                    if (field.Name.ToLower().Contains("items") || field.Name.ToLower().Contains("elements"))
                    {
                        var value = field.GetValue(__instance);
                        if (value == null) continue;

                        // Check your console logs if you need to verify the internal layout names!
                        System.Console.WriteLine($"[MalumMenu] Found chat field container: {field.Name}");
                    }
                }
            }
            catch (System.Exception ex)
            {
                ErrorReporter.Report(ex, HandlingId, "QuickChatMenuOpenPatch.Postfix: inspect chat fields");
                System.Console.WriteLine($"[MalumMenu] Custom injection exception handled: {ex.Message}");
            }
        }
        catch (Exception ex) { ErrorReporter.Report(ex, HandlingId, "QuickChatMenuOpenPatch.Postfix: inspect quick chat menu"); }
    }
}

// --- GLOBAL COLOR PATCH ---
// This runs first and modifies the text for EVERYONE.
// Because it uses 'ref', the modified text is passed into your second patch automatically.
[HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChat))]
public static class ChatColorGlobalPatch
{
    private const int HandlingId = 30003;
    [HarmonyPriority(Priority.High)]
    public static void Prefix(PlayerControl sourcePlayer, ref string chatText)
    {
        try
        {
            // 1. CONFLICT RESOLVER: If both are on, turn both off.
            if (CheatToggles.changeChatColor && CheatToggles.colorAsPlayer)
            {
                CheatToggles.changeChatColor = false;
                CheatToggles.colorAsPlayer = false;
                return;
            }

            // 2. MODE: Static Menu Color
            if (CheatToggles.changeChatColor && !string.IsNullOrEmpty(MalumMenu.menuChatColor.Value))
            {
                chatText = $"<color={MalumMenu.menuChatColor.Value}>{chatText}</color>";
            }
            
            // 3. MODE: Color as Player (Dynamic)
            else if (CheatToggles.colorAsPlayer && sourcePlayer != null && sourcePlayer.Data != null)
            {
                // In many versions, ColorId is inside DefaultOutfit
                // Use null checks to ensure we don't crash if an outfit isn't loaded yet
                if (sourcePlayer.Data.DefaultOutfit != null)
                {
                    int colorId = sourcePlayer.Data.DefaultOutfit.ColorId;

                    // Ensure the ID is within the bounds of the Palette array
                    if (colorId >= 0 && colorId < Palette.PlayerColors.Length)
                    {
                        Color pColor = Palette.PlayerColors[colorId];
                        string hex = $"#{ColorUtility.ToHtmlStringRGB(pColor)}";
                        chatText = $"<color={hex}>{chatText}</color>";
                    }
                }
            }
        }
        catch (Exception ex) { ErrorReporter.Report(ex, HandlingId, "ChatColorGlobalPatch.Prefix: apply chat color"); }
    }
}

// --- GHOST INTERCEPTION PATCH ---
// Exactly as you provided, no changes made to the logic or naming.
[HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChat))]
public static class ChatController_AddChat
{
    private const int HandlingId = 30003;
    // Prefix patch of ChatController.AddChat to receive ghost messages if CheatSettings.seeGhosts is enabled even if LocalPlayer is alive
    // Basically does what the original method did with the required modifications
    public static bool Prefix(PlayerControl sourcePlayer, string chatText, bool censor, ChatController __instance)
    {
        try
        {
            // Simply run original method if seeGhosts is disabled or LocalPlayer already dead
            if (!CheatToggles.seeGhosts || PlayerControl.LocalPlayer.Data.IsDead) return true;

            if (!sourcePlayer || !PlayerControl.LocalPlayer) return true;

            NetworkedPlayerInfo data = PlayerControl.LocalPlayer.Data;
            NetworkedPlayerInfo data2 = sourcePlayer.Data;

            if (data2 == null || data == null) return true; // Remove isDead check for LocalPlayer

            ChatBubble pooledBubble = __instance.GetPooledBubble();

            try
            {
                pooledBubble.transform.SetParent(__instance.scroller.Inner);
                pooledBubble.transform.localScale = Vector3.one;
                bool flag = sourcePlayer == PlayerControl.LocalPlayer;
                if (flag)
                {
                    pooledBubble.SetRight();
                }
                else
                {
                    pooledBubble.SetLeft();
                }
                bool didVote = MeetingHud.Instance && MeetingHud.Instance.DidVote(sourcePlayer.PlayerId);
                pooledBubble.SetCosmetics(data2);
                __instance.SetChatBubbleName(pooledBubble, data2, data2.IsDead, didVote, PlayerNameColor.Get(data2), null);
                if (censor && AmongUs.Data.DataManager.Settings.Multiplayer.CensorChat)
                {
                    chatText = BlockedWords.CensorWords(chatText, false);
                }
                pooledBubble.SetText(chatText);
                pooledBubble.AlignChildren();
                __instance.AlignAllBubbles();
                if (!__instance.IsOpenOrOpening && __instance.notificationRoutine == null)
                {
                    __instance.notificationRoutine = __instance.StartCoroutine(__instance.BounceDot());
                }
                if (!flag && !__instance.IsOpenOrOpening)
                {
                    SoundManager.Instance.PlaySound(__instance.messageSound, false).pitch = 0.5f + sourcePlayer.PlayerId / 15f;
                    __instance.chatNotification.SetUp(sourcePlayer, chatText);
                }
            }
            catch (Exception message)
            {
                ErrorReporter.Report(message, HandlingId, "ChatController_AddChat.Prefix: build ghost chat bubble");
                ChatController.Logger.Error(message.ToString(), null);
                __instance.chatBubblePool.Reclaim(pooledBubble);
            }

            return false; // Skips the original method completely
        }
        catch (Exception ex) { ErrorReporter.Report(ex, HandlingId, "ChatController_AddChat.Prefix: intercept ghost chat"); return true; }
    }
}

[HarmonyPatch(typeof(ChatController), nameof(ChatController.Update))]
public static class ChatController_Update
{
    private const int HandlingId = 30003;
    // Postfix patch of ChatController.Update to unlock longer message length
    public static void Postfix(ChatController __instance)
    {
        try
        {
            //__instance.freeChatField.textArea.allowAllCharacters = CheatToggles.chatJailbreak; // Not really used by the game's code, but I include it anyway
            //__instance.freeChatField.textArea.AllowSymbols = true; // Allow sending certain symbols
            //__instance.freeChatField.textArea.AllowEmail = CheatToggles.chatJailbreak; // Allow sending email addresses when chatJailbreak is enabled
            //__instance.freeChatField.textArea.AllowPaste = CheatToggles.chatJailbreak; // Allow pasting from clipboard in chat when chatJailbreak is enabled

            if (CheatToggles.longerMessages)
            {
                // Increasing the maximum length by 20 characters still avoids anticheat kicks
                __instance.freeChatField.textArea.characterLimit = 120;
            }
            else
            {
                __instance.freeChatField.textArea.characterLimit = 100;
            }
        }
        catch (Exception ex) { ErrorReporter.Report(ex, HandlingId, "ChatController_Update.Postfix: adjust message length"); }
    }
}

[HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
public static class ChatController_SendChat
{
    private const int HandlingId = 30003;
    // Postfix patch of ChatController.SendChat to unlock lower chat rate limits
    public static void Postfix(ChatController __instance)
    {
        try
        {
            if (!CheatToggles.lowerRateLimits) return;

            if (__instance.timeSinceLastMessage == 0f)
            {
                // Decreasing rate limit by 1 sec max still avoids anticheat kicks
                __instance.timeSinceLastMessage += 1f;
            }
        }
        catch (Exception ex) { ErrorReporter.Report(ex, HandlingId, "ChatController_SendChat.Postfix: lower rate limits"); }
    }
}

[HarmonyPatch(typeof(ChatController), nameof(ChatController.SendFreeChat))]
public static class ChatController_SendFreeChat
{
    private const int HandlingId = 30003;
    // Prefix patch of ChatController.SendFreeChat to allow sending URLs without being censored
    public static bool Prefix(ChatController __instance)
    {
        try
        {
            // Only works if CheatSettings.bypassUrlBlock is enabled
            if (!CheatToggles.bypassUrlBlock) return true;

            string text = __instance.freeChatField.Text;

            // Replace periods in URLs and email addresses with commas to avoid censorship
            string modifiedText = CensorUrlsAndEmails(text);

            ChatController.Logger.Debug("SendFreeChat () :: Sending message: '" + modifiedText + "'", null);
            PlayerControl.LocalPlayer.RpcSendChat(modifiedText);

            return false;
        }
        catch (Exception ex) { ErrorReporter.Report(ex, HandlingId, "ChatController_SendFreeChat.Prefix: bypass URL block"); return true; }
    }

    private static string CensorUrlsAndEmails(string text)
    {
        // Regular expression pattern to match URLs and email addresses
        string pattern = @"(http[s]?://)?([a-zA-Z0-9-]+\.)+[a-zA-Z]{2,6}(/[\w-./?%&=]*)?|([a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+)";
        Regex regex = new Regex(pattern);

        // Censor periods in each match
        return regex.Replace(text, match =>
        {
            var censored = match.Value;
            censored = censored.Replace('.', ',');
            return censored;
        });
    }
}
