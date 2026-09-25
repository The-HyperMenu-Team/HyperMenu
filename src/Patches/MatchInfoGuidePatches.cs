using System;
using HarmonyLib;
using UnityEngine;

namespace MalumMenu;

[HarmonyPatch(typeof(MatchInfoGuide), nameof(MatchInfoGuide.Open))]
public static class MatchInfoGuide_Open
{
    private const int HandlingId = 30009;
    // Prefix patch of MatchInfoGuide.Open to recreate player entries each time MatchInfo opens
    // Without this, entries are only created on the first open, making player names difficult to update afterward
    public static void Prefix(MatchInfoGuide __instance)
    {
        try
        {
            if (__instance.NormalModeSettings.Count > 0 || __instance.HnSModeSettings.Count > 0) // If not the first open...
            {
                __instance.ControllerSelectable.Clear();
                __instance.CreatePlayerEntries();
            }
        }
        catch (Exception ex) { ErrorReporter.Report(ex, HandlingId, "MatchInfoGuide_Open.Prefix: recreate player entries"); }
    }
}

[HarmonyPatch(typeof(MatchInfoGuide), nameof(MatchInfoGuide.CreatePlayerEntries))]
public static class MatchInfoGuide_CreatePlayerEntries
{
    private const int HandlingId = 30009;
    private static Vector2 _anchoredPosition = new Vector2(0f, 0f);

    // Prefix patch of MatchInfoGuide.CreatePlayerEntries for seePlayerInfo and seeRoles cheats
    public static bool Prefix(MatchInfoGuide __instance)
    {
        try
        {
            __instance.PlayerPool.ReclaimAll();
            int num = 51;
            foreach (NetworkedPlayerInfo networkedPlayerInfo in GameData.Instance.AllPlayers)
            {
                PlayerIdentifierButton component = __instance.PlayerPool.Get<PoolableBehavior>().GetComponent<PlayerIdentifierButton>();
                component.transform.localPosition = new Vector3(0f, 0f, -1f);
                component.Populate(networkedPlayerInfo);

                component.NameText.text = Utils.GetNameTag(networkedPlayerInfo, networkedPlayerInfo.PlayerName, false, true);

                // Adjust the text position to improve readability
                // Always start from the original anchoredPosition to prevent cumulative shifting on each open
                if (_anchoredPosition == Vector2.zero)
                {
                    _anchoredPosition = component.NameText.rectTransform.anchoredPosition;
                }
                component.NameText.rectTransform.anchoredPosition = new Vector2(_anchoredPosition.x, _anchoredPosition.y + 0.05f);

                __instance.ControllerSelectable.Add(component.Button);
                component.SetTextStencil(num++);
            }

            return false;
        }
        catch (Exception ex) { ErrorReporter.Report(ex, HandlingId, "MatchInfoGuide_CreatePlayerEntries.Prefix: build player entries"); return true; }
    }
}
