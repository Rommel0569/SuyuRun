using UnityEngine;
namespace SuyuRun
{
    // Cinematic-only actors. The runner, collision geometry and save data are untouched.
    public sealed class RunnerFinalMovie:MonoBehaviour
    {
        public static bool Active {get;private set;}
        public const float FilmSeconds=20,CreditsSeconds=10,TotalSeconds=30;
        const float ActorSize=68;
        readonly FinalMovieArt art=new FinalMovieArt();
        double began,pausedAt; AudioSource track,foley; float volume; bool ready,preview,paused;
        float previousAge=-1;
        FinalMovieScore score=new FinalMovieScore();
        AudioClip shot,step,air;
        readonly int[] collected=new int[12];int collectedCount;
        public float Age=>Mathf.Max(0,(float)((paused?pausedAt:AudioSettings.dspTime)-began));
        public static readonly string[] RequiredAssets={
            "costa_sky","costa_mid","costa_ground","sierra_sky","sierra_mid","sierra_ground",
            "selva_sky","selva_mid","selva_ground","costa_hero","sierra_hero","selva_hero",
            "selva_portrait","selva_fx","selva_memories","costa_life","sierra_life","selva_life",
            "epilogue_sky","epilogue_mid","epilogue_ground","epilogue_portal_back","epilogue_portal_front","epilogue_hero"};
        public static void Begin(GameObject host){if(!Active)host.AddComponent<RunnerFinalMovie>();}
        public void KeepPreviewOpen(){preview=true;}
        public void Replay()
        {
            began=AudioSettings.dspTime;paused=false;previousAge=-1;
            if(track){track.time=0;track.Play();}if(foley)foley.Stop();
        }
        public void TogglePause()
        {
            if(!paused){pausedAt=AudioSettings.dspTime;paused=true;if(track)track.Pause();if(foley)foley.Pause();}
            else{began+=AudioSettings.dspTime-pausedAt;paused=false;if(track)track.UnPause();if(foley)foley.UnPause();}
        }
        void Awake()
        {
            try
            {
                foreach(var name in RequiredAssets)art.Load(name);
                var data=Resources.Load<TextAsset>("FinalMovieV4/score");if(data)score=JsonUtility.FromJson<FinalMovieScore>(data.text);
                score.Validate();
                track=gameObject.AddComponent<AudioSource>();track.clip=Resources.Load<AudioClip>("FinalMovie/final_music");
                track.loop=true;track.playOnAwake=false;track.spatialBlend=0;track.volume=0;
                foley=gameObject.AddComponent<AudioSource>();foley.playOnAwake=false;foley.spatialBlend=0;
                shot=Resources.Load<AudioClip>("Sfx/shot");
                step=MakeFoley("FilmFootstep",.12f,0);air=MakeFoley("FilmAir",.26f,1);
                volume=.34f*PlayerPrefs.GetFloat("SuyuRun.Settings.MusicVol",1);
                for(int region=0;region<3;region++)for(int i=0;i<(region==1?3:4);i++)
                    if(SharedGameMenu.IsCollected(region,i))collected[collectedCount++]=region*4+i;
                began=AudioSettings.dspTime;ready=true;Active=true;if(track.clip)track.Play();
            }
            catch(System.Exception ex){Debug.LogException(ex);Destroy(this);}
        }
        // Quiet, deterministic noise-based foley, not substituted music or visual artwork.
        static AudioClip MakeFoley(string name,float seconds,int kind)
        {
            const int rate=22050;var values=new float[Mathf.CeilToInt(rate*seconds)];uint seed=7141;
            float low=0;
            for(int i=0;i<values.Length;i++)
            {
                seed=1664525*seed+1013904223;float noise=((seed>>8)/16777215f)*2-1;
                low=Mathf.Lerp(low,noise,kind==0?.14f:.045f);float u=i/(float)values.Length;
                float envelope=kind==0?Mathf.Exp(-u*9):Mathf.Sin(u*Mathf.PI);
                values[i]=(low+(kind==0?Mathf.Sin(i*2*Mathf.PI*90/rate)*.20f:0))*envelope;
            }
            var clip=AudioClip.Create(name,values.Length,1,rate,false);clip.SetData(values,0);return clip;
        }
        void Cue(float time,AudioClip clip,float gain)
        {
            if(previousAge<time&&Age>=time&&Age-time<.2f&&clip)
                foley.PlayOneShot(clip,gain*PlayerPrefs.GetFloat("SuyuRun.Settings.EffectsVol",1));
        }
        float CoastDuration=>score.cuts[1]-score.cuts[0];
        float SierraDuration=>score.cuts[2]-score.cuts[1];
        float SelvaDuration=>score.cuts[3]-score.cuts[2];
        void Update()
        {
            if(!ready||paused)return;
            float age=Age;
            if(track)track.volume=volume*Mathf.SmoothStep(0,1,age/.75f)*Mathf.SmoothStep(0,1,(TotalSeconds-age)/1.65f);
            Cue(score.cuts[0]+CoastDuration*(2.15f/3.5f),step,.11f);
            Cue(score.cuts[1]+SierraDuration*.52f,air,.10f);
            Cue(score.cuts[2]-.14f,air,.07f);
            Cue(score.cuts[2]+SelvaDuration*(1.55f/4.5f),air,.08f);
            Cue(score.cuts[2]+SelvaDuration*(LandingTime()/4.5f),step,.11f);
            Cue(score.cuts[2]+SelvaDuration*(3.2f/4.5f),shot,.045f);
            Cue(score.cuts[3]-.15f,air,.09f);
            previousAge=age;
            if(age>=TotalSeconds&&!preview)Destroy(this);
        }
        void OnDestroy()
        {
            Active=false;if(track){track.Stop();Destroy(track);}if(foley){foley.Stop();Destroy(foley);}
            if(step)Destroy(step);if(air)Destroy(air);
        }
        static float Ease(float t)=>Mathf.SmoothStep(0,1,Mathf.Clamp01(t));
        static int Pose(float t,int first,float[] exposures)
        {
            for(int i=0;i<exposures.Length;i++){if(t<exposures[i])return first+i;t-=exposures[i];}
            return first+exposures.Length-1;
        }
        void Band(string name,float travel,float speed,float offset=0)
        {
            float x=-Mathf.Repeat(travel*speed,640)-offset;
            float y=name.EndsWith("_ground")?146-art.Surface(name)*180:0;
            art.Draw(name,new Rect(x,y,640,180));if(speed!=0)art.Draw(name,new Rect(x+640,y,640,180));
        }
        void Ground(string r,float travel,float gap=-1000)
        {
            if(gap<-100){Band(r+"_ground",travel,58);return;}
            float left=Mathf.Clamp(gap,0,320),right=Mathf.Clamp(gap+48,0,320);
            if(left>0){GUI.BeginGroup(new Rect(0,0,left,180));Band(r+"_ground",travel,58);GUI.EndGroup();}
            if(right<320){GUI.BeginGroup(new Rect(right,0,320-right,180));Band(r+"_ground",travel,58,right);GUI.EndGroup();}
        }
        void World(string r,float age,float travel,float gap=-1000)
        {
            Band(r+"_sky",age,.35f);Band(r+"_mid",travel,10);
            Ambient(r,age);Ground(r,travel,gap);
            if(r=="selva")for(int i=0;i<7;i++)
            {
                float x=Mathf.Repeat(38+i*51-age*(3+i%3),340)-10;
                art.Draw("selva_fx",new Rect(x,62+i%3*24+Mathf.Sin(age*1.6f+i)*2,5,5),12+(int)(age*6+i)%4);
            }
        }
        void Ambient(string r,float t)
        {
            if(r=="costa")
            {
                art.Range("costa_life",new Rect(Mathf.Repeat(250-t*15,360)-20,43+Mathf.Sin(t*2)*2,34,28),t,0,4,8,true);
                art.Range("costa_life",new Rect(80,139,170,15),t,4,4,8,true);
            }
            else if(r=="sierra")
            {
                art.Range("sierra_life",new Rect(260-t*19,39+Mathf.Sin(t)*3,45,32),t,0,4,8,true);
                for(int i=0;i<5;i++)art.Range("sierra_life",new Rect(12+i*69,127,22,24),t+i*.15f,4,4,6,true);
            }
            else
            {
                art.Range("selva_life",new Rect(255-t*14,43+Mathf.Sin(t*2)*3,31,28),t,0,4,10,true);
                art.Range("selva_life",new Rect(-6,58,43,38),t,4,4,6,true);
                art.Range("selva_life",new Rect(288,73,43,38),t+.3f,4,4,6,true);
                art.Range("selva_life",new Rect(90+t*5,78+Mathf.Sin(t*2)*5,10,10),t,8,4,12,true);
                art.Range("selva_life",new Rect(192,119,62,9),t,12,4,8,true);
            }
        }
        void Hero(string r,int f,float x=156,float feet=146,float size=ActorSize)
        {art.Draw(r+"_hero",new Rect(x-size/2,feet-art.Foot(r+"_hero",f)*size,size,size),f);}
        void Fx(float age,int first,float x,float y,float size=26)
        {if(age>=0&&age<.36f)art.Range("selva_fx",new Rect(x-size/2,y-size/2,size,size),age,first,4,12);}
        void Opening(float t)
        {
            World("costa",t,t);
            float x=Mathf.Lerp(124,156,Ease(t/2.5f));
            int f=(int)(t*12)%8;Hero("costa",f,x);
            Fx(Mathf.Repeat(t,.32f),0,x-17,146,17);
        }
        void Coast(float t)
        {
            World("costa",t+2.5f,t+2.5f,224-t*58);
            int f;float feet=146;
            if(t<.35f)f=(int)((t+2.5f)*12)%8;
            else if(t<.60f)f=Pose(t-.35f,8,new[]{.09f,.07f,.05f,.04f});
            else if(t<2.15f)
            {
                float u=(t-.60f)/1.55f;feet-=4*43*u*(1-u);
                f=Pose(t-.60f,12,new[]{.32f,.38f,.45f,.40f});
            }
            else if(t<2.65f)f=Pose(t-2.15f,16,new[]{.055f,.13f,.17f,.145f});
            else if(t<3.25f)f=Pose(t-2.65f,20,new[]{.12f,.24f,.14f,.10f});
            else f=(int)((t-3.25f)*12)%8;
            Hero("costa",f,156,feet);Fx(t-2.15f,0,150,145);
        }
        void Sierra(float t)
        {
            // Only path travel stops; wind, birds and light continue on the shot clock.
            float travel=t<.42f?t-.5f*t*t/.42f:.21f;
            World("sierra",t,travel);
            int f=t<.42f?Pose(t,0,new[]{.08f,.09f,.11f,.14f}):
                t<1.04f?Pose(t-.42f,4,new[]{.12f,.16f,.20f,.14f}):
                t<1.94f?Pose(t-1.04f,8,new[]{.15f,.14f,.13f,.12f,.11f,.11f,.14f}):
                t<2.08f?15:t<2.43f?Pose(t-2.08f,16,new[]{.10f,.08f,.065f,.105f}):
                Pose(t-2.43f,20,new[]{.16f,.23f,.33f,.85f});
            Hero("sierra",f);
            float stone=t-2.26f;
            if(stone>=0&&stone<.50f)
            {
                float x=185+stone*210,y=94-stone*48+stone*stone*12;
                art.Draw("selva_fx",new Rect(x,y,6,6),4);
            }
            // No phantom target explosion: the stone leaves the frame, condor remains unhurt.
        }
        const float Release=1.55f,Length=65,Amplitude=.65f,ReleaseAngle=.32f,Gravity=130;
        static float Omega=>Mathf.Acos(-ReleaseAngle/Amplitude)/Release;
        static float Angle(float t)=>-Amplitude*Mathf.Cos(Omega*t);
        static float AngularVelocity(float t)=>Amplitude*Omega*Mathf.Sin(Omega*t);
        static Vector2 Pivot=>new Vector2(118,4);
        public static Vector2 SwingHand(float u)
        {float a=Angle(Mathf.Clamp01(u)*Release);return Pivot+new Vector2(Mathf.Sin(a)*Length,Mathf.Cos(a)*Length);}
        Vector2 ReleaseFeet()
        {
            var grip=art.Grip("selva_hero",7);
            return SwingHand(1)+new Vector2((.5f-grip.x)*ActorSize,(art.Foot("selva_hero",7)-grip.y)*ActorSize);
        }
        public static Vector2 ReleaseVelocity()
        {
            float a=Angle(Release),v=AngularVelocity(Release);
            return new Vector2(Length*Mathf.Cos(a)*v,-Length*Mathf.Sin(a)*v);
        }
        float LandingTime()
        {
            var start=ReleaseFeet();float v=ReleaseVelocity().y;
            return Release+(-v+Mathf.Sqrt(v*v+2*Gravity*Mathf.Max(0,146-start.y)))/Gravity;
        }
        public static Vector2 Ballistic(Vector2 start,Vector2 velocity,float dt)
        {return start+velocity*dt+new Vector2(0,.5f*Gravity*dt*dt);}
        void Rope(Vector2 hand,float bend)
        {
            Vector2 last=Pivot;
            for(int i=1;i<=24;i++)
            {
                float q=i/24f;var next=Vector2.Lerp(Pivot,hand,q)+new Vector2(Mathf.Sin(q*Mathf.PI)*bend,0);
                var delta=next-last;var m=GUI.matrix;
                GUI.matrix=m*Matrix4x4.TRS(new Vector3(last.x,last.y,0),Quaternion.Euler(0,0,Mathf.Atan2(delta.y,delta.x)*Mathf.Rad2Deg),Vector3.one);
                RunnerPresentation.Box(new Rect(0,-.7f,delta.magnitude+1,1.4f),new Color32(74,53,36,255));
                RunnerPresentation.Box(new Rect(0,-.25f,delta.magnitude+1,.5f),new Color32(212,167,44,255));
                GUI.matrix=m;last=next;
            }
        }
        void Selva(float t)
        {
            float landing=LandingTime();World("selva",t,Mathf.Min(t,landing)*.20f);
            if(t>2.6f)Portal(t,false);
            float dt=Mathf.Max(0,t-Release),a=Angle(Release);
            float ropeAngle=t<Release?Angle(t):Mathf.Exp(-dt*.22f)*(a*Mathf.Cos(Omega*dt)+AngularVelocity(Release)/Omega*Mathf.Sin(Omega*dt));
            Vector2 hand=t<Release?SwingHand(t/Release):Pivot+new Vector2(Mathf.Sin(ropeAngle)*Length,Mathf.Cos(ropeAngle)*Length);
            Rope(hand,t<Release?-Mathf.Sin(Angle(t))*1.5f:Mathf.Sin(dt*3)*.7f);
            float endX=Ballistic(ReleaseFeet(),ReleaseVelocity(),landing-Release).x;
            if(t<Release)
            {
                int f=Mathf.Min(7,(int)(Ease(t/Release)*8));var grip=art.Grip("selva_hero",f);
                art.Draw("selva_hero",new Rect(hand.x-grip.x*ActorSize,hand.y-grip.y*ActorSize,ActorSize,ActorSize),f);
            }
            else if(t<landing)
            {
                var p=Ballistic(ReleaseFeet(),ReleaseVelocity(),t-Release);
                int f=8+Mathf.Min(3,(int)((t-Release)/(landing-Release)*4));Hero("selva",f,p.x,p.y);
            }
            else if(t<landing+.48f)Hero("selva",Pose(t-landing,12,new[]{.065f,.13f,.15f,.135f}),endX);
            else if(t<3.2f)Hero("selva",Pose(t-landing-.48f,16,new[]{.10f,.10f,.13f,.30f}),endX);
            else if(t<3.5f)Hero("selva",Pose(t-3.2f,20,new[]{.055f,.08f,.075f,.09f}),endX);
            else
            {
                float u=Mathf.Clamp01((t-3.5f)/.75f),x=Mathf.Lerp(endX,270,u);
                // Arrive at the doorway center FIRST; only then pass through its light.
                float enter=Ease((t-4.25f)/.23f);var saved=GUI.color;
                GUI.color=new Color(1,1,1,1-enter);
                Hero("epilogue",(int)((t-3.5f)*12)%8,x,146,ActorSize);GUI.color=saved;
            }
            Fx(t-landing,0,endX-5,145);
            Fx(t-3.2f,8,endX+30,108,18);
            if(t>=3.2f&&t<3.47f)art.Draw("selva_fx",new Rect(endX+33+(t-3.2f)*260,106,7,7),8);
            if(t>2.6f)Portal(t,true);
        }
        void Portal(float t,bool front)
        {
            string name="epilogue_portal_"+(front?"front":"back");int frame=(int)(t*6)%4;
            var saved=GUI.color;GUI.color=new Color(1,1,1,Ease((t-2.6f)/.4f));
            const float size=90;
            art.Draw(name,new Rect(270-size/2,146-art.Foot(name,frame)*size,size,size),frame);
            GUI.color=saved;
        }
        void EpilogueWorld(float t)
        {
            // Full-width reunion vista; each depth plane is its own authored PNG.
            art.Draw("epilogue_sky",new Rect(0,0,320,180));
            float drift=Ease(t/5)*2;
            art.Draw("epilogue_mid",new Rect(-drift,0,324,180));
            float y=146-art.Surface("epilogue_ground")*180;
            art.Draw("epilogue_ground",new Rect(0,y,320,180));
            for(int i=0;i<8;i++)
            {
                float x=38+i*36+Mathf.Sin(t*.8f+i)*3,ySpark=45+i%3*22+Mathf.Sin(t+i)*2;
                art.Draw("selva_fx",new Rect(x,ySpark,3,3),4+(int)(t*5+i)%4);
            }
        }
        void Memories(float t)
        {
            EpilogueWorld(t);
            RunnerPresentation.Box(new Rect(0,12,320,156),new Color(0,0,0,.35f));
            int frame=Pose(t,0,new[]{.17f,.12f,.16f,.16f,.12f,.14f,.18f,.23f,.19f,.08f,.07f,.12f,.20f,.20f,.18f,.18f});
            art.Draw("selva_portrait",new Rect(178,35,119,119),frame);
            // Regional constellations: preserve all earned pieces without an inventory-strip layout.
            int[] rows={0,0,0};
            for(int i=0;i<collectedCount;i++)
            {
                int item=collected[i],r=item/4,col=rows[r]++;
                float alpha=Ease((t-.16f-r*.28f-col*.06f)/.35f);
                var saved=GUI.color;GUI.color=new Color(1,1,1,alpha);
                art.Draw("selva_memories",new Rect(22+col*34,35+r*35+(1-alpha)*5,29,29),item);GUI.color=saved;
            }
            if(collectedCount==0)Text("UN VIAJE · TRES TIERRAS",130,4,RunnerPresentation.Gold);
        }
        void Farewell(float t)
        {
            EpilogueWorld(t+2.5f);
            float reveal=Ease((t-.8f)/.7f);
            var saved=GUI.color;GUI.color=new Color(1,1,1,reveal);
            int victory=Pose(Mathf.Max(0,t-1),8,new[]{.20f,.20f,.30f,1.5f});
            Hero("epilogue",victory,160,146,65);GUI.color=saved;
            int f=Pose(t,16,new[]{.17f,.18f,.20f,.25f,.22f,.20f,.25f,1.53f});
            GUI.color=new Color(1,1,1,1-reveal);
            art.Draw("selva_portrait",new Rect(70,20,180,180),f);GUI.color=saved;
            if(t>1.35f)
            {
                GUI.color=new Color(1,1,1,Ease((t-1.35f)/.5f));
                Text("SUYURUN",29,9,RunnerPresentation.Gold);
                Text("VIAJE COMPLETADO",48,4,Color.white);GUI.color=saved;
            }
        }
        void Text(string value,float y,int size,Color color)
        {
            var m=GUI.matrix;var p=m.MultiplyPoint3x4(new Vector3(12,y,0));var q=m.MultiplyPoint3x4(new Vector3(308,y+18,0));
            float scale=m.MultiplyVector(Vector3.right).magnitude;
            var rect=new Rect(Mathf.Round(p.x),Mathf.Round(p.y),Mathf.Round(q.x-p.x),Mathf.Round(q.y-p.y));
            var style=RunnerHud.Centered(new GUIStyle(GUI.skin.label){fontSize=Mathf.Max(8,Mathf.RoundToInt(size*scale))});
            try
            {
                GUI.matrix=Matrix4x4.identity;
                style.normal.textColor=Color.black;GUI.Label(new Rect(rect.x+1,rect.y+1,rect.width,rect.height),value,style);
                style.normal.textColor=color;GUI.Label(rect,value,style);
            }
            finally{GUI.matrix=m;}
        }
        void Credits(float t)
        {
            string[] lines={"SUYURUN","CREADO POR","ROMMEL C. V.","WILLIAM R. Q.","JOE F. C.","PROFESOR","MG. SULLA TORRES","DESARROLLO DE SOFTWARE","PARA JUEGOS","UNSA"};
            float y=160-t*40;
            for(int i=0;i<lines.Length;i++)
            {
                bool heading=i==0||i==1||i==5;
                Text(lines[i],y+i*18,i==0?8:heading?4:5,heading?RunnerPresentation.Gold:Color.white);
            }
            RunnerPresentation.Box(new Rect(0,0,320,12),Color.black);
            RunnerPresentation.Box(new Rect(0,168,320,12),Color.black);
            if(t>=8)
            {
                var saved=GUI.color;GUI.color=new Color(1,1,1,Ease((t-8)/.5f));
                Text("GRACIAS POR JUGAR",77,6,RunnerPresentation.Gold);
                Text("COSTA · SIERRA · SELVA",99,3,Color.white);GUI.color=saved;
            }
        }
        public void RenderAt(float age)
        {
            if(age<score.cuts[0])Opening(age*2.5f/score.cuts[0]);
            else if(age<score.cuts[1])Coast((age-score.cuts[0])*3.5f/CoastDuration);
            else if(age<score.cuts[2])Sierra((age-score.cuts[1])*4/SierraDuration);
            else if(age<score.cuts[3])Selva((age-score.cuts[2])*4.5f/SelvaDuration);
            else if(age<17)Memories((age-score.cuts[3])*2.5f/(17-score.cuts[3]));
            else if(age<20)Farewell(age-17);
            else Credits(age-20);
        }
        static Matrix4x4 Lens(float zoom,Vector2 target)
        {return Matrix4x4.Translate(new Vector3(160,90,0))*Matrix4x4.Scale(new Vector3(zoom,zoom,1))*Matrix4x4.Translate(new Vector3(-target.x,-target.y,0));}
        Matrix4x4 CameraAt(float age)
        {
            if(age<score.cuts[0])
            {
                float u=Ease(age/score.cuts[0]);
                return Lens(Mathf.Lerp(1.08f,1,u),Vector2.Lerp(new Vector2(155,90),new Vector2(160,90),u));
            }
            if(age<score.cuts[1])return Lens(1, new Vector2(160,90));
            if(age<score.cuts[2])
            {
                float u=(age-score.cuts[1])/SierraDuration;
                float focus=Ease(u/.45f)*(1-Ease((u-.60f)/.36f));
                return Lens(1+.12f*focus,new Vector2(160+5*focus,90));
            }
            if(age<score.cuts[3])
            {
                float u=(age-score.cuts[2])/SelvaDuration;
                return Lens(1,new Vector2(160+Ease(u)*4,90));
            }
            return Lens(1,new Vector2(160,90));
        }
        void OnGUI()
        {
            if(!ready)return;
            var matrix=GUI.matrix;var color=GUI.color;int depth=GUI.depth;
            try
            {
                GUI.depth=-1000;GUI.color=Color.white;GUI.matrix=Matrix4x4.identity;
                RunnerPresentation.Box(new Rect(0,0,Screen.width,Screen.height),Color.black);
                var baseMatrix=RunnerHud.ScreenMatrix;float age=preview?Mathf.Min(Age,29.2f):Age;
                GUI.matrix=baseMatrix*CameraAt(age);RenderAt(age);GUI.matrix=baseMatrix;
                if(age>score.cuts[2]-.26f&&age<score.cuts[2]+.26f)
                {
                    float u=(age-score.cuts[2]+.26f)/.52f;
                    art.Draw("selva_life",new Rect(340-u*740,-20,400,220),4);
                }
                // Action match cuts retain motion; only the emotional chapter and final close fade.
                float fade=0;
                if(age<.45f)RunnerPresentation.Box(new Rect(0,0,320,180),new Color(1,.88f,.55f,1-Ease(age/.45f)));
                if(age>=score.cuts[3]-.15f&&age<score.cuts[3]+.15f)
                {
                    float glow=1-Mathf.Abs(age-score.cuts[3])/.15f;
                    RunnerPresentation.Box(new Rect(0,0,320,180),new Color(1,.88f,.55f,glow));
                }
                if(age>19.35f&&age<20)fade=Ease((age-19.35f)/.65f);
                if(age>=20&&age<20.25f)fade=1-Ease((age-20)/.25f);
                if(age>29.5f)fade=Ease((age-29.5f)/.5f);
                if(fade>0)RunnerPresentation.Box(new Rect(0,0,320,180),new Color(0,0,0,Mathf.Clamp01(fade)));
                GUI.matrix=Matrix4x4.identity;var v=RunnerHud.Viewport(Screen.width,Screen.height);
                RunnerPresentation.Box(new Rect(0,0,v.x,Screen.height),Color.black);
                RunnerPresentation.Box(new Rect(v.xMax,0,Screen.width-v.xMax,Screen.height),Color.black);
                RunnerPresentation.Box(new Rect(0,0,Screen.width,v.y),Color.black);
                RunnerPresentation.Box(new Rect(0,v.yMax,Screen.width,Screen.height-v.yMax),Color.black);
                if(preview)
                {
                    float w=Mathf.Min(520,Screen.width-12),x=(Screen.width-w)/2,y=Screen.height-34;
                    GUI.Box(new Rect(x-4,y-4,w+8,36),GUIContent.none);
                    if(GUI.Button(new Rect(x,y,w*.30f,28),"Repetir"))Replay();
                    if(GUI.Button(new Rect(x+w*.31f,y,w*.30f,28),paused?"Continuar":"Pausar"))TogglePause();
                    if(GUI.Button(new Rect(x+w*.62f,y,w*.38f,28),"Cerrar vista previa"))Destroy(gameObject);
                }
            }
            finally{GUI.matrix=matrix;GUI.color=color;GUI.depth=depth;}
        }
    }
    [System.Serializable]
    public sealed class FinalMovieScore
    {
        public float[] cuts={2.5f,6,10,14.5f};
        public string method="Authored fallback; not ear-validated musical cues";
        public void Validate()
        {
            if(cuts==null||cuts.Length!=4||cuts[0]<2||cuts[0]>3||cuts[1]-cuts[0]<3||cuts[2]-cuts[1]<3||cuts[3]-cuts[2]<3||cuts[3]>15)
                cuts=new[]{2.5f,6,10,14.5f};
        }
    }
}
