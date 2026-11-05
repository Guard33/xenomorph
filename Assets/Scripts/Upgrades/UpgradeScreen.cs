using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// "Evolve!" screen: shows the whole upgrade path as a tree and lets the player
// pick one of two new skills for the next evolution.
// Click a card (or press 1 / 2), then Evolve (or Enter). Esc goes back to the menu.
// Uses the game's pixel-art UI helpers (Hud / PixelFont).
public class UpgradeScreen : MonoBehaviour
{
    public string menuScene = "StartScreen";
    public string nextScene = "Level1";      // Level 2 is not built yet, so we go back to Level 1

    static readonly Color Pink = new Color(0.96f, 0.45f, 0.68f);
    static readonly Color Blue = new Color(0.45f, 0.7f, 1f);
    static readonly Color Green = new Color(0.55f, 1f, 0.6f);
    static readonly Color PanelColor = new Color(0.1f, 0.16f, 0.11f, 0.94f);
    static readonly Color Dim = new Color(0.12f, 0.12f, 0.15f, 0.85f);

    private int selected = -1;          // 0 = Brain, 1 = Sneak, for the current evolution
    private float evolvedFlash;         // short "evolved!" message after a pick
    private string evolvedName;

    void Update()
    {
        if (evolvedFlash > 0f) evolvedFlash -= Time.unscaledDeltaTime;
#if ENABLE_INPUT_SYSTEM
        Keyboard kb = Keyboard.current;
        if (kb == null) return;
        if (kb.digit1Key.wasPressedThisFrame || kb.numpad1Key.wasPressedThisFrame) Select(0);
        if (kb.digit2Key.wasPressedThisFrame || kb.numpad2Key.wasPressedThisFrame) Select(1);
        if (kb.enterKey.wasPressedThisFrame) Confirm();
        if (kb.escapeKey.wasPressedThisFrame) LoadIfExists(menuScene);
#else
        if (Input.GetKeyDown(KeyCode.Alpha1)) Select(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) Select(1);
        if (Input.GetKeyDown(KeyCode.Return)) Confirm();
        if (Input.GetKeyDown(KeyCode.Escape)) LoadIfExists(menuScene);
#endif
    }

    void Select(int choice)
    {
        if (UpgradePath.NextTier < UpgradePath.Tiers.Length) selected = choice;
    }

    void Confirm()
    {
        int t = UpgradePath.NextTier;
        if (t >= UpgradePath.Tiers.Length) { LoadIfExists(nextScene); return; }
        if (selected < 0) return;
        UpgradePath.Chosen[t] = selected;
        evolvedName = UpgradePath.Picked(t).name;
        evolvedFlash = 2f;
        selected = -1;
    }

    static void LoadIfExists(string scene)
    {
        if (Application.CanStreamedLevelBeLoaded(scene)) SceneManager.LoadScene(scene);
    }

    static Texture2D Icon(string name)
    {
        return Resources.Load<Texture2D>("Upgrades/" + name);
    }

    // ------------------------------------------------------------------ drawing
    void OnGUI()
    {
        float k = Hud.Scale;
        float W = Screen.width, H = Screen.height;
        int next = UpgradePath.NextTier;
        bool done = next >= UpgradePath.Tiers.Length;

        Hud.Fill(new Rect(0, 0, W, H), new Color(0.02f, 0.03f, 0.06f, 0.55f));

        // Title
        string title = done ? "Fully evolved!" : "Evolve!";
        Hud.Text(new Rect(0, 40f * k, W, 50f * k), title, 40, Green);
        string sub = done
            ? "Your alien's upgrade path is complete"
            : "Stage " + UpgradePath.Tiers[next].stage + " of 4: pick one new skill for " + UpgradePath.Tiers[next].key;
        Hud.Text(new Rect(0, 102f * k, W, 22f * k), sub, 15, Color.white);

        // Tree layout: start node, then one column per evolution
        float cardW = 300f * k, cardH = 128f * k, colGap = 70f * k, rowGap = 34f * k;
        float startW = 230f * k;
        float totalW = startW + UpgradePath.Tiers.Length * (cardW + colGap);
        float x0 = (W - totalW) / 2f;
        float midY = H * 0.5f + 10f * k;
        float topY = midY - cardH - rowGap / 2f;
        float botY = midY + rowGap / 2f;

        Rect startCard = new Rect(x0, midY - cardH / 2f, startW, cardH);
        Vector2 from = new Vector2(startCard.xMax, midY);

        float x = startCard.xMax + colGap;
        for (int t = 0; t < UpgradePath.Tiers.Length; t++)
        {
            UpgradePath.Tier tier = UpgradePath.Tiers[t];
            Rect a = new Rect(x, topY, cardW, cardH);
            Rect b = new Rect(x, botY, cardW, cardH);

            int picked = UpgradePath.Chosen[t];
            bool reachable = t <= next;
            Color lineColor = reachable ? new Color(1f, 1f, 1f, 0.55f) : new Color(1f, 1f, 1f, 0.15f);
            Connect(from, new Vector2(a.x, a.center.y), lineColor, picked == 0 || (t == next && selected == 0) ? Pink : lineColor);
            Connect(from, new Vector2(b.x, b.center.y), lineColor, picked == 1 || (t == next && selected == 1) ? Blue : lineColor);

            // Column label
            Hud.Text(new Rect(x, topY - 52f * k, cardW, 18f * k), "Evolution " + (t + 1) + "  -  key " + tier.key, 12, new Color(1f, 1f, 1f, reachable ? 0.85f : 0.35f));
            Hud.Text(new Rect(x, topY - 30f * k, cardW, 16f * k), tier.nextLevel, 12, new Color(1f, 1f, 1f, reachable ? 0.6f : 0.25f));

            DrawCard(a, tier.a, tier.key, t, 0, next, picked, "1");
            DrawCard(b, tier.b, tier.key, t, 1, next, picked, "2");

            if (picked >= 0) from = new Vector2((picked == 0 ? a : b).xMax, (picked == 0 ? a : b).center.y);
            else from = new Vector2(x + cardW, midY);
            x += cardW + colGap;
        }

        DrawStart(startCard);

        // Path legend
        float ly = botY + cardH + 34f * k;
        Hud.Fill(new Rect(W / 2f - 250f * k, ly + 4f * k, 14f * k, 14f * k), Pink);
        Hud.Text(new Rect(W / 2f - 228f * k, ly, 220f * k, 22f * k), "Brain path: catch more", 12, Color.white, TextAnchor.MiddleLeft);
        Hud.Fill(new Rect(W / 2f + 30f * k, ly + 4f * k, 14f * k, 14f * k), Blue);
        Hud.Text(new Rect(W / 2f + 52f * k, ly, 240f * k, 22f * k), "Sneak path: stay hidden", 12, Color.white, TextAnchor.MiddleLeft);

        // Bottom bar: threat info + buttons
        float bw = 220f * k, bh = 56f * k, by = H - bh - 40f * k;
        if (!done)
        {
            UpgradePath.Tier tier = UpgradePath.Tiers[next];
            Hud.Text(new Rect(0, by - 46f * k, W, 20f * k), "Next: " + tier.nextLevel + "   New threat: " + tier.threat, 13, new Color(1f, 0.85f, 0.4f));
            bool can = selected >= 0;
            Rect evolve = new Rect(W / 2f + 12f * k, by, bw, bh);
            if (can) { if (Hud.Button(evolve, "Evolve", 18)) Confirm(); }
            else { Hud.Panel(evolve, Dim); Hud.Text(evolve, "Pick a skill", 14, new Color(1f, 1f, 1f, 0.45f)); }
        }
        else
        {
            if (Hud.Button(new Rect(W / 2f + 12f * k, by, bw, bh), "Play", 18)) LoadIfExists(nextScene);
        }
        if (Hud.Button(new Rect(W / 2f - bw - 12f * k, by, bw, bh), "Reset path", 16)) { UpgradePath.Reset(); selected = -1; }
        Hud.Text(new Rect(0, H - 30f * k, W, 16f * k), "1 / 2 = pick    Enter = evolve    Esc = menu", 10, new Color(1f, 1f, 1f, 0.45f));

        if (evolvedFlash > 0f)
        {
            float a = Mathf.Clamp01(evolvedFlash);
            Hud.Text(new Rect(0, H * 0.19f, W, 30f * k), "Learned " + evolvedName + "!", 22, new Color(Green.r, Green.g, Green.b, a));
        }
    }

    void DrawStart(Rect r)
    {
        float k = Hud.Scale;
        Hud.Panel(r, PanelColor);
        Hud.Text(new Rect(r.x, r.y + 10f * k, r.width, 16f * k), "Stage 1", 11, new Color(1f, 1f, 1f, 0.6f));
        DrawIcon(new Rect(r.x + 14f * k, r.y + 34f * k, 64f * k, 64f * k), UpgradePath.Start.icon, 1f);
        Hud.Text(new Rect(r.x + 88f * k, r.y + 40f * k, r.width, 20f * k), "H", 18, Green, TextAnchor.MiddleLeft);
        Hud.Text(new Rect(r.x + 88f * k, r.y + 66f * k, r.width, 18f * k), "Small Beam", 12, Color.white, TextAnchor.MiddleLeft);
        Hud.Text(new Rect(r.x + 88f * k, r.y + 88f * k, r.width, 16f * k), "Start skill", 12, new Color(1f, 1f, 1f, 0.6f), TextAnchor.MiddleLeft);
    }

    void DrawCard(Rect r, UpgradePath.Option o, string key, int tier, int choice, int next, int picked, string hotkey)
    {
        float k = Hud.Scale;
        Color pathColor = o.brainPath ? Pink : Blue;
        bool current = tier == next;
        bool isPicked = picked == choice;
        bool isOther = picked >= 0 && !isPicked;
        bool future = tier > next;
        bool isSelected = current && selected == choice;
        float alpha = isOther ? 0.35f : future ? 0.45f : 1f;

        // Glow behind the selected / picked card
        if (isSelected || isPicked)
        {
            float g = 4f * Hud.Unit;
            Hud.Fill(new Rect(r.x - g / 2f, r.y - g / 2f, r.width + g, r.height + g), new Color(pathColor.r, pathColor.g, pathColor.b, isSelected ? 0.8f : 0.45f));
        }

        Color fill = future || isOther ? Dim : Color.Lerp(PanelColor, pathColor, current && Hud.Hover(r) ? 0.28f : 0.15f);
        Hud.Panel(r, fill);

        // Coloured strip on the left shows the path
        Hud.Fill(new Rect(r.x + Hud.Unit, r.y + Hud.Unit * 2f, Hud.Unit * 2f, r.height - Hud.Unit * 4f), new Color(pathColor.r, pathColor.g, pathColor.b, alpha));

        DrawIcon(new Rect(r.x + 18f * k, r.y + (r.height - 72f * k) / 2f, 72f * k, 72f * k), future ? "lock" : o.icon, alpha);

        float tx = r.x + 104f * k, tw = r.width - 112f * k;
        Hud.Text(new Rect(tx, r.y + 14f * k, tw, 22f * k), o.name, 15, new Color(1f, 1f, 1f, alpha), TextAnchor.MiddleLeft);
        Hud.Text(new Rect(tx, r.y + 46f * k, tw, 18f * k), o.line1, 12, new Color(1f, 1f, 1f, alpha * 0.8f), TextAnchor.MiddleLeft);
        Hud.Text(new Rect(tx, r.y + 70f * k, tw, 18f * k), o.line2, 12, new Color(1f, 1f, 1f, alpha * 0.8f), TextAnchor.MiddleLeft);

        string tag = isPicked ? "Learned" : current ? "Press " + hotkey : future ? "Locked" : "";
        Color tagColor = isPicked ? Green : current ? new Color(pathColor.r, pathColor.g, pathColor.b, 1f) : new Color(1f, 1f, 1f, 0.35f);
        Hud.Text(new Rect(tx, r.yMax - 32f * k, tw, 16f * k), tag, 12, tagColor, TextAnchor.MiddleLeft);
        Hud.Text(new Rect(r.xMax - 40f * k, r.y + 12f * k, 28f * k, 22f * k), key, 15, new Color(Green.r, Green.g, Green.b, alpha));

        if (current && Hud.Clicked(r)) Select(choice);
    }

    static void DrawIcon(Rect r, string name, float alpha)
    {
        if (Event.current.type != EventType.Repaint) return;
        Texture2D tex = Icon(name);
        if (tex == null) return;
        Color old = GUI.color;
        GUI.color = new Color(1f, 1f, 1f, alpha);
        GUI.DrawTexture(r, tex, ScaleMode.ScaleToFit);
        GUI.color = old;
    }

    // An elbow line: across, down/up, across
    static void Connect(Vector2 a, Vector2 b, Color first, Color second)
    {
        float t = Mathf.Max(2f, Hud.Unit);
        float mx = Mathf.Round((a.x + b.x) / 2f);
        Hud.Fill(new Rect(a.x, a.y - t / 2f, mx - a.x, t), first);
        Hud.Fill(new Rect(mx - t / 2f, Mathf.Min(a.y, b.y) - t / 2f, t, Mathf.Abs(b.y - a.y) + t), second);
        Hud.Fill(new Rect(mx, b.y - t / 2f, b.x - mx, t), second);
    }
}
