using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// Keeps score for level 1: befriend 10 of the 20 people (10 EXP each).
// The level is lost if 11 or more escape (can no longer win) or the alien's HP reaches 0.
// Also draws the HUD: friends bar, escaped count, unlocked skill keys, and HP when the alien can die.
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
    private string bigMessage;
    private string flashText;
    private float flashTimer;
    private AlienPlayer alien;

    public int Friends => friends;
    public int Escaped => escaped;
    public bool Won => won;
    public bool IsGameOver => gameOver;
    public int Exp => friends * expPerFriend;
    public int ExpNeeded => friendsNeeded * expPerFriend;

    void Awake() { Instance = this; }

    void Start() { alien = FindFirstObjectByType<AlienPlayer>(); }

    public void PersonBefriended()
    {
        friends++;
        if (friends >= friendsNeeded && !won && !gameOver)
        {
            won = true;
            bigMessage = "Friendship meter full!\nYou are ready to evolve. Level 2 (the suburbs) is coming soon.";
        }
    }

    public void PersonEscaped()
    {
        escaped++;
        if (!won && totalPeople - escaped < friendsNeeded)
            Lose("Too many people escaped!");
    }

    public void AlienDied()
    {
        if (!won) Lose("The alien was defeated!");
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

    void Update()
    {
        if (flashTimer > 0f) flashTimer -= Time.deltaTime;
#if ENABLE_INPUT_SYSTEM
        bool restart = Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame;
#else
        bool restart = Input.GetKeyDown(KeyCode.R);
#endif
        if (restart)
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Small HUD: score top-left (plus HP when the alien can be hurt), friendship meter
    // bottom-left, H J K L skill bar bottom-right. The mini map draws itself top-right.
    void OnGUI()
    {
        float k = Hud.Scale;
        float W = Screen.width, H = Screen.height;

        // Top-left: friends and escaped
        Hud.Text(new Rect(14f * k, 10f * k, 400f * k, 22f * k), "Friends  " + friends + " / " + friendsNeeded, 15, Color.white, TextAnchor.MiddleLeft, true);
        Hud.Text(new Rect(14f * k, 30f * k, 400f * k, 22f * k), "Escaped  " + escaped, 15, escaped > 0 ? new Color(1f, 0.6f, 0.55f) : new Color(1f, 1f, 1f, 0.8f), TextAnchor.MiddleLeft, true);

        // HP under the score, only when it matters
        if (alien != null && alienCanDie)
            for (int i = 0; i < alien.maxHp; i++)
                Hud.Fill(new Rect(14f * k + i * 16f * k, 56f * k, 12f * k, 12f * k), i < alien.Hp ? new Color(0.95f, 0.3f, 0.35f) : new Color(1f, 1f, 1f, 0.25f));

        // Bottom-left: friendship meter
        float bw = 220f * k, bh = 8f * k;
        float bx = 14f * k, by = H - 22f * k;
        Hud.Text(new Rect(bx, by - 22f * k, bw + 100f * k, 20f * k), "Friendship  " + Exp + " / " + ExpNeeded, 13, Color.white, TextAnchor.MiddleLeft);
        Hud.Fill(new Rect(bx, by, bw, bh), new Color(0f, 0f, 0f, 0.45f));
        Hud.Fill(new Rect(bx, by, bw * Mathf.Clamp01((float)Exp / ExpNeeded), bh), new Color(0.96f, 0.45f, 0.68f));

        // Bottom-right: skill bar H J K L
        if (alien != null) DrawSkills(k);

        if (flashTimer > 0f && !string.IsNullOrEmpty(flashText))
            Hud.Text(new Rect(0, H * 0.3f, W, 30f * k), flashText, 18, new Color(1f, 0.9f, 0.5f, Mathf.Clamp01(flashTimer * 2f)), TextAnchor.MiddleCenter, true);

        if (!string.IsNullOrEmpty(bigMessage))
        {
            Hud.Fill(new Rect(0, H / 2f - 50f * k, W, 100f * k), new Color(0f, 0f, 0f, 0.55f));
            Hud.Text(new Rect(0, H / 2f - 50f * k, W, 100f * k), bigMessage, 22, Color.white, TextAnchor.MiddleCenter, true);
        }
    }

    void DrawSkills(float k)
    {
        float size = 34f * k, gap = 6f * k;
        float x = Screen.width - 14f * k - size;
        for (int i = alien.skills.Length - 1; i >= 0; i--)
        {
            AlienSkill s = alien.skills[i];
            Rect r = new Rect(x, Screen.height - 14f * k - size, size, size);
            Hud.Fill(r, new Color(0f, 0f, 0f, s.unlocked ? 0.45f : 0.25f));
            if (s.unlocked && !s.Ready)
                Hud.Fill(new Rect(r.x, r.y, r.width, r.height * s.Cooldown01), new Color(0f, 0f, 0f, 0.5f));
            Color c = !s.unlocked ? new Color(1f, 1f, 1f, 0.25f) : s.Ready ? new Color(0.55f, 1f, 0.6f) : new Color(1f, 1f, 1f, 0.4f);
            Hud.Text(r, s.key, 18, c, TextAnchor.MiddleCenter, true);
            x -= size + gap;
        }
    }
}
