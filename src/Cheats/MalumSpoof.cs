using System;
using AmongUs.Data;

namespace MalumMenu;
public static class MalumSpoof
{
    private const int HandlingId = 20011;
    public static void SpoofLevel()
    {
        try
        {
            // Parse Spoofing.Level config entry and turn it into a uint
            if (!string.IsNullOrEmpty(MalumMenu.spoofLevel.Value) &&
                uint.TryParse(MalumMenu.spoofLevel.Value, out uint parsedLevel) &&
                parsedLevel != DataManager.Player.Stats.Level)
            {

                // Store the spoofed level using DataManager
                DataManager.Player.stats.level = parsedLevel - 1;
                DataManager.Player.Save();
            }
        }
        catch (Exception ex) { ErrorReporter.Report(ex, HandlingId, "MalumSpoof.SpoofLevel: spoofing level"); }
    }

    public static string SpoofFriendCode()
    {
        try
        {
            string friendCode = MalumMenu.guestFriendCode.Value;
            if (string.IsNullOrWhiteSpace(friendCode))
            {
                friendCode = DestroyableSingleton<AccountManager>.Instance.GetRandomName();
            }
            return friendCode;
        }
        catch (Exception ex) { ErrorReporter.Report(ex, HandlingId, "MalumSpoof.SpoofFriendCode: spoofing friend code"); return string.Empty; }
    }
}
