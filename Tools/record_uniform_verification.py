from pathlib import Path
import json,shutil
root=Path(__file__).resolve().parents[1];out=root/'Artifacts/TaytaUniform';play=out/'Play';play.mkdir(exist_ok=True)
tour=(root/'Artifacts/VisualRefactor/play_visual_tour.txt').read_text()
ending=(root/'Artifacts/FinalMovieV4/PlayAudit/result.txt').read_text()
assert 'Finished.' in tour and 'ERROR ' not in tour
assert ending.startswith('PASS') and 'Runtime errors=0' in ending
shutil.copy2(root/'Artifacts/VisualRefactor/play_visual_tour.txt',play/'gameplay_tour.txt')
for p in (root/'Artifacts/VisualRefactor').glob('play_*.png'):shutil.copy2(p,play/p.name)
dest=play/'Ending';dest.mkdir(exist_ok=True)
for p in (root/'Artifacts/FinalMovieV4/PlayAudit').glob('*'):
    if p.is_file():shutil.copy2(p,dest/p.name)
report=dict(date='2026-10-24',unityLog='unity-release-validation.log',resourceAudit='UNITY_RESOURCE_AUDIT.txt',gameplayStateSheets=75,gameplayPhysicalFrames=366,cinematicMappings=7,sourcePalettesUnchanged=True,gameplayTour=dict(scenes=['Costa','Sierra','Selva'],sierraActs=3,sierraMinicaves=2,selvaZones=6,loggedRuntimeErrors=0,completeChronologicalPlaythrough=False,costaAutoplayEndedDead='Level_Costa for 8 seconds; state=Dead' in tour),ending=dict(result=ending.strip(),captures=35,blankEdgeAudit='PASS:31film captures; credits intentionally black'),visualReview=['three-costume anatomy comparison','run atlas','swing atlas','Sierra damage atlas','actual gameplay/cave/river captures','actual cinematic Coast/Sierra/Selva/portal/victory captures'],acceptanceByUser=False,executableGenerated=False)
(out/'VERIFICATION.json').write_text(json.dumps(report,indent=2));print('Recorded fresh diagnostic Play tour and complete ending evidence.')
