"""Selva palette proposal (24 colors), ART_BIBLE.md section 8: "verdes
profundos, verde lima, marrón río, naranja, rojo guacamayo y turquesa".
Read the bible first. Separate palette file/folder - each level keeps its
own 24-color set per section 2. Palette only, no layer art (scene
production for Sierra/Selva is paused here per the user's call).
"""
from pathlib import Path
import json
from PIL import Image, ImageDraw

ROOT=Path(__file__).resolve().parents[1]
ART=ROOT/'Assets/SuyuRun/Art/PixelSelva'
ART.mkdir(parents=True,exist_ok=True)
OUT=ROOT/'Artifacts/ArtApproval'

colors=[
 # Verde profundo - dosel, sombra de la selva
 "#0D2818","#1A3D22","#2C5530","#4A7A3E",
 # Verde lima - hojas al sol
 "#6FA83D","#96C954","#C4E37A",
 # Marrón río - troncos, orillas
 "#4A3524","#6B4D32","#8F6B42",
 # Turquesa - río, cascadas
 "#1B5E5A","#2E8B84","#4FB3AA","#7FD4C9",
 # Naranja - antorchas, luz cálida
 "#C9641E","#E8862E","#F5A94D",
 # Rojo guacamayo
 "#8B2020","#C13838","#E85D3A",
 # Azul noche - bosque de noche
 "#0A0E2A","#1B2450",
 # Dorado - ídolo del templo final
 "#D4A72C","#F0D166",
]
assert len(colors)==24 and len(set(colors))==24

(ART/'selva_palette.json').write_text(json.dumps({
    "name":"SUYURUN Selva - dosel y rio v1",
    "status":"proposed_pending_user_approval",
    "color_count":24,
    "colors":colors
},indent=2),encoding='utf-8')

groups=[4,3,3,4,3,3,2,2]
sw=48
gap=16
total_w=sw*24+gap*(len(groups)-1)
im=Image.new('RGB',(total_w,sw+20),(20,20,24))
d=ImageDraw.Draw(im)
x=0;i=0
for g in groups:
    for _ in range(g):
        c=tuple(bytes.fromhex(colors[i][1:]))
        d.rectangle((x,20,x+sw,20+sw),fill=c)
        i+=1;x+=sw
    x+=gap
im.save(OUT/'selva_palette_preview.png')
print(json.dumps({"colors":colors,"status":"proposed_pending_user_approval"},indent=2))
