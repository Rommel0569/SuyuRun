"""Costa layer 2, independent transparent clouds + attached animated bird sprites.
Read ART_BIBLE.md before changing art. Never rewrites the approved sky.
Pillow only, native pixels, fixed palette, binary alpha, deterministic output.
"""
from pathlib import Path
import hashlib
import json
import math
from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parents[1]
ART = ROOT / 'Assets/SuyuRun/Art/PixelCosta'
PREVIEW = ROOT / 'Artifacts/ArtApproval'
COLORS = [tuple(bytes.fromhex(c[1:])) + (255,) for c in json.loads((ART/'costa_palette.json').read_text(encoding='utf-8'))['colors']]
sky_path = ART/'costa_capa1_cielo.png'
sky_hash = hashlib.sha256(sky_path.read_bytes()).hexdigest()
clouds = Image.new('RGBA', (640,180))
d = ImageDraw.Draw(clouds)

# Thin, high, wind-stretched forms leave the sun and future landmarks readable.
# Each shape has a hand-stepped contour rather than blurred ellipse stamps.
for x,y,w,h in [(36,18,91,9),(200,43,77,8),(355,11,109,10),(517,60,82,7)]:
    outline=[(x,y+h),(x+3,y+h-2),(x+13,y+h-2),(x+13,y+h-4),
             (x+26,y+h-4),(x+26,y+2),(x+37,y+2),(x+37,y),
             (x+48,y),(x+48,y+2),(x+61,y+2),(x+61,y+h-3),
             (x+w-9,y+h-3),(x+w-9,y+h-1),(x+w,y+h-1),(x+w,y+h)]
    d.polygon(outline,fill=COLORS[7])
    d.line((x+8,y+h-1,x+w-6,y+h-1),fill=COLORS[9])
    d.line((x+28,y+3,x+54,y+3),fill=COLORS[6])
    for xx in range(x+15,x+w-10):
        if xx%4==0: d.point((xx,y+h-2),fill=COLORS[8])

# 6 aligned frames; 3 authored distance sizes, always on the same 1px grid.
# Not semi-transparent fades or subpixel scaling: tiny distant silhouettes stay crisp.
birds=Image.new('RGBA',(96,48))
wing_tips=[(2,2),(2,4),(1,7),(3,11),(2,8),(2,4)]
for size in range(3):
    for f,(tip_x,tip_y) in enumerate(wing_tips):
        frame=Image.new('RGBA',(16,16));p=ImageDraw.Draw(frame)
        if size==0:
            p.line([(4,8),(10,8),(12,7),(14,7)],fill=COLORS[1],width=2)
            p.line([(6,8),(tip_x+1,tip_y),(tip_x,tip_y)],fill=COLORS[1],width=2)
            p.line([(7,8),(10,tip_y+1),(12,tip_y+1)],fill=COLORS[1],width=1)
            p.line((6,7,10,7),fill=COLORS[13],width=1)
            p.point((12,7),fill=COLORS[11])
        elif size==1:
            ty=5+(tip_y-2)//2
            p.line([(5,8),(10,8),(12,7)],fill=COLORS[2])
            p.line([(7,8),(4,ty),(3,ty)],fill=COLORS[2])
            p.line([(8,8),(10,ty),(11,ty)],fill=COLORS[2])
            p.point((10,7),fill=COLORS[14])
        else:
            ty=6 if f in (0,1,5) else 8 if f==2 else 9
            p.line([(6,8),(9,8)],fill=COLORS[3])
            p.line([(7,8),(5,ty)],fill=COLORS[3])
            p.line([(8,8),(10,ty)],fill=COLORS[3])
        birds.alpha_composite(frame,(f*16,size*16))

clouds.save(ART/'costa_capa2_nubes.png')
birds.save(ART/'costa_capa2_aves.png')

def flock_state(t, group, bird):
    phase=(t+group*6.0)%24.0
    step=(bird+1)//2
    side=1 if bird%2 else -1
    recede=group%2==1
    size=min(2,int(phase/7)) if recede else 1
    separation=12-size*3
    x=round(-62+phase*21-step*separation)
    y=round(48+group%3*15+side*step*(4-size)-phase*(.7 if recede else 0)+math.sin(t*.8+group))
    cycle=(t+bird*.13+group*.4)%3
    frame=int(cycle*8)%6 if cycle<.75 else 2
    return x,y,size,frame

sky=Image.open(sky_path).convert('RGBA').crop((160,0,480,180))
frames=[];minimum_visible=100
for index in range(160):
    t=index/10
    view=sky.copy()
    offset=round(t*(40*.05+.4))%640
    for start in (-offset,640-offset):view.alpha_composite(clouds,(start,0))
    visible=0
    for group in range(4):
        for bird in range(5):
            x,y,size,f=flock_state(t,group,bird)
            view.alpha_composite(birds.crop((f*16,size*16,f*16+16,size*16+16)),(x,y))
            visible+=(-16<x<320)
    minimum_visible=min(minimum_visible,visible)
    frames.append(view.convert('RGB').resize((1280,720),Image.Resampling.NEAREST))

# One fixed GIF palette prevents flashing/requantization and preserves approved colors.
pal=Image.new('P',(1,1));pal.putpalette([v for c in COLORS for v in c[:3]]+[0]*(768-72))
indexed=[f.quantize(palette=pal,dither=Image.Dither.NONE) for f in frames]
PREVIEW.mkdir(parents=True,exist_ok=True)
indexed[0].save(PREVIEW/'costa_capa2_movimiento.gif',save_all=True,append_images=indexed[1:],duration=100,loop=0,optimize=False,disposal=2)
frames[60].save(PREVIEW/'costa_capa2_preview_x4.png')

allowed=set(COLORS)
for image in (clouds,birds):
    assert all(c[3] in (0,255) and (c[3]==0 or c in allowed) for c in image.get_flattened_data())
assert clouds.crop((0,0,1,180)).tobytes()==clouds.crop((639,0,640,180)).tobytes()
assert hashlib.sha256(sky_path.read_bytes()).hexdigest()==sky_hash
assert minimum_visible>0
report={'cloud_size':[640,180],'bird_sheet_size':[96,48],'frames':6,'distance_variants':3,
        'palette_unchanged':True,'sky_sha256':sky_hash,'sky_unchanged':True,'binary_alpha':True,
        'matching_cloud_edges':True,'min_visible_birds_in_preview':minimum_visible,
        'preview_seconds':16,'preview_loop_note':'Preview replays at 16s; runtime wraps flocks only offscreen.',
        'status':'layer2_pending_approval'}
(PREVIEW/'costa_capa2_validation.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
print(json.dumps(report,indent=2))
