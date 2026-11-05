using UnityEngine;

// Put in each level scene: tells the game which level's map is loaded.
[DefaultExecutionOrder(-100)]
public class LevelInfo : MonoBehaviour
{
    public int level = 1;

    // When testing Level 2 straight from the editor (no evolve screen), J uses this skill
    public string defaultJSkill = "spread_beam";

    public static LevelInfo Instance { get; private set; }

    void Awake()
    {
        Instance = this;
        LevelMap.Current = LevelMap.ByNumber(level);
    }
}
