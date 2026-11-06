using UnityEngine;

// A police officer (Level 2+). Walks back and forth along a patrol route and never runs away.
// Takes 3 beam hits to befriend (Civilian.toughness). If the alien comes inside their
// watch circle, they blow the whistle after a short warning: everyone nearby panics.
// They don't hurt the alien yet - what deals damage is still to be decided.
[RequireComponent(typeof(Civilian))]
public class PolicePatrol : MonoBehaviour
{
    public int patrol;                 // index into the level's Patrols
    public float walkSpeed = 1.3f;
    public float watchRadius = 3.5f;
    public float whistleRadius = 6f;
    public float warningTime = 0.9f;
    public float whistleCooldown = 6f;

    public static int WhistlesBlown { get; private set; }

    private Civilian civ;
    private SpriteRenderer sr;
    private Transform alien;
    private Vector2[] route;
    private int next = 1, dir = 1;
    private float warning, cooldown;
    private SpriteRenderer circle;
    private TextMesh mark;

    void Awake()
    {
        civ = GetComponent<Civilian>();
        civ.holdsPosition = true;
        WhistlesBlown = 0;
    }

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        AlienPlayer player = FindFirstObjectByType<AlienPlayer>();
        if (player != null) alien = player.transform;

        Vector2Int[][] patrols = LevelMap.Current.Patrols;
        if (patrol >= 0 && patrol < patrols.Length)
        {
            route = new Vector2[patrols[patrol].Length];
            for (int i = 0; i < route.Length; i++) route[i] = RoadNetwork.TileCenter(patrols[patrol][i]);
        }
        circle = TownLookout.MakeCircle(transform, watchRadius);
        mark = TownLookout.MakeMark(transform);
    }

    void Update()
    {
        if (!civ.CanBeBefriended)
        {
            if (circle != null) circle.enabled = false;
            if (mark != null) mark.text = "";
            return;
        }

        bool seesAlien = alien != null && Vector2.Distance(alien.position, transform.position) < watchRadius;
        if (cooldown > 0f) cooldown -= Time.deltaTime;

        if (seesAlien && cooldown <= 0f && !civ.IsCalm)
        {
            warning += Time.deltaTime;   // stop and stare while deciding to whistle
            if (warning >= warningTime) BlowWhistle();
        }
        else
        {
            warning = Mathf.Max(0f, warning - Time.deltaTime * 2f);
            Walk();
        }

        bool alarmed = cooldown > whistleCooldown - 1.5f;
        mark.text = alarmed ? "!!" : warning > 0f ? "!" : "";
        mark.color = alarmed ? new Color(1f, 0.3f, 0.2f) : new Color(0.5f, 0.75f, 1f);
        circle.color = warning > 0f || alarmed ? new Color(1f, 0.35f, 0.25f, 0.22f) : new Color(0.4f, 0.65f, 1f, 0.12f);
    }

    void Walk()
    {
        if (route == null || route.Length < 2) return;
        Vector2 target = route[next];
        if (sr != null && Mathf.Abs(target.x - transform.position.x) > 0.01f)
            sr.flipX = target.x < transform.position.x;
        transform.position = Vector2.MoveTowards(transform.position, target, walkSpeed * Time.deltaTime);
        if (Vector2.Distance(transform.position, target) < 0.05f)
        {
            if (next + dir < 0 || next + dir >= route.Length) dir = -dir;   // turn around at the ends
            next += dir;
        }
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
        SpriteRenderer p = pulse.AddComponent<SpriteRenderer>();
        p.sprite = TownLookout.CircleSprite();
        p.sortingOrder = -1440;
        BeamEffect.FadeOut(p, new Color(0.4f, 0.65f, 1f, 0.4f), 0.6f);

        if (LevelManager.Instance != null) LevelManager.Instance.Flash("Police whistle!");
    }
}
