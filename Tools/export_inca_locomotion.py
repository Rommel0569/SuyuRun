"""Mechanically register generated Inca locomotion; never synthesize poses."""
from pathlib import Path
import argparse, json
from PIL import Image

root = Path(__file__).resolve().parents[1]
p = argparse.ArgumentParser()
p.add_argument('source', type=Path)
p.add_argument('state', choices=['charge_run', 'retreat'])
a = p.parse_args()
count = 12 if a.state == 'charge_run' else 8
rows = count // 4
im = Image.open(a.source).convert('RGBA')
im.putalpha(im.getchannel('A').point(lambda x: 255 if x >= 128 else 0))
assert im.getchannel('A').getextrema() == (0, 255)
alpha = im.getchannel('A').load()
pending = {(x,y) for y in range(im.height) for x in range(im.width) if alpha[x,y]}
groups = []
while pending:
    first = pending.pop(); group = {first}; queue = [first]
    while queue:
        x,y = queue.pop()
        for dx in (-1,0,1):
            for dy in (-1,0,1):
                q = x+dx,y+dy
                if q in pending:
                    pending.remove(q); group.add(q); queue.append(q)
    groups.append(group)
def box(g):
    return min(x for x,y in g),min(y for x,y in g),max(x for x,y in g)+1,max(y for x,y in g)+1
actors = sorted(groups,key=len,reverse=True)[:count]
assert len(actors)==count and min(map(len,actors))>max(map(len,actors))*.4, 'Reject fused/fragmented actors'
actors.sort(key=lambda g:(box(g)[1]+box(g)[3])/2)
cells = []
for row in range(rows):
    for g in sorted(actors[row*4:row*4+4],key=lambda g:box(g)[0]):
        b=box(g)
        assert b[0]>0 and b[1]>0 and b[2]<im.width and b[3]<im.height, 'Source clipped at atlas edge'
        c=Image.new('RGBA',(b[2]-b[0],b[3]-b[1]))
        for x,y in g:c.putpixel((x-b[0],y-b[1]),im.getpixel((x,y)))
        cells.append(c)
colors = [tuple(bytes.fromhex(c[1:])) for c in json.loads(
    (root/'Assets/SuyuRun/Art/PixelSierra/sierra_palette.json').read_text())['colors']]
# One common anatomical scale; never resize each pose independently.
scale = min(102/max(c.height for c in cells), 180/max(c.width for c in cells))
sheet = Image.new('RGBA', (count*192, 128))
preview = Image.new('RGBA', (768, rows*128), (33, 28, 45, 255))
views, bounds = [], []
for i, c in enumerate(cells):
    c = c.resize((round(c.width*scale), round(c.height*scale)), Image.Resampling.NEAREST)
    c.putdata([(*min(colors, key=lambda q: sum((q[k]-v[k])**2 for k in range(3))), 255)
               if v[3] else (0, 0, 0, 0) for v in c.getdata()])
    f = Image.new('RGBA', (192, 128))
    f.alpha_composite(c, ((192-c.width)//2, 124-c.height))
    b = f.getbbox()
    assert b[0] >= 4 and b[1] >= 4 and b[2] <= 188 and b[3] == 124
    bounds.append(b)
    sheet.alpha_composite(f, (i*192, 0))
    preview.alpha_composite(f, ((i%4)*192, (i//4)*128))
    v = Image.new('RGBA', f.size, (33, 28, 45, 255)); v.alpha_composite(f)
    views.append(v.resize((576, 384), Image.Resampling.NEAREST))
runtime = root/'Assets/SuyuRun/Art/RefactorRuntime/Resources/RefactorBank/inca/MotionV3'
delivery = root/'Artifacts/VisualRefactor/IncaLocomotion'
runtime.mkdir(parents=True, exist_ok=True); delivery.mkdir(parents=True, exist_ok=True)
sheet.save(runtime/f'{a.state}.png')
spec = dict(frames=count, frameWidth=192, frameHeight=128, fps=24 if count==12 else 12,
            loop=True, pixelsPerUnit=64, referenceExtent=102, footY=124,
            orientation='right', hitboxesUnchanged=True, playTested=False)
(runtime/f'{a.state}.json').write_text(json.dumps(spec, indent=2))
im.save(delivery/f'{a.state}_source.png')
preview.save(delivery/f'{a.state}_preview.png')
views[0].save(delivery/f'{a.state}.gif', save_all=True, append_images=views[1:],
              duration=42 if count==12 else 83, loop=0, disposal=2)
(delivery/f'{a.state}_validation.json').write_text(json.dumps(dict(
    bounds=bounds, colors=len({v[:3] for v in sheet.getdata() if v[3]}),
    uniqueFrames=len({f.tobytes() for f in views}), commonScale=scale), indent=2))
print(a.state, count, 'distinct drawn poses exported, original assets preserved')
