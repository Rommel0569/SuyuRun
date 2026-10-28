using System;
using System.IO;
using System.Text;
using UnityEngine;
namespace SuyuRun.Editor
{
    public static class TaytaCompatibilityAudit
    {
        public static void Run()
        {
            if(TaytaArt.UseUniformBank)throw new Exception("Uniform bank is still active");
            if(GameScale.NativeStride!=112)throw new Exception("Previous gait clock was not restored");
            var report=new StringBuilder();int checks=0;
            foreach(string region in new[]{"costa","sierra","selva"})
            {
                foreach(string state in new[]{"idle","run","slide","jump","double_jump","fall","land","burst","hit","death","reload","victory"})
                {
                    var sheet=TaytaArt.Get(region,state);
                    if(sheet==null||sheet.spec.uniformAnatomy)throw new Exception("Previous sheet not selected: "+region+"/"+state);
                    if(state=="run"&&sheet.frames.Length!=12||state=="slide"&&sheet.frames.Length!=4)throw new Exception("Previous cycle missing: "+region+"/"+state);
                    report.AppendLine("PASS "+region+"/"+state+": previous bank, "+sheet.frames.Length+" frames");checks++;
                }
            }
            foreach(string state in new[]{"huaraca_cargar","huaraca_girar","huaraca_lanzar"})
                if(TaytaArt.Get("sierra",state)==null)throw new Exception("Previous sling action missing: "+state);
            foreach(string state in new[]{"swing","grab","release","regrab","climb","punch","kick","uppercut","guard","dodge"})
                if(TaytaArt.Get("selva",state)==null)throw new Exception("Previous jungle action missing: "+state);
            if(new FinalMovieArt().Uniform("costa_run"))throw new Exception("New cinematic actor still selected");
            report.AppendLine("PASS "+checks+" previous state selections, regional abilities, previous gait and cinematic selection.");
            report.AppendLine("Resource selection check in Unity, not a new full playthrough or aesthetic approval.");
            Directory.CreateDirectory("Artifacts/TaytaUniform");File.WriteAllText("Artifacts/TaytaUniform/RESTORE_PREVIOUS.txt",report.ToString());Debug.Log(report.ToString());
        }
    }
}
