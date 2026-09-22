"""Costa results screen + end-of-level achievement animation. Read ART_BIBLE.md first.
Panel art has blank text zones - real copy renders with the pixel font
(Press Start 2P / m5x7) as a Unity font asset, not hand-drawn letters baked
into this PNG (see ART_APPROVALS.md ambiguity note on the UI text exception).
The achievement animation reuses the hero's base art: 4 frames of the golden
glow building up as arms appear, unlocking the Sierra power.
"""
from pathlib import Path
import hashlib
import json
from PIL import Image, ImageDraw

ROOT=Path(__file__).resolve().parents[1]
ART=ROOT/'Assets/SuyuRun/Art/PixelCosta'
OUT=ROOT/'Artifacts/ArtApproval'
C=[tuple(bytes.fromhex(h[1:]))+(255,) for h in json.loads((ART/'costa_palette.json').read_text(encoding='utf-8'))['colors']]
protected=[p for p in ART.iterdir() if p.name.startswith(('costa_capa','costa_plataformas','costa_heroe','costa_obstaculos','costa_coleccionables','costa_efectos')) or p.name=='costa_palette.json']
before={p.name:hashlib.sha256(p.read_bytes()).hexdigest() for p in protected}

# --- Results panel, 9-slice adobe frame with blank zones for the real pixel font ---
W,H=220,140
panel=Image.new('RGBA',(W,H),(0,0,0,0));d=ImageDraw.Draw(panel)
d.rectangle((0,0,W-1,H-1),fill=C[0])
d.rectangle((2,2,W-3,H-3),fill=C[15])
d.rectangle((4,4,W-5,H-5),fill=C[6])
d.rectangle((4,4,W-5,20),fill=C[2])  # title bar (blank - real text later)
d.line((4,20,W-5,20),fill=C[22])
row_icons=[C[22],C[19],C[9]]  # moneda / pieza / puntos accent colors
for i,tint in enumerate(row_icons):
    y=28+i*16
    d.rectangle((10,y,20,y+10),fill=tint)
    d.rectangle((28,y+3,W-14,y+7),fill=C[3])  # blank number bar
for i in range(4):
    x=10+i*24
    d.rectangle((x,H-46,x+18,H-28),fill=C[0])
    d.rectangle((x+1,H-45,x+17,H-29),fill=C[15])
for i,bx in enumerate((10,W-96)):
    d.rectangle((bx,H-18,bx+86,H-6),fill=C[1])
    d.rectangle((bx+1,H-17,bx+85,H-7),fill=C[6] if i==0 else C[18])
panel.save(ART/'costa_resultados.png')

# --- Achievement animation: 4 frames appended to a small dedicated sheet, same figure as the hero ---
FS=48
OUTLINE=C[0]; SKIN=C[14]; SKIN_SHADE=C[15]; TUNIC=C[8]; TUNIC_SHADE=C[6]; TUNIC_TRIM=C[22]
HAIR=C[1]; SASH=C[4]; GLOW=C[11]

def base_body(d,gx,gy):
    d.line((gx-4,32,gx-4,gy),fill=OUTLINE,width=5);d.line((gx-4,32,gx-4,gy),fill=SKIN_SHADE,width=3)
    d.line((gx+4,32,gx+4,gy),fill=OUTLINE,width=5);d.line((gx+4,32,gx+4,gy),fill=SKIN,width=3)
    d.polygon([(gx-9,20),(gx+9,20),(gx+13,32),(gx-13,32)],fill=OUTLINE)
    d.polygon([(gx-8,21),(gx+8,21),(gx+12,31),(gx-12,31)],fill=TUNIC)
    d.polygon([(gx+1,22),(gx+8,21),(gx+12,31),(gx+1,31)],fill=TUNIC_SHADE)
    d.line((gx-11,30,gx+11,30),fill=TUNIC_TRIM,width=1)
    d.line((gx-2,23,gx+2,28),fill=SASH,width=2)
    r=7
    d.ellipse((gx-r,14-r,gx+r,14+r),fill=OUTLINE);d.ellipse((gx-r+1,14-r+1,gx+r-1,14+r-1),fill=SKIN)
    d.pieslice((gx-r,14-r,gx+r,14+r),200,340,fill=HAIR);d.pieslice((gx-r+1,14-r+1,gx+r-1,14+r-1),205,335,fill=HAIR)

def logro(step):
    im=Image.new('RGBA',(FS,FS),(0,0,0,0));d=ImageDraw.Draw(im)
    gx,gy=24,44
    base_body(d,gx,gy)
    if step>=1:
        # Arm silhouettes fade in from step 1, fully solid by step 2+.
        d.line((gx-8,25,gx-16,29+step),fill=OUTLINE,width=4)
        d.line((gx-8,25,gx-16,29+step),fill=SKIN,width=2)
        d.line((gx+8,25,gx+16,29+step),fill=OUTLINE,width=4)
        d.line((gx+8,25,gx+16,29+step),fill=SKIN,width=2)
    if step>=2:
        rr=10+step*3
        import math
        for a in range(0,360,20):
            px=gx+round(rr*math.cos(math.radians(a)));py=14+round(rr*.7*math.sin(math.radians(a)))
            d.point((px,py),fill=GLOW)
    if step==3:
        d.line((gx-14,12,gx+14,12),fill=GLOW);d.line((gx,-2,gx,26),fill=(0,0,0,0))
    return im

sheet=Image.new('RGBA',(FS*4,FS),(0,0,0,0))
for i in range(4):
    sheet.alpha_composite(logro(i),(i*FS,0))
sheet.save(ART/'costa_heroe_logro.png')

pixels=list(panel.get_flattened_data())+list(sheet.get_flattened_data())
assert all(c[3]==0 or c in C for c in pixels),'off-palette or non-binary alpha pixel found'
assert before=={p.name:hashlib.sha256(p.read_bytes()).hexdigest() for p in protected}

OUT.mkdir(parents=True,exist_ok=True)
panel.resize((panel.width*4,panel.height*4),Image.Resampling.NEAREST).save(OUT/'costa_resultados_preview_x4.png')
sheet.resize((sheet.width*4,sheet.height*4),Image.Resampling.NEAREST).save(OUT/'costa_heroe_logro_preview_x4.png')

report={'resultados_size':[W,H],'logro_size':[sheet.width,sheet.height],'logro_frames':4,
        'text_note':'panel has blank zones; real numbers/labels render with the pixel font as a Unity font asset, not baked into this PNG',
        'previous_assets_unchanged':before,
        'purpose':'results screen art + achievement animation only; scoring, button logic and Sierra unlock wired separately in Unity',
        'status':'pending_user_approval'}
(OUT/'costa_resultados_validation.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
print(json.dumps(report,indent=2))
