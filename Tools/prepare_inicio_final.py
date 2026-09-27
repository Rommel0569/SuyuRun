"""Preserve INICIO_FINAL UI pixels; only mask the three scenery windows.
No background is cut into parallax strips: runtime uses actual independent level layers.
"""
from pathlib import Path
from PIL import Image, ImageDraw
import json
root=Path(__file__).resolve().parents[1]
source=Image.open(root/'Artifacts/ArtApproval/INICIO_FINAL.jpeg').convert('RGBA')
out=root/'Assets/SuyuRun/Art/SharedMenu/Resources';out.mkdir(parents=True,exist_ok=True)
# Coordinates measured against the reference displayed at 2048 x 1143.
sx,sy=source.width/2048,source.height/1143
polygons=[[(42,313),(649,313),(649,680),(664,709),(502,912),(159,912),(42,787)],
          [(729,313),(1328,313),(1328,684),(1360,737),(1246,901),(1158,955),(1001,955),(900,930),(728,740)],
          [(1408,313),(2010,313),(2010,876),(1964,953),(1720,1060),(1553,1016),(1410,926),(1386,761)]]
mask=Image.new('L',source.size,0);d=ImageDraw.Draw(mask)
for polygon in polygons:d.polygon([(round(x*sx),round(y*sy)) for x,y in polygon],fill=255)
# Keep the exact region plaques, all five button frames/text and the bottom tapestry.
for box in [(201,862,492,947),(882,862,1174,947),(1559,862,1851,947),(0,955,2048,1143)]:
    d.rectangle(tuple(round(v*(sx if i%2==0 else sy)) for i,v in enumerate(box)),fill=0)
result=source.copy();result.putalpha(mask.point(lambda p:255-p))
result.save(out/'inicio_final_marco.png')
# No palette quantization of approved reference UI: explicit latest user request takes precedence.
assert all(a==b for a,b,m in zip(source.getdata(),result.getdata(),mask.getdata()) if m==0)
(root/'Artifacts/MuseumMenu/inicio_frame_validation.json').write_text(json.dumps({'size':source.size,'unchanged_outside_scenery_masks':True,'source':'Artifacts/ArtApproval/INICIO_FINAL.jpeg'}),encoding='utf-8')
print('Frame saved; original UI pixels outside scenery masks unchanged.')
