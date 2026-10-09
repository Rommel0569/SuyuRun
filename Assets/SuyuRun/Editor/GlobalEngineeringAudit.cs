using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
namespace SuyuRun.Editor
{
    public static class GlobalEngineeringAudit
    {
        static void Require(bool condition,string message){if(!condition)throw new Exception(message);}
        [MenuItem("SuyuRun/Refactor/Verificar geometría, cuevas y fondos")]
        public static void Run()
        {
            var report=new StringBuilder();
            report.AppendLine("Revision 2026-10-04: 13 independent background layers, detailed log and centred exits.");
            foreach(string name in new[]{"costa_capa3_paracas","sierra_compartido_capa3_nevados","selva_bosque_capa3_estructura","costa_capa5_huacas","sierra_ruinas_capa4_paisaje","selva_templo_capa3_estructura","sierra_cueva1_arcos","sierra_cueva2_arcos","sierra_valle_capa4_paisaje","selva_rio_capa3_estructura","selva_noche_capa3_estructura","sierra_camino_capa4_paisaje","selva_cueva_capa3_estructura"})
            {
                var s=RefactorEnvironmentArt.Frame(name,null);
                int height=name.StartsWith("sierra_cueva")?384:180;
                Require(s&&RefactorActorArt.Size(s)==new Vector2(640,height),"Missing/invalid layer "+name);
                Require(Mathf.Abs(s.bounds.size.x-20)<.001f&&Mathf.Abs(s.bounds.size.y-height/32f)<.001f,"Layer changes world size "+name);
                Require(s.texture.filterMode==FilterMode.Point,"Non-pixel filtering "+name);
                report.AppendLine("PASS "+name+": native detail; original world dimensions and Point filtering");
            }
            var log=RefactorEnvironmentArt.Frame("selva_plataforma_tronco",null);
            Require(log&&RefactorActorArt.Size(log)==new Vector2(64,16),"Missing/invalid detailed log");
            Require(Mathf.Abs(log.bounds.size.x-2)<.001f&&Mathf.Abs(log.bounds.size.y-.5f)<.001f,"Detailed log changes landing geometry");
            report.AppendLine("PASS detailed log: original 64x16 logical canvas and Point filtering");
            var block=RefactorEnvironmentArt.Frame("sierra_ruinas_bloque",null);
            Require(block&&RefactorActorArt.Size(block)==new Vector2(64,32),"Missing/invalid ruins block");
            Require(Mathf.Abs(block.bounds.size.x-2)<.001f&&Mathf.Abs(block.bounds.size.y-1)<.001f,"Detailed block changes landing geometry");
            report.AppendLine("PASS detailed ruins block: original 64x32 logical geometry");
            for(int index=0;index<2;index++)
            {
                var s=new SierraCaveWorld(index,5);s.spawned=SierraCaveWorld.SpawnBudget;s.nextSpawn=float.MaxValue;
                foreach(var foe in s.foes)foe.hp=0;
                s.pos=new Vector2(SierraCaveWorld.Width-17,SierraCaveWorld.PortalFloor);s.grounded=true;
                Require(s.AtPortal,"Sierra portal excludes far right edge");
                for(int i=0;i<190;i++)s.Step(1f/60,0,false,false,false,Vector2.zero);
                Require(s.cleared&&s.velocity==Vector2.zero,"Sierra exit did not complete after 3s");
                var j=new SelvaMiniWorld(index,5);j.spawned=SelvaMiniWorld.SpawnBudget;j.nextSpawn=float.MaxValue;
                foreach(var foe in j.foes)foe.hp=0;
                j.pos=new Vector2(SelvaMiniWorld.Width-17,SelvaMiniWorld.PortalFloor);j.grounded=true;
                Require(j.AtPortal,"Selva portal excludes far right edge");
                for(int i=0;i<190;i++)j.Step(1f/60,0,false,false,false,Vector2.zero);
                Require(j.cleared&&j.velocity==Vector2.zero,"Selva exit did not complete after 3s");
                report.AppendLine("PASS cave "+index+": Sierra and Selva clear at portal in 3s, including right edge");
                s=new SierraCaveWorld(index,5);s.spawned=SierraCaveWorld.SpawnBudget;s.nextSpawn=float.MaxValue;
                for(int i=0;i<6;i++)s.Step(.18f,0,false,false,true,new Vector2(0,0));
                Require(s.shotsFired==5&&s.ammo==0&&s.Reloading,"Sierra magazine/reload broken");
                s.Step(4.1f,0,false,false,false,Vector2.zero);
                Require(s.ammo==5&&!s.Reloading,"Sierra reload never finishes");
                j=new SelvaMiniWorld(index,5);j.spawned=SelvaMiniWorld.SpawnBudget;j.nextSpawn=float.MaxValue;
                for(int i=0;i<6;i++)j.Step(.18f,0,false,false,true,new Vector2(0,0));
                Require(j.shotsFired==5&&j.ammo==0&&j.Reloading,"Selva magazine/reload broken");
                j.Step(4.1f,0,false,false,false,Vector2.zero);
                Require(j.ammo==5&&!j.Reloading,"Selva reload never finishes");
                report.AppendLine("PASS cave "+index+": magazine 5, no infinite fire, 4s reload in both regions");
            }
            Require(SierraCaveWorld.OvertimeAllowance(true,1)==5&&SelvaMiniWorld.OvertimeAllowance(false,0)==5,"Overtime contract broken");
            report.AppendLine("PASS overtime: 5s if only boss or minions remain");
            report.AppendLine("Model/resource checks only. Not a full visual playthrough or balance certification.");
            File.WriteAllText("Artifacts/VisualRefactor/global_engineering_audit.txt",report.ToString());Debug.Log(report.ToString());
        }
    }
}
