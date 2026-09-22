"""Phase 4 (Costa redesign): pixel-art swimmer (stylized tribute to Jose Olaya,
see CULTURAL_SOURCES.md) and his splash.

Three-frame front-crawl stroke (reach / pull / glide) plus a small foam
splash sprite, same technique as Tools/gen_costa_boat_sprites.py: flat
unblended colors on a small canvas, upscaled with nearest-neighbour
resampling. This is an original stylized silhouette, not a documented or
photographic likeness - no such record exists for a 19th-century figure.

Run with: python Tools/gen_costa_swimmer_sprites.py
"""
import os
from PIL import Image, ImageDraw

OUT_DIR = "Assets/SuyuRun/Art/Resources/Swimmer"
UPSCALE = 8

SKIN = (108, 68, 44, 255)
SKIN_SHADE = (84, 50, 32, 255)
HAIR = (43, 30, 22, 255)
SHORTS = (167, 39, 34, 255)
FOAM = (250, 245, 224, 255)
FOAM_SOFT = (196, 219, 214, 140)
TRANSPARENT = (0, 0, 0, 0)


def new_canvas(w, h):
    return Image.new("RGBA", (w, h), TRANSPARENT)


def save(im, name):
    os.makedirs(OUT_DIR, exist_ok=True)
    big = im.resize((im.width * UPSCALE, im.height * UPSCALE), Image.NEAREST)
    path = os.path.join(OUT_DIR, name)
    big.save(path)
    print("saved", path, big.size)


def swimmer_frame(name, pose):
    w, h = 26, 14
    im = new_canvas(w, h)
    d = ImageDraw.Draw(im)
    # Streamlined body, front crawl, swimming toward +x (screen right).
    d.ellipse([16, 4, 22, 9], fill=SKIN)  # head, low in the water
    d.polygon([(6, 6), (18, 5), (18, 9), (6, 9)], fill=SKIN)  # torso/back
    d.line([(6, 7), (2, 8)], fill=SKIN_SHADE, width=2)  # trailing hip/legs
    d.rectangle([16, 8, 19, 9], fill=SHORTS)
    if pose == "reach":
        d.line([(17, 4), (23, 1)], fill=SKIN, width=2)  # lead arm reaching forward, out of water
        d.line([(9, 8), (4, 12)], fill=SKIN, width=2)   # trailing arm recovering underwater
    elif pose == "pull":
        d.line([(15, 4), (19, 0)], fill=SKIN, width=2)  # lead arm entering the water
        d.line([(9, 7), (13, 11)], fill=SKIN, width=2)  # trailing arm mid-pull
    else:  # glide
        d.line([(14, 5), (18, 8)], fill=SKIN, width=2)
        d.line([(9, 7), (7, 11)], fill=SKIN, width=2)
    save(im, f"Swimmer{name}.png")


def splash():
    w, h = 14, 10
    im = new_canvas(w, h)
    d = ImageDraw.Draw(im)
    d.ellipse([1, 6, 12, 9], fill=FOAM_SOFT)
    for x, y, l in [(2, 4, 3), (5, 1, 5), (8, 2, 4), (11, 5, 2)]:
        d.line([(x, 8), (x, 8 - l)], fill=FOAM, width=1)
    save(im, "Splash.png")


swimmer_frame("Reach", "reach")
swimmer_frame("Pull", "pull")
swimmer_frame("Glide", "glide")
splash()
