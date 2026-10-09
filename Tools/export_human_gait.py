"""Mechanical atlas segmentation, palette normalization and gait registration.
All character drawings are generated artwork; no procedural repainting.
"""
from pathlib import Path
import argparse,json
from PIL import Image,ImageDraw

root=Path(__file__).resolve().parents[1]
parser=argparse.ArgumentParser();parser.add_argument('region');parser.add_argument('--state',default='run');args=parser.parse_args();region=args.region;state=args.state
source=root/f'Artifacts/VisualRefactor/Heroes/{region}_human_{state}_source.png'
im=Image.open(source).convert('RGBA');im.putalpha(im.getchannel('A').point(lambda a:255 if a>=128 else 0))
alpha=im.getchannel('A').load();pending={(x,y) for y in range(im.height) for x in range(im.width) if alpha[x,y]}
components=[]
while pending:
    first=pending.pop();group={first};queue=[first]
    while queue:
        x,y=queue.pop()
        for dx in (-1,0,1):
            for dy in (-1,0,1):
                p=(x+dx,y+dy)
                if p in pending:pending.remove(p);group.add(p);queue.append(p)
    components.append(group)
def box(g):return min(x for x,y in g),min(y for x,y in g),max(x for x,y in g)+1,max(y for x,y in g)+1
count=4 if state=='slide' else 12;rows=count//4
actors=sorted(components,key=len,reverse=True)[:count]
assert len(actors)==count and min(map(len,actors))>max(map(len,actors))*.35,'Reject fragmented source'
actors.sort(key=lambda g:(box(g)[1]+box(g)[3])/2)
cells=[]
for row in range(rows):
    for g in sorted(actors[row*4:row*4+4],key=lambda g:box(g)[0]):
        b=box(g);c=Image.new('RGBA',(b[2]-b[0],b[3]-b[1]))
        for x,y in g:c.putpixel((x-b[0],y-b[1]),im.getpixel((x,y)))
        cells.append(c)
palette=root/(f'Assets/SuyuRun/Art/Pixel{region.title()}/{region}_palette.json' if region!='selva' else 'Assets/SuyuRun/Art/PixelSelva/ProductionV2/Resources/SelvaV2/selva_palette.json')
colors=json.loads(palette.read_text())['colors']
if region=='selva':colors=[c for c in colors if c not in ('#C4E37A','#7FD4C9','#C67C73','#FFF0AE')]
colors=[tuple(bytes.fromhex(c[1:])) for c in colors]
scale=(44 if state=='slide' else 82)/max(c.height for c in cells)
lift=[0]*count if state=='slide' else [0,0,2,5,6,2,0,0,2,5,6,2]
frames=[];views=[];sheet=Image.new('RGBA',(192*count,128))
for i,c in enumerate(cells):
    c=c.resize((round(c.width*scale),round(c.height*scale)),Image.Resampling.NEAREST)
    c.putdata([(*min(colors,key=lambda q:sum((q[k]-p[k])**2 for k in range(3))),255) if p[3] else (0,0,0,0) for p in c.getdata()])
    assert c.width<188 and c.height+lift[i]<124
    f=Image.new('RGBA',(192,128));f.alpha_composite(c,((192-c.width)//2,124-c.height-lift[i]));frames.append(f);sheet.alpha_composite(f,(192*i,0))
    v=Image.new('RGBA',f.size,(33,28,45,255));v.alpha_composite(f);views.append(v.resize((576,384),Image.Resampling.NEAREST))
out=root/f'Artifacts/VisualRefactor/Bank/Tayta{region.title()}';out.mkdir(parents=True,exist_ok=True)
sheet.save(out/f'{state}_human.png');views[0].save(out/f'{state}_human.gif',save_all=True,append_images=views[1:],duration=42,loop=0,disposal=2)
spec=dict(frames=count,fps=12 if state=='slide' else 24,loop=True,frameWidth=192,frameHeight=128,pixelsPerUnit=64,footAnchor=[96,124],gaitLift=lift,
    silhouetteHeights=[f.getbbox()[3]-f.getbbox()[1] for f in frames],
    strideLogicalPixels=112,source=source.relative_to(root).as_posix(),integrated=False,playTested=False)
(out/f'{state}_human.json').write_text(json.dumps(spec,indent=2))
preview=Image.new('RGBA',(192*4,128*rows),(33,28,45,255))
for i,f in enumerate(frames):preview.alpha_composite(f,(192*(i%4),128*(i//4)))
preview.save(out/f'{state}_human_preview.png');print(region,state,count,'gait frames exported, common scale, flight phases preserved')
