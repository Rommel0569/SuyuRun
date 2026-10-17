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
            SelvaPanorama(FinalRect(1380,308,650,745));
            DrawTex(new Rect(0,0,1280,720),finalTitleFrame);
            if(InvisibleButton(FinalRect(111,955,560,88)))Begin();
            if(InvisibleButton(FinalRect(722,955,292,88)))mode=Mode.Selector;
            if(InvisibleButton(FinalRect(1040,955,288,88)))OpenMuseum();
            if(InvisibleButton(FinalRect(1360,955,288,88)))mode=Mode.Shop;
            if(InvisibleButton(FinalRect(1680,955,282,88)))Application.Quit();
            if(InvisibleButton(FinalRect(1875,10,155,55)))mode=Mode.Settings;
            if(CommonButton(new Rect(508,665,264,42),"TUTORIAL"))UnityEngine.SceneManagement.SceneManager.LoadScene("Level_Tutorial");
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
        void SelvaPanorama(Rect viewport)
        {
            GUI.BeginGroup(viewport);
            foreach(var layer in new[]{"bosque_capa1_cielo","bosque_capa2_lejania","bosque_capa3_estructura","bosque_capa5_cercania"})
            {var s=SelvaArt.Get(layer);var r=s.spec.rectsUnity[0];GUI.DrawTextureWithTexCoords(new Rect(-288,viewport.height-720,2560,720),s.texture,new Rect(r.x/(float)s.texture.width,r.y/(float)s.texture.height,r.width/(float)s.texture.width,r.height/(float)s.texture.height));}
            var bird=SelvaArt.Get("guacamayo_movimiento");int f=(int)(Time.unscaledTime*8)%bird.spec.frames;var b=bird.spec.rectsUnity[f];
            GUI.DrawTextureWithTexCoords(new Rect(84,viewport.height-360,128,128),bird.texture,new Rect(b.x/(float)bird.texture.width,b.y/(float)bird.texture.height,b.width/(float)bird.texture.width,b.height/(float)bird.texture.height));
            GUI.EndGroup();
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
                else SelvaPanorama(new Rect(x+8,120,368,320));
                GUI.Label(new Rect(x+8,450,368,40),names[i],new GUIStyle(title){fontSize=22,alignment=TextAnchor.MiddleCenter});
                int memories=i==0?DiscoveredCount:i==1?RegionalMemories("Sierra",SharedGameMenu.SierraIds):RegionalMemories("Selva",SharedGameMenu.SelvaIds);
                GUI.Label(new Rect(x+8,496,368,28),$"PIEZAS {memories}/{(i==1?3:4)}",new GUIStyle(small){alignment=TextAnchor.MiddleCenter});
                string caption=unlocked?"JUGAR":i==2?"COMPLETA SIERRA":"COMPLETA COSTA";
                bool old=GUI.enabled;GUI.enabled=unlocked;
                if(CommonButton(new Rect(x+24,546,336,40),caption,unlocked))
                {if(i==0)Begin();else if(i==2)RunnerProgress.OpenSelva();else {RunnerProgress.EnterSierraMuseum=false;RunnerProgress.OpenSierra();}}
                GUI.enabled=old;
            }
            if(CommonButton(new Rect(40,640,360,52),"VOLVER AL INICIO"))ReturnToMenu();
            if(CommonButton(new Rect(460,640,360,52),"TUTORIAL"))UnityEngine.SceneManagement.SceneManager.LoadScene("Level_Tutorial");
        }
    }
}
