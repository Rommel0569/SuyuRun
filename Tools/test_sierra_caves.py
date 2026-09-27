"""Read-only cave delivery checks. Runtime physics checks live in SierraCaveChecks.cs."""
import hashlib, json, pathlib, unittest
from PIL import Image
from build_sierra_artpack import C
ROOT=pathlib.Path(__file__).resolve().parents[1]
ART=ROOT/'Assets/SuyuRun/Art/PixelSierra/ProductionV3/Resources/SierraV3'

class CaveArtTests(unittest.TestCase):
    def test_palette_and_native_cells(self):
        manifest=json.loads((ROOT/'Artifacts/SierraCaves/manifest.json').read_text())
        self.assertEqual(len(manifest),28)
        for item in manifest:
            with self.subTest(asset=item['name']),Image.open(ART/item['file']) as im:
                self.assertTrue(all(c in C or c[3]==0 for c in im.convert('RGBA').getdata()))
                self.assertLessEqual(set(im.getchannel('A').getdata()),{0,255})
                self.assertEqual(item['ppu'],32)
                for frame in item['frames']:
                    self.assertLessEqual(frame['x']+frame['width'],im.width)
                    self.assertLessEqual(frame['y']+frame['height'],im.height)
    def test_layers_are_independent_and_repeat(self):
        hashes=set()
        for cave in (1,2):
            for layer in ('fondo','arcos'):
                p=ART/f'sierra_cueva{cave}_{layer}.png'
                hashes.add(hashlib.sha256(p.read_bytes()).hexdigest())
                with Image.open(p) as im:
                    self.assertEqual(im.size,(640,384))
                    self.assertEqual(im.crop((0,0,1,384)).tobytes(),im.crop((639,0,640,384)).tobytes())
        self.assertEqual(len(hashes),4)
    def test_selected_audio_copies_exact(self):
        hashes=[]
        for i in (1,2):
            source=(ROOT/f'Artifacts/Audio/cueva {i}.mp3').read_bytes()
            target=(ROOT/f'Assets/SuyuRun/Audio/Resources/Caves/cueva_{i}.mp3').read_bytes()
            self.assertEqual(hashlib.sha256(source).digest(),hashlib.sha256(target).digest())
            hashes.append(hashlib.sha256(target).digest())
        self.assertNotEqual(*hashes)

if __name__=='__main__':unittest.main()
