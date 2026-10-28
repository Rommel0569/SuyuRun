"""Check real Unity captures for blank frames and unintended black border strips."""
from pathlib import Path
from PIL import Image
import numpy as np,json,hashlib
root=Path(__file__).resolve().parents[1];folder=root/'Artifacts/FinalMovieV4/PlayAudit'
result=(folder/'result.txt').read_text();assert result.startswith('PASS'),result
entries=[]
for p in sorted(folder.glob('frame_*.png')):
    age=float(p.stem.split('_')[1]);im=Image.open(p).convert('RGB');a=np.array(im)
    if age>=20:continue # The user explicitly requested black credit backgrounds.
    assert len(im.getcolors(im.width*im.height))>20,('Blank/flat scene',p)
    black=(a.max(axis=2)<=2)
    strips=[black[:2,:].mean(),black[-2:,:].mean(),black[:,:2].mean(),black[:,-2:].mean()]
    assert max(strips)<.98,('Unintended full black edge',p,strips)
    entries.append({'frame':p.name,'size':im.size,'blackEdgeFractions':[float(v) for v in strips],'sha256':hashlib.sha256(p.read_bytes()).hexdigest()})
report={'playResult':result,'checkedFilmFrames':len(entries),'creditBlackIsIntentional':True,'entries':entries}
(folder/'FRAME_VALIDATION.json').write_text(json.dumps(report,indent=2))
print('PASS',len(entries),'actual film captures: nonblank, no full black viewport edges. Not aesthetic approval.')
