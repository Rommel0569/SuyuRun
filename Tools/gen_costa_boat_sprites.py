"""Phase 3 (Costa redesign): pixel-art boats and their rowers.

Two hull types (wood fishing boat with a small sail, totora reed canoe) and
a three-frame rowing cycle (reach / pull / recover) for the rower+oar, drawn
together per frame so the oar's position is baked into the animation - no
separate rotating game object needed. All flat, unblended colors, drawn on a
small canvas then upscaled with nearest-neighbour resampling, matching
Tools/gen_costa_sea_sprites.py's technique and palette family.

Run with: python Tools/gen_costa_boat_sprites.py
"""
import os
from PIL import Image, ImageDraw

OUT_DIR = "Assets/SuyuRun/Art/Resources/Boats"
UPSCALE = 8

WOOD = (74, 58, 47, 255)
WOOD_LIGHT = (150, 118, 78, 255)
REED = (196, 158, 90, 255)
REED_DARK = (150, 112, 62, 255)
SAND = (255, 214, 133, 255)
CREAM = (245, 230, 184, 255)
SHADOW = (58, 41, 43, 255)
SKIN = (108, 68, 44, 255)
SHIRT = (245, 230, 184, 255)
TRANSPARENT = (0, 0, 0, 0)


def new_canvas(w, h):
    return Image.new("RGBA", (w, h), TRANSPARENT)


def save(im, name):
    os.makedirs(OUT_DIR, exist_ok=True)
    big = im.resize((im.width * UPSCALE, im.height * UPSCALE), Image.NEAREST)
    path = os.path.join(OUT_DIR, name)
    big.save(path)
    print("saved", path, big.size)


def boat_wood():
    w, h = 56, 24
    im = new_canvas(w, h)
    d = ImageDraw.Draw(im)
    # Hull: shallow curved wood boat, flat deck line near the top.
    d.polygon([(4, 14), (10, 20), (44, 20), (52, 13), (46, 10), (10, 10)], fill=WOOD)
    d.line([(10, 10), (46, 10)], fill=WOOD_LIGHT, width=1)
    for x in range(12, 44, 6):
        d.line([(x, 11), (x, 19)], fill=SHADOW, width=1)
    # Mast and small sail.
    d.line([(30, 2), (30, 10)], fill=WOOD_LIGHT, width=1)
    d.polygon([(30, 2), (30, 9), (40, 8)], fill=CREAM)
    save(im, "BoatWood.png")


def boat_reed():
    w, h = 52, 18
    im = new_canvas(w, h)
    d = ImageDraw.Draw(im)
    # Totora canoe: tapered ends, the bow curling upward.
    d.polygon([(2, 13), (6, 16), (38, 16), (46, 11), (40, 15), (10, 15), (4, 12)], fill=REED)
    d.polygon([(38, 16), (46, 11), (44, 6), (41, 9), (39, 14)], fill=REED)
    d.line([(6, 15), (38, 15)], fill=REED_DARK, width=1)
    for x in range(8, 36, 5):
        d.line([(x, 13), (x, 16)], fill=REED_DARK, width=1)
    save(im, "BoatReed.png")


def rower_frame(name, oar_angle):
    # oar_angle: "reach" (paddle forward/down), "pull" (paddle back/up), "recover" (neutral).
    w, h = 20, 20
    im = new_canvas(w, h)
    d = ImageDraw.Draw(im)
    seat_y = 15
    # Seated body.
    d.rectangle([7, 6, 12, seat_y], fill=SHIRT)
    d.ellipse([6, 2, 13, 8], fill=SKIN)
    d.rectangle([7, seat_y, 12, 18], fill=SKIN)
    if oar_angle == "reach":
        arm = [(8, 8), (3, 11)]
        paddle = [(3, 11), (0, 15)]
    elif oar_angle == "pull":
        arm = [(11, 8), (16, 6)]
        paddle = [(16, 6), (19, 1)]
    else:
        arm = [(9, 8), (11, 5)]
        paddle = [(11, 5), (13, 0)]
    d.line(arm, fill=SKIN, width=2)
    d.line(paddle, fill=WOOD_LIGHT, width=2)
    save(im, f"Rower{name}.png")


boat_wood()
boat_reed()
rower_frame("Reach", "reach")
rower_frame("Pull", "pull")
rower_frame("Recover", "recover")
