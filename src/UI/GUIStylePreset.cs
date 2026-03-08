using UnityEngine;

namespace MalumMenu;

public static class GUIStylePreset
{
    private static GUIStyle _separator;
    private static GUIStyle _normalButton;
    private static GUIStyle _normalToggle;
    private static GUIStyle _modernButton;
    private static GUIStyle _modernToggle;
    private static GUIStyle _titleLabel;
    private static GUIStyle _subtitleLabel;
    private static GUIStyle _textField;
    private static GUIStyle _card;
    private static Texture2D _buttonTexture;
    private static Texture2D _buttonHoverTexture;
    private static Texture2D _cardTexture;

    private static Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++)
            pix[i] = col;

        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }

    public static GUIStyle Separator
    {
        get
        {
            if (_separator == null)
            {
                _separator = new GUIStyle(GUI.skin.box)
                {
                    normal = { background = Texture2D.whiteTexture },
                    margin = new RectOffset { top = 4, bottom = 4 },
                    padding = new RectOffset(),
                    border = new RectOffset()
                };
            }

            return _separator;
        }
    }

    public static GUIStyle NormalButton
    {
        get
        {
            if (_normalButton == null)
            {
                _normalButton = new GUIStyle(GUI.skin.button)
                {
                    fontSize = 13
                };
            }

            return _normalButton;
        }
    }

    public static GUIStyle NormalToggle
    {
        get
        {
            if (_normalToggle == null)
            {
                _normalToggle = new GUIStyle(GUI.skin.toggle)
                {
                    fontSize = 13
                };
            }

            return _normalToggle;
        }
    }

    public static GUIStyle ModernButton
    {
        get
        {
            if (_modernButton == null)
            {
                _buttonTexture ??= MakeTex(2, 2, new Color(0.25f, 0.25f, 0.25f, 0.8f));
                _buttonHoverTexture ??= MakeTex(2, 2, new Color(0.35f, 0.35f, 0.35f, 0.9f));

                _modernButton = new GUIStyle(GUI.skin.button)
                {
                    fontSize = 14,
                    fontStyle = FontStyle.Bold,
                    normal = { background = _buttonTexture, textColor = new Color(0.9f, 0.9f, 0.9f) },
                    hover = { background = _buttonHoverTexture, textColor = Color.white },
                    active = { background = _buttonHoverTexture, textColor = Color.white },
                    padding = new RectOffset { left = 12, right = 12, top = 8, bottom = 8 },
                    margin = new RectOffset { left = 4, right = 4, top = 4, bottom = 4 },
                    border = new RectOffset { left = 4, right = 4, top = 4, bottom = 4 }
                };
            }

            return _modernButton;
        }
    }

    public static GUIStyle ModernToggle
    {
        get
        {
            if (_modernToggle == null)
            {
                _modernToggle = new GUIStyle(GUI.skin.toggle)
                {
                    fontSize = 14,
                    padding = new RectOffset { left = 22, right = 4, top = 4, bottom = 4 },
                    margin = new RectOffset { left = 4, right = 4, top = 3, bottom = 3 },
                    normal = { textColor = new Color(0.9f, 0.9f, 0.9f) },
                    hover = { textColor = Color.white },
                    onNormal = { textColor = new Color(0.3f, 0.9f, 0.3f) },
                    onHover = { textColor = new Color(0.4f, 1f, 0.4f) }
                };
            }

            return _modernToggle;
        }
    }

    public static GUIStyle TitleLabel
    {
        get
        {
            if (_titleLabel == null)
            {
                _titleLabel = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 22,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleLeft,
                    normal = { textColor = Color.white },
                    padding = new RectOffset { left = 0, right = 0, top = 5, bottom = 10 }
                };
            }

            return _titleLabel;
        }
    }

    public static GUIStyle SubtitleLabel
    {
        get
        {
            if (_subtitleLabel == null)
            {
                _subtitleLabel = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 16,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleLeft,
                    normal = { textColor = new Color(0.8f, 0.8f, 0.8f) },
                    padding = new RectOffset { left = 0, right = 0, top = 8, bottom = 5 }
                };
            }

            return _subtitleLabel;
        }
    }

    public static GUIStyle ModernTextField
    {
        get
        {
            if (_textField == null)
            {
                var textBg = MakeTex(2, 2, new Color(0.2f, 0.2f, 0.2f, 0.9f));
                var textFocusBg = MakeTex(2, 2, new Color(0.25f, 0.25f, 0.3f, 0.95f));

                _textField = new GUIStyle(GUI.skin.textField)
                {
                    fontSize = 14,
                    normal = { background = textBg, textColor = Color.white },
                    focused = { background = textFocusBg, textColor = Color.white },
                    hover = { background = textFocusBg, textColor = Color.white },
                    padding = new RectOffset { left = 8, right = 8, top = 6, bottom = 6 },
                    margin = new RectOffset { left = 4, right = 4, top = 4, bottom = 4 },
                    border = new RectOffset { left = 4, right = 4, top = 4, bottom = 4 }
                };
            }

            return _textField;
        }
    }

    public static GUIStyle Card
    {
        get
        {
            if (_card == null)
            {
                _cardTexture ??= MakeTex(2, 2, new Color(0.15f, 0.15f, 0.15f, 0.6f));

                _card = new GUIStyle(GUI.skin.box)
                {
                    normal = { background = _cardTexture },
                    padding = new RectOffset { left = 12, right = 12, top = 12, bottom = 12 },
                    margin = new RectOffset { left = 4, right = 4, top = 4, bottom = 4 },
                    border = new RectOffset { left = 2, right = 2, top = 2, bottom = 2 }
                };
            }

            return _card;
        }
    }
}
