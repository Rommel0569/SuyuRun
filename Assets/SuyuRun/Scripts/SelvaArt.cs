using System;
using System.Collections.Generic;
using UnityEngine;
namespace SuyuRun
{
    // JSON coordinates are top-left native pixels; Unity UV coordinates are bottom-left.
    public static class SelvaArt
    {
        [Serializable] public class Cell { public int x,y,width,height; }
        [Serializable] public class Hitbox { public float[] body,mouth; public bool damaging,snap; }
        [Serializable] public class Spec { public int frameWidth,frameHeight,frames,fps; public bool loop; public Cell[] rectsUnity; public Hitbox[] hitboxes; }
        public sealed class Sheet {public Texture2D texture;public Spec spec;public Sprite[] frames;}
        static readonly Dictionary<string,Sheet> cache=new Dictionary<string,Sheet>();
        public static Sheet Get(string name)
        {
            if(cache.TryGetValue(name,out var result))return result;
            var t=Resources.Load<Texture2D>("SelvaV2/selva_"+name);
            var data=Resources.Load<TextAsset>("SelvaV2/selva_"+name);
            if(!t||!data)throw new InvalidOperationException("Missing Selva sheet: "+name);
            var s=JsonUtility.FromJson<Spec>(data.text);var sprites=Resources.LoadAll<Sprite>("SelvaV2/selva_"+name);
            Array.Sort(sprites,(a,b)=>String.CompareOrdinal(a.name,b.name));
            if(sprites.Length!=s.frames)
            {sprites=new Sprite[s.frames];for(int i=0;i<sprites.Length;i++){var r=s.rectsUnity[i];sprites[i]=Sprite.Create(t,new Rect(r.x,r.y,r.width,r.height),new Vector2(0,1),32,0,SpriteMeshType.FullRect);sprites[i].name=name+"_"+i;}}
            result=new Sheet{texture=t,spec=s,frames=sprites};cache.Add(name,result);return result;
        }
        public static Sprite Frame(string name,float seconds,int forced=-1)
        {var s=Get(name);int f=forced>=0?Mathf.Clamp(forced,0,s.frames.Length-1):(int)(Mathf.Max(0,seconds)*s.spec.fps);return s.frames[s.spec.loop?f%s.frames.Length:Mathf.Min(f,s.frames.Length-1)];}
        public static Rect NativeBox(float[] box,Vector2 origin,bool left,int width)
        {return new Rect(origin.x+(left?width-box[0]-box[2]:box[0]),origin.y+box[1],box[2],box[3]);}
    }
}
