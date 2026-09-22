"""Writes the MIDI-derived rhythm (Tools/analyze_costa_midi.py output) directly into
CostaAlcatraz.asset's `encounters` block, replacing the old repeated-template beats, without
needing Unity open. Mirrors RunnerSoundtrack.SecondsToBeat/BeatToSeconds and
RunnerCourse.ValidateCourse's ordering rule exactly, and keeps ~1/7 of the ground hits as
Breakable so RunnerCoastalCombat.BuildCoastalEncounters still has crates to spawn birds and
museum-piece drops from (see ApplyMidiRhythm.cs for the same logic run from inside Unity -
this script is the no-Unity-required equivalent, kept in sync by hand).
"""
import json
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
COURSE_PATH = ROOT / 'Assets/SuyuRun/Data/Resources/CostaAlcatraz.asset'
SOUNDTRACK_PATH = ROOT / 'Assets/SuyuRun/Data/Resources/AlcatrazSoundtrack.asset'
RHYTHM_PATH = ROOT / 'Artifacts/Audio/costa_rhythm.json'

def parse_beat_times(text):
    m = re.search(r'beatTimes:\n((?:  - [\d.]+\n)+)', text)
    return [float(x) for x in re.findall(r'- ([\d.]+)', m.group(1))]

def seconds_to_beat(beat_times, seconds):
    lo, hi = 0, len(beat_times) - 1
    while hi - lo > 1:
        mid = (hi + lo) // 2
        if beat_times[mid] <= seconds: lo = mid
        else: hi = mid
    return lo + (seconds - beat_times[lo]) / (beat_times[lo + 1] - beat_times[lo])

def beat_to_seconds(beat_times, beat):
    index = max(0, min(len(beat_times) - 2, int(beat)))
    t = beat - index
    return beat_times[index] + (beat_times[index + 1] - beat_times[index]) * t

def parse_gaps(text):
    m = re.search(r'gaps:\n((?:  - x: [\d.]+\n    width: [\d.]+\n)+)', text)
    gaps = []
    for gm in re.finditer(r'- x: ([\d.]+)\n    width: ([\d.]+)', m.group(1)):
        gaps.append((float(gm.group(1)), float(gm.group(2))))
    return gaps

def main():
    course_text = COURSE_PATH.read_text(encoding='utf-8')
    soundtrack_text = SOUNDTRACK_PATH.read_text(encoding='utf-8')
    beat_times = parse_beat_times(soundtrack_text)
    gaps = parse_gaps(course_text)
    duration = float(re.search(r'duration: ([\d.]+)', course_text).group(1))
    speed = float(re.search(r'speed: (\d+)', course_text).group(1))

    data = json.loads(RHYTHM_PATH.read_text(encoding='utf-8'))
    entries = []
    n = 0
    for hit in data['hits']:
        if hit['kind'] != 'ground':
            continue
        t = hit['t']
        if t <= 2.0 or t >= duration - 2.0:
            continue
        x = t * speed
        if any(gx - 1.5 < x < gx + gw + 1.5 for gx, gw in gaps):
            continue
        n += 1
        kind = 2 if n % 7 == 0 else (1 if n % 4 == 0 else 0)
        beat = seconds_to_beat(beat_times, t)
        entries.append((beat, kind))

    entries.sort(key=lambda e: e[0])
    deduped = []
    previous = -1.0
    for beat, kind in entries:
        if beat <= previous + 0.02:
            continue
        deduped.append((beat, kind))
        previous = beat

    lines = ['  encounters:']
    for beat, kind in deduped:
        lines.append(f'  - beat: {beat:.6g}')
        lines.append(f'    kind: {kind}')
        lines.append('    count: 1')
    new_block = '\n'.join(lines) + '\n'

    new_text = re.sub(r'  encounters:\n(?:  - beat:.*\n    kind:.*\n    count:.*\n)+', new_block, course_text)
    assert new_text != course_text, 'encounters block was not replaced - regex did not match'
    COURSE_PATH.write_text(new_text, encoding='utf-8')

    kinds_count = {0: 0, 1: 0, 2: 0}
    for _, k in deduped:
        kinds_count[k] += 1
    print(f'Wrote {len(deduped)} encounters to {COURSE_PATH.relative_to(ROOT)}: '
          f'{kinds_count[0]} Jump, {kinds_count[1]} Slide, {kinds_count[2]} Breakable.')
    print('First 8:', deduped[:8])
    print('Last 4:', deduped[-4:])

if __name__ == '__main__':
    main()
