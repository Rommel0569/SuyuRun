"""Native pixel animation extension; preserves the earlier cave sheets."""
import json, math
from PIL import Image, ImageDraw
import build_sierra_artpack as b
from build_sierra_caves import ART, OUT, stone, save

def limb(d, points, width, color):
    b.line(d,points,0,width+2);b.line(d,points,color,width)
    b.line(d,[(x-1,y-1) for x,y in points],14 if color==12 else 15,2)

def pose(warrior, move, f):
    im=b.canvas(96,96);d=ImageDraw.Draw(im)
    pulse=math.sin(f*math.pi/4); drive=[0,.15,.6,1,1,.6,.2,0][f]
    shift=round(drive*9) if move in ('jab','carga') else 0
    cx=43+shift;bob=round(abs(pulse)*2)
    if move=='slam':bob=round(drive*9)
    cy=47+bob
    # Weight transfer, bent knees and separate planted feet.
    for side in (-1,1):
        hip=(cx+side*10,68+bob);knee=(cx+side*(14+round(drive*3)),79)
        foot=(cx+side*17,91)
        limb(d,[hip,knee,foot],7 if warrior else 10,14 if warrior else 12)
        stone(d,foot[0]-6,89,15,6,10 if warrior else 11,8 if warrior else 14)
    if warrior:
        # Red mantle, gold woven bands, skeletal face and original royal silhouette.
        b.poly(d,[(cx-17,31),(cx+13,32),(cx+20,68),(cx-24-round(drive*7),78),(cx-20,50)],21,0)
        b.line(d,[(cx-16,37),(cx-19,66),(cx-24,73)],22,2)
        b.poly(d,[(cx-14,35),(cx+14,35),(cx+18,68),(cx,75),(cx-16,68)],22,0)
        for y in (43,63):
            b.rect(d,(cx-13,y+bob,cx+14,y+4+bob),8)
            for x in range(cx-10,cx+12,7):b.poly(d,[(x,y+1+bob),(x+3,y+3+bob),(x+5,y+1+bob)],21)
        stone(d,cx-12,13+bob,25,22,14,15)
        for x in (cx-8,cx+4):
            b.rect(d,(x,21+bob,x+5,25+bob),0);b.line(d,[(x+1,23+bob),(x+4,23+bob)],7)
        b.line(d,[(cx-5,30+bob),(cx+6,30+bob)],0)
        for x in range(cx-4,cx+6,3):b.line(d,[(x,29+bob),(x,32+bob)],0)
        b.poly(d,[(cx-14,17+bob),(cx-15,9+bob),(cx-5,9+bob),(cx-2,4+bob),(cx+5,4+bob),(cx+8,9+bob),(cx+15,9+bob),(cx+14,17+bob)],8,0)
        b.line(d,[(cx-11,11+bob),(cx-5,11+bob)],7);b.rect(d,(cx,9+bob,cx+4,15+bob),21)
    else:
        stone(d,cx-18,34+bob,37,37,12,14)
        stone(d,cx-14,10+bob,29,26,13,15)
        for x in (cx-10,cx+3):
            b.rect(d,(x,18+bob,x+7,23+bob),0);b.line(d,[(x+1,20+bob),(x+5,20+bob)],7)
        b.poly(d,[(cx-9,28+bob),(cx+10,28+bob),(cx+6,34+bob),(cx-5,34+bob)],0)
        for x in (cx-6,cx+4):b.poly(d,[(x,29+bob),(x+3,29+bob),(x+1,33+bob)],15)
        for y in (41,56):b.line(d,[(cx-12,y+bob),(cx-5,y+bob),(cx-5,y+7+bob),(cx+6,y+7+bob),(cx+6,y+bob),(cx+12,y+bob)],8)
        b.line(d,[(cx,35+bob),(cx-2,43+bob),(cx+2,50+bob),(cx-1,56+bob)],0)
    # Articulated shoulder -> elbow -> hand, not a translated rigid sprite.
    left=(cx-22,cy+8);right=(cx+22,cy-8)
    if move=='jab':right=(cx+22+round(drive*17),cy-5)
    if move=='upper':right=(cx+23-round(drive*4),cy-5-round(drive*34))
    if move=='slam':
        y=cy-26 if f<3 else cy+round(drive*29)
        left=(cx-22,y);right=(cx+23,y)
    if move=='maza':right=(cx+round(math.cos((f/7)*math.pi)*26),cy-15+round(math.sin(f/7*math.pi)*22))
    if move=='sol':left=(cx-25,cy-20);right=(cx+26,cy-24)
    if move=='carga':right=(cx+30,cy);left=(cx-24,cy+14)
    for side,hand in ((-1,left),(1,right)):
        shoulder=(cx+side*15,cy-8)
        elbow=(round((shoulder[0]+hand[0])/2)+side*3,round((shoulder[1]+hand[1])/2)+7)
        limb(d,[shoulder,elbow,hand],5 if warrior else 9,14 if warrior else 12)
        stone(d,hand[0]-5,hand[1]-5,11,11,8 if warrior else 13,7 if warrior else 15)
    if warrior:
        hx,hy=right;end=(hx+7,hy-18) if move!='carga' else (hx+10,hy-8)
        b.line(d,[(hx-3,hy+8),end],0,5);b.line(d,[(hx-3,hy+8),end],8,2)
        ex,ey=end;b.poly(d,[(ex-7,ey-4),(ex-3,ey-8),(ex+4,ey-8),(ex+8,ey-3),(ex+5,ey+4),(ex-4,ey+4)],8,0)
        b.line(d,[(ex-3,ey-6),(ex+3,ey-6),(ex+5,ey-3)],7)
        if move=='sol':
            x,y=left;b.ellipse(d,(x-7,y-15,x+7,y-1),8);b.ellipse(d,(x-4,y-12,x+4,y-4),6)
    # Short hard-edged streaks on the active frames, all palette pixels.
    if f in (3,4) and move not in ('guardia','sol'):
        for k in range(3):
            y=45+k*5;b.line(d,[(8+k*2,y),(20+k*2,y)],8 if warrior else 13)
    return im

def main():
    b.ART=ART;b.registry=[];OUT.mkdir(exist_ok=True)
    for warrior,moves in ((False,('guardia','jab','upper','slam')),(True,('guardia','maza','sol','carga'))):
        previews=[]
        for move in moves:
            frames=[pose(warrior,move,f) for f in range(8)]
            save('cueva_'+('inca_' if warrior else 'box_')+move,frames,fps=12,loop=move=='guardia')
            for frame in frames:
                bg=Image.new('RGBA',(96,96),b.C[0]);bg.alpha_composite(frame)
                previews.append(bg.convert('RGB').resize((384,384),Image.Resampling.NEAREST))
        previews[0].save(OUT/('inca_combos.gif' if warrior else 'piedra_combos.gif'),save_all=True,append_images=previews[1:],duration=100,loop=0)
    hits=[];solar=[]
    for f in range(4):
        im=b.canvas(32,32);d=ImageDraw.Draw(im);r=5+f*3
        for a in range(8):
            angle=a*math.pi/4;x=16+round(math.cos(angle)*r);y=16+round(math.sin(angle)*r)
            b.line(d,[(16+round(math.cos(angle)*(r-4)),16+round(math.sin(angle)*(r-4))),(x,y)],7 if f<2 else 8,2)
        hits.append(im)
        im=b.canvas(12,12);d=ImageDraw.Draw(im);b.ellipse(d,(1,1,10,10),8);b.ellipse(d,(3,3,8,8),7)
        b.line(d,[(4,5),(7,5)],6);d.point((2+f*2,2),b.C[6]);solar.append(im)
    save('cueva_golpe',hits,fps=12,loop=False);save('cueva_proyectil_sol',solar,fps=10)
    path=OUT/'manifest.json';previous=json.loads(path.read_text(encoding='utf-8'))
    names={a['name'] for a in b.registry};combined=[a for a in previous if a['name'] not in names]+b.registry
    for a in b.registry:
        with Image.open(ART/a['file']) as im:assert all(c in b.C or c[3]==0 for c in im.convert('RGBA').getdata())
    path.write_text(json.dumps(combined,indent=2),encoding='utf-8')
    print(f'{len(b.registry)} new combat sheets; {len(combined)} cave assets total; palette verified.')

if __name__=='__main__':main()
