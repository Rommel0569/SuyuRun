"""Mechanical registration of independently redrawn layers to existing contracts."""
from pathlib import Path
import argparse,json,shutil
from PIL import Image
root=Path(__file__).resolve().parents[1]
p=argparse.ArgumentParser();p.add_argument('name');p.add_argument('source',type=Path);p.add_argument('--output-folder',default='LayerPolish');p.add_argument('--density',type=int,choices=(2,4),default=2);a=p.parse_args()
bank=root/'Assets/SuyuRun/Art/RefactorRuntime/Resources/RefactorBank'
oldspec=next((bank/folder/(a.name+'.json') for folder in ('Environment','Materials') if (bank/folder/(a.name+'.json')).exists()),None)
if oldspec:
    spec=json.loads(oldspec.read_text());oldpath=oldspec.with_suffix('.png')
else:
    oldpath=root/f'Assets/SuyuRun/Art/PixelSierra/ProductionV3/Resources/SierraV3/{a.name}.png'
    oldspec=oldpath.with_suffix('.sprite.json')
    meta=json.loads(oldspec.read_text());assert meta['count']==1
    spec=dict(frames=1,frameWidth=meta['width']*2,frameHeight=meta['height']*2,logicalWidth=meta['width'],logicalHeight=meta['height'],pixelsPerUnit=64,fps=meta['fps'],loop=meta['loop'],original=oldpath.relative_to(root).as_posix())
assert spec['frames']==1,'Animated sheets require frame-preserving export, not a static layer resize'
spec.update(frameWidth=spec['logicalWidth']*a.density,frameHeight=spec['logicalHeight']*a.density,pixelsPerUnit=32*a.density)
old=Image.open(oldpath).convert('RGBA')
if old.size!=(spec['frameWidth'],spec['frameHeight']):old=old.resize((spec['frameWidth'],spec['frameHeight']),Image.Resampling.NEAREST)
region=a.name.split('_')[0]
palette=root/(f'Assets/SuyuRun/Art/Pixel{region.title()}/{region}_palette.json' if region!='selva' else 'Assets/SuyuRun/Art/PixelSelva/ProductionV2/Resources/SelvaV2/selva_palette.json')
hexes=json.loads(palette.read_text())['colors']
if region=='selva':hexes=[c for c in hexes if c not in ('#C4E37A','#7FD4C9','#C67C73','#FFF0AE')]
colors=[tuple(bytes.fromhex(c[1:])) for c in hexes];assert len(colors)==24
if a.name in ('sierra_camino_capa7_suelo','sierra_valle_capa7_suelo'):
    # Soil uses the earth/stone/ichu subset, not textile crimson.
    colors=[c for c in colors if c not in ((139,46,46),(185,74,61))]
im=Image.open(a.source).convert('RGBA');im.putalpha(im.getchannel('A').point(lambda v:255 if v>=128 else 0))
opaque=old.getchannel('A').getextrema()==(255,255)
if opaque:
    # Remove identical solid letterbox strips introduced by a different generator
    # aspect ratio. Never crop ordinary sky content or a single tonal band.
    first=list(im.crop((0,0,im.width,1)).getdata());last=list(im.crop((0,im.height-1,im.width,im.height)).getdata())
    if len(set(first))==1 and first==last:
        top=0;bottom=im.height
        while top<im.height//5 and set(im.crop((0,top,im.width,top+1)).getdata())==set(first):top+=1
        while bottom>im.height*4//5 and set(im.crop((0,bottom-1,im.width,bottom)).getdata())==set(first):bottom-=1
        if top and bottom<im.height:im=im.crop((0,top,im.width,bottom))
    elif '_capa1_cielo' in a.name:
        def blank(y):return all(max(v[:3])<45 or not v[3] for v in im.crop((0,y,im.width,y+1)).getdata())
        if blank(0) and blank(im.height-1):
            top=0;bottom=im.height
            while top<im.height//5 and blank(top):top+=1
            while bottom>im.height*4//5 and blank(bottom-1):bottom-=1
            if top and bottom<im.height:im=im.crop((0,top,im.width,bottom))
    dest=im.resize(old.size,Image.Resampling.NEAREST)
else:
    b=old.getbbox();s=im.getbbox();assert b and s
    dest=Image.new('RGBA',old.size)
    content=im.crop(s).resize((b[2]-b[0],b[3]-b[1]),Image.Resampling.NEAREST)
    dest.alpha_composite(content,(b[0],b[1]))
preserveMask='_capa7_' in a.name or '_tile_' in a.name or a.name in ('sierra_cueva_suelo','sierra_cueva_roca','sierra_cueva_plataforma','selva_niebla','selva_rayos_sol')
if preserveMask:
    # Mechanical alpha registration keeps every walkable edge identical.
    fallback=Image.composite(dest,old,dest.getchannel('A'))
    fallback.putalpha(old.getchannel('A'));dest=fallback
lut={};pixels=[]
for v in dest.getdata():
    if not v[3]:pixels.append((0,0,0,0));continue
    if v[:3] not in lut:lut[v[:3]]=min(colors,key=lambda q:sum((q[k]-v[k])**2 for k in range(3)))
    pixels.append((*lut[v[:3]],v[3] if preserveMask else 255))
dest.putdata(pixels)
if not opaque and not preserveMask:dest.paste(dest.crop((0,0,1,dest.height)),(dest.width-1,0))
out=bank/a.output_folder;out.mkdir(parents=True,exist_ok=True)
dest.save(out/(a.name+'.png'))
spec.update(source=f'Artifacts/VisualRefactor/{a.output_folder}/Sources/{a.name}.png',previous=oldspec.relative_to(root).as_posix(),finish='independent imagegen redraw; native clusters',fullPlaythroughTested=False,seamEndpointEqual=not opaque and not preserveMask,originalAlphaMaskPreserved=preserveMask)
(out/(a.name+'.json')).write_text(json.dumps(spec,indent=2))
sources=root/'Artifacts/VisualRefactor'/a.output_folder/'Sources';sources.mkdir(parents=True,exist_ok=True)
if a.source.resolve()!=(sources/(a.name+'.png')).resolve():shutil.copy2(a.source,sources/(a.name+'.png'))
print(a.name,dest.size,len({v[:3] for v in dest.getdata() if v[3]}),'colors')
