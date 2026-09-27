"""Single native pixel asset: rolling boulder. Preserve the prior approved resources."""
import json, math, shutil
from PIL import Image, ImageDraw
import build_sierra_artpack as b
from build_sierra_coast_style import ART, OUT, setup

def main():
    setup()
    name='sierra_roca_rodante'
    backup=OUT/'BeforeRollingRock';backup.mkdir(exist_ok=True)
    for ext in ('.png','.sprite.json'):
        p=ART/(name+ext)
        if not (backup/p.name).exists():shutil.copy2(p,backup/p.name)
    frames=[]
    for f in range(8):
        im=b.canvas(32,32);d=ImageDraw.Draw(im)
        angle=-f*math.tau/8  # Counterclockwise: travel toward the runner (left).
        def turn(p):
            x,y=p[0]-16,p[1]-16
            return (round(16+x*math.cos(angle)-y*math.sin(angle)),round(16+x*math.sin(angle)+y*math.cos(angle)))
        b.ellipse(d,(1,1,30,30),0);b.ellipse(d,(2,2,29,29),13)
        b.poly(d,[(5,24),(12,28),(23,27),(28,20),(27,25),(22,29),(9,29)],12)
        b.line(d,[(5,12),(7,7),(12,4),(19,3)],15)
        for path in ([(10,8),(13,12),(11,16),(15,19)],[(23,10),(21,15),(24,19)],[(9,23),(13,23),(16,26)]):
            b.line(d,[turn(p) for p in path],11)
        for point in ((9,12),(21,22),(19,7)):
            d.point(turn(point),fill=b.C[14])
        assert all(c in b.C or c[3]==0 for c in im.getdata())
        frames.append(im)
    b.save(name,frames,fps=8,loop=True,notes='Rodadura izquierda: grietas giran ocho fases completas; luz fija arriba, contorno1px, paleta Sierra existente.')
    p=ART/'sierra_manifest.json';manifest=json.loads(p.read_text())
    manifest['assets']=[a for a in manifest['assets'] if a['name']!=name]+b.registry
    p.write_text(json.dumps(manifest,indent=2),encoding='utf-8')
    preview=[f.resize((128,128),Image.Resampling.NEAREST) for f in frames]
    preview[0].save(OUT/'roca_rodante_revision.gif',save_all=True,append_images=preview[1:],duration=125,loop=0,disposal=2)
    print('One asset: rolling rock, 8 frames, 32x32. Previous preserved.')

if __name__=='__main__':main()
