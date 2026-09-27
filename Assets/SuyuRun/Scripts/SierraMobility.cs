using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SuyuRun
{
    public sealed partial class SierraStageDirector
    {
        [Serializable] sealed class MobilitySequence { public float t; public string kind; }
        readonly List<RunnerPickup> mobilityPoints=new List<RunnerPickup>();
        bool airImpulse;
        float diveUntil=-1;
        int padUses,orbUses,diveUses;
        public string MobilityReport=>$"Pads={padUses}; Orbs={orbUses}; Dives={diveUses}";
        bool RiserNeedsJump(float world)
        {
            float t=(world-HeroX)/Speed,local=Mathf.Repeat(world,640);
            // Mark the entrance to a staircase group, not every tiny tread. Leave the
            // authored MIDI obstacles and launch/dive corridors their full landing room.
            if(local<125||local>195||NearTunnel(t,2)||t>duration-5)return false;
            if(mobilityPoints.Any(p=>p.kind==5&&world>p.x-Speed*1.6f&&world<p.x+Speed*3))return false;
            return !enemies.Any(e=>!e.air&&Mathf.Abs(e.anchor-world)<Speed*1.25f);
        }

        void BuildMobility(MobilitySequence[] sequences,float resumeTime)
        {
            mobilityPoints.Clear();airImpulse=false;diveUntil=-1;
            if(resumeTime<=0)padUses=orbUses=diveUses=0;
            if(sequences==null)return;
            foreach(var sequence in sequences)
            {
                if(sequence.t<resumeTime)continue;
                float x=sequence.t*Speed+HeroX;
                float floor=Surface(x,ActAt(sequence.t));
                // Same pickup kind4 (air charge) / kind5 (pad) contract used by Costa.
                mobilityPoints.Add(new RunnerPickup{x=x,y=floor-4,width=20,height=12,kind=5});
                mobilityPoints.Add(new RunnerPickup{x=x+Speed*.36f,y=floor-67,width=28,height=32,kind=4});
                mobilityPoints.Add(new RunnerPickup{x=x+Speed*.70f,y=floor-116,width=28,height=36,kind=7});
                mobilityPoints.Add(new RunnerPickup{x=x+Speed*1.3f,y=Surface(x+Speed*1.3f,ActAt(sequence.t)),width=72,height=4,kind=8});
                float launch=sequence.t+.78f;
                float birdX=launch*Speed+340;
                enemies.Add(new Enemy{x=birdX,anchor=birdX,attackAt=launch,y=Mathf.Max(28,floor-91),air=true,kind="cross_bird"});
                float beamX=x+Speed*1.8f;
                enemies.Add(new Enemy{x=beamX,anchor=beamX,kind="slide"});
            }
        }

        bool UpdateMobility(bool jumpPressed,bool downPressed)
        {
            bool launched=false;
            float world=seconds*Speed+HeroX;
            foreach(var point in mobilityPoints)
            {
                if(point.taken||point.kind==8)continue;
                Vector2 center=new Vector2(world,point.kind==5?feet:feet-22);
                if(point.kind==5&&!grounded)continue;
                if(point.kind!=5&&grounded)continue;
                if(!point.TryCollect(center,new Vector2(9,point.kind==5?7:12)))continue;
                if(point.kind==5)
                {
                    verticalSpeed=-180;grounded=false;jumps=1;padUses++;
                    launched=true;
                    EffectAt("polvo_aterrizar",world-16,feet-18);
                }
                else if(point.kind==4){airImpulse=true;orbUses++;EffectAt("chispas",point.x-12,point.y-12);}
                else {diveUntil=seconds+.45f;EffectAt("chispas",point.x-12,point.y-12);}
            }
            if(airImpulse&&!grounded&&(jumpPressed||DiagnosticAutoplay))
            {
                verticalSpeed=-190;jumps=2;airImpulse=false;
                launched=true;
                EffectAt("polvo_aterrizar",world-16,feet-18);
            }
            if(!grounded&&seconds<diveUntil&&(downPressed||DiagnosticAutoplay))
            {
                // x continues at runner speed: positive y velocity creates a diagonal dive.
                verticalSpeed=270;diveUntil=-1;airImpulse=false;jumps=2;diveUses++;
                downInput.Clear();
                EffectAt("chispas",world-12,feet-30);
            }
            return launched;
        }

        void DrawMobilityHints()
        {
            var hint=new GUIStyle(label){fontSize=5,padding=new RectOffset(0,0,0,0)};
            if(act==2)
            {
                float distance=seconds*Speed;
                int group=Mathf.FloorToInt(distance/640);
                for(int i=group;i<=group+1;i++)
                {
                    float world=i*640+160,x=world-distance;
                    if(x<0||x>320||!RiserNeedsJump(world))continue;
                    // Follow the actual step profile: the warning belongs to the ground,
                    // not to a floating label or to an extra invisible collision box.
                    for(int j=-28;j<=12;j++)
                    {
                        float ground=Surface(world+j,2);
                        GUI.color=new Color32(26,32,54,255);
                        GUI.DrawTexture(new Rect(Mathf.Round(x+j),ground-3,1,5),Texture2D.whiteTexture);
                        GUI.color=(j+Mathf.FloorToInt(seconds*8))%8<4?new Color32(245,223,160,255):new Color32(237,146,84,255);
                        GUI.DrawTexture(new Rect(Mathf.Round(x+j),ground-2,1,2),Texture2D.whiteTexture);
                    }
                    float glyphY=Surface(world-12,2)-8-Mathf.FloorToInt(Mathf.Repeat(seconds*4,3))*3;
                    for(int pass=0;pass<2;pass++)for(int j=0;j<7;j++)
                    {
                        GUI.color=pass==0?new Color32(26,32,54,255):new Color32(255,243,208,255);
                        int border=pass==0?1:0;
                        GUI.DrawTexture(new Rect(x-12-j-border,glyphY+j-border,1+border*2,2+border*2),Texture2D.whiteTexture);
                        GUI.DrawTexture(new Rect(x-12+j-border,glyphY+j-border,1+border*2,2+border*2),Texture2D.whiteTexture);
                    }
                    GUI.color=Color.white;
                    GUI.Label(new Rect(x-18,Surface(world,2)-40,60,12),"SALTA",hint);
                }
            }
            foreach(var bird in enemies)
                if(bird.kind=="cross_bird"&&!bird.dead&&seconds>bird.attackAt-.65f&&seconds<bird.attackAt+.35f)
                {
                    GUI.color=new Color32(194,103,79,255);
                    GUI.Label(new Rect(190,35,126,18),"¡AVE VELOZ!\nS / ABAJO",hint);
                    GUI.DrawTexture(new Rect(304,bird.y-4,8,8),Texture2D.whiteTexture);
                    GUI.color=Color.white;
                }
            foreach(var point in mobilityPoints)
            {
                float x=Mathf.Round(point.x-seconds*Speed),y=Mathf.Round(point.y);
                if(point.taken||x<-35||x>350)continue;
                Color color=point.kind==7?new Color32(194,103,79,255):new Color32(245,223,160,255);
                GUI.color=color;
                if(point.kind==5||point.kind==8)
                {
                    GUI.DrawTexture(new Rect(x-(point.kind==8?36:10),y,point.kind==8?72:20,2),Texture2D.whiteTexture);
                    if(point.kind==5)for(int j=0;j<3;j++)GUI.DrawTexture(new Rect(x-7+j*6,y-3,3,2),Texture2D.whiteTexture);
                }
                else
                {
                    // Costa's nested diamond + directional glyph, rebuilt at native pixels
                    // with the existing Sierra palette (blue up / vermilion down).
                    for(int row=-12;row<=12;row++)
                    {
                        int half=12-Mathf.Abs(row);
                        GUI.color=new Color32(26,32,54,255);GUI.DrawTexture(new Rect(x-half-1,y+row,half*2+3,1),Texture2D.whiteTexture);
                        GUI.color=point.kind==7?new Color32(237,146,84,255):new Color32(155,196,198,255);
                        GUI.DrawTexture(new Rect(x-half,y+row,half*2+1,1),Texture2D.whiteTexture);
                        if(half>4){GUI.color=point.kind==7?new Color32(139,46,46,255):new Color32(62,100,133,255);GUI.DrawTexture(new Rect(x-half+4,y+row,half*2-7,1),Texture2D.whiteTexture);}
                    }
                    GUI.color=new Color32(255,243,208,255);
                    for(int i=0;i<4;i++)
                    {
                        float yy=y+(point.kind==7?3-i:i-3);
                        GUI.DrawTexture(new Rect(x-i,yy,1,1),Texture2D.whiteTexture);
                        GUI.DrawTexture(new Rect(x+i,yy,1,1),Texture2D.whiteTexture);
                    }
                }
                GUI.color=Color.white;
                if(point.kind!=8)GUI.Label(new Rect(x-15,y+11,50,9),point.kind==7?"S / ABAJO":point.kind==4?"ESPACIO":"IMPULSO",hint);
            }
            if(airImpulse||seconds<diveUntil)
                GUI.Label(new Rect(60,151,240,10),airImpulse?"ESPACIO: SEGUNDO IMPULSO":"S / ABAJO: DESCENSO DIAGONAL",hint);
        }
        Sprite readableBarrier;
        Sprite floatingRelic;
        Sprite FloatingRelic()
        {
            if(floatingRelic)return floatingRelic;
            var texture=new Texture2D(32,32,TextureFormat.RGBA32,false){filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp};
            var pixels=new Color32[32*32];
            Color32 ink=new Color32(26,32,54,255),blue=new Color32(62,100,133,255),aqua=new Color32(155,196,198,255),light=new Color32(255,243,208,255);
            for(int y=2;y<30;y++)for(int x=0;x<32;x++)
            {
                int inset=2+Mathf.Max(0,8-Mathf.Min(y-2,29-y));
                if(x<inset||x>31-inset)continue;
                bool border=x==inset||x==31-inset||y==2||y==29;
                pixels[y*32+x]=border?ink:x<inset+3||y>25?aqua:blue;
                // Inlaid stepped glyph: a stone seal, not a wooden sign.
                if((y==12||y==20)&&x>=10&&x<=22||x==10&&y>=12&&y<=20||x==22&&y>=16&&y<=20||y==16&&x>=15&&x<=22)pixels[y*32+x]=light;
            }
            texture.SetPixels32(pixels);texture.Apply();
            floatingRelic=Sprite.Create(texture,new Rect(0,0,32,32),new Vector2(0,1),32);
            return floatingRelic;
        }
        Sprite ReadableBarrier()
        {
            if(readableBarrier)return readableBarrier;
            var texture=new Texture2D(64,32,TextureFormat.RGBA32,false){filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp};
            var colors=new Color32[64*32];
            Color32 ink=new Color32(26,32,54,255),red=new Color32(139,46,46,255),light=new Color32(245,223,160,255),edge=new Color32(237,146,84,255);
            for(int y=0;y<32;y++)for(int x=0;x<64;x++)
            {
                int inset=y<4?4-y:y>27?y-27:0;
                if(x<inset||x>63-inset)continue;
                bool border=y==0||y==31||x==inset||x==63-inset;
                colors[y*64+x]=border?ink:y<6||y>25?((x+y)%12<6?light:ink):x<5||x>58?edge:red;
                if(x>=30&&x<=33&&((y>=16&&y<=23)||(y>=10&&y<=12)))colors[y*64+x]=light;
            }
            texture.SetPixels32(colors);texture.Apply();
            readableBarrier=Sprite.Create(texture,new Rect(0,0,64,32),new Vector2(0,1),32);
            return readableBarrier;
        }
    }
}
