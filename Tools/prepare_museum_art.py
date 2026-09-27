"""Normalize the generated museum atlas to the existing pixel/palette contract.
User authorized this whole batch and Pillow pixel processing. No previous PNG overwritten.
"""
from pathlib import Path
from PIL import Image, ImageOps
import json, shutil, sys

root=Path(__file__).resolve().parents[1]
out=root/'Assets/SuyuRun/Art/SharedMenu/Resources/Museum'
review=root/'Artifacts/MuseumMenu';out.mkdir(parents=True,exist_ok=True);review.mkdir(parents=True,exist_ok=True)
source=Path(sys.argv[1]);shutil.copy2(source,review/'museum_source_atlas.png')
atlas=Image.open(source).convert('RGBA')
ids=['huaco_moche','textil_paracas','nicomedes','maria_reiche','cabeza_clava','kero','quipu']
report={};sheet=Image.new('RGBA',(4*80,2*80),(33,28,45,255))
for i,name in enumerate(ids):
    region='Costa' if i<4 else 'Sierra'
    colors=json.loads((root/f'Assets/SuyuRun/Art/Pixel{region}/{region.lower()}_palette.json').read_text(encoding='utf-8'))['colors']
    rgb=[tuple(bytes.fromhex(c[1:])) for c in colors]
    x,y=i%4,i//4;w,h=atlas.size
    tile=atlas.crop((round(x*w/4),round(y*h/2),round((x+1)*w/4),round((y+1)*h/2)))
    alpha=tile.getchannel('A').point(lambda a:255 if a>=128 else 0)
    bounds=alpha.getbbox();assert bounds,name
    tile=tile.crop(bounds)
    tile=ImageOps.contain(tile,(60,60),Image.Resampling.NEAREST)
    frame=Image.new('RGBA',(64,64));frame.paste(tile,((64-tile.width)//2,(64-tile.height)//2))
    pixels=[]
    for r,g,b,a in frame.getdata():
        if a<128:pixels.append((0,0,0,0));continue
        color=min(rgb,key=lambda c:(c[0]-r)**2+(c[1]-g)**2+(c[2]-b)**2)
        pixels.append((*color,255))
    frame.putdata(pixels);frame.save(out/f'{name}.png')
    sheet.alpha_composite(frame,(x*80+8,y*80+8))
    used={c[:3] for c in pixels if c[3]};assert used.issubset(set(rgb)) and len(used)<=24
    report[name]={'size':[64,64],'colors':len(used),'palette':region,'binary_alpha':True,'slot':i}
sheet.resize((1280,640),Image.Resampling.NEAREST).save(review/'museum_pieces_x4.png')
(review/'art_validation.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
print(json.dumps(report,indent=2))
