"""Register generated independent cave arches without changing logical geometry.
Pillow normalization explicitly authorized by the project's art bible.
Original PNGs are never overwritten. Generated source is retained in Artifacts.
"""
from pathlib import Path
import argparse,json,hashlib
from PIL import Image

root=Path(__file__).resolve().parents[1]
parser=argparse.ArgumentParser()
parser.add_argument('source',type=Path)
parser.add_argument('--index',type=int,choices=[1,2])
parser.add_argument('--name',choices=['sierra_camino_capa4_paisaje','sierra_valle_capa4_paisaje','selva_rio_capa3_estructura','selva_noche_capa3_estructura','selva_cueva_capa3_estructura','selva_plataforma_tronco'])
args=parser.parse_args()
assert bool(args.index)!=bool(args.name), 'Choose index or name'
name=args.name or f'sierra_cueva{args.index}_arcos'
region=name.split('_')[0]
folder='PixelSierra/ProductionV3/Resources/SierraV3' if region=='sierra' else 'PixelSelva/ProductionV2/Resources/SelvaV2'
original=root/f'Assets/SuyuRun/Art/{folder}/{name}.png'
before=hashlib.sha256(original.read_bytes()).hexdigest()
source=Image.open(args.source).convert('RGBA')
retained=root/f'Artifacts/VisualRefactor/{name}_detail_source.png'
source.save(retained)
old=Image.open(original).convert('RGBA')
# Each arch occupies the same old canvas; align the entire independent layer.
old_bounds=old.getbbox();source_bounds=source.getbbox()
assert old_bounds and source_bounds
registered=old.resize((old.width*2,old.height*2),Image.Resampling.NEAREST)
content=source.crop(source_bounds).resize(((old_bounds[2]-old_bounds[0])*2,(old_bounds[3]-old_bounds[1])*2),Image.Resampling.NEAREST)
registered.alpha_composite(content,(old_bounds[0]*2,old_bounds[1]*2))
alpha=old.getchannel('A').resize(registered.size,Image.Resampling.NEAREST)
# Keep the approved cut-out silhouette exactly: no extra collision-looking stone.
registered.putalpha(alpha)
palette=['#1A2036','#2B3A5C','#3E6485','#5F8FA8','#9BC4C6','#E8EDF2','#FFF3D0','#F5DFA0','#D8B46B','#AD7559','#835746','#5B4A43','#4C5553','#788078','#B8B8A2','#D9CFAD','#263D2E','#3E5940','#648346','#9EAD56','#CBBD61','#8B2E2E','#C2674F','#ED9254']
colors=[tuple(bytes.fromhex(c[1:])) for c in palette]
if region=='selva':
    palette=json.loads((root/'Assets/SuyuRun/Art/PixelSelva/ProductionV2/Resources/SelvaV2/selva_palette.json').read_text())['colors']
    palette=[c for c in palette if c not in ('#C4E37A','#7FD4C9','#C67C73','#FFF0AE')]
    colors=[tuple(bytes.fromhex(c[1:])) for c in palette]
assert len(colors)==24
lookup={}
def quantize(p):
    if p[3]<128:return (0,0,0,0)
    rgb=p[:3]
    if rgb not in lookup:lookup[rgb]=min(colors,key=lambda q:sum((q[i]-rgb[i])**2 for i in range(3)))
    return (*lookup[rgb],255)
registered.putdata([quantize(p) for p in registered.getdata()])
seamless=not name.startswith('selva_plataforma_')
if seamless:registered.paste(registered.crop((0,0,1,registered.height)),(registered.width-1,0))
out=root/'Assets/SuyuRun/Art/RefactorRuntime/Resources/RefactorBank/Environment'
out.mkdir(parents=True,exist_ok=True)
registered.save(out/(name+'.png'))
spec=dict(frames=1,frameWidth=registered.width,frameHeight=registered.height,logicalWidth=old.width,logicalHeight=old.height,pixelsPerUnit=64,fps=1,loop=True,source=retained.relative_to(root).as_posix(),original=original.relative_to(root).as_posix(),seamEndpointEqual=seamless,fullPlaythroughTested=False)
(out/(name+'.json')).write_text(json.dumps(spec,indent=2),encoding='utf-8')
assert registered.getchannel('A').tobytes()==alpha.tobytes(), 'Alpha silhouette changed'
assert len({p[:3] for p in registered.getdata() if p[3]})<=24
assert hashlib.sha256(original.read_bytes()).hexdigest()==before
print(name,registered.size,'registered, palette <=24, unchanged original and silhouette')
