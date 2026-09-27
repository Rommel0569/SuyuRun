using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SuyuRun.Editor
{
    public static class SierraCaveChecks
    {
        static void Check(bool condition,string message){if(!condition)throw new Exception("Cave regression: "+message);}
        public static void Run()
        {
            var log=new System.Text.StringBuilder();
            Check(SierraCaveWorld.OvertimeAllowance(true,3)==5,"boss down grants five-second grace");
            Check(SierraCaveWorld.OvertimeAllowance(false,0)==5,"minions down grants five-second grace");
            Check(SierraCaveWorld.OvertimeAllowance(false,2)==0,"no progress does not grant grace");
            log.AppendLine("PASS five-second grace in either completed objective");
            var chargeWorld=new SierraCaveWorld(1,5);
            foreach(var e in chargeWorld.foes)if(!e.boss)e.hp=0;
            chargeWorld.nextSpawn=100;chargeWorld.pos=new Vector2(1130,320);
            var charger=chargeWorld.Boss;charger.awakened=true;charger.phase=1;charger.attack=2;
            for(int i=0;i<70;i++)chargeWorld.Step(.01f,0,false,false,false,Vector2.zero);
            float locked=charger.chargeTargetX;Check(charger.chargeLocked,"charge target locks during warning");
            chargeWorld.pos=new Vector2(1400,320);
            for(int i=0;i<100;i++)chargeWorld.Step(.01f,0,false,false,false,Vector2.zero);
            Check(charger.chargeTargetX==locked,"charge does not home after locking");
            Check(charger.pos.x<1200,"charge actually reaches the marked side");
            log.AppendLine("PASS aimed charge, delayed lock, committed direction");
            foreach(int fps in new[]{30,60,144})
            {
                float dt=1f/fps;
                var w=new SierraCaveWorld(0,3);
                w.Step(dt,0,true,false,false,Vector2.zero);
                Check(!w.grounded&&w.jumps==1&&w.velocity.y<0,"first jump");
                for(int f=0;f<fps/5;f++)w.Step(dt,0,false,false,false,Vector2.zero);
                w.Step(dt,0,true,false,false,Vector2.zero);Check(w.jumps==2,"double jump");
                w.Step(dt,0,true,false,false,Vector2.zero);Check(w.jumps==2,"third jump forbidden");
                for(int f=0;f<fps*2;f++)w.Step(dt,0,false,false,false,Vector2.zero);
                Check(w.grounded&&Mathf.Abs(w.pos.y-160)<.01f&&w.jumps==0,"landing resets jumps");
                w.pos=new Vector2(350,238);w.grounded=true;
                w.Step(dt,0,false,true,false,Vector2.zero);
                for(int f=0;f<fps;f++)w.Step(dt,0,false,false,false,Vector2.zero);
                Check(w.pos.y==288&&w.dropsUsed==1,"drop-through lands below");
                w=new SierraCaveWorld(0,3);float x=w.pos.x;w.Step(dt,-1,false,false,false,Vector2.zero);Check(w.pos.x<x,"A moves left");
                w.Step(dt,1,false,false,false,Vector2.zero);Check(w.pos.x>=x-.01f,"D moves right");
                log.AppendLine("PASS movement, two jumps, no third, landing, drop: "+fps+"fps");
            }
            for(int index=0;index<2;index++)
            {
                var magazine=new SierraCaveWorld(index,3);
                for(int i=0;i<120;i++)magazine.Step(1f/120,0,false,false,true,new Vector2(200,20));
                Check(magazine.shotsFired==5&&magazine.ammo==0&&magazine.Reloading,"five shots then automatic reload");
                float until=magazine.reloadUntil;
                while(magazine.time<until-.02f)magazine.Step(.01f,0,false,false,true,new Vector2(200,20));
                Check(magazine.shotsFired==5,"no firing during four-second reload");
                magazine.Step(.03f,0,false,false,false,Vector2.zero);
                Check(magazine.ammo==5&&!magazine.Reloading,"magazine refilled");
                log.AppendLine("PASS magazine 5, reload 4 seconds, firing lock");
                var waves=new SierraCaveWorld(index,3);
                for(int i=0;i<50;i++)
                {
                    waves.pos=new Vector2(650,250);waves.velocity=Vector2.zero;
                    foreach(var sentinel in waves.foes)if(!sentinel.boss)sentinel.hp=0;
                    waves.Step(1,0,false,false,false,Vector2.zero);
                }
                Check(waves.spawned==15&&waves.foes.Count(e=>!e.boss)==15,"fifteen-spawn budget, no unlimited duplicates");
                Check(waves.foes.Where(e=>!e.boss&&e.serial>=3).Select(e=>e.floor).Distinct().Count()>1,"waves use different platform heights");
                log.AppendLine("PASS fifteen spawns, multiple platform heights, fixed budget");
                var w=new SierraCaveWorld(index,3);w.pos=new Vector2(292,index==0?288:256);w.velocity=Vector2.zero;
                Check(w.torchPlacements.Count>=8,"sufficient wall lights");
                foreach(var torch in w.torchPlacements)
                    Check(!w.platforms.Any(p=>new Rect(torch.x-4,torch.y-4,torch.width+8,torch.height+8).Overlaps(p.bounds)),"torch clearance from all platforms");
                log.AppendLine("PASS cave wall lights: "+w.torchPlacements.Count+", all clear of platforms");
                var foe=w.foes[0];
                for(int i=0;i<120;i++)w.Step(1f/120,0,false,false,true,foe.pos-new Vector2(0,22));
                Check(foe.hp<=0&&w.kills>=1&&w.coins>=3,"pistol kills/rewards");
                w.pos=new Vector2(1120,320);w.Step(.01f,0,false,false,false,Vector2.zero);Check(w.Boss.awakened,"boss awakens");
                var introPos=w.pos;w.Step(1,1,true,false,true,Vector2.zero);
                Check(w.Cinematic&&w.pos==introPos,"cinematic freezes combat and movement");
                for(int f=0;f<610;f++)w.Step(.01f,0,false,false,false,Vector2.zero);
                w.pos=new Vector2(1000,320);w.Step(.01f,0,false,false,false,Vector2.zero);Check(w.pos.x>=1080,"arena locks behind player");
                w.Boss.hp=0;w.pos=new Vector2(1490,320);w.Step(.01f,0,false,false,false,Vector2.zero);Check(!w.cleared&&!w.ExitReady,"surviving minions block portal");
                foreach(var enemy in w.foes)enemy.hp=0;
                for(int f=0;f<290;f++)w.Step(.01f,0,false,false,false,Vector2.zero);
                Check(!w.cleared,"portal waits three seconds");
                w.pos.x=1400;w.Step(.01f,0,false,false,false,Vector2.zero);Check(w.exitCountdown==0,"leaving portal resets countdown");
                w.pos.x=1490;for(int f=0;f<310;f++)w.Step(.01f,0,false,false,false,Vector2.zero);Check(w.cleared,"all enemies plus three-second countdown opens exit");
                var clip=Resources.Load<AudioClip>("Caves/cueva_"+(index+1));Check(clip!=null,"audio import");
                Check(clip.length>59&&clip.length<61,"selected song approximately one minute");
                log.AppendLine("PASS cave "+(index+1)+": pistol, reward, boss awakening, arena, exit; audio="+clip.length.ToString("F4")+"s");
                w=new SierraCaveWorld(index,3);
                for(int step=0;step<7200&&!w.dead&&!w.cleared;step++)
                {float axis=0;bool jump=false,fire=false;Vector2 aim=Vector2.zero;w.AutoControl(ref axis,ref jump,ref fire,ref aim);w.Step(1f/120,axis,jump,false,fire,aim);}
                log.AppendLine("Autoplay cave "+(index+1)+": cleared="+w.cleared+" dead="+w.dead+" time="+w.time+" health="+w.health+" x="+w.pos.x+" y="+w.pos.y+" boss="+w.Boss.hp+" kills="+w.kills+" hits="+w.damageTrace);
                File.WriteAllText("Artifacts/SierraCaves/model-verification.txt",log.ToString());
                Check(w.cleared&&!w.dead,"combat is completable within the song at fixed steps");
                w=new SierraCaveWorld(index,3);
                for(int step=0;step<14400&&!w.dead&&!w.cleared;step++)
                {float axis=0;bool jump=false,fire=false;Vector2 aim=Vector2.zero;w.AutoControl(ref axis,ref jump,ref fire,ref aim);w.Step(step%2==0?1f/120:.0001f,axis,jump,false,fire,aim);}
                log.AppendLine("Variable steps cave "+index+": cleared="+w.cleared+" jumps="+w.jumpsUsed+" pos="+w.pos+" hits="+w.damageTrace);
                File.WriteAllText("Artifacts/SierraCaves/model-verification.txt",log.ToString());
                Check(w.cleared&&!w.dead,"variable DSP remainder must not teleport at platform lips");
            }
            Directory.CreateDirectory("Artifacts/SierraCaves");
            File.WriteAllText("Artifacts/SierraCaves/model-verification.txt",log+DateTime.Now.ToString("O"));
        }
    }
}
