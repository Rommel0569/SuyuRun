"""Export high-resolution review banks, preserving old runtime and sources."""
from pathlib import Path
from PIL import Image,ImageDraw,ImageOps
import json
from normalize_tayta_polish import isolate_character

root=Path(__file__).resolve().parents[1];src=root/'Artifacts/VisualRefactor/Bosses'
states=['idle','despertar','andar','anticipar','golpe','carga','hit','derrota']
report={}
for who,filename,region,flip in [('stone','stone_source_v2.png','sierra',True),('inca','inca_source.png','sierra',True),('treant','treant_source_v3.png','selva',False),('chulla','chulla_source.png','selva',True)]:
    im=Image.open(src/filename).convert('RGBA');im.putalpha(im.getchannel('A').point(lambda a:255 if a>=128 else 0))
    assert 0 in set(im.getchannel('A').getdata()),(who,'opaque source')
    path=root/('Assets/SuyuRun/Art/PixelSierra/sierra_palette.json' if region=='sierra' else 'Assets/SuyuRun/Art/PixelSelva/ProductionV2/Resources/SelvaV2/selva_palette.json')
    palette=json.loads(path.read_text())['colors']
    if region=='selva':palette=[c for c in palette if c not in ('#C4E37A','#7FD4C9','#C67C73','#FFF0AE')]
    colors=[tuple(bytes.fromhex(c[1:])) for c in palette]
    # Locate narrow inter-row gaps rather than assuming generated margins equal.
    counts=[sum(v>0 for v in im.getchannel('A').crop((0,y,im.width,y+1)).getdata()) for y in range(im.height)]
    cuts=[0]
    for k in range(1,8):
        mid=k*im.height/8;radius=im.height/8*.2
        cuts.append(min(range(round(mid-radius),round(mid+radius)),key=lambda y:counts[y]+abs(y-mid)*.03))
    cuts.append(im.height);cells=[]
    for row in range(8):
        for col in range(4):
            c=im.crop((round(col*im.width/4),cuts[row],round((col+1)*im.width/4),cuts[row+1]))
            c=isolate_character(c);box=c.getbbox();assert box,(who,row,col);cells.append(c.crop(box))
    scale=min(116/max(c.height for c in cells),116/max(c.width for c in cells));frames=[]
    for c in cells:
        c=c.resize((round(c.width*scale),round(c.height*scale)),Image.Resampling.NEAREST)
        c.putdata([(*min(colors,key=lambda q:sum((q[k]-p[k])**2 for k in range(3))),255) if p[3] else (0,0,0,0) for p in c.getdata()])
        if flip:c=ImageOps.mirror(c)
        f=Image.new('RGBA',(128,128));f.alpha_composite(c,((128-c.width)//2,124-c.height));frames.append(f)
    out=root/f'Artifacts/VisualRefactor/Bank/{who}';out.mkdir(parents=True,exist_ok=True)
    rows=[];manifest={}
    for row,state in enumerate(states):
        poses=frames[row*4:row*4+4];sheet=Image.new('RGBA',(512,128))
        for i,f in enumerate(poses):sheet.alpha_composite(f,(128*i,0))
        sheet.save(out/f'{state}.png');fps=6 if state=='idle' else 10
        spec=dict(frames=4,fps=fps,loop=state in ('idle','andar'),frameWidth=128,frameHeight=128,pixelsPerUnit=64,orientation='left',hitboxesUnchanged=True)
        (out/f'{state}.json').write_text(json.dumps(spec,indent=2))
        views=[]
        for f in poses:
            canvas=Image.new('RGBA',(128,128),(33,28,45,255));canvas.alpha_composite(f);views.append(canvas.resize((384,384),Image.Resampling.NEAREST))
        views[0].save(out/f'{state}.gif',save_all=True,append_images=views[1:],duration=round(1000/fps),loop=0,disposal=2)
        bounds=[f.getbbox() for f in poses];count=len({p[:3] for p in sheet.getdata() if p[3]})
        assert all(b and b[0]>=4 and b[1]>=4 and b[2]<=124 and b[3]<=124 for b in bounds),(who,state,bounds)
        assert count<=24 and set(sheet.getchannel('A').getdata())<={0,255}
        manifest[state]=dict(frames=4,bounds=bounds,colors=count,integrated=False)
        rows.append(sheet)
    preview=Image.new('RGBA',(608,8*136),(33,28,45,255));d=ImageDraw.Draw(preview)
    for row,sheet in enumerate(rows):d.text((3,row*136+56),states[row],fill=(240,209,102,255));preview.alpha_composite(sheet,(96,row*136+4))
    preview.save(out/'bank_preview.png')
    (out/'manifest.json').write_text(json.dumps(dict(character=who,region=region,sheets=8,frames=32,states=manifest,integrated=False,playTested=False),indent=2))
    report[who]=dict(sheets=8,frames=32,validation='PASS native size/palette/alpha/margins',integrated=False)
(src/'bank_validation.json').write_text(json.dumps(report,indent=2));print(json.dumps(report))
