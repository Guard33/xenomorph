# Level 3 "Fort Maple Command" layout. Edit the roads / objects / people below, then run:
#     python Tools/level3_design.py Assets/Scripts/Level3Data.cs
# It checks the layout (no overlaps, nothing on roads, everyone can reach an exit, patrol routes
# are on open ground), prints a text map, and writes Level3Data.cs.
# Then in Unity: Alien Game > Build Level 3.
#
# ---------------------------------------------------------------- the story of the map
# After Fort Maple's town, the alien slips through the rear gate into the inner base, where the
# army runs the operation. It is night and all concrete. Every area has a job:
#   South gate (start)   - the alien breaks in by the motor pool.
#   Motor pool (SW)      - garages, tanks and trucks on a dirt yard; mechanics working late.
#   Supply + ammo (W)    - depots, containers and an ammo bunker behind sandbags; a sentry guards it.
#   Barracks (NW)        - six barracks with the cook and the mess steward; a soldier walks the lane.
#   Command compound (N) - walled; HQ, flag offices, command centre and bunker around the parade
#                          ground. THE GENERAL (mini boss) paces the parade ground; two sentries
#                          hold the compound gate. The north gate behind HQ is Exit A.
#   Training ground (NE) - sandbag lines and tanks on dirt; a soldier walks the firing line.
#   Airfield (E)         - hangars with their aircraft parked in front, the runway, and the east
#                          gate (Exit B), where people try to fly out.
#   Two cross roads join everything; soldiers patrol both. Floodlights light every work area.
# Talia's military sprites are drawn at 2x (M(...)); footprints (w, h) are tiles at that size.
import sys
W, H = 72, 44
OUT = sys.argv[1] if len(sys.argv) > 1 else None

# ---------------------------------------------------------------- roads
road = set()
def hline(y, x0, x1):
    for x in range(x0, x1 + 1): road.add((x, y))
def vline(x, y0, y1):
    for y in range(y0, y1 + 1): road.add((x, y))
vline(36, 0, 43)     # main avenue: south gate -> compound gate -> north gate (Exit A)
hline(22, 2, 71)     # north cross road -> east gate (Exit B)
hline(35, 2, 69)     # south cross road along the motor pool and the runway
hline(39, 37, 69)    # runway
vline(2, 22, 35)     # west lane by the supply depot
EXITS = [(36, 0), (71, 22)]
START = (36, 40)

def mask(x, y):
    m = 0
    if (x, y - 1) in road or ((x, y) in EXITS and y == 0): m |= 1
    if (x + 1, y) in road or ((x, y) in EXITS and x == W - 1): m |= 2
    if (x, y + 1) in road: m |= 4
    if (x - 1, y) in road: m |= 8
    if (x, y) == (36, 43): m |= 4      # the avenue runs out through the south gate
    return m

# ---------------------------------------------------------------- ground
ground = [['c' for x in range(W)] for y in range(H)]
def dirt(x0, y0, x1, y1):
    for y in range(y0, y1 + 1):
        for x in range(x0, x1 + 1): ground[y][x] = 's' if (x + y) % 3 == 0 else 'd'
dirt(3, 36, 33, 42)        # motor pool yard
dirt(58, 1, 70, 19)        # training ground
dirt(3, 29, 15, 33)        # ammo dump
for y in range(1, 21):     # the barracks stand on a lawn
    for x in range(1, 15): ground[y][x] = ',' if (x * 7 + y * 3) % 9 == 0 else '.'

# ---------------------------------------------------------------- objects
objs = []
def O(s, x, y, w, h, k, sc=1): objs.append((s, x, y, w, h, k, sc))
def M(s, x, y, w, h, k): O(s, x, y, w, h, k, 2)
def flood(x, y): O("flood_light", x, y, 1, 1, "lamp")

# Outer wall of the base (gaps: north gate, east gate, south gate)
for x in range(0, 72, 2):
    if x != 36: O("wire_fence", x, 0, 2, 1, "fence"); O("wire_fence", x, 43, 2, 1, "fence")
for y in range(1, 43, 2):
    O("fence_post", 0, y, 1, 2, "fence")
    if y != 21: O("fence_post", 71, y, 1, 2, "fence")

# --- Command compound: inner wall with one gate on the avenue
for x in range(16, 56, 2):
    if x != 36: O("wire_fence", x, 21, 2, 1, "fence")
for y in range(1, 21, 2):
    O("fence_post", 16, y, 1, 2, "fence"); O("fence_post", 56, y, 1, 2, "fence")
M("base_hq_large", 20, 2, 8, 6, "building")
M("hq_flag_1", 40, 2, 5, 6, "building"); M("hq_flag_2", 46, 2, 5, 6, "building")
M("command_building", 18, 12, 4, 4, "building"); M("command_building", 51, 8, 4, 4, "building")
M("bunker", 50, 14, 4, 2, "vehicle")                       # command bunker
for (x, y) in [(23, 11), (45, 11), (34, 18), (39, 18)]: flood(x, y)   # parade ground + gate

# --- Barracks quarter (NW)
for (i, (x, y)) in enumerate([(2, 2), (7, 2), (2, 7), (7, 7), (2, 12), (7, 12), (2, 17), (7, 17)]):
    M(["barracks_1", "barracks_2", "barracks_camo"][i % 3], x, y, 2, 2, "building")
flood(5, 11); flood(10, 5)

# --- Supply depot and ammo dump (W)
M("supply_depot_1", 5, 24, 2, 2, "building"); M("supply_depot_2", 9, 24, 2, 2, "building")
M("container", 13, 24, 3, 2, "vehicle"); M("container_upright", 18, 24, 2, 3, "vehicle")
M("container", 22, 24, 3, 2, "vehicle"); M("army_truck", 27, 24, 4, 2, "vehicle")   # loading up
M("bunker", 5, 29, 4, 2, "vehicle")                        # ammo bunker
M("dirt_mound", 11, 29, 2, 1, "vehicle"); M("dirt_mound", 11, 32, 2, 1, "vehicle")  # sandbags
flood(20, 30); flood(30, 29)

# --- Motor pool (SW) by the south gate
M("garage_1", 4, 37, 2, 2, "building"); M("garage_2", 8, 37, 2, 2, "building"); M("garage_3", 12, 37, 2, 2, "building")
M("tank", 17, 37, 2, 2, "vehicle"); M("tank", 21, 37, 2, 2, "vehicle")
M("army_truck", 25, 38, 4, 2, "vehicle")
M("guard_hut_1", 32, 38, 2, 2, "building")                 # south gate hut (searchlight)
M("dirt_mound", 38, 41, 2, 1, "vehicle")                   # sandbags at the gate
flood(15, 36)

# --- Airfield (E): each aircraft parked in front of its hangar, runway below
for (i, x) in enumerate([40, 46, 52, 58, 64]):
    M("hangar_small_%d" % (i % 3 + 1), x, 24, 2, 2, "building")
for (i, x) in enumerate([39, 45, 51, 57, 63]):
    M("helicopter" if i % 2 == 0 else "jet", x, 28, 3, 2, "vehicle")
M("guard_hut_2", 67, 18, 2, 2, "building")                 # east gate hut (searchlight)
for (x, y) in [(44, 26), (56, 26), (48, 41), (60, 41)]: flood(x, y)

# --- Training ground (NE): sandbag firing lines and tanks
for (x, y) in [(59, 5), (64, 5), (59, 10), (64, 10)]: M("dirt_mound", x, y, 2, 1, "vehicle")
M("tank", 59, 15, 2, 2, "vehicle"); M("tank", 64, 15, 2, 2, "vehicle")
flood(62, 8); flood(69, 12)

# ---------------------------------------------------------------- people
civs = []   # (sprite, x, y, lookout, patrol, boss, toughness)
def C(s, x, y, lookout=False, patrol=-1, boss=False, tough=0): civs.append(("folk_" + s, x, y, lookout, patrol, boss, tough))
# Base staff (1 hit): people stand where they work
for (s, x, y) in [("baker", 5, 6), ("innkeeper", 10, 10),                    # mess hall cook and steward
                  ("scholar", 30, 11), ("courier", 38, 10), ("tailor", 31, 5),  # HQ clerks and the messenger
                  ("miner", 15, 32), ("merchant", 24, 29),                    # ammo dump, quartermaster
                  ("blacksmith", 16, 40), ("carpenter", 7, 41), ("painter", 29, 41),   # motor pool mechanics
                  ("carpenter", 43, 32), ("painter", 55, 32), ("courier", 62, 33),     # airfield ground crew
                  ("miner", 62, 7)]:                                          # training ground range officer
    C(s, x, y)
# Sentries (2 hits): stand guard and raise the alarm, like the lookouts in town
for (x, y) in [(35, 19), (38, 19), (16, 28), (66, 32), (69, 20)]: C("soldier", x, y, lookout=True, tough=2)
# Patrols: 0 is the General's beat on the parade ground, the rest are soldiers (3 hits)
patrols = [[(24, 16), (48, 16)],        # the General
           [(12, 2), (12, 19)],         # barracks lane
           [(3, 22), (34, 22)],         # north cross road, west
           [(38, 22), (69, 22)],        # north cross road, east (to the east gate)
           [(3, 35), (34, 35)],         # south cross road, motor pool
           [(38, 35), (68, 35)],        # south cross road along the runway
           [(58, 2), (70, 2)]]          # training ground firing line
C("general", 24, 16, patrol=0, boss=True)
for i in range(1, len(patrols)): C("soldier", patrols[i][0][0], patrols[i][0][1], patrol=i)
FRIENDS_NEEDED = 14

# ---------------------------------------------------------------- checks
problems = []
occ = {}
for (s, x, y, w, h, k, sc) in objs:
    for yy in range(y, y + h):
        for xx in range(x, x + w):
            if not (0 <= xx < W and 0 <= yy < H): problems.append("%s@%d,%d off map" % (s, x, y)); continue
            if (xx, yy) in occ: problems.append("%s@%d,%d overlaps %s" % (s, x, y, occ[(xx, yy)]))
            if (xx, yy) in road: problems.append("%s@%d,%d on road" % (s, x, y))
            occ[(xx, yy)] = "%s@%d,%d" % (s, x, y)
BLOCK = {"building", "tree", "vehicle", "fence", "lamp"}
blocked = [[False for y in range(H)] for x in range(W)]
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
seen_civ = {}
for (s, x, y, lk, p, b, t) in civs:
    if blocked[x][y]: problems.append("%s at %d,%d stands on a blocked tile (%s)" % (s, x, y, occ.get((x, y))))
    elif not any((x, y) in r for r in from_exit): problems.append("%s at %d,%d can't reach an exit" % (s, x, y))
    if (x, y) in seen_civ: problems.append("%s and %s share %d,%d" % (s, seen_civ[(x, y)], x, y))
    seen_civ[(x, y)] = s
if blocked[START[0]][START[1]] or not any(START in r for r in from_exit): problems.append("start blocked/unreachable")
for i, p in enumerate(patrols):
    for a, b in zip(p, p[1:]):
        if a[0] != b[0] and a[1] != b[1]: problems.append("patrol %d leg not straight" % i)
        for xx in range(min(a[0], b[0]), max(a[0], b[0]) + 1):
            for yy in range(min(a[1], b[1]), max(a[1], b[1]) + 1):
                if blocked[xx][yy]: problems.append("patrol %d crosses blocked %d,%d (%s)" % (i, xx, yy, occ.get((xx, yy))))
if EXITS[1] not in from_exit[0]: problems.append("exits not connected")
print("people:", len(civs), "| objects:", len(objs), "| problems:", problems or "none")

# ---------------------------------------------------------------- text map
grid = [row[:] for row in ground]
for y in range(H):
    for x in range(W):
        if (x, y) in road: grid[y][x] = '#'
        elif grid[y][x] in ',s': grid[y][x] = '.' if grid[y][x] == ',' else 'd'
sym = {"building": "B", "field": "f", "tree": "T", "deco": "*", "ufo": "U", "vehicle": "V", "fence": "|", "lamp": "i"}
for (s, x, y, w, h, k, sc) in objs:
    for yy in range(y, y + h):
        for xx in range(x, x + w):
            if 0 <= xx < W and 0 <= yy < H: grid[yy][xx] = sym[k]
for (s, x, y, lk, p, b, t) in civs: grid[y][x] = 'G' if b else 'P' if p >= 0 else 'L' if lk else 'c'
for e in EXITS: grid[e[1]][e[0]] = 'E'
grid[START[1]][START[0]] = 'S'
print("    " + "".join(str(x % 10) for x in range(W)))
for y in range(H): print("%2d  " % y + "".join(grid[y]))
if problems or OUT is None: sys.exit(1 if problems else 0)

# ---------------------------------------------------------------- write C#
def cs_str(rows): return ",\n".join('        "%s"' % r for r in rows)
G = ["".join(r) for r in ground]
P = ["".join(("%X" % mask(x, y)) if (x, y) in road else '.' for x in range(W)) for y in range(H)]
L = []
L.append("// Map data for Level 3 \"Fort Maple Command\": deep inside the army base, at night, with the General (mini boss).")
L.append("// Rows go from the TOP of the map to the bottom. Map is %d x %d tiles." % (W, H))
L.append("// Generated by Tools/level3_design.py - change the layout there (the story of the map is at its top), not here.")
L.append("using UnityEngine;\n")
L.append("public static class Level3Data\n{")
L.append("    public const int Width = %d;\n    public const int Height = %d;\n" % (W, H))
L.append("    // Ground: '.' grass, ',' tufts, 'c' concrete, 'd' dirt, 's' dirt with specks")
L.append("    public static readonly string[] Ground = {\n" + cs_str(G) + "\n    };\n")
L.append("    // Roads (asphalt): '.' none, '0'-'F' road piece (N=1, E=2, S=4, W=8)")
L.append("    public static readonly string[] Paths = {\n" + cs_str(P) + "\n    };\n")
L.append("    // Objects: sprite, x, y (top-left tile), w, h (tiles), kind, and drawing scale")
L.append("    public static readonly Obj[] Objects = {")
L.append(",\n".join('        new Obj("%s", %d, %d, %d, %d, "%s"%s)' % (s, x, y, w, h, k, (", %d" % sc) if sc != 1 else "")
                   for (s, x, y, w, h, k, sc) in objs))
L.append("    };\n")
L.append("    public static readonly Civ[] Civilians = {")
cl = []
for (s, x, y, lk, p, b, t) in civs:
    if b: e = 'Civ.Boss("%s", %d, %d, %d)' % (s, x, y, p)
    elif p >= 0: e = 'Civ.Police("%s", %d, %d, %d)' % (s, x, y, p)
    elif lk: e = 'new Civ("%s", %d, %d, true)' % (s, x, y)
    else: e = 'new Civ("%s", %d, %d)' % (s, x, y)
    if t: e += " { toughness = %d }" % t
    cl.append("        " + e)
L.append(",\n".join(cl))
L.append("    };\n")
L.append("    // Patrol routes: 0 is the General's beat, the rest are soldiers")
L.append("    public static readonly Vector2Int[][] Patrols = {")
L.append(",\n".join("        new[] { " + ", ".join("new Vector2Int(%d, %d)" % t for t in p) + " }" for p in patrols))
L.append("    };\n")
L.append("    public static readonly Vector2Int Start = new Vector2Int(%d, %d);" % START)
L.append("    public static readonly Vector2Int[] Exits = { " + ", ".join("new Vector2Int(%d, %d)" % e for e in EXITS) + " };\n")
L.append("    public static readonly LevelMap Map = new LevelMap")
L.append("    {")
L.append('        Name = "Fort Maple Command", Width = Width, Height = Height, Ground = Ground, Paths = Paths,')
L.append("        Objects = Objects, Civilians = Civilians, Start = Start, Exits = Exits, Patrols = Patrols, FriendsNeeded = %d," % FRIENDS_NEEDED)
L.append('        Night = true, RoadTiles = "road_", AlienCanDie = true,')
L.append("    };")
L.append("}")
open(OUT, "w", encoding="utf-8").write("\n".join(L) + "\n")
print("wrote", OUT)
