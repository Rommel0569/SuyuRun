"""Mechanical export of imagegen unified regional HUD kits; originals retained."""
from pathlib import Path
import argparse,json,shutil
from PIL import Image
root=Path(__file__).resolve().parents[1]
p=argparse.ArgumentParser();p.add_argument('source',type=Path);p.add_argument('region',choices=['costa','sierra','selva']);a=p.parse_args()
im=Image.open(a.source).convert('RGBA');im.putalpha(im.getchannel('A').point(lambda n:255 if n>=128 else 0))
counts=[sum(v>0 for v in im.getchannel('A').crop((0,y,im.width,y+1)).getdata()) for y in range(im.height)]
cuts=[0]
for i in (1,2):
    centre=i*im.height/3;radius=im.height/3*.3
    cuts.append(min(range(round(centre-radius),round(centre+radius)),key=lambda y:counts[y]+abs(y-centre)*.05))
cuts.append(im.height)
palette=root/(f'Assets/SuyuRun/Art/Pixel{a.region.title()}/{a.region}_palette.json' if a.region!='selva' else 'Assets/SuyuRun/Art/PixelSelva/ProductionV2/Resources/SelvaV2/selva_palette.json')
colors=[tuple(bytes.fromhex(c[1:])) for c in json.loads(palette.read_text())['colors']][:24]
out=root/'Assets/SuyuRun/Art/RefactorRuntime/Resources/UnifiedHud';out.mkdir(parents=True,exist_ok=True)
review=root/'Artifacts/VisualRefactor/UnifiedHud'/a.region;review.mkdir(parents=True,exist_ok=True)
shutil.copy2(a.source,review/'generated_source.png')
lookup={};report={}
for row,state in enumerate(('header_gun','header_sling','footer')):
    c=im.crop((0,cuts[row],im.width,cuts[row+1]));b=c.getbbox();assert b,'Missing row'
    c=c.crop(b);assert c.width/c.height>7,'Reject wrong row or fused strips'
    c=c.resize((1248,76 if state=='footer' else 80),Image.Resampling.NEAREST)
    pixels=[]
    for v in c.getdata():
        if not v[3]:pixels.append((0,0,0,0));continue
        if v[:3] not in lookup:lookup[v[:3]]=(*min(colors,key=lambda q:sum((q[k]-v[k])**2 for k in range(3))),255)
        pixels.append(lookup[v[:3]])
    c.putdata(pixels);c.save(out/f'{a.region}_{state}.png');c.save(review/f'{state}_preview.png')
    report[state]=dict(width=c.width,height=c.height,colors=len({v[:3] for v in pixels if v[3]}),sourceBounds=b)
    assert report[state]['colors']<=24
(review/'validation.json').write_text(json.dumps(report,indent=2))
print(a.region,report)
