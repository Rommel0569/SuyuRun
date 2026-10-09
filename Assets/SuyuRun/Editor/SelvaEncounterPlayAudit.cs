using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace SuyuRun.Editor
{
    [InitializeOnLoad] public static class SelvaEncounterPlayAudit
    {
        [Serializable] class SavedScene{public string path;public bool loaded,active;}
        [Serializable] class SavedScenes{public SavedScene[] scenes;}
        static double next;static int errors;static string log="";
        static SelvaEncounterPlayAudit(){EditorApplication.update+=Tick;Application.logMessageReceived+=(m,s,t)=>{if(SessionState.GetBool("Selva.EncounterPlay",false)&&(t==LogType.Error||t==LogType.Exception)){errors++;log+=m+"\n";}};}
        [MenuItem("SUYURUN/Selva/Verificar nuevos encuentros en Play")]
        public static void Run(){
            if(EditorApplication.isPlayingOrWillChangePlaymode)return;
            for(int i=0;i<SceneManager.sceneCount;i++)if(SceneManager.GetSceneAt(i).isDirty){File.WriteAllText("Artifacts/SelvaV2/encounters-play.txt","PENDING: guarda la escena actual antes de esta prueba.");return;}
            SessionState.SetString("Selva.EncounterScenes",JsonUtility.ToJson(new SavedScenes{scenes=EditorSceneManager.GetSceneManagerSetup().Select(s=>new SavedScene{path=s.path,loaded=s.isLoaded,active=s.isActive}).ToArray()}));
            SessionState.SetString("Selva.EncounterStart",AssetDatabase.GetAssetPath(EditorSceneManager.playModeStartScene));
            EditorSceneManager.OpenScene(SelvaStageSetup.ScenePath);
            EditorSceneManager.playModeStartScene=AssetDatabase.LoadAssetAtPath<SceneAsset>(SelvaStageSetup.ScenePath);
            SessionState.SetBool("Selva.EncounterPlay",true);SessionState.SetInt("Selva.EncounterStep",0);EditorApplication.isPlaying=true;
        }
        static void Tick(){
            if(EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.timeSinceStartup<next)return;
            next=EditorApplication.timeSinceStartup+1.2;
            if(!SessionState.GetBool("Selva.EncounterPlay",false)){
                const string request="Artifacts/SelvaV2/encounters-play.request";
                if(File.Exists(request)&&!EditorApplication.isPlayingOrWillChangePlaymode){File.Delete(request);Run();}return;
            }
            if(!EditorApplication.isPlaying){
                if(SessionState.GetInt("Selva.EncounterStep",0)<5)return;
                var saved=JsonUtility.FromJson<SavedScenes>(SessionState.GetString("Selva.EncounterScenes",""));
                EditorSceneManager.RestoreSceneManagerSetup(saved.scenes.Select(s=>new SceneSetup{path=s.path,isLoaded=s.loaded,isActive=s.active}).ToArray());
                EditorSceneManager.playModeStartScene=AssetDatabase.LoadAssetAtPath<SceneAsset>(SessionState.GetString("Selva.EncounterStart",""));
                SessionState.SetBool("Selva.EncounterPlay",false);return;
            }
            var stage=UnityEngine.Object.FindFirstObjectByType<SelvaStageDirector>();if(!stage)return;
            int step=SessionState.GetInt("Selva.EncounterStep",0);
            try{
                if(step==0){stage.DiagnosticAutoplay=true;stage.DiagnosticZone(2);stage.DiagnosticBoss();}
                if(step==1)ScreenCapture.CaptureScreenshot(Path.GetFullPath("Artifacts/SelvaV2/unity-guardian-raices-v3.png"));
                if(step==2){stage.DiagnosticZone(4);stage.DiagnosticBoss();}
                if(step==3)ScreenCapture.CaptureScreenshot(Path.GetFullPath("Artifacts/SelvaV2/unity-chullachaqui-v3.png"));
                if(step==4){stage.DiagnosticCombat();}
                if(step==5){ScreenCapture.CaptureScreenshot(Path.GetFullPath("Artifacts/SelvaV2/unity-punos-v3.png"));}
                if(step==6){File.WriteAllText("Artifacts/SelvaV2/encounters-play.txt",DateTime.Now.ToString("O")+"\n"+(errors==0?"PASS: ambas presentaciones, sprites, maldicion y combate cargan sin errores de runtime.\n":"FAIL runtime errors\n")+log);EditorApplication.isPlaying=false;}
                SessionState.SetInt("Selva.EncounterStep",step+1);
            }catch(Exception ex){File.WriteAllText("Artifacts/SelvaV2/encounters-play.txt","FAIL "+ex);SessionState.SetInt("Selva.EncounterStep",7);EditorApplication.isPlaying=false;}
        }
    }
}
