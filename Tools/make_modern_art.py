# Makes the modern-day people and props so every level fits one theme: a present-day farm town
# and army base (no spears, bells or robes). Same style as the town folk: 16 x 16, dark outline.
#   Assets/Art/Modern/folk_*.png     people (soldier with a tranquilliser rifle, police, base staff...)
#   Assets/Art/Modern/*.png          props (gravestone, flagpole, concrete barrier, helipad)
#   Assets/Art/Level3Pieces/folk_general.png   the General, now in a modern service uniform
# Run from the repo root:  python Tools/make_modern_art.py
import os
from PIL import Image

OUT = "Assets/Art/Modern"
os.makedirs(OUT, exist_ok=True)

BASE = {
    '.': None,
    'o': (26, 20, 35),      # outline
    'e': (58, 53, 71),      # eyes
    'b': (40, 36, 44),      # boots / shoes
}
SKIN = {
    'light': {'s': (255, 236, 210), 'z': (232, 200, 160)},
    'tan':   {'s': (226, 176, 126), 'z': (196, 146, 98)},
    'dark':  {'s': (150, 98, 66),  'z': (122, 76, 50)},
}

def make(name, rows, colours, skin='light', folder=OUT):
    assert len(rows) == 16, (name, len(rows))
    pal = dict(BASE); pal.update(SKIN[skin]); pal.update(colours)
    im = Image.new("RGBA", (16, 16), (0, 0, 0, 0))
    for y, r in enumerate(rows):
        assert len(r) == 16, (name, y, r, len(r))
        for x, ch in enumerate(r):
            c = pal[ch]
            if c is not None: im.putpixel((x, y), c + (255,))
    im.save(os.path.join(folder, name + ".png"))

FACE = ["...osesssesso...",
        "...osssssssso...",
        "....ozzzzzzo...."]
LEGS = ["...oPPooooPPo...",
        "...obbo..obbo...",
        "....oo....oo....",
        "................"]

# ---------------------------------------------------------------- army
# Soldier: helmet, camouflage, tranquilliser rifle with an orange dart tip
make("folk_trooper", ["................",
                      ".....oooooo.....",
                      "....oGGGGGGo....",
                      "...oGGgGGGgGo...",
                      "..oGGGGGGGGGGo..",
                      ] + FACE + [
                      "...oCcCCCcCCo...",
                      "..oCCcCCCcCCCo..",
                      "..osKKKKsKKKKKKT",
                      "...oCCCcCCCCo...",
                      ] + LEGS,
     {'G': (88, 100, 58), 'g': (64, 74, 42), 'C': (120, 134, 76), 'c': (82, 94, 52),
      'P': (70, 78, 48), 'K': (44, 44, 52), 'T': (255, 170, 60)})
make("folk_trooper2", [
                      "................",
                      ".....oooooo.....",
                      "....oGGGGGGo....",
                      "...oGgGGGGgGo...",
                      "..oGGGGGGGGGGo..",
                      ] + FACE + [
                      "...oCCcCCCcCo...",
                      "..oCcCCCcCCCCo..",
                      "..osKKKKsKKKKKKT",
                      "...oCcCCCCcCo...",
                      ] + LEGS,
     {'G': (88, 100, 58), 'g': (64, 74, 42), 'C': (120, 134, 76), 'c': (82, 94, 52),
      'P': (70, 78, 48), 'K': (44, 44, 52), 'T': (255, 170, 60)}, skin='dark')

# The General: peaked cap with gold, service jacket with medals, grey moustache
make("folk_general", ["................",
                      ".....oooooo.....",
                      "....oVVVVVVo....",
                      "...oVVVYYVVVo...",
                      "..oooooooooooo..",
                      "...osesssesso...",
                      "...osWWWWWWso...",
                      "....ozzzzzzo....",
                      "...oJJJtJJJJo...",
                      "..osJRYtJJJJso..",
                      "...oJJJtJJJJo...",
                      "...okkkYkkkKo...",
                      ] + LEGS,
     {'V': (84, 98, 58), 'Y': (255, 214, 72), 'W': (200, 200, 205), 'J': (96, 110, 64),
      't': (205, 180, 120), 'R': (200, 50, 50), 'k': (40, 36, 30), 'K': (30, 30, 34), 'P': (70, 80, 48)},
     folder="Assets/Art/Level3Pieces")

# Base staff
make("folk_clerk", ["................",
                    ".....oooooo.....",
                    "....oHHHHHHo....",
                    "...oHHHHHHHHo...",
                    "...oHssssssHo...",
                    ] + FACE + [
                    "...oKKKtKKKKo...",
                    "..osKKKtKKKKso..",
                    "...oKKKtKKKKo...",
                    "...oKKKKKKKKo...",
                    ] + LEGS,
     {'H': (70, 50, 36), 'K': (205, 180, 120), 't': (80, 90, 50), 'P': (84, 96, 58)})
make("folk_clerk2", ["................",
                     ".....oooooo.....",
                     "....oHHHHHHo....",
                     "...oHHHHHHHHo...",
                     "..oHHssssssHHo..",
                     "..oHosesssesoHo.",
                     "..oHosssssssoHo.",
                     "...oHozzzzzoHo..",
                     "...oKKKtKKKKo...",
                     "..osKKKtKKKKso..",
                     "...oKKKtKKKKo...",
                     "...oKKKKKKKKo...",
                     ] + LEGS,
     {'H': (40, 30, 26), 'K': (205, 180, 120), 't': (80, 90, 50), 'P': (84, 96, 58)}, skin='tan')
make("folk_mechanic", ["................",
                       ".....oooooo.....",
                       "....oRRRRRRo....",
                       "...oRRRRRRRRo...",
                       "...ooooooooooo..",
                       ] + FACE + [
                       "...oUuUUUUuUo...",
                       "..osUUUUUUUUsWo.",
                       "...oUUUUUUUUoW..",
                       "...oUUUUUUUUoWW.",
                       "...oUUooooUUo...",
                       "...obbo..obbo...",
                       "....oo....oo....",
                       "................"],
     {'R': (200, 60, 50), 'U': (64, 96, 170), 'u': (110, 140, 210), 'W': (170, 175, 185)})
make("folk_pilot", ["................",
                    ".....oooooo.....",
                    "....oWWWWWWo....",
                    "...oWWWWWWWWo...",
                    "...oWvvvvvvWo...",
                    ] + FACE + [
                    "...oFQFFFFFFo...",
                    "..osFFFFFFFFso..",
                    "...oFFFFFFFFo...",
                    "...oFFFFFFFFo...",
                    "...oFFooooFFo...",
                    "...obbo..obbo...",
                    "....oo....oo....",
                    "................"],
     {'W': (225, 228, 232), 'v': (40, 60, 90), 'F': (128, 138, 106), 'Q': (230, 120, 50)}, skin='tan')
make("folk_cook", [".....oooooo.....",
                   "....oWWWWWWo....",
                   "....oWWWWWWo....",
                   "...ooWWWWWWoo...",
                   "...oHssssssHo...",
                   ] + FACE + [
                   "...oWWWWWWWWo...",
                   "..osWWWWWWWWso..",
                   "...oAAAAAAAAo...",
                   "...oAAAAAAAAo...",
                   ] + LEGS,
     {'W': (240, 240, 244), 'H': (90, 60, 40), 'A': (190, 196, 206), 'P': (80, 80, 92)}, skin='dark')
make("folk_medic", ["................",
                    ".....oooooo.....",
                    "....oGGGGGGo....",
                    "...oGGGRGGGGo...",
                    "..oGGRRRGGGGGo..",
                    ] + FACE + [
                    "...oWWWWWWWWo...",
                    "..osWRWWWWWWso..",
                    "...oRRRWWWWWo...",
                    "...oWRWWWWWWo...",
                    ] + LEGS,
     {'G': (88, 100, 58), 'R': (210, 50, 50), 'W': (236, 236, 240), 'P': (70, 78, 48)})

# ---------------------------------------------------------------- town (police, volunteers)
make("folk_cop", ["................",
                  ".....oooooo.....",
                  "....oNNNNNNo....",
                  "...oNNNYNNNNo...",
                  "..oooooooooooo..",
                  ] + FACE + [
                  "...oLYLLLLLLo...",
                  "..osLLLLLLLLso..",
                  "...okkkYkkKKo...",
                  "...oNNNNNNNNo...",
                  "...oNNooooNNo...",
                  "...obbo..obbo...",
                  "....oo....oo....",
                  "................"],
     {'N': (40, 52, 96), 'Y': (255, 214, 72), 'L': (130, 170, 225), 'k': (30, 28, 34), 'K': (60, 60, 70)})
make("folk_watchman", ["................",
                    ".....oooooo.....",
                    "....oNNNNNNo....",
                    "...oNNNNNNNNo...",
                    "...ooooooooooo..",
                    ] + FACE + [
                    "...oYYgYYgYYo...",
                    "..osYYYYYYYYsLo.",
                    "...oYgYYYYgYoL..",
                    "...oDDDDDDDDo...",
                    "...oJJooooJJo...",
                    "...obbo..obbo...",
                    "....oo....oo....",
                    "................"],
     {'N': (40, 52, 96), 'Y': (230, 230, 60), 'g': (210, 210, 210), 'D': (80, 80, 90),
      'J': (60, 80, 130), 'L': (255, 250, 200)}, skin='tan')
make("folk_councillor", ["................",
                    ".....oooooo.....",
                    "....oHHHHHHo....",
                    "...oHHHHHHHHo...",
                    "...oHssssssHo...",
                    ] + FACE + [
                    "...oDDWRWDDDo...",
                    "..osDDWRWDDDso..",
                    "...oDDDRDDDDo...",
                    "...oDDDDDDDDo...",
                    "...oDDooooDDo...",
                    "...obbo..obbo...",
                    "....oo....oo....",
                    "................"],
     {'H': (190, 190, 196), 'D': (50, 55, 75), 'W': (240, 240, 244), 'R': (200, 50, 50)})
make("folk_lineman", ["................",
                      ".....oooooo.....",
                      "....oYYYYYYo....",
                      "...oYYYYYYYYo...",
                      "..ooYYYYYYYYoo..",
                      ] + FACE + [
                      "...oOOgOOgOOo...",
                      "..osOOOOOOOOso..",
                      "...okkkkkkkko...",
                      "...oJJJJJJJJo...",
                      "...oJJooooJJo...",
                      "...obbo..obbo...",
                      "....oo....oo....",
                      "................"],
     {'Y': (245, 200, 40), 'O': (240, 130, 40), 'g': (210, 210, 210), 'k': (90, 60, 30), 'J': (60, 80, 130)}, skin='dark')
make("folk_gardener", ["................",
                       "....oooooooo....",
                       "...oTTTTTTTTo...",
                       "..ooTTTTTTTToo..",
                       "...oHssssssHo...",
                       ] + FACE + [
                       "...oGGGGGGGGo...",
                       "..osGGGGGGGGso..",
                       "...oGGGGGGGGo...",
                       "...oGGGGGGGGo...",
                       "...oJJooooJJo...",
                       "...obbo..obbo...",
                       "....oo....oo....",
                       "................"],
     {'T': (230, 200, 110), 'H': (120, 80, 40), 'G': (90, 150, 80), 'J': (60, 80, 130)})

# ---------------------------------------------------------------- props
def prop(name, rows, colours):
    pal = {'.': None, 'o': (26, 20, 35)}; pal.update(colours)
    h, w = len(rows), len(rows[0])
    im = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    for y, r in enumerate(rows):
        assert len(r) == w, (name, y)
        for x, ch in enumerate(r):
            if pal[ch] is not None: im.putpixel((x, y), pal[ch] + (255,))
    im.save(os.path.join(OUT, name + ".png"))

# Gravestone (1 x 1 tile): a white headstone on a little grass mound
prop("grave", ["................",
               "................",
               "......oooo......",
               ".....oWWWWo.....",
               "....oWWWWWWo....",
               "....oWWwwWWo....",
               "....oWwWWwWo....",
               "....oWWwwWWo....",
               "....oWWWWWWo....",
               "....oWWWWWWo....",
               "....oSSSSSSo....",
               "...oGGGGGGGGo...",
               "..oGgGGGGGGgGo..",
               "...oooooooooo...",
               "................",
               "................"],
     {'W': (226, 228, 232), 'w': (170, 174, 184), 'S': (150, 154, 164), 'G': (70, 120, 60), 'g': (50, 92, 44)})

# Flagpole (1 x 3 tiles): the Fort Maple flag, a maple leaf on green
pole = ["................"] * 48
flag = ["....oooooooooooo",
        "....oFFFFFFFFFFo",
        "....oFFFFMMFFFFo",
        "....oFFFMMMMFFFo",
        "....oFFFFMMFFFFo",
        "....oFFFFFMFFFFo",
        "....oFFFFFFFFFFo",
        "....oooooooooooo"]
rows = []
for y in range(48):
    if y == 0: r = "..oYo..........."
    elif 2 <= y < 10: r = "..oPo" + flag[y - 2][5:]
    elif y >= 44: r = ".oSSSo.........."
    else: r = "..oPo..........."
    rows.append(r.ljust(16, '.')[:16])
prop("flagpole", rows, {'Y': (255, 214, 72), 'P': (200, 204, 212), 'F': (60, 130, 70), 'M': (230, 70, 50), 'S': (130, 134, 144)})

# Concrete barrier (2 x 1 tiles)
prop("barrier", ["................................",
                 "................................",
                 "................................",
                 "................................",
                 "................................",
                 "....oooooooooooooooooooooooo....",
                 "...oCCCCCCCCCCCCCCCCCCCCCCCCo...",
                 "..oCCCCCCCCCCCCCCCCCCCCCCCCCCo..",
                 "..oCYYYYCCCCYYYYCCCCYYYYCCCCCo..",
                 "..oCCCCCCCCCCCCCCCCCCCCCCCCCCo..",
                 ".oDDDDDDDDDDDDDDDDDDDDDDDDDDDDo.",
                 ".oDDDDDDDDDDDDDDDDDDDDDDDDDDDDo.",
                 ".oDDDDDDDDDDDDDDDDDDDDDDDDDDDDo.",
                 "..oooooooooooooooooooooooooooo..",
                 "................................",
                 "................................"],
     {'C': (196, 198, 204), 'D': (150, 154, 162), 'Y': (240, 200, 50)})

# Helipad (3 x 3 tiles, drawn on the ground): a white H in a yellow ring
hp = Image.new("RGBA", (48, 48), (0, 0, 0, 0))
for y in range(48):
    for x in range(48):
        d = ((x - 23.5) ** 2 + (y - 23.5) ** 2) ** 0.5
        if d <= 23: hp.putpixel((x, y), (58, 62, 72, 255))
        if 19 <= d <= 21: hp.putpixel((x, y), (240, 200, 50, 255))
for y in range(14, 34):
    for x in list(range(15, 19)) + list(range(29, 33)): hp.putpixel((x, y), (236, 236, 240, 255))
for y in range(22, 26):
    for x in range(19, 29): hp.putpixel((x, y), (236, 236, 240, 255))
hp.save(os.path.join(OUT, "helipad.png"))
print("done")
