using UnityEngine;

// A searchlight on a guard hut: the spot light sweeps slowly back and forth.
// Just for the look of the base at night; it doesn't spot the alien (yet).
public class Searchlight : MonoBehaviour
{
    public float facing = -90f;    // degrees, the middle of the sweep (0 = right, 90 = up)
    public float sweep = 50f;      // degrees either side
    public float speed = 0.35f;    // sweeps per second
    public float phase;

    void Update()
    {
        float a = facing + Mathf.Sin((Time.time * speed + phase) * Mathf.PI * 2f) * sweep;
        // Light2D spot lights point along their local up axis
        transform.rotation = Quaternion.Euler(0f, 0f, a - 90f);
    }
}
