// The alien's upgrade path (Danesh).
// The alien starts with the Small Beam on H. At the end of each level it evolves
// and picks ONE of two new skills. Evolutions unlock the J, K and L skill keys.
//   Brain path (pink): catch more people at once.
//   Sneak path (blue): control the crowd and stay hidden.
// The player can mix paths, so every playthrough can be different.
// Each pick answers the new threat of the next level (see Docs/Progression.md).
public static class UpgradePath
{
    public class Option
    {
        public string id;
        public string name;
        public string line1;
        public string line2;
        public string icon;       // texture in Resources/Upgrades
        public bool brainPath;    // true = Brain path, false = Sneak path
    }

    public class Tier
    {
        public int stage;         // alien stage after this evolution
        public string key;        // skill key this evolution unlocks
        public string nextLevel;  // the level where the new skill is first used
        public string threat;     // what that level adds
        public Option a;          // Brain path choice
        public Option b;          // Sneak path choice
    }

    public static readonly Option Start = new Option
    {
        id = "small_beam", name = "Small Beam", line1 = "Befriend one", line2 = "person", icon = "small_beam", brainPath = true
    };

    public static readonly Tier[] Tiers =
    {
        new Tier
        {
            stage = 2, key = "J", nextLevel = "Level 2: Suburbs", threat = "Panic spreads",
            a = new Option { id = "wide_wave", name = "Wide Wave", line1 = "Wave spreads", line2 = "hits 3 people", icon = "wide_wave", brainPath = true },
            b = new Option { id = "calm_hum", name = "Calm Hum", line1 = "People near you", line2 = "stop panicking", icon = "calm_hum", brainPath = false },
        },
        new Tier
        {
            stage = 3, key = "K", nextLevel = "Level 3: Downtown", threat = "Police patrols",
            a = new Option { id = "long_beam", name = "Long Beam", line1 = "Double range", line2 = "through people", icon = "long_beam", brainPath = true },
            b = new Option { id = "disguise", name = "Disguise", line1 = "Look human", line2 = "for 5 seconds", icon = "disguise", brainPath = false },
        },
        new Tier
        {
            stage = 4, key = "L", nextLevel = "Level 4: City Centre", threat = "Army and time limit",
            a = new Option { id = "tractor_beam", name = "Tractor Beam", line1 = "Pull everyone", line2 = "in a big circle", icon = "tractor_beam", brainPath = true },
            b = new Option { id = "friend_army", name = "Friend Army", line1 = "Friends return", line2 = "block an exit", icon = "friend_army", brainPath = false },
        },
    };

    // -1 = not picked yet, 0 = Brain path (a), 1 = Sneak path (b)
    public static readonly int[] Chosen = { -1, -1, -1 };

    // The evolution the player picks next (Tiers.Length when the path is complete)
    public static int NextTier
    {
        get
        {
            for (int i = 0; i < Chosen.Length; i++)
                if (Chosen[i] < 0) return i;
            return Chosen.Length;
        }
    }

    public static Option Picked(int tier)
    {
        if (tier < 0 || tier >= Tiers.Length) return null;
        return Chosen[tier] == 0 ? Tiers[tier].a : Chosen[tier] == 1 ? Tiers[tier].b : null;
    }

    public static void Reset()
    {
        for (int i = 0; i < Chosen.Length; i++) Chosen[i] = -1;
    }
}
