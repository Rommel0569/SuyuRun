"""Palette/atlas normalization only; all poses are imagegen drawings."""
from pathlib import Path
from PIL import Image,ImageDraw,ImageOps
import argparse,json
from normalize_tayta_polish import isolate_character

root=Path(__file__).resolve().parents[1]
parser=argparse.ArgumentParser();parser.add_argument('actor');parser.add_argument('--region',default='sierra');parser.add_argument('--flip',action='store_true');args=parser.parse_args()
im=Image.open(root/f'Artifacts/VisualRefactor/Actors/{args.actor}_source.png').convert('RGBA')
im.putalpha(im.getchannel('A').point(lambda a:255 if a>=128 else 0));assert im.getchannel('A').getextrema()==(0,255)
states=['idle','spawn','walk','anticipation','attack','charge','hit','death']
if args.actor=='caiman':states=['idle','stalk','anticipation','breach','snap','fall','dive','death']
if args.actor in ('condor','hawk','puna','coastbird'):states=['flight','flight2','anticipation','dive','recover','charge','hit','death']
if args.actor=='coastbird':states=['idle','spawn','flight','anticipation','attack','charge','hit','death']
if args.actor in ('condor','hawk'):states=['flight','flight2','anticipation','dive','charge','hit','death']
if args.actor=='dancer':states=['idle','dance','dance2','dance3']
rows=len(states)
counts=[sum(v>0 for v in im.getchannel('A').crop((0,y,im.width,y+1)).getdata()) for y in range(im.height)]
cuts=[0]
for k in range(1,rows):
    mid=k*im.height/rows;rad=im.height/rows*.2
    cuts.append(min(range(round(mid-rad),round(mid+rad)),key=lambda y:counts[y]+abs(y-mid)*.03))
cuts.append(im.height);cells=[]
for row in range(rows):
    band=im.crop((0,cuts[row],im.width,cuts[row+1]))
    projection=[sum(v>0 for v in band.getchannel('A').crop((x,0,x+1,band.height)).getdata()) for x in range(band.width)]
    runs=[]
    for x,count in enumerate(projection):
        if count<2:continue
        if not runs or x-runs[-1][-1]>18:runs.append([x])
        else:runs[-1].append(x)
    if len(runs)>=4:
        runs=sorted(sorted(runs,key=lambda r:sum(projection[x] for x in r),reverse=True)[:4],key=lambda r:r[0])
        xs=[0]+[(runs[k-1][-1]+runs[k][0])//2 for k in range(1,4)]+[band.width]
    else:
        xs=[0]
        for k in range(1,4):
            mid=k*band.width/4;rad=band.width/4*.15
            xs.append(min(range(round(mid-rad),round(mid+rad)),key=lambda x:projection[x]+abs(x-mid)*.02))
        xs.append(band.width)
    for col in range(4):
        c=band.crop((xs[col],0,xs[col+1],band.height));assert c.getbbox(),(row,col)
        cells.append(c.crop(c.getbbox()))
if args.actor=='caiman':
    # Tall diagonal poses overlap nominal row boundaries: never cut them into
    # equal bands. Extract complete connected bodies; detached droplets belong
    # to the separate splash effect, not to a neighbouring actor's cell.
    alpha=im.getchannel('A').load()
    pending={(x,y) for y in range(im.height) for x in range(im.width) if alpha[x,y]}
    components=[]
    while pending:
        first=pending.pop();group={first};queue=[first]
        while queue:
            x,y=queue.pop()
            for dx in (-1,0,1):
                for dy in (-1,0,1):
                    point=(x+dx,y+dy)
                    if point in pending:pending.remove(point);group.add(point);queue.append(point)
        components.append(group)
    def box_of(g):return (min(x for x,y in g),min(y for x,y in g),max(x for x,y in g)+1,max(y for x,y in g)+1)
    actors=sorted(components,key=len,reverse=True)[:32]
    assert len(actors)==32
    actors.sort(key=lambda g:(box_of(g)[1]+box_of(g)[3])/2);cells=[]
    for row in range(8):
        for actor in sorted(actors[row*4:row*4+4],key=lambda g:box_of(g)[0]):
            b=box_of(actor);c=Image.new('RGBA',(b[2]-b[0],b[3]-b[1]))
            for x,y in actor:c.putpixel((x-b[0],y-b[1]),im.getpixel((x,y)))
            cells.append(c)
palette_path=root/(f'Assets/SuyuRun/Art/Pixel{args.region.title()}/{args.region}_palette.json' if args.region!='selva' else 'Assets/SuyuRun/Art/PixelSelva/ProductionV2/Resources/SelvaV2/selva_palette.json')
palette=json.loads(palette_path.read_text())['colors']
if args.region=='selva':palette=[c for c in palette if c not in ('#C4E37A','#7FD4C9','#C67C73','#FFF0AE')]
colors=[tuple(bytes.fromhex(c[1:])) for c in palette]
scale=min(116/max(c.height for c in cells),116/max(c.width for c in cells))
frames=[]
for c in cells:
    c=c.resize((round(c.width*scale),round(c.height*scale)),Image.Resampling.NEAREST)
    if args.flip:c=ImageOps.mirror(c)
    c.putdata([(*min(colors,key=lambda q:sum((q[k]-p[k])**2 for k in range(3))),255) if p[3] else (0,0,0,0) for p in c.getdata()])
    f=Image.new('RGBA',(128,128));f.alpha_composite(c,((128-c.width)//2,124-c.height));frames.append(f)
out=root/f'Artifacts/VisualRefactor/Bank/Actors/{args.actor}';out.mkdir(parents=True,exist_ok=True)
preview=Image.new('RGBA',(608,1088),(33,28,45,255));draw=ImageDraw.Draw(preview);manifest={}
for row,state in enumerate(states):
    poses=frames[row*4:row*4+4];sheet=Image.new('RGBA',(512,128));views=[]
    for i,f in enumerate(poses):
        sheet.alpha_composite(f,(128*i,0));view=Image.new('RGBA',(128,128),(33,28,45,255));view.alpha_composite(f);views.append(view.resize((384,384),Image.Resampling.NEAREST))
    fps=6 if state=='idle' else 12;sheet.save(out/f'{state}.png')
    views[0].save(out/f'{state}.gif',save_all=True,append_images=views[1:],duration=round(1000/fps),loop=0,disposal=2)
    spec=dict(frames=4,frameWidth=128,frameHeight=128,pixelsPerUnit=64,fps=fps,loop=state in ('idle','walk','flight','flight2'),orientation='right',integrated=False,region=args.region)
    (out/f'{state}.json').write_text(json.dumps(spec,indent=2));manifest[state]=dict(frames=4,bounds=[f.getbbox() for f in poses])
    draw.text((2,row*136+50),state,fill='white');preview.alpha_composite(sheet,(96,row*136+4))
preview.save(out/'preview.png');(out/'manifest.json').write_text(json.dumps(dict(states=manifest,integrated=False,playTested=False),indent=2))
if args.actor=='dancer':
    poses=frames[4:16];sheet=Image.new('RGBA',(128*len(poses),128));views=[]
    for i,f in enumerate(poses):
        sheet.alpha_composite(f,(128*i,0));view=Image.new('RGBA',(128,128),(33,28,45,255));view.alpha_composite(f);views.append(view.resize((384,384),Image.Resampling.NEAREST))
    sheet.save(out/'dance.png');views[0].save(out/'dance.gif',save_all=True,append_images=views[1:],duration=83,loop=0,disposal=2)
    spec.update(frames=12,fps=12,loop=True);(out/'dance.json').write_text(json.dumps(spec,indent=2))
print(args.actor,len(frames),'poses exported')
