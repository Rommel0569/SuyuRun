"""Sierra V3: deterministic native pixel construction matching approved Costa.

Read ART_BIBLE.md and SIERRA_PRODUCTION_BRIEF.md before editing. User authorized
Pillow pixel-by-pixel workflow; current request corrects style, not Costa assets.
V2 is preserved; NO imagegen landscape is used in these new layers.
"""
import json, math, hashlib
from PIL import Image, ImageDraw
import build_sierra_artpack as b

ROOT=b.ROOT
PACK=ROOT/'Assets/SuyuRun/Art/PixelSierra/ProductionV3'
ART=PACK/'Resources/SierraV3'
OUT=ROOT/'Artifacts/SierraV3'
C=b.C; HEX=b.HEX
canvas=b.canvas;poly=b.poly;line=b.line;rect=b.rect;ellipse=b.ellipse

def setup():
    b.PACK=PACK;b.ART=ART;b.OUT=OUT;b.registry.clear()
    for p in (PACK,ART,OUT):p.mkdir(exist_ok=True,parents=True)

def save(*a,**kw):return b.save(*a,**kw)

def cloud(d,x,y,w,h):
    # Costa-like broad scalloped silhouettes, lit undersides and sky-only dithering.
    mask=Image.new('1',(640,180));md=ImageDraw.Draw(mask)
    for a,c,e,f in [(0,.5,.4,.91),(.15,.25,.53,.94),(.34,0,.67,.9),(.56,.27,.88,.98),(.76,.54,1,1)]:
        md.ellipse((round(x+a*w),round(y+c*h),round(x+e*w),round(y+f*h)),fill=1)
    md.polygon([(x+4,y+h-5),(x+w-8,y+h-7),(x+w+5,y+h-1),(x+10,y+h+1)],fill=1)
    pix=mask.load()
    for xx in range(max(0,x),min(640,x+w+6)):
        ys=[yy for yy in range(max(0,y),min(180,y+h+3)) if pix[xx,yy]]
        if not ys:continue
        low=ys[-1]
        for yy in ys:
            depth=low-yy
            col=2 if depth>9 else 3 if depth>4 else 7
            if 4<=depth<=9 and (xx+yy)%2==0:col=4
            d.point((xx,yy),fill=C[col])
    for k in range(3):
        yy=y+round(h*(.4+k*.15));xx=x+round(w*(.24+(k%2)*.14))
        line(d,[(xx,yy),(xx+round(w*.2),yy)],3 if k<2 else 4)

def shared():
    im=canvas();d=ImageDraw.Draw(im)
    bands=[(0,3),(39,4),(83,7),(126,6),(154,7)]
    for i,(y,c) in enumerate(bands):
        rect(d,(0,y,639,179),c)
        if i:
            for yy in range(y,y+4):
                for xx in range(640):
                    if (xx+yy*3)%4>=yy-y+1:im.putpixel((xx,yy),C[bands[i-1][1]])
    ellipse(d,(186,71,230,115),6);rect(d,(187,103,229,107),7)
    for args in [(-32,23,132,41),(102,-12,194,40),(323,35,119,29),(510,4,159,39)]:cloud(d,*args)
    save('sierra_compartido_capa1_cielo',im,kind='layer',factor=0,pivot=(0,1),notes='Construcción nativa de Costa: masas planas, contorno escalonado, dithering solo cielo. Amanecer Sierra.')
    im=canvas();d=ImageDraw.Draw(im)
    for x,y,w in [(44,57,56),(218,27,78),(381,78,63),(523,52,72)]:
        poly(d,[(x,y+4),(x+9,y+4),(x+13,y+1),(x+24,y+1),(x+29,y-3),(x+38,y-3),(x+44,y),(x+w-8,y+2),(x+w,y+5)],7)
        line(d,[(x+2,y+5),(x+w+3,y+5)],6)
    save('sierra_compartido_capa2_nubes',im,kind='layer',factor=.05,pivot=(0,1))
    im=canvas();d=ImageDraw.Draw(im)
    # Broad independent distant snow range; three shadow planes, no photographic texture.
    points=[(0,137),(20,132),(37,124),(51,117),(66,112),(78,97),(90,92),(101,74),(112,69),(125,82),(139,85),(150,101),(163,106),(178,126),(202,137),
            (242,141),(264,131),(280,124),(292,108),(303,99),(313,98),(327,82),(340,76),(350,65),(361,61),(374,76),(389,87),(402,86),(415,104),(434,117),(451,125),(475,139),
            (504,138),(530,127),(544,111),(561,107),(574,93),(586,95),(597,110),(612,119),(640,137),(640,180),(0,180)]
    poly(d,points,2)
    for pts in [[(112,69),(125,82),(139,85),(150,101),(163,106),(178,126),(151,115),(137,106),(124,106),(109,124),(88,136),(112,101)],
                [(361,61),(374,76),(389,87),(402,86),(415,104),(434,117),(451,125),(422,121),(403,109),(390,110),(374,98),(359,117),(335,135),(348,107)],
                [(574,93),(586,95),(597,110),(612,119),(623,130),(591,118),(577,117),(559,129)]]:poly(d,pts,3)
    for pts in [[(90,92),(101,74),(112,69),(125,82),(139,85),(146,96),(126,88),(123,95),(113,83),(101,102),(99,92)],
                [(327,82),(340,76),(350,65),(361,61),(374,76),(389,87),(380,90),(368,81),(363,86),(354,75),(344,89),(339,85),(322,97)],
                [(561,107),(574,93),(586,95),(597,110),(585,103),(580,111),(572,102)]]:poly(d,pts,5)
    line(d,[(101,78),(112,71),(119,80)],7);line(d,[(340,79),(353,67),(360,64),(366,73)],7)
    im.paste(im.crop((0,0,1,180)),(639,0))
    save('sierra_compartido_capa3_nevados',im,kind='layer',factor=.12,pivot=(0,1),notes='Nevados nativos con áreas grandes y solo5colores, sin textura moteada.')

def hill(d,pts):
    poly(d,pts,17)

def terrace(d,x,y,w,rows=3):
    for r in range(rows):
        xx=x-r*5;yy=y+r*6;ww=w+r*10
        rect(d,(xx,yy,xx+ww,yy+4),12)
        line(d,[(xx,yy),(xx+ww,yy)],19)
        line(d,[(xx+1,yy+1),(xx+ww-1,yy+1)],18)
        for j in range(xx+8+r%2*5,xx+ww,18):line(d,[(j,yy+2),(j,yy+4)],11)

def landscape(act):
    im=canvas();d=ImageDraw.Draw(im)
    if act=='camino':
        pts=[(0,137),(22,136),(48,128),(76,113),(93,103),(110,102),(128,108),(144,117),(170,130),(204,140),
             (237,137),(263,124),(279,109),(294,99),(310,94),(324,97),(342,109),(364,116),(389,131),(415,142),
             (442,137),(463,129),(480,121),(500,113),(517,112),(538,117),(555,127),(587,135),(612,137),(640,137),(640,180),(0,180)]
        hill(d,pts)
        for pts in [[(26,139),(76,114),(93,104),(110,103),(99,115),(75,131),(57,143)],
                    [(231,142),(263,125),(294,100),(310,95),(305,109),(284,124),(278,136),(264,149)],
                    [(442,144),(480,122),(500,114),(517,113),(509,122),(486,136),(480,148)]]:poly(d,pts,18)
        for pts in [[(114,108),(130,111),(144,120),(174,134),(157,136),(131,124)],[(322,100),(341,113),(366,119),(389,134),(373,137),(348,126)],[(528,118),(553,129),(587,136),(573,141),(548,135)]]:poly(d,pts,16)
        terrace(d,73,132,29,3);terrace(d,304,126,35,3);terrace(d,495,141,32,2)
        for x,y in [(119,126),(259,141),(363,142),(531,139)]:
            line(d,[(x,y),(x,y-10)],11);poly(d,[(x-5,y-6),(x-6,y-10),(x-1,y-15),(x+4,y-12),(x+5,y-7)],16)
    elif act=='valle':
        # Two repeated gorge openings. No river baked into this layer.
        for origin in (-320,0,320,640):
            pts=[(origin-62,180),(origin-62,108),(origin-42,88),(origin-29,84),(origin-12,91),(origin+3,111),(origin+24,116),(origin+37,138),(origin+61,144),(origin+76,167),(origin+96,180)]
            poly(d,pts,11)
            poly(d,[(origin-29,86),(origin-13,92),(origin+2,113),(origin+20,117),(origin+25,137),(origin+40,143),(origin+49,166),(origin+74,180),(origin+25,180),(origin+14,152),(origin-3,143),(origin-10,119)],9)
            poly(d,[(origin-60,111),(origin-43,89),(origin-30,86),(origin-29,97),(origin-42,113),(origin-46,140),(origin-61,150)],10)
            for xx,yy,ww in [(origin-45,102,27),(origin-17,124,37),(origin+16,145,33),(origin+36,165,29)]:
                line(d,[(xx,yy),(xx+ww,yy)],18,2);line(d,[(xx,yy-1),(xx+ww-4,yy-1)],19)
                line(d,[(xx+6,yy+3),(xx+8,yy+13)],11)
        # Distant low banks leave water/sky visible through the gorge.
        for x in (138,458):
            poly(d,[(x-59,180),(x-37,157),(x-19,150),(x-6,141),(x+8,139),(x+30,151),(x+44,175),(x+65,180)],17)
            poly(d,[(x-37,157),(x-6,141),(x+8,139),(x-8,155),(x-16,177)],18)
    else:
        # Iconic steep green outcrop, in the same broad-cluster language as Paracas.
        for x in (153,478):
            poly(d,[(x-85,169),(x-63,150),(x-48,129),(x-34,112),(x-24,90),(x-15,75),(x-3,71),(x+7,76),(x+17,94),(x+25,102),(x+36,130),(x+56,146),(x+85,166),(x+87,180),(x-86,180)],17)
            poly(d,[(x-15,77),(x-4,73),(x+4,78),(x+7,101),(x+19,129),(x+13,152),(x-1,172),(x-13,157),(x-5,131),(x-12,109)],18)
            poly(d,[(x+8,81),(x+17,96),(x+25,106),(x+37,132),(x+53,145),(x+31,153),(x+18,134),(x+15,113)],16)
            line(d,[(x-22,98),(x-15,82),(x-5,75)],19)
        for x,y,w,n in [(12,142,74,4),(116,129,95,5),(266,150,47,3),(350,144,71,4),(444,131,99,5),(587,150,36,3)]:terrace(d,x,y,w,n)
        for x,y,w,h in [(29,121,26,21),(121,111,28,18),(158,113,22,16),(192,104,24,25),(354,123,26,21),(454,111,25,20),(489,104,30,27),(534,117,18,17)]:
            poly(d,[(x,y+h),(x+2,y),(x+w-3,y),(x+w,y+h)],12,11)
            poly(d,[(x+3,y+2),(x+w-4,y+2),(x+w-2,y+h-2),(x+2,y+h-2)],14)
            line(d,[(x+3,y+1),(x+w-4,y+1)],15)
            poly(d,[(x+w//2-3,y+h-2),(x+w//2-2,y+8),(x+w//2+3,y+8),(x+w//2+4,y+h-2)],11)
            line(d,[(x+4,y+6),(x+w-5,y+6)],13)
            line(d,[(x+4,y+12),(x+7,y+12)],13)
        for x,y in [(112,134),(330,153),(435,137)]:
            for k in range(5):rect(d,(x-k, y+k*3,x+7+k,y+k*3+1),15)
    im.paste(im.crop((0,0,1,180)),(639,0))
    save('sierra_'+act+'_capa4_paisaje',im,kind='layer',factor=.3,biome=act,pivot=(0,1),notes='Dibujo original nativo640x180; planos de color estilo Costa, sin reducción de foto ni recortes de composición.')

def river():
    frames=[]
    for f in range(6):
        im=canvas();d=ImageDraw.Draw(im)
        for offset in (0,320):
            poly(d,[(offset+134,140),(offset+140,140),(offset+155,151),(offset+145,160),(offset+176,180),(offset+106,180),(offset+132,160),(offset+146,151)],2)
            poly(d,[(offset+137,141),(offset+150,151),(offset+139,160),(offset+159,180),(offset+124,180),(offset+137,160),(offset+148,151)],3)
            for j in range(9):
                y=145+j*4;x=offset+141+round(math.sin(j*.9)*6)+(f%3)-1
                line(d,[(x-j//2,y),(x+1+j//2,y)],4 if (j+f)%3 else 5)
        frames.append(im)
    save('sierra_valle_capa5_rio',frames,fps=8,kind='layer',factor=.3,biome='valle',pivot=(0,1),notes='Cauce independiente dibujado completo, alineado con dos gargantas de capa4; mismo offset de scroll.6frames.')

def finish_pack():
    """Sparse material accents; preserve silhouettes, pixel grid and 24-color palette."""
    targets=[a for a in b.registry if '_capa4_paisaje' in a['name'] or '_capa6_cercano' in a['name'] or a['name'].startswith('sierra_puente_')]
    for a in targets:
        sheet=Image.open(ART/a['file']).convert('RGBA');frames=[]
        for index,r in enumerate(a['frames']):
            yy=sheet.height-r['y']-r['height'];im=sheet.crop((r['x'],yy,r['x']+r['width'],yy+r['height']));d=ImageDraw.Draw(im)
            name=a['name']
            if '_capa4_paisaje' in name:
                # Intentional short clusters, not blanket noise or extra dithering.
                for x in range(18,622,23):
                    for y in range(108+(x%7),174,19):
                        c=im.getpixel((x,y))
                        if c==C[18]:line(d,[(x,y),(x+4,y-1),(x+7,y-1)],19)
                        elif c==C[17]:line(d,[(x,y),(x+5,y),(x+7,y+2)],16)
                        elif c in (C[9],C[10]):
                            line(d,[(x,y-2),(x-1,y+2),(x+2,y+5)],11)
                            line(d,[(x+1,y-2),(x,y+1)],8)
                        elif c==C[14]:line(d,[(x,y),(x+4,y)],13)
                if 'ruinas' in name:
                    for x,y,w,h in [(29,121,26,21),(121,111,28,18),(158,113,22,16),(192,104,24,25),(354,123,26,21),(454,111,25,20),(489,104,30,27),(534,117,18,17)]:
                        line(d,[(x+4,y+3),(x+w-5,y+3)],15)
                        for row in range(y+6,y+h-2,5):
                            for col in range(x+4+row%2*3,x+w-4,9):
                                if im.getpixel((col,row)) in (C[13],C[14]):line(d,[(col,row-2),(col,row)],12)
                        for xx in (x+1,x+w-2):b.grass(d,xx,y+h,index%2,.35)
            elif '_capa6_cercano' in name:
                for x in (68,183,352,529):
                    y=148 if 'ruinas' in name else 154
                    b.grass(d,x,y,[0,1,0,-1][index%4],.7)
                    # Small kantuta accents, animation in stem, not a blanket of flowers.
                    k=[0,1,0,-1][index%4]
                    line(d,[(x+5,y),(x+6+k,y-8)],17)
                    poly(d,[(x+5+k,y-9),(x+8+k,y-9),(x+7+k,y-5)],21)
                    d.point((x+7+k,y-5),fill=C[8])
                for x in (145,390):
                    poly(d,[(x,155),(x+3,151),(x+7,150),(x+11,155)],12,0);line(d,[(x+3,152),(x+7,151)],14)
            else:
                # Keep damage/collapse states readable; do not paint intact ropes across gaps.
                for x in (4,249):
                    for y in (24,28,48,52):
                        line(d,[(x-4,y),(x+3,y+2)],11);line(d,[(x-3,y),(x+3,y+1)],7)
                    line(d,[(x-1,56),(x,60),(x-1,63)],11)
                for x in range(15,241,21):
                    y=38+round(13*math.sin(math.pi*x/255))+round(math.sin(index*math.pi/2)*math.sin(math.pi*x/255)*2)
                    if 'colapso' not in name and 0<=y+2<80 and im.getpixel((x,y+2))[3]:
                        line(d,[(x,y+2),(x+3,y+3)],11)
                        d.point((x+1,y+1),fill=C[15])
            if a['kind']=='layer':im.paste(im.crop((0,0,1,im.height)),(im.width-1,0))
            frames.append(im)
        b.registry.remove(a)
        save(a['name'],frames,fps=a['fps'],loop=a['loop'],kind=a['kind'],factor=a['factor'],biome=a['biome'],pivot=(a['pivotX'],a['pivotY']),notes=a['notes']+' Acabado material V3.1.',surface=a.get('surfaceY'))

def main():
    protected=[p for p in (ROOT/'Assets/SuyuRun/Art/PixelCosta').rglob('*') if p.is_file() and p.suffix=='.png']
    before={str(p.relative_to(ROOT)):hashlib.sha256(p.read_bytes()).hexdigest() for p in protected}
    setup();shared()
    for act in ('camino','valle','ruinas'):landscape(act)
    b.near_layers()
    # Replace ONLY newly produced V3 auxiliary river; keep V2 untouched.
    b.registry[:]=[a for a in b.registry if a['name']!='sierra_valle_capa5_rio'];river()
    b.structures();b.creatures();b.extra_modules();b.hero_sheets();b.effects();b.collectibles_ui()
    finish_pack()
    manifest=dict(version=3,baseWidth=320,baseHeight=180,ppu=32,palette=HEX,assets=b.registry,status='proposed_coast_style_pending_approval')
    (ART/'sierra_manifest.json').write_text(json.dumps(manifest,indent=2),encoding='utf-8')
    (PACK/'sierra_palette_v3.json').write_text(json.dumps(dict(status='proposed',colors=HEX),indent=2),encoding='utf-8')
    after={str(p.relative_to(ROOT)):hashlib.sha256(p.read_bytes()).hexdigest() for p in protected}
    assert before==after,'Costa changed'
    (OUT/'costa_protected_hashes.json').write_text(json.dumps(before,indent=2),encoding='utf-8')
    print(json.dumps(dict(assets=len(b.registry),frames=sum(a['count'] for a in b.registry),costa_pngs_unchanged=len(before))))

if __name__=='__main__':main()
