using UnityEngine;

// Level 3 mini boss: the General, who runs Fort Maple Command.
// He paces the parade ground in front of HQ and never runs away. He takes 12 beam hits
// (shown as a health bar at the top of the screen) and, unlike everyone else, he fights back:
// when he can see the alien (inside his vision cone) he raises the base alarm and fires
// tranquilliser darts at it (1 damage each).
//   Phase 1 (full health)  one dart every 2.2 s
//   Phase 2 (below 2/3)    sounds the base alarm, then 3-dart spreads every 1.8 s
//   Phase 3 (below 1/3)    sounds the alarm again, faster spreads every 1.3 s
// Disguise hides the alien from him; the Psi Shield blocks his darts.
[RequireComponent(typeof(Civilian))]
public class GeneralBoss : MonoBehaviour
{
    public string title = "The General";
    public int patrol = -1;             // index into the level's Patrols
    public float sightRadius = 7.5f;    // tiles
    public float walkSpeed = 1f;
    public float alarmRadius = 14f;

    public static GeneralBoss Instance { get; private set; }
    public int Phase { get; private set; } = 1;
    public int DartsFired { get; private set; }

    private Civilian civ;
    private AlienPlayer alien;
    private SpriteRenderer sr;
    private Vector2[] route;
    private int next = 1, dir = 1;
    private float fireTimer = 1.5f;
    private int maxHits;
    private bool announced;
    private VisionCone cone;
    private TextMesh mark;

    void Awake()
    {
        Instance = this;
        civ = GetComponent<Civilian>();
        civ.holdsPosition = true;
        civ.hidePips = true;
    }

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        alien = FindFirstObjectByType<AlienPlayer>();
        maxHits = Mathf.Max(1, civ.toughness);

        Vector2Int[][] patrols = LevelMap.Current.Patrols;
        if (patrol >= 0 && patrol < patrols.Length)
        {
            route = new Vector2[patrols[patrol].Length];
            for (int i = 0; i < route.Length; i++) route[i] = RoadNetwork.TileCenter(patrols[patrol][i]);
        }

        // The "!" is a child of a 2x sprite, so undo the scale
        float s = Mathf.Max(0.01f, transform.localScale.x);
        cone = VisionCone.Attach(transform, sightRadius, 50f, 0f);
        mark = TownLookout.MakeMark(transform);
        mark.transform.localScale = Vector3.one / s;
        mark.transform.localPosition = new Vector3(0f, 1.1f / s + 0.5f, 0f);
    }

    public float Health01 => (float)civ.HitsLeft / maxHits;

    void Update()
    {
        if (!civ.CanBeBefriended)
        {
            if (cone != null) cone.Visible = false;
            if (mark != null) mark.text = "";
            if (!announced)
            {
                announced = true;
                if (LevelManager.Instance != null) LevelManager.Instance.Flash("The General is your friend now!", 3f);
            }
            return;
        }

        // Phase changes: sound the base alarm
        int phase = Health01 > 0.67f ? 1 : Health01 > 0.34f ? 2 : 3;
        if (phase > Phase)
        {
            Phase = phase;
            fireTimer = 0.8f;
            Civilian.AlertAround(transform.position, alarmRadius);
            if (alien != null) BaseAlarm.Raise(alien.transform.position, "The General: \"Sound the alarm!\"");
            if (LevelManager.Instance != null)
                LevelManager.Instance.Flash(Phase == 2 ? "The General: \"Sound the alarm!\"" : "The General: \"All units, stop that alien!\"", 2.5f);
        }

        bool sees = alien != null && !alien.IsDead && !AlienPlayer.Disguised && cone.Sees(alien.transform.position);
        if (sees)
        {
            Vector2 to = alien.transform.position - transform.position;
            cone.TurnTowards(VisionCone.AngleOf(to), 400f);
            if (!BaseAlarm.Active) BaseAlarm.Raise(alien.transform.position, "The General spotted the alien! BASE ALARM!");
            else BaseAlarm.Spotted(alien.transform.position);
            if (sr != null && Mathf.Abs(to.x) > 0.05f) sr.flipX = to.x < 0f;
            fireTimer -= Time.deltaTime;
            if (fireTimer <= 0f)
            {
                Fire(to.normalized);
                fireTimer = Phase == 1 ? 2.2f : Phase == 2 ? 1.8f : 1.3f;
            }
        }
        else
        {
            fireTimer = Mathf.Max(fireTimer, 0.6f);   // a moment to aim after spotting the alien again
            Walk();
        }

        mark.text = sees ? "!" : "";
        mark.color = new Color(1f, 0.35f, 0.25f);
        cone.color = sees ? new Color(1f, 0.3f, 0.25f, 0.2f) : new Color(1f, 0.6f, 0.35f, 0.12f);
    }

    void Fire(Vector2 aim)
    {
        float speed = 4.5f + Phase * 0.7f;
        float[] angles = Phase == 1 ? new[] { 0f } : new[] { -16f, 0f, 16f };
        foreach (float a in angles)
            Dart.Spawn(transform.position, Quaternion.Euler(0f, 0f, a) * aim, speed, "The General's darts knocked the alien out!");
        DartsFired += angles.Length;
    }

    void Walk()
    {
        if (route == null || route.Length < 2) return;
        Vector2 target = route[next];
        if (sr != null && Mathf.Abs(target.x - transform.position.x) > 0.01f)
            sr.flipX = target.x < transform.position.x;
        cone.TurnTowards(VisionCone.AngleOf(target - (Vector2)transform.position), 200f);
        transform.position = Vector2.MoveTowards(transform.position, target, walkSpeed * Time.deltaTime);
        if (Vector2.Distance(transform.position, target) < 0.05f)
        {
            if (next + dir < 0 || next + dir >= route.Length) dir = -dir;
            next += dir;
        }
    }

    // Health bar at the top of the screen, under the tip bubble
    void OnGUI()
    {
        if (!civ.CanBeBefriended || LevelManager.Instance == null || LevelManager.Instance.IsComplete) return;
        float k = Hud.Scale, u = Hud.Unit;
        float w = 360f * k, h = 6 * u;
        float x = (Screen.width - w) / 2f, y = 100f * k;   // below the tip bubble
        Hud.Text(new Rect(x, y - 22f * k, w, 20f * k), title + (Phase > 1 ? "  (phase " + Phase + ")" : ""), 13, new Color(1f, 0.75f, 0.6f));
        Hud.Panel(new Rect(x, y, w, h), new Color(0.12f, 0.06f, 0.06f, 0.9f));
        float fill = (w - 2 * u) * Health01;
        Hud.Fill(new Rect(x + u, y + u, Mathf.Round(fill / u) * u, h - 2 * u), new Color(0.9f, 0.25f, 0.2f));
    }
}

// A tranquilliser dart (the General, soldiers and sentries). Flies straight, stops at buildings
// and walls, and does 1 damage and slows the alien for a second if it hits (unless the Psi Shield blocks it).
public class Dart : MonoBehaviour
{
    public Vector2 velocity;
    public string reason = "A tranquilliser dart knocked the alien out!";
    private float life = 3f;
    private AlienPlayer alien;

    public static int Hits { get; private set; }
    public static int Blocked { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetCounts() { Hits = 0; Blocked = 0; }

    public static Dart Spawn(Vector2 from, Vector2 dir, float speed, string reason = null)
    {
        GameObject go = new GameObject("Dart");
        go.transform.position = from + dir * 0.6f;
        go.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
        go.transform.localScale = new Vector3(0.4f, 0.12f, 1f);
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>(); Glow.Unlit(sr);
        sr.sprite = BeamEffect.Pixel;
        sr.color = new Color(1f, 0.85f, 0.3f);
        sr.sortingOrder = 29500;
        Dart d = go.AddComponent<Dart>();
        d.velocity = dir.normalized * speed;
        if (reason != null) d.reason = reason;
        return d;
    }

    void Start() { alien = FindFirstObjectByType<AlienPlayer>(); }

    void Update()
    {
        life -= Time.deltaTime;
        transform.position += (Vector3)(velocity * Time.deltaTime);
        Vector2Int t = RoadNetwork.TileAt(transform.position);
        bool inMap = t.x >= 0 && t.y >= 0 && t.x < LevelMap.Current.Width && t.y < LevelMap.Current.Height;
        if (life <= 0f || !inMap) { Destroy(gameObject); return; }
        if (!RoadNetwork.Walkable(t) && !IsGround(t)) { Destroy(gameObject); return; }   // hit a building or wall

        if (alien != null && Vector2.Distance(alien.transform.position, transform.position) < 0.45f)
        {
            if (alien.TakeDamage(1, reason)) { Hits++; alien.Tranquilize(1f); }
            else Blocked++;
            Destroy(gameObject);
        }
    }

    // Water and low things (fields) don't stop darts; buildings, vehicles, trees and fences do
    static bool IsGround(Vector2Int t)
    {
        char g = LevelMap.Current.Ground[t.y][t.x];
        return g == '~' || g == '=';
    }
}
