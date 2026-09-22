using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SuyuRun
{
    // Original vector art, animation, music and course authored for SuyuRun.
    public sealed partial class RunnerPrototype : MonoBehaviour
    {
        const float Ground = -2.6f;
        // Real collision heights for standing vs. sliding (see Update()'s overlap check) - the
        // hero sprite's squash during slide must match this exact ratio, not an arbitrary factor,
        // or the visible silhouette can still poke into a duck obstacle even though the hitbox
        // itself already cleared it (see UpdateHeroSprite).
        const float HeroStandHeight = 1.06f, HeroSlideHeight = .48f;
        [SerializeField] RunnerCourse course;
        [SerializeField] bool legacyPrototype;
        public void ConfigureLegacy(RunnerCourse original){legacyPrototype=true;course=original;}
        public bool IsLegacy=>legacyPrototype;
        float Speed => course.speed;
        float Duration => course.duration;
        float Beat => 60f/course.EffectiveBpm;
        enum Mode { Menu, Running, Dead, Complete, Paused, Museum, Ending, Selector, Shop, Settings }
        sealed class Item { public Transform visual; public float x, y, width, height; public int kind; public bool taken,locked; public int cultureIndex=-1; }
        readonly List<Item> items = new List<Item>();
        readonly List<Transform> scenery = new List<Transform>();
        readonly List<Vector3> sceneryBase = new List<Vector3>();
        readonly List<Transform> shots = new List<Transform>();
        readonly List<Mesh> ownedMeshes = new List<Mesh>();
        readonly List<Material> ownedSpriteMaterials = new List<Material>();
        Transform world, hero;
        SpriteRenderer heroRenderer;
        float heroFrameW, heroFrameH;
        Sprite[] heroIdle, heroRun, heroJump, heroLand, heroSlide, heroRafaga;
        Camera cam;
        Material material;
        AudioSource music, effects;
        AudioClip song;
        Mode mode;
        double started;
        float distance, feet, velocity, slide, burst, nextShot, energy = 100, checkpoint, buffer, pausedAt;
        int coins, score, best, bank;
        public bool AutomatedSmokeTest { get; set; }
        public string CurrentState => mode.ToString();
        public float ProgressSeconds => distance / Speed;
        bool standaloneSmoke, smokeCaptured;
        string smokeDirectory;
        int smokeJumps, smokeSlides, smokeBursts;
        int smokeAirJumps, smokeLandings, smokeOrbs, smokePads;
        bool grounded=true, airCharge;
        float coyote, landingPulse, impulsePulse;
        string feedback = "", region = "COSTA";
        string lastFailure="none";
        float feedbackUntil;
        float lastFrame;
        GUIStyle title, text, small, button;
        readonly Color cream = new Color(.96f,.9f,.72f), teal = new Color(.17f,.94f,.83f), gold = new Color(1,.71f,.28f);

        void Awake()
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount=0;
            if(!course)course=Resources.Load<RunnerCourse>("CostaAlcatraz");
            if(!course)course=Resources.Load<RunnerCourse>("CostaVertical");
            if(!course)course=RunnerCourse.VerticalPrototype();
            course.ValidateCourse();region=course.region;
            Application.runInBackground = true;
            var args=Environment.GetCommandLineArgs();
            for(int i=0;i<args.Length-1;i++)if(args[i]=="-suyuSmokeTest") {standaloneSmoke=AutomatedSmokeTest=true;smokeDirectory=args[i+1];Directory.CreateDirectory(smokeDirectory);}
            for(int i=0;i<args.Length-1;i++)if(args[i]=="-suyuTestFps"&&int.TryParse(args[i+1],out int fps))Application.targetFrameRate=Mathf.Clamp(fps,30,144);
            best = PlayerPrefs.GetInt("SuyuRun.Prototype.Best");
            bank = PlayerPrefs.GetInt("SuyuRun.Prototype.Bank");
            if(!legacyPrototype)LoadUpgrades();
            cam = Camera.main;
            if (!cam) { var c = new GameObject("Camera"); cam = c.AddComponent<Camera>(); c.AddComponent<AudioListener>(); }
            cam.orthographic = true; cam.orthographicSize = 5.4f; cam.transform.position = new Vector3(0,0,-10);
            cam.backgroundColor = new Color(.035f,.09f,.15f);
            material = new Material(Resources.Load<Shader>("SuyuFlat"));
            world = new GameObject("Original vector environment").transform;
            BuildScenery(); BuildHero(); BuildCourse();
            if(!legacyPrototype){BuildCoastalEncounters();BuildMuseumDisplays();BuildCoastalEnding();LoadCoastalUIArt();}
            music = gameObject.AddComponent<AudioSource>(); music.loop = true; music.volume = .34f;
            effects = gameObject.AddComponent<AudioSource>(); effects.volume = .23f;
            LoadMusic();
            SetupMenuMusic();
            if(!legacyPrototype)LoadSettings();
            feet = Ground; mode = Mode.Menu;
            lastFrame=Time.realtimeSinceStartup;
        }

        Transform Shape(string name, Transform parent, Vector2 position, Vector2 size, Color color, int sides = 4, float angle = 45, int order = 0)
        {
            var go = new GameObject(name); go.transform.SetParent(parent,false);
            go.transform.localPosition = new Vector3(position.x,position.y,0); go.transform.localScale = new Vector3(size.x,size.y,1);
            var vertices = new Vector3[sides+1]; var triangles = new int[sides*3];
            for (int i=0;i<sides;i++) { float a=(angle+i*360f/sides)*Mathf.Deg2Rad; vertices[i+1]=new Vector3(Mathf.Cos(a)*.5f,Mathf.Sin(a)*.5f); triangles[i*3]=0; triangles[i*3+1]=i+1; triangles[i*3+2]=(i+1)%sides+1; }
            var mesh = new Mesh { name = name+" original mesh", vertices = vertices, triangles = triangles }; mesh.RecalculateBounds();
            ownedMeshes.Add(mesh);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer=go.AddComponent<MeshRenderer>(); renderer.sharedMaterial=material; renderer.sortingOrder=order;
            var colors = new Color[vertices.Length]; for(int i=0;i<colors.Length;i++) colors[i]=color; mesh.colors=colors;
            return go.transform;
        }
        Transform Rect(string n, Transform p, float x,float y,float w,float h,Color c,int order=0)
        { return Shape(n,p,new Vector2(x,y),new Vector2(w*1.414214f,h*1.414214f),c,4,45,order); }

        void BuildScenery()
        {
            if(legacyPrototype)BuildLegacyScenery();else BuildReferenceCoast();
        }
        void BuildHero()
        {
            hero=new GameObject("Suyu - energy runner").transform;
            // Pixel-art protagonist approved under the ART_BIBLE.md workflow (costa_heroe.png),
            // reusing this controller's existing physics/input/collision - only the rendering is
            // swapped, so jump/slide/burst/coyote-time behave exactly as before.
            var all=Resources.LoadAll<Sprite>("costa_heroe");
            if(all==null||all.Length==0){Debug.LogWarning("SuyuRun: costa_heroe sprite sheet not found in Resources; hero has no visual.",this);return;}
            heroIdle=HeroFrames(all,"idle",4);heroRun=HeroFrames(all,"run",8);heroJump=HeroFrames(all,"jump",3);
            heroLand=HeroFrames(all,"land",2);heroSlide=HeroFrames(all,"slide",3);heroRafaga=HeroFrames(all,"rafaga",4);
            var spriteGo=new GameObject("Costa pixel hero");spriteGo.transform.SetParent(hero,false);
            // Sprite pivot is the top-left pixel (ART_BIBLE.md convention); offset so the sprite's
            // bottom-center lands on the hero root, which AnimateHero positions at (x, feet).
            heroFrameH=heroIdle[0].rect.height/heroIdle[0].pixelsPerUnit;heroFrameW=heroIdle[0].rect.width/heroIdle[0].pixelsPerUnit;
            spriteGo.transform.localPosition=new Vector3(-heroFrameW/2f,heroFrameH,0);
            heroRenderer=spriteGo.AddComponent<SpriteRenderer>();heroRenderer.sortingOrder=10;heroRenderer.sprite=heroIdle[0];
        }
        static Sprite[] HeroFrames(Sprite[] all,string name,int count)
        {
            var frames=new Sprite[count];
            foreach(var s in all){int idx=System.Array.IndexOf(new[]{name+"_0",name+"_1",name+"_2",name+"_3",name+"_4",name+"_5",name+"_6",name+"_7"},s.name);if(idx>=0&&idx<count)frames[idx]=s;}
            return frames;
        }
        Sprite[] obstacleSheet;
        // costa_obstaculos.png approved under ART_BIBLE.md (hexagono/pajaro/bote_pescador); loaded
        // once and filtered by name, same pattern as HeroFrames.
        Sprite[] ObstacleFrames(string baseName,int count)
        {
            if(obstacleSheet==null)obstacleSheet=Resources.LoadAll<Sprite>("costa_obstaculos");
            var frames=new Sprite[count];
            foreach(var s in obstacleSheet)for(int i=0;i<count;i++)if(s.name==baseName+"_"+i)frames[i]=s;
            return frames;
        }
        // Centers a top-left-pivot sprite (ART_BIBLE.md convention) on its parent's local origin,
        // at native size (1 world unit per 32px, PPU 32) scaled to `worldSize`.
        Transform PixelSprite(string name,Transform parent,Sprite[] frames,float worldSize,float fps,int order)
        {
            var go=new GameObject(name);go.transform.SetParent(parent,false);
            go.transform.localPosition=new Vector3(-worldSize*.5f,worldSize*.5f,0);
            go.transform.localScale=Vector3.one*worldSize;
            var sr=go.AddComponent<SpriteRenderer>();sr.sortingOrder=order;if(frames!=null&&frames.Length>0)sr.sprite=frames[0];
            go.AddComponent<FrameAnimator>().SetFrames(frames,fps,true);
            return go.transform;
        }
        void BuildCourse()
        {
            // Hand-authored encounter beats: type 0 jump, 1 duck, 2 destructible, 3 coin.
            foreach(var encounter in course.encounters)
                for(int i=0;i<(encounter.kind==EncounterKind.Jump?Mathf.Max(1,encounter.count):1);i++)AddItem(course.BeatToSeconds(encounter.beat)*Speed+i*.85f,(int)encounter.kind);
            for(int i=4;i<Duration/Beat-4;i+=2)
            {
                float x=course.BeatToSeconds(i)*Speed;if(x<0||x>Duration*Speed)continue;
                bool nearPlatform=false;foreach(var p in course.platforms)if(x>p.x-4&&x<p.x+p.width)nearPlatform=true;
                if(RunnerTerrain.HasFloor(course,x)&&!nearPlatform)AddItem(x,3);
            }
            foreach(var point in course.orbs)AddItem(point.x,4,Ground+point.elevation);
            foreach(float x in course.jumpPads)AddItem(x,5,Ground+.12f);
            foreach(var gap in course.gaps)for(int i=1;i<=4;i++)AddItem(gap.x+i*gap.width/5,3,Ground+1.3f+Mathf.Sin(i*Mathf.PI/5)*1.3f);
            foreach(var p in course.platforms)for(int i=1;i<4;i++)AddItem(p.x+i*p.width/4,3,Ground+p.elevation+.8f);
            foreach(var p in course.platforms)for(int i=0;i<4;i++)
            {
                float t=i/3f,x=p.x-3.4f+t*5;
                float baseTop=RunnerTerrain.TopAt(course,p.x-3.4f);if(float.IsNegativeInfinity(baseTop))baseTop=Ground;
                AddItem(x,3,Mathf.Lerp(baseTop,Ground+p.elevation,t)+.7f+Mathf.Sin(t*Mathf.PI)*1.1f);
            }
        }
        void AddItem(float x,int kind,float overrideY=float.NaN)
        {
            float ground=RunnerTerrain.TopAt(course,x);
            if(float.IsNegativeInfinity(ground))ground=Ground;
            float y=kind==1?ground+1.3f:kind==3?ground+.7f:ground+.43f;
            if(!float.IsNaN(overrideY))y=overrideY;
            if(kind==3)foreach(var hazard in items)
                if(hazard.kind==0&&Mathf.Abs(hazard.x-x)<1.8f)y=Mathf.Max(y,hazard.y+hazard.height*.5f+.85f);
            if(kind==3)foreach(var existing in items)
                if(existing.kind==3&&Mathf.Abs(existing.x-x)<.8f&&Mathf.Abs(existing.y-y)<.65f)return;
            float w=kind==1?1.5f:kind==3?.3f:.65f, h=kind==1?.5f:kind==3?.3f:.85f;
            var root=new GameObject(kind==3?"Coin":"Encounter "+kind).transform;
            root.position=new Vector3(x,y,0);
            Color col=kind==3?gold:kind==2?new Color(.95f,.32f,.58f):new Color(.94f,.42f,.26f);
            if(kind==5)
            {
                Rect("Impulse pad carved base",root,0,0,1.3f,.24f,new Color(.22f,.16f,.35f),6);
                Rect("Impulse pad luminous seam",root,0,.15f,1.15f,.08f,new Color(.62f,.35f,1),7);
                for(int i=0;i<3;i++)Shape("Pad upward glyph",root,new Vector2((i-1)*.29f,.29f),new Vector2(.24f,.22f),new Color(.75f,.62f,1),3,90,8);
            }
            else if(kind==4)
            {
                Shape("Impulse outer diamond",root,Vector2.zero,Vector2.one*.95f,new Color(.22f,.12f,.46f),4,0,6);
                Shape("Impulse turquoise diamond",root,Vector2.zero,Vector2.one*.72f,new Color(.4f,1,1),4,0,7);
                Shape("Impulse violet heart",root,Vector2.zero,Vector2.one*.47f,new Color(.6f,.3f,1),4,0,8);
                Rect("Up left",root,-.08f,0,.08f,.22f,Color.white,9).localRotation=Quaternion.Euler(0,0,-35);
                Rect("Up right",root,.08f,0,.08f,.22f,Color.white,9).localRotation=Quaternion.Euler(0,0,35);
            }
            else if(kind==3)Shape("Octagonal sun coin",root,Vector2.zero,new Vector2(.36f,.36f),col,8,22.5f,4);
            else if(kind==0)
            {
                // ART_BIBLE.md "hexágono con patas" ground obstacle, replacing the old flat-color
                // vector spike per the user's request that obstacles be pixel art. Visual only -
                // the collision box below still uses w/h untouched.
                PixelSprite("Pixel obstacle (ART_BIBLE costa_obstaculos hexagono)",root,ObstacleFrames("hexagono",4),.9f,6f,6);
            }
            else Rect("Engraved form",root,0,0,w,h,col,4);
            if(kind>0&&kind<4)Shape("Inlay",root,Vector2.zero,new Vector2(w*.43f,h*.43f),cream,4,0,5);
            items.Add(new Item{visual=root,x=x,y=y,width=w,height=h,kind=kind});
        }
        void Begin(bool resumeCheckpoint=false)
        {
            lastFailure="none";
            distance=resumeCheckpoint?checkpoint:0; feet=Ground; velocity=0; slide=burst=buffer=0; energy=100;
            if(menuMusic)menuMusic.Stop();
            grounded=true;airCharge=false;coyote=0;landingPulse=impulsePulse=0;
            if(!resumeCheckpoint){smokeJumps=smokeSlides=smokeBursts=smokeAirJumps=smokeLandings=smokeOrbs=smokePads=0;}
            else foreach(var it in items)if((it.kind==4||it.kind==5)&&it.x>=checkpoint)it.taken=false;
            if(!resumeCheckpoint) { coins=score=0; checkpoint=0; foreach(var it in items) it.taken=false; }
            ResetCoastalEncounters(resumeCheckpoint);
            foreach(var shot in shots) if(shot) Destroy(shot.gameObject); shots.Clear();
            StartMusic(distance/Speed);
            endingPhase=0;endingTimer=0;if(heroRenderer)heroRenderer.enabled=true;
            mode=Mode.Running; feedback="¡Sigue el pulso!"; feedbackUntil=Time.unscaledTime+2;
        }
        void Update()
        {
            // Recreate transient PCM after an Editor hot reload or asset unload.
            if(!song){LoadMusic();if(mode==Mode.Running)StartMusic(distance/Speed);}
            var k=Keyboard.current; var m=Mouse.current; var pad=Gamepad.current;
            bool jump=(k!=null&&k.spaceKey.wasPressedThisFrame)||(m!=null&&m.leftButton.wasPressedThisFrame)||(pad!=null&&pad.buttonSouth.wasPressedThisFrame);
            bool duck=k!=null&&(k.sKey.isPressed||k.downArrowKey.isPressed)||pad!=null&&pad.buttonEast.isPressed;
            bool fire=k!=null&&k.eKey.wasPressedThisFrame||m!=null&&m.rightButton.wasPressedThisFrame||pad!=null&&pad.rightTrigger.wasPressedThisFrame;
            if(AutomatedSmokeTest)
            {
                if(mode==Mode.Menu)Begin();
                foreach(var obstacle in items)
                {
                    float ahead=obstacle.x-distance;
                    if(obstacle.taken||obstacle.kind>=3||ahead<-1.3f||ahead>5)continue;
                    if(obstacle.kind==1&&ahead<3.5f)duck=true;
                    else if(obstacle.kind==2&&ahead>2&&energy>=100)fire=true;
                    else if(obstacle.kind!=1&&ahead<3.4f&&ahead>.35f&&(grounded||velocity<0))jump=true;
                }
                foreach(var p in course.platforms)if(p.x-distance<(feet>Ground+.1f?4.7f:2.8f)&&p.x-distance>1.1f&&feet<Ground+p.elevation-.05f&&grounded)jump=true;
                foreach(var gap in course.gaps)if(gap.x-distance<1.6f&&gap.x-distance>0&&grounded)jump=true;
                foreach(var bird in birds)if(!bird.defeated&&bird.x-distance<3.4f&&bird.x-distance>1.5f&&grounded&&burst<=0)jump=true;
                if(airCharge&&!grounded&&velocity<3)jump=true;
            }
            // Latch the press into `buffer` the instant it happens, at real Update() cadence -
            // distance/physics below only advance in chunks of AudioSettings.dspTime, which ticks
            // once per audio buffer (~21ms) rather than every rendered frame, so a press landing in
            // a frame where dspTime hasn't moved yet used to be silently dropped (the substep loop
            // that used to read `jump` never ran that frame). Latching here first means no render
            // frame's press is missed regardless of the audio clock's cadence.
            if(jump)buffer=.13f;
            if(k!=null&&k.escapeKey.wasPressedThisFrame){if(mode==Mode.Museum||mode==Mode.Selector||mode==Mode.Shop||mode==Mode.Settings)ReturnToMenu();else TogglePause();}
            if(k!=null&&k.mKey.wasPressedThisFrame&&!legacyPrototype&&(mode==Mode.Menu||mode==Mode.Complete))OpenMuseum();
            if(mode==Mode.Menu && k!=null&&k.enterKey.wasPressedThisFrame) Begin();
            if((mode==Mode.Dead||mode==Mode.Complete)&&k!=null&&k.rKey.wasPressedThisFrame) Begin();
            float dt=Mathf.Min(Time.deltaTime,.05f);
            float actualDelta=Time.realtimeSinceStartup-lastFrame;lastFrame=Time.realtimeSinceStartup;
            if(mode==Mode.Running&&actualDelta>.2f)StartMusic(distance/Speed);
            if(mode==Mode.Running)
            {
                // Small DSP-driven physics steps keep jump arcs/collisions independent of
                // frame rate. Previously horizontal time advanced fully while vertical dt
                // was capped at .05, making the same jump fail after a slow editor frame.
                float targetDistance=Mathf.Max(distance,(float)(AudioSettings.dspTime-started)*Speed);
                float remaining=(targetDistance-distance)/Speed;
                while(remaining>.000001f&&mode==Mode.Running)
                {
                dt=Mathf.Min(remaining,1f/120f);remaining-=dt;
                float previous=distance;distance+=dt*Speed;
                grounded=grounded&&RunnerTerrain.Landing(course,distance,feet,feet-.03f,out float support)&&Mathf.Abs(feet-support)<.035f;
                coyote=grounded?.09f:Mathf.Max(0,coyote-dt);
                foreach(var padItem in items)if(padItem.kind==5&&!padItem.taken&&grounded&&previous<=padItem.x&&distance>=padItem.x)
                {padItem.taken=true;velocity=13.4f;grounded=false;coyote=buffer=0;impulsePulse=.45f;smokePads++;feedback="¡IMPULSO DE SUELO!";feedbackUntil=Time.unscaledTime+1;Beep(780,.15f);}
                foreach(var orb in items)if(orb.kind==4&&!orb.taken&&Mathf.Abs(orb.x-distance)<.9f&&Mathf.Abs(orb.y-(feet+.6f))<1.1f)
                {orb.taken=true;airCharge=true;smokeOrbs++;feedback="IMPULSO LISTO · ESPACIO";feedbackUntil=Time.unscaledTime+1.2f;Beep(1400,.1f);}
                if(buffer>0&&(grounded||coyote>0)) { velocity=11.5f;buffer=0;coyote=0;grounded=false;smokeJumps++;Judge();Beep(660,.1f); }
                else if(buffer>0&&airCharge&&!grounded){velocity=11.5f;airCharge=false;buffer=0;impulsePulse=.45f;smokeAirJumps++;Judge();Beep(880,.14f);}
                else buffer-=dt;
                float oldFeet=feet;
                velocity-=28*dt;feet+=velocity*dt;
                if(RunnerTerrain.Landing(course,distance,oldFeet,feet,out float surface))
                {
                    if(!grounded&&velocity<-1){landingPulse=.2f;if(surface>Ground+.1f)smokeLandings++;}
                    feet=surface;velocity=0;grounded=true;airCharge=false;
                }
                else grounded=false;
                if(RunnerTerrain.HitsWall(course,previous,distance,feet)||feet<Ground-4){lastFailure="platform wall or fall at "+distance;mode=Mode.Dead;music.Pause();Beep(100,.25f);}
                float oldSlide=slide;slide=duck&&grounded?1:0;if(slide>0&&oldSlide==0)smokeSlides++;
                if(fire&&energy>=100) { burst=3+upgradeRafaga;energy=0; nextShot=0; smokeBursts++; Judge(); }
                fire=false;
                energy=Mathf.Min(100,energy+dt*5); burst=Mathf.Max(0,burst-dt);
                if(burst>0) { nextShot-=dt; if(nextShot<=0) { nextShot=.12f; shots.Add(Rect("Light pulse",null,-2.4f,feet+.55f,.45f,.1f,cream,20)); Beep(980,.025f); } }
                for(int i=shots.Count-1;i>=0;i--) { shots[i].position+=Vector3.right*23*dt; if(shots[i].position.x>13) { Destroy(shots[i].gameObject);shots.RemoveAt(i); } }
                // Duck obstacles only need the ground-slide hitbox reduction while grounded on it,
                // not while airborne - but several duck bars sit right after a step-up platform
                // jump, close enough that the hero can still be mid-landing when it reaches them.
                // Holding duck now shrinks the collision box in the air too (the visible slide pose
                // still only plays while grounded, per `slide` above), so "duck through" keeps
                // working even if you're not back on the ground yet.
                float height=duck?HeroSlideHeight:HeroStandHeight;
                foreach(var it in items)
                {
                    if(it.taken||it.locked||it.kind==4||it.kind==5)continue;
                    float sx=it.x-distance-3;
                    if(it.kind==2) foreach(var shot in shots) if(Mathf.Abs(shot.position.x-sx)<.65f&&Mathf.Abs(shot.position.y-it.y)<.8f) { it.taken=true;score+=100;coins+=3;ReleaseMemory(it);Beep(330,.08f);break; }
                    if(it.taken)continue;
                    // Coin-magnet shop upgrade widens the coin pickup box only (other kinds unaffected).
                    float magnet=it.kind==3?upgradeIman*.4f:0;
                    bool overlap=it.x-it.width/2-magnet < distance+.33f+magnet && it.x+it.width/2+magnet>previous-.33f-magnet && feet<it.y+it.height/2+magnet && feet+height>it.y-it.height/2-magnet;
                    if(overlap&&it.kind==0)
                    {
                        float intersectionHeight=Mathf.Clamp01((feet-(it.y-it.height/2))/it.height);
                        float bladeHalfWidth=it.width*.5f*(1-intersectionHeight);
                        overlap=it.x-bladeHalfWidth<distance+.3f&&it.x+bladeHalfWidth>previous-.3f;
                    }
                    if(overlap) { if(it.kind==3) {it.taken=true;coins++;score+=25;energy=Mathf.Min(100,energy+4);Beep(1100,.055f);} else if(it.kind==6){it.taken=true;DiscoverCulture(it.cultureIndex);}else {lastFailure=$"encounter kind={it.kind} x={it.x} feet={feet} slide={slide} velocity={velocity}";mode=Mode.Dead;music.Pause();Beep(100,.25f);break;} }
                }
                UpdateCoastalCombat(dt,previous);
                float candidate=Mathf.Floor(distance/(Speed*course.checkpointSeconds))*Speed*course.checkpointSeconds;
                bool combatSafe=true;foreach(var bird in birds)if(!bird.defeated&&Mathf.Abs(bird.x-candidate)<6)combatSafe=false;
                if(candidate>checkpoint&&combatSafe&&RunnerTerrain.SafeCheckpoint(course,candidate))checkpoint=candidate;
                if(distance>=Duration*Speed&&mode==Mode.Running&&!legacyPrototype) { mode=Mode.Ending;endingPhase=0;endingTimer=0;music.Stop(); }
                else if(distance>=Duration*Speed&&mode==Mode.Running) { mode=Mode.Complete;music.Stop();bank+=coins;best=Mathf.Max(best,score);if(!AutomatedSmokeTest){PlayerPrefs.SetInt("SuyuRun.Prototype.Bank",bank);PlayerPrefs.SetInt("SuyuRun.Prototype.Best",best);PlayerPrefs.Save();} }
                }
            }
            foreach(var it in items) {it.visual.gameObject.SetActive(mode!=Mode.Museum&&!it.taken&&!it.locked&&Mathf.Abs(it.x-distance)<24);it.visual.position=new Vector3(it.x-distance-3,it.y,0);if(it.kind==3)it.visual.localRotation=Quaternion.Euler(0,0,Time.time*80);}
            for(int i=0;i<scenery.Count;i++) {Vector3 p=sceneryBase[i];p.x=Mathf.Repeat(p.x-distance*.15f+24,48)-24;scenery[i].position=p;}
            if(!legacyPrototype)UpdateReferenceCoast();
            UpdateMuseumDisplays();
            AnimateHero();
            if(!legacyPrototype)UpdateCoastalEnding();
            if(standaloneSmoke)
            {
                if(ProgressSeconds>10&&!smokeCaptured){smokeCaptured=true;ScreenCapture.CaptureScreenshot(Path.Combine(smokeDirectory,"prototype-play.png"));}
                if(mode==Mode.Dead||mode==Mode.Complete)
                {
                    File.WriteAllText(Path.Combine(smokeDirectory,"smoke-result.txt"),SmokeReport);
                    Application.Quit(mode==Mode.Complete?0:1);standaloneSmoke=false;
                }
            }
        }
        void AnimateHero()
        {
            float t=mode==Mode.Running?distance*2:Time.time*2;
            hero.position=new Vector3(-3,feet,0);
            bool run=mode==Mode.Running;
            landingPulse=Mathf.Max(0,landingPulse-Time.deltaTime);impulsePulse=Mathf.Max(0,impulsePulse-Time.deltaTime);
            UpdateHeroSprite(t,run);
            hero.gameObject.SetActive(mode!=Mode.Dead&&mode!=Mode.Museum);
            // Lazily created here (not in the legacy UpdateCoastalArt, which the current pixel-art
            // reference-coast path never calls) so the "impulse ready" orbit sparks reliably show
            // up at every air-charge orb, not just wherever UpdateCoastalArt happened to run once.
            if(!impulseVisual&&hero)
            {
                impulseVisual=new GameObject("Air impulse orbit").transform;impulseVisual.SetParent(hero,false);impulseVisual.localPosition=new Vector3(0,.65f,0);
                for(int i=0;i<4;i++){float a=i*Mathf.PI*.5f;Shape("Orbit spark",impulseVisual,new Vector2(Mathf.Cos(a)*.7f,Mathf.Sin(a)*.7f),Vector2.one*.18f,new Color(.55f,.3f,1),4,0,17);}
            }
            if(impulseVisual){impulseVisual.gameObject.SetActive(airCharge||impulsePulse>0);impulseVisual.localScale=Vector3.one*(airCharge?1:1+impulsePulse*3);impulseVisual.localRotation=Quaternion.Euler(0,0,Time.time*90);}
        }
        void UpdateHeroSprite(float t,bool run)
        {
            if(!heroRenderer||heroIdle==null)return;
            if(mode==Mode.Ending&&heroLogro!=null&&heroLogro.Length>0)
            {
                int logroIndex=endingPhase==0?Mathf.Clamp(Mathf.FloorToInt(endingTimer*3f),0,heroLogro.Length-1):heroLogro.Length-1;
                if(heroLogro[logroIndex])heroRenderer.sprite=heroLogro[logroIndex];
                return;
            }
            Sprite[] frames;int index;float cycle;
            if(!grounded){frames=heroJump;index=velocity>1f?1:velocity<-1f?2:0;}
            else if(slide>0){frames=heroSlide;cycle=Mathf.Repeat(t*.5f,1f);index=Mathf.Clamp(Mathf.FloorToInt(cycle*heroSlide.Length),0,heroSlide.Length-1);}
            else if(burst>0){frames=heroRafaga;cycle=Mathf.Repeat(t*.6f,1f);index=Mathf.Clamp(Mathf.FloorToInt(cycle*heroRafaga.Length),0,heroRafaga.Length-1);}
            else if(landingPulse>.08f){frames=heroLand;index=landingPulse>.15f?0:1;}
            else if(run&&grounded){frames=heroRun;cycle=Mathf.Repeat(t*.35f,1f);index=Mathf.Clamp(Mathf.FloorToInt(cycle*heroRun.Length),0,heroRun.Length-1);}
            else{frames=heroIdle;cycle=Mathf.Repeat(t*.12f,1f);index=Mathf.Clamp(Mathf.FloorToInt(cycle*heroIdle.Length),0,heroIdle.Length-1);}
            if(frames!=null&&frames.Length>0&&frames[index])heroRenderer.sprite=frames[index];
            // Second bug fix: squashing to an arbitrary .85 left the rendered sprite ~1.27 units
            // tall while sliding - still taller than the duck bar's own bottom edge (1.05 above
            // ground), so the silhouette visibly poked into it even though the real hitbox (0.48
            // tall) had already cleared it cleanly. The vertical squash now matches that hitbox
            // ratio exactly, so the visible character is never taller than what's actually safe.
            var squash=slide>0?new Vector3(1.12f,HeroSlideHeight/HeroStandHeight,1):landingPulse>0?new Vector3(1+landingPulse*.3f,1-landingPulse*.3f,1):Vector3.one;
            heroRenderer.transform.localScale=squash;
            // The sprite's local origin is its top-left pixel (BuildHero), so scaling the transform
            // without repositioning it shrinks the sprite DOWNWARD FROM THE HEAD, pulling the feet
            // up off the ground instead of crouching the head down. Recomputing the local position
            // from the current scale keeps the sprite's bottom edge pinned at `feet`.
            heroRenderer.transform.localPosition=new Vector3(-heroFrameW*.5f*squash.x,heroFrameH*squash.y,0);
        }
        public string SmokeReport=>$"State={mode}\nSeconds={ProgressSeconds}\nDistance={distance}\nFeet={feet}\nJumps={smokeJumps}\nAirJumps={smokeAirJumps}\nElevatedLandings={smokeLandings}\nOrbs={smokeOrbs}\nPads={smokePads}\nSlides={smokeSlides}\nBursts={smokeBursts}\nCoins={coins}\nScore={score}\nBirdsDefeated={birdsDefeated}\nMuseum={DiscoveredCount}/4\n{SeaSmokeReport}\n";
        void Judge() {float t=Mathf.Max(0,(float)(AudioSettings.dspTime-started));float phase=course.SecondsToBeat(t);float error=Mathf.Abs(t-course.BeatToSeconds(Mathf.Round(phase)));feedback=error<.09f?"PERFECTO":error<.18f?"BIEN":"SIGUE EL PULSO";if(error<.18f){score+=error<.09f?100:50;energy=Mathf.Min(100,energy+8);}feedbackUntil=Time.unscaledTime+.65f;}
        void TogglePause() {if(mode==Mode.Running){pausedAt=Mathf.Max(0,(float)(AudioSettings.dspTime-started));mode=Mode.Paused;music.Pause();}else if(mode==Mode.Paused){StartMusic(pausedAt);mode=Mode.Running;}}
        void Beep(float hz,float duration) {int count=(int)(22050*duration);var data=new float[count];for(int i=0;i<count;i++)data[i]=Mathf.Sin(i*hz*2*Mathf.PI/22050)*(.5f-i/(float)count*.5f);var clip=AudioClip.Create("Original synthesized cue",count,1,22050,false);clip.SetData(data,0);effects.PlayOneShot(clip);Destroy(clip,duration+1);}
        AudioClip Compose()
        {
            const int rate=22050;var data=new float[Mathf.RoundToInt(rate*32*Beat)];int[] melody={0,4,7,9,7,4,2,4,0,7,9,12,9,7,4,2};
            for(int i=0;i<data.Length;i++) {float t=i/(float)rate;int beat=(int)(t/Beat);float p=t%Beat;float f=220*Mathf.Pow(2,melody[beat%melody.Length]/12f);float pluck=(Mathf.Sin(2*Mathf.PI*f*p)+.25f*Mathf.Sin(4*Mathf.PI*f*p))*Mathf.Exp(-p*10)*.19f;float bass=Mathf.Sin(2*Mathf.PI*55*t)*Mathf.Exp(-p*7)*.17f;float kick=Mathf.Sin(2*Mathf.PI*(65*p-30*p*p))*Mathf.Exp(-p*24)*.3f;float hat=Mathf.Sin(i*1.73f)*Mathf.Sin(i*.97f)*Mathf.Exp(-(t%.25f)*90)*.05f;data[i]=pluck+bass+kick+hat; }
            var clip=AudioClip.Create("Suyu - original coastal prototype sketch 120 BPM",data.Length,1,rate,false);clip.SetData(data,0);return clip;
        }
        void OnGUI()
        {
            if(title==null)
            {
                // ART_BIBLE.md section 4: "Fuente pixel (Press Start 2P / m5x7)" for every label,
                // number and button in the game - Unity's default label/button font was never
                // that, on any screen. Sizes are much smaller than the old default-font values
                // because Press Start 2P's blocky glyphs are far wider per point size; no bold
                // variant exists for it, so title reads big through fontSize alone, not synthetic bold.
                var pixelFont=Resources.Load<Font>("PressStart2P-Regular");
                title=new GUIStyle(GUI.skin.label){fontSize=28,font=pixelFont};
                text=new GUIStyle(GUI.skin.label){fontSize=16,font=pixelFont};
                small=new GUIStyle(GUI.skin.label){fontSize=12,font=pixelFont};
                button=new GUIStyle(GUI.skin.button){fontSize=14,font=pixelFont};
                title.normal.textColor=text.normal.textColor=small.normal.textColor=cream;
            }
            float scale=Mathf.Min(Screen.width/1280f,Screen.height/720f);
            if(!legacyPrototype)scale=Mathf.Max(1,Mathf.Floor(Mathf.Min(Screen.width/320f,Screen.height/180f)))/4f;
            float ox=(Screen.width-1280*scale)*.5f,oy=(Screen.height-720*scale)*.5f;
            GUI.matrix=Matrix4x4.identity;GUI.color=Color.black;
            if(oy>0){GUI.DrawTexture(new Rect(0,0,Screen.width,oy),Texture2D.whiteTexture);GUI.DrawTexture(new Rect(0,Screen.height-oy,Screen.width,oy),Texture2D.whiteTexture);}
            if(ox>0){GUI.DrawTexture(new Rect(0,0,ox,Screen.height),Texture2D.whiteTexture);GUI.DrawTexture(new Rect(Screen.width-ox,0,ox,Screen.height),Texture2D.whiteTexture);}
            GUI.color=Color.white;
            if(legacyPrototype)cam.rect=new Rect(ox/Screen.width,oy/Screen.height,1280*scale/Screen.width,720*scale/Screen.height);
            GUI.matrix=Matrix4x4.TRS(new Vector3(ox,oy,0),Quaternion.identity,new Vector3(scale,scale,1));
            if(mode==Mode.Museum){DrawMuseum();return;}
            if(mode==Mode.Selector&&!legacyPrototype){DrawSelector();return;}
            if(mode==Mode.Shop&&!legacyPrototype){DrawShop();return;}
            if(mode==Mode.Settings&&!legacyPrototype){DrawSettings();return;}
            if(mode==Mode.Menu&&!legacyPrototype){DrawTitleScreen();return;}
            if(!legacyPrototype&&(mode==Mode.Running||mode==Mode.Paused))
            {
                DrawCompactCoastHud();
                if(mode==Mode.Running)return;
            }
            GUI.color=new Color(.035f,.11f,.18f,.88f);GUI.DrawTexture(new Rect(22,15,1236,85),Texture2D.whiteTexture);GUI.color=Color.white;
            GUI.Label(new Rect(42,25,450,50),"SUYU / RUN",text);GUI.Label(new Rect(42,62,480,35),region+"     /     RUTA DE LOS IMPULSOS",small);
            GUI.Label(new Rect(830,25,410,40),$"MONEDAS {coins:00}    PUNTOS {score:0000}",text);
            if(mode==Mode.Running)
            {
                GUI.color=new Color(.035f,.11f,.18f,.85f);GUI.DrawTexture(new Rect(24,626,1232,42),Texture2D.whiteTexture);GUI.DrawTexture(new Rect(24,107,275,60),Texture2D.whiteTexture);GUI.color=Color.white;
                string hint=airCharge?"IMPULSO CARGADO · pulsa ESPACIO en el aire":distance<35?"ESPACIO: salta · El borde claro es tu apoyo":distance>105&&distance<128?"SALTA AL HUECO → recoge el rombo → pulsa ESPACIO otra vez":distance>157&&distance<176?"BASE VIOLETA: te impulsa al pisarla. Prepárate para aterrizar.":"";
                if(hint.Length>0){GUI.color=new Color(.035f,.11f,.18f,.9f);GUI.DrawTexture(new Rect(350,105,890,38),Texture2D.whiteTexture);GUI.color=Color.white;GUI.Label(new Rect(370,111,855,32),hint,small);}
            }
            if(mode==Mode.Running||mode==Mode.Paused) {GUI.Label(new Rect(42,115,330,32),$"RÁFAGA  {(burst>0?burst.ToString("0.0")+" s":((int)energy)+"%")}",text);GUI.color=teal;GUI.DrawTexture(new Rect(42,150,230*energy/100,5),Texture2D.whiteTexture);GUI.color=gold;GUI.DrawTexture(new Rect(42,680,1196*Mathf.Clamp01(distance/(Speed*Duration)),4),Texture2D.whiteTexture);GUI.color=Color.white;GUI.Label(new Rect(42,636,1100,32),"ESPACIO · saltar     S / ↓ · deslizar     E · ráfaga     ESC · pausa",small);if(Time.unscaledTime<feedbackUntil){Panel(new Rect(345,153,895,48),new Color(.025f,.095f,.14f,.86f));GUI.Label(new Rect(361,162,860,36),feedback,small);}}
            if(mode==Mode.Menu||mode==Mode.Dead||mode==Mode.Complete||mode==Mode.Paused)
            {
                // costa_resultados.png replaces the flat box for the real pixel-art path (Menu
                // never reaches here - DrawTitleScreen already returned above). Panel kept at its
                // native 220x140 aspect (x2.609) instead of stretched into the old wider box.
                bool skin=!legacyPrototype&&texResultados;
                Rect panelRect=new Rect(353,185,574,365);
                if(skin)DrawTex(panelRect,texResultados);
                else{GUI.color=new Color(.025f,.065f,.11f,.96f);GUI.DrawTexture(new Rect(285,185,710,365),Texture2D.whiteTexture);GUI.color=Color.white;}
                string heading=mode==Mode.Menu?"EL PULSO DEL VIAJE":mode==Mode.Dead?"VUELVE A INTENTAR":mode==Mode.Complete?"¡RUTA COMPLETADA!":"PAUSA";
                Rect headRect=skin?new Rect(panelRect.x+10,198,panelRect.width-20,36):new Rect(320,212,680,75);
                GUI.Label(headRect,heading,skin?new GUIStyle(title){fontSize=20}:title);
                string body=mode==Mode.Menu?"Un núcleo de luz. Un camino por descubrir.\nSalta, desliza y libera tu energía en la costa.":mode==Mode.Complete?(legacyPrototype?$"Monedas guardadas: {bank}   /   Récord: {best}":"SIERRA DESBLOQUEADA"):mode==Mode.Dead?"Naranja: salta o desliza. Rosa: dispara o salta.":"La música y el recorrido esperan contigo.";
                Rect bodyRect=skin?new Rect(panelRect.x+20,245,panelRect.width-40,mode==Mode.Complete?36:120):new Rect(325,295,650,85);
                GUI.Label(bodyRect,body,skin?new GUIStyle(text){fontSize=13,wordWrap=true}:text);
                if(skin&&mode==Mode.Complete)
                {
                    // The stat rows + piece slots costa_resultados.png was actually drawn for -
                    // real numbers and real discovered-piece state, only shown for the genuine
                    // results moment (Dead/Paused reuse the same panel frame but skip these).
                    (Color tint,string label)[] rows={(new Color32(232,185,82,255),$"MONEDAS {coins:00}"),(new Color32(119,179,163,255),$"PUNTOS {score:0000}"),(gold,$"BANCO {bank}")};
                    for(int i=0;i<3;i++)
                    {
                        float ry=panelRect.y+28*2.609f+i*16*2.609f;
                        GUI.color=rows[i].tint;GUI.DrawTexture(new Rect(panelRect.x+26,ry,26,26),Texture2D.whiteTexture);GUI.color=Color.white;
                        GUI.Label(new Rect(panelRect.x+62,ry+2,panelRect.width-90,26),rows[i].label,new GUIStyle(small){fontSize=12});
                    }
                    for(int i=0;i<4;i++)
                    {
                        float px_=panelRect.x+(10+i*24)*2.609f,py_=panelRect.y+94*2.609f;
                        if(texMuseoVitrina)DrawTex(new Rect(px_,py_,47,47),texMuseoVitrina);
                    }
                }
                Rect btn1=skin?new Rect(panelRect.x+26,503,224,31):new Rect(325,402,280,52);
                Rect btn2=skin?new Rect(panelRect.x+323,503,224,31):new Rect(625,402,320,52);
                var btnStyle=skin?new GUIStyle(button){fontSize=12}:button;
                if(GUI.Button(btn1,mode==Mode.Paused?"CONTINUAR":"JUGAR  /  ENTER",btnStyle)){if(mode==Mode.Paused)TogglePause();else Begin();}
                if(mode==Mode.Dead&&checkpoint>0&&coins>=5&&GUI.Button(btn2,"REVIVIR · 5 MONEDAS",btnStyle)){coins-=5;Begin(true);}
                if(mode==Mode.Complete&&!legacyPrototype&&GUI.Button(btn2,"VER MUSEO · "+DiscoveredCount+" / 4",btnStyle))OpenMuseum();
                if(legacyPrototype&&GUI.Button(btn2,"IR A COSTA ACTUAL",btnStyle))UnityEngine.SceneManagement.SceneManager.LoadScene("Level_Costa");
                if(mode==Mode.Paused&&GUI.Button(btn2,"VOLVER AL INICIO",btnStyle))ReturnToMenu();
                if(!skin)GUI.Label(new Rect(325,480,650,40),course.HasRecordedMusic?$"{Duration:0} segundos · {course.soundtrack.trackTitle}":$"{Duration:0} segundos · {course.bpm:0} BPM · Música provisional original",small);
            }
        }
        void OnDestroy(){foreach(var sprite in referenceSprites)if(sprite)Destroy(sprite);foreach(var mesh in ownedMeshes)if(mesh)Destroy(mesh);foreach(var mat in ownedSpriteMaterials)if(mat)Destroy(mat);if(material)Destroy(material);if(panoramaMaterial)Destroy(panoramaMaterial);if(panoramaSprite)Destroy(panoramaSprite);if(song&&ownsSong)Destroy(song);}
    }
}
