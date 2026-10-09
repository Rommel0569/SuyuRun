using System;
using System.Collections.Generic;
using UnityEngine;

namespace SuyuRun
{
    // Visual migration only. Legacy sheets remain the source of gameplay hitboxes.
    public static class RefactorActorArt
    {
        [Serializable] sealed class Spec
        {
            public int frames,frameWidth,frameHeight,fps;
            public bool loop;
            public float referenceExtent,footY;
            public int[] frameDurationsMs;
        }
        sealed class Sheet { public Sprite[] frames; public Spec spec; }
        static readonly Dictionary<string,Sheet> sheets=new Dictionary<string,Sheet>();
        static readonly Dictionary<Sprite,Vector2> logicalSizes=new Dictionary<Sprite,Vector2>();
        public static bool IsRegistered(Sprite sprite)=>sprite&&logicalSizes.ContainsKey(sprite);
        public static void Register(Sprite sprite,Vector2 logicalSize)=>logicalSizes[sprite]=logicalSize;
        public static void RegisterHero(Sprite sprite)=>logicalSizes[sprite]=new Vector2(48,48);
        public static Vector2 Size(Sprite sprite)=>logicalSizes.TryGetValue(sprite,out var size)?size:sprite.rect.size;

        static string Map(string name,out float extent,out bool byWidth,int variant=-1)
        {
            extent=80;byWidth=false;
            if(name=="cueva_jefe_piedra_derrota")return "stone/derrota";
            if(name=="cueva_jefe_guerrero_derrota")return "inca/derrota";
            if(name=="cueva_esbirro_piedra_derrota"){extent=28;return "Actors/pebble/death";}
            if(name=="cueva_esqueleto_derrota"){extent=40;return "Actors/skeleton/death";}
            if(name=="guardian_idle"||name=="guardian_huaraca"){extent=40;return "Actors/guardian/"+(name=="guardian_idle"?"idle":"anticipation");}
            if(name=="hexagono"){extent=18;return "Actors/hexagon/walk";}
            if(name=="pajaro"){extent=23;byWidth=true;return "Actors/coastbird/flight";}
            if(name=="bote_pescador"){extent=54;byWidth=true;return "Actors/boat/walk";}
            if(name=="bailarin"){extent=38;return "Actors/dancer/dance";}
            if(name=="condor_vuelo"||name=="halcon_vuelo"||name=="ave_puna_vuelo")
            {byWidth=true;extent=name=="condor_vuelo"?58:30;return "Actors/"+(name=="condor_vuelo"?"condor":name=="halcon_vuelo"?"hawk":"puna")+"/flight";}
            if(name=="cueva_esbirro_piedra"){extent=28;return "Actors/pebble/walk";}
            if(name.StartsWith("cueva_esqueleto")){extent=40;return "Actors/skeleton/"+(name.EndsWith("despertar")?"spawn":"walk");}
            if(name=="cueva_jefe_piedra")return "stone/idle";
            if(name=="cueva_jefe_guerrero")return "inca/idle";
            if(name.StartsWith("cueva_box_"))
            {string action=name.Substring(10);return action=="andar"?"stone/andar":action=="reposo"?"stone/idle":action=="guardia"?"stone/anticipar":action=="upper"?"stone/MotionV2/uppercut":action=="jab"?"stone/MotionV2/jab":"stone/golpe";}
            if(name.StartsWith("cueva_inca_"))
            {string action=name.Substring(11);return action=="carrera_carga"?"inca/MotionV3/charge_run":action=="retroceso"?"inca/MotionV3/retreat":action=="andar"?"inca/andar":action=="reposo"?"inca/idle":action=="guardia"?"inca/anticipar":action=="carga"?"inca/MotionV2/charge":action=="sol"?"inca/MotionV2/summon":"inca/MotionV2/mace";}
            if(name.StartsWith("caiman_"))
            {
                byWidth=true;extent=58;string state=name.Substring(7);
                return "Actors/caiman/"+(state=="emerger"?"breach":state=="caida"?"fall":state=="inmersion"?"dive":state=="anticipacion"?"anticipation":state=="snap"?"snap":"idle");
            }
            string prefix=name.StartsWith("boss_raices_")?"boss_raices_":name.StartsWith("boss_chullachaqui_")?"boss_chullachaqui_":null;
            if(prefix!=null)
            {
                extent=prefix=="boss_raices_"?72:82;string actor=prefix=="boss_raices_"?"treant":"chulla";
                string state=name.Substring(prefix.Length);
                if(state=="golpe"&&variant>=0)return actor+"/MotionV2/"+(actor=="treant"?new[]{"charge","bind","roots"}[variant%3]:new[]{"curse","summon","lunge"}[variant%3]);
                return actor+"/"+state;
            }
            if(name.StartsWith("esbirro_brote_"))
            {extent=28;string state=name.Substring(14);return "Actors/sprout/"+(state=="derrota"?"death":state=="hit"?"hit":state=="caminar"?"walk":"idle");}
            foreach(string prefixEcho in new[]{"esbirro_ecopiedra_","esbirro_ecoinca_","esbirro_eco_"})
                if(name.StartsWith(prefixEcho))
                {
                    extent=28;string state=name.Substring(prefixEcho.Length);
                    string actor=prefixEcho=="esbirro_ecopiedra_"?"stone":prefixEcho=="esbirro_ecoinca_"?"inca":"Actors/skeleton";
                    return actor+"/"+(state=="derrota"?(actor.StartsWith("Actors/")?"death":"derrota"):state=="hit"?"hit":state=="caminar"?(actor.StartsWith("Actors/")?"walk":"andar"):"idle");
                }
            if(name=="fx_raiz_erupcion")return "Effects/raiz_dano";
            if(name=="fx_raiz_amarre")return "Effects/arbol_atadura";
            if(name=="fx_maldicion")return "Effects/maldicion";
            if(name=="fx_impacto")return "Effects/impacto_arma";
            if(name=="impacto_piedra")return "Effects/impacto_piedra";
            if(name=="fx_escudo_romper")return "Effects/escudo";
            return null;
        }

        public static Sprite Frame(string name,float age,int forced,Sprite legacy,int legacyCount=0,int variant=-1)
        {
            string path=Map(name,out float extent,out bool byWidth,variant);if(path==null)return legacy;
            string key=path+"/"+legacy.rect.width+"/"+legacy.rect.height;
            if(!sheets.TryGetValue(key,out var sheet)||sheet.frames.Length==0||!sheet.frames[0])
            {
                var texture=Resources.Load<Texture2D>("RefactorBank/"+path);
                var metadata=Resources.Load<TextAsset>("RefactorBank/"+path);
                if(!texture||!metadata)return legacy;
                var spec=JsonUtility.FromJson<Spec>(metadata.text);
                if(spec.frames<1||spec.referenceExtent<=0)return legacy;
                float ppu=32*spec.referenceExtent/extent,ratio=ppu/32;
                float foot=spec.footY>0?spec.footY:124;
                // Custom pivot represents the ORIGINAL logical top-left. Feet
                // are registered to the legacy bottom regardless of wide cells.
                var pivot=new Vector2((spec.frameWidth*.5f-legacy.rect.width*.5f*ratio)/spec.frameWidth,
                    (spec.frameHeight-foot+(legacy.rect.height-1)*ratio)/spec.frameHeight);
                if(name=="pajaro")pivot.y=(spec.frameHeight-foot+legacy.rect.height*.8f*ratio)/spec.frameHeight;
                if(path.StartsWith("Effects/")){ppu=32*spec.frameWidth/legacy.rect.width;pivot=new Vector2(0,1);}
                var frames=new Sprite[spec.frames];
                for(int i=0;i<frames.Length;i++)
                {
                    frames[i]=Sprite.Create(texture,new Rect(i*spec.frameWidth,0,spec.frameWidth,spec.frameHeight),pivot,ppu,0,SpriteMeshType.FullRect);
                    frames[i].name="RefactorPresentation/"+path+"/"+i;
                    logicalSizes[frames[i]]=legacy.rect.size;
                }
                sheet=new Sheet{frames=frames,spec=spec};sheets[key]=sheet;
            }
            int frame;
            if(forced>=0)frame=legacyCount>1?Mathf.RoundToInt(forced*(sheet.frames.Length-1f)/(legacyCount-1)):forced;
            else if(sheet.spec.frameDurationsMs!=null&&sheet.spec.frameDurationsMs.Length==sheet.frames.Length)
            {
                int total=0;foreach(int duration in sheet.spec.frameDurationsMs)total+=Mathf.Max(1,duration);
                float milliseconds=Mathf.Max(0,age)*1000;
                if(sheet.spec.loop)milliseconds%=total;
                frame=0;while(frame<sheet.frames.Length-1&&milliseconds>=sheet.spec.frameDurationsMs[frame])milliseconds-=sheet.spec.frameDurationsMs[frame++];
            }
            else frame=Mathf.FloorToInt(Mathf.Max(0,age)*sheet.spec.fps);
            if(forced<0&&path.Contains("/MotionV2/")&&name.StartsWith("cueva_"))
                frame=3+Mathf.FloorToInt(Mathf.Max(0,age)/.7f*(sheet.frames.Length-3));
            if(forced<0&&path.Contains("/MotionV2/")&&name.StartsWith("boss_"))
                frame=3+Mathf.FloorToInt(Mathf.Max(0,age)/.65f*(sheet.frames.Length-3));
            return sheet.frames[sheet.spec.loop?frame%sheet.frames.Length:Mathf.Clamp(frame,0,sheet.frames.Length-1)];
        }
    }
}
