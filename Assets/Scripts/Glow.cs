using UnityEngine;

// Effects that should glow at night (beams, pulses, hearts, watch circles) use an unlit
// sprite material, so the dark sky of a night level doesn't dim them.
// The material is Assets/Resources/SpriteUnlit.mat (made by the level builder).
public static class Glow
{
    static Material unlit;
    static bool loaded;

    public static void Unlit(SpriteRenderer sr)
    {
        if (!loaded)
        {
            unlit = Resources.Load<Material>("SpriteUnlit");
            loaded = true;
        }
        if (unlit != null && sr != null) sr.sharedMaterial = unlit;
    }
}
