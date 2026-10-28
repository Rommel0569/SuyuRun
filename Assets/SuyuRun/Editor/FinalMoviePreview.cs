using UnityEditor;
using UnityEngine;
namespace SuyuRun.Editor
{
    [InitializeOnLoad]
    public static class FinalMoviePreview
    {
        const string Key="SuyuRun.FinalMovieInteractivePreview";
        static FinalMoviePreview(){EditorApplication.playModeStateChanged+=Changed;}
        [MenuItem("SUYURUN/Final/Previsualizar pelicula (Play) %#&f")]
        public static void Preview()
        {
            if(!EditorApplication.isPlaying)FinalMovieScoreBuild.Build();
            SessionState.SetBool(Key,true);
            if(!EditorApplication.isPlaying){EditorApplication.isPlaying=true;return;}
            Open();
        }
        static void Changed(PlayModeStateChange state)
        {
            if(state==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Key,false))Open();
        }
        static void Open()
        {
            SessionState.SetBool(Key,false);
            if(!RunnerFinalMovie.Active)
            {
                var host=new GameObject("FinalMovieInteractivePreview");
                RunnerFinalMovie.Begin(host);
                host.GetComponent<RunnerFinalMovie>().KeepPreviewOpen();
                if(!Object.FindFirstObjectByType<AudioListener>())host.AddComponent<AudioListener>();
            }
            var type=typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameView");
            var window=ScriptableObject.CreateInstance(type) as EditorWindow;
            window.titleContent=new GUIContent("SUYURUN · Final");
            window.position=new Rect(80,80,1100,660);
            window.ShowUtility();window.Focus();
        }
    }
}
