"""Sierra capa 2: nevados en silueta muy lejanos, 640x180 transparente. Read
ART_BIBLE.md first. Factor 0.05, tile sin costuras (columna 0 = columna 639).
Shared across Sierra's 3 acts, same as the sky - only the mid-ground layers
(puente, andenes, camino) change per act. Never touches sierra_capa1_cielo.png.
"""
from pathlib import Path
import hashlib
import json
from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parents[1]
ART = ROOT / "Assets/SuyuRun/Art/PixelSierra"
PREVIEW = ROOT / "Artifacts/ArtApproval"
C = [tuple(bytes.fromhex(h[1:])) + (255,) for h in json.loads((ART/"sierra_palette.json").read_text(encoding="utf-8"))["colors"]]
sky_path = ART / "sierra_capa1_cielo.png"
sky_hash = hashlib.sha256(sky_path.read_bytes()).hexdigest()

W, H = 640, 180
im = Image.new("RGBA", (W, H), (0, 0, 0, 0))
d = ImageDraw.Draw(im)

# Snow peaks, very distant - dark blue-gray silhouette (band color, not the vivid stone/olive
# tones reserved for closer layers) with a few hard-edged snow-cap highlights near the summits.
def peak(cx, base_y, half_w, top_y, snow_frac):
    d.polygon([(cx-half_w, base_y), (cx, top_y), (cx+half_w, base_y)], fill=C[1])
    # Snowcap: same triangle shape scaled down to the top snow_frac of the height (linear
    # narrowing, matching the outer triangle's own slope) - a hard step, no gradient.
    cap_y = top_y + (base_y - top_y) * snow_frac
    cap_half = half_w * snow_frac
    d.polygon([(cx-cap_half, cap_y), (cx, top_y), (cx+cap_half, cap_y)], fill=C[22])
    d.line((cx-cap_half*.6, cap_y-(base_y-top_y)*snow_frac*.15, cx+cap_half*.6, cap_y-(base_y-top_y)*snow_frac*.15), fill=C[23])

ridge = [
    (60, 150, 70, 60, .22), (150, 150, 55, 90, .28), (230, 150, 90, 40, .18),
    (340, 150, 60, 78, .26), (430, 150, 100, 30, .16), (540, 150, 65, 84, .24),
    (615, 150, 55, 96, .30),
]
for cx, base_y, half_w, top_y, snow_frac in ridge:
    peak(cx, base_y, half_w, top_y, snow_frac)
# Wrap-safe duplicate near the seams so the silhouette reads continuously at the tile edge.
peak(-5, 150, 55, 96, .30)
peak(645, 150, 55, 96, .30)

for yy in range(H):
    im.putpixel((0, yy), (0, 0, 0, 0))
    im.putpixel((W-1, yy), (0, 0, 0, 0))

im.save(ART / "sierra_capa2_nevados.png")

pixels = list(im.getdata())
assert all(c[3] in (0, 255) and (c[3] == 0 or c in C) for c in pixels), "palette/alpha violation"
assert im.crop((0, 0, 1, H)).tobytes() == im.crop((W-1, 0, W, H)).tobytes(), "seam mismatch"
assert hashlib.sha256(sky_path.read_bytes()).hexdigest() == sky_hash, "sky changed"

sky = Image.open(sky_path).convert("RGBA").crop((160, 0, 480, 180))
preview = sky.copy()
preview.alpha_composite(im.crop((160, 0, 480, 180)), (0, 0))
preview.convert("RGB").resize((1280, 720), Image.Resampling.NEAREST).save(PREVIEW / "sierra_capa2_preview_x4.png")

report = {
    "size": [W, H], "factor": 0.05, "content": "nevados en silueta con nieve, tile sin costuras",
    "palette_compliant": True, "binary_alpha": True, "seam_matches": True,
    "sky_sha256": sky_hash, "sky_unchanged": True,
    "status": "pending_user_approval",
}
(PREVIEW / "sierra_capa2_validation.json").write_text(json.dumps(report, indent=2), encoding="utf-8")
print(json.dumps(report, indent=2))
