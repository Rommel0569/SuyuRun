"""Same onset/BPM analysis as analyze_rhythm.py (which is hardcoded to Alcatraz), generalized
for sierra.mp3 and selva.mp3 - this produces the "ingenieria de audio" piece needed for the
level courses: a real, measured beat grid (not the MIDI's own declared tempo, which is a
transcription-tool default,
see analyze_costa_midi.py's docstring) to drive RunnerSoundtrack.beatTimes for each level.
Estimates require listening review before trusting them for choreography.
"""
import json
from pathlib import Path
import numpy as np

root = Path(__file__).resolve().parents[1]
out = root / 'Artifacts' / 'Audio'

def analyze(name, source_label, max_excerpt=90.):
    info = json.loads((out / f'{name}-info.json').read_text(encoding='utf-8-sig'))
    audio = np.fromfile(out / f'{name}.f32', dtype='<f4').reshape(-1, info['channels']).mean(axis=1)
    decimate = max(1, info['frequency'] // 22050)
    audio = audio[::decimate]
    sr = info['frequency'] / decimate
    hop, win = 256, 1024
    frames = np.lib.stride_tricks.sliding_window_view(audio, win)[::hop]
    flux = np.zeros(len(frames))
    previous = np.zeros(win // 2 + 1)
    window = np.hanning(win)
    for start in range(0, len(frames), 512):
        spectrum = np.abs(np.fft.rfft(frames[start:start+512] * window, axis=1))
        spectrum = np.log1p(spectrum * 8)
        diff = np.diff(np.vstack((previous, spectrum)), axis=0)
        flux[start:start+len(spectrum)] = np.maximum(diff[:, 3:250], 0).mean(axis=1)
        previous = spectrum[-1]
    times = (np.arange(len(flux))*hop + win/2)/sr
    smooth = np.convolve(flux, np.ones(43)/43, mode='same')
    onsets = np.maximum(flux-smooth*.8, 0)
    onsets /= max(float(np.percentile(onsets, 95)), .00001)
    onsets = np.minimum(onsets, 3)
    duration = min(max_excerpt, float(info['duration']))
    excerpt = onsets[(times>=2)&(times<duration)]
    centered = excerpt-excerpt.mean()
    nfft = 1 << (2*len(centered)-1).bit_length()
    corr = np.fft.irfft(np.abs(np.fft.rfft(centered, nfft))**2, nfft)[:len(centered)]
    corr /= np.maximum(1, np.arange(len(centered), 0, -1))
    frame_rate = sr/hop
    bpms = np.arange(75., 161., .05)
    scores = np.zeros(len(bpms))
    for multiple, weight in [(1,1.), (2,.5), (3,.25)]:
        lags = multiple*60/bpms*frame_rate
        scores += np.interp(lags, np.arange(len(corr)), corr)*weight
    peaks = [i for i in range(1,len(bpms)-1) if scores[i]>=scores[i-1] and scores[i]>scores[i+1]]
    ranked = sorted(peaks, key=lambda i: float(scores[i]), reverse=True)
    candidates = []
    for i in ranked:
        if all(abs(float(bpms[i])-c['bpm'])>2 for c in candidates):
            candidates.append({'bpm': round(float(bpms[i]),2), 'score': round(float(scores[i]),4)})
        if len(candidates)==6: break
    bpm = candidates[0]['bpm']; period = 60/bpm
    phase_scores = []
    for phase in np.linspace(0, period, 128, endpoint=False):
        grid = np.arange(phase, duration, period)
        phase_scores.append(float(np.interp(grid, times, onsets).sum()))
    phase = float(np.argmax(phase_scores))/128*period
    marks = []
    for predicted in np.arange(phase, float(info['duration'])-.1, period):
        mask = (times>predicted-.075)&(times<predicted+.075)
        choices = np.flatnonzero(mask)
        if len(choices):
            preference = onsets[choices]-.4*np.abs(times[choices]-predicted)/.075
            picked = float(times[choices[np.argmax(preference)]])
        else:
            picked = float(predicted)
        if not marks or picked>marks[-1]+.15:
            marks.append(round(picked,5))
    report = {'source': source_label, 'duration': info['duration'],
              'clipStartSeconds': 0., 'excerptSeconds': duration, 'bpm': bpm, 'firstBeatSeconds': marks[0],
              'beatTimes': marks, 'candidates': candidates,
              'status': 'Automatic onset estimate; musical meter, beat phase and choreography require listening review.'}
    (out/f'{name}-analysis.json').write_text(json.dumps(report, indent=2), encoding='utf-8')
    print(name, json.dumps({k:v for k,v in report.items() if k!='beatTimes'}, indent=2))
    print(name, 'detected beat markers:', len(marks))

analyze('sierra', 'User supplied sierra.mp3')
analyze('selva', 'User supplied selva.mp3')
