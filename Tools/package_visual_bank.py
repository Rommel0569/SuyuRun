"""Create an auditable Unity staging package, never activate it in gameplay."""
from pathlib import Path
import json, hashlib, zipfile
from PIL import Image

root=Path(__file__).resolve().parents[1]
base=root/'Artifacts/VisualRefactor'
bank=base/'Bank'
entries=[]
for specpath in sorted(bank.rglob('*.json')):
    data=json.loads(specpath.read_text())
    if not all(k in data for k in ('frames','frameWidth','frameHeight')):
        continue
    png=specpath.with_suffix('.png')
    if not png.exists():
        raise ValueError(f'Missing texture: {specpath}')
    image=Image.open(png).convert('RGBA')
    width,height=data['frameWidth'],data['frameHeight']
    assert image.size==(width*data['frames'],height),png
    assert set(image.getchannel('A').getdata())<={0,255},png
    colors=len({p[:3] for p in image.getdata() if p[3]})
    assert colors<=24,(png,colors)
    bounds=[]
    for i in range(data['frames']):
        box=image.crop((i*width,0,(i+1)*width,height)).getbbox()
        assert box and box[0]>0 and box[1]>0 and box[2]<width and box[3]<height,(png,i,box)
        bounds.append(box)
    durations=data.get('frameDurationsMs',[round(1000/data['fps'])]*data['frames'])
    assert len(durations)==data['frames'] and all(d>0 for d in durations),png
    # Current custom renderer uses upper-left pivot. Do NOT silently switch
    # pivot to feet and displace the existing hero/portal/vine positioning.
    entries.append(dict(id=specpath.relative_to(bank).with_suffix('').as_posix(),
        texture=png.relative_to(bank).as_posix(),metadata=specpath.relative_to(bank).as_posix(),
        frames=data['frames'],frameWidth=width,frameHeight=height,
        pixelsPerUnit=data['pixelsPerUnit'],pivot=[0,1],filterMode='Point',
        compression='None',wrapMode='Clamp',durationsMs=durations,loop=data.get('loop',False),
        bounds=bounds,colors=colors,sha256=hashlib.sha256(png.read_bytes()).hexdigest(),
        status='staging-not-gameplay-verified',anchorReviewRequired=data.get('anchorReviewRequired',False),
        bodyRegistrationReviewRequired=True,footAnchor=data.get('footAnchor'),
        renderOffsetFrom96Cell=data.get('renderOffsetFrom96Cell')))
manifest=dict(version=2,sheets=len(entries),frames=sum(e['frames'] for e in entries),
    integrated=False,playTested=False,entries=entries,
    runtimeIntegration='Partial: Tayta core/all regions, Sierra extras, bosses and selected actors. See ESTADO_ACTUAL.md.',
    incomplete=['Full visual Play review of body registration, weapons and vine anchors',
                'Full gameplay playthrough; short smoke tests do not certify completion',
                'Mandatory environment/obstacle detail refactor without flow changes'])
(base/'unity_catalog.json').write_text(json.dumps(manifest,indent=2))
archive=base/'Banco_Refactor_Actual.zip'
with zipfile.ZipFile(archive,'w',zipfile.ZIP_DEFLATED) as z:
    for file in sorted(bank.rglob('*')):
        if file.is_file():z.write(file,file.relative_to(base))
    for name in ['unity_catalog.json','README.md','UNITY_IMPORT.md','bank_prompts.md','additional_prompts.md','ESTADO_ACTUAL.md','GLOBAL_SCALE_AND_DETAIL.md','global_engineering_audit.txt','physics_models.txt','human_gait_audit.txt','runtime_audit.txt','runtime_resources.json','play_smoke.txt']:
        file=base/name
        if file.exists():z.write(file,name)
    for file in sorted((base/'Bosses').glob('*motion_prompt.txt')):z.write(file,file.relative_to(base))
    for file in sorted((base/'Heroes').glob('*prompt.txt')):z.write(file,file.relative_to(base))
    for file in sorted((base/'Actors').glob('*prompt.txt')):z.write(file,file.relative_to(base))
    z.write(root/'Assets/SuyuRun/Editor/ImportVisualRefactorBank.cs','Editor/ImportVisualRefactorBank.cs')
    for name in ['GameScale.cs','RunnerTerrain.cs','RefactorEnvironmentArt.cs','SierraCaveWorld.cs','SelvaMiniWorld.cs','SelvaStageDirector.cs','RefactorActorArt.cs','TaytaArt.cs','SierraStageDirector.cs','SierraCaves.cs','SelvaStagePresentation.cs','RunnerPrototype.cs','RunnerCoastalEnding.cs','RunnerCoastalDancer.cs','RunnerReferenceCoast.cs','RunnerPresentation.cs']:
        z.write(root/'Assets/SuyuRun/Scripts'/name,'Integration/Scripts/'+name)
    for name in ['GlobalEngineeringAudit.cs','HumanGaitAudit.cs','RefactorRuntimeImport.cs','RefactorResourceAudit.cs','RefactorPlayAudit.cs']:
        z.write(root/'Assets/SuyuRun/Editor'/name,'Integration/Editor/'+name)
    for file in sorted((root/'Assets/SuyuRun/Art/RefactorRuntime/Resources/RefactorBank/Environment').glob('*')):
        if file.suffix in ('.png','.json'):z.write(file,'Environment/'+file.name)
    for file in sorted(base.glob('*detail_prompt.txt')):z.write(file,'EnvironmentPrompts/'+file.name)
print(json.dumps(dict(sheets=manifest['sheets'],frames=manifest['frames'],archive=str(archive),integrated=False)))
