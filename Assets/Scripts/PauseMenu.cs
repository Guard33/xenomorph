using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// In-level menus. Esc opens the pause menu (Resume, Restart, Controls, Settings, Main menu, Quit);
// Tab opens the Controls page straight away. The game is paused while any page is open.
// It adds itself to every level when the scene loads, so no scene changes are needed.
public class PauseMenu : MonoBehaviour
{
    enum Page { None, Main, Controls, Settings }

    static PauseMenu instance;
    public static bool IsOpen => instance != null && instance.page != Page.None;

    private Page page = Page.None;
    private bool fromMain;          // Controls / Settings opened from the pause menu (Back returns there)
    private int selected;
    private AlienPlayer alien;

    static readonly string[] Items = { "Resume", "Restart level", "Controls", "Settings", "Main menu", "Quit game" };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        Attach();
    }

    static void OnSceneLoaded(Scene s, LoadSceneMode m) { Attach(); }

    static void Attach()
    {
        LevelManager lm = FindFirstObjectByType<LevelManager>();
        if (lm != null && lm.GetComponent<PauseMenu>() == null) lm.gameObject.AddComponent<PauseMenu>();
    }

    void Awake() { instance = this; }
    void OnDestroy() { if (instance == this) instance = null; }
    void Start() { alien = FindFirstObjectByType<AlienPlayer>(); }

    // No pausing on the level complete card or after losing
    bool CanPause => LevelManager.Instance == null || (!LevelManager.Instance.IsComplete && !LevelManager.Instance.IsGameOver);

    void Open(Page p, bool viaMain)
    {
        page = p;
        fromMain = viaMain;
        if (p == Page.Main) selected = 0;
        Time.timeScale = 0f;
    }

    void Close()
    {
        page = Page.None;
        Time.timeScale = 1f;
    }

    void Back()
    {
        if (page == Page.Main || !fromMain) Close();
        else page = Page.Main;
    }

    void Update()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard kb = Keyboard.current;
        if (kb == null) return;
        bool esc = kb.escapeKey.wasPressedThisFrame, tab = kb.tabKey.wasPressedThisFrame;
        bool up = kb.wKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame;
        bool down = kb.sKey.wasPressedThisFrame || kb.downArrowKey.wasPressedThisFrame;
        bool enter = kb.enterKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame;
#else
        bool esc = Input.GetKeyDown(KeyCode.Escape), tab = Input.GetKeyDown(KeyCode.Tab);
        bool up = Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow);
        bool down = Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow);
        bool enter = Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space);
#endif
        if (page == Page.None)
        {
            if (!CanPause) return;
            if (esc) Open(Page.Main, false);
            else if (tab) Open(Page.Controls, false);
            return;
        }

        if (tab && page == Page.Controls) { Back(); return; }
        if (tab && page == Page.Main) { Open(Page.Controls, true); return; }
        if (esc) { Back(); return; }
        if (page == Page.Main)
        {
            if (up) selected = (selected + Items.Length - 1) % Items.Length;
            if (down) selected = (selected + 1) % Items.Length;
            if (enter) Choose(selected);
        }
    }

    void Choose(int i)
    {
        switch (i)
        {
            case 0: Close(); break;
            case 1: Close(); SceneManager.LoadScene(SceneManager.GetActiveScene().name); break;
            case 2: page = Page.Controls; fromMain = true; break;
            case 3: page = Page.Settings; fromMain = true; break;
            case 4:
                Close();
                SceneManager.LoadScene(Application.CanStreamedLevelBeLoaded("StartScreen") ? "StartScreen" : SceneManager.GetActiveScene().name);
                break;
            case 5:
                Application.Quit();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
                break;
        }
    }

    void OnGUI()
    {
        if (page == Page.None) return;
        Hud.Use(GameSettings.UiPart.Menus);
        GUI.depth = -100;   // on top of the HUD
        if (page == Page.Controls) { if (MenuPages.DrawControls(alien)) Back(); return; }
        if (page == Page.Settings) { if (MenuPages.DrawSettings()) Back(); return; }

        float W = Screen.width, H = Screen.height, k = Hud.Scale;
        MenuPages.Dim();
        float w = 420f * k, bh = 50f * k, gap = 12f * k;
        float h = 110f * k + Items.Length * (bh + gap) + 30f * k;
        Rect r = new Rect((W - w) / 2f, (H - h) / 2f, w, h);
        Hud.Panel(r, MenuPages.PanelColor);
        Hud.Text(new Rect(r.x, r.y + 20f * k, w, 44f * k), "PAUSED", 32, new Color(0.55f, 1f, 0.6f));
        float y = r.y + 90f * k;
        for (int i = 0; i < Items.Length; i++)
        {
            Rect b = new Rect(r.x + 40f * k, y, w - 80f * k, bh);
            if (Hud.Hover(b)) selected = i;
            if (MenuPages.Button(b, Items[i], i == selected)) Choose(i);
            y += bh + gap;
        }
        Hud.Text(new Rect(r.x, r.yMax - 30f * k, w, 18f * k), "W S choose   Enter select   Esc resume", 11, new Color(1f, 1f, 1f, 0.5f));
    }
}
