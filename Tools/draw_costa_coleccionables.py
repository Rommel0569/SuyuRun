"""Costa collectibles. Read ART_BIBLE.md first.
Four 16x16 pieces, each a 2-frame shine cycle: Moche portrait huaco, Paracas
textile swatch, Nazca polychrome ceramic, Moche gold coin (played as a game
token - see ART_APPROVALS.md ambiguity note, no historical circulation
claimed). Original stylized interpretations, not reproductions of
documented pieces. Palette-only, hard 1px outline, binary alpha.
"""
from pathlib import Path
import hashlib
import json
from PIL import Image, ImageDraw

ROOT=Path(__file__).resolve().parents[1]
ART=ROOT/'Assets/SuyuRun/Art/PixelCosta'
OUT=ROOT/'Artifacts/ArtApproval'
C=[tuple(bytes.fromhex(h[1:]))+(255,) for h in json.loads((ART/'costa_palette.json').read_text(encoding='utf-8'))['colors']]
protected=[p for p in ART.iterdir() if p.name.startswith(('costa_capa','costa_plataformas','costa_heroe','costa_obstaculos')) or p.name=='costa_palette.json']
before={p.name:hashlib.sha256(p.read_bytes()).hexdigest() for p in protected}

CELL=16
OUTLINE=C[0]

def shine(im,corner):
    # Small diagonal glint added on the "lit" frame only.
    d=ImageDraw.Draw(im)
    x,y=corner
    d.line((x,y,x+3,y),fill=C[12]);d.line((x,y,x,y+3),fill=C[12]);d.point((x+1,y+1),fill=C[12])

def huaco(lit):
    im=Image.new('RGBA',(CELL,CELL),(0,0,0,0));d=ImageDraw.Draw(im)
    d.ellipse((3,7,13,14),fill=OUTLINE);d.ellipse((4,8,12,13),fill=C[14])
    d.ellipse((5,3,11,9),fill=OUTLINE);d.ellipse((6,4,10,8),fill=C[15])
    d.point((7,6),fill=C[0]);d.point((9,6),fill=C[0]);d.line((7,7,9,7),fill=C[0])
    d.rectangle((7,2,9,3),fill=C[15])
    if lit:shine(im,(9,8))
    return im

def textil(lit):
    im=Image.new('RGBA',(CELL,CELL),(0,0,0,0));d=ImageDraw.Draw(im)
    d.rectangle((2,4,13,12),fill=OUTLINE)
    d.rectangle((3,5,12,11),fill=C[23])
    for i,x in enumerate(range(3,12,2)):
        d.line((x,5,x,11),fill=(C[19] if i%2 else C[21]))
    d.line((3,8,12,8),fill=C[1])
    if lit:shine(im,(9,5))
    return im

def ceramica(lit):
    im=Image.new('RGBA',(CELL,CELL),(0,0,0,0));d=ImageDraw.Draw(im)
    d.polygon([(4,13),(3,6),(6,3),(10,3),(13,6),(12,13)],fill=OUTLINE)
    d.polygon([(5,12),(4,6),(6,4),(10,4),(12,6),(11,12)],fill=C[9])
    d.line((4,8,12,8),fill=C[18],width=2)
    for x in (5,7,9,11):d.point((x,8),fill=C[22])
    if lit:shine(im,(9,5))
    return im

def moneda(lit):
    im=Image.new('RGBA',(CELL,CELL),(0,0,0,0));d=ImageDraw.Draw(im)
    d.ellipse((2,2,13,13),fill=OUTLINE)
    d.ellipse((3,3,12,12),fill=C[22])
    d.ellipse((5,5,10,10),fill=C[21])
    d.line((6,6,9,9),fill=C[12]);d.line((9,6,6,9),fill=C[12])
    if lit:
        d.point((3,3),fill=C[12]);d.point((11,11),fill=C[12])
    return im

rows=[('huaco',huaco),('textil',textil),('ceramica',ceramica),('moneda',moneda)]
sheet=Image.new('RGBA',(CELL*2,CELL*len(rows)),(0,0,0,0))
for row,(name,fn) in enumerate(rows):
    sheet.alpha_composite(fn(False),(0,row*CELL))
    sheet.alpha_composite(fn(True),(CELL,row*CELL))
sheet.save(ART/'costa_coleccionables.png')

pixels=list(sheet.get_flattened_data())
assert all(c[3]==0 or c in C for c in pixels),'off-palette or non-binary alpha pixel found'
assert before=={p.name:hashlib.sha256(p.read_bytes()).hexdigest() for p in protected}

preview=sheet.resize((sheet.width*8,sheet.height*8),Image.Resampling.NEAREST)
OUT.mkdir(parents=True,exist_ok=True)
preview.save(OUT/'costa_coleccionables_preview_x4.png')

report={'size':[sheet.width,sheet.height],'cell':CELL,
        'items':['huaco_retrato_moche','textil_paracas','ceramica_nazca','moneda_oro_moche'],
        'frames_per_item':2,
        'cultural_note':'original stylized interpretations, not reproductions of documented pieces; coin treated as a game token, no historical circulation claimed',
        'previous_assets_unchanged':before,
        'purpose':'collectible art only; pickup logic wired separately in Unity',
        'status':'pending_user_approval'}
(OUT/'costa_coleccionables_validation.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
print(json.dumps(report,indent=2))
