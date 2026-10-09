"""Assemble versioned review assets; never overwrite runtime sprites."""
from pathlib import Path
from PIL import Image,ImageDraw
import json
from normalize_tayta_polish import isolate_character

root=Path(__file__).resolve().parents[1];src=root/'Artifacts/VisualRefactor/Tayta'
out=root/'Artifacts/VisualRefactor/Bank/TaytaSelva';out.mkdir(parents=True,exist_ok=True)
palette=json.loads((root/'Assets/SuyuRun/Art/PixelSelva/ProductionV2/Resources/SelvaV2/selva_palette.json').read_text())['colors']
palette=[c for c in palette if c not in ('#4A7A3E','#6FA83D','#96C954','#C4E37A')]
colors=[tuple(bytes.fromhex(c[1:])) for c in palette]
manifest={};allframes={}

def load_native(name):
    im=Image.open(src/f'selva_{name}_96.png').convert('RGBA')
    return [im.crop((i*96,0,(i+1)*96,96)) for i in range(im.width//96)]

def normalize(name,rows):
    im=Image.open(src/f'selva_{name}_96_fuente.png').convert('RGBA')
    im.putalpha(im.getchannel('A').point(lambda a:255 if a>=128 else 0));cells=[]
    for row in range(rows):
        for col in range(4):
            c=im.crop((round(col*im.width/4),round(row*im.height/rows),round((col+1)*im.width/4),round((row+1)*im.height/rows)))
            c=isolate_character(c);cells.append(c.crop(c.getbbox()))
    scale=min(86/max(c.height for c in cells),88/max(c.width for c in cells))
    frames=[]
    for c in cells:
        c=c.resize((round(c.width*scale),round(c.height*scale)),Image.Resampling.NEAREST)
        c.putdata([(*min(colors,key=lambda q:sum((q[k]-p[k])**2 for k in range(3))),255) if p[3] else (0,0,0,0) for p in c.getdata()])
        f=Image.new('RGBA',(96,96));f.alpha_composite(c,((96-c.width)//2,93-c.height));frames.append(f)
    return frames

def emit(state,frames,fps=12,loop=False,grip=False):
    sheet=Image.new('RGBA',(96*len(frames),96))
    for i,f in enumerate(frames):sheet.alpha_composite(f,(96*i,0))
    sheet.save(out/f'{state}.png')
    spec=dict(frames=len(frames),fps=fps,loop=loop,frameWidth=96,frameHeight=96,pixelsPerUnit=64,gripCoordinateScale=.5)
    if grip:
        # Review anchor estimate. Validate alignment against moving vine in Play.
        points=[]
        for f in frames:
            candidates=[(x,y) for y in range(40) for x in range(96) if f.getpixel((x,y))[3] and f.getpixel((x,y))[:3] in ((232,134,46),(245,169,77),(201,100,30))]
            top=min((p[1] for p in candidates),default=5)
            band=[p[0] for p in candidates if p[1]<=top+3]
            points.append((sum(band)/len(band) if band else 48,top+2))
        spec['gripX']=[p[0] for p in points];spec['gripY']=[p[1] for p in points]
        spec['anchorReviewRequired']=True
    (out/f'{state}.json').write_text(json.dumps(spec,indent=2))
    views=[]
    for f in frames:
        canvas=Image.new('RGBA',(96,96),(33,28,45,255));canvas.alpha_composite(f)
        views.append(canvas.resize((288,288),Image.Resampling.NEAREST))
    views[0].save(out/f'{state}.gif',save_all=True,append_images=views[1:],duration=round(1000/fps),loop=0,disposal=2)
    count=len({p[:3] for p in sheet.getdata() if p[3]});bounds=[f.getbbox() for f in frames]
    assert count<=24 and set(sheet.getchannel('A').getdata())<={0,255}
    assert all(b and b[0]>=2 and b[1]>=2 and b[2]<=94 and b[3]<=94 for b in bounds), (state,bounds)
    manifest[state]=dict(frames=len(frames),colors=count,bounds=bounds,integrated=False)
    allframes[state]=frames

emit('idle',load_native('idle'),5,True);emit('run',load_native('run'),14,True)
jump=load_native('jump')
for state,ids in {'jump':[0,1,2,3,5],'double_jump':[4,3,5],'fall':[5],'land':[6,7]}.items():emit(state,[jump[i] for i in ids])
slide=normalize('slide',2)
emit('slide_enter',slide[:2]);emit('slide',slide[2:6],12,True);emit('slide_exit',slide[6:])
w=normalize('weapons',4);emit('burst',w[:8],14,True);emit('reload',w[8:12]);emit('guard',w[12:],6,True)
l=normalize('lianas',4)
emit('swing',l[:8],12,False,True);emit('grab',l[8:10],12,False,True)
emit('regrab',[l[8],l[9],l[3]],12,False,True);emit('release',l[10:12]);emit('climb',l[12:],10,True,True)
f=normalize('fight',4)
for state,start in [('punch',0),('kick',4),('uppercut',8),('dodge',12)]:emit(state,f[start:start+4],16)
r=normalize('reactions',4)
for state,start in [('hit',0),('death',4),('baile',8),('victory',12)]:emit(state,r[start:start+4],8,state in ('baile','victory'))
states=list(allframes);preview=Image.new('RGBA',(864,len(states)*104),(33,28,45,255));d=ImageDraw.Draw(preview)
for row,state in enumerate(states):
    d.text((3,row*104+42),state,fill=(240,209,102,255))
    for col,f in enumerate(allframes[state][:8]):preview.alpha_composite(f,(96+col*96,row*104+4))
preview.save(out/'bank_preview.png')
(out/'manifest.json').write_text(json.dumps(dict(character='Tayta protector Selva',sheets=len(manifest),frames=sum(v['frames'] for v in manifest.values()),states=manifest,notes=['Sources and old runtime preserved','Vine anchors require Play verification','Not integrated or Play-tested']),indent=2))
print(json.dumps(dict(sheets=len(manifest),frames=sum(v['frames'] for v in manifest.values()),validation='PASS size/palette/alpha/margins',runtimeIntegrated=False)))
