"""Original decorative adobe architecture, layer 5 only. Read ART_BIBLE.md first.
Not a reconstruction or a collider map. Native pixel clusters, no terrain dithering.
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
protected=[p for p in ART.iterdir() if p.name.startswith(('costa_capa1_','costa_capa2_','costa_capa3_','costa_capa4_')) or p.name=='costa_palette.json']
before={p.name:hashlib.sha256(p.read_bytes()).hexdigest() for p in protected}
im=Image.new('RGBA',(640,180));d=ImageDraw.Draw(im)

def huaca(x,width,top,levels):
    bottom=179;height=(bottom-top)//levels
    # Broad base and diminishing, horizontal adobe terraces.
    for tier in range(levels):
        inset=tier*8;left=x+inset;right=x+width-inset
        y=bottom-(tier+1)*height
        d.rectangle((left,y,right,bottom-tier*height),fill=C[1])
        d.rectangle((left+1,y+1,right-1,bottom-tier*height-1),fill=C[6 if tier%2 else 5])
        d.rectangle((right-7,y+2,right-1,bottom-tier*height-1),fill=C[15])
        d.line((left+1,y+1,right-1,y+1),fill=C[13])
        d.line((left+1,y+2,right-7,y+2),fill=C[7])
        # Selected adobe joints are clusters, not noisy repeated texture everywhere.
        for brick in range(left+5,right-8,7):
            d.line((brick,y+4,brick,y+height-2),fill=C[15])
            d.point((brick+2,y+4),fill=C[14])
    # Inset stairs on the facade. They are decorative, not traversable platforms.
    cx=x+width//2
    stair_top=bottom-levels*height+5
    d.polygon([(cx-3,stair_top),(cx+3,stair_top),(cx+8,178),(cx-8,178)],fill=C[3])
    for y in range(stair_top+2,179,3):
        half=3+(y-stair_top)*5//max(1,179-stair_top)
        d.line((cx-half,y,cx+half,y),fill=C[14])
    d.rectangle((x-3,178,x+width+3,179),fill=C[2])

def wall(x,width):
    # Low isolated wall sections, leaving long open views of sea between groups.
    d.polygon([(x,179),(x+2,153),(x+width-3,153),(x+width,179)],fill=C[1])
    d.rectangle((x+3,154,x+width-4,178),fill=C[14])
    d.line((x+3,154,x+width-4,154),fill=C[13])
    d.rectangle((x+6,159,x+width-7,169),fill=C[15])
    d.line((x+6,158,x+width-7,158),fill=C[13])
    d.line((x+6,170,x+width-7,170),fill=C[5])
    # Original stylized fish and wave relief, not a copied archaeological panel.
    for xx in range(x+10,x+width-15,16):
        d.polygon([(xx,163),(xx+4,161),(xx+8,163),(xx+4,165)],fill=C[13])
        d.polygon([(xx+8,163),(xx+11,161),(xx+11,165)],fill=C[13])
        d.point((xx+2,163),fill=C[3])
        d.line([(xx-1,167),(xx+3,167),(xx+3,166),(xx+7,166)],fill=C[14])
    for xx in (x+3,x+width-9):
        d.rectangle((xx,154,xx+4,178),fill=C[15])
        d.line((xx,154,xx,178),fill=C[13])
    d.line((x+8,174,x+width-11,174),fill=C[15])
    d.rectangle((x-2,178,x+width+2,179),fill=C[2])

wall(96,67)
huaca(302,86,131,4)
wall(557,61)
im.save(ART/'costa_capa5_huacas.png')
render_review(ART,OUT,'costa_capa5_huacas',im,.50,C)
pixels=list(im.get_flattened_data())
assert im.size==(640,180)
assert all(c[3] in (0,255) and (c[3]==0 or c in C) for c in pixels)
assert im.crop((0,0,1,180)).tobytes()==im.crop((639,0,640,180)).tobytes()
assert before=={p.name:hashlib.sha256(p.read_bytes()).hexdigest() for p in protected}
# Worst-case horizontal span coverage in a 320px view, including wraparound.
occupied=[any(im.getpixel((x,y))[3] for y in range(180)) for x in range(640)]
max_columns=max(sum(occupied[(start+x)%640] for x in range(320)) for start in range(640))
report={'size':[640,180],'factor':.5,'palette_compliant':True,'binary_alpha':True,
        'matching_edges':True,'previous_assets_unchanged':before,
        'opaque_fraction':round(sum(c[3]>0 for c in pixels)/len(pixels),4),
        'max_occupied_columns_per_view':max_columns,'viewport_width':320,
        'purpose':'background architecture only, no colliders',
        'preview_note':'12-second composite review; not a Unity capture',
        'status':'pending_user_approval'}
(OUT/'costa_capa5_validation.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
print(json.dumps(report,indent=2))
