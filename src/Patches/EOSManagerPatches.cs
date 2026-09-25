using System;
using HarmonyLib;

namespace MalumMenu;

// GuestMode cheats are commented out as they are broken in latest updates

// [HarmonyPatch(typeof(EOSManager), nameof(EOSManager.StartInitialLoginFlow))]
// public static class EOSManager_StartInitialLoginFlow
// {
//     /// <summary>
//     /// Prefix patch of EOSManager.StartInitialLoginFlow to automatically play with a guest account
//     /// when loading the game with guestMode enabled
//     /// </summary>
//     /// <param name="__instance">The <c>EOSManager</c> instance.</param>
//     /// <returns><c>false</c> to skip the original method, <c>true</c> to allow the original method to run.</returns>
//     public static bool Prefix(EOSManager __instance)
//     {
//         // Always delete old guest accounts to avoid merge account popup
//         __instance.DeleteDeviceID(new System.Action(__instance.EndMergeGuestAccountFlow));

//         // Log into a new temp account if the user is playing in guest mode
//         if (!MalumMenu.guestMode.Value) return true;
//         __instance.StartTempAccountFlow();
//         __instance.CloseStartupWaitScreen();

//         return false;
//     }
// }

[HarmonyPatch(typeof(EOSManager), nameof(EOSManager.HasServerTimestamp), MethodType.Getter)]
public static class EOSManager_HasServerTimestamp_Getter
{
    private const int HandlingId = 30004;
    // Postfix patch of EOSManager.HasServerTimestamp Getter method to ensure the date can be spoofed
    public static void Postfix(ref bool __result)
    {
        try
        {
            if (!CheatToggles.spoofAprilFoolsDate) return;

            __result = true;
        }
        catch (Exception ex) { ErrorReporter.Report(ex, HandlingId, "EOSManager_HasServerTimestamp_Getter.Postfix: spoof server timestamp flag"); }
    }
}

[HarmonyPatch(typeof(EOSManager), nameof(EOSManager.ApproximateServerTime), MethodType.Getter)]
public static class EOSManager_ApproximateServerTime_Getter
{
    private const int HandlingId = 30004;
    // Postfix patch of EOSManager.ApproximateServerTime Getter method to spoof the date to April 1st, 7:01 AM UTC
    public static void Postfix(ref Il2CppSystem.DateTime __result)
    {
        try
        {
            if (!CheatToggles.spoofAprilFoolsDate) return;

            var managedDate = new DateTime(DateTime.UtcNow.Year, 4, 1, 7, 1, 0, DateTimeKind.Utc);
            __result = new Il2CppSystem.DateTime(managedDate.Ticks);
        }
        catch (Exception ex) { ErrorReporter.Report(ex, HandlingId, "EOSManager_ApproximateServerTime_Getter.Postfix: spoof server time"); }
    }
}

[HarmonyPatch(typeof(EOSManager), nameof(EOSManager.IsFreechatAllowed))]
public static class EOSManager_IsFreechatAllowed
{
    private const int HandlingId = 30004;
    // Prefix patch of EOSManager.IsFreechatAllowed to unlock freechat
    public static void Postfix(ref bool __result)
    {
        try
        {
            if (CheatToggles.unlockFeatures)
            {
                __result = true;
            }
        }
        catch (Exception ex) { ErrorReporter.Report(ex, HandlingId, "EOSManager_IsFreechatAllowed.Postfix: unlock freechat"); }
    }
}

[HarmonyPatch(typeof(EOSManager), nameof(EOSManager.IsFriendsListAllowed))]
public static class EOSManager_IsFriendsListAllowed
{
    private const int HandlingId = 30004;
    // Prefix patch of EOSManager.IsFriendsListAllowed to unlock friend list
    public static void Postfix(ref bool __result)
    {
        try
        {
            if (CheatToggles.unlockFeatures)
            {
                __result = true;
            }
        }
        catch (Exception ex) { ErrorReporter.Report(ex, HandlingId, "EOSManager_IsFriendsListAllowed.Postfix: unlock friends list"); }
    }
}

[HarmonyPatch(typeof(EOSManager), nameof(EOSManager.IsAllowedOnline))]
public static class EOSManager_IsAllowedOnline
{
    private const int HandlingId = 30004;
    // Prefix patch of EOSManager.IsAllowedOnline to allow online games
    public static void Prefix(ref bool canOnline)
    {
        try
        {
            if (CheatToggles.unlockFeatures)
            {
                canOnline = true;
            }
        }
        catch (Exception ex) { ErrorReporter.Report(ex, HandlingId, "EOSManager_IsAllowedOnline.Prefix: allow online"); }
    }
}

[HarmonyPatch(typeof(EOSManager), nameof(EOSManager.IsMinorOrWaiting))]
public static class EOSManager_IsMinorOrWaiting
{
    private const int HandlingId = 30004;
    // Prefix patch of EOSManager.IsMinorOrWaiting to remove minor status
    public static void Postfix(ref bool __result)
    {
        try
        {
            if (CheatToggles.unlockFeatures)
            {
                __result = false;
            }
        }
        catch (Exception ex) { ErrorReporter.Report(ex, HandlingId, "EOSManager_IsMinorOrWaiting.Postfix: remove minor status"); }
    }
}
