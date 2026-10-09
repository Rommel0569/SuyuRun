"""Mechanical export of the generated attack extension; does not draw poses."""
from pathlib import Path
import json
import argparse
from PIL import Image, ImageDraw, ImageOps
from normalize_tayta_polish import isolate_character

root=Path(__file__).resolve().parents[1]
parser=argparse.ArgumentParser()
parser.add_argument('--character', choices=['stone','inca','treant','chulla'], default='stone')
args=parser.parse_args()
who=args.character
states={'stone':['jab','uppercut','charge'],'inca':['mace','charge','summon'],'treant':['roots','bind','charge'],'chulla':['curse','summon','lunge']}[who]
folder=root/'Artifacts/VisualRefactor/Bosses'
im=Image.open(folder/f'{who}_motion_source.png').convert('RGBA')
im.putalpha(im.getchannel('A').point(lambda a:255 if a>=128 else 0))
assert im.getchannel('A').getextrema()==(0,255), 'Expected genuine transparency'
counts=[sum(v>0 for v in im.getchannel('A').crop((0,y,im.width,y+1)).getdata()) for y in range(im.height)]
cuts=[0]
for k in range(1,6):
    mid=k*im.height/6
    cuts.append(min(range(round(mid-im.height/6*.18),round(mid+im.height/6*.18)),key=lambda y:counts[y]+abs(y-mid)*.03))
cuts.append(im.height)
cells=[]
for row in range(6):
    for col in range(4):
        c=isolate_character(im.crop((round(col*im.width/4),cuts[row],round((col+1)*im.width/4),cuts[row+1])))
        assert c.getbbox(), (row,col)
        cells.append(c.crop(c.getbbox()))
palette_path=root/('Assets/SuyuRun/Art/PixelSierra/sierra_palette.json' if who in ('stone','inca') else 'Assets/SuyuRun/Art/PixelSelva/ProductionV2/Resources/SelvaV2/selva_palette.json')
palette=json.loads(palette_path.read_text())['colors']
if who in ('treant','chulla'):
    palette=[c for c in palette if c not in ('#C4E37A','#7FD4C9','#C67C73','#FFF0AE')]
colors=[tuple(bytes.fromhex(c[1:])) for c in palette]
scale=min(116/max(c.height for c in cells),116/max(c.width for c in cells))
frames=[]
for c in cells:
    c=c.resize((round(c.width*scale),round(c.height*scale)),Image.Resampling.NEAREST)
    c.putdata([(*min(colors,key=lambda q:sum((q[k]-p[k])**2 for k in range(3))),255) if p[3] else (0,0,0,0) for p in c.getdata()])
    if who in ('inca','chulla'):
        c=ImageOps.mirror(c)
    f=Image.new('RGBA',(128,128));f.alpha_composite(c,((128-c.width)//2,124-c.height));frames.append(f)
out=root/f'Artifacts/VisualRefactor/Bank/{who}/MotionV2';out.mkdir(parents=True,exist_ok=True)
preview=Image.new('RGBA',(1024,408),(33,28,45,255));draw=ImageDraw.Draw(preview)
report={}
for row,state in enumerate(states):
    poses=frames[row*8:row*8+8];sheet=Image.new('RGBA',(1024,128));views=[]
    for i,f in enumerate(poses):
        sheet.alpha_composite(f,(i*128,0))
        view=Image.new('RGBA',(128,128),(33,28,45,255));view.alpha_composite(f);views.append(view.resize((384,384),Image.Resampling.NEAREST))
    sheet.save(out/f'{state}.png')
    views[0].save(out/f'{state}.gif',save_all=True,append_images=views[1:],duration=[130,130,100,70,110,90,110,170],loop=0,disposal=2)
    spec=dict(frames=8,frameWidth=128,frameHeight=128,pixelsPerUnit=64,fps=10,loop=False,orientation='left',integrated=False,playTested=False,bodyRegistrationReviewRequired=True,frameDurationsMs=[130,130,100,70,110,90,110,170])
    (out/f'{state}.json').write_text(json.dumps(spec,indent=2))
    bounds=[f.getbbox() for f in poses]
    count=len({p[:3] for p in sheet.getdata() if p[3]})
    assert count<=24 and all(b[0]>=4 and b[1]>=4 and b[2]<=124 and b[3]<=124 for b in bounds)
    report[state]=dict(frames=8,colors=count,bounds=bounds)
    preview.alpha_composite(sheet,(0,row*136+8));draw.text((2,row*136),state,fill='white')
preview.save(out/'preview.png')
(out/'validation.json').write_text(json.dumps(dict(states=report,integrated=False,playTested=False),indent=2))
print(json.dumps(report))
