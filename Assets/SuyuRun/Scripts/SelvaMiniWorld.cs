using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SuyuRun
{
    // Exploration uses its own clock and coordinates (pixels, +Y down), never MIDI beats.
    public sealed partial class SelvaMiniWorld
    {
        public sealed class Platform { public Rect bounds; public bool oneWay; }
        public sealed class Foe
        {
            public Vector2 pos, velocity; public float home, floor, timer, direction=1, hurt=-10, jumpAt=-10;
            public int hp, maxHp, attack, serial; public bool boss, awakened, rewarded; public int phase;
            public float chargeTargetX; public bool chargeLocked;
        }
        public sealed class Bullet { public Vector2 pos, velocity; public bool hostile, dead, solar; public float age; }
        public sealed class Spark { public Vector2 pos; public float born; public bool dust; }
        public readonly List<Platform> platforms=new List<Platform>();
        public readonly List<Foe> foes=new List<Foe>();
        public readonly List<Bullet> bullets=new List<Bullet>();
        public readonly List<Spark> sparks=new List<Spark>();
        public readonly List<Vector2> treasures=new List<Vector2>();
        public readonly List<Rect> torchPlacements=new List<Rect>();
        public readonly int index;
        public const float Width=1536, Height=384, MoveSpeed=112, Jump=205, Gravity=470;
        public Vector2 pos=new Vector2(56,160), velocity;
        public float gaitDistance;
        const float HeroHalfWidth=9*TaytaArt.WorldScale,HeroHeight=34*TaytaArt.WorldScale;
        public float time, fireAt=-10, damageAt=-10, dropUntil, lastGround=-10;
        public bool grounded=true, facingRight=true, dead, cleared;
        public int jumps, health, maxHealth, coins, kills, shotsFired, jumpsUsed, dropsUsed;
        public string damageTrace="";
        public int ammo=5,reloads,spawned;
        public float reloadUntil=-1,nextSpawn=5;
        public const int Magazine=5,SpawnBudget=15;
        public const float ReloadSeconds=4,IntroSeconds=6;
        public bool Reloading=>time<reloadUntil;
        public bool Cinematic=>Boss.awakened&&Boss.phase==0&&Boss.hp>0;
        public Foe Boss=>foes[foes.Count-1];
        public int RemainingMinions=>foes.Count(e=>!e.boss&&e.hp>0);
        public bool ExitReady=>Boss.hp<=0&&RemainingMinions==0;
        public const float PortalX=1470,PortalFloor=320;
        public bool AtPortal=>Mathf.Abs(pos.x-(PortalX+24))<40&&Mathf.Abs(pos.y-PortalFloor)<3&&grounded;
        public float exitCountdown;
        public const float ExitDelay=3;
        public static float OvertimeAllowance(bool bossDefeated,int minionsRemaining)=>bossDefeated||minionsRemaining==0?5:0;
        public Rect Body=>GameScale.Body(pos,9);

        public SelvaMiniWorld(int caveIndex,int lives)
        {
            index=caveIndex;health=maxHealth=Mathf.Max(5,lives);
            if(index==0)
            {
                Add(0,160,224,224,false);Add(224,288,288,96,false);Add(512,320,1024,64,false);
                Add(176,218,112,12,true);Add(320,238,128,12,true);
                Add(464,190,112,12,true);Add(592,142,128,12,true);
                Add(736,210,112,12,true);Add(848,262,144,12,true);
                Add(1032,220,112,12,true);Add(1176,248,112,12,true);Add(1360,250,112,12,true);
                AddFoe(358,288,false);AddFoe(510,190,false);AddFoe(652,142,false);
                AddFoe(710,320,false);AddFoe(900,262,false);AddFoe(1010,320,false);
                treasures.AddRange(new[]{new Vector2(364,210),new Vector2(630,115),new Vector2(870,230),new Vector2(1090,195)});
            }
            else
            {
                // Crypt: split galleries, deeper central chamber, a climb out to the tomb.
                Add(0,160,192,224,false);Add(192,256,224,128,false);Add(416,320,352,64,false);
                Add(768,288,256,96,false);Add(1024,320,512,64,false);
                Add(152,208,96,12,true);Add(288,208,96,12,true);Add(416,160,112,12,true);
                Add(560,216,128,12,true);Add(688,260,96,12,true);Add(800,224,96,12,true);
                Add(928,168,112,12,true);Add(1040,224,112,12,true);Add(1200,240,96,12,true);Add(1360,248,112,12,true);
                AddFoe(340,256,false);AddFoe(482,160,false);AddFoe(624,216,false);AddFoe(714,320,false);
                AddFoe(840,224,false);AddFoe(980,288,false);AddFoe(968,168,false);AddFoe(398,256,false);
                treasures.AddRange(new[]{new Vector2(320,183),new Vector2(460,135),new Vector2(944,140),new Vector2(1224,213)});
            }
            AddFoe(1320,320,true);
            // Fifteen total minion appearances, not fifteen simultaneous bodies.
            while(foes.Count>4)foes.RemoveAt(foes.Count-2);
            spawned=3;
            BuildTorchPlacements();
        }
        void BuildTorchPlacements()
        {
            for(int i=0;i<12;i++)
            {
                bool placed=false;
                foreach(int dx in new[]{0,-24,24,-48,48})
                {
                    float x=80+i*128+dx;
                    if(x<16||x+16>Width-16)continue;
                    var floor=platforms.FirstOrDefault(p=>!p.oneWay&&x+8>=p.bounds.xMin&&x+8<p.bounds.xMax);
                    if(floor==null)continue;
                    foreach(int gap in new[]{64,104,144})
                    {
                        var r=new Rect(x,floor.bounds.y-gap,16,32);
                        var clearance=new Rect(r.x-4,r.y-4,r.width+8,r.height+8);
                        if(r.y<30||platforms.Any(p=>clearance.Overlaps(p.bounds))||torchPlacements.Any(p=>clearance.Overlaps(p)))continue;
                        torchPlacements.Add(r);placed=true;break;
                    }
                    if(placed)break;
                }
            }
        }
        void Add(float x,float y,float w,float h,bool oneWay)=>platforms.Add(new Platform{bounds=new Rect(x,y,w,h),oneWay=oneWay});
        void AddFoe(float x,float y,bool boss)=>foes.Add(new Foe{pos=new Vector2(x,y),home=x,floor=y,boss=boss,hp=boss?(index==0?15:18):1,maxHp=boss?(index==0?15:18):1,serial=foes.Count});
        public void BeginReload(){if(!Cursed&&ammo<Magazine&&!Reloading&&!dead&&!cleared){reloadUntil=time+ReloadSeconds;reloads++;}}
        public void Step(float dt,float axis,bool jump,bool drop,bool fire,Vector2 aim)
        {
            if(dead)return;
            time+=dt;
            // Music duration is not a death timer: finish the fight before leaving.
            if(reloadUntil>=0&&time>=reloadUntil){ammo=Magazine;reloadUntil=-1;}
            if(cleared){sparks.RemoveAll(s=>time-s.born>.6f);return;}
            if(Cinematic){BossStep(Boss,dt);return;}
            if(ExitReady&&(AtPortal||exitCountdown>0)){axis=0;jump=false;drop=false;fire=false;}
            SpawnReinforcement();
            if(drop&&grounded&&platforms.Any(p=>p.oneWay&&Mathf.Abs(pos.y-p.bounds.y)<2&&pos.x>=p.bounds.xMin&&pos.x<=p.bounds.xMax))
            {dropUntil=time+.22f;grounded=false;pos.y+=2;velocity.y=25;dropsUsed++;}
            if(jump&&(grounded||time-lastGround<.09f||jumps<2))
            {jumps=(grounded||time-lastGround<.09f)?1:jumps+1;grounded=false;lastGround=-10;velocity.y=-Jump;jumpsUsed++;Dust(pos);}
            if(axis!=0)facingRight=axis>0;
            TickMagic(dt,fire,aim);velocity.x=time<rootedUntil?0:axis*MoveSpeed;
            Vector2 before=pos;pos.x=Mathf.Clamp(pos.x+velocity.x*dt,12,Width-16);
            if(Boss.awakened&&Boss.hp>0)pos.x=Mathf.Max(1080,pos.x);
            foreach(var p in platforms)if(!p.oneWay&&Body.Overlaps(p.bounds))
            {
                // Only resolve a wall actually crossed this step. At the trailing lip of
                // a floor, tiny DSP remainders must not teleport the body to its far edge.
                if(velocity.x>0&&before.x+HeroHalfWidth<=p.bounds.xMin+.01f)pos.x=p.bounds.xMin-HeroHalfWidth;
                else if(velocity.x<0&&before.x-HeroHalfWidth>=p.bounds.xMax-.01f)pos.x=p.bounds.xMax+HeroHalfWidth;
            }
            float oldFeet=pos.y;velocity.y+=Gravity*dt;pos.y+=velocity.y*dt;grounded=false;
            foreach(var p in platforms)
            {
                if(pos.x+HeroHalfWidth<=p.bounds.xMin||pos.x-HeroHalfWidth>=p.bounds.xMax)continue;
                if(velocity.y>=0&&oldFeet<=p.bounds.yMin+.5f&&pos.y>=p.bounds.yMin&&(!p.oneWay||time>=dropUntil))
                {pos.y=p.bounds.yMin;velocity.y=0;grounded=true;jumps=0;lastGround=time;}
                else if(!p.oneWay&&velocity.y<0&&oldFeet-HeroHeight>=p.bounds.yMax&&pos.y-HeroHeight<p.bounds.yMax)
                {pos.y=p.bounds.yMax+HeroHeight;velocity.y=0;}
            }
            if(grounded)gaitDistance+=Mathf.Abs(pos.x-before.x);
            if(!grounded&&jumps==0&&time-lastGround>=.09f)jumps=1;
            if(pos.y>Height+40){Hurt("fall");pos=before;pos.y=160;pos.x=56;velocity=Vector2.zero;}
            if(fire&&!Cursed&&ammo>0&&!Reloading&&time-fireAt>=.15f)
            {
                facingRight=aim.x>=pos.x;fireAt=time;shotsFired++;ammo--;Vector2 origin=pos+new Vector2(facingRight?12:-12,-23)*TaytaArt.WorldScale;
                Vector2 direction=aim-origin;if(direction.sqrMagnitude<1)direction=new Vector2(facingRight?1:-1,0);
                bullets.Add(new Bullet{pos=origin,velocity=direction.normalized*510});
                if(ammo==0)BeginReload();
            }
            for(int i=treasures.Count-1;i>=0;i--)if(Vector2.Distance(pos-new Vector2(0,18),treasures[i])<24)
            {coins+=5;health=Mathf.Min(maxHealth,health+1);Dust(treasures[i]);treasures.RemoveAt(i);}
            foreach(var e in foes.ToArray())
            {
                if(e.hp<=0)continue;
                if(e.boss)BossStep(e,dt);else MinionStep(e,dt);
                if(e.phase!=0&&Mathf.Abs(e.pos.x-pos.x)<(e.boss?23:16)&&Mathf.Abs(e.pos.y-pos.y)<30)Hurt(e.boss?"boss":"contact");
                if(e.boss&&e.phase==2&&BossAttackBox(e).Overlaps(Body))Hurt("boss-combo");
            }
            foreach(var b in bullets)
            {
                if(b.dead)continue;
                b.pos+=b.velocity*dt;b.age+=dt;
                if(b.hostile){if(Body.Contains(b.pos)){Hurt("projectile");b.dead=true;}}
                else foreach(var e in foes)
                {
                    if(e.hp<=0||(e.boss&&(!e.awakened||e.phase==0)))continue;
                    if(new Rect(e.pos.x-(e.boss?25:13),e.pos.y-(e.boss?64:38),e.boss?50:26,e.boss?64:38).Contains(b.pos))
                    {e.hp--;e.hurt=time;b.dead=true;sparks.Add(new Spark{pos=b.pos,born=time});if(e.hp==0){kills++;coins+=e.boss?25:3;Dust(e.pos);if(e.boss)health=maxHealth;else if(kills%3==0)health=Mathf.Min(maxHealth,health+1);}break;}
                }
                if(platforms.Any(p=>!p.oneWay&&p.bounds.Contains(b.pos)))b.dead=true;
            }
            bullets.RemoveAll(b=>b.dead||b.age>2.5f);sparks.RemoveAll(s=>time-s.born>.6f);
            if(ExitReady&&AtPortal){pos.x=Mathf.MoveTowards(pos.x,PortalX+24,90*dt);velocity=Vector2.zero;exitCountdown+=dt;if(exitCountdown>=ExitDelay){cleared=true;pos=new Vector2(PortalX+24,PortalFloor);}}
            else exitCountdown=0;
        }
        void MinionStep(Foe e,float dt)
        {
            float dist=pos.x-e.pos.x;
            if(Mathf.Abs(dist)>420&&!Boss.awakened)return;
            if(!e.awakened){e.awakened=true;e.timer=0;}
            e.timer+=dt;
            if(e.phase==0){if(e.timer<1)return;e.phase=1;e.timer=0;}
            var support=platforms.FirstOrDefault(p=>Mathf.Abs(e.pos.y-p.bounds.y)<1&&e.pos.x>p.bounds.xMin&&e.pos.x<p.bounds.xMax);
            float targetX=pos.x;
            if(pos.y<e.pos.y-36)
            {
                var upper=platforms.Where(p=>p.bounds.y<e.pos.y-8&&p.bounds.y>e.pos.y-85&&p.bounds.xMin<e.pos.x+135&&p.bounds.xMax>e.pos.x-135)
                    .OrderBy(p=>Mathf.Abs(p.bounds.center.x-pos.x)+Mathf.Abs(p.bounds.y-pos.y)).FirstOrDefault();
                if(upper!=null)targetX=Mathf.Clamp(pos.x,upper.bounds.xMin+12,upper.bounds.xMax-12);
                if(support!=null&&time-e.jumpAt>1.2f){e.velocity.y=-285;e.jumpAt=time;}
            }
            e.direction=targetX>=e.pos.x?1:-1;
            float oldX=e.pos.x;e.pos.x=Mathf.Clamp(e.pos.x+e.direction*48*dt,16,Width-20);
            bool wall=false;
            foreach(var p in platforms)if(!p.oneWay&&new Rect(e.pos.x-9,e.pos.y-30,18,30).Overlaps(p.bounds))
            {
                if(e.direction>0&&oldX+9<=p.bounds.xMin+.01f){e.pos.x=p.bounds.xMin-9;wall=true;}
                else if(e.direction<0&&oldX-9>=p.bounds.xMax-.01f){e.pos.x=p.bounds.xMax+9;wall=true;}
            }
            if(wall&&support!=null&&time-e.jumpAt>1.2f){e.velocity.y=-285;e.jumpAt=time;}
            float oldY=e.pos.y;e.velocity.y+=Gravity*dt;e.pos.y+=e.velocity.y*dt;
            foreach(var p in platforms)
                if(e.pos.x+8>p.bounds.xMin&&e.pos.x-8<p.bounds.xMax&&oldY<=p.bounds.y+.5f&&e.pos.y>=p.bounds.y&&e.velocity.y>=0&&!(p.oneWay&&pos.y>oldY+45))
                {e.pos.y=p.bounds.y;e.velocity.y=0;}
            if(e.pos.y>Height){e.pos.y=320;e.velocity=Vector2.zero;}
            if(e.serial%3==0&&e.timer>3&&Mathf.Abs(dist)<190)
            {e.timer=0;var origin=e.pos+new Vector2(0,-23);bullets.Add(new Bullet{pos=origin,velocity=(pos-new Vector2(0,18)-origin).normalized*85,hostile=true});}
        }
        void SpawnReinforcement()
        {
            if(spawned>=SpawnBudget||time<nextSpawn||foes.Count(e=>!e.boss&&e.hp>0)>=(Boss.awakened?3:5)||Boss.hp<=0)return;
            bool above=spawned%2==0;
            var choices=platforms.Where(p=>p.bounds.width>=64&&p.bounds.xMax>pos.x-230&&p.bounds.xMin<pos.x+230&&p.bounds.y<365)
                .OrderBy(p=>(above?(p.bounds.y<pos.y-20?0:300):(p.bounds.y>=pos.y-5?0:300))+Mathf.Abs(p.bounds.center.x-pos.x)).ToArray();
            Platform platform=choices.FirstOrDefault(p=>Mathf.Abs(Mathf.Clamp(pos.x+(spawned%3==0?-150:150),p.bounds.xMin+16,p.bounds.xMax-16)-pos.x)>85);
            if(platform==null){nextSpawn=time+.5f;return;}
            float x=Mathf.Clamp(pos.x+(spawned%3==0?-150:150),platform.bounds.xMin+16,platform.bounds.xMax-16);
            var foe=new Foe{pos=new Vector2(x,platform.bounds.y),home=x,floor=platform.bounds.y,hp=1,maxHp=1,serial=spawned};
            foes.Insert(foes.Count-1,foe);spawned++;nextSpawn=time+2.5f;
        }
        void LegacyBossStep(Foe e,float dt)
        {
            if(!e.awakened){if(pos.x>1090&&grounded){e.awakened=true;e.timer=0;e.phase=0;bullets.Clear();damageAt=time+IntroSeconds;velocity=Vector2.zero;}else return;}
            e.timer+=dt;
            if(e.phase==0&&e.timer>=IntroSeconds){e.phase=3;e.timer=-.5f;damageAt=time;}
            else if(e.phase==1)
            {
                bool charge=index==1&&e.attack==2;
                if(!e.chargeLocked){e.direction=pos.x>=e.pos.x?1:-1;e.chargeTargetX=Mathf.Clamp(pos.x,1100,1440);}
                if(charge&&e.timer<.3f)e.pos.x=Mathf.Clamp(e.pos.x-e.direction*60*dt,1120,1440);
                if(charge&&e.timer>=.6f)e.chargeLocked=true;
                if(e.timer>=(charge?1.1f:.85f)){e.phase=2;e.timer=0;}
            }
            else if(e.phase==2)
            {
                float travel=index==0?(e.attack==0?185:e.attack==1?65:0):(e.attack==0?55:e.attack==1?0:245);
                // Preserve a retreat pocket: the boss must not pin a reloading player
                // against the arena seal with unavoidable overlapping hitboxes.
                bool charge=index==1&&e.attack==2;
                e.pos.x=charge?Mathf.MoveTowards(e.pos.x,e.chargeTargetX,420*dt):Mathf.Clamp(e.pos.x+e.direction*travel*dt,1200,1440);
                e.pos.y=e.floor-(index==0&&e.attack==1?Mathf.Sin(Mathf.Clamp01(e.timer/.7f)*Mathf.PI)*55:0);
                if(e.timer>=.7f||(charge&&e.timer>.12f&&Mathf.Abs(e.pos.x-e.chargeTargetX)<1))
                {
                    e.pos.y=e.floor;e.phase=3;e.timer=0;Dust(e.pos);
                    if(index==1&&e.attack==1)for(int i=-1;i<=1;i++)bullets.Add(new Bullet{pos=e.pos-new Vector2(0,42),velocity=new Vector2(e.direction*100,-55+i*50),hostile=true,solar=true});
                    if(index==0&&e.attack==2)foreach(int side in new[]{-1,1})bullets.Add(new Bullet{pos=e.pos-new Vector2(0,8),velocity=new Vector2(side*135,0),hostile=true});
                }
            }
            else if(e.phase==3)
            {
                if(index==1&&e.attack==2)e.pos.x=Mathf.MoveTowards(e.pos.x,1320,100*dt);
                if(e.timer>=1.65f){e.phase=1;e.timer=0;e.attack=(e.attack+1)%3;e.chargeLocked=false;}
            }
        }
        public Rect BossAttackBox(Foe e)
        {
            if(index==0&&e.attack==1)return new Rect(e.pos.x+(e.direction>0?0:-40),e.pos.y-92,40,78);
            if(index==1&&e.attack==1)return new Rect(-1000,-1000,1,1);
            if(index==1&&e.attack==2)return new Rect(e.pos.x+(e.direction>0?0:-35),e.pos.y-32,35,28);
            return new Rect(e.pos.x+(e.direction>0?0:-55),e.pos.y-52,55,39);
        }
        void Hurt(string reason){if(time<guardUntil||time-damageAt<1.25f)return;damageAt=time;health--;damageTrace+=$"{time:F2}:{reason}@{pos}; ";if(health<=0)dead=true;}
        void Dust(Vector2 p)=>sparks.Add(new Spark{pos=p,born=time,dust=true});
        // Diagnostic drives the same movement/projectile/collision path, without immunity.
        public void AutoControl(ref float axis,ref bool jump,ref bool fire,ref Vector2 aim)
        {
            var enemy=foes.Where(e=>e.hp>0&&(!e.boss||e.awakened)).OrderBy(e=>(e.pos-pos).sqrMagnitude).FirstOrDefault();
            axis=pos.x<1482?1:0;fire=enemy!=null&&Vector2.Distance(enemy.pos,pos)<300;
            if(enemy!=null)aim=enemy.pos-new Vector2(0,enemy.boss?34:23);
            if(enemy!=null&&!enemy.boss)
            {
                float lead=Mathf.Min(.45f,Vector2.Distance(enemy.pos,pos)/510);
                aim.x+=enemy.direction*48*lead;
                if(Mathf.Abs(enemy.velocity.y)>1)aim.y+=enemy.velocity.y*lead+.5f*Gravity*lead*lead;
            }
            // Do not spend a magazine on a one-hit sentinel while the first round
            // is still travelling. This is diagnostic aiming, not extra damage.
            if(enemy!=null&&!enemy.boss&&bullets.Any(b=>!b.hostile&&!b.dead))fire=false;
            if(fire)
            {
                Vector2 origin=pos-new Vector2(0,23),target=aim;
                for(int i=1;i<16;i++)if(platforms.Any(p=>!p.oneWay&&p.bounds.Contains(Vector2.Lerp(origin,target,i/16f)))){fire=false;break;}
            }
            if(Cursed&&Boss.hp>0){axis=Mathf.Abs(Boss.pos.x-pos.x)>34?Mathf.Sign(Boss.pos.x-pos.x):0;fire=Mathf.Abs(Boss.pos.x-pos.x)<68;jump=grounded&&Boss.phase==2;return;}
            if(Boss.awakened&&Boss.hp>0)
            {
                axis=0;
                if(Mathf.Abs(Boss.pos.x-pos.x)<120&&(Boss.phase==3||Boss.phase==1&&index==0&&Boss.attack==1))axis=Mathf.Sign(pos.x-Boss.pos.x);
                if(pos.x<1095)axis=1;if(pos.x>1440)axis=-1;
                jump=grounded&&!(index==0&&Boss.attack==1)&&(Boss.phase==1&&Boss.timer>(index==1&&Boss.attack==2?.75f:.45f)||Boss.phase==2);
                if(index==1&&Boss.attack==2&&Boss.phase==2&&!grounded&&jumps<2&&velocity.y>-20)jump=true;
                if(grounded&&enemy!=null&&!enemy.boss&&Mathf.Abs(enemy.pos.x-pos.x)<80)jump=true;
                if(grounded&&bullets.Any(b=>b.hostile&&Mathf.Abs(b.pos.x-pos.x)<75&&Mathf.Abs(b.pos.y-pos.y)<40))jump=true;
            }
            else
            {
                if(Boss.hp<=0&&enemy!=null)axis=Mathf.Abs(enemy.pos.x-pos.x)>140?Mathf.Sign(enemy.pos.x-pos.x):0;
                jump=grounded&&((enemy!=null&&Mathf.Abs(enemy.pos.x-pos.x)<70)||platforms.Any(p=>!p.oneWay&&p.bounds.xMin-pos.x>=8&&p.bounds.xMin-pos.x<45&&p.bounds.yMin<pos.y-3&&p.bounds.yMin>pos.y-80));
            }
        }
    }
}


