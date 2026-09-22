using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEditor.Build.Reporting;
using System.IO;

namespace SuyuRun.Editor
{
    [InitializeOnLoad]
    public static class PrototypeSetup
    {
        const string ScenePath="Assets/SuyuRun/Scenes/Level_Costa.unity";
        const string LegacyPath="Assets/SuyuRun/Scenes/Prototype_Legacy.unity";
        static double nextPoll;
        static bool smoke;
        static PrototypeSetup(){EditorApplication.delayCall+=ConfigureStartup;EditorApplication.update+=Poll;}
        static void ConfigureStartup()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)return;
            EnsureScene();
            const string dataPath="Assets/SuyuRun/Data/Resources/CostaPrototype.asset";
            if(!AssetDatabase.LoadAssetAtPath<RunnerCourse>(dataPath))
            {Directory.CreateDirectory("Assets/SuyuRun/Data/Resources");AssetDatabase.Refresh();AssetDatabase.CreateAsset(RunnerCourse.Prototype(),dataPath);AssetDatabase.SaveAssets();}
            const string verticalPath="Assets/SuyuRun/Data/Resources/CostaVertical.asset";
            if(!AssetDatabase.LoadAssetAtPath<RunnerCourse>(verticalPath)){AssetDatabase.CreateAsset(RunnerCourse.VerticalPrototype(),verticalPath);AssetDatabase.SaveAssets();}
            EnsureLegacyScene();
            var startup=AssetDatabase.LoadAssetAtPath<SceneAsset>(EditorSceneManager.GetActiveScene().name=="Prototype_Legacy"?LegacyPath:ScenePath);
            if(startup!=null)EditorSceneManager.playModeStartScene=startup;
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true),new EditorBuildSettingsScene(LegacyPath,true)};
        }
        static void Poll()
        {
            if(EditorApplication.timeSinceStartup<nextPoll)return;
            nextPoll=EditorApplication.timeSinceStartup+1;
            if(EditorApplication.isCompiling||EditorApplication.isUpdating)return;
            if(File.Exists("prototype-command.txt")&&File.ReadAllText("prototype-command.txt").Trim()=="stop")
            {File.Delete("prototype-command.txt");SessionState.SetBool("SuyuRun.Smoke",false);EditorApplication.isPlaying=false;return;}
            if(EditorApplication.isPlaying && SessionState.GetBool("SuyuRun.Smoke",false))
            {
                var runner=UnityEngine.Object.FindFirstObjectByType<RunnerPrototype>();
                if(!runner)return;
                Directory.CreateDirectory("Artifacts");
                File.WriteAllText("Artifacts/live-state.txt",runner.CurrentState+" at "+runner.ProgressSeconds+" seconds; "+System.DateTime.Now.ToString("O"));
                runner.AutomatedSmokeTest=true;
                if(runner.ProgressSeconds>20&&!smoke){smoke=true;Directory.CreateDirectory("Artifacts");ScreenCapture.CaptureScreenshot(Path.GetFullPath(runner.IsLegacy?"Artifacts/legacy-play.png":"Artifacts/costa-presentation.png"));}
                if(runner.CurrentState=="Dead"||runner.CurrentState=="Complete")
                {
                    File.WriteAllText(runner.IsLegacy?"Artifacts/smoke-legacy.txt":"Artifacts/smoke-coast.txt",runner.SmokeReport);
                    SessionState.SetBool("SuyuRun.Smoke",false);EditorApplication.isPlaying=false;
                }
                return;
            }
            if(!File.Exists("prototype-command.txt")||EditorApplication.isPlayingOrWillChangePlaymode)return;
            string command=File.ReadAllText("prototype-command.txt").Trim();File.Delete("prototype-command.txt");
            if(command=="smoke"){Open();smoke=false;SessionState.SetBool("SuyuRun.Smoke",true);EditorApplication.isPlaying=true;}
            else if(command=="legacy-smoke"){OpenLegacy();smoke=false;SessionState.SetBool("SuyuRun.Smoke",true);EditorApplication.isPlaying=true;}
            else if(command=="reference-check")ReferenceCoastCheck.Run();
            else if(command=="refresh")AssetDatabase.Refresh();
            else if(command=="build")Build();
            else if(command=="open")Open();
            else if(command=="validate")ValidateRules();
            else if(command=="audio-export")SoundtrackPreparation.ExportSamples();
            else if(command=="audio-apply")SoundtrackPreparation.ApplyAnalysis();
            else if(command=="presentation-setup"){ConfigureStartup();SoundtrackPreparation.RegisterSelectedMusic();}
        }
        [MenuItem("SuyuRun/Preparar prototipo")]
        public static void EnsureScene()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode||File.Exists(ScenePath))return;
            var previous=EditorSceneManager.GetActiveScene();
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);
            UnityEngine.SceneManagement.SceneManager.SetActiveScene(scene);
            new GameObject("SuyuRun Prototype").AddComponent<RunnerPrototype>();
            Directory.CreateDirectory("Assets/SuyuRun/Scenes");
            EditorSceneManager.SaveScene(scene,ScenePath);
            EditorSceneManager.CloseScene(scene,true);
            if(previous.IsValid())UnityEngine.SceneManagement.SceneManager.SetActiveScene(previous);
            AssetDatabase.Refresh();
            Debug.Log("SuyuRun: Costa preparada. Usa SuyuRun > Abrir Costa y pulsa Play.");
        }
        [MenuItem("SuyuRun/Abrir Costa")]
        public static void Open(){EnsureScene();if(EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()){EditorSceneManager.OpenScene(ScenePath);EditorSceneManager.playModeStartScene=AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);}}
        [MenuItem("SuyuRun/Abrir prototipo original")]
        public static void OpenLegacy(){EnsureLegacyScene();if(EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()){EditorSceneManager.OpenScene(LegacyPath);EditorSceneManager.playModeStartScene=AssetDatabase.LoadAssetAtPath<SceneAsset>(LegacyPath);}}
        static void EnsureLegacyScene()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode||File.Exists(LegacyPath))return;
            var previous=EditorSceneManager.GetActiveScene();
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);
            UnityEngine.SceneManagement.SceneManager.SetActiveScene(scene);
            var runner=new GameObject("Original hills prototype - preserved").AddComponent<RunnerPrototype>();
            runner.ConfigureLegacy(AssetDatabase.LoadAssetAtPath<RunnerCourse>("Assets/SuyuRun/Data/Resources/CostaPrototype.asset"));
            EditorSceneManager.SaveScene(scene,LegacyPath);EditorSceneManager.CloseScene(scene,true);
            if(previous.IsValid())UnityEngine.SceneManagement.SceneManager.SetActiveScene(previous);
            AssetDatabase.Refresh();
        }
        [MenuItem("SuyuRun/Build Windows prototipo")]
        public static void Build()
        {
            EnsureScene();Directory.CreateDirectory("Builds/Prototype");
            ValidateRules();
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{ScenePath,LegacyPath},locationPathName="Builds/Prototype/SuyuRun.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});
            Debug.Log("SuyuRun build: "+report.summary.result);
            if(report.summary.result!=BuildResult.Succeeded)throw new System.Exception("Prototype build failed");
        }
        [MenuItem("SuyuRun/Validar datos del recorrido")]
        public static void ValidateRules()
        {
            var c=RunnerCourse.Prototype();c.ValidateCourse();
            if(c.encounters.Length!=14)throw new System.Exception("Prototype encounter count changed unexpectedly.");
            c.speed=0;bool rejected=false;
            try{c.ValidateCourse();}catch(System.InvalidOperationException){rejected=true;}
            if(!rejected)throw new System.Exception("Zero speed was accepted.");
            c.speed=7;c.encounters[1]=c.encounters[0];rejected=false;
            try{c.ValidateCourse();}catch(System.InvalidOperationException){rejected=true;}
            if(!rejected)throw new System.Exception("Overlapping encounters were accepted.");
            Object.DestroyImmediate(c);
            c=RunnerCourse.VerticalPrototype();c.ValidateCourse();
            Check(!RunnerTerrain.HasFloor(c,124),"Gap must remove floor");
            Check(RunnerTerrain.HasFloor(c,130),"Floor must resume after gap");
            Check(RunnerTerrain.Landing(c,65,0,-2,out float top)&&Mathf.Abs(top+1.6f)<.01f,"Descending platform landing");
            Check(!RunnerTerrain.Landing(c,65,-2,0,out _),"Cannot land while rising");
            Check(RunnerTerrain.HitsWall(c,59,60,-2.6f),"Platform side collision");
            Check(!RunnerTerrain.SafeCheckpoint(c,124),"Cannot respawn in gap");
            Check(RunnerTerrain.SafeCheckpoint(c,112),"First checkpoint safe");
            Object.DestroyImmediate(c);
            var coast=AssetDatabase.LoadAssetAtPath<RunnerCourse>("Assets/SuyuRun/Data/Resources/CostaAlcatraz.asset");
            Check(coast!=null,"Recorded coast course exists");coast.ValidateCourse();
            var tracks=AssetDatabase.LoadAssetAtPath<SelectedMusic>("Assets/SuyuRun/Data/Resources/SelectedMusic.asset");
            Check(tracks&&tracks.costa&&tracks.sierra&&tracks.selva&&tracks.inicio,"Four selected tracks imported");
            Check(File.Exists(ScenePath)&&File.Exists(LegacyPath),"Modern and legacy scenes exist");
            Check(CoastalCulture.Entries.Length==4,"Four coastal museum entries");
            Directory.CreateDirectory("Artifacts");File.WriteAllText("Artifacts/course-validation.txt","PASS: course timing; invalid speed; duplicate beats; vertical course; gap removes floor; floor resumes; descending landing; no upward landing; side collision; unsafe checkpoint rejected; safe checkpoint accepted; recorded coast validates; four selected tracks; two scenes; four museum entries.");
        }
        static void Check(bool condition,string reason){if(!condition)throw new System.Exception(reason);}
    }
}
