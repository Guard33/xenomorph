using System.Collections.Generic;
using UnityEngine;

// Finds the shortest way along the roads from a point to the closest exit.
public static class RoadNetwork
{
    // Centre of a map tile in world space (map rows start at the top).
    public static Vector2 TileCenter(Vector2Int tile)
    {
        return new Vector2(tile.x + 0.5f, Level1Data.Height - 1 - tile.y + 0.5f);
    }

    public static List<Vector2> RouteToClosestExit(Vector2 from)
    {
        Vector2Int[] nodes = Level1Data.RoadNodes;
        int n = nodes.Length;
        Vector2[] pos = new Vector2[n];
        for (int i = 0; i < n; i++) pos[i] = TileCenter(nodes[i]);

        List<int>[] links = new List<int>[n];
        for (int i = 0; i < n; i++) links[i] = new List<int>();
        foreach (Vector2Int e in Level1Data.RoadEdges)
        {
            links[e.x].Add(e.y);
            links[e.y].Add(e.x);
        }

        // Distance from every road point to its closest exit (Dijkstra from all exits)
        float[] dist = new float[n];
        int[] next = new int[n];
        bool[] done = new bool[n];
        for (int i = 0; i < n; i++) { dist[i] = float.MaxValue; next[i] = -1; }
        foreach (int e in Level1Data.ExitNodes) dist[e] = 0f;

        for (int step = 0; step < n; step++)
        {
            int u = -1;
            for (int i = 0; i < n; i++)
                if (!done[i] && (u < 0 || dist[i] < dist[u])) u = i;
            if (u < 0 || dist[u] == float.MaxValue) break;
            done[u] = true;
            foreach (int v in links[u])
            {
                float d = dist[u] + Vector2.Distance(pos[u], pos[v]);
                if (d < dist[v]) { dist[v] = d; next[v] = u; }
            }
        }

        // Find the best place to join a road: run to the nearest road first,
        // then take the direction along that road that reaches an exit soonest
        float bestTrip = float.MaxValue;
        Vector2 join = from;
        int start = 0;
        foreach (Vector2Int e in Level1Data.RoadEdges)
        {
            Vector2 a = pos[e.x], b = pos[e.y];
            Vector2 ab = b - a;
            float t = ab.sqrMagnitude > 0f ? Mathf.Clamp01(Vector2.Dot(from - a, ab) / ab.sqrMagnitude) : 0f;
            Vector2 p = a + ab * t;
            float toRoad = Vector2.Distance(from, p) * 3f;   // cutting across the grass is avoided
            float viaA = toRoad + Vector2.Distance(p, a) + dist[e.x];
            float viaB = toRoad + Vector2.Distance(p, b) + dist[e.y];
            if (viaA < bestTrip) { bestTrip = viaA; join = p; start = e.x; }
            if (viaB < bestTrip) { bestTrip = viaB; join = p; start = e.y; }
        }

        List<Vector2> route = new List<Vector2> { join };
        int node = start;
        int last = start;
        while (node >= 0)
        {
            route.Add(pos[node]);
            last = node;
            node = next[node];
        }

        // Keep running off the map through the exit
        Vector2Int exitTile = nodes[last];
        Vector2 outward = Vector2.zero;
        if (exitTile.x >= Level1Data.Width - 1) outward = Vector2.right;
        else if (exitTile.x <= 0) outward = Vector2.left;
        else if (exitTile.y >= Level1Data.Height - 1) outward = Vector2.down;
        else if (exitTile.y <= 0) outward = Vector2.up;
        route.Add(pos[last] + outward * 1.5f);
        return route;
    }
}
