"""Non-destructive MIDI timing audit. Keeps the previous174-hit engineering intact."""
from pathlib import Path
import json, bisect, hashlib
import mido

ROOT=Path(__file__).resolve().parents[1]
MID=ROOT/'Assets/sierra.midi'
OUT=ROOT/'Artifacts/Audio/SierraAudit'
RES=ROOT/'Assets/SuyuRun/Data/Resources/SierraIntegration'

def extract(path):
    m=mido.MidiFile(path);events=[];absolute=[]
    for track,seq in enumerate(m.tracks):
        tick=0
        for order,msg in enumerate(seq):
            tick+=msg.time;absolute.append((tick,track,order,msg))
    absolute.sort(key=lambda a:a[:3]);tempo=500000;prev=0;seconds=0.;tempos=[dict(tick=0,time=0.,tempo=tempo)]
    for tick,track,order,msg in absolute:
        seconds+=mido.tick2second(tick-prev,m.ticks_per_beat,tempo);prev=tick
        if msg.type=='set_tempo':
            tempo=msg.tempo;tempos.append(dict(tick=tick,time=seconds,tempo=tempo))
        if msg.type=='note_on' and msg.velocity>0:
            events.append(dict(tick=tick,time=round(seconds,7),track=track,channel=msg.channel,pitch=msg.note,velocity=msg.velocity))
    # Quarter-note grid is declared MIDI time, NOT a claim of perceived musical meter.
    grid=[]
    for tick in range(0,prev+1,m.ticks_per_beat):
        t=next(p for p in reversed(tempos) if p['tick']<=tick)
        grid.append(round(t['time']+mido.tick2second(tick-t['tick'],m.ticks_per_beat,t['tempo']),7))
    return dict(source=path.name,sha256=hashlib.sha256(path.read_bytes()).hexdigest(),ticksPerBeat=m.ticks_per_beat,
                duration=seconds,tempoMap=tempos,beatTimes=grid,notes=events)

def main():
    OUT.mkdir(parents=True,exist_ok=True);RES.mkdir(parents=True,exist_ok=True)
    data=extract(MID);prior=json.loads((ROOT/'Artifacts/Audio/sierra_rhythm.json').read_text())
    notes=data['notes'];times=[n['time'] for n in notes];audit=[]
    for i,h in enumerate(prior['hits']):
        lo=bisect.bisect_left(times,h['t']-1e-6);hi=bisect.bisect_left(times,h['t']+.12-1e-6)
        group=notes[lo:hi];peak=max(group,key=lambda n:n['velocity']) if group else None
        audit.append(dict(index=i,time=h['t'],kind=h['kind'],notesInWindow=len(group),
                          representativeNote=peak,flags=['unvalidated_by_ear','classification_by_intensity_not_instrument','binned_120ms_not_exact_note_onset']))
    summary=dict(source=data['source'],midiDuration=data['duration'],audioDuration=prior['duration'],
                 differenceSeconds=round(prior['duration']-data['duration'],7),noteOnCount=len(notes),
                 declaredTempoBpm=120,tracks=len({n['track'] for n in notes}),channels=sorted({n['channel'] for n in notes}),
                 drumChannelPresent=any(n['channel']==9 for n in notes),priorHits=len(prior['hits']),
                 binsWithoutNotes=[a['index'] for a in audit if not a['notesInWindow']],
                 conclusions=['No stretch applied. End-of-audio gap may be trailing audio, not proven global drift.',
                 'Previous extractor reads track0 and absolute ticks: safe only for this single-track constant-tempo file.',
                 'Previous ground/air distinction uses intensity, not identified bass/drums. All174 accents need ear review.',
                 '325 onset beats in SierraSoundtrack are MP3 estimates; do not conflate them with declared MIDI quarter notes.'])
    for path,value in [(OUT/'sierra_midi_exact.json',data),(RES/'sierra_midi_exact.json',data),(OUT/'sierra_hit_review.json',audit),(OUT/'summary.json',summary)]:
        path.write_text(json.dumps(value,indent=2),encoding='utf-8')
    print(json.dumps(summary,indent=2))

if __name__=='__main__':main()
