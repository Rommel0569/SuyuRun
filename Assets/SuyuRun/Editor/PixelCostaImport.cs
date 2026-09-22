using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SuyuRun.Editor
{
    // Import contract for every approved Assets/SuyuRun/Art/PixelCosta/ asset, per ART_BIBLE.md
    // section 4: Point filter, no compression, no mipmaps, PPU 32, pivot at the top-left pixel
    // (matching PixelCostaSkyLayer's existing convention). Scene layers (capa1/2/3/5/6) are a
    // single sprite; multi-frame sheets are sliced here with explicit rects computed the same
    // way the Tools/draw_costa_*.py generators laid them out, converted from PIL's top-down rows
    // to Unity's bottom-up sprite rects.
    public sealed class PixelCostaImport : AssetPostprocessor
    {
        const string Folder = "Assets/SuyuRun/Art/PixelCosta/";
        const float Ppu = 32;
        static readonly Vector2 TopLeft = new Vector2(0, 1);

        void OnPreprocessTexture()
        {
            var path = assetPath.Replace('\\', '/');
            if (!path.StartsWith(Folder)) return;
            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = Ppu;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            var settings = importer.GetDefaultPlatformTextureSettings();
            settings.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SetPlatformTextureSettings(settings);

            var name = System.IO.Path.GetFileName(path);
            var sheet = SheetFor(name);
            // Single-sprite scene/ground layers (sky, Paracas, huacas, algarrobos, ground tile) get
            // tiled by repeating their UVs (see RunnerParallax.cs's BuildBibleBandLayer/
            // BuildFacadeTexture) so they need Repeat wrap. Multi-sprite atlases (hero, obstacles,
            // collectibles, effects, platforms, the mar flipbook) are sampled by exact named
            // sub-rect and never wrap, so Clamp avoids bleed between neighbouring sub-sprites.
            importer.wrapMode = sheet == null ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
            var texSettings = new TextureImporterSettings();
            importer.ReadTextureSettings(texSettings);
            texSettings.spriteAlignment = (int)SpriteAlignment.Custom;
            texSettings.spritePivot = TopLeft;
            if (sheet == null)
            {
                texSettings.spriteMode = (int)SpriteImportMode.Single;
                importer.SetTextureSettings(texSettings);
            }
            else
            {
                texSettings.spriteMode = (int)SpriteImportMode.Multiple;
                importer.SetTextureSettings(texSettings);
#pragma warning disable 0618
                var meta = new List<SpriteMetaData>();
                foreach (var f in sheet) meta.Add(new SpriteMetaData { name = f.name, rect = f.rect, alignment = (int)SpriteAlignment.Custom, pivot = TopLeft });
                importer.spritesheet = meta.ToArray();
#pragma warning restore 0618
            }
        }

        sealed class Frame { public string name; public Rect rect; }

        // row layout: (name, columns per row, cell width, cell height), rows stacked top-to-bottom
        // exactly like the Python generators built them.
        static List<Frame> RowSheet(int imgHeight, (string name, int count, int cw, int ch)[] rows)
        {
            var frames = new List<Frame>();
            int y = 0;
            foreach (var row in rows)
            {
                for (int col = 0; col < row.count; col++)
                {
                    float unityY = imgHeight - (y + row.ch);
                    frames.Add(new Frame { name = $"{row.name}_{col}", rect = new Rect(col * row.cw, unityY, row.cw, row.ch) });
                }
                y += row.ch;
            }
            return frames;
        }

        static List<Frame> SheetFor(string fileName) => fileName switch
        {
            // Frames stacked vertically (not side by side), per ART_APPROVALS.md's already-computed
            // Unity rects: (0,540),(0,360),(0,180),(0,0), each 640x180, in that playback order.
            "costa_capa4_mar.png" => new List<Frame> {
                new Frame{name="mar_0",rect=new Rect(0,540,640,180)},
                new Frame{name="mar_1",rect=new Rect(0,360,640,180)},
                new Frame{name="mar_2",rect=new Rect(0,180,640,180)},
                new Frame{name="mar_3",rect=new Rect(0,0,640,180)} },
            "costa_heroe.png" => RowSheet(336, new (string, int, int, int)[] {
                ("idle", 4, 48, 48), ("run", 8, 48, 48), ("jump", 3, 48, 48), ("land", 2, 48, 48),
                ("slide", 3, 48, 48), ("rafaga", 4, 48, 48), ("dano", 2, 48, 48) }),
            "costa_obstaculos.png" => RowSheet(96, new (string, int, int, int)[] {
                ("hexagono", 4, 32, 32), ("pajaro", 6, 32, 32), ("bote_pescador", 4, 64, 32) }),
            "costa_coleccionables.png" => RowSheet(64, new (string, int, int, int)[] {
                ("huaco", 2, 16, 16), ("textil", 2, 16, 16), ("ceramica", 2, 16, 16), ("moneda", 2, 16, 16) }),
            "costa_efectos.png" => RowSheet(80, new (string, int, int, int)[] {
                ("polvo", 4, 16, 16), ("salpicadura", 3, 16, 16), ("brillo_moneda", 2, 16, 16),
                ("destello", 2, 16, 16), ("farol", 2, 16, 16) }),
            "costa_plataformas.png" => new List<Frame> {
                new Frame{name="bloque",rect=new Rect(0,0,48,32)},
                new Frame{name="rampa",rect=new Rect(56,0,32,32)},
                new Frame{name="pilar",rect=new Rect(96,0,16,48)} },
            "costa_bailarin.png" => RowSheet(40, new (string, int, int, int)[] { ("baile", 6, 24, 40) }),
            "costa_heroe_logro.png" => RowSheet(48, new (string, int, int, int)[] { ("logro", 4, 48, 48) }),
            "costa_puerta_dimensional.png" => RowSheet(96, new (string, int, int, int)[] { ("puerta", 2, 64, 96) }),
            "costa_efecto_sorpresa.png" => RowSheet(32, new (string, int, int, int)[] { ("sorpresa", 3, 32, 32) }),
            "costa_efecto_absorcion.png" => RowSheet(40, new (string, int, int, int)[] { ("absorcion", 4, 40, 40) }),
            // Pieces at fixed x offsets in a single 32px-tall row, not a uniform grid - rects taken
            // straight from Tools/draw_costa_hud.py's own layout (PIL top-down y=0), converted to
            // Unity's bottom-up y here (unityY = 32 - (y+h)).
            "costa_hud.png" => new List<Frame> {
                new Frame{name="panel_h",rect=new Rect(0,12,32,20)},
                new Frame{name="bar_rafaga_frame",rect=new Rect(40,22,64,10)},
                new Frame{name="bar_rafaga_fill",rect=new Rect(82,22,11,10)},
                new Frame{name="bar_progress_frame",rect=new Rect(120,26,64,6)},
                new Frame{name="bar_progress_fill",rect=new Rect(162,26,11,6)},
                new Frame{name="piece_slot",rect=new Rect(200,16,16,16)},
                new Frame{name="icon_puntos",rect=new Rect(220,20,12,12)} },
            _ => null,
        };
    }
}
