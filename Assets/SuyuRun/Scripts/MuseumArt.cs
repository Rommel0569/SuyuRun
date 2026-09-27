using System.Collections.Generic;
using UnityEngine;
namespace SuyuRun
{
    public static class MuseumArt
    {
        static readonly Dictionary<string,Texture2D> cache=new Dictionary<string,Texture2D>();
        public static Texture2D Get(string id)
        {
            if(!cache.TryGetValue(id,out var texture)||!texture)
            {texture=Resources.Load<Texture2D>("Museum/"+id);cache[id]=texture;}
            return texture;
        }
        public static bool Draw(Rect rect,string id,bool unlocked)
        {
            var texture=Get(id);if(!texture)return false;
            var old=GUI.color;GUI.color=unlocked?Color.white:new Color32(33,28,45,255);
            GUI.DrawTexture(rect,texture,ScaleMode.ScaleToFit);GUI.color=old;return true;
        }
    }
}
