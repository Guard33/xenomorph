using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// The scoreboard (Danesh): how many people the alien befriended on each level.
// Every time a level ends (complete or lost) the run is saved on this computer (PlayerPrefs):
// the best number of friends, how many times the level was played, whether it was cleared,
// and the top 5 runs. The start screen shows it all on the Scoreboard screen.
public static class Scoreboard
{
    public const int TopRuns = 5;

    // One finished run of a level
    public struct Run
    {
        public int friends, people;
        public bool cleared;
        public string date;
    }

    // Number of levels in the game: Level 1 plus one level after each evolution
    public static int LevelCount => UpgradePath.Tiers.Length + 1;

    // The result of the last run, for the level complete card
    public static int LastLevel { get; private set; }
    public static bool LastWasNewBest { get; private set; }
    public static int LastRank { get; private set; }   // 1 = top run, 0 = not in the top 5

    static string Key(int level, string what) => "scoreboard.level" + level + "." + what;

    public static int Best(int level) => PlayerPrefs.GetInt(Key(level, "best"), 0);
    public static int People(int level) => PlayerPrefs.GetInt(Key(level, "people"), 0);
    public static int Plays(int level) => PlayerPrefs.GetInt(Key(level, "plays"), 0);
    public static bool Cleared(int level) => PlayerPrefs.GetInt(Key(level, "cleared"), 0) == 1;

    // Total of the best scores on every level
    public static int TotalFriends
    {
        get
        {
            int sum = 0;
            for (int l = 1; l <= LevelCount; l++) sum += Best(l);
            return sum;
        }
    }

    public static string LevelName(int level)
    {
        if (level <= 1) return "Quiet Farm Town";
        string n = UpgradePath.Tiers[level - 2].nextLevel;
        int colon = n.IndexOf(':');
        return colon >= 0 ? n.Substring(colon + 1).Trim() : n;
    }

    // Saves a finished run. Returns true when it beats the best score on that level.
    public static bool Record(int level, int friends, int people, bool cleared)
    {
        level = Mathf.Clamp(level, 1, LevelCount);
        bool newBest = friends > Best(level) || Plays(level) == 0;
        if (friends > Best(level)) PlayerPrefs.SetInt(Key(level, "best"), friends);
        PlayerPrefs.SetInt(Key(level, "people"), people);
        PlayerPrefs.SetInt(Key(level, "plays"), Plays(level) + 1);
        if (cleared) PlayerPrefs.SetInt(Key(level, "cleared"), 1);

        // Top 5 runs: most friends first, cleared runs before lost runs on a tie
        Run run = new Run { friends = friends, people = people, cleared = cleared, date = System.DateTime.Now.ToString("MMM d") };
        List<Run> top = Top(level);
        int rank = 0;
        for (int i = 0; i <= top.Count && i < TopRuns; i++)
        {
            if (i == top.Count || Better(run, top[i]))
            {
                top.Insert(i, run);
                rank = i + 1;
                break;
            }
        }
        if (top.Count > TopRuns) top.RemoveRange(TopRuns, top.Count - TopRuns);
        Save(level, top);
        PlayerPrefs.Save();

        LastLevel = level;
        LastWasNewBest = newBest && friends > 0;
        LastRank = rank;
        return LastWasNewBest;
    }

    // 1st, 2nd, 3rd, 4th, 5th
    public static string Ordinal(int n) => n + (n == 1 ? "st" : n == 2 ? "nd" : n == 3 ? "rd" : "th");

    static bool Better(Run a, Run b) => a.friends > b.friends || (a.friends == b.friends && a.cleared && !b.cleared);

    // Runs are saved as "friends,people,cleared,date|friends,people,cleared,date|..."
    public static List<Run> Top(int level)
    {
        List<Run> list = new List<Run>();
        string s = PlayerPrefs.GetString(Key(level, "top"), "");
        foreach (string part in s.Split('|'))
        {
            string[] f = part.Split(',');
            if (f.Length < 4) continue;
            int friends, people;
            if (!int.TryParse(f[0], out friends) || !int.TryParse(f[1], out people)) continue;
            list.Add(new Run { friends = friends, people = people, cleared = f[2] == "1", date = f[3] });
        }
        return list;
    }

    static void Save(int level, List<Run> runs)
    {
        List<string> parts = new List<string>();
        foreach (Run r in runs) parts.Add(r.friends + "," + r.people + "," + (r.cleared ? "1" : "0") + "," + r.date);
        PlayerPrefs.SetString(Key(level, "top"), string.Join("|", parts));
    }

    public static void ResetAll()
    {
        for (int l = 1; l <= LevelCount; l++)
            foreach (string w in new[] { "best", "people", "plays", "cleared", "top" })
                PlayerPrefs.DeleteKey(Key(l, w));
        PlayerPrefs.Save();
        LastLevel = 0;
    }
}

// The Scoreboard screen, opened from the start screen (button or B).
// Same pixel-art style as the rest of the menus (Hud + PixelFont).
public class ScoreboardScreen
{
    public bool Open;
    int selected;          // level row picked (0 = Level 1)
    bool confirmReset;

    static readonly Color Pink = new Color(0.96f, 0.45f, 0.68f);
    static readonly Color Green = new Color(0.55f, 1f, 0.6f);
    static readonly Color Gold = new Color(1f, 0.85f, 0.35f);
    static readonly Color Dim = new Color(1f, 1f, 1f, 0.45f);

#if ENABLE_INPUT_SYSTEM
    public void HandleKeys(Keyboard kb)
    {
        int count = Scoreboard.LevelCount;
        if (kb.wKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame) selected = (selected + count - 1) % count;
        if (kb.sKey.wasPressedThisFrame || kb.downArrowKey.wasPressedThisFrame) selected = (selected + 1) % count;
        for (int i = 0; i < count && i < 9; i++)
            if (kb[(Key)((int)Key.Digit1 + i)].wasPressedThisFrame) selected = i;
        if (kb.escapeKey.wasPressedThisFrame || kb.bKey.wasPressedThisFrame) Close();
    }
#endif

    void Close() { Open = false; confirmReset = false; }

    public void Draw()
    {
        float W = Screen.width, H = Screen.height, k = Hud.Scale, u = Hud.Unit;
        Hud.Fill(new Rect(0, 0, W, H), new Color(0.02f, 0.03f, 0.05f, 0.55f));
        Hud.Text(new Rect(0, H * 0.05f, W, H * 0.09f), "SCOREBOARD", 40, Green);
        Hud.Text(new Rect(0, H * 0.135f, W, 24f * k), "Most people befriended on each level", 14, new Color(1f, 1f, 1f, 0.75f));

        // One row per level: name, plays, a pixel bar of the best score, and a star when cleared
        float rw = 760f * k, rh = 64f * k, gap = 10f * k;
        float rx = (W - rw) / 2f, y = H * 0.19f;
        for (int i = 0; i < Scoreboard.LevelCount; i++)
        {
            int level = i + 1;
            Rect r = new Rect(rx, y, rw, rh);
            int plays = Scoreboard.Plays(level);
            bool sel = i == selected;
            Color fill = sel ? new Color(0.25f, 0.55f, 0.3f, 0.95f)
                       : Hud.Hover(r) ? new Color(0.19f, 0.4f, 0.23f, 0.95f) : new Color(0.14f, 0.3f, 0.17f, 0.95f);
            if (plays == 0 && !sel) fill = new Color(0.1f, 0.1f, 0.12f, 0.88f);
            Hud.Panel(r, fill);

            Color tc = plays > 0 || sel ? Color.white : new Color(1f, 1f, 1f, 0.45f);
            Hud.Text(new Rect(r.x + 18f * k, r.y + 10f * k, 300f * k, 22f * k), level + "  " + Scoreboard.LevelName(level), 16, tc, TextAnchor.MiddleLeft);
            string sub = plays == 0 ? "Not played yet" : "Played " + plays + (plays == 1 ? " time" : " times");
            Hud.Text(new Rect(r.x + 18f * k + Hud.TextWidth(level + "  ", 16), r.y + 36f * k, 300f * k, 18f * k), sub, 11, new Color(1f, 1f, 1f, 0.6f), TextAnchor.MiddleLeft);

            // Best score bar
            int best = Scoreboard.Best(level), people = Mathf.Max(1, Scoreboard.People(level));
            float bx = r.x + 330f * k, bw = 66 * u, bh = 5 * u;
            float by = Mathf.Round(r.y + (rh - bh) / 2f);
            Hud.Panel(new Rect(bx, by, bw, bh), new Color(0.12f, 0.1f, 0.14f, 0.85f));
            float f = plays == 0 ? 0f : Mathf.Clamp01((float)best / people);
            Hud.Fill(new Rect(bx + u, by + u, Mathf.Round((bw - 2 * u) * f / u) * u, bh - 2 * u), Pink);
            string score = plays == 0 ? "-" : best + "/" + Scoreboard.People(level);
            Hud.Text(new Rect(bx + bw + 14f * k, r.y, 120f * k, rh), score, 18, plays == 0 ? Dim : Color.white, TextAnchor.MiddleLeft);

            // Star when the level has been cleared
            if (Scoreboard.Cleared(level)) Star(new Vector2(r.xMax - 40f * k, r.center.y), u, Gold);
            else if (plays > 0) Hud.Text(new Rect(r.x, r.y, r.width - 20f * k, rh), "not cleared", 11, Dim, TextAnchor.MiddleRight);

            if (Hud.Clicked(r)) selected = i;
            y += rh + gap;
        }

        // Total of all best scores
        y += 4f * k;
        Hud.Text(new Rect(0, y, W, 28f * k), "Total friends made: " + Scoreboard.TotalFriends, 18, Pink);
        y += 42f * k;

        // Top 5 runs of the selected level
        int lv = selected + 1;
        List<Scoreboard.Run> top = Scoreboard.Top(lv);
        float pw = 520f * k, ph = (54f + Scoreboard.TopRuns * 28f) * k;
        Rect p = new Rect((W - pw) / 2f, y, pw, ph);
        Hud.Board(p);
        Hud.Text(new Rect(p.x, p.y + 12f * k, pw, 22f * k), "Top runs: " + Scoreboard.LevelName(lv), 15, Color.white);
        for (int i = 0; i < Scoreboard.TopRuns; i++)
        {
            float ly = p.y + (44f + i * 28f) * k;
            Color c = i == 0 ? Gold : new Color(1f, 0.95f, 0.85f);
            if (i >= top.Count)
            {
                Hud.Text(new Rect(p.x + 40f * k, ly, pw, 22f * k), (i + 1) + ".  ---", 13, new Color(1f, 0.95f, 0.85f, 0.45f), TextAnchor.MiddleLeft);
                continue;
            }
            Scoreboard.Run run = top[i];
            Hud.Text(new Rect(p.x + 40f * k, ly, pw, 22f * k), (i + 1) + ".  " + run.friends + "/" + run.people + " friends", 13, c, TextAnchor.MiddleLeft);
            Hud.Text(new Rect(p.x + 270f * k, ly, 120f * k, 22f * k), run.cleared ? "cleared" : "lost", 13, run.cleared ? Green : new Color(1f, 0.62f, 0.55f), TextAnchor.MiddleLeft);
            Hud.Text(new Rect(p.x, ly, pw - 40f * k, 22f * k), run.date, 13, c, TextAnchor.MiddleRight);
        }

        // Back / Reset (reset asks twice so scores are not wiped by mistake)
        float bW = 220f * k, bH = 50f * k, bY = p.yMax + 20f * k;
        if (Hud.Button(new Rect(W / 2f - bW - 10f * k, bY, bW, bH), "Back", 18)) Close();
        if (Hud.Button(new Rect(W / 2f + 10f * k, bY, bW, bH), confirmReset ? "Sure? Click again" : "Reset scores", confirmReset ? 13 : 16))
        {
            if (confirmReset) { Scoreboard.ResetAll(); confirmReset = false; }
            else confirmReset = true;
        }
        Hud.Text(new Rect(0, H - 34f * k, W, 22f * k), "W S or 1-" + Scoreboard.LevelCount + " choose level   Esc back", 13, new Color(1f, 1f, 1f, 0.7f));
    }

    // A small pixel-art star (7 x 7 art pixels)
    public static void Star(Vector2 c, float u, Color color)
    {
        string[] rows = { "...#...", "...#...", "#######", ".#####.", "..###..", ".##.##.", "##...##" };
        float x0 = Mathf.Round(c.x - 3.5f * u), y0 = Mathf.Round(c.y - 3.5f * u);
        Color edge = new Color(0.25f, 0.15f, 0.02f, 1f);
        for (int y = 0; y < 7; y++)
            for (int x = 0; x < 7; x++)
                if (rows[y][x] == '#') Hud.Fill(new Rect(x0 + x * u + u, y0 + y * u + u, u, u), edge);   // shadow
        for (int y = 0; y < 7; y++)
            for (int x = 0; x < 7; x++)
                if (rows[y][x] == '#') Hud.Fill(new Rect(x0 + x * u, y0 + y * u, u, u), color);
    }
}
