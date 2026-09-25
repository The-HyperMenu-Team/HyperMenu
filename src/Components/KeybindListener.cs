using System;
using UnityEngine;

namespace MalumMenu;

public class KeybindListener : MonoBehaviour
{
    private const int HandlingId = 60201;

    public void Update()
    {
        try
        {
            if (MalumMenu.isPanicked) return;

            // Keybinds aren't triggered from typing in the chat
            if (HudManager.InstanceExists && HudManager.Instance.Chat && HudManager.Instance.Chat.IsOpenOrOpening) return;

            // Check each keybind to see if the user pressed it and toggle the corresponding cheat
            foreach (var (name, key) in CheatToggles.Keybinds)
            {
                if (key == KeyCode.None) continue;
                if (!Input.GetKeyDown(key)) continue;

                if (!CheatToggles.ToggleFields.TryGetValue(name, out var field)) continue;

                var current = (bool)field.GetValue(null);
                field.SetValue(null, !current);
            }
        }
        catch (Exception ex) { ErrorReporter.Report(ex, HandlingId, "KeybindListener.Update: poll cheat keybinds"); }
    }
}
