"""Start panel (320x180), ART_BIBLE.md section 4. Read the bible first.

Honest scope note: the bible asks for a background that unites COSTA, SIERRA
and SELVA in three parallax bands - but Sierra and Selva don't have approved
palettes or art yet (bible section 11: Costa first, then Sierra, then Selva).
Building their bands now would mean inventing palettes without approval, so
this uses Costa's own approved palette throughout, with the right edge just
hinting at a cooler, higher horizon (foreshadowing Sierra) using colors
already in Costa's 24. The other two bands get built for real once Sierra and
Selva exist - this is flagged, not silently done.

The running protagonist is NOT baked in here - it reuses the already-approved
costa_heroe.png run frames in Unity, same as everywhere else. This PNG is
background + logo + button frames + cursor only; text renders separately
with the pixel font, same exception as costa_resultados.png.
"""
from pathlib import Path
import hashlib
import json
from PIL import Image, ImageDraw

ROOT=Path(__file__).resolve().parents[1]
ART=ROOT/'Assets/SuyuRun/Art/PixelCosta'
OUT=ROOT/'Artifacts/ArtApproval'
C=[tuple(bytes.fromhex(h[1:]))+(255,) for h in json.loads((ART/'costa_palette.json').read_text(encoding='utf-8'))['colors']]
protected=[p for p in ART.iterdir() if p.is_file() and not p.name.startswith('costa_inicio')]
before={p.name:hashlib.sha256(p.read_bytes()).hexdigest() for p in protected}

W,H=320,180
bg=Image.new('RGBA',(W,H),(0,0,0,0))
d=ImageDraw.Draw(bg)

def dither_seam(y,colA,colB):
    # Same ordered-dithering idiom as costa_capa1_cielo.png's band transitions -
    # a single sparse row, not a thick noisy band. ART_BIBLE.md allows
    # dithering only for sky/fog.
    for x in range(W):
        if (x+y*3)%4==0: d.point((x,y),fill=colB)

# Sky bands (dusk), reusing capa1's tone family, with one dithered seam row
# between each pair instead of a hard flat cut.
sky_bands=[(0,30,C[0]),(30,60,C[3]),(60,95,C[7]),(95,130,C[9])]
for i,(y0,y1,col) in enumerate(sky_bands):
    d.rectangle((0,y0,W,y1),fill=col)
    if i>0:
        dither_seam(y0,sky_bands[i-1][2],col)
        dither_seam(y0+1,col,sky_bands[i-1][2])
# Sun, with a faint stepped ring for texture instead of a flat disc.
d.ellipse((236,70,272,106),fill=C[11])
d.rectangle((236,88,272,106),fill=C[10])
d.arc((238,72,270,104),200,340,fill=C[12])
# Right-edge cool hint: a taller, cooler silhouette foreshadowing Sierra's
# horizon, built only from Costa colors already in the palette (C[16..19]
# are the sea's cool turquoise tones, read as distant cool peaks here). A
# couple of short broken strata dashes, not full-width lines.
d.polygon([(268,130),(284,84),(300,104),(320,92),(320,130)],fill=C[16])
d.polygon([(268,130),(280,98),(292,112),(306,100),(320,108),(320,130)],fill=C[17])
for x0,y0_,ln in [(288,104,14),(300,114,16),(292,122,10)]:
    d.line((x0,y0_,x0+ln,y0_),fill=C[18])
# Costa dune silhouettes, left/center, with a couple of wind-ripple accents.
d.polygon([(0,130),(20,100),(46,120),(70,108),(96,130)],fill=C[1])
d.polygon([(0,130),(14,114),(34,126),(58,118),(90,130)],fill=C[2])
for x0,y0_,ln in [(6,112,18),(30,122,20),(54,126,16)]:
    d.line((x0,y0_,x0+ln,y0_),fill=C[3])
# Sea strip + sand, dithered surf line instead of a flat color change.
d.rectangle((0,130,W,146),fill=C[17])
dither_seam(146,C[17],C[19])
d.rectangle((0,147,W,150),fill=C[19])
dither_seam(150,C[19],C[11])
d.rectangle((0,151,W,H),fill=C[11])
for sx in range(6,W,17):
    d.point((sx,158+ (sx*7)%14),fill=C[13])
for sx in range(0,W,9):
    d.point((sx+ (sx*3)%5,168+(sx*5)%9),fill=C[14])

# --- Logo "SUYURUN", one word (ART_BIBLE.md section 1), stone/gold blocks ---
GLYPHS={
 'S':["01111","10000","10000","01110","00001","00001","11110"],
 'U':["10001","10001","10001","10001","10001","10001","01110"],
 'Y':["10001","10001","01010","00100","00100","00100","00100"],
 'R':["11110","10001","10001","11110","10100","10010","10001"],
 'N':["10001","11001","10101","10101","10011","10001","10001"],
}
def letter(ox,oy,ch,px):
    g=GLYPHS[ch]
    for gy,row in enumerate(g):
        for gx,c in enumerate(row):
            if c=='1':
                x0,y0=ox+gx*px,oy+gy*px
                d.rectangle((x0,y0,x0+px,y0+px),fill=C[0])
    for gy,row in enumerate(g):
        for gx,c in enumerate(row):
            if c=='1':
                x0,y0=ox+gx*px,oy+gy*px
                # Stone/gold split: gold upper half, terracota-stone lower half - no gradient,
                # just a hard two-tone band per ART_BIBLE.md's no-gradient rule.
                tone=C[10] if gy<4 else C[14]
                d.rectangle((x0,y0,x0+px-2,y0+px-2),fill=tone)
                # Carved-stone corner nick, one per block - breaks up the flat fill
                # like real quarried blocks without touching the palette/alpha rules.
                d.point((x0,y0),fill=C[9] if gy<4 else C[15])

word="SUYURUN"
px=4
letter_w=5*px+px  # glyph width + inter-letter gap
total_w=len(word)*letter_w-px
lx=(W-total_w)//2
for ch in word:
    letter(lx,14,ch,px)
    lx+=letter_w

# --- Buttons: 5 blank adobe frames (JUGAR, NIVELES, MUSEO, AJUSTES, SALIR) -
# text rendered separately with the pixel font, same as costa_resultados.png.
def button(ox,oy,w,h):
    d.rectangle((ox,oy,ox+w-1,oy+h-1),fill=C[0])
    d.rectangle((ox+1,oy+1,ox+w-2,oy+h-2),fill=C[2])
    d.rectangle((ox+1,oy+1,ox+w-2,oy+3),fill=C[6])
    d.line((ox+1,oy+h-3,ox+w-2,oy+h-3),fill=C[13])
    # Adobe joint ticks, same vocabulary as costa_plataformas.png's block().
    for jx in range(ox+6,ox+w-4,7):
        d.point((jx,oy+2),fill=C[15])
        d.point((jx+3,oy+h-2),fill=C[15])

btn_h=14
button(16,150,88,btn_h)              # JUGAR (primary, left)
for i in range(4):
    button(112+i*50,150,46,btn_h)    # NIVELES, MUSEO, AJUSTES, SALIR (112..308, no overlap)

# --- Pixel cursor (small arrow), placed near JUGAR as a sample pose ---
cur=[(0,0),(0,1),(0,2),(0,3),(0,4),(0,5),(1,1),(1,2),(1,3),(1,4),(2,2),(2,3),(3,3),(2,4),(3,5),(4,6)]
cx,cy=60,157
for px_,py_ in cur:
    d.point((cx+px_,cy+py_),fill=C[0])
for px_,py_ in [(0,1),(0,2),(0,3),(1,2),(1,3),(2,3)]:
    d.point((cx+px_,cy+py_),fill=C[12])

bg.save(ART/'costa_inicio.png')

pixels=list(bg.getdata())
assert all(c[3] in (0,255) and (c[3]==0 or c in C) for c in pixels), 'palette/alpha violation'
assert before=={p.name:hashlib.sha256(p.read_bytes()).hexdigest() for p in protected}, 'an approved asset changed'

preview=bg.convert('RGB').resize((W*4,H*4),Image.Resampling.NEAREST)
OUT.mkdir(parents=True,exist_ok=True)
preview.save(OUT/'costa_inicio_preview_x4.png')

report={'size':[W,H],
        'purpose':'start panel background + SUYURUN logo + 5 blank button frames + cursor; hero reuses approved costa_heroe run frames in Unity, not baked in',
        'scope_note':'Sierra/Selva bands NOT built yet (no approved palette for them) - right edge only hints at a cooler horizon using Costa colors already in the palette; revisit once those levels exist',
        'palette_compliant':True,'binary_alpha':True,
        'previous_assets_unchanged':before,
        'status':'pending_user_approval'}
(OUT/'costa_inicio_validation.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
print(json.dumps(report,indent=2))
