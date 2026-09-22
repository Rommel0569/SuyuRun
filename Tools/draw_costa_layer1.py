"""One original 640x180 pixel-art sky; no composited layers or borrowed pixels.

User-authorized Pillow workflow. Read ART_BIBLE.md in full before edits.
Production image is native resolution. Preview is nearest-neighbor x4 only.
"""
from pathlib import Path
import json
import math
from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parents[1]
ART = ROOT / "Assets/SuyuRun/Art/PixelCosta"
OUT = ART / "costa_capa1_cielo.png"
PREVIEW = ROOT / "Artifacts/ArtApproval"
PALETTE = json.loads((ART / "costa_palette.json").read_text(encoding="utf-8"))
COLORS = [tuple(bytes.fromhex(h[1:])) + (255,) for h in PALETTE["colors"]]
assert len(COLORS) == len(set(COLORS)) == 24
W, H = 640, 180
im = Image.new("RGBA", (W, H), COLORS[6])
px = im.load()

# Discrete sky bands, with sparse 1px ordered dithering at their boundaries.
# No interpolation, smooth gradients, anti-aliasing or postprocessing.
bands = [(0, 6), (34, 7), (72, 8), (106, 9), (146, 8), (170, 7)]
for n, (start, color) in enumerate(bands):
    end = bands[n + 1][0] if n + 1 < len(bands) else H
    for y in range(start, end):
        for x in range(W):
            old = bands[max(0, n - 1)][1]
            transition = n > 0 and y - start < 4
            px[x, y] = COLORS[old if transition and (x + y * 3) % 4 >= y - start + 1 else color]

# A low, stepped solar disc, avoiding fake bloom or radial gradient rings.
sun_x, sun_y, radius = 365, 100, 24
for dy in range(-radius, radius + 1):
    half = int(math.sqrt(radius * radius - dy * dy))
    for x in range(sun_x - half, sun_x + half + 1):
        y = sun_y + dy
        px[x, y] = COLORS[12 if dy < -8 else 11 if dy < 11 else 10]


def cloud(x, y, width, height, seed, dark=4):
    """Individually authored cloud mass with stepped contour and lit under-rim."""
    mask = Image.new("1", (W, H))
    d = ImageDraw.Draw(mask)
    # Low broad silhouette with asymmetric lobes, drawn at native resolution.
    lobes = [(0.02, .48, .38, .94), (.16, .2, .52, .92),
             (.36, 0, .69, .94), (.57, .24, .85, .98), (.74, .49, 1, 1)]
    for a, b, c, e in lobes:
        d.ellipse((int(x+a*width), int(y+b*height), int(x+c*width), int(y+e*height)), fill=1)
    d.polygon([(x+int(width*.05), y+int(height*.74)),
               (x+int(width*.93), y+int(height*.71)),
               (x+width+8, y+height-2), (x+int(width*.1), y+height+2)], fill=1)
    mp = mask.load()
    for xx in range(max(0, x-1), min(W, x+width+9)):
        ys = [yy for yy in range(max(0,y),min(H,y+height+4)) if mp[xx,yy]]
        if not ys:
            continue
        top, bottom = min(ys), max(ys)
        for yy in ys:
            depth = bottom-yy
            # Broad color clusters; dithering restricted to cloud-edge transitions.
            color = dark
            if yy-top < 3 and (xx+yy+seed)%4 == 0:
                color = 5
            if depth < 3:
                color = 8 if (xx+seed)%13 < 8 else 7
            elif depth < 8:
                color = 6 if (xx+yy)%2 else 7
            elif depth < 11 and (xx+yy)%4 == 0:
                color = 6
            elif yy-top > 7 and ((xx//11+seed)%4 == 0) and depth > 14:
                color = 3
            px[xx,yy] = COLORS[color]
    # Select a few broad interior horizontal streaks, not random speckle.
    for k in range(4):
        yy=y+int(height*(.36+k*.13))
        xa=x+int(width*(.17+(k%2)*.2))
        for xx in range(max(0,xa), min(W,xa+int(width*(.2+k*.02)))):
            if 0<=yy<H and mp[xx,yy]:
                px[xx,yy]=COLORS[5 if k<2 else 6]


# Left/right framing keeps the central sunlight and future play silhouettes clear.
cloud(-25, 24, 132, 41, 1)
cloud(105, 27, 155, 48, 2)
cloud(255, -16, 190, 45, 3)
cloud(433, 42, 151, 43, 4)
cloud(582, 7, 105, 43, 5)
cloud(34, 82, 116, 24, 6, dark=5)
cloud(482, 102, 121, 18, 7, dark=5)

# Thin warm cloud wisps, distinct from the moving high-cloud layer to come.
draw = ImageDraw.Draw(im)
wisps = [(181,89,45),(238,60,46),(290,40,45),(393,71,49),
         (317,128,68),(220,112,33),(44,137,66),(540,145,55)]
for i,(x,y,length) in enumerate(wisps):
    draw.line((x,y,x+length,y), fill=COLORS[8 if i%2 else 10], width=1)
    draw.line((x+7,y+1,x+length-11,y+1), fill=COLORS[8], width=1)

ART.mkdir(parents=True, exist_ok=True)
PREVIEW.mkdir(parents=True, exist_ok=True)
im.save(OUT)
# Approval preview: one 320px-wide viewport, not a sliced parallax layer.
im.crop((160,0,480,180)).resize((1280,720), Image.Resampling.NEAREST).save(PREVIEW / "costa_capa1_cielo_preview_x4.png")

actual=set(im.getdata())
assert im.size==(640,180) and im.mode=="RGBA"
assert actual.issubset(set(COLORS))
assert all(c[3]==255 for c in actual)
report={"asset":str(OUT.relative_to(ROOT)),"size":[640,180],"mode":"RGBA",
        "palette_colors":24,"used_colors":len(actual),"outside_palette":0,
        "partial_alpha_pixels":0,"scaling":"native production; nearest x4 preview",
        "parallax_factor":0.0,"seamless_required":False,"status":"pending_user_approval",
        "content":"sky, low sun, dithered clouds only"}
(PREVIEW / "costa_capa1_validation.json").write_text(json.dumps(report,indent=2), encoding="utf-8")
print(json.dumps(report,indent=2))
