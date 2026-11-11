using UnityEngine;
using UnityEngine.SceneManagement;

// A little arrow that floats next to the alien and points where the beam will go (Danesh).
// The alien can aim in 8 directions, so the arrow makes diagonal aiming easy to read.
// The arrow is bright green when the Small Beam is ready and faded while it recharges.
// It adds itself to the alien when a scene starts, so no scene or prefab changes are needed.
public class AimArrow : MonoBehaviour
{
    public float distance = 0.95f;      // tiles from the alien to the arrow
    public float turnSpeed = 900f;      // degrees per second
    public float bob = 0.06f;           // small in-and-out movement
    public Color readyColor = new Color(0.45f, 1f, 0.55f, 1f);
    public Color waitColor = new Color(1f, 1f, 1f, 0.4f);

    private AlienPlayer player;
    private SpriteRenderer sr;
    private float angle;

    // ------------------------------------------------------------------ setup
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        AttachAll();
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode) { AttachAll(); }

    static void AttachAll()
    {
        foreach (AlienPlayer p in FindObjectsByType<AlienPlayer>(FindObjectsSortMode.None))
        {
            bool has = false;
            foreach (AimArrow a in FindObjectsByType<AimArrow>(FindObjectsSortMode.None))
                if (a.player == p) has = true;
            if (!has) Create(p);
        }
    }

    public static AimArrow Create(AlienPlayer p)
    {
        GameObject go = new GameObject("Aim Arrow");
        AimArrow arrow = go.AddComponent<AimArrow>();
        arrow.player = p;
        arrow.sr = go.AddComponent<SpriteRenderer>();
        arrow.sr.sprite = ArrowSprite;
        arrow.sr.sortingOrder = 28990;   // above the map, just under the beam
        arrow.angle = Angle(p.Facing);
        arrow.Place();
        return arrow;
    }

    // ------------------------------------------------------------------ follow
    void LateUpdate()
    {
        if (player == null) { Destroy(gameObject); return; }
        sr.enabled = !player.IsDead;
        if (player.IsDead) return;

        angle = Mathf.MoveTowardsAngle(angle, Angle(player.Facing), turnSpeed * Time.deltaTime);
        Place();

        bool ready = player.skills != null && player.skills.Length > 0 && player.skills[0].Ready;
        sr.color = ready ? readyColor : waitColor;
    }

    void Place()
    {
        float rad = angle * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        float d = distance + Mathf.Sin(Time.time * 6f) * bob;
        transform.position = (Vector2)player.transform.position + dir * d;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    static float Angle(Vector2 dir) { return Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg; }

    // ------------------------------------------------------------------ pixel art
    // 9 x 9 arrow pointing right. # = white (tinted), o = dark outline, . = clear
    static readonly string[] Pattern =
    {
        "..oo.....",
        "..o#o....",
        "..o##o...",
        "..o###o..",
        "..o####o.",
        "..o###o..",
        "..o##o...",
        "..o#o....",
        "..oo.....",
    };

    static Sprite arrowSprite;

    public static Sprite ArrowSprite
    {
        get
        {
            if (arrowSprite != null) return arrowSprite;
            int n = Pattern.Length;
            Texture2D tex = new Texture2D(n, n, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;
            Color dark = new Color(0.05f, 0.1f, 0.08f, 1f);
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    char c = Pattern[n - 1 - y][x];
                    tex.SetPixel(x, y, c == '#' ? Color.white : c == 'o' ? dark : Color.clear);
                }
            tex.Apply();
            arrowSprite = Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), 16f);
            return arrowSprite;
        }
    }
}
