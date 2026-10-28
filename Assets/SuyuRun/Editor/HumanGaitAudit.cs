using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
namespace SuyuRun.Editor
{
    [InitializeOnLoad] public static class HumanGaitAudit
    {
        [Serializable] sealed class PoseSpec { public int[] silhouetteHeights; }
        static HumanGaitAudit()
        {
            EditorApplication.delayCall+=()=>
            {
                if(!File.Exists("Artifacts/VisualRefactor/REQUEST_HUMAN_GAIT_AUDIT.txt"))return;
                if(EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.isUpdating||EditorApplication.isCompiling)return;
                try{Run();GlobalEngineeringAudit.Run();HudLayoutAudit.Run();}catch(Exception e){File.WriteAllText("Artifacts/VisualRefactor/human_gait_audit.txt","FAIL "+e);Debug.LogException(e);}
            };
        }
        [MenuItem("SuyuRun/Refactor/Verificar carrera y tamaño")]
        public static void Run()
        {
            if(TaytaArt.Get("costa","run")?.spec.uniformAnatomy==true){TaytaUniformAudit.Run();return;}
            RefactorResourceAudit.Run();var report=new StringBuilder();
            foreach(string region in new[]{"costa","sierra","selva"})
            {
                var run=TaytaArt.Get(region,"run");if(run.frames.Length!=12)throw new Exception("Human gait absent: "+region);
                foreach(string state in new[]{"idle","run","jump","slide","burst","hit"})
                    foreach(var f in TaytaArt.Get(region,state).frames)
                        if(Mathf.Abs(f.pixelsPerUnit-64/TaytaArt.WorldScale)>.001f)throw new Exception("Mixed body scale "+region+"/"+state);
                report.AppendLine("PASS "+region+": 12 gait poses and common PPU for idle/run/jump/slide/burst/hit");
                var text=Resources.Load<TextAsset>("RefactorBank/Tayta"+char.ToUpperInvariant(region[0])+region.Substring(1)+"/slide_human");
                if(!text)throw new Exception("Deep crouch missing: "+region);
                var spec=JsonUtility.FromJson<PoseSpec>(text.text);
                if(spec.silhouetteHeights==null||spec.silhouetteHeights.Length!=4)throw new Exception("Crouch bounds missing: "+region);
                foreach(int height in spec.silhouetteHeights)
                    if(height/64f*32*TaytaArt.WorldScale>19)throw new Exception("Crouch visually clips ceiling: "+region);
                report.AppendLine("PASS "+region+": 4 deep crouch poses fit 19px clearance visually");
            }
            var stone=new SierraCaveWorld(0,5);var jungle=new SelvaMiniWorld(0,5);
            foreach(var body in new[]{stone.Body,jungle.Body})
                if(Mathf.Abs(body.width-15.3f)>.001f||Mathf.Abs(body.height-28.9f)>.001f)throw new Exception("Cave body scale mismatch");
            report.AppendLine("PASS Sierra/Selva minilevel bodies: 15.3 x 28.9 logical pixels, feet unchanged");
            // Existing Selva ceiling: bottom 129 with ground 148, 19px clearance.
            var ceiling=new Rect(88,105,32,24);
            var crouch=new Rect(100-8*TaytaArt.WorldScale,148-15*TaytaArt.WorldScale,16*TaytaArt.WorldScale,15*TaytaArt.WorldScale);
            var stand=new Rect(100-8*TaytaArt.WorldScale,148-34*TaytaArt.WorldScale,16*TaytaArt.WorldScale,34*TaytaArt.WorldScale);
            if(crouch.Overlaps(ceiling)||!stand.Overlaps(ceiling))throw new Exception("Duck clearance broken");
            report.AppendLine("PASS Selva duck obstacle: standing collides, crouching clears");
            if(GameScale.CrouchHeight>=GameScale.DuckClearance||GameScale.StandingHeight<=GameScale.DuckClearance)
                throw new Exception("Shared Costa/Sierra duck clearance broken");
            report.AppendLine("PASS shared Costa/Sierra clearance requires crouch at reduced hero scale");
            report.AppendLine("Geometry/resource assertions only; visual acceptance and full playthrough remain separate.");
            File.WriteAllText("Artifacts/VisualRefactor/human_gait_audit.txt",report.ToString());Debug.Log(report.ToString());
        }
    }
}
