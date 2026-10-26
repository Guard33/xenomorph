using UnityEngine;

// Things lower on the screen are drawn in front (top-down view).
[RequireComponent(typeof(SpriteRenderer))]
public class SortByY : MonoBehaviour
{
    public float footOffset = -0.5f;   // where the feet are, from the sprite centre
    private SpriteRenderer sr;

    void Awake() { sr = GetComponent<SpriteRenderer>(); }

    void LateUpdate()
    {
        sr.sortingOrder = Mathf.RoundToInt(-(transform.position.y + footOffset) * 10f);
    }
}
