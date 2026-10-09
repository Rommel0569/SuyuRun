using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SuyuRun.Editor
{
    // Included in the REVIEW ZIP only. No current scene or level is modified.
    public sealed class SelvaProposalArtImport : AssetPostprocessor
    {
        const string Root = "Assets/SuyuRun/Art/PixelSelva/ProductionV1/";
        [Serializable] public sealed class Cell { public int x, y, width, height; }
        [Serializable] public sealed class Spec
        {
            public string name;
            public int frames;
            public float fps, ppu;
            public bool loop;
            public float[] pivot, border;
            public Cell[] rectsUnity;
        }

        void OnPreprocessTexture()
        {
            if (!assetPath.Replace('\\', '/').StartsWith(Root, StringComparison.Ordinal)) return;
            string sidecar = Path.ChangeExtension(assetPath, ".json");
            if (!File.Exists(sidecar)) return;
            var spec = JsonUtility.FromJson<Spec>(File.ReadAllText(sidecar));
            if (spec.rectsUnity == null || spec.rectsUnity.Length != spec.frames) return;
            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = spec.ppu;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.crunchedCompression = false;
            importer.mipmapEnabled = false;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.maxTextureSize = 8192;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.ClearPlatformTextureSettings("Standalone");
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMode = (int)SpriteImportMode.Multiple;
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);
            Vector4 border = spec.border != null && spec.border.Length == 4
                ? new Vector4(spec.border[0], spec.border[1], spec.border[2], spec.border[3]) : Vector4.zero;
#pragma warning disable 0618
            importer.spritesheet = spec.rectsUnity.Select((r, i) => new SpriteMetaData {
                name = spec.name + "_" + i.ToString("D3"),
                rect = new Rect(r.x, r.y, r.width, r.height),
                pivot = new Vector2(spec.pivot[0], spec.pivot[1]),
                alignment = (int)SpriteAlignment.Custom, border = border
            }).ToArray();
#pragma warning restore 0618
        }

        [MenuItem("SUYURUN/Selva propuesta/Importar arte y crear clips")]
        static void Build()
        {
            if (!Directory.Exists(Root)) return;
            string output = Root + "AnimationClips";
            Directory.CreateDirectory(output);
            AssetDatabase.Refresh();
            int count = 0;
            foreach (string sidecar in Directory.GetFiles(Root, "*.json", SearchOption.AllDirectories))
            {
                var spec = JsonUtility.FromJson<Spec>(File.ReadAllText(sidecar));
                string path = Path.ChangeExtension(sidecar, ".png").Replace('\\', '/');
                if (!File.Exists(path) || spec.rectsUnity == null) continue;
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                if (spec.frames < 2 || spec.fps <= 0) continue;
                var sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(s => s.name).ToArray();
                if (sprites.Length != spec.frames) { Debug.LogError("Selva: slicing incompleto " + path); continue; }
                string clipPath = output + "/" + spec.name + ".anim";
                var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
                bool create = clip == null;
                if (create) clip = new AnimationClip { name = spec.name };
                clip.frameRate = spec.fps;
                var keys = sprites.Select((sprite, i) => new ObjectReferenceKeyframe {
                    time = i / spec.fps, value = sprite
                }).ToList();
                keys.Add(new ObjectReferenceKeyframe { time = spec.frames / spec.fps, value = sprites.Last() });
                AnimationUtility.SetObjectReferenceCurve(clip,
                    EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite"), keys.ToArray());
                var animation = AnimationUtility.GetAnimationClipSettings(clip);
                animation.loopTime = spec.loop;
                AnimationUtility.SetAnimationClipSettings(clip, animation);
                if (create) AssetDatabase.CreateAsset(clip, clipPath); else EditorUtility.SetDirty(clip);
                count++;
            }
            AssetDatabase.SaveAssets();
            Debug.Log("Selva: clips creados " + count + ". Sin crear escenas, colliders ni gameplay.");
        }
    }
}
