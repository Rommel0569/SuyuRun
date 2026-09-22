"""Costa effects. Read ART_BIBLE.md first.
Five small effect strips, each its own row: dust puff, water splash, coin
glow, collectible flash, glowing lantern (2-frame flicker). Palette-only,
hard 1px outline where it applies, binary alpha, no dithering.
"""
from pathlib import Path
import hashlib
import json
from PIL import Image, ImageDraw

ROOT=Path(__file__).resolve().parents[1]
ART=ROOT/'Assets/SuyuRun/Art/PixelCosta'
OUT=ROOT/'Artifacts/ArtApproval'
C=[tuple(bytes.fromhex(h[1:]))+(255,) for h in json.loads((ART/'costa_palette.json').read_text(encoding='utf-8'))['colors']]
protected=[p for p in ART.iterdir() if p.name.startswith(('costa_capa','costa_plataformas','costa_heroe','costa_obstaculos','costa_coleccionables')) or p.name=='costa_palette.json']
before={p.name:hashlib.sha256(p.read_bytes()).hexdigest() for p in protected}

CELL=16

def polvo(step):
    # Expanding, fading dust puff - fades by using fewer, more scattered dots each step.
    im=Image.new('RGBA',(CELL,CELL),(0,0,0,0));d=ImageDraw.Draw(im)
    spread=2+step*2
    dots=[(-spread,0),(spread,0),(0,-spread//2),(-spread//2,-spread//2),(spread//2,-spread//2)][:5-step]
    for dx,dy in dots:
        d.point((8+dx,10+dy),fill=C[23] if step<2 else C[21])
    return im

def salpicadura(step):
    im=Image.new('RGBA',(CELL,CELL),(0,0,0,0));d=ImageDraw.Draw(im)
    if step==0:
        d.ellipse((5,11,11,13),fill=C[19])
    elif step==1:
        d.ellipse((4,10,12,13),fill=C[19])
        for dx in (-4,-2,2,4):d.line((8+dx,9,8+dx,7),fill=C[20])
    else:
        d.ellipse((5,12,11,13),fill=C[19])
        for dx in (-5,-2,2,5):d.point((8+dx,4),fill=C[20])
    return im

def brillo_moneda(step):
    im=Image.new('RGBA',(CELL,CELL),(0,0,0,0));d=ImageDraw.Draw(im)
    r=3+step*2
    for a in range(0,360,45):
        import math
        x=8+round(r*math.cos(math.radians(a)));y=8+round(r*math.sin(math.radians(a)))
        d.point((x,y),fill=C[22] if step==0 else C[12])
    if step==0:
        d.ellipse((6,6,10,10),fill=C[21])
    return im

def destello(step):
    im=Image.new('RGBA',(CELL,CELL),(0,0,0,0));d=ImageDraw.Draw(im)
    d.line((8,2,8,14),fill=C[12] if step else C[11])
    d.line((2,8,14,8),fill=C[12] if step else C[11])
    if step:
        d.line((3,3,13,13),fill=C[11]);d.line((13,3,3,13),fill=C[11])
    return im

def farol(lit):
    im=Image.new('RGBA',(CELL,CELL),(0,0,0,0));d=ImageDraw.Draw(im)
    d.rectangle((6,3,9,11),fill=C[0])
    d.rectangle((7,4,8,10),fill=C[9] if lit else C[6])
    d.line((6,2,9,2),fill=C[15])
    d.rectangle((5,11,10,12),fill=C[15])
    if lit:
        for dx,dy in ((-3,-3),(3,-3),(-3,3),(3,3)):d.point((7+dx,7+dy),fill=C[11])
    return im

rows=[('polvo',[polvo(i) for i in range(4)]),
      ('salpicadura',[salpicadura(i) for i in range(3)]),
      ('brillo_moneda',[brillo_moneda(i) for i in range(2)]),
      ('destello_coleccionable',[destello(i) for i in range(2)]),
      ('farol',[farol(False),farol(True)])]
cols=max(len(f) for _,f in rows)
sheet=Image.new('RGBA',(cols*CELL,len(rows)*CELL),(0,0,0,0))
for row,(name,frames) in enumerate(rows):
    for col,fr in enumerate(frames):
        sheet.alpha_composite(fr,(col*CELL,row*CELL))
sheet.save(ART/'costa_efectos.png')

pixels=list(sheet.get_flattened_data())
assert all(c[3]==0 or c in C for c in pixels),'off-palette or non-binary alpha pixel found'
assert before=={p.name:hashlib.sha256(p.read_bytes()).hexdigest() for p in protected}

preview=sheet.resize((sheet.width*8,sheet.height*8),Image.Resampling.NEAREST)
OUT.mkdir(parents=True,exist_ok=True)
preview.save(OUT/'costa_efectos_preview_x4.png')

report={'size':[sheet.width,sheet.height],'cell':CELL,
        'rows':{name:len(frames) for name,frames in rows},
        'previous_assets_unchanged':before,
        'purpose':'effect art only; particle timing/triggers wired separately in Unity',
        'status':'pending_user_approval'}
(OUT/'costa_efectos_validation.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
print(json.dumps(report,indent=2))
