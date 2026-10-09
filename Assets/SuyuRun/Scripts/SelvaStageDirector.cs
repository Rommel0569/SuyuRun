using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;
namespace SuyuRun
{
    public sealed partial class SelvaStageDirector:MonoBehaviour
    {
        [Serializable] public class CourseEvent {public int beat;public float time,x;public string kind;}
        [Serializable] public class Course {public float duration,speed;public CourseEvent[] events;public string[] zones;}
        [Serializable] class Timing {public float[] beatTimes;}
        sealed class Surface {public Rect bounds;public string art;}
        sealed class Fx {public string name;public Vector2 pos;public float born;}
        sealed class Shot {public Vector2 pos,velocity;public float age;}
        sealed class Drop {public Vector2 pos;public float vy;}
        public RunnerSoundtrack soundtrack;public Font pixelFont;public bool DiagnosticAutoplay;
        public string Status=>complete?"Complete":dead?"Dead":paused?"Paused":"Running";
        public int Zone=>zone;public Vector2 Hero=>pos;public float Elapsed=>clock;public bool InMini=>mini!=null;
        public IReadOnlyList<SelvaCaiman> Caimans=>caimans;public IReadOnlyList<SelvaVine> Vines=>vines;
        public bool IsHanging=>hanging!=null;
        readonly List<Surface> surfaces=new List<Surface>();readonly List<SelvaCaiman> caimans=new List<SelvaCaiman>();
        readonly List<SelvaVine> vines=new List<SelvaVine>();readonly List<Fx> effects=new List<Fx>();readonly List<Shot> shots=new List<Shot>();
        readonly HashSet<int> pickups=new HashSet<int>();readonly HashSet<int> damagedEvents=new HashSet<int>();
        readonly List<Drop> drops=new List<Drop>();
        readonly RunnerInputBuffer jumpBuffer=new RunnerInputBuffer();
        Course course;Camera cam;AudioSource music,outgoingMusic;MuseumAmbience museum;SelvaMiniWorld mini;
        readonly float[] sectionStarts={0,45,90,90,135,135};
        int pendingZone=-1;float closing;
        float endingAt=-1;bool finishAfterFade;
        float FinalPortalX=>zoneEnd-480;
        Vector2 pos=new Vector2(92,148),velocity,mouse;SelvaVine hanging,requested;
        float clock,routeTime,zoneStart,zoneEnd,cameraX,cameraY,hurtAt=-10,landAt=-10,releaseAt=-10,lastGround=-10,fireAt=-10,reloadAt=-1,dropAt=-10;
        int zone,health=5,maxHealth=5,jumps,ammo=5,coins,score,bank;bool grounded=true,slide,paused,dead,complete;
        double lastDsp;float transition=1,musicFade=1,caveWarningUntil=-1;
        public const float Speed=160,Gravity=470,Jump=205;
        void Awake()
        {
            Application.runInBackground=true;
            var c=new GameObject("Selva Pixel Camera");cam=c.AddComponent<Camera>();cam.orthographic=true;cam.orthographicSize=180f/64;cam.transform.position=new Vector3(5,180f/64,-10);cam.backgroundColor=new Color32(8,24,21,255);
            var pixel=c.AddComponent<PixelPerfectCamera>();pixel.assetsPPU=64;pixel.refResolutionX=640;pixel.refResolutionY=360;pixel.gridSnapping=PixelPerfectCamera.GridSnapping.UpscaleRenderTexture;pixel.cropFrame=PixelPerfectCamera.CropFrame.StretchFill;
            RunnerPixelViewport.Configure(c.GetComponent<Camera>());
            c.AddComponent<AudioListener>();
            course=JsonUtility.FromJson<Course>(Resources.Load<TextAsset>("SelvaIntegration/selva_course").text);
            soundtrack=Instantiate(soundtrack?soundtrack:Resources.Load<RunnerSoundtrack>("SelvaSoundtrack"));
            soundtrack.beatTimes=JsonUtility.FromJson<Timing>(Resources.Load<TextAsset>("SelvaIntegration/selva_midi_exact").text).beatTimes;
            soundtrack.firstBeatSeconds=0; soundTrackCheck();
            music=gameObject.AddComponent<AudioSource>();music.clip=soundtrack.clip;music.playOnAwake=false;
            outgoingMusic=gameObject.AddComponent<AudioSource>();outgoingMusic.playOnAwake=false;
            museum=gameObject.AddComponent<MuseumAmbience>();pixelFont=pixelFont?pixelFont:Resources.Load<Font>("PressStart2P-Regular");
            maxHealth=5+PlayerPrefs.GetInt("SuyuRun.Upgrade.Vida",0);health=maxHealth;bank=PlayerPrefs.GetInt("SuyuRun.Prototype.Bank");
            EnterZone(0);lastDsp=AudioSettings.dspTime;InitRendering();
        }
        void soundTrackCheck(){if(!soundtrack.clip)throw new InvalidOperationException("Selva soundtrack missing");}
        void EnterZone(int next)
        {
            zone=next;endingAt=-1;finishAfterFade=false;zoneStart=sectionStarts[zone]*Speed;zoneEnd=(zone==5?course.duration:zone==0?45:zone==1?90:zone==3?135:sectionStarts[zone])*Speed;
            pos=new Vector2(zoneStart+92,148);velocity=new Vector2(Speed,0);jumps=0;grounded=true;hanging=requested=null;transition=1;musicFade=0;lastGround=clock;
            surfaces.Clear();vines.Clear();caimans.Clear();effects.Clear();shots.Clear();drops.Clear();mini=null;
            if(zone==2||zone==4){mini=new SelvaMiniWorld(zone==2?0:1,Mathf.Max(health,3));mini.maxHealth=maxHealth;mini.health=health;}
            else if(zone==1)
            {
                for(float x=zoneStart;x<zoneEnd;x+=704)
                {
                    // Finish with a continuous bank, not a partial pond whose vines overlap land.
                    if(x+704>zoneEnd-180){surfaces.Add(new Surface{bounds=new Rect(x,148,zoneEnd-x+320,32),art="tierra"});break;}
                    surfaces.Add(new Surface{bounds=new Rect(x,148,192,32),art="tierra"});
                    surfaces.Add(new Surface{bounds=new Rect(x+640,148,64,32),art="tierra"});
                    for(int k=0;k<3;k++)
                    {vines.Add(new SelvaVine(new Vector2(x+260+k*136,8),k*.8f));surfaces.Add(new Surface{bounds=new Rect(x+254+k*136,149,48,12),art="victoria"});}
                    var croc=new SelvaCaiman(x+210,x+605,146,(int)x+327);croc.effect=SpawnEffect;caimans.Add(croc);
                }
                surfaces.Add(new Surface{bounds=new Rect(zoneEnd-180,148,300,32),art="tierra"});
            }
            else surfaces.Add(new Surface{bounds=new Rect(zoneStart,148,zoneEnd-zoneStart+320,32),art=zone==5?"ruina":"tierra"});
            if(mini==null&&zone!=1)foreach(var e in course.events)if(e.kind=="double"&&e.x>=zoneStart&&e.x<zoneEnd)
                surfaces.Add(new Surface{bounds=new Rect(e.x+80,112,64,12),art=zone==5?"ruina":"tierra"});
            // The departure has already faded down: never restart a copy of the old song.
            outgoingMusic.Stop();
            music.Stop();music.volume=0;
            music.clip=mini!=null?Resources.Load<AudioClip>(zone==2?"SelvaAudio/CuevaGuardian":"SelvaAudio/Chullachaqui"):soundtrack.clip;
            if(!music.clip)throw new InvalidOperationException("Falta la musica propia del mininivel Selva");
            music.time=mini!=null?0:Mathf.Min(music.clip.length-.1f,sectionStarts[zone]+soundtrack.clipStartSeconds);music.loop=mini!=null;music.PlayScheduled(AudioSettings.dspTime+.04);
        }
        void RequestZone(int next){if(pendingZone>=0)return;pendingZone=next;closing=0;}
        void Update()
        {
            double now=AudioSettings.dspTime;float dt=(float)Math.Max(0,now-lastDsp);lastDsp=now;
            var key=Keyboard.current;var m=Mouse.current;
            if(key!=null&&key.escapeKey.wasPressedThisFrame){TogglePause();}
            if(paused||dead||complete){Render();return;}
            mouse=PointerNative();
            if(key!=null&&(key.spaceKey.wasPressedThisFrame||mini!=null&&key.wKey.wasPressedThisFrame))jumpBuffer.Press(clock);
            slide=key!=null&&(key.sKey.isPressed||key.downArrowKey.isPressed)&&mini==null&&hanging==null;
            bool click=m!=null&&m.leftButton.wasPressedThisFrame;
            if(key!=null&&key.rKey.wasPressedThisFrame){if(mini!=null)mini.BeginReload();else Reload();}
            if(click&&mini==null)
            {
                // Generous invisible pointer target; closest knot wins when areas overlap.
                SelvaVine target=null;float nearest=44;
                foreach(var vine in vines)
                {
                    float distance=Vector2.Distance(vine.Grip,mouse+new Vector2(cameraX,0));
                    if(distance<nearest){nearest=distance;target=vine;}
                }
                if(target!=null){requested=target;click=false;}
            }
            bool fire=(m!=null&&m.leftButton.isPressed&&hanging==null&&requested==null)||key!=null&&key.eKey.isPressed;
            float axis=key==null?0:(key.dKey.isPressed||key.rightArrowKey.isPressed?1:0)-(key.aKey.isPressed||key.leftArrowKey.isPressed?1:0);
            bool autoJump=false;
            if(DiagnosticAutoplay){axis=1;if(mini!=null){mini.AutoControl(ref axis,ref autoJump,ref fire,ref mouse);mouse-=new Vector2(cameraX,cameraY);}}
            // DSP-based capped substeps: never gate a physical input on a MIDI beat.
            float remaining=Mathf.Min(dt,.25f);while(remaining>0){float step=Mathf.Min(remaining,1f/120);Step(step,axis,fire,autoJump);remaining-=step;autoJump=false;}
            Render();
        }
        public void Step(float dt,float axis=0,bool fire=false,bool jump=false)
        {
            if(paused||dead||complete)return;clock+=dt;transition=Mathf.Max(0,transition-dt/.9f);musicFade=Mathf.Min(1,musicFade+dt/1.25f);
            if(pendingZone>=0){closing+=dt;transition=Mathf.Clamp01(closing/.8f);float gain=1-Mathf.SmoothStep(0,1,transition);music.volume=.34f*gain*PlayerPrefs.GetFloat("SuyuRun.Settings.MusicVol",1);outgoingMusic.volume=0;if(closing>=.8f){int next=pendingZone;pendingZone=-1;if(next>5)Finish();else EnterZone(next);}return;}
            music.volume=.34f*Mathf.SmoothStep(0,1,musicFade)*PlayerPrefs.GetFloat("SuyuRun.Settings.MusicVol",1);
            outgoingMusic.volume=0;
            if(jump)jumpBuffer.Press(clock);
            if(mini!=null)
            {
                bool doJump=jumpBuffer.Pending(clock);if(doJump)jumpBuffer.Consume(clock);
                Vector2 aim=mouse+new Vector2(cameraX,cameraY);
                int previousShots=mini.shotsFired;
                mini.Step(dt,axis,doJump,Keyboard.current!=null&&Keyboard.current.sKey.isPressed,fire,aim);
                if(mini.shotsFired>previousShots)RunnerShotAudio.Play(gameObject);
                if(Keyboard.current!=null){if(Keyboard.current.fKey.wasPressedThisFrame)mini.SpecialAttack("kick");if(Keyboard.current.qKey.wasPressedThisFrame)mini.SpecialAttack("uppercut");if(Keyboard.current.leftShiftKey.isPressed)mini.Guard();}
                health=mini.health;dead=mini.dead;if(dead){music.Pause();outgoingMusic.Pause();}
                if(mini.AtPortal&&!mini.ExitReady&&mini.time>60)caveWarningUntil=clock+5;
                if(mini.cleared){coins+=mini.coins;score+=mini.kills*100;Collect(zone==2?1:2);RequestZone(zone+1);}
                return;
            }
            // The river follows its song section, even if a player spends longer swinging.
            // Begin the exit fade before the boundary; never pause the track and leave gameplay running.
            if(zone<5&&music.isPlaying&&music.time>=zoneEnd/Speed+soundtrack.clipStartSeconds-.8f){RequestZone(zone+1);return;}
            if(zone==5&&endingAt>=0)
            {
                float age=clock-endingAt;pos=new Vector2(FinalPortalX,148);velocity=Vector2.zero;
                float tail=Mathf.Max(.1f,soundtrack.clip.length-music.time);
                if(!finishAfterFade&&(tail<=1.4f||!music.isPlaying)){finishAfterFade=true;closing=0;}
                if(finishAfterFade){closing+=dt;music.volume=.34f*(1-Mathf.SmoothStep(0,1,closing/1.4f))*PlayerPrefs.GetFloat("SuyuRun.Settings.MusicVol",1);}
                if(age>=2.5f&&finishAfterFade&&closing>=1.4f)Finish();
                return;
            }
            float oldFeet=pos.y;
            if(jumpBuffer.Pending(clock))foreach(var e in course.events)if(e.kind=="double"&&e.x>=zoneStart&&e.x<zoneEnd&&Vector2.Distance(pos-new Vector2(0,20),new Vector2(e.x,102))<24)
            {jumps=0;score+=25;SpawnEffect("rastro_salto",pos);break;}
            if(hanging!=null)
            {
                hanging.Step(dt,1.35f+axis*1.8f);
                var grip=TaytaArt.GripOffset("swing",TaytaArt.SwingPose(hanging.angle*Mathf.Rad2Deg));
                pos=hanging.Grip+new Vector2(24-grip.x,47-grip.y);grounded=false;
                if(jumpBuffer.Pending(clock)){jumpBuffer.Consume(clock);Release();}
            }
            else
            {
                bool jumpNow=jumpBuffer.Pending(clock)&&(grounded||clock-lastGround<.10f||jumps<2);
                if(DiagnosticAutoplay)
                {
                    foreach(var e in course.events)if(e.kind!="slide"&&e.x-pos.x>16&&e.x-pos.x<75&&grounded)jumpNow=true;
                    if(zone==1&&grounded&&SurfaceAt(pos.x+25)==null)jumpNow=true;
                }
                if(jumpNow){jumpBuffer.Consume(clock);jumps=grounded||clock-lastGround<.1f?1:jumps+1;velocity.y=-Jump;grounded=false;slide=false;SpawnEffect("polvo",pos);}
                velocity.x=Mathf.MoveTowards(velocity.x,Speed,dt*240);pos.x+=velocity.x*dt;velocity.y+=Gravity*dt;pos.y+=velocity.y*dt;grounded=false;
                foreach(var s in surfaces)if(pos.x+8>s.bounds.xMin&&pos.x-8<s.bounds.xMax&&velocity.y>=0&&oldFeet<=s.bounds.y+.5f&&pos.y>=s.bounds.y)
                {pos.y=s.bounds.y;velocity.y=0;grounded=true;jumps=0;lastGround=clock;if(oldFeet<s.bounds.y-1){landAt=clock;SpawnEffect("polvo",pos);}break;}
                if(!grounded&&jumps==0&&clock-lastGround>=.1f)jumps=1;
                if(requested!=null&&clock-releaseAt>.12f&&Vector2.Distance(pos-new Vector2(0,46),requested.Grip)<38)Grab(requested);
                if(pos.y>205){Hurt();pos=new Vector2(zoneStart+Mathf.Floor((pos.x-zoneStart)/704)*704+140,148);velocity=Vector2.zero;grounded=true;jumps=0;}
            }
            foreach(var v in vines)if(v!=hanging)v.Step(dt);
            foreach(var c in caimans){c.Step(dt,pos.x);if(c.Hits(Body))Hurt();}
            for(int i=0;i<course.events.Length;i++)
            {
                var e=course.events[i];if(e.x<zoneStart||e.x>=zoneEnd||zone==5&&e.x>FinalPortalX-160||e.x<pos.x-60||e.x>pos.x+350)continue;
                Rect box=ObstacleBox(e);
                if(e.kind=="double"&&Vector2.Distance(pos-new Vector2(0,20),new Vector2(e.x,102))<24&&jumpBuffer.Pending(clock))
                {velocity.y=-Jump;jumps=1;jumpBuffer.Consume(clock);SpawnEffect("rastro_salto",pos);}
                else if(e.kind!="double"&&!damagedEvents.Contains(i)&&box.Overlaps(Body))
                {Hurt();damagedEvents.Add(i);}
            }
            for(int i=0;i<4;i++){float x=zoneStart+800+i*900;if(Mathf.Abs(pos.x-x)<20&&!pickups.Contains(zone*100+i)){pickups.Add(zone*100+i);coins+=3;score+=50;if((zone==0&&i==0)||(zone==3&&i==3))Collect(zone==0?0:3);SpawnEffect("destello",pos);}}
            if(fire&&clock-fireAt>.18f&&ammo>0&&reloadAt<0){ammo--;fireAt=clock;RunnerShotAudio.Play(gameObject);var origin=pos-new Vector2(0,23*GameScale.HeroScale);var direction=mouse+new Vector2(cameraX,0)-origin;shots.Add(new Shot{pos=origin,velocity=direction.normalized*510});if(ammo==0)Reload();}
            if(reloadAt>=0&&clock>=reloadAt){ammo=5;reloadAt=-1;}
            foreach(var shot in shots)
            {
                shot.pos+=shot.velocity*dt;shot.age+=dt;
                foreach(var c in caimans)if(!c.wounded&&(c.phase==SelvaCaiman.Phase.Breach||c.phase==SelvaCaiman.Phase.Snap||c.phase==SelvaCaiman.Phase.Fall)&&Vector2.Distance(shot.pos,c.pos)<22)
                {shot.age=3;c.wounded=true;c.phase=SelvaCaiman.Phase.Fall;c.velocity.y=Mathf.Max(120,c.velocity.y);c.timer=0;SpawnEffect("impacto",shot.pos);score+=30;break;}
                for(int i=0;i<course.events.Length;i++)if((course.events[i].kind=="root"||course.events[i].kind=="air")&&!damagedEvents.Contains(i)&&ObstacleBox(course.events[i]).Contains(shot.pos))
                {damagedEvents.Add(i);shot.age=3;SpawnEffect("astillas",shot.pos);coins++;score+=25;break;}
            }
            shots.RemoveAll(s=>s.age>2.5f);effects.RemoveAll(f=>clock-f.born>1.2f);
            // Traversal zones are spatial. Landing resumes the authored song section at a crossfade boundary.
            routeTime=(pos.x-92)/Speed;
            if(zone==5&&pos.x>=FinalPortalX){pos.x=FinalPortalX;pos.y=148;grounded=true;velocity=Vector2.zero;endingAt=clock;return;}
            if(zone<5&&pos.x>=zoneEnd+70){pos.x=zoneEnd+70;velocity.x=0;}
            if(zone==1)
            {
                if(clock-dropAt>.23f){dropAt=clock;drops.Add(new Drop{pos=new Vector2(zoneStart+Mathf.Floor((pos.x-zoneStart)/704)*704+620,68),vy=30});}
                foreach(var d in drops){d.vy+=160*dt;d.pos.y+=d.vy*dt;if(d.pos.y>=146){SpawnEffect("ondas",new Vector2(d.pos.x,146));SpawnEffect("espuma",new Vector2(d.pos.x,146));}}
                drops.RemoveAll(d=>d.pos.y>=146);
            }
        }
        Surface SurfaceAt(float x)=>surfaces.Find(s=>x>s.bounds.xMin&&x<s.bounds.xMax);
        public Rect Body=>GameScale.Body(pos,8,slide);
        Rect ObstacleBox(CourseEvent e)=>e.kind=="slide"?new Rect(e.x-12,105,32,24):e.kind=="air"?new Rect(e.x-12,90+Mathf.Sin(clock*3+e.beat)*5,24,24):new Rect(e.x-10,126,24,22);
        void Grab(SelvaVine v){hanging=v;v.occupied=true;v.omega=Mathf.Clamp(velocity.x/SelvaVine.Length,1.4f,2.4f);requested=null;velocity=Vector2.zero;jumps=0;SpawnEffect("hojas",v.Grip);}
        void Release(){velocity=hanging.Tangent;velocity.x=Mathf.Max(100,velocity.x);velocity.y=Mathf.Min(-80,velocity.y);hanging.occupied=false;hanging=null;releaseAt=clock;jumps=1;}
        void Reload(){if(ammo<5&&reloadAt<0)reloadAt=clock+4;}
        void Hurt(){if(clock-hurtAt<1||DiagnosticAutoplay)return;hurtAt=clock;health--;SpawnEffect("escudo_romper",pos-new Vector2(0,23));if(health<=0){dead=true;music.Pause();outgoingMusic.Pause();}}
        void SpawnEffect(string name,Vector2 at){effects.Add(new Fx{name=name,pos=at,born=clock});}
        void Collect(int i){score+=150;if(!DiagnosticAutoplay){PlayerPrefs.SetInt("SuyuRun.Museum.Selva."+SharedGameMenu.SelvaIds[i],1);PlayerPrefs.Save();}SpawnEffect("destello",pos);}
        void Finish(){complete=true;RunnerProgress.Finish("Selva",coins,score,DiagnosticAutoplay,out bank,out _);music.Stop();music.loop=false;musicFade=0;if(!DiagnosticAutoplay)RunnerFinalMovie.Begin(gameObject);}
        void TogglePause(){if(dead||complete)return;StopMenuPreview();paused=!paused;if(paused){menuState.tab=0;music.Pause();outgoingMusic.Pause();}else{ApplyMenuSettings();lastDsp=AudioSettings.dspTime;music.UnPause();if(musicFade<1)outgoingMusic.UnPause();}}
        void ReviveInPlace()
        {
            int available=coins+(mini?.coins??0);
            if(!dead||!RunnerProgress.TryRevive(ref available))return;
            coins=available;if(mini!=null)mini.coins=0;
            StopMenuPreview();dead=paused=false;health=maxHealth;
            hurtAt=clock+2;velocity=Vector2.zero;ammo=5;reloadAt=-1;
            if(mini!=null){mini.dead=false;mini.health=mini.maxHealth;mini.damageAt=mini.time+2;mini.velocity=Vector2.zero;mini.ammo=5;mini.reloadUntil=-1;}
            else if(pos.y>180)
            {
                var landing=surfaces.OrderBy(s=>Mathf.Abs(s.bounds.center.x-pos.x)).FirstOrDefault();
                if(landing!=null){pos=new Vector2(Mathf.Clamp(pos.x,landing.bounds.xMin+12,landing.bounds.xMax-12),landing.bounds.yMin);grounded=true;jumps=0;}
            }
            bank=PlayerPrefs.GetInt("SuyuRun.Prototype.Bank",0);
            lastDsp=AudioSettings.dspTime;music.UnPause();
        }
        void Retry()
        {
            // Every biome is a checkpoint. Explicit replay after completion starts a new run.
            if(complete){SceneManager.LoadScene(RunnerProgress.SelvaScene);return;}
            StopMenuPreview();dead=paused=false;health=maxHealth;pendingZone=-1;
            hurtAt=-10;reloadAt=-1;ammo=5;jumpBuffer.Consume(clock);
            EnterZone(zone);lastDsp=AudioSettings.dspTime;
        }
        Vector2 PointerNative()
        {
            if(Mouse.current==null)return new Vector2(200,90);
            var p=cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());return new Vector2(p.x*32,180-p.y*32);
        }
        public void DiagnosticZone(int z){EnterZone(z);Render();}
        public void DiagnosticBoss(){if(mini==null)return;mini.pos=new Vector2(1100,320);mini.Step(1f/120,0,false,false,false,Vector2.zero);Render();}
        public void DiagnosticCombat(){if(mini==null)return;mini.Boss.phase=3;mini.Boss.timer=0;mini.pos=new Vector2(mini.Boss.pos.x-35,320);mini.SpecialAttack("punch");Render();}
        public void DiagnosticGrab(int i){Grab(vines[i]);Render();}
        public void DiagnosticRelease()=>Release();
    }
}
