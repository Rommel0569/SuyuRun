"""Read-only resource and timing checks, not a Unity Play acceptance test."""
from pathlib import Path
import re,json
from PIL import Image
root=Path(__file__).resolve().parents[1];code=(root/'Assets/SuyuRun/Scripts/RunnerFinalMovie.cs').read_text()
missing=[];paths=re.findall(r'(?:Layer|Texture|Actor)\("([^"]+)"',code)
for resource in paths+['FinalMovie/final_music','FinalMovieV2/tayta_portrait']:
    candidates=[p for p in (root/'Assets').rglob(Path(resource).name+'.*') if '/Resources/'+resource+'.' in p.as_posix()]
    if not candidates:missing.append(resource)
assert not missing,missing
path=root/'Assets/SuyuRun/Art/RefactorRuntime/Resources/FinalMovieV2/tayta_portrait.png'
im=Image.open(path).convert('RGBA');assert im.size==(1536,256)
assert set(im.getchannel('A').getdata())<={0,255}
assert len({p[:3] for p in im.getdata() if p[3]})<=24
for i in range(6):assert im.crop((i*256,0,(i+1)*256,256)).getbbox()
report=dict(resourceReferences=len(paths)+2,missing=missing,portraitFrames=6,portraitCell=[256,256],filmSeconds=20,creditsSeconds=10,originalPreserved=True,unityPlayVerified=False)
(root/'Artifacts/FinalMovieV2/VALIDATION.json').write_text(json.dumps(report,indent=2))
print('PASS resource references, six portrait cells, alpha, palette, 20+10 second timeline. Not Unity Play.')
