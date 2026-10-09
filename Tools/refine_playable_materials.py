"""Editable native material finish; no hitbox, timing or silhouette changes."""
from pathlib import Path
import json,hashlib
from PIL import Image,ImageDraw
import build_sierra_artpack as sierra
root=Path(__file__).resolve().parents[1]
out=root/'Assets/SuyuRun/Art/RefactorRuntime/Resources/RefactorBank/Materials'
out.mkdir(parents=True,exist_ok=True)
tasks={
'sierra_ruinas_escalera':'stone','sierra_ruinas_rampa':'stone','sierra_ruinas_pilar':'stone',
'sierra_cueva_plataforma':'stone','sierra_cueva_roca':'stone','sierra_piedra':'stone',
'sierra_pinchos':'spikes','sierra_tronco':'wood',
'selva_plataforma_piedra':'stone','selva_plataforma_rampa':'stone',
'selva_plataforma_puente':'wood','selva_plataforma_bote':'wood',
'selva_plataforma_victoria':'leaf','selva_obstaculo_techo':'wood',
'selva_obstaculo_raiz':'wood','selva_obstaculo_pinchos':'spikes',
'selva_obstaculo_estaca':'wood','selva_obstaculo_trampa':'stone'}
records=[]
for name in ('sierra_camino_capa7_suelo','sierra_valle_capa7_suelo','sierra_ruinas_capa7_suelo','sierra_cueva_suelo'):
    tasks[name]='earth' if 'camino' in name or 'valle' in name else 'stone'
for tile in ('tierra','roca','cueva','ruina'):
    for part in ('superficie','relleno','borde_izq','borde_der'):
        tasks[f'selva_tile_{tile}_{part}']='earth' if tile=='tierra' else 'stone'
for part in ('bloque','rampa','pilar'):tasks['costa_plataforma_'+part]='stone'
tasks['costa_capa7_suelo']='earth'
for name in ('sierra_tunel_entrada','sierra_tunel_salida','sierra_portal_ceremonial','selva_portal_salida'):
    tasks[name]='stone'
for name,kind in tasks.items():
    region=name.split('_')[0]
    folder=root/('Assets/SuyuRun/Art/PixelCosta/Resources' if region=='costa' else 'Assets/SuyuRun/Art/PixelSierra/ProductionV3/Resources/SierraV3' if region=='sierra' else 'Assets/SuyuRun/Art/PixelSelva/ProductionV2/Resources/SelvaV2')
    if region=='costa':
        original=folder/('costa_plataformas.png' if 'plataforma_' in name else name+'.png')
        source=Image.open(original).convert('RGBA');before=hashlib.sha256(original.read_bytes()).hexdigest()
        crop={'bloque':(0,0,48,32),'rampa':(56,0,32,32),'pilar':(96,0,16,48)}.get(name.split('_')[-1],(0,0,32,32))
        x,y,w,h=crop;count=1;spec={'fps':1,'loop':True,'rectsUnity':[{'x':x,'y':y}]}
    else:
        original=folder/(name+'.png');before=hashlib.sha256(original.read_bytes()).hexdigest()
        spec=json.loads((folder/(name+('.sprite.json' if region=='sierra' else '.json'))).read_text())
        w,h,count=(spec['width'],spec['height'],spec['count']) if region=='sierra' else (spec['frameWidth'],spec['frameHeight'],spec['frames'])
        source=Image.open(original).convert('RGBA')
    palette=sierra.HEX if region=='sierra' else json.loads((root/'Assets/SuyuRun/Art/PixelCosta/costa_palette.json' if region=='costa' else folder/'selva_palette.json').read_text())['colors']
    if region=='selva':palette=[c for c in palette if c not in ('#C4E37A','#7FD4C9','#C67C73','#FFF0AE')]
    colors=[tuple(bytes.fromhex(c[1:]))+(255,) for c in palette]
    assert len(colors)==24
    lookup={}
    def col(rgb):
        if rgb not in lookup:lookup[rgb]=min(colors,key=lambda q:sum((q[k]-rgb[k])**2 for k in range(3)))
        return lookup[rgb]
    def color(rgb):return col(tuple(rgb))
    dark=color((35,38,36));light=color((184,184,162));mid=color((102,110,96))
    if kind=='wood':dark=color((65,47,32));light=color((169,123,65));mid=color((109,77,48))
    if kind=='leaf':dark=color((27,61,34));light=color((150,201,84));mid=color((74,122,62))
    result=Image.new('RGBA',(w*count*2,h*2));cells=spec['frames'] if region=='sierra' else spec['rectsUnity']
    for index,cell in enumerate(cells):
        y=source.height-cell['y']-h
        old=source.crop((cell['x'],y,cell['x']+w,y+h))
        frame=old.resize((w*2,h*2),Image.Resampling.NEAREST)
        frame.putdata([(0,0,0,0) if p[3]<128 else col(p[:3]) for p in frame.getdata()])
        alpha=frame.getchannel('A');mask=alpha.load();d=ImageDraw.Draw(frame)
        # Native one-pixel bevels follow the exact original silhouette.
        for x in range(1,frame.width-1):
            ys=[y for y in range(frame.height) if mask[x,y]>=128]
            if not ys:continue
            top,bottom=ys[0],ys[-1]
            if bottom-top>4:
                d.point((x,top+1),fill=light)
                d.point((x,bottom-1),fill=dark)
        if kind=='stone':
            for row,y in enumerate(range(8,h*2-2,14)):
                for x in range(-((row%2)*13),w*2,27):
                    d.line([(x+2,y),(x+23,y)],fill=dark)
                    d.line([(x+3,y+2),(x+21,y+2)],fill=light)
                    d.line([(x+24,y+2),(x+24,y+11)],fill=dark)
                    if (row+x//27)%3==0:d.line([(x+9,y+5),(x+13,y+4)],fill=mid)
        elif kind=='wood':
            for row,y in enumerate(range(5,h*2-2,6)):
                for x in range((row%3)*4,w*2,23):
                    d.line([(x,y),(x+11,y),(x+14,y+1)],fill=dark)
                    d.line([(x+2,y+2),(x+10,y+2)],fill=light)
            if w>=40 and h>=12:
                x=w;y=h
                d.ellipse((x-6,y-3,x+6,y+3),outline=dark)
                d.line([(x-3,y),(x+3,y)],fill=mid)
        elif kind=='leaf':
            cx,cy=w,h
            for dx in (-32,-20,-10,10,20,32):
                d.line([(cx,cy+4),(cx+dx,cy-5)],fill=mid)
            d.line([(cx-30,cy+2),(cx+30,cy+2)],fill=light)
        elif kind=='earth':
            for x,y in ((7,19),(29,37),(49,23),(17,51)):
                if y<frame.height and x<frame.width:
                    d.polygon([(x,y),(x+4,y-1),(x+6,y+1),(x+3,y+3)],fill=mid)
                    d.line([(x+1,y),(x+3,y)],fill=light)
        else:
            for x in range(4,w*2,12):d.line([(x,h),(x+3,h+7)],fill=light)
        # Texture strokes are clipped to the interior so seams never erase
        # the outline or invent collision-looking extensions.
        detailed=frame.copy();base=old.resize(frame.size,Image.Resampling.NEAREST)
        base.putdata([(0,0,0,0) if p[3]<128 else col(p[:3]) for p in base.getdata()])
        for y in range(frame.height):
            for x in range(frame.width):
                original_pixel=base.getpixel((x,y))
                protected=kind=='stone' and (sum(original_pixel[:3])<160 or original_pixel[0]>180 and original_pixel[0]>original_pixel[1]*1.25)
                if protected or not mask[x,y] or x<2 or y<2 or x>=frame.width-2 or y>=frame.height-2 or any(not mask[x+dx,y+dy] for dx,dy in ((-1,0),(1,0),(0,-1),(0,1))):
                    detailed.putpixel((x,y),base.getpixel((x,y)))
        detailed.putalpha(alpha)
        assert detailed.getchannel('A').tobytes()==old.getchannel('A').resize(frame.size,Image.Resampling.NEAREST).tobytes()
        result.alpha_composite(detailed,(index*w*2,0))
    result.save(out/(name+'.png'))
    data=dict(frames=count,frameWidth=w*2,frameHeight=h*2,logicalWidth=w,logicalHeight=h,pixelsPerUnit=64,fps=spec['fps'],loop=spec['loop'],material=kind,original=original.relative_to(root).as_posix(),source='Tools/refine_playable_materials.py',fullPlaythroughTested=False)
    (out/(name+'.json')).write_text(json.dumps(data,indent=2),encoding='utf-8')
    assert hashlib.sha256(original.read_bytes()).hexdigest()==before
    assert len({p[:3] for p in result.getdata() if p[3]})<=24
    records.append(dict(name=name,frames=count,logical=[w,h],material=kind))
(root/'Artifacts/VisualRefactor/playable_materials.json').write_text(json.dumps(records,indent=2),encoding='utf-8')
print(f'PASS {len(records)} material sheets; native detail, original alpha, animation cells and source hashes preserved')
