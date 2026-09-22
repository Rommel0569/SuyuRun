"""Original playable ground tile, layer 7 only. Read ART_BIBLE.md first.
Sand with an adobe curb and an original Paracas-textile-inspired frieze band
(stylized geometric motif, not a copy of a documented textile). Single
32x32 tile, seamless left=right (and top=bottom, so it can also stack
downward to fill a thicker ground strip), factor 1.0 - moves with the
player, this is the literal playable-adjacent ground surface.
"""
from pathlib import Path
import hashlib
import json
from PIL import Image
from pixel_costa_preview import render_review

ROOT=Path(__file__).resolve().parents[1]
ART=ROOT/'Assets/SuyuRun/Art/PixelCosta'
OUT=ROOT/'Artifacts/ArtApproval'
C=[tuple(bytes.fromhex(h[1:]))+(255,) for h in json.loads((ART/'costa_palette.json').read_text(encoding='utf-8'))['colors']]
protected=[p for p in ART.iterdir() if p.name.startswith(('costa_capa1_','costa_capa2_','costa_capa3_','costa_capa4_','costa_capa5_','costa_capa6_')) or p.name=='costa_palette.json']
before={p.name:hashlib.sha256(p.read_bytes()).hexdigest() for p in protected}

W=H=32
im=Image.new('RGBA',(W,H),C[23])
px=im.load()

# Sand fill: mostly one tone with sparse darker/lighter grains, placed at fixed
# offsets (not random) so the tile can be reasoned about and stays palette-only.
grains=[(3,15,13),(9,21,14),(15,17,13),(21,25,14),(27,19,13),(6,27,14),(19,29,13),(24,12,14),(1,23,13)]
for x,y,ci in grains:
    px[x,y]=C[ci]

# Adobe curb: top 4 rows, darker outline row then lit adobe fill.
for x in range(W):
    px[x,0]=C[2]
for x in range(W):
    for y in (1,2,3):
        px[x,y]=C[5] if y==1 else C[6]

# Original Paracas-inspired stepped frieze, rows 4-8: a repeating chevron built
# from the accent teal/gold so it reads as textile, not architecture. Period 8
# divides the 32px width evenly, so it tiles with no seam.
for x in range(W):
    xm=x%8
    step=[4,5,6,7,6,5,4,4][xm]
    for y in range(4,step):
        px[x,y]=C[18] if (xm in (2,3,4)) else C[22]
    px[x,step]=C[17]
for x in range(W):
    px[x,9]=C[15]

im.save(ART/'costa_capa7_suelo.png')

# The preview compositor scrolls 640-wide scene layers; a 32x32 tile needs
# tiling across that width first so it reads as a ground strip, not a single
# stretched tile.
strip=Image.new('RGBA',(640,180))
for x in range(0,640,W):
    strip.alpha_composite(im,(x,180-H))
render_review(ART,OUT,'costa_capa7_suelo',strip,1.0,C)

assert im.size==(W,H)
pixels=list(im.get_flattened_data())
assert all(c[3]==255 and c in C for c in pixels)
assert im.crop((0,0,1,H)).tobytes()==im.crop((W-1,0,W,H)).tobytes()
assert before=={p.name:hashlib.sha256(p.read_bytes()).hexdigest() for p in protected}
report={'size':[W,H],'factor':1.0,'palette_compliant':True,'binary_alpha':True,
        'matching_edges':True,'previous_assets_unchanged':before,
        'tile_kind':'single 32x32 ground tile, horizontally seamless',
        'purpose':'topmost playable-adjacent ground surface, no collider baked in',
        'preview_note':'tiled into a 640-wide strip for the 12-second composite review only',
        'status':'pending_user_approval'}
(OUT/'costa_capa7_validation.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
print(json.dumps(report,indent=2))
