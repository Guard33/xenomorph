using System.Collections.Generic;
using UnityEngine;

// A tiny 5 x 7 pixel font drawn from code, so all game text matches the pixel art.
// Capital letters only (text is upper-cased). Each glyph is drawn from one small texture.
public static class PixelFont
{
    public const int GlyphHeight = 7;
    const int LetterGap = 1;
    const int LineGap = 3;

    static readonly Dictionary<char, string[]> Glyphs = new Dictionary<char, string[]>
    {
        ['A'] = new[] { ".###.", "#...#", "#...#", "#####", "#...#", "#...#", "#...#" },
        ['B'] = new[] { "####.", "#...#", "#...#", "####.", "#...#", "#...#", "####." },
        ['C'] = new[] { ".###.", "#...#", "#....", "#....", "#....", "#...#", ".###." },
        ['D'] = new[] { "####.", "#...#", "#...#", "#...#", "#...#", "#...#", "####." },
        ['E'] = new[] { "#####", "#....", "#....", "####.", "#....", "#....", "#####" },
        ['F'] = new[] { "#####", "#....", "#....", "####.", "#....", "#....", "#...." },
        ['G'] = new[] { ".###.", "#...#", "#....", "#.###", "#...#", "#...#", ".####" },
        ['H'] = new[] { "#...#", "#...#", "#...#", "#####", "#...#", "#...#", "#...#" },
        ['I'] = new[] { "###", ".#.", ".#.", ".#.", ".#.", ".#.", "###" },
        ['J'] = new[] { "..###", "...#.", "...#.", "...#.", "...#.", "#..#.", ".##.." },
        ['K'] = new[] { "#...#", "#..#.", "#.#..", "##...", "#.#..", "#..#.", "#...#" },
        ['L'] = new[] { "#....", "#....", "#....", "#....", "#....", "#....", "#####" },
        ['M'] = new[] { "#...#", "##.##", "#.#.#", "#.#.#", "#...#", "#...#", "#...#" },
        ['N'] = new[] { "#...#", "#...#", "##..#", "#.#.#", "#..##", "#...#", "#...#" },
        ['O'] = new[] { ".###.", "#...#", "#...#", "#...#", "#...#", "#...#", ".###." },
        ['P'] = new[] { "####.", "#...#", "#...#", "####.", "#....", "#....", "#...." },
        ['Q'] = new[] { ".###.", "#...#", "#...#", "#...#", "#.#.#", "#..#.", ".##.#" },
        ['R'] = new[] { "####.", "#...#", "#...#", "####.", "#.#..", "#..#.", "#...#" },
        ['S'] = new[] { ".####", "#....", "#....", ".###.", "....#", "....#", "####." },
        ['T'] = new[] { "#####", "..#..", "..#..", "..#..", "..#..", "..#..", "..#.." },
        ['U'] = new[] { "#...#", "#...#", "#...#", "#...#", "#...#", "#...#", ".###." },
        ['V'] = new[] { "#...#", "#...#", "#...#", "#...#", "#...#", ".#.#.", "..#.." },
        ['W'] = new[] { "#...#", "#...#", "#...#", "#.#.#", "#.#.#", "#.#.#", ".#.#." },
        ['X'] = new[] { "#...#", "#...#", ".#.#.", "..#..", ".#.#.", "#...#", "#...#" },
        ['Y'] = new[] { "#...#", "#...#", ".#.#.", "..#..", "..#..", "..#..", "..#.." },
        ['Z'] = new[] { "#####", "....#", "...#.", "..#..", ".#...", "#....", "#####" },
        ['0'] = new[] { ".###.", "#...#", "#..##", "#.#.#", "##..#", "#...#", ".###." },
        ['1'] = new[] { ".#.", "##.", ".#.", ".#.", ".#.", ".#.", "###" },
        ['2'] = new[] { ".###.", "#...#", "....#", "...#.", "..#..", ".#...", "#####" },
        ['3'] = new[] { "####.", "....#", "....#", ".###.", "....#", "....#", "####." },
        ['4'] = new[] { "...#.", "..##.", ".#.#.", "#..#.", "#####", "...#.", "...#." },
        ['5'] = new[] { "#####", "#....", "####.", "....#", "....#", "#...#", ".###." },
        ['6'] = new[] { ".###.", "#....", "#....", "####.", "#...#", "#...#", ".###." },
        ['7'] = new[] { "#####", "....#", "...#.", "..#..", ".#...", ".#...", ".#..." },
        ['8'] = new[] { ".###.", "#...#", "#...#", ".###.", "#...#", "#...#", ".###." },
        ['9'] = new[] { ".###.", "#...#", "#...#", ".####", "....#", "....#", ".###." },
        ['.'] = new[] { ".", ".", ".", ".", ".", ".", "#" },
        [','] = new[] { "..", "..", "..", "..", "..", ".#", "#." },
        ['!'] = new[] { "#", "#", "#", "#", "#", ".", "#" },
        ['?'] = new[] { ".###.", "#...#", "....#", "...#.", "..#..", ".....", "..#.." },
        [':'] = new[] { ".", ".", "#", ".", ".", "#", "." },
        ['\''] = new[] { "#", "#", ".", ".", ".", ".", "." },
        ['/'] = new[] { "....#", "...#.", "...#.", "..#..", ".#...", ".#...", "#...." },
        ['-'] = new[] { "...", "...", "...", "###", "...", "...", "..." },
        ['+'] = new[] { "...", "...", ".#.", "###", ".#.", "...", "..." },
        ['='] = new[] { "...", "...", "###", "...", "###", "...", "..." },
        ['('] = new[] { ".#", "#.", "#.", "#.", "#.", "#.", ".#" },
        [')'] = new[] { "#.", ".#", ".#", ".#", ".#", ".#", "#." },
        ['<'] = new[] { "...", "..#", ".#.", "#..", ".#.", "..#", "..." },
        ['>'] = new[] { "...", "#..", ".#.", "..#", ".#.", "#..", "..." },
        [' '] = new[] { "...", "...", "...", "...", "...", "...", "..." },
    };

    static Texture2D atlas;
    static readonly Dictionary<char, Rect> uv = new Dictionary<char, Rect>();
    static readonly Dictionary<char, int> width = new Dictionary<char, int>();

    static void Build()
    {
        int total = 0;
        foreach (var g in Glyphs) total += g.Value[0].Length + 1;
        atlas = new Texture2D(total, GlyphHeight, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
        Color32[] px = new Color32[total * GlyphHeight];
        int x0 = 0;
        foreach (var g in Glyphs)
        {
            int w = g.Value[0].Length;
            for (int row = 0; row < GlyphHeight; row++)
                for (int x = 0; x < w; x++)
                    if (g.Value[row][x] == '#')
                        px[(GlyphHeight - 1 - row) * total + x0 + x] = new Color32(255, 255, 255, 255);
            uv[g.Key] = new Rect((float)x0 / total, 0f, (float)w / total, 1f);
            width[g.Key] = w;
            x0 += w + 1;
        }
        atlas.SetPixels32(px);
        atlas.Apply();
    }

    // Size of one font pixel on screen for a "font size" in points (scaled with the window)
    public static float PixelSize(int fontSize) => Mathf.Max(1f, Mathf.Round(fontSize / 7f * Hud.Scale));

    public static float LineWidth(string line, float px)
    {
        if (atlas == null) Build();
        float w = 0f;
        foreach (char ch in line.ToUpperInvariant())
            w += ((width.TryGetValue(ch, out int gw) ? gw : 3) + LetterGap) * px;
        return Mathf.Max(0f, w - LetterGap * px);
    }

    public static float Height(string text, float px)
    {
        int lines = text.Split('\n').Length;
        return (lines * GlyphHeight + (lines - 1) * LineGap) * px;
    }

    // Draw text inside a rect. With 'outline', a dark 1-pixel outline is drawn first.
    public static void Draw(Rect r, string text, float px, Color color, TextAnchor anchor, bool outline = true)
    {
        if (Event.current.type != EventType.Repaint) return;
        if (atlas == null) Build();

        string[] lines = text.ToUpperInvariant().Split('\n');
        float blockH = Height(text, px);
        float y = r.y;
        if (anchor == TextAnchor.MiddleLeft || anchor == TextAnchor.MiddleCenter || anchor == TextAnchor.MiddleRight) y = r.y + (r.height - blockH) / 2f;
        else if (anchor == TextAnchor.LowerLeft || anchor == TextAnchor.LowerCenter || anchor == TextAnchor.LowerRight) y = r.yMax - blockH;
        y = Mathf.Round(y);

        Color old = GUI.color;
        foreach (string line in lines)
        {
            float lw = LineWidth(line, px);
            float x = r.x;
            if (anchor == TextAnchor.UpperCenter || anchor == TextAnchor.MiddleCenter || anchor == TextAnchor.LowerCenter) x = r.x + (r.width - lw) / 2f;
            else if (anchor == TextAnchor.UpperRight || anchor == TextAnchor.MiddleRight || anchor == TextAnchor.LowerRight) x = r.xMax - lw;
            x = Mathf.Round(x);

            if (outline)
            {
                GUI.color = new Color(0.05f, 0.04f, 0.08f, color.a * 0.9f);
                DrawLine(line, x - px, y, px); DrawLine(line, x + px, y, px);
                DrawLine(line, x, y - px, px); DrawLine(line, x, y + px, px);
                DrawLine(line, x + px, y + px, px);
            }
            GUI.color = color;
            DrawLine(line, x, y, px);
            y += (GlyphHeight + LineGap) * px;
        }
        GUI.color = old;
    }

    static void DrawLine(string line, float x, float y, float px)
    {
        foreach (char ch in line)
        {
            if (uv.TryGetValue(ch, out Rect t))
            {
                int w = width[ch];
                if (ch != ' ') GUI.DrawTextureWithTexCoords(new Rect(x, y, w * px, GlyphHeight * px), atlas, t);
                x += (w + LetterGap) * px;
            }
            else x += (3 + LetterGap) * px;
        }
    }
}
