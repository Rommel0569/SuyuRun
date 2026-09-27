using System;
using System.Collections;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SuyuRun.Editor
{
    // Editor-only visual checks: freeze the simulation clock, never alter the saved scene.
    public static class SierraFinishingPreview
    {
        const BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
        static FieldInfo Field(string name)=>typeof(SierraStageDirector).GetField(name,Flags);
        static void Set(SierraStageDirector stage,string name,object value)=>Field(name).SetValue(stage,value);
        static object Call(SierraStageDirector stage,string name,params object[] args)=>typeof(SierraStageDirector).GetMethod(name,Flags).Invoke(stage,args);
        public static void Tick(SierraStageDirector stage)
        {
            int step=SessionState.GetInt("SuyuRun.FinishingStep",0);
            if(step>=8)
            {
                File.WriteAllText("Artifacts/SierraV3/finishing-preview.txt","Four Play visual captures; errors="+SessionState.GetInt("SuyuRun.SierraErrors",0)+"; "+DateTime.Now.ToString("O"));
                SessionState.SetBool("SuyuRun.FinishingPreview",false);SessionState.SetBool("SuyuRun.SierraDiagnostic",false);EditorApplication.isPlaying=false;return;
            }
            int section=step/2;
            if(step%2==1)ScreenCapture.CaptureScreenshot(Path.GetFullPath("Artifacts/SierraV3/finishing-"+section+".png"));
            else
            {
                Call(stage,"ResetCaves",false);
                Set(stage,"interlude",Enum.ToObject(Field("interlude").FieldType,0));Set(stage,"guideSeen",true);
                Set(stage,"narrativeLine","");
                Set(stage,"dead",false);Set(stage,"complete",false);Set(stage,"paused",false);
                if(section<2)
                {
                    float time=0;
                    if(section==0)
                    {
                        foreach(var enemy in (IEnumerable)Field("enemies").GetValue(stage))
                        {
                            var type=enemy.GetType();if((string)type.GetField("kind").GetValue(enemy)!="log")continue;
                            time=((float)type.GetField("anchor").GetValue(enemy)-210)/224;break;
                        }
                    }
                    else
                    {
                        float duration=(float)Field("duration").GetValue(stage);
                        for(int group=Mathf.CeilToInt(duration*2/3*224/640);group<80;group++)
                        {
                            float world=group*640+160;
                            if((bool)Call(stage,"RiserNeedsJump",world)){time=(world-190)/224;break;}
                        }
                    }
                    Set(stage,"seconds",time);Set(stage,"act",section==0?0:2);
                    Set(stage,"feet",(float)Call(stage,"Surface",time*224+100,section==0?0:2));
                    Set(stage,"epoch",AudioSettings.dspTime+100);Set(stage,"previous",AudioSettings.dspTime+100);
                }
                else
                {
                    Call(stage,"BeginCave",section-2);
                    var cave=(SierraCaveWorld)Field("cave").GetValue(stage);cave.pos=new Vector2(590,280);
                    Set(stage,"cavePhase",Enum.ToObject(Field("cavePhase").FieldType,2));
                    Set(stage,"caveEpoch",AudioSettings.dspTime-10);Set(stage,"caveLast",AudioSettings.dspTime+100);
                    Set(stage,"caveCamera",new Vector2(465,160));
                }
                ((AudioSource)Field("music").GetValue(stage)).Stop();
            }
            SessionState.SetInt("SuyuRun.FinishingStep",step+1);
        }
    }
}
