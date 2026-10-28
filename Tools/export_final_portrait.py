"""Mechanical grid registration and regional palette export; no painted artwork."""
from pathlib import Path
from PIL import Image
import argparse,json,shutil
p=argparse.ArgumentParser();p.add_argument('source',type=Path);a=p.parse_args()
root=Path(__file__).resolve().parents[1]
out=root/'Assets/SuyuRun/Art/RefactorRuntime/Resources/FinalMovieV2';out.mkdir(parents=True,exist_ok=True)
src=Image.open(a.source).convert('RGBA')
assert src.getchannel('A').getextrema()[0]==0,'Generated background is opaque: request transparency edit'
palette=json.loads((root/'Assets/SuyuRun/Art/PixelSelva/ProductionV2/Resources/SelvaV2/selva_palette.json').read_text())['colors']
colors=[tuple(bytes.fromhex(c[1:])) for c in palette if c not in ('#C4E37A','#7FD4C9','#C67C73','#FFF0AE')]
cells=[]
for i in range(6):
    x=i%3;y=i//3
    cell=src.crop((round(x*src.width/3),round(y*src.height/2),round((x+1)*src.width/3),round((y+1)*src.height/2)))
    cell.putalpha(cell.getchannel('A').point(lambda v:255 if v>=128 else 0))
    assert cell.getbbox();cells.append(cell)
# One common registration keeps relative facial sizes and authored motion.
sheet=Image.new('RGBA',(256*6,256));lut={}
for i,cell in enumerate(cells):
    cell=cell.resize((256,256),Image.Resampling.NEAREST);pixels=[]
    for v in cell.getdata():
        if not v[3]:pixels.append((0,0,0,0));continue
        rgb=v[:3]
        if rgb not in lut:lut[rgb]=min(colors,key=lambda c:sum((c[k]-rgb[k])**2 for k in range(3)))
        pixels.append((*lut[rgb],255))
    cell.putdata(pixels);sheet.paste(cell,(i*256,0))
sheet.save(out/'tayta_portrait.png')
(out/'tayta_portrait.json').write_text(json.dumps(dict(frames=6,frameWidth=256,frameHeight=256,fps=6,loop=False),indent=2))
archive=root/'Artifacts/FinalMovieV2';archive.mkdir(parents=True,exist_ok=True);shutil.copy2(a.source,archive/'tayta_portrait_source.png')
assert len({v[:3] for v in sheet.getdata() if v[3]})<=24
print('PASS: six aligned 256x256 portrait poses, binary alpha, <=24 regional colors')
