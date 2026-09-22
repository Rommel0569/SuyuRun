"""Costa living obstacles/enemies. Read ART_BIBLE.md first.
Hexagon-with-legs crawler, an attacking gull, and a boat with a fisherman -
each 4-6 frames in its own cell row. Palette-only, hard 1px outline, no
dithering, binary alpha. Native resolution, no upscale baked into the PNG.
"""
from pathlib import Path
import hashlib
import json
from PIL import Image, ImageDraw

ROOT=Path(__file__).resolve().parents[1]
ART=ROOT/'Assets/SuyuRun/Art/PixelCosta'
OUT=ROOT/'Artifacts/ArtApproval'
C=[tuple(bytes.fromhex(h[1:]))+(255,) for h in json.loads((ART/'costa_palette.json').read_text(encoding='utf-8'))['colors']]
protected=[p for p in ART.iterdir() if p.name.startswith(('costa_capa','costa_plataformas','costa_heroe')) or p.name=='costa_palette.json']
before={p.name:hashlib.sha256(p.read_bytes()).hexdigest() for p in protected}

CELL=32
OUTLINE=C[0]

def hexagono(lift):
    # Hexagonal shell body with four thin legs; lift alternates which leg pair is raised.
    im=Image.new('RGBA',(CELL,CELL),(0,0,0,0));d=ImageDraw.Draw(im)
    cx,cy=16,14
    pts=[(cx-8,cy),(cx-4,cy-7),(cx+4,cy-7),(cx+8,cy),(cx+4,cy+7),(cx-4,cy+7)]
    d.polygon(pts,fill=OUTLINE)
    inpts=[(cx-6,cy),(cx-3,cy-5),(cx+3,cy-5),(cx+6,cy),(cx+3,cy+5),(cx-3,cy+5)]
    d.polygon(inpts,fill=C[5])
    d.polygon([(cx-3,cy-5),(cx+3,cy-5),(cx+3,cy),(cx-3,cy)],fill=C[6])
    d.point((cx-2,cy-1),fill=C[0]);d.point((cx+2,cy-1),fill=C[0])
    for side,legx in ((-1,cx-7),(1,cx+7)):
        for pair,dy in ((0,-3),(1,3)):
            raised=(pair==lift)
            fx=legx+side*(5 if raised else 3)
            fy=cy+dy+(-2 if raised else 3)
            d.line((legx,cy+dy,fx,fy),fill=OUTLINE,width=2)
    return im

def pajaro(phase):
    # Diving/attacking gull, wings up->down->up across the cycle.
    im=Image.new('RGBA',(CELL,CELL),(0,0,0,0));d=ImageDraw.Draw(im)
    cx,cy=16,16
    d.ellipse((cx-6,cy-3,cx+6,cy+4),fill=OUTLINE)
    d.ellipse((cx-5,cy-2,cx+5,cy+3),fill=C[2])
    d.polygon([(cx+5,cy),(cx+10,cy-1),(cx+5,cy+2)],fill=C[21])
    wing_y=[-9,-4,3,8,3,-4][phase%6]
    d.polygon([(cx-1,cy-1),(cx-11,cy+wing_y),(cx-4,cy+2)],fill=OUTLINE)
    d.polygon([(cx-1,cy-1),(cx-9,cy+wing_y),(cx-4,cy+1)],fill=C[1])
    d.polygon([(cx+1,cy-1),(cx+11,cy-wing_y),(cx+4,cy+2)],fill=OUTLINE)
    d.polygon([(cx+1,cy-1),(cx+9,cy-wing_y),(cx+4,cy+1)],fill=C[1])
    return im

def bote(bob):
    # 2-cell-wide boat with a seated fisherman and a rod; bob shifts the whole craft.
    im=Image.new('RGBA',(CELL*2,CELL),(0,0,0,0));d=ImageDraw.Draw(im)
    wy=18+bob
    d.polygon([(4,wy),(10,wy+8),(50,wy+8),(56,wy)],fill=OUTLINE)
    d.polygon([(6,wy+1),(11,wy+7),(49,wy+7),(54,wy+1)],fill=C[15])
    d.line((11,wy+1,49,wy+1),fill=C[13])
    d.rectangle((26,wy-11,29,wy+1),fill=OUTLINE)
    d.rectangle((27,wy-10,28,wy+1),fill=C[15])
    d.ellipse((20,wy-16,28,wy-8),fill=OUTLINE)
    d.ellipse((21,wy-15,27,wy-9),fill=C[2])
    d.rectangle((19,wy-9,29,wy-2),fill=OUTLINE)
    d.rectangle((20,wy-8,28,wy-3),fill=C[4])
    d.line((28,wy-9,38,wy-15),fill=C[15],width=1)
    d.line((38,wy-15,38,wy-4),fill=C[13],width=1)
    return im

hex_frames=[hexagono(0),hexagono(1),hexagono(0),hexagono(1)]
bird_frames=[pajaro(i) for i in range(6)]
boat_frames=[bote(b) for b in (0,-1,0,1)]

rows=[('hexagono',hex_frames,CELL),('pajaro',bird_frames,CELL),('bote_pescador',boat_frames,CELL*2)]
cols=max(len(f) for _,f,_ in rows)
sheet_w=cols*CELL*2  # widest cell (boat) sets the grid pitch so nothing overlaps
sheet=Image.new('RGBA',(sheet_w,len(rows)*CELL),(0,0,0,0))
for row,(name,frames,w) in enumerate(rows):
    for col,fr in enumerate(frames):
        sheet.alpha_composite(fr,(col*w,row*CELL))
sheet.save(ART/'costa_obstaculos.png')

pixels=list(sheet.get_flattened_data())
assert all(c[3]==0 or c in C for c in pixels),'off-palette or non-binary alpha pixel found'
assert before=={p.name:hashlib.sha256(p.read_bytes()).hexdigest() for p in protected}

preview=sheet.resize((sheet.width*4,sheet.height*4),Image.Resampling.NEAREST)
OUT.mkdir(parents=True,exist_ok=True)
preview.save(OUT/'costa_obstaculos_preview_x4.png')

report={'size':[sheet.width,sheet.height],'rows':{'hexagono':{'frames':4,'cell':CELL},
        'pajaro':{'frames':6,'cell':CELL},'bote_pescador':{'frames':4,'cell':CELL*2}},
        'previous_assets_unchanged':before,
        'purpose':'enemy/obstacle art only; hitboxes, movement and spawn rules wired separately in Unity',
        'status':'pending_user_approval'}
(OUT/'costa_obstaculos_validation.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
print(json.dumps(report,indent=2))
