"""Package actual clean-layer resources and evidence, not a game executable."""
from pathlib import Path
import json,zipfile
root=Path(__file__).resolve().parents[1]
out=root/'Artifacts/VisualRefactor/AtmosphereClean'
report=json.loads((out/'VALIDATION.json').read_text())
assert report['coveredLayers']==report['expectedLayers']==63 and not report['missingLayers']
folders=[root/'Assets/SuyuRun/Art/RefactorRuntime/Resources/RefactorBank/AtmosphereClean',out]
files=[]
for folder in folders:
    files.extend(p for p in folder.rglob('*') if p.is_file() and p.suffix not in ('.zip','.meta'))
files.extend(root/p for p in [
    'Assets/SuyuRun/Scripts/RefactorEnvironmentArt.cs',
    'Assets/SuyuRun/Scripts/RefactorMaterialArt.cs',
    'Assets/SuyuRun/Scripts/SelvaStagePresentation.cs',
    'Assets/SuyuRun/Editor/RefactorRuntimeImport.cs',
    'Tools/export_layer_polish.py','Tools/export_animated_layer_polish.py',
    'Tools/audit_atmosphere_clean.py','Tools/review_layer_polish.py',
    'Artifacts/VisualRefactor/runtime_texture_contracts.json',
    'Artifacts/VisualRefactor/CompileCheck/result.txt'])
archive=out/'SUYURUN_AtmosphereClean_Completo.zip'
with zipfile.ZipFile(archive,'w',zipfile.ZIP_DEFLATED) as z:
    for p in sorted(set(files)):z.write(p,p.relative_to(root).as_posix())
with zipfile.ZipFile(archive) as z:assert z.testzip() is None
print(f'PASS ZIP CRC: {len(set(files))} files, {archive.stat().st_size} bytes. No executable.')
