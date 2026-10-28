"""Mechanical extraction, rigid registration and palette export. Does not paint anatomy."""
from pathlib import Path
import json,shutil,argparse
import numpy as np
from PIL import Image,ImageDraw
from scipy import ndimage
ROOT=Path(__file__).resolve().parents[1]
parser=argparse.ArgumentParser();parser.add_argument('--only');args=parser.parse_args()
OUT=ROOT/'Artifacts/TaytaUniform'; BANK=ROOT/'Assets/SuyuRun/Art/RefactorRuntime/Resources/RefactorBank/TaytaUniform'
OUT.mkdir(parents=True,exist_ok=True);(OUT/'Sources').mkdir(exist_ok=True)
SOURCES=json.loads((OUT/'SOURCES.json').read_text())
W,H,FOOT=384,320,308
PPU=32*160/34
all_cells={}; scales={}; report=[]
def source_path(name):
    local=OUT/'Sources'/f'{name}.png'
    return local if local.exists() else Path(SOURCES[name])
def extract(name,cols,rows):
    source=source_path(name);destination=OUT/'Sources'/f'{name}.png'
    if source.resolve()!=destination.resolve():shutil.copy2(source,destination)
    rgba=np.array(Image.open(source).convert('RGBA'));mask=rgba[:,:,3]>=128
    labels,total=ndimage.label(mask,structure=np.ones((3,3)))
    sizes=np.bincount(labels.ravel());boxes=ndimage.find_objects(labels)
    main=[i for i in range(1,total+1) if sizes[i]>500]
    centers={i:ndimage.center_of_mass(mask,labels,i) for i in main}
    ordered=sorted(main,key=lambda i:centers[i][0])
    if name=='sierra_base':
        # The costume edit preserves the Selva layout but two neighbouring poses
        # touch. Reference component centroids provide purely spatial cut seeds.
        ref=np.array(Image.open(source_path('selva_base')).convert('RGBA'))[:,:,3]>=128
        ll,nn=ndimage.label(ref,structure=np.ones((3,3)));ss=np.bincount(ll.ravel())
        rr=[i for i in range(1,nn+1) if ss[i]>500];cc={i:ndimage.center_of_mass(ref,ll,i) for i in rr}
        rr=sorted(rr,key=lambda i:cc[i][0]);rr=sum([sorted(rr[k:k+cols],key=lambda i:cc[i][1]) for k in range(0,len(rr),cols)],[])
        seeds=np.array([cc[i] for i in rr]);assert len(seeds)==cols*rows
    else:
        assert len(main)>=cols*rows,(name,'Missing whole poses',len(main))
        # A discarded original has ten runners in its final row; only its eight
        # intact airborne cells are used, above that row.
        ordered=sum([sorted(ordered[k:k+cols],key=lambda i:centers[i][1]) for k in range(0,cols*rows,cols)],[])
        seeds=np.array([centers[i] for i in ordered[:cols*rows]])
    assignments=np.full(labels.shape,-1,dtype=np.int16)
    median=float(np.median([sizes[i] for i in main]))
    for label,box in enumerate(boxes,1):
        if box is None:continue
        yy,xx=np.nonzero(labels==label)
        if name=='sierra_base' and (yy.max()-yy.min()>rgba.shape[0]/rows*1.3 or sizes[label]>median*1.6):
            # Separate touching neighbours without synthesizing or changing pixels.
            nearest=np.argmin((yy[:,None]-seeds[:,0])**2+(xx[:,None]-seeds[:,1])**2,axis=1)
            assignments[yy,xx]=nearest
        else:
            center=np.array([yy.mean(),xx.mean()]);index=int(np.argmin(np.sum((seeds-center)**2,axis=1)))
            assignments[yy,xx]=index
    cells=[]
    for index in range(cols*rows):
        data=rgba.copy();data[:,:,3]=np.where((assignments==index)&mask,255,0)
        cell=Image.fromarray(data);box=cell.getbbox()
        assert box,(name,index,'empty generated cell')
        cells.append(cell.crop(box))
    return cells
for region in ('costa','sierra','selva'):
    cells=extract(region+'_base',8,8)
    reference=float(np.mean([cells[i].height for i in range(16,20)]))
    scales[region+'_base']=160/reference;all_cells[region+'_base']=cells
for name,cols,rows,reference in [('sierra_actions',8,5,19),('selva_actions',8,8,31),('ceremony',8,4,31)]:
    cells=extract(name,cols,rows);all_cells[name]=cells
    scales[name]=160/cells[reference].height
# The vine-removal edit removed pistols from the airborne row. Select the intact
# generated airborne cells; those contain no vines or backgrounds themselves.
air=extract('selva_air_original',8,8)
all_cells['selva_actions'][48:56]=air[48:56]
def palette(region):
    path=ROOT/f'Assets/SuyuRun/Art/Pixel{region.title()}/{region}_palette.json'
    if region=='selva':path=ROOT/'Assets/SuyuRun/Art/PixelSelva/ProductionV2/Resources/SelvaV2/selva_palette.json'
    values=json.loads(path.read_text())['colors']
    if region=='selva':values=[c for c in values if c not in ('#C4E37A','#7FD4C9','#C67C73','#FFF0AE')]
    if region=='sierra':values=[c for c in values if c.upper() not in ('#FFFFFF','#E8EDF2','#FFF3D0','#F5DFA0')]
    return [tuple(bytes.fromhex(c[1:])) for c in values]
def registered(cell,scale,colors,lift=0,center=False):
    patch=cell.resize((round(cell.width*scale),round(cell.height*scale)),Image.Resampling.NEAREST)
    assert patch.height<H-8,('Pose height exceeds safe canvas',patch.size)
    pixels=np.array(patch);mask=pixels[:,:,3]>0
    red=mask&(pixels[:,:,0]>100)&(pixels[:,:,0]>pixels[:,:,1]*1.35)&(pixels[:,:,1]<125)
    ys,xs=np.nonzero(red)
    if len(xs):
        top=ys.min();head=xs[ys<=top+round(160*.22)];anchor=float(np.median(head)) if len(head) else patch.width/2
    else:anchor=patch.width/2
    if center:anchor=patch.width/2
    lut={}
    flat=[]
    for v in patch.getdata():
        if not v[3]:flat.append((0,0,0,0));continue
        if v[:3] not in lut:lut[v[:3]]=(*min(colors,key=lambda c:sum((c[k]-v[k])**2 for k in range(3))),255)
        flat.append(lut[v[:3]])
    patch.putdata(flat);x=round(W/2-anchor);y=round(FOOT-patch.height-lift*160/34)
    assert x>=2 and x+patch.width<=W-2 and y>=2,(x,y,patch.size,'Clipped anatomy; increase fixed canvas')
    result=Image.new('RGBA',(W,H));result.alpha_composite(patch,(x,y));return result
base={'run':(range(16),32,True),'idle':(range(16,20),8,True),'jump':(range(20,24),12,False),'double_jump':(range(24,28),12,False),'fall':(range(28,32),12,False),'land':(range(32,36),16,False),'slide':(range(37,39),8,True),'slide_enter':([36,37],16,False),'slide_exit':([38,39],16,False),'shoot':(range(40,44),20,False),'reload':(range(44,48),10,False),'hit':(range(48,52),16,False),'death':(range(52,56),10,False),'victory':(range(56,60),8,False),'guard':(range(60,64),10,True)}
manifest=json.loads((OUT/'MANIFEST.json').read_text()) if args.only else {}
def write(region,state,job,indices,fps,loop):
    if args.only and region+'/'+state!=args.only:return
    indices=list(indices);colors=palette(region);frames=[];frame_scales=[]
    if region=='sierra' and state=='hit':indices=[48,50,51,51] # Reject the touching/cut source pose49.
    for i,index in enumerate(indices):
        lift=([0,0,0,1,2,3,4,2]*2)[i] if state=='run' else [0,0,1,3,0,0,1,3][i] if state=='burst' and len(indices)==8 else 0
        scale=scales[job]
        if job.endswith('_base'):
            # Later generated rows used a smaller drawing-camera scale. Correct
            # whole poses from each row's neutral human reference, never fit the
            # height of a bent knee or stretch limbs independently.
            neutral=35 if 32<=index<40 else 47 if 40<=index<48 else 51 if 48<=index<56 else 63 if index>=56 else None
            if neutral is not None:scale=160/all_cells[job][neutral].height
            if region=='costa' and state.startswith('slide'):
                scale=min(scale,104/max(all_cells[job][j].height for j in (37,38)))
        if job=='selva_actions' and state=='portal_entry':scale=160/all_cells[job][47].height
        frame_scales.append(scale)
        frames.append(registered(all_cells[job][index],scale,colors,lift,state=='death'))
    atlas=Image.new('RGBA',(W*len(frames),H));bounds=[];gx=[];gy=[]
    for i,cell in enumerate(frames):
        atlas.paste(cell,(W*i,0));bounds.append(cell.getbbox())
        data=np.array(cell);skin=(data[:,:,3]>0)&(data[:,:,0]>140)&(data[:,:,1]>75)&(data[:,:,1]<210)&(data[:,:,0]-data[:,:,1]>20)
        ys,xs=np.nonzero(skin)
        if len(xs):yy=ys.min();xx=xs[ys<=yy+3];gx.append(float(np.median(xx)));gy.append(float(yy+2))
        else:gx.append(W/2);gy.append(12)
    target=BANK/region;target.mkdir(parents=True,exist_ok=True);atlas.save(target/(state+'.png'))
    spec=dict(frames=len(frames),fps=fps,loop=loop,frameWidth=W,frameHeight=H,pixelsPerUnit=PPU,footAnchor=[W/2,FOOT],gripX=gx,gripY=gy,uniformAnatomy=True,source=job,sourceIndices=indices,sourceScale=scales[job],frameUniformScales=frame_scales,frameBounds=bounds,integrated=True,playTested=False)
    (target/(state+'.json')).write_text(json.dumps(spec,indent=2))
    manifest[region+'/'+state]=spec;report.append(dict(region=region,state=state,frames=len(frames),bounds=bounds))
for region in ('costa','sierra','selva'):
    for state,(indices,fps,loop) in base.items():write(region,state,region+'_base',indices,fps,loop)
    write(region,'dance','ceremony',range({'costa':0,'sierra':8,'selva':16}[region],{'costa':8,'sierra':16,'selva':24}[region]),16,True)
    write(region,'baile','ceremony',range({'costa':0,'sierra':8,'selva':16}[region],{'costa':8,'sierra':16,'selva':24}[region]),16,True)
write('costa','burst','costa_base',range(40,44),16,True)
for region in ('sierra','selva'):
    job=region+'_actions';start=32 if region=='sierra' else 56
    write(region,'burst',job,range(start,start+8),16,True)
    start=24 if region=='sierra' else 48
    for state,first in [('jump_shoot',start),('double_jump_shoot',start),('fall_shoot',start+4)]:write(region,state,job,range(first,first+4),12,False)
    first=20 if region=='sierra' else 16
    write(region,'climb',job,range(first,first+4),12,True)
for state,indices,fps,loop in [('huaraca_cargar',range(4),12,False),('huaraca_girar',range(4,12),24,True),('huaraca_lanzar',range(12,20),24,False)]:write('sierra',state,'sierra_actions',indices,fps,loop)
write('sierra','victory','ceremony',range(24,28),8,False)
write('sierra','transformation','ceremony',range(24,28),8,False)
for state,indices,fps,loop in [('swing',range(8),12,False),('grab',range(8,12),16,False),('release',range(12,16),16,False),('regrab',range(20,24),16,False),('punch',range(24,28),20,False),('kick',range(28,32),20,False),('uppercut',range(32,36),20,False),('dodge',range(36,40),16,False),('portal_entry',range(40,48),12,False)]:write('selva',state,'selva_actions',indices,fps,loop)
(OUT/'MANIFEST.json').write_text(json.dumps(manifest,indent=2))
(OUT/'EXPORT_REPORT.json').write_text(json.dumps(dict(sheets=len(manifest),physicalFrames=sum(x['frames'] for x in manifest.values()),posesGenerated=328,pixelsPerUnit=PPU,records=[dict(name=k,frames=v['frames'],bounds=v['frameBounds']) for k,v in manifest.items()]),indent=2))
# A comparison board uses the actual registered runtime idle frames.
board=Image.new('RGBA',(W*3,H),(30,35,43,255));d=ImageDraw.Draw(board)
for i,region in enumerate(('costa','sierra','selva')):
    im=Image.open(BANK/region/'idle.png').crop((0,0,W,H));board.alpha_composite(im,(i*W,0));d.text((i*W+12,8),region,fill='white')
board.convert('RGB').save(OUT/'ANATOMY_COMPARISON.png')
print('Exported',len(manifest),'state sheets at common PPU',PPU)

# The ending uses the SAME authored bodies through its original choreography.
# No older oversized cinematic actor remains selected.
def refs(region,state,indices):return [(region,state,i) for i in indices]
cinema={
 'costa_run':refs('costa','run',range(16)),
 'costa_hero':refs('costa','run',range(0,16,2))+refs('costa','jump',[0,1,1,2])+refs('costa','double_jump',[0,1])+refs('costa','fall',[0,2])+refs('costa','land',range(4))+refs('costa','slide_enter',[0])+refs('costa','slide',[0,1])+refs('costa','slide_exit',[1]),
 'sierra_hero':refs('sierra','run',[0,4,8,12])+refs('sierra','huaraca_cargar',range(4))+refs('sierra','huaraca_girar',range(8))+refs('sierra','huaraca_lanzar',[0,2,4,5])+refs('sierra','huaraca_lanzar',[4,5,6,7]),
 'selva_hero':refs('selva','swing',range(8))+refs('selva','release',range(4))+refs('selva','land',range(4))+refs('selva','shoot',range(4))+refs('selva','shoot',[2,3])+refs('selva','idle',[0,1]),
 'portal_entry':refs('selva','portal_entry',[i//2 for i in range(16)]),
 'epilogue_hero':refs('selva','run',range(0,16,2))+refs('selva','victory',range(4))+refs('selva','portal_entry',[4,5,6,7]),
 'selva_portrait':refs('selva','idle',range(4))+refs('selva','guard',range(4))+refs('selva','hit',range(4))+refs('selva','idle',range(4))+refs('selva','victory',range(4))+refs('selva','dance',range(4))
}
target=BANK/'cinema';target.mkdir(exist_ok=True)
for name,sequence in cinema.items():
    cells=[];feet=[];gx=[];gy=[]
    for region,state,index in sequence:
        im=Image.open(BANK/region/(state+'.png')).convert('RGBA');cell=im.crop((index*W,0,(index+1)*W,H))
        spec=manifest[region+'/'+state]
        if name=='selva_portrait':
            crop=cell.crop((110,85,290,245));factor=min(W/crop.width,H/crop.height)
            patch=crop.resize((round(crop.width*factor),round(crop.height*factor)),Image.Resampling.NEAREST)
            cell=Image.new('RGBA',(W,H));cell.alpha_composite(patch,((W-patch.width)//2,H-patch.height))
        cells.append(cell);feet.append(cell.getbbox()[3]/H);gx.append(spec['gripX'][index]/W);gy.append(spec['gripY'][index]/H)
    atlas=Image.new('RGBA',(W*len(cells),H))
    for i,cell in enumerate(cells):atlas.paste(cell,(i*W,0))
    atlas.save(target/(name+'.png'))
    reference=212 if name=='costa_run' else 217 if name=='portal_entry' else 193
    spec=dict(frames=len(cells),frameWidth=W,frameHeight=H,fps=24,footY=feet,gripX=gx,gripY=gy,uniformAnatomy=True,displayScale=reference*H/(256*160) if name!='selva_portrait' else 1,sharedCharacterBank=True)
    (target/(name+'.json')).write_text(json.dumps(spec,indent=2))
print('Exported',len(cinema),'cinematic mappings from the same character bank')
