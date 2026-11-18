using System.Collections.Generic;
using UnityEngine;

// A guard on a patrol route: a town police officer (Level 2) or a soldier (Levels 2 and 3).
// Walks back and forth along the route and never runs away. Takes 3 beam hits to befriend.
// They see the alien only inside their vision cone (buildings block it).
//   Police:   after a short stare they blow the whistle and people nearby panic.
//   Soldiers: they raise the BASE ALARM (every guard on the map is alerted) and fire
//             tranquilliser darts (1 damage, slows the alien). While the alarm is on, soldiers who
//             can't see the alien run to where it was last seen; afterwards they walk back to their route.
[RequireComponent(typeof(Civilian))]
public class PolicePatrol : MonoBehaviour
{
    public int patrol;                 // index into the level's Patrols
    public bool military;              // a soldier (tranquilliser rifle, base alarm)
    public float walkSpeed = 1.3f;
    public float runSpeed = 2.5f;      // hunting during the alarm
    public float sightRange = 5.5f;    // tiles
    public float sightHalfAngle = 35f;
    public float whistleRadius = 6f;
    public float warningTime = 0.9f;
    public float whistleCooldown = 6f;
    public float fireInterval = 2f;

    public static int WhistlesBlown { get; private set; }
    public static int DartsFired { get; private set; }

    private Civilian civ;
    private SpriteRenderer sr;
    private AlienPlayer alien;
    private Vector2[] route;
    private int next = 1, dir = 1;
    private float warning, cooldown, fireTimer = 0.6f;
    private VisionCone cone;
    private TextMesh mark;

    // Hunting / returning along a path found on the map
    private List<Vector2> path;
    private int pathIndex;
    private Vector2 pathGoal;
    private float repath;
    private bool returning;

    public bool Hunting { get; private set; }

    void Awake()
    {
        civ = GetComponent<Civilian>();
        civ.holdsPosition = true;
        WhistlesBlown = 0;
        DartsFired = 0;
    }

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        alien = FindFirstObjectByType<AlienPlayer>();

        Vector2Int[][] patrols = LevelMap.Current.Patrols;
        if (patrol >= 0 && patrol < patrols.Length)
        {
            route = new Vector2[patrols[patrol].Length];
            for (int i = 0; i < route.Length; i++) route[i] = RoadNetwork.TileCenter(patrols[patrol][i]);
        }
        float face = route != null && route.Length > 1 ? VisionCone.AngleOf(route[1] - route[0]) : -90f;
        cone = VisionCone.Attach(transform, sightRange, sightHalfAngle, face);
        mark = TownLookout.MakeMark(transform);
    }

    void Update()
    {
        if (!civ.CanBeBefriended)
        {
            if (cone != null) cone.Visible = false;
            if (mark != null) mark.text = "";
            Hunting = false;
            return;
        }

        bool alarm = military && BaseAlarm.Active;
        bool sees = alien != null && !alien.IsDead && !AlienPlayer.Disguised && !civ.IsCalm && cone.Sees(alien.transform.position);
        if (cooldown > 0f) cooldown -= Time.deltaTime;

        if (sees)
        {
            Vector2 to = alien.transform.position - transform.position;
            cone.TurnTowards(VisionCone.AngleOf(to), 400f);
            if (sr != null && Mathf.Abs(to.x) > 0.05f) sr.flipX = to.x < 0f;
            warning += Time.deltaTime;   // stop and stare
            if (warning >= (alarm ? 0.3f : warningTime))
            {
                if (military)
                {
                    if (!BaseAlarm.Active) BaseAlarm.Raise(alien.transform.position, "A soldier spotted the alien! BASE ALARM!");
                    else BaseAlarm.Spotted(alien.transform.position);
                    fireTimer -= Time.deltaTime;
                    if (fireTimer <= 0f) { Fire(to); fireTimer = fireInterval; }
                }
                else if (cooldown <= 0f) BlowWhistle();
            }
        }
        else
        {
            warning = Mathf.Max(0f, warning - Time.deltaTime * 2f);
            fireTimer = Mathf.Max(fireTimer, 0.6f);   // a moment to aim after spotting the alien again
            if (alarm) Hunt();
            else if (returning) ReturnToRoute();
            else Walk();
        }
        Hunting = alarm && !sees;

        bool red = alarm || cooldown > whistleCooldown - 1.5f;
        mark.text = red ? "!!" : warning > 0f ? "!" : "";
        mark.color = red ? new Color(1f, 0.3f, 0.2f) : new Color(1f, 0.85f, 0.3f);
        cone.color = red || warning > 0f ? new Color(1f, 0.3f, 0.25f, 0.2f)
                   : military ? new Color(1f, 0.95f, 0.65f, 0.12f) : new Color(0.45f, 0.7f, 1f, 0.12f);
    }

    void Fire(Vector2 to)
    {
        Dart.Spawn(transform.position, to.normalized, 5f, "A soldier's tranquilliser dart knocked the alien out!");
        DartsFired++;
    }

    void Walk()
    {
        if (route == null || route.Length < 2) return;
        Vector2 target = route[next];
        Step(target, walkSpeed, 240f);
        if (Vector2.Distance(transform.position, target) < 0.05f)
        {
            if (next + dir < 0 || next + dir >= route.Length) dir = -dir;   // turn around at the ends
            next += dir;
        }
    }

    // Run to where the alien was last seen, then look around
    void Hunt()
    {
        returning = true;
        repath -= Time.deltaTime;
        Vector2 goal = BaseAlarm.LastKnown;
        if (path == null || repath <= 0f || Vector2.Distance(goal, pathGoal) > 1.5f)
        {
            path = RoadNetwork.RouteTo(transform.position, goal);
            pathIndex = 0;
            pathGoal = goal;
            repath = 1f;
        }
        if (FollowPath(runSpeed)) cone.facing += 140f * Time.deltaTime;   // there: search around
    }

    void ReturnToRoute()
    {
        if (route == null || route.Length == 0) { returning = false; return; }
        if (path == null || Vector2.Distance(pathGoal, route[next]) > 0.1f)
        {
            path = RoadNetwork.RouteTo(transform.position, route[next]);
            pathIndex = 0;
            pathGoal = route[next];
        }
        if (FollowPath(walkSpeed)) { returning = false; path = null; }
    }

    // Move along the path; true when at the end
    bool FollowPath(float speed)
    {
        if (path == null || pathIndex >= path.Count) return true;
        Vector2 target = path[pathIndex];
        Step(target, speed, 400f);
        if (Vector2.Distance(transform.position, target) < 0.08f) pathIndex++;
        return pathIndex >= path.Count;
    }

    void Step(Vector2 target, float speed, float turn)
    {
        Vector2 d = target - (Vector2)transform.position;
        if (d.sqrMagnitude > 0.0001f)
        {
            cone.TurnTowards(VisionCone.AngleOf(d), turn);
            if (sr != null && Mathf.Abs(d.x) > 0.01f) sr.flipX = d.x < 0f;
        }
        transform.position = Vector2.MoveTowards(transform.position, target, speed * Time.deltaTime);
    }

    public void BlowWhistle()
    {
        warning = 0f;
        cooldown = whistleCooldown;
        WhistlesBlown++;
        Civilian.AlertAround(transform.position, whistleRadius);

        GameObject pulse = new GameObject("Whistle Pulse");
        pulse.transform.position = transform.position;
        pulse.transform.localScale = Vector3.one * whistleRadius * 2f;
        SpriteRenderer p = pulse.AddComponent<SpriteRenderer>(); Glow.Unlit(p);
        p.sprite = TownLookout.CircleSprite();
        p.sortingOrder = -1440;
        BeamEffect.FadeOut(p, new Color(0.4f, 0.65f, 1f, 0.4f), 0.6f);

        if (LevelManager.Instance != null) LevelManager.Instance.Flash("Police whistle!");
    }
}
