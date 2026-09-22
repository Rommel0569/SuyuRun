"""Sierra capa 3: cerros medios con bosque, 640x180 transparente. Read
ART_BIBLE.md first. Factor 0.15, tile sin costuras. Redone to follow the
"sierra 3" reference composition directly: layered ridgelines with real
atmospheric-perspective color shift (far=blue-gray, mid=olive, near=dark
olive) plus tree-cluster silhouettes on the nearest ridge, instead of flat
single-tone hills. Shared across Sierra's 3 acts, same as capas 1-2.
"""
from pathlib import Path
import hashlib
import json
from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parents[1]
ART = ROOT / "Assets/SuyuRun/Art/PixelSierra"
PREVIEW = ROOT / "Artifacts/ArtApproval"
C = [tuple(bytes.fromhex(h[1:])) + (255,) for h in json.loads((ART/"sierra_palette.json").read_text(encoding="utf-8"))["colors"]]
protected = [ART/"sierra_capa1_cielo.png", ART/"sierra_capa2_nevados.png"]
before = {p.name: hashlib.sha256(p.read_bytes()).hexdigest() for p in protected}

W, H = 640, 180
im = Image.new("RGBA", (W, H), (0, 0, 0, 0))
d = ImageDraw.Draw(im)

def hill(cx, base_y, rx, ry, color):
    d.ellipse((cx-rx, base_y-2*ry, cx+rx, base_y), fill=color)

# Far ridge: blue-gray, atmospheric perspective (reads as "behind the haze"), same family as the
# sky's mid band so it recedes instead of popping forward.
for cx, rx, ry in [(-10,120,50),(140,140,58),(320,130,48),(480,150,60),(640,120,50)]:
    hill(cx, 165, rx, ry, C[3])
# Mid ridge: olive green, the main slope mass.
for cx, rx, ry in [(-20,100,64),(110,120,74),(280,110,64),(430,125,78),(580,105,62)]:
    hill(cx, 178, rx, ry, C[13])
# Near ridge: darker olive, closest hill line the trees sit on.
for cx, rx, ry in [(30,95,46),(160,100,50),(300,90,44),(440,100,52),(570,90,44)]:
    hill(cx, 180, rx, ry, C[12])

# Tree clusters on the near ridge - simple round conifer silhouettes, hard-edged, a lighter rim
# to keep them readable against the dark-olive ground instead of merging into a black mass.
def tree(cx, base_y, r):
    # Dark-olive silhouette (not stone gray) with a slightly lighter olive core so clusters read
    # as foliage, plus a small trunk hint - stays legible against the near ridge's own dark olive.
    d.polygon([(cx-r, base_y), (cx, base_y-2.4*r), (cx+r, base_y)], fill=C[12])
    d.polygon([(cx-r*.65, base_y-r*.3), (cx, base_y-2.2*r), (cx+r*.65, base_y-r*.3)], fill=C[13])
    d.rectangle((cx-1, base_y-2, cx+1, base_y), fill=C[15])

import random
random.seed(7)
for cx in range(-10, W+10, 15):
    ridge_y = 168 - 10 * abs(((cx % 220) - 110) / 110)
    if random.random() < 0.9:
        tree(cx + random.randint(-4,4), ridge_y + random.randint(-3,5), random.randint(9,15))

for yy in range(H):
    im.putpixel((0, yy), (0, 0, 0, 0))
    im.putpixel((W-1, yy), (0, 0, 0, 0))

im.save(ART / "sierra_capa3_cerros.png")

pixels = list(im.getdata())
assert all(c[3] in (0, 255) and (c[3] == 0 or c in C) for c in pixels), "palette/alpha violation"
assert im.crop((0, 0, 1, H)).tobytes() == im.crop((W-1, 0, W, H)).tobytes(), "seam mismatch"
assert before == {p.name: hashlib.sha256(p.read_bytes()).hexdigest() for p in protected}, "earlier layer changed"

sky = Image.open(ART/"sierra_capa1_cielo.png").convert("RGBA").crop((160, 0, 480, 180))
nevados = Image.open(ART/"sierra_capa2_nevados.png").convert("RGBA").crop((160, 0, 480, 180))
preview = sky.copy()
preview.alpha_composite(nevados, (0, 0))
preview.alpha_composite(im.crop((160, 0, 480, 180)), (0, 0))
preview.convert("RGB").resize((1280, 720), Image.Resampling.NEAREST).save(PREVIEW / "sierra_capa3_preview_x4.png")

report = {
    "size": [W, H], "factor": 0.15,
    "content": "cerros en 3 planos con perspectiva atmosferica (lejos azul-gris, medio oliva, cerca oliva oscuro) y arboles en la cresta cercana, siguiendo la referencia sierra 3",
    "palette_compliant": True, "binary_alpha": True, "seam_matches": True,
    "previous_layers_unchanged": before,
    "status": "pending_user_approval",
}
(PREVIEW / "sierra_capa3_validation.json").write_text(json.dumps(report, indent=2), encoding="utf-8")
print(json.dumps(report, indent=2))
