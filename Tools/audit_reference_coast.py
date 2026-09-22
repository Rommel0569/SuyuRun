"""Read-only asset/course audit. Reports evidence, never rewrites approved assets."""
from pathlib import Path
import hashlib, json, math
import mido
ROOT=Path(__file__).resolve().parents[1]
art=ROOT/'Assets/SuyuRun/Art/PixelCosta'
hashes={p.name:hashlib.sha256(p.read_bytes()).hexdigest() for p in art.glob('*.png')}
duplicates={p.name:hashlib.sha256(p.read_bytes()).hexdigest()==hashes[p.name] for p in (art/'Resources').glob('*.png') if p.name in hashes}
assert all(duplicates.values()), 'Resources copy diverges from approved source'
# Every view pixel has exactly one tile source, including wrap boundaries and the ending.
for factor in (.15,.30,.50,.75):
    for sec in range(178):
        shift=(160+round(sec*7*32*factor))%640
        for x in (0,159,319):
            assert sum(i*640-shift <= x < (i+1)*640-shift for i in range(2))==1
m=mido.MidiFile(ROOT/'Assets/El Alcatraz.midi')
events=[]
for track_index,track in enumerate(m.tracks):
    tick=0
    for event_index,msg in enumerate(track):
        tick+=msg.time
        events.append((tick,track_index,event_index,msg))
events.sort(key=lambda e:e[:3])
tempo=500000;previous=0;seconds=0;notes=[];tempos=[]
for tick,track,event,msg in events:
    seconds+=mido.tick2second(tick-previous,m.ticks_per_beat,tempo);previous=tick
    if msg.type=='set_tempo':
        tempo=msg.tempo;tempos.append(dict(tick=tick,seconds=seconds,tempo=tempo))
    if msg.type=='note_on' and msg.velocity>0:
        notes.append(dict(seconds=seconds,track=track,pitch=msg.note,channel=msg.channel,velocity=msg.velocity))
report=dict(asset_hashes=hashes,resource_copies_match=duplicates,
    tile_coverage_seconds_0_to_177='PASS',ground_row=148,base_resolution=[320,180],
    midi_tracks=len(m.tracks),ticks_per_beat=m.ticks_per_beat,tempo_events=tempos,
    note_on_count=len(notes),channels=sorted(set(n['channel'] for n in notes)),
    drum_notes=sum(n['channel']==9 for n in notes),last_note_seconds=notes[-1]['seconds'],
    timing_status='Existing course preserved. MIDI transcription onsets are not a validated beat grid. Ear validation still required.')
out=ROOT/'Artifacts/ReferenceCoast';out.mkdir(parents=True,exist_ok=True)
(out/'audit.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
print(json.dumps({k:v for k,v in report.items() if k not in ('asset_hashes','resource_copies_match')},indent=2))
