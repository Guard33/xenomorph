using System.Collections.Generic;
using UnityEngine;

// Where people can walk on the current level's map, and the way to each exit.
// The map is a grid of tiles: water (except the bridge), buildings and trees are blocked.
// Scared people follow the cheapest route, and roads are cheaper than grass,
// so they run along the roads and only cross the river at the bridge.
// They avoid an exit the alien is guarding or standing in the way of.
public static class RoadNetwork
{
    const float RoadCost = 1f;
    const float GrassCost = 3f;
    const float GuardRadius = 3f;     // alien this close to an exit = exit is blocked
    const float PathRadius = 1.5f;    // alien this close to the way ahead = way is blocked
    const float IgnoreNear = 1.5f;    // the first bit of the route (right next to the person) doesn't count

    static bool[,] blocked;
    static bool[,] sightBlocked;      // buildings and vehicles: guards can't see through them
    static float[][,] distToExit;     // one cost map per exit
    static LevelMap builtFor;         // the map the caches above were made for

    static LevelMap Map => LevelMap.Current;
    public static Vector2Int[] Exits => Map.Exits;

    // Centre of a map tile in world space (map rows start at the top).
    public static Vector2 TileCenter(Vector2Int tile)
    {
        return new Vector2(tile.x + 0.5f, Map.Height - 1 - tile.y + 0.5f);
    }

    public static Vector2Int TileAt(Vector2 world)
    {
        return new Vector2Int(Mathf.FloorToInt(world.x), Map.Height - 1 - Mathf.FloorToInt(world.y));
    }

    public static bool Walkable(Vector2Int t)
    {
        Build();
        return InMap(t) && !blocked[t.x, t.y];
    }

    public static bool Walkable(Vector2 world) => Walkable(TileAt(world));

    static bool InMap(Vector2Int t) => t.x >= 0 && t.y >= 0 && t.x < Map.Width && t.y < Map.Height;

    static bool IsRoad(int x, int y) => Map.Paths[y][x] != '.';

    public static bool BlocksWalking(string kind) => kind == "building" || kind == "tree" || kind == "vehicle" || kind == "fence" || kind == "lamp";

    static readonly Vector2Int[] Dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

    static void Build()
    {
        if (blocked != null && builtFor == Map) return;
        builtFor = Map;
        int W = Map.Width, H = Map.Height;
        blocked = new bool[W, H];
        sightBlocked = new bool[W, H];

        for (int y = 0; y < H; y++)
            for (int x = 0; x < W; x++)
            {
                char g = Map.Ground[y][x];
                if ((g == '~' || g == '=') && Map.Paths[y][x] != '#') blocked[x, y] = true;
            }

        foreach (Obj o in Map.Objects)
        {
            if (!BlocksWalking(o.kind)) continue;
            bool solid = o.kind == "building" || o.kind == "vehicle";
            for (int y = o.y; y < o.y + o.h; y++)
                for (int x = o.x; x < o.x + o.w; x++)
                    if (InMap(new Vector2Int(x, y))) { blocked[x, y] = true; if (solid) sightBlocked[x, y] = true; }
        }

        Vector2Int[] exits = Exits;
        distToExit = new float[exits.Length][,];
        for (int i = 0; i < exits.Length; i++)
            distToExit[i] = CostMap(exits[i]);
    }

    // Cost to reach one exit from every tile (Dijkstra from the exit)
    static float[,] CostMap(Vector2Int exit)
    {
        int W = Map.Width, H = Map.Height;
        float[,] dist = new float[W, H];
        for (int y = 0; y < H; y++)
            for (int x = 0; x < W; x++) dist[x, y] = float.MaxValue;

        var open = new List<Vector2Int> { exit };
        dist[exit.x, exit.y] = 0f;
        while (open.Count > 0)
        {
            int best = 0;
            for (int i = 1; i < open.Count; i++)
                if (dist[open[i].x, open[i].y] < dist[open[best].x, open[best].y]) best = i;
            Vector2Int u = open[best];
            open.RemoveAt(best);
            foreach (Vector2Int d in Dirs)
            {
                Vector2Int v = u + d;
                if (!InMap(v) || blocked[v.x, v.y]) continue;
                float nd = dist[u.x, u.y] + (IsRoad(v.x, v.y) ? RoadCost : GrassCost);
                if (nd < dist[v.x, v.y])
                {
                    if (dist[v.x, v.y] == float.MaxValue) open.Add(v);
                    dist[v.x, v.y] = nd;
                }
            }
        }
        return dist;
    }

    // Can a guard at a see b? Buildings and vehicles in between block the view.
    public static bool LineOfSight(Vector2 a, Vector2 b)
    {
        Build();
        float d = Vector2.Distance(a, b);
        int n = Mathf.CeilToInt(d / 0.3f);
        for (int i = 1; i < n; i++)
        {
            Vector2 p = Vector2.Lerp(a, b, (float)i / n);
            if (Vector2.Distance(p, a) < 0.6f || Vector2.Distance(p, b) < 0.6f) continue;
            Vector2Int t = TileAt(p);
            if (InMap(t) && sightBlocked[t.x, t.y]) return false;
        }
        return true;
    }

    // Shortest walkable route between two points (guards hunting the alien during the alarm)
    public static List<Vector2> RouteTo(Vector2 from, Vector2 to)
    {
        Build();
        int W = Map.Width, H = Map.Height;
        Vector2Int start = NearestWalkable(TileAt(from)), goal = NearestWalkable(TileAt(to));
        List<Vector2> route = new List<Vector2>();
        if (!InMap(start) || !InMap(goal)) return route;
        int[] prev = new int[W * H];
        for (int i = 0; i < prev.Length; i++) prev[i] = -2;
        Queue<Vector2Int> q = new Queue<Vector2Int>();
        q.Enqueue(start);
        prev[start.y * W + start.x] = -1;
        while (q.Count > 0)
        {
            Vector2Int u = q.Dequeue();
            if (u == goal) break;
            foreach (Vector2Int d in Dirs)
            {
                Vector2Int v = u + d;
                if (!InMap(v) || blocked[v.x, v.y] || prev[v.y * W + v.x] != -2) continue;
                prev[v.y * W + v.x] = u.y * W + u.x;
                q.Enqueue(v);
            }
        }
        if (prev[goal.y * W + goal.x] == -2) return route;   // unreachable
        for (int c = goal.y * W + goal.x; c >= 0; c = prev[c]) route.Add(TileCenter(new Vector2Int(c % W, c / W)));
        route.Reverse();
        return route;
    }

    // Route from a point to one exit, then a little way off the map
    public static List<Vector2> RouteToExit(Vector2 from, int exit)
    {
        Build();
        float[,] dist = distToExit[exit];
        List<Vector2> route = new List<Vector2>();
        Vector2Int t = NearestWalkable(TileAt(from));

        // Walk downhill on the cost map until we reach the exit
        int guard = Map.Width * Map.Height;
        while (dist[t.x, t.y] > 0f && guard-- > 0)
        {
            route.Add(TileCenter(t));
            Vector2Int next = t;
            foreach (Vector2Int d in Dirs)
            {
                Vector2Int v = t + d;
                if (InMap(v) && dist[v.x, v.y] < dist[next.x, next.y]) next = v;
            }
            if (next == t) break;   // can't reach this exit
            t = next;
        }
        route.Add(TileCenter(t));

        Vector2 outward = Vector2.zero;
        if (t.x >= Map.Width - 1) outward = Vector2.right;
        else if (t.x <= 0) outward = Vector2.left;
        else if (t.y >= Map.Height - 1) outward = Vector2.down;
        else if (t.y <= 0) outward = Vector2.up;
        route.Add(TileCenter(t) + outward * 1.5f);
        return route;
    }

    // Is the alien guarding this exit, or standing on the way there?
    public static bool IsBlocked(List<Vector2> route, int exit, Vector2 from, Vector2 alien)
    {
        if (Vector2.Distance(alien, TileCenter(Exits[exit])) < GuardRadius) return true;
        foreach (Vector2 p in route)
            if (Vector2.Distance(p, from) > IgnoreNear && Vector2.Distance(p, alien) < PathRadius) return true;
        return false;
    }

    // How close the route gets to the alien (bigger = safer)
    static float Clearance(List<Vector2> route, Vector2 from, Vector2 alien)
    {
        float min = float.MaxValue;
        foreach (Vector2 p in route)
            if (Vector2.Distance(p, from) > IgnoreNear) min = Mathf.Min(min, Vector2.Distance(p, alien));
        return min;
    }

    // Pick the cheapest exit the alien isn't blocking. If the alien blocks every exit,
    // take the one whose route keeps furthest from it.
    public static int ChooseExit(Vector2 from, Vector2 alien, out List<Vector2> route)
    {
        Build();
        Vector2Int start = NearestWalkable(TileAt(from));
        int best = -1, safest = 0;
        float bestCost = float.MaxValue, bestClear = -1f;
        route = null;
        List<Vector2> safestRoute = null;
        for (int i = 0; i < Exits.Length; i++)
        {
            float cost = distToExit[i][start.x, start.y];
            if (cost == float.MaxValue) continue;
            List<Vector2> r = RouteToExit(from, i);
            if (!IsBlocked(r, i, from, alien) && cost < bestCost) { best = i; bestCost = cost; route = r; }
            float clear = Mathf.Min(Clearance(r, from, alien), Vector2.Distance(alien, TileCenter(Exits[i])));
            if (clear > bestClear) { safest = i; bestClear = clear; safestRoute = r; }
        }
        if (best >= 0) return best;
        route = safestRoute ?? RouteToExit(from, 0);
        return safest;
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
