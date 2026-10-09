using UnityEngine;
using System.Linq;
using System.Collections.Generic;
namespace SuyuRun
{
    // Selva-only combat. Sierra remains unchanged. Timers are advanced from the DSP step.
    public sealed partial class SelvaMiniWorld
    {
        public sealed class Hazard {public Vector2 pos;public float born;public bool trap,spent;}
        public readonly List<Hazard> hazards=new List<Hazard>();
        public bool Cursed=>index==1&&Boss.awakened;
        public float rootedUntil=-1,guardUntil=-1,meleeAt=-10,guardAt=-10;
        public string meleeState="punch";
        int meleeSerial;readonly Dictionary<Foe,int> struck=new Dictionary<Foe,int>();
        public void SpecialAttack(string action){if(!Cursed||Cinematic||dead||time-meleeAt<.4f)return;meleeState=action;meleeAt=time;meleeSerial++;}
        public void Guard(){if(Cursed&&!Cinematic&&time-guardAt>.65f){guardAt=time;guardUntil=time+.2f;}}
        void TickMagic(float dt,bool fire,Vector2 aim)
        {
            if(Cursed&&fire&&time-meleeAt>.38f){facingRight=aim.x>=pos.x;SpecialAttack(grounded?"punch":"kick");}
            if(Cursed&&time-meleeAt>=.07f&&time-meleeAt<.22f)
            {
                float reach=meleeState=="kick"?58:meleeState=="uppercut"?38:44;
                var box=new Rect(pos.x+(facingRight?0:-reach),pos.y-(meleeState=="uppercut"?65:40),reach,meleeState=="uppercut"?65:35);
                foreach(var e in foes){if(e.hp<=0||e.boss&&(!e.awakened||e.phase==0)||struck.TryGetValue(e,out int hit)&&hit==meleeSerial)continue;
                    if(!box.Overlaps(new Rect(e.pos.x-20,e.pos.y-(e.boss?64:32),40,e.boss?64:32)))continue;
                    struck[e]=meleeSerial;e.hp--;e.hurt=time;Dust(e.pos-new Vector2(0,30));if(e.hp==0){kills++;coins+=e.boss?25:3;if(e.boss)health=maxHealth;else if(kills%3==0)health=Mathf.Min(maxHealth,health+1);}}
            }
            foreach(var h in hazards){float age=time-h.born;if(age<.85f||age>1.65f||h.spent)continue;
                if(new Rect(h.pos.x-16,h.pos.y-(index==1?105:46),32,index==1?105:46).Overlaps(Body)){
                    h.spent=true;if(h.trap){rootedUntil=time+1.1f;Dust(pos);}else Hurt("roots-or-curse");}}
            hazards.RemoveAll(h=>time-h.born>2.2f);
        }
        void BossStep(Foe e,float dt)
        {
            if(!e.awakened){if(pos.x<=1090||!grounded)return;e.awakened=true;e.timer=0;e.phase=0;bullets.Clear();damageAt=time+IntroSeconds;velocity=Vector2.zero;}
            e.timer+=dt;
            if(e.phase==0){if(e.timer>=IntroSeconds){e.phase=3;e.timer=0;}return;}
            if(e.phase==3){if(e.timer>=1.35f){e.phase=1;e.timer=0;e.attack=(e.attack+1)%3;e.chargeTargetX=pos.x;hazards.Add(new Hazard{pos=new Vector2(pos.x,pos.y),born=time,trap=index==0&&e.attack==1});}return;}
            if(e.phase==1){if(e.timer>=.9f){e.phase=2;e.timer=0;
                if(index==1&&e.attack==1&&RemainingMinions<3){foes.Insert(foes.Count-1,new Foe{pos=new Vector2(Mathf.Clamp(pos.x+130,1120,1450),320),floor=320,hp=1,maxHp=1,serial=spawned++,phase=0});}
                if(index==1&&e.attack==2)for(int i=-1;i<=1;i++)bullets.Add(new Bullet{pos=e.pos-new Vector2(0,55),velocity=new Vector2(-75+i*30,65),hostile=true,solar=true});}return;}
            if(e.phase==2){if(index==0&&e.attack==0)e.pos.x=Mathf.MoveTowards(e.pos.x,e.chargeTargetX,120*dt);
                if(e.timer>.65f){e.phase=3;e.timer=0;}}
        }
    }
}
