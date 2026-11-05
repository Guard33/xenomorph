using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// "Evolve!" screen (Danesh): shows the whole upgrade path as a tree and lets the player
// pick one new skill for the next evolution. Each evolution can offer 2 or 3 skills.
// Click a card (or press 1 / 2 / 3), then Evolve (or Enter). After evolving, the next level loads.
// Esc goes back to the menu. Uses the game's pixel-art UI helpers (Hud / PixelFont).
public class UpgradeScreen : MonoBehaviour
{
    public string menuScene = "StartScreen";
    public string nextScene = "Level1";      // only used when a tier has no scene of its own

    static readonly Color Pink = new Color(0.96f, 0.45f, 0.68f);
    static readonly Color Blue = new Color(0.45f, 0.7f, 1f);
    static readonly Color Green = new Color(0.55f, 1f, 0.6f);
    static readonly Color PanelColor = new Color(0.1f, 0.16f, 0.11f, 0.94f);
    static readonly Color Dim = new Color(0.12f, 0.12f, 0.15f, 0.85f);

    private int selected = -1;          // index of the selected option for the current evolution
    private float evolvedFlash;         // short "learned!" message after a pick
    private string evolvedName;
    private string loadAfterFlash;      // scene to open once the message has shown

    void Update()
    {
        if (evolvedFlash > 0f)
        {
            evolvedFlash -= Time.unscaledDeltaTime;
            if (evolvedFlash <= 0f && loadAfterFlash != null) LoadIfExists(loadAfterFlash);
            return;
        }
#if ENABLE_INPUT_SYSTEM
        Keyboard kb = Keyboard.current;
        if (kb == null) return;
        if (kb.digit1Key.wasPressedThisFrame || kb.numpad1Key.wasPressedThisFrame) Select(0);
        if (kb.digit2Key.wasPressedThisFrame || kb.numpad2Key.wasPressedThisFrame) Select(1);
        if (kb.digit3Key.wasPressedThisFrame || kb.numpad3Key.wasPressedThisFrame) Select(2);
        if (kb.enterKey.wasPressedThisFrame) Confirm();
        if (kb.escapeKey.wasPressedThisFrame) LoadIfExists(menuScene);
#else
        if (Input.GetKeyDown(KeyCode.Alpha1)) Select(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) Select(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) Select(2);
        if (Input.GetKeyDown(KeyCode.Return)) Confirm();
        if (Input.GetKeyDown(KeyCode.Escape)) LoadIfExists(menuScene);
#endif
    }

    void Select(int choice)
    {
        int t = UpgradePath.NextTier;
        if (t < UpgradePath.Tiers.Length && choice < UpgradePath.Tiers[t].options.Length) selected = choice;
    }

    void Confirm()
    {
        int t = UpgradePath.NextTier;
        if (t >= UpgradePath.Tiers.Length) { LoadIfExists(menuScene); return; }
        if (selected < 0 || evolvedFlash > 0f) return;
        UpgradePath.Chosen[t] = selected;
        evolvedName = UpgradePath.Picked(t).name;
        evolvedFlash = 1.6f;
        selected = -1;
        // One evolution per visit: go on to the level this skill is for
        loadAfterFlash = UpgradePath.Tiers[t].scene ?? menuScene;
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

        // Tree layout: start node, then one column per evolution (2 or 3 cards each)
        float cardW = 300f * k, cardH = 112f * k, colGap = 70f * k, rowGap = 22f * k;
        float startW = 230f * k;
        float totalW = startW + UpgradePath.Tiers.Length * (cardW + colGap);
        float x0 = (W - totalW) / 2f;
        float midY = H * 0.5f + 10f * k;
        int most = 0;
        foreach (UpgradePath.Tier tr in UpgradePath.Tiers) most = Mathf.Max(most, tr.options.Length);
        float tallest = most * cardH + (most - 1) * rowGap;
        float labelY = midY - tallest / 2f;

        Rect startCard = new Rect(x0, midY - cardH / 2f, startW, cardH);
        Vector2 from = new Vector2(startCard.xMax, midY);

        float x = startCard.xMax + colGap;
        for (int t = 0; t < UpgradePath.Tiers.Length; t++)
        {
            UpgradePath.Tier tier = UpgradePath.Tiers[t];
            int n = tier.options.Length;
            float colH = n * cardH + (n - 1) * rowGap;
            float y0 = midY - colH / 2f;

            int picked = UpgradePath.Chosen[t];
            bool reachable = t <= next;
            Color lineColor = reachable ? new Color(1f, 1f, 1f, 0.55f) : new Color(1f, 1f, 1f, 0.15f);

            // Column label
            Hud.Text(new Rect(x, labelY - 52f * k, cardW, 18f * k), "Evolution " + (t + 1) + "  -  key " + tier.key, 12, new Color(1f, 1f, 1f, reachable ? 0.85f : 0.35f));
            Hud.Text(new Rect(x, labelY - 30f * k, cardW, 16f * k), tier.nextLevel, 12, new Color(1f, 1f, 1f, reachable ? 0.6f : 0.25f));

            Rect pickedCard = default;
            for (int c = 0; c < n; c++)
            {
                Rect card = new Rect(x, y0 + c * (cardH + rowGap), cardW, cardH);
                UpgradePath.Option o = tier.options[c];
                bool lit = picked == c || (t == next && selected == c);
                Connect(from, new Vector2(card.x, card.center.y), lineColor, lit ? (o.brainPath ? Pink : Blue) : lineColor);
                DrawCard(card, o, tier.key, t, c, next, picked, (c + 1).ToString());
                if (picked == c) pickedCard = card;
            }

            from = picked >= 0 ? new Vector2(pickedCard.xMax, pickedCard.center.y) : new Vector2(x + cardW, midY);
            x += cardW + colGap;
        }

        DrawStart(startCard);

        // Path legend
        float ly = midY + tallest / 2f + 28f * k;
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
            bool can = selected >= 0 && evolvedFlash <= 0f;
            Rect evolve = new Rect(W / 2f + 12f * k, by, bw, bh);
            if (can) { if (Hud.Button(evolve, "Evolve", 18)) Confirm(); }
            else { Hud.Panel(evolve, Dim); Hud.Text(evolve, evolvedFlash > 0f ? "Evolving..." : "Pick a skill", 14, new Color(1f, 1f, 1f, 0.45f)); }
        }
        else
        {
            if (Hud.Button(new Rect(W / 2f + 12f * k, by, bw, bh), "Menu", 18)) LoadIfExists(menuScene);
        }
        if (Hud.Button(new Rect(W / 2f - bw - 12f * k, by, bw, bh), "Reset path", 16)) { UpgradePath.Reset(); selected = -1; }
        Hud.Text(new Rect(0, H - 30f * k, W, 16f * k), "1 / 2 / 3 = pick    Enter = evolve    Esc = menu", 10, new Color(1f, 1f, 1f, 0.45f));

        if (evolvedFlash > 0f)
        {
            float a = Mathf.Clamp01(evolvedFlash * 2f);
            Hud.Text(new Rect(0, H * 0.19f, W, 30f * k), "Learned " + evolvedName + "!", 22, new Color(Green.r, Green.g, Green.b, a));
        }
    }

    void DrawStart(Rect r)
    {
        float k = Hud.Scale;
        Hud.Panel(r, PanelColor);
        Hud.Text(new Rect(r.x, r.y + 10f * k, r.width, 16f * k), "Stage 1", 11, new Color(1f, 1f, 1f, 0.6f));
        DrawIcon(new Rect(r.x + 14f * k, r.y + 30f * k, 60f * k, 60f * k), UpgradePath.Start.icon, 1f);
        Hud.Text(new Rect(r.x + 88f * k, r.y + 34f * k, r.width, 20f * k), "H", 18, Green, TextAnchor.MiddleLeft);
        Hud.Text(new Rect(r.x + 88f * k, r.y + 60f * k, r.width, 18f * k), "Small Beam", 12, Color.white, TextAnchor.MiddleLeft);
        Hud.Text(new Rect(r.x + 88f * k, r.y + 82f * k, r.width, 16f * k), "Start skill", 12, new Color(1f, 1f, 1f, 0.6f), TextAnchor.MiddleLeft);
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

        DrawIcon(new Rect(r.x + 18f * k, r.y + (r.height - 64f * k) / 2f, 64f * k, 64f * k), future ? "lock" : o.icon, alpha);

        float tx = r.x + 96f * k, tw = r.width - 104f * k;
        Hud.Text(new Rect(tx, r.y + 12f * k, tw, 22f * k), o.name, 15, new Color(1f, 1f, 1f, alpha), TextAnchor.MiddleLeft);
        Hud.Text(new Rect(tx, r.y + 40f * k, tw, 18f * k), o.line1, 12, new Color(1f, 1f, 1f, alpha * 0.8f), TextAnchor.MiddleLeft);
        Hud.Text(new Rect(tx, r.y + 60f * k, tw, 18f * k), o.line2, 12, new Color(1f, 1f, 1f, alpha * 0.8f), TextAnchor.MiddleLeft);

        string tag = isPicked ? "Learned" : current ? "Press " + hotkey : future ? "Locked" : "";
        Color tagColor = isPicked ? Green : current ? new Color(pathColor.r, pathColor.g, pathColor.b, 1f) : new Color(1f, 1f, 1f, 0.35f);
        Hud.Text(new Rect(tx, r.yMax - 28f * k, tw, 16f * k), tag, 12, tagColor, TextAnchor.MiddleLeft);
        Hud.Text(new Rect(r.xMax - 40f * k, r.y + 10f * k, 28f * k, 22f * k), key, 15, new Color(Green.r, Green.g, Green.b, alpha));

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
