"""Mechanical two-row generated HUD export, regional palettes, no repaint."""
from pathlib import Path
import json,sys,shutil
from PIL import Image
root=Path(__file__).resolve().parents[1]
source=Path(sys.argv[1]);im=Image.open(source).convert('RGBA')
im.putalpha(im.getchannel('A').point(lambda a:255 if a>=128 else 0))
review=root/'Artifacts/VisualRefactor/HudStatus';review.mkdir(parents=True,exist_ok=True)
shutil.copy2(source,review/'generated_source.png')
cells=[]
for row in range(2):
    c=im.crop((0,round(row*im.height/2),im.width,round((row+1)*im.height/2)))
    assert c.getbbox()
    c=c.crop(c.getbbox()).resize((568,64),Image.Resampling.NEAREST)
    cells.append(c)
for region in ('costa','sierra','selva'):
    path=root/(f'Assets/SuyuRun/Art/Pixel{region.title()}/{region}_palette.json' if region!='selva' else 'Assets/SuyuRun/Art/PixelSelva/ProductionV2/Resources/SelvaV2/selva_palette.json')
    colors=[tuple(bytes.fromhex(c[1:])) for c in json.loads(path.read_text())['colors']]
    out=root/'Assets/SuyuRun/Art/RefactorRuntime/Resources/HudStatus';out.mkdir(parents=True,exist_ok=True)
    for i,state in enumerate(('sling','gun')):
        c=cells[i].copy()
        c.putdata([(*min(colors,key=lambda q:sum((q[k]-p[k])**2 for k in range(3))),255) if p[3] else (0,0,0,0) for p in c.getdata()])
        assert len({p[:3] for p in c.getdata() if p[3]})<=24
        c.save(out/f'{region}_{state}.png')
        c.resize((1136,128),Image.Resampling.NEAREST).save(review/f'{region}_{state}_preview.png')
print('PASS six versioned native HUD modules, 24 regional colors, binary alpha')
