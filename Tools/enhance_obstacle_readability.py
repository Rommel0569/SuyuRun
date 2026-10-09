"""Versioned high-contrast hazard trims; original materials and hitboxes untouched."""
from pathlib import Path
import json
from PIL import Image, ImageDraw
import build_sierra_artpack as sierra
root=Path(__file__).resolve().parents[1]
bank=root/'Assets/SuyuRun/Art/RefactorRuntime/Resources/RefactorBank'
out=bank/'Readability';out.mkdir(exist_ok=True)
names=['sierra_pinchos','sierra_tronco','sierra_piedra','selva_obstaculo_techo','selva_obstaculo_raiz','selva_obstaculo_pinchos','selva_obstaculo_estaca','selva_obstaculo_trampa']
for name in names:
    spec=json.loads((bank/'Materials'/(name+'.json')).read_text())
    source=Image.open(bank/'Materials'/(name+'.png')).convert('RGBA')
    palette=sierra.HEX if name.startswith('sierra') else json.loads((root/'Assets/SuyuRun/Art/PixelSelva/ProductionV2/Resources/SelvaV2/selva_palette.json').read_text())['colors']
    if name.startswith('selva'):palette=[c for c in palette if c not in ('#C4E37A','#7FD4C9','#C67C73','#FFF0AE')]
    colors=[tuple(bytes.fromhex(c[1:]))+(255,) for c in palette]
    nearest=lambda rgb:min(colors,key=lambda c:sum((c[k]-rgb[k])**2 for k in range(3)))
    gold,red,ink=nearest((245,190,60)),nearest((205,77,46)),nearest((24,28,38))
    w,h=spec['frameWidth'],spec['frameHeight'];count=max(6,spec['frames'])
    sheet=Image.new('RGBA',(w*count,h))
    for f in range(count):
        cell=source.crop(((f%spec['frames'])*w,0,(f%spec['frames']+1)*w,h));alpha=cell.getchannel('A');p=cell.load()
        for x in range(w):
            ys=[y for y in range(h) if alpha.getpixel((x,y))]
            if not ys:continue
            top=ys[0]
            for dy in (1,2,3):
                if top+dy<h and alpha.getpixel((x,top+dy)):p[x,top+dy]=gold if (x//6)%2==0 else red
            # A small moving glint across the marked lip, never a full-screen flash.
            if abs(x-(f*w/count))<2 and top+4<h and alpha.getpixel((x,top+4)):p[x,top+4]=gold
        cell.putalpha(alpha);sheet.paste(cell,(f*w,0))
    sheet.save(out/(name+'.png'));spec.update(frames=count,fps=6,loop=True,readabilityLoop=True,source='Tools/enhance_obstacle_readability.py')
    (out/(name+'.json')).write_text(json.dumps(spec,indent=2))
print('PASS 8 versioned hazard sheets; fixed silhouettes, regional palette, animated danger trim')
