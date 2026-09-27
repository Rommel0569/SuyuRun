using System;
using UnityEngine;
namespace SuyuRun
{
    // Shared Costa/Sierra pause, retry and victory presentation; region supplies its skin/data.
    public static class RunnerModal
    {
        public sealed class Model
        {
            public string heading,body,footer,primary,secondary;
            public int coins,score,bank;
            public bool showStats,secondaryEnabled=true;
            public Action primaryAction,secondaryAction;
        }
        public static void Draw(Rect bounds,Texture2D background,Font font,Model model,bool nineSlice=false)
        {
            Matrix4x4 saved=GUI.matrix;Color savedColor=GUI.color;
            GUI.matrix=saved*Matrix4x4.TRS(new Vector3(bounds.x,bounds.y,0),Quaternion.identity,new Vector3(bounds.width/220,bounds.height/140,1));
            GUI.color=Color.white;
            if(nineSlice)
            {
                float[] xs={0,2,218,220},ys={0,2,138,140};
                float[] us={0,2f/background.width,1-2f/background.width,1},vs={1,1-2f/background.height,2f/background.height,0};
                for(int row=0;row<3;row++)for(int col=0;col<3;col++)
                    GUI.DrawTextureWithTexCoords(new Rect(xs[col],ys[row],xs[col+1]-xs[col],ys[row+1]-ys[row]),background,new Rect(us[col],vs[row+1],us[col+1]-us[col],vs[row]-vs[row+1]));
            }
            else GUI.DrawTexture(new Rect(0,0,220,140),background);
            var text=new GUIStyle(GUI.skin.label){font=font,fontSize=5,wordWrap=true,padding=new RectOffset(0,0,0,0),normal={textColor=new Color32(255,243,208,255)}};
            var heading=new GUIStyle(text){fontSize=8,alignment=TextAnchor.MiddleCenter};
            var button=new GUIStyle(GUI.skin.button){font=font,fontSize=5,padding=new RectOffset(1,1,1,1)};
            GUI.Label(new Rect(8,6,204,17),model.heading,heading);
            GUI.Label(new Rect(10,25,200,24),model.body,text);
            if(model.showStats)
            {
                GUI.Label(new Rect(20,53,180,10),"MONEDAS  "+model.coins,text);
                GUI.Label(new Rect(20,69,180,10),"PUNTOS   "+model.score,text);
                GUI.Label(new Rect(20,85,180,10),"BANCO    "+model.bank,text);
            }
            GUI.Label(new Rect(10,103,200,16),model.footer??"",text);
            if(nineSlice?RunnerPresentation.Button(new Rect(10,122,94,13),model.primary,font):GUI.Button(new Rect(10,122,94,13),model.primary,button))model.primaryAction?.Invoke();
            if(!string.IsNullOrEmpty(model.secondary))
            {
                bool enabled=GUI.enabled;GUI.enabled=enabled&&model.secondaryEnabled;
                if(nineSlice?RunnerPresentation.Button(new Rect(116,122,94,13),model.secondary,font,model.secondaryEnabled):GUI.Button(new Rect(116,122,94,13),model.secondary,button))model.secondaryAction?.Invoke();
                GUI.enabled=enabled;
            }
            GUI.matrix=saved;GUI.color=savedColor;
        }
    }
}
