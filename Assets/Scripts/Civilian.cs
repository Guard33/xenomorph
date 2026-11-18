using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A civilian walks around near home. When the alien comes close, the civilian
// gets scared, runs faster and follows the roads to the closest exit.
// If the alien is guarding that exit or standing in the way, they run for the other one,
// and they keep checking while they run.
public class Civilian : MonoBehaviour
{
    public static readonly List<Civilian> All = new List<Civilian>();

    public float walkSpeed = 1f;
    public float runSpeed = 2.2f;      // faster once alerted (from our outline)
    public float wanderRadius = 1f;
    public float alertRadius = 2.5f;
    public AudioClip friendshipNoise;  // played when befriended (Talia)
    public Sprite heartSprite;
    public bool holdsPosition;         // lookouts and police don't run away
    public int toughness = 1;          // beam hits needed to befriend (police: 3)
    public bool hidePips;              // the boss shows a health bar on the HUD instead

    public bool IsAlerted => alerted;
    public bool IsCalm => calm > 0f;
    public int TargetExit => exit;     // index into RoadNetwork.Exits

    const float RecheckEvery = 0.3f;   // seconds between "is the way still clear?" checks

    private Vector2 home;
    private Vector2 target;
    private float waitTimer;
    private bool alerted;
    private bool gone;
    private readonly List<Vector2> route = new List<Vector2>();
    private int exit = -1;
    private float recheck;
    private int hits;
    private float hitFlash;
    private float calm;                // Calm Hum: seconds left of not panicking
    private SpriteRenderer[] pips;     // hit points shown above tough people
    private Transform alien;
    private SpriteRenderer sr;

    public bool CanBeBefriended => !gone;

    void OnEnable() { All.Add(this); }
    void OnDisable() { All.Remove(this); }

    void Start()
    {
        home = transform.position;
        target = home;
        sr = GetComponent<SpriteRenderer>();
        AlienPlayer player = FindFirstObjectByType<AlienPlayer>();
        if (player != null) alien = player.transform;
        waitTimer = Random.Range(0.5f, 2.5f);
        if (toughness > 1 && !hidePips) MakePips();
    }

    void Update()
    {
        if (gone) return;

        if (calm > 0f) calm -= Time.deltaTime;

        if (hitFlash > 0f)
        {
            hitFlash -= Time.deltaTime;
            if (sr != null) sr.color = hitFlash > 0f ? new Color(0.6f, 1f, 0.7f) : Color.white;
        }

        if (!alerted && !holdsPosition && alien != null && !AlienPlayer.Disguised &&
            Vector2.Distance(alien.position, transform.position) < alertRadius)
            Alert();

        if (alerted) Run();
        else Wander();
    }

    // Scared: run along the roads to the closest exit the alien isn't blocking
    public void Alert()
    {
        if (alerted || gone || holdsPosition || IsCalm) return;
        alerted = true;
        PickRoute();
    }

    void PickRoute()
    {
        Vector2 alienPos = alien != null ? (Vector2)alien.position : new Vector2(-100f, -100f);
        exit = RoadNetwork.ChooseExit(transform.position, alienPos, out List<Vector2> r);
        route.Clear();
        route.AddRange(r);
        recheck = RecheckEvery;
    }

    // While running: if the alien now blocks our exit, switch to a clear one
    void RecheckRoute()
    {
        recheck -= Time.deltaTime;
        if (recheck > 0f || alien == null || exit < 0) return;
        recheck = RecheckEvery;
        if (RoadNetwork.IsBlocked(route, exit, transform.position, alien.position))
            PickRoute();
    }

    // Calm Hum: stop panicking for a while. Runners stop and settle where they are
    // (that becomes their new home, so they never walk through buildings to get back).
    public void CalmDown(float seconds)
    {
        if (gone) return;
        calm = Mathf.Max(calm, seconds);
        if (!alerted) return;
        alerted = false;
        route.Clear();
        exit = -1;
        home = transform.position;
        target = home;
        waitTimer = 0.5f;
    }

    // Everyone within radius of a point gets scared (used by the lookouts' alarm)
    public static int AlertAround(Vector2 point, float radius)
    {
        int count = 0;
        foreach (Civilian c in All.ToArray())
        {
            if (c.alerted || c.gone || c.holdsPosition || c.IsCalm) continue;
            if (Vector2.Distance(c.transform.position, point) > radius) continue;
            c.Alert();
            count++;
        }
        return count;
    }

    void Wander()
    {
        if (holdsPosition) return;   // lookouts stand guard
        if (waitTimer > 0f)
        {
            waitTimer -= Time.deltaTime;
            return;
        }
        MoveTowards(target, walkSpeed);
        if (Vector2.Distance(transform.position, target) < 0.05f)
        {
            target = PickWanderSpot();
            waitTimer = Random.Range(1f, 3f);
        }
    }

    // A spot near home on land (not water, buildings or trees), reachable in a straight line
    Vector2 PickWanderSpot()
    {
        Vector2 here = transform.position;
        for (int i = 0; i < 8; i++)
        {
            Vector2 p = home + Random.insideUnitCircle * wanderRadius;
            if (ClearLine(here, p)) return p;
        }
        return home;
    }

    // Every tile under the straight line (plus a small margin) is open ground,
    // so people never cut the corner of a house or step to the edge of the water
    static bool ClearLine(Vector2 a, Vector2 b)
    {
        const float margin = 0.2f;
        Vector2 min = Vector2.Min(a, b) - Vector2.one * margin;
        Vector2 max = Vector2.Max(a, b) + Vector2.one * margin;
        for (int x = Mathf.FloorToInt(min.x); x <= Mathf.FloorToInt(max.x); x++)
            for (int y = Mathf.FloorToInt(min.y); y <= Mathf.FloorToInt(max.y); y++)
                if (!RoadNetwork.Walkable(new Vector2(x + 0.5f, y + 0.5f))) return false;
        return true;
    }

    void Run()
    {
        if (route.Count == 0)
        {
            Escape();
            return;
        }
        RecheckRoute();
        MoveTowards(route[0], runSpeed);
        if (Vector2.Distance(transform.position, route[0]) < 0.05f)
            route.RemoveAt(0);
    }

    void MoveTowards(Vector2 point, float speed)
    {
        if (sr != null && Mathf.Abs(point.x - transform.position.x) > 0.01f)
            sr.flipX = point.x < transform.position.x;
        transform.position = Vector2.MoveTowards(transform.position, point, speed * Time.deltaTime);
    }

    void Escape()
    {
        gone = true;
        if (LevelManager.Instance != null) LevelManager.Instance.PersonEscaped();
        Destroy(gameObject);
    }

    // A beam (or trap) hits this person. Most people are befriended at once;
    // tough people (police) need several hits. Returns true if they are befriended.
    public bool Hit(int damage = 1)
    {
        if (gone) return false;
        hits += damage;
        if (hits >= toughness)
        {
            Befriend();
            return true;
        }
        hitFlash = 0.15f;
        UpdatePips();
        return false;
    }

    public int HitsLeft => Mathf.Max(0, toughness - hits);

    void MakePips()
    {
        pips = new SpriteRenderer[toughness];
        for (int i = 0; i < toughness; i++)
        {
            GameObject p = new GameObject("HP " + (i + 1));
            p.transform.SetParent(transform, false);
            p.transform.localPosition = new Vector3((i - (toughness - 1) / 2f) * 0.22f - 0.08f, 0.7f, 0f);
            p.transform.localScale = new Vector3(0.16f, 0.16f, 1f);
            SpriteRenderer s = p.AddComponent<SpriteRenderer>(); Glow.Unlit(s);
            s.sprite = BeamEffect.Pixel;
            s.sortingOrder = 30000;
            pips[i] = s;
        }
        UpdatePips();
    }

    void UpdatePips()
    {
        if (pips == null) return;
        for (int i = 0; i < pips.Length; i++)
            pips[i].color = i < HitsLeft ? new Color(0.35f, 0.6f, 1f) : new Color(0.1f, 0.1f, 0.15f, 0.6f);
    }

    public void Befriend()
    {
        if (gone) return;
        gone = true;
        if (LevelManager.Instance != null) LevelManager.Instance.PersonBefriended();
        if (friendshipNoise != null) AudioSource.PlayClipAtPoint(friendshipNoise, Camera.main != null ? Camera.main.transform.position : transform.position);
        StartCoroutine(BeamUpToUfo());
    }

    // A heart pops up, then the new friend is teleported to the UFO.
    IEnumerator BeamUpToUfo()
    {
        if (heartSprite != null)
        {
            GameObject heart = new GameObject("Heart");
            heart.transform.SetParent(transform, false);
            heart.transform.localPosition = new Vector3(0f, 0.8f, 0f);
            SpriteRenderer hs = heart.AddComponent<SpriteRenderer>(); Glow.Unlit(hs);
            hs.sprite = heartSprite;
            hs.sortingOrder = 30000;
        }

        Vector3 startScale = transform.localScale;
        float t = 0f;
        while (t < 0.7f)
        {
            t += Time.deltaTime;
            float k = t / 0.7f;
            transform.localScale = Vector3.Lerp(startScale, new Vector3(0.1f, 1.8f, 1f), k);
            if (sr != null) sr.color = new Color(1f, 1f, 1f, 1f - k);
            yield return null;
        }
        Destroy(gameObject);
    }
}
