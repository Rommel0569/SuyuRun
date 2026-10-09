"""Mechanical export of imagegen atlases: common row scale, palette and 48px cells.
No existing production sheet is overwritten. The generated sources remain intact.
"""
from pathlib import Path
from PIL import Image
import json, sys, statistics

ROOT=Path(__file__).resolve().parents[1]
OUT=ROOT/'Assets/SuyuRun/Art/Tayta/Resources/Tayta'
REVIEW=ROOT/'Artifacts/Tayta/Polish'
REVIEW.mkdir(parents=True,exist_ok=True)

def isolate_character(cell):
    """Remove neighbouring-cell fragments, retaining this hero and his flash."""
    alpha=cell.getchannel('A');pixels=alpha.load()
    pending={(x,y) for y in range(cell.height) for x in range(cell.width) if pixels[x,y]}
    groups=[]
    while pending:
        first=pending.pop();group={first};queue=[first]
        while queue:
            x,y=queue.pop()
            for dx in (-1,0,1):
                for dy in (-1,0,1):
                    p=x+dx,y+dy
                    if p in pending:pending.remove(p);group.add(p);queue.append(p)
        groups.append(group)
    main=max(groups,key=len)
    xs=[p[0] for p in main];ys=[p[1] for p in main]
    cx=(min(xs)+max(xs))/2;top=min(ys);h=max(ys)-top+1
    keep=set(main)
    for group in groups:
        # A muzzle flash may be detached, but always in front of this character,
        # never behind the back (the previous cell's flash).
        if min(p[0] for p in group)>cx and all(top+h*.2<=p[1]<=top+h*.65 for p in group):keep.update(group)
    for group in groups:
        for x,y in group-keep:cell.putpixel((x,y),(0,0,0,0))
    return cell

def export(source,state,regions,revision=2):
    image=Image.open(source).convert('RGBA')
    width,height=image.size
    cw=width//8
    mask=image.getchannel('A').point(lambda a:255 if a>=128 else 0)
    counts=[sum(v>0 for v in mask.crop((0,y,width,y+1)).getdata()) for y in range(height)]
    bands=[]
    for y,count in enumerate(counts):
        if count<width*.015:continue
        if not bands or y-bands[-1][-1]>6:bands.append([y])
        else:bands[-1].append(y)
    bands=sorted(sorted(bands,key=lambda b:sum(counts[y] for y in b),reverse=True)[:len(regions)],key=lambda b:b[0])
    if len(bands)!=len(regions):
        # Dense sprint sheets may not have a completely empty horizontal row.
        # Locate the narrowest inter-row gap near each intended grid boundary.
        cuts=[0]
        for row in range(1,len(regions)):
            nominal=row*height/len(regions);radius=height/len(regions)*.15
            cuts.append(min(range(round(nominal-radius),round(nominal+radius)),key=lambda y:counts[y]+abs(y-nominal)*.2))
        cuts.append(height)
        bands=[list(range(cuts[row],cuts[row+1])) for row in range(len(regions))]
    report=[]
    for row,region in enumerate(regions):
        palette_path=ROOT/f'Assets/SuyuRun/Art/Pixel{region.title()}/{region}_palette.json'
        if region=='selva':palette_path=ROOT/'Assets/SuyuRun/Art/PixelSelva/ProductionV2/Resources/SelvaV2/selva_palette.json'
        colors=[tuple(bytes.fromhex(c[1:])) for c in json.loads(palette_path.read_text())['colors']]
        cells=[]
        for f in range(8):
            cell=image.crop((f*cw,max(0,bands[row][0]-2),(f+1)*cw,min(height,bands[row][-1]+3)))
            cell.putalpha(cell.getchannel('A').point(lambda a:255 if a>=128 else 0))
            cell=isolate_character(cell)
            box=cell.getbbox()
            if not box:raise ValueError((region,state,f,'empty'))
            cells.append(cell.crop(box))
        # Never fit each individual pose to a different size. Same scale for
        # the whole cycle; a bent knee changes silhouette, not body proportions.
        target_height=45 if region=='selva' else 41
        scale=min(target_height/max(c.height for c in cells),46/max(c.width for c in cells))
        atlas=Image.new('RGBA',(384,48));frames=[];bounds=[]
        for f,cell in enumerate(cells):
            cell=cell.resize((round(cell.width*scale),round(cell.height*scale)),Image.Resampling.NEAREST)
            pixels=[]
            for r,g,b,a in cell.getdata():
                if not a:pixels.append((0,0,0,0));continue
                color=min(colors,key=lambda p:sum((p[i]-v)**2 for i,v in enumerate((r,g,b))))
                pixels.append((*color,255))
            cell.putdata(pixels)
            flight=2 if f in (3,7) else 0
            native=Image.new('RGBA',(48,48))
            x=(48-cell.width)//2
            if revision>=3 and state=='run':
                # The hat/head is a stable registration landmark. Do not centre
                # the expanding leg silhouette independently in every pose.
                head=cell.getchannel('A').crop((0,0,cell.width,max(1,round(cell.height*.3)))).getbbox()
                head_center=(head[0]+head[2])/2
                x=max(1,min(47-cell.width,round(30-head_center)))
            native.alpha_composite(cell,(x,48-flight-cell.height))
            bounds.append(native.getbbox());atlas.alpha_composite(native,(f*48,0))
            view=Image.new('RGBA',(64,64),(33,28,45,255));view.alpha_composite(native,(8,8))
            frames.append(view.resize((256,256),Image.Resampling.NEAREST))
        assert set(atlas.getchannel('A').getdata())<={0,255}
        assert all(b and b[3]-b[1]>=target_height-8 for b in bounds),(region,state,bounds)
        path=OUT/region/f'{state}_v{revision}'
        atlas.save(path.with_suffix('.png'))
        spec=dict(frames=8,fps=14,loop=True,frameWidth=48,frameHeight=48)
        if revision==3 and state=='run':spec['frameOrder']=[0,1,2,3,4,5,6,3]
        path.with_suffix('.json').write_text(json.dumps(spec))
        atlas.resize((1536,192),Image.Resampling.NEAREST).save(REVIEW/f'{region}_{state}_v{revision}_x4.png')
        frames[0].save(REVIEW/f'{region}_{state}_v{revision}.gif',save_all=True,append_images=frames[1:],duration=70,loop=0,disposal=2)
        # Distinct leg silhouettes, not merely arm movement.
        legs=[atlas.crop((f*48,32,(f+1)*48,48)).tobytes() for f in range(8)]
        assert len(set(legs))>=6,(region,'leg cycle too static')
        report.append(dict(region=region,state=state,scale=scale,bounds=bounds,distinctLegFrames=len(set(legs)),colors=len({p[:3] for p in atlas.getdata() if p[3]})))
    (REVIEW/f'{state}_v{revision}_validation.json').write_text(json.dumps(report,indent=2))
    print(json.dumps(report,indent=2))

if __name__=='__main__':
    args=sys.argv[3:];revision=2
    if args[-1].startswith('--revision='):revision=int(args.pop().split('=')[1])
    export(sys.argv[1],sys.argv[2],args,revision)
