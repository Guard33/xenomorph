using UnityEngine;

// Level 1 is the tutorial. This shows one instruction at a time at the top of the screen
// and moves on when the player has done it (or after a few seconds for tips).
// Later levels get a few short hints about what's new.
public class TutorialGuide : MonoBehaviour
{
    enum Step { Move, Beam, Exits, Lookouts, Goal, Done, NewSkill, Police, Goal2, Done2 }

    public int level = 1;

    private Step step = Step.Move;
    private float stepTime;
    private AlienPlayer alien;
    private TownLookout[] lookouts;

    public string CurrentStepName => step.ToString();

    void Start()
    {
        if (level >= 2) step = Step.NewSkill;
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
            case Step.NewSkill: return "New skill! Press  J  for your " + alien.skills[1].name;
            case Step.Police: return "Police take 3 hits and blow whistles";
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
        Hud.Text(new Rect(0, 18f * Hud.Scale, Screen.width, 30f * Hud.Scale), text, 20, new Color(1f, 1f, 1f, a), TextAnchor.MiddleCenter, true);
    }
}
