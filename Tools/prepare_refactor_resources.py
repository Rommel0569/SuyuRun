"""Mechanical resource deployment. Never alters source artwork or hitboxes."""
from pathlib import Path
import json
from PIL import Image,ImageOps

root=Path(__file__).resolve().parents[1]
bank=root/'Artifacts/VisualRefactor/Bank'
target=root/'Assets/SuyuRun/Art/RefactorRuntime/Resources/RefactorBank'
actors=['stone','inca','treant','chulla','Actors/skeleton','Actors/pebble','Actors/sprout','Actors/caiman','Actors/condor','Actors/hawk','Actors/puna','Actors/hexagon','Actors/coastbird','Actors/boat','Actors/dancer']
actors.append('Actors/guardian')
copied=[]
for actor in actors:
    folder=bank/actor
    reference=folder/('flight.png' if actor.endswith(('condor','hawk','puna','coastbird')) else 'idle.png')
    ref=Image.open(reference).convert('RGBA');refspec=json.loads(reference.with_suffix('.json').read_text())
    w,h=refspec['frameWidth'],refspec['frameHeight']
    bounds=ref.crop((0,0,w,h)).getbbox()
    bywidth=actor.endswith(('caiman','condor','hawk','puna','coastbird','boat'))
    extent=bounds[2]-bounds[0] if bywidth else bounds[3]-bounds[1]
    for specpath in folder.rglob('*.json'):
        spec=json.loads(specpath.read_text())
        if 'frameWidth' not in spec:continue
        source=specpath.with_suffix('.png')
        if not source.exists():continue
        # A stale early caiman export is retained for recovery, not activated.
        if actor=='Actors/caiman' and specpath.stem=='hit':continue
        im=Image.open(source).convert('RGBA');fw=spec['frameWidth'];fh=spec['frameHeight']
        reference_extent=extent
        if 'MotionV2' in specpath.parts:
            guard=im.crop(((spec['frames']-1)*fw,0,spec['frames']*fw,fh)).getbbox()
            reference_extent=guard[3]-guard[1]
        if spec.get('orientation')=='left':
            flipped=Image.new('RGBA',im.size)
            for i in range(spec['frames']):flipped.alpha_composite(ImageOps.mirror(im.crop((i*fw,0,(i+1)*fw,fh))),(i*fw,0))
            im=flipped
        spec.update(referenceExtent=reference_extent,footY=124,orientation='right')
        out=target/specpath.relative_to(bank);out.parent.mkdir(parents=True,exist_ok=True)
        out.write_text(json.dumps(spec,indent=2));im.save(out.with_suffix('.png'))
        copied.append(str(out.relative_to(target)))
for hero in ['TaytaCosta','TaytaSierra','TaytaSelva']:
    for metadata in (bank/hero).glob('*.json'):
        spec=json.loads(metadata.read_text());png=metadata.with_suffix('.png')
        if 'frameWidth' not in spec or not png.exists():continue
        out=target/metadata.relative_to(bank);out.parent.mkdir(parents=True,exist_ok=True)
        out.write_text(metadata.read_text());Image.open(png).save(out.with_suffix('.png'))
for metadata in (bank/'Effects').glob('*.json'):
    spec=json.loads(metadata.read_text());png=metadata.with_suffix('.png')
    if 'frameWidth' not in spec or not png.exists():continue
    spec['referenceExtent']=spec['frameWidth']
    out=target/metadata.relative_to(bank);out.parent.mkdir(parents=True,exist_ok=True)
    out.write_text(json.dumps(spec,indent=2));Image.open(png).save(out.with_suffix('.png'))
deployed=[p.relative_to(target).as_posix() for p in target.rglob('*.json')]
(root/'Artifacts/VisualRefactor/runtime_resources.json').write_text(json.dumps(dict(sheets=len(deployed),files=deployed,fullPlaythroughTested=False),indent=2))
print('Deployed',len(deployed),'resource sheets; original drawings conserved.')
