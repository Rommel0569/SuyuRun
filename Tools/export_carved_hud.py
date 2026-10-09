"""Segment generated HUD atlas; preserve source, normalize palette and Point imports."""
from pathlib import Path
from PIL import Image
import json, sys, shutil, uuid
root=Path(__file__).resolve().parents[1]
source=Path(sys.argv[1]);out=root/'Assets/SuyuRun/Art/RefactorRuntime/Resources/HudCarved'
out.mkdir(parents=True,exist_ok=True)
review=root/'Artifacts/VisualRefactor/HudCarved';review.mkdir(parents=True,exist_ok=True)
if source.resolve()!=(review/'generated_source.png').resolve():shutil.copy2(source,review/'generated_source.png')
im=Image.open(source).convert('RGBA')
im.putalpha(im.getchannel('A').point(lambda a:255 if a>=128 else 0))
bands=[];start=None
for y in range(im.height):
    occupied=sum(im.getpixel((x,y))[3]>0 for x in range(im.width))>im.width*.35
    if occupied and start is None:start=y
    if not occupied and start is not None:
        if y-start>10:bands.append((start,y))
        start=None
if start is not None:bands.append((start,im.height))
assert len(bands)==6,('Expected six separate panels',bands)
manifest=[]
for i,(top,bottom) in enumerate(bands):
    region=['costa','sierra','selva'][i//2];kind='header' if i%2==0 else 'footer'
    c=im.crop((0,top,im.width,bottom));c=c.crop(c.getbbox()).resize((1248,80),Image.Resampling.NEAREST)
    palette=json.loads((root/f'Assets/SuyuRun/Art/Pixel{region.title()}/{region}_palette.json').read_text())['colors']
    colors=[tuple(bytes.fromhex(v[1:])) for v in palette]
    lookup={}
    def quantize(p):
        if not p[3]:return (0,0,0,0)
        key=p[:3]
        if key not in lookup:lookup[key]=(*min(colors,key=lambda q:sum((q[k]-p[k])**2 for k in range(3))),255)
        return lookup[key]
    c.putdata([quantize(p) for p in c.getdata()]);path=out/f'{region}_{kind}_v2.png';c.save(path)
    # Texture (not sprite) is intentionally loaded through Resources by IMGUI.
    path.with_suffix('.png.meta').write_text('fileFormatVersion: 2\nguid: '+uuid.uuid4().hex+'\nTextureImporter:\n  serializedVersion: 13\n  mipmaps:\n    enableMipMap: 0\n  isReadable: 0\n  textureType: 0\n  textureSettings:\n    serializedVersion: 2\n    filterMode: 0\n    wrapU: 1\n    wrapV: 1\n    wrapW: 1\n  alphaIsTransparency: 1\n  textureCompression: 0\n  maxTextureSize: 2048\n')
    assert len({p[:3] for p in c.getdata() if p[3]})<=24
    manifest.append(dict(region=region,kind=kind,path=str(path),size=c.size,sourceBand=[top,bottom]))
preview=Image.new('RGBA',(1248,6*88),(30,23,31,255))
for i,item in enumerate(manifest):preview.alpha_composite(Image.open(item['path']).resize((1248,80),Image.Resampling.NEAREST),(0,i*88))
preview.save(review/'hud_preview_v2.png')
(review/'manifest_v2.json').write_text(json.dumps(manifest,indent=2))
print(json.dumps(manifest,indent=2))
