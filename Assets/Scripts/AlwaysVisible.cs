using UnityEngine;

// Keeps the alien from ever being completely hidden. A faint copy of its sprite is drawn on top of
// everything on the map (under the aim arrow). When nothing is in front, the copy sits exactly on the
// real sprite and looks the same; when a roof, tree or vehicle is in front, the alien shows through it.
// AlienPlayer adds this to itself when the level starts.
public class AlwaysVisible : MonoBehaviour
{
    public float alpha = 0.5f;
    public int order = 28980;          // above every building, below the aim arrow (28990)

    private SpriteRenderer source, ghost;

    void Start()
    {
        source = GetComponent<SpriteRenderer>();
        GameObject go = new GameObject("Always visible");
        go.transform.SetParent(transform, false);
        ghost = go.AddComponent<SpriteRenderer>();
        ghost.sharedMaterial = source.sharedMaterial;   // lit the same way as the alien (night levels)
        ghost.sortingOrder = order;
    }

    void LateUpdate()
    {
        if (source == null || ghost == null) return;
        ghost.sprite = source.sprite;
        ghost.flipX = source.flipX;
        ghost.enabled = source.enabled;
        Color c = source.color;
        ghost.color = new Color(c.r, c.g, c.b, c.a * alpha);
    }
}
