using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// Keeps score for level 1: befriend 10 of the 20 people (10 EXP each).
// The level is lost if 11 or more escape (can no longer win) or the alien's HP reaches 0.
// Also draws the HUD: friends/escaped, HP, the H J K L skill bar and the friendship meter.
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
    private GUIStyle boxStyle, textStyle, bigStyle, keyStyle, smallStyle;

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

    void MakeStyles()
    {
        boxStyle = new GUIStyle(GUI.skin.box);
        textStyle = new GUIStyle(GUI.skin.label) { fontSize = 18, fontStyle = FontStyle.Bold };
        textStyle.normal.textColor = Color.white;
        bigStyle = new GUIStyle(GUI.skin.box) { fontSize = 26, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, wordWrap = true };
        bigStyle.normal.textColor = Color.white;
        keyStyle = new GUIStyle(GUI.skin.label) { fontSize = 22, fontStyle = FontStyle.Bold, alignment = TextAnchor.UpperCenter };
        keyStyle.normal.textColor = Color.white;
        smallStyle = new GUIStyle(GUI.skin.label) { fontSize = 12, alignment = TextAnchor.LowerCenter, wordWrap = true };
        smallStyle.normal.textColor = Color.white;
    }

    void OnGUI()
    {
        if (boxStyle == null) MakeStyles();

        // Top-left: score and HP
        GUI.Box(new Rect(10, 10, 300, 66), GUIContent.none, boxStyle);
        GUI.Label(new Rect(20, 14, 290, 26), "Friends: " + friends + " / " + friendsNeeded + "     Escaped: " + escaped, textStyle);
        GUI.Label(new Rect(20, 42, 40, 26), "HP", textStyle);
        if (alien != null)
            for (int i = 0; i < alien.maxHp; i++)
                DrawRect(new Rect(56 + i * 26, 46, 20, 18), i < alien.Hp ? new Color(0.95f, 0.3f, 0.35f) : new Color(1f, 1f, 1f, 0.25f));

        // Bottom-centre: friendship (EXP) meter
        float w = Mathf.Min(520, Screen.width - 340);
        float x = 20f;
        float y = Screen.height - 50;
        GUI.Label(new Rect(x, y - 26, w, 24), "Friendship meter  " + Exp + " / " + ExpNeeded + " EXP", textStyle);
        DrawRect(new Rect(x, y, w, 26), new Color(1f, 1f, 1f, 0.85f));
        DrawRect(new Rect(x + 3, y + 3, (w - 6) * Mathf.Clamp01((float)Exp / ExpNeeded), 20), new Color(0.96f, 0.45f, 0.68f));

        // Bottom-right: skill bar H J K L
        if (alien != null) DrawSkillBar();

        if (flashTimer > 0f && !string.IsNullOrEmpty(flashText))
            GUI.Box(new Rect(Screen.width / 2f - 260, Screen.height * 0.28f, 520, 44), flashText, bigStyle);

        if (!string.IsNullOrEmpty(bigMessage))
            GUI.Box(new Rect(Screen.width / 2f - 330, Screen.height / 2f - 70, 660, 140), bigMessage, bigStyle);
    }

    void DrawSkillBar()
    {
        const float size = 64f, gap = 8f;
        int n = alien.skills.Length;
        float x0 = Screen.width - n * (size + gap) - 12f;
        float y0 = Screen.height - size - 14f;
        for (int i = 0; i < n; i++)
        {
            AlienSkill s = alien.skills[i];
            Rect r = new Rect(x0 + i * (size + gap), y0, size, size);
            DrawRect(r, s.unlocked ? new Color(0.15f, 0.45f, 0.25f, 0.9f) : new Color(0.2f, 0.2f, 0.2f, 0.75f));
            if (s.unlocked && !s.Ready)
                DrawRect(new Rect(r.x, r.y, r.width, r.height * s.Cooldown01), new Color(0f, 0f, 0f, 0.55f));
            GUI.Label(new Rect(r.x, r.y + 4, r.width, 28), s.key, keyStyle);
            GUI.Label(new Rect(r.x + 2, r.y, r.width - 4, r.height - 4), s.unlocked ? s.name : "locked", smallStyle);
        }
    }

    static void DrawRect(Rect r, Color c)
    {
        Color old = GUI.color;
        GUI.color = c;
        GUI.DrawTexture(r, Texture2D.whiteTexture);
        GUI.color = old;
    }
}
