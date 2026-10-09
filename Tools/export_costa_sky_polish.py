"""Mechanical nearest-neighbour export of the independent generated sky layer."""
from pathlib import Path
import json,shutil
from PIL import Image
root=Path(__file__).resolve().parents[1]
source=Path('C:/Users/USER/.codex/generated_images/01a0b183-22cf-7d11-8237-6260f5f81b06/exec-7e6e3d7c-6834-4289-af7d-c738afad6670.png')
dest=root/'Artifacts/VisualRefactor/LayerPolish/Costa01'
dest.mkdir(parents=True,exist_ok=True)
shutil.copy2(source,dest/'generated_source.png')
colors=[tuple(bytes.fromhex(c[1:])) for c in json.loads((root/'Assets/SuyuRun/Art/PixelCosta/costa_palette.json').read_text())['colors']]
im=Image.open(source).convert('RGB').resize((1280,360),Image.Resampling.NEAREST)
lookup={}
pixels=[]
for v in im.getdata():
    if v not in lookup:lookup[v]=min(colors,key=lambda q:sum((q[k]-v[k])**2 for k in range(3)))
    pixels.append(lookup[v])
im.putdata(pixels)
im.save(dest/'costa_capa1_cielo_pulido_v3.png')
report={'width':1280,'height':360,'logicalWidth':640,'logicalHeight':180,'colors':len(set(pixels)),'approved':False,'integrated':False,'source':str(source),'note':'Generated independent sky, resized to established layer contract. Previous asset untouched.'}
(dest/'validation.json').write_text(json.dumps(report,indent=2))
print(report)
