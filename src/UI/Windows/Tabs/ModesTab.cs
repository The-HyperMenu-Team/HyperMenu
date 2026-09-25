using System;
using UnityEngine;

namespace MalumMenu;

public class ModesTab : ITab
{
    private const int HandlingId = 60010;
    public string name => "Modes";

    public void Draw()
    {
        try
        {
            GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.425f));

            DrawGeneral();

            GUILayout.EndVertical();
        }
        catch (Exception ex) { ErrorReporter.Report(ex, HandlingId, "ModesTab.Draw: draw mode toggles"); }
    }

    private void DrawGeneral()
    {
        CheatToggles.rgbMode = GUILayout.Toggle(CheatToggles.rgbMode, " RGB Mode");

        CheatToggles.stealthMode = GUILayout.Toggle(CheatToggles.stealthMode, " Stealth Mode");

        if (MalumMenu.isDevRelease)
        {
            CheatToggles.streamerMode = GUILayout.Toggle(CheatToggles.streamerMode, " Streamer Mode");
        }
        else
        {
            GUILayout.Label("Coming Soon: Streamer Mode");

        }
        
        CheatToggles.panicMode = GUILayout.Toggle(CheatToggles.panicMode, " Panic Mode");
    }
}
