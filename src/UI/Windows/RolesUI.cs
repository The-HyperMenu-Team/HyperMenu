using System;
using UnityEngine;

namespace MalumMenu;

public class RolesUI : MonoBehaviour
{
    private const int HandlingId = 10006;
    public static int windowHeight = 100;
    public static int windowWidth = 450;
    public static Rect windowRect;

    private Vector2 _scrollPosition = Vector2.zero;

    private void Start()
    {
        try
        {
            // Instantiate 2D area of RolesUI
            windowRect = new(
                Screen.width / 2f - windowWidth / 2f,
                Screen.height / 2f - windowHeight / 2f,
                windowWidth,
                windowHeight
            );
        }
        catch (Exception ex) { ErrorReporter.Report(ex, HandlingId, "RolesUI.Start: init window rect"); }
    }

    private void OnGUI()
    {
        try
        {
            if (!CheatToggles.showRolesMenu || !(MenuUI.isGUIActive || MalumMenu.menuKeepSubwindowsOpen.Value) || MalumMenu.isPanicked) return;

            UIHelpers.ApplyUIColor();

            windowRect = GUI.Window((int)WindowId.RolesUI, windowRect, (GUI.WindowFunction)RolesWindow, "Assign Roles");
        }
        catch (Exception ex) { ErrorReporter.Report(ex, HandlingId, "RolesUI.OnGUI: draw roles window"); }
    }

    private void RolesWindow(int windowID)
    {
        GUILayout.BeginVertical();

        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, true);

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (!player.Data || !player.Data.Role || string.IsNullOrEmpty(player.Data.PlayerName) || player != PlayerControl.LocalPlayer) continue;

            GUILayout.BeginHorizontal();

            GUILayout.Label($"<color=#{ColorUtility.ToHtmlStringRGB(player.Data.Color)}>{player.Data.PlayerName}</color>", GUILayout.Width(140f));
            GUILayout.BeginHorizontal();
            GUILayout.Label($"{CheatToggles.forcedRole}");
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Reset", GUILayout.Width(80f)))
            {
                CheatToggles.forcedRole = null;
            }
            if (GUILayout.Button("Assign", GUILayout.Width(80f)))
            {
                CheatToggles.forceRole = true;
            }

            GUILayout.EndHorizontal();
            GUILayout.EndHorizontal();
        }

        GUILayout.EndScrollView();
        GUILayout.EndVertical();
        GUILayout.Label("Roles will be assigned on next game start");
        GUI.DragWindow();
    }
}
