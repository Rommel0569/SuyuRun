using System;
using System.Collections.Generic;
using UnityEngine;
namespace SuyuRun
{
    public static class RefactorMaterialArt
    {
        [Serializable] sealed class Spec {public int frames,frameWidth,frameHeight,logicalWidth,logicalHeight,pixelsPerUnit;public float fps;public bool loop,readabilityLoop;}
        sealed class Sheet {public Sprite[] frames;public Spec spec;}
        static readonly Dictionary<string,Sheet> cache=new Dictionary<string,Sheet>();
        public static Sprite Frame(string name,Sprite legacy,float age,int forced=-1)
        {
            if(!legacy)return legacy;
            bool ambient=name.StartsWith("costa_")||name.Contains("_capa")||name.Contains("_fx_")||name.Contains("_ui_")||name.Contains("_decor_")||name=="selva_niebla"||name=="selva_rayos_sol"||name.StartsWith("selva_agua_");
            if(!ambient&&!name.StartsWith("sierra_ruinas_")&&!name.StartsWith("sierra_cueva_")&&!name.EndsWith("_capa7_suelo")&&name!="sierra_piedra"&&name!="sierra_pinchos"&&name!="sierra_tronco"&&!name.StartsWith("selva_plataforma_")&&!name.StartsWith("selva_obstaculo_")&&!name.StartsWith("selva_tile_")&&!name.StartsWith("costa_plataforma_")&&name!="sierra_tunel_entrada"&&name!="sierra_tunel_salida"&&name!="sierra_portal_ceremonial"&&name!="selva_portal_salida")return legacy;
            if(!cache.TryGetValue(name,out var sheet)||!sheet.frames[0])
            {
                var texture=Resources.Load<Texture2D>("RefactorBank/AtmosphereClean/"+name)??Resources.Load<Texture2D>("RefactorBank/LayerPolish/"+name)??Resources.Load<Texture2D>("RefactorBank/Readability/"+name)??Resources.Load<Texture2D>("RefactorBank/Materials/"+name);var meta=Resources.Load<TextAsset>("RefactorBank/AtmosphereClean/"+name)??Resources.Load<TextAsset>("RefactorBank/LayerPolish/"+name)??Resources.Load<TextAsset>("RefactorBank/Readability/"+name)??Resources.Load<TextAsset>("RefactorBank/Materials/"+name);
                if(!texture||!meta)return legacy;
                // Costa facade meshes repeat UVs over the whole curb. Clamp
                // would smear the last texel instead of repeating its frieze.
                if(name=="costa_capa7_suelo")texture.wrapMode=TextureWrapMode.Repeat;
                var s=JsonUtility.FromJson<Spec>(meta.text);
                int density=s.pixelsPerUnit/32;
                if(s.frames<1||(s.pixelsPerUnit!=64&&s.pixelsPerUnit!=128)||s.logicalWidth!=legacy.rect.width||s.logicalHeight!=legacy.rect.height||s.frameWidth!=s.logicalWidth*density||s.frameHeight!=s.logicalHeight*density||texture.width!=s.frameWidth*s.frames||texture.height!=s.frameHeight)throw new InvalidOperationException("Invalid material registration "+name);
                var frames=new Sprite[s.frames];
                for(int i=0;i<frames.Length;i++)
                {
                    frames[i]=Sprite.Create(texture,new Rect(i*s.frameWidth,0,s.frameWidth,s.frameHeight),new Vector2(0,1),s.pixelsPerUnit,0,SpriteMeshType.FullRect);
                    frames[i].name="RefactorMaterial/"+name+"_"+i;RefactorActorArt.Register(frames[i],new Vector2(s.logicalWidth,s.logicalHeight));
                }
                sheet=new Sheet{frames=frames,spec=s};cache[name]=sheet;
            }
            int f=forced>=0&&!sheet.spec.readabilityLoop?forced:Mathf.FloorToInt(Mathf.Max(0,age)*sheet.spec.fps);
            return sheet.frames[sheet.spec.loop?f%sheet.frames.Length:Mathf.Clamp(f,0,sheet.frames.Length-1)];
        }
        public static Texture2D FullTexture(Texture2D legacy)
        {
            if(!legacy)return legacy;
            var texture=Resources.Load<Texture2D>("RefactorBank/AtmosphereClean/"+legacy.name)??Resources.Load<Texture2D>("RefactorBank/LayerPolish/"+legacy.name)??Resources.Load<Texture2D>("RefactorBank/Materials/"+legacy.name);
            return texture&&((texture.width==legacy.width*2&&texture.height==legacy.height*2)||(texture.width==legacy.width*4&&texture.height==legacy.height*4))?texture:legacy;
        }
    }
}
