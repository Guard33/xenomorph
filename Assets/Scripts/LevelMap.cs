using UnityEngine;

// One level's map: tiles, objects, people, the alien's start and the exits.
// Level1Data.Map and Level2Data.Map are the levels; the scene's LevelInfo says which one is loaded,
// so the pathfinding, mini map and HUD all work for any level.
public class LevelMap
{
    public string Name;
    public int Width, Height;
    // Ground: '.' grass, ',' grass with tufts, '~' water, '=' water with waves,
    //         'c' concrete, 'd' dirt, 's' dirt with specks
    public string[] Ground;
    // Dirt roads: '.' none, '#' bridge, '0'-'F' road piece (N=1, E=2, S=4, W=8)
    public string[] Paths;
    public Obj[] Objects;
    public Civ[] Civilians;
    public Vector2Int Start;
    public Vector2Int[] Exits;
    public Vector2Int[][] Patrols = new Vector2Int[0][];   // police patrol routes (tiles)
    public int FriendsNeeded = 10;
    public bool Night;                     // dark sky, lamps and floodlights light the map
    public string RoadTiles = "path_";     // road pieces: "path_" dirt (Level 1) or "road_" asphalt
    public bool AlienCanDie;               // something on this level can defeat the alien (Level 3: the General)

    static LevelMap current;

    // The level that is loaded right now (Level 1 when no LevelInfo is in the scene)
    public static LevelMap Current
    {
        get => current ?? Level1Data.Map;
        set => current = value;
    }

    public static LevelMap ByNumber(int level) => level == 3 ? Level3Data.Map : level == 2 ? Level2Data.Map : Level1Data.Map;
}

// An object on the map. x, y = top-left tile; w, h = size in tiles.
// kind: building, field, tree, deco, ufo, vehicle, fence, lamp
// scale: how big the sprite is drawn (Talia's military sprites are drawn at 2x)
public struct Obj
{
    public string sprite, kind; public int x, y, w, h; public float scale;
    public Obj(string s, int x, int y, int w, int h, string k, float scale = 1f) { sprite = s; this.x = x; this.y = y; this.w = w; this.h = h; kind = k; this.scale = scale; }
    public float Scale => scale > 0f ? scale : 1f;
}

// A person on the map. lookout = raises the alarm instead of running.
// patrol >= 0 = a police officer / soldier walking that patrol route (tougher, never runs).
// boss = the level's mini boss (Level 3: the General), who walks the patrol route and fights back.
// toughness = beam hits needed (0 = the default: 1, police 3, boss 12).
public struct Civ
{
    public string sprite; public int x, y; public bool lookout; public int patrol; public bool boss; public int toughness;
    public Civ(string s, int x, int y, bool lookout = false) { sprite = s; this.x = x; this.y = y; this.lookout = lookout; patrol = -1; boss = false; toughness = 0; }
    public static Civ Police(string s, int x, int y, int patrol) => new Civ(s, x, y) { patrol = patrol };
    public static Civ Boss(string s, int x, int y, int patrol) => new Civ(s, x, y) { patrol = patrol, boss = true };
    public bool IsPolice => patrol >= 0 && !boss;
}
