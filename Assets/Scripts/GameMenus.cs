using UnityEngine;

// Settings saved on this computer (volume, fullscreen), applied when the game starts.
public static class GameSettings
{
    const string VolumeKey = "xeno_volume";
    const string MiniMapKey = "xeno_minimap";

    // Mini map size: 0 small, 1 medium, 2 large
    public static readonly string[] MiniMapSizes = { "Small", "Medium", "Large" };
    static readonly float[] MiniMapWidths = { 170f, 260f, 400f };   // pixels wide on a 1080p screen
    public static int MiniMapSize
    {
        get => Mathf.Clamp(PlayerPrefs.GetInt(MiniMapKey, 1), 0, MiniMapSizes.Length - 1);
        set { PlayerPrefs.SetInt(MiniMapKey, (value + MiniMapSizes.Length) % MiniMapSizes.Length); PlayerPrefs.Save(); }
    }
    public static float MiniMapWidth => MiniMapWidths[MiniMapSize];

    // Size of each part of the UI, for accessibility
    public enum UiPart { Board, Meter, Skills, Tips, Menus }
    public static readonly string[] UiSizes = { "Small", "Medium", "Large", "Huge" };
    static readonly float[] HudScales = { 0.8f, 1f, 1.3f, 1.6f };
    static readonly float[] MenuScales = { 0.85f, 1f, 1.1f, 1.2f };

    public static int UiSize(UiPart part) => Mathf.Clamp(PlayerPrefs.GetInt("xeno_ui_" + part, 1), 0, UiSizes.Length - 1);
    public static void SetUiSize(UiPart part, int size)
    {
        PlayerPrefs.SetInt("xeno_ui_" + part, (size + UiSizes.Length) % UiSizes.Length);
        PlayerPrefs.Save();
    }
    public static float UiScale(UiPart part) => (part == UiPart.Menus ? MenuScales : HudScales)[UiSize(part)];

    public static float Volume
    {
        get => PlayerPrefs.GetFloat(VolumeKey, 0.8f);
        set
        {
            PlayerPrefs.SetFloat(VolumeKey, Mathf.Clamp01(Mathf.Round(value * 10f) / 10f));
            PlayerPrefs.Save();
            AudioListener.volume = Volume;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Apply() { AudioListener.volume = Volume; }
}

// Menu pages shared by the pause menu (Esc / Tab in a level) and the start screen:
// Controls (every key plus how to play) and Settings (volume, fullscreen).
public static class MenuPages
{
    static readonly Color Title = new Color(0.55f, 1f, 0.6f);
    static readonly Color Key = new Color(1f, 0.85f, 0.4f);
    static readonly Color Body = new Color(1f, 1f, 1f, 0.9f);
    public static readonly Color PanelColor = new Color(0.1f, 0.16f, 0.11f, 0.96f);

    public static void Dim() => Hud.Fill(new Rect(0, 0, Screen.width, Screen.height), new Color(0.02f, 0.03f, 0.05f, 0.75f));

    // A menu button; "selected" is the keyboard choice (W/S + Enter), drawn like a hovered button
    public static bool Button(Rect r, string label, bool selected, int size = 18)
    {
        bool lit = selected || Hud.Hover(r);
        Hud.Panel(r, lit ? new Color(0.25f, 0.55f, 0.3f, 0.97f) : new Color(0.14f, 0.3f, 0.17f, 0.97f));
        Hud.Text(r, (selected ? "> " : "") + label + (selected ? " <" : ""), size, lit ? Color.white : new Color(0.85f, 1f, 0.85f));
        return Hud.Clicked(r);
    }

    // ------------------------------------------------------------------ controls
    // Returns true when Back is clicked
    public static bool DrawControls(AlienPlayer alien)
    {
        Hud.Use(GameSettings.UiPart.Menus);
        float W = Screen.width, H = Screen.height, k = Hud.Scale;
        Dim();
        float w = Mathf.Min(W - 40f * k, 1180f * k), h = Mathf.Min(H - 40f * k, 640f * k);
        Rect r = new Rect((W - w) / 2f, (H - h) / 2f, w, h);
        Hud.Panel(r, PanelColor);
        Hud.Text(new Rect(r.x, r.y + 18f * k, w, 40f * k), "CONTROLS", 30, Title);

        string[,] keys =
        {
            { "W A S D", "Move" },
            { "H", "Small Beam" },
            { "J", SkillLine(alien, 1) },
            { "K", SkillLine(alien, 2) },
            { "L", SkillLine(alien, 3) },
            { "M", "Mini map on / off" },
            { "Tab", "This page" },
            { "Esc", "Pause menu" },
            { "R", "Restart level" },
        };
        float col = r.x + 40f * k, y = r.y + 84f * k, line = 34f * k;
        Hud.Text(new Rect(col, y, 300f * k, 24f * k), "Keys", 18, Title, TextAnchor.MiddleLeft);
        y += line + 4f * k;
        for (int i = 0; i < keys.GetLength(0); i++)
        {
            Hud.Text(new Rect(col, y, 120f * k, 24f * k), keys[i, 0], 15, Key, TextAnchor.MiddleLeft);
            Hud.Text(new Rect(col + 120f * k, y, w * 0.5f - 140f * k, 24f * k), keys[i, 1], 13, Body, TextAnchor.MiddleLeft);
            y += line;
        }

        string[] how =
        {
            "Beam people to befriend them.",
            "Blue pips = hits still needed.",
            "Scared people run for the exits.",
            "Too many escape = you lose.",
            "Yellow circle = lookout.",
            "  Beam them before the alarm.",
            "Guards see inside their cone.",
            "Soldiers shoot darts, and the",
            "  alarm puts every guard on you.",
            "Green arrow = where you aim.",
            "Fill the meter to evolve",
            "  and learn a new skill.",
        };
        float col2 = r.x + w * 0.54f;
        y = r.y + 84f * k;
        Hud.Text(new Rect(col2, y, 300f * k, 24f * k), "How to play", 18, Title, TextAnchor.MiddleLeft);
        y += line + 4f * k;
        foreach (string s in how)
        {
            Hud.Text(new Rect(col2, y, w * 0.44f, 24f * k), s, 13, Body, TextAnchor.MiddleLeft);
            y += line * 0.92f;
        }

        Rect back = new Rect(r.x + (w - 220f * k) / 2f, r.yMax - 70f * k, 220f * k, 48f * k);
        bool clicked = Button(back, "Back", false);
        Hud.Text(new Rect(r.x, r.yMax - 22f * k, w, 18f * k), "Tab or Esc to close", 11, new Color(1f, 1f, 1f, 0.5f));
        return clicked;
    }

    static string SkillLine(AlienPlayer alien, int slot)
    {
        if (alien != null && alien.skills != null && slot < alien.skills.Length && alien.skills[slot].unlocked)
            return alien.skills[slot].name;
        return "Locked until evolution " + slot;
    }

    // ------------------------------------------------------------------ settings
    // Returns true when Back is clicked
    public static bool DrawSettings()
    {
        Hud.Use(GameSettings.UiPart.Menus);
        float W = Screen.width, H = Screen.height, k = Hud.Scale, u = Hud.Unit;
        Dim();
        float w = 600f * k, h = 820f * k;
        Rect r = new Rect((W - w) / 2f, (H - h) / 2f, w, h);
        Hud.Panel(r, PanelColor);
        Hud.Text(new Rect(r.x, r.y + 18f * k, w, 40f * k), "SETTINGS", 30, Title);

        // Volume: < [bar] >
        float y = r.y + 100f * k;
        Hud.Text(new Rect(r.x + 40f * k, y, 160f * k, 40f * k), "Volume", 16, Body, TextAnchor.MiddleLeft);
        float vol = GameSettings.Volume;
        Rect minus = new Rect(r.x + 190f * k, y, 40f * k, 40f * k);
        Rect plus = new Rect(r.xMax - 80f * k, y, 40f * k, 40f * k);
        if (Button(minus, "-", false)) GameSettings.Volume = vol - 0.1f;
        if (Button(plus, "+", false)) GameSettings.Volume = vol + 0.1f;
        Rect bar = new Rect(minus.xMax + 12f * k, y + 12f * k, plus.x - minus.xMax - 24f * k, 16f * k);
        Hud.Panel(bar, new Color(0.05f, 0.08f, 0.06f, 1f));
        Hud.Fill(new Rect(bar.x + u, bar.y + u, Mathf.Round((bar.width - 2 * u) * GameSettings.Volume / u) * u, bar.height - 2 * u), Title);
        if (Hud.Clicked(bar)) GameSettings.Volume = (Event.current.mousePosition.x - bar.x) / bar.width;
        Hud.Text(new Rect(r.x, y + 44f * k, w, 20f * k), Mathf.RoundToInt(GameSettings.Volume * 10f) + " / 10", 13, Body);

        // Fullscreen on / off
        y += 100f * k;
        Hud.Text(new Rect(r.x + 40f * k, y, 160f * k, 44f * k), "Fullscreen", 16, Body, TextAnchor.MiddleLeft);
        if (Button(new Rect(r.x + 230f * k, y, 160f * k, 44f * k), Screen.fullScreen ? "On" : "Off", false))
            Screen.fullScreen = !Screen.fullScreen;

        // Mini map size: click to cycle Small / Medium / Large
        y += 76f * k;
        Hud.Text(new Rect(r.x + 40f * k, y, 160f * k, 44f * k), "Mini map", 16, Body, TextAnchor.MiddleLeft);
        if (Button(new Rect(r.x + 230f * k, y, 160f * k, 44f * k), GameSettings.MiniMapSizes[GameSettings.MiniMapSize], false))
            GameSettings.MiniMapSize++;

        // Size of each part of the HUD and the menus (click to cycle)
        y += 66f * k;
        Hud.Text(new Rect(r.x, y, w, 30f * k), "Interface size", 16, Title);
        string[] names = { "Friends board", "Friend meter", "Skill bar", "Tips, messages", "Menus" };
        for (int i = 0; i < names.Length; i++)
        {
            y += 54f * k;
            GameSettings.UiPart part = (GameSettings.UiPart)i;
            Hud.Text(new Rect(r.x + 40f * k, y, 200f * k, 44f * k), names[i], 14, Body, TextAnchor.MiddleLeft);
            if (Button(new Rect(r.x + 280f * k, y, 160f * k, 44f * k), GameSettings.UiSizes[GameSettings.UiSize(part)], false, 16))
                GameSettings.SetUiSize(part, GameSettings.UiSize(part) + 1);
            Hud.Use(GameSettings.UiPart.Menus);
        }

        Rect back = new Rect(r.x + (w - 220f * k) / 2f, r.yMax - 70f * k, 220f * k, 48f * k);
        return Button(back, "Back", false);
    }
}
