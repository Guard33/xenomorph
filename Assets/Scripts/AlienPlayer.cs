using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// The alien the player controls.
// W A S D move up, left, down, right. H J K L are the four skill slots.
// Level 1 only has H (Small Beam). After Level 1 the player picks a J skill on the
// Evolve screen: Wide Wave, Calm Hum or Friend Trap. K and L come later.
// The sprite is a placeholder (a dark villager) until our real alien art is ready.
[RequireComponent(typeof(Rigidbody2D))]
public class AlienPlayer : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 4f;        // tiles per second
    public float acceleration = 30f;    // how fast it reaches full speed
    public float deceleration = 40f;    // how fast it stops when you let go

    [Header("Health")]
    public int maxHp = 5;

    [Header("Skills (H J K L)")]
    public AlienSkill[] skills =
    {
        new AlienSkill { key = "H", name = "Small Beam", unlocked = true, cooldown = 0.6f },
        new AlienSkill { key = "J", name = "Locked" },
        new AlienSkill { key = "K", name = "Locked" },
        new AlienSkill { key = "L", name = "Locked" },
    };

    [Header("Small Beam")]
    public float beamRange = 3.5f;      // tiles
    public float beamWidth = 0.9f;      // tiles

    public int Hp { get; private set; }
    public bool IsDead => Hp <= 0;
    public Vector2 Facing { get; private set; } = Vector2.down;
    public float DistanceMoved { get; private set; }

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Vector2 input;
    private float hurtFlash;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        Hp = maxHp;
    }

    void Start()
    {
        // J is the skill picked on the Evolve screen after Level 1.
        // Testing Level 2 straight from the editor (nothing picked) uses the level's default.
        string j = UpgradePath.PickedId(0);
        if (j == null && LevelInfo.Instance != null && LevelInfo.Instance.level >= 2) j = LevelInfo.Instance.defaultJSkill;
        if (j != null) SetSkill(1, j);
    }

    // Put a skill on a key (by its UpgradePath id)
    public void SetSkill(int slot, string id)
    {
        AlienSkill s = skills[slot];
        UpgradePath.Option o = UpgradePath.Find(id);
        s.id = id;
        s.name = o != null ? o.name : id;
        s.unlocked = true;
        s.cooldown = id == "wide_wave" ? 1.2f : id == "calm_hum" ? 8f : id == "friend_trap" ? 6f : 1f;
        s.timer = 0f;
    }

    void Update()
    {
        if (IsDead) { input = Vector2.zero; return; }

        input = ReadMove();
        if (input.sqrMagnitude > 1f)
            input.Normalize();
        if (input.sqrMagnitude > 0.01f)
            Facing = input.normalized;

        if (sr != null && Mathf.Abs(input.x) > 0.01f)
            sr.flipX = input.x < 0f;

        for (int i = 0; i < skills.Length; i++)
        {
            skills[i].Tick(Time.deltaTime);
            if (SkillPressed(i))
                UseSkill(i);
        }

        if (hurtFlash > 0f)
        {
            hurtFlash -= Time.deltaTime;
            if (sr != null) sr.color = hurtFlash > 0f ? new Color(1f, 0.4f, 0.4f) : Color.white;
        }
    }

    void FixedUpdate()
    {
        Vector2 target = input * moveSpeed;
        float rate = input.sqrMagnitude > 0.01f ? acceleration : deceleration;
        rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, target, rate * Time.fixedDeltaTime);
        DistanceMoved += rb.linearVelocity.magnitude * Time.fixedDeltaTime;
    }

    // ------------------------------------------------------------------ skills
    public void UseSkill(int slot)
    {
        AlienSkill s = skills[slot];
        if (!s.unlocked)
        {
            if (LevelManager.Instance != null) LevelManager.Instance.Flash(s.key + " unlocks when you evolve.");
            return;
        }
        if (!s.Ready) return;
        s.Use();

        if (slot == 0) FireSmallBeam();
        else if (s.id == "wide_wave") FireWideWave();
        else if (s.id == "calm_hum") CalmHum();
        else if (s.id == "friend_trap") FriendTrap.Place(transform.position);
    }

    // A short straight beam in the direction the alien is facing.
    // The closest person inside the beam is hit (most people are befriended at once).
    public Civilian FireSmallBeam()
    {
        Vector2 origin = transform.position;
        BeamEffect.Spawn(origin, Facing, beamRange, beamWidth, new Color(0.45f, 1f, 0.55f, 0.85f));
        Civilian hit = FirstInLine(origin, Facing, beamRange, beamWidth);
        if (hit != null) hit.Hit();
        return hit;
    }

    // J option: Wide Wave (Danesh) - 3 beams in a 40 degree fan, each hits the first person it touches
    public int FireWideWave()
    {
        Vector2 origin = transform.position;
        int hits = 0;
        foreach (float angle in new[] { -20f, 0f, 20f })
        {
            Vector2 dir = Quaternion.Euler(0f, 0f, angle) * Facing;
            BeamEffect.Spawn(origin, dir, beamRange, beamWidth * 0.6f, new Color(0.96f, 0.5f, 0.75f, 0.85f));
            Civilian c = FirstInLine(origin, dir, beamRange, beamWidth * 0.7f);
            if (c != null) { c.Hit(); hits++; }
        }
        return hits;
    }

    // J option: Calm Hum (Danesh) - everyone within 3 tiles stops panicking for 4 seconds.
    // Runners stop and wander home; lookouts and police nearby don't raise the alarm meanwhile.
    public int CalmHum()
    {
        const float radius = 3f, seconds = 4f;
        int calmed = 0;
        foreach (Civilian c in Civilian.All)
            if (c.CanBeBefriended && Vector2.Distance(c.transform.position, transform.position) < radius) { c.CalmDown(seconds); calmed++; }

        GameObject pulse = new GameObject("Calm Hum");
        pulse.transform.position = transform.position;
        pulse.transform.localScale = Vector3.one * radius * 2f;
        SpriteRenderer p = pulse.AddComponent<SpriteRenderer>(); Glow.Unlit(p);
        p.sprite = TownLookout.CircleSprite();
        p.sortingOrder = -1440;
        BeamEffect.FadeOut(p, new Color(0.45f, 0.7f, 1f, 0.45f), 0.8f);
        return calmed;
    }

    static bool InLine(Vector2 origin, Vector2 dir, float range, float width, Vector2 p)
    {
        Vector2 to = p - origin;
        float along = Vector2.Dot(to, dir);
        float side = Mathf.Abs(to.x * dir.y - to.y * dir.x);
        return along >= -0.3f && along <= range && side <= width * 0.5f + 0.3f;
    }

    static Civilian FirstInLine(Vector2 origin, Vector2 dir, float range, float width)
    {
        Civilian hit = null;
        float best = float.MaxValue;
        foreach (Civilian c in Civilian.All)
        {
            if (!c.CanBeBefriended || !InLine(origin, dir, range, width, c.transform.position)) continue;
            float along = Vector2.Dot((Vector2)c.transform.position - origin, dir);
            if (along < best) { best = along; hit = c; }
        }
        return hit;
    }

    // ------------------------------------------------------------------ health
    public void TakeDamage(int amount)
    {
        if (IsDead || amount <= 0) return;
        bool canDie = LevelManager.Instance == null || LevelManager.Instance.alienCanDie;
        Hp = Mathf.Max(canDie ? 0 : 1, Hp - amount);
        hurtFlash = 0.2f;
        if (IsDead)
        {
            rb.linearVelocity = Vector2.zero;
            if (LevelManager.Instance != null) LevelManager.Instance.AlienDied();
        }
    }

    public void Heal(int amount)
    {
        if (IsDead) return;
        Hp = Mathf.Min(maxHp, Hp + amount);
    }

    // ------------------------------------------------------------------ input
    Vector2 ReadMove()
    {
        Vector2 v = Vector2.zero;
#if ENABLE_INPUT_SYSTEM
        Keyboard kb = Keyboard.current;
        if (kb == null) return v;
        if (kb.aKey.isPressed) v.x -= 1f;
        if (kb.dKey.isPressed) v.x += 1f;
        if (kb.sKey.isPressed) v.y -= 1f;
        if (kb.wKey.isPressed) v.y += 1f;
#else
        if (Input.GetKey(KeyCode.A)) v.x -= 1f;
        if (Input.GetKey(KeyCode.D)) v.x += 1f;
        if (Input.GetKey(KeyCode.S)) v.y -= 1f;
        if (Input.GetKey(KeyCode.W)) v.y += 1f;
#endif
        return v;
    }

    bool SkillPressed(int slot)
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard kb = Keyboard.current;
        if (kb == null) return false;
        switch (slot)
        {
            case 0: return kb.hKey.wasPressedThisFrame;
            case 1: return kb.jKey.wasPressedThisFrame;
            case 2: return kb.kKey.wasPressedThisFrame;
            case 3: return kb.lKey.wasPressedThisFrame;
        }
        return false;
#else
        KeyCode[] keys = { KeyCode.H, KeyCode.J, KeyCode.K, KeyCode.L };
        return slot < keys.Length && Input.GetKeyDown(keys[slot]);
#endif
    }
}

// One skill slot on H, J, K or L.
[System.Serializable]
public class AlienSkill
{
    public string key;
    public string id;     // which skill this is (UpgradePath id); H is always the Small Beam
    public string name;
    public bool unlocked;
    public float cooldown;

    [System.NonSerialized] public float timer;

    public bool Ready => timer <= 0f;
    public float Cooldown01 => cooldown > 0f ? Mathf.Clamp01(timer / cooldown) : 0f;

    public void Tick(float dt) { if (timer > 0f) timer -= dt; }
    public void Use() { timer = cooldown; }
}
