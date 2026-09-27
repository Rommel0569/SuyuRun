"""Offline art review, NOT evidence of a playable Unity level."""
from build_sierra_artpack import ROOT, ART, OUT, HEX
from PIL import Image, ImageDraw
import json, math, sys
V3='--v3' in sys.argv
if V3:
    from build_sierra_coast_style import ART, OUT

manifest=json.loads((ART/'sierra_manifest.json').read_text())
specs={a['name']:a for a in manifest['assets']}
cache={}

def frame(name,t=0,index=None):
    name='sierra_'+name if not name.startswith('sierra_') else name
    a=specs[name]
    if name not in cache:cache[name]=Image.open(ART/a['file']).convert('RGBA')
    sheet=cache[name]
    n=int(t*a['fps'])%a['count'] if index is None else index%a['count']
    r=a['frames'][n];y=sheet.height-r['y']-r['height']
    return sheet.crop((r['x'],y,r['x']+r['width'],y+r['height']))

def layer(im,name,t,travel=0,y=0):
    a=specs['sierra_'+name];f=frame(name,t)
    offset=round(travel*a['factor'])%640
    im.alpha_composite(f,(-offset,y));im.alpha_composite(f,(640-offset,y))

def put(im,name,x,y,t=0,index=None):im.alpha_composite(frame(name,t,index),(round(x),round(y)))

def scene(act,t=0):
    im=Image.new('RGBA',(320,180),(0,0,0,255));travel=80+t*15
    layer(im,'compartido_capa1_cielo',t,0)
    layer(im,'compartido_capa2_nubes',t,travel+300)
    layer(im,'compartido_capa3_nevados',t,travel,0 if V3 else -25)
    # Midground uses a contiguous 320px viewport of its independent640px panorama.
    extra=150 if act=='ruinas' else (-80 if V3 else 310) if act=='valle' else 0
    layer(im,act+'_capa4_paisaje',t,travel+extra)
    if act=='valle':
        if V3:layer(im,'valle_capa5_rio',t,travel+extra)
        # Only subtle sparkle sits over the existing distant water, not a second river.
        d=ImageDraw.Draw(im)
        for j in range(0 if V3 else 5):
            x=155+round(math.sin(t*2+j)*8);y=151+j*5
            d.line((x,y,x+2+j,y),fill=HEX[4])
        ground=frame('valle_capa7_suelo');im.alpha_composite(ground.crop((0,0,34,180)),(0,0));im.alpha_composite(ground.crop((0,0,34,180)),(286,0))
        state='estable' if t%8<3 else 'tension' if t%8<5 else 'danado' if t%8<7 else 'colapso'
        put(im,'puente_'+state,32,110,t)
        put(im,'condor_vuelo',205+math.sin(t)*22,45+math.sin(t*2)*5,t)
        put(im,'heroe_huaraca_girar',100,114,t)
        put(im,'piedra',155+(t*50)%90,100-(t*12)%30,t)
    else:
        layer(im,act+'_capa6_cercano',t,travel)
        layer(im,act+'_capa7_suelo',t,travel)
        profile=specs['sierra_'+act+'_capa7_suelo']['surfaceY']
        gx=(round(travel)+105)%640;yy=profile[gx]
        if act=='camino':
            put(im,'llegada_marca_quemada',10,143,t)
            put(im,'llama_caminar',236,profile[(round(travel)+255)%640]-44,t)
            put(im,'heroe_run',83,yy-47,t)
            put(im,'fx_polvo_correr',72,yy-17,t)
        else:
            put(im,'portal_ceremonial',222,30,t)
            put(im,'heroe_run',83,yy-47,t)
            put(im,'coleccionable_kero',178,101,t)
        put(im,'halcon_vuelo',180+math.sin(t*.7)*32,36+math.sin(t)*5,t)
    return im

def main():
    OUT.mkdir(exist_ok=True,parents=True)
    for act in ('camino','valle','ruinas'):
        frames=[scene(act,i/8).resize((1280,720),Image.Resampling.NEAREST).convert('RGB') for i in range(64)]
        frames[0].save(OUT/f'sierra_{act}_preview_x4.png')
        frames[0].save(OUT/f'sierra_{act}_animacion.gif',save_all=True,append_images=frames[1:],duration=125,loop=0,optimize=False)
    # Contact sheet is documentation with labels, never imported as game art.
    overview=Image.new('RGB',(1280,3*754),(26,32,54));d=ImageDraw.Draw(overview)
    for i,act in enumerate(('camino','valle','ruinas')):
        d.text((12,i*754+9),f'SIERRA / {act.upper()} / PROPUESTA V{3 if V3 else 2} - composición de capas independientes',fill='white')
        overview.paste(Image.open(OUT/f'sierra_{act}_preview_x4.png'),(0,i*754+34))
    overview.save(OUT/'SIERRA_TRES_BIOMAS.png')
    sprites=[a for a in manifest['assets'] if a['kind']!='layer']
    pages=[]
    for start in range(0,len(sprites),12):
        page=Image.new('RGB',(1280,720),(43,58,92));d=ImageDraw.Draw(page)
        for k,a in enumerate(sprites[start:start+12]):
            x=(k%3)*426;y=(k//3)*180
            d.text((x+8,y+5),a['name'].replace('sierra_','')+f" [{a['count']}f / {a['fps']}fps]",fill='white')
            f=frame(a['name']);scale=max(1,min(3,400//f.width,145//f.height))
            page.paste(f.resize((f.width*scale,f.height*scale),Image.Resampling.NEAREST),(x+12,y+28),f.resize((f.width*scale,f.height*scale),Image.Resampling.NEAREST))
        path=OUT/f'sierra_catalogo_{start//12+1}.png';page.save(path);pages.append(path.name)
    errors=[];palette={tuple(bytes.fromhex(x[1:])) for x in HEX};total=0
    for a in manifest['assets']:
        im=Image.open(ART/a['file']).convert('RGBA');colors=set(im.getdata())
        if any(c[3] not in (0,255) for c in colors):errors.append(a['name']+': non-binary alpha')
        if any(c[:3] not in palette for c in colors if c[3]):errors.append(a['name']+': off-palette')
        for n in range(a['count']):
            f=frame(a['name'],index=n)
            if not f.getbbox():errors.append(a['name']+f': empty frame {n}')
            if a['kind']=='layer' and a['factor']!=0 and list(f.crop((0,0,1,180)).getdata())!=list(f.crop((639,0,640,180)).getdata()):errors.append(a['name']+f': edge mismatch {n}')
            total+=1
    report=dict(assets=len(manifest['assets']),frames=total,paletteColors=24,errors=errors,
                scope='Validación de PNG/metadatos y previews offline. NO prueba de Unity, física ni ritmo.',catalogs=pages)
    (OUT/'validation.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
    print(json.dumps(report))

if __name__=='__main__':main()
