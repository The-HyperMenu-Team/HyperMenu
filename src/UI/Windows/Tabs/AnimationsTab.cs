using UnityEngine;

namespace MalumMenu;

public class AnimationsTab : ITab
{
    public string name => "Animations";

    public void Draw()
    {
        GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.425f));

        DrawGeneral();

        GUILayout.Space(15);

        DrawClientSided();

        GUILayout.EndVertical();
    }

    private void DrawGeneral()
    {
        CheatToggles.animShields = GUILayout.Toggle(CheatToggles.animShields, " Shields");

        CheatToggles.animAsteroids = GUILayout.Toggle(CheatToggles.animAsteroids, " Asteroids");

        CheatToggles.animEmptyGarbage = GUILayout.Toggle(CheatToggles.animEmptyGarbage, " Empty Garbage");

        CheatToggles.animMedScan = GUILayout.Toggle(CheatToggles.animMedScan, " Medbay Scan");

        CheatToggles.animCamsInUse = GUILayout.Toggle(CheatToggles.animCamsInUse, " Cams In Use");

        CheatToggles.animPet = GUILayout.Toggle(CheatToggles.animPet, " Pet");
        MalumMenu.routines.petPlayer.Enabled = CheatToggles.animPet;
        if(CheatToggles.animPet && MalumMenu.routines.petPlayer.target == null)
        {
            if(PlayersSection.selectedPlayer != null)
            {
                MalumMenu.routines.petPlayer.target = PlayersSection.selectedPlayer;
            }
            else
            {
                MalumMenu.notifications.Send("Pet Player", "Select a player in the Players tab first.", 10);
                CheatToggles.animPet = false;
            }
        }
    }

    private void DrawClientSided()
    {
        GUILayout.Label("Client-Sided", GUIStylePreset.TabSubtitle);

        CheatToggles.moonWalk = GUILayout.Toggle(CheatToggles.moonWalk, " Moonwalk");
    }
}
