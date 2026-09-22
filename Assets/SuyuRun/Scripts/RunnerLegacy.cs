using UnityEngine;
namespace SuyuRun
{
    public sealed partial class RunnerPrototype
    {
        void BuildLegacyScenery()
        {
            Shape("Prototype sunset",world,new Vector2(5,2.7f),Vector2.one*2.7f,new Color(.96f,.49f,.28f),64,0,-20);
            Rect("Prototype sea",world,0,-1.3f,40,2.4f,new Color(.06f,.3f,.36f),-19);
            for(int i=0;i<18;i++)
            {
                var hill=Shape("Simple prototype hill",world,new Vector2(-16+i*2.7f,-1.2f),new Vector2(6,3+i%3),new Color(.12f+i%2*.04f,.24f,.28f),3,90,-15);
                scenery.Add(hill);sceneryBase.Add(hill.position);
            }
            Rect("Simple prototype ground",world,0,-3.8f,45,2.4f,new Color(.16f,.21f,.24f),-5);
            Rect("Prototype landing edge",world,0,Ground-.04f,45,.08f,gold,-4);
        }
    }
}
