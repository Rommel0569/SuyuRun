"""Native micro-contours/cluster finish for existing independent regional assets.
Preserves every frame's silhouette, composition and timing. Never overwrites sources.
"""
from pathlib import Path
import json, hashlib, io
from PIL import Image
import build_sierra_artpack as sierra
root=Path(__file__).resolve().parents[1]
bank=root/'Assets/SuyuRun/Art/RefactorRuntime/Resources/RefactorBank'
out=bank/'Materials'
records=[]
mapped_fx={'selva_fx_raiz_erupcion','selva_fx_raiz_amarre','selva_fx_maldicion','selva_fx_impacto','selva_fx_escudo_romper','sierra_impacto_piedra'}
for region,folder in [('sierra',root/'Assets/SuyuRun/Art/PixelSierra/ProductionV3/Resources/SierraV3'),('selva',root/'Assets/SuyuRun/Art/PixelSelva/ProductionV2/Resources/SelvaV2'),('costa',root/'Assets/SuyuRun/Art/PixelCosta/Resources')]:
    colors=sierra.HEX if region=='sierra' else json.loads((root/'Assets/SuyuRun/Art/PixelCosta/costa_palette.json' if region=='costa' else folder/'selva_palette.json').read_text())['colors']
    if region=='selva':colors=[c for c in colors if c not in ('#C4E37A','#7FD4C9','#C67C73','#FFF0AE')]
    palette=[tuple(bytes.fromhex(c[1:])) for c in colors]
    descriptors=[]
    if region=='costa':
        for filename in ('costa_capa1_cielo','costa_capa2_nubes','costa_capa4_mar','costa_capa6_algarrobos','costa_puerta_dimensional','costa_efecto_sorpresa','costa_efecto_absorcion','costa_museo_panel','costa_museo_pared','costa_museo_vitrina','costa_resultados','costa_selector','costa_tienda'):
            image=Image.open(folder/(filename+'.png'));w,h=image.size
            n=4 if filename=='costa_capa4_mar' else 2 if filename=='costa_puerta_dimensional' else 3 if filename=='costa_efecto_sorpresa' else 4 if filename=='costa_efecto_absorcion' else 1
            vertical=filename=='costa_capa4_mar'
            if vertical:h//=n
            else:w//=n
            descriptors.append((filename,dict(frameWidth=w,frameHeight=h,frames=n,fps=4 if vertical else 6,loop=True,rectsUnity=[dict(x=0 if vertical else i*w,y=image.height-(i+1)*h if vertical else 0) for i in range(n)])))
        for part,x,y,w,h in [('panel_h',0,12,32,20),('bar_rafaga_frame',40,22,64,10),('bar_rafaga_fill',82,22,11,10),('bar_progress_frame',120,26,64,6),('bar_progress_fill',162,26,11,6),('piece_slot',200,16,16,16),('icon_puntos',220,20,12,12)]:
            descriptors.append(('costa_ui_'+part,dict(frameWidth=w,frameHeight=h,frames=1,fps=1,loop=True,originalFile='costa_hud.png',rectsUnity=[dict(x=x,y=y)])))
        descriptors.append(('costa_capa2_aves',dict(frameWidth=16,frameHeight=16,frames=18,fps=8,loop=True,rectsUnity=[dict(x=f*16,y=(2-size)*16) for size in range(3) for f in range(6)])))
    else:
        descriptors=[(meta.name.replace('.sprite.json','').replace('.json',''),json.loads(meta.read_text(encoding='utf-8-sig'))) for meta in sorted(folder.glob('*.json'))]
    for name,data in descriptors:
        if region=='sierra':
            if not all(k in data for k in ('width','height','count','frames')):continue
            w,h,n=data['width'],data['height'],data['count'];cells=data['frames']
        else:
            if not all(k in data for k in ('frameWidth','frameHeight','frames','rectsUnity')):continue
            w,h,n=data['frameWidth'],data['frameHeight'],data['frames'];cells=data['rectsUnity']
        if region!='costa' and not ('_capa' in name or '_fx_' in name or '_ui_' in name or '_decor_' in name or name in ('selva_niebla','selva_rayos_sol','selva_agua_fondo','selva_agua_superficie')):continue
        if name in mapped_fx or (bank/'Environment'/(name+'.png')).exists():continue
        existing=out/(name+'.json')
        if existing.exists() and json.loads(existing.read_text()).get('source')!='Tools/finish_remaining_visuals.py':continue
        original=folder/data.get('originalFile',name+'.png')
        if not original.exists():continue
        source=Image.open(original).convert('RGBA');before=hashlib.sha256(original.read_bytes()).hexdigest()
        sheet=Image.new('RGBA',(w*2*n,h*2));lut={}
        def nearest(rgb):
            if rgb not in lut:lut[rgb]=min(palette,key=lambda q:sum((q[k]-rgb[k])**2 for k in range(3)))
            return lut[rgb]
        for i,cell in enumerate(cells):
            y=source.height-cell['y']-h
            old=source.crop((cell['x'],y,cell['x']+w,y+h))
            im=old.resize((2*w,2*h),Image.Resampling.NEAREST)
            im.putdata([(*nearest(p[:3]),p[3]) if p[3] else (0,0,0,0) for p in im.getdata()])
            px=im.load();base=im.copy().load()
            # One native pixel refines stepped colour boundaries. Work only inside
            # opaque clusters; preserve sky/flat voids, alpha and tile endpoints.
            for yy in range(2,2*h-2):
                for xx in range(2,2*w-2):
                    p=base[xx,yy]
                    if not p[3] or any(not base[xx+dx,yy+dy][3] for dx,dy in ((-1,0),(1,0),(0,-1),(0,1))):continue
                    above=base[xx,yy-1];below=base[xx,yy+1]
                    if p[:3]==above[:3] and p[:3]!=below[:3] and (xx%2==1) and abs(sum(p[:3])-sum(below[:3]))<230:
                        px[xx,yy]=(*nearest(tuple(round(p[k]*.75+below[k]*.25) for k in range(3))),p[3])
            if old.crop((0,0,1,h)).tobytes()==old.crop((w-1,0,w,h)).tobytes():im.paste(im.crop((0,0,1,2*h)),(2*w-1,0))
            sheet.alpha_composite(im,(i*2*w,0))
        encoded=io.BytesIO();sheet.save(encoded,format='PNG');target=out/(name+'.png')
        if not target.exists() or target.read_bytes()!=encoded.getvalue():target.write_bytes(encoded.getvalue())
        spec=dict(frames=n,frameWidth=2*w,frameHeight=2*h,logicalWidth=w,logicalHeight=h,pixelsPerUnit=64,fps=data['fps'],loop=data['loop'],original=original.relative_to(root).as_posix(),source='Tools/finish_remaining_visuals.py',finish='native cluster micro-contours; preserved original composition',fullPlaythroughTested=False)
        if not set(source.getchannel('A').getdata())<={0,255}:
            spec['alphaMode']='preserveSource'
            spec['sourceRectsUnity']=[dict(x=c['x'],y=c['y'],width=w,height=h) for c in cells]
        target=out/(name+'.json');content=json.dumps(spec,indent=2)
        if not target.exists() or target.read_text()!=content:target.write_text(content)
        assert hashlib.sha256(original.read_bytes()).hexdigest()==before
        records.append(dict(name=name,frames=n,logical=[w,h]))
(root/'Artifacts/VisualRefactor/remaining_visual_finish.json').write_text(json.dumps(records,indent=2))
print(f'PASS {len(records)} independent sheets finished; original artwork preserved')
