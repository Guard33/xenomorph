using UnityEngine;

// Small helpers for the in-game HUD: text with a dark outline (readable on any
// background without a box) and plain rectangles. Sizes are for a 1080p screen
// and scale with the window.
public static class Hud
{
    static GUIStyle style;

    public static float Scale => Mathf.Max(0.75f, Screen.height / 1080f);

    public static void Text(Rect r, string text, int size, Color color, TextAnchor anchor = TextAnchor.MiddleCenter, bool bold = false)
    {
        if (style == null) style = new GUIStyle(GUI.skin.label) { wordWrap = false, richText = false };
        style.fontSize = Mathf.RoundToInt(size * Scale);
        style.alignment = anchor;
        style.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;

        Color old = GUI.color;
        GUI.color = new Color(0f, 0f, 0f, 0.75f * color.a);
        float o = Mathf.Max(1f, 1.5f * Scale);
        GUI.Label(new Rect(r.x - o, r.y, r.width, r.height), text, style);
        GUI.Label(new Rect(r.x + o, r.y, r.width, r.height), text, style);
        GUI.Label(new Rect(r.x, r.y - o, r.width, r.height), text, style);
        GUI.Label(new Rect(r.x, r.y + o, r.width, r.height), text, style);
        GUI.color = color;
        GUI.Label(r, text, style);
        GUI.color = old;
    }

    public static void Fill(Rect r, Color c)
    {
        Color old = GUI.color;
        GUI.color = c;
        GUI.DrawTexture(r, Texture2D.whiteTexture);
        GUI.color = old;
    }
}
