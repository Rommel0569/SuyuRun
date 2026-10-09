"""Native 64px museum revision. Originals preserved; matches existing museum scale."""
import json, math
from PIL import Image, ImageDraw
import build_selva_polish as p

REFERENCES={
 'ceramica':'https://americanindian.si.edu/exhibitions/infinityofnations/amazon/181949.html',
 'textil':'https://www.metmuseum.org/art/collection/search/829475',
 'flauta':'https://americanindian.si.edu/collections-search/object/NMAI_218689',
 'sonaja':'https://hanaqpacha.patternbyetsy.com/listing/1747624101/shipibo-gourd-rattle-from-the-peruvian'}

def pattern(d,x,y,color=25):
    p.line(d,[(x,y),(x+8,y),(x+8,y+8),(x+3,y+8),(x+3,y+3),(x+5,y+3)],color,1)
    p.line(d,[(x-2,y+10),(x+10,y+10),(x+10,y-2)],color,1)

def make(name):
    im=p.canvas(64,64);d=ImageDraw.Draw(im)
    if name=='ceramica':
        shape=[(24,11),(41,11),(41,20),(49,26),(55,35),(55,44),(50,53),(40,58),(23,58),(13,52),(8,43),(9,34),(15,26),(24,20)]
        p.poly(d,shape,25);p.poly(d,[(25,12),(40,12),(39,22),(48,27),(53,35),(53,44),(48,51),(39,56),(24,56),(15,51),(10,43),(11,35),(17,28),(26,22)],9)
        p.oval(d,(12,27,52,52),24);p.oval(d,(16,26,45,48),27)
        p.poly(d,[(47,31),(51,35),(51,44),(46,51),(39,54),(40,48),(46,43)],8)
        # Painted stepped labyrinths, bounded by the cream ceramic band.
        for x,y in ((18,30),(33,30),(21,43),(36,43)):pattern(d,x,y)
        p.line(d,[(16,27),(28,24),(43,26)],25,1)
        p.line(d,[(17,52),(28,55),(42,54),(48,50)],7,1)
        p.rect(d,(25,12,39,20),24)
        for x in range(27,40,4):p.line(d,[(x,13),(x,18)],25,1)
        p.oval(d,(22,5,43,13),25);p.oval(d,(24,6,41,11),16);p.oval(d,(26,7,39,10),7)
        p.line(d,[(24,11),(32,13),(42,11)],27,1)
        p.line(d,[(12,34),(11,39),(13,43)],27,1)
    elif name=='textil':
        p.poly(d,[(7,8),(53,5),(58,52),(14,58)],25)
        p.poly(d,[(9,10),(51,8),(55,51),(15,55)],24)
        p.poly(d,[(11,11),(48,9),(52,48),(17,52)],27)
        p.line(d,[(12,12),(49,10),(53,49),(16,54),(12,12)],18,2)
        for yy in (16,29,42):
            for xx in (19,33):pattern(d,xx,yy)
        p.line(d,[(29,15),(30,48)],17,1)
        p.poly(d,[(46,8),(53,5),(58,52),(52,48)],8)
        p.line(d,[(51,13),(54,45)],9,1)
        for x in range(15,53,3):p.line(d,[(x,55),(x+1,60)],9 if x%2 else 24,1)
        p.line(d,[(9,9),(45,7)],24,1)
    elif name=='flauta':
        # Open-end bamboo flute, not an invented panpipe; original game interpretation.
        p.poly(d,[(17,59),(9,54),(44,4),(52,9)],25)
        p.poly(d,[(18,57),(11,53),(45,6),(50,10)],9)
        p.line(d,[(14,52),(46,8)],16,3);p.line(d,[(12,53),(44,9)],24,1)
        p.line(d,[(19,55),(48,13)],8,2)
        for x,y in ((20,45),(25,38),(30,31),(35,24),(40,17)):
            p.oval(d,(x-1,y-1,x+3,y+3),7);p.oval(d,(x,y,x+2,y+2),25);p.rect(d,(x,y+3,x+2,y+3),16)
        for x,y in ((15,51),(28,33),(42,13)):
            p.line(d,[(x-3,y),(x+3,y+4)],7,2);p.line(d,[(x-2,y-1),(x+4,y+3)],23,1)
        p.oval(d,(43,4,52,11),25);p.oval(d,(45,5,50,8),7)
        p.line(d,[(10,54),(17,59)],24,1)
    else:
        # Calabash shell with incised geometry, slender chonta-like handle and cord grip.
        p.poly(d,[(27,29),(35,29),(36,59),(29,61),(25,58)],25)
        p.rect(d,(28,32,33,58),7);p.line(d,[(29,35),(29,58)],9,1)
        p.oval(d,(9,4,51,39),25);p.oval(d,(11,5,49,37),7)
        p.oval(d,(12,6,45,34),9);p.oval(d,(15,7,38,26),16)
        for xx,yy in ((18,13),(31,14),(20,26),(33,27)):pattern(d,xx,yy,24)
        p.line(d,[(14,12),(12,20),(14,28)],24,1)
        p.line(d,[(41,10),(46,17),(47,25),(42,32)],8,2)
        for y in range(42,57,2):p.line(d,[(27,y),(34,y)],11 if y%4 else 13,1)
        p.line(d,[(25,39),(35,40)],9,2)
    return im

def main():
    p.b.C=p.C;manifest=p.ART/'selva_manifest.json';data=json.loads(manifest.read_text());p.registry[:]=data['assets']
    preview=Image.new('RGBA',(320,80),(33,28,45,255))
    for i,name in enumerate(('ceramica','flauta','textil','sonaja')):
        im=make(name);p.save('museo_'+name+'_v3',im,group='Museum',reference=REFERENCES[name],notes='Native interpretation; not a facsimile or universal ceremonial attribution.')
        preview.alpha_composite(im,(i*80+8,8))
    preview.resize((1280,320),Image.Resampling.NEAREST).save(p.OUT/'museo_selva_v3_x4.png')
    base=Image.open(p.ART/'selva_heroe_idle.png').convert('RGBA').crop((0,0,48,48));frames=[]
    for i in range(8):
        im=p.canvas(48,48);bob=(0,1,2,1,0,1,2,1)[i];im.alpha_composite(base.crop((0,0,48,33)),(0,bob));d=ImageDraw.Draw(im)
        side=1 if i<4 else -1
        p.line(d,[(23,29+bob),(18,35),(16+side*4,45)],25,6);p.line(d,[(23,29+bob),(18,35),(16+side*4,45)],9,3)
        p.line(d,[(28,29+bob),(30,36),(34-side*4,45)],25,6);p.line(d,[(28,29+bob),(30,36),(34-side*4,45)],9,3)
        hand=(9,14 if i%4<2 else 23);p.line(d,[(19,24+bob),(13,26),hand],25,6);p.line(d,[(19,24+bob),(13,26),hand],9,3)
        hand=(39,23 if i%4<2 else 14);p.line(d,[(29,24+bob),(35,26),hand],25,6);p.line(d,[(29,24+bob),(35,26),hand],9,3)
        frames.append(im)
    p.save('heroe_baile',frames,fps=8,loop=True,group='Hero')
    data['assets']=p.registry;manifest.write_text(json.dumps(data,indent=2));p.validate()
    frames[0].resize((192,192),Image.Resampling.NEAREST).save(p.OUT/'baile_selva_preview.png')
if __name__=='__main__':main()
