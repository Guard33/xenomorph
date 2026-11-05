using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// Small map of level 1 in the top-right corner. Press M to hide or show it.
// Green dot = alien, white dots = people, red squares = exits, grey dot = UFO.
// The map picture is made from the current level's data, so it updates when the level data changes.
public class MiniMap : MonoBehaviour
{
    public int pixelsPerTile = 4;
    public float mapScale = 1f;         // on a 1080p screen
    public bool showOnStart = true;

    private bool visible;

    private Texture2D mapTexture;
    private Transform alien;
    private Vector2 ufoPosition;

    static readonly Color Grass = new Color32(96, 160, 72, 255);
    static readonly Color Road = new Color32(170, 120, 70, 255);
    static readonly Color Water = new Color32(70, 130, 210, 255);
    static readonly Color Bridge = new Color32(120, 80, 40, 255);
    static readonly Color Building = new Color32(160, 70, 60, 255);
    static readonly Color Field = new Color32(215, 185, 95, 255);
    static readonly Color Tree = new Color32(35, 95, 45, 255);
    static readonly Color Concrete = new Color32(128, 124, 108, 255);
    static readonly Color Dirt = new Color32(140, 96, 50, 255);
    static readonly Color Vehicle = new Color32(70, 90, 60, 255);
    static readonly Color Fence = new Color32(40, 36, 44, 255);

    void Start()
    {
        AlienPlayer player = FindFirstObjectByType<AlienPlayer>();
        if (player != null) alien = player.transform;

        GameObject ufo = GameObject.Find("UFO (start)");
        ufoPosition = ufo != null ? (Vector2)ufo.transform.position + Vector2.one : RoadNetwork.TileCenter(LevelMap.Current.Start);

        visible = showOnStart;
        BuildTexture();
    }

    void Update()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.mKey.wasPressedThisFrame) visible = !visible;
#else
        if (Input.GetKeyDown(KeyCode.M)) visible = !visible;
#endif
    }

    void BuildTexture()
    {
        LevelMap map = LevelMap.Current;
        int W = map.Width, H = map.Height, s = pixelsPerTile;
        mapTexture = new Texture2D(W * s, H * s, TextureFormat.RGBA32, false);
        mapTexture.filterMode = FilterMode.Point;
        mapTexture.wrapMode = TextureWrapMode.Clamp;
        Color[] pixels = new Color[W * s * H * s];

        void Fill(int tx, int topRow, int w, int h, Color c)
        {
            for (int row = topRow; row < topRow + h; row++)
                for (int x = tx; x < tx + w; x++)
                {
                    if (x < 0 || row < 0 || x >= W || row >= H) continue;
                    int py = (H - 1 - row) * s;
                    for (int yy = 0; yy < s; yy++)
                        for (int xx = 0; xx < s; xx++)
                            pixels[(py + yy) * W * s + x * s + xx] = c;
                }
        }

        for (int row = 0; row < H; row++)
            for (int x = 0; x < W; x++)
            {
                char g = map.Ground[row][x];
                char p = map.Paths[row][x];
                Color c = (g == '~' || g == '=') ? Water : g == 'c' ? Concrete : (g == 'd' || g == 's') ? Dirt : Grass;
                if (p == '#') c = Bridge;
                else if (p != '.') c = Road;
                Fill(x, row, 1, 1, c);
            }

        foreach (Obj o in map.Objects)
        {
            if (o.kind == "field") Fill(o.x, o.y, o.w, o.h, Field);
            else if (o.kind == "building") Fill(o.x, o.y, o.w, o.h, Building);
            else if (o.kind == "tree") Fill(o.x, o.y, 1, 1, Tree);
            else if (o.kind == "vehicle") Fill(o.x, o.y, o.w, o.h, Vehicle);
            else if (o.kind == "fence") Fill(o.x, o.y, o.w, o.h, Fence);
        }

        mapTexture.SetPixels(pixels);
        mapTexture.Apply();
    }

    void OnGUI()
    {
        if (!visible || mapTexture == null) return;
        float k = Mathf.Max(0.75f, Screen.height / 1080f) * mapScale;
        float w = mapTexture.width * k, h = mapTexture.height * k;
        Rect r = new Rect(Screen.width - w - 12, 12, w, h);
        DrawRect(new Rect(r.x - 2, r.y - 2, w + 4, h + 4), new Color(0f, 0f, 0f, 0.5f));
        GUI.DrawTexture(r, mapTexture);

        foreach (Vector2Int exit in LevelMap.Current.Exits)
            Mark(r, RoadNetwork.TileCenter(exit), new Color(1f, 0.15f, 0.15f), 7 * k);
        Mark(r, ufoPosition, new Color(0.8f, 0.8f, 0.85f), 6 * k);
        foreach (Civilian c in Civilian.All)
            if (c.CanBeBefriended) Mark(r, c.transform.position, Color.white, 4 * k);
        if (alien != null)
        {
            Mark(r, alien.position, Color.black, 8 * k);
            Mark(r, alien.position, new Color(0.3f, 1f, 0.4f), 6 * k);
        }
    }

    void Mark(Rect r, Vector2 world, Color color, float size)
    {
        float u = world.x / LevelMap.Current.Width;
        float v = world.y / LevelMap.Current.Height;
        Vector2 p = new Vector2(r.x + u * r.width, r.y + (1f - v) * r.height);
        DrawRect(new Rect(p.x - size / 2f, p.y - size / 2f, size, size), color);
    }

    static void DrawRect(Rect r, Color c)
    {
        Color old = GUI.color;
        GUI.color = c;
        GUI.DrawTexture(r, Texture2D.whiteTexture);
        GUI.color = old;
    }
}
