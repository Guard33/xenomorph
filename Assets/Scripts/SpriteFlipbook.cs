using UnityEngine;

// Loops through a few sprites, like a GIF. Used for the alien's idle bounce.
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteFlipbook : MonoBehaviour
{
    public Sprite[] frames;
    public float frameTime = 0.3f;   // seconds per frame

    private SpriteRenderer sr;
    private float t;
    private int index;

    void Awake() { sr = GetComponent<SpriteRenderer>(); }

    void Update()
    {
        if (frames == null || frames.Length == 0) return;
        t += Time.deltaTime;
        if (t < frameTime) return;
        t -= frameTime;
        index = (index + 1) % frames.Length;
        sr.sprite = frames[index];
    }
}
