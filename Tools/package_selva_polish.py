"""Package native sprites and the integration source; never build a player."""
from pathlib import Path
import json, zipfile, hashlib
ROOT=Path(__file__).resolve().parents[1]
OUT=ROOT/'Artifacts/SelvaV2'
report=json.loads((OUT/'art-validation.json').read_text())
assert report['failures']==[]
files=[]
for folder in ('Assets/SuyuRun/Art/PixelSelva/ProductionV2','Assets/SuyuRun/Data/Resources/SelvaIntegration','Assets/SuyuRun/Audio/Resources/SelvaAudio'):
    files += [(p,p.relative_to(ROOT)) for p in (ROOT/folder).rglob('*') if p.is_file()]
for folder,pattern in (('Assets/SuyuRun/Scripts','Selva*.cs*'),('Assets/SuyuRun/Editor','Selva*.cs*')):
    files += [(p,p.relative_to(ROOT)) for p in (ROOT/folder).glob(pattern) if p.is_file()]
for name in ('build_selva_polish.py','build_selva_artpack.py','audit_sierra_midi.py','build_selva_encounters.py','build_selva_museum_finish.py','probe_selva_audio.py','SelvaModelAudit.cs'):
    files.append((ROOT/'Tools'/name,Path('Tools')/name))
for name in ('RunnerProgress.cs','RunnerRegionalMenu.cs','RunnerSharedGameMenu.cs','SharedMenuBindings.cs','MuseumArt.cs'):
    p=ROOT/'Assets/SuyuRun/Scripts'/name
    files.append((p,p.relative_to(ROOT)))
for name in ('selva_emboscada_balanceo.gif','selva_pulido_x4.png','caiman_ciclo.png','caiman_fases_x4.png','art-validation.json','midi_review.json','unity-verification.txt'):
    p=OUT/name
    if p.exists():files.append((p,Path('Review')/name))
for p in OUT.glob('unity-*.png'):files.append((p,Path('Review')/p.name))
files.append((ROOT/'Docs/SELVA_V2_ARTE_E_INTEGRACION.md',Path('README.md')))
files.append((ROOT/'Docs/SELVA_ENCUENTROS_CHULLACHAQUI.md',Path('ENCUENTROS_CHULLACHAQUI.md')))
for name in ('chullachaqui_encuentro_x4.png','encounters-audit.txt'):
    if (OUT/name).exists():files.append((OUT/name,Path('Review')/name))
scene=ROOT/'Assets/SuyuRun/Scenes/Level_Selva.unity'
if scene.exists():files.append((scene,scene.relative_to(ROOT)))
if scene.with_suffix('.unity.meta').exists():files.append((scene.with_suffix('.unity.meta'),scene.with_suffix('.unity.meta').relative_to(ROOT)))
target=OUT/'SUYURUN_Selva_Encuentros_V3.zip'
with zipfile.ZipFile(target,'w',zipfile.ZIP_DEFLATED,compresslevel=9) as z:
    for file,name in files:z.write(file,name)
with zipfile.ZipFile(target) as z:assert z.testzip() is None
(OUT/'zip-verification.json').write_text(json.dumps(dict(entries=len(files),bytes=target.stat().st_size,sha256=hashlib.sha256(target.read_bytes()).hexdigest(),crc='PASS'),indent=2))
print(str(target))
