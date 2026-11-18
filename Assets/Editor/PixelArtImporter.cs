using UnityEditor;
using UnityEngine;

// Any image added under Assets/Art is imported as crisp pixel art:
// a sprite at 16 pixels per tile, no blurring, no compression, no mipmaps.
// (Menu backgrounds in Art/Screens keep their own settings.)
public class PixelArtImporter : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        bool art = assetPath.StartsWith("Assets/Art/") && !assetPath.StartsWith("Assets/Art/Screens/");
        bool alienStage = assetPath.StartsWith("Assets/Resources/Alien/");   // stage models (see AlienPlayer)
        if (!art && !alienStage) return;
        if (!assetImporter.importSettingsMissing) return;   // only first import; don't override later manual changes

        TextureImporter ti = (TextureImporter)assetImporter;
        ti.textureType = TextureImporterType.Sprite;
        ti.spriteImportMode = SpriteImportMode.Single;
        ti.spritePixelsPerUnit = 16f;
        ti.filterMode = FilterMode.Point;
        ti.textureCompression = TextureImporterCompression.Uncompressed;
        ti.mipmapEnabled = false;
    }
}
