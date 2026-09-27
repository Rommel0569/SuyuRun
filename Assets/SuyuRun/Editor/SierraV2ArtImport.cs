using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SuyuRun.Editor
{
    // Restricted to the new proposal: does not reimport Costa or old Sierra assets.
    public sealed class SierraV2ArtImport : AssetPostprocessor
    {
        const string Root = "Assets/SuyuRun/Art/PixelSierra/ProductionV2/";
        const string RootV3 = "Assets/SuyuRun/Art/PixelSierra/ProductionV3/";
        [Serializable] public sealed class Cell { public string name; public int x, y, width, height; }
        [Serializable] public sealed class Spec
        {
            public string name, kind; public float pivotX, pivotY, fps; public int count;
            public bool loop; public Cell[] frames;
        }
        void OnPreprocessTexture()
        {
            string path=assetPath.Replace('\\','/');
            if (!path.StartsWith(Root, StringComparison.Ordinal) && !path.StartsWith(RootV3, StringComparison.Ordinal)) return;
            string sidecar = Path.ChangeExtension(assetPath, ".sprite.json");
            if (!File.Exists(sidecar)) return;
            var spec = JsonUtility.FromJson<Spec>(File.ReadAllText(sidecar));
            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 32;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.crunchedCompression = false;
            importer.mipmapEnabled = false;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.maxTextureSize = 8192;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
            settings.spriteMode = (int)SpriteImportMode.Multiple;
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            settings.spritePivot = new Vector2(spec.pivotX, spec.pivotY);
            importer.SetTextureSettings(settings);
#pragma warning disable 0618
            importer.spritesheet = spec.frames.Select(f => new SpriteMetaData {
                name=f.name, rect=new Rect(f.x,f.y,f.width,f.height),
                pivot=new Vector2(spec.pivotX,spec.pivotY), alignment=(int)SpriteAlignment.Custom,
                border=spec.name.Contains("ui_panel") || spec.name.Contains("ui_boton") ? new Vector4(2,2,2,2) : Vector4.zero
            }).ToArray();
#pragma warning restore 0618
        }

        [MenuItem("SUYURUN/Sierra V2/Importar sprites y crear clips")]
        public static void BuildClips()
        {
            BuildClipsForRoot(Root);
        }

        [MenuItem("SUYURUN/Sierra V3/Importar sprites y crear clips")]
        public static void BuildClipsV3()
        {
            BuildClipsForRoot(RootV3);
        }

        static void BuildClipsForRoot(string root)
        {
            AssetDatabase.Refresh();
            string output = root + "AnimationClips";
            Directory.CreateDirectory(output);
            AssetDatabase.Refresh();
            int count=0;
            foreach (string sidecar in Directory.GetFiles(root, "*.sprite.json", SearchOption.AllDirectories))
            {
                string texture=sidecar.Replace('\\','/').Replace(".sprite.json", ".png");
                AssetDatabase.ImportAsset(texture, ImportAssetOptions.ForceUpdate);
                var spec=JsonUtility.FromJson<Spec>(File.ReadAllText(sidecar));
                if (spec.count<2) continue;
                var sprites=AssetDatabase.LoadAllAssetsAtPath(texture).OfType<Sprite>().ToDictionary(s=>s.name);
                if (spec.frames.Any(f=>!sprites.ContainsKey(f.name))) { Debug.LogError("Slicing incomplete: "+texture);continue; }
                string target=output+"/"+spec.name+".anim";
                var clip=AssetDatabase.LoadAssetAtPath<AnimationClip>(target);
                bool create=clip==null;
                if (create) clip=new AnimationClip { name=spec.name };
                clip.frameRate=spec.fps;
                var keys=spec.frames.Select((f,i)=>new ObjectReferenceKeyframe { time=i/spec.fps, value=sprites[f.name] }).ToList();
                keys.Add(new ObjectReferenceKeyframe { time=spec.count/spec.fps, value=sprites[spec.frames[spec.count-1].name] });
                AnimationUtility.SetObjectReferenceCurve(clip, EditorCurveBinding.PPtrCurve("",typeof(SpriteRenderer),"m_Sprite"),keys.ToArray());
                var settings=AnimationUtility.GetAnimationClipSettings(clip);settings.loopTime=spec.loop;
                AnimationUtility.SetAnimationClipSettings(clip,settings);
                if (create) AssetDatabase.CreateAsset(clip,target); else EditorUtility.SetDirty(clip);
                count++;
            }
            AssetDatabase.SaveAssets();
            Debug.Log($"{root}: {count} clips. Arte propuesto; no se ha cambiado ninguna escena ni Costa.");
        }
    }
}
