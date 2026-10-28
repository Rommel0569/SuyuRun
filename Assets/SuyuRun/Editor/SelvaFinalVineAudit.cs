using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace SuyuRun.Editor
{
    [InitializeOnLoad]
    public static class SelvaFinalVineAudit
    {
        const string Root="Artifacts/SelvaFinalVines";
        static SelvaFinalVineAudit(){EditorApplication.update+=Poll;}
        static void Poll()
        {
            string request=Root+"/audit.request";
            if(!File.Exists(request)||EditorApplication.isCompiling||EditorApplication.isUpdating||Application.isPlaying||EditorApplication.isPlaying||EditorApplication.isPlayingOrWillChangePlaymode)return;
            File.Delete(request);Run();
        }
        static FieldInfo Field(string name)=>typeof(SelvaStageDirector).GetField(name,BindingFlags.NonPublic|BindingFlags.Instance);
        static T Get<T>(SelvaStageDirector stage,string name)=>(T)Field(name).GetValue(stage);
        static void Set(SelvaStageDirector stage,string name,object value)=>Field(name).SetValue(stage,value);
        static void Require(bool value,string message){if(!value)throw new Exception(message);}
        [MenuItem("SUYURUN/Selva/Verificar lianas del tramo final")]
        public static void Run()
        {
            if(Application.isPlaying||EditorApplication.isPlaying||EditorApplication.isPlayingOrWillChangePlaymode)return;
            Directory.CreateDirectory(Root);
            var previous=SceneManager.GetActiveScene();
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            try
            {
                var stage=new GameObject("Final vine route audit").AddComponent<SelvaStageDirector>();stage.enabled=false;
                typeof(SelvaStageDirector).GetMethod("Awake",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(stage,null);
                stage.DiagnosticAutoplay=true;stage.DiagnosticZone(5);
                float start=Get<float>(stage,"zoneStart"),end=Get<float>(stage,"zoneEnd"),portal=end-480;
                Require(stage.Vines.Count==24&&stage.Caimans.Count==8,"Expected 8 ponds / 24 knots in final biome");
                Require(stage.Hero.x==start+92&&!stage.InMini,"Final biome starts on foot after the mini-level");
                foreach(var vine in stage.Vines)Require(vine.anchor.x>start+480&&vine.anchor.x<portal-320,"Vine outside reserved route");
                var surfaceAt=typeof(SelvaStageDirector).GetMethod("SurfaceAt",BindingFlags.NonPublic|BindingFlags.Instance);
                Require(surfaceAt.Invoke(stage,new object[]{start+100})!=null,"Opening path missing");
                Require(surfaceAt.Invoke(stage,new object[]{portal})!=null,"Portal has no floor");
                Require(surfaceAt.Invoke(stage,new object[]{portal-300})!=null,"Final approach missing");
                Capture(stage,start+650,"river.png");
                int released=0;
                for(int i=0;i<stage.Vines.Count;i++)
                {
                    stage.DiagnosticGrab(i);
                    for(int n=0;n<50;n++)stage.Step(1f/120,1);
                    Require(stage.IsHanging,"Vine attachment lost");
                    var before=stage.Hero;stage.DiagnosticRelease();stage.Step(1f/120,1);
                    Require(!stage.IsHanging&&stage.Hero.x>before.x,"Release does not carry hero forward");
                    Require(!float.IsNaN(stage.Hero.y)&&stage.Hero.y<205,"Invalid released trajectory");released++;
                }
                Set(stage,"pos",new Vector2(portal-350,148));Set(stage,"velocity",new Vector2(SelvaStageDirector.Speed,0));
                Capture(stage,portal-200,"final_path.png");
                for(int n=0;n<400;n++)stage.Step(1f/120,1);
                Require(Mathf.Abs(stage.Hero.x-portal)<.1f&&Get<float>(stage,"endingAt")>=0,"Hero failed to stop at final portal");
                // Reproduce the regression: song reaches its last phrase while still on the route.
                stage.DiagnosticZone(5);
                var music=Get<AudioSource>(stage,"music");
                float holdStart=Get<float>(stage,"finalHoldStart"),holdEnd=Get<float>(stage,"finalHoldEnd");
                Require(holdEnd>holdStart&&music.clip.length-holdEnd>=4.4f,"Musical cadence not reserved");
                music.time=holdEnd+.02f;stage.Step(1f/120,1);
                Require(Get<int>(stage,"finalMusicRepeats")==1&&Mathf.Abs(music.time-holdStart)<.15f,"Late traversal did not retain the music");
                Set(stage,"pos",new Vector2(portal-1,148));Set(stage,"velocity",new Vector2(SelvaStageDirector.Speed,0));
                stage.Step(1f/120,1);
                for(int n=0;n<290;n++)stage.Step(1f/120,1);
                Require(!Get<bool>(stage,"finishAfterFade")&&stage.Status!="Complete","Celebration was skipped before 2.5 seconds");
                typeof(SelvaStageDirector).GetMethod("Render",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(stage,null);
                Require(Get<string>(stage,"heroState")=="baile","Celebration is not rendering");
                for(int n=0;n<20;n++)stage.Step(1f/120,1);
                music.time=music.clip.length-.9f;stage.Step(1f/120,1);
                Require(Get<bool>(stage,"finishAfterFade"),"Cadence did not start the portal entry after celebration");
                for(int n=0;n<180;n++)stage.Step(1f/120,1);
                Require(stage.Status=="Complete","Portal fade did not complete");
                Set(stage,"complete",false);
                stage.DiagnosticZone(1);Require(stage.Vines.Count==27&&stage.Caimans.Count==9,"Original river route changed");
                File.WriteAllText(Root+"/result.txt",DateTime.Now.ToString("O")+"\nPASS Unity resource/route simulation: final 24 knots, 8 ponds; all 24 attachments/releases advance; continuous portal floor and stop; original river unchanged. Late music cursor repeats measured phrase with reserved cadence; celebration renders for at least 2.5s before 1.4s ingress and completion. Controlled cursor/physics/resource check, not a human playthrough or listening approval. Captures exclude OnGUI HUD.\n");
            }
            catch(Exception ex){File.WriteAllText(Root+"/result.txt","FAIL "+ex);Debug.LogException(ex);}
            finally{SceneManager.SetActiveScene(previous);EditorSceneManager.CloseScene(scene,true);}
        }
        static void Capture(SelvaStageDirector stage,float x,string name)
        {
            Set(stage,"pos",new Vector2(x,148));typeof(SelvaStageDirector).GetMethod("Render",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(stage,null);
            var camera=Get<Camera>(stage,"cam");var target=new RenderTexture(1280,720,24);target.Create();
            RenderPipeline.SubmitRenderRequest(camera,new UniversalRenderPipeline.SingleCameraRequest{destination=target});
            RenderTexture.active=target;var image=new Texture2D(1280,720,TextureFormat.RGBA32,false);
            image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();RenderTexture.active=null;
            File.WriteAllBytes(Root+"/"+name,image.EncodeToPNG());UnityEngine.Object.DestroyImmediate(image);target.Release();UnityEngine.Object.DestroyImmediate(target);
        }
    }
}
