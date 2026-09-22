"""Independent Paracas silhouettes, native 640x180 pixels, approved Costa palette.
Read ART_BIBLE.md before any edits. Does not rewrite layers 1/2 or their generator.
Geoglyph is an original small artistic interpretation, not an archaeological diagram.
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
protected=[ART/n for n in ('costa_capa1_cielo.png','costa_capa2_nubes.png','costa_capa2_aves.png','costa_palette.json')]
before={p.name:hashlib.sha256(p.read_bytes()).hexdigest() for p in protected}
im=Image.new('RGBA',(640,180));d=ImageDraw.Draw(im)

# Broad desert headlands, deliberately separated by open water/sky windows.
# Flat pixel clusters only: no texture dithering outside sky/fog.
d.polygon([(84,146),(102,140),(124,132),(143,116),(169,106),(188,103),
           (207,96),(226,96),(241,101),(259,112),(272,115),(289,134),
           (311,143),(328,146)],fill=C[4])
d.polygon([(84,146),(131,136),(161,124),(190,118),(207,108),
           (226,103),(212,123),(198,136),(183,146)],fill=C[3])
d.polygon([(239,108),(252,115),(259,130),(289,142),(328,146),
           (255,146),(242,133)],fill=C[3])
d.polygon([(166,109),(188,105),(207,98),(226,98),(234,102),
           (208,101),(190,108),(177,113)],fill=C[5])
d.polygon([(115,141),(145,135),(155,129),(169,127),(158,136),
           (146,140),(166,143),(181,146),(110,146)],fill=C[2])

# Long stepped ridge at the other end; no landmarks repeated inside the same tile.
d.polygon([(444,146),(466,142),(478,136),(491,135),(508,122),(527,116),
           (549,115),(565,120),(580,132),(598,136),(614,143),(628,146)],fill=C[4])
d.polygon([(473,142),(505,134),(527,119),(537,119),(524,133),
           (520,141),(554,146),(455,146)],fill=C[3])
d.polygon([(551,118),(565,123),(574,134),(602,141),(623,146),
           (578,146),(563,139)],fill=C[3])
d.line([(493,134),(509,123),(528,117),(547,117)],fill=C[5],width=1)

# Candelabro: three branches etched into the broad face, muted sand edge.
# The tiny interpretation carries no claims about its date or original meaning.
grooves=[[(223,108),(223,139)],[(220,112),(223,108),(226,112)],
         [(212,115),(212,126),(223,132),(235,126),(235,115)],
         [(210,117),(212,114),(215,117)],[(232,117),(235,114),(237,117)],
         [(220,139),(226,139)]]
for line in grooves:
    d.line([(x+1,y+1) for x,y in line],fill=C[3],width=2)
    d.line(line,fill=C[14],width=1)

im.save(ART/'costa_capa3_paracas.png')

sky=Image.open(ART/'costa_capa1_cielo.png').convert('RGBA').crop((160,0,480,180))
clouds=Image.open(ART/'costa_capa2_nubes.png').convert('RGBA')
birds=Image.open(ART/'costa_capa2_aves.png').convert('RGBA')
frames=[]
for frame in range(120):
    t=frame/10
    view=sky.copy()
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
    shift=(160+round(t*40*.15))%640
    for x in (-shift,640-shift):view.alpha_composite(im,(x,0))
    frames.append(view.convert('RGB').resize((1280,720),Image.Resampling.NEAREST))
OUT.mkdir(parents=True,exist_ok=True)
frames[0].save(OUT/'costa_capa3_preview_x4.png')
pal=Image.new('P',(1,1));pal.putpalette([v for c in C for v in c[:3]]+[0]*(768-72))
indexed=[f.quantize(palette=pal,dither=Image.Dither.NONE) for f in frames]
indexed[0].save(OUT/'costa_capa3_parallax.gif',save_all=True,append_images=indexed[1:],duration=100,loop=0,optimize=False,disposal=2)
pixels=list(im.get_flattened_data())
assert all(c[3] in (0,255) and (c[3]==0 or c in C) for c in pixels)
assert im.crop((0,0,1,180)).tobytes()==im.crop((639,0,640,180)).tobytes()
assert before=={p.name:hashlib.sha256(p.read_bytes()).hexdigest() for p in protected}
report={'size':[640,180],'mode':'RGBA','factor':.15,'used_opaque_colors':len({c for c in pixels if c[3]}),
        'alpha_binary':True,'matching_edges':True,'previous_assets_unchanged':before,
        'transparent_fraction':round(sum(c[3]==0 for c in pixels)/len(pixels),4),
        'waterline_y':146,'preview_note':'12-second demonstration restarts; sea layer not produced yet.',
        'status':'pending_user_approval',
        'cultural_reference':'https://www.peru.travel/es/atractivos/paracas-e-islas-ballestas'}
(OUT/'costa_capa3_validation.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
print(json.dumps(report,indent=2))
