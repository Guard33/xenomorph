using UnityEngine;

// A guard's field of view: a soft cone on the ground in front of them (like a flashlight).
// Soldiers, sentries, police and the General see the alien only inside their cone,
// and buildings and vehicles block the view. Town lookouts keep their watch circle.
public class VisionCone : MonoBehaviour
{
    public float range = 5f;        // tiles
    public float halfAngle = 35f;   // degrees either side of facing
    public float facing = -90f;     // degrees (0 = right, 90 = up)
    public Color color = new Color(1f, 0.95f, 0.65f, 0.12f);

    private Transform owner;
    private SpriteRenderer sr;
    const float BakedHalf = 35f;    // the cone sprite is drawn with this angle, then stretched

    public static VisionCone Attach(Transform owner, float range, float halfAngle, float facing)
    {
        GameObject go = new GameObject("Vision Cone (" + owner.name + ")");
        VisionCone v = go.AddComponent<VisionCone>();
        v.owner = owner;
        v.range = range;
        v.halfAngle = halfAngle;
        v.facing = facing;
        v.sr = go.AddComponent<SpriteRenderer>(); Glow.Unlit(v.sr);
        v.sr.sprite = ConeSprite();
        v.sr.sortingOrder = -1450;
        v.Place();
        return v;
    }

    void LateUpdate()
    {
        if (owner == null) { Destroy(gameObject); return; }
        Place();
    }

    void Place()
    {
        transform.position = owner.position;
        transform.rotation = Quaternion.Euler(0f, 0f, facing);
        float k = Mathf.Tan(halfAngle * Mathf.Deg2Rad) / Mathf.Tan(BakedHalf * Mathf.Deg2Rad);
        transform.localScale = new Vector3(range, range * k, 1f);
        sr.color = color;
    }

    public bool Visible { set { if (sr != null) sr.enabled = value; } }

    public Vector2 Dir => new Vector2(Mathf.Cos(facing * Mathf.Deg2Rad), Mathf.Sin(facing * Mathf.Deg2Rad));

    public static float AngleOf(Vector2 v) => Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;

    public void TurnTowards(float degrees, float degreesPerSecond)
    {
        facing = Mathf.MoveTowardsAngle(facing, degrees, degreesPerSecond * Time.deltaTime);
    }

    // Is this point inside the cone, with nothing solid in the way?
    public bool Sees(Vector2 target)
    {
        if (owner == null) return false;
        Vector2 from = owner.position;
        Vector2 d = target - from;
        if (d.magnitude > range) return false;
        if (d.magnitude > 0.6f && Vector2.Angle(Dir, d) > halfAngle) return false;
        return RoadNetwork.LineOfSight(from, target);
    }

    // ------------------------------------------------------------------ sprite
    static Sprite cone;

    // A sector pointing right from its pivot (the guard), 1 world unit long
    public static Sprite ConeSprite()
    {
        if (cone != null) return cone;
        const int w = 128;
        int h = Mathf.CeilToInt(2f * Mathf.Tan(BakedHalf * Mathf.Deg2Rad) * w) + 2;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float px = (x + 0.5f) / w, py = (y + 0.5f - h / 2f) / w;
                float dist = Mathf.Sqrt(px * px + py * py);
                float ang = Mathf.Atan2(Mathf.Abs(py), px) * Mathf.Rad2Deg;
                float a = 0f;
                if (dist <= 1f && ang <= BakedHalf)
                    a = dist > 0.95f || ang > BakedHalf - 2.5f ? 1f : Mathf.Lerp(0.75f, 0.35f, dist);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        tex.filterMode = FilterMode.Point;
        tex.Apply();
        cone = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0f, 0.5f), w);
        return cone;
    }
}
