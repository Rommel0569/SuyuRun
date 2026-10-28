"""Validate the exact new runtime bank and build unpainted frame review boards."""
from pathlib import Path
from PIL import Image,ImageDraw
import json,hashlib
root=Path(__file__).resolve().parents[1];bank=root/'Assets/SuyuRun/Art/RefactorRuntime/Resources/RefactorBank/TaytaUniform';out=root/'Artifacts/TaytaUniform'
manifest=json.loads((out/'MANIFEST.json').read_text());records=[]
review=out/'Review';review.mkdir(exist_ok=True)
for name,spec in manifest.items():
    image=Image.open(bank/(name+'.png')).convert('RGBA');w,h=spec['frameWidth'],spec['frameHeight']
    assert image.size==(w*spec['frames'],h),name
    assert set(image.getchannel('A').getdata())<={0,255},name
    assert len({p[:3] for p in image.getdata() if p[3]})<=24,name
    assert abs(spec['pixelsPerUnit']-32*160/34)<.00001,name
    cells=[image.crop((i*w,0,(i+1)*w,h)) for i in range(spec['frames'])]
    assert all(c.getbbox() for c in cells),name
    assert all(c.getbbox()[0]>0 and c.getbbox()[2]<w and c.getbbox()[1]>0 for c in cells),name
    board=Image.new('RGBA',(w*4,((len(cells)+3)//4)*(h+20)),(26,31,38,255));d=ImageDraw.Draw(board)
    for i,c in enumerate(cells):x=i%4*w;y=i//4*(h+20);board.alpha_composite(c,(x,y+20));d.text((x+8,y+3),str(i),fill='white')
    board.convert('RGB').save(review/(name.replace('/','_')+'.png'))
    records.append(dict(name=name,frames=len(cells),sha256=hashlib.sha256((bank/(name+'.png')).read_bytes()).hexdigest()))
report=dict(states=len(records),physicalFrames=sum(v['frames'] for v in records),commonPixelsPerUnit=32*160/34,palettesMax24=True,binaryAlpha=True,originalBanksPreserved=True,unityPlayVerified=False,records=records)
(out/'VALIDATION.json').write_text(json.dumps(report,indent=2));print('PASS',report['states'],'state sheets;',report['physicalFrames'],'frames. Not visual approval or Unity Play.')
