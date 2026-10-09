"""Mechanical export of generated Selva-choreography / Costa-costume poses."""
from pathlib import Path
from PIL import Image
import json,sys,shutil,uuid
root=Path(__file__).resolve().parents[1];source=Path(sys.argv[1]);region=sys.argv[2] if len(sys.argv)>2 else 'costa'
shoot=len(sys.argv)>3 and sys.argv[3]=='shoot'
assert region in ('costa','sierra')
review=root/'Artifacts/VisualRefactor'/f'{region.title()}Air{ "Gun" if shoot else ""}'
review.mkdir(parents=True,exist_ok=True)
shutil.copy2(source,review/'generated_source.png')
im=Image.open(source).convert('RGBA');im.putalpha(im.getchannel('A').point(lambda a:255 if a>=128 else 0))
cells=[]
for i in range(5):
    cell=im.crop((round(i*im.width/5),0,round((i+1)*im.width/5),im.height))
    assert cell.getbbox(),'Missing pose'
    cells.append(cell.crop(cell.getbbox()))
scale=min(82/max(c.height for c in cells),180/max(c.width for c in cells))
if region=='sierra':
    # Raised hands must not shrink the head/torso: register the chullo-to-foot
    # extent rather than the silhouette's highest fingertip. One scale for all.
    heights=[]
    for c in cells:
        rows=[y for y in range(c.height//2) if sum(1 for x in range(c.width) if (lambda p:p[3]>0 and p[0]>70 and p[1]<p[0]*.55 and p[2]<p[0]*.5)(c.getpixel((x,y))))>c.width*.06]
        heights.append(c.height-(min(rows) if rows else 0))
    scale=min(82/max(heights),120/max(c.height for c in cells),180/max(c.width for c in cells))
colors=[tuple(bytes.fromhex(c[1:])) for c in json.loads((root/f'Assets/SuyuRun/Art/Pixel{region.title()}/{region}_palette.json').read_text())['colors']]
frames=[];lookup={}
for cell in cells:
    cell=cell.resize((round(cell.width*scale),round(cell.height*scale)),Image.Resampling.NEAREST)
    pixels=[]
    for p in cell.getdata():
        if not p[3]:pixels.append((0,0,0,0));continue
        if p[:3] not in lookup:lookup[p[:3]]=(*min(colors,key=lambda q:sum((q[k]-p[k])**2 for k in range(3))),255)
        pixels.append(lookup[p[:3]])
    cell.putdata(pixels)
    f=Image.new('RGBA',(192,128));f.alpha_composite(cell,((192-cell.width)//2,124-cell.height));frames.append(f)
out=root/f'Assets/SuyuRun/Art/RefactorRuntime/Resources/RefactorBank/Tayta{region.title()}'
for state,ids in {'jump':[0,1,2,3],'double_jump':[1,2,3],'fall':[3,4]}.items():
    sheet=Image.new('RGBA',(192*len(ids),128))
    for i,n in enumerate(ids):sheet.alpha_composite(frames[n],(192*i,0))
    path=out/f'{state}_{"shoot" if shoot else "selva_pose"}.png';sheet.save(path)
    spec=dict(frames=len(ids),fps=12,loop=False,frameWidth=192,frameHeight=128,pixelsPerUnit=64,footAnchor=[96,124],source=str(source),choreography=f'Selva jump reference; {region} costume; common scale',playTested=False)
    path.with_suffix('.json').write_text(json.dumps(spec,indent=2))
    path.with_suffix('.png.meta').write_text('fileFormatVersion: 2\nguid: '+uuid.uuid4().hex+'\nTextureImporter:\n  serializedVersion: 13\n  mipmaps:\n    enableMipMap: 0\n  textureSettings:\n    serializedVersion: 2\n    filterMode: 0\n    wrapU: 1\n    wrapV: 1\n  alphaIsTransparency: 1\n  textureType: 0\n  textureCompression: 0\n  maxTextureSize: 2048\n')
preview=Image.new('RGBA',(960,128),(30,23,31,255))
for i,f in enumerate(frames):preview.alpha_composite(f,(192*i,0))
preview.resize((1920,256),Image.Resampling.NEAREST).save(review/f'{region}_air_preview.png')
assert all(f.getbbox()[2]<191 and f.getbbox()[3]<=124 for f in frames)
print('PASS: five poses, common scale, 24-color palette, fixed foot anchor; three versioned runtime sheets')
