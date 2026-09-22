"""Museum screen art (ART_BIBLE.md section 10). Read the bible first.
Interior adobe wall background, a vitrine/alcove frame (one per piece - the
game already reuses the approved costa_coleccionables.png sprites inside
each, tinted dark for undiscovered per RunnerMuseum.cs's existing
`discovered[]` logic - no separate silhouette art needed), and a detail
panel frame matching costa_resultados.png's exact palette choices. Text
renders separately with the pixel font, same exception as everywhere else.
"""
from pathlib import Path
import hashlib
import json
from PIL import Image, ImageDraw

ROOT=Path(__file__).resolve().parents[1]
ART=ROOT/'Assets/SuyuRun/Art/PixelCosta'
OUT=ROOT/'Artifacts/ArtApproval'
C=[tuple(bytes.fromhex(h[1:]))+(255,) for h in json.loads((ART/'costa_palette.json').read_text(encoding='utf-8'))['colors']]
protected=[p for p in ART.iterdir() if p.is_file() and not p.name.startswith('costa_museo')]
before={p.name:hashlib.sha256(p.read_bytes()).hexdigest() for p in protected}

# --- Wall tile (32x32, seamless) - adobe brick coursing, interior museum wall. ---
WT=32
wall=Image.new('RGBA',(WT,WT),(0,0,0,0))
wd=ImageDraw.Draw(wall)
wd.rectangle((0,0,WT-1,WT-1),fill=C[14])
# Alternate brick shading for real depth, not a flat fill between the joints.
for row,y in enumerate(range(0,WT,8)):
    offset=0 if row%2==0 else 4
    cols=list(range(-offset,WT,10))
    for i,x in enumerate(cols):
        shade=C[13] if (row+i)%2==0 else C[14]
        wd.rectangle((x+1,y+1,x+9,y+7),fill=shade)
    wd.line((0,y,WT,y),fill=C[15])
    for x in cols:
        wd.line((x,y,x,y+8),fill=C[15])
wd.point((3,3),fill=C[13]);wd.point((18,11),fill=C[13]);wd.point((9,19),fill=C[13]);wd.point((25,27),fill=C[13])
wd.point((6,5),fill=C[15]);wd.point((22,21),fill=C[15]);wd.point((14,29),fill=C[15])
wall.save(ART/'costa_museo_pared.png')

# --- Vitrine alcove frame (48x48) - the piece sprite sits centered inside this in Unity. ---
V=48
vit=Image.new('RGBA',(V,V),(0,0,0,0))
vd=ImageDraw.Draw(vit)
vd.rectangle((0,0,V-1,V-1),fill=C[0])
vd.rectangle((2,2,V-3,V-3),fill=C[1])
vd.rectangle((4,4,V-5,V-5),fill=C[15])
for jx in range(6,V-5,7):
    vd.point((jx,3),fill=C[13]);vd.point((jx,V-4),fill=C[13])
# Small arch top, echoing the huaca terraces' step motif.
vd.rectangle((10,4,V-11,8),fill=C[2])
vd.rectangle((14,4,V-15,6),fill=C[6])
vd.line((14,7,V-15,7),fill=C[13])
# Pedestal shelf at the base, with adobe joint ticks.
vd.rectangle((6,V-12,V-7,V-9),fill=C[6])
vd.line((6,V-9,V-7,V-9),fill=C[13])
for jx in range(9,V-9,6):
    vd.point((jx,V-11),fill=C[2])
vit.save(ART/'costa_museo_vitrina.png')

# --- Detail panel frame (280x300) - matches costa_resultados.png's exact colors. ---
P_W,P_H=280,300
panel=Image.new('RGBA',(P_W,P_H),(0,0,0,0))
pd=ImageDraw.Draw(panel)
pd.rectangle((0,0,P_W-1,P_H-1),fill=C[0])
pd.rectangle((2,2,P_W-3,P_H-3),fill=C[15])
pd.rectangle((4,4,P_W-5,P_H-5),fill=C[6])
pd.rectangle((4,4,P_W-5,26),fill=C[2])          # title zone (blank)
pd.line((4,26,P_W-5,26),fill=C[22])
pd.rectangle((4,34,P_W-5,58),fill=C[3])          # place/culture zone (blank)
pd.rectangle((4,66,P_W-5,P_H-40),fill=C[6])      # description zone (blank)
pd.rectangle((4,P_H-32,4+140,P_H-8),fill=C[1])   # source-link button (blank)
pd.rectangle((5,P_H-31,4+139,P_H-9),fill=C[18])
for jx in range(10,P_W-8,14):
    pd.point((jx,1),fill=C[13]);pd.point((jx,P_H-2),fill=C[13])
panel.save(ART/'costa_museo_panel.png')

all_pixels=list(wall.getdata())+list(vit.getdata())+list(panel.getdata())
assert all(c[3] in (0,255) and (c[3]==0 or c in C) for c in all_pixels), 'palette/alpha violation'
assert before=={p.name:hashlib.sha256(p.read_bytes()).hexdigest() for p in protected}, 'an approved asset changed'

OUT.mkdir(parents=True,exist_ok=True)
wall.resize((WT*8,WT*8),Image.Resampling.NEAREST).save(OUT/'costa_museo_pared_preview_x8.png')
vit.resize((V*6,V*6),Image.Resampling.NEAREST).save(OUT/'costa_museo_vitrina_preview_x6.png')
panel.resize((P_W*3,P_H*3),Image.Resampling.NEAREST).save(OUT/'costa_museo_panel_preview_x3.png')

# Context mock: tiled wall + 4 vitrines + panel, roughly like the real layout.
mock=Image.new('RGB',(1280,720),C[14][:3])
for ty in range(0,720,WT*4):
    for tx in range(0,1280,WT*4):
        mock.paste(wall.resize((WT*4,WT*4),Image.Resampling.NEAREST),(tx,ty))
mockrgba=mock.convert('RGBA')
for i in range(4):
    mockrgba.alpha_composite(vit.resize((V*4,V*4),Image.Resampling.NEAREST),(40+i*304,570))
mockrgba.alpha_composite(panel.resize((P_W*2,P_H*2),Image.Resampling.NEAREST),(660,150))
mockrgba.convert('RGB').save(OUT/'costa_museo_context_preview.png')

report={'wall_size':[WT,WT],'vitrina_size':[V,V],'panel_size':[P_W,P_H],
        'purpose':'museum interior wall (seamless), vitrine/alcove frame (piece sprites already approved, reused, tinted for undiscovered in code), detail panel frame matching costa_resultados.png colors',
        'palette_compliant':True,'binary_alpha':True,
        'previous_assets_unchanged':before,
        'status':'pending_user_approval'}
(OUT/'costa_museo_validation.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
print(json.dumps(report,indent=2))
