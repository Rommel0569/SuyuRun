using System;
using System.Collections.Generic;
using UnityEngine;
namespace SuyuRun
{
    // Post-game presentation only; gameplay, save data and future secret level stay unchanged.
    public sealed class RunnerFinalMovie:MonoBehaviour
    {
        public static bool Active {get;private set;}
        double began;AudioSource track;
        [Serializable] sealed class AtlasSpec {public int frames,frameWidth,frameHeight,fps;public bool loop;}
        readonly Dictionary<string,Sprite[]> actors=new Dictionary<string,Sprite[]>();
        readonly Dictionary<string,AtlasSpec> actorSpecs=new Dictionary<string,AtlasSpec>();
        readonly Dictionary<string,Texture2D> layers=new Dictionary<string,Texture2D>();
        public static void Begin(GameObject host)
        {
            if(Active)return;
            host.AddComponent<RunnerFinalMovie>();
        }
        void Awake(){Active=true;began=AudioSettings.dspTime;track=gameObject.AddComponent<AudioSource>();track.clip=Resources.Load<AudioClip>("FinalMovie/final_music");track.loop=true;track.volume=.34f*PlayerPrefs.GetFloat("SuyuRun.Settings.MusicVol",1);if(track.clip)track.Play();}
        void OnDestroy(){Active=false;if(track)Destroy(track);foreach(var sheet in actors.Values)foreach(var sprite in sheet)if(sprite)Destroy(sprite);}
        void Update(){float age=(float)(AudioSettings.dspTime-began);if(age>28&&track)track.volume=.34f*PlayerPrefs.GetFloat("SuyuRun.Settings.MusicVol",1)*Mathf.Clamp01((30-age)/2);if(age>=30)Destroy(this);}
        void Layer(string resource,float t,float speed)
        {
            if(!layers.TryGetValue(resource,out var texture))
            {texture=RefactorMaterialArt.FullTexture(Resources.Load<Texture2D>(resource));layers[resource]=texture;}
            if(!texture)return;
            texture.filterMode=FilterMode.Point;
            float x=-Mathf.Repeat(t*speed,640);
            GUI.DrawTexture(new Rect(x,0,640,180),texture,ScaleMode.StretchToFill,true);
            GUI.DrawTexture(new Rect(x+640,0,640,180),texture,ScaleMode.StretchToFill,true);
        }
        void Actor(string path,Rect rect,float time,bool left=false)
        {
            if(!actors.TryGetValue(path,out var frames))
            {
                string resource=path.StartsWith("SelvaV2/")?path:"RefactorBank/"+path;
                var texture=Resources.Load<Texture2D>(resource);
                var meta=Resources.Load<TextAsset>(resource);
                if(!texture||!meta)return;
                var spec=JsonUtility.FromJson<AtlasSpec>(meta.text);
                if(spec.frames<1||spec.frameWidth*spec.frames!=texture.width||spec.frameHeight!=texture.height)return;
                texture.filterMode=FilterMode.Point;frames=new Sprite[spec.frames];
                for(int i=0;i<frames.Length;i++)frames[i]=Sprite.Create(texture,new Rect(i*spec.frameWidth,0,spec.frameWidth,spec.frameHeight),new Vector2(.5f,0),64);
                actors[path]=frames;actorSpecs[path]=spec;
            }
            var s=actorSpecs[path];int index=Mathf.FloorToInt(Mathf.Max(0,time)*Mathf.Max(1,s.fps));
            var previous=GUI.matrix;
            if(left)GUI.matrix=previous*Matrix4x4.Translate(new Vector3(rect.center.x*2,0,0))*Matrix4x4.Scale(new Vector3(-1,1,1));
            RunnerHud.Sprite(rect,frames[s.loop?index%frames.Length:Mathf.Min(index,frames.Length-1)]);GUI.matrix=previous;
        }
        void Choreography(string region,float local)
        {
            const float ground=146;
            // Same anatomy and foot anchor in every action. A continuous
            // ballistic arc chooses ascent/apex/descent, then an explicit land.
            float jumpAge=local-1.45f,flight=1.55f;
            float lift=jumpAge>=0&&jumpAge<flight?4*37*(jumpAge/flight)*(1-jumpAge/flight):0;
            string state;float poseAge;
            if(local<1.45f){state="run";poseAge=local;}
            else if(local<2.3f){state="jump";poseAge=jumpAge;}
            else if(local<3){state="fall";poseAge=local-2.3f;}
            else if(local<3.18f){state="land";poseAge=local-3;}
            else if(local<3.55f){state="run";poseAge=local-3.18f;}
            else if(local<4.4f){state="slide";poseAge=local-3.55f;}
            else if(local<4.75f){state="run";poseAge=local-4.4f;}
            else if(local<5.75f){state="burst";poseAge=local-4.75f;}
            else {state="run";poseAge=local-5.75f;}
            // Birds and dust use the same production art, not stand-in shapes.
            string bird=region=="selva"?"SelvaV2/selva_guacamayo_movimiento":"Actors/"+(region=="costa"?"coastbird":"condor")+"/flight";
            Actor(bird,new Rect(290-local*15,43+Mathf.Sin(local*2)*4,48,48),local,true);
            if(local>=1.1f&&local<3.4f)
            {
                float x=220-(local-1.1f)*57;
                string foe=region=="costa"?"hexagon":region=="sierra"?"pebble":"sprout";
                Actor("Actors/"+foe+"/walk",new Rect(x,ground-30,32,32),local,foe!="pebble");
            }
            if(local>=3&&local<4.6f)
            {
                float x=238-(local-3)*84;
                Actor(bird,new Rect(x,ground-55,48,48),local,true);
            }
            if(local>=4.5f&&local<5.55f)
                Actor(bird,new Rect(246-(local-4.5f)*24,ground-60,48,48),local,true);
            if(local>=4.8f&&local<5.55f)
            {
                float bulletAge=Mathf.Repeat(local-4.8f,.18f);
                RunnerPresentation.Box(new Rect(155+bulletAge*420,ground-27,4,1),RunnerPresentation.Gold);
            }
            if(local>=5.5f&&local<5.95f)
                Actor("Effects/impacto_arma",new Rect(204,ground-51,40,40),local-5.5f);
            var sprite=TaytaArt.Frame(region,state,poseAge);
            if(!sprite)sprite=TaytaArt.Frame(region,"run",poseAge);
            if(sprite)
            {
                // Convert the actual registered sprite bounds, not a fixed
                // stretched cell: 128px and 192px sheets retain equal anatomy.
                const float framing=1.2f;
                var b=sprite.bounds;
                RunnerHud.Sprite(new Rect(120+b.min.x*32*framing,
                    ground-47*framing-b.max.y*32*framing-lift,
                    b.size.x*32*framing,b.size.y*32*framing),sprite);
            }
        }
        void OnGUI()
        {
            GUI.depth=-1000;var previous=GUI.matrix;GUI.matrix=RunnerHud.ScreenMatrix;
            float age=(float)(AudioSettings.dspTime-began);
            RunnerPresentation.Box(new Rect(0,0,320,180),Color.black);
            if(age<20)
            {
                int cut=Mathf.Min(2,(int)(age/(20f/3)));string region=new[]{"costa","sierra","selva"}[cut];float local=age-cut*(20f/3);
                // Small tracking push-in; all layers and actors share the camera.
                var shotMatrix=GUI.matrix;float zoom=1+Mathf.SmoothStep(0,.065f,local/(20f/3));
                GUI.matrix=shotMatrix*Matrix4x4.Translate(new Vector3(160,146,0))*Matrix4x4.Scale(new Vector3(zoom,zoom,1))*Matrix4x4.Translate(new Vector3(-160,-146,0));
                if(cut==0){Layer("costa_capa1_cielo",local,0);Layer("costa_capa3_paracas",local,8);Layer("costa_capa5_huacas",local,25);Layer("costa_capa7_suelo",local,60);}
                else if(cut==1){Layer("SierraV3/sierra_compartido_capa1_cielo",local,0);Layer("SierraV3/sierra_compartido_capa3_nevados",local,8);Layer("SierraV3/sierra_camino_capa4_paisaje",local,20);Layer("SierraV3/sierra_camino_capa6_cercano",local,25);Layer("SierraV3/sierra_camino_capa7_suelo",local,60);}
                else {Layer("SelvaV2/selva_bosque_capa1_cielo",local,0);Layer("SelvaV2/selva_bosque_capa2_lejania",local,8);Layer("SelvaV2/selva_bosque_capa3_estructura",local,20);Layer("SelvaV2/selva_bosque_capa5_cercania",local,40);}
                Choreography(region,local);GUI.matrix=shotMatrix;
                RunnerPresentation.Box(new Rect(0,0,320,16),Color.black);RunnerPresentation.Box(new Rect(0,164,320,16),Color.black);
                RunnerHud.Text(new Rect(10,166,300,10),"SUYURUN · "+region.ToUpperInvariant(),new GUIStyle(GUI.skin.label){fontSize=5,normal={textColor=RunnerPresentation.Gold}});
                float fade=Mathf.Max(1-Mathf.Clamp01(local/.35f),Mathf.Clamp01((local-20f/3+.35f)/.35f));
                RunnerPresentation.Box(new Rect(0,0,320,180),new Color(0,0,0,fade));
            }
            else
            {
                string[] lines={"SUYURUN","CREADO POR","ROMMEL C. V.","WILLIAM R. Q.","JOE F. C.","PROFESOR","MG. SULLA TORRES","DESARROLLO DE SOFTWARE PARA JUEGOS","UNSA","COSTA · SIERRA · SELVA","GRACIAS POR JUGAR"};
                float y=Mathf.Lerp(182,-260,Mathf.Clamp01((age-20)/10));
                for(int i=0;i<lines.Length;i++)RunnerHud.Text(new Rect(10,y+i*23,300,16),lines[i],new GUIStyle(GUI.skin.label){fontSize=i==0?8:5,normal={textColor=i==0?RunnerPresentation.Gold:Color.white}});
            }
            GUI.matrix=previous;
        }
    }
}
