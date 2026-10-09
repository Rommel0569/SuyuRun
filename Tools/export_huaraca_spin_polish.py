"""Register generated circular wrist cycle without per-frame body fitting."""
from pathlib import Path
from PIL import Image
import json,shutil
root=Path(__file__).resolve().parents[1]
src=Path('C:/Users/USER/.codex/generated_images/01a0b183-22cf-7d11-8237-6260f5f81b06/exec-38a4eab5-8edb-4a44-a28d-fc0e6d8a3f71.png')
im=Image.open(src).convert('RGBA');im.putalpha(im.getchannel('A').point(lambda a:255 if a>=128 else 0))
counts=[sum(v>0 for v in im.getchannel('A').crop((x,0,x+1,im.height)).getdata()) for x in range(im.width)]
cuts=[0]
for i in range(1,8):
    centre=i*im.width/8;radius=im.width/8*.28
    cuts.append(min(range(round(centre-radius),round(centre+radius)),key=lambda x:counts[x]+abs(x-centre)*.05))
cuts.append(im.width)
cells=[]
for i in range(8):
    c=im.crop((cuts[i],0,cuts[i+1],im.height));assert c.getbbox();cells.append(c.crop(c.getbbox()))
# Fixed height from feet to identical chullo top, measured below the overhead sling.
scale=82/251
palette=[tuple(bytes.fromhex(v[1:])) for v in json.loads((root/'Assets/SuyuRun/Art/PixelSierra/sierra_palette.json').read_text())['colors']]
sheet=Image.new('RGBA',(2048,192));lut={}
for i,c in enumerate(cells):
    c=c.resize((round(c.width*scale),round(c.height*scale)),Image.Resampling.NEAREST);assert c.height<188
    pix=[]
    for v in c.getdata():
        if not v[3]:pix.append((0,0,0,0));continue
        if v[:3] not in lut:lut[v[:3]]=(*min(palette,key=lambda q:sum((q[k]-v[k])**2 for k in range(3))),255)
        pix.append(lut[v[:3]])
    c.putdata(pix)
    # Locate the common feet range in lower half to anchor torso, not sling extent.
    points=[x for y in range(c.height-6,c.height) for x in range(c.width) if c.getpixel((x,y))[3]]
    centre=(min(points)+max(points))/2
    sheet.alpha_composite(c,(round(i*256+128-centre),188-c.height))
out=root/'Assets/SuyuRun/Art/RefactorRuntime/Resources/RefactorBank/TaytaSierra'
sheet.save(out/'huaraca_spin_v3.png')
spec=dict(frames=8,fps=24,loop=True,frameWidth=256,frameHeight=192,pixelsPerUnit=64,footAnchor=[128,188],uniformScale=scale,integrated=True,playTested=False)
(out/'huaraca_spin_v3.json').write_text(json.dumps(spec,indent=2))
# Loading uses the first two existing fixed-scale throw poses; no stretched old load.
throw=Image.open(out/'huaraca_throw_v3.png')
throw.crop((0,0,512,128)).save(out/'huaraca_load_v3.png')
spec.update(frames=2,fps=16,loop=False,frameHeight=128,footAnchor=[128,124])
(out/'huaraca_load_v3.json').write_text(json.dumps(spec,indent=2))
review=root/'Artifacts/VisualRefactor/LayerPolish/Huaraca'
shutil.copy2(src,review/'spin_generated_source.png');sheet.save(review/'spin_preview.png')
print('8 spin and 2 load poses registered; body registration visual QA required')
