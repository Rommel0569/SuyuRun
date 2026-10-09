using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
namespace SuyuRun.Editor
{
    // Explicit marker request; checks the pure combat model without changing the user's scene.
    [InitializeOnLoad] public static class SelvaEncounterAudit
    {
        static double next;
        static SelvaEncounterAudit(){EditorApplication.update+=Tick;}
        [MenuItem("SUYURUN/Selva/Auditar encuentros (sin cambiar escena)")]
        public static void Run()
        {
            var report=new StringBuilder(DateTime.Now.ToString("O")+"\n");
            try{
                SelvaStageSetup.PrepareAnimationAssets();
                foreach(var name in new[]{"CuevaGuardian","Chullachaqui"}){
                    var clip=Resources.Load<AudioClip>("SelvaAudio/"+name);
                    if(!clip)throw new Exception("Missing music: "+name);
                    report.AppendLine(name+" seconds="+clip.length);
                }
                for(int index=0;index<2;index++){
                    var w=new SelvaMiniWorld(index,5);
                    for(int i=0;i<36000&&!w.dead&&!w.cleared;i++){
                        float axis=0;bool jump=false,fire=false;Vector2 aim=Vector2.zero;
                        w.AutoControl(ref axis,ref jump,ref fire,ref aim);w.Step(1f/120,axis,jump,false,fire,aim);
                    }
                    report.AppendLine($"mini {index}: cleared={w.cleared} dead={w.dead} time={w.time} boss={w.Boss.hp} remaining={w.RemainingMinions} cursed={w.Cursed} health={w.health}");
                    if(w.dead||!w.cleared)report.AppendLine("BALANCE_REVIEW: "+w.damageTrace);
                }
                report.AppendLine("Completed model audit. Does not certify visual/gameplay balance.");
            }catch(Exception e){report.AppendLine("FAIL "+e);}
            Directory.CreateDirectory("Artifacts/SelvaV2");File.WriteAllText("Artifacts/SelvaV2/encounters-audit.txt",report.ToString());
        }
        static void Tick(){
            if(EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.timeSinceStartup<next)return;
            next=EditorApplication.timeSinceStartup+1;
            const string request="Artifacts/SelvaV2/encounters-audit.request";
            if(!File.Exists(request))return;File.Delete(request);Run();
        }
    }
}
