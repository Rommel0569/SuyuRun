"""Phase 2 (Costa redesign): generates the pixel-art sea sprites used by
RunnerWater.cs - big waves, small waves, foam and a reflection sparkle.

Each tile is authored on a small pixel grid with flat, unblended colors
(no anti-aliasing) so it reads as pixel art, then scaled up with nearest-
neighbour resampling. Wave/foam tiles are drawn as a function of (x mod
tile_width) so the left and right edges match exactly and they can be
tiled/scrolled seamlessly by the game's UV animation.

Run with: python Tools/gen_costa_sea_sprites.py
"""
import math
import os
from PIL import Image

OUT_DIR = "Assets/SuyuRun/Art/Resources/Sea"
UPSCALE = 8

# Palette shared with the rest of Costa's water (RunnerWater.cs / ART_DIRECTION.md).
DEEP = (10, 82, 117, 255)
MID = (24, 138, 154, 255)
NEAR = (36, 194, 196, 255)
CREST = (150, 235, 224, 255)
FOAM = (250, 245, 224, 255)
FOAM_SHADE = (196, 219, 214, 255)
TRANSPARENT = (0, 0, 0, 0)


def save_pixel_art(name, w, h, pixel_fn):
    im = Image.new("RGBA", (w, h), TRANSPARENT)
    px = im.load()
    for y in range(h):
        for x in range(w):
            px[x, y] = pixel_fn(x, y)
    big = im.resize((w * UPSCALE, h * UPSCALE), Image.NEAREST)
    os.makedirs(OUT_DIR, exist_ok=True)
    path = os.path.join(OUT_DIR, name)
    big.save(path)
    print("saved", path, big.size)


def wave_big(x, y):
    w = 64
    # A slow chevron: crest row rises and falls across the tile width.
    crest_row = 6 + round(3 * math.sin(2 * math.pi * (x % w) / w))
    if y < crest_row - 1:
        return TRANSPARENT
    if y == crest_row - 1:
        return CREST
    if y < crest_row + 4:
        return NEAR
    if y < crest_row + 9:
        return MID
    return DEEP


def wave_small(x, y):
    w = 32
    crest_row = 3 + round(2 * math.sin(2 * math.pi * 2 * (x % w) / w))
    if y < crest_row - 1:
        return TRANSPARENT
    if y == crest_row - 1:
        return CREST
    if y < crest_row + 3:
        return NEAR
    return MID


def foam(x, y):
    w = 48
    xm = x % w
    # Three uneven foam clumps with a soft shaded underside, gaps of clear water between.
    clumps = [(4, 9), (20, 10), (34, 8)]
    for cx, cw in clumps:
        d = abs(((xm - cx + w // 2) % w) - w // 2)
        if d < cw // 2:
            local_h = round((cw // 2 - d) * 0.55)
            if y < local_h - 1:
                return FOAM
            if y < local_h + 1:
                return FOAM_SHADE
    return TRANSPARENT


def sparkle(x, y):
    # 8x8 diamond glint: bright core, soft edge, matches the water's crest color family.
    cx, cy = 3.5, 3.5
    d = abs(x - cx) + abs(y - cy)
    if d < 1.2:
        return (255, 255, 245, 255)
    if d < 2.6:
        return CREST
    if d < 3.4:
        return (CREST[0], CREST[1], CREST[2], 90)
    return TRANSPARENT


save_pixel_art("SeaWaveBig.png", 64, 19, wave_big)
save_pixel_art("SeaWaveSmall.png", 32, 9, wave_small)
save_pixel_art("SeaFoam.png", 48, 7, foam)
save_pixel_art("SeaSparkle.png", 8, 8, sparkle)
