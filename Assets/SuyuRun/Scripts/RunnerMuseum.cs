using UnityEngine;
using UnityEngine.SceneManagement;

namespace SuyuRun
{
    public sealed partial class RunnerPrototype
    {
        readonly bool[] discovered=new bool[4];
        readonly Transform[] museumDisplays=new Transform[4];
        int museumSelection;
        GUIStyle bodyStyle;
        int DiscoveredCount {get{int n=0;foreach(bool value in discovered)if(value)n++;return n;}}
        void BuildMuseumDisplays()
        {
            for(int i=0;i<4;i++)
            {
                discovered[i]=PlayerPrefs.GetInt("SuyuRun.Museum.Costa."+CoastalCulture.Entries[i].id,0)==1;
                museumDisplays[i]=BuildCultureVisual(i,null);museumDisplays[i].position=new Vector3(-4.6f,0,0);museumDisplays[i].localScale=Vector3.one*3;
                museumDisplays[i].gameObject.AddComponent<UnityEngine.Rendering.SortingGroup>().sortingOrder=30;
                museumDisplays[i].gameObject.SetActive(false);
            }
        }
        Transform BuildCultureVisual(int index,Transform parent)
        {
            var root=new GameObject("Cultural memory - "+CoastalCulture.Entries[index].name).transform;root.SetParent(parent,false);
            if(index==0)
            {
                var clay=new Color(.77f,.39f,.2f);
                Shape("Portrait ceramic body",root,new Vector2(0,-.05f),new Vector2(.65f,.67f),clay,16,0,8);
                Rect("Ceramic base",root,0,-.34f,.43f,.1f,clay,8);
                Rect("Stirrup handle left",root,-.17f,.37f,.08f,.25f,clay,8);Rect("Stirrup handle right",root,.17f,.37f,.08f,.25f,clay,8);
                Rect("Stirrup bridge",root,0,.47f,.4f,.09f,clay,8);Rect("Spout",root,0,.59f,.11f,.21f,clay,8);
                Rect("Headband",root,0,.16f,.58f,.12f,cream,9);
                Rect("Portrait brow left",root,-.12f,.025f,.13f,.04f,shadowStone,9);Rect("Portrait brow right",root,.12f,.025f,.13f,.04f,shadowStone,9);
                Shape("Modeled nose",root,new Vector2(0,-.06f),new Vector2(.13f,.21f),sand,3,270,9);
                Rect("Portrait mouth",root,0,-.2f,.18f,.035f,shadowStone,9);
            }
            else if(index==1)
            {
                Rect("Woven mantle",root,0,0,.76f,.87f,new Color(.55f,.17f,.18f),8);
                Rect("Textile upper border",root,0,.33f,.75f,.08f,gold,9);Rect("Textile lower border",root,0,-.33f,.75f,.08f,gold,9);
                for(int row=0;row<3;row++)for(int col=0;col<3;col++)Shape("Original woven geometric stitch",root,new Vector2((col-1)*.22f,(row-1)*.22f),Vector2.one*.14f,(row+col)%2==0?cream:gold,4,0,10);
                for(int i=0;i<6;i++)Rect("Textile fringe",root,-.32f+i*.125f,-.48f,.035f,.12f,gold,9);
            }
            else
            {
                Shape("Memory portrait medallion",root,Vector2.zero,Vector2.one*.95f,gold,32,0,8);
                Shape("Portrait field",root,Vector2.zero,Vector2.one*.8f,new Color(.06f,.27f,.32f),32,0,9);
                Color skin=index==2?new Color(.4f,.24f,.16f):new Color(.82f,.63f,.45f);
                Shape("Illustrated shoulders",root,new Vector2(0,-.22f),new Vector2(.58f,.4f),index==2?cream:new Color(.72f,.53f,.36f),5,90,10);
                Shape("Original interpreted portrait",root,new Vector2(.01f,.08f),new Vector2(.33f,.42f),skin,12,0,11);
                if(index==2){Shape("Hair silhouette",root,new Vector2(-.015f,.24f),new Vector2(.36f,.18f),shadowStone,12,0,12);Rect("Mustache",root,.045f,.015f,.16f,.04f,shadowStone,12);}
                else {Shape("Hat crown",root,new Vector2(0,.31f),new Vector2(.48f,.25f),cream,8,0,12);Rect("Hat brim",root,0,.23f,.64f,.07f,sand,13);}
                Rect("Eyes",root,.04f,.11f,.14f,.025f,shadowStone,13);
            }
            return root;
        }
        void DiscoverCulture(int index)
        {
            if(index<0||index>=4)return;
            bool fresh=!discovered[index];discovered[index]=true;score+=fresh?250:50;
            feedback=(fresh?"MEMORIA DESCUBIERTA: ":"MEMORIA: ")+CoastalCulture.Entries[index].name;feedbackUntil=Time.unscaledTime+3;
            if(!AutomatedSmokeTest){PlayerPrefs.SetInt("SuyuRun.Museum.Costa."+CoastalCulture.Entries[index].id,1);PlayerPrefs.Save();}
            Beep(1320,.14f);
        }
        void OpenMuseum(){music.Stop();mode=Mode.Museum;if(menuMusic&&!menuMusic.isPlaying)menuMusic.Play();}
        void UpdateMuseumDisplays(){for(int i=0;i<4;i++)if(museumDisplays[i])museumDisplays[i].gameObject.SetActive(mode==Mode.Museum&&museumSelection==i);}
        void Panel(Rect rect,Color color){GUI.color=color;GUI.DrawTexture(rect,Texture2D.whiteTexture);GUI.color=Color.white;}
        // A hitbox with nothing drawn - the real look is the pixel-art background already baked
        // into the screen texture. GUIStyle.normal.background=null still falls back to the skin's
        // default box in IMGUI, so this forces true transparency via GUI.color alpha instead.
        bool InvisibleButton(Rect r)
        {
            var prev=GUI.color;GUI.color=new Color(0,0,0,0);
            bool clicked=GUI.Button(r,"",GUI.skin.button);
            GUI.color=prev;
            return clicked;
        }
        void DrawTitleScreen()
        {
            if(legacyPrototype||!texInicio){DrawTitleScreenLegacyFallback();return;}
            DrawTex(new Rect(0,0,1280,720),texInicio);
            // Button hitboxes match costa_inicio.png's baked frames exactly (art coords x4 - the
            // art canvas is 320x180, GUI space here is 1280x720, a clean x4).
            if(InvisibleButton(new Rect(64,600,352,56)))Begin();
            GUI.Label(new Rect(64,600,352,56),"JUGAR",new GUIStyle(small){alignment=TextAnchor.MiddleCenter});
            if(InvisibleButton(new Rect(448,600,184,56)))mode=Mode.Selector;
            GUI.Label(new Rect(448,600,184,56),"NIVELES",new GUIStyle(small){alignment=TextAnchor.MiddleCenter});
            if(InvisibleButton(new Rect(648,600,184,56)))OpenMuseum();
            GUI.Label(new Rect(648,600,184,56),"MUSEO "+DiscoveredCount+"/4",new GUIStyle(small){alignment=TextAnchor.MiddleCenter});
            if(InvisibleButton(new Rect(848,600,184,56)))mode=Mode.Shop;
            GUI.Label(new Rect(848,600,184,56),"TIENDA",new GUIStyle(small){alignment=TextAnchor.MiddleCenter});
            if(InvisibleButton(new Rect(1048,600,184,56)))Application.Quit();
            GUI.Label(new Rect(1048,600,184,56),"SALIR",new GUIStyle(small){alignment=TextAnchor.MiddleCenter});
            // costa_inicio.png only baked 5 button frames (JUGAR/NIVELES/MUSEO/TIENDA/SALIR - TIENDA
            // took the slot the bible drew as AJUSTES), so Settings is reached through a plain text
            // link in the empty sky instead of a sixth pixel-art button. Flagged, not hidden.
            if(GUI.Button(new Rect(1180,16,84,28),"AJUSTES",new GUIStyle(small){fontSize=10}))mode=Mode.Settings;
        }
        void DrawTitleScreenLegacyFallback()
        {
            Panel(new Rect(35,55,580,610),new Color(.025f,.095f,.14f,.95f));
            GUI.Label(new Rect(75,87,520,40),"UN VIAJE POR NUESTRA MEMORIA",small);
            var logo=new GUIStyle(title){fontSize=36};GUI.Label(new Rect(68,145,550,120),"SUYU RUN",logo);
            GUI.Label(new Rect(78,270,460,85),"La costa tiene ritmo.\nSu historia también se descubre jugando.",text);
            if(GUI.Button(new Rect(78,381,485,62),"JUGAR COSTA · EL ALCATRAZ",button))Begin();
            if(GUI.Button(new Rect(78,460,485,52),"MUSEO DE COSTA   "+DiscoveredCount+" / 4",button))OpenMuseum();
            if(GUI.Button(new Rect(78,529,485,46),"PROTOTIPO ORIGINAL · 64 s",button))SceneManager.LoadScene("Prototype_Legacy");
            GUI.Label(new Rect(78,600,500,42),"Sierra y Selva · músicas guardadas, niveles en desarrollo",small);
            Panel(new Rect(710,586,520,70),new Color(.025f,.095f,.14f,.83f));GUI.Label(new Rect(730,603,490,50),"ESPACIO salta · S desliza · E libera la ráfaga",small);
        }
        void DrawSelector()
        {
            if(!texSelector){DrawTitleScreenLegacyFallback();return;}
            DrawTex(new Rect(0,0,1280,720),texSelector);
            GUI.Label(new Rect(40,20,1200,72),"SELECTOR DE NIVEL",new GUIStyle(title){fontSize=20});
            // Card rects match costa_selector.png's card_frame() layout (x4).
            int cardW=92*4,cardH=132*4,gap=8*4;
            int total=cardW*3+gap*2;
            int x0=(1280-total)/2,y0=(180-132)/2*4;
            if(InvisibleButton(new Rect(x0,y0,cardW,cardH)))Begin();
            GUI.Label(new Rect(x0,y0+cardH-4,cardW,28),"COSTA",new GUIStyle(small){alignment=TextAnchor.MiddleCenter});
            GUI.Label(new Rect(x0+cardW+gap,y0+cardH/2-16,cardW,32),"SIERRA\n(bloqueado)",new GUIStyle(small){alignment=TextAnchor.MiddleCenter,wordWrap=true});
            GUI.Label(new Rect(x0+2*(cardW+gap),y0+cardH/2-16,cardW,32),"SELVA\n(bloqueado)",new GUIStyle(small){alignment=TextAnchor.MiddleCenter,wordWrap=true});
            if(GUI.Button(new Rect(40,640,260,44),"VOLVER AL INICIO",button))ReturnToMenu();
        }
        void DrawMuseum()
        {
            if(bodyStyle==null)bodyStyle=new GUIStyle(text){wordWrap=true,fontSize=13};
            if(texMuseoPared)
            {
                // Tiled wall, native tile 32px at x4 GUI scale = 128px per tile.
                float tiles_x=1280f/128f,tiles_y=720f/128f;
                GUI.DrawTextureWithTexCoords(new Rect(0,0,1280,720),texMuseoPared,new Rect(0,0,tiles_x,tiles_y));
            }
            Panel(new Rect(20,20,1240,100),new Color(.025f,.095f,.14f,.8f));
            GUI.Label(new Rect(48,38,970,70),"MUSEO / COSTA",title);GUI.Label(new Rect(970,57,280,35),DiscoveredCount+" de 4 memorias",text);
            // Detail panel keeps costa_museo_panel.png's native 280x300 aspect instead of
            // stretching it into the old flat box's wider proportions; same top/bottom budget
            // as before (145..550) so it doesn't collide with the selector row below.
            Rect panelRect=new Rect(610,145,378,405);
            if(texMuseoPanel)DrawTex(panelRect,texMuseoPanel);else Panel(panelRect,new Color(.025f,.095f,.14f,.97f));
            var entry=CoastalCulture.Entries[museumSelection];
            GUI.Label(new Rect(panelRect.x+14,panelRect.y+6,panelRect.width-28,30),entry.name,new GUIStyle(title){fontSize=16});
            GUI.Label(new Rect(panelRect.x+14,panelRect.y+46,panelRect.width-28,32),entry.place,new GUIStyle(small){wordWrap=true});
            GUI.Label(new Rect(panelRect.x+14,panelRect.y+90,panelRect.width-28,255),discovered[museumSelection]?entry.description:entry.teaser+"\n\nRecoge su memoria en Costa para revelar la ficha completa.",bodyStyle);
            if(discovered[museumSelection]&&GUI.Button(new Rect(panelRect.x+14,panelRect.y+361,190,32),"CONSULTAR FUENTE",button))Application.OpenURL(entry.source);
            for(int i=0;i<4;i++)
            {
                Rect br=new Rect(40+i*304,575,292,54);
                if(texMuseoVitrina)DrawTex(new Rect(br.x,br.y,48,48),texMuseoVitrina);
                if(InvisibleButton(br))museumSelection=i;
                GUI.Label(new Rect(br.x+52,br.y,br.width-56,br.height),(discovered[i]?"✓ ":"○ ")+CoastalCulture.Entries[i].name,new GUIStyle(button){fontSize=15,alignment=TextAnchor.MiddleLeft});
            }
            if(GUI.Button(new Rect(40,651,260,44),"VOLVER AL INICIO",button))ReturnToMenu();
        }
    }
}
