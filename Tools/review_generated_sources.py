"""Contact board for identifying generated source files; analysis only."""
from pathlib import Path
from PIL import Image,ImageDraw
source=Path('C:/Users/USER/.codex/generated_images/01a0b183-22cf-7d11-8237-6260f5f81b06')
files=sorted(source.glob('*.png'),key=lambda p:p.stat().st_mtime,reverse=True)[:7]
board=Image.new('RGB',(1200,3*320),(25,29,38));draw=ImageDraw.Draw(board)
for i,p in enumerate(files):
    im=Image.open(p).convert('RGBA');im.thumbnail((390,280))
    x=i%3*400;y=i//3*320;board.paste(im,(x,y+32),im)
    draw.text((x+3,y+3),str(i)+' '+p.name[:22],fill='white')
out=Path(__file__).resolve().parents[1]/'Artifacts/FinalMovieV4/RecoveredSources.png'
board.save(out)
print('\n'.join(str(i)+' '+str(p) for i,p in enumerate(files)))
