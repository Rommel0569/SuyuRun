using System;
using SuyuRun;
using UnityEngine;
public static class SelvaModelAudit
{
    public static void Main(){
        var moving=new SelvaMiniWorld(0,5);float startX=moving.pos.x;
        moving.Step(.1f,1,false,false,true,new Vector2(220,120));
        if(moving.pos.x<=startX+10||moving.shotsFired!=1)throw new Exception("Firing blocks movement");
        moving.BeginReload();startX=moving.pos.x;moving.Step(.1f,1,false,false,false,Vector2.zero);
        if(moving.pos.x<=startX+10)throw new Exception("Reload blocks movement");
        Console.WriteLine("PASS firing and reloading preserve horizontal movement");
        var cursed=new SelvaMiniWorld(1,5);cursed.Boss.awakened=true;cursed.Boss.phase=3;cursed.pos=new Vector2(cursed.Boss.pos.x-35,320);
        cursed.SpecialAttack("punch");cursed.Step(.1f,0,false,false,true,cursed.Boss.pos);
        if(cursed.ammo!=5||cursed.shotsFired!=0||cursed.Boss.hp!=17)throw new Exception("Curse must disable guns and melee must hit exactly once");
        Console.WriteLine("PASS curse seals pistol; punch deals damage without consuming ammo");
        var roots=new SelvaMiniWorld(0,5);roots.hazards.Add(new SelvaMiniWorld.Hazard{pos=roots.pos,born=-1,trap=true});roots.Step(.01f,1,false,false,false,Vector2.zero);
        if(roots.rootedUntil<=roots.time||roots.velocity.x!=0)throw new Exception("Roots do not immobilize");
        float y=roots.pos.y;roots.Step(.01f,1,true,false,false,Vector2.zero);if(roots.pos.y>=y)throw new Exception("Roots prevent escape jump");
        Console.WriteLine("PASS root telegraph immobilizes horizontal motion; jump remains available");
        for(int index=0;index<2;index++){
        var exit=new SelvaMiniWorld(index,5);foreach(var foe in exit.foes)foe.hp=0;
        exit.pos=new Vector2(SelvaMiniWorld.PortalX+24,SelvaMiniWorld.PortalFloor);
        for(int tick=0;tick<359;tick++)exit.Step(1f/120,0,false,false,false,Vector2.zero);
        if(exit.cleared)throw new Exception("Portal exits before three seconds");
        for(int tick=0;tick<3;tick++)exit.Step(1f/120,0,false,false,false,Vector2.zero);
        if(!exit.cleared||exit.time>3.03f)throw new Exception("Early victory must exit in three seconds");
        Console.WriteLine("PASS portal exits after 3 seconds, without waiting for minute-end");
        var w=new SelvaMiniWorld(index,5);
        for(int i=0;i<36000&&!w.dead&&!w.cleared;i++){
            float axis=0;bool jump=false,fire=false;Vector2 aim=Vector2.zero;
            w.AutoControl(ref axis,ref jump,ref fire,ref aim);w.Step(1f/120,axis,jump,false,fire,aim);
        }
        Console.WriteLine($"mini={index} cleared={w.cleared} dead={w.dead} seconds={w.time:F2} boss={w.Boss.hp} minions={w.RemainingMinions} health={w.health} cursed={w.Cursed}");
        if(!w.cleared||w.dead||w.time>65)throw new Exception("Encounter completion regression");
        if(!w.cleared)Console.WriteLine(w.damageTrace);
    }}
}
