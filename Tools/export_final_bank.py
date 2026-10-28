"""Mechanical export of independently generated cinematic layers and grids."""
from pathlib import Path
import argparse,json,shutil
from PIL import Image
p=argparse.ArgumentParser();p.add_argument('name');p.add_argument('source',type=Path);p.add_argument('--revision',default='FinalMovieV3');a=p.parse_args()
root=Path(__file__).resolve().parents[1]
plan=json.loads((root/'Artifacts'/a.revision/'PLAN_COMPLETE.json').read_text());j=next(x for x in plan if x['name']==a.name)
region=j['region'];palette=root/(f'Assets/SuyuRun/Art/Pixel{region.title()}/{region}_palette.json' if region!='selva' else 'Assets/SuyuRun/Art/PixelSelva/ProductionV2/Resources/SelvaV2/selva_palette.json')
hexes=json.loads(palette.read_text())['colors']
if region=='selva':hexes=[c for c in hexes if c not in ('#C4E37A','#7FD4C9','#C67C73','#FFF0AE')]
colors=[tuple(bytes.fromhex(c[1:])) for c in hexes];assert len(colors)==24
if a.revision=='FinalMovieV4' and a.name=='sierra_hero':
    # Matte cloth/skin use only existing regional shades; no new palette is introduced.
    colors=[c for c in colors if c not in [tuple(bytes.fromhex(h)) for h in ('FFFFFF','E8EDF2','FFF3D0','F5DFA0')]]
im=Image.open(a.source).convert('RGBA');n=j['frames'];cols=j['columns'];rows=(n+cols-1)//cols
if j['transparent']:assert im.getchannel('A').getextrema()[0]==0,'Opaque source requires generated transparency correction'
w,h=(256,256) if j['kind']=='actor' else (2560,720)
out=Image.new('RGBA',(w*n,h));lut={};feet=[];grips=[]
isolated=None
if a.revision=='FinalMovieV4' and a.name=='selva_portrait':
    # Generated portraits have a large outside gutter, not four equal author columns.
    # Extract complete connected portraits, order by row/x, and register the torso.
    import numpy as np
    from scipy import ndimage
    rgba=np.array(im);labels,total=ndimage.label(rgba[:,:,3]>=128,structure=np.ones((3,3)))
    row_groups=[[] for _ in range(rows)]
    for label,box in enumerate(ndimage.find_objects(labels),1):
        if box is None or np.count_nonzero(labels[box]==label)<500:continue
        yy,xx=box;row=min(rows-1,int((yy.start+yy.stop)*.5/im.height*rows))
        row_groups[row].append((xx.start,label,box))
    portraits=[];anchors=[];bounds=[]
    for group in row_groups:
        assert len(group)==cols,('Portrait component row mismatch',len(group))
        for _,label,box in sorted(group):
            data=rgba.copy();data[:,:,3]=np.where(labels==label,data[:,:,3],0)
            obj=Image.fromarray(data);b=obj.getbbox();assert b
            band=(labels[max(b[1],b[3]-20):b[3],:]==label)
            anchor=float(np.nonzero(band)[1].mean())
            portraits.append(obj);anchors.append(anchor);bounds.append(b)
    left=max(x-b[0] for x,b in zip(anchors,bounds))+4
    right=max(b[2]-x for x,b in zip(anchors,bounds))+4
    height=max(b[3]-b[1] for b in bounds)+8
    isolated=[obj.crop((round(x-left),round(b[3]-height),round(x+right),b[3])) for obj,x,b in zip(portraits,anchors,bounds)]
if a.name.endswith('_life') or a.name.endswith('_memories') or a.name=='sierra_hero' or (a.revision=='FinalMovieV4' and (a.name.endswith('_hero') or a.name in ('portal_entry','costa_run'))):
    # Register whole connected sprites, including wings that exceed the nominal
    # grid row. This is extraction/alignment only, never painting missing pixels.
    import numpy as np
    from scipy import ndimage
    rgba=np.array(im);labels,total=ndimage.label(rgba[:,:,3]>=128,structure=np.ones((3,3)))
    boxes=ndimage.find_objects(labels);groups=[[] for _ in range(n)]
    for label,box in enumerate(boxes,1):
        if box is None:continue
        yy,xx=box;cx=(xx.start+xx.stop)/2;cy=(yy.start+yy.stop)/2
        index=min(rows-1,int(cy/im.height*rows))*cols+min(cols-1,int(cx/im.width*cols))
        if index<n:groups[index].append(label)
    isolated=[];bounds=[]
    for i,group in enumerate(groups):
        assert group,('No object in cell',a.name,i)
        data=rgba.copy();data[:,:,3]=np.where(np.isin(labels,group),data[:,:,3],0)
        obj=Image.fromarray(data);b=obj.getbbox();assert b;isolated.append(obj);bounds.append(b)
    cw=im.width/cols;ch=im.height/rows
    left=max(0,max(i%cols*cw-b[0] for i,b in enumerate(bounds)))
    right=max(0,max(b[2]-(i%cols+1)*cw for i,b in enumerate(bounds)))
    top=max(0,max(i//cols*ch-b[1] for i,b in enumerate(bounds)))
    bottom=max(0,max(b[3]-(i//cols+1)*ch for i,b in enumerate(bounds)))
    extracted=[]
    for i,obj in enumerate(isolated):
        extracted.append(obj.crop((round(i%cols*cw-left),round(i//cols*ch-top),round((i%cols+1)*cw+right),round((i//cols+1)*ch+bottom))))
    isolated=extracted
if a.name=='costa_run':
    # Mechanical rigid registration removes generated row gutters. Register the
    # head/pelvis cohort, never the lowest boot: flight must retain raised feet.
    anchors=[]
    for cell in isolated:
        b=cell.getbbox();top=cell.crop((0,b[1],cell.width,b[1]+round((b[3]-b[1])*.35))).getbbox()
        anchors.append((top[2],b[1]))
    left=max(x-cell.getbbox()[0] for cell,(x,y) in zip(isolated,anchors))+3
    right=max(cell.getbbox()[2]-x for cell,(x,y) in zip(isolated,anchors))+3
    depth=max(cell.getbbox()[3]-y for cell,(x,y) in zip(isolated,anchors))+6
    isolated=[cell.crop((x-left,y-3,x+right,y+depth)) for cell,(x,y) in zip(isolated,anchors)]
row_bounds=None
if j['kind']=='actor' and ('hero' in a.name or 'portrait' in a.name or a.name in ('portal_entry','costa_run')):
    alpha=im.getchannel('A').point(lambda v:255 if v>=128 else 0)
    bands=[];start=None
    for y in range(im.height+1):
        occupied=y<im.height and alpha.crop((0,y,im.width,y+1)).getbbox() is not None
        if occupied and start is None:start=y
        if not occupied and start is not None:bands.append((start,y));start=None
    assert len(bands)==rows,('Authored row count mismatch',a.name,bands)
    # Unequal outside margins must never split boots into the following row.
    cuts=[0]+[(bands[k-1][1]+bands[k][0])//2 for k in range(1,rows)]+[im.height]
    row_bounds=[(cuts[k],cuts[k+1]) for k in range(rows)]
for i in range(n):
    y0,y1=row_bounds[i//cols] if row_bounds else (round(i//cols*im.height/rows),round((i//cols+1)*im.height/rows))
    cell=isolated[i] if isolated else im.crop((round(i%cols*im.width/cols),y0,round((i%cols+1)*im.width/cols),y1))
    assert cell.getbbox(),f'Empty frame{i}'
    if isolated:
        scale=min(w/max(c.width for c in isolated),h/max(c.height for c in isolated))
        patch=cell.resize((max(1,round(cell.width*scale)),max(1,round(cell.height*scale))),Image.Resampling.NEAREST)
        cell=Image.new('RGBA',(w,h));cell.paste(patch,((w-patch.width)//2,0))
    elif row_bounds:
        scale=min(w/(im.width/cols),h/max(b-a for a,b in row_bounds))
        patch=cell.resize((max(1,round(cell.width*scale)),max(1,round(cell.height*scale))),Image.Resampling.NEAREST)
        cell=Image.new('RGBA',(w,h));cell.paste(patch,((w-patch.width)//2,0))
    else:cell=cell.resize((w,h),Image.Resampling.NEAREST)
    cell.putalpha(cell.getchannel('A').point(lambda v:255 if v>=128 else 0) if j['transparent'] else Image.new('L',(w,h),255))
    pixels=[]
    for v in cell.getdata():
        if not v[3]:pixels.append((0,0,0,0));continue
        rgb=v[:3]
        if rgb not in lut:lut[rgb]=min(colors,key=lambda c:sum((c[k]-rgb[k])**2 for k in range(3)))
        pixels.append((*lut[rgb],255))
    cell.putdata(pixels);out.paste(cell,(i*w,0))
    feet.append(cell.getbbox()[3]/h)
    grip=(.5,.1)
    if a.name=='selva_hero' and i<8:
        for y in range(h//2):
            xs=[x for x in range(w) if (lambda v:v[3] and v[0]>140 and 75<v[1]<210 and v[2]<130 and v[0]-v[1]>25)(cell.getpixel((x,y)))]
            if len(xs)>2:grip=((min(xs)+max(xs))*.5/w,(y+2)/h);break
    grips.append(grip)
dest=root/'Assets/SuyuRun/Art/RefactorRuntime/Resources'/a.revision;dest.mkdir(parents=True,exist_ok=True)
out.save(dest/(a.name+'.png'))
surface=next((y/h for y in range(h) if out.getpixel((w//2,y))[3]),.81) if a.name.endswith('_ground') else .81
(dest/(a.name+'.json')).write_text(json.dumps(dict(frames=n,frameWidth=w,frameHeight=h,fps=12,loop=False,surfaceY=surface,footY=feet,gripX=[g[0] for g in grips],gripY=[g[1] for g in grips],cinematicOnly=True,source=f'Artifacts/{a.revision}/Sources/{a.name}.png'),indent=2))
sources=root/'Artifacts'/a.revision/'Sources';sources.mkdir(parents=True,exist_ok=True);shutil.copy2(a.source,sources/(a.name+'.png'))
print(a.name,out.size,'exported; no original textures modified')
