"""Costa HUD pieces: read ART_BIBLE.md first. Section 4: pixel font (rendered
separately as a Unity font asset, not baked here - same text-exception
already used for costa_resultados.png), 9-slice adobe/terracota panels, no
translucent boxes. Matches costa_resultados.png's exact frame colors (C[0]
outer / C[15] inner for slots) so the running HUD and the results screen read
as the same family. Pieces only - text, numbers and the moneda icon (already
approved in costa_coleccionables.png) are placed separately in Unity.
"""
from pathlib import Path
import hashlib
import json
from PIL import Image, ImageDraw

ROOT=Path(__file__).resolve().parents[1]
ART=ROOT/'Assets/SuyuRun/Art/PixelCosta'
OUT=ROOT/'Artifacts/ArtApproval'
C=[tuple(bytes.fromhex(h[1:]))+(255,) for h in json.loads((ART/'costa_palette.json').read_text(encoding='utf-8'))['colors']]
protected=[p for p in ART.iterdir() if p.is_file() and not p.name.startswith('costa_hud')]
before={p.name:hashlib.sha256(p.read_bytes()).hexdigest() for p in protected}

sheet=Image.new('RGBA',(256,32),(0,0,0,0))
d=ImageDraw.Draw(sheet)

# panel_h: 9-slice adobe/terracota bar, 32x20 at (0,0). 6px end caps, a 20px
# stretchable middle column - the same lit-top/shadow-bottom scheme as
# costa_plataformas.png's block(), just flattened into a thin bar.
def panel_h(ox):
    w,h=32,20
    d.rectangle((ox,0,ox+w-1,h-1),fill=C[0])
    d.rectangle((ox+1,1,ox+w-2,h-2),fill=C[2])
    d.rectangle((ox+1,1,ox+w-2,3),fill=C[6])
    d.line((ox+1,3,ox+w-2,3),fill=C[13])
    for cap in (ox+5,ox+w-6):
        d.line((cap,1,cap,h-2),fill=C[0])
    d.rectangle((ox,h-2,ox+w-1,h-1),fill=C[13])
    # Adobe joint ticks in the stretchable middle zone (6..26), same
    # vocabulary as costa_plataformas.png's block() - spaced to tile cleanly
    # when Unity repeats this 20px middle column.
    for jx in (ox+11,ox+21):
        d.point((jx,7),fill=C[15]);d.point((jx,12),fill=C[15])
panel_h(0)

# bar_rafaga: frame with 6 notch segments, 64x10 at (40,0). Fill is a
# separate flat teal strip (bar_rafaga_fill) the same height, stretched by
# Unity to the current energy fraction.
def bar_frame(ox,w,h,notches):
    d.rectangle((ox,0,ox+w-1,h-1),fill=C[0])
    d.rectangle((ox+1,1,ox+w-2,h-2),fill=C[1])
    if notches:
        step=(w-2)//notches
        for i in range(1,notches):
            d.line((ox+1+i*step,1,ox+1+i*step,h-2),fill=C[0])
    # Corner rivets, breaks up the flat frame fill.
    for rx,ry in [(ox+2,1),(ox+w-3,1),(ox+2,h-2),(ox+w-3,h-2)]:
        d.point((rx,ry),fill=C[13])
bar_frame(40,64,10,6)
d.rectangle((40+42,0,40+42+11,9),fill=C[19])  # bar_rafaga_fill sample swatch (11x10), tiled/scaled by Unity

# bar_progress: wider frame, no notches, gold fill swatch - the bottom
# level-progress bar.
bar_frame(120,64,6,0)
d.rectangle((120+42,0,120+42+11,5),fill=C[9])  # bar_progress_fill sample swatch

# piece_slot: empty adobe frame for a patrimony piece, 16x16 at (200,0) -
# identical colors to costa_resultados.png's slots (C[0] outer / C[15]
# inner) so the two screens visibly match.
def piece_slot(ox):
    d.rectangle((ox,0,ox+15,15),fill=C[0])
    d.rectangle((ox+1,1,ox+14,14),fill=C[15])
    d.rectangle((ox+3,3,ox+12,12),outline=C[1])
    d.point((ox+3,3),fill=C[13]);d.point((ox+12,3),fill=C[13])
    d.point((ox+3,12),fill=C[13]);d.point((ox+12,12),fill=C[13])
piece_slot(200)

# icon_puntos: small sun-burst, 12x12 at (220,0) - points icon (moneda reuses
# the already-approved costa_coleccionables.png icon, not redrawn here).
def icon_puntos(ox):
    cx,cy=ox+6,6
    d.point((cx,cy-5),fill=C[9]);d.point((cx,cy+5),fill=C[9])
    d.point((cx-5,cy),fill=C[9]);d.point((cx+5,cy),fill=C[9])
    d.point((cx-4,cy-4),fill=C[9]);d.point((cx+4,cy-4),fill=C[9])
    d.point((cx-4,cy+4),fill=C[9]);d.point((cx+4,cy+4),fill=C[9])
    d.ellipse((cx-3,cy-3,cx+3,cy+3),fill=C[0])
    d.ellipse((cx-2,cy-2,cx+2,cy+2),fill=C[10])
icon_puntos(220)

sheet.save(ART/'costa_hud.png')

pixels=list(sheet.getdata())
assert all(c[3] in (0,255) and (c[3]==0 or c in C) for c in pixels), 'palette/alpha violation'
assert before=={p.name:hashlib.sha256(p.read_bytes()).hexdigest() for p in protected}, 'an approved asset changed'

preview=sheet.resize((sheet.width*8,sheet.height*8),Image.Resampling.NEAREST)
OUT.mkdir(parents=True,exist_ok=True)
preview.save(OUT/'costa_hud_preview_x8.png')

# Composite mock of the running HUD, on the approved sky/sea, to see it in context.
sky=Image.open(ART/'costa_capa1_cielo.png').convert('RGBA').crop((160,0,480,180))
sea=Image.open(ART/'costa_capa4_mar.png').convert('RGBA').crop((0,0,640,180))
mock=sky.copy()
mock.alpha_composite(sea,(-80,0))
top=sheet.crop((0,0,32,20)).resize((300,20*(300//32) if False else 20),Image.Resampling.NEAREST)
# Stretch panel_h across the top by tiling its middle column, 9-slice style.
def stretch_panel(src_ox, target_w, target_h):
    left=sheet.crop((src_ox,0,src_ox+6,target_h))
    right=sheet.crop((src_ox+26,0,src_ox+32,target_h))
    mid=sheet.crop((src_ox+6,0,src_ox+26,target_h))
    out=Image.new('RGBA',(target_w,target_h),(0,0,0,0))
    out.alpha_composite(left,(0,0))
    x=6
    while x<target_w-6:
        out.alpha_composite(mid,(x,0))
        x+=20
    out.alpha_composite(right,(target_w-6,0))
    return out
top_bar=stretch_panel(0,300,20)
mock.alpha_composite(top_bar,(10,6))
mock.alpha_composite(sheet.crop((200,0,216,16)),(14,9))
for i in range(1,4):
    mock.alpha_composite(sheet.crop((200,0,216,16)),(14+i*18,9))
mock.alpha_composite(sheet.crop((220,0,232,12)),(240,9))
bot_bar=stretch_panel(120,180,6)
mock.alpha_composite(bot_bar,(70,168))
preview_mock=mock.convert('RGB').resize((1280,720),Image.Resampling.NEAREST)
preview_mock.save(OUT/'costa_hud_context_preview_x4.png')

report={'sheet_size':[256,32],
        'pieces':{'panel_h':[0,0,32,20],'bar_rafaga_frame':[40,0,64,10],'bar_rafaga_fill_swatch':[82,0,11,10],
                  'bar_progress_frame':[120,0,64,6],'bar_progress_fill_swatch':[162,0,11,6],
                  'piece_slot':[200,0,16,16],'icon_puntos':[220,0,12,12]},
        'purpose':'running HUD pieces (bars/frames/slot), matching costa_resultados.png colors; text and the moneda icon come from elsewhere',
        'palette_compliant':True,'binary_alpha':True,
        'previous_assets_unchanged':before,
        'status':'pending_user_approval'}
(OUT/'costa_hud_validation.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
print(json.dumps(report,indent=2))
