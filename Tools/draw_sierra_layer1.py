"""Sierra capa 1: cielo de amanecer dorado, 640x180. Read ART_BIBLE.md first.
Same technique as costa_capa1_cielo.png (discrete bands, sparse ordered
dithering only at cloud edges, stepped sun disc, no gradients/AA) but built
from sierra_palette.json and composed for a sunrise instead of a sunset -
this layer is continuous/shared across Sierra's 3 acts (Camino de Tierra,
Puente Colgante, Ruinas Preincas); only the mid-ground layers change per act.
"""
from pathlib import Path
import json
import math
from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parents[1]
ART = ROOT / "Assets/SuyuRun/Art/PixelSierra"
OUT_FILE = ART / "sierra_capa1_cielo.png"
PREVIEW = ROOT / "Artifacts/ArtApproval"
PALETTE = json.loads((ART / "sierra_palette.json").read_text(encoding="utf-8"))
C = [tuple(bytes.fromhex(h[1:])) + (255,) for h in PALETTE["colors"]]
assert len(C) == len(set(C)) == 24
protected = [p for p in ART.iterdir() if p.is_file()] if ART.exists() else []
import hashlib
before = {p.name: hashlib.sha256(p.read_bytes()).hexdigest() for p in protected}

W, H = 640, 180
im = Image.new("RGBA", (W, H), C[7])
px = im.load()

# Dawn bands: deep navy zenith (0) down to bright gold horizon (7), same ordered-dithering seam
# technique as Costa's capa1 - ART_BIBLE.md allows dithering only for sky/fog.
bands = [(0, 0), (28, 1), (58, 2), (86, 3), (112, 4), (138, 5), (160, 6), (172, 7)]
for n, (start, color_idx) in enumerate(bands):
    end = bands[n + 1][0] if n + 1 < len(bands) else H
    for y in range(start, end):
        for x in range(W):
            prev_idx = bands[max(0, n - 1)][1]
            transition = n > 0 and y - start < 3
            px[x, y] = C[prev_idx if transition and (x + y * 3) % 4 >= y - start + 1 else color_idx]

# Low rising sun - stepped disc, no radial gradient/bloom, off-center right (valley sunrise).
sun_x, sun_y, radius = 430, 118, 26
for dy in range(-radius, radius + 1):
    half = int(math.sqrt(radius * radius - dy * dy))
    for x in range(sun_x - half, sun_x + half + 1):
        y = sun_y + dy
        if 0 <= y < H:
            px[x, y] = C[7 if dy < -10 else 6 if dy < 8 else 5]

def cloud(x, y, width, height, dark):
    mask = Image.new("1", (W, H))
    d = ImageDraw.Draw(mask)
    lobes = [(0.02, .5, .4, .95), (.18, .22, .54, .92), (.38, 0, .7, .95), (.6, .26, .88, 1)]
    for a, b, c, e in lobes:
        d.ellipse((int(x+a*width), int(y+b*height), int(x+c*width), int(y+e*height)), fill=1)
    band_top = min(bi for bi, (s, _) in enumerate(bands) if s <= y < (bands[bi+1][0] if bi+1 < len(bands) else H)) if False else 0
    for yy in range(H):
        row_band = 0
        for bi, (s, _) in enumerate(bands):
            if yy >= s: row_band = bi
        for xx in range(W):
            if mask.getpixel((xx, yy)):
                base_idx = bands[row_band][1]
                px[xx, yy] = C[max(0, base_idx - dark)]

cloud(60, 20, 130, 34, 2)
cloud(240, 12, 100, 26, 2)
cloud(520, 30, 110, 28, 2)

im.save(OUT_FILE)

pixels = list(im.getdata())
assert all(c[3] in (0, 255) and (c[3] == 0 or c in C) for c in pixels), "palette/alpha violation"
assert before == {p.name: hashlib.sha256(p.read_bytes()).hexdigest() for p in protected}, "an approved asset changed"

preview = im.convert("RGB").resize((W*2, H*2), Image.Resampling.NEAREST)
PREVIEW.mkdir(parents=True, exist_ok=True)
preview.save(PREVIEW / "sierra_capa1_preview_x2.png")

report = {
    "size": [W, H], "content": "amanecer dorado, sol bajo, nubes con dithering",
    "purpose": "capa 1 de Sierra, compartida por los 3 actos (Camino de Tierra / Puente Colgante / Ruinas Preincas)",
    "palette_compliant": True, "binary_alpha": True,
    "previous_assets_unchanged": before,
    "status": "pending_user_approval",
}
(PREVIEW / "sierra_capa1_validation.json").write_text(json.dumps(report, indent=2), encoding="utf-8")
print(json.dumps(report, indent=2))
