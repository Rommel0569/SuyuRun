using System.Collections.Generic;
using UnityEngine;

namespace SuyuRun
{
    public sealed partial class RunnerPrototype
    {
        // One coordinate system, identical to pixel_costa_preview.py: the central 320x180
        // window of each 640px layer, and a ground surface at image row 148. Physics stays
        // in course coordinates; no changes to encounters, speed, jump or music timing.
        const float CoastPpu=32f, CoastLeft=-5f, CoastTop=Ground+148f/CoastPpu;
        sealed class ReferenceBand
        {
            public float factor;
            public Sprite[] frames;
            public SpriteRenderer[] copies=new SpriteRenderer[2];
        }
        readonly List<ReferenceBand> referenceBands=new List<ReferenceBand>();
        readonly List<Sprite> referenceSprites=new List<Sprite>();
        readonly SpriteRenderer[] referenceDancers=new SpriteRenderer[2];
        Sprite[] referenceDanceFrames;
        double referenceEpoch;
        float referenceTime;

        Sprite ReferenceSprite(Texture2D texture,Rect rect)
        {
            var s=Sprite.Create(texture,rect,new Vector2(0,1),CoastPpu,0,SpriteMeshType.FullRect);
            referenceSprites.Add(s);return s;
        }
        SpriteRenderer ReferenceRenderer(string label,Sprite sprite,int order,Transform parent)
        {
            var go=new GameObject(label);go.transform.SetParent(parent,false);
            var r=go.AddComponent<SpriteRenderer>();r.sprite=sprite;r.sortingOrder=order;return r;
        }
        void ReferenceLayer(string name,float factor,int order,int count=1)
        {
            var texture=Resources.Load<Texture2D>(name);
            if(!texture)throw new System.InvalidOperationException("Missing approved Costa layer: "+name);
            var band=new ReferenceBand{factor=factor,frames=new Sprite[count]};
            for(int f=0;f<count;f++)band.frames[f]=ReferenceSprite(texture,new Rect(0,texture.height-(f+1)*180,640,180));
            for(int i=0;i<2;i++)band.copies[i]=ReferenceRenderer(name+" / tile "+i,band.frames[0],order,world);
            referenceBands.Add(band);
        }
        void BuildReferenceCoast()
        {
            world.name="Costa / approved pixel composition";
            cam.transform.position=new Vector3(0,CoastTop-90f/CoastPpu,-10);
            cam.orthographicSize=90f/CoastPpu;
            cam.backgroundColor=new Color32(33,28,45,255);
            var pp=cam.GetComponent<UnityEngine.Rendering.Universal.PixelPerfectCamera>();
            if(!pp)pp=cam.gameObject.AddComponent<UnityEngine.Rendering.Universal.PixelPerfectCamera>();
            pp.assetsPPU=32;pp.refResolutionX=320;pp.refResolutionY=180;
            pp.gridSnapping=UnityEngine.Rendering.Universal.PixelPerfectCamera.GridSnapping.UpscaleRenderTexture;
            pp.cropFrame=UnityEngine.Rendering.Universal.PixelPerfectCamera.CropFrame.Windowbox;
            var skyTexture=Resources.Load<Texture2D>("costa_capa1_cielo");
            var sky=ReferenceRenderer("Fixed sky / approved central window",ReferenceSprite(skyTexture,new Rect(160,0,320,180)),-40,world);
            sky.transform.localPosition=new Vector3(CoastLeft,CoastTop,0);
            var clouds=new GameObject("Clouds and continuous flocks");clouds.transform.SetParent(world,false);
            clouds.transform.localPosition=new Vector3(CoastLeft,CoastTop,0);
            bibleSkyLayer=clouds.AddComponent<PixelCostaSkyLayer>();
            bibleSkyLayer.UseReferenceWindow();
            bibleSkyLayer.SetTextures(Resources.Load<Texture2D>("costa_capa2_nubes"),Resources.Load<Texture2D>("costa_capa2_aves"));
            ReferenceLayer("costa_capa3_paracas",.15f,-36);
            ReferenceLayer("costa_capa4_mar",.30f,-34,4);
            ReferenceLayer("costa_capa5_huacas",.50f,-32);
            ReferenceLayer("costa_capa6_algarrobos",.75f,-30);
            float start=-30;
            foreach(var gap in course.gaps){BuildStructure(start,gap.x-start,0,false);start=gap.x+gap.width;}
            BuildStructure(start,Duration*Speed+30-start,0,false);
            foreach(var p in course.platforms)BuildStructure(p.x,p.width,p.elevation,true,p.style);
            var danceTexture=Resources.Load<Texture2D>("costa_bailarin");
            if(danceTexture)
            {
                referenceDanceFrames=new Sprite[6];
                for(int f=0;f<6;f++)referenceDanceFrames[f]=ReferenceSprite(danceTexture,new Rect(f*24,0,24,40));
                for(int i=0;i<2;i++)referenceDancers[i]=ReferenceRenderer("Festejo on huaca roof / "+i,referenceDanceFrames[0],-29,world);
            }
            referenceEpoch=AudioSettings.dspTime;
            UpdateReferenceCoast();
        }
        void UpdateReferenceCoast()
        {
            if(mode==Mode.Running)referenceTime=distance/Speed;
            else if(mode==Mode.Menu)referenceTime=(float)(AudioSettings.dspTime-referenceEpoch);
            waterTime=referenceTime;
            foreach(var band in referenceBands)
            {
                int shift=(160+Mathf.RoundToInt(distance*CoastPpu*band.factor))%640;
                var frame=band.frames[Mathf.FloorToInt(referenceTime*4)%band.frames.Length];
                for(int i=0;i<2;i++)
                {
                    band.copies[i].sprite=frame;
                    band.copies[i].transform.localPosition=new Vector3(CoastLeft+(i*640-shift)/CoastPpu,CoastTop,0);
                }
            }
            if(bibleSkyLayer)
            {
                bibleSkyLayer.SetPaused(mode!=Mode.Menu&&mode!=Mode.Running);
                bibleSkyLayer.SetTravelPixels(distance*CoastPpu);
            }
            if(referenceDanceFrames!=null)
            {
                float beat=course.SecondsToBeat(referenceTime);
                int f=Mathf.FloorToInt(Mathf.Repeat(beat*.5f,1)*6);
                int shift=(160+Mathf.RoundToInt(distance*CoastPpu*.5f))%640;
                // x=340 is the roof centre in the ORIGINAL layer 5; dancer moves with it,
                // not on an unrelated repeated island. Feet meet row 131, the top terrace.
                for(int i=0;i<2;i++)
                {
                    referenceDancers[i].sprite=referenceDanceFrames[f];
                    referenceDancers[i].transform.localPosition=new Vector3(CoastLeft+(340-12+i*640-shift)/CoastPpu,CoastTop-(131-40)/CoastPpu,0);
                }
            }
            foreach(var s in structures)
            {
                s.root.position=new Vector3(Mathf.Round((s.x-distance-3)*CoastPpu)/CoastPpu,0,0);
                s.root.gameObject.SetActive(s.x+s.width>distance-3&&s.x<distance+9);
            }
            AnimateBirds();
        }
        Sprite monedaIcon;
        void DrawCompactCoastHud()
        {
            if(!texHud){DrawCompactCoastHudLegacyFallback();return;}
            if(!monedaIcon)foreach(var s in Resources.LoadAll<Sprite>("costa_coleccionables"))if(s.name=="moneda_0")monedaIcon=s;
            // Top bar: costa_hud.png's panel_h, stretched (its adobe joint ticks are subtle enough
            // not to smear noticeably), with the 4 patrimony-piece slots, coin and points readouts.
            DrawSprite(new Rect(16,10,1248,40),HudSprite("panel_h"));
            for(int i=0;i<4;i++)
            {
                Rect sr=new Rect(24+i*44,14,32,32);
                DrawSprite(sr,HudSprite("piece_slot"));
                if(discovered[i]){GUI.color=gold;DrawSprite(new Rect(sr.x+6,sr.y+6,20,20),monedaIcon);GUI.color=Color.white;}
            }
            DrawSprite(new Rect(860,16,24,24),monedaIcon);
            GUI.Label(new Rect(888,15,140,26),$"{coins:00}",small);
            DrawSprite(new Rect(1040,16,24,24),HudSprite("icon_puntos"));
            GUI.Label(new Rect(1068,15,196,26),$"{score:0000}",small);
            var sandText=new GUIStyle(small);sandText.normal.textColor=new Color32(57,40,61,255);
            GUI.Label(new Rect(20,654,1200,26),$"ESPACIO saltar · S deslizar · E ráfaga {energy:0}% · ESC pausa     {ProgressSeconds:0} / {Duration:0}s",sandText);
            // Bottom progress bar: frame + fill sprite instead of a flat gold rect.
            DrawSprite(new Rect(20,690,1240,10),HudSprite("bar_progress_frame"));
            DrawSpriteTiled(new Rect(22,692,1236*Mathf.Clamp01(ProgressSeconds/Duration),6),HudSprite("bar_progress_fill"),3f);
            if(Time.unscaledTime<feedbackUntil)GUI.Label(new Rect(20,620,1100,26),feedback,sandText);
        }
        void DrawCompactCoastHudLegacyFallback()
        {
            Panel(new Rect(16,12,1248,34),new Color32(57,40,61,255));
            GUI.Label(new Rect(28,15,530,26),"SUYURUN  /  COSTA · EL ALCATRAZ",small);
            GUI.Label(new Rect(865,15,380,26),$"MONEDAS {coins:00}   PUNTOS {score:0000}",small);
            var sandText=new GUIStyle(small);sandText.normal.textColor=new Color32(57,40,61,255);
            GUI.Label(new Rect(20,654,1200,26),$"ESPACIO saltar · S deslizar · E ráfaga {energy:0}% · ESC pausa     {ProgressSeconds:0} / {Duration:0}s",sandText);
            GUI.color=gold;GUI.DrawTexture(new Rect(20,692,1240*Mathf.Clamp01(ProgressSeconds/Duration),4),Texture2D.whiteTexture);GUI.color=Color.white;
            if(Time.unscaledTime<feedbackUntil)GUI.Label(new Rect(20,620,1100,26),feedback,sandText);
        }
    }
}
