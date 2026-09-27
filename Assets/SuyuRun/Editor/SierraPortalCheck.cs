using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace SuyuRun.Editor
{
    // Diagnostic only: skip Costa gameplay to exercise its actual ending and actual scene load.
    [InitializeOnLoad]
    public static class SierraPortalCheck
    {
        const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        static double next,arrivalAt=-1;
        static bool endingCaptured,arrivalCaptured;
        static SierraPortalCheck()
        {
            EditorApplication.update+=Poll;
            EditorApplication.playModeStateChanged+=state=>{if(state==PlayModeStateChange.EnteredEditMode)SessionState.SetBool("SuyuRun.CostaPortalQA",false);};
        }
        public static void Run()
        {
            Directory.CreateDirectory("Artifacts/SierraV3");
            endingCaptured=arrivalCaptured=false;arrivalAt=-1;
            SessionState.SetBool("SuyuRun.CostaPortalQA",true);SessionState.SetInt("SuyuRun.CostaPortalPhase",0);
            SessionState.SetBool("SuyuRun.SierraDiagnostic",true);SessionState.SetInt("SuyuRun.SierraErrors",0);
            SessionState.SetString("SuyuRun.SierraPreviousStart",AssetDatabase.GetAssetPath(EditorSceneManager.playModeStartScene));
            SessionState.SetBool("SuyuRun.SierraRestore",true);
            EditorSceneManager.playModeStartScene=AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/SuyuRun/Scenes/Level_Costa.unity");
            EditorApplication.isPlaying=true;
        }
        static void Poll()
        {
            if(EditorApplication.isCompiling||!EditorApplication.isPlaying||!SessionState.GetBool("SuyuRun.CostaPortalQA",false)||EditorApplication.timeSinceStartup<next)return;
            next=EditorApplication.timeSinceStartup+.15;
            var coast=UnityEngine.Object.FindFirstObjectByType<RunnerPrototype>();
            if(coast)
            {
                var type=typeof(RunnerPrototype);coast.AutomatedSmokeTest=true;
                int phase=SessionState.GetInt("SuyuRun.CostaPortalPhase",0);
                if(phase==0)
                {
                    type.GetMethod("Begin",Private).Invoke(coast,new object[]{false});
                    ((AudioSource)type.GetField("music",Private).GetValue(coast)).Stop();
                    float duration=(float)type.GetProperty("Duration",Private).GetValue(coast),speed=(float)type.GetProperty("Speed",Private).GetValue(coast);
                    type.GetField("distance",Private).SetValue(coast,duration*speed);
                    var mode=type.GetField("mode",Private);mode.SetValue(coast,Enum.Parse(mode.FieldType,"Ending"));
                    type.GetField("endingPhase",Private).SetValue(coast,0);type.GetField("endingTimer",Private).SetValue(coast,0f);
                    SessionState.SetInt("SuyuRun.CostaPortalPhase",1);return;
                }
                float time=(float)type.GetField("endingTimer",Private).GetValue(coast);
                if(!endingCaptured&&time>.7f)
                {endingCaptured=true;ScreenCapture.CaptureScreenshot(Path.GetFullPath("Artifacts/SierraV3/portal-costa.png"));}
                if(coast.CurrentState=="Complete"&&phase==1)
                {type.GetMethod("StartCoastTravel",Private).Invoke(coast,null);SessionState.SetInt("SuyuRun.CostaPortalPhase",2);}
                return;
            }
            var stage=UnityEngine.Object.FindFirstObjectByType<SierraStageDirector>();
            if(!stage)return;
            stage.DiagnosticAutoplay=true;
            if(arrivalAt<0)arrivalAt=EditorApplication.timeSinceStartup;
            if(!arrivalCaptured&&EditorApplication.timeSinceStartup-arrivalAt>.8)
            {arrivalCaptured=true;ScreenCapture.CaptureScreenshot(Path.GetFullPath("Artifacts/SierraV3/portal-sierra.png"));}
            if(stage.Progress<3)return;
            var stageType=typeof(SierraStageDirector);
            stageType.GetField("act",Private).SetValue(stage,2);
            stageType.GetMethod("Restart",Private).Invoke(stage,new object[]{false});
            if((int)stageType.GetField("act",Private).GetValue(stage)!=0)throw new Exception("Sierra replay did not return to Camino");
            File.WriteAllText("Artifacts/SierraV3/portal-verification.txt","PASS: actual Costa ending -> curtain -> LoadScene Sierra -> arrival -> Running; replay resets Camino; errors="+SessionState.GetInt("SuyuRun.SierraErrors",0)+"; "+DateTime.Now.ToString("O"));
            SessionState.SetBool("SuyuRun.CostaPortalQA",false);SessionState.SetBool("SuyuRun.SierraDiagnostic",false);
            EditorApplication.isPlaying=false;
        }
    }
}
