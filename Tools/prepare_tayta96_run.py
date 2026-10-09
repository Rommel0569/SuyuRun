"""Normalize the image-generated 4x2 running sheet without runtime replacement."""
from pathlib import Path
from PIL import Image
import json, statistics
from normalize_tayta_polish import isolate_character

root=Path(__file__).resolve().parents[1];out=root/'Artifacts/VisualRefactor/Tayta'
image=Image.open(out/'selva_run_96_fuente.png').convert('RGBA')
image.putalpha(image.getchannel('A').point(lambda a:255 if a>=128 else 0))
palette=json.loads((root/'Assets/SuyuRun/Art/PixelSelva/ProductionV2/Resources/SelvaV2/selva_palette.json').read_text())['colors']
palette=[c for c in palette if c not in ('#4A7A3E','#6FA83D','#96C954','#C4E37A')]
colors=[tuple(bytes.fromhex(c[1:])) for c in palette]
cells=[]
for i in range(8):
    col=i%4;row=i//4
    cell=image.crop((round(col*image.width/4),round(row*image.height/2),round((col+1)*image.width/4),round((row+1)*image.height/2)))
    cell=isolate_character(cell);cells.append(cell.crop(cell.getbbox()))
# Common scale for the full animation, never enlarge a crouched/airborne frame.
scale=min(83/max(c.height for c in cells),88/max(c.width for c in cells))
atlas=Image.new('RGBA',(768,96));frames=[];bounds=[]
for i,cell in enumerate(cells):
    cell=cell.resize((round(cell.width*scale),round(cell.height*scale)),Image.Resampling.NEAREST)
    cell.putdata([(*min(colors,key=lambda c:sum((c[k]-p[k])**2 for k in range(3))),255) if p[3] else (0,0,0,0) for p in cell.getdata()])
    # Register the chullo/face, not the varying silhouette of outstretched legs.
    head=[(x,y) for y in range(min(36,cell.height)) for x in range(cell.width) if cell.getpixel((x,y))[:3] in ((139,32,32),(193,56,56),(232,93,58)) and cell.getpixel((x,y))[3]]
    anchor=statistics.median([p[0] for p in head]) if head else cell.width/2
    left=max(3,min(93-cell.width,round(59-anchor)))
    frame=Image.new('RGBA',(96,96));frame.alpha_composite(cell,(left,10 if i not in (3,7) else 8))
    assert frame.getbbox()[3]<=94,frame.getbbox()
    frames.append(frame);bounds.append(frame.getbbox());atlas.alpha_composite(frame,(i*96,0))
atlas.save(out/'selva_run_96.png')
preview=Image.new('RGBA',(384,192))
for i,f in enumerate(frames):preview.alpha_composite(f,(i%4*96,i//4*96))
preview.resize((1536,768),Image.Resampling.NEAREST).save(out/'selva_run_96_preview_x4.png')
views=[]
for f in frames:
    canvas=Image.new('RGBA',(96,96),(33,28,45,255));canvas.alpha_composite(f);views.append(canvas.resize((384,384),Image.Resampling.NEAREST))
views[0].save(out/'selva_run_96.gif',save_all=True,append_images=views[1:],duration=70,loop=0,disposal=2)
(out/'selva_run_96.json').write_text(json.dumps(dict(frames=8,fps=14,loop=True,frameWidth=96,frameHeight=96,pixelsPerUnit=64,gripCoordinateScale=.5),indent=2))
report=dict(frames=8,colors=len({p[:3] for p in atlas.getdata() if p[3]}),bounds=bounds,alpha=sorted(set(atlas.getchannel('A').getdata())),uniquePoses=len({f.tobytes() for f in frames}),runtimeIntegrated=False)
assert report['colors']<=24 and report['alpha']==[0,255] and report['uniquePoses']==8,report
assert all(b[0]>=2 and b[1]>=2 and b[2]<=94 and b[3]<=94 for b in bounds),bounds
(out/'run_validation.json').write_text(json.dumps(report,indent=2));print(json.dumps(report))
