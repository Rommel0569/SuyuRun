"""Audit the sheets actually selected by TaytaArt, including remapped poses."""
from pathlib import Path
from PIL import Image
import json
ROOT=Path(__file__).resolve().parents[1]
base=ROOT/'Assets/SuyuRun/Art/Tayta/Resources/Tayta'
report=[];failures=[]
for original in sorted(base.rglob('*.json')):
    if original.stem.endswith(('_v2','_v3')):continue
    suffix='_v3' if original.stem=='run' else '_v2'
    revised=original.with_name(original.stem+suffix+'.json')
    spec=json.loads((revised if revised.exists() else original).read_text())
    revised_png=original.with_name(original.stem+suffix+'.png')
    image=Image.open(revised_png if revised_png.exists() else original.with_suffix('.png')).convert('RGBA')
    region=original.parent.name;state=original.stem
    heights=[];feet=[];legs=[]
    for f in range(spec['frames']):
        src=spec.get('frameOrder',list(range(spec['frames'])))[f]
        cell=image.crop((src*48,0,(src+1)*48,48));box=cell.getbbox()
        if not box:failures.append(f'{region}/{state}/{f}: empty');continue
        heights.append(box[3]-box[1]);feet.append(box[3])
        if state not in ('slide','death') and box[3]-box[1]<25:failures.append(f'{region}/{state}/{f}: incomplete body')
        legs.append(cell.crop((0,32,48,48)).tobytes())
    if set(image.getchannel('A').getdata())-{0,255}:failures.append(f'{region}/{state}: alpha')
    palette=ROOT/f'Assets/SuyuRun/Art/Pixel{region.title()}/{region}_palette.json'
    if region=='selva':palette=ROOT/'Assets/SuyuRun/Art/PixelSelva/ProductionV2/Resources/SelvaV2/selva_palette.json'
    allowed={tuple(bytes.fromhex(c[1:])) for c in json.loads(palette.read_text())['colors']}
    if {p[:3] for p in image.getdata() if p[3]}-allowed:failures.append(f'{region}/{state}: palette')
    if state in ('run','burst') and region!='costa' or state=='run':
        if min(heights)<33 or max(heights)-min(heights)>8:failures.append(f'{region}/{state}: scale jump')
        if len(set(legs))<6:failures.append(f'{region}/{state}: static legs')
        for f in range(spec['frames']):
            src=spec.get('frameOrder',list(range(spec['frames'])))[f]
            box=image.crop((src*48,0,(src+1)*48,48)).getbbox()
            if box[0]<1 or box[2]>47 or box[1]<1:failures.append(f'{region}/{state}/{f}: no safety margin')
    report.append(dict(region=region,state=state,heights=heights,feet=feet,distinctLegs=len(set(legs)),revision=revised.exists()))
out=ROOT/'Artifacts/Tayta/Polish/effective_frames_validation.json'
out.write_text(json.dumps(dict(sheets=len(report),frames=sum(len(r['heights']) for r in report),failures=failures,details=report),indent=2))
print(json.dumps(dict(sheets=len(report),frames=sum(len(r['heights']) for r in report),failures=failures)))
assert not failures,failures
