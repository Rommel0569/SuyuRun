"""Original playable platform tileset for Costa. Read ART_BIBLE.md first.
Stepped adobe/rock block, ramp and pillar - real jugable pieces (collision
added later in Unity, not baked into the PNG). Palette-only, hard 1px
outlines, no dithering (reserved for sky/fog).
"""
from pathlib import Path
import hashlib
import json
from PIL import Image, ImageDraw

ROOT=Path(__file__).resolve().parents[1]
ART=ROOT/'Assets/SuyuRun/Art/PixelCosta'
OUT=ROOT/'Artifacts/ArtApproval'
C=[tuple(bytes.fromhex(h[1:]))+(255,) for h in json.loads((ART/'costa_palette.json').read_text(encoding='utf-8'))['colors']]
protected=[p for p in ART.iterdir() if p.name.startswith('costa_capa') or p.name=='costa_palette.json']
before={p.name:hashlib.sha256(p.read_bytes()).hexdigest() for p in protected}

W,H=128,64
im=Image.new('RGBA',(W,H),(0,0,0,0));d=ImageDraw.Draw(im)

def block(x):
    # 48x32 stepped adobe/rock landing block: lit top-left face, shadow bottom-right.
    w,h=48,32;bottom=H
    d.rectangle((x,bottom-h,x+w,bottom),fill=C[1])
    d.rectangle((x+1,bottom-h+1,x+w-1,bottom-1),fill=C[5])
    d.polygon([(x+1,bottom-h+1),(x+w-1,bottom-h+1),(x+w-1,bottom-1),(x+1,bottom-1)],fill=C[5])
    # Shadow wedge, lower-right.
    d.polygon([(x+w-14,bottom-1),(x+w-1,bottom-h+8),(x+w-1,bottom-1)],fill=C[2])
    # Two illuminated terraces stepping down toward the right.
    d.rectangle((x+1,bottom-h+1,x+w-16,bottom-h+11),fill=C[6])
    d.line((x+1,bottom-h+11,x+w-16,bottom-h+11),fill=C[13])
    d.rectangle((x+w-16,bottom-h+11,x+w-1,bottom-1),fill=C[5])
    # Adobe joint marks, sparse.
    for jx in range(x+5,x+w-18,8):
        d.line((jx,bottom-h+3,jx,bottom-h+9),fill=C[15])
    for jx in range(x+w-13,x+w-3,8):
        d.line((jx,bottom-h+13,jx,bottom-1),fill=C[15])
    d.rectangle((x,bottom-1,x+w,bottom),fill=C[13])

def ramp(x):
    # 32x32 ascending ramp, left low to right high.
    w,h=32,32;bottom=H
    d.polygon([(x,bottom),(x,bottom-6),(x+w,bottom-h),(x+w,bottom)],fill=C[1])
    d.polygon([(x+1,bottom-1),(x+1,bottom-6),(x+w-1,bottom-h+1),(x+w-1,bottom-1)],fill=C[5])
    d.line((x+1,bottom-6,x+w-1,bottom-h+1),fill=C[13],width=1)
    for i in range(1,6):
        xx=x+1+i*(w-2)//6
        yy=bottom-6-i*(h-8)//6
        d.line((xx,yy,xx,bottom-1),fill=C[6] if i%2 else C[5])
    d.rectangle((x,bottom-1,x+w,bottom),fill=C[13])

def pillar(x):
    # 16x48 column: base, shaft, simple capital.
    w,h=16,48;bottom=H
    d.rectangle((x,bottom-h,x+w,bottom),fill=C[1])
    d.rectangle((x+1,bottom-h+5,x+w-1,bottom-4),fill=C[5])
    d.rectangle((x+2,bottom-h+5,x+w-2,bottom-4),fill=C[6])
    d.line((x+3,bottom-h+7,x+3,bottom-6),fill=C[13])
    d.line((x+w-4,bottom-h+7,x+w-4,bottom-6),fill=C[2])
    d.rectangle((x,bottom-h,x+w,bottom-h+5),fill=C[15])
    d.rectangle((x,bottom-4,x+w,bottom),fill=C[15])
    d.line((x,bottom-h+5,x+w,bottom-h+5),fill=C[13])
    d.line((x,bottom-4,x+w,bottom-4),fill=C[2])

block(0)
ramp(56)
pillar(96)
im.save(ART/'costa_plataformas.png')

pixels=list(im.get_flattened_data())
assert all(c[3] in (0,255) and (c[3]==0 or c in C) for c in pixels)
assert before=={p.name:hashlib.sha256(p.read_bytes()).hexdigest() for p in protected}

# Static context preview: the three pieces sitting on the approved ground/sea composite,
# not a scrolling layer (platforms are placed objects, not a parallax background).
sky=Image.open(ART/'costa_capa1_cielo.png').convert('RGBA').crop((160,0,480,180))
sea=Image.open(ART/'costa_capa4_mar.png').convert('RGBA').crop((0,0,640,180))
ground_tile=Image.open(ART/'costa_capa7_suelo.png').convert('RGBA')
scene=sky.copy()
scene.alpha_composite(sea,(-80,0))
for gx in range(0,320,32):
    scene.alpha_composite(ground_tile,(gx,180-32))
scene.alpha_composite(im.crop((0,32,48,64)),(40,148))
scene.alpha_composite(im.crop((56,32,88,64)),(120,148))
scene.alpha_composite(im.crop((96,16,112,64)),(190,132))
preview=scene.convert('RGB').resize((1280,720),Image.Resampling.NEAREST)
OUT.mkdir(parents=True,exist_ok=True)
preview.save(OUT/'costa_plataformas_preview_x4.png')

report={'size':[W,H],'pieces':{'block':[0,32,48,32],'ramp':[56,32,32,32],'pillar':[96,16,16,48]},
        'palette_compliant':True,'binary_alpha':True,
        'previous_assets_unchanged':before,
        'purpose':'playable platform art; collision added separately in Unity, not baked into PNG',
        'preview_note':'static context composite on approved sky/sea/ground; not a Unity capture',
        'status':'pending_user_approval'}
(OUT/'costa_plataformas_validation.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
print(json.dumps(report,indent=2))
