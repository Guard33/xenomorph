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

    // Same pixel-art style as the in-game HUD (PixelFont + Hud panels and buttons)
    void OnGUI()
    {
        float W = Screen.width, H = Screen.height, k = Hud.Scale;
        bool start = kind == Kind.Start;

        string title = start ? gameTitle : "GAME OVER";
        Color titleColor = start ? new Color(0.55f, 1f, 0.6f) : new Color(1f, 0.35f, 0.35f);
        Hud.Text(new Rect(0, H * 0.14f, W, H * 0.16f), title, 72, titleColor);

        string sub = start
            ? "A lonely alien crash-lands on Earth.\nBrainwash the people before they escape!"
            : GameResult.Reason + "\nFriends " + GameResult.Friends + "/" + GameResult.Needed + "   Escaped " + GameResult.Escaped;
        Hud.Text(new Rect(0, H * 0.34f, W, H * 0.12f), sub, 16, Color.white);

        float bw = 260f * k, bh = 56f * k;
        float bx = (W - bw) / 2f, by = H * 0.52f;
        if (Hud.Button(new Rect(bx, by, bw, bh), start ? "Start" : "Try again", 20)) Play();
        if (Hud.Button(new Rect(bx, by + bh + 18f * k, bw, bh), start ? "Quit" : "Main menu", 20)) Back();

        bool blink = Mathf.Repeat(Time.unscaledTime, 1.2f) < 0.8f;
        if (blink)
            Hud.Text(new Rect(0, H * 0.79f, W, 24f * k), start ? "Press Enter to start" : "Press Enter to try again", 14, new Color(1f, 1f, 1f, 0.85f));
        if (start)
            Hud.Text(new Rect(0, H * 0.87f, W, 24f * k), "WASD move   H beam   M map   R restart", 13, new Color(1f, 1f, 1f, 0.7f));
    }
}

// Score from the last game, shown on the game over screen.
public static class GameResult
{
    public static string Reason = "Too many people escaped!";
    public static int Friends;
    public static int Escaped;
    public static int Needed = 10;
}
