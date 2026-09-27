using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
namespace SuyuRun.Editor
{
    public static class SierraGrazersCheck
    {
        const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
        static void Shot(string name)=>ScreenCapture.CaptureScreenshot(Path.GetFullPath("Artifacts/SierraGrazers/"+name+".png"));
        public static void Tick(int step)
        {
            try
            {
                if(step==0)
                {
                    for(int species=0;species<2;species++)for(int f=0;f<12;f++)
                        if(!SierraGrazers.Sprite(species,f))throw new Exception("Missing grazing frame");
                    if(SierraGrazers.Atlas.filterMode!=FilterMode.Point)throw new Exception("Nonpixel importer");
                    for(int world=160;world<50000;world+=640)
                    {
                        if(!SierraGrazers.Visible(world,world+31)||SierraGrazers.Visible(world,world+33)||!SierraGrazers.Visible(world,world-320))throw new Exception("Premature culling");
                    }
                }
                if(step==1)Shot("inicio-1");
                if(step==2)Shot("inicio-2");
                if(step==3){RunnerProgress.EnterSierraMuseum=false;RunnerProgress.OpenSierra();}
                if(step==4||step==6)
                {
                    var stage=UnityEngine.Object.FindFirstObjectByType<SierraStageDirector>();
                    stage.DiagnosticAutoplay=true;stage.enabled=false;
                    typeof(SierraStageDirector).GetField("act",F).SetValue(stage,step==4?0:2);
                    typeof(SierraStageDirector).GetField("seconds",F).SetValue(stage,step==4?.35f:140f);
                    typeof(SierraStageDirector).GetMethod("Render",F).Invoke(stage,null);
                }
                if(step==5)Shot("camino");
                if(step==7)Shot("ruinas");
                if(step==8)
                {
                    File.WriteAllText("Artifacts/SierraGrazers/play-check.txt","PASS 24 grazing frames, Point, full-sprite visibility at 78 absolute world anchors, menu/Camino/Ruinas captures; errors="+SessionState.GetInt("MuseumErrors",0)+"; "+DateTime.Now.ToString("O"));
                    EditorApplication.isPlaying=false;
                }
            }
            catch(Exception e){File.WriteAllText("Artifacts/SierraGrazers/play-check.txt","FAIL "+e);Debug.LogException(e);EditorApplication.isPlaying=false;}
        }
    }
}
