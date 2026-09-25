using System;
using UnityEngine;

namespace MalumMenu;

public class ConsoleTab : ITab
{
    private const int HandlingId = 60006;
    public string name => "Console";

    public void Draw()
    {
        try
        {
            GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.425f));

            DrawGeneral();

            GUILayout.EndVertical();
        }
        catch (Exception ex) { ErrorReporter.Report(ex, HandlingId, "ConsoleTab.Draw: draw console toggles"); }
    }

    private void DrawGeneral()
    {
        CheatToggles.showConsole = GUILayout.Toggle(CheatToggles.showConsole, " Show Console");

        CheatToggles.logDeaths = GUILayout.Toggle(CheatToggles.logDeaths, " Log Deaths");

        CheatToggles.logShapeshifts = GUILayout.Toggle(CheatToggles.logShapeshifts, " Log Shapeshifts");

        CheatToggles.logVents = GUILayout.Toggle(CheatToggles.logVents, " Log Vents");

        CheatToggles.logTasks = GUILayout.Toggle(CheatToggles.logTasks, " Log Tasks");

        CheatToggles.logGameState  = GUILayout.Toggle(CheatToggles.logGameState, " Log Game State");
    }
}
