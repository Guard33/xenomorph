using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// Start screen and game over screen (same script, two kinds).
public class MenuScreen : MonoBehaviour
{
    public enum Kind { Start, GameOver }

    public Kind kind = Kind.Start;
    public string gameTitle = "XENOMORPH";
    public string gameScene = "Level1";
    public string startScene = "StartScreen";

    // Stage select (start screen only): jump straight to any built level.
    // Stages after Level 1 need the skills from the evolutions before them, so those are picked here too.
    private bool stageSelect;
    private int selected;                                   // index into Stages()
    private readonly int[] picks = new int[UpgradePath.Tiers.Length];   // skill picked per evolution

    // Scoreboard screen (start screen only): best number of friends on each level
    private readonly ScoreboardScreen scores = new ScoreboardScreen();

    struct Stage { public string name, scene; }

    static Stage[] Stages()
    {
        Stage[] list = new Stage[UpgradePath.Tiers.Length + 1];
        list[0] = new Stage { name = "Quiet Farm Town", scene = "Level1" };
        for (int i = 0; i < UpgradePath.Tiers.Length; i++)
        {
            string n = UpgradePath.Tiers[i].nextLevel;
            int colon = n.IndexOf(':');
            list[i + 1] = new Stage { name = colon >= 0 ? n.Substring(colon + 1).Trim() : n, scene = UpgradePath.Tiers[i].scene };
        }
        return list;
    }

    static bool Playable(Stage s) => !string.IsNullOrEmpty(s.scene) && Application.CanStreamedLevelBeLoaded(s.scene);

    void Update()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard kb = Keyboard.current;
        if (kb == null) return;
        if (scores.Open) { scores.HandleKeys(kb); return; }
        if (stageSelect)
        {
            int count = Stages().Length;
            if (kb.wKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame) selected = (selected + count - 1) % count;
            if (kb.sKey.wasPressedThisFrame || kb.downArrowKey.wasPressedThisFrame) selected = (selected + 1) % count;
            for (int i = 0; i < count && i < 9; i++)
                if (kb[(Key)((int)Key.Digit1 + i)].wasPressedThisFrame) selected = i;
            if (kb.enterKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame) PlayStage(selected);
            if (kb.escapeKey.wasPressedThisFrame) stageSelect = false;
            return;
        }
        if (kind == Kind.Start && kb.tabKey.wasPressedThisFrame) stageSelect = true;
        if (kind == Kind.Start && kb.bKey.wasPressedThisFrame) { scores.Open = true; return; }
        if (kb.enterKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame || kb.rKey.wasPressedThisFrame) Play();
        if (kb.escapeKey.wasPressedThisFrame) Back();
#else
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.R)) Play();
        if (Input.GetKeyDown(KeyCode.Escape)) Back();
#endif
    }

    void Play()
    {
        if (kind == Kind.Start)
        {
            UpgradePath.Reset();   // a new game starts with no evolutions
            SceneManager.LoadScene(gameScene);
        }
        else SceneManager.LoadScene(Application.CanStreamedLevelBeLoaded(GameResult.Scene) ? GameResult.Scene : gameScene);   // try the same level again
    }

    void PlayStage(int index)
    {
        Stage[] stages = Stages();
        if (index < 0 || index >= stages.Length || !Playable(stages[index])) return;
        UpgradePath.Reset();
        // Give the alien the skills it would have evolved by this stage
        for (int t = 0; t < index && t < UpgradePath.Chosen.Length; t++) UpgradePath.Chosen[t] = picks[t];
        SceneManager.LoadScene(stages[index].scene);
    }

    void Back()
    {
        if (kind == Kind.GameOver) SceneManager.LoadScene(startScene);
        else Application.Quit();
    }

    // Same pixel-art style as the in-game HUD (PixelFont + Hud panels and buttons)
    void OnGUI()
    {
        if (stageSelect) { DrawStageSelect(); return; }
        if (scores.Open) { scores.Draw(); return; }
        float W = Screen.width, H = Screen.height, k = Hud.Scale;
        bool start = kind == Kind.Start;

        string title = start ? gameTitle : "GAME OVER";
        Color titleColor = start ? new Color(0.55f, 1f, 0.6f) : new Color(1f, 0.35f, 0.35f);
        Hud.Text(new Rect(0, H * 0.14f, W, H * 0.16f), title, 72, titleColor);

        string sub = start
            ? "A lonely alien crash-lands on Earth.\nBrainwash the people before they escape!"
            : GameResult.Reason + "\nFriends " + GameResult.Friends + "/" + GameResult.Needed + "   Escaped " + GameResult.Escaped
              + "\nBest on this level: " + Scoreboard.Best(GameResult.Level) + " friends";
        Hud.Text(new Rect(0, H * 0.34f, W, H * 0.12f), sub, 16, Color.white);

        // The start screen has 4 buttons, so they are a little smaller there
        float bw = 260f * k, bh = (start ? 48f : 56f) * k, gap = (start ? 12f : 18f) * k;
        float bx = (W - bw) / 2f, by = start ? H * 0.5f : H * 0.52f;
        if (Hud.Button(new Rect(bx, by, bw, bh), start ? "Start" : "Try again", 20)) Play();
        if (start)
        {
            if (Hud.Button(new Rect(bx, by + bh + gap, bw, bh), "Stage select", 20)) stageSelect = true;
            by += bh + gap;
            if (Hud.Button(new Rect(bx, by + bh + gap, bw, bh), "Scoreboard", 20)) scores.Open = true;
            by += bh + gap;
        }
        if (Hud.Button(new Rect(bx, by + bh + gap, bw, bh), start ? "Quit" : "Main menu", 20)) Back();

        bool blink = Mathf.Repeat(Time.unscaledTime, 1.2f) < 0.8f;
        if (blink)
            Hud.Text(new Rect(0, H * 0.79f, W, 24f * k), start ? "Press Enter to start" : "Press Enter to try again", 14, new Color(1f, 1f, 1f, 0.85f));
        if (start)
            Hud.Text(new Rect(0, H * 0.87f, W, 24f * k), "WASD move   H beam   M map   R restart   Tab stage select   B scores", 13, new Color(1f, 1f, 1f, 0.7f));
    }

    void DrawStageSelect()
    {
        float W = Screen.width, H = Screen.height, k = Hud.Scale;
        Stage[] stages = Stages();
        Hud.Text(new Rect(0, H * 0.07f, W, H * 0.1f), "STAGE SELECT", 40, new Color(0.55f, 1f, 0.6f));

        // Stage list
        float rw = 460f * k, rh = 52f * k, gap = 12f * k;
        float rx = (W - rw) / 2f, ry = H * 0.2f;
        for (int i = 0; i < stages.Length; i++)
        {
            Rect r = new Rect(rx, ry + i * (rh + gap), rw, rh);
            bool ok = Playable(stages[i]);
            bool sel = i == selected;
            Color fill = !ok ? new Color(0.1f, 0.1f, 0.12f, 0.85f)
                       : sel ? new Color(0.25f, 0.55f, 0.3f, 0.95f)
                       : Hud.Hover(r) ? new Color(0.19f, 0.4f, 0.23f, 0.95f) : new Color(0.14f, 0.3f, 0.17f, 0.95f);
            Hud.Panel(r, fill);
            Color tc = ok ? Color.white : new Color(1f, 1f, 1f, 0.35f);
            Hud.Text(new Rect(r.x + 18f * k, r.y, r.width, r.height), (i + 1) + "  " + stages[i].name, 18, tc, TextAnchor.MiddleLeft);
            if (!ok) Hud.Text(new Rect(r.x, r.y, r.width - 18f * k, r.height), "soon", 13, tc, TextAnchor.MiddleRight);
            if (Hud.Clicked(r)) selected = i;
        }

        // Skills the alien has by the selected stage (one pick per earlier evolution)
        float y = ry + stages.Length * (rh + gap) + 10f * k;
        for (int t = 0; t < selected && t < UpgradePath.Tiers.Length; t++)
        {
            UpgradePath.Tier tier = UpgradePath.Tiers[t];
            Hud.Text(new Rect(rx, y, rw, 24f * k), tier.key + " skill", 15, new Color(1f, 0.9f, 0.5f), TextAnchor.MiddleLeft);
            y += 30f * k;
            int n = tier.options.Length;
            float ow = (rw - (n - 1) * 10f * k) / n, oh = 44f * k;
            for (int o = 0; o < n; o++)
            {
                Rect r = new Rect(rx + o * (ow + 10f * k), y, ow, oh);
                bool on = picks[t] == o;
                Hud.Panel(r, on ? new Color(0.62f, 0.3f, 0.6f, 0.95f) : Hud.Hover(r) ? new Color(0.3f, 0.22f, 0.32f, 0.95f) : new Color(0.18f, 0.14f, 0.2f, 0.95f));
                Hud.Text(r, tier.options[o].name, 14, on ? Color.white : new Color(1f, 1f, 1f, 0.7f));
                if (Hud.Clicked(r)) picks[t] = o;
            }
            y += oh + 14f * k;
        }

        // Play / Back
        float bw = 220f * k, bh = 52f * k;
        float by = Mathf.Max(y + 10f * k, H * 0.72f);
        bool canPlay = Playable(stages[selected]);
        if (Hud.Button(new Rect(W / 2f - bw - 10f * k, by, bw, bh), canPlay ? "Play" : "Not built yet", 18) && canPlay) PlayStage(selected);
        if (Hud.Button(new Rect(W / 2f + 10f * k, by, bw, bh), "Back", 18)) stageSelect = false;
        Hud.Text(new Rect(0, H - 40f * k, W, 24f * k), "W S or 1-" + Mathf.Min(9, stages.Length) + " choose   Enter play   Esc back", 13, new Color(1f, 1f, 1f, 0.7f));
    }
}

// Score from the last game, shown on the game over screen.
public static class GameResult
{
    public static string Reason = "Too many people escaped!";
    public static string Scene = "Level1";   // the level that was lost
    public static int Level = 1;             // its number, for the scoreboard
    public static int Friends;
    public static int Escaped;
    public static int Needed = 10;
}
