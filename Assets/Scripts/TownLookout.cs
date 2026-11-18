using UnityEngine;

// Someone who keeps watch and never runs away.
// Town lookouts (neighbourhood watch, councillor): if the alien stays inside their watch circle
// they raise the alarm after a short warning and everyone nearby runs. They don't hurt the alien.
// Sentries (military = true, army levels): they stand guard and scan around with a vision cone.
// When they spot the alien they raise the BASE ALARM and fire tranquilliser darts.
[RequireComponent(typeof(Civilian))]
public class TownLookout : MonoBehaviour
{
    public float watchRadius = 4f;     // tiles; the faint circle on the ground
    public float alarmRadius = 7f;     // tiles; who hears the alarm
    public float warningTime = 1.2f;   // seconds of "!" before the alarm goes off
    public float alarmCooldown = 6f;
    public AudioSource alarm;          // sounds (Talia)
    public AudioSource gasp;
    public bool military;              // an army sentry: vision cone, base alarm, tranquilliser darts
    public float sightRange = 6f;      // sentry cone length (tiles)
    public float fireInterval = 2.4f;

    public static int AlarmsRung { get; private set; }

    private Civilian civ;
    private Transform alien;
    private float warning;
    private float cooldown;
    private SpriteRenderer circle;
    private VisionCone cone;
    private AlienPlayer player;
    private float fireTimer = 0.6f;
    private TextMesh mark;

    void Awake()
    {
        civ = GetComponent<Civilian>();
        civ.holdsPosition = true;
        AlarmsRung = 0;
    }

    void Start()
    {
        player = FindFirstObjectByType<AlienPlayer>();
        if (player != null) alien = player.transform;

        // Sentries each start scanning in a different direction
        if (military) cone = VisionCone.Attach(transform, sightRange, 40f, Mathf.Repeat(transform.position.x * 47f + transform.position.y * 23f, 360f));
        else circle = MakeCircle(transform, watchRadius);
        mark = MakeMark(transform);
    }

    public bool AlienInView =>
        alien != null && !AlienPlayer.Disguised && (military
            ? cone != null && !player.IsDead && cone.Sees(alien.position)
            : Vector2.Distance(alien.position, transform.position) < watchRadius);

    void Update()
    {
        if (!civ.CanBeBefriended)
        {
            if (circle != null) circle.enabled = false;
            if (cone != null) cone.Visible = false;
            if (mark != null) mark.text = "";
            return;
        }
        if (military) { SentryUpdate(); return; }

        if (cooldown > 0f) cooldown -= Time.deltaTime;

        if (AlienInView && cooldown <= 0f && !civ.IsCalm)
        {
            warning += Time.deltaTime;
            if (warning >= warningTime) RingAlarm();
        }
        else
        {
            warning = Mathf.Max(0f, warning - Time.deltaTime * 2f);
        }

        bool alarmed = cooldown > alarmCooldown - 1.5f;
        mark.text = alarmed ? "!!" : warning > 0f ? "!" : "";
        mark.color = alarmed ? new Color(1f, 0.3f, 0.2f) : new Color(1f, 0.85f, 0.2f);
        circle.color = warning > 0f || alarmed ? new Color(1f, 0.35f, 0.25f, 0.28f) : new Color(1f, 0.9f, 0.4f, 0.14f);
    }

    // Army sentry: scan around; on sight raise the base alarm and fire darts
    void SentryUpdate()
    {
        bool alarm = BaseAlarm.Active;
        if (AlienInView && !civ.IsCalm)
        {
            Vector2 to = alien.position - transform.position;
            cone.TurnTowards(VisionCone.AngleOf(to), 400f);
            warning += Time.deltaTime;
            if (warning >= (alarm ? 0.3f : 0.7f))
            {
                if (!BaseAlarm.Active) { AlarmsRung++; BaseAlarm.Raise(alien.position, "A sentry spotted the alien! BASE ALARM!"); if (gasp != null) gasp.Play(); }
                else BaseAlarm.Spotted(alien.position);
                fireTimer -= Time.deltaTime;
                if (fireTimer <= 0f)
                {
                    Dart.Spawn(transform.position, to.normalized, 5f, "A sentry's tranquilliser dart knocked the alien out!");
                    fireTimer = fireInterval;
                }
            }
        }
        else
        {
            warning = Mathf.Max(0f, warning - Time.deltaTime * 2f);
            fireTimer = Mathf.Max(fireTimer, 0.6f);
            // Scan: during the alarm, look toward where the alien was last seen
            if (alarm) cone.TurnTowards(VisionCone.AngleOf(BaseAlarm.LastKnown - (Vector2)transform.position) + Mathf.Sin(Time.time * 2f) * 50f, 150f);
            else cone.facing += 35f * Time.deltaTime;
        }
        mark.text = alarm ? "!!" : warning > 0f ? "!" : "";
        mark.color = alarm ? new Color(1f, 0.3f, 0.2f) : new Color(1f, 0.85f, 0.2f);
        cone.color = alarm || warning > 0f ? new Color(1f, 0.3f, 0.25f, 0.2f) : new Color(1f, 0.95f, 0.65f, 0.12f);
    }

    public void RingAlarm()
    {
        warning = 0f;
        cooldown = alarmCooldown;
        AlarmsRung++;
        Civilian.AlertAround(transform.position, alarmRadius);
        if (gasp != null) gasp.Play();
        if (alarm != null) alarm.Play();

        // A red pulse showing how far the alarm reached
        GameObject pulse = new GameObject("Alarm Pulse");
        pulse.transform.position = transform.position;
        pulse.transform.localScale = Vector3.one * alarmRadius * 2f;
        SpriteRenderer sr = pulse.AddComponent<SpriteRenderer>(); Glow.Unlit(sr);
        sr.sprite = CircleSprite();
        sr.sortingOrder = -1440;
        BeamEffect.FadeOut(sr, new Color(1f, 0.25f, 0.2f, 0.45f), 0.6f);

        if (LevelManager.Instance != null) LevelManager.Instance.Flash("The " + DisplayName() + " raised the alarm!");
    }

    string DisplayName()
    {
        string n = civ.name;
        int a = n.IndexOf('('), b = n.IndexOf(')');
        return a >= 0 && b > a ? n.Substring(a + 1, b - a - 1).Replace('-', ' ') : "lookout";
    }

    // Shared with PolicePatrol: the faint watch circle and the "!" above the head
    public static SpriteRenderer MakeCircle(Transform parent, float radius)
    {
        GameObject go = new GameObject("Watch Circle");
        go.transform.SetParent(parent, false);
        go.transform.localScale = Vector3.one * radius * 2f;
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>(); Glow.Unlit(sr);
        sr.sprite = CircleSprite();
        sr.sortingOrder = -1450;
        return sr;
    }

    public static TextMesh MakeMark(Transform parent)
    {
        GameObject go = new GameObject("Alert Mark");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = new Vector3(0f, 1.1f, 0f);
        TextMesh t = go.AddComponent<TextMesh>();
        t.anchor = TextAnchor.MiddleCenter;
        t.alignment = TextAlignment.Center;
        t.fontSize = 96;
        t.characterSize = 0.13f;
        t.fontStyle = FontStyle.Bold;
        Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.font = f;
        MeshRenderer mr = go.GetComponent<MeshRenderer>();
        mr.sharedMaterial = f.material;
        mr.sortingOrder = 30001;
        return t;
    }

    static Sprite circleSprite;

    // A soft-edged disc, 1 world unit across
    public static Sprite CircleSprite()
    {
        if (circleSprite != null) return circleSprite;
        const int size = 64;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float r = size / 2f;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(r, r)) / r;
                float a = d > 1f ? 0f : d > 0.92f ? 1f : 0.55f;
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        tex.Apply();
        circleSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        return circleSprite;
    }
}
