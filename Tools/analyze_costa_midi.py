"""ART_BIBLE.md section 14: extract a playable rhythm reference from the user-provided MIDI,
NOT a literal note-by-note obstacle map (the file is a dense automatic audio transcription -
12k+ note_on events on a single channel/track, sub-15ms apart - not a hand-arranged drum/bass
part). This bins onsets into short windows, weights bass-register notes higher (proxy for
"bateria/bajo" per the bible, since there is no separate drum channel), keeps only local
peaks with a minimum gap enforced between them, and classifies each kept hit as "ground"
(grave -> obstaculo de suelo) or "air" (aguda -> aereo/moneda) by its dominant pitch - exactly
the "graves=suelo, agudas=aereo" rule in section 14.5. Output is seconds, not beats: the
MIDI's declared tempo (a flat 120 BPM, no changes) is very likely a transcription-tool
default rather than a musically validated tempo, so onset SECONDS are the trustworthy part,
not a derived BPM.
"""
from pathlib import Path
import json
import mido

ROOT=Path(__file__).resolve().parents[1]

def load_onsets(path):
    m=mido.MidiFile(path)
    tpb=m.ticks_per_beat
    tempo=500000
    t=0
    events=[]
    for msg in m.tracks[0]:
        t+=msg.time
        if msg.type=='set_tempo':tempo=msg.tempo
        if msg.type=='note_on' and msg.velocity>0:
            events.append((mido.tick2second(t,tpb,tempo),msg.note,msg.velocity))
    return events

def build_rhythm(events,duration,window=0.12,min_gap=0.28,keep_fraction=.22):
    n_bins=int(duration/window)+2
    weight=[0.0]*n_bins
    for sec,pitch,vel in events:
        b=int(sec/window)
        if b>=n_bins:continue
        weight[b]+=vel
    nonzero=[w for w in weight if w>0]
    if not nonzero:return []
    threshold=sorted(nonzero)[int(len(nonzero)*(1-keep_fraction))]
    hits=[]
    last_t=-999
    for b in range(n_bins):
        if weight[b]<threshold:continue
        t=b*window
        if t-last_t<min_gap:continue
        # Local peak check: skip if a taller bin sits within the next couple of windows (avoid
        # picking the rising edge of the same hit twice).
        if b+1<n_bins and weight[b+1]>weight[b]:continue
        hits.append({'t':round(t,3),'weight':round(weight[b],1)})
        last_t=t
    # Kind by relative intensity, not pitch: this file has no clean drum/bass separation (see
    # module docstring), so a pitch-based grave/aguda split was arbitrary and swung to nearly
    # all-one-kind either way depending on the cutoff. Intensity is the one signal this data
    # actually supports: the strongest third of kept hits become ground obstacles (the song's
    # biggest accents), the rest become coins/air - still "graves=suelo, agudas=aire" in spirit
    # (big hit -> obstacle, lighter hit -> reward), just measured by loudness instead of pitch.
    weights_sorted=sorted(h['weight'] for h in hits)
    cut=weights_sorted[int(len(weights_sorted)*.67)] if weights_sorted else 0
    for h in hits:
        h['kind']='ground' if h['weight']>=cut else 'air'
    return hits

def summarize(name,hits,duration):
    ground=sum(1 for h in hits if h['kind']=='ground')
    air=len(hits)-ground
    gaps=[hits[i+1]['t']-hits[i]['t'] for i in range(len(hits)-1)]
    avg_gap=sum(gaps)/len(gaps) if gaps else 0
    print(f"{name}: duration={duration:.1f}s hits={len(hits)} (ground={ground} air={air}) "
          f"avg_gap={avg_gap:.2f}s min_gap={min(gaps) if gaps else 0:.2f}s max_gap={max(gaps) if gaps else 0:.2f}s "
          f"density={len(hits)/duration:.2f}/s")

if __name__=='__main__':
    OUT=ROOT/'Artifacts/Audio'
    OUT.mkdir(parents=True,exist_ok=True)
    # Costa's duration is the actual imported clip length from PROGRESS.md (177.3976s), not a
    # rounded guess - matters here because CostaAlcatraz.asset's course.duration is derived from
    # that same clip and hits within the last 2s of course.duration are filtered out downstream.
    # Sierra/selva durations corrected to the REAL imported mp3 clip length (measured via Unity
    # AudioClip.length), not the earlier rough guess - sierra's guess (171.0) was 6.5s short of
    # the real 177.5543s clip, which would have clipped the last ~6.5s of hits.
    songs={'costa':(ROOT/'Assets/El Alcatraz.midi',177.3976),
           'sierra':(ROOT/'Assets/sierra.midi',177.5543),
           'selva':(ROOT/'Assets/selva.midi',180.5322)}
    for name,(path,duration) in songs.items():
        events=load_onsets(path)
        hits=build_rhythm(events,duration)
        summarize(name,hits,duration)
        (OUT/f'{name}_rhythm.json').write_text(json.dumps({'source':path.name,'duration':duration,'hits':hits},indent=2),encoding='utf-8')
        print('  first 10 hits:',hits[:10])
