using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// Keeps score for level 1: befriend 10 of the 20 civilians (10 EXP each).
// If 11 or more escape, the player can no longer win, so the level must restart.
public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    public int totalPeople = 20;
    public int friendsNeeded = 10;
    public int expPerFriend = 10;

    private int friends;
    private int escaped;
    private string bigMessage;
    private string signText;
    private Object signOwner;
    private GUIStyle boxStyle, textStyle, bigStyle;

    public int Exp => friends * expPerFriend;
    public int ExpNeeded => friendsNeeded * expPerFriend;

    void Awake() { Instance = this; }

    public void PersonBefriended()
    {
        friends++;
        if (friends >= friendsNeeded)
            bigMessage = "Friendship meter full!\nFinish the quest to level up and go to Level 2 (the suburbs).";
    }

    public void PersonEscaped()
    {
        escaped++;
        if (friends < friendsNeeded && totalPeople - escaped < friendsNeeded)
            bigMessage = "Too many people escaped!\nPress R to try again.";
    }

    public void SetSign(Object owner, string text)
    {
        if (!string.IsNullOrEmpty(text)) { signText = text; signOwner = owner; }
        else if (signOwner == owner) { signText = null; signOwner = null; }
    }

    void Update()
    {
#if ENABLE_INPUT_SYSTEM
        bool restart = Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame;
#else
        bool restart = Input.GetKeyDown(KeyCode.R);
#endif
        if (restart)
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void OnGUI()
    {
        if (boxStyle == null)
        {
            boxStyle = new GUIStyle(GUI.skin.box);
            textStyle = new GUIStyle(GUI.skin.label) { fontSize = 18, fontStyle = FontStyle.Bold };
            textStyle.normal.textColor = Color.white;
            bigStyle = new GUIStyle(GUI.skin.box) { fontSize = 26, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, wordWrap = true };
            bigStyle.normal.textColor = Color.white;
        }

        // Top-left info
        GUI.Box(new Rect(10, 10, 360, 84), GUIContent.none, boxStyle);
        GUI.Label(new Rect(20, 14, 350, 26), "Friends: " + friends + " / " + friendsNeeded + "     Escaped: " + escaped, textStyle);
        GUI.Label(new Rect(20, 40, 350, 26), "WASD / arrows: move", textStyle);
        GUI.Label(new Rect(20, 64, 350, 26), "SPACE near a person: befriend", textStyle);

        // Friendship (EXP) meter at the bottom, like in our outline
        float w = Mathf.Min(600, Screen.width - 40);
        float x = (Screen.width - w) / 2f;
        float y = Screen.height - 50;
        GUI.Label(new Rect(x, y - 26, w, 24), "Friendship meter  " + Exp + " / " + ExpNeeded + " EXP", textStyle);
        DrawRect(new Rect(x, y, w, 26), new Color(1f, 1f, 1f, 0.85f));
        DrawRect(new Rect(x + 3, y + 3, (w - 6) * Mathf.Clamp01((float)Exp / ExpNeeded), 20), new Color(0.96f, 0.45f, 0.68f));

        if (!string.IsNullOrEmpty(signText))
            GUI.Box(new Rect(Screen.width / 2f - 300, Screen.height - 190, 600, 90), signText, bigStyle);

        if (!string.IsNullOrEmpty(bigMessage))
            GUI.Box(new Rect(Screen.width / 2f - 330, Screen.height / 2f - 70, 660, 140), bigMessage, bigStyle);
    }

    static void DrawRect(Rect r, Color c)
    {
        Color old = GUI.color;
        GUI.color = c;
        GUI.DrawTexture(r, Texture2D.whiteTexture);
        GUI.color = old;
    }
}
