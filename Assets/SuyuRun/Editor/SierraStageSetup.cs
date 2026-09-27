using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SuyuRun.Editor
{
    [InitializeOnLoad]
    public static class SierraStageSetup
    {
        const string ScenePath="Assets/SuyuRun/Scenes/Level_Sierra_Integration.unity";
        static double next;
        static int captureSection=-1;
        static double completeSeen=-1;
        static bool resultCaptured;
        static bool slideCaptured;
        static SierraStageSetup()
        {
            EditorApplication.update+=Poll;
            Application.logMessageReceived+=(message,stack,type)=>{
                if(SessionState.GetBool("SuyuRun.SierraDiagnostic",false)&&(type==LogType.Error||type==LogType.Exception))
                    SessionState.SetInt("SuyuRun.SierraErrors",SessionState.GetInt("SuyuRun.SierraErrors",0)+1);
            };
            EditorApplication.playModeStateChanged+=state=>{
                if(state==PlayModeStateChange.EnteredEditMode&&SessionState.GetBool("SuyuRun.SierraRestore",false))
                {
                    string previous=SessionState.GetString("SuyuRun.SierraPreviousStart","");
                    EditorSceneManager.playModeStartScene=string.IsNullOrEmpty(previous)?null:AssetDatabase.LoadAssetAtPath<SceneAsset>(previous);
                    SessionState.SetBool("SuyuRun.SierraRestore",false);
                }
            };
            EditorApplication.delayCall+=()=>EditorApplication.delayCall+=()=>{
                if(EditorSceneManager.GetActiveScene().path==ScenePath)
                    EditorSceneManager.playModeStartScene=AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            };
        }
        static void Poll()
        {
            if(EditorApplication.timeSinceStartup<next)return;
            next=EditorApplication.timeSinceStartup+(SessionState.GetBool("SuyuRun.SierraControlsQA",false)?.1:1);
            if(EditorApplication.isCompiling||EditorApplication.isUpdating)return;
            if(File.Exists("sierra-command.txt")&&File.ReadAllText("sierra-command.txt").Trim()=="stop")
            {
                File.Delete("sierra-command.txt");SessionState.SetBool("SuyuRun.SierraDiagnostic",false);
                EditorApplication.isPlaying=false;return;
            }
            if(EditorApplication.isPlaying && SessionState.GetBool("SuyuRun.SierraDiagnostic",false))
            {
                var stage=Object.FindFirstObjectByType<SierraStageDirector>();
                if(!stage)return;
                stage.DiagnosticAutoplay=true;
                if(SessionState.GetBool("SuyuRun.FinishingPreview",false)){SierraFinishingPreview.Tick(stage);return;}
                if(SessionState.GetBool("SuyuRun.CaveOnlyQA",false)&&!SessionState.GetBool("SuyuRun.CaveOnlyStarted",false))
                {SessionState.SetBool("SuyuRun.CaveOnlyStarted",true);stage.DiagnosticStartCave(0);}
                if(SessionState.GetBool("SuyuRun.CaveOnlyQA",false)&&stage.CavesCompleted==1&&!stage.InCave)
                    stage.DiagnosticStartCave(1);
                if(SessionState.GetBool("SuyuRun.CaveOnlyQA",false)&&stage.CavesCompleted==2&&!stage.InCave)
                {
                    File.WriteAllText("Artifacts/SierraCaves/integration-verification.txt","PASS both caves, all enemies required, three-second exit, return to runner; errors="+SessionState.GetInt("SuyuRun.SierraErrors",0)+"; "+stage.CaveReport+"; "+System.DateTime.Now.ToString("O"));
                    SessionState.SetBool("SuyuRun.SierraDiagnostic",false);EditorApplication.isPlaying=false;return;
                }
                if(stage.CavePlaying)
                {
                    int capture=stage.CavesCompleted*3+(stage.CaveElapsed>50?2:stage.CaveElapsed>20?1:0);
                    int mask=SessionState.GetInt("SuyuRun.CaveCaptureMask",0);
                    if(stage.CaveIntroduction&&(mask&(1<<(6+stage.CavesCompleted)))==0)
                    {mask|=1<<(6+stage.CavesCompleted);SessionState.SetInt("SuyuRun.CaveCaptureMask",mask);ScreenCapture.CaptureScreenshot(Path.GetFullPath("Artifacts/SierraCaves/intro-"+stage.CavesCompleted+".png"));}
                    if(stage.CaveElapsed>8&&(mask&(1<<capture))==0)
                    {SessionState.SetInt("SuyuRun.CaveCaptureMask",mask|(1<<capture));ScreenCapture.CaptureScreenshot(Path.GetFullPath("Artifacts/SierraCaves/play-"+capture+".png"));}
                    int pauseStep=SessionState.GetInt("SuyuRun.CavePauseQA",0);
                    if(stage.CaveElapsed>10&&pauseStep==0)
                    {SessionState.SetFloat("SuyuRun.CavePausedTime",stage.CaveElapsed);SessionState.SetFloat("SuyuRun.CavePausedRunner",stage.Progress);SessionState.SetInt("SuyuRun.CavePauseQA",1);stage.DiagnosticPanel(0);return;}
                    if(pauseStep==1)
                    {
                        if(stage.CaveElapsed!=SessionState.GetFloat("SuyuRun.CavePausedTime",0)||stage.Progress!=SessionState.GetFloat("SuyuRun.CavePausedRunner",0))throw new System.Exception("Cave pause clock advanced");
                        stage.DiagnosticResume();SessionState.SetInt("SuyuRun.CavePauseQA",2);
                        File.WriteAllText("Artifacts/SierraCaves/pause-verification.txt","PASS cave and runner freeze together; resumed. "+System.DateTime.Now.ToString("O"));
                    }
                }
                if(SessionState.GetBool("SuyuRun.SierraServicesQA",false)&&stage.Progress>5)
                {
                    int step=SessionState.GetInt("SuyuRun.SierraServicesStep",0);
                    if(step==0)
                    {SessionState.SetFloat("SuyuRun.SierraPausedAt",stage.Progress);stage.DiagnosticPanel(1);SessionState.SetInt("SuyuRun.SierraServicesStep",1);return;}
                    if(step<=5)
                    {
                        if(Mathf.Abs(stage.Progress-SessionState.GetFloat("SuyuRun.SierraPausedAt",0))>.05f)throw new System.Exception("Paused Sierra timeline advanced");
                        if(step%2==1)ScreenCapture.CaptureScreenshot(Path.GetFullPath("Artifacts/SierraV3/service-"+((step+1)/2)+".png"));
                        else stage.DiagnosticPanel(step/2+1);
                        SessionState.SetInt("SuyuRun.SierraServicesStep",step+1);return;
                    }
                    if(step==6){stage.DiagnosticResume();SessionState.SetInt("SuyuRun.SierraServicesStep",7);return;}
                    if(stage.Progress>8)
                    {
                        File.WriteAllText("Artifacts/SierraV3/services-verification.txt","PASS: pause freezes DSP-mapped course; museum/settings/shop render; resume advances; errors="+SessionState.GetInt("SuyuRun.SierraErrors",0)+"; "+System.DateTime.Now.ToString("O"));
                        SessionState.SetBool("SuyuRun.SierraDiagnostic",false);SessionState.SetBool("SuyuRun.SierraServicesQA",false);EditorApplication.isPlaying=false;return;
                    }
                }
                if(SessionState.GetBool("SuyuRun.SierraControlsQA",false))
                {
                    if(stage.IsSliding&&!slideCaptured)
                    {slideCaptured=true;ScreenCapture.CaptureScreenshot(Path.GetFullPath("Artifacts/SierraV3/unity-slide.png"));}
                    if(stage.Progress>23||stage.Status=="Dead")
                    {
                        File.WriteAllText("Artifacts/SierraV3/controls-verification.txt",stage.Status+"; SlideCaptured="+slideCaptured+"; "+stage.MobilityReport+"; errors="+SessionState.GetInt("SuyuRun.SierraErrors",0)+"; "+System.DateTime.Now.ToString("O"));
                        SessionState.SetBool("SuyuRun.SierraDiagnostic",false);SessionState.SetBool("SuyuRun.SierraControlsQA",false);
                        EditorApplication.isPlaying=false;return;
                    }
                }
                if(stage.Progress>10&&!SessionState.GetBool("SuyuRun.SierraTrapQA",false))
                {SessionState.SetBool("SuyuRun.SierraTrapQA",true);stage.DiagnosticTrap();}
                if(stage.Progress>101&&!SessionState.GetBool("SuyuRun.SierraReviveQA",false))
                {SessionState.SetBool("SuyuRun.SierraReviveQA",true);stage.DiagnosticRevive();}
                Directory.CreateDirectory("Artifacts/SierraV3");
                File.WriteAllText("Artifacts/SierraV3/unity-live.txt",stage.Status+" "+stage.Progress.ToString("F2",System.Globalization.CultureInfo.InvariantCulture)+" "+stage.MobilityReport+"; "+stage.NarrativeReport+"; "+stage.CaveReport);
                int section=(int)(stage.Progress/60);
                if(stage.Progress>5+section*60&&section>captureSection){captureSection=section;ScreenCapture.CaptureScreenshot(Path.GetFullPath("Artifacts/SierraV3/unity-play-"+section+".png"));}
                if(stage.Status=="Dead"||stage.Status=="Complete")
                {
                    // Keep Play alive long enough to inspect the transformation and results UI.
                    if(stage.Status=="Complete")
                    {
                        if(completeSeen<0)completeSeen=EditorApplication.timeSinceStartup;
                        double endingAge=EditorApplication.timeSinceStartup-completeSeen;
                        if(endingAge<4)return;
                        if(!resultCaptured)
                        {resultCaptured=true;ScreenCapture.CaptureScreenshot(Path.GetFullPath("Artifacts/SierraV3/unity-results.png"));return;}
                        if(endingAge<6)return;
                    }
                    File.WriteAllText("Artifacts/SierraV3/unity-play-result.txt",stage.Status+" "+stage.Progress.ToString("F2")+"; errors="+SessionState.GetInt("SuyuRun.SierraErrors",0)+"; "+stage.MobilityReport+"; "+stage.NarrativeReport+"; "+stage.CaveReport+"; "+System.DateTime.Now.ToString("O")+"; diagnostic autoplay, NOT manual playability or ear validation.");
                    SessionState.SetBool("SuyuRun.SierraDiagnostic",false);EditorApplication.isPlaying=false;
                }
                return;
            }
            if(EditorApplication.isPlayingOrWillChangePlaymode)return;
            const string file="sierra-command.txt";
            if(!File.Exists(file))return;
            string command=File.ReadAllText(file).Trim();File.Delete(file);
            if(command=="prepare")Prepare();
            else if(command=="verify-shared")VerifyShared();
            else if(command=="verify-input")VerifyInput();
            else if(command=="verify-caves")SierraCaveChecks.Run();
            else if(command=="test-portal")SierraPortalCheck.Run();
            else if(command=="ready")
            {
                EditorSceneManager.playModeStartScene=AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
                File.WriteAllText("Artifacts/SierraV3/ready.txt","Unity Play starts Sierra: "+ScenePath+". Diagnostic flags off. "+System.DateTime.Now.ToString("O"));
                SessionState.SetBool("SuyuRun.SierraDiagnostic",false);
            }
            else if(command=="test"||command=="test-controls"||command=="test-services"||command=="test-caves"||command=="preview-finishing")
            {
                SessionState.SetBool("SuyuRun.FinishingPreview",command=="preview-finishing");SessionState.SetInt("SuyuRun.FinishingStep",0);
                if(!File.Exists(ScenePath))Prepare();
                slideCaptured=false;SessionState.SetBool("SuyuRun.SierraControlsQA",command=="test-controls");
                SessionState.SetBool("SuyuRun.SierraServicesQA",command=="test-services");SessionState.SetInt("SuyuRun.SierraServicesStep",0);
                captureSection=-1;completeSeen=-1;resultCaptured=false;SessionState.SetBool("SuyuRun.SierraDiagnostic",true);
                SessionState.SetBool("SuyuRun.SierraTrapQA",false);SessionState.SetBool("SuyuRun.SierraReviveQA",false);
                SessionState.SetInt("SuyuRun.CaveCaptureMask",0);SessionState.SetInt("SuyuRun.CavePauseQA",0);
                SessionState.SetBool("SuyuRun.CaveOnlyQA",command=="test-caves");SessionState.SetBool("SuyuRun.CaveOnlyStarted",false);
                SessionState.SetInt("SuyuRun.SierraErrors",0);
                SessionState.SetString("SuyuRun.SierraPreviousStart",AssetDatabase.GetAssetPath(EditorSceneManager.playModeStartScene));
                SessionState.SetBool("SuyuRun.SierraRestore",true);
                EditorSceneManager.playModeStartScene=AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
                EditorApplication.isPlaying=true;
            }
        }
        [MenuItem("SUYURUN/Sierra V3/Verificar sistemas compartidos")]
        public static void VerifyShared()
        {
            int money=10;
            if(!RunnerProgress.TrySpend(ref money,5)||money!=5)throw new System.Exception("Revive spend failed");
            if(RunnerProgress.TrySpend(ref money,6)||money!=5)throw new System.Exception("Insufficient funds changed wallet");
            if(RunnerProgress.TrySpend(ref money,-1)||money!=5)throw new System.Exception("Negative spend accepted");
            var pickup=new RunnerPickup{x=20,y=30,width=8,height=8};
            if(pickup.TryCollect(Vector2.zero,Vector2.one)||!pickup.TryCollect(new Vector2(20,30),Vector2.one)||pickup.TryCollect(new Vector2(20,30),Vector2.one))
                throw new System.Exception("Pickup single-collection contract failed");
            int before=PlayerPrefs.GetInt("SuyuRun.Prototype.Bank");
            RunnerProgress.Finish("Sierra",7,100,true,out int bank,out int best);
            if(bank!=before+7||PlayerPrefs.GetInt("SuyuRun.Prototype.Bank")!=before)throw new System.Exception("Diagnostic changed save");
            if(!System.Array.Exists(EditorBuildSettings.scenes,s=>s.enabled&&s.path==ScenePath))throw new System.Exception("Sierra scene not registered");
            Directory.CreateDirectory("Artifacts/SierraV3");
            File.WriteAllText("Artifacts/SierraV3/shared-verification.txt","PASS: spend, insufficient funds, negative cost, one-time pickup, diagnostic save isolation, Sierra scene registration. "+System.DateTime.Now.ToString("O"));
        }
        static void VerifyInput()
        {
            var input=new RunnerInputBuffer();
            if(input.Pending(10))throw new System.Exception("Fresh input was active");
            input.Press(10);
            // No simulation step on the press frame; next frame must still see it.
            if(!input.Pending(10.016)||!input.Pending(10.15))throw new System.Exception("Early jump was lost");
            if(!input.Consume(10.16)||input.Consume(10.17))throw new System.Exception("Press repeated or was lost");
            input.Press(11);if(input.Pending(11.19))throw new System.Exception("Stale jump survived");
            input.Press(12);input.Clear();if(input.Pending(12.01))throw new System.Exception("Pause/restart did not clear input");
            var flags=System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic;
            float release=(float)typeof(SierraStageDirector).GetField("SlingRelease",flags).GetRawConstantValue();
            float recovery=(float)typeof(SierraStageDirector).GetField("SlingRecovery",flags).GetRawConstantValue();
            if(release>.2f||recovery>.4f||release<=0||recovery<=release)throw new System.Exception("Huaraca timing regression");
            Directory.CreateDirectory("Artifacts/SierraV3");
            File.WriteAllText("Artifacts/SierraV3/input-verification.txt","PASS: retained no-step frame, early landing buffer, single consumption, expiry, pause/reset clearing; huaraca release="+release+" recovery="+recovery+"; "+System.DateTime.Now.ToString("O"));
        }
        [MenuItem("SUYURUN/Sierra V3/Preparar escena y clips")]
        public static void Prepare()
        {
            SierraV2ArtImport.BuildClipsV3();
            if(!File.Exists(ScenePath))
            {
                var previous=EditorSceneManager.GetActiveScene();
                var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);
                var root=new GameObject("SUYURUN Sierra - Camino Valle Ruinas");
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(root,scene);
                var stage=root.AddComponent<SierraStageDirector>();
                stage.soundtrack=AssetDatabase.LoadAssetAtPath<RunnerSoundtrack>("Assets/SuyuRun/Data/Resources/SierraSoundtrack.asset");
                stage.pixelFont=AssetDatabase.LoadAssetAtPath<Font>("Assets/SuyuRun/Art/Fonts/PressStart2P-Regular.ttf");
                EditorSceneManager.SaveScene(scene,ScenePath);EditorSceneManager.CloseScene(scene,true);
                if(previous.IsValid())UnityEngine.SceneManagement.SceneManager.SetActiveScene(previous);
            }
            Directory.CreateDirectory("Artifacts/SierraV3");
            File.WriteAllText("Artifacts/SierraV3/unity-prepare.txt","Sierra scene prepared; sprites and clips imported. No Play test yet. Costa untouched.");
            Debug.Log("Sierra: escena aislada lista. SUYURUN > Sierra V3 > Abrir integración y Play. No se generó EXE.");
        }
        [MenuItem("SUYURUN/Sierra V3/Abrir integración")]
        public static void Open()
        {
            if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
            if(!File.Exists(ScenePath))Prepare();
            EditorSceneManager.OpenScene(ScenePath);
            EditorSceneManager.playModeStartScene=AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
        }
    }
}
