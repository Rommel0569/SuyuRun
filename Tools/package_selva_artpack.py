"""Assemble the art-only ZIP after preview_selva_artpack.py passes validation."""
from pathlib import Path
import json, shutil, zipfile, hashlib
ROOT=Path(__file__).resolve().parents[1]
OUT=ROOT/'Artifacts/SelvaArtPack_v1';PACK=OUT/'Package'
report=json.loads((OUT/'validation.json').read_text())
assert report['failures']==[]
for folder in ('Tools','SourceReferences','Previews'):(PACK/folder).mkdir(exist_ok=True)
for file in ('build_selva_artpack.py','preview_selva_artpack.py'):
    shutil.copy2(ROOT/'Tools'/file,PACK/'Tools'/file)
for file in (ROOT/'Assets/SuyuRun/Art/PixelSierra/ProductionV3/Resources/SierraV3/sierra_premio_traje_idle.png',
             ROOT/'Artifacts/ArtApproval/costa_capa7_suelo_preview_x4.png',
             ROOT/'Artifacts/SierraV3/sierra_camino_preview_x4.png'):
    shutil.copy2(file,PACK/'SourceReferences'/file.name)
for file in (OUT/'Previews').iterdir():
    if file.suffix in ('.png','.gif'):shutil.copy2(file,PACK/'Previews'/file.name)
shutil.copy2(OUT/'validation.json',PACK/'validation.json')
inventory=json.loads((PACK/'manifest.json').read_text())
rows=['# Inventario del lote Selva','',f"{report['pngs']} PNG de producción; {report['frames']} celdas de animación/arte;24colores.",'',
      '| Archivo | Tamaño de frame | Frames | FPS |','|---|---:|---:|---:|']
for s in inventory['assets']:rows.append(f"| {s['file']} | {s['frameWidth']}×{s['frameHeight']} | {s['frames']} | {s['fps']} |")
(PACK/'INVENTARIO.md').write_text('\n'.join(rows),encoding='utf-8')
target=OUT/'SUYURUN_Selva_Arte_v1.zip'
with zipfile.ZipFile(target,'w',zipfile.ZIP_DEFLATED,compresslevel=9) as archive:
    for file in sorted(PACK.rglob('*')):
        if file.is_file() and '__pycache__' not in str(file):archive.write(file,file.relative_to(PACK))
with zipfile.ZipFile(target) as archive:
    assert archive.testzip() is None
    count=len(archive.namelist())
digest=hashlib.sha256(target.read_bytes()).hexdigest()
(OUT/'zip-verification.json').write_text(json.dumps(dict(file=target.name,entries=count,bytes=target.stat().st_size,sha256=digest,crc='PASS'),indent=2),encoding='utf-8')
print(json.dumps(dict(file=str(target),pngs=report['pngs'],frames=report['frames'],zipMB=round(target.stat().st_size/1e6,2),entries=count)))
