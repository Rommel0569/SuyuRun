"""Export effects separately from actors, no runtime changes."""
from pathlib import Path
from PIL import Image,ImageDraw
import json
root=Path(__file__).resolve().parents[1]
source=root/'Artifacts/VisualRefactor/Effects/effects_source.png'
im=Image.open(source).convert('RGBA');im.putalpha(im.getchannel('A').point(lambda a:255 if a>=128 else 0))
assert 0 in set(im.getchannel('A').getdata())
states=['raiz_dano','arbol_atadura','maldicion','invocacion','impacto_piedra','impacto_dorado','escudo','impacto_arma']
colors=json.loads((root/'Assets/SuyuRun/Art/PixelSelva/ProductionV2/Resources/SelvaV2/selva_palette.json').read_text())['colors']
colors=[tuple(bytes.fromhex(c[1:])) for c in colors if c not in ('#C4E37A','#C67C73','#7FD4C9','#FFF0AE')]
counts=[sum(v>0 for v in im.getchannel('A').crop((0,y,im.width,y+1)).getdata()) for y in range(im.height)]
cuts=[0]
for k in range(1,8):
    mid=k*im.height/8;radius=im.height/8*.15
    cuts.append(min(range(round(mid-radius),round(mid+radius)),key=lambda y:counts[y]+abs(y-mid)*.04))
cuts.append(im.height)
out=root/'Artifacts/VisualRefactor/Bank/Effects';out.mkdir(parents=True,exist_ok=True)
report={};preview=Image.new('RGBA',(480,8*104),(33,28,45,255));draw=ImageDraw.Draw(preview)
for row,state in enumerate(states):
    cells=[]
    for col in range(4):
        c=im.crop((round(col*im.width/4),cuts[row],round((col+1)*im.width/4),cuts[row+1]))
        assert c.getbbox(),(state,col);cells.append(c.crop(c.getbbox()))
    scale=min(88/max(c.height for c in cells),88/max(c.width for c in cells))
    sheet=Image.new('RGBA',(384,96));frames=[]
    for i,c in enumerate(cells):
        c=c.resize((round(c.width*scale),round(c.height*scale)),Image.Resampling.NEAREST)
        c.putdata([(*min(colors,key=lambda q:sum((q[k]-p[k])**2 for k in range(3))),255) if p[3] else (0,0,0,0) for p in c.getdata()])
        f=Image.new('RGBA',(96,96));f.alpha_composite(c,((96-c.width)//2,92-c.height if row in (0,1,3,4) else (96-c.height)//2))
        frames.append(f);sheet.alpha_composite(f,(i*96,0));preview.alpha_composite(f,(96+i*96,row*104+4))
    draw.text((2,row*104+42),state,fill=(240,209,102,255));sheet.save(out/f'{state}.png')
    (out/f'{state}.json').write_text(json.dumps(dict(frames=4,fps=12,loop=False,frameWidth=96,frameHeight=96,pixelsPerUnit=64,integrated=False),indent=2))
    views=[]
    for f in frames:
        canvas=Image.new('RGBA',(96,96),(33,28,45,255));canvas.alpha_composite(f);views.append(canvas.resize((288,288),Image.Resampling.NEAREST))
    views[0].save(out/f'{state}.gif',save_all=True,append_images=views[1:],duration=100,loop=0,disposal=2)
    count=len({p[:3] for p in sheet.getdata() if p[3]});assert count<=24
    assert set(sheet.getchannel('A').getdata())<={0,255}
    bounds=[f.getbbox() for f in frames];assert all(b[0]>=4 and b[1]>=4 and b[2]<=92 and b[3]<=92 for b in bounds)
    report[state]=dict(frames=4,colors=count,bounds=bounds,integrated=False)
preview.save(out/'bank_preview.png');(out/'manifest.json').write_text(json.dumps(dict(sheets=8,frames=32,states=report,integrated=False,playTested=False),indent=2))
print('8 effects / 32 frames; native bounds, palette and alpha PASS; no runtime replacement')
