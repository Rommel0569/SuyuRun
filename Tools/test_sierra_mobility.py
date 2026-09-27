"""Read-only fixed-course and trajectory regression, no Unity or art changes."""
import json, pathlib, unittest
from PIL import Image
ROOT=pathlib.Path(__file__).resolve().parents[1]
COURSE=json.loads((ROOT/'Assets/SuyuRun/Data/Resources/SierraIntegration/sierra_course.json').read_text())
ART=ROOT/'Assets/SuyuRun/Art/PixelSierra/ProductionV3/Resources/SierraV3'
PROFILES=[json.loads((ART/f'sierra_{a}_capa7_suelo.sprite.json').read_text())['surfaceY'] for a in ('camino','valle','ruinas')]

class MobilityTests(unittest.TestCase):
    def test_crouch_foot_anchors_match_existing_frames(self):
        with Image.open(ART/'sierra_heroe_slide.png') as sheet:
            for i,padding in enumerate((10,10,4)):
                self.assertEqual(48-sheet.crop((i*48,0,(i+1)*48,48)).getbbox()[3],padding)

    def test_fast_bird_can_be_dodged_by_diving(self):
        for s in COURSE['mobility']:
            for dive in (False,True):
                dt=1/120;x0=s['t']*224+100
                profile=PROFILES[min(2,int(s['t']/(COURSE['duration']/3)))];floor=profile[int(x0%640)]
                points=[(x0,floor-4,5,19,13),(x0+224*.36,floor-67,4,23,28),(x0+224*.70,floor-116,7,23,30)]
                used=set();v=0;t=-.3;feet=profile[int((x0+t*224)%640)];grounded=True;hit=False
                while t<1.5:
                    world=x0+t*224
                    for i,(x,y,kind,rx,ry) in enumerate(points):
                        if i in used or (kind==5)!=grounded or (kind==7 and not dive):continue
                        if abs(world-x)<=rx and abs((feet if kind==5 else feet-22)-y)<=ry:
                            used.add(i);v=-180 if kind==5 else -190 if kind==4 else 270;grounded=False
                    surface=profile[int(world%640)]
                    if grounded:feet=surface
                    else:
                        v+=420*dt;feet+=v*dt
                        if v>=0 and feet>=surface:feet=surface;grounded=True
                    bird_screen=340-(224+800)*(t-.78)
                    if t>=.78 and abs(bird_screen-100)<15 and abs(feet-25-max(28,floor-91))<22:hit=True
                    t+=dt
                with self.subTest(t=s['t'],dive=dive):
                    self.assertEqual(hit,not dive)

    def test_variety_and_reaction_spacing(self):
        kinds={e['kind'] for e in COURSE['events']}
        self.assertTrue({'jump','double_jump','slide','rock','log','puddle','spike_pair','guardian','bird'}<=kinds)
        ordered=sorted(e['t'] for e in COURSE['events'])
        self.assertGreaterEqual(min(b-a for a,b in zip(ordered,ordered[1:])),1.6)
        self.assertTrue(all(0<t<COURSE['duration'] for t in ordered))

    def test_waves_have_reserved_ground_corridors(self):
        self.assertEqual([w['count'] for w in COURSE['waves']],[3,5])
        for w in COURSE['waves']:
            self.assertFalse(any(w['t']-1.5<e['t']<w['t']+w['count']*1.6+2 for e in COURSE['events']))
            self.assertFalse(any(w['t']-1.5<s['t']<w['t']+w['count']*1.6+2 for s in COURSE['mobility']))

    def test_corridors_are_clear(self):
        self.assertEqual(len(COURSE['mobility']),7)
        for s in COURSE['mobility']:
            self.assertFalse(any(s['t']-2<e['t']<s['t']+3 for e in COURSE['events']))
            self.assertTrue(all(abs(s['t']-b)>3 for b in (COURSE['duration']/3,COURSE['duration']*2/3)))

    def test_sequences_reachable_at_multiple_frame_rates(self):
        for fps in (30,60,144):
            for s in COURSE['mobility']:
                with self.subTest(fps=fps,t=s['t']):
                    dt=1/fps;x0=s['t']*224+100;region=min(2,int(s['t']/(COURSE['duration']/3)))
                    profile=PROFILES[region];floor=profile[int(x0%640)]
                    points=[(x0,floor-4,5,19,13),(x0+224*.36,floor-67,4,23,28),(x0+224*.70,floor-116,7,23,30)]
                    used=set();v=0;t=-.3;feet=profile[int((x0+t*224)%640)];grounded=True;landed=False
                    while t<1.6:
                        world=x0+t*224
                        for i,(x,y,kind,rx,ry) in enumerate(points):
                            if i in used or (kind==5)!=grounded:continue
                            if abs(world-x)<=rx and abs((feet if kind==5 else feet-22)-y)<=ry:
                                used.add(i);v=-180 if kind==5 else -190 if kind==4 else 270;grounded=False
                        surface=profile[int(world%640)]
                        if grounded:feet=surface
                        else:
                            v+=420*dt;feet+=v*dt
                            if v>=0 and feet>=surface:feet=surface;grounded=True;landed=True
                        t+=dt
                    self.assertEqual(used,{0,1,2});self.assertTrue(landed)

    def test_midi_anchors_unchanged(self):
        hits=json.loads((ROOT/'Artifacts/Audio/sierra_rhythm.json').read_text())['hits']
        for e in COURSE['events']+COURSE['mobility']:
            if e['sourceIndex']>=0:self.assertEqual(e['t'],hits[e['sourceIndex']]['t'])
            else:self.assertEqual(e['t'],e['beat']*.5)

if __name__=='__main__':unittest.main()
