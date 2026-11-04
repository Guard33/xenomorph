using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Menu: Alien Game > Build Upgrade Screen
// Makes the UpgradeScreen scene (the alien's upgrade path) and adds it to the build.
public static class UpgradeScreenBuilder
{
    const string Template = "Assets/Scenes/SampleScene.unity";
    const string ScenePath = "Assets/Scenes/UpgradeScreen.unity";
    const string Background = "Assets/Art/Screens/start_background.png";
    const string IconFolder = "Assets/Resources/Upgrades";

    [MenuItem("Alien Game/Build Upgrade Screen")]
    public static void Build()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        // Pixel icons: keep them sharp
        foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { IconFolder }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti == null || (ti.filterMode == FilterMode.Point && ti.textureType == TextureImporterType.Default && !ti.mipmapEnabled)) continue;
            ti.textureType = TextureImporterType.Default;
            ti.filterMode = FilterMode.Point;
            ti.mipmapEnabled = false;
            ti.alphaIsTransparency = true;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.SaveAndReimport();
        }

        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null) AssetDatabase.DeleteAsset(ScenePath);
        AssetDatabase.CopyAsset(Template, ScenePath);
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        Camera cam = Object.FindFirstObjectByType<Camera>();
        if (cam != null)
        {
            cam.orthographic = true;
            cam.orthographicSize = 3.6f;
            cam.transform.position = new Vector3(0f, 0f, -10f);
            cam.backgroundColor = new Color(0.03f, 0.04f, 0.08f);
        }

        Sprite bg = AssetDatabase.LoadAssetAtPath<Sprite>(Background);
        if (bg != null)
        {
            GameObject go = new GameObject("Background", typeof(SpriteRenderer));
            SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
            sr.sprite = bg;
            sr.color = new Color(0.55f, 0.6f, 0.75f);
            sr.sortingOrder = -100;
            go.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
        }

        new GameObject("Upgrade Screen").AddComponent<UpgradeScreen>();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);

        List<EditorBuildSettingsScene> list = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        if (!list.Exists(s => s.path == ScenePath)) list.Add(new EditorBuildSettingsScene(ScenePath, true));
        EditorBuildSettings.scenes = list.ToArray();
        Debug.Log("Upgrade screen ready: " + ScenePath);
    }
}
