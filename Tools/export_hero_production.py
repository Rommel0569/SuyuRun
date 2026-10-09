"""Export fixed-scale core hero atlases, no procedural replacement of drawings."""
from pathlib import Path
import argparse,json
from statistics import median
from PIL import Image,ImageDraw
from normalize_tayta_polish import isolate_character

root=Path(__file__).resolve().parents[1]
parser=argparse.ArgumentParser();parser.add_argument('region',choices=['costa','sierra']);parser.add_argument('--extras',action='store_true');args=parser.parse_args();region=args.region
source=root/f'Artifacts/VisualRefactor/Heroes/{region}_{"extra_fixed" if args.extras else "production"}_source.png'
rows=7 if args.extras and region=='costa' else 8
im=Image.open(source).convert('RGBA');im.putalpha(im.getchannel('A').point(lambda a:255 if a>=128 else 0))
assert im.getchannel('A').getextrema()==(0,255)
counts=[sum(v>0 for v in im.getchannel('A').crop((0,y,im.width,y+1)).getdata()) for y in range(im.height)]
cuts=[0]
for k in range(1,8):
    mid=k*im.height/8;rad=im.height/8*.2
    cuts.append(min(range(round(mid-rad),round(mid+rad)),key=lambda y:counts[y]+abs(y-mid)*.03))
cuts.append(im.height);cells=[]
# Generated atlases may have unequal horizontal margins. Segment whole actors
# first: equal-width cutting cropped the first idle head in the source layout.
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
actors=sorted(components,key=len,reverse=True)[:rows*4]
assert len(actors)==rows*4
def box_of(group):
    return (min(x for x,y in group),min(y for x,y in group),max(x for x,y in group)+1,max(y for x,y in group)+1)
actors.sort(key=lambda g:(box_of(g)[1]+box_of(g)[3])/2)
for row in range(rows):
    ordered=sorted(actors[row*4:row*4+4],key=lambda g:box_of(g)[0])
    for actor in ordered:
        box=box_of(actor)
        c=Image.new('RGBA',(box[2]-box[0],box[3]-box[1]))
        for x,y in actor:c.putpixel((x-box[0],y-box[1]),im.getpixel((x,y)))
        cells.append(c)
# Idle anatomical scale for ALL states: wide guns/ponchos must not shrink actor.
areas=[sum(a>0 for a in c.getchannel('A').getdata()) for c in cells]
assert min(areas)>median(areas)*.25,'Incomplete or merged actor segmentation; reject this export and fix source.'
scale=82/max(c.height for c in (cells[20:24] if args.extras and region=='costa' else cells[16:20] if args.extras else cells[:4]))
palette=json.loads((root/f'Assets/SuyuRun/Art/Pixel{region.capitalize()}/{region}_palette.json').read_text())['colors']
colors=[tuple(bytes.fromhex(c[1:])) for c in palette]
frames=[]
cell_h=max(128,((round(max(c.height for c in cells)*scale)+11)//64+1)*64)
cell_w=max(192,((round(max(c.width for c in cells)*scale)+11)//64+1)*64)
for c in cells:
    c=c.resize((round(c.width*scale),round(c.height*scale)),Image.Resampling.NEAREST)
    assert c.width<cell_w-4 and c.height<cell_h-4,c.size
    c.putdata([(*min(colors,key=lambda q:sum((q[k]-p[k])**2 for k in range(3))),255) if p[3] else (0,0,0,0) for p in c.getdata()])
    f=Image.new('RGBA',(cell_w,cell_h));f.alpha_composite(c,((cell_w-c.width)//2,cell_h-4-c.height));frames.append(f)
out=root/f'Artifacts/VisualRefactor/Bank/Tayta{region.capitalize()}';out.mkdir(parents=True,exist_ok=True)
groups={'idle':frames[:4],'run':frames[4:12],'jump':frames[12:16],
        'slide':frames[16:20],'burst':frames[20:24],'hit':frames[24:28],'death':frames[28:32]}
if args.extras:groups=dict(zip(['huaraca_load','huaraca_spin','huaraca_throw','reload','guard','climb','dance','transformation'],[frames[i:i+4] for i in range(0,32,4)]))
if args.extras and region=='costa':groups=dict(zip(['double_jump','fall','land','reload','guard','dance','victory'],[frames[i:i+4] for i in range(0,28,4)]))
manifest={};preview=Image.new('RGBA',(96+cell_w*8,len(groups)*(cell_h+8)),(33,28,45,255));draw=ImageDraw.Draw(preview)
for row,(state,poses) in enumerate(groups.items()):
    sheet=Image.new('RGBA',(cell_w*len(poses),cell_h));views=[]
    for i,f in enumerate(poses):
        sheet.alpha_composite(f,(i*cell_w,0));view=Image.new('RGBA',(cell_w,cell_h),(33,28,45,255));view.alpha_composite(f);views.append(view.resize((cell_w*3,cell_h*3),Image.Resampling.NEAREST))
    fps=14 if state in ('run','burst') else 8
    sheet.save(out/f'{state}.png');views[0].save(out/f'{state}.gif',save_all=True,append_images=views[1:],duration=round(1000/fps),loop=0,disposal=2)
    data=dict(frames=len(poses),fps=fps,loop=state in ('idle','run','slide','burst'),frameWidth=cell_w,frameHeight=cell_h,pixelsPerUnit=64,
        bodyScaleReference='idle-common-source-scale',footAnchor=[cell_w//2,cell_h-4],renderOffsetFrom96Cell=[48-cell_w//2,93-(cell_h-4)],
        integrated=False,playTested=False,bodyRegistrationReviewRequired=True)
    (out/f'{state}.json').write_text(json.dumps(data,indent=2))
    manifest[state]=dict(frames=len(poses),bounds=[f.getbbox() for f in poses])
    draw.text((2,row*(cell_h+8)+50),state,fill='white');preview.alpha_composite(sheet,(96,row*(cell_h+8)+4))
preview.save(out/('extras_preview.png' if args.extras else 'bank_preview.png'));(out/('extras_manifest.json' if args.extras else 'manifest.json')).write_text(json.dumps(dict(states=manifest,integrated=False,playTested=False),indent=2))
print(region,len(frames),'poses exported, common scale, wide cells keep weapons inside frame')
