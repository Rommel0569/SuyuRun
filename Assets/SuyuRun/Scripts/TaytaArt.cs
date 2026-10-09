using System;
using System.Collections.Generic;
using UnityEngine;
namespace SuyuRun
{
    // Presentation only: never changes regional physics, inputs or damage hitboxes.
    public static class TaytaArt
    {
        // Applied through PPU/pivot, not per-state Transform scale: firing,
        // crouching and rewards retain the same anatomy and planted feet.
        public const float WorldScale=GameScale.HeroScale;
        [Serializable] public class Spec {public int frames,fps,frameWidth,frameHeight;public float pixelsPerUnit,gripCoordinateScale;public bool loop;public float[] gripX,gripY,footAnchor;public int[] frameOrder;}
        public sealed class Sheet {public Sprite[] frames;public Spec spec;}
        static readonly Dictionary<string,Sheet> cache=new Dictionary<string,Sheet>();
        public static Sheet Get(string region,string state)
        {
            if(state=="rafaga")state="burst";
            if(state=="trepar")state="climb";
            string key=region+"/"+state;
            if(cache.TryGetValue(key,out var found)&&found.frames.Length>0&&found.frames[0])return found;
            string revised=key+(state=="run"?"_v3":"_v2");
            var texture=Resources.Load<Texture2D>("Tayta/"+revised)??Resources.Load<Texture2D>("Tayta/"+key);
            var data=Resources.Load<TextAsset>("Tayta/"+revised)??Resources.Load<TextAsset>("Tayta/"+key);
            string highState=state=="huaraca_cargar"?"huaraca_load":state=="huaraca_girar"?"huaraca_spin":state=="huaraca_lanzar"?"huaraca_throw":state=="victory"&&region=="sierra"?"transformation":state;
            string highPath="RefactorBank/Tayta"+char.ToUpperInvariant(region[0])+region.Substring(1)+"/"+highState;
            if(region!="selva"&&state=="double_jump")highPath="RefactorBank/Tayta"+char.ToUpperInvariant(region[0])+region.Substring(1)+"/jump";
            // Sierra has no high-resolution fall sheet. Reuse the terminal
            // airborne pose with the SAME PPU/pivot instead of shrinking to 48px.
            if(region=="sierra"&&state=="fall")highPath="RefactorBank/TaytaSierra/jump";
            var highTexture=Resources.Load<Texture2D>(highPath);var highData=Resources.Load<TextAsset>(highPath);
            if(region=="sierra"&&(highState=="huaraca_throw"||highState=="huaraca_spin"||highState=="huaraca_load"))
            {
                var revisedThrow=Resources.Load<Texture2D>(highPath+"_v3");var revisedData=Resources.Load<TextAsset>(highPath+"_v3");
                if(revisedThrow&&revisedData){highTexture=revisedThrow;highData=revisedData;}
            }
            if((region=="costa"||region=="sierra")&&(state=="jump"||state=="double_jump"||state=="fall"))
            {
                string airPath="RefactorBank/Tayta"+char.ToUpperInvariant(region[0])+region.Substring(1)+"/"+state+"_selva_pose";
                var airTexture=Resources.Load<Texture2D>(airPath);var airData=Resources.Load<TextAsset>(airPath);
                if(airTexture&&airData){highTexture=airTexture;highData=airData;}
            }
            if(state=="run"||state=="burst"||state=="slide")
            {
                var gaitTexture=Resources.Load<Texture2D>(highPath+"_human");var gaitData=Resources.Load<TextAsset>(highPath+"_human");
                if(gaitTexture&&gaitData){highTexture=gaitTexture;highData=gaitData;}
            }
            bool registered=highTexture&&highData&&!highData.text.Contains("\"quarantined\": true");
            if(registered){texture=highTexture;data=highData;}
            if(!texture||!data)return state.EndsWith("_shoot")?Get(region,state.Substring(0,state.Length-6)):null;
            texture.filterMode=FilterMode.Point;texture.wrapMode=TextureWrapMode.Clamp;
            var spec=JsonUtility.FromJson<Spec>(data.text);var frames=new Sprite[spec.frames];
            int width=spec.frameWidth>0?spec.frameWidth:48,height=spec.frameHeight>0?spec.frameHeight:48;
            float ppu=spec.pixelsPerUnit>0?spec.pixelsPerUnit:32;
            if(registered)ppu/=WorldScale;
            Vector2 pivot=new Vector2(0,1);
            if(registered){float foot=spec.footAnchor!=null&&spec.footAnchor.Length>1?spec.footAnchor[1]:height==96?93:124;float ratio=ppu/32;pivot=new Vector2((width*.5f-24*ratio)/width,(height-foot+47*ratio)/height);}
            for(int i=0;i<frames.Length;i++)
            {int source=spec.frameOrder!=null&&i<spec.frameOrder.Length?spec.frameOrder[i]:i;frames[i]=Sprite.Create(texture,new Rect(source*width,0,width,height),pivot,ppu,0,SpriteMeshType.FullRect);frames[i].name=state+"_"+i;if(registered)RefactorActorArt.RegisterHero(frames[i]);}
            found=new Sheet{spec=spec,frames=frames};cache[key]=found;return found;
        }
        public static Sprite Frame(string region,string state,float age,int forced=-1)
        {
            var s=Get(region,state);if(s==null)return null;
            int i=forced>=0?Mathf.Clamp(forced,0,s.frames.Length-1):Mathf.FloorToInt(Mathf.Max(0,age)*s.spec.fps);
            if(region=="sierra"&&state=="fall"&&s.spec.frames==4)return s.frames[s.frames.Length-1];
            // Keep every regional pose. Mapping a four-frame fall onto Selva's
            // single fall pose froze the animation at frame zero.
            if(forced<0&&(state=="jump"||state=="double_jump"||state=="fall"))i=Mathf.FloorToInt(Mathf.Max(0,age)*12);
            return s.frames[s.spec.loop?i%s.frames.Length:Mathf.Min(i,s.frames.Length-1)];
        }
        public static Sprite[] Frames(string region,string state,int count)
        {
            var s=Get(region,state);if(s==null)return null;
            var result=new Sprite[count];for(int i=0;i<count;i++)result[i]=s.frames[Mathf.RoundToInt(i*(s.frames.Length-1f)/Mathf.Max(1,count-1))];
            return result;
        }
        public static Vector2 GripOffset(string state,int pose)
        {
            var s=Get("selva",state);int i=s==null?0:Mathf.Clamp(pose,0,s.frames.Length-1);
            if(s==null||s.spec.gripX==null||s.spec.gripY==null)return new Vector2(24,2);
            float scale=s.spec.gripCoordinateScale>0?s.spec.gripCoordinateScale:1;
            var offset=new Vector2(s.spec.gripX[i],s.spec.gripY[i])*scale;
            if(RefactorActorArt.IsRegistered(s.frames[i]))
            {
                float ratio=s.frames[i].pixelsPerUnit/32;
                float foot=s.spec.footAnchor!=null&&s.spec.footAnchor.Length>1?s.spec.footAnchor[1]:s.spec.frameHeight==96?93:124;
                offset=new Vector2(24+(s.spec.gripX[i]-s.spec.frameWidth*.5f)/ratio,47+(s.spec.gripY[i]-foot)/ratio);
            }
            return offset;
        }
        public static int SwingPose(float angleDegrees)
        {
            int count=Get("selva","swing")?.frames.Length??16;
            return Mathf.Clamp(Mathf.RoundToInt((angleDegrees+52)/104*(count-1)),0,count-1);
        }
    }
}
