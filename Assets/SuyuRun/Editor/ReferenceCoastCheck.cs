using System.IO;
using UnityEditor;
using UnityEngine;

namespace SuyuRun.Editor
{
    [InitializeOnLoad]
    public static class ReferenceCoastCheck
    {
        static double next;
        static ReferenceCoastCheck(){EditorApplication.update+=Poll;}
        static void Poll()
        {
            if(EditorApplication.timeSinceStartup<next||EditorApplication.isCompiling)return;
            next=EditorApplication.timeSinceStartup+.25;
            if(!SessionState.GetBool("SuyuRun.ReferenceCheck",false)||!EditorApplication.isPlaying)return;
            var runner=Object.FindFirstObjectByType<RunnerPrototype>();if(!runner)return;
            runner.AutomatedSmokeTest=true;
            Directory.CreateDirectory("Artifacts/ReferenceCoast");
            foreach(int t in new[]{0,20,60,120,175})
            {
                string key="SuyuRun.ReferenceShot."+t;
                if(runner.CurrentState=="Running"&&runner.ProgressSeconds>=t&&!SessionState.GetBool(key,false))
                {
                    ScreenCapture.CaptureScreenshot(Path.GetFullPath("Artifacts/ReferenceCoast/play-"+t+".png"));
                    SessionState.SetBool(key,true);
                }
            }
            string reason=(string)typeof(RunnerPrototype).GetField("lastFailure",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).GetValue(runner);
            File.WriteAllText("Artifacts/ReferenceCoast/state.txt",runner.SmokeReport+"Failure="+reason);
            if(runner.CurrentState=="Dead"||runner.CurrentState=="Complete")
            {
                File.WriteAllText("Artifacts/ReferenceCoast/result.txt",runner.SmokeReport+"Failure="+reason);
                SessionState.SetBool("SuyuRun.ReferenceCheck",false);
                EditorApplication.isPlaying=false;
            }
        }
        [MenuItem("SuyuRun/QA/Probar Costa de referencia (sin EXE)")]
        public static void Run()
        {
            PrototypeSetup.Open();
            if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().path!="Assets/SuyuRun/Scenes/Level_Costa.unity")return;
            foreach(int t in new[]{0,20,60,120,175})SessionState.SetBool("SuyuRun.ReferenceShot."+t,false);
            SessionState.SetBool("SuyuRun.ReferenceCheck",true);
            var gameType=typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");
            var game=EditorWindow.GetWindow(gameType);game.Focus();
            EditorApplication.isPlaying=true;
        }
    }
}
