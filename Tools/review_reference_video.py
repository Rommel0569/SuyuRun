"""Extract reference-video samples for analysis only; never an in-game asset."""
from pathlib import Path
import cv2,json
from PIL import Image,ImageDraw
source=Path('C:/Users/USER/Downloads/gemini_generated_video_6aa99b98.mp4')
root=Path(__file__).resolve().parents[1]
out=root/'Artifacts/FinalMovieV4/ReferenceReview';out.mkdir(parents=True,exist_ok=True)
capture=cv2.VideoCapture(str(source));fps=capture.get(cv2.CAP_PROP_FPS)
duration=capture.get(cv2.CAP_PROP_FRAME_COUNT)/fps
board=Image.new('RGB',(1280,4*210),(20,20,20));draw=ImageDraw.Draw(board)
for i in range(16):
    time=(duration-.1)*i/15;capture.set(cv2.CAP_PROP_POS_MSEC,time*1000)
    ok,frame=capture.read()
    if not ok:continue
    im=Image.fromarray(cv2.cvtColor(frame,cv2.COLOR_BGR2RGB))
    im.save(out/f'frame_{time:.2f}.png')
    im.thumbnail((320,180));x=i%4*320;y=i//4*210
    board.paste(im,(x,y+22));draw.text((x+4,y+4),f'{time:.2f}s',fill='white')
capture.release();board.save(out/'contact.png')
(out/'source.json').write_text(json.dumps({'path':str(source),'duration':duration,'fps':fps,'purpose':'reference analysis only, not integrated footage'},indent=2))
print(duration,'seconds, reference samples saved')
