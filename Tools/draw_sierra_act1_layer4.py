"""Sierra capa 4, Acto 1 (Camino de Tierra), 640x180 transparente. Read
ART_BIBLE.md first. Factor ~0.35. Redone to match the "sierra 3" reference
directly: a WOODEN fence (posts + rails), not stone - that was the mistake
in the first pass. Per-act layer, plays 0-57s (fusion into Acto 2 51-57s),
timing confirmed against sierra_rhythm.json's real 171s MIDI duration.
"""
from pathlib import Path
import hashlib
import json
from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parents[1]
ART = ROOT / "Assets/SuyuRun/Art/PixelSierra"
PREVIEW = ROOT / "Artifacts/ArtApproval"
C = [tuple(bytes.fromhex(h[1:])) + (255,) for h in json.loads((ART/"sierra_palette.json").read_text(encoding="utf-8"))["colors"]]
protected = [ART/"sierra_capa1_cielo.png", ART/"sierra_capa2_nevados.png", ART/"sierra_capa3_cerros.png"]
before = {p.name: hashlib.sha256(p.read_bytes()).hexdigest() for p in protected}

W, H = 640, 180
im = Image.new("RGBA", (W, H), (0, 0, 0, 0))
d = ImageDraw.Draw(im)

# Near-mid hillside ground mass the path/fence sit on, dark olive.
d.polygon([(-10,180),(-10,120),(120,96),(260,110),(400,90),(540,108),(650,96),(650,180)], fill=C[12])
# Sunlit grass patches, hard-edged, breaking up the flat green.
for x, y, w in [(40,130,50),(180,116,60),(340,100,55),(470,118,50),(590,104,45)]:
    d.ellipse((x, y, x+w, y+16), fill=C[14])

# Dirt path, winding, with a visible lighter dust band down the middle (still flat color, no
# gradient) and scattered stone/dirt texture points instead of a single flat fill.
path_top = [(-10,150),(90,140),(180,148),(300,132),(410,140),(520,128),(650,134)]
path_bot = [(-10,168),(90,160),(180,166),(300,152),(410,160),(520,150),(650,154)]
d.polygon(path_top + path_bot[::-1], fill=C[16])
mid_top = [(x, y+4) for x, y in path_top]
mid_bot = [(x, y-4) for x, y in path_bot]
d.polygon(mid_top + mid_bot[::-1], fill=C[17])
for i in range(len(path_top)-1):
    d.line([path_top[i], path_top[i+1]], fill=C[15])
    d.line([path_bot[i], path_bot[i+1]], fill=C[15])
for x in range(0, W, 11):
    y = 147 + (x * 7 % 14)
    d.point((x, y), fill=C[9] if x % 22 else C[15])

# Wooden fence, matching the reference: posts + two horizontal rails, hard-edged, brown tones
# (reusing the ichu-gold family's darker end for wood since Sierra's 24 colors have no separate
# brown - C[13]/C[12] read as weathered wood here, distinct from the stone grays).
def fence(x0, length, y0, h=16):
    for x in range(x0, x0+length, 18):
        d.rectangle((x, y0-h, x+3, y0), fill=C[8])
        d.rectangle((x+1, y0-h, x+2, y0), fill=C[9])
    d.rectangle((x0, y0-h+3, x0+length, y0-h+6), fill=C[9])
    d.rectangle((x0, y0-6, x0+length, y0-3), fill=C[9])

fence(30, 170, 126)
fence(360, 150, 116)
fence(560, 80, 118)

for yy in range(H):
    im.putpixel((0, yy), (0, 0, 0, 0))
    im.putpixel((W-1, yy), (0, 0, 0, 0))

im.save(ART / "sierra_act1_camino.png")

pixels = list(im.getdata())
assert all(c[3] in (0, 255) and (c[3] == 0 or c in C) for c in pixels), "palette/alpha violation"
assert before == {p.name: hashlib.sha256(p.read_bytes()).hexdigest() for p in protected}, "earlier layer changed"

sky = Image.open(ART/"sierra_capa1_cielo.png").convert("RGBA").crop((160, 0, 480, 180))
nevados = Image.open(ART/"sierra_capa2_nevados.png").convert("RGBA").crop((160, 0, 480, 180))
cerros = Image.open(ART/"sierra_capa3_cerros.png").convert("RGBA").crop((160, 0, 480, 180))
preview = sky.copy()
preview.alpha_composite(nevados, (0, 0))
preview.alpha_composite(cerros, (0, 0))
preview.alpha_composite(im.crop((160, 0, 480, 180)), (0, 0))
preview.convert("RGB").resize((1280, 720), Image.Resampling.NEAREST).save(PREVIEW / "sierra_act1_camino_preview_x4.png")

report = {
    "size": [W, H], "factor": 0.35, "act": "1 - Camino de Tierra",
    "range_seconds": [0, 57], "fusion_into_act2_seconds": [51, 57],
    "content": "sendero de tierra con franja de polvo mas clara, cerco de MADERA (postes + 2 rieles, no piedra), parches de pasto",
    "palette_compliant": True, "binary_alpha": True,
    "previous_layers_unchanged": before,
    "status": "pending_user_approval",
}
(PREVIEW / "sierra_act1_camino_validation.json").write_text(json.dumps(report, indent=2), encoding="utf-8")
print(json.dumps(report, indent=2))
