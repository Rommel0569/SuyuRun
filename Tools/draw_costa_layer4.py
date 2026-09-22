"""Layer 4: one PNG, four independent 640x180 animation frames stacked vertically.
Pillow native-pixel workflow authorized by ART_BIBLE.md. Read that file before edits.
Never modifies approved assets; the GIF is a composite preview, not production art.
"""
from pathlib import Path
import hashlib
import json
import math
from PIL import Image, ImageDraw

ROOT=Path(__file__).resolve().parents[1]
ART=ROOT/'Assets/SuyuRun/Art/PixelCosta'
OUT=ROOT/'Artifacts/ArtApproval'
palette=json.loads((ART/'costa_palette.json').read_text(encoding='utf-8'))
C=[tuple(bytes.fromhex(h[1:]))+(255,) for h in palette['colors']]
protected=[ART/n for n in ('costa_capa1_cielo.png','costa_capa2_nubes.png','costa_capa2_aves.png','costa_capa3_paracas.png','costa_palette.json')]
before={p.name:hashlib.sha256(p.read_bytes()).hexdigest() for p in protected}
water=[]
for frame in range(4):
    im=Image.new('RGBA',(640,180));d=ImageDraw.Draw(im)
    d.rectangle((0,146,639,154),fill=C[18])
    d.rectangle((0,155,639,166),fill=C[17])
    d.rectangle((0,167,639,179),fill=C[16])
    # Garua: binary-alpha pixels only along a few short horizon wisps.
    # Dithering is confined to this fog, never used as noisy water texture.
    for start,length in [(25,85),(179,97),(335,74),(502,94)]:
        for x in range(start,start+length):
            for y in range(143,148):
                if (x+2*y)%5 < (y-142)//2:
                    d.point((x,y),fill=C[20] if y>145 else C[19])
    # Sparse stretched wave crests; four distinct poses return gently to frame zero.
    travel=(0,1,2,1)[frame];lift=(0,-1,0,1)[frame]
    for row,y in enumerate((151,158,165,173)):
        for wave in range(9):
            x=15+wave*69+(row%2)*21+travel*(1+row%2)
            length=10+(wave*7+row*3)%16
            if x+length>=635:continue
            yy=y+(lift if (wave+row)%2==0 else -lift)
            tone=19 if row<2 else 18
            d.line([(x,yy),(x+4,yy),(x+4,yy-1),(x+length-4,yy-1),(x+length-4,yy),(x+length,yy)],fill=C[tone])
            if (wave+row)%3==0:
                d.line((x+6,yy-1,x+length-5,yy-1),fill=C[20])
            if row>1:
                d.line((x+3,yy+2,x+length-2,yy+2),fill=C[17])
    # Original small reed craft with an upswept bow; no sail or large foreground prop.
    # Bobbing is authored in the four frames, never subpixel scaling/rotation.
    bx,by=394,152+(0,-1,0,1)[frame]
    d.line((bx-5,by+4,bx+23,by+4),fill=C[18])
    d.polygon([(bx-2,by-1),(bx+4,by+1),(bx+15,by),(bx+23,by-7),
               (bx+21,by),(bx+16,by+3),(bx+4,by+3)],fill=C[1])
    d.line([(bx,by),(bx+5,by+1),(bx+15,by),(bx+22,by-5)],fill=C[14],width=1)
    d.line((bx+4,by+2,bx+15,by+2),fill=C[15])
    for x in (bx+5,bx+9,bx+13):d.point((x,by+1),fill=C[13])
    # Modest seated silhouette and diagonal paddle, legible at the distant scale.
    d.rectangle((bx+8,by-9,bx+10,by-7),fill=C[1])
    d.line([(bx+9,by-6),(bx+9,by-2),(bx+13,by)],fill=C[1],width=2)
    d.line([(bx+9,by-5),(bx+13,by-3)],fill=C[14])
    d.line((bx+12,by-6,bx+19,by+3),fill=C[15])
    d.line((bx+18,by+2,bx+20,by+4),fill=C[14])
    water.append(im)

sheet=Image.new('RGBA',(640,720))
for f,im in enumerate(water):sheet.alpha_composite(im,(0,f*180))
sheet.save(ART/'costa_capa4_mar.png')

sky=Image.open(ART/'costa_capa1_cielo.png').convert('RGBA').crop((160,0,480,180))
clouds=Image.open(ART/'costa_capa2_nubes.png').convert('RGBA')
birds=Image.open(ART/'costa_capa2_aves.png').convert('RGBA')
land=Image.open(ART/'costa_capa3_paracas.png').convert('RGBA')
frames=[]
for index in range(120):
    t=index/10;view=sky.copy()
    shift=round(t*(40*.05+.4))%640
    for x in (-shift,640-shift):view.alpha_composite(clouds,(x,0))
    for group in range(4):
        for bird in range(5):
            phase=(t+group*6)%24;step=(bird+1)//2;side=1 if bird%2 else -1
            recede=group%2==1;size=min(2,int(phase/7)) if recede else 1
            x=round(-62+phase*21-step*(12-size*3))
            y=round(48+group%3*15+side*step*(4-size)-phase*(.7 if recede else 0)+math.sin(t*.8+group))
            cycle=(t+bird*.13+group*.4)%3;f=int(cycle*8)%6 if cycle<.75 else 2
            view.alpha_composite(birds.crop((f*16,size*16,f*16+16,size*16+16)),(x,y))
    for layer,factor in ((land,.15),(water[int(t*4)%4],.30)):
        shift=(160+round(t*40*factor))%640
        for x in (-shift,640-shift):view.alpha_composite(layer,(x,0))
    frames.append(view.convert('RGB').resize((1280,720),Image.Resampling.NEAREST))
OUT.mkdir(parents=True,exist_ok=True)
frames[0].save(OUT/'costa_capa4_preview_x4.png')
pal=Image.new('P',(1,1));pal.putpalette([v for c in C for v in c[:3]]+[0]*(768-72))
indexed=[f.quantize(palette=pal,dither=Image.Dither.NONE) for f in frames]
indexed[0].save(OUT/'costa_capa4_mar.gif',save_all=True,append_images=indexed[1:],duration=100,loop=0,optimize=False,disposal=2)
for im in water:
    assert all(c[3] in (0,255) and (c[3]==0 or c in C) for c in im.get_flattened_data())
    assert im.crop((0,0,1,180)).tobytes()==im.crop((639,0,640,180)).tobytes()
assert len({hashlib.sha256(im.tobytes()).hexdigest() for im in water})==4
assert before=={p.name:hashlib.sha256(p.read_bytes()).hexdigest() for p in protected}
report={'asset':'costa_capa4_mar.png','sheet_size':[640,720],'frame_size':[640,180],
        'frame_count':4,'frame_order':'top to bottom','fps':4,'parallax_factor':.30,
        'seamless_edges_each_frame':True,'binary_alpha':True,'palette_compliant':True,
        'previous_assets_unchanged':before,'waterline_y':146,'dither_only_in_fog':True,
        'preview_note':'12-second review restarts; not a runtime capture.',
        'status':'pending_user_approval'}
(OUT/'costa_capa4_validation.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
print(json.dumps(report,indent=2))
