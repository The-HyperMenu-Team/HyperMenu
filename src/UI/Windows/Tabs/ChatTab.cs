using UnityEngine;

namespace MalumMenu;

public class ChatTab : ITab
{
    public string name => "Chat";

    private TextField _chatColorField;
    private bool _initialized = false;

    public void Draw()
    {
        if (!_initialized)
        {
            Initialize();
            _initialized = true;
        }

        GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.425f));

        DrawGeneral();

        GUILayout.Space(15);

        DrawTextbox();

        GUILayout.Space(15);

        DrawColorSettings();

        GUILayout.EndVertical();
    }

    public void Initialize()
    {
        _chatColorField = new TextField(MalumMenu.menuChatColor.Value);
    }

    private void DrawGeneral()
    {
        CheatToggles.enableChat = GUILayout.Toggle(CheatToggles.enableChat, " Enable Chat");

        CheatToggles.bypassUrlBlock = GUILayout.Toggle(CheatToggles.bypassUrlBlock, " Bypass URL Block");

        CheatToggles.lowerRateLimits = GUILayout.Toggle(CheatToggles.lowerRateLimits, " Lower Rate Limits");

        CheatToggles.showGhostsChat = GUILayout.Toggle(CheatToggles.showGhostsChat, " Show Ghost Messages");
        features.Chat.OnChat.ShowMessagesByGhosts = CheatToggles.showGhostsChat;

        CheatToggles.alwaysVisibleChat = GUILayout.Toggle(CheatToggles.alwaysVisibleChat, " Always Visible Chat");
        features.Chat.AlwaysVisibleChat.Enabled = CheatToggles.alwaysVisibleChat;
    }

    private void DrawTextbox()
    {
        GUILayout.Label("Textbox", GUIStylePreset.TabSubtitle);

        CheatToggles.unlockCharacters = GUILayout.Toggle(CheatToggles.unlockCharacters, " Unlock Extra Characters");

        CheatToggles.longerMessages = GUILayout.Toggle(CheatToggles.longerMessages, " Allow Longer Messages");

        CheatToggles.unlockClipboard = GUILayout.Toggle(CheatToggles.unlockClipboard, " Unlock Clipboard");
    }

    private void DrawColorSettings()
    {
        GUILayout.Label("Chat Color", GUIStylePreset.TabSubtitle);
        CheatToggles.colorAsPlayer = GUILayout.Toggle(CheatToggles.colorAsPlayer, " Chat messages colored as the player who sent them");
        CheatToggles.changeChatColor = GUILayout.Toggle(CheatToggles.changeChatColor, " Enable Custom Chat Color");
        GUILayout.Space(5);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Chat HTML Color:", GUILayout.Width(150));
        _chatColorField.Draw(150);
        if (GUILayout.Button("Save", GUILayout.Width(100)))
        {
            MalumMenu.menuChatColor.Value = _chatColorField.Content;
        }
        GUILayout.EndHorizontal();
    }
}
