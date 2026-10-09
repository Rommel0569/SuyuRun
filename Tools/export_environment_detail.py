"""Register generated independent layers to existing logical canvas; no repainting."""
from pathlib import Path
import json
from PIL import Image
root=Path(__file__).resolve().parents[1]
tasks=[('sierra','sierra_compartido_capa3_nevados','sierra_nevados_detail_source.png','PixelSierra/ProductionV3/Resources/SierraV3/sierra_compartido_capa3_nevados.png'),
       ('costa','costa_capa3_paracas','costa_paracas_detail_source.png','PixelCosta/costa_capa3_paracas.png'),
       ('selva','selva_bosque_capa3_estructura','selva_bosque_detail_source.png','PixelSelva/ProductionV2/Resources/SelvaV2/selva_bosque_capa3_estructura.png'),
       ('costa','costa_capa5_huacas','costa_huacas_detail_source.png','PixelCosta/costa_capa5_huacas.png'),
       ('sierra','sierra_ruinas_capa4_paisaje','sierra_ruinas_detail_source.png','PixelSierra/ProductionV3/Resources/SierraV3/sierra_ruinas_capa4_paisaje.png'),
       ('selva','selva_templo_capa3_estructura','selva_templo_detail_source.png','PixelSelva/ProductionV2/Resources/SelvaV2/selva_templo_capa3_estructura.png')]
def component_bounds(image,count):
    alpha=image.getchannel('A').load();pending={(x,y) for y in range(image.height) for x in range(image.width) if alpha[x,y]>=128};groups=[]
    while pending:
        first=pending.pop();group=[first];queue=[first]
        while queue:
            x,y=queue.pop()
            for dx in (-1,0,1):
                for dy in (-1,0,1):
                    p=(x+dx,y+dy)
                    if p in pending:pending.remove(p);group.append(p);queue.append(p)
        groups.append(group)
    boxes=[(min(x for x,y in g),min(y for x,y in g),max(x for x,y in g)+1,max(y for x,y in g)+1) for g in sorted(groups,key=len,reverse=True)[:count]]
    assert len(boxes)==count
    return sorted(boxes,key=lambda b:b[0])
out=root/'Assets/SuyuRun/Art/RefactorRuntime/Resources/RefactorBank/Environment';out.mkdir(parents=True,exist_ok=True)
for region,name,source,legacy in tasks:
    im=Image.open(root/'Artifacts/VisualRefactor'/source).convert('RGBA')
    im.putalpha(im.getchannel('A').point(lambda a:255 if a>=128 else 0))
    old=Image.open(root/'Assets/SuyuRun/Art'/legacy).convert('RGBA')
    assert old.size==(640,180),old.size
    oldbox=old.getbbox();newbox=im.getbbox();assert newbox and oldbox
    # Preserve the legacy layer's occupied rectangle and its vertical baseline.
    dst=Image.new('RGBA',(1280,360))
    registrations=list(zip(component_bounds(im,3),component_bounds(old,3))) if name=='costa_capa5_huacas' else [(newbox,oldbox)]
    palette=root/(f'Assets/SuyuRun/Art/Pixel{region.title()}/{region}_palette.json' if region!='selva' else 'Assets/SuyuRun/Art/PixelSelva/ProductionV2/Resources/SelvaV2/selva_palette.json')
    colors=json.loads(palette.read_text())['colors']
    if region=='selva':colors=[c for c in colors if c not in ('#C4E37A','#7FD4C9','#C67C73','#FFF0AE')]
    colors=[tuple(bytes.fromhex(c[1:])) for c in colors];assert len(colors)==24
    lut={}
    def quantize(p):
        if not p[3]:return (0,0,0,0)
        rgb=p[:3]
        if rgb not in lut:lut[rgb]=min(colors,key=lambda q:sum((q[k]-rgb[k])**2 for k in range(3)))
        return (*lut[rgb],255)
    for srcbox,dstbox in registrations:
        content=im.crop(srcbox).resize(((dstbox[2]-dstbox[0])*2,(dstbox[3]-dstbox[1])*2),Image.Resampling.NEAREST)
        content.putdata([quantize(p) for p in content.getdata()]);dst.alpha_composite(content,(dstbox[0]*2,dstbox[1]*2))
    # Match the endpoint column exactly for the existing two-copy recycler.
    dst.paste(dst.crop((0,0,1,360)),(1279,0))
    dst.save(out/(name+'.png'))
    spec=dict(frames=1,frameWidth=1280,frameHeight=360,pixelsPerUnit=64,logicalWidth=640,logicalHeight=180,
              fps=1,loop=True,source=source,original=legacy,sourceBounds=list(newbox),registeredBounds=list(oldbox),seamEndpointEqual=True,fullPlaythroughTested=False)
    (out/(name+'.json')).write_text(json.dumps(spec,indent=2))
    assert set(dst.getchannel('A').getdata())<={0,255}
    assert len({p[:3] for p in dst.getdata() if p[3]})<=24
    print(name,'native 1280x360 -> logical 640x180; original retained')
