using UnityEngine;
using System.Globalization;

namespace MalumMenu;

public class SettingsTab : ITab
{
    public string name => "Settings";

    private bool _initialized = false;

    // Custom text fields
    private TextField _menuKeybindField;
    private TextField _menuColorField;
    private TextField _spoofLevelField;
    private TextField _spoofPlatformField;

    // Pending scale values (applied on Apply button)
    private float _pendingScale;
    private float _pendingWidthMult;
    private float _pendingHeightMult;
    private float _pendingTextScale;

    public void Draw()
    {
        if (!_initialized)
        {
            InitializeInputFields();
            _initialized = true;
        }

        GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.425f));

        DrawGUISettings();

        GUILayout.Space(15);

        DrawSpoofingSettings();

        GUILayout.Space(15);

        DrawPrivacySettings();

        GUILayout.EndVertical();
    }

    private void InitializeInputFields()
    {
        _menuKeybindField = new TextField(MalumMenu.menuKeybind.Value);
        _menuColorField = new TextField(MalumMenu.menuHtmlColor.Value);
        _spoofLevelField = new TextField(MalumMenu.spoofLevel.Value);
        _spoofPlatformField = new TextField(MalumMenu.spoofPlatform.Value);

        _pendingScale = MalumMenu.menuScale.Value;
        _pendingWidthMult = MalumMenu.menuWidthMult.Value;
        _pendingHeightMult = MalumMenu.menuHeightMult.Value;
        _pendingTextScale = MalumMenu.menuTextScale.Value;
    }


    private void DrawGUISettings()
    {
        GUILayout.Label("GUI Settings", GUIStylePreset.TabSubtitle);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Menu Keybind:", GUILayout.Width(150));
        _menuKeybindField.Draw(150);
        if (GUILayout.Button("Save", GUILayout.Width(100)))
        {
            MalumMenu.menuKeybind.Value = _menuKeybindField.Content;
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(5);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Menu Color (HTML):", GUILayout.Width(150));
        _menuColorField.Draw(150);
        if (GUILayout.Button("Save", GUILayout.Width(100)))
        {
            MalumMenu.menuHtmlColor.Value = _menuColorField.Content;
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(5);

        MalumMenu.menuOpenOnMouse.Value = GUILayout.Toggle(MalumMenu.menuOpenOnMouse.Value, " Open Menu on Mouse Position");

        GUILayout.Space(5);

        MalumMenu.autoLoadProfile.Value = GUILayout.Toggle(MalumMenu.autoLoadProfile.Value, " Auto-Load Profile on Startup");

        GUILayout.Space(5);

        GUILayout.BeginHorizontal();
        GUILayout.Label($"Menu Scale: {_pendingScale:F2}", GUILayout.Width(150));
        _pendingScale = GUILayout.HorizontalSlider(_pendingScale, 0.5f, 2.0f);
        GUILayout.EndHorizontal();

        GUILayout.Space(5);

        GUILayout.BeginHorizontal();
        GUILayout.Label($"Width: {_pendingWidthMult:F2}", GUILayout.Width(150));
        _pendingWidthMult = GUILayout.HorizontalSlider(_pendingWidthMult, 0.5f, 2.0f);
        GUILayout.EndHorizontal();

        GUILayout.Space(5);

        GUILayout.BeginHorizontal();
        GUILayout.Label($"Height: {_pendingHeightMult:F2}", GUILayout.Width(150));
        _pendingHeightMult = GUILayout.HorizontalSlider(_pendingHeightMult, 0.5f, 2.0f);
        GUILayout.EndHorizontal();

        GUILayout.Space(5);

        GUILayout.BeginHorizontal();
        GUILayout.Label($"Text Scale: {_pendingTextScale:F2}", GUILayout.Width(150));
        _pendingTextScale = GUILayout.HorizontalSlider(_pendingTextScale, 0.5f, 2.0f);
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        if (GUILayout.Button("Apply", GUILayout.Width(200)))
        {
            MalumMenu.menuScale.Value = _pendingScale;
            MalumMenu.menuWidthMult.Value = _pendingWidthMult;
            MalumMenu.menuHeightMult.Value = _pendingHeightMult;
            MalumMenu.menuTextScale.Value = _pendingTextScale;
        }
    }

    private void DrawSpoofingSettings()
    {
        GUILayout.Label("Spoofing Settings", GUIStylePreset.TabSubtitle);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Spoof Level (1-100001):", GUILayout.Width(150));
        _spoofLevelField.Draw(150);
        if (GUILayout.Button("Save", GUILayout.Width(100)))
        {
            // Validate that it's a number between 1 and 100001
            if (int.TryParse(_spoofLevelField.Content, NumberStyles.Integer, CultureInfo.InvariantCulture, out int level) &&
                level >= 1 && level <= 100001)
            {
                MalumMenu.spoofLevel.Value = _spoofLevelField.Content;
            }
            else
            {
                _spoofLevelField.Content = MalumMenu.spoofLevel.Value;
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(5);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Spoof Platform:", GUILayout.Width(150));
        _spoofPlatformField.Draw(150);
        if (GUILayout.Button("Save", GUILayout.Width(100)))
        {
            MalumMenu.spoofPlatform.Value = _spoofPlatformField.Content;
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(5);

        GUILayout.Label("Supported Platforms: StandaloneEpicPC, StandaloneSteamPC, StandaloneMac, StandaloneWin10, etc.");
    }

    private void DrawPrivacySettings()
    {
        GUILayout.Label("Privacy Settings", GUIStylePreset.TabSubtitle);

        MalumMenu.spoofDeviceId.Value = GUILayout.Toggle(MalumMenu.spoofDeviceId.Value, " Hide Device ID");

        GUILayout.Space(5);

        MalumMenu.noTelemetry.Value = GUILayout.Toggle(MalumMenu.noTelemetry.Value, " Disable Telemetry");

        GUILayout.Space(10);

        if (GUILayout.Button("Open Config File", GUILayout.Width(200)))
        {
            Utils.OpenConfigFile();
        }

        GUILayout.Space(5);

        GUILayout.Label("For more advanced configuration options, click 'Open Config File'", GUIStylePreset.TabSubtitle);
    }
}
