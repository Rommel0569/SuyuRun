using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
namespace SuyuRun.Editor
{
    public static class TaytaUniformAudit
    {
        const string Root="Assets/SuyuRun/Art/RefactorRuntime/Resources/RefactorBank/TaytaUniform";
        [MenuItem("SUYURUN/Tayta/Verificar anatomia y registro")]
        public static void Run()
        {
            var report=new StringBuilder();int sheets=0,frames=0;float common=0;
            foreach(string region in new[]{"costa","sierra","selva"})
                foreach(string path in Directory.GetFiles(Root+"/"+region,"*.json"))
                {
                    string state=Path.GetFileNameWithoutExtension(path);var sheet=TaytaArt.Get(region,state);
                    if(sheet==null||!sheet.spec.uniformAnatomy)throw new Exception("Mixed old anatomy: "+region+"/"+state);
                    foreach(var sprite in sheet.frames)
                    {
                        if(!sprite.name.StartsWith("UniformTayta/")||!RefactorActorArt.IsRegistered(sprite))throw new Exception("Unregistered pose: "+state);
                        if(common==0)common=sprite.pixelsPerUnit;
                        if(Mathf.Abs(common-sprite.pixelsPerUnit)>.001f)throw new Exception("Body scale changed: "+state);
                        float footX=(sheet.spec.frameWidth*.5f-sprite.pivot.x)/common-24f/32;
                        float footY=(sheet.spec.frameHeight-sheet.spec.footAnchor[1]-sprite.pivot.y)/common+48f/32;
                        if(Mathf.Abs(footX)>.001f||Mathf.Abs(footY)>.001f)throw new Exception("Foot registration shifted: "+state);
                        frames++;
                    }
                    sheets++;report.AppendLine("PASS "+region+"/"+state+": "+sheet.frames.Length+" poses, common PPU/foot origin");
                }
            foreach(string name in RunnerFinalMovie.RequiredAssets)new FinalMovieArt().Load(name);
            if(!new FinalMovieArt().Uniform("costa_run"))throw new Exception("Ending still selects old proportions");
            report.AppendLine("PASS "+sheets+" gameplay state sheets / "+frames+" frames; ending also loads unified actor mappings.");
            report.AppendLine("Resource and geometry checks only. Visual anatomy and gameplay Play are reviewed separately.");
            Directory.CreateDirectory("Artifacts/TaytaUniform");File.WriteAllText("Artifacts/TaytaUniform/UNITY_RESOURCE_AUDIT.txt",report.ToString());Debug.Log(report.ToString());
        }
        public static void RunAndPlay(){Run();RefactorPlayAudit.Start();}
        public static void FullValidation(){Run();SessionState.SetBool("SuyuRun.Uniform.EndingAfterTour",true);RefactorPlayAudit.Start();}
    }
}
