using MalumMenu.features;
using UnityEngine;

namespace MalumMenu;

public class TrollTab : ITab
{
    public string name => "Troll";

    public void Draw()
    {
        if (PlayerControl.LocalPlayer == null)
        {
            GUILayout.Label("You are not currently in a game, these options will not work.");
        }

        GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.425f));

        Troll.AutoReportBodies.Enabled = GUILayout.Toggle(Troll.AutoReportBodies.Enabled, "Automatically Report Bodies");
         MalumMenu.routines.autoTriggerSpores.Enabled = GUILayout.Toggle(MalumMenu.routines.autoTriggerSpores.Enabled, "Auto Trigger Spores");
        Troll.BlockSabotages.Enabled = GUILayout.Toggle(Troll.BlockSabotages.Enabled, "Block Sabotages");
        Troll.BlockVenting.Enabled = GUILayout.Toggle(Troll.BlockVenting.Enabled, "Disable Vents");

        if (GUILayout.Button(" Trigger All Spores"))
        {
            if (Utilities.GetCurrentMap() != MapNames.Fungle)
               {
                   MalumMenu.notifications.Send("Trigger Spores", "This option only works on the Fungle map.");
               }
               else
               {
                   FungleShipStatus shipStatus = ShipStatus.Instance.Cast<FungleShipStatus>();

                   foreach (Mushroom mushroom in shipStatus.sporeMushrooms.Values)
                   {
                       PlayerControl.LocalPlayer.RpcTriggerSpores(mushroom);
                   }

                   MalumMenu.notifications.Send("Trigger Spores", "All spores have been triggered.", 5);
            }
        }

        if (GUILayout.Button(" Copy Random Player"))
        {
            PlayerControl randomPl = Utilities.GetRandomPlayer();
            Utilities.CopyPlayer(randomPl);
        }

        GUILayout.Space(5);

        GUILayout.Label("Door Troller:");
        MalumMenu.routines.doorTroller.Enabled = GUILayout.Toggle(MalumMenu.routines.doorTroller.Enabled, "Enabled");

        GUILayout.Label($"Lock and Unlock Delay: {MalumMenu.routines.doorTroller.doorDelay:F2}s");
        MalumMenu.routines.doorTroller.doorDelay = GUILayout.HorizontalSlider(MalumMenu.routines.doorTroller.doorDelay, 0.1f, 2.0f);

        GUILayout.EndVertical();
    }
}
