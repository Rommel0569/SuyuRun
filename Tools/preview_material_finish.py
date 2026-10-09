"""Contact sheet for review only, not a game atlas."""
from pathlib import Path
import json,math
from PIL import Image,ImageDraw
root=Path(__file__).resolve().parents[1]
records=json.loads((root/'Artifacts/VisualRefactor/playable_materials.json').read_text())
bank=root/'Assets/SuyuRun/Art/RefactorRuntime/Resources/RefactorBank/Materials'
sheet=Image.new('RGB',(1800,math.ceil(len(records)/6)*180),(25,27,35));d=ImageDraw.Draw(sheet)
for index,row in enumerate(records):
    spec=json.loads((bank/(row['name']+'.json')).read_text());image=Image.open(bank/(row['name']+'.png')).convert('RGBA').crop((0,0,spec['frameWidth'],spec['frameHeight']))
    scale=max(1,min(3,280//image.width,140//image.height))
    if image.width>280:image=image.resize((280,round(image.height*280/image.width)),Image.Resampling.NEAREST)
    else:image=image.resize((image.width*scale,image.height*scale),Image.Resampling.NEAREST)
    x=(index%6)*300;y=(index//6)*180
    d.rectangle((x+4,y+4,x+295,y+174),outline=(66,72,83))
    d.text((x+10,y+10),row['name'],fill=(229,215,174))
    sheet.paste(image,(x+(300-image.width)//2,y+32+(136-image.height)//2),image)
sheet.save(root/'Artifacts/VisualRefactor/playable_materials_contact.png')
print('Saved material contact sheet (review only)')
