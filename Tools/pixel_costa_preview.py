"""Read-only composition of approved PNGs for asset approval; never generates layers."""
import math
from PIL import Image

def render_review(art, output, name, new_layer, factor, colors):
    sky=Image.open(art/'costa_capa1_cielo.png').convert('RGBA').crop((160,0,480,180))
    clouds=Image.open(art/'costa_capa2_nubes.png').convert('RGBA')
    birds=Image.open(art/'costa_capa2_aves.png').convert('RGBA')
    land=Image.open(art/'costa_capa3_paracas.png').convert('RGBA')
    sea=Image.open(art/'costa_capa4_mar.png').convert('RGBA')
    water=[sea.crop((0,f*180,640,(f+1)*180)) for f in range(4)]
    # Approved layers beyond capa4 are added here as they land, each behind a file-exists
    # check so this keeps working unchanged for capa1-4's own review. Factor matches each
    # layer's approved parallax rate (see ART_APPROVALS.md).
    huacas_path=art/'costa_capa5_huacas.png'
    huacas=Image.open(huacas_path).convert('RGBA') if huacas_path.exists() else None
    frames=[]
    for index in range(120):
        t=index/10;view=sky.copy()
        shift=round(t*(40*.05+.4))%640
        for x in (-shift,640-shift):view.alpha_composite(clouds,(x,0))
        for group in range(4):
            for bird in range(5):
                phase=(t+group*6)%24;step=(bird+1)//2;side=1 if bird%2 else -1
                recede=group%2==1;size=min(2,int(phase/7)) if recede else 1
                x=round(-62+phase*21-step*(12-size*3))
                y=round(48+group%3*15+side*step*(4-size)-phase*(.7 if recede else 0)+math.sin(t*.8+group))
                cycle=(t+bird*.13+group*.4)%3;f=int(cycle*8)%6 if cycle<.75 else 2
                view.alpha_composite(birds.crop((f*16,size*16,f*16+16,size*16+16)),(x,y))
        layers=[(land,.15),(water[int(t*4)%4],.30)]
        if huacas is not None:layers.append((huacas,.50))
        layers.append((new_layer,factor))
        for layer,rate in layers:
            shift=(160+round(t*40*rate))%640
            for x in (-shift,640-shift):view.alpha_composite(layer,(x,0))
        frames.append(view.convert('RGB').resize((1280,720),Image.Resampling.NEAREST))
    output.mkdir(parents=True,exist_ok=True)
    frames[0].save(output/(name+'_preview_x4.png'))
    pal=Image.new('P',(1,1));pal.putpalette([v for c in colors for v in c[:3]]+[0]*(768-72))
    indexed=[f.quantize(palette=pal,dither=Image.Dither.NONE) for f in frames]
    indexed[0].save(output/(name+'.gif'),save_all=True,append_images=indexed[1:],duration=100,loop=0,optimize=False,disposal=2)
