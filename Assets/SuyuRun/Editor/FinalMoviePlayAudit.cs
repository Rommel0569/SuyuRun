using System;
using System.Collections;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace SuyuRun.Editor
{
    [InitializeOnLoad]
    public static class FinalMoviePlayAudit
    {
        const string Key="SuyuRun.FinalMovieAudit";
        const string InteractiveKey="SuyuRun.FinalMovieInteractiveAudit";
        const string IsolatedKey="SuyuRun.FinalMovieIsolatedAudit";
        const string SceneKey="SuyuRun.FinalMoviePreviousScene";
        static FinalMoviePlayAudit(){EditorApplication.playModeStateChanged+=Changed;}
        public static void Run()
        {
            FinalMovieScoreBuild.Build();
            foreach(string name in RunnerFinalMovie.RequiredAssets)new FinalMovieArt().Load(name);
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            SessionState.SetBool(Key,true);
            EditorApplication.ExecuteMenuItem("Window/General/Game");
            var assembly=typeof(UnityEditor.Editor).Assembly;
            var game=EditorWindow.GetWindow(assembly.GetType("UnityEditor.GameView"));game.maximized=true;
            // Fixed render size makes captures independent of the user's dock layout.
            var sizesType=assembly.GetType("UnityEditor.GameViewSizes");
            var singleton=typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
            var sizes=singleton.GetProperty("instance").GetValue(null,null);
            var groupType=assembly.GetType("UnityEditor.GameViewSizeGroupType");
            var group=sizesType.GetMethod("GetGroup").Invoke(sizes,new object[]{Enum.Parse(groupType,"Standalone")});
            var sizeType=assembly.GetType("UnityEditor.GameViewSize");var modeType=assembly.GetType("UnityEditor.GameViewSizeType");
            var constructor=sizeType.GetConstructor(System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic,null,new[]{modeType,typeof(int),typeof(int),typeof(string)},null);
            var size=constructor.Invoke(new object[]{Enum.Parse(modeType,"FixedResolution"),1280,720,"SUYURUN Film QA"});
            group.GetType().GetMethod("AddCustomSize").Invoke(group,new[]{size});
            int count=(int)group.GetType().GetMethod("GetTotalCount").Invoke(group,null);
            game.GetType().GetProperty("selectedSizeIndex",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic).SetValue(game,count-1,null);
            EditorApplication.isPlaying=true;
        }
        static void Changed(PlayModeStateChange state)
        {
            if(state==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(InteractiveKey,false))StartInteractive();
            if(!SessionState.GetBool(Key,false))return;
            if(state==PlayModeStateChange.EnteredPlayMode)
            {
                var camera=new GameObject("FilmAuditCamera").AddComponent<Camera>();camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.black;camera.gameObject.AddComponent<AudioListener>();
                var host=new GameObject("FilmAudit");RunnerFinalMovie.Begin(host);host.AddComponent<FinalMovieAuditCapture>();
            }
            if(state==PlayModeStateChange.EnteredEditMode)
            {
                SessionState.SetBool(Key,false);
                if(SessionState.GetBool(IsolatedKey,false))
                {
                    SessionState.SetBool(IsolatedKey,false);
                    string path=SessionState.GetString(SceneKey,"");
                    if(!string.IsNullOrEmpty(path))EditorSceneManager.OpenScene(path);
                }
                else EditorApplication.Exit(0);
            }
        }
        [MenuItem("SUYURUN/Final/Verificar pelicula aislada y restaurar escena %#&n")]
        public static void Isolated()
        {
            if(EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying=false;EditorApplication.update+=WaitForEdit;return;
            }
            var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if(scene.isDirty||string.IsNullOrEmpty(scene.path))throw new Exception("Save the current scene before isolated film verification");
            SessionState.SetString(SceneKey,scene.path);SessionState.SetBool(IsolatedKey,true);Run();
        }
        static void WaitForEdit()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)return;
            EditorApplication.update-=WaitForEdit;Isolated();
        }
        [MenuItem("SUYURUN/Final/Verificar final V4 (sin cerrar Unity) %#&v")]
        public static void Interactive()
        {
            FinalMovieScoreBuild.Build();
            if(EditorApplication.isPlaying){StartInteractive();return;}
            SessionState.SetBool(InteractiveKey,true);EditorApplication.isPlaying=true;
        }
        static void StartInteractive()
        {
            SessionState.SetBool(InteractiveKey,false);
            foreach(string name in RunnerFinalMovie.RequiredAssets)new FinalMovieArt().Load(name);
            var old=UnityEngine.Object.FindFirstObjectByType<RunnerFinalMovie>();
            if(old)UnityEngine.Object.DestroyImmediate(old.gameObject);
            var host=new GameObject("FilmV4InteractiveAudit");
            if(!UnityEngine.Object.FindFirstObjectByType<AudioListener>())host.AddComponent<AudioListener>();
            RunnerFinalMovie.Begin(host);host.AddComponent<FinalMovieAuditCapture>().StopPlayAtEnd=false;
            EditorApplication.ExecuteMenuItem("Window/General/Game");
        }
    }
    public sealed class FinalMovieAuditCapture:MonoBehaviour
    {
        public bool StopPlayAtEnd=true;
        IEnumerator Start()
        {
            string dir=Path.GetFullPath("Artifacts/FinalMovieV4/PlayAudit");Directory.CreateDirectory(dir);
            int errors=0;Application.LogCallback log=(message,trace,type)=>{if(type==LogType.Error||type==LogType.Exception)errors++;};Application.logMessageReceived+=log;
            var film=GetComponent<RunnerFinalMovie>();float[] times={.5f,1.5f,2.4f,2.7f,3.2f,3.7f,4.2f,4.7f,5.1f,5.5f,6.1f,6.8f,7.3f,7.8f,8.3f,8.8f,9.5f,10.1f,10.6f,11.1f,11.6f,12.1f,12.6f,13.1f,13.6f,14.2f,15f,16f,17.5f,18.5f,19.5f,21f,24f,27f,29f};
            if(!film||!RunnerFinalMovie.Active)
            {
                Application.logMessageReceived-=log;
                File.WriteAllText(Path.Combine(dir,"result.txt"),"FAIL: movie did not initialize. No successful Play verification.");
                yield break;
            }
            float started=Time.realtimeSinceStartup;
            Vector2 velocity=RunnerFinalMovie.ReleaseVelocity();
            Vector2 numeric=(RunnerFinalMovie.SwingHand(1)-RunnerFinalMovie.SwingHand(.9999f))/.000155f;
            if(velocity.x<=0||(numeric-velocity).magnitude>.1f)throw new Exception("Swing release tangent mismatch");
            Vector2 start=new Vector2(140,110),p=RunnerFinalMovie.Ballistic(start,velocity,.0001f);
            if(((p-start)/.0001f-velocity).magnitude>.2f)throw new Exception("Ballistic launch discontinuity");
            foreach(float time in times)
            {
                while(film&&film.Age<time)yield return null;
                yield return new WaitForEndOfFrame();
                var image=ScreenCapture.CaptureScreenshotAsTexture();
                File.WriteAllBytes(Path.Combine(dir,"frame_"+time.ToString("F1",System.Globalization.CultureInfo.InvariantCulture)+".png"),image.EncodeToPNG());Destroy(image);
            }
            while(RunnerFinalMovie.Active)yield return null;
            Application.logMessageReceived-=log;
            float elapsed=Time.realtimeSinceStartup-started;
            File.WriteAllText(Path.Combine(dir,"result.txt"),(elapsed>=28&&errors==0?"PASS":"FAIL")+" Play finished. Runtime errors="+errors+". Observed seconds="+elapsed.ToString("F2")+". Captures="+times.Length+". Release tangent and ballistic continuity checked. Screenshots require visual review.");
            if(StopPlayAtEnd)EditorApplication.isPlaying=false;
            else Destroy(gameObject);
        }
    }
}
