"""Art review/validation only, not a gameplay capture or runtime."""
import build_selva_artpack as b
from PIL import Image, ImageDraw, ImageFont
from pathlib import Path
import json, math, hashlib, zipfile, shutil

def sprite(name,f=0):return b.FRAMES[name][f%len(b.FRAMES[name])]
def blit(im,name,x,y,f=0):im.alpha_composite(sprite(name,f),(round(x),round(y)))

def scene(zone,t=0,travel=0):
    im=b.canvas(320,180)
    for spec in b.REG:
        if spec.get('zone')!=zone or not spec['file'].startswith('Layers/'):continue
        if 'frontal' in spec['name']:continue
        fr=sprite(spec['name'],int(t*spec['fps']))
        off=int(travel*spec['parallax'])%640 if spec['seamlessX'] else 0
        im.alpha_composite(fr,(-off,0))
        if off>320:im.alpha_composite(fr,(640-off,0))
    # Review layouts only. All objects are separate images; no baked gameplay.
    f=int(t*8)
    if zone in ('bosque','noche','templo'):
        for x in range(0,320,32):blit(im,'selva_tile_tierra_superficie',x,153)
        for x in (24,273):blit(im,'selva_flor',x,125,f)
        blit(im,'selva_heroe_run',67,106,int(t*12))
        blit(im,'selva_obstaculo_pinchos',165,122,f)
        blit(im,'selva_plataforma_tronco',226,133)
        blit(im,'selva_guacamayo_movimiento',211,49,f)
        if zone=='noche':
            for x,y in [(20,80),(46,116),(127,102),(208,79),(285,126)]:blit(im,'selva_luciernaga_movimiento',x,y,f+x)
        if zone=='templo':blit(im,'selva_portal_salida',245,58,f)
    elif zone=='rio':
        for x in (0,32,272,304):blit(im,'selva_tile_tierra_superficie',x,154)
        for x in (70,183):blit(im,'selva_plataforma_victoria',x,161,f)
        for x in (87,228):blit(im,'selva_caiman_nadar',x,149,f)
        # Same hand socket as hero and same moving grip as liana frame.
        for x,phase in [(35,0),(151,2)]:
            index=(f+phase)%8;blit(im,'selva_decor_rama_anclaje',x+31,-15);blit(im,'selva_liana_balanceo',x,1,index)
            if phase==0:
                spec=next(s for s in b.REG if s['name']=='selva_liana_balanceo')
                gx,gy=spec['gripPerFrame'][index]
                blit(im,'selva_heroe_swing',x+gx-34,1+gy-1,index)
        blit(im,'selva_cascada',283,51,f)
    else:
        material='cueva' if zone=='cueva' else 'ruina'
        for x in range(0,320,32):blit(im,'selva_tile_'+material+'_superficie',x,157)
        for x,y in [(36,113),(108,77),(198,104)]:blit(im,'selva_plataforma_piedra',x,y)
        for x,y in [(20,55),(149,34),(274,62)]:blit(im,'selva_antorcha',x,y,f)
        blit(im,'selva_heroe_burst',42,110,f)
        blit(im,'selva_boss_'+('raices' if zone=='cueva' else 'escarabajo')+'_idle',211,70,f)
        blit(im,'selva_murcielago_movimiento',93,34,f)
        blit(im,'selva_serpiente_movimiento',136,128,f)
    for spec in b.REG:
        if spec.get('zone')==zone and 'frontal' in spec['name']:
            fr=sprite(spec['name']);off=int(travel*1.3)%640;im.alpha_composite(fr,(-off,0))
            if off>320:im.alpha_composite(fr,(640-off,0))
    return im

def traversal_demo():
    frames=[];sockets=next(s for s in b.REG if s['name']=='selva_liana_balanceo')['gripPerFrame']
    for f in range(64):
        im=b.canvas(320,180)
        for s in b.REG:
            if s.get('zone')=='rio' and s['file'].startswith('Layers/') and 'frontal' not in s['name']:
                im.alpha_composite(sprite(s['name'],f//2),(0,0))
        first=min(4,f//5) if f<=20 else (4+(f-20)//2)%8
        second=0 if f<37 else ((f-37)//2)%8
        blit(im,'selva_decor_rama_anclaje',51,-15);blit(im,'selva_decor_rama_anclaje',219,-15)
        blit(im,'selva_liana_balanceo',20,1,first);blit(im,'selva_liana_balanceo',188,1,second)
        for x,phase in [(83,0),(209,3)]:blit(im,'selva_caiman_salto',x,149,(f//4+phase)%6)
        if f<=20:
            # First20frames lead towards the outward arc; keep hand attached.
            idx=first;gx,gy=sockets[idx];state='swing';hf=idx;hx=20+gx;hy=1+gy
        elif f<37:
            a=(f-21)/15;start=sockets[4];end=sockets[0]
            hx=20+start[0]+(188+end[0]-(20+start[0]))*a
            hy=1+start[1]+(end[1]-start[1])*a-32*math.sin(math.pi*a)
            state='release' if a<.25 else 'jump' if a<.65 else 'regrab';hf=min(2,int(a*3))
        else:
            gx,gy=sockets[second];hx=188+gx;hy=1+gy;state='swing';hf=second
        spec=next(s for s in b.REG if s['name']=='selva_heroe_'+state);hand=spec['handPerFrame'][hf]
        blit(im,'selva_heroe_'+state,hx-hand[0],hy-hand[1],hf)
        for x in (0,32,272,304):blit(im,'selva_tile_tierra_superficie',x,159)
        frames.append(im.resize((1280,720),Image.Resampling.NEAREST).convert('RGB'))
    frames[0].save(b.PRE/'lianas_transferencia.gif',save_all=True,append_images=frames[1:],duration=90,loop=0)
    strip=Image.new('RGB',(1280,720))
    for i,f in enumerate((16,27,36,43)):
        strip.paste(frames[f].resize((640,360),Image.Resampling.NEAREST),((i%2)*640,(i//2)*360))
    strip.save(b.PRE/'lianas_secuencia.png')

def run():
    b.run()
    zones=['bosque','rio','cueva','ruinas','noche','templo']
    contact=Image.new('RGB',(1280,1128),(13,20,24));draw=ImageDraw.Draw(contact)
    for i,z in enumerate(zones):
        frame=scene(z);frame.resize((1280,720),Image.Resampling.NEAREST).save(b.PRE/(z+'_x4.png'))
        x=i%2*640;y=i//2*376
        contact.paste(frame.resize((640,360),Image.Resampling.NEAREST),(x,y+16));draw.text((x+8,y+2),z.upper(),fill='#F0D166')
        frames=[scene(z,f/8,0 if z in ('cueva','ruinas') else f*2).resize((960,540),Image.Resampling.NEAREST).convert('RGB') for f in range(32)]
        frames[0].save(b.PRE/(z+'_animado.gif'),save_all=True,append_images=frames[1:],duration=125,loop=0)
    contact.save(b.PRE/'selva_seis_ambientes.png')
    traversal_demo()
    comparison=Image.new('RGB',(1280,720*3),(13,20,24))
    for i,p in enumerate([b.ROOT/'Artifacts/ArtApproval/costa_capa7_suelo_preview_x4.png',b.ROOT/'Artifacts/SierraV3/sierra_camino_preview_x4.png',b.PRE/'bosque_x4.png']):
        if not p.exists():p=b.ROOT/'SourceReferences'/p.name
        src=Image.open(p).convert('RGB');comparison.paste(src.resize((1280,720),Image.Resampling.NEAREST),(0,i*720))
    comparison.save(b.PRE/'comparacion_costa_sierra_selva.png')
    # Visible animation proof with a checker background (not baked into sprite PNGs).
    for category in ('Hero','Enemies','Bosses','Traversal','Fauna','Effects','Props','Collectibles','Museum','UI'):
        specs=[a for a in b.REG if a['file'].startswith(category+'/')]
        w=640;rows=(len(specs)+3)//4;frames=[]
        for f in range(12):
            im=Image.new('RGB',(w,rows*140),(13,20,24));d=ImageDraw.Draw(im)
            for j,s in enumerate(specs):
                x=j%4*160;y=j//4*140
                fr=sprite(s['name'],f if s['loop'] else min(f,s['frames']-1))
                scale=min(2,144//fr.width,112//fr.height);scale=max(1,scale)
                fr=fr.resize((fr.width*scale,fr.height*scale),Image.Resampling.NEAREST)
                im.paste(fr,(x+(160-fr.width)//2,y+12+(112-fr.height)//2),fr)
                d.text((x+4,y+123),s['name'].replace('selva_','')[:25],fill='#F0D166')
            frames.append(im)
        frames[0].save(b.PRE/(category.lower()+'_contact.png'))
        frames[0].save(b.PRE/(category.lower()+'_animado.gif'),save_all=True,append_images=frames[1:],duration=125,loop=0)
    # Technical validation on every source PNG, not enlarged previews.
    checks=[];colors=set();fail=[]
    for s in b.REG:
        p=b.ART/s['file'];im=Image.open(p).convert('RGBA');used=set(im.getdata());opaque={c for c in used if c[3]};colors|=opaque
        ok=opaque.issubset(set(b.C)) and all(c[3] in (0,255) for c in used)
        seam=all(list(fr.crop((0,0,1,fr.height)).getdata())==list(fr.crop((fr.width-1,0,fr.width,fr.height)).getdata()) for fr in b.FRAMES[s['name']]) if s['seamlessX'] else True
        distinct=len({hashlib.sha256(fr.tobytes()).hexdigest() for fr in b.FRAMES[s['name']]})
        if not(ok and seam):fail.append(s['name'])
        checks.append(dict(name=s['name'],size=im.size,colors=len(opaque),palette=ok,seam=seam,uniqueFrames=distinct,sha256=hashlib.sha256(p.read_bytes()).hexdigest()))
    report=dict(pngs=len(checks),frames=sum(s['frames'] for s in b.REG),paletteColorsUsed=len(colors),failures=fail,checks=checks)
    (b.OUT/'validation.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
    # Seam inspection strip: 32px on either side of each independent layer seam.
    strips=[s for s in b.REG if s['file'].startswith('Layers/') and s['seamlessX']]
    seams=Image.new('RGB',(max(1,len(strips))*64,180),(13,20,24))
    for i,s in enumerate(strips):
        fr=sprite(s['name']);strip=b.canvas(64,180)
        strip.alpha_composite(fr.crop((608,0,640,180)),(0,0));strip.alpha_composite(fr.crop((0,0,32,180)),(32,0))
        seams.paste(strip,(i*64,0),strip)
    seams.save(b.PRE/'costuras_capas.png')
    print(json.dumps({k:v for k,v in report.items() if k!='checks'}))
    assert not fail

if __name__=='__main__':run()
