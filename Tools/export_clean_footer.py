"""Register generated clean interior while preserving the approved frame pixels."""
from pathlib import Path
import sys,json,shutil
from PIL import Image
root=Path(__file__).resolve().parents[1]
src=Path(sys.argv[1]);region=sys.argv[2]
folder=root/'Assets/SuyuRun/Art/RefactorRuntime/Resources/UnifiedHud'
old=Image.open(folder/f'{region}_footer.png').convert('RGBA')
im=Image.open(src).convert('RGBA');im.putalpha(im.getchannel('A').point(lambda a:255 if a>=128 else 0))
im=im.crop(im.getbbox()).resize(old.size,Image.Resampling.NEAREST)
colors=[tuple(bytes.fromhex(c[1:])) for c in json.loads((root/f'Assets/SuyuRun/Art/Pixel{region.title()}/{region}_palette.json').read_text())['colors']]
lookup={}
def quant(p):
    if not p[3]:return (0,0,0,0)
    if p[:3] not in lookup:lookup[p[:3]]=min(colors,key=lambda c:sum((p[k]-c[k])**2 for k in range(3)))
    return (*lookup[p[:3]],255)
im.putdata([quant(p) for p in im.getdata()])
# Preserve the EXACT existing border and collectible-slot geometry. Only the
# left interior comes from the removal edit, not the generator's moved frame.
area=(56,23,818,57)
old.paste(im.crop(area),area)
old.save(folder/f'{region}_footer_clean.png')
review=root/'Artifacts/VisualRefactor/AtmosphereClean';review.mkdir(parents=True,exist_ok=True)
shutil.copy2(src,review/f'{region}_footer_source.png')
shutil.copy2(folder/f'{region}_footer.png',review/f'{region}_footer_original.png')
print('footer',region,old.size,len({p[:3] for p in old.getdata() if p[3]}))
