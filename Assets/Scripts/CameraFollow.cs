using UnityEngine;

// The camera follows the alien smoothly and never shows past the edge of the map.
public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothTime = 0.15f;
    public Vector2 minBounds = Vector2.zero;
    public Vector2 maxBounds = new Vector2(40f, 26f);

    private Vector3 velocity;
    private Camera cam;

    void Awake() { cam = GetComponent<Camera>(); }

    void LateUpdate()
    {
        if (target == null) return;
        Vector3 goal = new Vector3(target.position.x, target.position.y, transform.position.z);
        if (cam != null && cam.orthographic)
        {
            float halfH = cam.orthographicSize;
            float halfW = halfH * cam.aspect;
            goal.x = Clamp(goal.x, minBounds.x + halfW, maxBounds.x - halfW);
            goal.y = Clamp(goal.y, minBounds.y + halfH, maxBounds.y - halfH);
        }
        transform.position = Vector3.SmoothDamp(transform.position, goal, ref velocity, smoothTime);
    }

    static float Clamp(float v, float min, float max)
    {
        return min > max ? (min + max) * 0.5f : Mathf.Clamp(v, min, max);
    }
}
