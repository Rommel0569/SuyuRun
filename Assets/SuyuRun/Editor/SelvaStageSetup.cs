using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Animations;
using UnityEngine;
namespace SuyuRun.Editor
{
    public sealed class SelvaPixelImport:AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            if(!assetPath.Contains("/PixelSelva/ProductionV2/")||!assetPath.EndsWith(".png"))return;
            var t=(TextureImporter)assetImporter;t.textureType=TextureImporterType.Sprite;t.spriteImportMode=SpriteImportMode.Single;t.spritePixelsPerUnit=32;
            t.filterMode=FilterMode.Point;t.textureCompression=TextureImporterCompression.Uncompressed;t.mipmapEnabled=false;t.alphaIsTransparency=true;t.npotScale=TextureImporterNPOTScale.None;t.wrapMode=TextureWrapMode.Clamp;t.maxTextureSize=8192;
            t.isReadable=assetPath.EndsWith("selva_agua_superficie.png");
            var settings=new TextureImporterSettings();t.ReadTextureSettings(settings);settings.spriteAlignment=(int)SpriteAlignment.TopLeft;settings.spritePivot=new Vector2(0,1);settings.spriteMeshType=SpriteMeshType.FullRect;t.SetTextureSettings(settings);
            string json=Path.ChangeExtension(assetPath,".json");
            if(File.Exists(json))
            {
                var s=JsonUtility.FromJson<SelvaArt.Spec>(File.ReadAllText(json));
                if(s.frames>1)
                {
                    t.spriteImportMode=SpriteImportMode.Multiple;
#pragma warning disable 0618
                    t.spritesheet=s.rectsUnity.Select((r,i)=>new SpriteMetaData{name=Path.GetFileNameWithoutExtension(assetPath)+"_"+i.ToString("D3"),rect=new Rect(r.x,r.y,r.width,r.height),alignment=(int)SpriteAlignment.Custom,pivot=new Vector2(0,1)}).ToArray();
#pragma warning restore 0618
                }
            }
        }
    }
    public static class SelvaStageSetup
    {
        public const string ScenePath="Assets/SuyuRun/Scenes/Level_Selva.unity";
        [MenuItem("SuyuRun/Selva/Preparar escena")]
        public static void Prepare()
        {
            AssetDatabase.Refresh();
            foreach(string path in Directory.GetFiles("Assets/SuyuRun/Art/PixelSelva/ProductionV2","*.png",SearchOption.AllDirectories))
            {
                var t=(TextureImporter)AssetImporter.GetAtPath(path.Replace('\\','/'));
                if(t!=null&&(t.filterMode!=FilterMode.Point||t.textureCompression!=TextureImporterCompression.Uncompressed))t.SaveAndReimport();
            }
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var root=new GameObject("Selva Stage");var stage=root.AddComponent<SelvaStageDirector>();stage.soundtrack=Resources.Load<RunnerSoundtrack>("SelvaSoundtrack");stage.pixelFont=Resources.Load<Font>("PressStart2P-Regular");
            EditorSceneManager.SaveScene(scene,ScenePath);
            var scenes=EditorBuildSettings.scenes.ToList();if(!scenes.Any(s=>s.path==ScenePath)){scenes.Add(new EditorBuildSettingsScene(ScenePath,true));EditorBuildSettings.scenes=scenes.ToArray();}
            AssetDatabase.SaveAssets();Debug.Log("SELVA_SETUP_PASS");
        }
        public static void PrepareAnimationAssets()
        {
            const string root="Assets/SuyuRun/Art/PixelSelva/ProductionV2/Resources/SelvaV2";
            foreach(string path in Directory.GetFiles(root,"*.png"))
            {var t=(TextureImporter)AssetImporter.GetAtPath(path.Replace('\\','/'));var s=JsonUtility.FromJson<SelvaArt.Spec>(File.ReadAllText(Path.ChangeExtension(path,".json")));if(s.frames>1&&t.spriteImportMode!=SpriteImportMode.Multiple||path.EndsWith("selva_agua_superficie.png")&&!t.isReadable)t.SaveAndReimport();}
            const string folder="Assets/SuyuRun/Art/PixelSelva/ProductionV2/Resources/SelvaV2Animation";
            Directory.CreateDirectory(folder);AssetDatabase.Refresh();
            var controller=AssetDatabase.LoadAssetAtPath<AnimatorController>(folder+"/Hero.controller");if(!controller)controller=AnimatorController.CreateAnimatorControllerAtPath(folder+"/Hero.controller");
            var machine=controller.layers[0].stateMachine;
            foreach(var path in Directory.GetFiles(root,"selva_heroe_*.png"))
            {
                var spec=JsonUtility.FromJson<SelvaArt.Spec>(File.ReadAllText(Path.ChangeExtension(path,".json")));string name=Path.GetFileNameWithoutExtension(path).Substring(12);
                var sprites=AssetDatabase.LoadAllAssetsAtPath(path.Replace('\\','/')).OfType<Sprite>().OrderBy(s=>s.name).ToArray();
                var clip=AssetDatabase.LoadAssetAtPath<AnimationClip>(folder+"/"+name+".anim");if(!clip){clip=new AnimationClip();AssetDatabase.CreateAsset(clip,folder+"/"+name+".anim");}
                clip.frameRate=spec.fps;
                AnimationUtility.SetObjectReferenceCurve(clip,EditorCurveBinding.PPtrCurve("",typeof(SpriteRenderer),"m_Sprite"),sprites.Select((s,i)=>new ObjectReferenceKeyframe{time=i/(float)spec.fps,value=s}).Concat(new[]{new ObjectReferenceKeyframe{time=spec.frames/(float)spec.fps,value=sprites[spec.loop?0:sprites.Length-1]}}).ToArray());
                var settings=AnimationUtility.GetAnimationClipSettings(clip);settings.loopTime=spec.loop;AnimationUtility.SetAnimationClipSettings(clip,settings);
                var state=machine.states.Select(s=>s.state).FirstOrDefault(s=>s.name==name)??machine.AddState(name);state.motion=clip;EditorUtility.SetDirty(clip);
            }
            EditorUtility.SetDirty(controller);AssetDatabase.SaveAssets();
        }
        [MenuItem("SuyuRun/Selva/Abrir nivel")]
        public static void Open(){if(!File.Exists(ScenePath))Prepare();EditorSceneManager.OpenScene(ScenePath);EditorSceneManager.playModeStartScene=AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);}
        public static void BatchPrepare(){Prepare();EditorApplication.Exit(0);}
    }
}
