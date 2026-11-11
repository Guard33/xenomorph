using UnityEngine;

// A townsperson who keeps watch (Town Crier, Mayor). They never run away.
// If the alien stays inside their watch circle, they ring the alarm after a short
// warning, and everyone nearby runs for the exits. Beam them first to keep the town calm.
// They don't hurt the alien - Level 1 is the tutorial.
[RequireComponent(typeof(Civilian))]
public class TownLookout : MonoBehaviour
{
    public float watchRadius = 4f;     // tiles; the faint circle on the ground
    public float alarmRadius = 7f;     // tiles; who hears the alarm
    public float warningTime = 1.2f;   // seconds of "!" before the alarm goes off
    public float alarmCooldown = 6f;
    public AudioSource alarm;
    public AudioSource gasp;

    public static int AlarmsRung { get; private set; }

    private Civilian civ;
    private Transform alien;
    private float warning;
    private float cooldown;
    private SpriteRenderer circle;
    private TextMesh mark;

    void Awake()
    {
        civ = GetComponent<Civilian>();
        civ.holdsPosition = true;
        AlarmsRung = 0;
    }

    void Start()
    {
        AlienPlayer player = FindFirstObjectByType<AlienPlayer>();
        if (player != null) alien = player.transform;

        circle = MakeCircle(transform, watchRadius);
        mark = MakeMark(transform);
    }

    public bool AlienInView =>
        alien != null && Vector2.Distance(alien.position, transform.position) < watchRadius;

    void Update()
    {
        if (!civ.CanBeBefriended)
        {
            if (circle != null) circle.enabled = false;
            if (mark != null) mark.text = "";
            return;
        }

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

    public void RingAlarm()
    {
        warning = 0f;
        cooldown = alarmCooldown;
        AlarmsRung++;
        Civilian.AlertAround(transform.position, alarmRadius);
        if (gasp != null)
            gasp.Play();
        if (alarm != null)
            alarm.Play();


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
