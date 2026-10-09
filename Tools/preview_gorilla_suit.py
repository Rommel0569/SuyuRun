"""Normalize the image-generated costume proposal; never overwrite gameplay art."""
from pathlib import Path
from PIL import Image
import json, sys

root = Path(__file__).resolve().parents[1]
folder = root / 'Artifacts/Tayta'
revision = sys.argv[1] if len(sys.argv) > 1 else ''
suffix = '_v' + revision if revision else ''
source = Image.open(folder / ('traje_gorila_propuesta_fuente' + suffix + '.png')).convert('RGBA')
source.putalpha(source.getchannel('A').point(lambda a: 255 if a >= 128 else 0))
source = source.crop(source.getbbox())
scale = min(44 / source.height, 44 / source.width)
source = source.resize((round(source.width * scale), round(source.height * scale)), Image.Resampling.NEAREST)
palette = json.loads((root / 'Assets/SuyuRun/Art/PixelSelva/ProductionV2/Resources/SelvaV2/selva_palette.json').read_text())['colors']
colors = [tuple(bytes.fromhex(c.lstrip('#'))) for c in palette]
source.putdata([(*min(colors, key=lambda c: sum((c[i]-p[i])**2 for i in range(3))), p[3]) if p[3] else (0,0,0,0) for p in source.getdata()])
frame = Image.new('RGBA', (48,48))
frame.paste(source, ((48-source.width)//2, 47-source.height))
frame.save(folder / ('selva_traje_gorila_propuesta' + suffix + '.png'))
frame.resize((384,384), Image.Resampling.NEAREST).save(folder / ('selva_traje_gorila_propuesta' + suffix + '_x8.png'))
print('Proposal only: 48x48, approved Selva palette, binary alpha. Not integrated.')
