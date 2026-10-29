using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A civilian walks around near home. When the alien comes close, the civilian
// gets scared, runs faster and follows the roads to the closest exit.
public class Civilian : MonoBehaviour
{
    public static readonly List<Civilian> All = new List<Civilian>();

    public float walkSpeed = 1f;
    public float runSpeed = 2.2f;      // faster once alerted (from our outline)
    public float wanderRadius = 1f;
    public float alertRadius = 2.5f;
    public Sprite heartSprite;
    public bool holdsPosition;         // lookouts stay put and raise the alarm instead of running

    public bool IsAlerted => alerted;

    private Vector2 home;
    private Vector2 target;
    private float waitTimer;
    private bool alerted;
    private bool gone;
    private readonly List<Vector2> route = new List<Vector2>();
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
    }

    void Update()
    {
        if (gone) return;

        if (!alerted && !holdsPosition && alien != null &&
            Vector2.Distance(alien.position, transform.position) < alertRadius)
            Alert();

        if (alerted) Run();
        else Wander();
    }

    // Scared: run along the roads to the closest exit
    public void Alert()
    {
        if (alerted || gone || holdsPosition) return;
        alerted = true;
        route.Clear();
        route.AddRange(RoadNetwork.RouteToClosestExit(transform.position));
    }

    // Everyone within radius of a point gets scared (used by the lookouts' alarm)
    public static int AlertAround(Vector2 point, float radius)
    {
        int count = 0;
        foreach (Civilian c in All.ToArray())
        {
            if (c.alerted || c.gone || c.holdsPosition) continue;
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

    public void Befriend()
    {
        if (gone) return;
        gone = true;
        if (LevelManager.Instance != null) LevelManager.Instance.PersonBefriended();
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
            SpriteRenderer hs = heart.AddComponent<SpriteRenderer>();
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
