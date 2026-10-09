"""Selva art-only review package. Native pixel authoring authorized by ART_BIBLE.

Never writes Assets or existing Costa/Sierra files. Run from any directory.
All generated bitmaps use the existing proposed Selva 24-color palette.
"""
from pathlib import Path
from PIL import Image, ImageDraw
import math, json, random, hashlib, zipfile, shutil

ROOT=Path(__file__).resolve().parents[1]
OUT=ROOT/'Artifacts/SelvaArtPack_v1'
PACK=OUT/'Package'
ART=PACK/'Assets/SuyuRun/Art/PixelSelva/ProductionV1'
PRE=OUT/'Previews'
PALETTE_SOURCE=ROOT/'Assets/SuyuRun/Art/PixelSelva/selva_palette.json'
if not PALETTE_SOURCE.exists():PALETTE_SOURCE=ROOT/'palette.json'
PAL=json.loads(PALETTE_SOURCE.read_text())
C=[tuple(bytes.fromhex(h[1:]))+(255,) for h in PAL['colors']]
T=(0,0,0,0)
REG=[]; FRAMES={}

def canvas(w=640,h=180):return Image.new('RGBA',(w,h),T)
def poly(d,p,c,o=None):d.polygon([(round(x),round(y)) for x,y in p],fill=C[c],outline=C[o] if o is not None else None)
def line(d,p,c,w=1):d.line([(round(x),round(y)) for x,y in p],fill=C[c],width=w)
def rect(d,b,c):d.rectangle(tuple(round(v) for v in b),fill=C[c])
def oval(d,b,c,o=None):d.ellipse(tuple(round(v) for v in b),fill=C[c],outline=C[o] if o is not None else None)
def quant(im):
    p=Image.new('P',(1,1));p.putpalette([v for c in C for v in c[:3]]+list(C[0][:3])*(256-len(C)))
    out=im.convert('RGB').quantize(palette=p,dither=Image.Dither.NONE).convert('RGBA')
    out.putalpha(im.convert('RGBA').getchannel('A').point(lambda a:255 if a>=128 else 0))
    out.paste(T,(0,0,*out.size),out.getchannel('A').point(lambda a:255-a));return out

def save(name,frames,group='Props',fps=8,loop=True,factor=1,seam=False,**extra):
    if isinstance(frames,Image.Image):frames=[frames]
    w,h=frames[0].size
    if seam:
        for im in frames:
            # Periodic drawing already wraps its shapes. Lock boundary sample.
            im.paste(im.crop((0,0,1,h)),(w-1,0))
    vertical=group=='Layers'
    sheet=canvas(w if vertical else w*len(frames),h*len(frames) if vertical else h)
    for i,im in enumerate(frames):sheet.alpha_composite(im,(0,i*h) if vertical else (i*w,0))
    path=ART/group/(name+'.png');path.parent.mkdir(parents=True,exist_ok=True);sheet.save(path)
    spec=dict(name=name,file=str(path.relative_to(ART)).replace('\\','/'),frameWidth=w,frameHeight=h,
              frames=len(frames),layout='vertical' if vertical else 'horizontal',fps=fps,loop=loop,
              ppu=32,pivot=[.5,0],parallax=factor,seamlessX=seam,status='proposal_pending_visual_approval',**extra)
    spec['rectsUnity']=[dict(x=0 if vertical else i*w,y=sheet.height-(i+1)*h if vertical else 0,width=w,height=h) for i in range(len(frames))]
    path.with_suffix('.json').write_text(json.dumps(spec,indent=2,ensure_ascii=False),encoding='utf-8')
    REG.append(spec);FRAMES[name]=frames
    return name

def leaf(d,x,y,w,h,c,light=None):
    x,y,w,h=round(x),round(y),round(w),round(h)
    if w<8:
        poly(d,[(x-w,y),(x-1,y-h),(x+w,y),(x+1,y+h)],c)
        if light is not None:line(d,[(x-w+1,y),(x,y-1)],light)
        return
    # Broad flat leaf clusters, the same selective detailing as Sierra V3 trees.
    pts=[(x-w,y+2),(x-w,y-h//3),(x-w+4,y-h//3),(x-w+4,y-h*2//3),
         (x-w//2,y-h*2//3),(x-w//2,y-h),(x-2,y-h),(x+2,y-h-2),
         (x+w//3,y-h-2),(x+w//3+4,y-h+2),(x+w-4,y-h//2),
         (x+w-4,y-2),(x+w,y-2),(x+w,y+h//2),(x+w-6,y+h//2),
         (x+w-6,y+h),(x+3,y+h),(x-2,y+h+2),(x-w//2,y+h+2),(x-w//2-3,y+h//2)]
    poly(d,pts,c)
    if light is not None:
        line(d,[(x-w+5,y-h//3),(x-w+5,y-h*2//3+1),(x-w//2+1,y-h*2//3+1),(x-w//2+1,y-h+1),(x-2,y-h+1),(x+2,y-h-1),(x+w//3-1,y-h-1)],light)

def palm(d,x,y,h=80,c=2,lit=4):
    poly(d,[(x-4,y),(x+1,y-h),(x+5,y-h),(x+3,y)],7,0)
    line(d,[(x+1,y-3),(x+4,y-h+3)],9)
    for yy in range(y-h+7,y,8):line(d,[(x,yy),(x+3,yy+2)],8)
    for dx,dy in [(-37,5),(-28,-13),(-10,-23),(15,-22),(32,-7),(38,10)]:
        top=(x+3,y-h)
        poly(d,[top,(x+dx*.5,y-h+dy-5),(x+dx,y-h+dy+9),(x+dx*.65,y-h+dy+3)],c)
        line(d,[top,(x+dx*.5,y-h+dy-3),(x+dx,y-h+dy+9)],lit)
        for t in (.4,.6,.8):line(d,[(x+dx*t,y-h+dy*t),(x+dx*t-3,y-h+dy*t+7)],c)

def ceiba(d,x,y,s=1,dark=False):
    def p(a,b):return x+a*s,y+b*s
    poly(d,[p(-27,0),p(-10,-16),p(-7,-75),p(-20,-97),p(-13,-96),p(0,-83),p(13,-108),p(18,-104),p(7,-75),p(10,-18),p(28,0),p(8,-5),p(0,-11),p(-9,-3)],0 if dark else 7)
    if not dark:
        poly(d,[p(-6,-72),p(-1,-80),p(1,-21),p(13,-5),p(3,-9),p(-2,-22)],9)
        line(d,[p(-4,-61),p(-5,-38),p(-2,-29)],8)
    for dx,dy,w,h in [(-28,-96,28,12),(-5,-117,35,16),(29,-105,33,14),(-43,-110,20,10),(4,-100,27,10)]:
        leaf(d,*p(dx,dy),round(w*s),round(h*s),0 if dark else 2,None if dark else 4)
        if not dark:leaf(d,*p(dx-5,dy-5),round(w*.6*s),round(h*.45*s),3,5)
    if not dark:
        line(d,[p(20,-98),p(16,-72),p(20,-57),p(17,-38)],1,2)
        for a,b in [(16,-72),(20,-58)]:leaf(d,*p(a,b),3,2,4)

def fern(d,x,y,c=4):
    for dx,h in [(-15,10),(-10,16),(-4,20),(5,18),(12,12)]:
        line(d,[(x,y),(x+dx,y-h)],c)
        for t in (.3,.55,.8):
            xx=x+round(dx*t);yy=y-round(h*t)
            line(d,[(xx-4,yy-1),(xx,yy),(xx+3,yy-3)],c)

def masonry(d,x,y,w,h,lit=3,base=2):
    rect(d,(x,y,x+w-1,y+h-1),0)
    for yy in range(y,y+h,12):
        for xx in range(x-16 if (yy-y)//12%2 else x,x+w,24):
            l=max(x,xx);r=min(x+w-1,xx+22)
            if l>=r:continue
            poly(d,[(l+1,yy+1),(r-1,yy+1),(r,yy+3),(r-1,min(y+h-1,yy+10)),(l,min(y+h-1,yy+10))],base)
            line(d,[(l+2,yy+2),(r-2,yy+2)],lit)
            if (xx+yy)%3==0:line(d,[(l+5,yy+3),(l+8,yy+5),(l+6,yy+7)],0)

def clouds(d,night=False):
    for x,y,w in [(-18,22,115),(104,5,156),(296,35,115),(457,10,169)]:
        c=21 if night else 7;light=10 if night else 16
        mask=Image.new('1',(640,180));md=ImageDraw.Draw(mask);h=30
        for a,b,e,f in [(0,.5,.4,.91),(.15,.25,.53,.94),(.34,0,.67,.9),(.56,.27,.88,.98),(.76,.54,1,1)]:
            md.ellipse((round(x+a*w),round(y+b*h),round(x+e*w),round(y+f*h)),fill=1)
        md.polygon([(x+4,y+h-5),(x+w-8,y+h-7),(x+w+5,y+h-1),(x+10,y+h+1)],fill=1)
        for xx in range(max(0,x),min(640,x+w+6)):
            ys=[yy for yy in range(max(0,y),min(180,y+h+3)) if mask.getpixel((xx,yy))]
            if not ys:continue
            for yy in ys:
                depth=ys[-1]-yy;col=c if depth>6 else light
                if 3<=depth<=8 and (xx+yy)%2==0:col=21 if night else 15
                d.point((xx,yy),C[col])
        line(d,[(x+w*.30,y+10),(x+w*.58,y+10)],10 if night else 15)

def layers():
    for zone in ['bosque','rio','cueva','ruinas','noche','templo']:
        night=zone=='noche';cave=zone=='cueva';inside=zone in ('cueva','ruinas')
        im=canvas();d=ImageDraw.Draw(im)
        bands=[(0,20),(58,21),(111,10),(151,2)] if night else [(0,7),(70,1),(120,0)] if cave else [(0,14),(45,15),(97,16),(143,23)]
        for y,c in bands:rect(d,(0,y,639,179),c)
        if not cave:
            oval(d,(205,55,244,94),23 if not night else 13)
            if night:oval(d,(216,51,248,85),21)
            clouds(d,night)
        save(f'selva_{zone}_capa1_cielo' if not cave else 'selva_cueva_capa1_oscuridad',im,'Layers',factor=0,zone=zone)
        im=canvas();d=ImageDraw.Draw(im)
        if cave:
            for anchor in (30,120,210,300,390,480,570):
                for offset in (-640,0,640):
                    x=anchor+offset
                    poly(d,[(x,0),(x+65,0),(x+73,32),(x+45,65),(x+34,114),(x+16,88),(x+9,36)],7)
                    line(d,[(x+50,8),(x+56,32),(x+33,70)],8)
        else:
            for anchor in range(0,640,40):
                h=28+int(19*math.sin(anchor*.035)**2)
                for offset in (-640,0,640):
                    x=anchor+offset
                    leaf(d,x,129-h//2,35,h,10 if night else 3)
                    rect(d,(x-3,125-h//2,x+3,172),10 if night else 3)
            rect(d,(0,134,639,179),10 if night else 3)
        save(f'selva_{zone}_capa2_lejania',im,'Layers',factor=.15,seam=True,zone=zone)
        im=canvas();d=ImageDraw.Draw(im)
        if inside:
            for x in (40,255,487):
                if cave:
                    poly(d,[(x,0),(x+48,0),(x+53,21),(x+36,53),(x+29,101),(x+18,63),(x+4,25)],2)
                    poly(d,[(x+4,0),(x+28,0),(x+23,45),(x+18,64),(x+13,27)],3)
                    poly(d,[(x-15,180),(x+3,143),(x+15,133),(x+31,180)],2)
                else:
                    masonry(d,x,33,42,143);masonry(d,x-8,29,58,14,4,3)
                    line(d,[(x+10,54),(x+31,54),(x+31,74),(x+16,74),(x+16,66),(x+25,66)],22,2)
                    fern(d,x+8,178)
            if zone=='ruinas':
                masonry(d,97,18,165,18);masonry(d,334,18,175,18)
                for x in (125,378,440):line(d,[(x,31),(x+3,53),(x-2,73)],1,2)
        else:
            for x in (177,448,602):
                for offset in (-640,0,640):ceiba(d,x+offset,171,.85,night)
            for x in (91,364,563):palm(d,x,169,70,1 if night else 2,2 if night else 4)
        save(f'selva_{zone}_capa3_estructura',im,'Layers',factor=.30,seam=True,zone=zone)
        frames=[]
        for f in range(4):
            im=canvas();d=ImageDraw.Draw(im);water=146 if zone in ('rio','cueva') else 155
            rect(d,(0,water,639,179),10 if not cave else 20)
            line(d,[(0,water),(639,water)],12 if not cave else 10)
            for j in range(45):
                xx=(j*57+f*(1+j%3))%640;yy=water+3+j*7%max(1,177-water)
                length=4+j%13
                for off in (-640,0,640):line(d,[(xx+off,yy),(xx+length+off,yy)],11 if j%3 else 12)
            frames.append(im)
        save(f'selva_{zone}_capa4_agua',frames,'Layers',fps=5,factor=.50,seam=True,zone=zone)
        im=canvas();d=ImageDraw.Draw(im)
        if zone=='templo':
            for x in (78,378):
                for k in range(4):masonry(d,x+k*12,141-k*17,122-k*24,17,9,7)
                rect(d,(x+47,72,x+74,107),0);rect(d,(x+52,76,x+69,107),22)
                line(d,[(x+56,78),(x+56,101),(x+65,101),(x+65,85)],23)
        elif zone=='cueva':
            for x in (45,263,528):
                poly(d,[(x-22,175),(x-11,142),(x+1,127),(x+7,138),(x+19,175)],7,0)
                line(d,[(x-10,149),(x+1,130),(x+1,162)],9)
        elif zone=='ruinas':
            for x in (12,218,525):
                masonry(d,x,134,70,46);fern(d,x+7,134)
        else:
            for x in (25,315,580):
                for offset in (-640,0,640):ceiba(d,x+offset,180,1.05,night)
        save(f'selva_{zone}_capa5_cercania',im,'Layers',factor=.75,seam=True,zone=zone)
        im=canvas();d=ImageDraw.Draw(im)
        for anchor in (7,109,286,492,616):
            for offset in (-640,0,640):
                x=anchor+offset;fern(d,x,179,0)
                for xx in (x-7,x+8):leaf(d,xx,171,13,5,0)
        save(f'selva_{zone}_capa6_frontal',im,'Layers',factor=1.3,seam=True,zone=zone)
    # True independent haze; its only stippling is permitted atmospheric dithering.
    frames=[]
    for f in range(4):
        im=canvas();d=ImageDraw.Draw(im)
        for j in range(5):
            y=111+j*7
            for x in range(640):
                if (x+f*3+j*11)%67<42 and (x+y)%4==0:d.point((x,y),C[6])
        frames.append(im)
    save('selva_compartido_niebla',frames,'Layers',fps=3,factor=.05,seam=True)

def terrain():
    for material in ('tierra','roca','ruina','cueva'):
        for part in ('superficie','relleno','borde_izq','borde_der'):
            im=canvas(32,32);d=ImageDraw.Draw(im)
            if material in ('ruina','cueva'):masonry(d,0,0,32,32,3 if material=='ruina' else 8,2 if material=='ruina' else 7)
            else:
                rect(d,(0,0,31,31),7)
                for j in range(16):
                    x=j*17%30;y=4+j*11%26;line(d,[(x,y),(min(31,x+3),y)],8 if j%3 else 9)
            if part!='relleno':
                rect(d,(0,0,31,3),0);line(d,[(0,1),(31,1)],5);line(d,[(0,3),(31,3)],3)
                for x in range(2,32,7):line(d,[(x,3),(x-1,6)],2)
            if part=='borde_izq':line(d,[(0,0),(0,31)],0)
            if part=='borde_der':line(d,[(31,0),(31,31)],0)
            save(f'selva_tile_{material}_{part}',im,'Tiles',seam=part in ('superficie','relleno'))
    for name,w,h in [('tronco',64,16),('piedra',48,16),('victoria',48,12),('puente',64,24),('bote',80,32),('rampa',64,32)]:
        frames=[]
        for f in range(2 if name=='victoria' else 4 if name in ('bote','puente') else 1):
            im=canvas(w,h);d=ImageDraw.Draw(im)
            if name=='tronco':
                poly(d,[(0,5),(4,1),(59,1),(63,5),(62,13),(4,15),(0,11)],8,0)
                line(d,[(5,3),(58,3)],9)
                for y in (6,10,13):line(d,[(9,y),(25,y-1),(36,y+1),(57,y)],7)
                oval(d,(1,3,9,13),9,0);oval(d,(3,5,7,11),7);line(d,[(14,1),(30,1),(34,3)],5)
            elif name=='piedra':
                poly(d,[(0,6),(6,1),(39,1),(47,7),(44,14),(4,15)],2,0)
                poly(d,[(3,6),(7,2),(39,2),(43,5),(32,8),(12,7)],4)
                line(d,[(20,9),(25,11),(23,14)],0);line(d,[(8,2),(37,2)],6)
            elif name=='victoria':
                oval(d,(0,1+f,47,10),3,0);oval(d,(2,2+f,45,8),5)
                poly(d,[(24,5),(42,7),(45,11),(32,10)],0)
                for x in (8,17,33,40):line(d,[(24,5),(x,3 if x%2 else 8)],4)
                line(d,[(6,2+f),(36,2+f)],6)
            elif name=='puente':
                for x in range(0,64,8):
                    y=13+round(math.sin((x/64)*math.pi)*(1+f%2))
                    rect(d,(x,y,x+6,y+4),8);line(d,[(x,y),(x+6,y)],9)
                    line(d,[(x+3,1),(x+3,y)],7)
                line(d,[(0,1),(32,4),(63,1)],9)
            elif name=='bote':
                y=f%2
                poly(d,[(1,15+y),(18,20+y),(65,20+y),(78,12+y),(70,29),(17,29),(7,24)],8,0)
                line(d,[(8,19+y),(23,24),(65,24),(74,18+y)],9)
                line(d,[(34,19),(34,4),(39,0)],7,2)
                line(d,[(43,13),(67-f*4,29)],9,2)
            else:
                poly(d,[(0,30),(63,0),(63,31),(0,31)],7,0)
                line(d,[(0,29),(63,0)],5,2)
                line(d,[(7,29),(57,6)],3,2)
            frames.append(im)
        save('selva_plataforma_'+name,frames,'Tiles',fps=4,collision='top_only; define collider separately from decoration')
    for name in ('pinchos','raiz','techo','trampa','estaca'):
        frames=[]
        for f in range(4):
            im=canvas(32,32);d=ImageDraw.Draw(im)
            rect(d,(1,27,30,31),7)
            if name in ('pinchos','estaca','trampa'):
                for x,h in [(6,14),(16,23),(26,16)]:
                    height=h if name!='trampa' else round(h*[.15,.45,1,.65][f])
                    poly(d,[(x-4,28),(x,28-height),(x+4,28)],9,0)
                    line(d,[(x,29-height),(x+2,26)],23)
            elif name=='raiz':
                poly(d,[(0,30),(7,24),(9,11),(18,7),(25,16),(31,24),(31,30),(20,21),(15,15),(13,29)],8,0)
                line(d,[(10,23),(12,13),(17,10),(22,16)],9)
            else:
                rect(d,(0,0,31,8),7)
                for x in (5,16,26):poly(d,[(x-5,7),(x+4,7),(x,23-f%2)],9,0)
            for x in (3,27):rect(d,(x,28,x+1,29),19 if f%2 else 23)
            frames.append(im)
        save('selva_obstaculo_'+name,frames,'Obstacles',fps=6)

def lianas():
    frames=[]
    for f in range(2):
        im=canvas(16,96);d=ImageDraw.Draw(im)
        line(d,[(8,0),(7,25),(9,53),(8,91)],0,3)
        line(d,[(8,0),(7,25),(9,53),(8,91)],4)
        for y in (10,30,49,68):leaf(d,7 if y%3 else 10,y,4,2,3,5)
        oval(d,(4,84,12,94),12 if f==0 else 23,0);line(d,[(6,87),(10,90),(6,91)],6)
        frames.append(im)
    save('selva_liana_recta',frames,'Traversal',fps=4,pinTop=[8,0],grip=[8,89],notes='Anchor remains fixed. Glow is an interaction cue, not an extra collider.')
    frames=[];sockets=[]
    for angle in (-36,-24,0,24,36,24,0,-24):
        im=canvas(128,112);d=ImageDraw.Draw(im);a=math.radians(angle)
        end=(64+round(88*math.sin(a)),3+round(88*math.cos(a)));sockets.append(end)
        line(d,[(64,3),end],0,3);line(d,[(64,3),end],4)
        for t in (.18,.38,.61,.79):leaf(d,64+(end[0]-64)*t,3+(end[1]-3)*t,3,2,3,5)
        oval(d,(end[0]-3,end[1]-3,end[0]+3,end[1]+3),23,0)
        frames.append(im)
    save('selva_liana_balanceo',frames,'Traversal',fps=8,anchor=[64,3],gripPerFrame=sockets,
         notes='Visual reference cycle only. Physics must sample angle and keep hero hand exactly at grip. No whole-sprite smooth rotation.')

def limb(d,points):
    line(d,points,0,6);line(d,points,7,4);line(d,[(x-1,y) for x,y in points],8,2)
    x,y=points[-1];rect(d,(x-2,y-1,x+2,y+1),9)

def hero():
    source=ROOT/'Assets/SuyuRun/Art/PixelSierra/ProductionV3/Resources/SierraV3/sierra_premio_traje_idle.png'
    if not source.exists():source=ROOT/'SourceReferences/sierra_premio_traje_idle.png'
    src=quant(Image.open(source).crop((0,0,48,48)))
    states={'idle':4,'run':8,'jump':3,'double_jump':4,'land':2,'slide':3,'burst':4,'reload':4,'hit':2,
            'grab':3,'swing':8,'release':3,'regrab':3,'fall':3,'climb':6,'victory':4,'death':5}
    for state,n in states.items():
        frames=[];hands=[]
        for f in range(n):
            im=canvas(48,48);d=ImageDraw.Draw(im)
            airborne=state in ('jump','double_jump','grab','swing','release','regrab','fall')
            bob=f%2 if state in ('idle','run') else 0
            swing=round(6*math.sin(f*math.tau/n)) if state=='run' else 0
            shift=round(3*math.sin(f*math.tau/n)) if state=='swing' else 0
            by=2 if airborne else 0
            for side in (-1,1):
                x=24+side*6
                knee=(x+side*swing,38+by)
                foot=(x-side*swing,46 if not airborne else 40+side*2)
                if state=='swing':knee=(x+shift,37);foot=(x+shift*2,43)
                limb(d,[(x,31+bob),knee,foot])
            im.alpha_composite(src.crop((12,20,37,34)),(12+shift,20+bob))
            im.alpha_composite(src.crop((14,3,35,22)),(14+shift,3+bob))
            d=ImageDraw.Draw(im)
            limb(d,[(15+shift,24+bob),(10,29-swing),(9,35-swing)])
            hand=(40,34+swing)
            if state in ('grab','swing','regrab','climb'):hand=(34,1);limb(d,[(32+shift,24),(36,13),hand])
            elif state in ('burst','reload'):
                recoil=f%2 if state=='burst' else 0
                hand=(40-recoil,25);limb(d,[(32,24),(36-recoil,28),hand]);rect(d,(39-recoil,21,47-recoil,25),20);rect(d,(40-recoil,22,47-recoil,23),12)
                if state=='reload':
                    limb(d,[(16,24),(22+f*2,30),(32+f*2,28-f)])
                    rect(d,(38,30-f,40,34-f),20);line(d,[(38,31-f),(40,31-f)],12)
            elif state=='victory':hand=(40,7);limb(d,[(32,24),(38,17),hand])
            elif state=='release':hand=(44,9+f*5);limb(d,[(32,24),(38,17),hand])
            else:limb(d,[(32+shift,24),(39,27+swing),hand])
            if state in ('jump','double_jump','fall'):
                # Distinct take-off / tuck / extend poses, not duplicated frames.
                im.paste(T,(8,34,42,48));d=ImageDraw.Draw(im)
                tuck=[0,6,2,4][f%4] if state!='fall' else [6,3,0][f]
                for side in (-1,1):limb(d,[(24+side*6,32),(24+side*(8+f),39-tuck),(24+side*(5+f),46-tuck)])
                if state=='double_jump':line(d,[(7+f,17),(4+f,22),(7+f,27)],13)
            if state in ('grab','regrab'):
                # Reach through three poses; socket reaches knot on final frame.
                im.paste(T,(33,0,43,20));d=ImageDraw.Draw(im)
                hand=(34+2*(2-f),1+5*(2-f));limb(d,[(32,24),(36,13),hand])
            if state=='climb':
                other=(15,8+(f%3)*5);limb(d,[(16,24),(12,17),other])
                im.paste(T,(10,35,38,48));d=ImageDraw.Draw(im)
                for side in (-1,1):limb(d,[(24+side*6,32),(24+side*8,36),(24+side*6,41+(f+side)%3*2)])
            if state=='victory':
                hand=(40-f%2*2,7+f%3);limb(d,[(32,24),(38,17),hand])
            if state=='idle' and f==3:line(d,[(24,12+bob),(29,12+bob)],7)
            if state=='slide':
                height=[29,24,27][f];body=im.crop((6,4,45,47)).resize((39,height),Image.Resampling.NEAREST);im=canvas(48,48);im.alpha_composite(body,(5,47-height))
            if state=='land':
                body=im.crop((4,2,45,48)).resize((44,40+f*4),Image.Resampling.NEAREST);im=canvas(48,48);im.alpha_composite(body,(2,8-f*4))
            if state=='death':
                height=40-f*6;body=im.crop((4,2,45,48)).resize((43,height),Image.Resampling.NEAREST);im=canvas(48,48);im.alpha_composite(body,(2,48-height))
            if state=='hit':
                d=ImageDraw.Draw(im);line(d,[(2,18+f*4),(7,15+f*4)],19);line(d,[(3,26),(7,26)],23)
            frames.append(im);hands.append(hand)
        save('selva_heroe_'+state,frames,'Hero',fps=12 if state=='run' else 8,loop=state in ('idle','run','swing','climb'),
             handPerFrame=hands,notes='Same face and protective suit awarded in Sierra, palette remap only. Frames 48x48. No identity redesign.')

def caiman_pose(f,state):
    im=canvas(64,32);d=ImageDraw.Draw(im)
    y=17+([7,2,-5,-7,-2,6][f%6] if state=='salto' else f%2 if state=='nadar' else 0)
    jaw=[0,2,6,8,4,0][f%6] if state in ('salto','morder') else 0
    poly(d,[(1,y+5),(12,y+1),(24,y-5),(41,y-6),(46,y-2),(61,y-1),(63,y+3),(45,y+5),(39,y+10),(20,y+10),(11,y+7)],2,0)
    poly(d,[(13,y+3),(25,y-4),(39,y-5),(43,y-1),(59,y),(60,y+1),(43,y+3),(27,y+4)],4)
    for x in (21,28,35,41):poly(d,[(x,y-3),(x+2,y-7),(x+4,y-3)],3,0)
    oval(d,(41,y-5,46,y),4,0);rect(d,(44,y-3,45,y-2),23);d.point((45,y-3),C[20])
    poly(d,[(42,y+4),(62,y+2+jaw),(60,y+5+jaw),(43,y+8)],3,0)
    line(d,[(47,y+4),(59,y+3+jaw)],17 if jaw else 0)
    for x in (48,53,58):poly(d,[(x,y+3),(x+2,y+3),(x+1,y+6)],6)
    for x in (23,36):
        step=round(math.sin(f*math.pi/3)*2) if state=='caminar' else 0
        poly(d,[(x,y+5),(x+4,y+6),(x+6+step,y+12),(x-1+step,y+12),(x-2,y+10)],2,0)
        line(d,[(x,y+11),(x+3,y+11)],5)
    if state=='hit':line(d,[(35+f,1),(37+f,5),(41+f,2)],23)
    if state=='idle' and f==1:line(d,[(43,y-3),(45,y-3)],2)
    if state=='derrota':
        im=im.transpose(Image.Transpose.FLIP_TOP_BOTTOM)
        im=im.transform((64,32),Image.Transform.AFFINE,(1,0,0,0,1,-f*2),resample=Image.Resampling.NEAREST)
    return im

def fauna():
    for state,n in [('idle',2),('nadar',6),('salto',6),('morder',4),('caminar',6),('hit',2),('derrota',4)]:
        save('selva_caiman_'+state,[caiman_pose(f,state) for f in range(n)],'Enemies',loop=state in ('idle','nadar','caminar'),fps=9)
    for animal,w,h,n in [('guacamayo',32,32,6),('murcielago',32,24,6),('serpiente',48,32,6),('mono',32,32,6),('delfin',64,32,6),('luciernaga',8,8,4)]:
        frames=[]
        for f in range(n):
            im=canvas(w,h);d=ImageDraw.Draw(im);a=math.sin(f*math.tau/n)
            if animal in ('guacamayo','murcielago'):
                wing=round(a*10);cx=16;cy=13
                poly(d,[(cx,cy),(5,8-wing),(1,14-wing),(7,14-wing),(10,19),(cx,17)],18 if animal=='guacamayo' else 21,0)
                poly(d,[(cx,cy),(25,7-wing),(31,12-wing),(27,14-wing),(23,18),(cx,17)],19 if animal=='guacamayo' else 21,0)
                oval(d,(12,9,20,20),18 if animal=='guacamayo' else 7,0)
                if animal=='guacamayo':
                    poly(d,[(13,18),(14,29),(18,23),(20,17)],11,0);oval(d,(16,8,23,14),23,0);poly(d,[(22,10),(26,12),(22,15)],7,0)
                    line(d,[(5,11-wing),(11,17)],11,2)
                else:
                    poly(d,[(13,10),(12,4),(16,8),(20,4),(20,11)],7,0)
                rect(d,(18,10,19,11),23)
            elif animal=='serpiente':
                pts=[(2,26),(12,24+round(a*2)),(25,27),(35,22),(31,16),(37,9)]
                line(d,pts,0,7);line(d,pts,4,5);line(d,pts,5,1)
                oval(d,(31,5,45,13),3,0);d.point((41,7),C[23]);line(d,[(44,10),(47,11-f%2)],18)
                for x in range(7,27,6):line(d,[(x,24),(x+2,27)],7)
            elif animal=='mono':
                line(d,[(12,20),(4,23),(1,16),(5,12)],0,4);line(d,[(12,20),(4,23),(1,16),(5,12)],8,2)
                oval(d,(10,10,22,26),7,0);oval(d,(14,6,27,17),8,0);oval(d,(20,9,26,15),9);d.point((24,10),C[0])
                limb(d,[(18,15),(25,18),(28,11+round(a*3))]);line(d,[(14,24),(10,30),(16,30)],0,3)
            elif animal=='delfin':
                y=17-round(math.sin(f*math.pi/5)*9)
                poly(d,[(4,y+6),(13,y),(31,y-5),(43,y-3),(50,y+2),(61,y+3),(58,y+6),(44,y+6),(32,y+11),(15,y+8),(5,y+12),(9,y+6)],19,0)
                poly(d,[(26,y-4),(31,y-10),(36,y-3)],18,0);poly(d,[(28,y+7),(39,y+14),(36,y+6)],18)
                line(d,[(16,y+2),(28,y-2),(42,y)],16);d.point((46,y+2),C[0])
            else:
                oval(d,(2,2,5,5),23 if f%2 else 6);d.point((3,3),C[13])
                if f%2:line(d,[(0,3),(1,3)],6);line(d,[(6,3),(7,3)],6)
            frames.append(im)
        save('selva_'+animal+'_movimiento',frames,'Enemies' if animal in ('murcielago','serpiente') else 'Fauna',fps=9 if animal=='guacamayo' else 7)

def bosses():
    # Entirely fictional guardians, no tribal people used as enemy stereotypes.
    for boss in ('raices','escarabajo'):
        for state,n in [('despertar',6),('idle',4),('andar',6),('anticipar',4),('golpe',6),('carga',6),('hit',2),('derrota',6)]:
            frames=[]
            for f in range(n):
                im=canvas(96,96);d=ImageDraw.Draw(im)
                crouch=f*3 if state=='derrota' else (12-f*2 if state=='despertar' else 3+f if state=='anticipar' else f%2)
                step=round(math.sin(f*math.pi/3)*5) if state in ('andar','carga') else 0
                for side in (-1,1):
                    x=48+side*16
                    poly(d,[(x-7,58+crouch),(x+7,58+crouch),(x+9+side*step,86),(x-11+side*step,86),(x-9,78)],7 if boss=='raices' else 22,0)
                    line(d,[(x-4,64+crouch),(x-1+side*step,79)],9 if boss=='raices' else 23,2)
                poly(d,[(28,26+crouch),(42,20+crouch),(60,20+crouch),(72,31+crouch),(69,64+crouch),(50,73+crouch),(27,63+crouch),(22,41+crouch)],7 if boss=='raices' else 22,0)
                if boss=='raices':
                    for x in (28,37,49,59):
                        line(d,[(x,29+crouch),(x+4,43+crouch),(x-1,65+crouch)],9,3)
                    for x,y in [(30,22),(58,26),(39,60)]:leaf(d,x,y+crouch,9,4,3,5)
                    poly(d,[(37,24+crouch),(31,9+crouch),(38,16+crouch),(42,6+crouch),(47,23+crouch)],8,0)
                else:
                    poly(d,[(28,31+crouch),(46,24+crouch),(46,64+crouch),(31,60+crouch)],23)
                    line(d,[(49,25+crouch),(49,68+crouch)],7,2)
                    poly(d,[(41,24+crouch),(39,13+crouch),(44,8+crouch),(48,17+crouch),(55,7+crouch),(58,13+crouch),(54,27+crouch)],22,0)
                    for x in (34,60):line(d,[(x,37+crouch),(x+4,42+crouch),(x,47+crouch)],7,2)
                    for side in (-1,1):
                        for yy in (44,58):
                            line(d,[(48+side*18,yy+crouch),(48+side*(35+step//2),yy+5+crouch),(48+side*40,yy+16)],0,5)
                            line(d,[(48+side*18,yy+crouch),(48+side*(35+step//2),yy+5+crouch),(48+side*40,yy+16)],22,3)
                oval(d,(38,26+crouch,60,42+crouch),0)
                glow=23 if state not in ('despertar','derrota') or f>2 else 3
                rect(d,(41,31+crouch,46,33+crouch),glow);rect(d,(52,31+crouch,57,33+crouch),glow)
                for side in (-1,1):
                    shoulder=(48+side*23,36+crouch)
                    extension=round(18*math.sin(f*math.pi/max(1,n-1))) if state=='golpe' and side==1 else 0
                    fist=(48+side*(31+extension),63+crouch-extension)
                    fist=(min(90,max(5,fist[0])),min(89,fist[1]))
                    line(d,[shoulder,(48+side*32,49+crouch),fist],0,12)
                    line(d,[shoulder,(48+side*32,49+crouch),fist],8 if boss=='raices' else 22,9)
                    oval(d,(fist[0]-6,fist[1]-7,fist[0]+5,fist[1]+4),9 if boss=='raices' else 23,0)
                if state=='anticipar':line(d,[(29,14-f),(33,18-f)],19,2);line(d,[(66,14-f),(62,18-f)],19,2)
                if state=='hit':line(d,[(10+f*3,19),(14+f*3,23)],23,2)
                frames.append(im)
            save(f'selva_boss_{boss}_{state}',frames,'Bosses',fps=8,loop=state in ('idle','andar'),
                 role='Cueva: guardian de raices' if boss=='raices' else 'Ruinas: centinela escarabajo de bronce',fiction=True)

def effects_props():
    for name,col in [('salpicadura',12),('polvo',9),('hojas',5),('impacto',23),('astillas',8),('escudo_romper',13),('destello',23),('portal',12),('veneno',5),('disparo',16),('rastro_salto',13)]:
        frames=[];size=48 if name in ('escudo_romper','portal') else 32
        for f in range(4 if name in ('salpicadura','escudo_romper') else 6):
            im=canvas(size,size);d=ImageDraw.Draw(im);cx=cy=size//2
            for j in range(8):
                a=j*math.tau/8;r=3+f*3;x=cx+round(math.cos(a)*r);y=cy+round(math.sin(a)*r)+f*f//6
                if name=='escudo_romper':
                    poly(d,[(x,y-3),(x+3,y),(x+1,y+4),(x-2,y+2)],col)
                elif name=='hojas':leaf(d,x,y,3,1,col)
                else:line(d,[(x,y),(x+round(math.cos(a)*max(1,4-f)),y+round(math.sin(a)*max(1,4-f)))],col,2 if f<3 else 1)
            frames.append(im)
        save('selva_fx_'+name,frames,'Effects',fps=12,loop=False)
    frames=[]
    for f in range(4):
        im=canvas(48,48);d=ImageDraw.Draw(im)
        pts=[(24+round(22*math.cos(j*math.pi/4)),24+round(22*math.sin(j*math.pi/4))) for j in range(9)]
        line(d,pts,12 if f%2 else 13)
        for j in range(0,8,2):line(d,[pts[j],pts[j+1]],23)
        frames.append(im)
    save('selva_fx_escudo_activo',frames,'Effects',fps=6)
    for obj in ('antorcha','flor','hongo','cascada','portal_salida','entrada_cueva','entrada_ruinas','cristal_salto','cristal_descenso'):
        w,h=(64,96) if obj.startswith('entrada') or obj=='portal_salida' else (32,96) if obj=='cascada' else (32,32)
        frames=[]
        for f in range(4):
            im=canvas(w,h);d=ImageDraw.Draw(im)
            if obj=='antorcha':
                rect(d,(14,16,17,31),7);line(d,[(15,18),(15,30)],9)
                poly(d,[(9,17),(10,9),(14,3+f%3),(16,11),(21,5+f%2),(23,15),(19,21),(13,22)],14,0)
                poly(d,[(13,17),(16,10+f%3),(19,17),(17,20)],23)
            elif obj=='flor':
                line(d,[(15,30),(17,9)],3,2)
                for x,y in [(12,10),(19,9),(15,15)]:leaf(d,x,y,4,3,18,19)
                leaf(d,10,23,6,2,4,5);oval(d,(14,8,17,11),23)
            elif obj=='hongo':
                rect(d,(14,14,17,29),8);oval(d,(4,7,27,20),18,0)
                line(d,[(7,17),(24,17)],16);rect(d,(10,11,12,12),23);rect(d,(19,10,21,11),23)
            elif obj=='cascada':
                poly(d,[(4,0),(26,0),(24,42),(28,91),(4,95),(8,46)],11)
                for x in (8,13,20,24):
                    for y in range(-14+f*4,96,22):line(d,[(x,y),(x-1,y+12)],13 if x%2 else 12)
                for x in (3,12,24):line(d,[(x,94),(x-2,88-f%2)],13,2)
            elif obj.startswith('cristal'):
                y=f%3;col=12 if obj.endswith('salto') else 18
                poly(d,[(16,2+y),(29,15+y),(16,28+y),(3,15+y)],col,0)
                poly(d,[(16,6+y),(23,15+y),(16,24+y),(9,15+y)],0)
                if obj.endswith('salto'):line(d,[(12,17+y),(16,11+y),(20,17+y)],23,2)
                else:line(d,[(12,12+y),(16,19+y),(20,12+y)],23,2)
            else:
                masonry(d,3,23,15,73,3,2);masonry(d,46,23,15,73,3,2);masonry(d,13,9,38,22,4,3)
                rect(d,(18,31,45,95),20)
                line(d,[(20,91),(20,30),(29,21),(37,21),(43,30),(43,91)],22)
                if obj=='portal_salida':
                    for j in range(7):
                        yy=36+(j*9+f*3)%55;line(d,[(24,yy),(37,yy-3)],13 if j%2 else 23)
                else:
                    for x in (7,51):fern(d,x,94,3)
                    line(d,[(8,14),(9,37),(5,55)],1,2)
            frames.append(im)
        save('selva_'+obj,frames,'Props',fps=5)

def extras():
    # Small minions and individual animated scenery, separate from the backgrounds.
    for who in ('brote','escarabajo'):
        for state,n in [('idle',4),('caminar',6),('ataque',6),('hit',2),('derrota',4)]:
            frames=[]
            for f in range(n):
                im=canvas(32,32);d=ImageDraw.Draw(im);bob=f%2;stride=round(math.sin(f*math.pi/3)*3)
                if who=='brote':
                    for x,side in [(11,-1),(21,1)]:limb(d,[(x,20),(x+side*stride,25),(x+side*stride,30)])
                    poly(d,[(7,9+bob),(13,6+bob),(23,9+bob),(25,24),(8,24)],8,0)
                    line(d,[(12,10),(14,21),(11,23)],9)
                    leaf(d,15,5+bob,8,3,3,5)
                    for x in (12,20):rect(d,(x,12+bob,x+1,13+bob),23)
                    arm=4+round(4*math.sin(f*math.pi/5)) if state=='ataque' else 3
                    line(d,[(23,16),(25+arm,12+f%2)],7,3)
                else:
                    for x in (8,15,23):
                        line(d,[(x,19),(x+stride,25),(x+stride+3,27)],0,3)
                        line(d,[(x,19),(x+stride,25),(x+stride+3,27)],22)
                    oval(d,(4,9+bob,26,24+bob),22,0);oval(d,(6,10+bob,23,19+bob),23)
                    line(d,[(15,10+bob),(15,22+bob)],7)
                    oval(d,(23,13+bob,30,21+bob),7,0);d.point((28,14+bob),C[23])
                    line(d,[(28,18+bob),(31,16-f%2)],22)
                if state=='hit':line(d,[(1+f,4),(4+f,7)],23)
                if state=='derrota':
                    body=im.crop((0,0,32,32)).resize((32,28-f*6),Image.Resampling.NEAREST);im=canvas(32,32);im.alpha_composite(body,(0,4+f*6))
                frames.append(im)
            save(f'selva_esbirro_{who}_{state}',frames,'Enemies',fps=9,loop=state in ('idle','caminar'),fiction=True)
    frames=[]
    for f in range(4):
        im=canvas(64,64);d=ImageDraw.Draw(im);bend=[0,1,2,1][f]
        for dx,h in [(-24,28),(-12,41),(2,51),(18,39),(26,24)]:
            line(d,[(31,63),(31+dx//2,63-h//2),(31+dx+bend,63-h)],0,3)
            line(d,[(31,63),(31+dx//2,63-h//2),(31+dx+bend,63-h)],3)
            for t in (.4,.7,1):leaf(d,31+dx*t+bend,63-h*t,7,3,3,5)
        frames.append(im)
    save('selva_helecho_viento',frames,'Fauna',fps=3)
    frames=[]
    for f in range(4):
        im=canvas(16,24);d=ImageDraw.Draw(im);yy=f*5
        oval(d,(6,yy,9,yy+4),12);d.point((7,yy),C[13]);frames.append(im)
    save('selva_gota_cueva',frames,'Effects',fps=8)
    for obj in ('roca_musgo','raiz_colgante','rama_anclaje','casa_ribera','barca_remador'):
        w,h=(96,80) if obj=='casa_ribera' else (80,48) if obj=='barca_remador' else (64,64)
        frames=[]
        for f in range(4 if obj=='barca_remador' else 1):
            im=canvas(w,h);d=ImageDraw.Draw(im)
            if obj=='roca_musgo':
                poly(d,[(3,58),(9,32),(24,20),(47,23),(60,47),(56,61)],2,0)
                poly(d,[(10,32),(25,22),(46,25),(51,34),(33,40)],3)
                line(d,[(14,31),(25,24),(43,27)],5,2);fern(d,27,24)
            elif obj=='raiz_colgante':
                for x in (12,31,48):
                    line(d,[(x,0),(x-5,15),(x+3,31),(x-4,49),(x-1,62)],0,5)
                    line(d,[(x,0),(x-5,15),(x+3,31),(x-4,49),(x-1,62)],8,3)
                    leaf(d,x,32,5,2,4)
            elif obj=='rama_anclaje':
                poly(d,[(0,13),(17,16),(31,10),(48,7),(63,0),(63,6),(51,13),(34,17),(18,23),(0,19)],8,0)
                line(d,[(1,15),(17,18),(31,13),(48,10)],9)
                for x,y in [(16,13),(32,8),(49,3)]:leaf(d,x,y,8,3,3,5)
            elif obj=='casa_ribera':
                for x in (17,76):rect(d,(x,45,x+4,79),7);line(d,[(x+1,47),(x+1,77)],9)
                rect(d,(15,27,80,59),8);rect(d,(42,39,57,59),0)
                poly(d,[(3,33),(28,8),(64,8),(91,33)],7,0)
                for y in range(15,32,4):line(d,[(31-(y-10),y),(63+(y-10),y)],9)
                line(d,[(16,57),(80,57)],9)
            else:
                im.alpha_composite(FRAMES['selva_plataforma_bote'][f],(0,16))
                oval(d,(31,5,39,13),8,0);poly(d,[(31,14),(40,14),(42,28),(29,28)],9,0)
                line(d,[(38,17),(46,19),(57-f*3,33)],7,2)
            frames.append(im)
        save('selva_decor_'+obj,frames,'Props',fps=5)

def collectibles_ui():
    for obj in ('ceramica','flauta','textil','sonaja','moneda'):
        frames=[]
        for f in range(2):
            im=canvas(16,16);d=ImageDraw.Draw(im)
            if obj=='ceramica':
                poly(d,[(5,1),(10,1),(10,4),(13,7),(13,12),(10,14),(5,14),(2,12),(2,7),(5,4)],9,0)
                line(d,[(4,8),(6,6),(8,8),(10,6),(12,8)],7);line(d,[(4,10),(7,12),(10,10)],23)
            elif obj=='flauta':
                poly(d,[(2,13),(11,1),(14,3),(5,15)],9,0)
                for x,y in [(6,10),(8,7),(10,4)]:d.point((x,y),C[0])
            elif obj=='textil':
                rect(d,(2,2,13,13),8);rect(d,(3,3,12,12),23)
                line(d,[(4,6),(7,6),(7,10),(10,10),(10,5)],17)
                for x in (3,6,9,12):line(d,[(x,13),(x,15)],9)
            elif obj=='sonaja':
                line(d,[(6,14),(9,6)],8,3);oval(d,(5,1,13,9),9,0);line(d,[(7,3),(11,3)],23)
            else:oval(d,(2,1,13,14),22,0);oval(d,(4,3,11,12),23);line(d,[(8,5),(6,7),(9,9)],22)
            d.point((12 if f else 3,2 if f else 12),C[23]);frames.append(im)
        save('selva_coleccionable_'+obj,frames,'Collectibles',fps=3,
             notes='Original illustrative motif, not a museum reconstruction. Cultural attribution must be researched before a historical ficha.')
        if obj!='moneda':
            icon=canvas(64,64);d=ImageDraw.Draw(icon)
            if obj=='ceramica':
                poly(d,[(24,7),(39,7),(40,13),(37,20),(46,28),(52,36),(51,48),(45,55),(20,55),(13,48),(12,37),(18,28),(26,20),(23,13)],8,0)
                poly(d,[(25,9),(36,9),(34,18),(36,24),(23,32),(20,45),(24,53),(18,49),(15,39),(20,29),(29,21)],9)
                oval(d,(23,5,40,12),9,0);oval(d,(26,7,37,10),7)
                for yy in (30,43):
                    line(d,[(17,yy),(48,yy)],23)
                    for x in (20,30,40):line(d,[(x,yy+2),(x+4,yy+2),(x+4,yy+7),(x+8,yy+7)],7)
            elif obj=='flauta':
                poly(d,[(18,58),(11,52),(45,5),(52,10)],9,0)
                line(d,[(15,51),(47,9)],23,2)
                for x,y in [(22,43),(27,36),(32,29),(37,22),(42,15)]:oval(d,(x,y,x+2,y+2),7)
                line(d,[(13,52),(19,56)],7);line(d,[(41,10),(48,14)],7)
            elif obj=='textil':
                poly(d,[(10,10),(50,7),(54,52),(15,55)],8,0)
                poly(d,[(13,12),(47,10),(51,49),(17,52)],23)
                for y in (16,31):
                    for x in (19,34):line(d,[(x,y),(x+8,y),(x+8,y+10),(x+3,y+10),(x+3,y+5)],17,2)
                for x in range(16,53,4):line(d,[(x,54),(x-1,60)],9)
                line(d,[(14,14),(17,49)],14)
            else:
                poly(d,[(29,34),(36,34),(38,59),(31,59)],8,0)
                oval(d,(15,6,49,40),8,0);oval(d,(18,9,44,34),9)
                line(d,[(22,14),(29,11),(37,12)],23,2)
                for x,y in [(25,22),(32,26),(39,20)]:rect(d,(x,y,x+1,y+1),7)
                for y in (42,48,54):line(d,[(31,y),(36,y)],23)
            save('selva_museo_'+obj,icon,'Museum',notes='Original native64px illustration; cultural object type proposal, not a documented historical replica.')
    # Palette-only skin components. Shared window layout remains untouched.
    for name,w,h in [('panel',48,48),('boton',48,16),('casilla',24,24),('barra',64,10)]:
        im=canvas(w,h);d=ImageDraw.Draw(im)
        rect(d,(0,0,w-1,h-1),0);rect(d,(1,1,w-2,h-2),7);rect(d,(2,2,w-3,h-3),22);rect(d,(4,4,w-5,h-5),1)
        line(d,[(3,h-3),(w-3,h-3),(w-3,3)],8)
        for x,y in [(2,2),(w-4,2),(2,h-4),(w-4,h-4)]:rect(d,(x,y,x+1,y+1),23)
        save('selva_ui_'+name,im,'UI',border=[5,5,5,5] if name=='panel' else [4,4,4,4],notes='Optional skin only. Reuse current shared UI; do not change layout or bake text.')

def run():
    for p in (OUT,PACK,ART,PRE):p.mkdir(parents=True,exist_ok=True)
    layers();terrain();lianas();hero();fauna();bosses();effects_props();extras();collectibles_ui()
    (PACK/'palette.json').write_text(json.dumps(PAL,indent=2),encoding='utf-8')
    (PACK/'manifest.json').write_text(json.dumps(dict(version=1,artOnly=True,status='review',assets=REG),indent=2,ensure_ascii=False),encoding='utf-8')
    print(json.dumps(dict(pngs=len(REG),frames=sum(a['frames'] for a in REG),output=str(OUT))))

if __name__=='__main__':run()
