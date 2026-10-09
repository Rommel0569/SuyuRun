"""Selva V2, native pixel art and organic motion. User direction 02 October 2026.

Preserves V1 and every Costa/Sierra bitmap. Unity consumes SelvaV2 Resources.
28-color palette, integer clusters, alpha steps only on water surface/god rays.
"""
from pathlib import Path
from PIL import Image, ImageDraw
import math, json, random, hashlib, shutil, zipfile
import build_selva_artpack as b
from audit_sierra_midi import extract

ROOT=b.ROOT;OUT=ROOT/'Artifacts/SelvaV2'
ART=ROOT/'Assets/SuyuRun/Art/PixelSelva/ProductionV2/Resources/SelvaV2'
RES=ROOT/'Assets/SuyuRun/Data/Resources/SelvaIntegration'
EXTRA=['#E3EDC2','#30251F','#C67C73','#FFF0AE']
HEX=b.PAL['colors']+EXTRA
C=[tuple(bytes.fromhex(c[1:]))+(255,) for c in HEX]
canvas=b.canvas;poly=b.poly;line=b.line;rect=b.rect;oval=b.oval
registry=[]

def save(name,frames,fps=8,loop=True,group='Props',**extra):
    name=name.removeprefix('selva_')
    if isinstance(frames,Image.Image):frames=[frames]
    w,h=frames[0].size
    if extra.get('seamlessX'):
        for fr in frames:fr.paste(fr.crop((0,0,1,h)),(w-1,0))
    layer=group=='Layers';sheet=canvas(w if layer else w*len(frames),h*len(frames) if layer else h)
    for i,fr in enumerate(frames):sheet.alpha_composite(fr,(0,i*h) if layer else (i*w,0))
    sheet.save(ART/('selva_'+name+'.png'))
    cells=[dict(x=0 if layer else i*w,y=sheet.height-(i+1)*h if layer else 0,width=w,height=h) for i in range(len(frames))]
    spec=dict(name='selva_'+name,frameWidth=w,frameHeight=h,frames=len(frames),fps=fps,loop=loop,
              ppu=32,pivot=[0,1],rectsUnity=cells,group=group,**extra)
    (ART/('selva_'+name+'.json')).write_text(json.dumps(spec,indent=2),encoding='utf-8')
    registry[:]=[r for r in registry if r['name']!=spec['name']];registry.append(spec)
    return frames

def ellipse_cluster(d,x,y,w,h,c,light=None):
    # Overlapping lobes form soft volumes made of crisp native pixels.
    oval(d,(x-w,y-h//3,x+w*.5,y+h),c)
    oval(d,(x-w*.5,y-h,x+w*.55,y+h*.6),c)
    oval(d,(x,y-h*.55,x+w,y+h*.8),c)
    if light is not None:
        oval(d,(x-w*.55,y-h*.8,x+w*.1,y-h*.12),light)
        oval(d,(x+w*.03,y-h*.35,x+w*.60,y+h*.08),light)

def organic_tree(d,x,y,s=1,dark=False):
    def p(a,z):return x+a*s,y+z*s
    bark=25 if dark else 7
    poly(d,[p(-28,0),p(-15,-6),p(-8,-24),p(-6,-55),p(-12,-74),p(-28,-92),p(-24,-96),p(-5,-82),p(0,-92),p(14,-112),p(19,-112),p(12,-92),p(6,-75),p(9,-48),p(6,-23),p(14,-10),p(30,0),p(11,-2),p(0,-11),p(-10,-1)],bark)
    if not dark:
        poly(d,[p(-2,-82),p(2,-73),p(4,-48),p(0,-22),p(12,-7),p(6,-7),p(-5,-22),p(-3,-53)],8)
        line(d,[p(-2,-75),p(0,-54),p(-2,-30),p(5,-14)],9)
        line(d,[p(-5,-39),p(-8,-16),p(-19,-6)],8,2)
        for yy in (-65,-48,-28):line(d,[p(-4,yy),p(1,yy+5),p(-1,yy+9)],25)
    for dx,dy,w,h in [(-42,-94,29,13),(-19,-109,37,19),(20,-112,34,20),(47,-95,31,14),(3,-92,39,17)]:
        ellipse_cluster(d,*p(dx,dy),w*s,h*s,0 if dark else 2,None if dark else 3)
        if not dark:
            ellipse_cluster(d,*p(dx-5,dy-5),w*.5*s,h*.45*s,3,4)
            line(d,[p(dx-w*.3,dy-h*.6),p(dx,dy-h*.6)],5)

def layers():
    zones=['bosque','rio','cueva','ruinas','noche','templo']
    for zone in zones:
        night=zone=='noche';inside=zone in ('cueva','ruinas')
        # Keep the established Costa/Sierra cloud vocabulary but tune lighting.
        sky=canvas();d=ImageDraw.Draw(sky)
        for yy,col in ([(0,20),(52,21),(108,10),(146,1)] if night else [(0,25),(80,7),(130,1)] if inside else [(0,14),(48,15),(103,16),(146,23)]):rect(d,(0,yy,639,179),col)
        if not inside:
            oval(d,(202,59,243,100),27 if not night else 24)
            if night:oval(d,(214,53,246,87),21)
            b.clouds(d,night)
        save(zone+'_capa1_cielo',sky,group='Layers',factor=0,seamlessX=False)
        far=canvas();d=ImageDraw.Draw(far)
        if inside:
            for x in range(-80,720,80):
                poly(d,[(x,0),(x+70,0),(x+65,31),(x+47,45),(x+35,82),(x+27,53),(x+10,30)],7)
                line(d,[(x+57,5),(x+52,30),(x+38,47),(x+35,64)],8)
        else:
            for origin in range(0,640,40):
                for off in (-640,0,640):
                    x=origin+off;h=12+origin*7%25
                    ellipse_cluster(d,x,129-h*.5,38,h,10 if night else 3,11 if night else 4)
            rect(d,(0,135,639,179),10 if night else 3)
        far.paste(far.crop((0,0,1,180)),(639,0));save(zone+'_capa2_lejania',far,group='Layers',factor=.15,seamlessX=True)
        mid=canvas();d=ImageDraw.Draw(mid)
        if inside:
            for x in (35,262,483):
                if zone=='cueva':
                    poly(d,[(x,0),(x+40,0),(x+48,23),(x+31,46),(x+23,94),(x+15,66),(x+5,34)],7,25)
                    poly(d,[(x+11,0),(x+29,0),(x+33,19),(x+23,38),(x+21,65),(x+17,41)],8)
                    line(d,[(x+26,5),(x+27,19),(x+21,33)],9)
                else:
                    b.masonry(d,x,38,44,141,3,2);b.masonry(d,x-7,28,58,14,4,3)
                    line(d,[(x+10,62),(x+31,62),(x+31,76),(x+17,76)],9)
                    line(d,[(x+7,30),(x+4,60),(x+9,86),(x+5,123)],1,2)
                    b.fern(d,x+15,174,3)
        else:
            for x in (155,418,609):
                for off in (-640,0,640):organic_tree(d,x+off,179,.95,night)
            for x in (73,317,517):b.palm(d,x,174,74,1 if night else 2,2 if night else 4)
        mid.paste(mid.crop((0,0,1,180)),(639,0));save(zone+'_capa3_estructura',mid,group='Layers',factor=.32,seamlessX=True)
        near=canvas();d=ImageDraw.Draw(near)
        if zone=='templo':
            for x in (114,414):
                for k in range(4):b.masonry(d,x+k*12,156-k*15,104-k*24,15,9,7)
                rect(d,(x+39,95,x+63,126),25);rect(d,(x+42,96,x+60,124),22)
                line(d,[(x+46,99),(x+46,119),(x+55,119),(x+55,103)],23)
        else:
            for x in (14,347,576):
                for off in (-640,0,640):
                    xx=x+off
                    if inside:
                        poly(d,[(xx-20,180),(xx-15,157),(xx,139),(xx+9,144),(xx+21,180)],7,25)
                        line(d,[(xx-12,159),(xx,142),(xx+5,150)],9)
                    else:organic_tree(d,xx,181,1.05,night)
        near.paste(near.crop((0,0,1,180)),(639,0));save(zone+'_capa5_cercania',near,group='Layers',factor=.72,seamlessX=True)
        front=canvas();d=ImageDraw.Draw(front)
        for origin in (20,202,471,611):
            for off in (-640,0,640):
                x=origin+off
                for dx,dy,w,h in [(-11,172,18,6),(4,164,16,5),(21,174,15,6)]:ellipse_cluster(d,x+dx,dy,w,h,0)
                b.fern(d,x,179,0)
        front.paste(front.crop((0,0,1,180)),(639,0));save(zone+'_capa6_frontal',front,group='Layers',factor=1.3,seamlessX=True)
    # Mist and light are separate images, not part of vegetation or colliders.
    fog=canvas();d=ImageDraw.Draw(fog)
    bayer=((0,8,2,10),(12,4,14,6),(3,11,1,9),(15,7,13,5))
    for y in range(109,137):
        for x in range(640):
            center=123+math.sin(x*math.tau/640)*4
            density=max(0,3-int(abs(y-center)/2.5))
            if density>bayer[y%4][x%4]:d.point((x,y),C[3])
    fog.paste(fog.crop((0,0,1,180)),(639,0));save('niebla',fog,group='Layers',factor=.08,seamlessX=True)
    rays=canvas();d=ImageDraw.Draw(rays)
    for x,w in [(69,18),(236,23),(442,13),(566,21)]:
        d.polygon([(x,0),(x+w,0),(x+w-39,150),(x-80,150)],fill=C[27][:3]+(32,))
        d.polygon([(x+4,0),(x+7,0),(x-59,150),(x-69,150)],fill=C[27][:3]+(48,))
    rays.paste(rays.crop((0,0,1,180)),(639,0));save('rayos_sol',rays,group='Layers',factor=.20,seamlessX=True,alphaException='discrete sun shafts; no blur or alpha gradient')

def water():
    back=[];surface=[]
    for f in range(6):
        im=canvas();d=ImageDraw.Draw(im)
        rect(d,(0,0,639,179),10)
        for y,c in [(4,11),(16,10),(50,1),(110,0)]:rect(d,(0,y,639,179),c)
        for j in range(92):
            x=(j*37+f*(1+j%3))%640;y=4+(j*11)%110
            width=5+(j%7)*2
            for off in (-640,0,640):line(d,[(x+off,y),(x+off+width,y)],11 if j%4 else 12)
        for x in range(640):
            yy=2+round(math.sin((x+f*4)*math.tau/80))
            if x%32<20:d.point((x,yy),C[13])
        im.paste(im.crop((0,0,1,180)),(639,0));back.append(im)
        im=canvas();d=ImageDraw.Draw(im)
        d.rectangle((0,3,639,179),fill=C[10][:3]+(125,))
        for j in range(35):
            x=(j*59-f*3)%640;y=5+j*9%31
            for off in (-640,0,640):line(d,[(x+off,y),(x+off+3+j%9,y)],12 if j%3 else 24)
        for x in range(640):
            yy=1+round(math.sin((x+f*4)*math.tau/80));im.putpixel((x,yy),C[13] if x%13<8 else C[11])
        im.paste(im.crop((0,0,1,180)),(639,0));surface.append(im)
    save('agua_fondo',back,group='Layers',fps=8,factor=1,seamlessX=True)
    save('agua_superficie',surface,group='Layers',fps=8,factor=1,seamlessX=True,alphaException='125/255 tinted water above submerged objects')
    for effect,w,h,n in [('ondas',64,24,8),('burbujas',32,32,8),('splash_salida',64,48,8),('splash_entrada',80,40,10),('espuma',64,16,8)]:
        frames=[]
        for f in range(n):
            im=canvas(w,h);d=ImageDraw.Draw(im)
            if effect in ('ondas','espuma'):
                for ring in range(3):
                    r=3+f*3+ring*4
                    d.ellipse((w//2-r,h//2-max(1,r//5),w//2+r,h//2+max(1,r//5)),outline=C[[11,13,24][ring]])
                if effect=='espuma':
                    for j in range(max(0,12-f)):
                        x=(j*17)%w;y=5+j%3*2;line(d,[(x,y),(x+2,y)],24)
            elif effect=='burbujas':
                for j in range(5):
                    x=4+j*7%25;y=28-((f*3+j*5)%27)
                    d.ellipse((x-1,y-2,x+2,y+1),outline=C[12]);d.point((x,y-2),C[24])
            else:
                cx=w//2;base=h-6;count=13
                for j in range(count):
                    a=-math.pi+.15+j*(math.pi-.3)/(count-1)
                    r=(3+f*3)*(1+.3*math.sin(j*7))
                    x=cx+math.cos(a)*r;y=base+math.sin(a)*r+f*f*.45
                    if f<4:
                        line(d,[(cx+(j-6)*2,base),(x,y)],11 if j%2 else 13,2)
                    if y<h-1:
                        oval(d,(x-1,y-2,x+1,y+1),24 if j%3==0 else 13)
                r=4+f*3;d.ellipse((cx-r,base-2,cx+r,base+3),outline=C[24] if f<4 else C[12])
            frames.append(im)
        save('fx_'+effect,frames,fps=12,loop=False,group='Effects')
    # Shadow used in telegraphing, prior to the solid body appearing.
    im=canvas(64,16);d=ImageDraw.Draw(im)
    oval(d,(4,4,59,11),0);oval(d,(18,2,43,13),0)
    save('caiman_sombra',im,group='Enemies')

def caiman_frame(angle=0,jaw=0,tuck=0,light=True):
    im=canvas(64,64);d=ImageDraw.Draw(im);a=math.radians(angle)
    def p(x,y):
        x-=30;y-=33
        return 30+round(x*math.cos(a)-y*math.sin(a)),33+round(x*math.sin(a)+y*math.cos(a))
    def shape(pts,c,o=None):poly(d,[p(x,y) for x,y in pts],c,o)
    # S-shaped taper, rounded belly and high eye ridge, no rectangular body.
    shape([(1,36),(9,35),(15,30),(21,27),(31,24),(40,25),(45,29),(52,28),(61,30),(62,34),(55,37),(43,38),(36,44),(26,45),(17,42),(10,39)],2,25)
    shape([(11,36),(19,31),(29,26),(39,27),(43,30),(53,30),(59,31),(55,33),(41,32),(33,35),(22,36)],4)
    shape([(16,39),(27,40),(39,37),(42,38),(35,43),(26,44)],3)
    for x,y in [(18,29),(25,26),(32,25),(39,26)]:shape([(x-2,y),(x,y-3),(x+3,y+1)],3,25)
    shape([(44,33),(61,34+jaw),(58,38+jaw),(44,39)],3,25)
    if jaw:
        shape([(45,33),(59,33+jaw),(57,36+jaw),(46,37)],26)
        shape([(47,34),(57,34+jaw),(53,36+jaw)],17)
        for x in (47,52,57):shape([(x,32),(x+2,33),(x+1,36)],24)
        for x in (48,54):shape([(x,36+jaw),(x+1,33+jaw),(x+2,37+jaw)],24)
    oval(d,(*p(41,25),*p(45,29)),4) if angle==0 else shape([(40,28),(41,25),(45,25),(46,28)],4,25)
    shape([(43,26),(45,26),(45,27),(43,27)],23)
    for x,sgn in [(21,-1),(37,1)]:
        shape([(x,39),(x+4,39),(x+7,43-tuck),(x+5,47-tuck),(x-1,47-tuck),(x,43)],2,25)
        for xx in (x,x+3):shape([(xx,46-tuck),(xx+1,46-tuck),(xx+1,47-tuck)],24)
    # Selective specular streaks on wet spine, never stipple/noise.
    if light:
        for pts in [[(19,30),(23,28),(28,28)],[(33,27),(38,28)],[(50,30),(55,30)]]:line(d,[p(x,y) for x,y in pts],6)
        for pts in [[(24,27),(26,26)],[(36,26),(38,26)]]:line(d,[p(x,y) for x,y in pts],24)
    def box(points):
        pts=[p(x,y) for x,y in points];xs=[v[0] for v in pts];ys=[v[1] for v in pts]
        return [min(xs),min(ys),max(xs)-min(xs),max(ys)-min(ys)]
    return im,box([(17,28),(40,27),(42,40),(20,44)]),box([(45,29),(61,30),(61,38+jaw),(45,39)])

def caimans():
    phases={'acecho':[(0,0,0)]*4,'anticipacion':[(-5,2,0),(-15,4,2),(-26,8,4)],
            'emerger':[(-48,7,5),(-39,10,5),(-26,10,4),(-14,10,3)],
            'snap':[(-6,8,3),(0,0,2),(9,1,1)],'caida':[(18,2,2),(31,2,4),(43,1,5),(57,0,5)],
            'inmersion':[(58,0,5),(63,0,5),(66,0,5),(70,0,5),(73,0,5),(75,0,5)],
            'nadar':[(0,0,0),(2,0,1),(0,0,0),(-2,0,1)]}
    for state,poses in phases.items():
        frames=[];boxes=[]
        for i,(a,j,t) in enumerate(poses):
            im,body,mouth=caiman_frame(a,j,t)
            if state=='acecho':
                im=canvas(64,64);d=ImageDraw.Draw(im);oval(d,(4,35,58,43),0)
                for z in range(3):d.ellipse((20+z*9,30-i*2,23+z*9,33-i*2),outline=C[12])
            if state=='inmersion':
                ratio=i/(len(poses)-1);pixels=im.load()
                for yy in range(64):
                    for xx in range(64):
                        c=pixels[xx,yy]
                        if c[3]:pixels[xx,yy]=C[10] if ratio<.35 else C[1] if ratio<.7 else C[0]
                # Dive travels physically below river; last frame is a faint silhouette.
            frames.append(im);boxes.append(dict(body=body,mouth=mouth,damaging=state in ('emerger','snap','caida'),snap=state=='snap' and i==1))
        save('caiman_'+state,frames,fps=16 if state=='snap' else 10,loop=state in ('acecho','nadar'),group='Enemies',
             hitboxes=boxes,notes='64x64 rotation authored at native pixels. Body excludes tail. Only snap frame1 closes the mouth. Dive tint is palette-limited.')
    hitboxes=[caiman_frame(a,j,t)[1:] for a,j,t in phases['snap']]
    # Overview proving every key phase, marked boxes are PREVIEW only.
    contact=Image.new('RGB',(7*256,256),(10,14,42));d=ImageDraw.Draw(contact)
    for k,state in enumerate(phases):
        im=Image.open(ART/('selva_caiman_'+state+'.png')).crop((0,0,64,64)).convert('RGBA')
        contact.paste(im.resize((256,256),Image.Resampling.NEAREST),(k*256,0),im.resize((256,256),Image.Resampling.NEAREST))
        d.text((k*256+4,4),state.upper(),fill=HEX[27])
    contact.save(OUT/'caiman_fases_x4.png')

def hero():
    src=b.quant(Image.open(ROOT/'Assets/SuyuRun/Art/PixelSierra/ProductionV3/Resources/SierraV3/sierra_premio_traje_idle.png').crop((0,0,48,48)))
    head=src.crop((14,3,35,22));body=src.crop((12,20,37,34))
    states={'idle':6,'run':12,'jump':5,'double_jump':5,'land':3,'slide':4,'burst':4,'reload':5,'hit':3,
            'grab':4,'swing':16,'release':5,'regrab':4,'fall':4,'climb':6,'victory':6,'death':6}
    for state,n in states.items():
        frames=[];hands=[]
        for f in range(n):
            im=canvas(48,48);d=ImageDraw.Draw(im);phase=f*math.tau/n
            bob=round(math.sin(phase)*1) if state=='idle' else round(abs(math.sin(phase))*2) if state=='run' else 0
            angle=-52+104*f/(n-1) if state=='swing' else 0
            tilt=round(angle*.17) if state=='swing' else 2 if state=='run' else -6+f*3 if state=='jump' else 0
            bx=23;by=26+bob;torsoY=19+bob
            if state=='swing':by=26;torsoY=20
            # Jointed limbs: stagger knees, feet trail torso, elbows flex.
            for side in (-1,1):
                hip=(bx+side*5,32+bob)
                if state=='run':
                    stride=math.sin(phase+side*.7);knee=(bx+side*5+round(stride*7),38+bob-round(max(0,-stride)*4));foot=(bx+side*5-round(stride*9),46-round(max(0,-stride)*3))
                elif state=='swing':
                    tuck=abs(angle)/52;trail=round(math.sin(math.radians(angle))*8)
                    knee=(bx+side*5+trail,39-round(tuck*6));foot=(bx+side*4+trail*2,46-round(tuck*10))
                elif state in ('jump','double_jump','fall','release','regrab','grab'):
                    tuck=round(math.sin(f/(n-1)*math.pi)*6)
                    knee=(bx+side*(7+f%2),38-tuck);foot=(bx+side*(5+f),45-tuck)
                else:knee=(bx+side*5,38);foot=(bx+side*5,46)
                line(d,[hip,knee,foot],25,5);line(d,[hip,knee,foot],7,3);line(d,[hip,knee],8)
                oval(d,(foot[0]-3,foot[1]-1,foot[0]+2,foot[1]+1),25)
            im.alpha_composite(body,(11,torsoY));im.alpha_composite(head,(13,3+bob))
            if tilt:
                # Rounded joints stay pixel-sharp; discrete torso lean is not runtime interpolation.
                upper=im.crop((6,2,41,35)).rotate(-tilt,resample=Image.Resampling.NEAREST,expand=False)
                im.paste((0,0,0,0),(6,2,41,35));im.alpha_composite(upper,(6,2));d=ImageDraw.Draw(im)
            def arm(p):
                line(d,p,25,5);line(d,p,8,3);oval(d,(p[-1][0]-2,p[-1][1]-2,p[-1][0]+1,p[-1][1]+1),9)
            if state=='swing':
                hand=(24,2);elbow=(29+round(angle/30),13);arm([(31,22),elbow,hand]);arm([(15,23),(10,30),(8,33)])
            elif state in ('grab','regrab'):
                hand=(24+round((n-1-f)*3),2+round((n-1-f)*3));arm([(31,22),(34,12),hand]);arm([(15,23),(10,28),(11,34)])
            elif state=='release':
                hand=(26+f*3,3+f*3);arm([(31,22),(34,12),hand]);arm([(15,23),(10,30),(9,33)])
            elif state in ('burst','reload'):
                hand=(42,24-f%2);arm([(30,23),(37,26),hand]);rect(d,(39,20-f%2,47,24-f%2),25);line(d,[(40,21-f%2),(47,21-f%2)],12)
                arm([(15,23),(20,30),(34,26)])
                if state=='reload':rect(d,(38,27-f,40,31-f),25)
            else:
                trail=round(math.sin(phase)*5) if state=='run' else f%2
                hand=(39,32+trail);arm([(31,23+bob),(37,26+trail),hand]);arm([(15,23+bob),(10,27-trail),(8,33-trail)])
            if state=='idle' and f==4:line(d,[(23,12+bob),(28,12+bob)],8)
            if state=='slide':
                h=[29,25,24,28][f];small=im.crop((4,2,46,48)).resize((44,h),Image.Resampling.NEAREST);im=canvas(48,48);im.alpha_composite(small,(2,48-h))
            if state=='land':
                h=[38,42,46][f];small=im.crop((3,2,45,48)).resize((44,h),Image.Resampling.NEAREST);im=canvas(48,48);im.alpha_composite(small,(2,48-h))
            if state=='death':
                h=46-f*5;small=im.crop((2,2,46,48)).resize((46,h),Image.Resampling.NEAREST);im=canvas(48,48);im.alpha_composite(small,(1,48-h))
            if state=='hit':line(ImageDraw.Draw(im),[(3,15+f),(7,19+f)],19)
            frames.append(im);hands.append(hand)
        save('heroe_'+state,frames,fps=14 if state=='run' else 9,loop=state in ('idle','run','swing','climb'),group='Hero',handPerFrame=hands)

def lianas():
    frames=[];grips=[];angles=[]
    for i in range(17):
        angle=-56+i*7;a=math.radians(angle);end=(64+round(82*math.sin(a)),3+round(82*math.cos(a)))
        bend=math.sin(a)*9;pts=[]
        for j in range(40):
            t=j/39;pts.append((64+(end[0]-64)*t+bend*math.sin(t*math.pi),3+(end[1]-3)*t))
        im=canvas(144,104);d=ImageDraw.Draw(im);line(d,pts,25,3);line(d,pts,4)
        for j in (8,17,26):
            xx,yy=pts[j];b.leaf(d,xx,yy,3,2,3,5)
        oval(d,(end[0]-3,end[1]-3,end[0]+3,end[1]+3),23,25)
        frames.append(im);grips.append(end);angles.append(angle)
    save('liana_curva',frames,fps=8,group='Traversal',anchor=[64,3],gripPerFrame=grips,angles=angles,
         notes='Pose bank sampled by pendulum angle. Runtime bend follows angular inertia; no rigid rod rotation.')

def midi_course():
    data=extract(ROOT/'Assets/selva.midi');RES.mkdir(parents=True,exist_ok=True)
    (RES/'selva_midi_exact.json').write_text(json.dumps(data),encoding='utf-8')
    duration=json.loads((ROOT/'Artifacts/Audio/selva_rhythm.json').read_text())['duration']
    beats=data['beatTimes'];events=[];last=-5
    # Fixed authored motifs: at least one full jump-flight between consecutive hazards.
    for i in range(8,len(beats)-4,3):
        t=beats[i]
        if t>duration-3 or any(abs(t-k)<2 for k in (45,90,135)):continue
        # River dangers only on stable banks; vines and caimans handle the water.
        if 45<=t<90 and not 48<((t-45)*160+92)%704<145:continue
        kind=['jump','slide','root','double','estaca','slide','trampa','air'][i//3%8]
        if 45<=t<90:kind='jump'
        events.append(dict(beat=i,time=t,x=t*160+92,kind=kind));last=t
    course=dict(duration=duration,speed=160,events=events,zones=['bosque','rio','cueva','noche','ruinas','templo'],
                runnerIntervals=[[0,45],[45,90],[90,135],[135,duration]],miniDuration=60)
    (RES/'selva_course.json').write_text(json.dumps(course,indent=2),encoding='utf-8')
    review=dict(midiDuration=data['duration'],audioDuration=duration,noteOnCount=len(data['notes']),
                doubtful=[n for n in data['notes'] if abs(n['tick']/data['ticksPerBeat']-round(n['tick']/data['ticksPerBeat']))>.18][:150],
                note='Exact tempo map preserved. Ornament offsets and bass/drum identity require listening. No live analysis or global stretch.')
    (OUT/'midi_review.json').write_text(json.dumps(review,indent=2),encoding='utf-8')

def previews():
    # Native-sprite phase study: arc, snap and gradual immersion with reactive water.
    frames=[]
    for i in range(80):
        im=canvas(320,180)
        for layer in ('rio_capa1_cielo','rio_capa2_lejania','rio_capa3_estructura','rio_capa5_cercania'):
            src=Image.open(ART/('selva_'+layer+'.png')).convert('RGBA');im.alpha_composite(src,(0,0))
        def put(name,x,y,frame=0):
            s=next(s for s in registry if s['name']=='selva_'+name);w,h=s['frameWidth'],s['frameHeight']
            tex=Image.open(ART/(s['name']+'.png')).convert('RGBA');f=frame%s['frames'];r=s['rectsUnity'][f]
            top=tex.height-r['y']-h;cell=tex.crop((r['x'],top,r['x']+w,top+h));im.alpha_composite(cell,(round(x),round(y)))
        put('agua_fondo',0,145,i//2)
        t=i*.05
        if t<.8:
            put('caiman_sombra',135,151);put('fx_burbujas',151,128,i//2);put('fx_ondas',135,139,i//3)
        elif t<1.7:
            a=t-.8;y=145-260*a+310*a*a;state='emerger' if a<.32 else 'snap' if a<.51 else 'caida';f=int(a*12)
            put('caiman_'+state,130+a*38,y-38,f)
            if a<.5:put('fx_splash_salida',128,103,min(7,int(a*14)))
        elif t<2.5:
            a=t-1.7;put('caiman_inmersion',165,113+a*60,min(5,int(a*9)));put('fx_splash_entrada',157,113,min(9,int(a*14)))
        put('agua_superficie',0,145,i//2)
        if t>=2.5:put('fx_ondas',155,139,min(7,int((t-2.5)*9)))
        angle=math.sin(t*1.7)*48;idx=round((angle+56)/7);put('liana_curva',12,3,idx)
        spec=next(s for s in registry if s['name']=='selva_liana_curva');gx,gy=spec['gripPerFrame'][idx]
        hf=round((angle+52)/104*15);put('heroe_swing',12+gx-24,3+gy-2,hf)
        put('rayos_sol',0,0)
        frames.append(im.resize((1280,720),Image.Resampling.NEAREST).convert('RGB'))
    frames[0].save(OUT/'selva_emboscada_balanceo.gif',save_all=True,append_images=frames[1:],duration=50,loop=0)
    frames[24].save(OUT/'selva_pulido_x4.png')
    contact=Image.new('RGB',(1280,720))
    for k,frame in enumerate((8,24,34,41)):contact.paste(frames[frame].resize((640,360),Image.Resampling.NEAREST),((k%2)*640,(k//2)*360))
    contact.save(OUT/'caiman_ciclo.png')

def validate():
    failures=[];seen=set()
    for s in registry:
        im=Image.open(ART/(s['name']+'.png')).convert('RGBA');used=set(im.getdata());colors={v[:3] for v in used if v[3]}
        if not colors.issubset({c[:3] for c in C}):failures.append(s['name']+' palette')
        if any(v[3] not in (0,255) for v in used) and s['name'] not in ('selva_agua_superficie','selva_rayos_sol'):failures.append(s['name']+' alpha')
        if s.get('seamlessX'):
            for r in s['rectsUnity']:
                top=im.height-r['y']-r['height']
                w=s['frameWidth']
                if list(im.crop((0,top,1,top+r['height'])).getdata())!=list(im.crop((w-1,top,w,top+r['height'])).getdata()):failures.append(s['name']+' seam')
        seen|=colors
    report=dict(pngs=len(registry),frames=sum(s['frames'] for s in registry),paletteColorsUsed=len(seen),failures=failures)
    (OUT/'art-validation.json').write_text(json.dumps(report,indent=2),encoding='utf-8');assert not failures,failures
    print(json.dumps(report))

def main():
    for p in (OUT,ART,RES):p.mkdir(parents=True,exist_ok=True)
    b.C=C;b.PAL['colors']=HEX
    # Reuse unchanged supporting modules; new files only, retain V1 proposal.
    original_save=b.save
    def bridge_save(name,frames,group='Props',fps=8,loop=True,factor=1,seam=False,**extra):
        b.FRAMES[name]=[frames] if isinstance(frames,Image.Image) else frames
        return save(name,frames,fps,loop,group,factor=factor,seamlessX=seam,**extra)
    b.save=bridge_save
    b.terrain();b.fauna();b.bosses();b.effects_props();b.extras();b.collectibles_ui()
    layers();water();caimans();hero();lianas();midi_course()
    (ART/'selva_palette.json').write_text(json.dumps(dict(name='Selva V2 organic',colors=HEX,color_count=len(C),status='user-directed revision, visual review pending'),indent=2),encoding='utf-8')
    (ART/'selva_manifest.json').write_text(json.dumps(dict(version=2,assets=registry),indent=2),encoding='utf-8')
    previews();validate()

if __name__=='__main__':main()
