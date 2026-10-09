"""Read-only export/registration checks, not Unity Play or visual acceptance."""
from pathlib import Path
import json
from PIL import Image
root=Path(__file__).resolve().parents[1]
bank=root/'Assets/SuyuRun/Art/RefactorRuntime/Resources/RefactorBank/AtmosphereClean'
checks=[]
for path in sorted(bank.glob('*.json')):
    s=json.loads(path.read_text());im=Image.open(path.with_suffix('.png')).convert('RGBA')
    ok=im.size==(s['frameWidth']*s['frames'],s['frameHeight']) and s['pixelsPerUnit']==128
    ok=ok and s['frameWidth']/128==s['logicalWidth']/32 and s['frameHeight']/128==s['logicalHeight']/32
    ok=ok and len({p[:3] for p in im.getdata() if p[3]})<=24
    # Liquid and atmosphere retain authored transparency, not binary cutouts.
    if s.get('alphaMode')=='preserveSource' or s.get('originalAlphaMaskPreserved'):
        previous=root/s['previous']
        prior_path=Path(str(previous).replace('.sprite.json','.png')) if previous.name.endswith('.sprite.json') else previous.with_suffix('.png')
        prior=Image.open(prior_path).convert('RGBA')
        alpha=prior.getchannel('A').resize(im.size,Image.Resampling.NEAREST)
        ok=ok and im.getchannel('A').tobytes()==alpha.tobytes()
    else:
        ok=ok and set(im.getchannel('A').getdata())<={0,255}
    checks.append(dict(asset=path.stem,registration=ok,frames=s['frames'],size=im.size))
catalog=json.loads((root/'Artifacts/VisualRefactor/LayerPolish/CAPAS_POR_MAPA.json').read_text())
expected={entry['name'] for entry in catalog['layers']}
covered={entry['asset'] for entry in checks}
missing=sorted(expected-covered)
assert not missing and all(c['registration'] for c in checks),(missing,checks)
hud=root/'Assets/SuyuRun/Art/RefactorRuntime/Resources/UnifiedHud'
old=Image.open(hud/'sierra_footer.png').convert('RGBA');new=Image.open(hud/'sierra_footer_clean.png').convert('RGBA')
assert old.size==new.size
assert all(old.getpixel((x,y))==new.getpixel((x,y)) for y in range(old.height) for x in range(old.width) if not (56<=x<818 and 23<=y<57))
report=dict(checks=checks,contexts=catalog['contexts'],expectedLayers=len(expected),coveredLayers=len(expected&covered),missingLayers=missing,footerOuterFrameUnchanged=True,unityPlayVerified=False)
out=root/'Artifacts/VisualRefactor/AtmosphereClean/VALIDATION.json';out.write_text(json.dumps(report,indent=2))
print(f'PASS: {len(expected&covered)}/{len(expected)} layer registrations across {len(catalog["contexts"])} contexts, palettes, alpha and preserved footer borders. Not Unity Play.')
