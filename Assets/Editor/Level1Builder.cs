using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

// Menu: Alien Game > Build Level 1
// Builds the Level 1 "Quiet Farm Town" scene from Talia's assets
// (Tiny Tile Town + Town Folk) using the map in Level1Data.
public static class Level1Builder
{
    const string ArtFolder = "Assets/Art";
    const string PieceFolder = "Assets/Art/Level1Pieces";
    const string TileFolder = "Assets/Tiles/Level1";
    const string TemplateScene = "Assets/Scenes/SampleScene.unity";
    const string ScenePath = "Assets/Scenes/Level1.unity";

    static readonly string[] CenterPivot = { "grass", "water", "path_", "bridge", "folk_", "alien", "heart", "exit_marker" };

    [MenuItem("Alien Game/Build Level 1")]
    public static void Build()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        SetupSprites();
        Dictionary<string, TileBase> tiles = MakeTiles();
        BuildScene(tiles);
        Debug.Log("Level 1 built: " + ScenePath);
    }

    // ------------------------------------------------------------------ sprites
    static void SetupSprites()
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { ArtFolder });
        AssetDatabase.StartAssetEditing();
        try
        {
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
                if (ti == null) continue;

                bool isPiece = path.StartsWith(PieceFolder);
                string file = System.IO.Path.GetFileNameWithoutExtension(path);
                int align = (int)SpriteAlignment.Center;
                if (isPiece)
                {
                    align = (int)SpriteAlignment.BottomLeft;
                    foreach (string c in CenterPivot)
                        if (file.StartsWith(c)) align = (int)SpriteAlignment.Center;
                }

                TextureImporterSettings s = new TextureImporterSettings();
                ti.ReadTextureSettings(s);
                bool changed = ti.textureType != TextureImporterType.Sprite
                    || !Mathf.Approximately(ti.spritePixelsPerUnit, 16f)
                    || ti.filterMode != FilterMode.Point
                    || ti.textureCompression != TextureImporterCompression.Uncompressed
                    || ti.mipmapEnabled
                    || (isPiece && (s.spriteAlignment != align || s.spriteMeshType != SpriteMeshType.FullRect));
                if (!changed) continue;

                ti.textureType = TextureImporterType.Sprite;
                if (isPiece) ti.spriteImportMode = SpriteImportMode.Single;
                ti.spritePixelsPerUnit = 16f;
                ti.filterMode = FilterMode.Point;
                ti.textureCompression = TextureImporterCompression.Uncompressed;
                ti.mipmapEnabled = false;
                ti.ReadTextureSettings(s);
                if (isPiece)
                {
                    s.spriteAlignment = align;
                    s.spriteMeshType = SpriteMeshType.FullRect;
                }
                ti.SetTextureSettings(s);
                ti.SaveAndReimport();
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }
        AssetDatabase.Refresh();
    }

    static Sprite Piece(string name)
    {
        Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(PieceFolder + "/" + name + ".png");
        if (s == null) Debug.LogWarning("Missing sprite: " + name);
        return s;
    }

    // ------------------------------------------------------------------ tiles
    static Dictionary<string, TileBase> MakeTiles()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Tiles")) AssetDatabase.CreateFolder("Assets", "Tiles");
        if (!AssetDatabase.IsValidFolder(TileFolder)) AssetDatabase.CreateFolder("Assets/Tiles", "Level1");

        List<string> names = new List<string> { "grass", "grass_tuft", "water", "water_wave", "bridge" };
        for (int m = 0; m < 16; m++) names.Add("path_" + m.ToString("00"));

        Dictionary<string, TileBase> tiles = new Dictionary<string, TileBase>();
        foreach (string n in names)
            tiles[n] = MakeTile(n, n, Tile.ColliderType.None);
        tiles["water_block"] = MakeTile("water_block", "water", Tile.ColliderType.Grid);
        AssetDatabase.SaveAssets();
        return tiles;
    }

    static TileBase MakeTile(string tileName, string spriteName, Tile.ColliderType collider)
    {
        string path = TileFolder + "/" + tileName + ".asset";
        Tile tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
        if (tile == null)
        {
            tile = ScriptableObject.CreateInstance<Tile>();
            AssetDatabase.CreateAsset(tile, path);
        }
        tile.sprite = Piece(spriteName);
        tile.colliderType = collider;
        EditorUtility.SetDirty(tile);
        return tile;
    }

    // ------------------------------------------------------------------ scene
    static void BuildScene(Dictionary<string, TileBase> tiles)
    {
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null)
            AssetDatabase.DeleteAsset(ScenePath);
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(TemplateScene) != null)
        {
            AssetDatabase.CopyAsset(TemplateScene, ScenePath);
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }
        else
        {
            var newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            EditorSceneManager.SaveScene(newScene, ScenePath);
        }

        int W = Level1Data.Width, H = Level1Data.Height;

        // Opening a scene can unload the tile objects we just made, so load them again from disk
        Dictionary<string, TileBase> fresh = new Dictionary<string, TileBase>();
        foreach (KeyValuePair<string, TileBase> kv in tiles)
            fresh[kv.Key] = AssetDatabase.LoadAssetAtPath<TileBase>(TileFolder + "/" + kv.Key + ".asset");
        tiles = fresh;

        // --- tilemaps
        GameObject grid = new GameObject("Grid", typeof(Grid));
        Tilemap ground = MakeTilemap(grid, "Ground", -2000, true);
        Tilemap paths = MakeTilemap(grid, "Roads", -1999, true);
        Tilemap water = MakeTilemap(grid, "Water Collision", 0, false);
        water.gameObject.AddComponent<TilemapCollider2D>();

        PaintTiles(ground, paths, water, tiles);

        // --- map objects
        Dictionary<string, Transform> groups = new Dictionary<string, Transform>();
        Transform Group(string name)
        {
            if (!groups.TryGetValue(name, out Transform t))
            {
                t = new GameObject(name).transform;
                groups[name] = t;
            }
            return t;
        }

        int signCount = 0;
        foreach (Level1Data.Obj o in Level1Data.Objects)
        {
            string groupName = o.kind == "building" ? "Buildings" : o.kind == "field" ? "Fields" : o.kind == "tree" ? "Trees"
                             : o.kind == "deco" ? "Decorations" : o.kind == "sign" ? "Tutorial Signs" : "UFO";
            GameObject go = new GameObject(o.sprite, typeof(SpriteRenderer));
            go.transform.SetParent(Group(groupName));
            float bottom = H - (o.y + o.h);
            go.transform.position = new Vector3(o.x, bottom, 0f);
            SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
            sr.sprite = Piece(o.sprite);
            sr.sortingOrder = o.kind == "field" ? -1500 : (o.kind == "deco" || o.sprite == "fountain_square") ? -1400 : Mathf.RoundToInt(-bottom * 10f);

            if (o.kind == "building")
            {
                BoxCollider2D box = go.AddComponent<BoxCollider2D>();
                if (o.sprite == "fountain_square")
                {
                    box.offset = new Vector2(1f, 1.6f);
                    box.size = new Vector2(0.9f, 0.9f);
                }
                else
                {
                    box.offset = new Vector2(o.w / 2f, o.h * 0.35f);
                    box.size = new Vector2(o.w - 0.15f, o.h * 0.7f - 0.1f);
                }
            }
            else if (o.kind == "tree")
            {
                CircleCollider2D c = go.AddComponent<CircleCollider2D>();
                c.offset = new Vector2(0.5f, 0.35f);
                c.radius = 0.28f;
            }
            else if (o.kind == "sign")
            {
                TutorialSign sign = go.AddComponent<TutorialSign>();
                sign.message = signCount == 0
                    ? "Welcome to Earth!\nUse WASD or the arrow keys to move."
                    : "Walk up to a person and press SPACE to befriend them.\nBefriend 10 people before they run to an exit!";
                signCount++;
            }
            else if (o.kind == "ufo")
            {
                go.name = "UFO (start)";
            }
        }

        // --- exits
        Transform exits = Group("Exits");
        MakeExit(exits, "Exit A (bridge)", Level1Data.ExitA);
        MakeExit(exits, "Exit B (south road)", Level1Data.ExitB);

        // --- map edges (the alien cannot leave the map)
        Transform edges = Group("Map Edges");
        MakeWall(edges, "Left", new Vector2(-0.5f, H / 2f), new Vector2(1f, H + 2f));
        MakeWall(edges, "Right", new Vector2(W + 0.5f, H / 2f), new Vector2(1f, H + 2f));
        MakeWall(edges, "Bottom", new Vector2(W / 2f, -0.5f), new Vector2(W + 2f, 1f));
        MakeWall(edges, "Top", new Vector2(W / 2f, H + 0.5f), new Vector2(W + 2f, 1f));

        // --- the alien (placeholder sprite)
        GameObject alien = new GameObject("Alien (player)", typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(CircleCollider2D));
        alien.transform.position = RoadNetwork.TileCenter(Level1Data.Start);
        alien.GetComponent<SpriteRenderer>().sprite = Piece("alien_placeholder");
        Rigidbody2D rb = alien.GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        CircleCollider2D body = alien.GetComponent<CircleCollider2D>();
        body.radius = 0.3f;
        body.offset = new Vector2(0f, -0.2f);
        alien.AddComponent<AlienPlayer>();
        alien.AddComponent<SortByY>();

        // --- civilians
        Transform people = Group("Civilians");
        Sprite heart = Piece("heart");
        int i = 1;
        foreach (Level1Data.Civ c in Level1Data.Civilians)
        {
            GameObject civ = new GameObject("Civilian " + i++ + " (" + c.sprite.Substring(5) + ")", typeof(SpriteRenderer));
            civ.transform.SetParent(people);
            civ.transform.position = RoadNetwork.TileCenter(new Vector2Int(c.x, c.y));
            civ.GetComponent<SpriteRenderer>().sprite = Piece(c.sprite);
            Civilian script = civ.AddComponent<Civilian>();
            script.heartSprite = heart;
            civ.AddComponent<SortByY>();
        }

        // --- level manager (score + friendship meter)
        new GameObject("Level Manager").AddComponent<LevelManager>();

        // --- camera
        Camera cam = Object.FindFirstObjectByType<Camera>();
        if (cam != null)
        {
            cam.orthographic = true;
            cam.orthographicSize = 7f;
            cam.backgroundColor = new Color(0.16f, 0.27f, 0.12f);
            cam.transform.position = new Vector3(alien.transform.position.x, alien.transform.position.y, -10f);
            CameraFollow follow = cam.GetComponent<CameraFollow>();
            if (follow == null) follow = cam.gameObject.AddComponent<CameraFollow>();
            follow.target = alien.transform;
            follow.minBounds = Vector2.zero;
            follow.maxBounds = new Vector2(W, H);
        }

        var scene = EditorSceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);

        // Level 1 is the first scene in the build
        List<EditorBuildSettingsScene> list = new List<EditorBuildSettingsScene> { new EditorBuildSettingsScene(ScenePath, true) };
        foreach (EditorBuildSettingsScene s in EditorBuildSettings.scenes)
            if (s.path != ScenePath) list.Add(s);
        EditorBuildSettings.scenes = list.ToArray();

        Selection.activeGameObject = alien;
        SceneView.lastActiveSceneView?.FrameSelected();
    }

    static void PaintTiles(Tilemap ground, Tilemap paths, Tilemap water, Dictionary<string, TileBase> tiles)
    {
        int W = Level1Data.Width, H = Level1Data.Height;
        ground.ClearAllTiles(); paths.ClearAllTiles(); water.ClearAllTiles();
        for (int row = 0; row < H; row++)
        {
            for (int x = 0; x < W; x++)
            {
                Vector3Int cell = new Vector3Int(x, H - 1 - row, 0);
                char g = Level1Data.Ground[row][x];
                char p = Level1Data.Paths[row][x];
                string gName = g == ',' ? "grass_tuft" : g == '~' ? "water" : g == '=' ? "water_wave" : "grass";
                ground.SetTile(cell, tiles[gName]);
                if (p == '#') paths.SetTile(cell, tiles["bridge"]);
                else if (p != '.') paths.SetTile(cell, tiles["path_" + System.Convert.ToInt32(p.ToString(), 16).ToString("00")]);
                if ((g == '~' || g == '=') && p != '#') water.SetTile(cell, tiles["water_block"]);
            }
        }

        ground.RefreshAllTiles(); paths.RefreshAllTiles(); water.RefreshAllTiles();
        EditorUtility.SetDirty(ground); EditorUtility.SetDirty(paths); EditorUtility.SetDirty(water);
    }

    // Menu: Alien Game > Repaint Map Tiles (if the ground ever looks empty)
    [MenuItem("Alien Game/Repaint Map Tiles")]
    public static void RepaintTiles()
    {
        Dictionary<string, TileBase> tiles = new Dictionary<string, TileBase>();
        foreach (string guid in AssetDatabase.FindAssets("t:TileBase", new[] { TileFolder }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            tiles[System.IO.Path.GetFileNameWithoutExtension(path)] = AssetDatabase.LoadAssetAtPath<TileBase>(path);
        }
        Tilemap ground = GameObject.Find("Grid/Ground").GetComponent<Tilemap>();
        Tilemap paths = GameObject.Find("Grid/Roads").GetComponent<Tilemap>();
        Tilemap water = GameObject.Find("Grid/Water Collision").GetComponent<Tilemap>();
        PaintTiles(ground, paths, water, tiles);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
    }

    static Tilemap MakeTilemap(GameObject grid, string name, int order, bool visible)
    {
        GameObject go = new GameObject(name, typeof(Tilemap));
        go.transform.SetParent(grid.transform);
        if (visible)
        {
            TilemapRenderer r = go.AddComponent<TilemapRenderer>();
            r.sortingOrder = order;
        }
        return go.GetComponent<Tilemap>();
    }

    static void MakeExit(Transform parent, string name, Vector2Int tile)
    {
        GameObject go = new GameObject(name, typeof(SpriteRenderer));
        go.transform.SetParent(parent);
        go.transform.position = RoadNetwork.TileCenter(tile);
        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
        sr.sprite = Piece("exit_marker");
        sr.sortingOrder = -1490;
    }

    static void MakeWall(Transform parent, string name, Vector2 center, Vector2 size)
    {
        GameObject go = new GameObject(name, typeof(BoxCollider2D));
        go.transform.SetParent(parent);
        go.transform.position = center;
        go.GetComponent<BoxCollider2D>().size = size;
    }
}
