"""Costa level-end landmark: a trapezoidal stone gateway ("puerta dimensional")
whose silhouette nods to Inca trapezoidal doorways, foreshadowing the Sierra
power the hero receives at the end of Costa (sections 5/15 of ART_BIBLE.md).
Read ART_BIBLE.md first. Palette-only (Costa's 24 approved colors, no new
hues - a level's assets share its one palette), hard 1px outlines, no
dithering (reserved for sky/fog), binary alpha. Two animation frames: the
portal's inner glow slowly swirls/pulses, everything else static.
"""
from pathlib import Path
import hashlib
import json
from PIL import Image, ImageDraw

ROOT=Path(__file__).resolve().parents[1]
ART=ROOT/'Assets/SuyuRun/Art/PixelCosta'
OUT=ROOT/'Artifacts/ArtApproval'
C=[tuple(bytes.fromhex(h[1:]))+(255,) for h in json.loads((ART/'costa_palette.json').read_text(encoding='utf-8'))['colors']]
protected=[p for p in ART.iterdir() if p.is_file() and not p.name.startswith('costa_puerta_dimensional')]
before={p.name:hashlib.sha256(p.read_bytes()).hexdigest() for p in protected}

FW,FH=64,96
sheet=Image.new('RGBA',(FW*2,FH),(0,0,0,0))

BOTTOM=94          # ground contact row (leaves 2px clearance like other objects)
TOP=10
BASE_HALF=28       # outer half-width at the ground
TOP_HALF=18        # outer half-width at the lintel - narrower top = trapezoidal Inca doorway
WALL=7             # stone thickness

def lerp(a,b,t): return a+(b-a)*t

def trapezoid(d,top,bottom,top_half,bottom_half,cx,fill):
    d.polygon([(cx-bottom_half,bottom),(cx+bottom_half,bottom),
               (cx+top_half,top),(cx-top_half,top)],fill=fill)

def draw_frame(frame_index):
    d=ImageDraw.Draw(sheet)
    ox=frame_index*FW
    cx=ox+FW//2

    # Outer stone mass, dark base tone.
    trapezoid(d,TOP,BOTTOM,TOP_HALF,BASE_HALF,cx,C[1])
    # Lit face (upper-left half reads brighter, matching the huaca/platform
    # lighting convention already established - light from upper-left).
    d.polygon([(cx-BASE_HALF,BOTTOM),(cx,BOTTOM),(cx,TOP),(cx-TOP_HALF,TOP)],fill=C[5])
    d.polygon([(cx,BOTTOM),(cx+BASE_HALF,BOTTOM),(cx+TOP_HALF,TOP),(cx,TOP)],fill=C[2])

    # Stepped pediment on top, echoing the huacas' terraces (capa 5).
    step_w=[TOP_HALF*2+10,TOP_HALF*2-2,TOP_HALF*2-14]
    step_y=TOP
    for i,w in enumerate(step_w):
        h=4
        d.rectangle((cx-w//2,step_y-h,cx+w//2,step_y),fill=C[6] if i%2==0 else C[5])
        d.line((cx-w//2,step_y-h,cx+w//2,step_y-h),fill=C[13])
        step_y-=h

    # Chevron frieze near the base, echoing the Paracas textile motif on the
    # playable ground tile (capa 7) - keeps the gate visibly "Costa-made".
    frieze_y=BOTTOM-14
    period=8
    x=cx-BASE_HALF+4
    while x<cx+BASE_HALF-4:
        d.line((x,frieze_y+4,x+period//2,frieze_y-2),fill=C[15])
        d.line((x+period//2,frieze_y-2,x+period,frieze_y+4),fill=C[15])
        x+=period

    # Inner opening (the actual doorway) - inset trapezoid, wall thickness WALL.
    inner_bottom=BOTTOM
    inner_top=TOP+4
    inner_base_half=BASE_HALF-WALL
    inner_top_half=TOP_HALF-WALL+2
    trapezoid(d,inner_top,inner_bottom,inner_top_half,inner_base_half,cx,C[0])

    # Dimensional glow inside the opening: concentric flat-color bands (no
    # gradients allowed) from a cool teal rim to a warm gold-cream core,
    # matching the violet/turquoise "impulse" motif already used in-game for
    # the air-charge orbs, but built from Costa's own palette only.
    bands=[
        (0.92,C[17]),(0.74,C[18]),(0.56,C[19]),
        (0.40,C[9]),(0.26,C[10]),(0.14,C[11]),
    ]
    swirl=frame_index  # 0 or 1: alternates which band pair leads, for the pulse/swirl
    for i,(t,col) in enumerate(bands):
        bh=lerp(inner_top,inner_bottom,1-t*0.94)
        half=lerp(inner_top_half,inner_base_half,1-t*0.94)*t
        if (i+swirl)%2==0:
            half*=1.06
        trapezoid(d,bh,inner_bottom-2,max(half*.55,2),half,cx,col)
    # Bright core, sits highest so it always reads through.
    core_y=lerp(inner_top,inner_bottom,0.62 if swirl==0 else 0.58)
    d.ellipse((cx-5,core_y-5,cx+5,core_y+5),fill=C[12])

    # Absorb cue: short streaks along the rim angled INWARD toward the core
    # (not just static motes) so the portal visibly pulls things toward its
    # center - it can "swallow" the hero at the end of the level, not just glow.
    rim=[(-15,-34),(13,-30),(-16,-6),(15,-4),(-13,16),(12,18)] if swirl==0 else \
        [(-13,-32),(15,-28),(-14,-8),(17,-2),(-15,14),(10,20)]
    for rx,ry in rim:
        px,py=cx+rx,int(core_y)+ry
        # unit step toward the core, drawn as a short 3px streak (motion trail)
        dx=(0 if px==cx else (1 if px<cx else -1))
        dy=(0 if py==core_y else (1 if py<core_y else -1))
        for s in range(3):
            d.point((px+dx*s,py+dy*s),fill=C[11] if s<2 else C[9])

    # A few drifting motes inside the glow - shift position between frames
    # for the swirl read, same "sparse points" treatment as costa_efectos.
    motes=[(-9,-30),(7,-22),(-4,-10),(10,-2),(-11,6)] if swirl==0 else \
          [(-7,-28),(9,-18),(-2,-6),(8,4),(-10,10)]
    for mx,my in motes:
        d.point((cx+mx,int(core_y)+my),fill=C[12])

    # Doorway frame outline, hard 1px.
    d.polygon([(cx-BASE_HALF,BOTTOM),(cx+BASE_HALF,BOTTOM),(cx+TOP_HALF,TOP),(cx-TOP_HALF,TOP)],outline=C[0],width=1)
    trapezoid_outline=[(cx-inner_base_half,inner_bottom),(cx+inner_base_half,inner_bottom),
                        (cx+inner_top_half,inner_top),(cx-inner_top_half,inner_top)]
    d.polygon(trapezoid_outline,outline=C[0],width=1)

for f in range(2):
    draw_frame(f)

sheet.save(ART/'costa_puerta_dimensional.png')

pixels=list(sheet.getdata())
assert all(c[3] in (0,255) and (c[3]==0 or c in C) for c in pixels), 'palette/alpha violation'
assert before=={p.name:hashlib.sha256(p.read_bytes()).hexdigest() for p in protected}, 'an approved asset changed'

# Static context preview on the approved sky/sea/ground, like the platforms preview.
sky=Image.open(ART/'costa_capa1_cielo.png').convert('RGBA').crop((160,0,480,180))
sea=Image.open(ART/'costa_capa4_mar.png').convert('RGBA').crop((0,0,640,180))
ground_tile=Image.open(ART/'costa_capa7_suelo.png').convert('RGBA')
frames_preview=[]
for f in range(2):
    scene=sky.copy()
    scene.alpha_composite(sea,(-80,0))
    for gx in range(0,320,32):
        scene.alpha_composite(ground_tile,(gx,180-32))
    # Door's ground-contact row (BOTTOM, within its own frame) must land on
    # the TOP of the sand strip (180-32=148), not the bottom of the canvas -
    # that mismatch was making the gate look sunk in from below.
    scene.alpha_composite(sheet.crop((f*FW,0,f*FW+FW,FH)),(128,148-BOTTOM))
    frames_preview.append(scene.convert('RGB').resize((1280,720),Image.Resampling.NEAREST))
OUT.mkdir(parents=True,exist_ok=True)
frames_preview[0].save(OUT/'costa_puerta_dimensional_preview_x4.png')
frames_preview[0].save(OUT/'costa_puerta_dimensional.gif',save_all=True,
                        append_images=[frames_preview[1]],duration=500,loop=0)

report={'size_per_frame':[FW,FH],'frames':2,
        'purpose':'level-end landmark; trapezoidal Inca-doorway silhouette foreshadowing the Sierra power gained at the end of Costa (ART_BIBLE.md sections 5/15)',
        'palette_compliant':True,'binary_alpha':True,
        'previous_assets_unchanged':before,
        'preview_note':'static context composite on approved sky/sea/ground; not a Unity capture',
        'status':'pending_user_approval'}
(OUT/'costa_puerta_dimensional_validation.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
print(json.dumps(report,indent=2))
