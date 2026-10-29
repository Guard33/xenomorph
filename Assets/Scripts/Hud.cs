using UnityEngine;

// Small helpers for the game's on-screen UI, all in the same pixel-art style:
// pixel-font text with a dark outline, chunky panels with notched corners, and buttons.
// Sizes are for a 1080p screen and scale with the window.
public static class Hud
{
    public static float Scale => Mathf.Max(0.75f, Screen.height / 1080f);

    // 'size' is a font size in points; it is snapped to whole font pixels so text stays crisp
    public static void Text(Rect r, string text, int size, Color color, TextAnchor anchor = TextAnchor.MiddleCenter, bool bold = false)
    {
        PixelFont.Draw(r, text, PixelFont.PixelSize(size), color, anchor);
    }

    // Width of one line of text at a font size, in screen pixels
    public static float TextWidth(string text, int size) => PixelFont.LineWidth(text, PixelFont.PixelSize(size));

    public static void Fill(Rect r, Color c)
    {
        if (Event.current.type != EventType.Repaint) return;
        Color old = GUI.color;
        GUI.color = c;
        GUI.DrawTexture(r, Texture2D.whiteTexture);
        GUI.color = old;
    }

    // Size of one "art pixel" for panels and borders
    public static float Unit => Mathf.Max(2f, Mathf.Round(3f * Scale));

    // A pixel-art box: dark outline with cut corners, a light line along the top
    // and a shadow line along the bottom.
    public static void Panel(Rect r, Color fill)
    {
        float u = Unit;
        r = new Rect(Mathf.Round(r.x), Mathf.Round(r.y), Mathf.Round(r.width), Mathf.Round(r.height));
        Color edge = new Color(0.05f, 0.04f, 0.08f, Mathf.Max(fill.a, 0.9f));
        Fill(new Rect(r.x + u, r.y, r.width - 2 * u, u), edge);
        Fill(new Rect(r.x + u, r.yMax - u, r.width - 2 * u, u), edge);
        Fill(new Rect(r.x, r.y + u, u, r.height - 2 * u), edge);
        Fill(new Rect(r.xMax - u, r.y + u, u, r.height - 2 * u), edge);
        Fill(new Rect(r.x + u, r.y + u, r.width - 2 * u, r.height - 2 * u), fill);
        Fill(new Rect(r.x + 2 * u, r.y + u, r.width - 4 * u, u), Color.Lerp(fill, Color.white, 0.25f));
        Fill(new Rect(r.x + 2 * u, r.yMax - 2 * u, r.width - 4 * u, u), Color.Lerp(fill, Color.black, 0.35f));
    }

    public static bool Hover(Rect r) => r.Contains(Event.current.mousePosition);

    // True on the frame the rect is clicked
    public static bool Clicked(Rect r)
    {
        if (Event.current.type == EventType.MouseDown && r.Contains(Event.current.mousePosition))
        {
            Event.current.Use();
            return true;
        }
        return false;
    }

    public static bool Button(Rect r, string label, int size = 16)
    {
        bool hover = Hover(r);
        Panel(r, hover ? new Color(0.25f, 0.55f, 0.3f, 0.95f) : new Color(0.14f, 0.3f, 0.17f, 0.95f));
        Text(r, label, size, hover ? Color.white : new Color(0.85f, 1f, 0.85f));
        return Clicked(r);
    }
}
