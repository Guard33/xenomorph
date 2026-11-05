using UnityEngine;

// J option: Friend Trap. The alien drops a trap where it stands. When anyone walks near it,
// it goes off and hits everyone around it (most people are befriended; police take one hit).
// Only one trap at a time - placing a new one removes the old one.
public class FriendTrap : MonoBehaviour
{
    public float armDelay = 0.4f;       // seconds before it can go off
    public float triggerRadius = 1.3f;  // someone this close sets it off
    public float effectRadius = 3f;     // everyone this close is hit
    public float lifetime = 25f;

    public static FriendTrap Active { get; private set; }

    private float age;
    private SpriteRenderer ring, core;

    public static FriendTrap Place(Vector2 at)
    {
        if (Active != null) Destroy(Active.gameObject);
        GameObject go = new GameObject("Friend Trap");
        go.transform.position = at;
        Active = go.AddComponent<FriendTrap>();
        return Active;
    }

    void Start()
    {
        ring = MakeDisc("Reach", effectRadius * 2f, -1430);
        core = MakeDisc("Trap", 0.7f, -1420);
    }

    SpriteRenderer MakeDisc(string name, float size, int order)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(transform, false);
        go.transform.localScale = Vector3.one * size;
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = TownLookout.CircleSprite();
        sr.sortingOrder = order;
        return sr;
    }

    void Update()
    {
        age += Time.deltaTime;
        float pulse = 0.5f + 0.5f * Mathf.Sin(age * 6f);
        ring.color = new Color(0.96f, 0.45f, 0.68f, 0.08f + 0.06f * pulse);
        core.color = new Color(0.96f, 0.45f, 0.68f, age < armDelay ? 0.4f : 0.7f + 0.3f * pulse);

        if (age > lifetime) { Destroy(gameObject); return; }
        if (age < armDelay) return;

        foreach (Civilian c in Civilian.All)
            if (c.CanBeBefriended && Vector2.Distance(c.transform.position, transform.position) < triggerRadius)
            {
                Spring();
                return;
            }
    }

    public int Spring()
    {
        int hits = 0;
        foreach (Civilian c in Civilian.All.ToArray())
            if (c.CanBeBefriended && Vector2.Distance(c.transform.position, transform.position) < effectRadius) { c.Hit(); hits++; }

        GameObject pulse = new GameObject("Trap Pulse");
        pulse.transform.position = transform.position;
        pulse.transform.localScale = Vector3.one * effectRadius * 2f;
        SpriteRenderer p = pulse.AddComponent<SpriteRenderer>();
        p.sprite = TownLookout.CircleSprite();
        p.sortingOrder = -1440;
        BeamEffect.FadeOut(p, new Color(0.96f, 0.45f, 0.68f, 0.5f), 0.6f);

        if (LevelManager.Instance != null && hits > 0) LevelManager.Instance.Flash("Trap! " + hits + " hit");
        Destroy(gameObject);
        return hits;
    }

    void OnDestroy()
    {
        if (Active == this) Active = null;
    }
}
