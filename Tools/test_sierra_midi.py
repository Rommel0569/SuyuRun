import tempfile, unittest
from pathlib import Path
import mido
from audit_sierra_midi import extract

class MidiTimingTests(unittest.TestCase):
    def export(self,m):
        with tempfile.TemporaryDirectory(prefix='sierra-midi-test-') as folder:
            path=Path(folder)/'fixture.mid';m.save(path);return extract(path)
    def test_tempo_change_integrates_intervals(self):
        m=mido.MidiFile(ticks_per_beat=480);t=mido.MidiTrack();m.tracks.append(t)
        t.extend([mido.MetaMessage('set_tempo',tempo=500000,time=0),mido.Message('note_on',note=60,velocity=100,time=0),
                  mido.MetaMessage('set_tempo',tempo=1000000,time=480),mido.Message('note_on',note=62,velocity=100,time=480)])
        result=self.export(m)
        self.assertEqual([n['time'] for n in result['notes']],[0,1.5]);self.assertEqual(result['beatTimes'],[0,.5,1.5])
    def test_all_tracks_and_velocity_zero(self):
        m=mido.MidiFile(ticks_per_beat=480);a=mido.MidiTrack();b=mido.MidiTrack();m.tracks.extend([a,b])
        a.append(mido.MetaMessage('set_tempo',tempo=600000,time=0))
        b.extend([mido.Message('note_on',note=36,velocity=99,channel=9,time=480),mido.Message('note_on',note=36,velocity=0,channel=9,time=480)])
        result=self.export(m);self.assertEqual(len(result['notes']),1)
        self.assertEqual(result['notes'][0]['track'],1);self.assertEqual(result['notes'][0]['channel'],9)
        self.assertAlmostEqual(result['notes'][0]['time'],.6)
    def test_no_tempo_uses_midi_default(self):
        m=mido.MidiFile(ticks_per_beat=100);t=mido.MidiTrack();m.tracks.append(t)
        t.append(mido.Message('note_on',note=48,velocity=70,time=200))
        self.assertEqual(self.export(m)['notes'][0]['time'],1)

if __name__=='__main__':unittest.main()
