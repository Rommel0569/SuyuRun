"""Export the single image-generated idle asset without replacing runtime art."""
from pathlib import Path
from PIL import Image
import json
from normalize_tayta_polish import isolate_character

root=Path(__file__).resolve().parents[1]
out=root/'Artifacts/VisualRefactor/Tayta'
image=Image.open(out/'selva_idle_96_fuente.png').convert('RGBA')
image.putalpha(image.getchannel('A').point(lambda a:255 if a>=128 else 0))
cells=[]
for i in range(4):
    cell=image.crop((round(i*image.width/4),0,round((i+1)*image.width/4),image.height))
    cell=isolate_character(cell)
    cells.append(cell.crop(cell.getbbox()))
scale=min(86/max(c.height for c in cells),88/max(c.width for c in cells))
palette=json.loads((root/'Assets/SuyuRun/Art/PixelSelva/ProductionV2/Resources/SelvaV2/selva_palette.json').read_text())['colors']
# Existing regional colors only; discard four vegetation shades for this sprite.
palette=[c for c in palette if c not in ('#4A7A3E','#6FA83D','#96C954','#C4E37A')]
colors=[tuple(bytes.fromhex(c[1:])) for c in palette]
atlas=Image.new('RGBA',(384,96));frames=[];bounds=[]
for i,cell in enumerate(cells):
    cell=cell.resize((round(cell.width*scale),round(cell.height*scale)),Image.Resampling.NEAREST)
    cell.putdata([(*min(colors,key=lambda c:sum((c[k]-p[k])**2 for k in range(3))),255) if p[3] else (0,0,0,0) for p in cell.getdata()])
    frame=Image.new('RGBA',(96,96));frame.alpha_composite(cell,((96-cell.width)//2,93-cell.height))
    frames.append(frame);bounds.append(frame.getbbox());atlas.alpha_composite(frame,(i*96,0))
atlas.save(out/'selva_idle_96.png')
atlas.resize((1536,384),Image.Resampling.NEAREST).save(out/'selva_idle_96_preview_x4.png')
views=[]
for frame in frames:
    canvas=Image.new('RGBA',(96,96),(33,28,45,255));canvas.alpha_composite(frame)
    views.append(canvas.resize((384,384),Image.Resampling.NEAREST))
views[0].save(out/'selva_idle_96.gif',save_all=True,append_images=views[1:],duration=[240,240,120,240],loop=0,disposal=2)
spec=dict(frames=4,fps=5,loop=True,frameWidth=96,frameHeight=96,pixelsPerUnit=64,gripCoordinateScale=.5)
(out/'selva_idle_96.json').write_text(json.dumps(spec,indent=2))
assert all(b and b[0]>=2 and b[1]>=2 and b[2]<=94 and b[3]<=94 for b in bounds),bounds
assert len({p[:3] for p in atlas.getdata() if p[3]})<=24
assert set(atlas.getchannel('A').getdata())<={0,255}
(out/'validation.json').write_text(json.dumps(dict(size=atlas.size,bounds=bounds,colors=len({p[:3] for p in atlas.getdata() if p[3]}),alpha='binary',runtimeIntegrated=False),indent=2))
print('4 frames, 96x96, <=24 regional colors, bounds/alpha PASS; pending approval')
