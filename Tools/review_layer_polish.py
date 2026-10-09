"""Read-only contact sheets of actual layer resources for visual evaluation."""
from pathlib import Path
import json,argparse
from PIL import Image,ImageDraw
root=Path(__file__).resolve().parents[1]
p=argparse.ArgumentParser();p.add_argument('--folder',default='LayerPolish');a=p.parse_args()
out=root/'Artifacts/VisualRefactor'/a.folder/'Review';out.mkdir(parents=True,exist_ok=True)
rows=json.loads((root/'Artifacts/VisualRefactor/LayerPolish/CAPAS_POR_MAPA.json').read_text())['layers']
for r in rows:
    candidate=root/'Assets/SuyuRun/Art/RefactorRuntime/Resources/RefactorBank'/a.folder/(r['name']+'.png')
    if candidate.exists():r['resource']=candidate.relative_to(root).as_posix()
for region in ('costa','sierra','selva'):
    group=[r for r in rows if r['name'].startswith(region)]
    for page in range(0,len(group),8):
        board=Image.new('RGB',(1280,230*4),(30,30,38));d=ImageDraw.Draw(board)
        for i,r in enumerate(group[page:page+8]):
            im=Image.open(root/r['resource']).convert('RGBA');spec=json.loads((root/r['resource']).with_suffix('.json').read_text())
            im=im.crop((0,0,spec['frameWidth'],spec['frameHeight']));im.thumbnail((630,196),Image.Resampling.NEAREST)
            x=(i%2)*640;y=(i//2)*230
            board.paste(im,(x,y+27),im);d.text((x+4,y+4),r['name'],fill='white')
        board.save(out/f'{region}_{page//8}.png')
print('Review contacts created; no runtime images modified')
