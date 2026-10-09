"""Read native pixels to record precise attachment anchors; does not redraw art."""
from pathlib import Path
from PIL import Image
import json
root=Path(__file__).resolve().parents[1]
out=root/'Artifacts/Tayta'
report={'sheets':0,'failures':[],'grips':[]}
for metadata in (root/'Assets/SuyuRun/Art/Tayta/Resources/Tayta').rglob('*.json'):
    spec=json.loads(metadata.read_text(encoding='utf-8'))
    image=Image.open(metadata.with_suffix('.png')).convert('RGBA')
    assert image.size==(48*spec['frames'],48),metadata
    anchors=[]
    for frame in range(spec['frames']):
        cell=image.crop((frame*48,0,(frame+1)*48,48))
        bounds=cell.getchannel('A').getbbox()
        assert bounds,metadata
        assert bounds[3]==48, (metadata,frame,'feet not on baseline')
        if metadata.stem in ['swing','grab','regrab','release','climb'] and metadata.parent.name=='selva':
            y=bounds[1]
            pts=[(x,yy) for yy in range(y,min(y+4,48)) for x in range(48) if cell.getpixel((x,yy))[3]]
            anchors.append((round(sum(x+.5 for x,yy in pts)/len(pts),2),y+2))
    if anchors:
        spec['gripX']=[a[0] for a in anchors];spec['gripY']=[a[1] for a in anchors]
        metadata.write_text(json.dumps(spec),encoding='utf-8')
        report['grips'].append(str(metadata.relative_to(root)))
    report['sheets']+=1
(out/'alignment_validation.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
print(json.dumps(report,indent=2))
