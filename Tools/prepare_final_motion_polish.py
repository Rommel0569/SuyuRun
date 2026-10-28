"""Preserve the previous ending and append the requested independently authored sheets."""
from pathlib import Path
import json, shutil
root=Path(__file__).resolve().parents[1]
out=root/'Artifacts/FinalMovieV4'
bank=root/'Assets/SuyuRun/Art/RefactorRuntime/Resources/FinalMovieV4'
backup=out/'BeforeMotionPolish'
if not backup.exists():
    backup.mkdir()
    for name in ['costa_hero.png','costa_hero.json']:
        shutil.copy2(bank/name,backup/name)
    for name in ['RunnerFinalMovie.cs','FinalMovieArt.cs']:
        shutil.copy2(root/'Assets/SuyuRun/Scripts'/name,backup/name)
    for name in ['PLAN_COMPLETE.json','PLAY_VERIFICATION.json']:
        shutil.copy2(out/name,backup/name)
    shutil.copytree(out/'PlayAudit',backup/'PlayAudit')
    shutil.copy2(out/'Sources/costa_hero.png',backup/'costa_hero_source.png')
jobs=json.loads((out/'MOTION_POLISH_PROMPTS.json').read_text())
plan=json.loads((out/'PLAN_COMPLETE.json').read_text())
for job in jobs:
    spec=next((item for item in plan if item['name']==job['name']),None)
    if spec is None:
        spec=dict(name=job['name'],region='selva' if job['name']=='portal_entry' else 'costa',kind='actor' if job['name']=='portal_entry' else 'layer',frames=16 if job['name']=='portal_entry' else 1,columns=4 if job['name']=='portal_entry' else 1,transparent=True)
        plan.append(spec)
    spec['prompt']=job['prompt']
(out/'PLAN_COMPLETE.json').write_text(json.dumps(plan,indent=2))
print('Previous ending preserved; motion polish plan:',len(plan),'sheets')
