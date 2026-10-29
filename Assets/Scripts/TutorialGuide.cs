using UnityEngine;

// Level 1 is the tutorial. This shows one instruction at a time at the top of the screen
// and moves on when the player has done it (or after a few seconds for tips).
public class TutorialGuide : MonoBehaviour
{
    enum Step { Move, Beam, Exits, Lookouts, Goal, Done }

    private Step step = Step.Move;
    private float stepTime;
    private AlienPlayer alien;
    private TownLookout[] lookouts;
    private GUIStyle panelStyle, headStyle;

    public string CurrentStepName => step.ToString();

    void Start()
    {
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
                if (stepTime > 7f) Next();
                break;
            case Step.Lookouts:
                if (stepTime > 12f || AnyLookoutBefriended()) Next();
                break;
            case Step.Goal:
                if (stepTime > 8f) Next();
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

    string Text()
    {
        switch (step)
        {
            case Step.Move: return "Welcome to Earth!\nMove with  W  A  S  D.";
            case Step.Beam: return "Face a person and press  H  to fire your Small Beam.\nAnyone the beam touches becomes your friend.";
            case Step.Exits: return "Nice! People who notice you run for the red exit markers.\nIf 11 people escape, you have to start over.";
            case Step.Lookouts: return "Lookouts stand inside a yellow circle (Town Crier, Mayor).\nStay in it too long and they ring the alarm - everyone nearby runs. Beam them first!";
            case Step.Goal: return "Befriend 10 people to fill the Friendship meter and evolve.\nJ  K  L  skills unlock as you evolve.";
        }
        return null;
    }

    void OnGUI()
    {
        if (LevelManager.Instance != null && (LevelManager.Instance.IsGameOver || LevelManager.Instance.Won)) return;
        string text = Text();
        if (text == null) return;

        if (panelStyle == null)
        {
            panelStyle = new GUIStyle(GUI.skin.box) { fontSize = 20, alignment = TextAnchor.MiddleCenter, wordWrap = true, padding = new RectOffset(16, 16, 10, 10) };
            panelStyle.normal.textColor = Color.white;
            headStyle = new GUIStyle(GUI.skin.label) { fontSize = 13, fontStyle = FontStyle.Bold, alignment = TextAnchor.UpperCenter };
            headStyle.normal.textColor = new Color(0.55f, 1f, 0.6f);
        }

        float w = Mathf.Min(720, Screen.width - 380);
        Rect r = new Rect((Screen.width - w) / 2f, 12, w, 86);
        GUI.Box(r, text, panelStyle);
        GUI.Label(new Rect(r.x, r.y + 2, r.width, 18), "TUTORIAL  " + ((int)step + 1) + " / 5", headStyle);
    }
}
