"""Original, native-pixel cave pack. Batch explicitly authorized 25 September.
No existing Sierra/Costa PNG is overwritten; uses the approved 24 Sierra colors.
"""
import json, math
from pathlib import Path
from PIL import Image, ImageDraw
import build_sierra_artpack as b

ROOT=Path(__file__).resolve().parents[1]
ART=ROOT/'Assets/SuyuRun/Art/PixelSierra/ProductionV3/Resources/SierraV3'
OUT=ROOT/'Artifacts/SierraCaves'

def save(name,frames,**kw):
    return b.save('sierra_'+name,frames,**kw)

def stone(d,x,y,w,h,base=12,edge=14):
    b.poly(d,[(x+3,y),(x+w-4,y),(x+w-1,y+3),(x+w-1,y+h-3),(x+w-4,y+h-1),(x+2,y+h-1),(x,y+h-4),(x,y+3)],base,0)
    b.line(d,[(x+3,y+1),(x+w-5,y+1),(x+w-3,y+3)],edge)
    b.line(d,[(x+2,y+h-3),(x+w-4,y+h-3),(x+w-2,y+h-5)],11)

def backgrounds(index):
    im=b.canvas(640,384);d=ImageDraw.Draw(im);b.rect(d,(0,0,639,383),0)
    shade=1 if index==1 else 11
    # Geological silhouettes: hard planes, no rock dithering or gradients.
    for x in range(-64,700,128):
        b.poly(d,[(x,0),(x+98,0),(x+112,38),(x+77,52),(x+68,88),(x+32,127),(x+10,72)],shade)
        b.poly(d,[(x+14,0),(x+68,0),(x+74,34),(x+42,53),(x+32,99),(x+23,55)],12 if index==2 else 2)
        b.poly(d,[(x,384),(x+120,384),(x+104,301),(x+79,254),(x+46,272),(x+32,321)],shade)
    for x in range(35,640,160):
        b.poly(d,[(x,170),(x+18,117),(x+65,113),(x+87,172),(x+87,326),(x,326)],shade)
        b.poly(d,[(x+10,173),(x+25,135),(x+55,135),(x+73,173),(x+73,321),(x+10,321)],0)
        b.line(d,[(x+15,172),(x+27,143),(x+48,143)],12)
    im.paste(im.crop((0,0,1,384)),(639,0))
    save(f'cueva{index}_fondo',im,kind='layer',pivot=(0,1),factor=.15)
    im=b.canvas(640,384);d=ImageDraw.Draw(im)
    for x in range(50,640,224):
        for y in range(45,354,30):
            stone(d,x,y,28,29,11 if index==2 else 1,12)
            stone(d,x+132,y,28,29,11 if index==2 else 1,12)
        for j in range(4):
            stone(d,x+28+j*26,45-j%2*3,26,26,11 if index==2 else 1,12)
        # Carved repeating stepped motif, subdued behind the playable floor.
        for y in (99,189,279):
            b.line(d,[(x+6,y+16),(x+6,y+4),(x+18,y+4),(x+18,y+9),(x+12,y+9),(x+12,y+16)],13 if index==2 else 2)
    im.paste(im.crop((0,0,1,384)),(639,0))
    save(f'cueva{index}_arcos',im,kind='layer',pivot=(0,1),factor=.4)

def tiles():
    for name,top in [('cueva_suelo',True),('cueva_roca',False)]:
        im=b.canvas(32,32);d=ImageDraw.Draw(im);b.rect(d,(0,0,31,31),0)
        stone(d,0,0,32,16,12,14);stone(d,-8,16,24,16,11,13);stone(d,16,16,24,16,12,13)
        if top:
            b.line(d,[(0,0),(31,0)],7);b.line(d,[(0,1),(31,1)],8)
            b.line(d,[(2,5),(6,5),(6,8),(10,8),(10,5),(14,5)],13)
        b.line(d,[(23,3),(21,7),(24,10)],0)
        save(name,im)
    im=b.canvas(16,12);d=ImageDraw.Draw(im);stone(d,0,0,16,12,12,14)
    b.line(d,[(0,0),(15,0)],7);b.line(d,[(2,4),(5,4),(5,7),(10,7),(10,4),(13,4)],8)
    save('cueva_plataforma',im)
    frames=[]
    for f in range(4):
        im=b.canvas(16,32);d=ImageDraw.Draw(im)
        b.poly(d,[(5,30),(5,16),(2,14),(13,14),(10,18),(10,30)],0)
        b.rect(d,(7,17,9,28),10);b.line(d,[(7,18),(7,27)],8)
        b.poly(d,[(3,14),(4,8),(7+f%2,3+f),(9,8),(12,5+f),(12,14),(9,17),(5,17)],22,0)
        b.poly(d,[(5,13),(7,7),(8,12),(10,10),(10,15),(7,16)],23)
        b.line(d,[(7,14),(8,11),(9,14)],6)
        d.point((4+f*2,2+f%2),fill=b.C[8]);frames.append(im)
    save('cueva_antorcha',frames,fps=7)
    im=b.canvas(12,8);d=ImageDraw.Draw(im)
    b.poly(d,[(0,1),(10,1),(11,2),(11,4),(5,4),(4,7),(1,7),(2,4),(0,4)],0)
    b.rect(d,(1,2,10,3),13);b.rect(d,(2,4,3,6),10);b.line(d,[(3,1),(9,1)],14)
    save('cueva_pistola',im)
    im=b.canvas(8,8);d=ImageDraw.Draw(im);b.rect(d,(1,3,6,4),8);b.rect(d,(4,3,7,4),6);save('cueva_bala',im)

def minion(skeleton=False):
    frames=[]
    for f in range(6):
        im=b.canvas(32,48);d=ImageDraw.Draw(im);s=round(math.sin(f*math.pi/3)*3);bob=1 if f in (1,4) else 0
        if skeleton:
            # Original bone sentinel: visible skull, rib cage, narrow pelvis, red textile.
            for x,off in [(12,s),(19,-s)]:
                b.line(d,[(x,31),(x+off,38),(x+off,45),(x+off+4,45)],0,4)
                b.line(d,[(x,31),(x+off,38),(x+off,44),(x+off+3,44)],15,2)
            b.line(d,[(15,19+bob),(15,32)],0,7);b.line(d,[(15,20+bob),(15,32)],14,2)
            for y in (22,26,30):
                b.line(d,[(10,y-1),(10,y+1),(20,y+1),(20,y-1)],0,3)
                b.line(d,[(10,y),(20,y)],15)
            b.poly(d,[(9,31),(21,31),(24,37),(9,35)],21,0);b.line(d,[(12,33),(21,34)],22)
            b.poly(d,[(9,8+bob),(22,8+bob),(24,13+bob),(21,18+bob),(20,21+bob),(12,21+bob),(11,18+bob),(8,15+bob)],0)
            b.poly(d,[(10,9+bob),(21,9+bob),(22,14+bob),(19,16+bob),(20,19+bob),(12,19+bob),(12,16+bob),(10,15+bob)],15)
            b.rect(d,(11,12+bob,14,15+bob),0);b.rect(d,(18,12+bob,21,15+bob),0)
            b.line(d,[(14,18+bob),(18,18+bob)],0)
            for x in (12,20):
                b.line(d,[(x,22),(x+(s if x==20 else -4),27),(x+5,30)],0,4)
                b.line(d,[(x,22),(x+(s if x==20 else -4),27),(x+5,30)],14,2)
            b.line(d,[(25,24),(27,38)],8,2)
        else:
            for x,off in [(8,s),(20,-s)]:stone(d,x+off,34,9,12,11,13)
            stone(d,7,18+bob,19,18,12,14)
            stone(d,7,5+bob,20,15,13,15)
            b.line(d,[(11,9+bob),(15,9+bob),(15,12+bob)],0,2)
            b.line(d,[(19,9+bob),(23,9+bob),(23,12+bob)],0,2)
            b.line(d,[(11,11+bob),(13,11+bob)],7);b.line(d,[(20,11+bob),(22,11+bob)],7)
            b.line(d,[(13,16+bob),(21,16+bob)],0)
            stone(d,1,20+s//2,7,14,11,14);stone(d,25,19-s//2,7,14,12,14)
            b.line(d,[(12,22),(20,22),(20,29),(14,29),(14,26),(17,26)],8)
        frames.append(im)
    save('cueva_esqueleto' if skeleton else 'cueva_esbirro_piedra',frames,fps=8)
    if skeleton:
        assembly=[]
        for f in range(6):
            im=b.canvas(32,48)
            # Separate skull, rib cage and legs rise from a pile, not a scaled sprite.
            for top,bottom in ((32,48),(21,32),(0,21)):
                part=frames[0].crop((0,top,32,bottom))
                progress=max(0,min(1,(f-(0 if top==32 else 1 if top==21 else 2))/3))
                y=round(top+(48-bottom)*(1-progress))
                im.alpha_composite(part,(0,y))
            assembly.append(im)
        save('cueva_esqueleto_despertar',assembly,fps=6,loop=False)

def boss(warrior=False):
    frames=[]
    for f in range(6):
        im=b.canvas(64,80);d=ImageDraw.Draw(im);bob=1 if f in (1,4) else 0;s=round(math.sin(f*math.pi/3)*3)
        if not warrior:
            for x,off in [(14,s),(39,-s)]:stone(d,x+off,62,13,16,11,14)
            stone(d,12,25+bob,41,39,12,14)
            b.poly(d,[(17,27),(13,5),(25,1),(41,3),(49,9),(47,29)],13,0)
            b.line(d,[(17,7),(25,3),(39,5),(44,9)],15)
            for x in (19,35):
                b.line(d,[(x,11),(x+9,11),(x+9,18),(x+3,18),(x+3,15),(x+6,15)],0,2)
                b.line(d,[(x+2,13),(x+7,13)],7 if f%3 else 23)
            b.line(d,[(23,22),(42,22),(40,28),(26,28),(23,22)],0,2)
            for x in (26,37):b.poly(d,[(x,23),(x+3,23),(x+1,28)],15)
            stone(d,1,31+s,12,26,11,14);stone(d,53,31-s,11,26,12,14)
            for y in (35,48):
                b.line(d,[(18,y),(25,y),(25,y+8),(39,y+8),(39,y),(47,y)],0,2)
                b.line(d,[(19,y+1),(24,y+1),(24,y+9),(40,y+9),(40,y+1),(46,y+1)],8)
            b.line(d,[(32,29),(30,35),(34,42),(31,47),(33,58)],0)
        else:
            for x,off in [(20,s),(38,-s)]:
                b.line(d,[(x,55),(x+off,64),(x+off,74),(x+off+6,74)],0,7)
                b.line(d,[(x,55),(x+off,64),(x+off,73)],14,3)
                b.line(d,[(x+off-2,75),(x+off+7,75)],10,3)
            b.poly(d,[(17,26),(43,26),(51,57),(37,65),(17,59),(12,41)],21,0)
            b.poly(d,[(18,28),(39,28),(40,55),(31,59),(18,54)],22)
            b.rect(d,(17,34,41,39),8);b.rect(d,(18,51,44,56),8)
            for x in range(20,42,7):b.poly(d,[(x,52),(x+3,55),(x+6,52)],21)
            b.poly(d,[(18,7+bob),(39,7+bob),(43,13+bob),(39,24+bob),(32,28+bob),(20,23+bob),(17,15+bob)],0)
            b.poly(d,[(20,9+bob),(38,9+bob),(40,14+bob),(36,23+bob),(30,25+bob),(21,21+bob)],14)
            b.rect(d,(21,14+bob,26,17+bob),0);b.rect(d,(32,14+bob,38,17+bob),0)
            b.line(d,[(22,16+bob),(25,16+bob)],7);b.line(d,[(33,16+bob),(36,16+bob)],7)
            b.line(d,[(26,22+bob),(33,22+bob)],0)
            b.poly(d,[(17,10),(16,5),(25,5),(28,1),(34,1),(36,5),(42,5),(41,10)],8,0)
            b.rect(d,(28,5,32,8),21);b.line(d,[(19,6),(25,6)],7)
            b.line(d,[(17,29),(10,39+s),(16,46+s)],0,7);b.line(d,[(17,29),(10,39+s),(16,46+s)],14,3)
            b.line(d,[(40,29),(49,38-s),(52,30-s)],0,7);b.line(d,[(40,29),(49,38-s),(52,30-s)],14,3)
            b.line(d,[(54,18-s),(50,55-s)],0,5);b.line(d,[(54,18-s),(50,55-s)],8,2)
            b.poly(d,[(48,15-s),(60,15-s),(63,20-s),(59,27-s),(49,26-s),(46,20-s)],8,0)
            b.line(d,[(50,17-s),(59,17-s),(61,20-s)],7)
        frames.append(im)
    save('cueva_jefe_guerrero' if warrior else 'cueva_jefe_piedra',frames,fps=7)

def props():
    for name in ('cueva_tumba','cueva_monolito_base'):
        im=b.canvas(96,64);d=ImageDraw.Draw(im)
        stone(d,2,45,92,18,11,14);stone(d,10,30,76,18,12,15)
        if name=='cueva_tumba':
            b.poly(d,[(12,27),(21,12),(79,12),(86,29)],11,0)
            b.line(d,[(22,13),(78,13),(83,26)],8,2)
            for x in range(20,79,12):b.line(d,[(x,34),(x+6,34),(x+6,41),(x,41)],8)
        else:
            stone(d,30,8,34,25,13,15);b.line(d,[(35,14),(56,14),(56,23),(39,23),(39,18),(50,18)],8)
        save(name,im)
    frames=[]
    for f in range(4):
        im=b.canvas(64,32);d=ImageDraw.Draw(im)
        b.poly(d,[(1,30),(5,20),(15,20),(22,8),(34,6),(43,14),(54,17),(63,30)],0)
        for x,y in [(4,25),(14,22),(22,15),(33,12),(40,20),(50,24),(22,25),(34,25)]:
            b.ellipse(d,(x,y,x+10,y+5),8);b.line(d,[(x+2,y+1),(x+7,y+1)],7)
        x,y=[(24,14),(37,11),(17,23),(51,24)][f]
        b.line(d,[(x-2,y),(x+2,y)],6);b.line(d,[(x,y-2),(x,y+2)],6)
        frames.append(im)
    save('cueva_oro',frames,fps=5)

def main():
    OUT.mkdir(exist_ok=True);ART.mkdir(parents=True,exist_ok=True);b.ART=ART;b.registry=[]
    backgrounds(1);backgrounds(2);tiles();minion();minion(True);boss();boss(True);props()
    for a in b.registry:
        im=Image.open(ART/a['file']).convert('RGBA')
        assert all(c in b.C or c[3]==0 for c in im.getdata()),a['name']
        assert set(im.getchannel('A').getdata()) <= {0,255}
    (OUT/'manifest.json').write_text(json.dumps(b.registry,indent=2),encoding='utf-8')
    # Independent art composition, clearly not a gameplay screenshot.
    for n in (1,2):
        bg=Image.open(ART/f'sierra_cueva{n}_fondo.png').crop((0,110,320,290))
        arches=Image.open(ART/f'sierra_cueva{n}_arcos.png').crop((0,110,320,290));bg.alpha_composite(arches)
        floor=Image.open(ART/'sierra_cueva_suelo.png')
        for x in range(0,320,32):bg.alpha_composite(floor,(x,148))
        for x in (22,248):bg.alpha_composite(Image.open(ART/'sierra_cueva_antorcha.png').crop((0,0,16,32)),(x,96))
        who='piedra' if n==1 else 'guerrero'
        bg.alpha_composite(Image.open(ART/f'sierra_cueva_jefe_{who}.png').crop((0,0,64,80)),(194,69))
        foe='esbirro_piedra' if n==1 else 'esqueleto'
        bg.alpha_composite(Image.open(ART/f'sierra_cueva_{foe}.png').crop((0,0,32,48)),(115,101))
        bg.alpha_composite(Image.open(ART/'sierra_heroe_run.png').crop((0,0,48,48)),(48,101))
        bg.resize((1280,720),Image.Resampling.NEAREST).save(OUT/f'cueva{n}_arte_x4.png')
    print(f'{len(b.registry)} original cave assets; palette24 / binary alpha verified. Existing art untouched.')

if __name__=='__main__':main()
