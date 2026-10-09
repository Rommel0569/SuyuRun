"""Framewise registration of generated animation grids; preserve source alpha."""
from pathlib import Path
import argparse,json,shutil
from PIL import Image
root=Path(__file__).resolve().parents[1]
p=argparse.ArgumentParser();p.add_argument('name');p.add_argument('source',type=Path);p.add_argument('--columns',type=int,required=True);p.add_argument('--output-folder',default='LayerPolish');p.add_argument('--density',type=int,choices=(2,4),default=2);a=p.parse_args()
bank=root/'Assets/SuyuRun/Art/RefactorRuntime/Resources/RefactorBank'
meta=next(bank/folder/(a.name+'.json') for folder in ('Environment','Materials') if (bank/folder/(a.name+'.json')).exists())
spec=json.loads(meta.read_text());old=Image.open(meta.with_suffix('.png')).convert('RGBA')
spec.update(frameWidth=spec['logicalWidth']*a.density,frameHeight=spec['logicalHeight']*a.density,pixelsPerUnit=32*a.density)
old=old.resize((spec['frameWidth']*spec['frames'],spec['frameHeight']),Image.Resampling.NEAREST)
region=a.name.split('_')[0]
palette=root/(f'Assets/SuyuRun/Art/Pixel{region.title()}/{region}_palette.json' if region!='selva' else 'Assets/SuyuRun/Art/PixelSelva/ProductionV2/Resources/SelvaV2/selva_palette.json')
hexes=json.loads(palette.read_text())['colors']
if region=='selva':hexes=[c for c in hexes if c not in ('#C4E37A','#7FD4C9','#C67C73','#FFF0AE')]
colors=[tuple(bytes.fromhex(c[1:])) for c in hexes];assert len(colors)==24
src=Image.open(a.source).convert('RGBA');n=spec['frames'];cols=a.columns;rows=(n+cols-1)//cols
if a.name=='costa_capa2_aves':
    # The generated grid includes exterior blank rows; do not count that
    # padding as an animation cell or split wings between species rows.
    alpha=src.getchannel('A').point(lambda v:255 if v>=128 else 0);bounds=alpha.getbbox();assert bounds
    src=src.crop((0,bounds[1],src.width,bounds[3]))
w,h=spec['frameWidth'],spec['frameHeight'];sheet=Image.new('RGBA',(w*n,h));lut={}
for i in range(n):
    x=i%cols;y=i//cols
    cell=src.crop((round(x*src.width/cols),round(y*src.height/rows),round((x+1)*src.width/cols),round((y+1)*src.height/rows)))
    original=old.crop((i*w,0,(i+1)*w,h));b=original.getbbox();assert b
    dest=Image.new('RGBA',(w,h))
    if a.name=='costa_capa2_aves':
        cell.putalpha(cell.getchannel('A').point(lambda v:255 if v>=128 else 0));bbox=cell.getbbox();assert bbox
        cell=cell.crop(bbox)
        # Shared scale for all flight poses prevents varying species/body width.
        span=26*a.density/2
        scale=min(span/(src.width/cols),span/(src.height/rows))
        cell=cell.resize((max(1,round(cell.width*scale)),max(1,round(cell.height*scale))),Image.Resampling.NEAREST)
        dest.alpha_composite(cell,((w-cell.width)//2,(h-cell.height)//2))
    elif a.name=='sierra_decor_arbol_queuna':
        cell.putalpha(cell.getchannel('A').point(lambda v:255 if v>=128 else 0))
        # Use one scale for all poses and keep the trunk base fixed. The new
        # canopy must NOT be clipped into the old geometric foliage mask.
        boxes=[]
        for j in range(n):
            ref=src.crop((round((j%cols)*src.width/cols),round((j//cols)*src.height/rows),round((j%cols+1)*src.width/cols),round((j//cols+1)*src.height/rows)))
            boxes.append(ref.getchannel('A').point(lambda v:255 if v>=128 else 0).getbbox())
        scale=min((w-8)/max(q[2]-q[0] for q in boxes),(h-8)/max(q[3]-q[1] for q in boxes))
        cell=cell.crop(cell.getbbox())
        cell=cell.resize((max(1,round(cell.width*scale)),max(1,round(cell.height*scale))),Image.Resampling.NEAREST)
        dest.alpha_composite(cell,((w-cell.width)//2,h-4-cell.height))
    else:
        # Trim blank panel padding, including opaque black generated mattes.
        mask=Image.new('L',cell.size)
        mask.putdata([255 if v[3]>=128 and max(v[:3])>28 else 0 for v in cell.getdata()])
        cb=mask.getbbox();assert cb;cell=cell.crop(cb)
        if a.name in ('selva_agua_fondo','selva_agua_superficie'):
            # Runtime samples the bottom 34 logical rows. Keep the authored
            # surface band there instead of stretching foam over all 180 rows.
            dest=Image.new('RGBA',(w,h),cell.getpixel((cell.width//2,cell.height-1)))
            patch=cell.resize((w,34*a.density),Image.Resampling.NEAREST)
            dest.paste(patch,(0,h-patch.height))
        elif a.name=='sierra_valle_capa5_rio':
            # One authored ribbon master registered to each original river,
            # rather than stretching across the empty gap between two ribbons.
            xs=[x for x in range(w) if original.getchannel('A').crop((x,0,x+1,h)).getbbox()]
            groups=[];start=last=xs[0]
            for x in xs[1:]:
                if x>last+1:groups.append((start,last+1));start=x
                last=x
            groups.append((start,last+1))
            for x0,x1 in groups:
                rb=original.crop((x0,0,x1,h)).getbbox()
                patch=cell.resize((x1-x0,rb[3]-rb[1]),Image.Resampling.NEAREST)
                dest.paste(patch,(x0,rb[1]))
        else:
            cell=cell.resize((b[2]-b[0],b[3]-b[1]),Image.Resampling.NEAREST)
            dest.paste(cell,(b[0],b[1]))
        dest.putalpha(original.getchannel('A'))
    pix=[]
    for v in dest.getdata():
        if not v[3]:pix.append((0,0,0,0));continue
        if v[:3] not in lut:lut[v[:3]]=min(colors,key=lambda q:sum((q[k]-v[k])**2 for k in range(3)))
        pix.append((*lut[v[:3]],v[3]))
    dest.putdata(pix);sheet.alpha_composite(dest,(i*w,0))
out=bank/a.output_folder;out.mkdir(parents=True,exist_ok=True);sheet.save(out/(a.name+'.png'))
spec.update(previous=meta.relative_to(root).as_posix(),source=f'Artifacts/VisualRefactor/{a.output_folder}/Sources/{a.name}.png',finish='independent imagegen framewise redraw',fullPlaythroughTested=False)
if a.name not in ('costa_capa2_aves','sierra_decor_arbol_queuna'):spec['alphaMode']='preserveSource'
(out/(a.name+'.json')).write_text(json.dumps(spec,indent=2))
sources=root/'Artifacts/VisualRefactor'/a.output_folder/'Sources';sources.mkdir(parents=True,exist_ok=True)
if a.source.resolve()!=(sources/(a.name+'.png')).resolve():shutil.copy2(a.source,sources/(a.name+'.png'))
print(a.name,'exported',n,'frames',sheet.size)
