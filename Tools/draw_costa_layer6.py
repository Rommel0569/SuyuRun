"""Original midground silhouettes, layer 6 only. Read ART_BIBLE.md first.
Dry algarrobo trees, rocks and resting sea lions. No dithering (reserved for
sky/fog), hard 1px outlines, palette-only colors.
"""
from pathlib import Path
import hashlib
import json
from PIL import Image, ImageDraw
from pixel_costa_preview import render_review

ROOT=Path(__file__).resolve().parents[1]
ART=ROOT/'Assets/SuyuRun/Art/PixelCosta'
OUT=ROOT/'Artifacts/ArtApproval'
C=[tuple(bytes.fromhex(h[1:]))+(255,) for h in json.loads((ART/'costa_palette.json').read_text(encoding='utf-8'))['colors']]
protected=[p for p in ART.iterdir() if p.name.startswith(('costa_capa1_','costa_capa2_','costa_capa3_','costa_capa4_','costa_capa5_')) or p.name=='costa_palette.json']
before={p.name:hashlib.sha256(p.read_bytes()).hexdigest() for p in protected}
im=Image.new('RGBA',(640,180));d=ImageDraw.Draw(im)

def algarrobo(x,base_y,height,lean):
    # Gnarled dry trunk with a handful of bare angular branches, not a full canopy.
    trunk_top=base_y-height
    d.line((x,base_y,x+lean,trunk_top),fill=C[1],width=3)
    d.line((x,base_y,x+lean,trunk_top),fill=C[15],width=1)
    branches=[(0.55,-1,10,-6),(0.7,1,9,-5),(0.85,-1,7,4),(1.0,1,6,3)]
    for t,side,length,drop in branches:
        bx=x+round(lean*t);by=trunk_top+round(height*(1-t))
        ex=bx+side*length;ey=by+drop
        d.line((bx,by,ex,ey),fill=C[1],width=2)
        d.line((bx,by,ex,ey),fill=C[15],width=1)
        # A few short twigs off the branch tip, never a filled canopy blob.
        for twig in (-3,2):
            d.line((ex,ey,ex+twig,ey-3),fill=C[1],width=1)
    d.ellipse((x-5,base_y-2,x+lean+5,base_y+2),fill=C[2])

def rock_cluster(x,base_y,sizes):
    cx=x
    for w,h in sizes:
        d.polygon([(cx,base_y),(cx+2,base_y-h),(cx+w-3,base_y-h),(cx+w,base_y)],fill=C[2])
        d.polygon([(cx+2,base_y-1),(cx+3,base_y-h+2),(cx+w-4,base_y-h+2),(cx+w-2,base_y-1)],fill=C[14])
        d.line((cx+2,base_y-h+2,cx+w-4,base_y-h+2),fill=C[13])
        cx+=w+4
    return cx

def sea_lion(x,y):
    # Resting silhouette: rounded body, small raised head, one visible flipper.
    d.ellipse((x,y,x+22,y+11),fill=C[1])
    d.ellipse((x+1,y+1,x+21,y+9),fill=C[2])
    d.ellipse((x+16,y-4,x+24,y+4),fill=C[1])
    d.ellipse((x+17,y-3,x+23,y+3),fill=C[2])
    d.polygon([(x+3,y+9),(x+8,y+9),(x+5,y+14)],fill=C[1])
    d.line((x+16,y-1,x+19,y-2),fill=C[15],width=1)

algarrobo(70,178,46,10)
rock_cluster(150,179,[(20,11),(14,7)])
sea_lion(163,166)
algarrobo(430,178,38,-8)
rock_cluster(500,179,[(16,9),(24,13),(15,8)])
sea_lion(522,163)

im.save(ART/'costa_capa6_algarrobos.png')
render_review(ART,OUT,'costa_capa6_algarrobos',im,.75,C)
pixels=list(im.get_flattened_data())
assert im.size==(640,180)
assert all(c[3] in (0,255) and (c[3]==0 or c in C) for c in pixels)
assert im.crop((0,0,1,180)).tobytes()==im.crop((639,0,640,180)).tobytes()
assert before=={p.name:hashlib.sha256(p.read_bytes()).hexdigest() for p in protected}
occupied=[any(im.getpixel((x,y))[3] for y in range(180)) for x in range(640)]
max_columns=max(sum(occupied[(start+x)%640] for x in range(320)) for start in range(640))
report={'size':[640,180],'factor':.75,'palette_compliant':True,'binary_alpha':True,
        'matching_edges':True,'previous_assets_unchanged':before,
        'opaque_fraction':round(sum(c[3]>0 for c in pixels)/len(pixels),4),
        'max_occupied_columns_per_view':max_columns,'viewport_width':320,
        'purpose':'midground silhouettes only, no colliders',
        'preview_note':'12-second composite review; not a Unity capture',
        'status':'pending_user_approval'}
(OUT/'costa_capa6_validation.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
print(json.dumps(report,indent=2))
