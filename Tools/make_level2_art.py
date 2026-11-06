# Makes the extra Level 2 night art in Assets/Art/Level2Pieces:
#   street_lamp.png   town street lamp (16 x 32)
#   flood_light.png   army base floodlight mast (16 x 40)
#   folk_soldier.png  army-green recolour of folk_police
#   road_00..15.png   asphalt versions of the dirt road pieces (same shapes as path_00..15)
# Run from the repo root:  python Tools/make_level2_art.py
from PIL import Image

OUT = "Assets/Art/Level2Pieces/"
L1 = "Assets/Art/Level1Pieces/"

def canvas(w, h): return Image.new("RGBA", (w, h), (0, 0, 0, 0))
def rect(im, x0, y0, x1, y1, c):
    for y in range(y0, y1 + 1):
        for x in range(x0, x1 + 1): im.putpixel((x, y), c)

IRON, IRON_HI, OUTL = (44, 44, 56, 255), (86, 88, 104, 255), (20, 18, 26, 255)
GLASS, GLASS_HI = (255, 214, 128, 255), (255, 246, 205, 255)

# ---------------------------------------------------------------- street lamp
lamp = canvas(16, 32)
rect(lamp, 5, 29, 10, 31, OUTL); rect(lamp, 6, 29, 9, 30, IRON)          # base
rect(lamp, 7, 9, 8, 28, IRON); rect(lamp, 7, 9, 7, 28, IRON_HI)           # post
rect(lamp, 6, 9, 9, 9, OUTL)
rect(lamp, 4, 2, 11, 2, OUTL); rect(lamp, 6, 1, 9, 1, OUTL)               # cap
rect(lamp, 4, 3, 11, 8, OUTL); rect(lamp, 5, 3, 10, 7, GLASS)             # lantern
rect(lamp, 6, 4, 7, 5, GLASS_HI); rect(lamp, 5, 8, 10, 8, IRON)
lamp.save(OUT + "street_lamp.png")

# ---------------------------------------------------------------- floodlight mast
STEEL, STEEL_HI = (70, 76, 84, 255), (120, 128, 136, 255)
WHITE, WHITE_HI = (226, 238, 255, 255), (255, 255, 255, 255)
fl = canvas(16, 40)
rect(fl, 4, 37, 11, 39, OUTL); rect(fl, 5, 37, 10, 38, STEEL)
rect(fl, 7, 9, 8, 36, STEEL); rect(fl, 7, 9, 7, 36, STEEL_HI)
for y in range(12, 36, 6): rect(fl, 6, y, 9, y, OUTL)                    # ladder rungs
rect(fl, 2, 7, 13, 8, OUTL); rect(fl, 3, 7, 12, 7, STEEL)                  # crossbar
for x0 in (1, 9):                                                         # two lamp heads
    rect(fl, x0, 1, x0 + 5, 6, OUTL); rect(fl, x0 + 1, 2, x0 + 4, 5, WHITE); rect(fl, x0 + 1, 2, x0 + 2, 3, WHITE_HI)
fl.save(OUT + "flood_light.png")

# ---------------------------------------------------------------- soldier
recolour = {
    (128, 176, 248): (132, 150, 78), (68, 127, 215): (96, 112, 56), (37, 55, 99): (50, 60, 32),
    (199, 207, 220): (118, 126, 84), (138, 147, 168): (92, 100, 66), (83, 90, 115): (66, 72, 46),
    (255, 240, 137): (196, 186, 112),
}
pol = Image.open(OUT + "folk_police.png").convert("RGBA")
sol = pol.copy()
for y in range(sol.height):
    for x in range(sol.width):
        r, g, b, a = sol.getpixel((x, y))
        if a and (r, g, b) in recolour: sol.putpixel((x, y), recolour[(r, g, b)] + (a,))
sol.save(OUT + "folk_soldier.png")

# ---------------------------------------------------------------- asphalt roads
ASPH, EDGE, SPECK = (64, 62, 72, 255), (44, 42, 52, 255), (80, 78, 90, 255)
for m in range(16):
    src = Image.open(L1 + "path_%02d.png" % m).convert("RGBA")
    out = canvas(16, 16)
    for y in range(16):
        for x in range(16):
            if src.getpixel((x, y))[3] == 0: continue
            edge = any(not (0 <= x + dx < 16 and 0 <= y + dy < 16) and False or
                       (0 <= x + dx < 16 and 0 <= y + dy < 16 and src.getpixel((x + dx, y + dy))[3] == 0)
                       for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)))
            c = EDGE if edge else SPECK if (x * 7 + y * 13) % 23 == 0 else ASPH
            out.putpixel((x, y), c)
    out.save(OUT + "road_%02d.png" % m)
print("done")
