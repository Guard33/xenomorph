using UnityEditor;
using UnityEngine;

// Frames dropped into Assets/Resources/Alien/stageN import as pixel art (16 pixels per tile, no blur)
// with the pivot near the feet, so taller stage models stand on the same spot as the Level 1 slime.
public class AlienStageImporter : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        if (!assetPath.Replace('\\', '/').StartsWith("Assets/Resources/Alien/stage")) return;
        TextureImporter ti = (TextureImporter)assetImporter;
        ti.textureType = TextureImporterType.Sprite;
        ti.spriteImportMode = SpriteImportMode.Single;
        ti.spritePixelsPerUnit = 16f;
        ti.filterMode = FilterMode.Point;
        ti.textureCompression = TextureImporterCompression.Uncompressed;
        ti.mipmapEnabled = false;
        ti.alphaIsTransparency = true;

        TextureImporterSettings s = new TextureImporterSettings();
        ti.ReadTextureSettings(s);
        s.spriteAlignment = (int)SpriteAlignment.Custom;
        // The slime's feet are 8 pixels below its centre; put this drawing's bottom edge there too.
        s.spritePivot = new Vector2(0.5f, 8f / 32f);
        s.spriteMeshType = SpriteMeshType.FullRect;
        ti.SetTextureSettings(s);
    }
}
