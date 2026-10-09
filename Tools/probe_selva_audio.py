"""Read-only MP3 frame-duration audit; Unity AudioClip.length is authoritative."""
from pathlib import Path
import json
def duration(path):
    blob=path.read_bytes();i=0;seconds=0;frames=0
    while i+4<len(blob):
        h=int.from_bytes(blob[i:i+4],'big');version=(h>>19)&3;layer=(h>>17)&3;br=(h>>12)&15;sr=(h>>10)&3
        if h>>21!=2047 or version==1 or layer!=1 or br in (0,15) or sr==3:i+=1;continue
        rate=(44100,48000,32000)[sr]//(1 if version==3 else 2 if version==2 else 4)
        kbps=([0,32,40,48,56,64,80,96,112,128,160,192,224,256,320] if version==3 else [0,8,16,24,32,40,48,56,64,80,96,112,128,144,160])[br]
        length=(144000 if version==3 else 72000)*kbps//rate+((h>>9)&1)
        if i+length>len(blob):break
        seconds+=(1152 if version==3 else 576)/rate;frames+=1;i+=length
    return dict(name=path.name,seconds=seconds,frames=frames,bytes=len(blob))
if __name__=='__main__':
    root=Path(__file__).resolve().parents[1]
    result=[duration(p) for p in (root/'Assets/SuyuRun/Audio/Resources/SelvaAudio').glob('*.mp3')]
    print(json.dumps(result,indent=2))
