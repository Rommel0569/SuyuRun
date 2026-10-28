from pathlib import Path
from PIL import Image
import numpy as np
from scipy import ndimage
b=Path(__file__).resolve().parents[1]/'Assets/SuyuRun/Art/RefactorRuntime/Resources/RefactorBank/TaytaUniform'
for name in ['selva/idle','selva/victory','selva/run','sierra/idle','sierra/shoot','costa/idle']:
    im=np.array(Image.open(b/(name+'.png')))[:,:384]
    red=(im[:,:,3]>0)&(im[:,:,0]>100)&(im[:,:,0]>im[:,:,1]*1.35)&(im[:,:,1]<125)
    labels,total=ndimage.label(red);found=[]
    for i,box in enumerate(ndimage.find_objects(labels),1):
        if box is not None and np.count_nonzero(labels[box]==i)>12:found.append((int(np.count_nonzero(labels[box]==i)),[(s.start,s.stop) for s in box]))
    print(name,sorted(found,key=lambda v:v[1][0][0])[:4])
