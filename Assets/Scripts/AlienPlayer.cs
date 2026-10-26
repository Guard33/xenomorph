using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// The alien the player controls.
// The sprite is a placeholder (a dark villager) until our real alien art is ready.
// SPACE befriends a close person - a placeholder until the brain wave attack is made.
[RequireComponent(typeof(Rigidbody2D))]
public class AlienPlayer : MonoBehaviour
{
    public float moveSpeed = 4f;        // tiles per second
    public float befriendRange = 1.5f;  // tiles

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Vector2 input;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        input = ReadMove();
        if (input.sqrMagnitude > 1f)
            input.Normalize();

        if (sr != null && Mathf.Abs(input.x) > 0.01f)
            sr.flipX = input.x < 0f;

        if (BefriendPressed())
            TryBefriend();
    }

    void FixedUpdate()
    {
        rb.linearVelocity = input * moveSpeed;
    }

    void TryBefriend()
    {
        Civilian closest = null;
        float best = befriendRange;
        foreach (Civilian c in Civilian.All)
        {
            if (!c.CanBeBefriended) continue;
            float d = Vector2.Distance(c.transform.position, transform.position);
            if (d < best)
            {
                best = d;
                closest = c;
            }
        }
        if (closest != null)
            closest.Befriend();
    }

    Vector2 ReadMove()
    {
        Vector2 v = Vector2.zero;
#if ENABLE_INPUT_SYSTEM
        Keyboard kb = Keyboard.current;
        if (kb == null) return v;
        if (kb.leftArrowKey.isPressed || kb.aKey.isPressed) v.x -= 1f;
        if (kb.rightArrowKey.isPressed || kb.dKey.isPressed) v.x += 1f;
        if (kb.downArrowKey.isPressed || kb.sKey.isPressed) v.y -= 1f;
        if (kb.upArrowKey.isPressed || kb.wKey.isPressed) v.y += 1f;
#else
        v.x = Input.GetAxisRaw("Horizontal");
        v.y = Input.GetAxisRaw("Vertical");
#endif
        return v;
    }

    bool BefriendPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.Space);
#endif
    }
}
