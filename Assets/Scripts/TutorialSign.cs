using UnityEngine;

// Shows a tutorial message when the alien walks up to the sign.
public class TutorialSign : MonoBehaviour
{
    [TextArea] public string message;
    public float showRange = 1.6f;

    private Transform alien;

    void Start()
    {
        AlienPlayer player = FindFirstObjectByType<AlienPlayer>();
        if (player != null) alien = player.transform;
    }

    void Update()
    {
        if (alien == null || LevelManager.Instance == null) return;
        Vector2 center = (Vector2)transform.position + new Vector2(0.5f, 0.5f);
        bool near = Vector2.Distance(alien.position, center) < showRange;
        LevelManager.Instance.SetSign(this, near ? message : null);
    }
}
