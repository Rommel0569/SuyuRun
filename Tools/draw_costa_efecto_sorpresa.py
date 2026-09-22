"""Bare "?!" surprise mark that pops above the hero's head right after the
arms-reveal animation, before the dimensional door absorbs him - no speech
bubble/frame, just the two characters growing from small to big (classic
pop/surprise read). Read ART_BIBLE.md first. Kept as its OWN file (not added
into costa_efectos.png, which is already approved and must not be rewritten).
Palette-only, hard 1px outline, binary alpha, 3 frames (small -> big
overshoot -> settled).
"""
from pathlib import Path
import hashlib
import json
from PIL import Image, ImageDraw

ROOT=Path(__file__).resolve().parents[1]
ART=ROOT/'Assets/SuyuRun/Art/PixelCosta'
OUT=ROOT/'Artifacts/ArtApproval'
C=[tuple(bytes.fromhex(h[1:]))+(255,) for h in json.loads((ART/'costa_palette.json').read_text(encoding='utf-8'))['colors']]
protected=[p for p in ART.iterdir() if p.is_file() and not p.name.startswith('costa_efecto_sorpresa')]
before={p.name:hashlib.sha256(p.read_bytes()).hexdigest() for p in protected}

FW,FH=32,32

# Fixed 5x7 glyph bitmaps (1=ink). Drawn as blocks scaled by `px`, with a hard
# 1px dark outline (drawn as an offset dark copy behind the bright fill) so
# it reads over any background, since there's no bubble panel anymore.
GLYPH_Q=[
 "01110",
 "10001",
 "00001",
 "00010",
 "00100",
 "00000",
 "00100",
]
GLYPH_BANG=[
 "010",
 "010",
 "010",
 "010",
 "010",
 "000",
 "010",
]

def glyph_mask(glyph,px):
    w,h=len(glyph[0])*px,len(glyph)*px
    m=Image.new('L',(w,h),0)
    md=ImageDraw.Draw(m)
    for gy,row in enumerate(glyph):
        for gx,c in enumerate(row):
            if c=='1':
                md.rectangle((gx*px,gy*px,gx*px+px-1,gy*px+px-1),fill=255)
    return m

def paste_glyph(sheet,ox,oy,glyph,px,fill):
    mask=glyph_mask(glyph,px)
    outline=mask.filter(ImageFilter.MaxFilter(3))
    solid=Image.new('RGBA',mask.size,C[0])
    sheet.paste(solid,(ox-1,oy-1),outline)
    solid2=Image.new('RGBA',mask.size,fill)
    sheet.paste(solid2,(ox,oy),mask)

from PIL import ImageFilter

def draw_frame(ox,px,fill):
    q=glyph_mask(GLYPH_Q,px)
    b=glyph_mask(GLYPH_BANG,px)
    gap=px
    total_w=q.width+gap+b.width
    total_h=max(q.height,b.height)
    start_x=ox+(FW-total_w)//2
    start_y=(FH-total_h)//2
    paste_glyph(sheet,start_x,start_y,GLYPH_Q,px,fill)
    paste_glyph(sheet,start_x+q.width+gap,start_y,GLYPH_BANG,px,fill)

sheet=Image.new('RGBA',(FW*3,FH),(0,0,0,0))
draw_frame(0,1,C[12])       # tiny, just appearing
draw_frame(FW,4,C[9])       # pops big and warm (overshoot)
draw_frame(FW*2,3,C[12])    # settles at a slightly smaller, steady size

sheet.save(ART/'costa_efecto_sorpresa.png')

pixels=list(sheet.getdata())
assert all(c[3] in (0,255) and (c[3]==0 or c in C) for c in pixels), 'palette/alpha violation'
assert before=={p.name:hashlib.sha256(p.read_bytes()).hexdigest() for p in protected}, 'an approved asset changed'

preview=sheet.resize((FW*3*6,FH*6),Image.Resampling.NEAREST)
OUT.mkdir(parents=True,exist_ok=True)
preview.save(OUT/'costa_efecto_sorpresa_preview_x6.png')
frames=[sheet.crop((i*FW,0,i*FW+FW,FH)).resize((FW*6,FH*6),Image.Resampling.NEAREST) for i in range(3)]
frames[0].save(OUT/'costa_efecto_sorpresa.gif',save_all=True,append_images=frames[1:]+[frames[2]]*2,duration=90,loop=0)

report={'size_per_frame':[FW,FH],'frames':3,
        'purpose':'bare "?!" surprise mark (no bubble/frame) that pops small-to-big above the hero right after the arms-reveal animation, before the portal absorbs him',
        'palette_compliant':True,'binary_alpha':True,
        'previous_assets_unchanged':before,
        'status':'pending_user_approval'}
(OUT/'costa_efecto_sorpresa_validation.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
print(json.dumps(report,indent=2))
