using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SuyuRun
{
    public sealed partial class SierraStageDirector
    {
        enum CavePhase { None, Entering, Explore, Leaving, Exiting }
        CavePhase cavePhase;
        SierraCaveWorld cave;
        AudioSource caveMusic;
        readonly bool[] cavesDone=new bool[2];
        readonly RunnerInputBuffer caveJump=new RunnerInputBuffer();
        double cavePhaseAt,caveEpoch,caveLast;
        int caveIndex;
        float caveLength=60,caveLimitElapsed;
        Vector2 caveCamera;
        string caveFailure="";
        double runnerFadeAt=-10;
        bool caveDeadlineChecked;
        float caveExtraSeconds;
        void UpdateMusicTransition()
        {
            double now=AudioSettings.dspTime;
            float main=cavePhase==CavePhase.Entering?1-Mathf.Clamp01(CavePhaseAge/1.6f):cavePhase==CavePhase.None?Mathf.Clamp01((float)(now-runnerFadeAt)/1.2f):0;
            if(cavePhase==CavePhase.None)main*=Mathf.Clamp01((duration-seconds)/1.6f);
            music.volume=.34f*liveMusicVolume*main;
            if(caveMusic)
            {
                float incoming=Mathf.Clamp01((float)(now-caveEpoch)/1.2f);
                float outgoing=cave!=null&&cave.exitCountdown>0?1-Mathf.Clamp01(cave.exitCountdown/SierraCaveWorld.ExitDelay):Mathf.Clamp01((caveLength-caveLimitElapsed)/1.5f);
                caveMusic.volume=.34f*liveMusicVolume*incoming*outgoing;
            }
        }
        float CavePhaseAge=>(float)((paused?pauseDsp:AudioSettings.dspTime)-cavePhaseAt);
        public string CaveReport=>$"Cave={cavePhase}; Done={cavesDone.Count(v=>v)}/2; Local={caveLimitElapsed:F2}; Kills={cave?.kills??0}; Spawn={cave?.spawned}; Reloads={cave?.reloads}; Jumps={cave?.jumpsUsed??0}; Position={cave?.pos}; HP={cave?.health}; Boss={cave?.Boss.hp}; Remaining={cave?.RemainingMinions}; Exit={cave?.exitCountdown:F2}; Extra={caveExtraSeconds}; Music={music.volume:F3}/{(caveMusic?caveMusic.volume:0):F3}; Hits={cave?.damageTrace}";
        public bool InCave=>cavePhase!=CavePhase.None;
        public bool CavePlaying=>cavePhase==CavePhase.Explore;
        public bool CaveIntroduction=>CavePlaying&&cave!=null&&cave.Cinematic&&cave.Boss.timer>2;
        public float CaveElapsed=>caveLimitElapsed;
        public int CavesCompleted=>cavesDone.Count(v=>v);
        public void DiagnosticStartCave(int index)
        {
            if(!DiagnosticAutoplay)return;
            interlude=Interlude.None;act=index;seconds=duration*(index+1)/3;
            feet=Surface(seconds*Speed+HeroX,act);BeginCave(index);
        }
        void ResetCaves(bool resume)
        {
            if(caveMusic)caveMusic.Stop();cavePhase=CavePhase.None;cave=null;caveJump.Clear();
            if(!resume){cavesDone[0]=cavesDone[1]=false;}
        }
        void BeginCave(int index)
        {
            caveIndex=index;cavePhase=CavePhase.Entering;cavePhaseAt=AudioSettings.dspTime;
            ClearActionInputs();hovered=locked=null;slide=false;
            cave=new SierraCaveWorld(index,MaxHealth);caveFailure="";
            if(!caveMusic){caveMusic=gameObject.AddComponent<AudioSource>();caveMusic.playOnAwake=false;}
            caveMusic.clip=Resources.Load<AudioClip>("Caves/cueva_"+(index+1));caveMusic.loop=false;
            if(!caveMusic.clip){Debug.LogError("Missing cave soundtrack "+(index+1));enabled=false;return;}
            caveLength=caveMusic.clip.length;
            caveCamera=Vector2.zero;caveLimitElapsed=0;
        }
        void StartCaveExploration()
        {
            cavePhase=CavePhase.Explore;cavePhaseAt=AudioSettings.dspTime;
            caveDeadlineChecked=false;caveExtraSeconds=0;
            music.Pause();
            caveEpoch=AudioSettings.dspTime+.1;caveLast=caveEpoch;caveJump.Clear();
            caveMusic.Stop();caveMusic.volume=0;caveMusic.time=0;caveMusic.PlayScheduled(caveEpoch);
        }
        void RetryCave()
        {
            if(cavePhase!=CavePhase.Explore)return;
            cave=new SierraCaveWorld(caveIndex,MaxHealth);caveFailure="";caveLimitElapsed=0;
            caveCamera=Vector2.zero;paused=false;StartCaveExploration();
        }
        void ToggleCavePause()
        {
            if(cave==null||cave.dead)return;
            caveJump.Clear();ClearActionInputs();
            if(!paused){paused=true;pauseDsp=AudioSettings.dspTime;caveMusic.Pause();if(cavePhase==CavePhase.Entering)music.Pause();}
            else
            {
                double offset=AudioSettings.dspTime-pauseDsp;paused=false;
                caveEpoch+=offset;caveLast+=offset;cavePhaseAt+=offset;
                if(cavePhase==CavePhase.Explore)caveMusic.UnPause();
                if(cavePhase==CavePhase.Entering)music.UnPause();
            }
        }
        bool UpdateCave(Keyboard k,Mouse m)
        {
            if(cavePhase==CavePhase.None)return false;
            double now=AudioSettings.dspTime;
            if(cavePhase==CavePhase.Entering){if(CavePhaseAge>=1.8f)StartCaveExploration();return true;}
            if(cavePhase==CavePhase.Leaving)
            {
                if(CavePhaseAge>=1)
                {
                    cavesDone[caveIndex]=true;coins+=cave.coins;score+=cave.kills*100;
                    health=Mathf.Min(MaxHealth,Mathf.Max(health,cave.health));act=caveIndex+1;feet=Surface(seconds*Speed+HeroX,act);
                    verticalSpeed=0;grounded=true;jumps=0;damageAt=seconds;
                    cavePhase=CavePhase.Exiting;cavePhaseAt=now;caveMusic.Stop();
                }
                return true;
            }
            if(cavePhase==CavePhase.Exiting)
            {
                if(CavePhaseAge>=1.3f){cavePhase=CavePhase.None;ClearActionInputs();EndInterlude(now);}
                return true;
            }
            if(cave.dead){caveMusic.Pause();return true;}
            if(k!=null&&(k.spaceKey.wasPressedThisFrame||k.wKey.wasPressedThisFrame||k.upArrowKey.wasPressedThisFrame))caveJump.Press(now);
            bool drop=k!=null&&(k.sKey.wasPressedThisFrame||k.downArrowKey.wasPressedThisFrame);
            float axis=k==null?0:(k.dKey.isPressed||k.rightArrowKey.isPressed?1:0)-(k.aKey.isPressed||k.leftArrowKey.isPressed?1:0);
            bool fire=(m!=null&&m.leftButton.isPressed)||(k!=null&&k.eKey.isPressed);
            Vector2 aim=cave.pos+new Vector2(cave.facingRight?100:-100,-23);
            if(m!=null&&view.pixelRect.Contains(m.position.ReadValue()))
            {Vector3 p=view.ScreenToWorldPoint(m.position.ReadValue());aim=new Vector2(p.x*32,180-p.y*32)+caveCamera;}
            double target=now;
            int oldShots=cave.shotsFired,oldJumps=cave.jumpsUsed,oldHealth=cave.health,oldKills=cave.kills;
            while(caveLast<target)
            {
                float dt=(float)System.Math.Min(RunnerMotion.MaxStep,target-caveLast);caveLast+=dt;
                bool jump=caveJump.Pending(now)&&(cave.grounded||cave.jumps<2);
                if(jump)caveJump.Consume(now);
                if(DiagnosticAutoplay)CaveAutoControl(ref axis,ref jump,ref fire,ref aim);
                cave.Step(dt,axis,jump,drop,fire,aim);drop=false;
                if(cave.dead)break;
            }
            if(cave.shotsFired!=oldShots)Cue(660);
            if(cave.jumpsUsed!=oldJumps)Cue(520);
            if(cave.health<oldHealth)Cue(120);
            if(cave.kills!=oldKills)Cue(900);
            caveLimitElapsed=Mathf.Max(0,(float)(now-caveEpoch));
            if(caveLimitElapsed>=caveLength&&!caveDeadlineChecked)
            {caveDeadlineChecked=true;caveExtraSeconds=SierraCaveWorld.OvertimeAllowance(cave.Boss.hp<=0,cave.RemainingMinions);}
            if(cave.cleared&&!cave.dead){cavePhase=CavePhase.Leaving;cavePhaseAt=now;caveMusic.Stop();}
            else if(caveLimitElapsed>=caveLength+caveExtraSeconds&&!cave.dead&&cave.exitCountdown<=0)
            {
                caveFailure="EL TIEMPO TERMINO · JEFE Y ESBIRROS";cave.dead=true;caveMusic.Stop();
            }
            Vector2 desired=new Vector2(Mathf.Clamp(cave.pos.x-125,0,SierraCaveWorld.Width-320),Mathf.Clamp(cave.pos.y-120,0,SierraCaveWorld.Height-180));
            if(cave.Boss.awakened&&cave.Boss.hp>0)
                desired=new Vector2(Mathf.Clamp((cave.pos.x+cave.Boss.pos.x)*.5f-160,0,SierraCaveWorld.Width-320),Mathf.Clamp(Mathf.Min(cave.pos.y-110,cave.Boss.floor-132),0,SierraCaveWorld.Height-180));
            if(cave.Cinematic)desired=new Vector2(Mathf.Clamp(cave.Boss.pos.x-160,0,SierraCaveWorld.Width-320),Mathf.Clamp(cave.Boss.floor-132,0,SierraCaveWorld.Height-180));
            caveCamera=Vector2.MoveTowards(caveCamera,desired,180*(float)System.Math.Min(.1,Time.unscaledDeltaTime));
            return true;
        }
        void CaveAutoControl(ref float axis,ref bool jump,ref bool fire,ref Vector2 aim)
        {
            cave.AutoControl(ref axis,ref jump,ref fire,ref aim);
        }
        void CaveDraw(string name,float x,float y,int order,float t=-1)
        {Draw(name,x-caveCamera.x,y-caveCamera.y,order,t<0?cave.time:t);}
        void RenderCave()
        {
            used=0;
            RunnerPresentation.CameraPose(view,new Vector3(5,180f/64,-10),Vector3.zero,0,false);
            string prefix="cueva"+(caveIndex+1);
            foreach(var layer in new[]{("fondo",.15f,0),("arcos",.4f,1)})
            {
                float x=-Mathf.Repeat(caveCamera.x*layer.Item2,640),y=-caveCamera.y*layer.Item2;
                Draw(prefix+"_"+layer.Item1,x,y,layer.Item3,cave.time);Draw(prefix+"_"+layer.Item1,x+640,y,layer.Item3,cave.time);
            }
            foreach(var torch in cave.torchPlacements)
            {
                float x=torch.x,y=torch.y;
                if(x-caveCamera.x<-30||x-caveCamera.x>350)continue;
                CaveDraw("cueva_antorcha",x,y,3);
            }
            foreach(var p in cave.platforms)
            {
                if(p.bounds.xMax<caveCamera.x||p.bounds.xMin>caveCamera.x+320)continue;
                if(p.oneWay)for(float x=p.bounds.xMin;x<p.bounds.xMax;x+=16)CaveDraw("cueva_plataforma",x,p.bounds.y,7);
                else for(float y=p.bounds.y;y<p.bounds.yMax;y+=32)for(float x=p.bounds.xMin;x<p.bounds.xMax;x+=32)
                    if(x>caveCamera.x-32&&x<caveCamera.x+320&&y>caveCamera.y-32&&y<caveCamera.y+180)
                        CaveDraw(y==p.bounds.y?"cueva_suelo":"cueva_roca",x,y,6);
            }
            CaveDraw(caveIndex==0?"cueva_monolito_base":"cueva_tumba",1275,256,4);
            if(caveIndex==1)for(int i=0;i<4;i++)CaveDraw("cueva_oro",1200+i*64,288,5);
            foreach(var e in cave.foes)
            {
                if(e.pos.x-caveCamera.x<-80||e.pos.x-caveCamera.x>400)continue;
                string name=e.boss?(caveIndex==0?"cueva_jefe_piedra":"cueva_jefe_guerrero"):(caveIndex==0?"cueva_esbirro_piedra":"cueva_esqueleto");
                if(e.hp<=0)
                {
                    if(cave.time-e.hurt<.6f)
                    {
                        if(caveIndex==1&&!e.boss)CaveDraw("cueva_esqueleto_despertar",e.pos.x-16,e.pos.y-48,10,Mathf.Max(0,.83f-(cave.time-e.hurt)*1.4f));
                        else CaveDraw("fx_escombros_piedra",e.pos.x-16,e.pos.y-26,11,cave.time-e.hurt);
                    }
                    continue;
                }
                float lift=e.boss&&e.phase==0?(e.awakened?Mathf.Clamp01((e.timer-2)/3):0):1;
                float y=e.pos.y-(e.boss?80:48)+(e.boss?(1-lift)*42:0);
                if(e.boss&&e.awakened&&e.phase!=0)
                {
                    string attack=caveIndex==0?new[]{"jab","upper","slam"}[e.attack]:new[]{"maza","sol","carga"}[e.attack];
                    string sprite="cueva_"+(caveIndex==0?"box_":"inca_")+(e.phase==2?attack:"guardia");
                    CaveDraw(sprite,e.pos.x-48,e.pos.y-95,10,e.phase==2?e.timer:cave.time);
                }
                else if(caveIndex==1&&!e.boss&&e.phase==0)CaveDraw("cueva_esqueleto_despertar",e.pos.x-16,y,10,e.awakened?e.timer:0);
                else CaveDraw(name,e.pos.x-(e.boss?32:16),y,e.boss?(e.phase==0?5:8):10,e.boss&&!e.awakened?0:cave.time);
                pool[used-1].flipX=e.direction<0;
                if(!e.boss&&e.phase==0&&e.awakened)CaveDraw("fx_teleport_anillo",e.pos.x-32,e.pos.y-20,11,e.timer);
                if(e.boss&&e.phase==1&&e.awakened)CaveDraw("ui_reticula",e.pos.x-8,e.pos.y-88,14);
                if(e.boss&&e.phase==2)
                {CaveDraw("cueva_golpe",e.pos.x+e.direction*26-16,e.pos.y-(e.attack==1?84:46),11,e.timer);pool[used-1].flipX=e.direction<0;}
            }
            foreach(var p in cave.treasures)CaveDraw("coleccionable_moneda",p.x-8,p.y-8,11);
            foreach(var b in cave.bullets)CaveDraw(b.solar?"cueva_proyectil_sol":b.hostile?"piedra":"cueva_bala",b.pos.x-(b.solar?6:4),b.pos.y-(b.solar?6:4),12);
            foreach(var s in cave.sparks)CaveDraw(s.dust?"fx_polvo_aterrizar":"fx_chispas",s.pos.x-16,s.pos.y-20,14,cave.time-s.born);
            string pose=cave.dead?"hit":cave.time-cave.fireAt<.14f?"rafaga":!cave.grounded?"jump":Mathf.Abs(cave.velocity.x)>1?"run":"idle";
            CaveDraw("heroe_"+pose,cave.pos.x-24,cave.pos.y-47,12);pool[used-1].flipX=!cave.facingRight;
            CaveDraw("cueva_pistola",cave.pos.x+(cave.facingRight?8:-20),cave.pos.y-27,13);pool[used-1].flipX=!cave.facingRight;
            if(cave.grounded&&Mathf.Abs(cave.velocity.x)>1)CaveDraw("fx_polvo_correr",cave.pos.x-20,cave.pos.y-16,11);
            CaveDraw("tunel_salida",1460,320-Frames("tunel_salida")[0].rect.height,9);
            if(Mouse.current!=null&&!cave.dead&&!paused&&!cave.Cinematic&&view.pixelRect.Contains(Mouse.current.position.ReadValue()))
            {var p=view.ScreenToWorldPoint(Mouse.current.position.ReadValue());Draw("ui_reticula",p.x*32-8,180-p.y*32-8,20,cave.time);}
            for(int i=used;i<pool.Count;i++)pool[i].gameObject.SetActive(false);
        }
        void DrawCaveGUI()
        {
            var text=new GUIStyle(GUI.skin.label){font=pixelFont,fontSize=5,wordWrap=true,normal={textColor=new Color32(255,243,208,255)}};
            if(cavePhase==CavePhase.Entering||cavePhase==CavePhase.Exiting)
            {
                RunnerPresentation.Curtain(cavePhase==CavePhase.Entering?(CavePhaseAge-.7f)/1.1f:1-CavePhaseAge/.9f);
                return;
            }
            if(cave.Cinematic&&!paused)
            {
                RunnerPresentation.Box(new Rect(0,0,320,18),RunnerPresentation.Ink);
                RunnerPresentation.Box(new Rect(0,135,320,45),RunnerPresentation.Ink);
                string title=caveIndex==0?"LA CALAVERA DE PIEDRA":"PACHACUTEC · EL INCA CAIDO";
                string history=caveIndex==0?"Chavin destaco por sus esculturas de piedra y galerias subterraneas.":"Pachacutec expandio el poder inca en el siglo XV.";
                string fiction=caveIndex==0?"FICCION: la Calavera de Piedra cobra vida para protegerlas.":"FICCION: el Inca caido despierta para custodiar esta cripta.";
                GUI.Label(new Rect(10,4,300,12),title,text);
                GUI.Label(new Rect(12,145,296,28),cave.Boss.timer<3?history:fiction,text);
                return;
            }
            RunnerPresentation.Box(new Rect(0,0,320,22),RunnerPresentation.Ink);
            GUI.Label(new Rect(5,3,164,9),(caveIndex==0?"GALERIA DEL MONOLITO":"CRIPTA DEL INCA")+" · "+cave.health+"/"+cave.maxHealth,text);
            for(int i=0;i<cave.maxHealth;i++)
            {
                float x=173+i*12;
                RunnerPresentation.Box(new Rect(x,3,11,7),RunnerPresentation.Ink);
                Color fill=i<cave.health?new Color32(158,173,86,255):cave.time-cave.damageAt<.6f&&i==cave.health?new Color32(237,146,84,255):RunnerPresentation.Stone;
                RunnerPresentation.Box(new Rect(x+1,4,9,5),fill);
            }
            GUI.Label(new Rect(265,3,53,9),(caveExtraSeconds>0?"EXTRA ":"")+Mathf.Max(0,Mathf.CeilToInt(caveLength+caveExtraSeconds-caveLimitElapsed))+"s",text);
            GUI.Label(new Rect(5,13,310,10),"A/D · W/ESP x2 · S BAJAR · CLICK/E PISTOLA",text);
            GUI.Label(new Rect(5,24,190,10),cave.Reloading?"RECARGA "+Mathf.Max(0,cave.reloadUntil-cave.time).ToString("F1")+"s":"BALAS "+cave.ammo+"/5 · R RECARGAR",text);
            for(int i=0;i<5;i++)RunnerPresentation.Box(new Rect(5+i*7,35,4,7),i<cave.ammo?RunnerPresentation.Gold:RunnerPresentation.Stone);
            GUI.Label(new Rect(219,24,96,10),"VIVOS "+cave.RemainingMinions,text);
            if(cave.Boss.awakened&&cave.Boss.hp>0)
            {
                RunnerPresentation.Box(new Rect(75,36,170,6),RunnerPresentation.Ink);
                RunnerPresentation.Box(new Rect(76,37,168*cave.Boss.hp/cave.Boss.maxHp,4),RunnerPresentation.Gold);
                if(cave.Boss.phase==1)GUI.Label(new Rect(74,47,245,10),caveIndex==0?new[]{"DIRECTO · SALTA","UPPERCUT · ALEJATE","GOLPE AL SUELO · SALTA"}[cave.Boss.attack]:new[]{"MAZA · SALTA","PODER SOLAR · ESQUIVA","CARGA REAL · SALTA"}[cave.Boss.attack],text);
            }
            if(caveIndex==1&&cave.Boss.attack==2&&cave.Boss.phase==1)
            {
                float targetX=Mathf.Round(cave.Boss.chargeTargetX-caveCamera.x),floor=Mathf.Round(cave.Boss.floor-caveCamera.y);
                for(int y=0;y<48;y+=8)RunnerPresentation.Box(new Rect(targetX-1,floor-y-5,3,4),new Color32(237,146,84,255));
                GUI.Label(new Rect(targetX-18,floor-60,65,10),cave.Boss.chargeLocked?"FIJADO":"APUNTANDO",text);
            }
            string hint=cave.exitCountdown>0?"SALIDA EN "+Mathf.Max(0,Mathf.CeilToInt(SierraCaveWorld.ExitDelay-cave.exitCountdown))+" · MANTENTE EN EL PORTAL":(cave.AtPortal||caveExtraSeconds>0)&&!cave.ExitReady?(cave.Boss.hp>0?"FALTA DERROTAR AL JEFE":"FALTAN "+cave.RemainingMinions+" ESBIRROS · DERROTALOS"):cave.ExitReady?"TODOS VENCIDOS · SALIDA A LA DERECHA":caveLimitElapsed<5?"Derrota al jefe Y a sus esbirros; llega al portal":"";
            if(hint.Length>0&&!cave.Cinematic){RunnerPresentation.Box(new Rect(0,164,320,16),RunnerPresentation.Ink);GUI.Label(new Rect(6,167,308,12),hint,text);}
            if(cave.dead||paused)
            {
                RunnerPresentation.Box(new Rect(50,48,220,100),RunnerPresentation.Ink);
                GUI.Label(new Rect(64,60,192,24),paused?"PAUSA":caveFailure.Length>0?caveFailure:"INTENTALO DE NUEVO",text);
                GUI.Label(new Rect(64,88,192,18),"La cueva se reinicia sin perder el runner.",text);
                if(RunnerPresentation.Button(new Rect(64,115,192,17),paused?"CONTINUAR":"REINTENTAR CUEVA",pixelFont))
                {if(paused)ToggleCavePause();else RetryCave();}
            }
            if(cavePhase==CavePhase.Leaving)RunnerPresentation.Curtain(CavePhaseAge);
            else if(caveLimitElapsed<.7f)RunnerPresentation.Curtain(1-caveLimitElapsed/.7f);
        }
    }
}
