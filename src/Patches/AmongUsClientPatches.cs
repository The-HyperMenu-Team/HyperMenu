using System;
using BepInEx.Unity.IL2CPP.Utils;
using HarmonyLib;
using UnityEngine;

namespace MalumMenu;

[HarmonyPatch(typeof(LobbyBehaviour), nameof(LobbyBehaviour.Start))]
public static class LobbyBehaviour_Start
{
    private const int HandlingId = 30001;
    // Postfix patch of LobbyBehaviour.Start to apply randomized cosmetics when entering a lobby
    // Fires both when joining a new lobby and when returning to the same lobby after a game ends
    public static void Postfix(LobbyBehaviour __instance)
    {
        try
        {
            if (!CheatToggles.randomizeCosmetics) return;

            // Skip when we are in the middle of a leave-randomize-rejoin sequence to avoid
            // triggering a second randomize cycle after the automatic rejoin.
            if (MalumRandomizer.isRejoinInProgress) return;

            AmongUsClient.Instance.StartCoroutine(ErrorReporter.GuardCoroutine(DelayedRandomize(), HandlingId, "DelayedRandomize"));
        }
        catch (Exception ex) { ErrorReporter.Report(ex, HandlingId, "LobbyBehaviour_Start.Postfix: start delayed randomize coroutine"); }
    }

    private static System.Collections.IEnumerator DelayedRandomize()
    {
        yield return new WaitForSeconds(1.0f);
        MalumRandomizer.Randomize();
    }
}

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.Update))]
public static class AmongUsClient_Update
{
    private const int HandlingId = 30001;
    public static void Postfix()
    {
        try
        {
            MalumSpoof.SpoofLevel();

            // GuestMode cheats are commented out as they are broken in latest updates

            // Code to treat temp accounts the same as full accounts, including access to friend codes
            // if (!EOSManager.Instance.loginFlowFinished || !MalumMenu.guestMode.Value) return;
            // DataManager.Player.Account.LoginStatus = EOSManager.AccountLoginStatus.LoggedIn;

            // if (!string.IsNullOrWhiteSpace(EOSManager.Instance.FriendCode)) return;
            // var friendCode = MalumSpoof.spoofFriendCode();
            // var editUsername = EOSManager.Instance.editAccountUsername;
            // editUsername.UsernameText.SetText(friendCode);
            // editUsername.SaveUsername();
            // EOSManager.Instance.FriendCode = friendCode;
        }
        catch (Exception ex) { ErrorReporter.Report(ex, HandlingId, "AmongUsClient_Update.Postfix: spoof level"); }
    }
}

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameJoined))]
public static class AmongUsClient_OnGameJoined
{
    private const int HandlingId = 30001;
    // Postfix patch of AmongUsClient.OnGameJoined to store the last joined game ID string
    public static string lastGameIdString = "";

    public static void Postfix(string gameIdString)
    {
        try
        {
            lastGameIdString = gameIdString;
        }
        catch (Exception ex) { ErrorReporter.Report(ex, HandlingId, "AmongUsClient_OnGameJoined.Postfix: store last game id"); }
    }
}

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
public static class AmongUsClient_CoStartGame
{
    private const int HandlingId = 30001;
    public static void Postfix()
    {
        try
        {
            if (CheatToggles.logGameState)
                ConsoleUI.Log("Game started");
        }
        catch (Exception ex) { ErrorReporter.Report(ex, HandlingId, "AmongUsClient_CoStartGame.Postfix: log game started"); }
    }
}

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
public static class AmongUsClient_OnGameEnd
{
    private const int HandlingId = 30001;
    public static void Postfix(EndGameResult endGameResult)
    {
        try
        {
            if (CheatToggles.logGameState)
                ConsoleUI.Log($"Game ended with reason {endGameResult.GameOverReason}");
        }
        catch (Exception ex) { ErrorReporter.Report(ex, HandlingId, "AmongUsClient_OnGameEnd.Postfix: log game ended"); }
    }
}
