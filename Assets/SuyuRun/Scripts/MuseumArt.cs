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
            {texture=id.StartsWith("selva_")?Resources.Load<Texture2D>("SelvaV2/selva_museo_"+id.Substring(6)+"_v4"):Resources.Load<Texture2D>("Museum/"+id);if(!texture&&id.StartsWith("selva_"))texture=Resources.Load<Texture2D>("SelvaV2/selva_museo_"+id.Substring(6));cache[id]=texture;}
            return texture;
        }
        public static bool Draw(Rect rect,string id,bool unlocked)
        {
            var texture=Get(id);if(!texture)return false;
            var old=GUI.color;GUI.color=unlocked?Color.white:Color.black;
            GUI.DrawTexture(rect,texture,ScaleMode.ScaleToFit);GUI.color=old;return true;
        }
        // Tiny HUD slots use the readable pickup sprite, not a downsampled museum plate.
        // The full-resolution museum illustrations remain untouched.
        public static bool DrawHud(Rect rect,string id,bool unlocked)
        {
            if(!id.StartsWith("selva_"))return Draw(rect,id,unlocked);
            var sprite=SelvaArt.Frame("coleccionable_"+id.Substring(6),0,0);
            if(!sprite)return Draw(rect,id,unlocked);
            var old=GUI.color;GUI.color=unlocked?Color.white:Color.black;
            RunnerHud.Sprite(rect,sprite);GUI.color=old;return true;
        }
    }
}
