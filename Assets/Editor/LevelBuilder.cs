using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

// Menu: Alien Game > Build Level 1 / Build Level 2
// Builds a level scene from its map data (Level1Data / Level2Data) using Talia's assets
// (Tiny Tile Town, Town Folk and the military sheet). Started by Danesh for Level 1.
public static class LevelBuilder
{
    const string ArtFolder = "Assets/Art";
    const string AlienFolder = "Assets/Art/Alien";
    const string TileFolder = "Assets/Tiles/Level1";   // tile assets shared by all levels
    const string TemplateScene = "Assets/Scenes/SampleScene.unity";

    // Sprites placed on the map; looked up in this order
    static readonly string[] PieceFolders = { "Assets/Art/Level1Pieces", "Assets/Art/Level2Pieces", "Assets/Art/Level3Pieces", "Assets/Art/Military" };
    static readonly string[] CenterPivot = { "grass", "water", "path_", "bridge", "folk_", "alien", "heart", "exit_marker", "ground_", "road_" };

    [MenuItem("Alien Game/Build Level 1")]
    public static void Build() => BuildLevel(1);

    [MenuItem("Alien Game/Build Level 2")]
    public static void BuildLevel2() => BuildLevel(2);

    [MenuItem("Alien Game/Build Level 3")]
    public static void BuildLevel3() => BuildLevel(3);

    public static void BuildLevel(int level)
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        LevelMap map = LevelMap.ByNumber(level);
        LevelMap.Current = map;   // so tile positions use this level's size
        string scenePath = "Assets/Scenes/Level" + level + ".unity";

        SetupSprites();
        Dictionary<string, TileBase> tiles = MakeTiles();
        BuildScene(map, level, scenePath, tiles);
        Debug.Log("Level " + level + " built: " + scenePath);
    }

    // ------------------------------------------------------------------ sprites
    static bool IsPiece(string path)
    {
        foreach (string f in PieceFolders)
            if (path.StartsWith(f + "/")) return true;
        return false;
    }

    static void SetupSprites()
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { ArtFolder });
        AssetDatabase.StartAssetEditing();
        try
        {
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.StartsWith(ArtFolder + "/Screens")) continue;   // menu backgrounds have their own settings
                TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
                if (ti == null) continue;

                bool isPiece = IsPiece(path);
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

    // Talia's sounds in Assets/Art/Audio
    static AudioClip Clip(string file) => AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Art/Audio/" + file);

    static AudioSource Sound(GameObject go, string file, bool loop, float volume)
    {
        AudioClip clip = Clip(file);
        if (clip == null) return null;
        AudioSource a = go.AddComponent<AudioSource>();
        a.clip = clip;
        a.loop = loop;
        a.volume = volume;
        a.playOnAwake = false;
        a.spatialBlend = 0f;   // 2D: same volume anywhere on screen
        return a;
    }

    static Sprite Piece(string name)
    {
        foreach (string f in PieceFolders)
        {
            Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(f + "/" + name + ".png");
            if (s != null) return s;
        }
        Debug.LogWarning("Missing sprite: " + name);
        return null;
    }

    static bool IsMilitary(string name) => AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Military/" + name + ".png") != null;

    // ------------------------------------------------------------------ tiles
    static Dictionary<string, TileBase> MakeTiles()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Tiles")) AssetDatabase.CreateFolder("Assets", "Tiles");
        if (!AssetDatabase.IsValidFolder(TileFolder)) AssetDatabase.CreateFolder("Assets/Tiles", "Level1");

        List<string> names = new List<string> { "grass", "grass_tuft", "water", "water_wave", "bridge", "ground_concrete", "ground_dirt", "ground_dirt_speckled" };
        for (int m = 0; m < 16; m++) { names.Add("path_" + m.ToString("00")); names.Add("road_" + m.ToString("00")); }

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
    static void BuildScene(LevelMap map, int level, string scenePath, Dictionary<string, TileBase> tiles)
    {
        // Start from the template and save over the level scene. Saving over the file (instead of
        // deleting it) keeps the scene's ID, so Build Settings and other references stay valid.
        var newScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(TemplateScene) != null
            ? EditorSceneManager.OpenScene(TemplateScene, OpenSceneMode.Single)
            : EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        EditorSceneManager.SaveScene(newScene, scenePath);
        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        int W = map.Width, H = map.Height;

        // Opening a scene can unload the tile objects we just made, so load them again from disk
        Dictionary<string, TileBase> fresh = new Dictionary<string, TileBase>();
        foreach (KeyValuePair<string, TileBase> kv in tiles)
            fresh[kv.Key] = AssetDatabase.LoadAssetAtPath<TileBase>(TileFolder + "/" + kv.Key + ".asset");
        tiles = fresh;

        new GameObject("Level Info").AddComponent<LevelInfo>().level = level;

        // --- tilemaps
        GameObject grid = new GameObject("Grid", typeof(Grid));
        Tilemap ground = MakeTilemap(grid, "Ground", -2000, true);
        Tilemap paths = MakeTilemap(grid, "Roads", -1999, true);
        Tilemap water = MakeTilemap(grid, "Water Collision", 0, false);
        water.gameObject.AddComponent<TilemapCollider2D>();

        PaintTiles(map, ground, paths, water, tiles);

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

        // Tiles covered by wire fence runs: a fence post standing on top of one is stretched down
        // half a tile so it meets the wire (the wire is drawn in the bottom half of its tile)
        HashSet<Vector2Int> wireTiles = new HashSet<Vector2Int>();
        foreach (Obj o in map.Objects)
            if (o.sprite == "wire_fence")
                for (int dx = 0; dx < o.w; dx++) wireTiles.Add(new Vector2Int(o.x + dx, o.y));

        foreach (Obj o in map.Objects)
        {
            string groupName = o.kind == "building" ? "Buildings" : o.kind == "field" ? "Fields" : o.kind == "tree" ? "Trees"
                             : o.kind == "deco" ? "Decorations" : o.kind == "vehicle" ? "Vehicles" : o.kind == "fence" ? "Fences" : o.kind == "lamp" ? "Lamps" : "UFO";
            GameObject go = new GameObject(o.sprite, typeof(SpriteRenderer));
            go.transform.SetParent(Group(groupName));
            float bottom = H - (o.y + o.h);
            go.transform.position = new Vector3(o.x, bottom, 0f);
            go.transform.localScale = Vector3.one * o.Scale;
            SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
            sr.sprite = Piece(o.sprite);
            if (o.sprite == "fence_post" && sr.sprite != null && wireTiles.Contains(new Vector2Int(o.x, o.y + o.h)))
            {
                float tall = sr.sprite.bounds.size.y;
                go.transform.position += Vector3.down * 0.5f;
                go.transform.localScale = new Vector3(o.Scale, o.Scale * (tall + 0.5f) / tall, 1f);
            }
            sr.sortingOrder = o.kind == "field" ? -1500 : (o.kind == "deco" || o.sprite == "fountain_square") ? -1400 : Mathf.RoundToInt(-bottom * 10f);

            if (o.kind == "building" || o.kind == "vehicle" || o.kind == "fence")
            {
                BoxCollider2D box = go.AddComponent<BoxCollider2D>();
                if (o.sprite == "fountain_square")
                {
                    box.offset = new Vector2(1f, 1.6f);
                    box.size = new Vector2(0.9f, 0.9f);
                }
                else if (sr.sprite != null && IsMilitary(o.sprite))
                {
                    // Military sprites aren't whole tiles: fit the box to the drawing (bottom part for buildings)
                    Vector2 size = sr.sprite.bounds.size;
                    float h = o.kind == "building" ? size.y * 0.7f : size.y;
                    box.offset = new Vector2(size.x / 2f, h / 2f);
                    box.size = new Vector2(Mathf.Max(0.2f, size.x - 0.1f), Mathf.Max(0.2f, h - 0.05f));
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
            else if (o.kind == "lamp")
            {
                CircleCollider2D c = go.AddComponent<CircleCollider2D>();
                c.offset = new Vector2(0.5f, 0.15f);
                c.radius = 0.2f;
                if (map.Night)
                {
                    bool flood = o.sprite == "flood_light";
                    // light at the lamp head (street lamp head is ~1.65 tiles up, floodlight ~2.3)
                    AddLight(Group("Lights"), o.sprite, new Vector2(o.x + 0.5f, bottom + (flood ? 2.3f : 1.65f)),
                             flood ? new Color(0.85f, 0.92f, 1f) : new Color(1f, 0.78f, 0.45f),
                             flood ? 0.75f : 0.7f, flood ? 4.6f : 3f, 0.35f);
                }
            }
            else if (o.kind == "ufo")
            {
                go.name = "UFO (start)";
                if (map.Night) AddLight(Group("Lights"), "UFO glow", new Vector2(o.x + o.w / 2f, bottom + o.h / 2f), new Color(0.55f, 1f, 0.75f), 0.7f, 3.5f, 0.5f);
            }
            if (map.Night && o.sprite.StartsWith("guard_hut"))
            {
                // Searchlights on the gate huts: south gate looks at the town, north gate looks out
                Vector2 top = new Vector2(o.x + o.w / 2f, bottom + o.h * 0.8f);
                Light2D spot = AddLight(Group("Lights"), "Searchlight", top, new Color(0.9f, 0.95f, 1f), 0.9f, 11f, 1f);
                spot.pointLightInnerAngle = 8f;
                spot.pointLightOuterAngle = 26f;
                Searchlight sweep = spot.gameObject.AddComponent<Searchlight>();
                sweep.facing = o.y > H / 2 ? -90f : 90f;
                sweep.phase = o.x * 0.1f;
                spot.transform.rotation = Quaternion.Euler(0f, 0f, sweep.facing - 90f);
            }
        }

        // --- exits
        Transform exits = Group("Exits");
        for (int e = 0; e < map.Exits.Length; e++)
            MakeExit(exits, "Exit " + (char)('A' + e), map.Exits[e]);

        // --- map edges (the alien cannot leave the map)
        Transform edges = Group("Map Edges");
        MakeWall(edges, "Left", new Vector2(-0.5f, H / 2f), new Vector2(1f, H + 2f));
        MakeWall(edges, "Right", new Vector2(W + 0.5f, H / 2f), new Vector2(1f, H + 2f));
        MakeWall(edges, "Bottom", new Vector2(W / 2f, -0.5f), new Vector2(W + 2f, 1f));
        MakeWall(edges, "Top", new Vector2(W / 2f, H + 0.5f), new Vector2(W + 2f, 1f));

        // --- the alien (green slime, 2-frame idle bounce)
        GameObject alien = new GameObject("Alien (player)", typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(CircleCollider2D));
        alien.transform.position = RoadNetwork.TileCenter(map.Start);
        Sprite[] alienFrames =
        {
            AssetDatabase.LoadAssetAtPath<Sprite>(AlienFolder + "/alien_idle_0.png"),
            AssetDatabase.LoadAssetAtPath<Sprite>(AlienFolder + "/alien_idle_1.png"),
        };
        alien.GetComponent<SpriteRenderer>().sprite = alienFrames[0];
        alien.AddComponent<SpriteFlipbook>().frames = alienFrames;
        Rigidbody2D rb = alien.GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        CircleCollider2D body = alien.GetComponent<CircleCollider2D>();
        body.radius = 0.3f;
        body.offset = new Vector2(0f, -0.2f);
        AlienPlayer ap = alien.AddComponent<AlienPlayer>();
        ap.disguiseSprite = Piece("folk_soldier");
        ap.beamAudio = Sound(alien, "freesound_beam.wav", false, 0.3f);
        ap.walkingAudio = Sound(alien, "freesound_community-walking-on-the-muddy-road-29586.mp3", true, 0.2f);
        alien.AddComponent<SortByY>();
        if (map.Night)
            AddLight(alien.transform, "Alien glow", Vector2.zero, new Color(0.55f, 1f, 0.6f), 0.55f, 2.6f, 0.3f).transform.localPosition = Vector3.zero;

        // --- people
        Transform people = Group("Civilians");
        Sprite heart = Piece("heart");
        int i = 1;
        foreach (Civ c in map.Civilians)
        {
            string label = c.boss ? "Boss " : c.IsPolice ? (c.sprite == "folk_soldier" ? "Soldier " : "Police ")
                         : c.lookout ? (c.sprite == "folk_soldier" ? "Sentry " : "Lookout ") : "Civilian ";
            GameObject civ = new GameObject(label + i++ + " (" + c.sprite.Substring(5) + ")", typeof(SpriteRenderer));
            civ.transform.SetParent(people);
            civ.transform.position = RoadNetwork.TileCenter(new Vector2Int(c.x, c.y));
            civ.GetComponent<SpriteRenderer>().sprite = Piece(c.sprite);
            Civilian script = civ.AddComponent<Civilian>();
            script.heartSprite = heart;
            script.friendshipNoise = Clip(i % 2 == 0 ? "freesound_community-wow.mp3" : "floraphonic-teenage-girl-says-yay-185316.mp3");
            if (c.lookout)
            {
                script.holdsPosition = true;
                TownLookout look = civ.AddComponent<TownLookout>();
                look.alarm = Sound(civ, "freesound_alarm 1.wav", false, 0.35f);
                look.gasp = Sound(civ, "freesound_community-gasp-7063.mp3", false, 0.4f);
            }
            if (c.IsPolice)
            {
                script.holdsPosition = true;
                script.toughness = 3;
                civ.AddComponent<PolicePatrol>().patrol = c.patrol;
            }
            if (c.boss)
            {
                // The mini boss: drawn at 2x, 12 hits, health bar on the HUD, fights back
                script.holdsPosition = true;
                script.toughness = 12;
                civ.transform.localScale = Vector3.one * 2f;
                civ.AddComponent<GeneralBoss>().patrol = c.patrol;
            }
            if (c.toughness > 0) script.toughness = c.toughness;
            civ.AddComponent<SortByY>();
        }

        // --- level manager (score + friendship meter), tutorial, mini map
        LevelManager lm = new GameObject("Level Manager").AddComponent<LevelManager>();
        lm.totalPeople = map.Civilians.Length;
        lm.friendsNeeded = map.FriendsNeeded;
        lm.alienCanDie = map.AlienCanDie;
        new GameObject("Tutorial Guide").AddComponent<TutorialGuide>().level = level;
        new GameObject("Mini Map").AddComponent<MiniMap>();

        // --- camera
        Camera cam = Object.FindFirstObjectByType<Camera>();
        if (cam != null)
        {
            cam.orthographic = true;
            cam.orthographicSize = 7f;
            cam.backgroundColor = map.Night ? new Color(0.04f, 0.06f, 0.1f) : new Color(0.16f, 0.27f, 0.12f);
            cam.transform.position = new Vector3(alien.transform.position.x, alien.transform.position.y, -10f);
            CameraFollow follow = cam.GetComponent<CameraFollow>();
            if (follow == null) follow = cam.gameObject.AddComponent<CameraFollow>();
            follow.target = alien.transform;
            follow.minBounds = Vector2.zero;
            follow.maxBounds = new Vector2(W, H);
        }

        SetupLighting(map);

        var scene = EditorSceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, scenePath);

        // Make sure the level is in the build (keeps the Start screen first if it is already there)
        List<EditorBuildSettingsScene> list = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        if (!list.Exists(s => s.path == scenePath))
        {
            list.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = list.ToArray();
        }

        Selection.activeGameObject = alien;
        SceneView.lastActiveSceneView?.FrameSelected();
    }

    // ------------------------------------------------------------------ lighting
    // Night levels: a dim blue sky light, plus the lamps / floodlights / searchlights made above.
    static void SetupLighting(LevelMap map)
    {
        MakeUnlitMaterial();
        foreach (Light2D l in Object.FindObjectsByType<Light2D>(FindObjectsSortMode.None))
        {
            if (l.lightType != Light2D.LightType.Global) continue;
            l.color = map.Night ? new Color(0.4f, 0.47f, 0.88f) : Color.white;
            l.intensity = map.Night ? 0.5f : 1f;
            l.gameObject.name = map.Night ? "Night Sky (Global Light 2D)" : "Global Light 2D";
        }
    }

    static Light2D AddLight(Transform parent, string name, Vector2 pos, Color color, float intensity, float radius, float inner)
    {
        GameObject go = new GameObject(name + " light");
        go.transform.SetParent(parent, false);
        go.transform.position = pos;
        Light2D l = go.AddComponent<Light2D>();
        l.lightType = Light2D.LightType.Point;
        l.color = color;
        l.intensity = intensity;
        l.pointLightOuterRadius = radius;
        l.pointLightInnerRadius = inner;
        l.falloffIntensity = 0.7f;
        // light every sorting layer (new lights may start with none)
        SerializedObject so = new SerializedObject(l);
        SerializedProperty layers = so.FindProperty("m_ApplyToSortingLayers");
        if (layers != null)
        {
            SortingLayer[] all = SortingLayer.layers;
            layers.arraySize = all.Length;
            for (int i = 0; i < all.Length; i++) layers.GetArrayElementAtIndex(i).intValue = all[i].id;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
        return l;
    }

    // Assets/Resources/SpriteUnlit.mat: used by Glow.Unlit for beams, pulses and watch circles
    static void MakeUnlitMaterial()
    {
        const string path = "Assets/Resources/SpriteUnlit.mat";
        if (AssetDatabase.LoadAssetAtPath<Material>(path) != null) return;
        Shader sh = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
        if (sh == null) { Debug.LogWarning("Sprite-Unlit-Default shader not found"); return; }
        if (!AssetDatabase.IsValidFolder("Assets/Resources")) AssetDatabase.CreateFolder("Assets", "Resources");
        AssetDatabase.CreateAsset(new Material(sh), path);
        AssetDatabase.SaveAssets();
    }

    static void PaintTiles(LevelMap map, Tilemap ground, Tilemap paths, Tilemap water, Dictionary<string, TileBase> tiles)
    {
        int W = map.Width, H = map.Height;
        ground.ClearAllTiles(); paths.ClearAllTiles(); water.ClearAllTiles();
        for (int row = 0; row < H; row++)
        {
            for (int x = 0; x < W; x++)
            {
                Vector3Int cell = new Vector3Int(x, H - 1 - row, 0);
                char g = map.Ground[row][x];
                char p = map.Paths[row][x];
                string gName = g == ',' ? "grass_tuft" : g == '~' ? "water" : g == '=' ? "water_wave"
                             : g == 'c' ? "ground_concrete" : g == 'd' ? "ground_dirt" : g == 's' ? "ground_dirt_speckled" : "grass";
                ground.SetTile(cell, tiles[gName]);
                if (p == '#') paths.SetTile(cell, tiles["bridge"]);
                else if (p != '.') paths.SetTile(cell, tiles[map.RoadTiles + System.Convert.ToInt32(p.ToString(), 16).ToString("00")]);
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
        LevelInfo info = Object.FindFirstObjectByType<LevelInfo>();
        LevelMap map = LevelMap.ByNumber(info != null ? info.level : 1);
        LevelMap.Current = map;
        Dictionary<string, TileBase> tiles = new Dictionary<string, TileBase>();
        foreach (string guid in AssetDatabase.FindAssets("t:TileBase", new[] { TileFolder }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            tiles[System.IO.Path.GetFileNameWithoutExtension(path)] = AssetDatabase.LoadAssetAtPath<TileBase>(path);
        }
        Tilemap ground = GameObject.Find("Grid/Ground").GetComponent<Tilemap>();
        Tilemap paths = GameObject.Find("Grid/Roads").GetComponent<Tilemap>();
        Tilemap water = GameObject.Find("Grid/Water Collision").GetComponent<Tilemap>();
        PaintTiles(map, ground, paths, water, tiles);
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
