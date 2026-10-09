using System;
using SuyuRun;
using UnityEngine;
public static class PortalAlignmentAudit
{
    public static void Main()
    {
        // Reproduce holding D until the actual right-hand movement clamp.
        for(int region=0;region<2;region++)for(int index=0;index<2;index++)
        {
            if(region==0)
            {
                var w=new SierraCaveWorld(index,5);foreach(var f in w.foes)f.hp=0;
                w.pos=new Vector2(SierraCaveWorld.Width-16,SierraCaveWorld.PortalFloor);
                if(!w.AtPortal)throw new Exception("Sierra right wall cannot activate exit");
                for(int tick=0;tick<362;tick++)w.Step(1f/120,1,false,false,false,Vector2.zero);
                if(!w.cleared)throw new Exception("Sierra holding D resets exit countdown");
            }
            else
            {
                var w=new SelvaMiniWorld(index,5);foreach(var f in w.foes)f.hp=0;
                w.pos=new Vector2(SelvaMiniWorld.Width-16,SelvaMiniWorld.PortalFloor);
                if(!w.AtPortal)throw new Exception("Selva right wall cannot activate exit");
                for(int tick=0;tick<362;tick++)w.Step(1f/120,1,false,false,false,Vector2.zero);
                if(!w.cleared)throw new Exception("Selva holding D resets exit countdown");
            }
            Console.WriteLine("PASS "+(region==0?"Sierra":"Selva")+" mini "+index+": right wall exits while holding D");
        }
        for(int region=0;region<2;region++)for(int index=0;index<2;index++)
        {
            if(region==0)
            {
                var w=new SierraCaveWorld(index,5);foreach(var f in w.foes)f.hp=0;
                w.pos=new Vector2(SierraCaveWorld.PortalX,280);w.grounded=true;
                if(w.AtPortal)throw new Exception("Sierra accepts a different floor");
                w.pos=new Vector2(SierraCaveWorld.PortalX+15,SierraCaveWorld.PortalFloor);
                for(int tick=0;tick<362;tick++)w.Step(1f/120,0,false,false,false,Vector2.zero);
                if(!w.cleared||w.pos!=new Vector2(SierraCaveWorld.PortalX,SierraCaveWorld.PortalFloor))throw new Exception("Sierra exit anchor mismatch");
            }
            else
            {
                var w=new SelvaMiniWorld(index,5);foreach(var f in w.foes)f.hp=0;
                w.pos=new Vector2(SelvaMiniWorld.PortalX+24+15,SelvaMiniWorld.PortalFloor);
                for(int tick=0;tick<362;tick++)w.Step(1f/120,0,false,false,false,Vector2.zero);
                if(!w.cleared||w.pos!=new Vector2(SelvaMiniWorld.PortalX+24,SelvaMiniWorld.PortalFloor))throw new Exception("Selva exit anchor mismatch");
            }
            Console.WriteLine("PASS "+(region==0?"Sierra":"Selva")+" mini "+index+": 3s exit locks onto gate centre/floor");
        }
    }
}
