using UnityEngine;
using UnityEngine.Rendering.Universal;

// The searchlight on a watchtower. It sweeps back and forth over the base, and if the alien is
// caught in the beam for a moment it raises the BASE ALARM (army levels). It then locks on and
// follows the alien (turning red) until it loses it. Disguise fools it; buildings block it.
public class Searchlight : MonoBehaviour
{
    public float facing = -90f;    // degrees, the middle of the sweep (0 = right, 90 = up)
    public float sweep = 50f;      // degrees either side
    public float speed = 0.2f;     // sweeps per second
    public float phase;
    public float range = 9.5f;     // tiles the beam reaches
    public float beamHalf = 11f;   // degrees either side of the beam's centre
    public float spotTime = 0.4f;  // seconds in the beam before the alarm

    public static int Spots { get; private set; }

    private Light2D spot;
    private AlienPlayer alien;
    private float angle, inBeam, locked;
    private Color normal;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetCount() { Spots = 0; }

    void Start()
    {
        spot = GetComponent<Light2D>();
        if (spot != null) normal = spot.color;
        alien = FindFirstObjectByType<AlienPlayer>();
        angle = facing;
    }

    public float Angle => angle;
    public bool Locked => locked > 0f;

    void Update()
    {
        bool alarm = BaseAlarm.Active;
        bool caught = alien != null && !alien.IsDead && !AlienPlayer.Disguised && InBeam(alien.transform.position);

        if (locked > 0f && alien != null)
        {
            // Follow the alien while it is in the light
            angle = Mathf.MoveTowardsAngle(angle, VisionCone.AngleOf((Vector2)alien.transform.position - (Vector2)transform.position), 90f * Time.deltaTime);
            locked = caught ? 2f : locked - Time.deltaTime;
        }
        else
        {
            float s = alarm ? speed * 1.8f : speed;
            float target = facing + Mathf.Sin((Time.time * s + phase) * Mathf.PI * 2f) * sweep;
            angle = Mathf.MoveTowardsAngle(angle, target, 160f * Time.deltaTime);
        }

        if (caught)
        {
            inBeam += Time.deltaTime;
            if (inBeam >= spotTime)
            {
                if (!BaseAlarm.Active) { Spots++; BaseAlarm.Raise(alien.transform.position, "A watchtower searchlight spotted the alien! BASE ALARM!"); }
                else BaseAlarm.Spotted(alien.transform.position);
                locked = 2f;
            }
        }
        else inBeam = 0f;

        // Light2D spot lights point along their local up axis
        transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
        if (spot != null) spot.color = locked > 0f ? new Color(1f, 0.35f, 0.3f) : normal;
    }

    bool InBeam(Vector2 p)
    {
        Vector2 d = p - (Vector2)transform.position;
        if (d.magnitude > range || d.magnitude < 0.5f) return false;
        Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
        return Vector2.Angle(dir, d) < beamHalf && RoadNetwork.LineOfSight(transform.position, p);
    }
}
