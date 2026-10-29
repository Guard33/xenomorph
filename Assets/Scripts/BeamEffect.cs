using UnityEngine;

// The glowing bar you see when the alien fires a beam. It fades out and removes itself.
public class BeamEffect : MonoBehaviour
{
    public float life = 0.25f;

    private SpriteRenderer sr;
    private Color color;
    private float t;

    static Sprite pixel;

    // A 1 x 1 tile white square, stretched and tinted for each beam
    public static Sprite Pixel
    {
        get
        {
            if (pixel == null)
                pixel = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 4, 4), new Vector2(0f, 0.5f), 4f);
            return pixel;
        }
    }

    public static BeamEffect Spawn(Vector2 origin, Vector2 dir, float length, float width, Color color)
    {
        GameObject go = new GameObject("Beam");
        go.transform.position = origin;
        go.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
        go.transform.localScale = new Vector3(length, width, 1f);
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Pixel;
        sr.sortingOrder = 29000;
        return FadeOut(sr, color, 0.25f);
    }

    // Fade any sprite out over 'life' seconds, then remove it
    public static BeamEffect FadeOut(SpriteRenderer sr, Color color, float life)
    {
        BeamEffect b = sr.gameObject.AddComponent<BeamEffect>();
        b.sr = sr;
        b.color = color;
        b.life = life;
        sr.color = color;
        return b;
    }

    void Update()
    {
        t += Time.deltaTime;
        float k = Mathf.Clamp01(t / life);
        sr.color = new Color(color.r, color.g, color.b, color.a * (1f - k));
        if (k >= 1f) Destroy(gameObject);
    }
}
