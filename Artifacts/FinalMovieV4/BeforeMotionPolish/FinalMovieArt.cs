using System;
using System.Collections.Generic;
using UnityEngine;
namespace SuyuRun
{
    // Dedicated cinematic resources. Never falls back to gameplay textures.
    public sealed class FinalMovieArt
    {
        [Serializable] public sealed class Spec {public int frames,frameWidth,frameHeight,fps;public float surfaceY;public float[] footY,gripX,gripY;}
        sealed class Sheet {public Texture2D texture;public Spec spec;}
        readonly Dictionary<string,Sheet> bank=new Dictionary<string,Sheet>();
        public void Load(string name)
        {
            if(bank.ContainsKey(name))return;
            var t=Resources.Load<Texture2D>("FinalMovieV4/"+name);var json=Resources.Load<TextAsset>("FinalMovieV4/"+name);
            if(!t||!json)throw new InvalidOperationException("Missing cinematic-only asset: "+name);
            var s=JsonUtility.FromJson<Spec>(json.text);
            if(s.frames<1||t.width!=s.frames*s.frameWidth||t.height!=s.frameHeight)throw new InvalidOperationException("Invalid cinematic atlas: "+name);
            t.filterMode=FilterMode.Point;t.wrapMode=TextureWrapMode.Clamp;bank[name]=new Sheet{texture=t,spec=s};
        }
        public void Draw(string name,Rect rect,int frame=0,bool flip=false)
        {
            Load(name);var b=bank[name];frame=Mathf.Clamp(frame,0,b.spec.frames-1);
            // Quarter-logical-pixel registration uses the denser source art.
            rect.x=Mathf.Round(rect.x*4)/4;rect.y=Mathf.Round(rect.y*4)/4;
            float w=1f/b.spec.frames;
            GUI.DrawTextureWithTexCoords(rect,b.texture,new Rect((frame+(flip?1:0))*w,0,flip?-w:w,1),true);
        }
        public void Range(string name,Rect rect,float age,int first,int count,float fps=12,bool loop=false)
        {
            int f=Mathf.FloorToInt(Mathf.Max(0,age)*fps);
            Draw(name,rect,first+(loop?f%count:Mathf.Min(f,count-1)));
        }
        public float Foot(string name,int frame){Load(name);var s=bank[name].spec;return s.footY!=null?s.footY[Mathf.Clamp(frame,0,s.frames-1)]:1;}
        public float Surface(string name){Load(name);return bank[name].spec.surfaceY;}
        public Vector2 Grip(string name,int frame){Load(name);var s=bank[name].spec;return new Vector2(s.gripX[frame],s.gripY[frame]);}
    }
}
