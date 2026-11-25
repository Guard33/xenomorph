using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// Keeps score for level 1: befriend 10 of the 20 people (10 EXP each).
// The level is lost if 11 or more escape (can no longer win) or the alien's HP reaches 0.
// Once every person is either befriended or gone (and the meter is full), the level is complete.
// Also draws the pixel-art HUD: score, friendship meter, skill keys, popups and the ending screen.
// When a level ends, the run is saved on the Scoreboard (how many people were befriended).
// Combo and star rating (Danesh), from Jory's plan in Docs/Progression.md:
// befriend people quickly one after another for a combo bonus, and a cleared level gets
// 1 to 3 stars depending on how many people escaped.
public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    public int totalPeople = 20;
    public int friendsNeeded = 10;
    public int expPerFriend = 10;
    public bool alienCanDie = false;   // Level 1 is the tutorial: HP never drops below 1

    public float comboWindow = 3f;     // seconds to befriend the next person and keep a combo going
    public int comboBonus = 5;         // bonus per extra person in a combo: x2 = +5, x3 = +10, x4 = +15 ...

    private int friends;
    private int escaped;
    private bool gameOver;
    private bool won;
    private bool complete;
    private bool showEnding;
    private bool winPopupOpen;
    private string bigMessage;
    private string flashText;
    private float flashTimer;
    private AlienPlayer alien;

    private int combo;                 // people befriended in a row, each within comboWindow of the last
    private int bestCombo;
    private int bonus;                 // total combo bonus this run
    private int lastBonus;             // bonus from the latest friend, for the popup
    private float comboTimer;          // seconds left to keep the combo going
    private float comboPop;            // the combo text pops bigger for a moment
    private float endingTime;          // when the level complete card opened (stars appear one by one)

    public int Friends => friends;
    public int Escaped => escaped;
    public bool Won => won;
    public bool IsComplete => complete;
    public bool IsGameOver => gameOver;
    public int Exp => friends * expPerFriend;
    public int ExpNeeded => friendsNeeded * expPerFriend;
    public int Combo => combo;
    public int BestCombo => bestCombo;
    public int Bonus => bonus;
    public int Score => Exp + bonus;

    // Escapes a player can afford before the level is lost
    public int EscapesAllowed => totalPeople - friendsNeeded;

    // Star rating for a cleared level (0 if the meter is not full):
    // 1 star = cleared, 2 stars = half the allowed escapes or fewer, 3 stars = nobody escaped
    public int Stars
    {
        get
        {
            if (!won) return 0;
            if (escaped == 0) return 3;
            if (escaped <= EscapesAllowed / 2) return 2;
            return 1;
        }
    }

    static readonly Color Pink = new Color(0.96f, 0.45f, 0.68f);
    static readonly Color Green = new Color(0.55f, 1f, 0.6f);
    static readonly Color PanelColor = new Color(0.1f, 0.16f, 0.11f, 0.94f);

    void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
    }

    void Start() { alien = FindFirstObjectByType<AlienPlayer>(); }

    public void PersonBefriended()
    {
        friends++;
        AddToCombo();
        if (friends >= friendsNeeded && !won && !gameOver)
        {
            won = true;
            winPopupOpen = true;
        }
        CheckComplete();
    }

    // Combo: a friend made within comboWindow seconds of the last one adds to the combo.
    // Every extra person in a combo gives a bonus (x2 = +5, x3 = +10 ...). Wide Wave and
    // Friend Trap can start a big combo in one go.
    void AddToCombo()
    {
        combo = comboTimer > 0f ? combo + 1 : 1;
        comboTimer = comboWindow;
        if (combo > bestCombo) bestCombo = combo;
        if (combo >= 2)
        {
            lastBonus = comboBonus * (combo - 1);
            bonus += lastBonus;
            comboPop = 0.25f;
        }
    }

    public void PersonEscaped()
    {
        escaped++;
        if (!won && totalPeople - escaped < friendsNeeded)
            Lose("Too many people escaped!");
        CheckComplete();
    }

    public void AlienDied(string reason = null)
    {
        if (!won) { Lose(reason ?? "The alien was defeated!"); return; }

        // The meter was already full: the level still counts, so end it here instead of
        // leaving a knocked-out alien on the map with nothing to do
        if (complete || gameOver) return;
        complete = true;
        winPopupOpen = false;
        Scoreboard.Record(LevelNumber, friends, totalPeople, true, Stars, bestCombo);
        Flash("The alien was knocked out, but the meter was full!", 2f);
        Invoke(nameof(ShowEnding), 1.5f);
    }

    // Nobody left on the map to brainwash: end the level
    void CheckComplete()
    {
        if (complete || gameOver || !won || friends + escaped < totalPeople) return;
        complete = true;
        winPopupOpen = false;
        Scoreboard.Record(LevelNumber, friends, totalPeople, true, Stars, bestCombo);   // save the run on the scoreboard
        Invoke(nameof(ShowEnding), 1.2f);   // let the last beam-up finish
    }

    void ShowEnding()
    {
        showEnding = true;
        endingTime = Time.unscaledTime;   // the game is paused, so the stars use unscaled time
        Time.timeScale = 0f;
    }

    void Lose(string reason)
    {
        if (gameOver) return;
        gameOver = true;
        bigMessage = reason;
        Scoreboard.Record(LevelNumber, friends, totalPeople, false, 0, bestCombo);  // lost runs count on the scoreboard too
        GameResult.Level = LevelNumber;
        GameResult.Reason = reason;
        GameResult.Friends = friends;
        GameResult.Escaped = escaped;
        GameResult.Needed = friendsNeeded;
        GameResult.Scene = SceneManager.GetActiveScene().name;
        Invoke(nameof(ShowGameOver), 1.5f);
    }

    // Go to the game over screen (if it is in the build), otherwise stay and wait for R
    void ShowGameOver()
    {
        if (Application.CanStreamedLevelBeLoaded("GameOver")) SceneManager.LoadScene("GameOver");
        else bigMessage = GameResult.Reason + "\nPress R to try again.";
    }

    // A short message in the middle of the screen
    public void Flash(string text, float seconds = 2.5f)
    {
        flashText = text;
        flashTimer = seconds;
    }

    void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void MainMenu()
    {
        Time.timeScale = 1f;
        if (Application.CanStreamedLevelBeLoaded("StartScreen")) SceneManager.LoadScene("StartScreen");
        else Restart();
    }

    int LevelNumber => LevelInfo.Instance != null ? LevelInfo.Instance.level : 1;

    // After this level the alien evolves (Evolve screen), then the next level is played.
    // Only offered when the next level exists.
    bool HasNextLevel
    {
        get
        {
            int tier = LevelNumber - 1;
            return tier < UpgradePath.Tiers.Length && UpgradePath.Tiers[tier].scene != null;
        }
    }

    void NextLevel()
    {
        Time.timeScale = 1f;
        int tier = LevelNumber - 1;
        // Not evolved for this level yet: pick the new skill first. Already picked (replay): go straight on.
        string scene = UpgradePath.Chosen[tier] < 0 ? "UpgradeScreen" : UpgradePath.Tiers[tier].scene;
        if (Application.CanStreamedLevelBeLoaded(scene)) SceneManager.LoadScene(scene);
        else MainMenu();
    }

    void Update()
    {
        if (flashTimer > 0f) flashTimer -= Time.deltaTime;
        if (comboPop > 0f) comboPop -= Time.deltaTime;
        if (comboTimer > 0f)
        {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0f) combo = 0;   // too slow: the combo is over
        }
        if (PauseMenu.IsOpen) return;   // the pause menu handles keys while it is open
#if ENABLE_INPUT_SYSTEM
        Keyboard kb = Keyboard.current;
        bool restart = kb != null && kb.rKey.wasPressedThisFrame;
        bool esc = kb != null && kb.escapeKey.wasPressedThisFrame;
        bool enter = kb != null && kb.enterKey.wasPressedThisFrame;
#else
        bool restart = Input.GetKeyDown(KeyCode.R);
        bool esc = Input.GetKeyDown(KeyCode.Escape);
        bool enter = Input.GetKeyDown(KeyCode.Return);
#endif
        if (showEnding)
        {
            if (enter) { if (HasNextLevel) NextLevel(); else Restart(); }
            else if (restart) Restart();
            else if (esc) MainMenu();
            return;
        }
        if (esc) winPopupOpen = false;
        if (restart) Restart();
    }

    // HUD: score top-left (plus HP when the alien can be hurt), friendship meter
    // bottom-left, H J K L keys bottom-right. The mini map draws itself top-right.
    void OnGUI()
    {
        Hud.Use(GameSettings.UiPart.Board);
        float k = Hud.Scale;
        float u = Hud.Unit;
        float W = Screen.width, H = Screen.height;

        // Top-left: friends and escaped
        // Before the meter is full the goal is 10; after that it's the whole town
        int goal = won ? totalPeople : friendsNeeded;
        string friendsLine = "Friends " + friends + "/" + goal, escapedLine = "Escaped " + escaped;
        bool showHp = alien != null && alienCanDie;
        // Wooden board behind the stats so they stand out from the map
        float tx = 24f * k;
        float boardW = Mathf.Max(Hud.TextWidth(friendsLine, 15), Hud.TextWidth(escapedLine, 15)) + 32f * k;
        if (showHp) boardW = Mathf.Max(boardW, alien.maxHp * 5 * u + 22f * k);
        Hud.Board(new Rect(10f * k, 8f * k, boardW, (showHp ? 86f : 68f) * k));
        BoardBottom = (8f + (showHp ? 86f : 68f)) * k;
        Hud.Text(new Rect(tx, 16f * k, 400f * k, 20f * k), friendsLine, 15, Color.white, TextAnchor.MiddleLeft);
        Hud.Text(new Rect(tx, 40f * k, 400f * k, 20f * k), escapedLine, 15, escaped > 0 ? new Color(1f, 0.62f, 0.55f) : new Color(1f, 0.93f, 0.8f), TextAnchor.MiddleLeft);

        // HP under the score, only when it matters
        if (showHp)
            for (int i = 0; i < alien.maxHp; i++)
                Hud.Fill(new Rect(tx + i * 5 * u, 64f * k, 4 * u, 4 * u), i < alien.Hp ? new Color(0.95f, 0.3f, 0.35f) : new Color(0f, 0f, 0f, 0.35f));

        // Bottom-left: friendship meter (pixel bar)
        Hud.Use(GameSettings.UiPart.Meter);
        k = Hud.Scale; u = Hud.Unit;
        float bw = 70 * u, bh = 4 * u;
        float bx = Mathf.Round(14f * k), by = Mathf.Round(H - 16f * k - bh);
        Hud.Text(new Rect(bx, by - 26f * k, 400f * k, 20f * k), "Friendship " + Mathf.Min(Exp, ExpNeeded) + "/" + ExpNeeded, 13, Color.white, TextAnchor.MiddleLeft);
        Hud.Panel(new Rect(bx, by, bw, bh), new Color(0.12f, 0.1f, 0.14f, 0.8f));
        float fill = (bw - 2 * u) * Mathf.Clamp01((float)Exp / ExpNeeded);
        Hud.Fill(new Rect(bx + u, by + u, Mathf.Round(fill / u) * u, bh - 2 * u), Pink);

        // Bottom-right: skill keys H J K L
        Hud.Use(GameSettings.UiPart.Skills);
        if (alien != null) DrawSkills(Hud.Scale, Hud.Unit);

        Hud.Use(GameSettings.UiPart.Tips);
        k = Hud.Scale; u = Hud.Unit;
        if (flashTimer > 0f && !string.IsNullOrEmpty(flashText))
            Hud.Text(new Rect(0, H * 0.3f, W, 30f * k), flashText, 18, new Color(1f, 0.9f, 0.5f, Mathf.Clamp01(flashTimer * 2f)));

        if (combo >= 2 && comboTimer > 0f && !showEnding) DrawCombo(k, u);

        if (winPopupOpen) DrawWinPopup(k);

        if (!string.IsNullOrEmpty(bigMessage))
        {
            Rect r = new Rect(W / 2f - 280f * k, H / 2f - 40f * k, 560f * k, 80f * k);
            Hud.Panel(r, PanelColor);
            Hud.Text(r, bigMessage, 18, Color.white);
        }

        Hud.Use(GameSettings.UiPart.Menus);
        if (showEnding) DrawEnding(Hud.Scale);
    }

    // Bottom of the friends board in screen pixels (the alarm banner goes under it)
    public float BoardBottom { get; private set; } = 80f;

    // Combo popup above the alien: "x3 COMBO!", the bonus, and a bar for the time left to keep it going
    void DrawCombo(float k, float u)
    {
        float W = Screen.width, H = Screen.height;
        float a = Mathf.Clamp01(comboTimer * 3f);   // fades out in the last third of a second
        Color gold = new Color(1f, 0.85f, 0.35f, a);
        float y = H * 0.36f;
        Hud.Text(new Rect(0, y, W, 34f * k), "x" + combo + " COMBO!", comboPop > 0f ? 26 : 22, gold);
        Hud.Text(new Rect(0, y + 34f * k, W, 20f * k), "+" + lastBonus + " bonus", 13, new Color(1f, 1f, 1f, 0.9f * a));

        float bw = 30 * u, bh = 2 * u;
        float bx = Mathf.Round((W - bw) / 2f), by = Mathf.Round(y + 60f * k);
        Hud.Fill(new Rect(bx, by, bw, bh), new Color(0f, 0f, 0f, 0.45f * a));
        Hud.Fill(new Rect(bx, by, Mathf.Round(bw * Mathf.Clamp01(comboTimer / comboWindow) / u) * u, bh), gold);
    }

    // Small card on the right when the friendship meter fills. Close with the X or Esc;
    // the player can keep brainwashing the rest of the town afterwards.
    void DrawWinPopup(float k)
    {
        const string title = "Meter full!", line1 = "Ready to evolve", line2 = "Get everyone to finish";
        float pad = 18f * k, closeW = 34f * k;
        // Size the card to its text so nothing spills out
        float textW = Mathf.Max(Hud.TextWidth(title, 20) + closeW, Mathf.Max(Hud.TextWidth(line1, 14), Hud.TextWidth(line2, 14)));
        float w = textW + 2f * pad, h = 112f * k;
        Rect r = new Rect(Screen.width - w - 14f * k, Screen.height * 0.36f, w, h);
        Hud.Panel(r, PanelColor);

        Hud.Text(new Rect(r.x + pad, r.y + 14f * k, w, 24f * k), title, 20, Pink, TextAnchor.MiddleLeft);
        Hud.Text(new Rect(r.x + pad, r.y + 48f * k, w, 18f * k), line1, 14, Color.white, TextAnchor.MiddleLeft);
        Hud.Text(new Rect(r.x + pad, r.y + 72f * k, w, 18f * k), line2, 14, new Color(1f, 1f, 1f, 0.6f), TextAnchor.MiddleLeft);

        Rect x = new Rect(r.xMax - 34f * k, r.y + 10f * k, 24f * k, 24f * k);
        Hud.Text(x, "X", 15, Hud.Hover(x) ? Color.white : new Color(1f, 1f, 1f, 0.5f));
        if (Hud.Clicked(x)) winPopupOpen = false;
    }

    // Level complete: everyone on the map is brainwashed (or gone)
    void DrawEnding(float k)
    {
        float W = Screen.width, H = Screen.height;
        Hud.Fill(new Rect(0, 0, W, H), new Color(0.02f, 0.03f, 0.05f, 0.7f));

        float w = 520f * k, h = 420f * k;
        Rect r = new Rect((W - w) / 2f, (H - h) / 2f, w, h);
        Hud.Panel(r, PanelColor);

        Hud.Text(new Rect(r.x, r.y + 22f * k, w, 36f * k), "Level complete!", 28, Green);
        DrawStars(r, k);
        Hud.Text(new Rect(r.x, r.y + 150f * k, w, 20f * k), LevelMap.Current.Name + " is brainwashed", 14, Color.white);
        Hud.Text(new Rect(r.x, r.y + 178f * k, w, 20f * k), "Friends " + friends + "   Escaped " + escaped, 15, Pink);
        string comboLine = (bestCombo >= 2 ? "Best combo x" + bestCombo + "   " : "") + "Score " + Score;
        Hud.Text(new Rect(r.x, r.y + 206f * k, w, 20f * k), comboLine, 14, new Color(0.75f, 0.9f, 1f));
        DrawBest(r, k);

        float bh = 52f * k, gap = 18f * k;
        float by = r.y + 282f * k;
        if (HasNextLevel)
        {
            // Evolve (or Next level on a replay) / Play again / Menu
            float bw = 150f * k;
            float bx = r.x + (w - 3 * bw - 2 * gap) / 2f;
            bool evolved = UpgradePath.Chosen[LevelNumber - 1] >= 0;
            if (Hud.Button(new Rect(bx, by, bw, bh), evolved ? "Next level" : "Evolve")) NextLevel();
            if (Hud.Button(new Rect(bx + bw + gap, by, bw, bh), "Play again", 14)) Restart();
            if (Hud.Button(new Rect(bx + 2 * (bw + gap), by, bw, bh), "Menu")) MainMenu();
            Hud.Text(new Rect(r.x, r.yMax - 40f * k, w, 16f * k), "Enter = " + (evolved ? "next level" : "evolve") + "   R = play again   Esc = menu", 11, new Color(1f, 1f, 1f, 0.45f));
        }
        else
        {
            float bw = 200f * k;
            if (Hud.Button(new Rect(r.x + (w - 2 * bw - gap) / 2f, by, bw, bh), "Play again")) Restart();
            if (Hud.Button(new Rect(r.x + (w + gap) / 2f, by, bw, bh), "Main menu")) MainMenu();
            Hud.Text(new Rect(r.x, r.yMax - 40f * k, w, 16f * k), "Enter = play again   Esc = menu", 11, new Color(1f, 1f, 1f, 0.45f));
        }
    }

    // Three big stars under the title. Earned stars pop in one by one; the rest stay grey.
    // A line under them says what the next star needs.
    void DrawStars(Rect r, float k)
    {
        int stars = Stars;
        float u = Hud.Unit, big = 2f * u;
        float cy = r.y + 88f * k, step = 22f * u;
        float t = Time.unscaledTime - endingTime;
        for (int i = 0; i < 3; i++)
        {
            Vector2 c = new Vector2(r.center.x + (i - 1) * step, cy);
            float shown = 0.25f + 0.3f * i;   // seconds after the card opens
            if (i < stars && t >= shown)
                ScoreboardScreen.Star(c, t < shown + 0.12f ? 3f * u : big, new Color(1f, 0.85f, 0.35f));
            else
                ScoreboardScreen.Star(c, big, new Color(0.3f, 0.3f, 0.36f, 0.9f));
        }

        int half = EscapesAllowed / 2;
        string hint = stars >= 3 ? "Perfect! Nobody escaped"
                    : stars == 2 || half <= 0 ? "Let nobody escape for 3 stars"
                    : "Let " + half + " or fewer escape for 2 stars";
        Hud.Text(new Rect(r.x, r.y + 120f * k, r.width, 16f * k), hint, 11, new Color(1f, 1f, 1f, 0.6f));
    }

    // Scoreboard line on the level complete card: a new best score, or the best so far
    void DrawBest(Rect r, float k)
    {
        Color gold = new Color(1f, 0.85f, 0.35f);
        Rect line = new Rect(r.x, r.y + 234f * k, r.width, 22f * k);
        if (Scoreboard.LastWasNewBest || Scoreboard.LastWasNewStars)
        {
            string text = Scoreboard.LastWasNewBest ? "New best score!" : "New star record!";
            Hud.Text(line, text, 15, gold);
            // A gold star on each side
            float half = Hud.TextWidth(text, 15) / 2f + 24f * k;
            ScoreboardScreen.Star(new Vector2(line.center.x - half, line.center.y), Hud.Unit, gold);
            ScoreboardScreen.Star(new Vector2(line.center.x + half, line.center.y), Hud.Unit, gold);
        }
        else
        {
            string text = "Best " + Scoreboard.Best(LevelNumber) + "/" + totalPeople;
            if (Scoreboard.LastRank > 0) text += "   Your " + Scoreboard.Ordinal(Scoreboard.LastRank) + " best run";
            Hud.Text(line, text, 14, gold);
        }
    }

    void DrawSkills(float k, float u)
    {
        float size = 12 * u, gap = 2 * u;
        float x = Mathf.Round(Screen.width - 14f * k - size);
        float y = Mathf.Round(Screen.height - 14f * k - size);
        for (int i = alien.skills.Length - 1; i >= 0; i--)
        {
            AlienSkill s = alien.skills[i];
            Rect r = new Rect(x, y, size, size);
            Hud.Panel(r, s.unlocked ? new Color(0.14f, 0.3f, 0.17f, 0.9f) : new Color(0.15f, 0.15f, 0.17f, 0.6f));
            if (s.unlocked && !s.Ready)
                Hud.Fill(new Rect(r.x + u, r.y + u, r.width - 2 * u, Mathf.Round((r.height - 2 * u) * s.Cooldown01 / u) * u), new Color(0f, 0f, 0f, 0.55f));
            Color c = !s.unlocked ? new Color(1f, 1f, 1f, 0.25f) : s.Ready ? Green : new Color(1f, 1f, 1f, 0.4f);
            Hud.Text(r, s.key, 18, c);
            x -= size + gap;
        }
    }
}
