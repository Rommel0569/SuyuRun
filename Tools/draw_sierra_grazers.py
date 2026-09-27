"""One native-pixel atlas: llama/alpaca grazing, existing Sierra V3 palette.
Extends the project's editable Pillow asset pipeline; keeps previous llama PNGs.
"""
from PIL import Image, ImageDraw
import json
import build_sierra_artpack as b

root=b.ROOT
out=root/'Assets/SuyuRun/Art/PixelSierra/ProductionV3/Resources/SierraV3'
review=root/'Artifacts/SierraGrazers';review.mkdir(parents=True,exist_ok=True)
poses=[0,0,.2,.5,.8,1,1,1,.8,.5,.2,0]
atlas=Image.new('RGBA',(32*12,32*2));frames=[]
for species in range(2):
    row=[]
    for f,bend in enumerate(poses):
        im=b.canvas(48,48);d=ImageDraw.Draw(im)
        base,light,shadow=(15,6,14) if species==0 else (9,15,10)
        # Feet never change position: the neck lowers, not the entire animal.
        for x in (12,17,28,32):
            b.line(d,[(x,29),(x,42),(x+2,42)],0,3)
            b.line(d,[(x,30),(x,40)],shadow,1)
        b.poly(d,[(5,25),(7,19),(12,17),(18,18),(24,17),(31,20),(34,27),(30,34),(11,34),(6,30)],base,0)
        b.poly(d,[(8,22),(13,19),(22,19),(29,22),(27,26),(11,27)],light)
        b.line(d,[(9,31),(17,32),(27,31)],shadow,2)
        b.line(d,[(7,25),(3,20),(3,17)],0,3);b.line(d,[(7,24),(3,19)],base)
        hx=34+round(6*bend);hy=(10 if species==0 else 15)+round((27 if species==0 else 22)*bend)
        b.poly(d,[(27,27),(28,21),(hx-3,hy-1),(hx+2,hy),(hx+3,hy+6),(32,30)],base,0)
        b.line(d,[(29,23),(hx-1,hy+2)],light,2)
        ears=7 if species==0 else 4
        b.poly(d,[(hx-3,hy+1),(hx-5,hy-ears),(hx-2,hy-ears-1),(hx,hy)],base,0)
        b.poly(d,[(hx+1,hy),(hx+2,hy-ears),(hx+4,hy-ears+1),(hx+3,hy+2)],base,0)
        b.poly(d,[(hx-4,hy),(hx+2,hy-1),(hx+5,hy+2),(hx+6,hy+5),(hx+3,hy+7),(hx-3,hy+6)],base,0)
        b.line(d,[(hx-2,hy+1),(hx+2,hy+1)],light)
        d.point((hx+1,hy+2),fill=b.C[0]);b.line(d,[(hx+3,hy+5),(hx+5,hy+5)],shadow)
        if bend==1:
            b.line(d,[(hx+4,hy+6),(hx+5+(f%2),hy+7)],19)
        if species:
            for x,y in [(10,20),(16,19),(23,20),(9,26),(17,27),(24,26)]:
                b.line(d,[(x,y),(x+2,y-1),(x+4,y)],light)
        tile=im.resize((32,32),Image.Resampling.NEAREST)
        atlas.alpha_composite(tile,(f*32,species*32));row.append(tile)
    frames.append(row)
atlas.save(out/'sierra_camelidos_pastando.png')
assert all(p[3] in (0,255) and (not p[3] or p in b.C) for p in atlas.getdata())
preview=[]
for f in range(12):
    im=Image.new('RGBA',(128,48),b.C[16]);d=ImageDraw.Draw(im)
    d.rectangle((0,37,127,47),fill=b.C[17])
    for j in range(3):im.alpha_composite(frames[j%2][(f+j*3)%12],(10+j*38,10))
    preview.append(im.resize((768,288),Image.Resampling.NEAREST).convert('RGB'))
preview[0].save(review/'pastando.gif',save_all=True,append_images=preview[1:],duration=230,loop=0)
(review/'validation.json').write_text(json.dumps({'atlas':[384,64],'frame':[32,32],'frames_per_species':12,'palette':b.HEX,'binary_alpha':True,'previous_llamas_preserved':True},indent=2))
print('24 grazing frames, 32px, Sierra V3 palette and binary alpha verified.')
