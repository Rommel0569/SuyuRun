using UnityEngine;
namespace SuyuRun
{
    public sealed partial class RunnerPrototype
    {
        Texture2D[] menuCoast,menuSierra;
        Texture2D finalTitleFrame;
        Rect FinalRect(float x,float y,float w,float h)=>new Rect(x/2048*1280,y/1143*720,w/2048*1280,h/1143*720);
        bool DrawFinalTitle()
        {
            if(!finalTitleFrame)finalTitleFrame=Resources.Load<Texture2D>("inicio_final_marco");
            if(!finalTitleFrame)return false;
            Panel(new Rect(0,0,1280,720),new Color32(33,28,45,255));
            RegionalPanorama(FinalRect(32,308,630,650),false);
            RegionalPanorama(FinalRect(715,308,654,660),true);
            // Selva keeps its frame and label but no invented/unimplemented gameplay scene.
            Panel(FinalRect(1380,308,650,745),new Color32(33,28,45,255));
            DrawTex(new Rect(0,0,1280,720),finalTitleFrame);
            if(InvisibleButton(FinalRect(111,955,560,88)))Begin();
            if(InvisibleButton(FinalRect(722,955,292,88)))mode=Mode.Selector;
            if(InvisibleButton(FinalRect(1040,955,288,88)))OpenMuseum();
            if(InvisibleButton(FinalRect(1360,955,288,88)))mode=Mode.Shop;
            if(InvisibleButton(FinalRect(1680,955,282,88)))Application.Quit();
            if(InvisibleButton(FinalRect(1875,10,155,55)))mode=Mode.Settings;
            return true;
        }
        void LoadRegionalMenu()
        {
            if(menuCoast!=null)return;
            menuCoast=new[]{Resources.Load<Texture2D>("costa_capa1_cielo"),Resources.Load<Texture2D>("costa_capa2_nubes"),Resources.Load<Texture2D>("costa_capa3_paracas"),Resources.Load<Texture2D>("costa_capa4_mar"),Resources.Load<Texture2D>("costa_capa5_huacas")};
            menuSierra=new[]{Resources.Load<Texture2D>("SierraV3/sierra_compartido_capa1_cielo"),Resources.Load<Texture2D>("SierraV3/sierra_compartido_capa2_nubes"),Resources.Load<Texture2D>("SierraV3/sierra_compartido_capa3_nevados"),Resources.Load<Texture2D>("SierraV3/sierra_ruinas_capa4_paisaje"),Resources.Load<Texture2D>("SierraV3/sierra_ruinas_capa6_cercano")};
        }
        void RegionalPanorama(Rect viewport,bool sierra)
        {
            LoadRegionalMenu();var layers=sierra?menuSierra:menuCoast;
            GUI.BeginGroup(viewport);
            for(int i=0;i<layers.Length;i++)
            {
                var texture=layers[i];if(!texture)continue;
                // Original independent layers, never horizontal strips cut out of one illustration.
                const float scale=4;float width=texture.width*scale;
                float offset=(sierra?88:112)*scale;
                int frames=Mathf.Max(1,texture.height/180),frame=(int)(Time.unscaledTime*4)%frames;
                var uv=new Rect(0,1-(frame+1)/(float)frames,1,1f/frames);
                for(float x=-offset;x<viewport.width;x+=width)
                    GUI.DrawTextureWithTexCoords(new Rect(x,viewport.height-720,width,720),texture,uv);
            }
            if(sierra)
            {
                // Fixed terrace positions; only neck/chewing frames animate.
                SierraGrazers.DrawMenu(new Rect((120-88)*4,viewport.height-720+130*4,64,64),0,0);
                SierraGrazers.DrawMenu(new Rect((139-88)*4,viewport.height-720+143*4,64,64),1,4);
                SierraGrazers.DrawMenu(new Rect((163-88)*4,viewport.height-720+136*4,64,64),0,8);
            }
            GUI.EndGroup();
        }
        void DrawRegionalMenu()
        {
            RegionalPanorama(new Rect(0,176,640,416),false);
            RegionalPanorama(new Rect(640,176,640,416),true);
            Panel(new Rect(636,176,8,416),new Color32(33,28,45,255));
            var style=new GUIStyle(small){alignment=TextAnchor.MiddleCenter,fontSize=16};
            GUI.Label(new Rect(32,540,552,32),"COSTA · EL ALCATRAZ",style);
            GUI.Label(new Rect(688,540,552,32),"SIERRA · CAMINO DE LOS ANDES",style);
        }
        int RegionalMemories(string region,string[] ids)
        {int count=0;foreach(var id in ids)if(PlayerPrefs.GetInt("SuyuRun.Museum."+region+"."+id,0)==1)count++;return count;}
        void CommonFrame(Rect r)
        {
            var old=GUI.matrix;GUI.matrix=old*Matrix4x4.Scale(Vector3.one*4);
            SharedGameMenu.Frame(new Rect(r.x/4,r.y/4,r.width/4,r.height/4),false);GUI.matrix=old;
        }
        bool CommonButton(Rect r,string caption,bool enabled=true)
        {
            var old=GUI.matrix;GUI.matrix=old*Matrix4x4.Scale(Vector3.one*4);
            bool clicked=SharedGameMenu.Button(new Rect(r.x/4,r.y/4,r.width/4,r.height/4),caption,false,enabled);
            GUI.matrix=old;return clicked;
        }
        void DrawRegionalSelector()
        {
            CommonFrame(new Rect(20,28,1240,676));
            GUI.Label(new Rect(40,20,1200,72),"SELECTOR DE NIVEL",new GUIStyle(title){fontSize=24});
            string[] names={"COSTA","SIERRA","SELVA"};
            for(int i=0;i<3;i++)
            {
                float x=48+i*400;bool unlocked=i==0||i==1&&RunnerProgress.SierraUnlocked||i==2&&RunnerProgress.SelvaUnlocked;
                CommonFrame(new Rect(x,112,384,492));
                if(i<2)RegionalPanorama(new Rect(x+8,120,368,320),i==1);
                else Panel(new Rect(x+8,120,368,320),new Color32(46,59,34,255));
                GUI.Label(new Rect(x+8,450,368,40),names[i],new GUIStyle(title){fontSize=22,alignment=TextAnchor.MiddleCenter});
                int memories=i==0?DiscoveredCount:RegionalMemories("Sierra",new[]{"cabeza_clava","kero","quipu"});
                GUI.Label(new Rect(x+8,496,368,28),i<2?$"PIEZAS {memories}/{(i==0?4:3)}":"ARTE Y NIVEL PENDIENTES",new GUIStyle(small){alignment=TextAnchor.MiddleCenter});
                string caption=i==2?(unlocked?"PRÓXIMAMENTE":"BLOQUEADO"):unlocked?"JUGAR":"COMPLETA COSTA";
                bool old=GUI.enabled;GUI.enabled=unlocked&&i<2;
                if(CommonButton(new Rect(x+24,546,336,40),caption,unlocked&&i<2))
                {if(i==0)Begin();else {RunnerProgress.EnterSierraMuseum=false;RunnerProgress.OpenSierra();}}
                GUI.enabled=old;
            }
            if(CommonButton(new Rect(40,640,360,52),"VOLVER AL INICIO"))ReturnToMenu();
        }
    }
}
