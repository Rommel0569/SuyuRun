"""Shop screen (ART_BIBLE.md section 10): coins spend on upgrades (more
RÁFAGA, coin magnet). Read the bible first.

Honest scope note: the bible's upgrade list includes "más vida", but Costa
has no vida stat (instant-death runner, confirmed earlier - vida only
starts in Selva). So the third slot is a locked placeholder, not a fake
vida upgrade, consistent with the HUD/selector decisions already made.

Panel/joint vocabulary matches costa_resultados.png and costa_museo_panel.png
exactly (same C[0]/C[15]/C[1] frame, same adobe joint-tick texture, same
restrained dithering-only-for-sky rule) per the "more authenticity" pass.
Text and numbers render separately with the pixel font, same exception as
every other panel so far.
"""
from pathlib import Path
import hashlib
import json
from PIL import Image, ImageDraw

ROOT=Path(__file__).resolve().parents[1]
ART=ROOT/'Assets/SuyuRun/Art/PixelCosta'
OUT=ROOT/'Artifacts/ArtApproval'
C=[tuple(bytes.fromhex(h[1:]))+(255,) for h in json.loads((ART/'costa_palette.json').read_text(encoding='utf-8'))['colors']]
protected=[p for p in ART.iterdir() if p.is_file() and not p.name.startswith('costa_tienda')]
before={p.name:hashlib.sha256(p.read_bytes()).hexdigest() for p in protected}

W,H=280,180
panel=Image.new('RGBA',(W,H),(0,0,0,0))
d=ImageDraw.Draw(panel)

d.rectangle((0,0,W-1,H-1),fill=C[0])
d.rectangle((2,2,W-3,H-3),fill=C[15])
d.rectangle((4,4,W-5,H-5),fill=C[1])
d.rectangle((4,4,W-5,24),fill=C[2])           # title zone (blank)
d.line((4,24,W-5,24),fill=C[22])
for jx in range(10,W-8,14):
    d.point((jx,1),fill=C[13]);d.point((jx,H-2),fill=C[13])

CARD_W,CARD_H=78,120
gap=6
x0=10
y0=34

def upgrade_icon_rafaga(cx,cy):
    # Segmented burst ring, echoing the HUD's RÁFAGA bar notches.
    d.ellipse((cx-10,cy-10,cx+10,cy+10),fill=C[0])
    d.ellipse((cx-8,cy-8,cx+8,cy+8),fill=C[19])
    for a in range(0,360,45):
        import math
        x=cx+round(9*math.cos(math.radians(a)));y=cy+round(9*math.sin(math.radians(a)))
        d.point((x,y),fill=C[12])
    d.ellipse((cx-4,cy-4,cx+4,cy+4),fill=C[12])

def upgrade_icon_iman(cx,cy):
    # Horseshoe magnet, gold poles.
    d.arc((cx-9,cy-9,cx+9,cy+9),20,160,fill=C[0],width=6)
    d.arc((cx-7,cy-7,cx+7,cy+7),20,160,fill=C[9],width=3)
    d.rectangle((cx-10,cy-2,cx-5,cy+9),fill=C[0])
    d.rectangle((cx+5,cy-2,cx+10,cy+9),fill=C[0])
    d.rectangle((cx-9,cy-1,cx-6,cy+7),fill=C[6])
    d.rectangle((cx+6,cy-1,cx+9,cy+7),fill=C[19])

def upgrade_icon_locked(cx,cy):
    d.rectangle((cx-9,cy-2,cx+9,cy+9),fill=C[0])
    d.rectangle((cx-7,cy,cx+7,cy+7),fill=C[15])
    d.arc((cx-6,cy-11,cx+6,cy+3),180,360,fill=C[0],width=3)

def card(ox,oy,icon_fn,pips,locked=False):
    d.rectangle((ox,oy,ox+CARD_W-1,oy+CARD_H-1),fill=C[0])
    d.rectangle((ox+2,oy+2,ox+CARD_W-3,oy+CARD_H-3),fill=C[15] if not locked else C[1])
    d.rectangle((ox+4,oy+4,ox+CARD_W-5,oy+CARD_H-5),fill=C[6] if not locked else C[2])
    for jx in range(ox+7,ox+CARD_W-5,9):
        d.point((jx,oy+3),fill=C[13]);d.point((jx,oy+CARD_H-4),fill=C[13])
    icon_fn(ox+CARD_W//2,oy+28)
    # Level pips (filled = owned tier), 3 max.
    for i in range(3):
        px_=ox+CARD_W//2-14+i*12
        py_=oy+52
        d.rectangle((px_,py_,px_+8,py_+8),fill=C[0])
        d.rectangle((px_+1,py_+1,px_+7,py_+7),fill=C[15])
        if i<pips and not locked:
            d.rectangle((px_+2,py_+2,px_+6,py_+6),fill=C[9])
    # Price tag zone (blank, filled with real number via font).
    d.rectangle((ox+8,oy+CARD_H-24,ox+CARD_W-9,oy+CARD_H-8),fill=C[1])
    d.rectangle((ox+9,oy+CARD_H-23,ox+CARD_W-10,oy+CARD_H-9),fill=C[18])

card(x0,y0,upgrade_icon_rafaga,2)
card(x0+CARD_W+gap,y0,upgrade_icon_iman,1)
card(x0+2*(CARD_W+gap),y0,upgrade_icon_locked,0,locked=True)

panel.save(ART/'costa_tienda.png')

pixels=list(panel.getdata())
assert all(c[3] in (0,255) and (c[3]==0 or c in C) for c in pixels), 'palette/alpha violation'
assert before=={p.name:hashlib.sha256(p.read_bytes()).hexdigest() for p in protected}, 'an approved asset changed'

preview=panel.convert('RGB').resize((W*4,H*4),Image.Resampling.NEAREST)
OUT.mkdir(parents=True,exist_ok=True)
preview.save(OUT/'costa_tienda_preview_x4.png')

report={'size':[W,H],
        'purpose':'shop panel: RÁFAGA upgrade, coin-magnet upgrade, and a locked third slot (no vida stat exists in Costa, so no fake vida upgrade - matches the HUD/selector decision)',
        'palette_compliant':True,'binary_alpha':True,
        'previous_assets_unchanged':before,
        'status':'pending_user_approval'}
(OUT/'costa_tienda_validation.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
print(json.dumps(report,indent=2))
