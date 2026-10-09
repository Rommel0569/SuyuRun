using System;
using UnityEngine;
namespace SuyuRun
{
    public sealed class SelvaCaiman
    {
        public enum Phase { Hidden,Telegraph,Anticipation,Breach,Snap,Fall,Dive }
        public Phase phase; public Vector2 pos,velocity;public float timer;public bool left;
        public readonly float minX,maxX,waterY;readonly System.Random random;
        public int emerged,immersed,relocated;
        public bool wounded;
        public Action<string,Vector2> effect;
        public SelvaCaiman(float min,float max,float surface,int seed)
        {minX=min;maxX=max;waterY=surface;random=new System.Random(seed);Relocate();timer=-(float)random.NextDouble()*2;}
        void Relocate(){pos=new Vector2(Mathf.Lerp(minX,maxX,(float)random.NextDouble()),waterY+32);left=random.Next(2)==0;relocated++;wounded=false;}
        void State(Phase p){phase=p;timer=0;}
        public void Step(float dt,float targetX)
        {
            timer+=dt;
            switch(phase)
            {
                case Phase.Hidden:if(timer>1.4f){Relocate();State(Phase.Telegraph);}break;
                case Phase.Telegraph:if(timer>1.05f){left=targetX<pos.x;State(Phase.Anticipation);}break;
                case Phase.Anticipation:pos.y=waterY+8;if(timer>.24f){velocity=new Vector2((left?-1:1)*45,-265);pos.y=waterY;emerged++;effect?.Invoke("splash_salida",new Vector2(pos.x,waterY));State(Phase.Breach);}break;
                case Phase.Breach:case Phase.Snap:case Phase.Fall:
                    velocity.y+=560*dt;pos+=velocity*dt;
                    if(phase==Phase.Breach&&velocity.y>-28)State(Phase.Snap);
                    else if(phase==Phase.Snap&&timer>.11f)State(Phase.Fall);
                    if(velocity.y>0&&pos.y>=waterY){pos.y=waterY;immersed++;effect?.Invoke("splash_entrada",new Vector2(pos.x,waterY));effect?.Invoke("ondas",new Vector2(pos.x,waterY));State(Phase.Dive);}break;
                case Phase.Dive:pos.y+=68*dt;if(timer>.8f)State(Phase.Hidden);break;
            }
        }
        public string Animation=>phase==Phase.Anticipation?"anticipacion":phase==Phase.Breach?"emerger":phase==Phase.Snap?"snap":phase==Phase.Fall?"caida":phase==Phase.Dive?"inmersion":"acecho";
        public int Frame {get{var s=SelvaArt.Get("caiman_"+Animation);return Mathf.Min(s.frames.Length-1,(int)(timer*s.spec.fps));}}
        public Vector2 Origin=>pos-new Vector2(30,33);
        public bool Hits(Rect hero)
        {
            if(wounded||phase!=Phase.Breach&&phase!=Phase.Snap&&phase!=Phase.Fall)return false;
            var h=SelvaArt.Get("caiman_"+Animation).spec.hitboxes[Frame];
            return h.damaging&&SelvaArt.NativeBox(h.body,Origin,left,64).Overlaps(hero)||h.snap&&SelvaArt.NativeBox(h.mouth,Origin,left,64).Overlaps(hero);
        }
    }
    public sealed class SelvaVine
    {
        public readonly Vector2 anchor;public const float Length=82;
        public float angle,omega;public bool occupied;
        public SelvaVine(Vector2 at,float phase){anchor=at;angle=Mathf.Sin(phase)*.3f;}
        public Vector2 Grip=>anchor+new Vector2(Mathf.Sin(angle),Mathf.Cos(angle))*Length;
        public Vector2 Tangent=>new Vector2(Mathf.Cos(angle),-Mathf.Sin(angle))*Length*omega;
        public void Step(float dt,float push=0)
        {omega+=(-420/Length*Mathf.Sin(angle)+push)*dt;omega*=Mathf.Exp(-.10f*dt);angle=Mathf.Clamp(angle+omega*dt,-1.05f,1.05f);}
        public int Pose=>Mathf.Clamp(Mathf.RoundToInt((angle*Mathf.Rad2Deg+56)/7),0,16);
        // Curvature changes sign with inertia, not just with the angular position.
        public Vector2 Curve(float t)=>anchor+(Grip-anchor)*t+new Vector2(-Mathf.Cos(angle),Mathf.Sin(angle))*(Mathf.Clamp(omega*4,-10,10)*Mathf.Sin(t*Mathf.PI));
    }
}
