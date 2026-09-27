"""One reviewed asset: huaraca animation set. Native pixel source, fixed Sierra palette.
Never writes Costa. Preserve previous sheets before replacing their approved paths.
"""
import json, math, shutil
from PIL import Image, ImageDraw
import build_sierra_artpack as b
from build_sierra_coast_style import ART, OUT, setup

def main():
    setup()
    backup=OUT/'BeforeHuaracaCorrection';backup.mkdir(exist_ok=True)
    manifest_path=ART/'sierra_manifest.json'
    manifest=json.loads(manifest_path.read_text(encoding='utf-8'))
    source=Image.open(b.ROOT/'Assets/SuyuRun/Art/PixelCosta/costa_heroe.png').convert('RGBA')
    preview=[]
    for state,count in [('cargar',2),('girar',6),('lanzar',4)]:
        name='sierra_heroe_huaraca_'+state
        for suffix in ('.png','.sprite.json'):
            p=ART/(name+suffix)
            if p.exists() and not (backup/p.name).exists():shutil.copy2(p,backup/p.name)
        frames=[]
        for f in range(count):
            # Continue running legs throughout the upper-body action, same Costa body.
            body=b.nearest_palette(source.crop(((f%8)*48,48,(f%8+1)*48,96)))
            d=ImageDraw.Draw(body)
            if state=='cargar':
                elbow=(39,29);hand=(35,33);pouch=(29,35)
                other=[(14,23),(20,30),(28,34-f*2)]
            elif state=='girar':
                angle=f*math.tau/6;elbow=(39,22);hand=(35+round(math.cos(angle)*2),16+round(math.sin(angle)*2))
                pouch=(29+round(math.cos(angle)*14),9+round(math.sin(angle)*6))
                other=[(14,23),(9,29),(12,32)]
            else:
                elbow=[(38,18),(40,20),(41,25),(39,29)][f]
                hand=[(32,12),(44,18),(46,24),(43,32)][f]
                pouch=[(24,6),(45,7),(46,33),(39,42)][f]
                other=[(14,23),(9,29),(12,32)]
            b.line(d,other,0,5);b.line(d,other,9,3)
            b.line(d,[(33,23),elbow,hand],0,6);b.line(d,[(33,23),elbow,hand],9,4)
            b.line(d,[(34,23),elbow],8)
            # Two distinct braided cords and a red wool cradle, not a rigid stick.
            b.line(d,[hand,(pouch[0]-2,pouch[1])],0)
            b.line(d,[(hand[0]+2,hand[1]),(pouch[0]+2,pouch[1])],11)
            for t in (.3,.6):
                x=round(hand[0]+(pouch[0]-hand[0])*t);y=round(hand[1]+(pouch[1]-hand[1])*t)
                d.point((x,y),fill=b.C[21])
            x,y=pouch
            b.ellipse(d,(x-3,y-2,x+3,y+2),0);b.ellipse(d,(x-2,y-1,x+2,y+1),21)
            b.line(d,[(x-1,y+1),(x+1,y+1)],22)
            if state!='lanzar' or f==0:
                if state=='cargar':b.ellipse(d,(26,30-f,29,33-f),14)
                else:b.line(d,[(x-1,y-1),(x+1,y-1)],14)
            if state=='lanzar' and f>=1:b.line(d,[hand,(min(47,hand[0]+3),min(47,hand[1]+7))],21)
            frames.append(body);preview.append(body)
        b.save(name,frames,fps=10 if state=='cargar' else 12,loop=state=='girar',
               notes='Cargar piedra, dos cuerdas y cuna roja trenzada, giro, flexión/extensión. Piernas corriendo. Release runtime a0.8s, solo click válido.')
    names={a['name'] for a in b.registry}
    manifest['assets']=[a for a in manifest['assets'] if a['name'] not in names]+b.registry
    manifest_path.write_text(json.dumps(manifest,indent=2),encoding='utf-8')
    sheet=b.canvas(48*len(preview),48)
    for i,f in enumerate(preview):sheet.alpha_composite(f,(i*48,0))
    sheet.resize((sheet.width*4,192),Image.Resampling.NEAREST).save(OUT/'huaraca_secuencia_x4.png')
    preview[0].resize((192,192),Image.Resampling.NEAREST).save(OUT/'huaraca_accion.gif',save_all=True,
        append_images=[f.resize((192,192),Image.Resampling.NEAREST) for f in preview[1:]],duration=85,loop=0,disposal=2)
    assert all(c in b.C or c[3]==0 for im in preview for c in im.getdata())
    print('Huaraca: 12 frames, 48x48, palette24, original sheets preserved; Costa read-only.')

if __name__=='__main__':main()
