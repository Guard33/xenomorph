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

# ---------------------------------------------------------------- the story of the map
# Fort Maple is a small town that grew up next to an army base. Tonight a UFO crash-landed in
# a farmer's corn field south-east of town. Every area has a job:
#   Crash farm (SE)   - where the alien starts: a lonely farm, 3 easy first friends, no lamps.
#   Meadow + hedgerow - open ground between the farm and town; the hedgerow is cover to sneak in.
#   Town (W)          - main street with the shops, church, police station and the fountain
#                       square in the middle (both lookouts stand there), houses on the side
#                       streets, a pond park. The west road out of town is Exit A.
#   Town farm (NW)    - the town's barn and fields, reached by the farm lane off the avenue.
#   Maple woods       - forest between the town farm and the base: cover to reach the base fence,
#                       which is why a soldier walks the outside of the west fence.
#   Army base (NE)    - main gate (south) with guard hut, sandbags and barracks; HQ with the
#                       parade ground in front (open and floodlit: dangerous to cross); command
#                       centre; airfield (each aircraft parked in front of its hangar); supply depot
#                       and motor pool next to the rear gate road; ammo dump in the dirt yard.
#                       The rear gate road is Exit B.
#   Street lamps light the roads people use; floodlights light the base's open ground.

# ---------------------------------------------------------------- roads
road = set()
def hline(y, x0, x1):
    for x in range(x0, x1 + 1): road.add((x, y))
def vline(x, y0, y1):
    for y in range(y0, y1 + 1): road.add((x, y))
hline(22, 0, 44)     # main street: west exit -> town -> base gate -> crash farm
vline(32, 12, 22)    # main gate road into the base
hline(12, 32, 52)    # base road along the parade ground
vline(52, 0, 12)     # rear gate road (north exit)
vline(8, 12, 30)     # avenue (north to the town farm)
hline(12, 8, 19)     # farm lane in front of the barn
hline(28, 8, 24)     # south street (houses)
vline(18, 22, 28)    # side street
vline(44, 22, 28)    # crash farm lane
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
    for x in range(50, 61): ground[y][x] = 's' if (x + y) % 3 == 0 else 'd'   # ammo dump / truck yard
pond = {(2,26),(3,26),(4,26),(1,27),(2,27),(3,27),(4,27),(5,27),(1,28),(2,28),(3,28),(4,28),(5,28),(2,29),(3,29),(4,29)}
for (x, y) in pond: ground[y][x] = '=' if (x + y) % 4 == 0 else '~'

# ---------------------------------------------------------------- objects
objs = []
def O(s, x, y, w, h, k, sc=1): objs.append((s, x, y, w, h, k, sc))
def M(s, x, y, w, h, k): O(s, x, y, w, h, k, 2)     # military piece at 2x
def lamp(x, y): O("street_lamp", x, y, 1, 1, "lamp")
def flood(x, y): O("flood_light", x, y, 1, 1, "lamp")
def tree(x, y): O("tree%d" % (len(objs) % 4 + 1), x, y, 1, 1, "tree")

# --- Army base. Fence: wire along top and bottom (gaps for the gates), posts down the sides
for x in range(28, 62, 2):
    if x != 52: O("wire_fence", x, 1, 2, 1, "fence")
    if x != 32: O("wire_fence", x, 20, 2, 1, "fence")
for y in range(2, 20, 2):
    O("fence_post", 28, y, 1, 2, "fence"); O("fence_post", 61, y, 1, 2, "fence")
# Command: HQ in the middle, flag offices either side, command centre, parade ground in front
M("hq_flag_1", 29, 2, 5, 6, "building")
M("base_hq_large", 36, 2, 8, 6, "building")
M("hq_flag_2", 46, 2, 5, 6, "building")
M("command_building", 29, 8, 4, 4, "building")
flood(34, 8); flood(45, 8)                              # parade ground (x34-45, y8-11) is lit
# Supply depot by the rear gate road (deliveries come in the back)
M("supply_depot_1", 46, 9, 2, 2, "building"); M("supply_depot_2", 48, 9, 2, 2, "building")
M("container_upright", 50, 8, 2, 3, "vehicle")
# Rear gate and motor pool: guard hut, garages, tanks parked in front, a truck ready to leave
M("guard_hut_2", 53, 2, 2, 2, "building")
M("garage_1", 55, 2, 2, 2, "building"); M("garage_2", 57, 2, 2, 2, "building"); M("garage_3", 59, 2, 2, 2, "building")
M("tank", 55, 5, 2, 2, "vehicle"); M("tank", 58, 5, 2, 2, "vehicle")
M("army_truck", 54, 9, 4, 2, "vehicle")
flood(53, 6); flood(60, 9)
# Main gate: guard hut and sandbags, barracks right inside so soldiers are close to the gate
M("guard_hut_1", 33, 18, 2, 2, "building")
M("barracks_1", 29, 13, 2, 2, "building"); M("barracks_2", 29, 15, 2, 2, "building"); M("barracks_camo", 29, 17, 2, 2, "building")
M("dirt_mound", 29, 19, 2, 1, "vehicle")                # sandbags
flood(31, 17)
# Airfield: each aircraft parked in front of its hangar
M("hangar_small_1", 35, 14, 2, 2, "building"); M("helicopter", 35, 17, 3, 2, "vehicle")
M("hangar_small_2", 39, 14, 2, 2, "building"); M("helicopter", 39, 17, 3, 2, "vehicle")
M("hangar_small_3", 43, 14, 2, 2, "building"); M("jet", 43, 17, 3, 2, "vehicle")
flood(37, 14); flood(41, 14); flood(47, 17)
# Ammo dump in the dirt yard: bunker behind sandbags, a supply truck and a container
M("bunker", 50, 14, 4, 2, "vehicle")
M("dirt_mound", 50, 17, 2, 1, "vehicle"); M("dirt_mound", 52, 17, 2, 1, "vehicle")
M("army_truck", 55, 14, 4, 2, "vehicle"); M("container", 55, 17, 3, 2, "vehicle")
flood(59, 17)

# --- Town. Main street, north side: houses, general store, church, police station, houses
O("house1", 2, 19, 2, 2, "building"); O("house3", 5, 18, 2, 3, "building")
O("general_store", 10, 19, 2, 2, "building"); O("church", 13, 19, 2, 2, "building")
O("base_office", 16, 19, 2, 2, "building")          # police station (town size, not 2x)
O("house4", 20, 18, 2, 3, "building"); O("house5", 23, 19, 2, 2, "building")
# main street, south side: the fountain square is the middle of town
O("house6", 2, 23, 2, 2, "building"); O("house2", 10, 23, 2, 2, "building")
O("fountain_square", 13, 23, 2, 3, "building")
O("house7", 20, 23, 2, 3, "building"); O("house8", 23, 23, 2, 3, "building")
# avenue: houses on the way north to the farm
O("house4", 5, 15, 2, 3, "building"); O("house8", 10, 15, 2, 3, "building")
# south street: houses
O("house2", 10, 29, 2, 2, "building"); O("house5", 13, 29, 2, 2, "building"); O("house6", 16, 29, 2, 2, "building")
O("house1", 20, 29, 2, 2, "building"); O("house3", 23, 29, 2, 3, "building")
# Street lamps: along the streets people walk, and on the road up to the main gate
for (x, y) in [(4, 21), (9, 21), (15, 21), (19, 21), (25, 21), (7, 23), (17, 23), (27, 23),
               (7, 17), (9, 26), (15, 27), (19, 27), (7, 29),
               (30, 21), (31, 23), (34, 21), (38, 21), (42, 23)]: lamp(x, y)
# Pond park: hedges around the pond
for (x, y) in [(0, 26), (6, 29), (1, 30), (5, 25)]: O("bush", x, y, 1, 1, "deco")

# --- Town farm (NW): fields behind the barn yard, the farm lane in front of it
for (x, y) in [(2, 4), (5, 4), (2, 7), (5, 7)]: O("corn", x, y, 3, 3, "field")
O("crops_green", 10, 4, 3, 3, "field"); O("crops_red", 13, 4, 3, 3, "field"); O("orchard", 16, 4, 3, 3, "field")
O("barn", 10, 8, 3, 3, "building"); O("silo", 14, 9, 2, 2, "building"); O("animal_pen", 17, 8, 3, 3, "building")
lamp(13, 11)                                         # barn yard light

# --- Crash farm (SE): farmhouse and barn on the lane, corn fields, the UFO in a flattened gap
O("house3", 45, 24, 2, 3, "building")                # farmhouse
O("barn", 45, 28, 3, 3, "building")
lamp(43, 26)                                         # porch light at the end of the lane
for (x, y) in [(48, 22), (51, 22), (54, 25), (48, 25)]: O("corn", x, y, 3, 3, "field")
O("crops_green", 54, 22, 3, 3, "field"); O("crops_green", 57, 22, 3, 3, "field"); O("crops_green", 57, 25, 3, 3, "field")
O("crops_red", 48, 28, 3, 3, "field"); O("orchard", 55, 28, 3, 3, "field"); O("orchard", 58, 28, 3, 3, "field")
O("ufo", 51, 25, 2, 2, "ufo")                        # crashed in the corn
# The farm's pasture and hay fields fill the land between town and farmhouse
O("animal_pen", 31, 24, 3, 3, "building"); O("silo", 36, 24, 2, 2, "building")
for x in [29, 32, 35, 38]: O("crops_green" if x % 2 else "crops_red", x, 28, 3, 3, "field")
START = (52, 28)

# --- Trees
for x in [0, 1, 3, 6, 9, 12, 15, 17, 26, 30, 35, 40, 45, 57, 60]: tree(x, 0)            # forest edge (north)
for y in [3, 6, 9, 12, 15, 18]: tree(0, y)                                              # forest edge (west)
for x in [0, 3, 6, 27, 43, 53, 61]: tree(x, 31)                        # forest edge (south)
for y in [21, 24, 27]: tree(61, y)                                                      # forest edge (east)
# Maple woods: a real forest wrapping behind the town, between the town farm and the base.
# Every other column is left open so the alien can slip through it (cover on the way to the fence).
def woods(x0, y0, x1, y1):
    for y in range(y0, y1 + 1):
        for x in range(x0, x1 + 1):
            if x % 2 == 0 and (x * 3 + y * 5) % 7 != 0: tree(x, y)
woods(20, 2, 26, 10)
woods(14, 14, 26, 17)
# Hedgerow between the crash farm's pasture and its corn: cover to sneak from the crash site to town
for y in [24, 25, 27, 28, 30]: tree(41, y)
# Shade trees on the south street and in the pond park
tree(12, 26); tree(16, 26); tree(1, 24)

# ---------------------------------------------------------------- people
civs = []   # (sprite, x, y, lookout, patrol)
def C(s, x, y, lookout=False, patrol=-1): civs.append(("folk_" + s, x, y, lookout, patrol))
# Town: people stand where they work or live
town = [("shopkeeper", 10, 21), ("merchant", 12, 21),        # outside the general store
        ("scholar", 14, 21),                                  # the church
        ("tailor", 22, 21), ("baker", 3, 21),                 # in front of their houses
        ("courier", 26, 22),                                  # heading up the road to the base
        ("lineman", 7, 22),                                   # fixing the main street lamps
        ("bartender", 9, 24), ("innkeeper", 24, 26),
        ("grandpa", 10, 25), ("grandma", 11, 25),             # on their porch
        ("florist", 15, 26),                                  # by the fountain
        ("fisherman", 6, 27), ("painter", 6, 25),             # pond park
        ("gardener", 4, 17)]
for (s, x, y) in town: C(s, x, y)
C("watchman", 12, 23, lookout=True); C("councillor", 16, 23, lookout=True)   # both watch the square
C("farmer", 11, 11)                                                     # town farm, barn yard
# Crash farm: the first people the alien meets
for (s, x, y) in [("farmer", 46, 27), ("carpenter", 34, 27), ("florist", 47, 23)]: C(s, x, y)
# Base staff (unarmed): mechanic at the motor pool, depot clerk, HQ clerk, cook by the barracks, hangar mechanic
for (s, x, y) in [("mechanic", 57, 7), ("clerk2", 47, 11), ("clerk", 40, 9), ("cook", 31, 15), ("mechanic", 38, 16)]:
    C(s, x, y)
# Patrols: the police officer walks from the police station up through the main gate;
# soldiers walk the base road, the rear gate road and the outside of the west fence (by the woods)
patrols = [([(17, 22), (32, 22), (32, 13)], "cop"),
           ([(33, 12), (51, 12)], "trooper"),
           ([(52, 2), (52, 11)], "trooper2"),
           ([(27, 2), (27, 19)], "trooper")]
for i, (p, s) in enumerate(patrols): C(s, p[0][0], p[0][1], patrol=i)
patrols = [p for (p, s) in patrols]

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
lines.append('        Night = true, RoadTiles = "road_", AlienCanDie = true,')
lines.append("    };")
lines.append("}")
open(OUT, "w", encoding="utf-8").write("\n".join(lines) + "\n")
print("wrote", OUT)
