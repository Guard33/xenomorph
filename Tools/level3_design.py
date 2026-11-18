# Level 3 "Fort Maple Command" layout. Edit the roads / objects / people below, then run:
#     python Tools/level3_design.py Assets/Scripts/Level3Data.cs
# It checks the layout (no overlaps, nothing on roads, everyone can reach an exit, patrol routes
# are on open ground), prints a text map, and writes Level3Data.cs.
# Then in Unity: Alien Game > Build Level 3.
#
# ---------------------------------------------------------------- the story of the map
# After Fort Maple's town, the alien slips through the south gate into the main army post. It is laid
# out like a real installation, at night, in the present day:
#   Perimeter            - fence all round, a perimeter road just inside it, and watchtowers at the
#                          corners and gates. Their searchlights sweep the base: get caught in a beam
#                          and the base alarm goes off.
#   South gate (start)   - the entry control point. The main avenue runs north to HQ and the north gate.
#   Cantonment (NW)      - two rows of barracks around a quad, the dining hall and the gym.
#   Clinic (W)           - the clinic and its medevac helipad; medics on duty.
#   Training range (W)   - firing lines of sandbags; the range officer.
#   Headquarters (N)     - HQ facing the parade field, flag offices either side, the command centre and
#                          bunker. THE GENERAL (mini boss) paces the parade field. North gate = Exit A.
#   Cemetery (NE)        - rows of headstones, a memorial flag and the groundskeeper.
#   Motor pool (SW)      - a fenced yard with maintenance bays and trucks and tanks parked in rows.
#   Ammo point (S)       - its own fence; ammunition bunkers separated by earth berms.
#   Airfield (SE)        - hangars, aircraft on the apron, control tower, helipads, fuel, the runway.
#                          The east gate (Exit B) is at the end of the cross street.
# Soldiers patrol the perimeter road, the cross street, the runway and the barracks quad. Sentries guard
# HQ, the motor pool, the ammo point, the airfield and the east gate. Everyone in uniform has a
# tranquilliser rifle; when anyone spots the alien the whole base goes on alarm.
# Military sprites are drawn at 2x (M(...)); footprints (w, h) are tiles at that size.
import sys
W, H = 96, 58
OUT = sys.argv[1] if len(sys.argv) > 1 else None

# ---------------------------------------------------------------- roads
# Laid out like a real installation: a perimeter road just inside the fence, a main avenue from
# the south gate (entry control point) to the north gate, cross streets between the districts.
road = set()
def hline(y, x0, x1):
    for x in range(x0, x1 + 1): road.add((x, y))
def vline(x, y0, y1):
    for y in range(y0, y1 + 1): road.add((x, y))
hline(3, 3, 92); hline(54, 3, 92); vline(3, 3, 54); vline(92, 3, 54)   # perimeter road
vline(48, 0, 57)     # main avenue: south gate -> HQ -> north gate (Exit A)
hline(30, 3, 95)     # cross street -> east gate (Exit B)
hline(17, 3, 47)     # cantonment street
hline(44, 3, 92)     # south street: motor pool, ammo point, airfield apron
hline(49, 52, 90)    # runway
vline(60, 45, 48); vline(84, 45, 48)   # taxiways
EXITS = [(48, 0), (95, 30)]
START = (48, 56)

def mask(x, y):
    m = 0
    if (x, y - 1) in road or ((x, y) in EXITS and y == 0): m |= 1
    if (x + 1, y) in road or ((x, y) in EXITS and x == W - 1): m |= 2
    if (x, y + 1) in road: m |= 4
    if (x - 1, y) in road: m |= 8
    if (x, y) == (48, 57): m |= 4      # the avenue runs out through the south gate
    return m

# ---------------------------------------------------------------- ground
ground = [['c' for x in range(W)] for y in range(H)]
def dirt(x0, y0, x1, y1):
    for y in range(y0, y1 + 1):
        for x in range(x0, x1 + 1): ground[y][x] = 's' if (x + y) % 3 == 0 else 'd'
def lawn(x0, y0, x1, y1):
    for y in range(y0, y1 + 1):
        for x in range(x0, x1 + 1): ground[y][x] = ',' if (x * 7 + y * 3) % 9 == 0 else '.'
lawn(4, 4, 47, 16)         # cantonment quad
lawn(4, 18, 20, 29)        # clinic grounds
lawn(54, 18, 74, 29)       # parade field
lawn(77, 4, 91, 29)        # cemetery
dirt(22, 18, 47, 29)       # training range
dirt(5, 32, 46, 43)        # motor pool yard
dirt(6, 46, 44, 53)        # ammunition supply point

# ---------------------------------------------------------------- objects
objs = []
def O(s, x, y, w, h, k, sc=1): objs.append((s, x, y, w, h, k, sc))
def M(s, x, y, w, h, k): O(s, x, y, w, h, k, 2)
def flood(x, y): O("flood_light", x, y, 1, 1, "lamp")
def tree(x, y): O("tree%d" % (len(objs) % 4 + 1), x, y, 1, 1, "tree")

# Perimeter fence (gaps: north gate, east gate, south gate) with watchtowers at the corners and gates.
# Every watchtower has a searchlight that sweeps the base and raises the alarm if it catches the alien.
for x in range(0, 96, 2):
    if x != 48: O("wire_fence", x, 0, 2, 1, "fence"); O("wire_fence", x, 57, 2, 1, "fence")
for y in range(1, 57, 2):
    O("fence_post", 0, y, 1, 2, "fence")
    if y != 29: O("fence_post", 95, y, 1, 2, "fence")
for (i, (x, y)) in enumerate([(1, 1), (93, 1), (1, 55), (93, 55), (50, 1), (43, 55), (93, 26), (1, 27)]):
    M("guard_hut_%d" % (i % 2 + 1), x, y, 2, 2, "building")

# --- Headquarters: HQ facing the parade field, flag offices either side, command centre
M("hq_flag_1", 50, 5, 5, 6, "building"); M("base_hq_large", 58, 5, 8, 6, "building"); M("hq_flag_2", 68, 5, 5, 6, "building")
M("command_building", 50, 12, 4, 4, "building"); M("command_building", 70, 12, 4, 4, "building")
M("bunker", 58, 13, 4, 2, "vehicle")                      # command bunker behind the field
O("flagpole", 64, 18, 1, 3, "lamp")                         # colours at the head of the parade field
for (x, y) in [(55, 19), (73, 19), (55, 28), (73, 28)]: flood(x, y)

# --- Cemetery (NE): rows of headstones, a memorial flag, trees along the sides
O("flagpole", 84, 5, 1, 3, "lamp")
for y in range(10, 29, 2):
    for x in (79, 81, 83, 85, 87, 89): O("grave", x, y, 1, 1, "fence")
for y in (5, 9, 13, 17, 21, 25): tree(77, y); tree(91, y)

# --- Cantonment (NW): two rows of barracks facing a quad, the dining hall and the gym
for x in (6, 11, 16, 21, 26, 31):
    M(["barracks_1", "barracks_2", "barracks_camo"][(x // 5) % 3], x, 5, 2, 2, "building")
    M(["barracks_camo", "barracks_1", "barracks_2"][(x // 5) % 3], x, 13, 2, 2, "building")
M("supply_depot_2", 38, 5, 2, 2, "building")               # dining hall
M("supply_depot_1", 38, 13, 2, 2, "building")              # gym
flood(9, 8); flood(24, 8); flood(42, 9)

# --- Clinic with its helipad
M("command_building", 6, 19, 4, 4, "building")
O("helipad", 12, 19, 3, 3, "deco")
flood(17, 24)

# --- Training range: firing lines of sandbags
for x in (24, 28, 32, 36, 40, 44):
    M("dirt_mound", x, 21, 2, 1, "vehicle"); M("dirt_mound", x, 27, 2, 1, "vehicle")

# --- Motor pool (SW): fenced yard, maintenance bays, vehicles parked in neat rows
for x in range(4, 48, 2):
    if x != 26: O("wire_fence", x, 31, 2, 1, "fence")
for y in range(32, 44, 2): O("fence_post", 4, y, 1, 2, "fence"); O("fence_post", 47, y, 1, 2, "fence")
for (i, x) in enumerate((7, 11, 15, 19)): M("garage_%d" % (i % 3 + 1), x, 33, 2, 2, "building")
for x in (7, 13, 19, 25, 31, 37): M("army_truck", x, 37, 4, 2, "vehicle")
for x in (7, 11, 15, 19, 23, 27, 31, 35, 39): M("tank", x, 40, 2, 2, "vehicle")
flood(30, 33); flood(42, 33)

# --- Ammunition supply point (S): its own fence, bunkers separated by earth berms
for x in range(6, 46, 2):
    if x != 24: O("wire_fence", x, 45, 2, 1, "fence")
for y in range(46, 54, 2): O("fence_post", 5, y, 1, 2, "fence"); O("fence_post", 45, y, 1, 2, "fence")
for y in (47, 50):
    for x in (8, 18, 28, 36): M("bunker", x, y, 4, 2, "vehicle")
    for x in (14, 24, 33): M("dirt_mound", x, y, 2, 1, "vehicle")

# --- Airfield (SE): hangars, aircraft on the apron, control tower, helipads, fuel, runway
for (i, x) in enumerate((52, 58, 64, 70, 76)): M("hangar_small_%d" % (i % 3 + 1), x, 32, 2, 2, "building")
for (i, x) in enumerate((51, 57, 63, 69, 75)): M("helicopter" if i % 2 == 0 else "jet", x, 36, 3, 2, "vehicle")
M("command_building", 84, 32, 4, 4, "building")             # control tower
O("helipad", 82, 38, 3, 3, "deco"); O("helipad", 87, 38, 3, 3, "deco")
M("container", 52, 40, 3, 2, "vehicle"); M("container", 56, 40, 3, 2, "vehicle")   # fuel
for (x, y) in [(62, 34), (74, 34), (80, 42), (66, 51), (78, 51)]: flood(x, y)

# ---------------------------------------------------------------- people
civs = []   # (sprite, x, y, lookout, patrol, boss, toughness)
def C(s, x, y, lookout=False, patrol=-1, boss=False, tough=0): civs.append(("folk_" + s, x, y, lookout, patrol, boss, tough))
# Base staff (unarmed, 1 hit): people stand where they work
for (s, x, y) in [("cook", 40, 8), ("cook", 36, 8), ("clerk", 14, 8),                  # dining hall, barracks
                  ("medic", 8, 24), ("medic", 11, 24),                                   # clinic
                  ("clerk2", 34, 24),                                                    # range officer
                  ("clerk", 52, 11), ("clerk2", 56, 8), ("clerk", 67, 11),               # HQ staff
                  ("mechanic", 24, 34), ("mechanic", 10, 39), ("mechanic", 35, 39),      # motor pool
                  ("clerk2", 42, 49),                                                    # ammo technician
                  ("pilot", 54, 38), ("pilot", 66, 38), ("pilot", 80, 36), ("mechanic", 72, 40),  # airfield
                  ("gardener", 84, 9)]:                                                  # cemetery groundskeeper
    C(s, x, y)
# Sentries (2 hits): stand guard at the doors and gates, scan around, raise the base alarm, shoot darts
for (x, y) in [(63, 12), (27, 32), (24, 46), (88, 42), (94, 28)]: C("trooper", x, y, lookout=True, tough=2)
# Patrols: 0 is the General's beat on the parade field, the rest are soldiers (3 hits, darts)
patrols = [[(57, 24), (71, 24)],        # the General
           [(4, 3), (91, 3)],           # north perimeter road
           [(3, 4), (3, 53)],           # west perimeter road
           [(92, 4), (92, 53)],         # east perimeter road
           [(91, 54), (52, 54)],        # south perimeter road (east of the gate)
           [(4, 30), (47, 30)],         # cross street, west
           [(49, 30), (91, 30)],        # cross street, east
           [(52, 49), (90, 49)],        # the runway
           [(5, 10), (35, 10)]]         # through the barracks quad
C("general", 57, 24, patrol=0, boss=True)
for i in range(1, len(patrols)): C("trooper" if i % 2 else "trooper2", patrols[i][0][0], patrols[i][0][1], patrol=i)
FRIENDS_NEEDED = 17

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
