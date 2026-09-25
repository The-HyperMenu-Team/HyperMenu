using System;
using UnityEngine;
using MalumMenu.features;

namespace MalumMenu;

public class ProtectionsTab : ITab
{
    private const int HandlingId = 60014;
    public string name => "Protections";

    public void Draw()
    {
        try
        {
            GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.425f));

            // Network
            Protections.ForceDTLS.Enabled = GUILayout.Toggle(Protections.ForceDTLS.Enabled, "Force enable DTLS to encrypt network data");

            Protections.BlockServerTeleports.Enabled = GUILayout.Toggle(Protections.BlockServerTeleports.Enabled, "Block position updates from server");

            // Overloads
            Protections.HardenedReadPackedUInt.Enabled = GUILayout.Toggle(Protections.HardenedReadPackedUInt.Enabled, "Use hardened packed int deserializer");
            Protections.BlockLargeGameMessages = GUILayout.Toggle(Protections.BlockLargeGameMessages, "Block large game messages");
            Protections.BlockInvalidGameDataMessages = GUILayout.Toggle(Protections.BlockInvalidGameDataMessages, "Block invalid game data messages");
            Protections.BlockUnauthorizedSystemUpdates = GUILayout.Toggle(Protections.BlockUnauthorizedSystemUpdates, "Block unauthorized system updates");
            Protections.ProtectAgainstNonHostKickExploit = GUILayout.Toggle(Protections.ProtectAgainstNonHostKickExploit, "Protect against non-host kick exploit");

            Protections.Votekicks.Enabled = GUILayout.Toggle(Protections.Votekicks.Enabled, "Prevent being votekicked as host");

            GUILayout.EndVertical();
        }
        catch (Exception ex) { ErrorReporter.Report(ex, HandlingId, "ProtectionsTab.Draw: draw protection toggles"); }
    }
}