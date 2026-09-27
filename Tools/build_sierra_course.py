"""Fixed authored encounter selection, anchored to existing MIDI accent indices.
Not a live analyzer. Keeping provenance makes every encounter reviewable by ear.
"""
from pathlib import Path
import json
ROOT=Path(__file__).resolve().parents[1]
rhythm=json.loads((ROOT/'Artifacts/Audio/sierra_rhythm.json').read_text())
hits=rhythm['hits'];duration=rhythm['duration']
# Hand-chosen ground encounters: teach, combine, then climax; safe tunnel approaches.
authored=[(7,'jump'),(12,'jump'),(18,'slide'),(22,'guardian'),(29,'jump'),(35,'slide'),(42,'jump'),
          (70,'jump'),(88,'jump'),
          (111,'double_jump'),(121,'guardian'),(128,'slide'),(135,'jump'),(144,'guardian'),(151,'double_jump'),(157,'slide'),(173,'guardian')]
events=[]
for index,kind in authored:
    h=hits[index];events.append(dict(sourceIndex=index,t=h['t'],beat=h['t']*2,kind=kind,
        validation='auditory_review_pending',reason='authored_ground_sequence'))
# Aerial optional targets retain the prior MIDI accents, with approach room from ground hazards.
last=-20
for index,h in enumerate(hits):
    t=h['t']
    if h['kind']!='air' or t<11 or t>duration-9 or t-last<6:continue
    if any(abs(t-b)<3 for b in (duration/3,duration*2/3)):continue
    if any(abs(t-e['t'])<2.5 for e in events):continue
    if any(duration/3+3+i*15<t<duration/3+12+i*15 for i in range(3)):continue
    events.append(dict(sourceIndex=index,t=t,beat=t*2,kind='bird',validation='auditory_review_pending',reason='optional_aerial_target'));last=t
mobility=[]
waves=[dict(t=85.0,count=3),dict(t=147.0,count=5)]
for desired in (14,32,50,75,106,139,160):
    index=min(range(len(hits)),key=lambda i:abs(hits[i]['t']-desired))
    t=hits[index]['t']
    mobility.append(dict(t=t,sourceIndex=index,kind='launch_orb_dive',validation='auditory_review_pending'))
# Reserve complete reaction/landing corridors: no unrelated enemy over the exercise.
events=[e for e in events if not any(m['t']-2<e['t']<m['t']+3 for m in mobility)]
def wave_reserved(t):return any(w['t']-1.5<t<w['t']+w['count']*1.6+2 for w in waves)
events=[e for e in events if not wave_reserved(e['t'])]
added=0
for index,h in enumerate(hits):
    t=h['t']
    if wave_reserved(t):continue
    if t<9 or t>duration-7 or any(abs(t-b)<3 for b in (duration/3,duration*2/3)):continue
    if any(m['t']-2<t<m['t']+3 for m in mobility):continue
    if any(duration/3+3+i*15<t<duration/3+11+i*15 for i in range(3)):continue
    kind=('rock','slide','log','spike_pair','puddle','guardian','double_jump')[added%7]
    gap=2.2 if t<duration/3 else 1.85 if t<duration*2/3 else 1.6
    if any(abs(t-e['t'])<(max(gap,2.25) if e['kind']=='bird' or e['kind']=='guardian' or kind=='guardian' else gap) for e in events):continue
    added+=1
    events.append(dict(sourceIndex=index,t=t,beat=t*2,kind=kind,validation='auditory_review_pending',reason='variety_with_reaction_clearance'))
# Fill sparse transcription stretches on the MIDI's explicit 120BPM beat grid.
# Authored chart, not audio analysis. Keep provenance distinct from detected accents.
for beat in range(12,int((duration-7)*2)):
    t=beat*.5
    if wave_reserved(t):continue
    if any(abs(t-b)<3 for b in (duration/3,duration*2/3)):continue
    if any(m['t']-2<t<m['t']+3 for m in mobility):continue
    if any(duration/3+3+i*15<t<duration/3+11+i*15 for i in range(3)):continue
    gap=2.2 if t<duration/3 else 1.9 if t<duration*2/3 else 1.65
    kind=('log','jump','slide','puddle','rock','spike_pair')[added%6]
    if any(abs(t-e['t'])<(max(gap,2.25) if e['kind'] in ('bird','guardian') else gap) for e in events):continue
    added+=1
    events.append(dict(sourceIndex=-1,t=t,beat=beat,kind=kind,validation='auditory_review_pending',reason='authored_on_declared_midi_grid'))
events.sort(key=lambda e:e['t'])
for i,e in enumerate(events):
    assert (hits[e['sourceIndex']]['t']==e['t']) if e['sourceIndex']>=0 else e['t']==e['beat']*.5
    assert 6<=e['t']<duration-6
    if i:assert e['t']-events[i-1]['t']>=1.6,(events[i-1],e)
course=dict(duration=duration,speedPixelsPerSecond=224,status='fixed_authored_draft_pending_human_play_and_ear_review',
            timing='MIDI-derived existing120ms accent bins. No tempo stretch. Mobility offsets are authored choreography.',events=events,mobility=mobility,waves=waves)
dest=ROOT/'Assets/SuyuRun/Data/Resources/SierraIntegration/sierra_course.json'
dest.write_text(json.dumps(course,indent=2),encoding='utf-8')
(ROOT/'Artifacts/Audio/SierraAudit/course_selection.json').write_text(json.dumps(course,indent=2),encoding='utf-8')
report=dict(encounters=len(events),ground=sum(e['kind']!='bird' for e in events),air=sum(e['kind']=='bird' for e in events),mobilitySequences=len(mobility),mobilityPoints=len(mobility)*3,crossingBirds=len(mobility),minGap=min(events[i]['t']-events[i-1]['t'] for i in range(1,len(events))))
print(json.dumps(report))
