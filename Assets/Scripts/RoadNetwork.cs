using System.Collections.Generic;
using UnityEngine;

// Where people can walk on the Level 1 map, and the way to the closest exit.
// The map is a grid of tiles: water (except the bridge), buildings and trees are blocked.
// Scared people follow the cheapest route, and roads are cheaper than grass,
// so they run along the roads and only cross the river at the bridge.
public static class RoadNetwork
{
    const float RoadCost = 1f;
    const float GrassCost = 3f;

    static bool[,] blocked;
    static float[,] distToExit;

    // Centre of a map tile in world space (map rows start at the top).
    public static Vector2 TileCenter(Vector2Int tile)
    {
        return new Vector2(tile.x + 0.5f, Level1Data.Height - 1 - tile.y + 0.5f);
    }

    public static Vector2Int TileAt(Vector2 world)
    {
        return new Vector2Int(Mathf.FloorToInt(world.x), Level1Data.Height - 1 - Mathf.FloorToInt(world.y));
    }

    public static bool Walkable(Vector2Int t)
    {
        Build();
        return InMap(t) && !blocked[t.x, t.y];
    }

    public static bool Walkable(Vector2 world) => Walkable(TileAt(world));

    static bool InMap(Vector2Int t) => t.x >= 0 && t.y >= 0 && t.x < Level1Data.Width && t.y < Level1Data.Height;

    static bool IsRoad(int x, int y) => Level1Data.Paths[y][x] != '.';

    static void Build()
    {
        if (blocked != null) return;
        int W = Level1Data.Width, H = Level1Data.Height;
        blocked = new bool[W, H];

        for (int y = 0; y < H; y++)
            for (int x = 0; x < W; x++)
            {
                char g = Level1Data.Ground[y][x];
                if ((g == '~' || g == '=') && Level1Data.Paths[y][x] != '#') blocked[x, y] = true;
            }

        foreach (Level1Data.Obj o in Level1Data.Objects)
        {
            if (o.kind != "building" && o.kind != "tree") continue;
            for (int y = o.y; y < o.y + o.h; y++)
                for (int x = o.x; x < o.x + o.w; x++)
                    if (InMap(new Vector2Int(x, y))) blocked[x, y] = true;
        }

        // Cost to reach the closest exit from every tile (Dijkstra from both exits)
        distToExit = new float[W, H];
        for (int y = 0; y < H; y++)
            for (int x = 0; x < W; x++) distToExit[x, y] = float.MaxValue;

        var open = new List<Vector2Int>();
        foreach (Vector2Int e in new[] { Level1Data.ExitA, Level1Data.ExitB })
        {
            distToExit[e.x, e.y] = 0f;
            open.Add(e);
        }
        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        while (open.Count > 0)
        {
            int best = 0;
            for (int i = 1; i < open.Count; i++)
                if (distToExit[open[i].x, open[i].y] < distToExit[open[best].x, open[best].y]) best = i;
            Vector2Int u = open[best];
            open.RemoveAt(best);
            foreach (Vector2Int d in dirs)
            {
                Vector2Int v = u + d;
                if (!InMap(v) || blocked[v.x, v.y]) continue;
                float nd = distToExit[u.x, u.y] + (IsRoad(v.x, v.y) ? RoadCost : GrassCost);
                if (nd < distToExit[v.x, v.y])
                {
                    if (distToExit[v.x, v.y] == float.MaxValue) open.Add(v);
                    distToExit[v.x, v.y] = nd;
                }
            }
        }
    }

    public static List<Vector2> RouteToClosestExit(Vector2 from)
    {
        Build();
        List<Vector2> route = new List<Vector2>();
        Vector2Int t = NearestWalkable(TileAt(from));
        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        // Walk downhill on the cost map until we reach an exit
        int guard = Level1Data.Width * Level1Data.Height;
        while (distToExit[t.x, t.y] > 0f && guard-- > 0)
        {
            route.Add(TileCenter(t));
            Vector2Int next = t;
            foreach (Vector2Int d in dirs)
            {
                Vector2Int v = t + d;
                if (InMap(v) && distToExit[v.x, v.y] < distToExit[next.x, next.y]) next = v;
            }
            if (next == t) break;   // nowhere to go (shouldn't happen)
            t = next;
        }
        route.Add(TileCenter(t));

        // Keep running off the map through the exit
        Vector2 outward = Vector2.zero;
        if (t.x >= Level1Data.Width - 1) outward = Vector2.right;
        else if (t.x <= 0) outward = Vector2.left;
        else if (t.y >= Level1Data.Height - 1) outward = Vector2.down;
        else if (t.y <= 0) outward = Vector2.up;
        route.Add(TileCenter(t) + outward * 1.5f);
        return route;
    }

    static Vector2Int NearestWalkable(Vector2Int t)
    {
        if (Walkable(t)) return t;
        for (int r = 1; r < 6; r++)
            for (int dy = -r; dy <= r; dy++)
                for (int dx = -r; dx <= r; dx++)
                {
                    Vector2Int v = new Vector2Int(t.x + dx, t.y + dy);
                    if (Walkable(v)) return v;
                }
        return t;
    }
}
