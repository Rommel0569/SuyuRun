using System;
using UnityEngine;
namespace SuyuRun
{
    // Shared runtime UI: identical window geometry for both regions and their caves.
    public static class SharedGameMenu
    {
        public sealed class State {public int tab,index,region;}
        public sealed class Context
        {
            public bool sierra,paused,dead,complete,diagnostic,canRevive;
            public int coins,score,bank;
            public string detail,calibrationInfo,regionName;
            public Action resume,retry,menu,next,revive,settingsChanged,previewMusic,applyCalibration,resetCalibration;
            public Func<int,int,bool> collected;
        }
        static Font font;
        static readonly Color Ink=new Color32(33,28,45,255),Gold=new Color32(248,197,110,255),Paper=new Color32(255,240,194,255);
        public static readonly string[] SierraIds={"cabeza_clava","kero","quipu"};
        public static readonly string[] SelvaIds={"selva_ceramica","selva_flauta","selva_textil","selva_sonaja"};
        static readonly string[] SelvaNames={"Ceramica con kene","Flauta del viajero","Textil con kene","Sonaja del viajero"};
        static readonly string[] SelvaOrigins={"SHIPIBO-KONIBO · UCAYALI","OBJETO ORIGINAL · SUYURUN","SHIPIBO-KONIBO · UCAYALI","OBJETO ORIGINAL · SUYURUN"};
        static readonly string[] SelvaDescriptions={"El kene es un sistema de disenos aplicado a ceramica, textiles y otros soportes. Esta pieza pixel art es una interpretacion, no una replica arqueologica.","Instrumento de viento original del juego. No se atribuye este diseno a un pueblo o periodo historico especifico.","El kene expresa conocimientos y estetica Shipibo-Konibo. Los disenos se aplican tambien en textiles; este sprite es una interpretacion artistica.","Instrumento de percusion original del juego. No representa una pieza historica documentada."};
        public static bool IsCollected(int region,int index)
        {string key=region==0?"Costa."+CoastalCulture.Entries[index].id:region==1?"Sierra."+SierraIds[index]:"Selva."+SelvaIds[index];return PlayerPrefs.GetInt("SuyuRun.Museum."+key,0)==1;}
        static readonly string[] SierraNames={"Cabeza clava","Kero ceremonial","Quipu"};
        static readonly string[] Origins={"CHAVIN · ANCASH","TRADICION ANDINA · INCA","REGISTRO ANDINO · INCA"};
        static readonly string[] Descriptions={
            "Escultura de piedra que formaba parte de los muros de Chavin de Huantar. Es patrimonio Chavin, no una escultura inca de Machu Picchu.",
            "Vaso ceremonial usado para beber chicha. Los keros de madera pertenecen a una larga tradicion andina, anterior y posterior al periodo inca.",
            "Cordeles anudados que registraban cantidades mediante nudos y posiciones decimales. Su lectura requeria conocimientos especializados."};
        static readonly string[] Sources={
            "https://www.gob.pe/institucion/cultura/noticias/126679-la-milenaria-cultura-chavin-lista-para-ser-vista-de-manera-virtual-desde-tu-casa",
            "https://www.metmuseum.org/art/collection/search/316842",
            "https://library.harvard.edu/about/news/2023-04-12/long-w-2-there-was-quipu-accounting-systems-incan-and-andean-peoples"};
        static Color Skin(bool sierra)=>sierra?new Color32(43,58,92,255):new Color32(84,52,73,255);
        static GUIStyle Text(int size=5,TextAnchor align=TextAnchor.UpperLeft)
        {
            if(!font)font=Resources.Load<Font>("PressStart2P-Regular");
            return new GUIStyle(GUI.skin.label){font=font,fontSize=size,wordWrap=true,alignment=align,padding=new RectOffset(),normal={textColor=Paper}};
        }
        public static void Frame(Rect r,bool sierra)
        {
            RunnerPresentation.Box(r,Ink);
            RunnerPresentation.Box(new Rect(r.x+1,r.y+1,r.width-2,r.height-2),Gold);
            RunnerPresentation.Box(new Rect(r.x+3,r.y+3,r.width-6,r.height-6),Skin(sierra));
            // Native half-logical-pixel bevels stay inside the existing shared layout.
            if(r.width>12&&r.height>12)
            {
                RunnerPresentation.Box(new Rect(r.x+3,r.y+3,r.width-6,.5f),Paper);
                RunnerPresentation.Box(new Rect(r.x+3,r.yMax-3.5f,r.width-6,.5f),Ink);
                RunnerPresentation.Box(new Rect(r.x+3,r.y+3,.5f,r.height-6),Paper);
                RunnerPresentation.Box(new Rect(r.xMax-3.5f,r.y+3,.5f,r.height-6),Ink);
            }
            foreach(float x in new[]{r.x,r.xMax-6})foreach(float y in new[]{r.y,r.yMax-6})
            {RunnerPresentation.Box(new Rect(x,y,6,6),Ink);RunnerPresentation.Box(new Rect(x+1,y+1,4,4),Gold);RunnerPresentation.Box(new Rect(x+2,y+2,2,2),Skin(sierra));}
        }
        public static bool Button(Rect r,string text,bool sierra,bool enabled=true,int size=5)
        {
            Frame(r,sierra);bool hover=enabled&&r.Contains(Event.current.mousePosition);
            if(hover)RunnerPresentation.Box(new Rect(r.x+4,r.y+4,r.width-8,1),Gold);
            var old=GUI.color;if(!enabled)GUI.color=new Color(.6f,.6f,.6f,1);
            GUI.Label(new Rect(r.x+5,r.y+3,r.width-10,r.height-6),text,Text(size,TextAnchor.MiddleCenter));GUI.color=old;
            return enabled&&GUI.Button(r,GUIContent.none,GUIStyle.none);
        }
        public static void Draw(State state,Context c)
        {
            Frame(new Rect(5,7,310,169),c.sierra);
            GUI.Label(new Rect(16,15,182,11),"SUYURUN",Text(8));
            GUI.Label(new Rect(204,16,97,10),c.regionName??(c.sierra?"SIERRA":"COSTA"),Text(5,TextAnchor.MiddleRight));
            string[] tabs={"RESUMEN","MUSEO","AJUSTES","TIENDA","AYUDA"};
            for(int i=0;i<5;i++)if(Button(new Rect(14+i*59,29,56,15),tabs[i],c.sierra))state.tab=i;
            if(state.tab==1)Museum(state,c);
            else if(state.tab==2)Settings(c);
            else if(state.tab==3)Shop(c);
            else if(state.tab==4)Tutorial(c);
            else Summary(state,c);
            if(Button(new Rect(14,154,142,15),c.paused?"CONTINUAR":"VOLVER AL INICIO",c.sierra))
            {if(c.paused)c.resume?.Invoke();else c.menu?.Invoke();}
            if(Button(new Rect(164,154,142,15),"MENU PRINCIPAL",c.sierra))c.menu?.Invoke();
        }
        static void Tutorial(Context c)
        {
            GUI.Label(new Rect(20,51,280,13),"COMO JUGAR",Text(7,TextAnchor.MiddleCenter));
            GUI.Label(new Rect(22,70,276,72),"RUNNER: avanzas automaticamente.\nESP: salto; pulsa otra vez para doble salto.\nS / ABAJO: deslizar bajo obstaculos.\nE: rafaga. SIERRA: click en el ave, huaraca.\nSELVA: click en nudo de liana, ESP soltar.\nCUEVAS: A/D mover, W/ESP x2, S bajar.\nCLICK/E disparar; R recargar.\nCHULLACHAQUI: E golpe, F patada, Q upper.\nVence al jefe y esbirros; entra en la puerta.",Text(4));
        }
        static void Summary(State state,Context c)
        {
            GUI.Label(new Rect(20,52,280,16),c.complete?"NIVEL COMPLETADO":c.dead?"VUELVE A INTENTAR":"PAUSA",Text(8,TextAnchor.MiddleCenter));
            GUI.Label(new Rect(24,75,272,27),c.detail??"La musica y el recorrido estan en pausa.",Text(5,TextAnchor.MiddleCenter));
            GUI.Label(new Rect(24,107,272,12),$"MONEDAS {c.coins}    PUNTOS {c.score}",Text(5,TextAnchor.MiddleCenter));
            if(c.dead||c.complete)
            {
                if(Button(new Rect(32,128,122,17),"REINTENTAR",c.sierra))c.retry?.Invoke();
                if(Button(new Rect(166,128,122,17),c.dead?"REVIVIR · 5":c.next!=null?"SIGUIENTE":"MUSEO",c.sierra,!c.dead||c.canRevive))
                {if(c.dead)c.revive?.Invoke();else if(c.next!=null)c.next.Invoke();else state.tab=1;}
            }
        }
        static void Museum(State s,Context c)
        {
            if(Button(new Rect(16,49,42,13),"COSTA",c.sierra)){s.region=0;s.index=0;}
            if(Button(new Rect(62,49,42,13),"SIERRA",c.sierra)){s.region=1;s.index=0;}
            if(Button(new Rect(108,49,38,13),"SELVA",c.sierra)){s.region=2;s.index=0;}
            int count=s.region==1?3:4;s.index=Mathf.Clamp(s.index,0,count-1);
            var entry=s.region==0?CoastalCulture.Entries[s.index]:null;
            string id=entry!=null?entry.id:s.region==1?SierraIds[s.index]:SelvaIds[s.index];
            bool owned=s.region==2?IsCollected(2,s.index):c.collected!=null&&c.collected(s.region,s.index);
            Frame(new Rect(16,68,74,77),c.sierra);
            MuseumArt.Draw(new Rect(21,73,64,64),id,owned);
            GUI.Label(new Rect(98,68,204,14),entry!=null?entry.name:s.region==1?SierraNames[s.index]:SelvaNames[s.index],Text(6));
            GUI.Label(new Rect(98,86,204,16),entry!=null?entry.place:s.region==1?Origins[s.index]:SelvaOrigins[s.index],Text(4));
            string description=entry!=null?entry.description:s.region==1?Descriptions[s.index]:SelvaDescriptions[s.index];
            GUI.Label(new Rect(98,105,204,39),owned?description:"Memoria aun no recogida. Encuentrala en "+(s.region==0?"Costa":s.region==1?"Sierra":"Selva")+" para revelar su historia.",Text(4));
            if(Button(new Rect(150,49,20,13),"<",c.sierra))s.index=(s.index+count-1)%count;
            GUI.Label(new Rect(175,52,43,10),(s.index+1)+" / "+count,Text());
            if(Button(new Rect(216,49,20,13),">",c.sierra))s.index=(s.index+1)%count;
            if(Button(new Rect(240,49,64,13),"FUENTE",c.sierra,s.region!=2||s.index==0||s.index==2))Application.OpenURL(entry!=null?entry.source:s.region==1?Sources[s.index]:"https://bdpi.cultura.gob.pe/pueblos/shipibo-konibo");
        }
        static void Settings(Context c)
        {
            string[] keys={"MusicVol","EffectsVol"};string[] labels={"MUSICA","EFECTOS"};
            for(int i=0;i<2;i++)
            {
                float value=PlayerPrefs.GetFloat("SuyuRun.Settings."+keys[i],1),next=value;float y=51+i*23;
                GUI.Label(new Rect(21,y+4,169,12),labels[i]+"   "+Mathf.RoundToInt(value*100)+"%",Text());
                if(Button(new Rect(213,y,36,17),"-",c.sierra))next=Mathf.Max(0,value-.1f);
                if(Button(new Rect(258,y,36,17),"+",c.sierra))next=Mathf.Min(1,value+.1f);
                if(next!=value&&!c.diagnostic){PlayerPrefs.SetFloat("SuyuRun.Settings."+keys[i],next);PlayerPrefs.Save();c.settingsChanged?.Invoke();}
            }
            float latency=PlayerPrefs.GetFloat("SuyuRun.Settings.Latency",0),newLatency=latency;
            GUI.Label(new Rect(21,101,188,12),"LATENCIA "+Mathf.RoundToInt(latency*1000)+"ms",Text());
            if(Button(new Rect(213,97,36,17),"-",c.sierra))newLatency=Mathf.Max(-.25f,latency-.01f);
            if(Button(new Rect(258,97,36,17),"+",c.sierra))newLatency=Mathf.Min(.25f,latency+.01f);
            if(newLatency!=latency&&!c.diagnostic){PlayerPrefs.SetFloat("SuyuRun.Settings.Latency",newLatency);PlayerPrefs.Save();c.settingsChanged?.Invoke();}
            GUI.Label(new Rect(21,120,282,11),c.calibrationInfo??"CALIBRACION CON T DURANTE PREESCUCHA",Text(4));
            if(Button(new Rect(20,133,91,14),"ESCUCHAR",c.sierra,c.previewMusic!=null))c.previewMusic?.Invoke();
            if(Button(new Rect(116,133,91,14),"APLICAR TAPS",c.sierra,c.applyCalibration!=null))c.applyCalibration?.Invoke();
            if(Button(new Rect(212,133,91,14),"BORRAR TAPS",c.sierra,c.resetCalibration!=null))c.resetCalibration?.Invoke();
        }
        static void Shop(Context c)
        {
            int wallet=PlayerPrefs.GetInt("SuyuRun.Prototype.Bank",0);
            GUI.Label(new Rect(20,52,280,12),"BANCO COMPARTIDO: "+wallet,Text());
            string[] ids={"Vida","Rafaga","Iman"},labels={"VIDA EXTRA","MAS RAFAGA","IMAN DE MONEDAS"};
            for(int i=0;i<3;i++)
            {
                int level=Mathf.Clamp(PlayerPrefs.GetInt("SuyuRun.Upgrade."+ids[i],0),0,3),cost=(level+1)*20;
                GUI.Label(new Rect(20,76+i*23,163,12),labels[i]+" "+level+"/3",Text());
                if(Button(new Rect(197,70+i*23,103,19),level==3?"MAXIMO":cost+" MONEDAS",c.sierra,level<3&&wallet>=cost&&!c.diagnostic))
                {PlayerPrefs.SetInt("SuyuRun.Prototype.Bank",wallet-cost);PlayerPrefs.SetInt("SuyuRun.Upgrade."+ids[i],level+1);PlayerPrefs.Save();c.settingsChanged?.Invoke();}
            }
            GUI.Label(new Rect(20,142,280,9),"VIDA: SIERRA Y CUEVAS · MEJORAS PERMANENTES",Text(4));
        }
    }
}
