from pathlib import Path
import zipfile,json,argparse
p=argparse.ArgumentParser();p.add_argument('--revision',default='FinalMovieV3');p.add_argument('--suffix',default='');a=p.parse_args()
root=Path(__file__).resolve().parents[1];out=root/'Artifacts'/a.revision
report=json.loads((out/'VALIDATION.json').read_text());assert report['sheets']==len(json.loads((out/'PLAN_COMPLETE.json').read_text()))
folders=[out,root/'Assets/SuyuRun/Art/RefactorRuntime/Resources'/a.revision]
files=[p for folder in folders for p in folder.rglob('*') if p.is_file() and p.suffix not in ('.zip','.log')]
files += [root/p for p in ['Assets/SuyuRun/Scripts/RunnerFinalMovie.cs','Assets/SuyuRun/Scripts/FinalMovieArt.cs','Assets/SuyuRun/Editor/FinalMoviePreview.cs','Assets/SuyuRun/Editor/FinalMoviePlayAudit.cs','Assets/SuyuRun/Editor/FinalMovieScoreBuild.cs','Assets/SuyuRun/Editor/RefactorRuntimeImport.cs','Tools/export_final_bank.py','Tools/audit_final_bank.py','Tools/review_final_bank.py','Tools/package_final_movie.py','Artifacts/VisualRefactor/CompileCheck/result.txt']]
for relative in ['Assets/SuyuRun/Audio/Resources/FinalMovie/final_music.mp3','Assets/SuyuRun/Audio/Resources/Sfx/shot.ogg','Assets/SuyuRun/Art/Fonts/Resources/PressStart2P-Regular.ttf']:
    resource=root/relative;files.extend([resource,resource.with_name(resource.name+'.meta')])
files.append(root/'Tools/audit_final_captures.py')
files.append(root/'Tools/prepare_final_motion_polish.py')
target=out/('SUYURUN_Final_Cinematico_'+a.revision.replace('FinalMovie','')+a.suffix+'.zip')
with zipfile.ZipFile(target,'w',zipfile.ZIP_DEFLATED) as z:
    for p in sorted(set(files)):z.write(p,p.relative_to(root).as_posix())
with zipfile.ZipFile(target) as z:assert z.testzip() is None
print('PASS ZIP CRC:',len(set(files)),'files;',target.stat().st_size,'bytes; no executable')
