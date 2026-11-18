using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// Level 1 is the tutorial. This shows one instruction at a time at the top of the screen
// and moves on when the player has done it (or after a few seconds for tips). It covers every
// key and every part of the HUD: moving, aiming, the beam, hit pips, exits, lookouts, the meter,
// the mini map (M), the controls page (Tab) and the pause menu (Esc).
// Later levels get a few short hints about what's new.
public class TutorialGuide : MonoBehaviour
{
    enum Step { Move, Aim, Beam, Pips, Exits, Lookouts, Meter, Goal, Map, Help, Pause, Done, Intro, NewSkill, Police, Goal2, Help2, Done2 }

    public int level = 1;

    private Step step = Step.Move;
    private float stepTime;
    private AlienPlayer alien;
    private TownLookout[] lookouts;
    private int newSkill = 1;

    public string CurrentStepName => step.ToString();

    void Start()
    {
        if (level >= 2) step = Step.Intro;
        alien = FindFirstObjectByType<AlienPlayer>();
        newSkill = level >= 3 ? 2 : 1;   // the skill this level's evolution unlocked (J or K)
        lookouts = FindObjectsByType<TownLookout>(FindObjectsSortMode.None);
    }

    void Update()
    {
        if (alien == null || LevelManager.Instance == null) return;
        stepTime += Time.deltaTime;
        LevelManager lm = LevelManager.Instance;

        switch (step)
        {
            case Step.Move:
                if (alien.DistanceMoved > 3f) Next();
                break;
            case Step.Aim:
                if (stepTime > 5f) Next();
                break;
            case Step.Beam:
                if (lm.Friends >= 1) Next();
                break;
            case Step.Pips:
            case Step.Meter:
                if (stepTime > 5f) Next();
                break;
            case Step.Exits:
                if (stepTime > 6f) Next();
                break;
            case Step.Map:
                if (stepTime > 6f || Pressed('M')) Next();
                break;
            case Step.Help:
                if (stepTime > 7f || PauseMenu.IsOpen) Next();
                break;
            case Step.Pause:
                if (stepTime > 6f && !PauseMenu.IsOpen) Next();
                break;
            case Step.Help2:
                if (stepTime > 5f || PauseMenu.IsOpen) Next();
                break;
            case Step.Lookouts:
                if (stepTime > 12f || AnyLookoutBefriended()) Next();
                break;
            case Step.Goal:
                if (stepTime > 5f) Next();
                break;
            case Step.Intro:
                if (stepTime > 4f) Next();
                break;
            case Step.NewSkill:
                if (stepTime > 10f || !alien.skills[newSkill].Ready) Next();
                break;
            case Step.Police:
                if (stepTime > 7f) Next();
                break;
            case Step.Goal2:
                if (stepTime > 5f) Next();
                break;
        }
    }

    void Next()
    {
        step++;
        stepTime = 0f;
    }

    static bool Pressed(char key)
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && key == 'M' && Keyboard.current.mKey.wasPressedThisFrame;
#else
        return key == 'M' && Input.GetKeyDown(KeyCode.M);
#endif
    }

    bool AnyLookoutBefriended()
    {
        foreach (TownLookout l in lookouts)
            if (l == null || !l.GetComponent<Civilian>().CanBeBefriended) return true;
        return false;
    }

    // One short line at a time
    string Text()
    {
        switch (step)
        {
            case Step.Move: return "Move with  W A S D";
            case Step.Aim: return "The green arrow shows where your beam will go";
            case Step.Beam: return "Face someone and press  H  to befriend them";
            case Step.Pips: return "Blue pips over someone = beam hits still needed";
            case Step.Exits: return "Scared people run for the exits. Too many escape and you lose";
            case Step.Lookouts: return "Yellow circle = lookout. Beam them first!";
            case Step.Meter: return "Each friend fills the friendship meter (bottom left)";
            case Step.Goal: return "Befriend " + LevelManager.Instance.friendsNeeded + " people to evolve";
            case Step.Map: return "Press  M  to hide or show the mini map";
            case Step.Help: return "Press  Tab  any time to see all the controls";
            case Step.Pause: return "Press  Esc  to pause, change settings or quit";
            case Step.Help2: return "Tab  shows the controls.  Esc  pauses";
            case Step.Intro: return level >= 3 ? "Fort Maple Command. The General is waiting" : "Fort Maple. The army is on alert tonight";
            case Step.NewSkill: return "New skill: press  " + alien.skills[newSkill].key + "  for " + alien.skills[newSkill].name;
            case Step.Police: return level >= 3 ? "The General shoots darts. Dodge them or hide!" : "Police and soldiers take 3 hits and raise the alarm";
            case Step.Goal2: return "Befriend " + LevelManager.Instance.friendsNeeded + " people to evolve";
        }
        return null;
    }

    void OnGUI()
    {
        if (LevelManager.Instance != null && (LevelManager.Instance.IsGameOver || LevelManager.Instance.Won)) return;
        string text = Text();
        if (text == null) return;

        // Fade each line in
        float a = Mathf.Clamp01(stepTime * 3f);
        float k = Hud.Scale;
        // Speech bubble behind the tip so it reads over any part of the map
        float w = Hud.TextWidth(text, 18) + 48f * k, h = 46f * k;
        Rect bubble = new Rect((Screen.width - w) / 2f, 12f * k, w, h);
        Hud.Bubble(bubble, a);
        PixelFont.Draw(bubble, text, PixelFont.PixelSize(18), new Color(0.17f, 0.12f, 0.17f, a), TextAnchor.MiddleCenter, false);
    }
}
