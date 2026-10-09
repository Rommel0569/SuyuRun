"""Native editable masonry asset; keep original geometry and art intact."""
from pathlib import Path
import hashlib,json
from PIL import Image,ImageDraw
import build_sierra_artpack as art
root=Path(__file__).resolve().parents[1]
original=root/'Assets/SuyuRun/Art/PixelSierra/ProductionV3/Resources/SierraV3/sierra_ruinas_bloque.png'
before=hashlib.sha256(original.read_bytes()).hexdigest()
old=Image.open(original).convert('RGBA')
assert old.size==(64,32)
im=Image.new('RGBA',(128,64),art.C[0]);d=ImageDraw.Draw(im)
# Interlocking courses, not a smooth block. Hand-positioned offsets stay
# deterministic and readable at gameplay scale; no noise/dither on stone.
for row in range(4):
    y=row*16
    offset=(0,12,5,17)[row]
    for n,x in enumerate(range(-offset,128,27)):
        width=(25,26,24)[(n+row)%3]
        points=[(x+2,y+2),(x+width-3,y+1),(x+width,y+4),
                (x+width-1,y+12),(x+width-4,y+14),(x+2,y+15),(x,y+11),(x,y+5)]
        base=12 if (row+n)%3 else 13
        d.polygon(points,fill=art.C[base])
        d.line([(x+3,y+3),(x+width-4,y+2),(x+width-2,y+4)],fill=art.C[14],width=1)
        d.line([(x+2,y+12),(x+width-4,y+13),(x+width-2,y+11)],fill=art.C[11],width=2)
        d.line([(x+width-2,y+5),(x+width-3,y+10)],fill=art.C[11])
        # Small planar chips and short fissures, not stochastic speckling.
        if (row+n)%3==0:
            d.polygon([(x+7,y+6),(x+13,y+5),(x+11,y+8),(x+7,y+8)],fill=art.C[14])
            d.line([(x+17,y+7),(x+15,y+9),(x+16,y+11)],fill=art.C[11])
        elif (row+n)%3==1:
            d.line([(x+5,y+8),(x+9,y+7)],fill=art.C[13])
# Stable bright top contact edge; decorative wear stays inside the silhouette.
d.line([(0,0),(127,0)],fill=art.C[0])
d.line([(1,1),(126,1)],fill=art.C[15])
d.line([(0,63),(127,63)],fill=art.C[0])
d.line([(0,2),(0,62)],fill=art.C[0]);d.line([(127,2),(127,62)],fill=art.C[0])
for x,y in [(3,21),(119,43)]:
    d.polygon([(x,y),(x+4,y),(x+5,y+2),(x+2,y+3),(x,y+2)],fill=art.C[17])
    d.line([(x+1,y),(x+3,y)],fill=art.C[18])
im.putalpha(old.getchannel('A').resize(im.size,Image.Resampling.NEAREST))
out=root/'Assets/SuyuRun/Art/RefactorRuntime/Resources/RefactorBank/Environment'
out.mkdir(parents=True,exist_ok=True)
name='sierra_ruinas_bloque'
im.save(out/(name+'.png'))
spec=dict(frames=1,frameWidth=128,frameHeight=64,logicalWidth=64,logicalHeight=32,
          pixelsPerUnit=64,fps=1,loop=True,original=original.relative_to(root).as_posix(),
          source='Tools/refine_ruins_block.py',fullPlaythroughTested=False)
(out/(name+'.json')).write_text(json.dumps(spec,indent=2),encoding='utf-8')
preview=root/'Artifacts/VisualRefactor/sierra_ruinas_bloque_detail_preview_x4.png'
im.resize((512,256),Image.Resampling.NEAREST).save(preview)
assert hashlib.sha256(original.read_bytes()).hexdigest()==before
assert len({p[:3] for p in im.getdata() if p[3]})<=24
assert im.getchannel('A').tobytes()==old.getchannel('A').resize(im.size,Image.Resampling.NEAREST).tobytes()
print('PASS native masonry 128x64; logical64x32; alpha and original preserved')
