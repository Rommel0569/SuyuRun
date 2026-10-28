"""Read-only contracts and registration checks. Not visual/Play approval."""
from pathlib import Path
import json,re,hashlib,argparse
from PIL import Image
p=argparse.ArgumentParser();p.add_argument('--revision',default='FinalMovieV3');a=p.parse_args()
root=Path(__file__).resolve().parents[1];bank=root/'Assets/SuyuRun/Art/RefactorRuntime/Resources'/a.revision
jobs=json.loads((root/'Artifacts'/a.revision/'PLAN_COMPLETE.json').read_text());entries=[]
for j in jobs:
    p=bank/(j['name']+'.json');s=json.loads(p.read_text());im=Image.open(p.with_suffix('.png')).convert('RGBA')
    assert im.size==(s['frames']*s['frameWidth'],s['frameHeight']),p
    assert len({v[:3] for v in im.getdata() if v[3]})<=24,p
    assert set(im.getchannel('A').getdata())<={0,255},p
    for i in range(s['frames']):
        cell=im.crop((i*s['frameWidth'],0,(i+1)*s['frameWidth'],s['frameHeight']));assert cell.getbbox(),(p,i)
        assert abs(s['footY'][i]-cell.getbbox()[3]/s['frameHeight'])<.00001
    if j['transparent']:assert im.getchannel('A').getextrema()==(0,255)
    entries.append(dict(name=j['name'],frames=s['frames'],size=im.size,sha256=hashlib.sha256(p.with_suffix('.png').read_bytes()).hexdigest()))
code=(root/'Assets/SuyuRun/Scripts/RunnerFinalMovie.cs').read_text()
assert not any(x in code for x in ('TaytaArt','MuseumArt','RefactorBank/','SelvaV2/','SierraV3/')),'Gameplay artwork still referenced'
assert 'PlayerPrefs.Set' not in code
assert 't*58' in code
report=dict(sheets=len(entries),frames=sum(e['frames'] for e in entries),entries=entries,gameplayArtFallback=False,unityPlayVerified=False)
(root/'Artifacts'/a.revision/'VALIDATION.json').write_text(json.dumps(report,indent=2))
print(f'PASS {len(entries)} dedicated sheets, {report["frames"]} cells, alpha/palettes/foot registration; no gameplay artwork references. Not Unity Play.')
