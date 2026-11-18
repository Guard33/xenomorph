# Makes the extra Level 3 art:
#   Assets/Art/Level3Pieces/folk_general.png   the General (mini boss): the soldier with a red beret
#                                              and gold insignia; drawn at 2x in the game
#   Assets/Resources/Upgrades/psi_shield.png    skill icon for the Psi Shield (16 x 16)
# Run from the repo root:  python Tools/make_level3_art.py
import os
from PIL import Image

os.makedirs("Assets/Art/Level3Pieces", exist_ok=True)

# ---------------------------------------------------------------- the General
sol = Image.open("Assets/Art/Level2Pieces/folk_soldier.png").convert("RGBA")
recolour = {
    (118, 126, 84): (150, 36, 40),     # helmet -> red beret
    (92, 100, 66): (112, 26, 32),
    (196, 186, 112): (255, 214, 72),   # badge -> gold
    (132, 150, 78): (84, 98, 58),      # uniform darker, officer green
    (96, 112, 56): (64, 76, 44),
}
gen = sol.copy()
for y in range(gen.height):
    for x in range(gen.width):
        r, g, b, a = gen.getpixel((x, y))
        if a and (r, g, b) in recolour:
            gen.putpixel((x, y), recolour[(r, g, b)] + (a,))
# gold shoulder marks
for (x, y) in [(5, 10), (10, 10)]:
    if gen.getpixel((x, y))[3]: gen.putpixel((x, y), (255, 214, 72, 255))
gen.save("Assets/Art/Level3Pieces/folk_general.png")

# ---------------------------------------------------------------- Psi Shield icon
ico = Image.new("RGBA", (16, 16), (0, 0, 0, 0))
OUT, RING, HI, IN = (30, 16, 48, 255), (176, 120, 255, 255), (232, 214, 255, 255), (96, 56, 160, 255)
for y in range(16):
    for x in range(16):
        d = ((x - 7.5) ** 2 + (y - 7.5) ** 2) ** 0.5
        if d <= 7.5:
            c = OUT if d > 6.6 else RING if d > 5.2 else IN
            ico.putpixel((x, y), c)
for (x, y) in [(4, 4), (5, 3), (3, 5), (5, 4)]:
    ico.putpixel((x, y), HI)
# a little alien eye in the middle
for (x, y) in [(6, 7), (7, 7), (8, 7), (9, 7), (6, 8), (9, 8), (6, 9), (7, 9), (8, 9), (9, 9)]:
    ico.putpixel((x, y), (120, 230, 120, 255))
ico.putpixel((7, 8), (20, 20, 20, 255)); ico.putpixel((8, 8), (20, 20, 20, 255))
ico.save("Assets/Resources/Upgrades/psi_shield.png")
print("done")
