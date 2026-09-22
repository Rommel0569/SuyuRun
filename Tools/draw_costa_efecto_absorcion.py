"""Absorption vortex effect: plays over the hero right as the dimensional
door pulls him in at the end of Costa. Rotating spiral arms of streaks
converging on a bright core, same teal-to-gold language as
costa_puerta_dimensional.png's inner glow, so the two read as one event.
Read ART_BIBLE.md first. Kept as its own file (not added into
costa_efectos.png, which is already approved and must not be rewritten).
Palette-only, hard 1px-scale streaks, binary alpha, 4 rotating frames.
"""
from pathlib import Path
import hashlib
import json
import math
from PIL import Image, ImageDraw

ROOT=Path(__file__).resolve().parents[1]
ART=ROOT/'Assets/SuyuRun/Art/PixelCosta'
OUT=ROOT/'Artifacts/ArtApproval'
C=[tuple(bytes.fromhex(h[1:]))+(255,) for h in json.loads((ART/'costa_palette.json').read_text(encoding='utf-8'))['colors']]
protected=[p for p in ART.iterdir() if p.is_file() and not p.name.startswith('costa_efecto_absorcion')]
before={p.name:hashlib.sha256(p.read_bytes()).hexdigest() for p in protected}

FW,FH=40,40
FRAMES=4
sheet=Image.new('RGBA',(FW*FRAMES,FH),(0,0,0,0))
d=ImageDraw.Draw(sheet)

ARMS=4
BAND_COLORS=[C[17],C[18],C[19],C[9],C[10]]

def draw_frame(fi):
    cx,cy=fi*FW+FW//2,FH//2
    ang0=fi*(math.pi/2/ (FRAMES))*2  # steady rotation step between frames
    for arm in range(ARMS):
        base_ang=ang0+arm*(2*math.pi/ARMS)
        # Each arm is a short spiral of points stepping inward and rotating
        # as it approaches the core - a pulled-in streak, not a static ray.
        r=16.0
        ang=base_ang
        step=0
        while r>2:
            x=cx+math.cos(ang)*r
            y=cy+math.sin(ang)*r*0.65  # slight vertical squash, matches the door's trapezoid framing
            col=BAND_COLORS[min(step,len(BAND_COLORS)-1)]
            d.point((int(x),int(y)),fill=col)
            r-=2.1
            ang+=0.55
            step+=1
    # Bright absorbing core.
    d.ellipse((cx-3,cy-3,cx+3,cy+3),fill=C[12])
    d.ellipse((cx-1,cy-1,cx+1,cy+1),fill=C[11])

for f in range(FRAMES):
    draw_frame(f)

sheet.save(ART/'costa_efecto_absorcion.png')

pixels=list(sheet.getdata())
assert all(c[3] in (0,255) and (c[3]==0 or c in C) for c in pixels), 'palette/alpha violation'
assert before=={p.name:hashlib.sha256(p.read_bytes()).hexdigest() for p in protected}, 'an approved asset changed'

scale=7
preview=sheet.resize((FW*FRAMES*scale,FH*scale),Image.Resampling.NEAREST)
OUT.mkdir(parents=True,exist_ok=True)
preview.save(OUT/'costa_efecto_absorcion_preview_x7.png')
frames=[sheet.crop((i*FW,0,i*FW+FW,FH)).resize((FW*scale,FH*scale),Image.Resampling.NEAREST) for i in range(FRAMES)]
frames[0].save(OUT/'costa_efecto_absorcion.gif',save_all=True,append_images=frames[1:],duration=90,loop=0)

report={'size_per_frame':[FW,FH],'frames':FRAMES,
        'purpose':'absorption vortex played over the hero as the dimensional door pulls him in at the end of Costa; same teal-to-gold language as the door glow',
        'palette_compliant':True,'binary_alpha':True,
        'previous_assets_unchanged':before,
        'status':'pending_user_approval'}
(OUT/'costa_efecto_absorcion_validation.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
print(json.dumps(report,indent=2))
