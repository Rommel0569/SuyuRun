"""Costa background dance vignette - a "zambo chinchano" dancing festejo. Read ART_BIBLE.md
first. Replaces the vector-mesh dancer per the user's request that it be pixel art while
keeping (and improving on) the original's lively movement - not a static swap. Palette-only,
hard 1px outline, binary alpha, no dithering. Native resolution (24x40 per frame), no upscale
baked in. Original stylized silhouette, an interpretive tribute to festejo footwork
(zapateo), not a documented individual or a specific choreography.
"""
from pathlib import Path
import hashlib
import json
from PIL import Image, ImageDraw

ROOT=Path(__file__).resolve().parents[1]
ART=ROOT/'Assets/SuyuRun/Art/PixelCosta'
OUT=ROOT/'Artifacts/ArtApproval'
C=[tuple(bytes.fromhex(h[1:]))+(255,) for h in json.loads((ART/'costa_palette.json').read_text(encoding='utf-8'))['colors']]
protected=[p for p in ART.iterdir() if p.name.startswith(('costa_capa','costa_plataformas','costa_heroe','costa_obstaculos','costa_coleccionables','costa_efectos','costa_resultados')) or p.name=='costa_palette.json']
before={p.name:hashlib.sha256(p.read_bytes()).hexdigest() for p in protected}

W,H=24,40
OUTLINE=C[0]; SKIN=C[14]; SKIN_SHADE=C[15]; SHIRT=C[12]; SASH=C[7]; HAIR=C[1]; TROUSER=C[2]

def limb(d,a,b,width,fill):
    d.line((a[0],a[1],b[0],b[1]),fill=OUTLINE,width=width+2)
    d.line((a[0],a[1],b[0],b[1]),fill=fill,width=width)

def frame(lean,kick,stomp):
    # lean: -1/0/1 which side the weight/energy leans to. kick: 0/1 raises the OPPOSITE leg in a
    # zapateo-style bent kick. stomp: 0/1 squashes the torso slightly for a footwork accent.
    im=Image.new('RGBA',(W,H),(0,0,0,0));d=ImageDraw.Draw(im)
    sq=1 if stomp else 0
    cx=12+lean*2
    hip_y=24+sq
    support_x=cx-lean*3
    d.ellipse((support_x-3,hip_y+12,support_x+3,hip_y+16),fill=(0,0,0,60)) if False else None
    # Support leg: straight to the ground, slight bend on a stomp frame.
    knee_s=(support_x-lean,hip_y+7+sq)
    foot_s=(support_x-lean*2,38)
    limb(d,(cx-lean,hip_y),knee_s,3,SKIN)
    limb(d,knee_s,foot_s,3,SKIN_SHADE)
    d.ellipse((foot_s[0]-2,foot_s[1]-1,foot_s[0]+2,foot_s[1]+2),fill=OUTLINE)
    if kick:
        # Kicking leg: bent and lifted out to the side opposite the lean - the zapateo accent.
        side=-lean if lean!=0 else 1
        knee_k=(cx+side*6,hip_y+2)
        foot_k=(cx+side*10,hip_y-3)
        limb(d,(cx+side,hip_y),knee_k,3,SKIN)
        limb(d,knee_k,foot_k,3,SKIN_SHADE)
        d.ellipse((foot_k[0]-2,foot_k[1]-2,foot_k[0]+2,foot_k[1]+1),fill=OUTLINE)
    else:
        side=lean if lean!=0 else -1
        knee_k=(cx+side*2,hip_y+7+sq)
        foot_k=(cx+side*3,38)
        limb(d,(cx+side,hip_y),knee_k,3,SKIN)
        limb(d,knee_k,foot_k,3,SKIN_SHADE)
        d.ellipse((foot_k[0]-2,foot_k[1]-1,foot_k[0]+2,foot_k[1]+2),fill=OUTLINE)
    # Torso (shirt) with the sash, squashed a touch on stomp frames for footwork punch.
    shoulder_y=12+sq*2
    d.rectangle((cx-5,shoulder_y,cx+5,hip_y),fill=OUTLINE)
    d.rectangle((cx-4,shoulder_y+1,cx+4,hip_y-1),fill=SHIRT)
    d.rectangle((cx-4,hip_y-5,cx+4,hip_y-3),fill=SASH)
    d.rectangle((cx-6,hip_y-2,cx+6,hip_y+1),fill=TROUSER)
    # Arms: one raised triumphantly toward the lean side, the other swept out low - classic
    # festejo arm carriage framing the footwork.
    up_side=lean if lean!=0 else (1 if kick else -1)
    shoulder_up=(cx+up_side*4,shoulder_y+2)
    hand_up=(cx+up_side*9,shoulder_y-7)
    limb(d,shoulder_up,hand_up,3,SKIN)
    shoulder_dn=(cx-up_side*4,shoulder_y+2)
    hand_dn=(cx-up_side*8,shoulder_y+9)
    limb(d,shoulder_dn,hand_dn,3,SKIN)
    # Head with a short festejo pañuelo/hair silhouette.
    head_y=shoulder_y-9
    d.ellipse((cx-4,head_y-4,cx+4,head_y+4),fill=OUTLINE)
    d.ellipse((cx-3,head_y-3,cx+3,head_y+3),fill=SKIN)
    d.pieslice((cx-4,head_y-5,cx+4,head_y+2),180,360,fill=HAIR)
    return im

# Six-frame alternating zapateo cycle - kick right, stomp right, recover, kick left, stomp
# left, recover - so the beat-driven phase in RunnerCoastalDancer.cs steps through real
# footwork instead of a simple lean swap.
frames=[frame(0,0,0),frame(1,1,0),frame(1,0,1),frame(0,0,0),frame(-1,1,0),frame(-1,0,1)]
sheet=Image.new('RGBA',(W*len(frames),H),(0,0,0,0))
for i,fr in enumerate(frames): sheet.alpha_composite(fr,(i*W,0))
sheet.save(ART/'costa_bailarin.png')

pixels=list(sheet.get_flattened_data())
off_palette=sorted({c for c in pixels if c[3] not in (0,255) or (c[3]==255 and c not in C)})
assert not off_palette,f'off-palette or non-binary alpha pixels found: {off_palette[:5]}'
assert before=={p.name:hashlib.sha256(p.read_bytes()).hexdigest() for p in protected}

preview=sheet.resize((sheet.width*4,sheet.height*4),Image.Resampling.NEAREST)
OUT.mkdir(parents=True,exist_ok=True)
preview.save(OUT/'costa_bailarin_preview_x4.png')

report={'size':[sheet.width,sheet.height],'frames':len(frames),'cell':[W,H],
        'previous_assets_unchanged':before,
        'purpose':'small background dance vignette figure (zambo chinchano, festejo footwork) only, replacing vector mesh art per user request; not a collectible or hazard; original stylized silhouette, not a documented individual or choreography',
        'status':'pending_user_approval'}
(OUT/'costa_bailarin_validation.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
print(json.dumps(report,indent=2))
