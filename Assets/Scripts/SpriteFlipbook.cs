using UnityEngine;

// Loops through a few sprites, like a GIF. Used for the alien's idle bounce, and its walk cycle
// when a stage model has walk frames and the alien is moving.
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteFlipbook : MonoBehaviour
{
    public Sprite[] frames;
    public float frameTime = 0.3f;   // seconds per frame

    public Sprite[] walkFrames;      // optional; used while moving is true
    public float walkFrameTime = 0.12f;
    [HideInInspector] public bool moving;

    private SpriteRenderer sr;
    private float t;
    private int index;
    private bool wasWalking;

    void Awake() { sr = GetComponent<SpriteRenderer>(); }

    void Update()
    {
        bool walking = moving && walkFrames != null && walkFrames.Length > 0;
        Sprite[] set = walking ? walkFrames : frames;
        if (set == null || set.Length == 0) return;
        if (walking != wasWalking)
        {
            // Switch straight away so the first step shows as soon as the alien moves
            wasWalking = walking;
            t = 0f;
            index = 0;
            sr.sprite = set[0];
            return;
        }
        t += Time.deltaTime;
        float step = walking ? walkFrameTime : frameTime;
        if (t < step) return;
        t -= step;
        index = (index + 1) % set.Length;
        sr.sprite = set[index];
    }
}
