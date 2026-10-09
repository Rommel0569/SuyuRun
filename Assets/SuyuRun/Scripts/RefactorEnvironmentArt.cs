using System;
using System.Collections.Generic;
using UnityEngine;
namespace SuyuRun
{
    // Texture resolution is independent of level geometry, parallax and MIDI X.
    public static class RefactorEnvironmentArt
    {
        [Serializable] sealed class Spec
        {
            public int frames,frameWidth,frameHeight,logicalWidth,logicalHeight,pixelsPerUnit;
        }
        static readonly Dictionary<string,Sprite> cache=new Dictionary<string,Sprite>();
        public static Sprite Frame(string name,Sprite legacy)
        {
            if(name!="costa_capa3_paracas"&&name!="sierra_compartido_capa3_nevados"&&name!="selva_bosque_capa3_estructura"&&
               name!="costa_capa5_huacas"&&name!="sierra_ruinas_capa4_paisaje"&&name!="selva_templo_capa3_estructura"&&
               name!="sierra_cueva1_arcos"&&name!="sierra_cueva2_arcos"&&name!="sierra_valle_capa4_paisaje"&&
               name!="selva_rio_capa3_estructura"&&name!="selva_noche_capa3_estructura"&&
               name!="sierra_camino_capa4_paisaje"&&name!="selva_cueva_capa3_estructura"&&name!="selva_plataforma_tronco"&&name!="sierra_ruinas_bloque")return legacy;
            if(cache.TryGetValue(name,out var cached)&&cached)return cached;
            var texture=Resources.Load<Texture2D>("RefactorBank/AtmosphereClean/"+name)??Resources.Load<Texture2D>("RefactorBank/LayerPolish/"+name)??Resources.Load<Texture2D>("RefactorBank/Environment/"+name);
            var metadata=Resources.Load<TextAsset>("RefactorBank/AtmosphereClean/"+name)??Resources.Load<TextAsset>("RefactorBank/LayerPolish/"+name)??Resources.Load<TextAsset>("RefactorBank/Environment/"+name);
            // Import/domain reload can briefly make a resource unavailable.
            // Never cache that transient miss for the entire game session.
            if(!texture||!metadata)return legacy;
            var spec=JsonUtility.FromJson<Spec>(metadata.text);
            bool cave=name=="sierra_cueva1_arcos"||name=="sierra_cueva2_arcos";
            bool log=name=="selva_plataforma_tronco";
            bool block=name=="sierra_ruinas_bloque";
            int expectedHeight=block?32:log?16:cave?384:180,expectedWidth=(log||block)?64:640;
            int density=spec.pixelsPerUnit/32;
            if(spec.frames!=1||spec.logicalWidth!=expectedWidth||spec.logicalHeight!=expectedHeight||
               spec.frameWidth!=texture.width||spec.frameHeight!=texture.height||
               (spec.pixelsPerUnit!=64&&spec.pixelsPerUnit!=128)||texture.width!=expectedWidth*density||texture.height!=expectedHeight*density)
                throw new InvalidOperationException("Invalid environment registration: "+name);
            var sprite=Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),new Vector2(0,1),spec.pixelsPerUnit,0,SpriteMeshType.FullRect);
            sprite.name="RefactorEnvironment/"+name;
            RefactorActorArt.Register(sprite,new Vector2(spec.logicalWidth,spec.logicalHeight));
            cache[name]=sprite;return sprite;
        }
    }
}
