"""Inventory real authored layers and runtime assignments, without guessing new assets."""
from pathlib import Path
import json
root=Path(__file__).resolve().parents[1]
bank=root/'Assets/SuyuRun/Art/RefactorRuntime/Resources/RefactorBank'
zones=json.loads((root/'Assets/SuyuRun/Data/Resources/SelvaIntegration/selva_course.json').read_text())['zones']
contexts={'Costa':['costa_capa1_cielo','costa_capa2_nubes','costa_capa2_aves','costa_capa3_paracas','costa_capa4_mar','costa_capa5_huacas','costa_capa6_algarrobos','costa_capa7_suelo']}
for biome in ('camino','valle','ruinas'):
    contexts['Sierra/'+biome]=['sierra_compartido_capa1_cielo','sierra_compartido_capa2_nubes','sierra_compartido_capa3_nevados',f'sierra_{biome}_capa4_paisaje',f'sierra_{biome}_capa7_suelo']+(['sierra_valle_capa5_rio'] if biome=='valle' else [])
for i in (1,2):contexts[f'Sierra/minicueva{i}']=[f'sierra_cueva{i}_fondo',f'sierra_cueva{i}_arcos','sierra_cueva_suelo','sierra_cueva_roca','sierra_cueva_plataforma']
for i,biome in enumerate(zones):
    contexts[f'Selva/{i+1}-{biome}']=[f'selva_{biome}_capa{layer}_{desc}' for layer,desc in ((1,'cielo'),(2,'lejania'),(3,'estructura'),(5,'cercania'),(6,'frontal'))]+['selva_niebla','selva_rayos_sol']
    if i==1:contexts[f'Selva/{i+1}-{biome}']+=['selva_agua_fondo','selva_agua_superficie']
    if i in (2,4):contexts[f'Selva/{i+1}-{biome}'] += [f'selva_tile_{"cueva" if i==2 else "ruina"}_{part}' for part in ('superficie','relleno')]
names=sorted({n for rows in contexts.values() for n in rows})
rows=[]
for name in names:
    chosen=next((bank/folder/(name+'.json') for folder in ('LayerPolish','Environment','Readability','Materials') if (bank/folder/(name+'.json')).exists()),None)
    if chosen:
        spec=json.loads(chosen.read_text());png=chosen.with_suffix('.png')
        size=[spec['frameWidth'],spec['frameHeight']];status='redrawn-current-lot' if chosen.parent.name=='LayerPolish' else 'existing-refactor-review-pending'
    else:
        candidates=list((root/'Assets/SuyuRun/Art').rglob(name+'.png'))
        png=candidates[0] if candidates else None;size=None;status='legacy-detail-pending' if png else 'missing-resource'
    rows.append(dict(name=name,contexts=[ctx for ctx,assets in contexts.items() if name in assets],resource=png.relative_to(root).as_posix() if png else None,native=size,status=status))
out=root/'Artifacts/VisualRefactor/LayerPolish';out.mkdir(parents=True,exist_ok=True)
(out/'CAPAS_POR_MAPA.json').write_text(json.dumps(dict(contexts=contexts,uniqueLayers=len(rows),layers=rows),indent=2))
lines=['# Capas reales por nivel y mininivel','', 'Inventario basado en RunnerReferenceCoast, SierraStageDirector, SierraCaves, SelvaStagePresentation y selva_course.json. Compartidas aparecen en cada contexto, sin duplicar su producción. Las posiciones, MIDI y colisiones no cambian.','']
for ctx,assets in contexts.items():
    lines+=['## '+ctx,'']+[f'- {name}: '+next(r['status'] for r in rows if r['name']==name) for name in assets]+['']
lines+=['## Límites','', 'La producción de capas está cubierta cuando todas figuran redrawn-current-lot; eso no certifica un recorrido visual completo en Unity Play ni aprobación estética del usuario. HUD/portales comprobados se conservan; jugabilidad Selva y acabado cinematográfico pospuestos no forman parte de este lote.']
(out/'CAPAS_POR_MAPA.md').write_text('\n'.join(lines),encoding='utf-8')
print(json.dumps(dict(contexts=len(contexts),uniqueLayers=len(rows),redrawn=sum(r['status']=='redrawn-current-lot' for r in rows),legacy=sum(r['status']=='legacy-detail-pending' for r in rows),missing=[r['name'] for r in rows if r['status']=='missing-resource'])))
