using UnityEngine;
namespace SuyuRun
{
    public static class SierraGrazers
    {
        static Texture2D atlas;
        static readonly Sprite[,] frames=new Sprite[2,12];
        public static bool Visible(float world,float camera)=>world-camera+32>=0&&world-camera<=320;
        public static int Frame(float seconds,int offset)=>(Mathf.FloorToInt(seconds*4.3f)+offset)%12;
        public static Texture2D Atlas=>atlas?atlas:(atlas=Resources.Load<Texture2D>("SierraV3/sierra_camelidos_pastando"));
        public static Sprite Sprite(int species,int frame)
        {
            if(!Atlas)return null;
            if(!frames[species,frame])frames[species,frame]=UnityEngine.Sprite.Create(Atlas,new Rect(frame*32,(1-species)*32,32,32),new Vector2(0,1),32);
            return frames[species,frame];
        }
        public static void DrawMenu(Rect r,int species,int offset)
        {
            if(!Atlas)return;
            int frame=Frame(Time.unscaledTime,offset);
            GUI.DrawTextureWithTexCoords(r,Atlas,new Rect(frame/12f,(1-species)/2f,1/12f,.5f));
        }
    }
    public sealed partial class SierraStageDirector
    {
        void DrawGrazers(float distance)
        {
            if(act!=0&&act!=2)return;
            int first=Mathf.FloorToInt(distance/640)-1;
            // Absolute world anchors: recycle only once the WHOLE sprite is off-screen.
            for(int tile=first;tile<=first+2;tile++)
            for(int animal=0;animal<3;animal++)
            {
                float world=tile*640+(animal==0?160:animal==1?360:510),x=world-distance;
                if(!SierraGrazers.Visible(world,distance))continue;
                var sprite=SierraGrazers.Sprite(animal%2,SierraGrazers.Frame(seconds,animal*3));
                if(sprite)Paint(sprite,x,Surface(world+16,act)-29,7);
            }
        }
    }
}
