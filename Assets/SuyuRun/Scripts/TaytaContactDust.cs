using System.Collections.Generic;
using UnityEngine;
namespace SuyuRun
{
    // Contact-only presentation. No collisions, input or damage changes.
    public sealed class TaytaContactDust:MonoBehaviour
    {
        sealed class Particle {public SpriteRenderer sprite;public Vector3 velocity;public float age,life;}
        readonly List<Particle> particles=new List<Particle>();
        static Sprite pixel;SpriteRenderer actor;float previousTravel,previousTime,scroll;Vector3 previousFoot;
        int contact=-1;bool previousAir,initialized;string region;
        public static void Track(SpriteRenderer actor,string region,string state,float travel)
        {
            if(!actor||!actor.sprite||!actor.sprite.name.StartsWith("UniformTayta/"))return;
            var fx=actor.GetComponent<TaytaContactDust>();if(!fx)fx=actor.gameObject.AddComponent<TaytaContactDust>();
            fx.Sample(actor,region,state,travel);
        }
        void Sample(SpriteRenderer renderer,string area,string state,float travel)
        {
            actor=renderer;region=area;float now=Time.time;
            bool air=state.Contains("jump")||state.Contains("fall")||state=="swing"||state=="grab"||state=="release"||state=="regrab"||state=="climb";
            var sprite=actor.sprite;float x=(sprite.rect.width*.5f-sprite.pivot.x)/sprite.pixelsPerUnit;
            if(actor.flipX)x=-x;
            var foot=actor.transform.TransformPoint(new Vector3(x,(sprite.rect.height-308-sprite.pivot.y)/sprite.pixelsPerUnit,0));
            float dt=now-previousTime;
            if(initialized&&dt>0&&dt<.25f)scroll=(travel-previousTravel)/dt/32-(foot.x-previousFoot.x)/dt;
            int step=Mathf.FloorToInt(travel/(GameScale.NativeStride*.5f));
            if(initialized&&!air&&previousAir)Spawn(foot,12,true);
            else if(initialized&&!air&&(state=="run"||state=="burst")&&step!=contact&&Mathf.Abs(travel-previousTravel)<12)Spawn(foot,5,false);
            previousTravel=travel;previousTime=now;previousFoot=foot;previousAir=air;contact=step;initialized=true;
        }
        void Spawn(Vector3 foot,int count,bool landing)
        {
            if(!pixel)
            {
                var texture=new Texture2D(1,1,TextureFormat.RGBA32,false);texture.SetPixel(0,0,Color.white);texture.Apply();texture.filterMode=FilterMode.Point;
                pixel=Sprite.Create(texture,new Rect(0,0,1,1),Vector2.one*.5f,32);
            }
            for(int i=0;i<count;i++)
            {
                Particle p=particles.Find(v=>v.age>=v.life);
                if(p==null){var go=new GameObject("Tayta contact dust");go.transform.SetParent(transform.parent,false);p=new Particle{sprite=go.AddComponent<SpriteRenderer>()};particles.Add(p);}
                p.age=0;p.life=landing?.55f:.38f;p.sprite.enabled=true;p.sprite.sprite=pixel;p.sprite.sharedMaterial=actor.sharedMaterial;p.sprite.sortingOrder=actor.sortingOrder-1;
                p.sprite.transform.position=foot+new Vector3((i%3-1)*.025f,.015f,0);p.sprite.transform.localScale=Vector3.one*(i%3==0?1.1f:.65f);
                p.velocity=new Vector3((i*7%13-6)*(landing?.035f:.02f),.12f+(i*11%7)*.035f,0);
            }
        }
        void LateUpdate()
        {
            foreach(var p in particles)
            {
                if(p.age>=p.life){p.sprite.enabled=false;continue;}
                p.age+=Time.deltaTime;p.velocity.y-=Time.deltaTime*.55f;
                p.sprite.transform.position+=(p.velocity+Vector3.left*scroll)*Time.deltaTime;
                Color color=region=="selva"?new Color32(143,107,66,255):region=="sierra"?new Color32(138,138,148,255):new Color32(212,155,100,255);
                color.a=.65f*Mathf.Clamp01(1-p.age/p.life);p.sprite.color=color;
            }
        }
        void OnDestroy(){foreach(var p in particles)if(p.sprite)Destroy(p.sprite.gameObject);}
    }
}
