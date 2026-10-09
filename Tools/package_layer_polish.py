"""Consolidate complete existing art + explicit new polish, never mislabel legacy."""
from pathlib import Path
import json,hashlib,zipfile
root=Path(__file__).resolve().parents[1]
out=root/'Artifacts/VisualRefactor/LayerPolish'
bank=root/'Assets/SuyuRun/Art/RefactorRuntime/Resources/RefactorBank'
newnames={p.stem for p in (bank/'LayerPolish').glob('*.png')}|{'huaraca_throw_v3','huaraca_spin_v3','huaraca_load_v3'}
records=[]
for file in sorted(bank.rglob('*.json')):
    png=file.with_suffix('.png')
    if not png.exists():continue
    spec=json.loads(file.read_text(encoding='utf-8-sig'))
    records.append(dict(path=png.relative_to(root).as_posix(),frames=spec.get('frames'),native=[spec.get('frameWidth'),spec.get('frameHeight')],status='redrawn-this-revision' if png.parent.name=='LayerPolish' or png.stem in ('huaraca_throw_v3','huaraca_spin_v3','huaraca_load_v3') else 'existing-bank-not-redrawn-this-revision',sha256=hashlib.sha256(png.read_bytes()).hexdigest()))
catalog=json.loads((out/'CAPAS_POR_MAPA.json').read_text())
audit=out/'UNITY_AUDIT.txt'
manifest=dict(newSheets=sum(r['status']=='redrawn-this-revision' for r in records),totalSheets=len(records),records=records,layerSetProduced=all(r['status']=='redrawn-current-lot' for r in catalog['layers']),layerCount=catalog['uniqueLayers'],unityResourceAuditPassed=audit.exists() and 'PASS 63 layers' in audit.read_text(),fullVisualPolishComplete=False,unityPlayVerified=False,scope='All 63 runtime scenery layers redrawn, plus three huaraca sheets. Other assets included from existing library, not newly drawn.')
(out/'MANIFEST.json').write_text(json.dumps(manifest,indent=2))
archive=out/'SUYURUN_Banco_Consolidado_Pulido.zip'
with zipfile.ZipFile(archive,'w',zipfile.ZIP_DEFLATED) as z:
    for folder in ('Assets/SuyuRun/Art','Assets/SuyuRun/Scripts','Assets/SuyuRun/Editor'):
        for file in sorted((root/folder).rglob('*')):
            if file.is_file():z.write(file,file.relative_to(root).as_posix())
    for file in sorted(out.rglob('*')):
        if file.is_file() and file.suffix!='.zip':z.write(file,file.relative_to(root).as_posix())
    for name in ('ART_BIBLE.md','ART_APPROVALS.md'):z.write(root/name,name)
    for name in ('export_layer_polish.py','export_animated_layer_polish.py','export_huaraca_polish.py','export_huaraca_spin_polish.py','catalog_visual_layers.py','review_layer_polish.py','package_layer_polish.py','compile_refactor_check.ps1','audit_runtime_art.py'):
        z.write(root/'Tools'/name,'Tools/'+name)
with zipfile.ZipFile(archive) as z:assert z.testzip() is None
print(json.dumps(dict(archive=str(archive),bytes=archive.stat().st_size,totalSheets=len(records),newSheets=manifest['newSheets'],fullVisualPolishComplete=False)))
