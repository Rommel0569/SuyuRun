"""Level selector (320x180), ART_BIBLE.md section 4: three cards (Costa,
Sierra, Selva), each with its palette and collected-piece count. Read the
bible first.

Honest scope note, same as costa_inicio.png: Sierra and Selva have no
approved palette yet (bible section 11 orders Costa fully first). The bible
itself says those two start LOCKED ("candado") anyway, so this sidesteps the
palette problem honestly instead of faking their colors - locked cards are
silhouette + lock icon, no invented palette. Costa's card uses Costa's real
approved tones. Text (labels, piece counts) renders separately with the
pixel font, same exception as every other panel so far.
"""
from pathlib import Path
import hashlib
import json
from PIL import Image, ImageDraw

ROOT=Path(__file__).resolve().parents[1]
ART=ROOT/'Assets/SuyuRun/Art/PixelCosta'
OUT=ROOT/'Artifacts/ArtApproval'
C=[tuple(bytes.fromhex(h[1:]))+(255,) for h in json.loads((ART/'costa_palette.json').read_text(encoding='utf-8'))['colors']]
protected=[p for p in ART.iterdir() if p.is_file() and not p.name.startswith('costa_selector')]
before={p.name:hashlib.sha256(p.read_bytes()).hexdigest() for p in protected}

W,H=320,180
bg=Image.new('RGBA',(W,H),(0,0,0,0))
d=ImageDraw.Draw(bg)

CARD_W,CARD_H=92,132
GAP=8
total=CARD_W*3+GAP*2
x0=(W-total)//2
y0=(H-CARD_H)//2

def card_frame(ox,oy):
    d.rectangle((ox,oy,ox+CARD_W-1,oy+CARD_H-1),fill=C[0])
    d.rectangle((ox+2,oy+2,ox+CARD_W-3,oy+CARD_H-3),fill=C[15])
    d.rectangle((ox+4,oy+4,ox+CARD_W-5,oy+CARD_H-5),fill=C[1])
    # Adobe joint ticks along the outer border, same vocabulary as
    # costa_plataformas.png's block() - breaks up the flat double-frame.
    for jx in range(ox+8,ox+CARD_W-6,10):
        d.point((jx,oy+1),fill=C[13]);d.point((jx,oy+CARD_H-2),fill=C[13])

def slot4(ox,oy,filled):
    for i in range(4):
        sx=ox+8+i*19
        sy=oy+CARD_H-24
        d.rectangle((sx,sy,sx+14,sy+14),fill=C[0])
        d.rectangle((sx+1,sy+1,sx+13,sy+13),fill=C[15])
        d.rectangle((sx+3,sy+3,sx+11,sy+11),outline=C[1])
        if i<filled:
            d.rectangle((sx+4,sy+4,sx+10,sy+10),fill=C[9])

def costa_card(ox,oy):
    card_frame(ox,oy)
    # Small thumbnail: sunset sky + sea + dune, Costa's real approved tones.
    tx,ty,tw,th=ox+6,oy+6,CARD_W-12,64
    d.rectangle((tx,ty,tx+tw,ty+th*.5),fill=C[8])
    d.rectangle((tx,ty+th*.5,tx+tw,ty+th),fill=C[9])
    for xx in range(int(tx),int(tx+tw)):
        if (xx+int(ty+th*.5))%4==0: d.point((xx,int(ty+th*.5)),fill=C[8])
    d.ellipse((tx+tw-18,ty+8,tx+tw-4,ty+22),fill=C[11])
    d.polygon([(tx,ty+th),(tx+10,ty+th-14),(tx+22,ty+th-4),(tx+34,ty+th-12),(tx+tw*.55,ty+th)],fill=C[1])
    d.line((tx+4,ty+th-9,tx+18,ty+th-7),fill=C[2])
    d.rectangle((tx,ty+th-4,tx+tw,ty+th),fill=C[17])
    for xx in range(int(tx),int(tx+tw),3):
        d.point((xx,ty+th-1),fill=C[19])
    # Piece-count slots (4), reusing the exact HUD/resultados slot colors.
    slot4(ox,oy,2)  # sample: 2/4 collected

def locked_card(ox,oy,tint):
    card_frame(ox,oy)
    tx,ty,tw,th=ox+6,oy+6,CARD_W-12,64
    d.rectangle((tx,ty,tx+tw,ty+th),fill=tint)
    for xx in range(int(tx),int(tx+tw)):
        if (xx+ty)%5==0: d.point((xx,ty),fill=C[1])
    # Silhouette mountain hint, dark, no invented level palette - just shape,
    # with a couple of short strata dashes for texture.
    d.polygon([(tx,ty+th),(tx+16,ty+18),(tx+32,ty+th-10),(tx+50,ty+10),(tx+tw,ty+th)],fill=C[0])
    for x0,y0_,ln in [(tx+6,ty+40,10),(tx+28,ty+50,12)]:
        d.line((x0,y0_,x0+ln,y0_),fill=C[1])
    # Padlock icon, centered.
    lx,ly=ox+CARD_W//2,oy+CARD_H//2+4
    d.rectangle((lx-9,ly-2,lx+9,ly+14),fill=C[0])
    d.rectangle((lx-7,ly,lx+7,ly+12),fill=C[14])
    d.rectangle((lx-7,ly,lx+7,ly+2),fill=C[13])
    d.arc((lx-6,ly-14,lx+6,ly+2),180,360,fill=C[0],width=3)
    d.rectangle((lx-2,ly+4,lx+2,ly+9),fill=C[0])

costa_card(x0,y0)
locked_card(x0+CARD_W+GAP,y0,C[3])       # Sierra slot - cool-leaning neutral tint, no invented palette
locked_card(x0+2*(CARD_W+GAP),y0,C[2])   # Selva slot - warm-leaning neutral tint, no invented palette

# Title bar strip.
d.rectangle((0,0,W,18),fill=C[0])

bg.save(ART/'costa_selector.png')

pixels=list(bg.getdata())
assert all(c[3] in (0,255) and (c[3]==0 or c in C) for c in pixels), 'palette/alpha violation'
assert before=={p.name:hashlib.sha256(p.read_bytes()).hexdigest() for p in protected}, 'an approved asset changed'

preview=bg.convert('RGB').resize((W*4,H*4),Image.Resampling.NEAREST)
OUT.mkdir(parents=True,exist_ok=True)
preview.save(OUT/'costa_selector_preview_x4.png')

report={'size':[W,H],
        'purpose':'level selector: Costa card (real approved thumbnail + piece count) and locked Sierra/Selva cards (silhouette + padlock, no invented palette)',
        'palette_compliant':True,'binary_alpha':True,
        'previous_assets_unchanged':before,
        'status':'pending_user_approval'}
(OUT/'costa_selector_validation.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
print(json.dumps(report,indent=2))
