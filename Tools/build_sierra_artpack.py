"""Sierra production V2. Native pixel sprites; never writes Costa or previous Sierra.

Run after reading ART_BIBLE.md + Docs/SIERRA_PRODUCTION_BRIEF.md. Layer sources made
with image_gen stay independent; this tool only normalizes each single layer and
authors editable game modules/animation sheets. Pillow authorized by the art bible.
"""
from pathlib import Path
from PIL import Image, ImageDraw
import math, random, json, hashlib, argparse

ROOT=Path(__file__).resolve().parents[1]
PACK=ROOT/'Assets/SuyuRun/Art/PixelSierra/ProductionV2'
ART=PACK/'Resources/SierraV2'
OUT=ROOT/'Artifacts/SierraV2'
SOURCES=ROOT/'SourceMedia/SierraV2'
HEX=['#1A2036','#2B3A5C','#3E6485','#5F8FA8','#9BC4C6','#E8EDF2',
     '#FFF3D0','#F5DFA0','#D8B46B','#AD7559','#835746','#5B4A43',
     '#4C5553','#788078','#B8B8A2','#D9CFAD','#263D2E','#3E5940',
     '#648346','#9EAD56','#CBBD61','#8B2E2E','#C2674F','#ED9254']
C=[tuple(bytes.fromhex(h[1:]))+(255,) for h in HEX]
T=(0,0,0,0)
registry=[]
def canvas(w=640,h=180):return Image.new('RGBA',(w,h),T)
def poly(d,pts,c,outline=None):d.polygon([(round(x),round(y)) for x,y in pts],fill=C[c],outline=C[outline] if outline is not None else None)
def line(d,pts,c,width=1):d.line([(round(x),round(y)) for x,y in pts],fill=C[c],width=width)
def rect(d,b,c):d.rectangle(tuple(round(x) for x in b),fill=C[c])
def ellipse(d,b,c):d.ellipse(tuple(round(x) for x in b),fill=C[c])
def nearest_palette(im):
    im=im.convert('RGBA');rgb=im.convert('RGB')
    palette=Image.new('P',(1,1));palette.putpalette([v for c in C for v in c[:3]]+list(C[0][:3])*(256-24))
    result=rgb.quantize(palette=palette,dither=Image.Dither.NONE).convert('RGBA')
    result.putalpha(im.getchannel('A').point(lambda x:255 if x>=128 else 0))
    result.paste(T,(0,0,result.width,result.height),result.getchannel('A').point(lambda x:255-x))
    return result
def save(name,frames,*,fps=6,loop=True,kind='sprite',factor=1,biome='shared',pivot=(.5,0),notes='',surface=None):
    if isinstance(frames,Image.Image):frames=[frames]
    w,h=frames[0].size
    vertical=kind=='layer';sheet=canvas(w if vertical else w*len(frames),h*len(frames) if vertical else h)
    cells=[]
    for i,f in enumerate(frames):
        assert f.size==(w,h)
        x,y=(0,i*h) if vertical else (i*w,0)
        sheet.alpha_composite(f,(x,y))
        cells.append(dict(name=f'{name}_{i}',x=x,y=sheet.height-y-h,width=w,height=h))
    sheet.save(ART/(name+'.png'))
    spec=dict(name=name,file=name+'.png',width=w,height=h,count=len(frames),fps=fps,loop=loop,
              kind=kind,factor=factor,biome=biome,pivotX=pivot[0],pivotY=pivot[1],ppu=32,frames=cells,notes=notes)
    if surface is not None:spec['surfaceY']=surface
    (ART/(name+'.sprite.json')).write_text(json.dumps(spec,indent=2),encoding='utf-8')
    registry.append(spec);return frames

def sky():
    im=canvas();d=ImageDraw.Draw(im)
    for y,c in [(0,2),(34,3),(87,4),(138,7),(167,6)]:rect(d,(0,y,639,179),c)
    # Dither ONLY the sky, in narrow ordered transitions, never vegetation/stone.
    for y0,a,b in [(34,2,3),(87,3,4),(138,4,7),(167,7,6)]:
        for y in range(y0,y0+5):
            for x in range(640):
                if (x*3+y)%5>y-y0:im.putpixel((x,y),C[a])
    ellipse(d,(418,88,458,128),6);rect(d,(417,112,459,114),7)
    return save('sierra_compartido_capa1_cielo',im,kind='layer',factor=0,pivot=(0,1),notes='Cielo fijo: excepción a tile. Amanecer, no atardecer de Costa.')

def cloud(d,x,y,w,h):
    pts=[(x,y+h),(x+2,y+h-4),(x+w*.15,y+h-4),(x+w*.19,y+h*.4),(x+w*.34,y+h*.4),
         (x+w*.4,y),(x+w*.57,y),(x+w*.63,y+h*.3),(x+w*.77,y+h*.3),(x+w*.8,y+h*.6),(x+w-2,y+h*.6),(x+w,y+h)]
    poly(d,pts,4);poly(d,[(a,b-2) for a,b in pts[:-1]]+[(x+w-3,y+h-4),(x+3,y+h-4)],5)
    line(d,[(x+8,y+h-7),(x+w*.28,y+h-7),(x+w*.35,y+h-10),(x+w*.54,y+h-10)],6)

def shared_layers():
    sky();im=canvas();d=ImageDraw.Draw(im)
    for x,y,w,h in [(32,16,110,18),(225,33,96,13),(419,9,116,21),(557,48,62,11)]:cloud(d,x,y,w,h)
    save('sierra_compartido_capa2_nubes',im,kind='layer',factor=.05,pivot=(0,1))
    im=canvas();d=ImageDraw.Draw(im)
    for cx,top,wide in [(-25,72,100),(70,47,105),(201,28,122),(352,55,108),(477,37,120),(615,72,100)]:
        pts=[(cx-wide,160),(cx-wide*.64,top+53),(cx-wide*.49,top+49),(cx-wide*.31,top+20),(cx-wide*.17,top+23),(cx,top),
             (cx+wide*.13,top+16),(cx+wide*.22,top+19),(cx+wide*.39,top+43),(cx+wide*.54,top+39),(cx+wide,161)]
        poly(d,pts,2);poly(d,[(cx,top),(cx+wide*.13,top+16),(cx+wide*.22,top+19),(cx+wide*.54,top+39),(cx+wide,161),(cx+wide*.24,150),(cx-wide*.1,70+top)],3)
        poly(d,[(cx-wide*.31,top+20),(cx-wide*.17,top+23),(cx,top),(cx+wide*.22,top+19),(cx+wide*.33,top+35),(cx+wide*.1,top+27),(cx+3,top+44),(cx-wide*.09,top+23),(cx-wide*.23,top+40)],5)
        line(d,[(cx,top+3),(cx-3,top+20),(cx-wide*.09,top+31)],4,2)
        for j in range(4):line(d,[(cx+wide*.2+j*7,top+46+j*9),(cx+wide*.33+j*7,top+61+j*9)],4)
    # Matching non-empty end columns: no artificial transparent tear at the seam.
    im.paste(im.crop((0,0,1,180)),(639,0))
    if (SOURCES/'nevados.png').exists():
        im=nearest_palette(Image.open(SOURCES/'nevados.png').convert('RGBA').resize((640,180),Image.Resampling.NEAREST))
        # Gentle mirrored edge collars avoid an unmatched exposed silhouette.
        im.paste(im.crop((0,0,12,180)).transpose(Image.Transpose.FLIP_LEFT_RIGHT),(628,0))
    save('sierra_compartido_capa3_nevados',im,kind='layer',factor=.12,pivot=(0,1))

def tree(d,x,y,s=1,wind=0):
    def p(dx,dy):return (x+round(dx*s),y+round(dy*s))
    line(d,[p(-2,0),p(1,-20),p(-2,-34),p(4,-49)],0,max(1,round(6*s)))
    line(d,[p(-2,-2),p(1,-20),p(-2,-34),p(4,-49)],10,max(1,round(3*s)))
    for dx,dy in [(-11,-33),(12,-38),(-8,-49),(9,-56)]:
        line(d,[p(0,-19),p(dx*.5,dy*.65),p(dx,dy)],11,max(1,round(3*s)))
        xx,yy=p(dx+wind,dy)
        poly(d,[(xx-13*s,yy+6*s),(xx-15*s,yy),(xx-9*s,yy-7*s),(xx-3*s,yy-9*s),(xx+3*s,yy-12*s),(xx+10*s,yy-5*s),(xx+13*s,yy+3*s),(xx+6*s,yy+9*s)],16)
        poly(d,[(xx-11*s,yy),(xx-6*s,yy-6*s),(xx+1*s,yy-9*s),(xx+7*s,yy-4*s),(xx+9*s,yy+1*s),(xx+2*s,yy+6*s),(xx-4*s,yy+3*s)],18)
        line(d,[(xx-7*s,yy-2*s),(xx-2*s,yy-6*s),(xx+3*s,yy-5*s)],19,max(1,round(2*s)))

def grass(d,x,y,wind=0,scale=1):
    for i,(dx,h) in enumerate([(-5,5),(-3,8),(0,10),(3,7),(5,4)]):
        line(d,[(x,y),(x+round((dx+wind)*scale),y-round(h*scale))],18 if i%2 else 20)
        if h>6:d.point((x+round((dx+wind)*scale),y-round(h*scale)),fill=C[7])

def fence(d,x,y,w=72):
    for row in (9,18):
        poly(d,[(x,y-row),(x+w,y-row-3),(x+w,y-row),(x,y-row+3)],11,0)
        line(d,[(x+1,y-row),(x+w-1,y-row-3)],8)
    for xx in range(x,x+w+1,18):
        poly(d,[(xx-2,y),(xx-2,y-27),(xx,y-30),(xx+3,y-27),(xx+3,y)],10,0)
        line(d,[(xx,y-27),(xx,y-3)],9);line(d,[(xx-2,y-16),(xx+3,y-16)],7)

def terrain_profile(act):
    if act=='camino':return [round(148+8*math.sin(x*2*math.pi/320)+5*math.sin(x*2*math.pi/640)) for x in range(640)]
    if act=='ruinas':return [148-([0,0,8,8,16,16,8,0][(x//80)%8]) for x in range(640)]
    return [148 for _ in range(640)]

def ground_layer(act):
    im=canvas();d=ImageDraw.Draw(im);ys=terrain_profile(act)
    for x,y in enumerate(ys):
        line(d,[(x,y),(x,179)],11)
        line(d,[(x,y+4),(x,179)],10 if act!='ruinas' else 12)
        line(d,[(x,y+1),(x,y+3)],8 if act!='ruinas' else 15)
        im.putpixel((x,y),C[0])
        if act!='ruinas' and x%13<7:im.putpixel((x,y-1),C[18])
    rng=random.Random(80)
    if act=='ruinas':
        stone=canvas();sd=ImageDraw.Draw(stone)
        for y in range(128,180,11):
            x=-15 if y%22 else -4
            while x<640:
                w=rng.randrange(16,31)
                poly(sd,[(x+2,y),(x+w-3,y),(x+w,y+3),(x+w-1,y+9),(x+2,y+10),(x,y+7),(x,y+2)],12,0)
                poly(sd,[(x+2,y+1),(x+w-4,y+1),(x+w-2,y+3),(x+w-3,y+7),(x+3,y+8),(x+1,y+5)],13)
                line(sd,[(x+3,y+1),(x+w-5,y+1)],14)
                if w>22:line(sd,[(x+8,y+2),(x+11,y+4),(x+9,y+6)],12)
                x+=w
        for x in range(640):
            for y in range(ys[x]+4,180):
                if stone.getpixel((x,y))[3]:im.putpixel((x,y),stone.getpixel((x,y)))
    else:
        for _ in range(750):
            x=rng.randrange(640);y=rng.randrange(130,180)
            if y>ys[x]+5:
                col=9 if y<ys[x]+15 else 11
                line(d,[(x,y),(x+rng.randrange(1,4),y)],col)
    im.paste(im.crop((0,0,1,180)),(639,0));ys[-1]=ys[0]
    save(f'sierra_{act}_capa7_suelo',im,kind='layer',biome=act,pivot=(0,1),surface=ys,notes='Perfil nativo exportado: usarlo en collider, no collider horizontal.')

def near_layers():
    for act in ('camino','valle','ruinas'):
        frames=[]
        for f in range(4):
            im=canvas();d=ImageDraw.Draw(im)
            if act=='camino':
                for x,y,s in [(38,151,.7),(215,153,.52),(571,150,.75)]:tree(d,x,y,s,[0,1,0,-1][f])
                fence(d,87,152,72);fence(d,406,147,90)
            elif act=='valle':
                for x in (40,310,588):
                    poly(d,[(x-16,180),(x-13,155),(x-3,151),(x+11,155),(x+17,180)],11,0)
                    poly(d,[(x-12,156),(x-3,153),(x+8,157),(x+3,180),(x-13,180)],13)
                    grass(d,x,157,[0,1,0,-1][f])
            else:
                for x,y,s in [(40,154,.38),(601,153,.45)]:tree(d,x,y,s,[0,1,0,-1][f])
            for x in range(18,620,41):grass(d,x,153 if act!='ruinas' else 147,[0,1,0,-1][f],.65)
            frames.append(im)
        save(f'sierra_{act}_capa6_cercano',frames,fps=5,kind='layer',factor=.75,biome=act,pivot=(0,1))
        ground_layer(act)
    water=[]
    for f in range(6):
        im=canvas();d=ImageDraw.Draw(im)
        poly(d,[(290,111),(326,111),(355,130),(301,146),(388,179),(216,179),(267,146),(322,128)],2)
        for j in range(16):
            y=116+j*4;x=306+round(math.sin((y-112)*.14)*26)
            width=5+(y-110)//2;line(d,[(x-width+(f+j)%4,y),(x+width,y)],4 if j%3 else 5)
        water.append(im)
    save('sierra_valle_capa5_rio',water,fps=8,kind='layer',factor=.3,biome='valle',pivot=(0,1),notes='Río del fondo, no superficie caminable.')
    fog=[]
    for f in range(4):
        im=canvas()
        for x in range(640):
            y0=117+round(3*math.sin((x+f*5)*.021))
            for y in range(y0,y0+7):
                if (x+2*y)%4==0:im.putpixel((x,y),C[4])
        im.paste(im.crop((0,0,1,180)),(639,0));fog.append(im)
    save('sierra_compartido_capa5_neblina',fog,fps=4,kind='layer',factor=.25,pivot=(0,1),notes='Dithering binario, nunca alfa degradado; usar detrás de módulos jugables.')

def bridge_frame(phase=0,damage=0,collapse=0):
    im=canvas(256,80);d=ImageDraw.Draw(im);surface=[]
    for x in range(256):
        sag=round(13*math.sin(math.pi*x/255))
        wobble=round(math.sin(phase*math.pi/2)*math.sin(math.pi*x/255)*2)
        surface.append(38+sag+wobble)
    for x in (4,249):
        rect(d,(x-3,15,x+3,67),0);rect(d,(x-2,16,x+1,66),10);line(d,[(x-1,18),(x-1,61)],8)
        for yy in (25,29,49):line(d,[(x-4,yy),(x+4,yy)],7)
    for x in range(8,248,7):
        y=surface[x]+(round(collapse*((x%5)+3)) if 88<x<169 else 0)
        if damage>=2 and x%5==0:continue
        poly(d,[(x,y),(x+6,y),(x+6,y+6),(x,y+5)],10,0)
        line(d,[(x+1,y+1),(x+5,y+1)],8 if damage<2 else 9)
        line(d,[(x+2,y+3),(x+5,y+3)],11)
        if damage and x%3==0:line(d,[(x+3,y),(x+2,y+3),(x+4,y+5)],0)
    for off in (-22,-18,7,10):
        pts=[]
        for x in range(5,251,2):
            if collapse and 104<x<151:
                if pts:line(d,pts,0,3);line(d,pts,8);pts=[]
                continue
            pts.append((x,surface[x]+off+(collapse*3 if 95<x<167 else 0)))
        if pts:line(d,pts,0,3);line(d,pts,8 if off<0 else 9)
    for x in range(13,246,14):
        if collapse and 95<x<168:continue
        line(d,[(x,surface[x]-22),(x+2,surface[x]-2)],11,2)
        line(d,[(x,surface[x]-22),(x+2,surface[x]-2)],8)
    return im,surface

def structures():
    for name,damage in [('estable',0),('tension',1),('danado',2)]:
        frames=[bridge_frame(f,damage)[0] for f in range(4)]
        save('sierra_puente_'+name,frames,fps=7,biome='valle',surface=bridge_frame(0,damage)[1],notes='Módulo256px, conexiones en extremos. Lógica de integridad debe elegir estado; sin daño automático por música.')
    save('sierra_puente_colapso',[bridge_frame(0,2,f*2)[0] for f in range(6)],fps=10,loop=False,biome='valle',notes='Retirar soporte físico al frame2; hojas de tablas/escombros aparte.')
    for kind in ('bloque','escalera','rampa','pilar'):
        w,h=(64,32) if kind!='pilar' else (24,64)
        im=canvas(w,h);d=ImageDraw.Draw(im)
        ys=[0 if kind in ('bloque','pilar') else round((1-x/(w-1))*24)//(8 if kind=='escalera' else 1)*(8 if kind=='escalera' else 1) for x in range(w)]
        for x,y in enumerate(ys):line(d,[(x,y),(x,h-1)],12);im.putpixel((x,y),C[0]);im.putpixel((x,min(h-1,y+1)),C[15])
        for y in range(7,h,8):
            for x in range(-6,w,13):
                xx=x+(6 if y%16 else 0)
                if y>=ys[max(0,min(w-1,xx))]+3:
                    line(d,[(xx,y),(xx+12,y)],0);line(d,[(xx,y),(xx,y+7)],11);line(d,[(xx+1,y+1),(xx+10,y+1)],14)
        save('sierra_ruinas_'+kind,im,biome='ruinas',surface=ys,notes='Módulo original de mampostería; perfil superior para colisión.')
    # A single tunnel asset per entrance orientation, with an opaque interior mask.
    for exit_side in (False,True):
        frames=[]
        for f in range(4):
            im=canvas(128,160);d=ImageDraw.Draw(im)
            poly(d,[(0,0),(106,0),(116,19),(111,42),(127,66),(121,104),(127,159),(0,159)],11,0)
            for row in range(0,160,14):
                for col in range(-9,128,24):
                    x=col+(12 if row%28 else 0)
                    poly(d,[(x,row+1),(x+20,row),(x+24,row+9),(x+18,row+13),(x+1,row+12)],12)
                    line(d,[(x+2,row+2),(x+17,row+1)],13)
            poly(d,[(23,160),(23,65),(29,49),(43,35),(63,31),(85,37),(98,54),(103,75),(103,160)],0)
            poly(d,[(29,160),(29,66),(36,52),(51,42),(68,40),(89,50),(96,72),(96,160)],1)
            rect(d,(43,64,89,159),0)
            for x in (16,109):
                line(d,[(x,102),(x,116)],10,3)
                poly(d,[(x-4,103),(x-2,93-f%2*2),(x+1,96),(x+2,90+f%3),(x+4,101),(x+1,106)],23)
                poly(d,[(x-2,102),(x,96),(x+2,102)],6)
            for x in (7,117):grass(d,x,32,0,.7)
            if exit_side:im=im.transpose(Image.Transpose.FLIP_LEFT_RIGHT)
            frames.append(im)
        save('sierra_tunel_'+('salida' if exit_side else 'entrada'),frames,fps=7,notes='Cubrir pantalla con sierra_tunel_oclusor durante el cambio. Apertura dibujada no define colisión.')
    save('sierra_tunel_oclusor',Image.new('RGBA',(32,180),C[0]),pivot=(0,1),notes='Expandir horizontalmente, opaco. Swap de bioma solo con pantalla cubierta.')
    im=canvas(80,24);d=ImageDraw.Draw(im);ellipse(d,(4,10,75,22),11);ellipse(d,(12,12,66,20),0)
    for x in range(9,72,6):line(d,[(39,16),(x,10+(x%5)),(x+2,8)],11)
    save('sierra_llegada_marca_quemada',im,notes='Marca persistente tras teleport, sin colisión. Anclar a superficie inclinada local.')

def llama_frame(f,walk=False):
    im=canvas(48,48);d=ImageDraw.Draw(im);bob=1 if walk and f%2 else 0
    for x in (13,29):
        a=round(math.sin(f*math.pi/3+(x%2))*3) if walk else 0
        line(d,[(x,30),(x+a,42),(x+a+3,42)],0,3);line(d,[(x,30),(x+a,40)],14)
    ellipse(d,(6,18+bob,35,34+bob),0);ellipse(d,(7,19+bob,34,33+bob),15)
    poly(d,[(26,26+bob),(28,9+bob),(32,5+bob),(37,8+bob),(37,17+bob),(34,29+bob)],15,0)
    poly(d,[(31,8+bob),(29,0),(32,1),(34,8+bob)],15,0);poly(d,[(35,8+bob),(37,1),(39,2),(38,10+bob)],15,0)
    ellipse(d,(32,9+bob,43,16+bob),15);rect(d,(40,12+bob,43,15+bob),11)
    d.point((36,10+bob),fill=C[0] if f%4!=3 else C[11])
    line(d,[(7,25+bob),(3,19+bob)],14,3)
    for x,y in ((9,22),(15,20),(22,23),(10,28),(20,29),(28,24)):line(d,[(x,y+bob),(x+3,y-1+bob)],6)
    rect(d,(29,19+bob,36,21+bob),21);d.point((34,22+bob),fill=C[8])
    return im

def bird_frame(f,w,h,kind,state='vuelo'):
    im=canvas(w,h);d=ImageDraw.Draw(im);cx=w//2;cy=h//2
    wing=[-12,-5,5,10,4,-6][f%6]
    span=w//2-3
    for side in (-1,1):
        poly(d,[(cx+side*3,cy),(cx+side*11,cy-3),(cx+side*span,cy+wing),(cx+side*(span-1),cy+wing+4),(cx+side*12,cy+8),(cx+side*4,cy+6)],0)
        poly(d,[(cx+side*5,cy+1),(cx+side*12,cy-1),(cx+side*(span-4),cy+wing+2),(cx+side*11,cy+5)],1 if kind=='condor' else 11)
        for k in range(3):line(d,[(cx+side*(span-2-k*4),cy+wing+1),(cx+side*(span-3-k*4),cy+wing+5+k)],13 if kind=='condor' else 9)
    poly(d,[(cx-3,cy+4),(cx-6,cy+13),(cx+4,cy+10),(cx+5,cy)],0)
    ellipse(d,(cx-3,cy-3,cx+7,cy+7),11 if kind!='condor' else 0)
    if kind=='condor':line(d,[(cx+3,cy-3),(cx+6,cy),(cx+3,cy+2)],5,2)
    ellipse(d,(cx+4,cy-8,cx+11,cy-2),9 if kind=='condor' else 10)
    poly(d,[(cx+11,cy-6),(cx+15,cy-4),(cx+11,cy-2)],8)
    d.point((cx+9,cy-6),fill=C[0])
    if state=='aviso':line(d,[(cx+10,cy-14),(cx+10,cy-11)],23,2);d.point((cx+10,cy-9),fill=C[6])
    if state=='impacto':
        for a in range(0,360,60):
            x=cx+round(math.cos(a*math.pi/180)*12);y=cy+round(math.sin(a*math.pi/180)*12)
            line(d,[(x,y),(x+2,y)],7)
    if state in ('picada','caida'):
        # Nearest-neighbour right-angle rotation, no antialiasing or interpolation.
        rot=im.rotate(-90 if state=='picada' else (f%4)*90,resample=Image.Resampling.NEAREST,expand=False)
        return rot
    return im

def creatures():
    save('sierra_llama_idle',[llama_frame(f) for f in range(4)],fps=3)
    save('sierra_llama_caminar',[llama_frame(f,True) for f in range(6)],fps=7)
    for kind,w,h in [('condor',64,48),('halcon',32,32),('ave_puna',32,32)]:
        for state,n,fps,loop in [('vuelo',6,8,True),('aviso',4,6,False),('picada',4,10,False),('impacto',2,10,False),('caida',4,8,False)]:
            save('sierra_'+kind+'_'+state,[bird_frame(f,w,h,kind,state) for f in range(n)],fps=fps,loop=loop,notes='Telegraph aviso antes de atacar héroe o anclaje. Condor simbólico fantástico; no enseñanza de caza real.')

def extra_modules():
    for name,w,h,n in [('arbol_queuna',64,72,4),('cerco_madera',96,40,1),('ichu',24,20,4),('roca',48,32,1)]:
        frames=[]
        for f in range(n):
            im=canvas(w,h);d=ImageDraw.Draw(im)
            if name=='arbol_queuna':tree(d,32,70,1,[0,1,0,-1][f])
            elif name=='cerco_madera':fence(d,4,37,86)
            elif name=='ichu':grass(d,12,19,[0,1,0,-1][f],1.6)
            else:
                poly(d,[(2,29),(5,13),(18,2),(32,4),(45,19),(44,29)],12,0)
                poly(d,[(6,13),(18,3),(30,5),(23,15),(12,19)],14)
                poly(d,[(12,19),(23,15),(34,12),(44,20),(41,27),(25,28)],13)
                line(d,[(18,5),(13,11),(7,15)],15);line(d,[(28,8),(24,17),(28,22),(25,28)],11)
            frames.append(im)
        save('sierra_decor_'+name,frames,fps=5,notes='Módulo independiente, no extraído del fondo; decoración sin colisión por defecto.')
    for state,n in [('idle',4),('marcha',6),('huaraca',6),('impacto',2),('rotura',6)]:
        frames=[]
        for f in range(n):
            im=canvas(48,48);d=ImageDraw.Draw(im)
            if state=='rotura':
                for j in range(9):
                    x=24+round(math.cos(j*2.4)*(5+f*3));y=23+round(math.sin(j*2.4)*f*2)+f*f//2
                    poly(d,[(x,y),(x+4,y-2),(x+6,y+3),(x+1,y+4)],13 if j%2 else 9,0)
            else:
                sway=round(math.sin(f*math.pi/3)*3) if state=='marcha' else 0
                for x,sgn in [(17,-1),(29,1)]:
                    line(d,[(x,32),(x+sway*sgn,43)],0,7);line(d,[(x,32),(x+sway*sgn,41)],12,5)
                poly(d,[(14,18),(32,18),(36,34),(12,34)],12,0)
                poly(d,[(15,19),(24,19),(23,32),(14,32)],14)
                rect(d,(15,4,32,19),0);rect(d,(16,5,31,18),13);line(d,[(17,5),(30,5)],15)
                eye=12 if state=='idle' and f==3 else 7
                rect(d,(18,9,21,11),eye if state!='impacto' else 23);rect(d,(26,9,29,11),eye)
                line(d,[(23,9),(23,14),(27,14)],0);line(d,[(19,16),(28,16)],11)
                hand=(38,13) if state=='huaraca' else (38,29)
                line(d,[(31,22),hand],0,5);line(d,[(31,22),hand],13,3)
                line(d,[(14,22),(8,29)],0,5);line(d,[(14,22),(8,29)],13,3)
                if state=='huaraca':
                    a=f*math.pi/3;tip=(round(34+11*math.cos(a)),round(8+6*math.sin(a)))
                    line(d,[hand,tip],8);ellipse(d,(tip[0]-1,tip[1]-1,tip[0]+1,tip[1]+1),14)
            frames.append(im)
        save('sierra_guardian_'+state,frames,fps=8,loop=state in ('idle','marcha'),notes='Enemigo fantástico original de piedra animada con huaraca, NO figura histórica. Aviso visual antes del proyectil; lanzamiento frame4.')

def hero_sheets():
    original=Image.open(ROOT/'Assets/SuyuRun/Art/PixelCosta/costa_heroe.png').convert('RGBA')
    rows={'idle':(0,4),'run':(1,8),'jump':(2,3),'land':(3,2),'slide':(4,3),'rafaga':(5,4),'hit':(6,2)}
    for state,(row,n) in rows.items():
        frames=[]
        for f in range(n):
            body=nearest_palette(original.crop((f*48,row*48,(f+1)*48,(row+1)*48)))
            im=canvas(48,48);d=ImageDraw.Draw(im);swing=round(math.sin(f*math.pi/4)*3) if state=='run' else 0
            arm_y=0
            for x,side in [(15,-1),(33,1)]:
                pts=[(x,23),(x+side*6,29+swing*side),(x+side*7,34+swing*side)]
                if state=='slide':pts=[(x,22),(x+side*3,25),(x+side*4,27)]
                pts=[(px,py+arm_y) for px,py in pts]
                line(d,pts,0,6);line(d,pts,9,4);line(d,[(x+side*2,25+arm_y),(x+side*5,28+swing*side+arm_y)],8)
            im.alpha_composite(body)
            # Arms in front of tunic edges, while face/body remain the same source design.
            d=ImageDraw.Draw(im)
            front=[(34,23),(40,27+swing),(41,32+swing)] if state!='slide' else [(31,22),(35,25),(39,26)]
            line(d,front,0,5);line(d,front,9,3)
            if state!='slide':line(d,[(41,32+swing),(44,35+swing),(42,39+swing)],10)
            if state=='rafaga':
                line(d,[(34,23),(40,27),(42,27)],0,5);line(d,[(34,23),(40,27),(42,27)],9,3)
                rect(d,(39,23,46,27),0);rect(d,(40,24,46,25),13);rect(d,(40,27,42,30),11)
                if f%2:line(d,[(47,22),(47,27)],7)
            frames.append(im)
        save('sierra_heroe_'+state,frames,fps=10 if state=='run' else 6,loop=state in ('idle','run','rafaga'),notes='Mismo cuerpo/rostro Costa remapeado a paleta Sierra; brazos añadidos. Base48x48.')
    base=nearest_palette(original.crop((0,0,48,48)))
    for state,n in [('huaraca_girar',6),('huaraca_lanzar',3),('trepar',4)]:
        frames=[]
        for f in range(n):
            im=base.copy();d=ImageDraw.Draw(im)
            line(d,[(14,23),(7,27),(9,33)],0,5);line(d,[(14,23),(7,27),(9,33)],9,3)
            hand=(38,15) if state=='huaraca_girar' else (44,23+f*2) if state=='huaraca_lanzar' else (36,5+f%2*8)
            line(d,[(33,23),(38,21),hand],0,6);line(d,[(33,23),(38,21),hand],9,4)
            if state.startswith('huaraca'):
                angle=f*math.pi*2/n;tip=(round(31+13*math.cos(angle)),round(9+5*math.sin(angle))) if state.endswith('girar') else (47,14+f*6)
                line(d,[hand,tip],0,2);line(d,[hand,tip],8);ellipse(d,(tip[0]-2,tip[1]-1,tip[0]+2,tip[1]+1),11)
            frames.append(im)
        save('sierra_heroe_'+state,frames,fps=10,loop=state!='huaraca_lanzar',notes='Evento de lanzamiento en frame1 de lanzar; dirección de proyectil viene del cursor, no del PNG.')
    # Full-body fantasy protective fur suit. Face remains an unchanged source face.
    suit_frames=[]
    for f in range(6):
        im=base.copy();d=ImageDraw.Draw(im)
        if f:
            amount=min(4,f)
            for x in (18,29):
                rect(d,(x-3,33,x+3,46),0);rect(d,(x-2,34,x+2,44),11)
            if amount>=2:
                poly(d,[(13,20),(34,20),(38,34),(10,34)],0)
                poly(d,[(14,21),(33,21),(36,33),(12,33)],11)
                for y in range(23,33,3):
                    for x in range(15,34,4):poly(d,[(x,y),(x+3,y),(x+1,y+3)],10 if x<24 else 12)
            if amount>=3:
                for side in (-1,1):
                    x=24+side*14;ellipse(d,(x-5,20,x+5,29),0);ellipse(d,(x-4,21,x+4,28),11)
                    line(d,[(x,26),(x+side*2,36)],0,7);line(d,[(x,26),(x+side*2,35)],12,5)
            if amount>=4:
                ellipse(d,(14,4,34,22),0);ellipse(d,(15,5,33,21),11)
                face=base.crop((18,9,31,21));im.alpha_composite(face,(18,9));d=ImageDraw.Draw(im)
                line(d,[(16,8),(19,5),(23,7),(27,5),(31,8)],13)
            if f<5:
                for a in range(0,360,45):
                    x=24+round(21*math.cos(math.radians(a+f*12)));y=24+round(20*math.sin(math.radians(a+f*12)))
                    d.point((x,y),fill=C[7])
        suit_frames.append(im)
    save('sierra_premio_traje_transformacion',suit_frames,fps=5,loop=False,notes='Protección fantástica con apariencia de pelaje simiesco; rostro visible. No fauna/vestimenta histórica peruana.')
    save('sierra_premio_traje_idle',[suit_frames[-1],suit_frames[-1].transform((48,48),Image.Transform.AFFINE,(1,0,0,0,1,1),resample=Image.Resampling.NEAREST)],fps=2)
    surprise=Image.open(ROOT/'Assets/SuyuRun/Art/PixelCosta/costa_efecto_sorpresa.png').convert('RGBA')
    save('sierra_efecto_sorpresa',[nearest_palette(surprise.crop((f*32,0,f*32+32,32))) for f in range(3)],fps=7,loop=False,notes='Reutiliza el ?! existente, remapeado sin modificar Costa.')

def effects():
    for effect,size,count in [('polvo_correr',24,6),('polvo_aterrizar',32,6),('impacto_piedra',24,5),('chispas',24,6),
                              ('humo_teleport',64,8),('escombros_piedra',32,6),('astillas_madera',32,6),('cuerda_rotura',32,5),('salpicadura',32,6)]:
        frames=[]
        for f in range(count):
            im=canvas(size,size);d=ImageDraw.Draw(im);rng=random.Random(12)
            for j in range(14 if effect=='humo_teleport' else 9):
                angle=rng.random()*math.pi*2;speed=rng.uniform(.9,2.7);cx=size//2;cy=size//2
                x=round(cx+math.cos(angle)*speed*(f+1));y=round(cy+math.sin(angle)*speed*(f+1)+(.25*f*f if 'escombros' in effect or 'astillas' in effect else -f*.5))
                r=max(1,3-f//2)
                color=13 if 'piedra' in effect else 8 if 'polvo' in effect else 7 if effect=='chispas' else 4 if effect=='salpicadura' else 11 if effect=='humo_teleport' else 9
                if effect=='humo_teleport':ellipse(d,(x-r*2,y-r,x+r*2,y+r),color)
                elif effect=='cuerda_rotura':line(d,[(x,y),(x+2,y-3),(x+3,y+1)],color)
                else:poly(d,[(x-r,y),(x,y-r),(x+r,y+1),(x,y+r)],color)
            frames.append(im)
        save('sierra_fx_'+effect,frames,fps=12,loop=False,notes='Partículas con alfa binario; el controlador debe destruir/reciclar tras último frame.')
    for name in ('piedra','roca_rodante','pinchos','tronco','farol'):
        size=8 if name=='piedra' else 32;frames=[]
        for f in range(4):
            im=canvas(size,size);d=ImageDraw.Draw(im)
            if name in ('piedra','roca_rodante'):
                r=size//2-1;cx=cy=size//2
                pts=[(cx+round(r*math.cos((a*45+f*30)*math.pi/180)),cy+round(r*math.sin((a*45+f*30)*math.pi/180))) for a in range(8)]
                poly(d,pts,13,0);line(d,[(cx-r+2,cy-1),(cx-1,cy-r+2),(cx+2,cy-r+3)],15)
                line(d,[(cx,cy-2),(cx+2,cy+2),(cx-1,cy+4)],11)
            elif name=='pinchos':
                rect(d,(0,27,31,31),11)
                for x,h in [(5,18),(15,26),(25,18)]:poly(d,[(x-5,27),(x,27-h),(x+5,27)],13,0);line(d,[(x,28-h),(x+2,25)],15)
            elif name=='tronco':
                rect(d,(1,15,30,28),10);line(d,[(2,16),(29,16)],8);ellipse(d,(1,15,10,28),11);ellipse(d,(3,17,8,26),9)
                for yy in (19,23,26):line(d,[(11,yy),(29,yy)],11)
            else:
                rect(d,(13,15,17,31),11);poly(d,[(9,17),(8,10),(12,3+f),(15,9),(19,4+f%2),(22,13),(19,19)],23,0);ellipse(d,(12,10,17,17),7)
            frames.append(im)
        save('sierra_'+name,frames[:1] if name in ('pinchos','tronco') else frames,fps=8,loop=True,notes='Objeto de gameplay original; configurar hitbox por silueta, no por lienzo.')
    portal=[]
    for f in range(8):
        im=canvas(96,112);d=ImageDraw.Draw(im)
        # A freestanding stepped sun-ring, clearly unlike Costa's rectangular portal.
        poly(d,[(7,106),(13,97),(20,97),(20,40),(29,22),(40,15),(40,8),(55,8),(55,15),(68,23),(76,40),(76,97),(84,97),(89,106)],0)
        poly(d,[(12,103),(24,98),(24,41),(32,26),(43,19),(43,12),(52,12),(52,19),(64,27),(72,41),(72,98),(84,103)],12)
        poly(d,[(29,98),(29,43),(35,32),(44,27),(53,27),(62,33),(67,44),(67,98)],1)
        for y in range(42,98,10):
            line(d,[(25,y),(29,y-2)],15);line(d,[(67,y-2),(71,y)],8)
            line(d,[(24,y+2),(28,y+1)],0);line(d,[(68,y+1),(72,y+2)],0)
            line(d,[(24,y+3),(24,y+8)],13);line(d,[(70,y+4),(71,y+8)],11)
        for x,side in [(32,-1),(62,1)]:
            line(d,[(x,28),(x+side*4,34),(x+side*5,40)],14)
        for yy in (100,103):line(d,[(19,yy),(77,yy)],13)
        for xx in (30,43,56,69):line(d,[(xx,99),(xx+1,104)],0)
        for j in range(12):
            a=(j*30+f*9)*math.pi/180;x=48+round(14*math.cos(a));y=64+round(28*math.sin(a))
            line(d,[(x,y),(x+round(math.sin(a)*3),y+2)],7 if j%3 else 4,2)
        ellipse(d,(40,17,55,31),8);ellipse(d,(44,20,51,27),7)
        for x in (12,80):
            line(d,[(x,93),(x,79)],10,2);poly(d,[(x-3,81),(x,70+f%3),(x+3,81)],23);d.point((x,77),fill=C[6])
        for x in (30,65):grass(d,x,103,0,.6)
        portal.append(im)
    save('sierra_portal_ceremonial',portal,fps=8,notes='Portal fantástico inspirado en piedra/sol escalonado; no ritual histórico documentado.')
    for name in ('teleport_anillo','escudo_activar','escudo_romper','portal_absorcion'):
        frames=[]
        for f in range(8):
            im=canvas(64,64);d=ImageDraw.Draw(im);r=(27-f*3 if name=='portal_absorcion' else 5+f*3)
            for a in range(0,360,10):
                if name=='escudo_romper' and (a//30)%8<f:continue
                x=32+round(r*math.cos(math.radians(a+f*12)));y=32+round(r*(.4 if name=='teleport_anillo' else 1)*math.sin(math.radians(a+f*12)))
                line(d,[(x,y),(x+1,y)],7 if (a//10)%3 else 4)
            frames.append(im)
        save('sierra_fx_'+name,frames,fps=10,loop=False)

def collectibles_ui():
    for kind in ('cabeza_clava','kero','quipu','moneda'):
        frames=[]
        for f in range(4):
            im=canvas(16,16);d=ImageDraw.Draw(im)
            if kind=='cabeza_clava':
                rect(d,(11,5,15,10),12);poly(d,[(3,2),(10,2),(13,6),(11,13),(3,13),(1,9)],13,0)
                rect(d,(3,5,5,6),0);rect(d,(8,5,10,6),0);line(d,[(6,6),(5,9),(8,9)],15);line(d,[(4,11),(10,11)],0)
            elif kind=='kero':
                poly(d,[(2,2),(13,2),(10,13),(5,13)],10,0);line(d,[(3,3),(12,3)],8);line(d,[(4,7),(11,7)],21,2);line(d,[(5,11),(10,11)],8)
            elif kind=='quipu':
                line(d,[(2,3),(13,3)],10,2)
                for x in (3,6,9,12):
                    line(d,[(x,3),(x,11+x%3)],8 if x%2 else 21)
                    for y in (6,9):rect(d,(x-1,y,x+1,y+1),8 if x%2 else 22)
            else:ellipse(d,(2,1,13,14),0);ellipse(d,(3,2,12,13),8);ellipse(d,(5,4,10,11),7);line(d,[(7,5),(7,10)],8)
            x,y=[(3,2),(11,3),(12,10),(5,12)][f];line(d,[(x-1,y),(x+1,y)],6);line(d,[(x,y-1),(x,y+1)],6)
            frames.append(im)
        save('sierra_coleccionable_'+kind,frames,fps=5,notes='Interpretación estilizada. La cabeza clava pertenece a Chavín, NO atribuir a Machu Picchu; museo debe explicarlo.')
    for name,w,h in [('panel',96,48),('boton',64,16),('casilla',20,20),('barra_integridad',64,12),('reticula',16,16)]:
        im=canvas(w,h);d=ImageDraw.Draw(im)
        if name=='reticula':
            for pts in [[(1,5),(1,1),(5,1)],[(10,1),(14,1),(14,5)],[(1,10),(1,14),(5,14)],[(10,14),(14,14),(14,10)]]:line(d,pts,7)
            d.point((8,8),fill=C[6])
        else:
            rect(d,(0,0,w-1,h-1),0);rect(d,(1,1,w-2,h-2),12);rect(d,(2,2,w-3,h-3),11 if name=='panel' else 1)
            line(d,[(2,1),(w-3,1)],15);line(d,[(1,2),(1,h-3)],13)
            if name=='barra_integridad':
                for x in range(4,w-4,9):rect(d,(x,4,x+6,h-5),18)
        save('sierra_ui_'+name,im,notes='Sin texto horneado. Usar fuente pixel existente, bordes9-slice2px para panel/botón.')

def backgrounds_from_sources():
    for act in ('camino','valle','ruinas'):
        source=SOURCES/(act+'.png')
        if not source.exists():continue
        im=Image.open(source).convert('RGBA')
        # Each source IS ONE independently generated layer. No scene splitting.
        im=nearest_palette(im.resize((640,180),Image.Resampling.NEAREST))
        # Close a 40px seam with discrete nearest-palette colors, not a one-column jump.
        # Each output remains the single source layer, never a cut-up full scene.
        seam=Image.new('RGBA',(40,180))
        left=im.crop((0,0,40,180));right=im.crop((600,0,640,180))
        for x in range(40):
            t=x/39
            a=right.crop((x,0,x+1,180));b=left.crop((x,0,x+1,180))
            seam.paste(Image.blend(a,b,t),(x,0))
        seam=nearest_palette(seam)
        im.paste(seam.crop((0,0,20,180)),(620,0))
        im.paste(seam.crop((20,0,40,180)),(0,0))
        im.paste(im.crop((0,0,1,180)),(639,0))
        save(f'sierra_{act}_capa4_paisaje',im,kind='layer',factor=.3,biome=act,pivot=(0,1),notes='Capa generada independientemente con image_gen; normalizada640x180/24colores. Fuente y prompt conservados.')

def main():
    for p in (ART,OUT,SOURCES):p.mkdir(parents=True,exist_ok=True)
    protected=[p for p in (ROOT/'Assets/SuyuRun/Art/PixelCosta').rglob('*') if p.is_file() and p.suffix=='.png']
    before={str(p.relative_to(ROOT)):hashlib.sha256(p.read_bytes()).hexdigest() for p in protected}
    (PACK/'sierra_palette_v2.json').write_text(json.dumps(dict(status='proposed_batch_2026_09_24',colors=HEX,color_count=24),indent=2),encoding='utf-8')
    shared_layers();backgrounds_from_sources();near_layers();structures();creatures();extra_modules();hero_sheets();effects();collectibles_ui()
    manifest=dict(version=2,baseWidth=320,baseHeight=180,ppu=32,palette=HEX,assets=registry,status='proposed_not_yet_user_approved')
    (ART/'sierra_manifest.json').write_text(json.dumps(manifest,indent=2),encoding='utf-8')
    assert before=={str(p.relative_to(ROOT)):hashlib.sha256(p.read_bytes()).hexdigest() for p in protected},'Costa modified'
    print(json.dumps(dict(assets=len(registry),frames=sum(a['count'] for a in registry),protected_costa_pngs=len(before))))

if __name__=='__main__':main()
