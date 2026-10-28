"""Mechanical contact sheets from final exported resources, not repainted assets."""
from pathlib import Path
from PIL import Image,ImageDraw
import json,argparse
p=argparse.ArgumentParser();p.add_argument('--revision',default='FinalMovieV3');a=p.parse_args()
root=Path(__file__).resolve().parents[1];bank=root/'Assets/SuyuRun/Art/RefactorRuntime/Resources'/a.revision;out=root/'Artifacts'/a.revision/'Review';out.mkdir(parents=True,exist_ok=True)
for name in ('costa','sierra','selva'):
    scene=Image.new('RGBA',(1280,720))
    for layer in ('sky','mid','ground'):
        im=Image.open(bank/(name+'_'+layer+'.png')).convert('RGBA')
        # Match runtime's first 320 logical pixels of the 640px plane.
        im=im.resize((2560,720),Image.Resampling.NEAREST).crop((0,0,1280,720))
        if layer=='ground':
            s=json.loads((bank/(name+'_ground.json')).read_text());shift=round(146*4-s['surfaceY']*720)
            shifted=Image.new('RGBA',im.size);shifted.paste(im,(0,shift));im=shifted
        scene.alpha_composite(im)
    scene.save(out/(name+'_composition.png'))
for path in bank.glob('*.json'):
    s=json.loads(path.read_text());n=s.get('frames',1)
    if n==1:continue
    im=Image.open(path.with_suffix('.png')).convert('RGBA');board=Image.new('RGBA',(1024,((n+3)//4)*280),(25,29,38,255));d=ImageDraw.Draw(board)
    for i in range(n):
        cell=im.crop((i*256,0,(i+1)*256,256));x=i%4*256;y=i//4*280
        board.alpha_composite(cell,(x,y+20));d.text((x+4,y+4),str(i),fill='white')
    board.convert('RGB').save(out/(path.stem+'_frames.png'))
print('Review boards generated from actual exported layers and cells.')
