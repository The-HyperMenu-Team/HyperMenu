using System;
using HarmonyLib;

namespace MalumMenu;

[HarmonyPatch(typeof(VoteBanSystem), nameof(VoteBanSystem.AddVote))]
public static class VoteBanSystem_AddVote
{
    private const int HandlingId = 30021;
    // Prefix patch of VoteBanSystem.AddVote to instantly kick players when host votes to kick them
    public static bool Prefix(VoteBanSystem __instance, int srcClient, int clientId)
    {
        try
        {
            if (!Utils.isHost) return true;

            if (AmongUsClient.Instance.ClientId == srcClient)
            {
                AmongUsClient.Instance.KickPlayer(clientId, false);
            }

            return false;
        }
        catch (Exception ex) { ErrorReporter.Report(ex, HandlingId, "VoteBanSystem_AddVote.Prefix: instant kick"); return false; }
    }
}

[HarmonyPatch(typeof(VoteBanSystem), nameof(VoteBanSystem.CmdAddVote))]
public static class VoteBanSystem_CmdAddVote
{
    private const int HandlingId = 30021;
    // Prefix patch of VoteBanSystem.CmdAddVote to prevent AddVoteBan RPC from being sent when host votes to kick a player
    public static bool Prefix()
    {
        try
        {
            return !Utils.isHost;
        }
        catch (Exception ex) { ErrorReporter.Report(ex, HandlingId, "VoteBanSystem_CmdAddVote.Prefix: block vote RPC"); return false; }
    }
}
