using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// Keeps score for level 1: befriend 10 of the 20 people (10 EXP each).
// The level is lost if 11 or more escape (can no longer win) or the alien's HP reaches 0.
// Once every person is either befriended or gone (and the meter is full), the level is complete.
// Also draws the pixel-art HUD: score, friendship meter, skill keys, popups and the ending screen.
public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    public int totalPeople = 20;
    public int friendsNeeded = 10;
    public int expPerFriend = 10;
    public bool alienCanDie = false;   // Level 1 is the tutorial: HP never drops below 1

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

    public int Friends => friends;
    public int Escaped => escaped;
    public bool Won => won;
    public bool IsComplete => complete;
    public bool IsGameOver => gameOver;
    public int Exp => friends * expPerFriend;
    public int ExpNeeded => friendsNeeded * expPerFriend;

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
        if (friends >= friendsNeeded && !won && !gameOver)
        {
            won = true;
            winPopupOpen = true;
        }
        CheckComplete();
    }

    public void PersonEscaped()
    {
        escaped++;
        if (!won && totalPeople - escaped < friendsNeeded)
            Lose("Too many people escaped!");
        CheckComplete();
    }

    public void AlienDied()
    {
        if (!won) Lose("The alien was defeated!");
    }

    // Nobody left on the map to brainwash: end the level
    void CheckComplete()
    {
        if (complete || gameOver || !won || friends + escaped < totalPeople) return;
        complete = true;
        winPopupOpen = false;
        Invoke(nameof(ShowEnding), 1.2f);   // let the last beam-up finish
    }

    void ShowEnding()
    {
        showEnding = true;
        Time.timeScale = 0f;
    }

    void Lose(string reason)
    {
        if (gameOver) return;
        gameOver = true;
        bigMessage = reason;
        GameResult.Reason = reason;
        GameResult.Friends = friends;
        GameResult.Escaped = escaped;
        GameResult.Needed = friendsNeeded;
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

    void Update()
    {
        if (flashTimer > 0f) flashTimer -= Time.deltaTime;
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
            if (restart || enter) Restart();
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
        float k = Hud.Scale;
        float u = Hud.Unit;
        float W = Screen.width, H = Screen.height;

        // Top-left: friends and escaped
        Hud.Text(new Rect(14f * k, 12f * k, 400f * k, 20f * k), "Friends " + friends + "/" + friendsNeeded, 15, Color.white, TextAnchor.MiddleLeft);
        Hud.Text(new Rect(14f * k, 36f * k, 400f * k, 20f * k), "Escaped " + escaped, 15, escaped > 0 ? new Color(1f, 0.6f, 0.55f) : new Color(1f, 1f, 1f, 0.75f), TextAnchor.MiddleLeft);

        // HP under the score, only when it matters
        if (alien != null && alienCanDie)
            for (int i = 0; i < alien.maxHp; i++)
                Hud.Fill(new Rect(14f * k + i * 5 * u, 62f * k, 4 * u, 4 * u), i < alien.Hp ? new Color(0.95f, 0.3f, 0.35f) : new Color(1f, 1f, 1f, 0.25f));

        // Bottom-left: friendship meter (pixel bar)
        float bw = 70 * u, bh = 4 * u;
        float bx = Mathf.Round(14f * k), by = Mathf.Round(H - 16f * k - bh);
        Hud.Text(new Rect(bx, by - 26f * k, 400f * k, 20f * k), "Friendship " + Exp + "/" + ExpNeeded, 13, Color.white, TextAnchor.MiddleLeft);
        Hud.Panel(new Rect(bx, by, bw, bh), new Color(0.12f, 0.1f, 0.14f, 0.8f));
        float fill = (bw - 2 * u) * Mathf.Clamp01((float)Exp / ExpNeeded);
        Hud.Fill(new Rect(bx + u, by + u, Mathf.Round(fill / u) * u, bh - 2 * u), Pink);

        // Bottom-right: skill keys H J K L
        if (alien != null) DrawSkills(k, u);

        if (flashTimer > 0f && !string.IsNullOrEmpty(flashText))
            Hud.Text(new Rect(0, H * 0.3f, W, 30f * k), flashText, 18, new Color(1f, 0.9f, 0.5f, Mathf.Clamp01(flashTimer * 2f)));

        if (winPopupOpen) DrawWinPopup(k);

        if (!string.IsNullOrEmpty(bigMessage))
        {
            Rect r = new Rect(W / 2f - 280f * k, H / 2f - 40f * k, 560f * k, 80f * k);
            Hud.Panel(r, PanelColor);
            Hud.Text(r, bigMessage, 18, Color.white);
        }

        if (showEnding) DrawEnding(k);
    }

    // Small card on the right when the friendship meter fills. Close with the X or Esc;
    // the player can keep brainwashing the rest of the town afterwards.
    void DrawWinPopup(float k)
    {
        float w = 330f * k, h = 112f * k;
        Rect r = new Rect(Screen.width - w - 14f * k, Screen.height * 0.36f, w, h);
        Hud.Panel(r, PanelColor);

        float pad = 18f * k;
        Hud.Text(new Rect(r.x + pad, r.y + 14f * k, w, 24f * k), "Meter full!", 20, Pink, TextAnchor.MiddleLeft);
        Hud.Text(new Rect(r.x + pad, r.y + 48f * k, w, 18f * k), "You're ready to evolve.", 14, Color.white, TextAnchor.MiddleLeft);
        Hud.Text(new Rect(r.x + pad, r.y + 72f * k, w, 18f * k), "Level 2 coming soon", 14, new Color(1f, 1f, 1f, 0.6f), TextAnchor.MiddleLeft);

        Rect x = new Rect(r.xMax - 34f * k, r.y + 10f * k, 24f * k, 24f * k);
        Hud.Text(x, "X", 15, Hud.Hover(x) ? Color.white : new Color(1f, 1f, 1f, 0.5f));
        if (Hud.Clicked(x)) winPopupOpen = false;
    }

    // Level complete: everyone on the map is brainwashed (or gone)
    void DrawEnding(float k)
    {
        float W = Screen.width, H = Screen.height;
        Hud.Fill(new Rect(0, 0, W, H), new Color(0.02f, 0.03f, 0.05f, 0.7f));

        float w = 520f * k, h = 300f * k;
        Rect r = new Rect((W - w) / 2f, (H - h) / 2f, w, h);
        Hud.Panel(r, PanelColor);

        Hud.Text(new Rect(r.x, r.y + 26f * k, w, 36f * k), "Level complete!", 28, Green);
        Hud.Text(new Rect(r.x, r.y + 84f * k, w, 20f * k), "Quiet Farm Town is brainwashed", 14, Color.white);
        Hud.Text(new Rect(r.x, r.y + 120f * k, w, 20f * k), "Friends " + friends + "   Escaped " + escaped, 15, Pink);

        float bw = 200f * k, bh = 52f * k, gap = 24f * k;
        float by = r.y + 172f * k;
        if (Hud.Button(new Rect(r.x + (w - 2 * bw - gap) / 2f, by, bw, bh), "Play again")) Restart();
        if (Hud.Button(new Rect(r.x + (w + gap) / 2f, by, bw, bh), "Main menu")) MainMenu();
        Hud.Text(new Rect(r.x, r.yMax - 40f * k, w, 16f * k), "Enter = play again   Esc = menu", 11, new Color(1f, 1f, 1f, 0.45f));
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
