using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace SuyuRun.Editor
{
    [InitializeOnLoad]
    public static class SelvaVerification
    {
        static double next;static int step;static string report="";static int errors;
        static SelvaVerification(){EditorApplication.update+=Tick;Application.logMessageReceived+=(m,s,t)=>{if(SessionState.GetBool("Selva.QA",false)&&(t==LogType.Error||t==LogType.Exception)){errors++;report+="ERROR "+m+"\n";}};}
        public static void Run()
        {
            SessionState.SetBool("SuyuRun.SierraDiagnostic",false);SessionState.SetBool("SuyuRun.MuseumPreview",false);
            if(!File.Exists(SelvaStageSetup.ScenePath))SelvaStageSetup.Prepare();
            SelvaStageSetup.PrepareAnimationAssets();
            EditorSceneManager.OpenScene(SelvaStageSetup.ScenePath);EditorSceneManager.playModeStartScene=AssetDatabase.LoadAssetAtPath<SceneAsset>(SelvaStageSetup.ScenePath);
            SessionState.SetBool("Selva.QA",true);SessionState.SetInt("Selva.QAStep",0);EditorApplication.isPlaying=true;
        }
        static void Require(bool ok,string text){if(!ok)throw new Exception(text);report+="PASS "+text+"\n";}
        static void Tick()
        {
            if(!SessionState.GetBool("Selva.QA",false)||EditorApplication.isCompiling||EditorApplication.isUpdating||!EditorApplication.isPlaying||EditorApplication.timeSinceStartup<next)return;
            var stage=UnityEngine.Object.FindFirstObjectByType<SelvaStageDirector>();if(!stage)return;
            next=EditorApplication.timeSinceStartup+1.4;step=SessionState.GetInt("Selva.QAStep",0);
            try
            {
                if(step==0)
                {
                    stage.DiagnosticAutoplay=true;
                    var paths=Directory.GetFiles("Assets/SuyuRun/Art/PixelSelva/ProductionV2/Resources/SelvaV2","*.png");
                    foreach(var path in paths){string name=Path.GetFileNameWithoutExtension(path).Substring(6);var a=SelvaArt.Get(name);Require(a.frames.Length==a.spec.frames,"sheet "+name);var t=(TextureImporter)AssetImporter.GetAtPath(path.Replace('\\','/'));Require(t.filterMode==FilterMode.Point&&t.textureCompression==TextureImporterCompression.Uncompressed&&!t.mipmapEnabled,"pixel import "+name);}
                    Require(Application.CanStreamedLevelBeLoaded(RunnerProgress.SelvaScene),"Selva registered in scene list");
                    var c=new SelvaCaiman(210,600,146,42);var phases=new HashSet<SelvaCaiman.Phase>();int splash=0;c.effect=(n,p)=>splash++;
                    for(int i=0;i<3600;i++){c.Step(1f/120,300);phases.Add(c.phase);RequireFinite(c.pos);}
                    Require(phases.Count==7&&c.emerged>=4&&c.immersed>=4&&c.relocated>=5&&splash>=12,"full caiman cycle including relocation and reactive splashes");
                    c.phase=SelvaCaiman.Phase.Snap;c.timer=.07f;c.pos=new Vector2(260,80);var h=SelvaArt.Get("caiman_snap").spec.hitboxes[c.Frame];
                    Require(c.Hits(SelvaArt.NativeBox(h.mouth,c.Origin,c.left,64)),"snap frame mouth matches damaging hitbox");
                    c.phase=SelvaCaiman.Phase.Telegraph;Require(!c.Hits(new Rect(0,0,640,180)),"telegraph is harmless");
                    var v=new SelvaVine(new Vector2(120,8),0){omega=2};for(int i=0;i<400;i++)v.Step(1f/120);
                    Require(Mathf.Abs(Vector2.Distance(v.anchor,v.Grip)-82)<.001f&&Mathf.Abs(v.angle)<1.06f,"pendulum length and bounded arc");
                    v.omega=2;Require(Vector2.Distance(v.Curve(.5f),Vector2.Lerp(v.anchor,v.Grip,.5f))>1,"rope bows with angular inertia");
                    stage.DiagnosticZone(0);var before=stage.Hero;stage.Step(1f/120,0,false,true);Require(stage.Hero.y<before.y,"jump responds on the input substep");
                    stage.Step(1f/120,0,false,true);Require(stage.Hero.y<before.y-2,"second jump responds without beat gating");
                    stage.DiagnosticZone(0);before=stage.Hero;stage.Step(1f/120,0,true,false);Require(Mathf.Abs(stage.Hero.y-before.y)<.01f,"fire does not inject an unsolicited jump");
                }
                else if(step==1){Capture("unity-bosque.png");}
                else if(step==2){stage.DiagnosticZone(1);}
                else if(step==3){stage.DiagnosticGrab(0);}
                else if(step==4){Require(stage.IsHanging,"hero remains attached to moving rope");Capture("unity-balanceo.png");}
                else if(step==5){stage.DiagnosticRelease();Require(stage.Status=="Running","release keeps gameplay running");Capture("unity-rio.png");}
                else if(step==6){stage.DiagnosticZone(2);Require(stage.InMini,"cave opens as a manual platforming space");}
                else if(step==7){Capture("unity-cueva.png");}
                else if(step==8){stage.DiagnosticZone(4);Require(stage.InMini,"Chullachaqui opens as a second manual space");}
                else if(step==9){Capture("unity-ruinas.png");}
                else if(step==10){stage.DiagnosticZone(3);}
                else if(step==11){Capture("unity-noche.png");}
                else if(step==12){stage.DiagnosticZone(5);}
                else if(step==13){Capture("unity-templo.png");}
                else if(step==14)
                {
                    for(int index=0;index<2;index++)
                    {
                        var world=new SelvaMiniWorld(index,5);
                        for(int i=0;i<24000&&!world.dead&&!world.cleared;i++)
                        {float axis=0;bool jump=false,fire=false;Vector2 aim=Vector2.zero;world.AutoControl(ref axis,ref jump,ref fire,ref aim);world.Step(1f/120,axis,jump,false,fire,aim);}
                        Require(world.cleared&&!world.dead&&world.Boss.hp==0&&world.RemainingMinions==0,"mini "+index+" completable with 5 health and finite magazine");
                        Require(world.time<65,"early victory exits without waiting for song end "+index);
                    }
                    Require(errors==0,"zero runtime errors");File.WriteAllText("Artifacts/SelvaV2/unity-verification.txt",DateTime.Now.ToString("O")+"\n"+report);
                    SessionState.SetBool("Selva.QA",false);EditorApplication.isPlaying=false;EditorApplication.delayCall+=()=>EditorApplication.Exit(0);return;
                }
                SessionState.SetInt("Selva.QAStep",step+1);
            }
            catch(Exception ex){report+="FAIL "+ex+"\n";File.WriteAllText("Artifacts/SelvaV2/unity-verification.txt",report);SessionState.SetBool("Selva.QA",false);EditorApplication.isPlaying=false;EditorApplication.delayCall+=()=>EditorApplication.Exit(1);}
        }
        static void RequireFinite(Vector2 p){if(float.IsNaN(p.x)||float.IsNaN(p.y)||float.IsInfinity(p.x)||float.IsInfinity(p.y))throw new Exception("nonfinite physics");}
        static void Capture(string name)=>ScreenCapture.CaptureScreenshot(Path.GetFullPath("Artifacts/SelvaV2/"+name));
    }
}
