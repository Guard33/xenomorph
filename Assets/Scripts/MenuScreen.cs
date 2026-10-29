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

    private GUIStyle titleStyle, subStyle, buttonStyle, smallStyle;

    void Update()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard kb = Keyboard.current;
        if (kb == null) return;
        if (kb.enterKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame || kb.rKey.wasPressedThisFrame) Play();
        if (kb.escapeKey.wasPressedThisFrame) Back();
#else
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.R)) Play();
        if (Input.GetKeyDown(KeyCode.Escape)) Back();
#endif
    }

    void Play() { SceneManager.LoadScene(gameScene); }

    void Back()
    {
        if (kind == Kind.GameOver) SceneManager.LoadScene(startScene);
        else Application.Quit();
    }

    void MakeStyles()
    {
        int h = Screen.height;
        titleStyle = new GUIStyle(GUI.skin.label) { fontSize = Mathf.Max(36, h / 8), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        subStyle = new GUIStyle(GUI.skin.label) { fontSize = Mathf.Max(18, h / 28), alignment = TextAnchor.MiddleCenter, wordWrap = true };
        subStyle.normal.textColor = Color.white;
        buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = Mathf.Max(18, h / 26), fontStyle = FontStyle.Bold };
        smallStyle = new GUIStyle(GUI.skin.label) { fontSize = Mathf.Max(14, h / 40), alignment = TextAnchor.MiddleCenter };
        smallStyle.normal.textColor = new Color(1f, 1f, 1f, 0.85f);
    }

    void OnGUI()
    {
        if (titleStyle == null) MakeStyles();
        float W = Screen.width, H = Screen.height;
        bool start = kind == Kind.Start;

        string title = start ? gameTitle : "GAME OVER";
        Color titleColor = start ? new Color(0.55f, 1f, 0.6f) : new Color(1f, 0.35f, 0.35f);
        Shadow(new Rect(0, H * 0.12f, W, H * 0.2f), title, titleStyle, titleColor);

        string sub = start
            ? "A lonely alien crash-lands on Earth.\nCatch the people before they escape!"
            : "Too many people escaped!\nFriends: " + GameResult.Friends + " / " + GameResult.Needed + "      Escaped: " + GameResult.Escaped;
        GUI.Label(new Rect(W * 0.1f, H * 0.33f, W * 0.8f, H * 0.14f), sub, subStyle);

        float bw = Mathf.Max(220, W * 0.22f), bh = Mathf.Max(48, H * 0.08f);
        float bx = (W - bw) / 2f, by = H * 0.52f;
        if (GUI.Button(new Rect(bx, by, bw, bh), start ? "START" : "TRY AGAIN", buttonStyle)) Play();
        if (GUI.Button(new Rect(bx, by + bh * 1.3f, bw, bh), start ? "QUIT" : "MAIN MENU", buttonStyle)) Back();

        bool blink = Mathf.Repeat(Time.unscaledTime, 1.2f) < 0.8f;
        if (blink)
            GUI.Label(new Rect(0, H * 0.78f, W, 30), start ? "Press ENTER to start" : "Press ENTER to try again", smallStyle);
        if (start)
            GUI.Label(new Rect(0, H * 0.86f, W, 30), "WASD: move     SPACE: befriend     M: mini map", smallStyle);
    }

    static void Shadow(Rect r, string text, GUIStyle style, Color color)
    {
        Color old = style.normal.textColor;
        style.normal.textColor = new Color(0f, 0f, 0f, 0.8f);
        GUI.Label(new Rect(r.x + 4, r.y + 4, r.width, r.height), text, style);
        style.normal.textColor = color;
        GUI.Label(r, text, style);
        style.normal.textColor = old;
    }
}

// Score from the last game, shown on the game over screen.
public static class GameResult
{
    public static int Friends;
    public static int Escaped;
    public static int Needed = 10;
}
