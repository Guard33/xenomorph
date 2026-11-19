using UnityEngine;

// Small helpers for the game's on-screen UI, all in the same pixel-art style:
// pixel-font text with a dark outline, chunky panels with notched corners, and buttons.
// Sizes are for a 1080p screen and scale with the window.
public static class Hud
{
    public static float Scale => Mathf.Max(0.75f, Screen.height / 1080f) * Mult;

    // Size of the UI part being drawn (Settings: Small / Medium / Large / Huge for each part).
    // Each OnGUI calls Use(...) before drawing a part.
    public static float Mult = 1f;
    public static void Use(GameSettings.UiPart part) { Mult = GameSettings.UiScale(part); }

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

    // A speech bubble for tips: cream fill, dark outline with stepped pixel corners,
    // and a small tail under the middle.
    public static void Bubble(Rect r, float alpha = 1f)
    {
        float u = Unit;
        r = new Rect(Mathf.Round(r.x), Mathf.Round(r.y), Mathf.Round(r.width), Mathf.Round(r.height));
        Color edge = new Color(0.1f, 0.08f, 0.1f, 0.95f * alpha);
        Color fill = new Color(1f, 0.96f, 0.86f, 0.96f * alpha);
        Color shade = new Color(0.86f, 0.8f, 0.68f, 0.96f * alpha);
        // outline with two-step corners
        Fill(new Rect(r.x + 2 * u, r.y, r.width - 4 * u, u), edge);
        Fill(new Rect(r.x + 2 * u, r.yMax - u, r.width - 4 * u, u), edge);
        Fill(new Rect(r.x, r.y + 2 * u, u, r.height - 4 * u), edge);
        Fill(new Rect(r.xMax - u, r.y + 2 * u, u, r.height - 4 * u), edge);
        Fill(new Rect(r.x + u, r.y + u, u, u), edge);
        Fill(new Rect(r.xMax - 2 * u, r.y + u, u, u), edge);
        Fill(new Rect(r.x + u, r.yMax - 2 * u, u, u), edge);
        Fill(new Rect(r.xMax - 2 * u, r.yMax - 2 * u, u, u), edge);
        // fill, with a shadow line along the bottom
        Fill(new Rect(r.x + 2 * u, r.y + u, r.width - 4 * u, r.height - 2 * u), fill);
        Fill(new Rect(r.x + u, r.y + 2 * u, r.width - 2 * u, r.height - 4 * u), fill);
        Fill(new Rect(r.x + 2 * u, r.yMax - 2 * u, r.width - 4 * u, u), shade);
        // tail
        float cx = Mathf.Round(r.center.x / u) * u;
        Fill(new Rect(cx - 2 * u, r.yMax - u, 4 * u, u), shade);
        Fill(new Rect(cx - u, r.yMax, 2 * u, u), shade);
        Fill(new Rect(cx - 3 * u, r.yMax - u, u, u), edge);
        Fill(new Rect(cx + 2 * u, r.yMax - u, u, u), edge);
        Fill(new Rect(cx - 2 * u, r.yMax, u, u), edge);
        Fill(new Rect(cx + u, r.yMax, u, u), edge);
        Fill(new Rect(cx - u, r.yMax + u, 2 * u, u), edge);
    }

    // A small wooden sign: planks with dark seams, a light top edge on each plank, nails in the corners.
    public static void Board(Rect r)
    {
        float u = Unit;
        r = new Rect(Mathf.Round(r.x), Mathf.Round(r.y), Mathf.Round(r.width / u) * u, Mathf.Round(r.height / u) * u);
        Color edge = new Color(0.16f, 0.09f, 0.05f, 0.97f);
        Color wood = new Color(0.56f, 0.36f, 0.2f, 0.97f);
        Color light = new Color(0.68f, 0.46f, 0.27f, 0.97f);
        Color seam = new Color(0.36f, 0.22f, 0.12f, 0.97f);
        Fill(new Rect(r.x + u, r.y, r.width - 2 * u, u), edge);
        Fill(new Rect(r.x + u, r.yMax - u, r.width - 2 * u, u), edge);
        Fill(new Rect(r.x, r.y + u, u, r.height - 2 * u), edge);
        Fill(new Rect(r.xMax - u, r.y + u, u, r.height - 2 * u), edge);
        Fill(new Rect(r.x + u, r.y + u, r.width - 2 * u, r.height - 2 * u), wood);
        // planks every 8 art pixels
        float plank = 8 * u;
        for (float y = r.y + u; y < r.yMax - 2 * u; y += plank)
        {
            if (y > r.y + u) Fill(new Rect(r.x + u, y - u, r.width - 2 * u, u), seam);
            Fill(new Rect(r.x + u, y, r.width - 2 * u, u), light);
        }
        // wood grain: a few short dark streaks
        int cols = Mathf.Max(8, (int)(r.width / u) - 10), rows = Mathf.Max(4, (int)(r.height / u) - 6);
        for (int i = 0; i < 7; i++)
            Fill(new Rect(r.x + u * (4 + (i * 17) % cols), r.y + u * (3 + (i * 5) % rows), 3 * u, u), seam);
        Color nail = new Color(0.85f, 0.82f, 0.75f, 1f);
        Fill(new Rect(r.x + 2 * u, r.y + 2 * u, u, u), nail);
        Fill(new Rect(r.xMax - 3 * u, r.y + 2 * u, u, u), nail);
        Fill(new Rect(r.x + 2 * u, r.yMax - 3 * u, u, u), nail);
        Fill(new Rect(r.xMax - 3 * u, r.yMax - 3 * u, u, u), nail);
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
