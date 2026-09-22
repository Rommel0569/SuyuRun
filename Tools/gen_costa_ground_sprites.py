"""Phase 6 (Costa redesign, final phase): tileable pixel-art textures for the
playable structures - an adobe block texture for elevated platforms, and a
sand texture for the ground-level coastal foundation. Both are trivially
seamless (regular grid / independent speckles, nothing straddles a tile
edge) since RunnerCoastalArt.cs tiles them in both directions across
structures of very different sizes.

Run with: python Tools/gen_costa_ground_sprites.py
"""
import os
import random
from PIL import Image, ImageDraw

OUT_DIR = "Assets/SuyuRun/Art/Resources/Ground"
UPSCALE = 8


def save(im, name):
    os.makedirs(OUT_DIR, exist_ok=True)
    big = im.resize((im.width * UPSCALE, im.height * UPSCALE), Image.NEAREST)
    path = os.path.join(OUT_DIR, name)
    big.save(path)
    print("saved", path, big.size)


def adobe_tile():
    w, h = 32, 24
    im = Image.new("RGBA", (w, h), (0, 0, 0, 255))
    d = ImageDraw.Draw(im)
    bw, bh = 16, 12
    mortar = (64, 42, 40, 255)
    tones = [(196, 110, 70, 255), (173, 92, 58, 255), (206, 124, 82, 255), (180, 98, 62, 255)]
    i = 0
    for row in range(h // bh):
        for col in range(w // bw):
            x0, y0 = col * bw, row * bh
            d.rectangle([x0, y0, x0 + bw - 2, y0 + bh - 2], fill=tones[i % len(tones)])
            i += 1
    for row in range(h // bh):
        d.line([(0, row * bh + bh - 1), (w, row * bh + bh - 1)], fill=mortar)
    for col in range(w // bw):
        d.line([(col * bw + bw - 1, 0), (col * bw + bw - 1, h)], fill=mortar)
    save(im, "Adobe.png")


def sand_tile():
    w, h = 32, 32
    im = Image.new("RGBA", (w, h), (255, 207, 117, 255))
    px = im.load()
    rnd = random.Random(42)
    light = (255, 231, 176, 255)
    dark = (196, 143, 68, 255)
    # 2x2 speckles read clearly once nearest-upscaled, unlike single stray pixels.
    placed = set()
    for _ in range(90):
        x, y = rnd.randrange(0, w, 2), rnd.randrange(0, h, 2)
        if (x, y) in placed:
            continue
        placed.add((x, y))
        c = dark if rnd.random() < .55 else light
        for dx in range(2):
            for dy in range(2):
                px[(x + dx) % w, (y + dy) % h] = c
    save(im, "Sand.png")


adobe_tile()
sand_tile()
