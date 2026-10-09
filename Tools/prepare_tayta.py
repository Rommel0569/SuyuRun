"""Normalize imagegen animation sources to native sheets; retain every source."""
from pathlib import Path
from PIL import Image, ImageOps
import json, shutil, sys

ROOT=Path(__file__).resolve().parents[1]
OUT=ROOT/'Assets/SuyuRun/Art/Tayta/Resources/Tayta'
REVIEW=ROOT/'Artifacts/Tayta';REVIEW.mkdir(parents=True,exist_ok=True)
specs={}; previews=[]

def source(path,rows,trim_rope=False):
    path=Path(path);shutil.copy2(path,REVIEW/path.name)
    im=Image.open(path).convert('RGBA');w,h=im.size
    # Generated atlases may have uneven vertical spacing. Detect real alpha bands
    # rather than slicing into nominal rows (which clips hats / includes neighbours).
    mask=im.getchannel('A').point(lambda v:255 if v>=128 else 0)
    counts=[sum(v>0 for v in mask.crop((0,y,w,y+1)).getdata()) for y in range(h)]
    ys=[y for y,n in enumerate(counts) if n>max(12,int(w*.012))]
    groups=[]
    for y in ys:
        if not groups or y-groups[-1][-1]>5:groups.append([y])
        else:groups[-1].append(y)
    bands=sorted(sorted(groups,key=lambda g:sum(counts[y] for y in g),reverse=True)[:rows],key=lambda g:g[0])
    if len(bands)!=rows:
        boundaries=[0]
        for k in range(1,rows):
            nominal=k*h/rows;radius=h/rows*.28
            cut=min(range(round(nominal-radius),round(nominal+radius)),key=lambda y:counts[y]+abs(y-nominal)*.03)
            boundaries.append(cut)
        boundaries.append(h)
        bands=[list(range(boundaries[k],boundaries[k+1])) for k in range(rows)]
    # Keep common scale based on designed grid width, not varying band height.
    result=[]
    for row in range(rows):
        top=max(0,bands[row][0]-2);bottom=min(h,bands[row][-1]+3)
        strip=mask.crop((0,top,w,bottom))
        xc=[sum(v>0 for v in strip.crop((x,0,x+1,strip.height)).getdata()) for x in range(w)]
        xgroups=[]
        for x,n in enumerate(xc):
            if n<4:continue
            if not xgroups or x-xgroups[-1][-1]>7:xgroups.append([x])
            else:xgroups[-1].append(x)
        if len(xgroups) in (7,8):
            xboxes=[(max(0,g[0]-2),min(w,g[-1]+3)) for g in xgroups]
            while len(xboxes)<8:xboxes.append(xboxes[-1])
        else:
            cuts=[0]
            for k in range(1,8):
                mid=k*w/8;radius=w/8*.25
                cuts.append(min(range(round(mid-radius),round(mid+radius)),key=lambda x:xc[x]+abs(x-mid)*.03))
            cuts.append(w);xboxes=list(zip(cuts,cuts[1:]))
        cells=[]
        for col in range(8):
            cell=im.crop((xboxes[col][0],top,xboxes[col][1],bottom))
            cw=w/8;ch=cell.height
            scale_height=w/8
            if trim_rope:
                cell=cell.crop((0,round(ch*.22),cell.width,ch))
                for yy in range(cell.height):
                    if sum(v>=128 for v in cell.getchannel('A').crop((0,yy,cell.width,yy+1)).getdata())>cw*.07:
                        cell=cell.crop((0,max(0,yy-2),cell.width,cell.height));break
            a=cell.getchannel('A').point(lambda v:255 if v>=128 else 0)
            cell.putalpha(a);box=a.getbbox();assert box,(path,row,col)
            crop=cell.crop(box)
            cells.append(crop.resize((max(1,round(crop.width*48/cw)),max(1,round(crop.height*48/scale_height))),Image.Resampling.NEAREST))
        result.append(cells)
    return result

def emit(region,state,cells,loop=True,fps=12):
    palpath=ROOT/f'Assets/SuyuRun/Art/Pixel{region.title()}/{region}_palette.json'
    if region=='selva':palpath=ROOT/'Assets/SuyuRun/Art/PixelSelva/ProductionV2/Resources/SelvaV2/selva_palette.json'
    colors=[tuple(bytes.fromhex(c[1:])) for c in json.loads(palpath.read_text(encoding='utf-8'))['colors']]
    target=OUT/region;target.mkdir(parents=True,exist_ok=True)
    atlas=Image.new('RGBA',(48*len(cells),48))
    # Preserve atlas-relative proportions: crouching/tucking does not enlarge the hero.
    scale=1
    for i,c in enumerate(cells):
        c=c.resize((max(1,round(c.width*scale)),max(1,round(c.height*scale))),Image.Resampling.NEAREST)
        if c.width>46 or c.height>46:c=ImageOps.contain(c,(46,46),Image.Resampling.NEAREST)
        pixels=[]
        for r,g,b,a in c.getdata():
            if not a:pixels.append((0,0,0,0));continue
            nearest=min(colors,key=lambda p:(p[0]-r)**2+(p[1]-g)**2+(p[2]-b)**2)
            pixels.append((*nearest,255))
        c.putdata(pixels);atlas.alpha_composite(c,(i*48+(48-c.width)//2,48-c.height))
    atlas.save(target/f'{state}.png')
    spec=dict(frames=len(cells),fps=fps,loop=loop,frameWidth=48,frameHeight=48)
    (target/f'{state}.json').write_text(json.dumps(spec),encoding='utf-8')
    specs[f'{region}/{state}']=spec
    preview=Image.new('RGBA',(384,48),(33,28,45,255));preview.alpha_composite(atlas.crop((0,0,min(384,atlas.width),48)))
    previews.append((region,state,preview))

def subset(row,ids):return [row[i] for i in ids]

def main(paths):
    costa=source(paths[0],7);sierra=source(paths[1],8);selva=source(paths[2],7)
    se=source(paths[3],4);ve=source(paths[4],7);swing=sum(source(paths[5],2,True),[])
    for region,rows in [('costa',costa),('sierra',sierra),('selva',selva)]:
        emit(region,'idle',rows[0],fps=5);emit(region,'run',rows[1],fps=14)
        emit(region,'jump',rows[2],False);emit(region,'land',subset(rows[2],[6,7]),False)
        emit(region,'double_jump',subset(rows[2],[1,2,3,4,5]),False)
        emit(region,'fall',subset(rows[2],[4,5,6]),False)
    for state,row in [('slide',3),('burst',4),('hit',5),('victory',6)]:emit('costa',state,costa[row][1:6] if state=='slide' else costa[row],state in ['slide','burst'])
    for state,row in [('slide',3),('huaraca_cargar',4),('huaraca_girar',5),('huaraca_lanzar',6),('victory',7)]:emit('sierra',state,sierra[row],state in ['slide','huaraca_girar'],24 if state.startswith('huaraca') else 12)
    emit('sierra','climb',subset(sierra[2],[1,2,3,4]),True)
    for state,row in [('burst',0),('hit',1),('death',2),('baile',3)]:emit('sierra',state,se[row],state in ['burst','baile'])
    for state,row in [('burst',3),('punch',5),('kick',6)]:emit('selva',state,selva[row],state=='burst')
    for state,row in [('slide',0),('reload',1),('guard',2),('uppercut',3),('hit',4),('death',5),('baile',6)]:emit('selva',state,ve[row],state in ['slide','reload','guard','baile'])
    emit('selva','victory',ve[6],True)
    emit('selva','swing',swing,False)
    for state,ids in [('grab',[7,8]),('regrab',[6,7,8]),('release',[8,9,10]),('climb',[5,6,7,8])]:emit('selva',state,subset(swing,ids),state=='climb')
    (REVIEW/'manifest.json').write_text(json.dumps(specs,indent=2),encoding='utf-8')
    for region in ['costa','sierra','selva']:
        rows=[p for r,s,p in previews if r==region]
        sheet=Image.new('RGBA',(384,len(rows)*48),(33,28,45,255))
        for i,row in enumerate(rows):sheet.alpha_composite(row,(0,i*48))
        sheet.resize((1536,len(rows)*192),Image.Resampling.NEAREST).save(REVIEW/f'tayta_{region}_preview_x4.png')
    comparison=Image.new('RGBA',(192,64),(33,28,45,255))
    for i,region in enumerate(['costa','sierra','selva']):
        im=Image.open(OUT/region/'idle.png').convert('RGBA').crop((0,0,48,48))
        comparison.alpha_composite(im,(i*64+8,8))
        run=Image.open(OUT/region/'run.png').convert('RGBA')
        frames=[]
        for f in range(8):
            canvas=Image.new('RGBA',(64,64),(33,28,45,255));canvas.alpha_composite(run.crop((f*48,0,(f+1)*48,48)),(8,8))
            frames.append(canvas.resize((256,256),Image.Resampling.NEAREST))
        frames[0].save(REVIEW/f'tayta_{region}_run.gif',save_all=True,append_images=frames[1:],duration=80,loop=0,disposal=2)
    comparison.resize((768,256),Image.Resampling.NEAREST).save(REVIEW/'tayta_tres_formas_x4.png')
    failures=[]
    for key,spec in specs.items():
        im=Image.open(OUT/f'{key}.png').convert('RGBA')
        if im.size!=(48*spec['frames'],48):failures.append(key+' size')
        if set(im.getchannel('A').getdata())-set([0,255]):failures.append(key+' alpha')
        colors={p[:3] for p in im.getdata() if p[3]}
        if len(colors)> (28 if key.startswith('selva/') else 24):failures.append(key+' colors')
    (REVIEW/'validation.json').write_text(json.dumps(dict(sheets=len(specs),frames=sum(s['frames'] for s in specs.values()),failures=failures),indent=2),encoding='utf-8')
    assert not failures,failures
    print(json.dumps(dict(sheets=len(specs),frames=sum(s['frames'] for s in specs.values())),indent=2))

if __name__=='__main__':main(sys.argv[1:])
