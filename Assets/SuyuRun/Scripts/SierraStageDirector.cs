using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

namespace SuyuRun
{
    // Sierra terrain/combat; shared wallet, pickup data and scene flow.
    public sealed partial class SierraStageDirector : MonoBehaviour
    {
        [Serializable] sealed class Spec { public int[] surfaceY; public float fps; public bool loop; }
        [Serializable] sealed class Hit { public float t; public string kind; }
        [Serializable] sealed class Rhythm { public Hit[] hits; }
        [Serializable] sealed class Wave { public float t; public int count; }
        [Serializable] sealed class Course { public Hit[] events; public MobilitySequence[] mobility; public Wave[] waves; }
        [Serializable] sealed class MidiTiming { public float[] beatTimes; }
        sealed class Enemy { public float x,y,anchor,activeAt=-1,attackAt,phase; public Vector2 velocity; public string kind; public double lastAttack=-100; public bool air,dead,attacker; public int bridge=-1; }
        sealed class Shot { public Vector2 origin,velocity,position; public Enemy target; public double born; public bool dead,hostile,guided; }
        sealed class Bridge { public float x; public int life=3; public double hit=-100; }
        sealed class Effect { public string name; public float x,y,time; }
        public RunnerSoundtrack soundtrack;
        public Font pixelFont;
        public bool DiagnosticAutoplay;
        public string Status => complete ? "Complete" : dead||(cavePhase!=CavePhase.None&&cave!=null&&cave.dead) ? "Dead" : paused ? "Paused" : "Running";
        public float Progress => seconds;
        public bool IsSliding => slide&&grounded&&!dead&&interlude==Interlude.None;
        const float Speed=224, HeroX=100, Gravity=420, JumpSpeed=190;
        // Same standing/crouch proportion and grounded presentation as Costa.
        const float CrouchRatio=.48f/1.06f;
        readonly string[] acts={"camino","valle","ruinas"};
        readonly Dictionary<string,Sprite[]> sprites=new Dictionary<string,Sprite[]>();
        readonly Dictionary<string,Spec> specs=new Dictionary<string,Spec>();
        readonly List<SpriteRenderer> pool=new List<SpriteRenderer>();
        readonly List<Enemy> enemies=new List<Enemy>();
        readonly List<Shot> shots=new List<Shot>();
        readonly List<Bridge> bridges=new List<Bridge>();
        readonly List<Effect> effects=new List<Effect>();
        readonly HashSet<int> collected=new HashSet<int>();
        readonly List<RunnerPickup> pickups=new List<RunnerPickup>();
        Camera view; AudioSource music; Material material; GUIStyle label;
        RunnerSoundtrack midiTimeline;
        double epoch,previous,shotAt=-10,damageAt=-10,endingAt;
        float seconds,feet=148,verticalSpeed,duration=177.5543f;
        float burstEnergy=3;
        Enemy hovered,locked;
        Enemy queuedTarget;
        readonly RunnerInputBuffer jumpInput=new RunnerInputBuffer(),orbInput=new RunnerInputBuffer(),downInput=new RunnerInputBuffer(),slingInput=new RunnerInputBuffer();
        const float SlingRelease=.18f,SlingRecovery=.38f;
        float slingStarted=-10;
        bool slingReleased;
        int jumps;
        int stones=6,bank,best;
        float checkpointTime;
        float nextAirAttack;
        const float BridgeWidth=768;
        bool rewardSaved;
        Sprite[] scenicBirds;
        int used,act,health=3,coins,score; bool grounded=true,dead,complete,paused,slide;
        Sprite[][] groundTiles=new Sprite[3][];

        void Awake()
        {
            // Same behavior as the existing Costa controller: DSP must not stall when the
            // editor loses focus during diagnostics or a scene transition.
            Application.runInBackground=true;
            view=new GameObject("Sierra pixel camera").AddComponent<Camera>();view.transform.SetParent(transform);
            view.orthographic=true;view.orthographicSize=180f/64;view.transform.position=new Vector3(5,180f/64,-10);
            view.clearFlags=CameraClearFlags.SolidColor;view.backgroundColor=new Color32(26,32,54,255);
            var pixel=view.gameObject.AddComponent<PixelPerfectCamera>();pixel.assetsPPU=32;pixel.refResolutionX=320;pixel.refResolutionY=180;
            pixel.cropFrame=PixelPerfectCamera.CropFrame.Windowbox;pixel.gridSnapping=PixelPerfectCamera.GridSnapping.UpscaleRenderTexture;
            if(!FindFirstObjectByType<AudioListener>())view.gameObject.AddComponent<AudioListener>();
            var shader=Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default")??Shader.Find("Sprites/Default");
            material=new Material(shader);
            soundtrack=soundtrack?soundtrack:Resources.Load<RunnerSoundtrack>("SierraSoundtrack");
            music=gameObject.AddComponent<AudioSource>();music.playOnAwake=false;
            music.volume=PlayerPrefs.GetFloat("SuyuRun.Settings.MusicVol",.7f);
            if(soundtrack&&soundtrack.clip){music.clip=soundtrack.clip;duration=music.clip.length-soundtrack.clipStartSeconds;}
            else {Debug.LogError("Sierra: soundtrack missing. Integration cannot run without its selected audio.");enabled=false;return;}
            var timing=Resources.Load<TextAsset>("SierraIntegration/sierra_midi_exact");
            if(timing)
            {
                // Clone only: original325 MP3-estimated beats stay untouched on disk.
                midiTimeline=Instantiate(soundtrack);midiTimeline.beatTimes=JsonUtility.FromJson<MidiTiming>(timing.text).beatTimes;
                midiTimeline.bpm=120;midiTimeline.firstBeatSeconds=0;
            }
            BuildGroundTiles();
            SetupServices();
            Restart();
            if(RunnerProgress.EnterSierraMuseum){RunnerProgress.EnterSierraMuseum=false;interlude=Interlude.None;ToggleSierraPause();serviceTab=1;}
        }
        void BuildGroundTiles()
        {
            for(int a=0;a<3;a++)
            {
                string name=acts[a]+"_capa7_suelo";var source=Frames(name)[0];groundTiles[a]=new Sprite[20];
                for(int i=0;i<20;i++)groundTiles[a][i]=Sprite.Create(source.texture,new Rect(i*32,0,32,180),new Vector2(0,1),32,0,SpriteMeshType.FullRect);
            }
        }
        void OnEnable()
        {
            // Unity hot reload does not serialize dictionaries/runtime Sprite caches.
            // Restart the isolated run rather than continuing with missing collision data.
            if(Application.isPlaying&&music&&groundTiles[0]==null)
            {
                foreach(var r in GetComponentsInChildren<SpriteRenderer>())Destroy(r.gameObject);
                pool.Clear();BuildGroundTiles();Restart();
                Debug.Log("Sierra: reinicio seguro tras recompilar scripts durante Play.");
            }
        }
        Sprite[] Frames(string name)
        {
            if(sprites.TryGetValue(name,out var value))return value;
            value=Resources.LoadAll<Sprite>("SierraV3/sierra_"+name).OrderBy(s=>int.Parse(s.name.Substring(s.name.LastIndexOf('_')+1))).ToArray();
            if(value.Length==0)throw new InvalidOperationException("Missing Sierra sprite: "+name+". Run SUYURUN > Sierra V3 > Importar sprites y crear clips.");
            sprites[name]=value;return value;
        }
        Spec Data(string name)
        {
            if(specs.TryGetValue(name,out var value))return value;
            var json=Resources.Load<TextAsset>("SierraV3/sierra_"+name+".sprite");
            if(!json)throw new InvalidOperationException("Missing sprite metadata: "+name);
            value=JsonUtility.FromJson<Spec>(json.text);specs[name]=value;return value;
        }
        void Restart(bool resume=false)
        {
            ResetCaves(resume);
            var defeated=new HashSet<float>(enemies.Where(e=>e.dead).Select(e=>e.anchor));
            int keptCoins=coins,keptScore=score,keptStones=stones;
            float resumeTime=resume?checkpointTime:0;
            health=MaxHealth;coins=score=0;seconds=0;act=0;feet=148;verticalSpeed=0;burstEnergy=MaxBurst;grounded=true;dead=complete=paused=false;
            combo=0;serviceTab=0;bank=PlayerPrefs.GetInt("SuyuRun.Prototype.Bank");
            shots.Clear();enemies.Clear();bridges.Clear();if(!resume)collected.Clear();effects.Clear();damageAt=shotAt=-10;
            jumps=0;hovered=locked=null;slingStarted=-10;slingReleased=false;
            ClearActionInputs();
            rewardSaved=false;stones=6;
            nextAirAttack=0;
            if(!resume){checkpointTime=0;pickups.Clear();}
            for(int i=0;i<3;i++)bridges.Add(new Bridge{x=Mathf.Ceil((duration/3+6+i*15)*Speed/32)*32});
            feet=Surface(HeroX,0);
            var json=Resources.Load<TextAsset>("SierraIntegration/sierra_course");
            if(!json)throw new InvalidOperationException("Missing fixed Sierra course. Run Tools/build_sierra_course.py.");
            foreach(var hit in JsonUtility.FromJson<Course>(json.text).events)
            {
                float eventSeconds=midiTimeline?midiTimeline.BeatToSeconds(midiTimeline.SecondsToBeat(hit.t)):hit.t;
                float x=eventSeconds*Speed+HeroX;
                enemies.Add(new Enemy{x=x,anchor=x,kind=hit.kind,air=hit.kind=="bird",y=hit.kind=="bird"?76:0});
                if(hit.kind=="rock")
                {
                    // Visible above the jump approach; fixed choreography leaves time to lock and throw.
                    float birdX=x+72;
                    enemies.Add(new Enemy{x=birdX,anchor=birdX,kind="rock_bird",air=true,y=Surface(x,ActAt(eventSeconds))-85,attackAt=eventSeconds+.65f});
                }
            }
            for(int i=0;i<bridges.Count;i++)enemies.Add(new Enemy{x=bridges[i].x+160,anchor=bridges[i].x+160,y=75,air=true,attacker=true,bridge=i});
            epoch=AudioSettings.dspTime+.2;previous=epoch;music.Stop();music.time=soundtrack.clipStartSeconds;music.PlayScheduled(epoch);
            if(!resume)for(float t=4;t<duration-6;t+=2)
            {
                if(NearTunnel(t,2))continue;
                float x=t*Speed+HeroX;
                pickups.Add(new RunnerPickup{x=x,y=Surface(x,ActAt(t))-20,width=12,height=12,kind=(int)t%8==0?1:0});
            }
            if(resume)
            {
                coins=keptCoins;score=keptScore;stones=keptStones;seconds=resumeTime;act=ActAt(seconds);
                feet=Surface(seconds*Speed+HeroX,act);damageAt=seconds;
                foreach(var e in enemies)if(e.anchor<seconds*Speed+HeroX-32||defeated.Contains(e.anchor))e.dead=true;
                epoch=AudioSettings.dspTime+.2-seconds;previous=AudioSettings.dspTime+.2;
                music.Stop();music.time=soundtrack.clipStartSeconds+seconds;music.PlayScheduled(AudioSettings.dspTime+.2);
            }
            BuildMobility(JsonUtility.FromJson<Course>(json.text).mobility,resumeTime);
            var waves=JsonUtility.FromJson<Course>(json.text).waves;
            if(waves!=null)foreach(var wave in waves)for(int i=0;i<wave.count;i++)
            {
                float at=wave.t+i*1.6f;if(at<resumeTime)continue;
                float x=at*Speed+HeroX+180;enemies.Add(new Enemy{x=x,anchor=x,y=55,air=true,kind="bird"});
            }
            if(resume)foreach(var e in enemies)if(defeated.Contains(e.anchor))e.dead=true;
            interlude=Interlude.None;narrativeLine="";
            if(!resume){guideSeen=false;escapedTraps=paidRevives=0;BeginInterlude(Interlude.Arrival);}
        }
        void Revive()
        {
            if(dead&&checkpointTime>0&&RunnerProgress.TrySpend(ref coins,RunnerProgress.ReviveCost))
            {paidRevives++;dead=false;rewindFrom=seconds;BeginInterlude(Interlude.Rewind);}
        }
        bool NearTunnel(float t,float margin) => Mathf.Abs(t-duration/3)<margin||Mathf.Abs(t-duration*2/3)<margin;
        int ActAt(float t)=>Mathf.Clamp((int)(t/(duration/3)),0,2);
        float Surface(float world,int region)
        {
            if(region==1)foreach(var b in bridges)if(world>=b.x&&world<b.x+BridgeWidth)
            {
                if(b.life<=0&&seconds-b.hit>=.2)return 300;
                float phase=Mathf.Floor(seconds*Data("puente_estable").fps)%4;
                float curve=Mathf.Sin(Mathf.PI*Mathf.Repeat(world-b.x,256)/255);
                return 148+Mathf.Round(13*curve)+Mathf.Round(Mathf.Sin(phase*Mathf.PI/2)*curve*2);
            }
            var profile=Data(acts[region]+"_capa7_suelo").surfaceY;
            return profile[Mathf.FloorToInt(Mathf.Repeat(world,640))];
        }
        void Update()
        {
            UpdateMuseumMusic();
            var k=Keyboard.current;var m=Mouse.current;
            UpdateServiceInput(k);
            if(k!=null&&k.rKey.wasPressedThisFrame&&!paused){if(cavePhase!=CavePhase.None){if(cave.dead)RetryCave();else cave.BeginReload();}else Restart();}
            if(k!=null&&k.backspaceKey.wasPressedThisFrame&&!paused&&cavePhase==CavePhase.Explore)RetryCave();
            if(k!=null&&k.escapeKey.wasPressedThisFrame&&!complete&&!dead)ToggleSierraPause();
            if(paused){Render();return;}
            UpdateMusicTransition();
            if(UpdateCave(k,m)){Render();return;}
            if(UpdateInterlude(k,AudioSettings.dspTime)){Render();return;}
            double now=AudioSettings.dspTime;
            if(!paused&&!dead&&!complete)
            {
                // Capture once per rendered frame, BEFORE checking whether DSP has advanced.
                if(k!=null&&(k.spaceKey.wasPressedThisFrame||k.upArrowKey.wasPressedThisFrame)){jumpInput.Press(now);orbInput.Press(now);}
                if(k!=null&&(k.sKey.wasPressedThisFrame||k.downArrowKey.wasPressedThisFrame))downInput.Press(now);
                hovered=m!=null?HoveredBird(m.position.ReadValue()):null;
                if(m!=null&&m.leftButton.wasPressedThisFrame){queuedTarget=hovered;slingInput.Press(now);}
                float targetSeconds=Mathf.Max(seconds,(float)(now-epoch));
                while(targetSeconds-seconds>.000001f&&!dead&&!complete&&interlude==Interlude.None)
                {
                float dt=Mathf.Min(targetSeconds-seconds,RunnerMotion.MaxStep);
                float boundary=duration*(act+1)/3;
                if(act<2&&!cavesDone[act]&&seconds+dt>=boundary)
                {seconds=boundary;BeginCave(act);break;}
                float oldSeconds=seconds;seconds+=dt;act=ActAt(seconds);previous=now;
                if(ActAt(oldSeconds)!=act){feet=Surface(seconds*Speed+HeroX,act);verticalSpeed=0;grounded=true;}
                bool wasSliding=slide;
                slide=k!=null&&(k.sKey.isPressed||k.downArrowKey.isPressed);
                bool jump=jumpInput.Pending(now);
                if(DiagnosticAutoplay)
                {
                    jump=grounded&&enemies.Any(e=>(e.kind=="jump"||e.kind=="double_jump"||e.kind=="rock"||e.kind=="log"||e.kind=="puddle"||e.kind=="spike_pair")&&!e.dead&&e.x-seconds*Speed-HeroX>65&&e.x-seconds*Speed-HeroX<95);
                    if(!grounded&&jumps==1&&verticalSpeed>-40&&enemies.Any(e=>e.kind=="double_jump"&&Mathf.Abs(e.x-seconds*Speed-HeroX)<140))jump=true;
                    slide=enemies.Any(e=>e.kind=="slide"&&Mathf.Abs(e.x-seconds*Speed-HeroX)<65);
                    if(act==2&&grounded&&RiserNeedsJump(seconds*Speed+HeroX+45)&&Surface(seconds*Speed+HeroX+45,2)<feet-4)jump=true;
                }
                bool wasGrounded=grounded;
                bool mobilityJump=UpdateMobility(jump||orbInput.Pending(now),downInput.Pending(now));
                if(mobilityJump){jumpInput.Clear();orbInput.Clear();}
                if(!mobilityJump&&jump&&(grounded||jumps<2))
                {
                    jumpInput.Consume(now);
                    if(grounded)orbInput.Clear();
                    JudgeAction();Cue(520);
                    if(!grounded)EffectAt("polvo_aterrizar",seconds*Speed+HeroX-16,feet-18);
                    jumps=grounded?1:jumps+1;verticalSpeed=-JumpSpeed;grounded=false;
                }
                if(slide&&!wasSliding&&grounded)JudgeAction();
                float floor=Surface(seconds*Speed+HeroX,act);
                if(grounded)
                {
                    // Stone risers are obstacles, not an automatic elevator under the feet.
                    if(act==2&&floor<feet-4&&RiserNeedsJump(seconds*Speed+HeroX))
                    {if(seconds-damageAt>1.5){health--;damageAt=seconds;Cue(120);if(health<=0)Fail();}verticalSpeed=-125;grounded=false;EffectAt("polvo_aterrizar",seconds*Speed+HeroX-16,floor-12);}
                    else if(floor-feet>16)grounded=false;
                    else feet=floor;
                }
                if(!grounded){RunnerMotion.Integrate(ref feet,ref verticalSpeed,Gravity,dt);if(verticalSpeed>=0&&feet>=floor&&floor<220){feet=floor;verticalSpeed=0;grounded=true;}}
                if(grounded){jumps=0;airImpulse=false;diveUntil=-1;}
                if(!wasGrounded&&grounded)EffectAt("polvo_aterrizar",seconds*Speed+HeroX-16,feet-18);
                if(feet>230)Fail();
                if(seconds>=duration*.5f&&grounded&&!NearTunnel(seconds,2)&&!bridges.Any(b=>seconds*Speed+HeroX>=b.x-40&&seconds*Speed+HeroX<b.x+BridgeWidth+40)&&checkpointTime==0)
                    checkpointTime=seconds;
                foreach(var item in pickups)
                {
                    float magnet=PlayerPrefs.GetInt("SuyuRun.Upgrade.Iman",0)*12;
                    if(item.kind==0&&!item.taken&&magnet>0&&Vector2.Distance(new Vector2(item.x,item.y),new Vector2(seconds*Speed+HeroX,feet-20))<magnet+20)
                    {item.x=Mathf.MoveTowards(item.x,seconds*Speed+HeroX,220*dt);item.y=Mathf.MoveTowards(item.y,feet-20,180*dt);}
                    if(item.TryCollect(new Vector2(seconds*Speed+HeroX,feet-20),new Vector2(12,slide?12:24)))
                    {
                        Cue(item.kind==1?700:1100);
                        if(item.kind==1)stones=Mathf.Min(20,stones+3);else {coins++;score+=10;}
                        EffectAt("chispas",item.x-12,item.y-12);
                    }
                }
                bool fire=slingInput.Pending(now)&&queuedTarget!=null&&!queuedTarget.dead;
                var requestedTarget=queuedTarget;
                bool burstHeld=k!=null&&k.eKey.isPressed;
                if(DiagnosticAutoplay)burstHeld=enemies.Any(e=>e.kind=="guardian"&&!e.dead&&e.x-seconds*Speed>HeroX&&e.x-seconds*Speed<320);
                bool burst=burstHeld&&burstEnergy>.08f;
                burstEnergy=Mathf.Clamp(burstEnergy+(burstHeld?-dt:dt*.25f),0,MaxBurst);
                if(DiagnosticAutoplay){hovered=enemies.FirstOrDefault(e=>e.air&&e.kind!="cross_bird"&&!e.dead&&e.activeAt>=0);requestedTarget=hovered;fire=hovered!=null;}
                if(fire&&requestedTarget!=null&&locked==null&&stones>0&&seconds-slingStarted>=SlingRecovery)
                {locked=requestedTarget;slingInput.Clear();queuedTarget=null;slingStarted=seconds;slingReleased=false;stones--;JudgeAction();Cue(350);}
                if(locked!=null)
                {
                    if(locked.dead){locked=null;}
                    else if(!slingReleased&&seconds-slingStarted>=SlingRelease)
                    {
                        Vector2 origin=new Vector2(seconds*Speed+HeroX+18,feet-27);
                        shots.Add(new Shot{origin=origin,position=origin,target=locked,guided=true,born=seconds});slingReleased=true;
                    }
                    if(seconds-slingStarted>=SlingRecovery)locked=null;
                }
                if(burst&&seconds-shotAt>.13)
                {
                    Vector2 origin=new Vector2(seconds*Speed+HeroX+18,feet-25);
                    shots.Add(new Shot{origin=origin,velocity=new Vector2(Speed+390,0),born=seconds});shotAt=seconds;
                }
                SimulateCombat(dt);effects.RemoveAll(e=>seconds-e.time>1);
                for(int i=0;i<3;i++)
                {
                    float target=(duration*(i+.5f)/3)*Speed+HeroX;
                    if(!collected.Contains(i)&&Mathf.Abs(target-(seconds*Speed+HeroX))<14&&grounded)
                    {collected.Add(i);score+=250;if(!DiagnosticAutoplay){PlayerPrefs.SetInt("SuyuRun.Museum.Sierra."+new[]{"cabeza_clava","kero","quipu"}[i],1);PlayerPrefs.Save();}}
                }
                if(seconds>=duration&&!dead)
                {
                    seconds=duration;complete=true;endingAt=now;music.Stop();Cue(1500);
                    if(!rewardSaved){RunnerProgress.Finish("Sierra",coins,score,DiagnosticAutoplay,out bank,out best);rewardSaved=true;}
                }
                }
            }
            Render();
        }
        Vector2 ShotPosition(Shot s)=>s.guided?s.position:s.origin+s.velocity*(float)(seconds-s.born);
        void ClearActionInputs(){jumpInput.Clear();orbInput.Clear();downInput.Clear();slingInput.Clear();queuedTarget=null;}
        Enemy HoveredBird(Vector2 screen)
        {
            if(!view.pixelRect.Contains(screen))return null;
            Vector3 p=view.ScreenToWorldPoint(screen);Vector2 cursor=new Vector2(p.x*32+seconds*Speed,180-p.y*32);
            return enemies.Where(e=>e.air&&e.kind!="cross_bird"&&!e.dead&&e.activeAt>=0&&Mathf.Abs(cursor.x-e.x)<(e.attacker?24:20)&&Mathf.Abs(cursor.y-e.y)<16)
                .OrderBy(e=>(cursor-new Vector2(e.x,e.y)).sqrMagnitude).FirstOrDefault();
        }
        void EffectAt(string name,float x,float y){effects.Add(new Effect{name="fx_"+name,x=x,y=y,time=seconds});}
        void SimulateCombat(float dt)
        {
            foreach(var s in shots)if(s.guided&&!s.dead)
            {
                if(s.target==null||s.target.dead){s.dead=true;continue;}
                s.position=Vector2.MoveTowards(s.position,new Vector2(s.target.x,s.target.y),(Speed+900)*dt);
            }
            foreach(var e in enemies)
            {
                if(e.dead)continue;
                float sx=e.x-seconds*Speed;
                if(e.kind=="cross_bird")
                {
                    if(seconds<e.attackAt)continue;
                    if(e.activeAt<0)e.activeAt=seconds;
                    e.x-=800*dt;sx=e.x-seconds*Speed;
                    if(sx<-40)e.dead=true;
                }
                else if(e.kind=="rock")
                {
                    // Rolls toward the runner, crossing the MIDI anchor at its original time.
                    float encounter=(e.anchor-HeroX)/Speed;
                    e.x=e.anchor-55*Mathf.Clamp(seconds-encounter,-1.1f,1.1f);sx=e.x-seconds*Speed;
                }
                else if(e.kind=="rock_bird")
                {
                    if(sx>340)continue;
                    if(e.activeAt<0)e.activeAt=seconds;
                    float age=seconds-e.activeAt;
                    e.x+=Speed*dt; sx=e.x-seconds*Speed;
                    e.y+=Mathf.Sin(age*5)*10*dt;
                    if(seconds>e.attackAt){e.x-=280*dt;e.y=Mathf.MoveTowards(e.y,feet-24,80*dt);}
                    if(age>3.5f){e.dead=true;continue;}
                }
                else if(e.air)
                {
                    if(e.activeAt<0&&sx<340)
                    {
                        e.activeAt=seconds;e.phase=DiagnosticAutoplay?e.anchor*.013f:UnityEngine.Random.Range(0f,6.283f);
                        if(!e.attacker){e.x=seconds*Speed+(Mathf.Sin(e.phase)>0?-28:340);e.y=32+12*Mathf.Abs(Mathf.Sin(e.phase));}
                        e.attackAt=Mathf.Max(seconds+1.35f,nextAirAttack);nextAirAttack=e.attackAt+1.5f;
                    }
                    if(e.activeAt<0)continue;
                    float age=seconds-e.activeAt,seed=e.phase;
                    // Continuous deterministic wandering: stable on retry, never teleporting.
                    Vector2 desired=new Vector2(seconds*Speed+265+22*Mathf.Sin(age*2.7f+seed),65+18*Mathf.Sin(age*3.1f+seed));
                    if(seconds>e.attackAt)desired=new Vector2(seconds*Speed+HeroX+12,feet-24+9*Mathf.Sin(age*5+seed));
                    Vector2 relative=new Vector2(e.x-seconds*Speed,e.y);
                    Vector2 destination=new Vector2(desired.x-seconds*Speed,desired.y);
                    e.velocity=Vector2.MoveTowards(e.velocity,(destination-relative)*3,320*dt);
                    relative+=e.velocity*dt;e.x=relative.x+seconds*Speed+Speed*dt;e.y=Mathf.Clamp(relative.y,32,165);
                    sx=e.x-seconds*Speed;
                    if(age>5.5f){e.dead=true;continue;}
                }
                float y=e.air?e.y:Surface(e.x,ActAt(e.x/Speed))-22;
                bool solid=e.kind=="jump"||e.kind=="double_jump"||e.kind=="slide"||e.kind=="puddle"||e.kind=="spike_pair"||e.kind=="rock"||e.kind=="cross_bird";
                foreach(var s in shots)if(!solid&&!s.hostile&&!s.dead&&Vector2.Distance(ShotPosition(s),new Vector2(e.x,y))<20)
                {e.dead=true;s.dead=true;coins+=3;score+=100;EffectAt("impacto_piedra",e.x-12,y-12);break;}
                if(e.dead)continue;
                if(e.kind=="guardian"&&sx>HeroX+25&&sx<240&&seconds-e.lastAttack>1.8)
                {
                    float first=(e.x-320)/Speed+.25f;
                    if(seconds>first)
                    {
                        Vector2 origin=new Vector2(e.x,y);Vector2 target=new Vector2(seconds*Speed+HeroX,feet-25);
                        shots.Add(new Shot{origin=origin,velocity=(target-origin).normalized*185,born=seconds,hostile=true});e.lastAttack=seconds;
                    }
                }
                if(e.attacker&&sx<280&&sx>45)
                {
                    var b=bridges[e.bridge];
                    // At least 2s visual warning before first structural hit, then1.3s cadence.
                    float dangerStart=e.activeAt;
                    if(seconds>dangerStart+1.25f&&seconds-b.hit>.75f&&b.life>0){b.life--;b.hit=seconds;EffectAt(b.life==0?"cuerda_rotura":"astillas_madera",seconds*Speed+220,143);}
                }
                float ground=Surface(e.x,ActAt(e.x/Speed));
                bool overlap=e.kind=="jump"||e.kind=="rock"||e.kind=="log"?Mathf.Abs(sx-HeroX)<18&&feet>ground-26:
                    e.kind=="spike_pair"?Mathf.Abs(sx-HeroX)<34&&feet>ground-26:
                    e.kind=="puddle"?Mathf.Abs(sx-HeroX)<24&&feet>ground-9:
                    e.kind=="double_jump"?Mathf.Abs(sx-HeroX)<28&&feet>ground-58:
                    e.kind=="slide"?Mathf.Abs(sx-HeroX)<41&&!slide&&feet>ground-62&&feet-40<ground-30:
                    Mathf.Abs(sx-HeroX)<15&&Mathf.Abs((feet-(slide?15:25))-y)<(slide?14:22);
                // This explicitly signposted dodge hazard is lethal, even during damage grace.
                if(overlap&&e.kind=="cross_bird"){health=0;Fail();break;}
                if(overlap&&seconds-damageAt>1.5)
                {
                    if(e.kind=="puddle"){e.dead=true;BeginTrap();break;}
                    health--;damageAt=seconds;if(!solid)e.dead=true;if(health<=0)Fail();
                }
            }
            foreach(var s in shots)if(s.hostile&&!s.dead&&seconds-damageAt>1.5&&Vector2.Distance(ShotPosition(s),new Vector2(seconds*Speed+HeroX,feet-(slide?15:25)))<(slide?13:19))
            {s.dead=true;health--;damageAt=seconds;EffectAt("impacto_piedra",seconds*Speed+HeroX-12,feet-25);if(health<=0)Fail();}
            shots.RemoveAll(s=>s.dead||seconds-s.born>2);
        }
        void Fail(){dead=true;combo=0;music.Stop();Cue(120);}
        SpriteRenderer Paint(Sprite sprite,float x,float y,int order)
        {
            if(used==pool.Count){var go=new GameObject("Sierra pooled pixel visual");go.transform.SetParent(transform);var r=go.AddComponent<SpriteRenderer>();r.sharedMaterial=material;pool.Add(r);}
            var sr=pool[used++];sr.gameObject.SetActive(true);sr.sprite=sprite;sr.sortingOrder=order;sr.drawMode=SpriteDrawMode.Simple;sr.color=Color.white;
            sr.transform.localScale=Vector3.one;sr.flipX=false;
            sr.transform.position=new Vector3((Mathf.Round(x)+sprite.pivot.x)/32,(180-Mathf.Round(y)-sprite.rect.height+sprite.pivot.y)/32,0);
            return sr;
        }
        void Draw(string name,float x,float y,int order,float clock=-1)
        {
            var a=Frames(name);var spec=Data(name);float t=clock<0?seconds:clock;
            int f=spec.loop?Mathf.FloorToInt(t*spec.fps)%a.Length:Mathf.Clamp(Mathf.FloorToInt(t*spec.fps),0,a.Length-1);
            Paint(a[f],x,y,order);
        }
        void Layer(string name,float factor,int order)
        {
            float shift=Mathf.Repeat(seconds*Speed*factor,640);Draw(name,-shift,0,order);Draw(name,640-shift,0,order);
        }
        void DrawScenery(float distance)
        {
            if(scenicBirds==null)
            {
                var texture=Resources.Load<Texture2D>("costa_capa2_aves");
                scenicBirds=new Sprite[18];
                if(texture)for(int row=0;row<3;row++)for(int f=0;f<6;f++)
                    scenicBirds[row*6+f]=Sprite.Create(texture,new Rect(f*16,texture.height-(row+1)*16,16,16),new Vector2(.5f,.5f),32);
            }
            for(int flock=0;flock<3;flock++)for(int bird=0;bird<5;bird++)
            {
                float x=Mathf.Repeat(70+flock*137-seconds*(9+flock*3),410)-40+bird*9;
                float y=40+flock*12+Mathf.Abs(bird-2)*4+Mathf.Sin(seconds*.8f+flock)*3;
                int f=(Mathf.FloorToInt(seconds*7)+bird)%6;
                var sprite=scenicBirds[flock*6+f];
                if(sprite)Paint(sprite,x,y,2);
            }
            // Ground props use the SAME world position/profile as collision, never parallax.
            int start=Mathf.FloorToInt(distance/112)-1;
            for(int i=start;i<start+6;i++)
            {
                float world=i*112+24;
                if(act==1&&bridges.Any(b=>world+96>b.x&&world<b.x+BridgeWidth))continue;
                if(act==0)
                {
                    // Slice existing fence into narrow columns, each conforming to the slope.
                    Sprite source=Frames("decor_cerco_madera")[0];
                    for(int part=0;part<12;part++)
                    {
                        string key="fence_"+part;
                        if(!sprites.TryGetValue(key,out var pieces))
                        {
                            pieces=new[]{Sprite.Create(source.texture,new Rect(source.rect.x+part*8,source.rect.y,8,40),new Vector2(0,1),32)};
                            sprites[key]=pieces;
                        }
                        Paint(pieces[0],world+part*8-distance,Surface(world+part*8+4,act)-37,5);
                    }
                }
                float px=world+68;
                Draw(i%3==0?"decor_arbol_queuna":"decor_ichu",px-distance,Surface(px+12,act)-(i%3==0?71:19),5);
            }
        }
        void Render()
        {
            if(cavePhase==CavePhase.Explore||cavePhase==CavePhase.Leaving){RenderCave();return;}
            used=0;float distance=seconds*Speed;string region=acts[act];
            Layer("compartido_capa1_cielo",0,0);Layer("compartido_capa2_nubes",.05f,1);Layer("compartido_capa3_nevados",.12f,2);
            Layer(region+"_capa4_paisaje",.3f,3);if(act==1)Layer("valle_capa5_rio",.3f,4);
            // Valley vegetation belongs on solid land, never a continuous strip over voids.
            DrawScenery(distance);
            int first=Mathf.FloorToInt(distance/32);
            for(int i=first;i<=first+11;i++)
            {
                float x=i*32;
                if(act==1&&bridges.Any(b=>x>=b.x&&x<b.x+BridgeWidth))continue;
                Paint(groundTiles[act][(i%20+20)%20],x-distance,0,6);
            }
            if(act==1)foreach(var b in bridges)
            {
                float x=b.x-distance;if(x>350||x<-BridgeWidth-24)continue;
                string state=b.life<=0?"colapso":b.life==1?"danado":b.life==2?"tension":"estable";
                for(int span=0;span<3;span++)Draw("puente_"+state,x+span*256,110,7,b.life<=0?(float)(seconds-b.hit):-1);
            }
            DrawGrazers(distance);
            if(act==0&&seconds<4)Draw("llegada_marca_quemada",60-distance,133,8);
            foreach(var e in enemies)
            {
                if(e.kind=="cross_bird"&&seconds<e.attackAt)continue;
                float x=e.x-distance;if(e.dead||x<-64||x>350)continue;
                float y=e.air?e.y:Surface(e.x,ActAt(e.x/Speed))-22;
                if(e.kind=="jump")Draw("pinchos",x-16,Surface(e.x,ActAt(e.x/Speed))-32,10);
                else if(e.kind=="spike_pair")
                {Draw("pinchos",x-32,Surface(e.x-16,ActAt(e.x/Speed))-32,10);Draw("pinchos",x,Surface(e.x+16,ActAt(e.x/Speed))-32,10);}
                else if(e.kind=="rock")Draw("roca_rodante",x-16,Surface(e.x,ActAt(e.x/Speed))-32,10,Mathf.Max(0,(e.anchor-e.x+61)/(2*Mathf.PI*15)));
                else if(e.kind=="log")Paint(FloatingRelic(),x-16,Surface(e.x,ActAt(e.x/Speed))-34-Mathf.Round(Mathf.Sin(seconds*3+e.anchor)*2),10);
                else if(e.kind=="puddle")Draw("fx_salpicadura",x-16,Surface(e.x,ActAt(e.x/Speed))-13,10,Mathf.Repeat(seconds,.6f));
                else if(e.kind=="double_jump")
                {
                    float top=Surface(e.x,ActAt(e.x/Speed))-58;
                    Paint(ReadableBarrier(),x-32,top+26,10);Draw("pinchos",x-16,top,11);
                }
                else if(e.kind=="slide")Paint(ReadableBarrier(),x-32,Surface(e.x,ActAt(e.x/Speed))-62,10);
                else
                {
                    string enemyPose=e.air?(e.attacker?"condor_vuelo":"halcon_vuelo"):
                        x>HeroX+25&&x<240?"guardian_huaraca":"guardian_idle";
                    Draw(enemyPose,x-(e.attacker?32:e.air?16:24),y-(e.air&&!e.attacker?16:24),10);
                    if(e.air)pool[used-1].flipX=true;
                }
            }
            foreach(var s in shots){Vector2 p=ShotPosition(s);Draw("piedra",p.x-distance-4,p.y-4,12);}
            foreach(var item in pickups)if(!item.taken&&item.x-distance>-20&&item.x-distance<340)
                Draw(item.kind==1?"piedra":"coleccionable_moneda",item.x-distance-(item.kind==1?4:8),item.y-(item.kind==1?4:8),11);
            foreach(var e in effects)Draw(e.name,e.x-distance,e.y,17,seconds-e.time);
            for(int i=0;i<3;i++)if(!collected.Contains(i))
            {
                float world=duration*(i+.5f)/3*Speed+HeroX;float x=world-distance;
                if(x>-20&&x<340)Draw("coleccionable_"+new[]{"cabeza_clava","kero","quipu"}[i],x-8,Surface(world,i)-22,11);
            }
            float attackAge=seconds-slingStarted;
            float slingAge=attackAge<.04f?attackAge*5:attackAge<SlingRelease?.2f+(attackAge-.04f)*(.5f/.14f):.7f+(attackAge-SlingRelease)*(.35f/.2f);
            string pose=dead?"hit":interlude!=Interlude.None?"idle":seconds-damageAt<.25?"hit":slide&&grounded?"slide":slingAge<.2f?"huaraca_cargar":slingAge<.7f?"huaraca_girar":slingAge<1.05f?"huaraca_lanzar":seconds-shotAt<.15?"rafaga":grounded?"run":"jump";
            if(complete)
            {
                float ending=(float)(AudioSettings.dspTime-endingAt);
                float center=Mathf.Lerp(HeroX,160,Mathf.Clamp01(ending/.7f));
                float endFeet=Surface(distance+center,act);
                if(ending<.7f)Draw("heroe_run",center-24,endFeet-47,15,ending);
                else Draw("premio_traje_transformacion",center-24,endFeet-47,15,ending-.7f);
                if(ending>.7f&&ending<2.1f)Draw("fx_escudo_activar",center-32,endFeet-58,16,ending-.7f);
                if(ending>1.2&&ending<2.3)Draw("efecto_sorpresa",center-16,endFeet-78,16,ending-1.2f);
            }
            else if(cavePhase==CavePhase.Entering||cavePhase==CavePhase.Exiting)
            {
                float cx=cavePhase==CavePhase.Entering?Mathf.Lerp(HeroX,235,Mathf.Clamp01(CavePhaseAge/1.25f)):Mathf.Lerp(55,HeroX,Mathf.Clamp01(CavePhaseAge/1.2f));
                Draw("heroe_run",cx-24,Surface(distance+cx,act)-47,15,CavePhaseAge);
            }
            else if(interlude==Interlude.Arrival)
            {
                float arrival=NarrativeTime;
                float arrivalX=Mathf.Lerp(48,HeroX,Mathf.SmoothStep(0,1,(arrival-.35f)/1.1f));
                float arrivalFeet=Surface(distance+arrivalX,act);
                Draw(arrival>.35f&&arrival<1.45f?"heroe_run":"heroe_idle",arrivalX-24,arrivalFeet-47,15,arrival);
            }
            else
            {
                Draw("heroe_"+pose,HeroX-24,feet-47,15,interlude!=Interlude.None?NarrativeTime:pose=="slide"?Mathf.Repeat(seconds*.5f,1f)*.5f:pose=="huaraca_lanzar"?slingAge-.7f:pose=="huaraca_girar"?slingAge-.2f:pose=="huaraca_cargar"?slingAge:-1);
                if(pose=="slide")
                {
                    var hero=pool[used-1];hero.transform.localScale=new Vector3(1.12f,CrouchRatio,1);
                    // Anchor the scaled frame at the feet, instead of dropping the whole sprite.
                    int crouchFrame=Mathf.Clamp(Mathf.FloorToInt(Mathf.Repeat(seconds*.5f,1f)*3),0,2);
                    // Existing frames0/1 end at row38, frame2 at44 (48px canvas).
                    // Align the opaque feet, not the transparent bottom padding.
                    float bottomPadding=crouchFrame==2?4:10;
                    hero.transform.position=new Vector3(HeroX/32,(180-feet-bottomPadding*CrouchRatio)/32,0);
                }
            }
            RenderNarrative();
            var aim=locked??hovered;
            if(aim!=null&&!aim.dead)
            {
                Draw("ui_reticula",aim.x-distance-8,aim.y-8,19);
                pool[used-1].color=locked!=null||Mathf.FloorToInt(seconds*10)%2==0?new Color32(237,146,84,255):new Color32(139,46,46,255);
            }
            if(seconds<2&&interlude==Interlude.None)Draw("fx_teleport_anillo",HeroX-32,feet-21,16,seconds);
            if(grounded&&!dead&&!complete&&interlude==Interlude.None)Draw("fx_polvo_correr",HeroX-35,feet-16,14,Mathf.Repeat(seconds,.4f));
            if(seconds>duration-6)Draw("portal_ceremonial",duration*Speed+HeroX+10-distance,Surface(duration*Speed+HeroX,2)-106,9);
            foreach(float t in new[]{duration/3,duration*2/3})
            {
                float delta=seconds-t;
                if(Mathf.Abs(delta)<2)
                {
                    bool entering=delta<0||cavePhase==CavePhase.Entering;
                    string tunnel=entering?"tunel_entrada":"tunel_salida";
                    float tx=entering?180-delta*Speed:-45-delta*Speed;
                    float baseY=Surface(distance+tx+64,act);
                    Draw(tunnel,tx,baseY-Frames(tunnel)[0].rect.height,20);
                }
            }
            for(int i=used;i<pool.Count;i++)pool[i].gameObject.SetActive(false);
            UpdateSierraCamera();
        }
        void OnGUI()
        {
            if(!enabled)return;
            float scale=Mathf.Max(1,Mathf.Floor(Mathf.Min(Screen.width/320f,Screen.height/180f)));
            GUI.matrix=Matrix4x4.TRS(new Vector3((Screen.width-320*scale)/2,(Screen.height-180*scale)/2),Quaternion.identity,Vector3.one*scale);
            if(DrawSharedWindow()){GUI.matrix=Matrix4x4.identity;return;}
            if(cavePhase!=CavePhase.None){DrawCaveGUI();GUI.matrix=Matrix4x4.identity;return;}
            if(interlude==Interlude.Arrival&&!paused)
            {
                var introStyle=new GUIStyle(GUI.skin.label){font=pixelFont,fontSize=5,wordWrap=true,normal={textColor=new Color32(255,243,208,255)}};
                RunnerPresentation.Box(new Rect(0,155,320,25),RunnerPresentation.Ink);
                GUI.Label(new Rect(8,159,304,19),narrativeLine,introStyle);
                RunnerPresentation.Curtain(1-(float)(AudioSettings.dspTime-interludeAt)/.55f);
                GUI.matrix=Matrix4x4.identity;return;
            }
            // Explicit pixel9-slice in screen space keeps the HUD inside the letterboxed320x180.
            Texture2D panel=Frames("ui_panel")[0].texture;
            float[] xs={0,2,318,320},ys={0,2,18,20};float[] us={0,2f/96,94f/96,1},vs={1,46f/48,2f/48,0};
            for(int row=0;row<3;row++)for(int col=0;col<3;col++)
                GUI.DrawTextureWithTexCoords(new Rect(xs[col],ys[row],xs[col+1]-xs[col],ys[row+1]-ys[row]),panel,new Rect(us[col],vs[row+1],us[col+1]-us[col],vs[row]-vs[row+1]));
            if(label==null)label=new GUIStyle(GUI.skin.label){font=pixelFont,fontSize=6,normal={textColor=new Color32(255,243,208,255)}};
            var hudText=new GUIStyle(label){fontSize=5,padding=new RectOffset(0,0,0,0)};
            RunnerHud.Draw(.25f,coins,score,hudText,i=>collected.Contains(i),
                r=>RunnerHud.Sprite(r,Frames("ui_panel")[0]),r=>RunnerHud.Sprite(r,Frames("ui_casilla")[0]),
                r=>RunnerHud.Sprite(r,Frames("coleccionable_moneda")[0]),r=>RunnerHud.Sprite(r,Frames("coleccionable_quipu")[0]),new Color32(245,223,160,255));
            GUI.Label(new Rect(52,3,160,10),"SIERRA · VIDA "+health,label);
            GUI.Label(new Rect(4,13,315,10),"ESP x2  S  CLICK · PIEDRAS "+stones+"  E "+Mathf.RoundToInt(burstEnergy/MaxBurst*100)+"%",label);
            if(!paused&&!dead&&!complete&&AudioSettings.dspTime<feedbackUntil)GUI.Label(new Rect(65,35,245,10),rhythmFeedback,new GUIStyle(label){fontSize=5});
            if(act==1)
            {
                var active=bridges.FirstOrDefault(b=>b.x+BridgeWidth>seconds*Speed+HeroX&&b.x-seconds*Speed<330);
                if(active!=null)GUI.Label(new Rect(4,23,310,10),"PUENTE "+active.life+"/3 · PROTEGE LOS ANCLAJES",label);
            }
            string phrase=seconds<4?"FRANCISCO: ¿Dónde estoy? ¡Los Andes!":
                NearTunnel(seconds,2)?"¡El camino sigue al otro lado!":
                seconds>duration-6?"El portal... ¿qué habrá en la Selva?":
                checkpointTime>0&&seconds-checkpointTime<3?"PUNTO DE RETORNO ACTIVADO":
                act==1?"Apunta al ave. ¡Protege las cuerdas!":act==2?"SALTA LOS ESCALONES · No suben solos":"";
            if(!string.IsNullOrEmpty(narrativeLine))phrase=narrativeLine;
            if(phrase.Length>0&&!complete&&!dead)GUI.Label(new Rect(8,164,308,12),phrase,label);
            if(!paused&&!dead&&!complete&&interlude==Interlude.None)DrawMobilityHints();
            if(DrawServices()){GUI.matrix=Matrix4x4.identity;return;}
            if(paused||dead||(complete&&AudioSettings.dspTime-endingAt>3))
                RunnerModal.Draw(new Rect(50,32,220,140),panel,pixelFont,new RunnerModal.Model{
                    heading=complete?"SIERRA COMPLETADA":dead?"VUELVE A INTENTAR":"PAUSA",
                    body=complete?"TRAJE PROTECTOR OBTENIDO\nSELVA DESBLOQUEADA":dead?"Usa las marcas para preparar el salto.":"El recorrido y la música están en pausa.",
                    coins=coins,score=score,bank=bank,showStats=complete,
                    footer=complete?"MEMORIAS "+collected.Count+" / 3 · SELVA EN DESARROLLO":"ESP x2 · S/ABAJO · CLICK · E",
                    primary=complete?"REJUGAR":dead?"REINICIAR":"CONTINUAR",
                    primaryAction=()=>{if(complete||dead)Restart();else ToggleSierraPause();},
                    secondary=dead?"REVIVIR · 5":"MENU",secondaryEnabled=!dead||(checkpointTime>0&&coins>=RunnerProgress.ReviveCost),
                    secondaryAction=()=>{if(dead)Revive();else UnityEngine.SceneManagement.SceneManager.LoadScene("Level_Costa");}
                },true);
            GUI.matrix=Matrix4x4.identity;
        }
        void OnDestroy()
        {
            if(readableBarrier){Destroy(readableBarrier.texture);Destroy(readableBarrier);}
            if(floatingRelic){Destroy(floatingRelic.texture);Destroy(floatingRelic);}
            foreach(var clip in cueClips.Values)if(clip)Destroy(clip);
            if(material)Destroy(material);
            if(midiTimeline)Destroy(midiTimeline);
            if(scenicBirds!=null)foreach(var bird in scenicBirds)if(bird)Destroy(bird);
            foreach(var pair in sprites)if(pair.Key.StartsWith("fence_"))foreach(var piece in pair.Value)Destroy(piece);
            foreach(var group in groundTiles)if(group!=null)foreach(var tile in group)if(tile)Destroy(tile);
        }
    }
}
