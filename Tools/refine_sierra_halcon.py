"""One asset refinement: side-view falcon, six native pixel frames, Sierra palette.
Read ART_BIBLE before editing. User authorizes native Pillow pixel construction.
"""
import json, shutil
from PIL import ImageDraw, Image
import build_sierra_artpack as b
from build_sierra_coast_style import ART, OUT, setup

def main():
    setup();name='sierra_halcon_vuelo'
    backup=OUT/'BeforeFalconRefinement';backup.mkdir(exist_ok=True)
    for ext in ('.png','.sprite.json'):
        p=ART/(name+ext)
        if not (backup/p.name).exists():shutil.copy2(p,backup/p.name)
    frames=[]
    for f,lift in enumerate((-12,-8,0,8,3,-7)):
        im=b.canvas(32,32);d=ImageDraw.Draw(im)
        # Far wing, forked tail, side-on chest and head: asymmetric silhouette.
        b.poly(d,[(15,17),(11,6+max(0,lift)),(19,10+max(0,lift)),(23,18)],1,0)
        b.poly(d,[(12,18),(2,21),(1,25),(7,23),(4,27),(13,23)],11,0)
        b.line(d,[(3,23),(9,21),(12,21)],14)
        b.ellipse(d,(9,13,24,23),0);b.ellipse(d,(10,14,23,22),11)
        b.poly(d,[(19,16),(24,16),(23,21),(19,23),(14,21)],9)
        b.line(d,[(17,19),(20,20)],8);b.line(d,[(16,21),(19,22)],8)
        b.poly(d,[(12,17),(4,16+lift),(1,18+lift),(5,20+lift),(3,22+lift),(8,23+lift),(11,22),(19,19)],0)
        b.poly(d,[(12,17),(5,17+lift),(6,20+lift),(11,21),(17,19)],10)
        for i in range(3):b.line(d,[(5+i*3,18+lift+i),(10+i*2,20)],9 if i%2==0 else 13)
        b.ellipse(d,(21,12,28,19),0);b.ellipse(d,(22,13,27,18),10)
        b.line(d,[(23,14),(27,14)],15);d.point((26,15),fill=b.C[0])
        b.poly(d,[(28,15),(31,17),(29,18),(28,20),(27,17)],8,0)
        b.line(d,[(17,23),(18,25),(21,25)],8);d.point((21,26),fill=b.C[0])
        frames.append(im)
    b.save(name,frames,fps=10,loop=True,notes='Halcón original lateral: ala cercana/lejana, primarias, cola bifurcada, pico curvo, ojo y garras; paleta24, sin AA.')
    manifest_path=ART/'sierra_manifest.json';manifest=json.loads(manifest_path.read_text())
    manifest['assets']=[a for a in manifest['assets'] if a['name']!=name]+b.registry
    manifest_path.write_text(json.dumps(manifest,indent=2),encoding='utf-8')
    preview=[f.resize((192,192),Image.Resampling.NEAREST) for f in frames]
    preview[0].save(OUT/'halcon_revision.gif',save_all=True,append_images=preview[1:],duration=100,loop=0,disposal=2)
    assert all(c in b.C or c[3]==0 for im in frames for c in im.getdata())
    print('One asset: 6x32x32, palette24, binary alpha; previous asset preserved.')

if __name__=='__main__':main()
