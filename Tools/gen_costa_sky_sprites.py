"""Phase 5 (Costa redesign): pixel-art mountains and clouds.

Four mountain-silhouette variants and four cloud variants, each drawn with a
different seeded jagged/fluffy profile so RunnerParallax.cs/RunnerCoastalSky.cs
can pick a "randomized but controlled" combination per instance (a small,
fixed set of interchangeable art pieces, not truly unbounded randomness).
Same flat-color, no-antialiasing, nearest-neighbour-upscale technique as the
other Tools/gen_costa_*.py scripts.

Run with: python Tools/gen_costa_sky_sprites.py
"""
import os
import random
from PIL import Image, ImageDraw

OUT_DIR = "Assets/SuyuRun/Art/Resources/Sky"
UPSCALE = 8

FACE = (219, 145, 89, 255)
WALL = (173, 105, 82, 255)
SUMMIT = (250, 196, 122, 255)
SHADE_TINTS = [0, 10, -8, 18]

CLOUD_SHADE = (186, 224, 230, 148)
CLOUD_WHITE = (250, 245, 217, 191)


def new_canvas(w, h):
    return Image.new("RGBA", (w, h), (0, 0, 0, 0))


def save(im, name):
    os.makedirs(OUT_DIR, exist_ok=True)
    big = im.resize((im.width * UPSCALE, im.height * UPSCALE), Image.NEAREST)
    path = os.path.join(OUT_DIR, name)
    big.save(path)
    print("saved", path, big.size)


def tint(color, amount):
    r, g, b, a = color
    return (max(0, min(255, r + amount)), max(0, min(255, g + amount)), max(0, min(255, b + amount)), a)


def mountain(name, seed):
    w, h = 72, 34
    rnd = random.Random(seed)
    im = new_canvas(w, h)
    d = ImageDraw.Draw(im)
    face = tint(FACE, SHADE_TINTS[seed % len(SHADE_TINTS)])
    wall = tint(WALL, SHADE_TINTS[seed % len(SHADE_TINTS)])
    # A jagged ridge line built from a handful of random peaks, base at the bottom.
    n_peaks = rnd.randint(3, 5)
    xs = sorted(rnd.sample(range(6, w - 6), n_peaks))
    points = [(0, h - 2)]
    for x in xs:
        peak_h = rnd.randint(10, h - 6)
        points.append((x, h - peak_h))
    points.append((w, h - 2))
    poly = points + [(w, h), (0, h)]
    d.polygon(poly, fill=face)
    # Shaded flank on the right side of each peak for a little volume.
    for i in range(1, len(points) - 1):
        px, py = points[i]
        nx, ny = points[i + 1]
        d.polygon([(px, py), (nx, ny), (nx, h), (px, h)], fill=wall)
    # Summit highlight dashes.
    for i in range(1, len(points) - 1):
        px, py = points[i]
        d.point((px, py), fill=SUMMIT)
        d.point((px - 1, py + 1), fill=SUMMIT)
    save(im, name)


def cloud(name, seed):
    w, h = 44, 20
    rnd = random.Random(seed)
    im = new_canvas(w, h)
    d = ImageDraw.Draw(im)
    base_y = rnd.randint(11, 13)
    d.ellipse([2, base_y, w - 2, base_y + 4], fill=CLOUD_SHADE)
    n_lobes = rnd.randint(3, 4)
    for i in range(n_lobes):
        cx = 6 + i * (w - 12) / max(1, n_lobes - 1) + rnd.randint(-2, 2)
        r = rnd.randint(7, 11)
        cy = base_y - r * 0.55 + rnd.randint(-2, 2)
        d.ellipse([cx - r, cy - r, cx + r, cy + r * 0.85], fill=CLOUD_WHITE)
    save(im, name)


for i in range(4):
    mountain(f"Mountain{i}.png", seed=100 + i * 7)
for i in range(4):
    cloud(f"Cloud{i}.png", seed=200 + i * 5)
