# Level 2 "Fort Maple" layout. Edit the roads / objects / people below, then run:
#     python Tools/level2_design.py Assets/Scripts/Level2Data.cs
# It checks the layout (no overlaps, nothing on roads or water, everyone can reach an exit,
# police routes are on open ground), prints a text map, and writes Level2Data.cs.
# Then in Unity: Alien Game > Build Level 2.
import heapq, sys
W, H = 48, 30
OUT = sys.argv[1]

# ---------------------------------------------------------------- roads
road = set()
def hline(y, x0, x1):
    for x in range(x0, x1 + 1): road.add((x, y))
def vline(x, y0, y1):
    for y in range(y0, y1 + 1): road.add((x, y))
hline(20, 0, 31)     # main street (west exit)
vline(31, 10, 20)    # gate road into the base
hline(10, 31, 40)    # base road
vline(40, 0, 10)     # north gate road (north exit)
vline(8, 12, 28)     # avenue
hline(26, 8, 24)     # south street
vline(18, 20, 26)    # side street
EXITS = [(0, 20), (40, 0)]

def mask(x, y):
    m = 0
    if (x, y - 1) in road or ((x, y) in EXITS and y == 0): m |= 1
    if (x + 1, y) in road: m |= 2
    if (x, y + 1) in road: m |= 4
    if (x - 1, y) in road or ((x, y) in EXITS and x == 0): m |= 8
    return m

# ---------------------------------------------------------------- ground
ground = [[('.' if (x * 7 + y * 3) % 9 else ',') for x in range(W)] for y in range(H)]
for y in range(3, 17):
    for x in range(27, 46): ground[y][x] = 'c'          # base concrete
for y in range(13, 17):
    for x in range(41, 46): ground[y][x] = 's' if (x + y) % 3 == 0 else 'd'   # vehicle yard
pond = {(2,24),(3,24),(4,24),(1,25),(2,25),(3,25),(4,25),(5,25),(1,26),(2,26),(3,26),(4,26),(5,26),(2,27),(3,27),(4,27)}
for (x, y) in pond: ground[y][x] = '=' if (x + y) % 4 == 0 else '~'

# ---------------------------------------------------------------- objects
objs = []
def O(s, x, y, w, h, k): objs.append((s, x, y, w, h, k))
# Base fence: wire along top and bottom (gaps for the two roads), posts down the sides
for x in list(range(26, 40, 2)) + [41, 43, 45]: O("wire_fence", x, 2, 2, 1, "fence")
for x in [26, 28] + list(range(32, 46, 2)): O("wire_fence", x, 17, 2, 1, "fence")
for y in range(3, 17, 2):
    O("fence_post", 26, y, 1, 2, "fence"); O("fence_post", 46, y, 1, 2, "fence")
# Base buildings and vehicles
O("hq_flag_1", 27, 4, 3, 3, "building")
O("command_building", 27, 8, 2, 2, "building")
O("barracks_1", 27, 13, 1, 1, "building"); O("barracks_2", 28, 13, 1, 1, "building"); O("barracks_camo", 29, 13, 1, 1, "building")
O("guard_hut_2", 29, 16, 1, 1, "building"); O("guard_hut_1", 32, 16, 1, 1, "building")
O("dirt_mound", 33, 16, 1, 1, "vehicle")          # sandbags by the gate
O("base_hq_large", 33, 4, 4, 3, "building")
O("helicopter", 33, 8, 2, 1, "vehicle"); O("jet", 36, 8, 2, 1, "vehicle")
O("hq_flag_2", 42, 4, 3, 3, "building")
O("supply_depot_1", 42, 8, 1, 1, "building"); O("supply_depot_2", 43, 8, 1, 1, "building")
O("container_upright", 45, 8, 1, 2, "vehicle")
O("hangar_small_1", 33, 12, 1, 1, "building"); O("hangar_small_2", 34, 12, 1, 1, "building"); O("hangar_small_3", 35, 12, 1, 1, "building")
O("tank", 37, 12, 1, 1, "vehicle"); O("bunker", 37, 15, 2, 1, "vehicle")
O("army_truck", 41, 14, 2, 1, "vehicle")
O("garage_1", 42, 12, 1, 1, "building"); O("garage_2", 43, 12, 1, 1, "building"); O("garage_3", 44, 12, 1, 1, "building")
O("container", 43, 15, 2, 1, "vehicle")
# Town: main street north side
O("house1", 2, 18, 2, 2, "building"); O("house3", 5, 17, 2, 3, "building")
O("general_store", 10, 18, 2, 2, "building"); O("church", 13, 18, 2, 2, "building")
O("base_office", 16, 18, 2, 2, "building")          # police station
O("house4", 20, 17, 2, 3, "building"); O("house5", 23, 18, 2, 2, "building")
# main street south side
O("house6", 2, 21, 2, 2, "building"); O("house2", 10, 21, 2, 2, "building")
O("fountain_square", 13, 21, 2, 3, "building")
O("house7", 20, 21, 2, 3, "building"); O("house8", 23, 21, 2, 3, "building")
# avenue (north part)
O("house4", 5, 13, 2, 3, "building"); O("house8", 10, 13, 2, 3, "building")
# south street, south side
O("house2", 10, 27, 2, 2, "building"); O("house5", 13, 27, 2, 2, "building"); O("house6", 16, 27, 2, 2, "building")
O("house1", 20, 27, 2, 2, "building"); O("house3", 23, 27, 2, 3, "building")
# farmland on the north-west outskirts
for (x, y) in [(2, 2), (5, 2), (2, 5), (5, 5)]: O("corn", x, y, 3, 3, "field")
O("crops_green", 10, 2, 3, 3, "field"); O("crops_red", 13, 2, 3, 3, "field"); O("orchard", 16, 2, 3, 3, "field")
O("barn", 10, 6, 3, 3, "building"); O("silo", 14, 7, 2, 2, "building"); O("animal_pen", 17, 6, 3, 3, "building")
# UFO field (south-east) and its crops
O("ufo", 36, 24, 2, 2, "ufo")
O("corn", 40, 22, 3, 3, "field"); O("crops_green", 40, 26, 3, 3, "field")
# Trees: forest along the edges, a shelter belt outside the base fence, shade trees in the park
trees = [(0,0),(1,0),(3,0),(6,0),(9,0),(12,0),(15,0),(19,0),(21,0),(22,0),(24,0),(0,3),(0,6),(0,9),(0,12),(0,15),
         (21,3),(22,6),(23,9),(20,10),(24,12),(22,14),
         (25,3),(25,6),(25,9),(25,12),(25,15),
         (47,0),(47,3),(44,0),(47,19),(47,22),(47,25),(45,28),(42,29),(39,29),(35,29),(31,29),(28,28),(27,25),
         (0,29),(3,29),(6,29),(1,22),(0,18),
         (11,24),(16,24),(12,25),(30,23),(33,20),(29,27)]
for i, (x, y) in enumerate(trees): O("tree%d" % (i % 4 + 1), x, y, 1, 1, "tree")
# Decorations: flower beds by houses, bushes along the fence and by the pond
decos = [("flowers", 4, 18), ("flowers2", 7, 18), ("flowers", 12, 18), ("flowers2", 19, 18), ("flowers", 22, 18),
         ("flowers2", 4, 22), ("flowers", 12, 22), ("flowers2", 22, 22), ("flowers", 9, 28), ("flowers2", 19, 27),
         ("bush", 6, 24), ("bush", 6, 27), ("bush", 0, 24), ("bush", 24, 3), ("bush", 24, 8), ("bush", 24, 16),
         ("flowers", 13, 25), ("flowers2", 15, 25), ("bush", 34, 21), ("flowers", 38, 21), ("bush", 26, 19)]
for (s, x, y) in decos: O(s, x, y, 1, 1, "deco")

# ---------------------------------------------------------------- people
civs = []   # (sprite, x, y, lookout, patrol)
def C(s, x, y, lookout=False, patrol=-1): civs.append(("folk_" + s, x, y, lookout, patrol))
town = [("shopkeeper", 12, 19), ("baker", 4, 19), ("farmer", 9, 5), ("fisherman", 6, 25), ("innkeeper", 15, 22),
        ("grandpa", 10, 24), ("grandma", 14, 24), ("merchant", 19, 19), ("tailor", 22, 19), ("courier", 26, 21),
        ("painter", 25, 24), ("florist", 16, 23), ("bartender", 9, 22), ("herbalist", 4, 15), ("lamplighter", 7, 21),
        ("king", 21, 25), ("farmer", 15, 9), ("innkeeper", 19, 28)]
for (s, x, y) in town: C(s, x, y)
C("town-crier", 12, 21, lookout=True); C("mayor", 16, 21, lookout=True)
base = [("blacksmith", 30, 7), ("carpenter", 32, 6), ("miner", 38, 9), ("scholar", 35, 14), ("noble-lady", 44, 11), ("fisherman", 39, 16)]
for (s, x, y) in base: C(s, x, y)
patrols = [[(2, 20), (16, 20)], [(17, 20), (30, 20)], [(8, 13), (8, 25)], [(9, 26), (24, 26)]]
for i, p in enumerate(patrols): C("police", p[0][0], p[0][1], patrol=i)
START = (37, 27)

# ---------------------------------------------------------------- checks
problems = []
occ = {}
for (s, x, y, w, h, k) in objs:
    for yy in range(y, y + h):
        for xx in range(x, x + w):
            if not (0 <= xx < W and 0 <= yy < H): problems.append("%s@%d,%d off map" % (s, x, y)); continue
            if (xx, yy) in occ: problems.append("%s@%d,%d overlaps %s" % (s, x, y, occ[(xx, yy)]))
            if (xx, yy) in road: problems.append("%s@%d,%d on road" % (s, x, y))
            if ground[yy][xx] in '~=': problems.append("%s@%d,%d on water" % (s, x, y))
            occ[(xx, yy)] = "%s@%d,%d" % (s, x, y)
BLOCK = {"building", "tree", "vehicle", "fence"}
blocked = [[ground[y][x] in '~=' for y in range(H)] for x in range(W)]
for (s, x, y, w, h, k) in objs:
    if k in BLOCK:
        for yy in range(y, y + h):
            for xx in range(x, x + w): blocked[xx][yy] = True
for e in EXITS:
    if blocked[e[0]][e[1]]: problems.append("exit %s blocked" % (e,))
def reach(src):
    seen = {src}; st = [src]
    while st:
        x, y = st.pop()
        for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
            n = (x + dx, y + dy)
            if 0 <= n[0] < W and 0 <= n[1] < H and n not in seen and not blocked[n[0]][n[1]]:
                seen.add(n); st.append(n)
    return seen
from_exit = [reach(e) for e in EXITS]
for (s, x, y, lk, p) in civs:
    if blocked[x][y]: problems.append("%s at %d,%d stands on a blocked tile" % (s, x, y))
    elif not any((x, y) in r for r in from_exit): problems.append("%s at %d,%d can't reach an exit" % (s, x, y))
    for (o, ox, oy, w, h, k) in objs:
        if k in ("field",) and False: pass
if blocked[START[0]][START[1]] or not any(START in r for r in from_exit): problems.append("start blocked/unreachable")
for i, p in enumerate(patrols):
    for a, b in zip(p, p[1:]):
        if a[0] != b[0] and a[1] != b[1]: problems.append("patrol %d leg not straight" % i)
        xs = range(min(a[0], b[0]), max(a[0], b[0]) + 1); ys = range(min(a[1], b[1]), max(a[1], b[1]) + 1)
        for xx in xs:
            for yy in ys:
                if blocked[xx][yy]: problems.append("patrol %d crosses blocked %d,%d" % (i, xx, yy))
# both exits must be reachable from each other (so people can switch exits)
if EXITS[1] not in from_exit[0]: problems.append("exits not connected")
print("people:", len(civs), "| objects:", len(objs), "| problems:", problems or "none")

# ---------------------------------------------------------------- text map
grid = [row[:] for row in ground]
for y in range(H):
    for x in range(W):
        if (x, y) in road: grid[y][x] = '#'
        elif grid[y][x] in ',': grid[y][x] = '.'
sym = {"building": "B", "field": "f", "tree": "T", "deco": "*", "ufo": "U", "vehicle": "V", "fence": "|"}
for (s, x, y, w, h, k) in objs:
    for yy in range(y, y + h):
        for xx in range(x, x + w): grid[yy][xx] = sym[k]
for (s, x, y, lk, p) in civs: grid[y][x] = 'P' if p >= 0 else 'L' if lk else 'c'
for e in EXITS: grid[e[1]][e[0]] = 'E'
grid[START[1]][START[0]] = 'S'
print("    " + "".join(str(x % 10) for x in range(W)))
for y in range(H): print("%2d  " % y + "".join(grid[y]))

# ---------------------------------------------------------------- write C#
def cs_str(rows): return ",\n".join('        "%s"' % r for r in rows)
G = ["".join(r) for r in ground]
P = ["".join(("%X" % mask(x, y)) if (x, y) in road else '.' for x in range(W)) for y in range(H)]
lines = []
lines.append("// Map data for Level 2 \"Fort Maple\": a fenced army base next to a small town.")
lines.append("// Rows go from the TOP of the map to the bottom. Map is 48 x 30 tiles.")
lines.append("// Town in the west and south, army base (Talia's military sprites) in the north-east,")
lines.append("// farmland in the north-west and the UFO field in the south-east.")
lines.append("// Exits: west end of the main street, and the base's north gate.")
lines.append("// Generated by Tools/level2_design.py - change the layout there, not here.")
lines.append("using UnityEngine;\n")
lines.append("public static class Level2Data\n{")
lines.append("    public const int Width = %d;\n    public const int Height = %d;\n" % (W, H))
lines.append("    // Ground: '.' grass, ',' tufts, '~' water, '=' waves, 'c' concrete, 'd' dirt, 's' dirt with specks")
lines.append("    public static readonly string[] Ground = {\n" + cs_str(G) + "\n    };\n")
lines.append("    // Dirt roads: '.' none, '0'-'F' road piece (N=1, E=2, S=4, W=8)")
lines.append("    public static readonly string[] Paths = {\n" + cs_str(P) + "\n    };\n")
groups = []
lines.append("    public static readonly Obj[] Objects = {")
lines.append(",\n".join('        new Obj("%s", %d, %d, %d, %d, "%s")' % o for o in objs))
lines.append("    };\n")
lines.append("    public static readonly Civ[] Civilians = {")
cl = []
for (s, x, y, lk, p) in civs:
    if p >= 0: cl.append('        Civ.Police("%s", %d, %d, %d)' % (s, x, y, p))
    elif lk: cl.append('        new Civ("%s", %d, %d, true)' % (s, x, y))
    else: cl.append('        new Civ("%s", %d, %d)' % (s, x, y))
lines.append(",\n".join(cl))
lines.append("    };\n")
lines.append("    // Police patrol routes (they walk back and forth along these tiles)")
lines.append("    public static readonly Vector2Int[][] Patrols = {")
lines.append(",\n".join("        new[] { " + ", ".join("new Vector2Int(%d, %d)" % t for t in p) + " }" for p in patrols))
lines.append("    };\n")
lines.append("    public static readonly Vector2Int Start = new Vector2Int(%d, %d);" % START)
lines.append("    public static readonly Vector2Int[] Exits = { " + ", ".join("new Vector2Int(%d, %d)" % e for e in EXITS) + " };\n")
lines.append("    public static readonly LevelMap Map = new LevelMap")
lines.append("    {")
lines.append('        Name = "Fort Maple", Width = Width, Height = Height, Ground = Ground, Paths = Paths,')
lines.append("        Objects = Objects, Civilians = Civilians, Start = Start, Exits = Exits, Patrols = Patrols, FriendsNeeded = 15,")
lines.append("    };")
lines.append("}")
open(OUT, "w", encoding="utf-8").write("\n".join(lines) + "\n")
