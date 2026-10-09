using System;
using System.Collections.Generic;
using UnityEngine;
namespace SuyuRun
{
    // Shared 320x180 safe zones. Costa's GUI uses four times these coordinates.
    public static class RunnerHud
    {
        // Match PixelPerfectCamera.StretchFill's aspect-preserving viewport.
        public static Rect Viewport(float width,float height)
        {
            float scale=Mathf.Min(width/320f,height/180f);
            return new Rect((width-320*scale)/2,(height-180*scale)/2,320*scale,180*scale);
        }
        public static float ScreenScale=>Viewport(Screen.width,Screen.height).width/320f;
        public static Matrix4x4 ScreenMatrix=>Matrix4x4.TRS(new Vector3((Screen.width-320*ScreenScale)/2,(Screen.height-180*ScreenScale)/2,0),Quaternion.identity,new Vector3(ScreenScale,ScreenScale,1));
        public static Vector2 Pointer(Vector2 screen)=>new Vector2((screen.x-(Screen.width-320*ScreenScale)/2)/ScreenScale,(Screen.height-screen.y-(Screen.height-180*ScreenScale)/2)/ScreenScale);
        public static Rect PieceSlotFor(string region,int index)=>new Rect(region=="sierra"?216+22*index:214+22.5f*index,162,20,15);
        public static Rect PieceSlot(int index)=>PieceSlotFor(Region,index);
        public static Rect PieceIcon(int index){var r=PieceSlot(index);return new Rect(r.center.x-5,r.center.y-5,10,10);}
        public static Rect Message=>new Rect(14,147,192,11);
        public static Rect Footer=>new Rect(4,160,312,19);
        public static Rect Header=>new Rect(4,4,312,20);
        public static Rect ControlsFor(string region)=>new Rect(22,165,178,9);
        public static Rect Controls=>ControlsFor(Region);
        public static Rect CoinCounter=>new Rect(219,10,Region=="sierra"?29:24,8);
        public static Rect ScoreCounter=>new Rect(272,10,26,8);
        static Rect LifeTrough=>Region=="costa"?new Rect(74,12,36,6):Region=="sierra"?new Rect(71,12,44,6):new Rect(68,12,46,6);
        static Rect WeaponTrough=>Region=="costa"?new Rect(145,11,20,8):Region=="sierra"?new Rect(148,11,15,8):new Rect(148,11,18,8);
        public static Rect Notice=>new Rect(88,26,224,23);
        public static void Paragraph(Rect rect,string value,GUIStyle source)
        {
            var style=Centered(source);style.wordWrap=true;style.fontSize=Mathf.Max(3,source.fontSize);
            while(style.fontSize>3&&style.CalcHeight(new GUIContent(value),rect.width-8)>rect.height-4)style.fontSize--;
            style.padding=new RectOffset(4,4,2,2);
            GUI.Label(rect,value,style);
        }
        public static void NoticeText(string value,GUIStyle source)
        {
            var style=Centered(source);style.fontSize=4;style.wordWrap=true;
            float width=Mathf.Clamp(style.CalcSize(new GUIContent(value)).x+12,70,224);
            float height=Mathf.Clamp(style.CalcHeight(new GUIContent(value),width-8)+6,12,23);
            var r=new Rect(Mathf.Round(160-width/2),27,Mathf.Ceil(width),Mathf.Ceil(height));
            RunnerPresentation.Box(r,RunnerPresentation.Ink);
            Paragraph(r,value,style);
        }
        public static void RunnerStatus(int health,int maximum,float energy,int stones=-1)
        {
            StatusModule(health,maximum,stones>=0?"sling":"gun",stones>=0?stones.ToString():Mathf.RoundToInt(energy*100)+"%",energy,false);
        }
        public static bool HasCarvedArt=>Resources.Load<Texture2D>("HudCarved/costa_header")!=null;
        public static void CombatStatus(int health,int maxHealth,int ammo,float reloadSeconds,bool sealedWeapons=false)
        {
            StatusModule(health,maxHealth,"gun",sealedWeapons?"X":reloadSeconds>0?reloadSeconds.ToString("F1")+"s":ammo+"/5",reloadSeconds>0?1-reloadSeconds/4f:ammo/5f,sealedWeapons);
        }
        static readonly Dictionary<string,Texture2D> statusTextures=new Dictionary<string,Texture2D>();
        static readonly Dictionary<string,Texture2D> unifiedTextures=new Dictionary<string,Texture2D>();
        static string Region
        {
            get {string scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;return scene.IndexOf("selva",StringComparison.OrdinalIgnoreCase)>=0?"selva":scene.IndexOf("sierra",StringComparison.OrdinalIgnoreCase)>=0?"sierra":"costa";}
        }
        static Texture2D Unified(string state)
        {
            string key=Region+"_"+state;
            if(!unifiedTextures.TryGetValue(key,out var art))
            {art=(state=="footer"?Resources.Load<Texture2D>("UnifiedHud/"+key+"_clean"):null)??Resources.Load<Texture2D>("UnifiedHud/"+key);if(art){art.filterMode=FilterMode.Point;art.wrapMode=TextureWrapMode.Clamp;}unifiedTextures[key]=art;}
            return art;
        }
        static GUIStyle statusNumbers;
        static void StatusModule(int health,int maximum,string weapon,string number,float charge,bool sealedWeapons)
        {
            string scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            string region=scene.IndexOf("selva",StringComparison.OrdinalIgnoreCase)>=0?"selva":scene.IndexOf("sierra",StringComparison.OrdinalIgnoreCase)>=0?"sierra":"costa";
            string key=region+"_"+weapon;
            if(!statusTextures.TryGetValue(key,out var texture))
            {
                texture=Resources.Load<Texture2D>("HudStatus/"+key);
                if(texture){texture.filterMode=FilterMode.Point;texture.wrapMode=TextureWrapMode.Clamp;}
                statusTextures[key]=texture;
            }
            var module=new Rect(48,6,142,16);
            var integrated=Unified("header_"+weapon);
            if(integrated)GUI.DrawTexture(Header,integrated,ScaleMode.StretchToFill,true);
            else if(texture)GUI.DrawTexture(module,texture,ScaleMode.StretchToFill,true);
            else RunnerPresentation.Box(module,RunnerPresentation.Ink);
            // Fill only the artist's recessed trough; icons and engraved edges
            // remain untouched. The shared module has identical regional geometry.
            float fill=53*Mathf.Clamp01(health/(float)Mathf.Max(1,maximum));
            if(fill>0)
            {
                var trough=integrated?LifeTrough:new Rect(69,11,53,6);
                float left=trough.x,width=trough.width;fill=width*Mathf.Clamp01(health/(float)Mathf.Max(1,maximum));
                RunnerPresentation.Box(new Rect(left,trough.y,fill,trough.height),new Color32(139,48,46,255));
                RunnerPresentation.Box(new Rect(left,trough.y,fill,1),new Color32(244,162,111,255));
                RunnerPresentation.Box(new Rect(left,trough.y+1,fill,2),new Color32(220,91,68,255));
                for(int i=1;i<maximum;i++)RunnerPresentation.Box(new Rect(Mathf.Round(left+width*i/maximum),trough.y,1,trough.height),RunnerPresentation.Ink);
            }
            if(statusNumbers==null)statusNumbers=new GUIStyle(GUI.skin.label){fontSize=4,normal={textColor=RunnerPresentation.Gold}};
            Text(integrated?LifeTrough:new Rect(71,9,49,10),Mathf.Max(0,health)+"/"+maximum,statusNumbers);
            var weaponRect=integrated?WeaponTrough:new Rect(164,8,22,9);
            Text(weaponRect,number,statusNumbers);
            RunnerPresentation.Box(new Rect(weaponRect.x,weaponRect.yMax,weaponRect.width,1),RunnerPresentation.Ink);
            RunnerPresentation.Box(new Rect(weaponRect.x,weaponRect.yMax,weaponRect.width*Mathf.Clamp01(charge),1),sealedWeapons?RunnerPresentation.Stone:RunnerPresentation.Gold);
            // Drawing the unified header includes its numeric compartments;
            // callers add coin and point text AFTER this method.
        }
        public static GUIStyle Centered(GUIStyle source)=>new GUIStyle(source){font=Resources.Load<Font>("PressStart2P-Regular")??source.font,alignment=TextAnchor.MiddleCenter,wordWrap=false,clipping=TextClipping.Clip,padding=new RectOffset(0,0,0,0)};
        public static void Text(Rect rect,string value,GUIStyle source)
        {
            var style=Centered(source);
            style.fontSize=Mathf.Max(1,style.fontSize);
            while(style.fontSize>1&&(style.CalcSize(new GUIContent(value)).x>rect.width-2||style.CalcSize(new GUIContent(value)).y>rect.height))style.fontSize--;
            float pixel=style.fontSize>=12?4:1;
            var shadow=new GUIStyle(style);shadow.normal.textColor=RunnerPresentation.Ink;
            GUI.Label(new Rect(rect.x,rect.y+pixel,rect.width,rect.height),value,shadow);
            GUI.Label(rect,value,style);
        }
        // Real keycaps, laid out as a centred group rather than a baked sentence.
        public static void ControlHints(Rect rect,string value,GUIStyle source)
        {
            float p=rect.width/194f;
            var style=Centered(source);style.fontSize=Mathf.Max(1,Mathf.RoundToInt(3*p));
            var groups=value.Split(new[]{" · "},StringSplitOptions.RemoveEmptyEntries);
            float Total(){float width=0;foreach(var g in groups){int n=g.IndexOf(' ');string key=n<0?g:g.Substring(0,n),action=n<0?"":g.Substring(n+1);width+=style.CalcSize(new GUIContent(key)).x+6*p+style.CalcSize(new GUIContent(action)).x+7*p;}return width-5*p;}
            while(style.fontSize>1&&Total()>rect.width)style.fontSize--;
            float x=rect.center.x-Total()/2;
            foreach(var group in groups)
            {
                int split=group.IndexOf(' ');string key=split<0?group:group.Substring(0,split),action=split<0?"":group.Substring(split+1);
                float w=style.CalcSize(new GUIContent(key)).x+6*p;
                var cap=new Rect(Mathf.Round(x),Mathf.Round(rect.center.y-3.5f*p),Mathf.Round(w),7*p);
                RunnerPresentation.Box(cap,RunnerPresentation.Ink);
                RunnerPresentation.Box(new Rect(cap.x+p,cap.y+p,cap.width-2*p,cap.height-2*p),new Color32(102,83,54,255));
                RunnerPresentation.Box(new Rect(cap.x+p,cap.y+p,cap.width-2*p,p),RunnerPresentation.Gold);
                RunnerPresentation.Box(new Rect(cap.x+p,cap.y+p,p,cap.height-2*p),RunnerPresentation.Stone);
                RunnerPresentation.Box(new Rect(cap.x+2*p,cap.yMax-2*p,cap.width-3*p,p),new Color32(57,40,31,255));
                Text(new Rect(cap.x+p,cap.y,cap.width-2*p,cap.height-p),key,style);
                x+=w+2*p;float a=style.CalcSize(new GUIContent(action)).x;
                Text(new Rect(x,rect.y,a+2*p,rect.height),action,style);x+=a+5*p;
            }
        }
        public static void Chrome(Rect rect)
        {
            string scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.ToLowerInvariant();
            string region=scene.Contains("selva")?"selva":scene.Contains("sierra")?"sierra":"costa";
            bool footer=rect.y>rect.height*2;
            var unified=Unified(footer?"footer":"header_gun");
            if(unified)
            {
                if(footer)GUI.DrawTexture(rect,unified,ScaleMode.StretchToFill,true);
                // Header is drawn once by StatusModule, with its actual weapon.
                return;
            }
            string artPath="HudCarved/"+region+(footer?"_footer":"_header");
            var art=Resources.Load<Texture2D>(artPath+"_v2")??Resources.Load<Texture2D>(artPath);
            if(art)
            {
                art.filterMode=FilterMode.Point;art.wrapMode=TextureWrapMode.Clamp;
                // Stretch only the empty interior for the taller cave combat HUD;
                // preserve the native carved top and bottom rails.
                GUI.DrawTextureWithTexCoords(rect,art,new Rect(0,0,1,1));
                return;
            }
            RunnerPresentation.Box(rect,RunnerPresentation.Ink);
            float p=Mathf.Max(1,Mathf.Round(rect.width/312));
            RunnerPresentation.Box(new Rect(rect.x+p,rect.y+p,rect.width-2*p,rect.height-2*p),RunnerPresentation.Ink);
            RunnerPresentation.Box(new Rect(rect.x+p,rect.y+p,rect.width-2*p,p),RunnerPresentation.Gold);
            RunnerPresentation.Box(new Rect(rect.x+p,rect.yMax-2*p,rect.width-2*p,p),RunnerPresentation.Stone);
            // Carved pixel trim: small corner plates and alternating bevels,
            // with a calm centre reserved for the numbers and control hints.
            var clay=new Color32(146,87,64,255);var light=new Color32(226,167,103,255);
            RunnerPresentation.Box(new Rect(rect.x+2*p,rect.y+2*p,rect.width-4*p,p),clay);
            RunnerPresentation.Box(new Rect(rect.x+2*p,rect.yMax-3*p,rect.width-4*p,p),clay);
            RunnerPresentation.Box(new Rect(rect.x+6*p,rect.y+3*p,rect.width-12*p,p),RunnerPresentation.Stone);
            RunnerPresentation.Box(new Rect(rect.x+6*p,rect.yMax-4*p,rect.width-12*p,p),RunnerPresentation.Ink);
            for(float x=rect.x+14*p;x<rect.xMax-12*p;x+=24*p)
            {
                RunnerPresentation.Box(new Rect(x,rect.y+p,p,p),light);
                RunnerPresentation.Box(new Rect(x+2*p,rect.yMax-3*p,2*p,p),RunnerPresentation.Gold);
            }
            foreach(float x in new[]{rect.x,rect.xMax-6*p})foreach(float y in new[]{rect.y,rect.yMax-6*p})
            {
                RunnerPresentation.Box(new Rect(x,y,6*p,6*p),RunnerPresentation.Ink);
                RunnerPresentation.Box(new Rect(x+p,y+p,4*p,4*p),clay);
                RunnerPresentation.Box(new Rect(x+p,y+p,4*p,p),light);
                RunnerPresentation.Box(new Rect(x+p,y+p,p,4*p),RunnerPresentation.Gold);
                RunnerPresentation.Box(new Rect(x+2*p,y+2*p,2*p,2*p),RunnerPresentation.Ink);
                RunnerPresentation.Box(new Rect(x+3*p,y+3*p,p,p),RunnerPresentation.Gold);
            }
        }
        public static void Progress(float progress)
        {
            progress=Mathf.Clamp01(progress);
            RunnerPresentation.Box(new Rect(5,0,310,3),RunnerPresentation.Ink);
            RunnerPresentation.Box(new Rect(6,.5f,308*progress,2),RunnerPresentation.Gold);
            var style=new GUIStyle(GUI.skin.label){font=Resources.Load<Font>("PressStart2P-Regular"),fontSize=3,alignment=TextAnchor.MiddleCenter,padding=new RectOffset(),normal={textColor=Color.white}};
            RunnerPresentation.Box(new Rect(143,0,34,4),RunnerPresentation.Ink);
            GUI.Label(new Rect(143,0,34,4),Mathf.FloorToInt(progress*100)+"%",style);
        }
        public static void Draw(float scale,int coins,int score,GUIStyle text,Func<int,bool> found,
            Action<Rect> panel,Action<Rect> slot,Action<Rect> coin,Action<Rect> points,Color highlight,Action<int,Rect> piece=null)
        {
            Rect R(float x,float y,float w,float h)=>new Rect(Mathf.Round(x*scale),Mathf.Round(y*scale),Mathf.Round(w*scale),Mathf.Round(h*scale));
            Chrome(R(16,16,1248,80));
            var footer=Footer;
            Chrome(R(footer.x*4,footer.y*4,footer.width*4,footer.height*4));
            var numeric=Centered(text);
            for(int i=0;i<4;i++)
            {
                var s=PieceSlot(i);var icon=PieceIcon(i);
                if(!HasCarvedArt)slot(R(s.x*4,s.y*4,s.width*4,s.height*4));
                if(piece!=null)piece(i,R(icon.x*4,icon.y*4,icon.width*4,icon.height*4));
                else if(found(i)){var saved=GUI.color;GUI.color=highlight;coin(R(icon.x*4,icon.y*4,icon.width*4,icon.height*4));GUI.color=saved;}
            }
            // StatusModule draws the integral header after this call. Counters
            // are redrawn by the controller once that skin is in place.
            if(!Unified("header_gun"))
            {
                Text(R(860,36,128,40),$"MON {coins:00}",new GUIStyle(numeric){fontSize=Mathf.Max(1,Mathf.RoundToInt(16*scale))});
                Text(R(1060,36,140,40),$"PTS {score:0000}",new GUIStyle(numeric){fontSize=Mathf.Max(1,Mathf.RoundToInt(16*scale))});
            }
        }
        public static void Panel(Rect rect,Sprite sprite,float pixelScale=1)
        {
            if(!sprite)return;
            var s=sprite.rect;var t=sprite.texture;
            float ratio=RefactorActorArt.IsRegistered(sprite)?s.width/RefactorActorArt.Size(sprite).x:1;
            float border=Mathf.Min(2*ratio,Mathf.Min(s.width,s.height)/3),b=border/ratio*pixelScale;
            float[] x={rect.x,rect.x+b,rect.xMax-b,rect.xMax};
            float[] y={rect.y,rect.y+b,rect.yMax-b,rect.yMax};
            float[] u={s.x/t.width,(s.x+border)/t.width,(s.xMax-border)/t.width,s.xMax/t.width};
            float[] v={s.yMax/t.height,(s.yMax-border)/t.height,(s.y+border)/t.height,s.y/t.height};
            for(int row=0;row<3;row++)for(int col=0;col<3;col++)
                GUI.DrawTextureWithTexCoords(new Rect(x[col],y[row],x[col+1]-x[col],y[row+1]-y[row]),t,new Rect(u[col],v[row+1],u[col+1]-u[col],v[row]-v[row+1]));
        }
        public static void Sprite(Rect rect,Sprite sprite)
        {
            if(!sprite)return;
            Rect r=sprite.rect;Texture2D t=sprite.texture;
            GUI.DrawTextureWithTexCoords(rect,t,new Rect(r.x/t.width,r.y/t.height,r.width/t.width,r.height/t.height));
        }
    }
}
