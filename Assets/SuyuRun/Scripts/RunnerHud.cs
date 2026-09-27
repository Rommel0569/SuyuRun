using System;
using UnityEngine;
namespace SuyuRun
{
    // Layout extracted verbatim from Costa's compact HUD. Each region supplies its own skin.
    public static class RunnerHud
    {
        public static void Draw(float scale,int coins,int score,GUIStyle text,Func<int,bool> found,
            Action<Rect> panel,Action<Rect> slot,Action<Rect> coin,Action<Rect> points,Color highlight)
        {
            Rect R(float x,float y,float w,float h)=>new Rect(Mathf.Round(x*scale),Mathf.Round(y*scale),Mathf.Round(w*scale),Mathf.Round(h*scale));
            panel(R(16,10,1248,40));
            for(int i=0;i<4;i++)
            {
                slot(R(24+i*44,14,32,32));
                if(found(i)){GUI.color=highlight;coin(R(30+i*44,20,20,20));GUI.color=Color.white;}
            }
            coin(R(860,16,24,24));GUI.Label(R(888,15,140,26),$"{coins:00}",text);
            points(R(1040,16,24,24));GUI.Label(R(1068,15,196,26),$"{score:0000}",text);
        }
        public static void Sprite(Rect rect,Sprite sprite)
        {
            if(!sprite)return;
            Rect r=sprite.rect;Texture2D t=sprite.texture;
            GUI.DrawTextureWithTexCoords(rect,t,new Rect(r.x/t.width,r.y/t.height,r.width/t.width,r.height/t.height));
        }
    }
}
