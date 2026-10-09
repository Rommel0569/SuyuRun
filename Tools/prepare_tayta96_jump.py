"""Native jump poses and state selections, without changing gameplay physics."""
from pathlib import Path
from PIL import Image
import json
from normalize_tayta_polish import isolate_character

root=Path(__file__).resolve().parents[1];out=root/'Artifacts/VisualRefactor/Tayta'
image=Image.open(out/'selva_jump_96_fuente.png').convert('RGBA')
image.putalpha(image.getchannel('A').point(lambda a:255 if a>=128 else 0))
palette=json.loads((root/'Assets/SuyuRun/Art/PixelSelva/ProductionV2/Resources/SelvaV2/selva_palette.json').read_text())['colors']
palette=[c for c in palette if c not in ('#4A7A3E','#6FA83D','#96C954','#C4E37A')]
colors=[tuple(bytes.fromhex(c[1:])) for c in palette]
cells=[]
for i in range(8):
    col=i%4;row=i//4
    cell=image.crop((round(col*image.width/4),round(row*image.height/2),round((col+1)*image.width/4),round((row+1)*image.height/2)))
    cell=isolate_character(cell);cells.append(cell.crop(cell.getbbox()))
# One common scale: compressed landing and tucked jump retain their proportions.
scale=min(85/max(c.height for c in cells),88/max(c.width for c in cells))
frames=[];bounds=[];atlas=Image.new('RGBA',(768,96))
for i,cell in enumerate(cells):
    cell=cell.resize((round(cell.width*scale),round(cell.height*scale)),Image.Resampling.NEAREST)
    cell.putdata([(*min(colors,key=lambda c:sum((c[k]-p[k])**2 for k in range(3))),255) if p[3] else (0,0,0,0) for p in cell.getdata()])
    frame=Image.new('RGBA',(96,96));frame.alpha_composite(cell,((96-cell.width)//2,93-cell.height))
    frames.append(frame);bounds.append(frame.getbbox());atlas.alpha_composite(frame,(i*96,0))
atlas.save(out/'selva_jump_96.png')
selections={'jump':[0,1,2,3,5],'double_jump':[4,3,5],'fall':[5],'land':[6,7]}
for state,order in selections.items():
    spec=dict(frames=len(order),fps=12,loop=False,frameWidth=96,frameHeight=96,pixelsPerUnit=64,gripCoordinateScale=.5,frameOrder=order)
    (out/f'selva_{state}_96.json').write_text(json.dumps(spec,indent=2))
preview=Image.new('RGBA',(384,192))
for i,f in enumerate(frames):preview.alpha_composite(f,(i%4*96,i//4*96))
preview.resize((1536,768),Image.Resampling.NEAREST).save(out/'selva_jump_96_preview_x4.png')
# Review-only arc. Actual trajectories remain driven by existing game physics.
views=[]
for i,f in enumerate(frames):
    canvas=Image.new('RGBA',(112,160),(33,28,45,255))
    height=[0,18,35,46,52,30,0,0][i]
    canvas.alpha_composite(f,(8,58-height))
    views.append(canvas.resize((336,480),Image.Resampling.NEAREST))
views[0].save(out/'selva_jump_96.gif',save_all=True,append_images=views[1:],duration=[120,90,100,140,110,120,100,180],loop=0,disposal=2)
report=dict(frames=8,colors=len({p[:3] for p in atlas.getdata() if p[3]}),bounds=bounds,alpha=sorted(set(atlas.getchannel('A').getdata())),uniquePoses=len({f.tobytes() for f in frames}),runtimeIntegrated=False)
assert report['colors']<=24 and report['alpha']==[0,255] and report['uniquePoses']==8,report
assert all(b[0]>=2 and b[1]>=2 and b[2]<=94 and b[3]<=94 for b in bounds),bounds
(out/'jump_validation.json').write_text(json.dumps(report,indent=2));print(json.dumps(report))
