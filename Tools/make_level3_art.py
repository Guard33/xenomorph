# Makes the extra Level 3 art:
#   Assets/Resources/Upgrades/psi_shield.png    skill icon for the Psi Shield (16 x 16)
# Run from the repo root:  python Tools/make_level3_art.py
import os
from PIL import Image

os.makedirs("Assets/Art/Level3Pieces", exist_ok=True)

# The General (folk_general.png) is now drawn by Tools/make_modern_art.py in a modern uniform.

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
