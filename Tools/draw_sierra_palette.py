"""Sierra palette proposal (24 colors), ART_BIBLE.md section 7: "azul cielo
profundo, dorado de ichu, verde oliva, gris piedra, rojo textil, blanco
nieve". Read the bible first. Separate palette file/folder from Costa's -
each level keeps its own 24-color set per section 2 ("Paleta de 24 colores
POR NIVEL"), nothing here touches costa_palette.json or any Costa asset.
Not producing any layer yet - this is the checkpoint the bible itself asks
for before capa 1 ("cuando apruebe un asset, guarda su paleta").
"""
from pathlib import Path
import json
from PIL import Image, ImageDraw

ROOT=Path(__file__).resolve().parents[1]
ART=ROOT/'Assets/SuyuRun/Art/PixelSierra'
ART.mkdir(parents=True,exist_ok=True)
OUT=ROOT/'Artifacts/ArtApproval'

colors=[
 # Amanecer dorado - cielo, de cenit frío a horizonte cálido (opuesto al atardecer de Costa)
 "#1A2036","#2B3A5C","#4C5F86","#7B8FB3","#A8B8D1","#E8C878","#F5DFA0","#FFF3D0",
 # Gris piedra - andenes, muros incas, puente colgante
 "#3A3A42","#5C5C68","#8A8A94","#B8B8BE",
 # Verde oliva - laderas, vegetación de altura
 "#2E3B22","#45592E","#6B8040","#96A85C",
 # Dorado ichu - pasto andino
 "#C9A227","#E0BB45","#F0D375",
 # Rojo textil - acentos (poncho, franjas)
 "#8B2E2E","#B94A3D","#D97B55",
 # Blanco nieve - nevados
 "#E8EDF2","#FFFFFF",
]
assert len(colors)==24 and len(set(colors))==24

(ART/'sierra_palette.json').write_text(json.dumps({
    "name":"SUYURUN Sierra - amanecer dorado v1",
    "status":"proposed_pending_user_approval",
    "color_count":24,
    "colors":colors
},indent=2),encoding='utf-8')

# Swatch preview: 24 blocks in a row, grouped with small gaps by family.
groups=[8,4,4,3,3,2]
sw=48
gap_between_groups=16
total_w=sw*24+gap_between_groups*(len(groups)-1)
im=Image.new('RGB',(total_w,sw+20),(20,20,24))
d=ImageDraw.Draw(im)
x=0;i=0
labels=["cielo amanecer","gris piedra","verde oliva","dorado ichu","rojo textil","nieve"]
for gi,g in enumerate(groups):
    for _ in range(g):
        c=tuple(bytes.fromhex(colors[i][1:]))
        d.rectangle((x,20,x+sw,20+sw),fill=c)
        i+=1;x+=sw
    x+=gap_between_groups
im.save(OUT/'sierra_palette_preview.png')
print(json.dumps({"colors":colors,"status":"proposed_pending_user_approval"},indent=2))
