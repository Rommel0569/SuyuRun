using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace SuyuRun
{
    public sealed partial class SelvaStageDirector
    {
        readonly List<SpriteRenderer> renderers=new List<SpriteRenderer>();int used;Material spriteMaterial;SpriteRenderer heroDisplay;Animator heroAnimator;
        sealed class VineTexture{public Texture2D texture;public Sprite sprite;public Color32[] pixels=new Color32[288*240];}
        readonly Dictionary<SelvaVine,VineTexture> ropeTextures=new Dictionary<SelvaVine,VineTexture>();
        readonly SharedGameMenu.State menuState=new SharedGameMenu.State{region=2};
        string heroState;float heroStateAt;
        readonly RunnerAirClock heroAirClock=new RunnerAirClock();
        Texture2D waterMask;Sprite waterMaskSprite;Color32[] waterSource,waterPixels;Texture2D waterAtlas;int waterDetail=1;
        float HeroFrameTime(string state,float now)
        {
            float airborneAge=heroAirClock.Age(mini?.grounded??grounded,mini?.jumps??jumps,mini?.velocity.y??velocity.y,now);
            // Track locomotion too: returning to jump after running must restart that jump.
            if(heroState!=state||now<heroStateAt){heroState=state;heroStateAt=now;}
            // Run and running-fire share a gait clock: tapping fire must not
            // restart the legs at frame zero on every shot.
            if(state=="burst"&&mini!=null&&Mathf.Abs(mini.velocity.x)<1)return now-mini.fireAt;
            if(state=="run"||state=="burst")return GameScale.GaitClock(mini!=null?mini.gaitDistance:routeTime*Speed);
            if(state=="jump"||state=="double_jump"||state=="fall")return airborneAge;
            return now-heroStateAt;
        }
        void InitRendering()
        {
            var shader=Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default")??Shader.Find("Sprites/Default");spriteMaterial=new Material(shader);
            var go=new GameObject("Selva Hero Animator");go.transform.SetParent(transform);heroDisplay=go.AddComponent<SpriteRenderer>();heroDisplay.sharedMaterial=spriteMaterial;
            // A legacy Animator can overwrite the DSP-selected Tayta sprite later
            // in Unity's animation update. One renderer has exactly one owner.
            heroAnimator=go.AddComponent<Animator>();heroAnimator.enabled=false;
        }
        void HeroSprite(string state,float x,float y,float age,bool left=false,int pose=-1)
        {
            heroDisplay.enabled=true;heroDisplay.sortingOrder=58;heroDisplay.flipX=left;
            heroDisplay.color=Color.white;
            if(heroDisplay.GetComponent<PortalDoorMask>())RunnerPortalIngress.Apply(heroDisplay,Vector3.zero,0);
            heroDisplay.transform.localScale=Vector3.one;
            heroDisplay.transform.position=new Vector3((GameScale.SnapVisual(x,true)+(left?48:0))/32,(180-GameScale.SnapVisual(y,true))/32,0);
            // Exact sprite key selection at DSP time: no blended frames or off-grid interpolation.
            heroDisplay.sprite=TaytaArt.Frame("selva",state,age,pose)??SelvaArt.Frame("heroe_"+state,age,pose);
            TaytaContactDust.Track(heroDisplay,"selva",state,mini!=null?mini.gaitDistance:routeTime*Speed);
        }
        void Paint(Sprite sprite,float x,float y,int order,bool left=false)
        {
            left=RunnerFacing.FlipFor(sprite,left);
            Vector2 logicalSize=RefactorActorArt.Size(sprite);
            if(x+logicalSize.x<-40||x>360||y+logicalSize.y<-24||y>204)return;
            if(used>=renderers.Count){var go=new GameObject("Selva native sprite");go.transform.SetParent(transform);var r=go.AddComponent<SpriteRenderer>();r.sharedMaterial=spriteMaterial;renderers.Add(r);}
            var sr=renderers[used++];sr.enabled=true;sr.sprite=sprite;sr.sortingOrder=order;sr.flipX=left;sr.color=Color.white;
            // Top-left pivot flips around x=0; compensate so hitboxes and visuals share an origin.
            bool detailed=RefactorActorArt.IsRegistered(sprite);
            sr.transform.position=new Vector3((GameScale.SnapVisual(x,detailed)+(left?logicalSize.x:0))/32,(180-GameScale.SnapVisual(y,detailed))/32,0);
        }
        void Draw(string name,float x,float y,int order,float time=-1,bool left=false,int frame=-1,int variant=-1)
        {
            float age=time<0?clock:time;var legacy=SelvaArt.Frame(name,age,frame);
            var replacement=RefactorEnvironmentArt.Frame("selva_"+name,legacy);
            if(replacement==legacy)replacement=RefactorMaterialArt.Frame("selva_"+name,legacy,age,frame);
            Paint(replacement!=legacy?replacement:RefactorActorArt.Frame(name,age,frame,legacy,SelvaArt.Get(name).spec.frames,variant),x,y,order,left);
        }
        void Layer(string name,float factor,int order,float y=0)
        {float offset=Mathf.Repeat(cameraX*factor,640);Draw(name,-offset,y,order);Draw(name,640-offset,y,order);}
        void Render()
        {
            if(course==null)return;used=0;
            RunnerPixelViewport.Configure(cam,false,mini!=null&&mini.Boss.awakened&&mini.Boss.hp>0&&!mini.Cinematic);
            cameraX=mini==null?Mathf.Clamp(pos.x-92,zoneStart,Mathf.Max(zoneStart,zoneEnd-210)):mini.Cinematic?Mathf.Clamp(Mathf.Lerp(mini.pos.x-120,mini.Boss.pos.x-160,Mathf.SmoothStep(0,1,mini.Boss.timer/.8f)),0,SelvaMiniWorld.Width-320):Mathf.Clamp(mini.pos.x-120,0,SelvaMiniWorld.Width-320);
            cameraY=mini==null?0:mini.Cinematic?Mathf.Clamp(mini.Boss.pos.y-135,0,SelvaMiniWorld.Height-180):Mathf.Clamp(mini.pos.y-124,0,SelvaMiniWorld.Height-180);
            string biome=course.zones[zone];Layer(biome+"_capa1_cielo",0,0);Layer(biome+"_capa2_lejania",.15f,10);Layer("niebla",.08f,15);Layer(biome+"_capa3_estructura",.32f,20);Layer(biome+"_capa5_cercania",.72f,25);
            if(mini!=null)RenderMini();else RenderRunner();
            Layer("rayos_sol",.20f,90);Layer(biome+"_capa6_frontal",1.3f,95);
            for(int i=used;i<renderers.Count;i++)renderers[i].enabled=false;
            museum.Tick((paused||complete||dead)&&menuState.tab==1,PlayerPrefs.GetFloat("SuyuRun.Settings.MusicVol",1));
        }
        void RenderRunner()
        {
            bool river=vines.Count>0;
            if(river)Layer("agua_fondo",.30f,30,146);
            foreach(var s in surfaces)
            {
                if(s.bounds.xMax<cameraX-32||s.bounds.xMin>cameraX+352)continue;
                if(s.art=="victoria")Draw("plataforma_victoria",s.bounds.x-cameraX,s.bounds.y,50);
                // Anchor the tile grid to the platform, never to the moving camera.
                else for(float x=s.bounds.x+Mathf.Floor((Mathf.Max(s.bounds.x,cameraX-32)-s.bounds.x)/32)*32;x<Mathf.Min(s.bounds.xMax,cameraX+352);x+=32)
                    Draw("tile_"+s.art+"_superficie",x-cameraX,s.bounds.y,50);
            }
            if(river)
            {
                foreach(var v in vines)
                {
                    if(v.anchor.x<cameraX-100||v.anchor.x>cameraX+400)continue;
                    // Temple foreground foliage must not hide the new playable knots.
                    Draw("decor_rama_anclaje",v.anchor.x-cameraX-32,0,zone==5?96:51);
                    Paint(Rope(v),v.anchor.x-cameraX-64,v.anchor.y-3,zone==5?97:52);
                    if(zone==5)
                    {
                        var knot=SelvaArt.Get("fx_destello").spec;
                        Draw("fx_destello",v.Grip.x-cameraX-knot.frameWidth/2f,v.Grip.y-knot.frameHeight/2f,98,Mathf.Repeat(clock+v.anchor.x*.001f,knot.frames/(float)knot.fps));
                    }
                }
                foreach(var c in caimans)
                {
                    if(c.pos.x<cameraX-100||c.pos.x>cameraX+420)continue;
                    if(c.phase==SelvaCaiman.Phase.Hidden)continue;
                    if(c.phase==SelvaCaiman.Phase.Telegraph)
                    {Draw("caiman_sombra",c.pos.x-cameraX-32,151,39);Draw("fx_burbujas",c.pos.x-cameraX-16,121,60,c.timer);Draw("fx_ondas",c.pos.x-cameraX-32,138,65,c.timer);}
                    else Draw("caiman_"+c.Animation,c.Origin.x-cameraX,c.Origin.y,40,c.timer,c.left,c.Frame);
                }
                foreach(var pond in caimans)if(pond.maxX+3>cameraX-64&&pond.maxX+3<cameraX+350)
                    Draw("cascada",pond.maxX+3-cameraX,50,36);
                foreach(var d in drops)Draw("gota_cueva",d.pos.x-cameraX,d.pos.y,63);
            }
            foreach(var e in course.events)
            {
                if(!CourseEventActive(e)||e.x<cameraX-64||e.x>cameraX+350)continue;
                int index=System.Array.IndexOf(course.events,e);if(damagedEvents.Contains(index))continue;
                if(e.kind=="double")Draw("fx_destello",e.x-cameraX-16,86,61);
                else Draw(e.kind=="slide"?"obstaculo_techo":e.kind=="root"?"obstaculo_raiz":e.kind=="air"?"guacamayo_movimiento":e.kind=="estaca"?"obstaculo_estaca":e.kind=="trampa"?"obstaculo_trampa":"obstaculo_pinchos",e.x-cameraX-12,e.kind=="air"?90+Mathf.Sin(clock*3+e.beat)*5:e.kind=="slide"?99:116,55,left:e.kind=="air"&&RunnerFacing.TowardLeft(e.x,pos.x));
            }
            for(int i=0;i<4;i++)
            {float x=zoneStart+800+i*900;if(!pickups.Contains(zone*100+i))Draw("coleccionable_"+((zone==0&&i==0)?"ceramica":zone==3&&i==3?"sonaja":"moneda"),x-cameraX-8,112,60);}
            string state=complete||endingAt>=0?"baile":dead?"death":clock-hurtAt<.25f?"hit":hanging!=null?"swing":clock-releaseAt<.25f?"release":slide?"slide":!grounded?velocity.y<0?jumps>1?"double_jump":"jump":"fall":clock-fireAt<.18f?"burst":clock-landAt<.14f?"land":"run";
            int pose=hanging==null?-1:TaytaArt.SwingPose(hanging.angle*Mathf.Rad2Deg);
            if(TaytaArt.UseUniformBank&&hanging==null&&!grounded&&clock-fireAt<.18f)state+="_shoot";
            var attachment=hanging!=null?TaytaArt.GripOffset(state,pose):Vector2.zero;
            float heroX=hanging!=null?hanging.Grip.x-cameraX-attachment.x:pos.x-cameraX-24;
            float heroY=hanging!=null?hanging.Grip.y-attachment.y:pos.y-48;
            HeroSprite(state,heroX,heroY,HeroFrameTime(state,clock),false,pose);
            if(zone==5&&endingAt>=0&&finishAfterFade)heroDisplay.sprite=TaytaArt.Frame("selva","run",closing*.5f)??heroDisplay.sprite;
            if(zone==5&&endingAt>=0&&finishAfterFade)RunnerPortalIngress.Apply(heroDisplay,new Vector3((FinalPortalX-cameraX)/32,(180-148+40)/32f,0),closing/1.4f);
            // Suit protection remains; octagonal visual overlay removed.
            foreach(var s in shots)Draw("fx_disparo",s.pos.x-cameraX-4,s.pos.y-4,62,s.age);
            // Foreground surface naturally obscures both body and tint as it descends, never pop-hide.
            if(river)WaterSurface();
            foreach(var f in effects)
            {
                var spec=SelvaArt.Get("fx_"+f.name).spec;float age=clock-f.born;if(age>spec.frames/(float)spec.fps)continue;
                Draw("fx_"+f.name,f.pos.x-cameraX-spec.frameWidth/2,f.pos.y-(f.name=="splash_salida"?43:f.name=="splash_entrada"?32:spec.frameHeight/2),67,age);
            }
            for(int i=0;i<3;i++)Draw(zone==3?"luciernaga_movimiento":"guacamayo_movimiento",Mathf.Repeat(i*143-clock*(8+i*3),390)-35,32+i*13+Mathf.Sin(clock*2+i)*3,28,clock+i,left:zone!=3);
            if(zone==5){var portal=SelvaArt.Get("portal_salida").spec;Draw("portal_salida",FinalPortalX-cameraX-portal.frameWidth/2,148-portal.frameHeight,56);RunnerPortalIngress.Frame(renderers[used-1],58);}
        }
        void WaterSurface()
        {
            var sheet=SelvaArt.Get("agua_superficie");
            int f=(int)(clock*sheet.spec.fps)%sheet.spec.frames;
            var detailed=RefactorMaterialArt.Frame("selva_agua_superficie",sheet.frames[f],clock,f);
            if(!waterMask)
            {
                waterAtlas=detailed.texture;waterDetail=RefactorActorArt.IsRegistered(detailed)?Mathf.RoundToInt(detailed.pixelsPerUnit/32):1;
                waterSource=waterAtlas.GetPixels32();waterPixels=new Color32[320*34*waterDetail*waterDetail];
                waterMask=new Texture2D(320*waterDetail,34*waterDetail,TextureFormat.RGBA32,false);waterMask.filterMode=FilterMode.Point;
                waterMaskSprite=Sprite.Create(waterMask,new Rect(0,0,waterMask.width,waterMask.height),new Vector2(0,1),32*waterDetail,0,SpriteMeshType.FullRect);
                if(waterDetail>1)RefactorActorArt.Register(waterMaskSprite,new Vector2(320,34));
            }
            var frame=detailed.rect;int width=320*waterDetail,height=34*waterDetail;
            for(int x=0;x<width;x++)
            {
                float logicalX=x/(float)waterDetail;
                var ground=SurfaceAt(cameraX+logicalX);bool dry=ground!=null&&ground.art!="victoria";int sx=(int)(Mathf.Repeat(cameraX*.3f+logicalX,640)*waterDetail);
                for(int y=0;y<height;y++)waterPixels[(height-1-y)*width+x]=dry?new Color32():waterSource[((int)frame.y+180*waterDetail-1-y)*waterAtlas.width+(int)frame.x+sx];
            }
            waterMask.SetPixels32(waterPixels);waterMask.Apply(false);Paint(waterMaskSprite,0,146,64);
        }
        Sprite Rope(SelvaVine v)
        {
            if(!ropeTextures.TryGetValue(v,out var r)){r=new VineTexture{texture=new Texture2D(288,240,TextureFormat.RGBA32,false)};r.texture.filterMode=FilterMode.Point;r.texture.wrapMode=TextureWrapMode.Clamp;r.sprite=Sprite.Create(r.texture,new Rect(0,0,288,240),new Vector2(0,1),64,0,SpriteMeshType.FullRect);RefactorActorArt.Register(r.sprite,new Vector2(144,120));ropeTextures.Add(v,r);}
            System.Array.Clear(r.pixels,0,r.pixels.Length);
            var dark=new Color32(48,37,31,255);var bark=new Color32(107,77,50,255);var light=new Color32(212,167,44,255);
            for(float t=0;t<=1;t+=.0025f)
            {Vector2 p=v.Curve(t)-v.anchor+new Vector2(64,3);int x=Mathf.RoundToInt(p.x*2),y=Mathf.RoundToInt(p.y*2);for(int dx=-2;dx<=2;dx++)SetPixel(r,x+dx,y,Mathf.Abs(dx)<2?bark:dark);if((int)(t*82)%13<2)SetPixel(r,x,y,light);}
            var g=v.Grip-v.anchor+new Vector2(64,3);for(int x=-6;x<=6;x++)for(int y=-6;y<=6;y++)if(x*x+y*y<=40)SetPixel(r,Mathf.RoundToInt(g.x*2)+x,Mathf.RoundToInt(g.y*2)+y,light);
            r.texture.SetPixels32(r.pixels);r.texture.Apply(false);return r.sprite;
        }
        void SetPixel(VineTexture r,int x,int y,Color32 c){if(x>=0&&x<288&&y>=0&&y<240)r.pixels[(239-y)*288+x]=c;}
        void RenderMini()
        {
            foreach(var p in mini.platforms)
            {
                if(p.bounds.xMax<cameraX-40||p.bounds.xMin>cameraX+360)continue;
                string tile=zone==2?"cueva":"ruina";
                for(float x=p.bounds.x;x<p.bounds.xMax;x+=32)
                {Draw("tile_"+tile+"_superficie",x-cameraX,p.bounds.y-cameraY,50);if(!p.oneWay)for(float y=p.bounds.y+32;y<384;y+=32)Draw("tile_"+tile+"_relleno",x-cameraX,y-cameraY,49);}
            }
            foreach(var r in mini.torchPlacements)Draw("antorcha",r.x-cameraX,r.y-cameraY,42);
            foreach(var foe in mini.foes)
            {
                string creature=foe.boss?(zone==2?"boss_raices_":"boss_chullachaqui_"):(zone==2?"esbirro_brote_":foe.serial%3==0?"esbirro_ecopiedra_":foe.serial%3==1?"esbirro_ecoinca_":"esbirro_eco_");
                string action=foe.hp<=0?"derrota":mini.time-foe.hurt<.25?"hit":foe.boss?(foe.phase==0?foe.awakened&&foe.timer<.6f?"despertar":"idle":foe.phase==1?"anticipar":foe.phase==2?"golpe":foe.phase==3?"idle":"andar"):(foe.phase==0?"idle":"caminar");
                if(foe.hp<=0&&mini.time-foe.hurt>1)continue;
                float height=foe.boss?(zone==4?96:84):SelvaArt.Get(creature+action).spec.frameHeight;
                float poseAge=foe.hp<=0||action=="hit"?Mathf.Max(0,mini.time-foe.hurt):foe.timer;
                bool facingLeft=RunnerFacing.TowardLeft(foe.pos.x,mini.pos.x);
                Draw(creature+action,foe.pos.x-cameraX-(foe.boss?48:16),foe.pos.y-cameraY-height,55,poseAge,facingLeft,-1,foe.boss?foe.attack:-1);
            }
            string state=mini.dead?"death":mini.time-mini.damageAt<.25?"hit":mini.Cursed&&mini.time<mini.guardUntil?"guard":mini.Cursed&&mini.time-mini.meleeAt<.4f?mini.meleeState:!mini.Cursed&&mini.grounded&&mini.time-mini.fireAt<.2?"burst":mini.grounded?Mathf.Abs(mini.velocity.x)>1?"run":!mini.Cursed&&mini.Reloading?"reload":"idle":mini.velocity.y<0?mini.jumps>1?"double_jump":"jump":"fall";
            if(TaytaArt.UseUniformBank&&!mini.grounded&&!mini.Cursed&&mini.time-mini.fireAt<.2f)state+="_shoot";
            HeroSprite(state,mini.pos.x-cameraX-24,mini.pos.y-cameraY-48,HeroFrameTime(state,mini.time),!mini.facingRight);
            if(mini.exitCountdown>2.15f){heroDisplay.flipX=false;heroDisplay.sprite=TaytaArt.Frame("selva","run",mini.exitCountdown*.5f)??heroDisplay.sprite;}
            if(mini.exitCountdown>0)RunnerPortalIngress.Apply(heroDisplay,new Vector3((SelvaMiniWorld.PortalX+24-cameraX)/32,(180-SelvaMiniWorld.PortalFloor+cameraY+40)/32f,0),(mini.exitCountdown-2.15f)/.85f);
            foreach(var h in mini.hazards){float age=mini.time-h.born;Draw(age<.85f?"fx_raiz_aviso":zone==4?"fx_maldicion":h.trap?"fx_raiz_amarre":"fx_raiz_erupcion",h.pos.x-cameraX-32,h.pos.y-cameraY-60,64,age);}
            if(mini.time<mini.rootedUntil)Draw("fx_raiz_amarre",mini.pos.x-cameraX-32,mini.pos.y-cameraY-60,65,mini.time);
            if(mini.Cursed&&mini.Cinematic)Draw("fx_maldicion",mini.pos.x-cameraX-32,mini.pos.y-cameraY-64,66,mini.Boss.timer);
            foreach(var b in mini.bullets)Draw(b.hostile?"fx_veneno":"fx_disparo",b.pos.x-cameraX-4,b.pos.y-cameraY-4,60,b.age);
            foreach(var s in mini.sparks)Draw("fx_impacto",s.pos.x-cameraX-16,s.pos.y-cameraY-16,65,mini.time-s.born);
            foreach(var p in mini.treasures)Draw("coleccionable_moneda",p.x-cameraX-8,p.y-cameraY-8,54);
            var portalSpec=SelvaArt.Get("portal_salida").spec;
            Draw("portal_salida",SelvaMiniWorld.PortalX+24-cameraX-portalSpec.frameWidth*.5f,SelvaMiniWorld.PortalFloor-portalSpec.frameHeight-cameraY,56);
            RunnerPortalIngress.Frame(renderers[used-1],58);
        }
        GUIStyle Label(int size=5)=>new GUIStyle(GUI.skin.label){font=pixelFont,fontSize=size,wordWrap=true,normal={textColor=new Color32(255,240,174,255)}};
        void OnGUI()
        {
            if(RunnerFinalMovie.Active)return;
            var saved=GUI.matrix;
            GUI.matrix=RunnerHud.ScreenMatrix;
            if(paused||dead||complete)
            {
                DrawSelvaSharedMenu();
            }
            else if(mini!=null&&mini.Cinematic)
            {
                RunnerPresentation.Box(new Rect(0,0,320,20),new Color32(10,14,42,255));
                RunnerHud.Text(new Rect(10,5,300,12),zone==2?"GUARDIAN DE RAICES":"CHULLACHAQUI · GUARDIAN DEL BOSQUE",Label(6));
                SharedGameMenu.Frame(new Rect(5,137,310,39),true);
                RunnerHud.Paragraph(new Rect(14,144,292,25),zone==2?
                    "Un guardian de madera despierta bajo la selva. Sus raices brotan y atrapan a los intrusos. Observa sus avisos antes de atacar.":
                    "Guardian del bosque en la tradicion amazonica. En esta ficcion sella tus armas y convoca ecos de tus rivales. Lucha con punos y patadas.",Label(4));
                if(transition>0)RunnerPresentation.Curtain(transition);
            }
            else
            {
                RunnerHud.Chrome(RunnerHud.Footer);
                RunnerHud.Progress(routeTime/(soundtrack.clip.length-soundtrack.clipStartSeconds));
                RunnerHud.Chrome(RunnerHud.Header);
                if(mini!=null)RunnerHud.CombatStatus(health,maxHealth,mini.ammo,Mathf.Max(0,mini.reloadUntil-mini.time),mini.Cursed);
                if(mini==null)RunnerHud.CombatStatus(health,maxHealth,ammo,reloadAt>=0?Mathf.Max(0,reloadAt-clock):0);
                RunnerHud.Text(RunnerHud.CoinCounter,$"MON {coins+(mini?.coins??0):00}",Label(4));
                RunnerHud.Text(RunnerHud.ScoreCounter,$"PTS {score:0000}",Label(4));
                RunnerHud.ControlHints(RunnerHud.Controls,mini!=null?mini.Cursed?"A/D · ESP x2 · E golpe · F patada · Q upper · SHIFT":"A/D · ESP x2 · S bajar · E disparar · ESC pausa":"ESP x2 · S deslizar · CLICK liana · E disparar · ESC pausa",Label(4));
                if(vines.Count>0&&pos.x>=VineRouteStart&&pos.x<VineRouteEnd)RunnerHud.NoticeText(hanging!=null?"ESP: SOLTAR · CLICK SIGUIENTE NUDO PARA ENLAZAR":"CLICK EN NUDO BRILLANTE PARA COLGARTE",Label(4));
                if(mini!=null)
                {
                    if(mini.Boss.awakened){RunnerPresentation.Box(new Rect(84,28,152,5),new Color32(24,41,32,255));RunnerPresentation.Box(new Rect(85,29,150*mini.Boss.hp/(float)mini.Boss.maxHp,3),new Color32(205,112,61,255));}
                    RunnerHud.Text(new Rect(243,28,73,10),mini.time>=60?"COMBATE":"RITUAL "+Mathf.CeilToInt(60-mini.time)+"s",Label(4));
                    if(mini.time>=60&&!mini.ExitReady)RunnerHud.Paragraph(new Rect(26,122,284,16),"DERROTA A LOS ENEMIGOS PENDIENTES",Label(4));
                    if(mini.AtPortal)RunnerHud.Text(RunnerHud.Message,mini.ExitReady?"SALIDA EN "+Mathf.CeilToInt(3-mini.exitCountdown)+"s":mini.Boss.hp==0&&mini.RemainingMinions==0?"RITUAL TERMINA EN "+Mathf.CeilToInt(60-mini.time)+"s":$"FALTAN {mini.RemainingMinions} ESBIRROS"+(mini.Boss.hp>0?" Y EL GUARDIAN":""),Label(4));
                }
                for(int i=0;i<4;i++){if(!RunnerHud.HasCarvedArt)SharedGameMenu.Frame(RunnerHud.PieceSlot(i),true);MuseumArt.DrawHud(RunnerHud.PieceIcon(i),SharedGameMenu.SelvaIds[i],SharedGameMenu.IsCollected(2,i));}
                if(transition>0)RunnerPresentation.Curtain(transition);
            }
            GUI.matrix=saved;
        }
        void OnDestroy(){foreach(var r in ropeTextures.Values){Destroy(r.sprite);Destroy(r.texture);}if(waterMaskSprite)Destroy(waterMaskSprite);if(waterMask)Destroy(waterMask);if(spriteMaterial)Destroy(spriteMaterial);}
    }
}
