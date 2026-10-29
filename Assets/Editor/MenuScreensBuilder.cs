using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Menu: Alien Game > Build Start, Game Over and Mini Map
// Makes the StartScreen and GameOver scenes, adds the mini map to Level1,
// and puts the scenes in the build order: StartScreen, Level1, GameOver.
public static class MenuScreensBuilder
{
    const string Template = "Assets/Scenes/SampleScene.unity";
    const string Level1 = "Assets/Scenes/Level1.unity";
    const string StartPath = "Assets/Scenes/StartScreen.unity";
    const string GameOverPath = "Assets/Scenes/GameOver.unity";

    [MenuItem("Alien Game/Build Start, Game Over and Mini Map")]
    public static void Build()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        Sprite startBg = ImportBackground("Assets/Art/Screens/start_background.png");
        Sprite overBg = ImportBackground("Assets/Art/Screens/gameover_background.png");

        MakeScreen(StartPath, startBg, MenuScreen.Kind.Start);
        MakeScreen(GameOverPath, overBg, MenuScreen.Kind.GameOver);
        AddMiniMapToLevel1();

        List<EditorBuildSettingsScene> list = new List<EditorBuildSettingsScene>
        {
            new EditorBuildSettingsScene(StartPath, true),
            new EditorBuildSettingsScene(Level1, true),
            new EditorBuildSettingsScene(GameOverPath, true),
        };
        foreach (EditorBuildSettingsScene s in EditorBuildSettings.scenes)
            if (s.path != StartPath && s.path != Level1 && s.path != GameOverPath) list.Add(s);
        EditorBuildSettings.scenes = list.ToArray();

        EditorSceneManager.OpenScene(StartPath, OpenSceneMode.Single);
        Debug.Log("Start screen, game over screen and mini map are ready.");
    }

    static Sprite ImportBackground(string path)
    {
        TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti != null && (ti.textureType != TextureImporterType.Sprite || ti.spriteImportMode != SpriteImportMode.Single))
        {
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.spritePixelsPerUnit = 100f;
            ti.mipmapEnabled = false;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    static void MakeScreen(string path, Sprite background, MenuScreen.Kind kind)
    {
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) != null) AssetDatabase.DeleteAsset(path);
        AssetDatabase.CopyAsset(Template, path);
        var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

        Camera cam = Object.FindFirstObjectByType<Camera>();
        if (cam != null)
        {
            cam.orthographic = true;
            cam.orthographicSize = 3.6f;
            cam.transform.position = new Vector3(0f, 0f, -10f);
            cam.backgroundColor = Color.black;
        }

        GameObject bg = new GameObject("Background", typeof(SpriteRenderer));
        bg.GetComponent<SpriteRenderer>().sprite = background;
        bg.GetComponent<SpriteRenderer>().sortingOrder = -100;
        bg.transform.localScale = new Vector3(1.2f, 1.2f, 1f);   // covers wide and tall screens

        MenuScreen menu = new GameObject(kind == MenuScreen.Kind.Start ? "Start Screen" : "Game Over Screen").AddComponent<MenuScreen>();
        menu.kind = kind;

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, path);
    }

    static void AddMiniMapToLevel1()
    {
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(Level1) == null)
        {
            Debug.LogWarning("Level1 scene not found. Run Alien Game > Build Level 1 first.");
            return;
        }
        var scene = EditorSceneManager.OpenScene(Level1, OpenSceneMode.Single);
        if (Object.FindFirstObjectByType<MiniMap>() == null)
            new GameObject("Mini Map").AddComponent<MiniMap>();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }
}
