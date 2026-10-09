"""Import imagegen museum proposals non-destructively; never changes runtime references."""
from pathlib import Path
from PIL import Image, ImageOps
import json, shutil, sys

root = Path(__file__).resolve().parents[1]
out = root / 'Artifacts/MuseumSelvaV4'
out.mkdir(parents=True, exist_ok=True)
palette = json.loads((root / 'Assets/SuyuRun/Art/PixelSelva/ProductionV2/Resources/SelvaV2/selva_palette.json').read_text(encoding='utf-8'))['colors']
rgb = [tuple(bytes.fromhex(c[1:])) for c in palette]
ids = ['ceramica', 'flauta', 'textil', 'sonaja']
sheet = Image.new('RGBA', (320, 80), (33, 28, 45, 255))
report = {}
for i, (name, source) in enumerate(zip(ids, sys.argv[1:])):
    shutil.copy2(source, out / f'selva_museo_{name}_v4_source.png')
    tile = Image.open(source).convert('RGBA')
    bounds = tile.getchannel('A').point(lambda a: 255 if a >= 128 else 0).getbbox()
    assert bounds, name
    tile = ImageOps.contain(tile.crop(bounds), (60, 60), Image.Resampling.NEAREST)
    frame = Image.new('RGBA', (64, 64))
    frame.paste(tile, ((64-tile.width)//2, (64-tile.height)//2))
    pixels = []
    for r, g, b, a in frame.getdata():
        if a < 128:
            pixels.append((0, 0, 0, 0))
        else:
            c = min(rgb, key=lambda c: (c[0]-r)**2+(c[1]-g)**2+(c[2]-b)**2)
            pixels.append((*c, 255))
    frame.putdata(pixels)
    frame.save(out / f'selva_museo_{name}_v4.png')
    sheet.alpha_composite(frame, (i*80+8, 8))
    used = {p[:3] for p in pixels if p[3]}
    assert used.issubset(set(rgb)) and len(used) <= 28
    report[name] = dict(size=[64, 64], colors=len(used), binary_alpha=True, approval='pending')
assert len(sys.argv[1:]) == 4
sheet.resize((1280, 320), Image.Resampling.NEAREST).save(out / 'museo_selva_v4_x4.png')
(out / 'validation.json').write_text(json.dumps(report, indent=2), encoding='utf-8')
print(json.dumps(report, indent=2))
