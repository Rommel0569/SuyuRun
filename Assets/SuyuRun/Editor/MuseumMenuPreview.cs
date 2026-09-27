using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace SuyuRun.Editor
{
    [InitializeOnLoad]
    public static class MuseumMenuPreview
    {
        const BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
        static double next;
        static MuseumMenuPreview()
        {
            EditorApplication.update+=Tick;
            Application.logMessageReceived+=(message,stack,type)=>{if(SessionState.GetBool("MuseumQA",false)&&(type==LogType.Error||type==LogType.Exception))SessionState.SetInt("MuseumErrors",SessionState.GetInt("MuseumErrors",0)+1);};
            EditorApplication.playModeStateChanged+=state=>{
                if(state!=PlayModeStateChange.EnteredEditMode||!SessionState.GetBool("MuseumQA",false))return;
                EditorSceneManager.playModeStartScene=AssetDatabase.LoadAssetAtPath<SceneAsset>(SessionState.GetString("MuseumPreviousStart",""));
                SessionState.SetBool("MuseumQA",false);
            };
        }
        static void Set(object target,string name,object value)=>target.GetType().GetField(name,Flags).SetValue(target,value);
        static void Mode(RunnerPrototype coast,string mode)
        {var f=typeof(RunnerPrototype).GetField("mode",Flags);f.SetValue(coast,Enum.Parse(f.FieldType,mode));}
        static void Tick()
        {
            if(EditorApplication.isCompiling||EditorApplication.isUpdating)return;
            if(!EditorApplication.isPlayingOrWillChangePlaymode&&(File.Exists("museum-preview.txt")||File.Exists("grazers-preview.txt")))
            {
                SessionState.SetBool("GrazersQA",File.Exists("grazers-preview.txt"));
                if(File.Exists("grazers-preview.txt"))AssetDatabase.ImportAsset("Assets/SuyuRun/Art/PixelSierra/ProductionV3/Resources/SierraV3/sierra_camelidos_pastando.png",ImportAssetOptions.ForceUpdate);
                if(File.Exists("grazers-preview.txt"))File.Delete("grazers-preview.txt");
                File.Delete("museum-preview.txt");Directory.CreateDirectory("Artifacts/MuseumMenu");
                SessionState.SetString("MuseumPreviousStart",AssetDatabase.GetAssetPath(EditorSceneManager.playModeStartScene));
                SessionState.SetBool("MuseumQA",true);SessionState.SetBool("SuyuRun.SierraDiagnostic",false);
                SessionState.SetBool("SuyuRun.Smoke",false);SessionState.SetBool("SuyuRun.ReferenceCheck",false);SessionState.SetBool("SuyuRun.CostaPortalQA",false);
                SessionState.SetInt("MuseumStep",0);SessionState.SetInt("MuseumErrors",0);
                EditorSceneManager.playModeStartScene=AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/SuyuRun/Scenes/Level_Costa.unity");
                EditorApplication.isPlaying=true;next=EditorApplication.timeSinceStartup+3;return;
            }
            if(!EditorApplication.isPlaying||!SessionState.GetBool("MuseumQA",false)||EditorApplication.timeSinceStartup<next)return;
            next=EditorApplication.timeSinceStartup+2;
            int step=SessionState.GetInt("MuseumStep",0);
            if(SessionState.GetBool("GrazersQA",false))
            {SierraGrazersCheck.Tick(step);SessionState.SetInt("MuseumStep",step+1);return;}
            var coast=UnityEngine.Object.FindFirstObjectByType<RunnerPrototype>();
            try
            {
                if(step<8)
                {
                    if(!coast)return;coast.AutomatedSmokeTest=false;
                    if(step==0)Mode(coast,"Menu");
                    if(step==2)Mode(coast,"Selector");
                    if(step==4||step==6)
                    {
                        typeof(RunnerPrototype).GetMethod("OpenMuseum",Flags).Invoke(coast,null);
                        var discovered=(bool[])typeof(RunnerPrototype).GetField("discovered",Flags).GetValue(coast);
                        for(int i=0;i<4;i++)discovered[i]=true;
                        Set(coast,"museumSelection",step==4?0:3);
                        var state=(SharedGameMenu.State)typeof(RunnerPrototype).GetField("sharedMenu",Flags).GetValue(coast);
                        state.index=step==4?0:3;
                    }
                    if(step%2==1)
                    {
                        if(step==1&&coast.CurrentState!="Menu")throw new Exception("Menu QA was interrupted: "+coast.CurrentState);
                        ScreenCapture.CaptureScreenshot(Path.GetFullPath("Artifacts/MuseumMenu/play-"+step+".png"));
                    }
                }
                if(step==8)
                {
                    foreach(var id in new[]{"huaco_moche","textil_paracas","nicomedes","maria_reiche","cabeza_clava","kero","quipu"})
                    {
                        var texture=MuseumArt.Get(id);if(!texture||texture.width!=64||texture.height!=64||texture.filterMode!=FilterMode.Point)throw new Exception("Invalid museum texture: "+id);
                    }
                    if(!Resources.Load<AudioClip>("Museum/MUSEO"))throw new Exception("Museum music missing");
                    RunnerProgress.EnterSierraMuseum=true;if(!RunnerProgress.OpenSierra())throw new Exception("Sierra unavailable");
                }
                if(step==9)
                {
                    var stage=UnityEngine.Object.FindFirstObjectByType<SierraStageDirector>();if(!stage)return;
                    stage.DiagnosticAutoplay=true;
                    var collected=(System.Collections.Generic.HashSet<int>)typeof(SierraStageDirector).GetField("collected",Flags).GetValue(stage);
                    collected.Add(0);collected.Add(1);collected.Add(2);
                }
                if(step==10)ScreenCapture.CaptureScreenshot(Path.GetFullPath("Artifacts/MuseumMenu/play-sierra.png"));
                if(step==11)
                {
                    var stage=UnityEngine.Object.FindFirstObjectByType<SierraStageDirector>();
                    var ambience=stage.GetComponent<MuseumAmbience>();
                    if(!ambience||ambience.Mix<.9f)throw new Exception("Sierra museum did not fade in");
                    var source=(AudioSource)typeof(MuseumAmbience).GetField("source",Flags).GetValue(ambience);
                    if(!source.isPlaying)throw new Exception("Museum source not playing");
                    Set(stage,"serviceTab",0);
                }
                if(step==12)ScreenCapture.CaptureScreenshot(Path.GetFullPath("Artifacts/MuseumMenu/pause-sierra.png"));
                if(step==13)Set(UnityEngine.Object.FindFirstObjectByType<SierraStageDirector>(),"serviceTab",2);
                if(step==14)ScreenCapture.CaptureScreenshot(Path.GetFullPath("Artifacts/MuseumMenu/settings-sierra.png"));
                if(step==15)Set(UnityEngine.Object.FindFirstObjectByType<SierraStageDirector>(),"serviceTab",3);
                if(step==16)ScreenCapture.CaptureScreenshot(Path.GetFullPath("Artifacts/MuseumMenu/shop-sierra.png"));
                if(step==17)UnityEngine.SceneManagement.SceneManager.LoadScene("Level_Costa");
                if(step==18)
                {
                    if(!coast)return;
                    typeof(RunnerPrototype).GetMethod("Begin",Flags).Invoke(coast,new object[]{false});
                    typeof(RunnerPrototype).GetMethod("TogglePause",Flags).Invoke(coast,null);
                    if(coast.CurrentState!="Paused")throw new Exception("Costa failed to pause");
                }
                if(step==19)ScreenCapture.CaptureScreenshot(Path.GetFullPath("Artifacts/MuseumMenu/pause-costa.png"));
                if(step==20)
                {
                    var state=(SharedGameMenu.State)typeof(RunnerPrototype).GetField("sharedMenu",Flags).GetValue(coast);state.tab=2;
                }
                if(step==21)ScreenCapture.CaptureScreenshot(Path.GetFullPath("Artifacts/MuseumMenu/settings-costa.png"));
                if(step==22)
                {
                    File.WriteAllText("Artifacts/MuseumMenu/play-verification.txt","PASS INICIO_FINAL menu, selector, seven 64px Point textures, museum playback/fade, shared Costa/Sierra museum/pause/settings/shop; errors="+SessionState.GetInt("MuseumErrors",0)+"; "+DateTime.Now.ToString("O"));
                    EditorApplication.isPlaying=false;return;
                }
                SessionState.SetInt("MuseumStep",step+1);
            }
            catch(Exception e){File.WriteAllText("Artifacts/MuseumMenu/play-verification.txt","FAIL "+e);Debug.LogException(e);EditorApplication.isPlaying=false;}
        }
    }
}
