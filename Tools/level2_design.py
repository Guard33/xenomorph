# Level 2 "Fort Maple" layout. Edit the roads / objects / people below, then run:
#     python Tools/level2_design.py Assets/Scripts/Level2Data.cs
# It checks the layout (no overlaps, nothing on roads or water, everyone can reach an exit,
# patrol routes are on open ground), prints a text map, and writes Level2Data.cs.
# Then in Unity: Alien Game > Build Level 2.
#
# Night level: asphalt roads, street lamps in town, floodlights on the base.
# Talia's military sprites are drawn at 2x (M(...) below) so the vehicles and buildings
# match the size of the town houses; their footprints (w, h) are in tiles at that size.
import sys
W, H = 62, 32
OUT = sys.argv[1] if len(sys.argv) > 1 else None

# ---------------------------------------------------------------- roads
road = set()
def hline(y, x0, x1):
    for x in range(x0, x1 + 1): road.add((x, y))
def vline(x, y0, y1):
    for y in range(y0, y1 + 1): road.add((x, y))
hline(22, 0, 32)     # main street (west exit)
vline(32, 12, 22)    # gate road into the base (south gate)
hline(12, 32, 52)    # base road
vline(52, 0, 12)     # north gate road (north exit)
vline(8, 14, 30)     # avenue
hline(28, 8, 24)     # south street
vline(18, 22, 28)    # side street
EXITS = [(0, 22), (52, 0)]

def mask(x, y):
    m = 0
    if (x, y - 1) in road or ((x, y) in EXITS and y == 0): m |= 1
    if (x + 1, y) in road: m |= 2
    if (x, y + 1) in road: m |= 4
    if (x - 1, y) in road or ((x, y) in EXITS and x == 0): m |= 8
    return m

# ---------------------------------------------------------------- ground
ground = [[('.' if (x * 7 + y * 3) % 9 else ',') for x in range(W)] for y in range(H)]
for y in range(2, 20):
    for x in range(29, 61): ground[y][x] = 'c'          # base concrete
for y in range(14, 20):
    for x in range(50, 61): ground[y][x] = 's' if (x + y) % 3 == 0 else 'd'   # vehicle yard
pond = {(2,26),(3,26),(4,26),(1,27),(2,27),(3,27),(4,27),(5,27),(1,28),(2,28),(3,28),(4,28),(5,28),(2,29),(3,29),(4,29)}
for (x, y) in pond: ground[y][x] = '=' if (x + y) % 4 == 0 else '~'

# ---------------------------------------------------------------- objects
objs = []
def O(s, x, y, w, h, k, sc=1): objs.append((s, x, y, w, h, k, sc))
def M(s, x, y, w, h, k): O(s, x, y, w, h, k, 2)     # military piece at 2x
def lamp(x, y): O("street_lamp", x, y, 1, 1, "lamp")
def flood(x, y): O("flood_light", x, y, 1, 1, "lamp")

# Base fence: wire along top and bottom (gaps for the two gates), posts down the sides
for x in range(28, 62, 2):
    if x != 52: O("wire_fence", x, 1, 2, 1, "fence")
    if x != 32: O("wire_fence", x, 20, 2, 1, "fence")
for y in range(2, 20, 2):
    O("fence_post", 28, y, 1, 2, "fence"); O("fence_post", 61, y, 1, 2, "fence")

# North half of the base: HQ in the middle between the two flag offices, helipad in front
M("hq_flag_1", 29, 2, 5, 6, "building")
M("command_building", 29, 8, 4, 4, "building")
M("base_hq_large", 36, 2, 8, 6, "building")
M("hq_flag_2", 46, 2, 5, 6, "building")
M("helicopter", 34, 9, 3, 2, "vehicle"); M("jet", 38, 9, 3, 2, "vehicle"); M("helicopter", 42, 9, 3, 2, "vehicle")
M("supply_depot_1", 46, 9, 2, 2, "building"); M("supply_depot_2", 48, 9, 2, 2, "building")
M("container_upright", 50, 8, 2, 3, "vehicle")
# East of the north gate: guard hut, garages, motor pool
M("guard_hut_2", 53, 2, 2, 2, "building")
M("garage_1", 55, 2, 2, 2, "building"); M("garage_2", 57, 2, 2, 2, "building"); M("garage_3", 59, 2, 2, 2, "building")
M("army_truck", 54, 5, 4, 2, "vehicle"); M("container", 58, 5, 3, 2, "vehicle")
M("tank", 54, 8, 2, 2, "vehicle"); M("tank", 57, 8, 2, 2, "vehicle")
# South half: barracks by the gate, hangars, bunker, vehicle yard
M("barracks_1", 29, 13, 2, 2, "building"); M("barracks_2", 29, 16, 2, 2, "building")
M("guard_hut_1", 33, 18, 2, 2, "building")
M("dirt_mound", 35, 19, 2, 1, "vehicle")               # sandbags by the south gate
M("hangar_small_1", 35, 14, 2, 2, "building"); M("hangar_small_2", 37, 14, 2, 2, "building"); M("hangar_small_3", 39, 14, 2, 2, "building")
M("barracks_camo", 41, 14, 2, 2, "building")
M("bunker", 44, 14, 4, 2, "vehicle")
M("army_truck", 51, 15, 4, 2, "vehicle"); M("container", 56, 15, 3, 2, "vehicle"); M("container_upright", 59, 14, 2, 3, "vehicle")
M("dirt_mound", 51, 18, 2, 1, "vehicle"); M("dirt_mound", 55, 18, 2, 1, "vehicle")
# Floodlights around the base
for (x, y) in [(34, 3), (44, 3), (34, 11), (45, 11), (33, 13), (48, 13), (43, 17), (58, 18), (53, 10), (60, 11)]: flood(x, y)

# Town (west) - main street north side
O("house1", 2, 19, 2, 2, "building"); O("house3", 5, 18, 2, 3, "building")
O("general_store", 10, 19, 2, 2, "building"); O("church", 13, 19, 2, 2, "building")
O("base_office", 16, 19, 2, 2, "building")          # police station (town size, not 2x)
O("house4", 20, 18, 2, 3, "building"); O("house5", 23, 19, 2, 2, "building")
# main street south side
O("house6", 2, 23, 2, 2, "building"); O("house2", 10, 23, 2, 2, "building")
O("fountain_square", 13, 23, 2, 3, "building")
O("house7", 20, 23, 2, 3, "building"); O("house8", 23, 23, 2, 3, "building")
# avenue (north part)
O("house4", 5, 15, 2, 3, "building"); O("house8", 10, 15, 2, 3, "building")
# south street, south side
O("house2", 10, 29, 2, 2, "building"); O("house5", 13, 29, 2, 2, "building"); O("house6", 16, 29, 2, 2, "building")
O("house1", 20, 29, 2, 2, "building"); O("house3", 23, 29, 2, 3, "building")
# Street lamps along the town roads and outside the south gate
for (x, y) in [(4, 21), (9, 21), (15, 21), (19, 21), (25, 21), (7, 23), (17, 23), (27, 23), (30, 23),
               (7, 17), (9, 26), (15, 27), (19, 27), (7, 29), (31, 21), (34, 21)]: lamp(x, y)
# farmland on the north-west outskirts
for (x, y) in [(2, 4), (5, 4), (2, 7), (5, 7)]: O("corn", x, y, 3, 3, "field")
O("crops_green", 10, 4, 3, 3, "field"); O("crops_red", 13, 4, 3, 3, "field"); O("orchard", 16, 4, 3, 3, "field")
O("barn", 10, 8, 3, 3, "building"); O("silo", 14, 9, 2, 2, "building"); O("animal_pen", 17, 8, 3, 3, "building")
# UFO field (south-east) and the fields around it
O("ufo", 44, 25, 2, 2, "ufo")
O("corn", 50, 23, 3, 3, "field"); O("crops_green", 50, 27, 3, 3, "field")
O("corn", 54, 23, 3, 3, "field"); O("crops_red", 54, 27, 3, 3, "field")
# Trees: forest along the edges, a shelter belt west of the base fence
trees = [(0,0),(1,0),(3,0),(6,0),(9,0),(12,0),(15,0),(19,0),(21,0),(22,0),(24,0),(26,0),(30,0),(35,0),(40,0),(45,0),(57,0),(60,0),
         (0,3),(0,6),(0,9),(0,12),(0,15),(21,5),(22,8),(23,11),(20,12),(24,14),(22,16),
         (25,3),(25,6),(25,9),(25,12),(25,15),(25,18),
         (61,22),(61,25),(61,28),(59,31),(56,31),(52,31),(48,31),(44,31),(40,31),(36,31),(32,31),(28,30),(27,27),
         (0,31),(3,31),(6,31),(1,24),(0,20),(11,26),(16,26),(12,27),
         (36,23),(39,25),(33,26),(30,25),(35,28),(41,28),(58,25),(48,24)]
for i, (x, y) in enumerate(trees): O("tree%d" % (i % 4 + 1), x, y, 1, 1, "tree")
# Bushes (no flower beds: the town is on lockdown)
for (x, y) in [(6, 26), (6, 29), (0, 26), (24, 5), (24, 10), (24, 17), (26, 21), (38, 22), (47, 22)]: O("bush", x, y, 1, 1, "deco")

# ---------------------------------------------------------------- people
civs = []   # (sprite, x, y, lookout, patrol)
def C(s, x, y, lookout=False, patrol=-1): civs.append(("folk_" + s, x, y, lookout, patrol))
town = [("shopkeeper", 12, 21), ("baker", 4, 20), ("farmer", 9, 7), ("fisherman", 6, 27), ("innkeeper", 15, 24),
        ("grandpa", 10, 26), ("grandma", 14, 26), ("merchant", 19, 20), ("tailor", 22, 21), ("courier", 26, 23),
        ("painter", 25, 26), ("florist", 16, 25), ("bartender", 9, 24), ("herbalist", 4, 17), ("lamplighter", 7, 22),
        ("king", 21, 27), ("farmer", 15, 11), ("innkeeper", 19, 30)]
for (s, x, y) in town: C(s, x, y)
C("town-crier", 12, 23, lookout=True); C("mayor", 16, 23, lookout=True)
base = [("blacksmith", 35, 6), ("carpenter", 45, 8), ("miner", 53, 7), ("courier", 41, 13), ("merchant", 48, 16), ("farmer", 56, 13)]
for (s, x, y) in base: C(s, x, y)
# Patrols: one police officer from town up to the south gate, three soldiers on the base
patrols = [([(22, 22), (32, 22), (32, 13)], "police"),     # main street -> through the south gate
           ([(33, 12), (51, 12)], "soldier"),               # base road
           ([(52, 2), (52, 11)], "soldier"),                # north gate road
           ([(27, 2), (27, 19)], "soldier")]                # outside the west fence
for i, (p, s) in enumerate(patrols): C(s, p[0][0], p[0][1], patrol=i)
patrols = [p for (p, s) in patrols]
START = (45, 28)

# ---------------------------------------------------------------- checks
problems = []
occ = {}
for (s, x, y, w, h, k, sc) in objs:
    for yy in range(y, y + h):
        for xx in range(x, x + w):
            if not (0 <= xx < W and 0 <= yy < H): problems.append("%s@%d,%d off map" % (s, x, y)); continue
            if (xx, yy) in occ: problems.append("%s@%d,%d overlaps %s" % (s, x, y, occ[(xx, yy)]))
            if (xx, yy) in road: problems.append("%s@%d,%d on road" % (s, x, y))
            if ground[yy][xx] in '~=': problems.append("%s@%d,%d on water" % (s, x, y))
            occ[(xx, yy)] = "%s@%d,%d" % (s, x, y)
BLOCK = {"building", "tree", "vehicle", "fence", "lamp"}
blocked = [[ground[y][x] in '~=' for y in range(H)] for x in range(W)]
for (s, x, y, w, h, k, sc) in objs:
    if k in BLOCK:
        for yy in range(y, y + h):
            for xx in range(x, x + w):
                if 0 <= xx < W and 0 <= yy < H: blocked[xx][yy] = True
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
    if blocked[x][y]: problems.append("%s at %d,%d stands on a blocked tile (%s)" % (s, x, y, occ.get((x, y), "water")))
    elif not any((x, y) in r for r in from_exit): problems.append("%s at %d,%d can't reach an exit" % (s, x, y))
seen_civ = {}
for (s, x, y, lk, p) in civs:
    if (x, y) in seen_civ: problems.append("%s and %s share %d,%d" % (s, seen_civ[(x, y)], x, y))
    seen_civ[(x, y)] = s
if blocked[START[0]][START[1]] or not any(START in r for r in from_exit): problems.append("start blocked/unreachable")
for i, p in enumerate(patrols):
    for a, b in zip(p, p[1:]):
        if a[0] != b[0] and a[1] != b[1]: problems.append("patrol %d leg not straight" % i)
        xs = range(min(a[0], b[0]), max(a[0], b[0]) + 1); ys = range(min(a[1], b[1]), max(a[1], b[1]) + 1)
        for xx in xs:
            for yy in ys:
                if blocked[xx][yy]: problems.append("patrol %d crosses blocked %d,%d (%s)" % (i, xx, yy, occ.get((xx, yy))))
# both exits must be reachable from each other (so people can switch exits)
if EXITS[1] not in from_exit[0]: problems.append("exits not connected")
print("people:", len(civs), "| objects:", len(objs), "| problems:", problems or "none")

# ---------------------------------------------------------------- text map
grid = [row[:] for row in ground]
for y in range(H):
    for x in range(W):
        if (x, y) in road: grid[y][x] = '#'
        elif grid[y][x] in ',': grid[y][x] = '.'
sym = {"building": "B", "field": "f", "tree": "T", "deco": "*", "ufo": "U", "vehicle": "V", "fence": "|", "lamp": "i"}
for (s, x, y, w, h, k, sc) in objs:
    for yy in range(y, y + h):
        for xx in range(x, x + w):
            if 0 <= xx < W and 0 <= yy < H: grid[yy][xx] = sym[k]
for (s, x, y, lk, p) in civs: grid[y][x] = 'P' if p >= 0 else 'L' if lk else 'c'
for e in EXITS: grid[e[1]][e[0]] = 'E'
grid[START[1]][START[0]] = 'S'
print("    " + "".join(str(x % 10) for x in range(W)))
for y in range(H): print("%2d  " % y + "".join(grid[y]))
if problems or OUT is None: sys.exit(1 if problems else 0)

# ---------------------------------------------------------------- write C#
def cs_str(rows): return ",\n".join('        "%s"' % r for r in rows)
G = ["".join(r) for r in ground]
P = ["".join(("%X" % mask(x, y)) if (x, y) in road else '.' for x in range(W)) for y in range(H)]
lines = []
lines.append("// Map data for Level 2 \"Fort Maple\": a fenced army base next to a small town, at night.")
lines.append("// Rows go from the TOP of the map to the bottom. Map is %d x %d tiles." % (W, H))
lines.append("// Town in the west and south, army base (Talia's military sprites, drawn at 2x) in the north-east,")
lines.append("// farmland in the north-west and the UFO field in the south-east.")
lines.append("// Exits: west end of the main street, and the base's north gate.")
lines.append("// Generated by Tools/level2_design.py - change the layout there, not here.")
lines.append("using UnityEngine;\n")
lines.append("public static class Level2Data\n{")
lines.append("    public const int Width = %d;\n    public const int Height = %d;\n" % (W, H))
lines.append("    // Ground: '.' grass, ',' tufts, '~' water, '=' waves, 'c' concrete, 'd' dirt, 's' dirt with specks")
lines.append("    public static readonly string[] Ground = {\n" + cs_str(G) + "\n    };\n")
lines.append("    // Roads (asphalt in this level): '.' none, '0'-'F' road piece (N=1, E=2, S=4, W=8)")
lines.append("    public static readonly string[] Paths = {\n" + cs_str(P) + "\n    };\n")
lines.append("    // Objects: sprite, x, y (top-left tile), w, h (tiles), kind, and drawing scale")
lines.append("    public static readonly Obj[] Objects = {")
def obj_cs(o):
    s, x, y, w, h, k, sc = o
    return '        new Obj("%s", %d, %d, %d, %d, "%s"%s)' % (s, x, y, w, h, k, (", %d" % sc) if sc != 1 else "")
lines.append(",\n".join(obj_cs(o) for o in objs))
lines.append("    };\n")
lines.append("    public static readonly Civ[] Civilians = {")
cl = []
for (s, x, y, lk, p) in civs:
    if p >= 0: cl.append('        Civ.Police("%s", %d, %d, %d)' % (s, x, y, p))
    elif lk: cl.append('        new Civ("%s", %d, %d, true)' % (s, x, y))
    else: cl.append('        new Civ("%s", %d, %d)' % (s, x, y))
lines.append(",\n".join(cl))
lines.append("    };\n")
lines.append("    // Patrol routes for the police officer and the soldiers (they walk back and forth along these tiles)")
lines.append("    public static readonly Vector2Int[][] Patrols = {")
lines.append(",\n".join("        new[] { " + ", ".join("new Vector2Int(%d, %d)" % t for t in p) + " }" for p in patrols))
lines.append("    };\n")
lines.append("    public static readonly Vector2Int Start = new Vector2Int(%d, %d);" % START)
lines.append("    public static readonly Vector2Int[] Exits = { " + ", ".join("new Vector2Int(%d, %d)" % e for e in EXITS) + " };\n")
lines.append("    public static readonly LevelMap Map = new LevelMap")
lines.append("    {")
lines.append('        Name = "Fort Maple", Width = Width, Height = Height, Ground = Ground, Paths = Paths,')
lines.append("        Objects = Objects, Civilians = Civilians, Start = Start, Exits = Exits, Patrols = Patrols, FriendsNeeded = 15,")
lines.append('        Night = true, RoadTiles = "road_",')
lines.append("    };")
lines.append("}")
open(OUT, "w", encoding="utf-8").write("\n".join(lines) + "\n")
print("wrote", OUT)
