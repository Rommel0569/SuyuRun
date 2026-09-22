"""Costa protagonist sprite sheet. Read ART_BIBLE.md first.
Resolved by the user: NEVER shows arms during Costa (idle or run - he runs
the whole time anyway, so no Costa gameplay animation ever has arms). Arms
are the end-of-level Sierra unlock (separate, future delivery). 48x48 native
per frame, no upscale baked into the PNG (the x4 view is a preview-only
render, matching the parallax layers' convention). Hard 1px outline,
silhouette-forward shading (2-3 tones), no dithering, palette-only.
"""
from pathlib import Path
import hashlib
import json
from PIL import Image, ImageDraw

ROOT=Path(__file__).resolve().parents[1]
ART=ROOT/'Assets/SuyuRun/Art/PixelCosta'
OUT=ROOT/'Artifacts/ArtApproval'
C=[tuple(bytes.fromhex(h[1:]))+(255,) for h in json.loads((ART/'costa_palette.json').read_text(encoding='utf-8'))['colors']]
protected=[p for p in ART.iterdir() if p.name.startswith(('costa_capa','costa_plataformas')) or p.name=='costa_palette.json']
before={p.name:hashlib.sha256(p.read_bytes()).hexdigest() for p in protected}

FS=48
OUTLINE=C[0]; SKIN=C[14]; SKIN_SHADE=C[15]; TUNIC=C[8]; TUNIC_SHADE=C[6]; TUNIC_TRIM=C[22]
HAIR=C[1]; SASH=C[4]; GLOW=C[11]; FLASH=C[9]

def frame(*, stride=0.0, bob=0, squash=0.0, lean=0, blink=False, crouch=0, glow=0.0, flash=False, dust=False):
    im=Image.new('RGBA',(FS,FS),(0,0,0,0));d=ImageDraw.Draw(im)
    gx=24+lean
    gy=44-crouch
    sy=round(squash*3)  # >0 flattens (land), <0 stretches (jump ascend)
    head_y=14+bob-sy
    torso_top=20+bob-sy//2
    hem_y=32-crouch//2
    # Ground contact + optional sand puff (only drawn on foot-plant frames, passed in by caller).
    # Small solid dots, not a soft translucent cloud - keeps binary alpha like every other asset.
    if dust:
        for dx,dy in ((-9,-1),(-11,1),(9,-1),(11,0),(0,-2)):
            d.point((gx+dx,gy+dy),fill=C[22])
    # Legs: stride in [-1,1], 0 = together.
    back=round(stride*6); front=-back
    for lx,ext,shade in ((gx-4,back,SKIN_SHADE),(gx+4,front,SKIN)):
        lift=max(0,-ext)//2
        d.line((lx,hem_y,lx+ext//2,gy-lift),fill=OUTLINE,width=5)
        d.line((lx,hem_y,lx+ext//2,gy-lift),fill=shade,width=3)
        d.ellipse((lx+ext//2-3,gy-lift-3,lx+ext//2+3,gy-lift+3),fill=OUTLINE)
        d.ellipse((lx+ext//2-2,gy-lift-2,lx+ext//2+2,gy-lift+2),fill=shade)
    # Tunic: trapezoid torso, NO ARMS - sides stay flush, nothing extends past the hem width.
    top_w=9; bot_w=13+ (2 if stride else 0)
    d.polygon([(gx-top_w,torso_top),(gx+top_w,torso_top),(gx+bot_w,hem_y),(gx-bot_w,hem_y)],fill=OUTLINE)
    d.polygon([(gx-top_w+1,torso_top+1),(gx+top_w-1,torso_top+1),(gx+bot_w-1,hem_y-1),(gx-bot_w+1,hem_y-1)],fill=TUNIC)
    d.polygon([(gx+1,torso_top+2),(gx+top_w-1,torso_top+1),(gx+bot_w-1,hem_y-1),(gx+1,hem_y-1)],fill=TUNIC_SHADE)
    d.line((gx-bot_w+2,hem_y-2,gx+bot_w-2,hem_y-2),fill=TUNIC_TRIM,width=1)
    d.line((gx-2,torso_top+3,gx+2,torso_top+8),fill=SASH,width=2)
    # Head + hair, no neck gap.
    r=7
    d.ellipse((gx-r,head_y-r,gx+r,head_y+r),fill=OUTLINE)
    d.ellipse((gx-r+1,head_y-r+1,gx+r-1,head_y+r-1),fill=SKIN)
    d.pieslice((gx-r,head_y-r,gx+r,head_y+r),200,340,fill=HAIR)
    d.pieslice((gx-r+1,head_y-r+1,gx+r-1,head_y+r-1),205,335,fill=HAIR)
    if not blink:
        d.point((gx+3,head_y),fill=OUTLINE)
    else:
        d.line((gx+1,head_y,gx+5,head_y),fill=OUTLINE)
    # Ability glow ring (rafaga) - a halo, matching the no-arms silhouette instead of a held weapon.
    if glow>0:
        rr=r+3+round(glow*4)
        for a in range(0,360,30):
            import math
            px=gx+round(rr*math.cos(math.radians(a)));py=head_y+round(rr*.6*math.sin(math.radians(a)))
            d.point((px,py),fill=GLOW)
    if flash:
        # Fixed palette-to-palette swap (never blended RGB, stays strictly palette-legal):
        # every body tone jumps to a hot flash tone, outline/hair stay dark and readable.
        remap={SKIN:FLASH,SKIN_SHADE:C[9],TUNIC:C[11],TUNIC_SHADE:FLASH,TUNIC_TRIM:C[12],SASH:C[7]}
        px=im.load()
        for x in range(FS):
            for y in range(FS):
                c=px[x,y]
                if c[3] and c in remap:px[x,y]=remap[c]
    return im

anims={
 'idle':[frame(bob=0,blink=False),frame(bob=-1,blink=False),frame(bob=0,blink=True),frame(bob=1,blink=False)],
 'run':[frame(stride=s,bob=(-1 if abs(s)<0.5 else 0),dust=(abs(s)==1)) for s in (1,.5,0,-.5,-1,-.5,0,.5)],
 'jump':[frame(squash=-1,crouch=2),frame(squash=-2,stride=.3),frame(squash=1,stride=-.3)],
 'land':[frame(squash=2,crouch=3,dust=True),frame(squash=.3,crouch=0)],
 'slide':[frame(crouch=10,stride=.6,lean=3),frame(crouch=10,stride=-.6,lean=4,dust=True),frame(crouch=4,lean=1)],
 'rafaga':[frame(glow=.2),frame(glow=.6),frame(glow=1.0),frame(glow=.6)],
 'dano':[frame(flash=True,lean=-3),frame(flash=True,lean=3)],
}
order=['idle','run','jump','land','slide','rafaga','dano']
cols=max(len(v) for v in anims.values())
sheet=Image.new('RGBA',(cols*FS,len(order)*FS),(0,0,0,0))
for row,name in enumerate(order):
    for col,fr in enumerate(anims[name]):
        sheet.alpha_composite(fr,(col*FS,row*FS))
sheet.save(ART/'costa_heroe.png')

pixels=list(sheet.get_flattened_data())
assert all(c[3]==0 or c in C for c in pixels),'off-palette or non-binary alpha pixel found'
assert before=={p.name:hashlib.sha256(p.read_bytes()).hexdigest() for p in protected}

preview=sheet.resize((sheet.width*4,sheet.height*4),Image.Resampling.NEAREST)
OUT.mkdir(parents=True,exist_ok=True)
preview.save(OUT/'costa_heroe_preview_x4.png')

report={'size':[sheet.width,sheet.height],'frame_size':[FS,FS],
        'rows':order,'frame_counts':{k:len(v) for k,v in anims.items()},
        'no_arms_confirmed':'idle and run (and every other Costa state) never draw arms, per user decision',
        'previous_assets_unchanged':before,
        'purpose':'protagonist art only; controller/collider/state machine wiring not part of this delivery',
        'status':'pending_user_approval'}
(OUT/'costa_heroe_validation.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
print(json.dumps(report,indent=2))
