using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
namespace SuyuRun.Editor
{
    public static class RefactorResourceAudit
    {
        [MenuItem("SuyuRun/Refactor/Verificar recursos y registro")]
        public static void Run()
        {
            var report=new StringBuilder();int checks=0;
            foreach(string region in new[]{"costa","sierra","selva"})
                foreach(string state in new[]{"idle","run","jump","slide","burst","hit","death"})
                {
                    var sheet=TaytaArt.Get(region,state);
                    if(sheet==null)throw new Exception("Falta Tayta: "+region+"/"+state);
                    foreach(var frame in sheet.frames){if(!RefactorActorArt.IsRegistered(frame))throw new Exception("Tayta no migrado: "+region+"/"+state);checks++;}
                    report.AppendLine("PASS Tayta "+region+"/"+state);
                }
            foreach(string region in new[]{"costa","sierra","selva"})
            {
                string[] extra=region=="costa"?new[]{"land","victory","reload","double_jump","fall"}:region=="sierra"?new[]{"huaraca_cargar","huaraca_girar","huaraca_lanzar","victory"}:new[]{"swing","grab","release","climb","punch","kick","uppercut","guard"};
                foreach(string state in extra)
                {
                    var sheet=TaytaArt.Get(region,state);if(sheet==null)throw new Exception("Falta extra Tayta "+region+"/"+state);
                    foreach(var frame in sheet.frames){if(!RefactorActorArt.IsRegistered(frame))throw new Exception("Extra no migrado "+region+"/"+state);checks++;}
                    report.AppendLine("PASS Tayta extra "+region+"/"+state);
                }
            }
            foreach(string name in new[]{"boss_raices_idle","boss_raices_golpe","boss_chullachaqui_idle","boss_chullachaqui_golpe","esbirro_brote_caminar","esbirro_eco_caminar","caiman_emerger","caiman_inmersion"})
            {
                var legacy=SelvaArt.Get(name);
                for(int i=0;i<legacy.frames.Length;i++)
                {
                    var frame=RefactorActorArt.Frame(name,0,i,legacy.frames[i],legacy.frames.Length);
                    if(!RefactorActorArt.IsRegistered(frame))throw new Exception("No migrado: "+name);
                    if(RefactorActorArt.Size(frame)!=legacy.frames[i].rect.size)throw new Exception("Registro incompatible: "+name);
                    checks++;
                }
                report.AppendLine("PASS "+name);
            }
            foreach(string name in new[]{"guardian_idle","guardian_huaraca","condor_vuelo","halcon_vuelo","cueva_esqueleto","cueva_esbirro_piedra","cueva_box_jab","cueva_box_upper","cueva_inca_maza","cueva_inca_carga"})
            {
                var legacy=Resources.LoadAll<Sprite>("SierraV3/sierra_"+name);
                if(legacy.Length==0)throw new Exception("Falta sprite Sierra: "+name);
                for(int i=0;i<legacy.Length;i++)
                {
                    var frame=RefactorActorArt.Frame(name,0,i,legacy[i],legacy.Length);
                    if(!RefactorActorArt.IsRegistered(frame))throw new Exception("No migrado: "+name);
                    checks++;
                }
                report.AppendLine("PASS "+name);
            }
            report.AppendLine("PASS "+checks+" frame/resource checks. Not a visual Play certification.");
            Directory.CreateDirectory("Artifacts/VisualRefactor");File.WriteAllText("Artifacts/VisualRefactor/runtime_audit.txt",report.ToString());
            Debug.Log(report.ToString());
        }
    }
}
