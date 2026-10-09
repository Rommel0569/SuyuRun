"""Export redrawn fixed-anatomy throw, preserving one scale and foot anchor."""
from pathlib import Path
import json,shutil
from PIL import Image
root=Path(__file__).resolve().parents[1]
src=Path('C:/Users/USER/.codex/generated_images/01a0b183-22cf-7d11-8237-6260f5f81b06/exec-3da996e5-8e2f-43ff-a158-e3a6ddc89ae9.png')
im=Image.open(src).convert('RGBA');im.putalpha(im.getchannel('A').point(lambda a:255 if a>=128 else 0))
# Locate clear vertical separators rather than assume equidistant generated poses.
counts=[sum(v>0 for v in im.getchannel('A').crop((x,0,x+1,im.height)).getdata()) for x in range(im.width)]
cuts=[0]
for i in range(1,8):
    centre=i*im.width/8;radius=im.width/8*.3
    cuts.append(min(range(round(centre-radius),round(centre+radius)),key=lambda x:counts[x]+abs(x-centre)*.05))
cuts.append(im.width)
cells=[]
for i in range(8):
    c=im.crop((cuts[i],0,cuts[i+1],im.height));b=c.getbbox();assert b
    cells.append(c.crop(b))
scale=82/cells[0].height
colors=[tuple(bytes.fromhex(c[1:])) for c in json.loads((root/'Assets/SuyuRun/Art/PixelSierra/sierra_palette.json').read_text())['colors']]
out=root/'Assets/SuyuRun/Art/RefactorRuntime/Resources/RefactorBank/TaytaSierra'
sheet=Image.new('RGBA',(256*8,128));bounds=[];lut={}
for i,c in enumerate(cells):
    c=c.resize((round(c.width*scale),round(c.height*scale)),Image.Resampling.NEAREST);assert c.width<184 and c.height<124
    pixels=[]
    for v in c.getdata():
        if not v[3]:pixels.append((0,0,0,0));continue
        if v[:3] not in lut:lut[v[:3]]=(*min(colors,key=lambda q:sum((q[k]-v[k])**2 for k in range(3))),255)
        pixels.append(lut[v[:3]])
    c.putdata(pixels)
    # Torso is on the left for outstretched-arm poses, centre its neutral width
    # instead of centring the entire weapon reach (which shifts anatomy).
    neutralWidth=round(cells[0].width*scale)
    x=128-neutralWidth//2
    if i in (2,3):x-=round(18*scale)
    assert x+c.width<256,'Weapon reaches neighbouring frame'
    sheet.alpha_composite(c,(256*i+x,124-c.height));bounds.append([x,124-c.height,c.width,c.height])
sheet.save(out/'huaraca_throw_v3.png')
spec=dict(frames=8,fps=24,loop=False,frameWidth=256,frameHeight=128,pixelsPerUnit=64,footAnchor=[128,124],uniformScale=scale,bodyScaleReference='82px-neutral-reference-no-anisotropic-scaling',frameBounds=bounds,integrated=True,playTested=False)
(out/'huaraca_throw_v3.json').write_text(json.dumps(spec,indent=2))
review=root/'Artifacts/VisualRefactor/LayerPolish/Huaraca';review.mkdir(parents=True,exist_ok=True)
shutil.copy2(src,review/'generated_source.png');sheet.save(review/'throw_preview.png')
print('8 frames exported, uniform scale, no X/Y deformation')
