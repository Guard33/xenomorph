using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// The alien the player controls.
// W A S D move up, left, down, right. H J K L are the four skill slots.
// Level 1 only has H (Small Beam). After Level 1 the player picks a J skill on the
// Evolve screen (Wide Wave, Calm Hum or Friend Trap), after Level 2 a K skill
// (Long Beam, Disguise or Psi Shield). L comes later.
// Every evolution also makes the alien stronger: +1 max HP, a longer beam and a bit more speed.
// Stage sprites: put the frames for stage N in Assets/Resources/Alien/stageN/ and that level
// uses them automatically (e.g. stage2 for Level 2). Until then the slime is drawn a little bigger.
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

    [Header("Sounds (Talia)")]
    public AudioSource beamAudio;       // every beam
    public AudioSource walkingAudio;    // loops while moving

    [Header("Disguise")]
    public Sprite disguiseSprite;       // what the alien looks like while disguised (a soldier)

    public int Hp { get; private set; }
    public int Stage { get; private set; } = 1;

    // Disguise: nobody notices the alien. Psi Shield: no damage.
    public static bool Disguised { get; private set; }
    public bool Shielded => shieldTimer > 0f;

    // While the Psi Shield (psionic field) is up, every other skill does +1 damage
    static AlienPlayer current;
    public static int PowerBonus => current != null && current.Shielded ? 1 : 0;
    public const float ShieldSeconds = 10f;
    public bool IsDead => Hp <= 0;
    public Vector2 Facing { get; private set; } = Vector2.down;
    public float DistanceMoved { get; private set; }

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Vector2 input;
    private float hurtFlash;
    private float slowTimer;            // tranquilliser darts slow the alien for a moment
    private float disguiseTimer, shieldTimer;
    private SpriteFlipbook flipbook;
    private Sprite normalSprite;
    private SpriteRenderer shieldBubble;

    void Awake()
    {
        current = this;
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        flipbook = GetComponent<SpriteFlipbook>();
        Disguised = false;
        Hp = maxHp;
    }

    void Start()
    {
        if (GetComponent<AlwaysVisible>() == null) gameObject.AddComponent<AlwaysVisible>();   // never fully hidden behind a structure
        // J is the skill picked on the Evolve screen after Level 1.
        // Testing Level 2 straight from the editor (nothing picked) uses the level's default.
        int level = LevelInfo.Instance != null ? LevelInfo.Instance.level : 1;
        string j = UpgradePath.PickedId(0);
        if (j == null && level >= 2) j = LevelInfo.Instance.defaultJSkill;
        if (j != null) SetSkill(1, j);
        string k = UpgradePath.PickedId(1);
        if (k == null && level >= 3) k = LevelInfo.Instance.defaultKSkill;
        if (k != null) SetSkill(2, k);

        // Each level is one evolution stage further: the alien grows stronger
        Stage = Mathf.Max(1, level);
        int ups = Stage - 1;
        maxHp += ups;
        Hp = maxHp;
        beamRange += 0.5f * ups;
        moveSpeed *= 1f + 0.06f * ups;
        UseStageSprites();
    }

    // Frames for this stage from Resources/Alien/stageN (falls back to the slime, drawn bigger)
    void UseStageSprites()
    {
        Sprite[] frames = null;
        for (int st = Stage; st >= 2 && (frames == null || frames.Length == 0); st--)
            frames = Resources.LoadAll<Sprite>("Alien/stage" + st);
        if (frames != null && frames.Length > 0)
        {
            System.Array.Sort(frames, (a, b) => string.CompareOrdinal(a.name, b.name));
            if (flipbook != null) flipbook.frames = frames;
            if (sr != null) sr.sprite = frames[0];
        }
        else if (Stage > 1)
            transform.localScale = Vector3.one * (1f + 0.12f * (Stage - 1));
        normalSprite = sr != null ? sr.sprite : null;
    }

    // Put a skill on a key (by its UpgradePath id)
    public void SetSkill(int slot, string id)
    {
        AlienSkill s = skills[slot];
        UpgradePath.Option o = UpgradePath.Find(id);
        s.id = id;
        s.name = o != null ? o.name : id;
        s.unlocked = true;
        s.cooldown = id == "wide_wave" ? 1.2f : id == "calm_hum" ? 8f : id == "friend_trap" ? 6f
                   : id == "long_beam" ? 1.5f : id == "disguise" ? 12f : id == "psi_shield" ? 20f : 1f;
        s.timer = 0f;
    }

    void Update()
    {
        if (IsDead || PauseMenu.IsOpen) { input = Vector2.zero; return; }

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

        if (slowTimer > 0f) slowTimer -= Time.deltaTime;
        if (hurtFlash > 0f)
        {
            hurtFlash -= Time.deltaTime;
            if (sr != null) sr.color = hurtFlash > 0f ? new Color(1f, 0.4f, 0.4f) : Color.white;
        }

        if (disguiseTimer > 0f)
        {
            disguiseTimer -= Time.deltaTime;
            if (disguiseTimer <= 0f) EndDisguise();
        }
        if (shieldTimer > 0f)
        {
            shieldTimer -= Time.deltaTime;
            if (shieldBubble != null)
            {
                // blinks during the last second so the player knows it is about to drop
                float a = shieldTimer < 1f && Mathf.Repeat(shieldTimer * 8f, 1f) < 0.5f ? 0.15f : 0.45f;
                shieldBubble.color = new Color(0.75f, 0.5f, 1f, shieldTimer > 0f ? a : 0f);
            }
        }
    }

    void FixedUpdate()
    {
        Vector2 target = input * moveSpeed * (slowTimer > 0f ? 0.55f : 1f);
        float rate = input.sqrMagnitude > 0.01f ? acceleration : deceleration;
        rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, target, rate * Time.fixedDeltaTime);
        DistanceMoved += rb.linearVelocity.magnitude * Time.fixedDeltaTime;
        if (walkingAudio != null)
        {
            bool moving = rb.linearVelocity.magnitude > 0.1f && !IsDead;
            if (moving && !walkingAudio.isPlaying) walkingAudio.Play();
            else if (!moving && walkingAudio.isPlaying) walkingAudio.Stop();
        }
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

        // Using any other skill gives the disguise away
        if (Disguised && s.id != "disguise") EndDisguise();

        if (slot == 0) FireSmallBeam();
        else if (s.id == "wide_wave") FireWideWave();
        else if (s.id == "calm_hum") CalmHum();
        else if (s.id == "friend_trap") FriendTrap.Place(transform.position);
        else if (s.id == "long_beam") FireLongBeam();
        else if (s.id == "disguise") StartDisguise();
        else if (s.id == "psi_shield") RaiseShield();
    }

    // K option: Long Beam (Danesh) - double range, goes through people and hits everyone in the line
    public int FireLongBeam()
    {
        if (beamAudio != null) beamAudio.Play();
        Vector2 origin = transform.position;
        float range = beamRange * 2f;
        BeamEffect.Spawn(origin, Facing, range, beamWidth, new Color(0.4f, 0.9f, 1f, 0.9f));
        int hits = 0;
        foreach (Civilian c in Civilian.All.ToArray())
            if (c.CanBeBefriended && InLine(origin, Facing, range, beamWidth, c.transform.position)) { c.Hit(2 + PowerBonus); hits++; }
        return hits;
    }

    // K option: Disguise (Danesh) - look like a soldier for 5 seconds: nobody notices the alien,
    // guards don't raise the alarm and the General doesn't shoot. Using another skill ends it.
    public void StartDisguise()
    {
        Disguised = true;
        disguiseTimer = 5f;
        if (flipbook != null) flipbook.enabled = false;
        if (sr != null && disguiseSprite != null) sr.sprite = disguiseSprite;
        Puff(new Color(0.75f, 0.85f, 0.6f, 0.5f));
        if (LevelManager.Instance != null) LevelManager.Instance.Flash("Disguised!", 1.2f);
    }

    public void EndDisguise()
    {
        if (!Disguised) return;
        Disguised = false;
        disguiseTimer = 0f;
        if (flipbook != null) flipbook.enabled = true;
        if (sr != null && normalSprite != null) sr.sprite = normalSprite;
        Puff(new Color(0.75f, 0.85f, 0.6f, 0.5f));
    }

    // K option: Psi Shield (new) - a psionic field that blocks all damage for 10 seconds
    // and gives every other skill +1 damage while it is up (20 s cooldown)
    public void RaiseShield()
    {
        shieldTimer = ShieldSeconds;
        if (shieldBubble == null)
        {
            GameObject b = new GameObject("Psi Shield");
            b.transform.SetParent(transform, false);
            b.transform.localScale = Vector3.one * 1.5f / Mathf.Max(0.01f, transform.localScale.x);
            shieldBubble = b.AddComponent<SpriteRenderer>(); Glow.Unlit(shieldBubble);
            shieldBubble.sprite = TownLookout.CircleSprite();
            shieldBubble.sortingOrder = 29000;
        }
        shieldBubble.color = new Color(0.75f, 0.5f, 1f, 0.45f);
    }

    void Puff(Color c)
    {
        GameObject pulse = new GameObject("Puff");
        pulse.transform.position = transform.position;
        pulse.transform.localScale = Vector3.one * 1.6f;
        SpriteRenderer p = pulse.AddComponent<SpriteRenderer>(); Glow.Unlit(p);
        p.sprite = TownLookout.CircleSprite();
        p.sortingOrder = 29000;
        BeamEffect.FadeOut(p, c, 0.4f);
    }

    // A short straight beam in the direction the alien is facing.
    // The closest person inside the beam is hit (most people are befriended at once).
    public Civilian FireSmallBeam()
    {
        if (beamAudio != null) beamAudio.Play();
        Vector2 origin = transform.position;
        BeamEffect.Spawn(origin, Facing, beamRange, beamWidth, new Color(0.45f, 1f, 0.55f, 0.85f));
        Civilian hit = FirstInLine(origin, Facing, beamRange, beamWidth);
        if (hit != null) hit.Hit(1 + PowerBonus);
        return hit;
    }

    // J option: Wide Wave (Danesh) - 3 beams in a 40 degree fan, each hits the first person it touches
    public int FireWideWave()
    {
        if (beamAudio != null) beamAudio.Play();
        Vector2 origin = transform.position;
        int hits = 0;
        foreach (float angle in new[] { -20f, 0f, 20f })
        {
            Vector2 dir = Quaternion.Euler(0f, 0f, angle) * Facing;
            BeamEffect.Spawn(origin, dir, beamRange, beamWidth * 0.6f, new Color(0.96f, 0.5f, 0.75f, 0.85f));
            Civilian c = FirstInLine(origin, dir, beamRange, beamWidth * 0.7f);
            if (c != null) { c.Hit(1 + PowerBonus); hits++; }
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
    // Hit by a tranquilliser dart: slower for a moment
    public void Tranquilize(float seconds) { slowTimer = Mathf.Max(slowTimer, seconds); }
    public bool Slowed => slowTimer > 0f;

    // Returns false if the hit did nothing (already dead, or blocked by the Psi Shield)
    public bool TakeDamage(int amount, string reason = null)
    {
        if (IsDead || amount <= 0) return false;
        if (Shielded) return false;
        bool canDie = LevelManager.Instance == null || LevelManager.Instance.alienCanDie;
        Hp = Mathf.Max(canDie ? 0 : 1, Hp - amount);
        hurtFlash = 0.2f;
        if (IsDead)
        {
            rb.linearVelocity = Vector2.zero;
            if (LevelManager.Instance != null) LevelManager.Instance.AlienDied(reason);
        }
        return true;
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
