using HarmonyLib;
using System;

namespace MalumMenu;

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
public static class HudManager_Start
{
    private const int HandlingId = 30005;
	// Postfix patch of HudManager.Start to give minimap access to impostors too
	public static void Postfix(HudManager __instance)
	{
        try
        {
            __instance.MapButton.OnClick.RemoveAllListeners(); // Remove previous OnClick action

            // Always open normal map when map button is clicked
            // To access sabotage map, sabotage button can be used
            __instance.MapButton.OnClick.AddListener((Action) (() =>
            {
                try
                {
                    __instance.ToggleMapVisible(new MapOptions
                    {
                        Mode = MapOptions.Modes.Normal
                    });
                }
                catch (Exception ex) { ErrorReporter.Report(ex, HandlingId, "HudManager_Start.Postfix: toggle map visible"); }

            }));
        }
        catch (Exception ex) { ErrorReporter.Report(ex, HandlingId, "HudManager_Start.Postfix: hook map button"); }
	}
}

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
public static class HudManager_Update
{
    private const int HandlingId = 30005;
	public static void Postfix(HudManager __instance)
    {
        try
        {
            __instance.ShadowQuad.gameObject.SetActive(!MalumESP.IsFullbrightActive()); // Fullbright

            if (Utils.IsChatUiActive()) // AlwaysChat
            {
                __instance.Chat.gameObject.SetActive(true);
            }
            else
            {
                Utils.CloseChat();
                __instance.Chat.gameObject.SetActive(false);
            }

            MalumCheats.UseVentCheat(__instance);
            MalumESP.ZoomOut(__instance);
            MalumESP.FreecamCheat();

            // Close PlayerPickMenu if there is no PPM cheat enabled
            if (PlayerPickMenu.playerpickMenu != null && CheatToggles.ShouldPPMClose())
            {
                PlayerPickMenu.playerpickMenu.Close();
            }
        }
        catch (Exception ex) { ErrorReporter.Report(ex, HandlingId, "HudManager_Update.Postfix: update HUD cheats"); }
    }
}
