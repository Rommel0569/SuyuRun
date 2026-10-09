"""Bundle all existing game art plus versioned refactor without hiding legacy."""
from pathlib import Path
import hashlib,json,zipfile

root=Path(__file__).resolve().parents[1];out=root/'Artifacts/VisualRefactor'
art=root/'Assets/SuyuRun/Art';records=[]
for file in sorted(art.rglob('*')):
    if file.is_file():
        records.append(dict(path=file.relative_to(root).as_posix(),bytes=file.stat().st_size,
            sha256=hashlib.sha256(file.read_bytes()).hexdigest(),status='existing-game-art-not-new-refactor'))
(out/'base_art_inventory.json').write_text(json.dumps(dict(files=len(records),entries=records),indent=2))
archive=out/'Pack_Arte_Completo_Base_y_Refactor.zip'
with zipfile.ZipFile(archive,'w',zipfile.ZIP_DEFLATED) as z:
    for item in records:
        path=root/item['path'];z.write(path,'BaseJuego/'+item['path'])
    for path in sorted((out/'Bank').rglob('*')):
        if path.is_file():z.write(path,'Refactor/'+path.relative_to(out).as_posix())
    for name in ['README.md','PACK_COMPLETO.md','UNITY_IMPORT.md','unity_catalog.json','base_art_inventory.json','bank_prompts.md','additional_prompts.md','ESTADO_ACTUAL.md','GLOBAL_SCALE_AND_DETAIL.md','global_engineering_audit.txt','physics_models.txt','human_gait_audit.txt','runtime_audit.txt','play_smoke.txt']:
        file=out/name
        if file.exists():z.write(file,'Refactor/'+name)
    for folder in ['Bosses','Heroes','Actors']:
        for file in sorted((out/folder).glob('*prompt.txt')):z.write(file,'Refactor/'+file.relative_to(out).as_posix())
    for file in sorted(out.glob('*detail_source.png')):z.write(file,'Refactor/EnvironmentProposals/'+file.name)
    for file in sorted(out.glob('*detail_prompt.txt')):z.write(file,'Refactor/EnvironmentPrompts/'+file.name)
    z.write(root/'Assets/SuyuRun/Editor/ImportVisualRefactorBank.cs','Refactor/Editor/ImportVisualRefactorBank.cs')
    for folder in ['Scripts','Editor']:
        for file in sorted((root/'Assets/SuyuRun'/folder).glob('*.cs')):
            z.write(file,'Integration/'+folder+'/'+file.name)
    for file in sorted((root/'Tools').glob('*refactor*.py')):
        z.write(file,'Integration/Tools/'+file.name)
    z.write(root/'Tools/export_human_gait.py','Integration/Tools/export_human_gait.py')
    z.write(root/'Tools/export_environment_detail.py','Integration/Tools/export_environment_detail.py')
    z.write(root/'Tools/enhance_obstacle_readability.py','Integration/Tools/enhance_obstacle_readability.py')
    for file in sorted((root/'Assets/SuyuRun/Audio/Resources/Sfx').glob('*')):
        if file.is_file():z.write(file,'Integration/Audio/Resources/Sfx/'+file.name)
    for name in ['Kenney_Digital_Audio_License.txt','SHOT_SOURCE.md']:
        file=root/'Artifacts/Audio'/name
        if file.exists():z.write(file,'Integration/Audio/'+name)
    for name in ['CIERRE_08_OCT.md','door_audit.txt','revive_audit.txt']:
        file=out/name
        if file.exists():z.write(file,'Refactor/'+name)
    for name in ['check_physics_models.ps1','compile_refactor_check.ps1','check_portal_centres.ps1','check_hud_layout.ps1','check_animation_clocks.ps1','audit_runtime_art.py','register_cave_detail.py']:
        z.write(root/'Tools'/name,'Integration/Tools/'+name)
    for name in ['ORIENTACION_PORTALES_04_OCT.md','HUD_VIEWPORT_04_OCT.md','environment_detail_prompts_04_oct.md','runtime_texture_contracts.json','play_visual_tour.txt','LOTE_MATERIALES_04_OCT.md','playable_materials.json','playable_materials_contact.png','material_runtime_audit.txt','animation_clock_audit.txt','FLUIDEZ_04_OCT.md','remaining_visual_finish.json','CIERRE_REFACTOR_VISUAL.md','pixel_viewport_audit.txt']:
        path=out/name
        if path.exists():z.write(path,'Refactor/'+name)
    for name in ['refine_playable_materials.py','refine_ruins_block.py','preview_material_finish.py','finish_remaining_visuals.py','build_sierra_artpack.py']:
        path=root/'Tools'/name
        if path.exists():z.write(path,'Integration/Tools/'+name)
print(json.dumps(dict(archive=str(archive),baseFiles=len(records),bytes=archive.stat().st_size,
    note='Complete versioned art library and integration; diagnostic tests included, not certification of a full chronological playthrough')))
