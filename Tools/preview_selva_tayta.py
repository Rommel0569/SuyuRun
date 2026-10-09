"""Contact sheet of the effective Selva frames; never edits production assets."""
from pathlib import Path
from PIL import Image,ImageDraw
import json
root=Path(__file__).resolve().parents[1]
base=root/'Assets/SuyuRun/Art/Tayta/Resources/Tayta/selva'
states=['idle','run','jump','double_jump','fall','land','slide','burst','reload','grab','swing','regrab','release','climb','guard','punch','kick','uppercut','hit','death','baile','victory']
rows=[]
for state in states:
    suffix='_v3' if state=='run' else '_v2'
    data=base/(state+suffix+'.json');texture=base/(state+suffix+'.png')
    if not data.exists():data=base/(state+'.json')
    if not texture.exists():texture=base/(state+'.png')
    spec=json.loads(data.read_text());sheet=Image.open(texture).convert('RGBA')
    order=spec.get('frameOrder',list(range(spec['frames'])))
    for start in range(0,len(order),8):
        row=Image.new('RGBA',(480,56),(33,28,45,255));draw=ImageDraw.Draw(row)
        draw.text((3,20),state+(f' {start+1}+' if start else ''),fill=(245,223,160,255))
        for col,src in enumerate(order[start:start+8]):
            row.alpha_composite(sheet.crop((src*48,0,(src+1)*48,48)),(96+col*48,4))
        rows.append(row)
preview=Image.new('RGBA',(480,56*len(rows)))
for index,row in enumerate(rows):preview.alpha_composite(row,(0,56*index))
target=root/'Artifacts/Tayta/Polish/selva_frames_actuales_x4.png'
preview.resize((preview.width*4,preview.height*4),Image.Resampling.NEAREST).save(target)
print(target)
