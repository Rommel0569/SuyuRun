from pathlib import Path
import zipfile
root=Path(__file__).resolve().parents[1];out=root/'Artifacts/TaytaUniform'
files=[]
for folder in [out,root/'Assets/SuyuRun/Art/RefactorRuntime/Resources/RefactorBank/TaytaUniform',root/'Assets/SuyuRun/Art/RefactorRuntime/Resources/FinalMovieV4']:
    files.extend(p for p in folder.rglob('*') if p.is_file() and p.suffix not in ('.zip','.log'))
for name in ['TaytaArt','TaytaContactDust','GameScale','RunnerPrototype','SierraStageDirector','SelvaStagePresentation','ProjectileStageDirector','FinalMovieArt','RunnerFinalMovie']:
    p=root/'Assets/SuyuRun/Scripts'/(name+'.cs');files.extend([p,p.with_name(p.name+'.meta')])
for name in ['TaytaUniformAudit','HumanGaitAudit','RefactorRuntimeImport','FinalMoviePlayAudit','FinalMoviePreview','FinalMovieScoreBuild']:
    p=root/'Assets/SuyuRun/Editor'/(name+'.cs');files.extend([p,p.with_name(p.name+'.meta')])
for name in ['export_uniform_tayta.py','audit_uniform_tayta.py','package_uniform_tayta.py','record_uniform_verification.py']:
    files.append(root/'Tools'/name)
files.extend([root/'ART_BIBLE.md',root/'ART_APPROVALS.md'])
target=out/'SUYURUN_Tayta_Uniforme_Juego_Final.zip'
with zipfile.ZipFile(target,'w',zipfile.ZIP_DEFLATED) as z:
    for p in sorted(set(files)):
        if p.is_file():z.write(p,p.relative_to(root).as_posix())
with zipfile.ZipFile(target) as z:assert z.testzip() is None
print('PASS bank ZIP CRC:',target.stat().st_size,'bytes;',len(set(files)),'files; no executable')
