using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace SuyuRun.Editor
{
    // Short real Editor Play smoke test, not a complete playthrough or visual QA.
    [InitializeOnLoad] public static class RefactorPlayAudit
    {
        const string Active="SuyuRun.Refactor.PlayAudit.Active",Stage="SuyuRun.Refactor.PlayAudit.Stage",Started="SuyuRun.Refactor.PlayAudit.Started";
        static readonly string[] scenes={"Level_Costa","Level_Sierra_Integration","Level_Selva"};
        static readonly float[] sampleLengths={8,36,27};
        [System.Serializable] sealed class SceneSnapshot{public SceneSetup[] scenes;}
        static RefactorPlayAudit(){EditorApplication.update+=Tick;Application.logMessageReceived+=Log;}
        public static void Start()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new System.InvalidOperationException("Leave current Play before starting the audit.");
            for(int i=0;i<UnityEngine.SceneManagement.SceneManager.sceneCount;i++)
                if(UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty)
                    throw new System.InvalidOperationException("Save your scene first; the audit will not discard edits.");
            SessionState.SetString(Active+".SceneSnapshot",JsonUtility.ToJson(new SceneSnapshot{scenes=EditorSceneManager.GetSceneManagerSetup()}));
            Directory.CreateDirectory("Artifacts/VisualRefactor");File.WriteAllText("Artifacts/VisualRefactor/play_visual_tour.txt","Editor Play diagnostic visual tour: all Sierra acts and caves; all Selva zones. Not a complete chronological playthrough.\n");
            SessionState.SetString(Active+".PreviousStart",EditorSceneManager.playModeStartScene?AssetDatabase.GetAssetPath(EditorSceneManager.playModeStartScene):"");
            SessionState.SetBool(Active,true);SessionState.SetInt(Stage,0);Load();
        }
        static void Load()
        {
            string path="Assets/SuyuRun/Scenes/"+scenes[SessionState.GetInt(Stage,0)]+".unity";
            EditorSceneManager.OpenScene(path);EditorSceneManager.playModeStartScene=AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
            SessionState.SetFloat(Started,0);SessionState.SetBool(Started+".Captured",false);SessionState.SetInt(Started+".Step",-1);EditorApplication.EnterPlaymode();
        }
        static void Log(string message,string trace,LogType type)
        {
            if(SessionState.GetBool(Active,false)&&(type==LogType.Exception||type==LogType.Error))
                File.AppendAllText("Artifacts/VisualRefactor/play_visual_tour.txt","ERROR "+message+"\n"+trace+"\n");
        }
        static void Tick()
        {
            const string request="Artifacts/VisualRefactor/REQUEST_PLAY_AUDIT.txt";
            if(!SessionState.GetBool(Active,false)&&!EditorApplication.isCompiling&&!EditorApplication.isUpdating&&System.IO.File.Exists(request))
            {
                File.Delete(request);
                try{GlobalEngineeringAudit.Run();RefactorMaterialAudit.Run();PixelViewportAudit.Run();Start();if(File.Exists("Artifacts/VisualRefactor/play_request_result.txt"))File.Delete("Artifacts/VisualRefactor/play_request_result.txt");}
                catch(System.Exception e){File.WriteAllText("Artifacts/VisualRefactor/play_request_result.txt",e.ToString());Debug.LogWarning(e.Message);}
            }
            if(!SessionState.GetBool(Active,false)||EditorApplication.isCompiling||EditorApplication.isUpdating)return;
            int stage=SessionState.GetInt(Stage,0);
            if(EditorApplication.isPlaying)
            {
                float began=SessionState.GetFloat(Started,0);
                if(began==0)
                {
                    SessionState.SetFloat(Started,(float)EditorApplication.timeSinceStartup);
                    var runner=Object.FindFirstObjectByType<RunnerPrototype>();
                    if(runner){runner.AutomatedSmokeTest=true;typeof(RunnerPrototype).GetMethod("Begin",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(runner,new object[]{false});}
                    return;
                }
                float age=(float)EditorApplication.timeSinceStartup-began;
                var costa=Object.FindFirstObjectByType<RunnerPrototype>();if(costa)costa.AutomatedSmokeTest=true;
                var sierra=Object.FindFirstObjectByType<SierraStageDirector>();if(sierra)sierra.DiagnosticAutoplay=true;
                var selva=Object.FindFirstObjectByType<SelvaStageDirector>();
                int step=SessionState.GetInt(Started+".Step",-1);
                if(sierra)
                {
                    int wanted=age<4?0:age<8?1:age<12?2:age<24?3:4;
                    if(wanted!=step){SessionState.SetInt(Started+".Step",wanted);if(wanted<3)sierra.DiagnosticAct(wanted);else sierra.DiagnosticStartCave(wanted-3);}
                    if((wanted<3&&age%4>2)||wanted>=3&&sierra.CavePlaying)CaptureOnce("Sierra_"+wanted,wanted);
                }
                if(selva)
                {
                    selva.DiagnosticAutoplay=true;
                    int wanted=Mathf.Min(5,Mathf.FloorToInt(age/4));
                    if(wanted!=step){SessionState.SetInt(Started+".Step",wanted);selva.DiagnosticZone(wanted);if(wanted==2||wanted==4)selva.DiagnosticBoss();if(wanted==1&&selva.Vines.Count>0)selva.DiagnosticGrab(0);}
                    if(age%4>2)CaptureOnce("Selva_"+wanted,wanted);
                }
                if(age>(stage==0?1:7)&&!SessionState.GetBool(Started+".Captured",false))
                {SessionState.SetBool(Started+".Captured",true);Capture(scenes[stage]);ScreenCapture.CaptureScreenshot(Path.GetFullPath("Artifacts/VisualRefactor/hud_"+scenes[stage]+".png"));}
                if(age<sampleLengths[stage])return;
                File.AppendAllText("Artifacts/VisualRefactor/play_visual_tour.txt","PLAY sampled "+scenes[stage]+" for "+sampleLengths[stage]+" seconds; state="+(costa?costa.CurrentState:sierra?sierra.Status:selva?selva.Status:"NO DIRECTOR")+"\n");
                EditorApplication.ExitPlaymode();SessionState.SetInt(Stage,stage+1);return;
            }
            if(EditorApplication.isPlayingOrWillChangePlaymode)return;
            if(SessionState.GetFloat(Started,0)==0)return;
            if(stage<scenes.Length){Load();return;}
            SessionState.SetBool(Active,false);File.AppendAllText("Artifacts/VisualRefactor/play_visual_tour.txt","Finished. Diagnostic jumps do not certify complete chronological gameplay, transitions or music.\n");
            string previous=SessionState.GetString(Active+".PreviousStart","");EditorSceneManager.playModeStartScene=string.IsNullOrEmpty(previous)?null:AssetDatabase.LoadAssetAtPath<SceneAsset>(previous);
            var snapshot=JsonUtility.FromJson<SceneSnapshot>(SessionState.GetString(Active+".SceneSnapshot","{}"));
            if(snapshot?.scenes!=null&&snapshot.scenes.Length>0)EditorSceneManager.RestoreSceneManagerSetup(snapshot.scenes);
            if(Application.isBatchMode)EditorApplication.Exit(0);
        }
        static void CaptureOnce(string name,int step)
        {
            string key=Started+".Capture."+SessionState.GetInt(Stage,0)+"."+step;
            // Mark against this run's start time, so a later tour captures again.
            float began=SessionState.GetFloat(Started,0);
            if(SessionState.GetFloat(key,-1)==began)return;
            SessionState.SetFloat(key,began);Capture(name);
            ScreenCapture.CaptureScreenshot(Path.GetFullPath("Artifacts/VisualRefactor/hud_"+name+".png"));
            File.AppendAllText("Artifacts/VisualRefactor/play_visual_tour.txt","CAPTURE "+name+" (world + GameView HUD)\n");
        }
        static void Capture(string scene)
        {
            if(SystemInfo.graphicsDeviceType==GraphicsDeviceType.Null)return;
            var camera=Camera.main;if(!camera)camera=Object.FindFirstObjectByType<Camera>();if(!camera)return;
            var target=new RenderTexture(640,360,24);target.Create();
            RenderPipeline.SubmitRenderRequest(camera,new UniversalRenderPipeline.SingleCameraRequest{destination=target});
            var previous=RenderTexture.active;RenderTexture.active=target;
            var image=new Texture2D(640,360,TextureFormat.RGBA32,false);image.ReadPixels(new Rect(0,0,640,360),0,0);image.Apply();
            File.WriteAllBytes("Artifacts/VisualRefactor/play_"+scene+".png",image.EncodeToPNG());
            RenderTexture.active=previous;Object.DestroyImmediate(image);target.Release();Object.DestroyImmediate(target);
        }
    }
}
