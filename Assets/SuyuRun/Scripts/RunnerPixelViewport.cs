using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace SuyuRun
{
    // A docked GameView can be smaller than the new 640x360 render. Reduce
    // render density, never the visible world or the source texture resolution.
    [DefaultExecutionOrder(10000)]
    public sealed class RunnerPixelViewport : MonoBehaviour
    {
        Camera view;
        PixelPerfectCamera pixel;
        int cutIn=1;
        bool combat;

        public static int DensityDivisor(int width,int height)
            =>width>=640&&height>=360?1:width>=320&&height>=180?2:4;

        public static void Configure(Camera camera,bool close=false,bool wideCombat=false)
        {
            var policy=camera.GetComponent<RunnerPixelViewport>();
            if(!policy)policy=camera.gameObject.AddComponent<RunnerPixelViewport>();
            policy.view=camera;policy.pixel=camera.GetComponent<PixelPerfectCamera>();
            policy.cutIn=close?2:1;policy.combat=wideCombat;policy.Apply();
        }

        void LateUpdate(){Apply();}
        void Apply()
        {
            if(!view||!pixel)return;
            var target=view.targetTexture;
            int targetWidth=target?target.width:Screen.width,targetHeight=target?target.height:Screen.height;
            bool wide=combat&&targetWidth>=192&&targetHeight>=108;
            int baseWidth=wide?768:640,baseHeight=wide?432:360;
            int divisor=targetWidth>=baseWidth&&targetHeight>=baseHeight?1:targetWidth>=baseWidth/2&&targetHeight>=baseHeight/2?2:4;
            int width=baseWidth/divisor,height=baseHeight/divisor,ppu=64/divisor*cutIn;
            if(pixel.refResolutionX!=width)pixel.refResolutionX=width;
            if(pixel.refResolutionY!=height)pixel.refResolutionY=height;
            if(pixel.assetsPPU!=ppu)pixel.assetsPPU=ppu;
        }
    }
}
