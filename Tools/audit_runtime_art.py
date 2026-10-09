"""Read-only sprite-contract audit. Does not repaint or export textures."""
from pathlib import Path
import json,hashlib
from PIL import Image
root=Path(__file__).resolve().parents[1]
bank=root/'Assets/SuyuRun/Art/RefactorRuntime/Resources/RefactorBank'
entries=[];failures=[]
for specpath in sorted(bank.rglob('*.json')):
    spec=json.loads(specpath.read_text(encoding='utf-8-sig'))
    if not all(k in spec for k in ('frames','frameWidth','frameHeight')):continue
    path=specpath.with_suffix('.png')
    if not path.exists():failures.append(f'Missing PNG {path}');continue
    im=Image.open(path).convert('RGBA')
    colors={p[:3] for p in im.getdata() if p[3]}
    alpha=set(im.getchannel('A').getdata())
    checks=dict(dimensions=im.size==(spec['frameWidth']*spec['frames'],spec['frameHeight']),
                alphaContract=alpha<={0,255},palette=len(colors)<=24,
                occupiedFrames=all(im.crop((i*spec['frameWidth'],0,(i+1)*spec['frameWidth'],spec['frameHeight'])).getbbox() is not None for i in range(spec['frames'])))
    if not checks['alphaContract'] and spec.get('alphaMode')=='preserveSource':
        original=Image.open(root/spec['original']).convert('RGBA')
        expected=Image.new('L',im.size)
        for i,c in enumerate(spec['sourceRectsUnity']):
            y=original.height-c['y']-c['height']
            cell=original.getchannel('A').crop((c['x'],y,c['x']+c['width'],y+c['height'])).resize((spec['frameWidth'],spec['frameHeight']),Image.Resampling.NEAREST)
            expected.paste(cell,(i*spec['frameWidth'],0))
        checks['alphaContract']=expected.tobytes()==im.getchannel('A').tobytes()
        checks['sourceAlphaPreserved']=checks['alphaContract']
    if spec.get('seamEndpointEqual'):
        checks['endpoint']=im.crop((0,0,1,im.height)).tobytes()==im.crop((im.width-1,0,im.width,im.height)).tobytes()
    for key,ok in checks.items():
        if not ok:failures.append(f'{path.relative_to(root)}: {key}')
    entries.append(dict(path=path.relative_to(root).as_posix(),frames=spec['frames'],colors=len(colors),checks=checks,sha256=hashlib.sha256(path.read_bytes()).hexdigest()))
report=dict(sheets=len(entries),frames=sum(e['frames'] for e in entries),failures=failures,entries=entries,
            limitation='Texture contracts only; not visual approval, animation quality or a gameplay playthrough.')
out=root/'Artifacts/VisualRefactor/runtime_texture_contracts.json'
out.write_text(json.dumps(report,indent=2),encoding='utf-8')
print(json.dumps({k:report[k] for k in ('sheets','frames','failures','limitation')}))
if failures:raise SystemExit(1)
