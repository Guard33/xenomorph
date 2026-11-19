using UnityEngine;

// The base alarm (army levels). When a soldier, sentry, watchtower or the General spots the alien,
// the alarm goes off for EVERY guard on the map at once: soldiers leave their routes and hunt the
// alien's last known position, sentries and searchlights turn faster, and staff near the alien run.
// It lasts while anyone can still see the alien, then dies down after a few quiet seconds.
// Added to the level by the Level Builder only on maps with soldiers.
public class BaseAlarm : MonoBehaviour
{
    public float duration = 12f;   // seconds of alarm after the alien was last seen
    public float staffRadius = 10f;
    public AudioSource siren;

    public static BaseAlarm Instance { get; private set; }
    public static bool Active => Instance != null && Instance.timer > 0f;
    public static Vector2 LastKnown { get; private set; }
    public static int TimesRaised { get; private set; }
    public float SecondsLeft => timer;

    private float timer;

    void Awake()
    {
        Instance = this;
        TimesRaised = 0;
    }

    void OnDestroy() { if (Instance == this) Instance = null; }

    // Someone saw the alien: start the alarm (or keep it going)
    public static void Raise(Vector2 alienPos, string message)
    {
        if (Instance == null) return;
        bool wasActive = Active;
        LastKnown = alienPos;
        Instance.timer = Instance.duration;
        if (wasActive) return;
        TimesRaised++;
        Civilian.AlertAround(alienPos, Instance.staffRadius);
        if (Instance.siren != null) Instance.siren.Play();
        if (LevelManager.Instance != null) LevelManager.Instance.Flash(message, 3f);
    }

    // A guard can still see the alien: keep the alarm up and update where it is
    public static void Spotted(Vector2 alienPos)
    {
        if (!Active) return;
        LastKnown = alienPos;
        Instance.timer = Instance.duration;
    }

    void Update()
    {
        if (timer <= 0f) return;
        timer -= Time.deltaTime;
        if (timer <= 0f && LevelManager.Instance != null && !LevelManager.Instance.IsGameOver)
            LevelManager.Instance.Flash("The alarm died down. The guards go back to their posts.", 2.5f);
    }

    // Red pulse on the screen edges and a banner under the stats board
    void OnGUI()
    {
        if (!Active || LevelManager.Instance == null || LevelManager.Instance.IsComplete || LevelManager.Instance.IsGameOver) return;
        Hud.Use(GameSettings.UiPart.Tips);
        float k = Hud.Scale, u = Hud.Unit, W = Screen.width, H = Screen.height;
        float a = 0.25f + 0.2f * Mathf.Sin(Time.unscaledTime * 8f);
        Color red = new Color(0.9f, 0.1f, 0.1f, a);
        float t = 3 * u;
        Hud.Fill(new Rect(0, 0, W, t), red);
        Hud.Fill(new Rect(0, H - t, W, t), red);
        Hud.Fill(new Rect(0, 0, t, H), red);
        Hud.Fill(new Rect(W - t, 0, t, H), red);

        string text = "BASE ALARM " + Mathf.CeilToInt(timer);
        float w = Hud.TextWidth(text, 15) + 28f * k;
        Rect r = new Rect(10f * k, LevelManager.Instance.BoardBottom + 10f * k, w, 30f * k);
        Hud.Panel(r, new Color(0.45f, 0.06f, 0.06f, 0.92f));
        Hud.Text(r, text, 15, Color.white);
    }
}
