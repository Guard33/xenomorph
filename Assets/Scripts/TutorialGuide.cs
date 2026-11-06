using UnityEngine;

// Level 1 is the tutorial. This shows one instruction at a time at the top of the screen
// and moves on when the player has done it (or after a few seconds for tips).
// Later levels get a few short hints about what's new.
public class TutorialGuide : MonoBehaviour
{
    enum Step { Move, Beam, Exits, Lookouts, Goal, Done, Intro, NewSkill, Police, Goal2, Done2 }

    public int level = 1;

    private Step step = Step.Move;
    private float stepTime;
    private AlienPlayer alien;
    private TownLookout[] lookouts;

    public string CurrentStepName => step.ToString();

    void Start()
    {
        if (level >= 2) step = Step.Intro;
        alien = FindFirstObjectByType<AlienPlayer>();
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
            case Step.Beam:
                if (lm.Friends >= 1) Next();
                break;
            case Step.Exits:
                if (stepTime > 5f) Next();
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
                if (stepTime > 10f || !alien.skills[1].Ready) Next();
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
            case Step.Beam: return "Face someone and press  H  to befriend them";
            case Step.Exits: return "Scared people run for the exits";
            case Step.Lookouts: return "Yellow circle = lookout. Beam them first!";
            case Step.Goal: return "Befriend 10 people to evolve";
            case Step.Intro: return "Fort Maple. The army is on alert tonight";
            case Step.NewSkill: return "New skill: press  J  for " + alien.skills[1].name;
            case Step.Police: return "Police and soldiers take 3 hits and raise the alarm";
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
