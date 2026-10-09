"""Original native-pixel Selva combat expansion. Retains all older sprites."""
import json, math
from PIL import Image, ImageDraw
import build_selva_polish as p

def main():
    p.b.C=p.C
    manifest=p.ART/'selva_manifest.json'
    data=json.loads(manifest.read_text(encoding='utf-8'))
    p.registry[:]=data['assets']
    def sheet(name,frames,size,draw):
        images=[]
        for i in range(frames):
            im=p.canvas(*size);d=ImageDraw.Draw(im);draw(d,i,im);images.append(im)
        p.save(name,images,fps=12,loop=name.endswith(('idle','andar','carga','caminar')),group='Combat')
    for action in ('idle','andar','despertar','anticipar','golpe','carga','hit','derrota'):
        def boss(d,i,im,a=action):
            bob=round(math.sin(i*math.pi/3)*2);y=10+bob
            if a=='derrota':y+=i*3
            # Asymmetric feet, root-like left foot; leaf skirt and original face.
            p.poly(d,[(31,61+y),(43,61+y),(42,76+y),(30,82+y),(22,80+y)],25)
            p.poly(d,[(50,61+y),(60,60+y),(65,76+y),(78,78+y),(76,84+y),(54,82+y)],25)
            p.rect(d,(34,64+y,40,76+y),9);p.rect(d,(54,64+y,61,78+y),9)
            p.line(d,[(29,77+y),(24,81+y),(19,79+y)],7,2)
            p.oval(d,(25,27+y,66,64+y),25);p.oval(d,(27,28+y,64,62+y),8)
            p.oval(d,(32,30+y,60,56+y),9)
            p.poly(d,[(33,32+y),(43,31+y),(45,39+y),(37,44+y),(32,40+y)],16)
            p.poly(d,[(47,34+y),(56,32+y),(60,42+y),(53,46+y),(47,43+y)],9)
            p.line(d,[(36,46+y),(49,48+y),(57,45+y)],7,1)
            p.line(d,[(41,37+y),(44,40+y),(49,39+y)],8,1)
            for k in range(8):p.poly(d,[(28+k*5,53+y),(35+k*4,51+y),(33+k*4,69+y)],3 if k%2 else 2)
            p.line(d,[(29,54+y),(63,53+y)],7,3)
            for k in range(6):p.line(d,[(32+k*5,57+y),(30+k*5,64+y)],4,1)
            p.oval(d,(29,0+y,62,32+y),25);p.oval(d,(31,1+y,60,30+y),9)
            p.oval(d,(36,3+y,56,12+y),16);p.poly(d,[(30,9+y),(23,5+y),(25,20+y),(32,23+y)],8)
            p.poly(d,[(61,9+y),(70,4+y),(66,21+y),(60,23+y)],8)
            p.line(d,[(34,14+y),(43,16+y)],25,2);p.line(d,[(49,16+y),(58,13+y)],25,2)
            p.rect(d,(39,17+y,42,19+y),5);p.rect(d,(50,17+y,53,19+y),5)
            p.rect(d,(41,18+y,42,19+y),25);p.rect(d,(50,18+y,51,19+y),25)
            p.line(d,[(43,18+y),(42,22+y),(47,23+y)],7,1)
            p.line(d,[(36,21+y),(39,22+y)],8,1);p.line(d,[(53,20+y),(57,19+y)],8,1)
            p.poly(d,[(41,23+y),(56,22+y),(50,28+y),(42,27+y)],25)
            p.line(d,[(44,24+y),(53,24+y)],24,1)
            p.line(d,[(43,26+y),(49,27+y),(53,25+y)],7,1)
            casting=a in ('anticipar','golpe','despertar')
            hand=(77,18+y-round(math.sin(i*.7)*4)) if casting else (73,46+y)
            p.line(d,[(59,32+y),(70,31+y),hand],25,10);p.line(d,[(59,32+y),(70,31+y),hand],9,7)
            p.line(d,[(29,32+y),(18,41+y),(17,53+y)],25,9);p.line(d,[(29,32+y),(18,41+y),(17,53+y)],8,6)
            p.line(d,[(29,32+y),(21,39+y)],9,2)
            p.line(d,[(61,32+y),(69,32+y)],16,2)
            p.rect(d,(14,50+y,19,55+y),9)
            for finger in range(3):p.line(d,[(14+finger*2,54+y),(14+finger*2,58+y)],7,1)
            if casting:
                p.oval(d,(hand[0]-7,hand[1]-7,hand[0]+7,hand[1]+7),2)
                p.oval(d,(hand[0]-4,hand[1]-4,hand[0]+4,hand[1]+4),5)
                p.rect(d,(hand[0]-1,hand[1]-1,hand[0]+1,hand[1]+1),27)
            if a=='hit':p.line(d,[(25,29+y),(66,29+y)],24,1)
        sheet('boss_chullachaqui_'+action,6,(96,96),boss)
    # Hero art stays 48x48. Retain head, armor and native palette from approved design.
    hero=Image.open(p.ART/'selva_heroe_idle.png').convert('RGBA').crop((0,0,48,48))
    for action in ('punch','kick','uppercut','guard'):
        def fight(d,i,im,a=action):
            im.alpha_composite(hero)
            extension=(0,5,12,18,8,0)[i]
            if a=='kick':
                p.line(d,[(24,31),(31+extension//2,32),(30+extension,26)],25,8)
                p.line(d,[(24,31),(31+extension//2,32),(30+extension,26)],9,5)
            else:
                hand=(min(46,29+extension),10 if a=='uppercut' else 18 if a=='guard' else 24)
                p.line(d,[(24,24),(28,28),hand],25,8);p.line(d,[(24,24),(28,28),hand],9,5)
                p.rect(d,(hand[0]-3,hand[1]-3,hand[0]+2,hand[1]+2),16)
        sheet('heroe_'+action,6,(48,48),fight)
    for name in ('raiz_aviso','raiz_erupcion','raiz_amarre','maldicion','eco','impacto_puno'):
        def fx(d,i,im,n=name):
            if n.startswith('raiz'):
                p.oval(d,(4,51,59,61),4);p.oval(d,(9,53,54,58),5)
                if n!='raiz_aviso':
                    for k in range(4):
                        height=16+((i+k)%4)*7
                        p.line(d,[(10+k*12,55),(13+k*12,55-height),(8+k*12,49-height)],25,5)
                        p.line(d,[(10+k*12,55),(13+k*12,55-height),(8+k*12,49-height)],3,3)
            else:
                for k in range(7):
                    a=k*math.pi*2/7+i*.6;r=8+i*3;x=32+round(math.cos(a)*r);y=32+round(math.sin(a)*r)
                    p.oval(d,(x-3,y-3,x+3,y+3),5 if k%2 else 13)
                    p.rect(d,(x,y,x+1,y+1),27)
        sheet('fx_'+name,6,(64,64),fx)
    # A spectral root sentinel: not a historic person resurrected as an enemy.
    for action in ('idle','caminar','hit','derrota'):
        def echo(d,i,im):
            y=round(math.sin(i)*2)
            p.oval(d,(9,3+y,23,17+y),25);p.oval(d,(10,4+y,22,16+y),11)
            p.rect(d,(12,9+y,14,11+y),27);p.rect(d,(19,9+y,21,11+y),27)
            p.poly(d,[(10,17+y),(23,17+y),(27,30),(18,28),(8,31)],10)
            p.line(d,[(11,19+y),(4,23+y),(3,16+y)],12,2)
            p.line(d,[(23,19+y),(29,23+y),(30,16+y)],12,2)
        sheet('esbirro_eco_'+action,6,(32,32),echo)
    # Recognizable echoes of the two Sierra bosses; source originals remain untouched.
    source=p.ROOT/'Assets/SuyuRun/Art/PixelSierra/ProductionV3/Resources/SierraV3'
    for foe,original in (('ecopiedra','sierra_cueva_jefe_piedra.png'),('ecoinca','sierra_cueva_jefe_guerrero.png')):
        src=Image.open(source/original).convert('RGBA');cells=[]
        for frame in range(6):
            cell=src.crop((frame*64,0,(frame+1)*64,80)).resize((32,40),Image.Resampling.NEAREST)
            colors=[]
            for r,g,b,a in cell.getdata():
                value=(r+g+b)//3
                colors.append((0,0,0,0) if a==0 else p.C[25 if value<45 else 10 if value<100 else 11 if value<155 else 12 if value<210 else 13])
            cell.putdata(colors);cells.append(cell)
        for action in ('idle','caminar','hit','derrota'):p.save('esbirro_'+foe+'_'+action,cells,fps=10,loop=action in ('idle','caminar'),group='Combat')
    data['assets']=p.registry;data['encountersRevision']=3
    manifest.write_text(json.dumps(data,indent=2),encoding='utf-8')
    p.midi_course()
    p.validate()
    preview=p.canvas(320,180)
    for name in ('ruinas_capa1_cielo','ruinas_capa2_lejania','ruinas_capa3_estructura','ruinas_capa5_cercania'):
        src=Image.open(p.ART/('selva_'+name+'.png')).convert('RGBA');preview.alpha_composite(src.crop((0,0,320,180)))
    preview.alpha_composite(Image.open(p.ART/'selva_boss_chullachaqui_anticipar.png').crop((0,0,96,96)),(190,58))
    preview.alpha_composite(Image.open(p.ART/'selva_heroe_punch.png').crop((96,0,144,48)),(110,100))
    preview.resize((1280,720),Image.Resampling.NEAREST).save(p.OUT/'chullachaqui_encuentro_x4.png')
    print('Combat sheets:',30,'; total assets:',len(p.registry))

if __name__=='__main__':main()
