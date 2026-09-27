using UnityEngine;
using UnityEngine.Rendering.Universal;
namespace SuyuRun
{
    // Native-pixel presentation shared by regional menus and portal transitions.
    public static class RunnerPresentation
    {
        public static readonly Color Ink=new Color32(26,32,54,255),Stone=new Color32(76,85,83,255),Gold=new Color32(216,180,107,255);
        public static void Box(Rect r,Color color)
        {var old=GUI.color;GUI.color=color;GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=old;}
        public static bool Button(Rect r,string text,Font font,bool enabled=true)
        {
            bool hover=r.Contains(Event.current.mousePosition)&&enabled;
            Box(r,Ink);Box(new Rect(r.x+1,r.y+1,r.width-2,r.height-2),hover?Gold:Stone);
            Box(new Rect(r.x+2,r.y+1,r.width-4,1),Gold);
            var style=new GUIStyle(GUI.skin.label){font=font,fontSize=5,alignment=TextAnchor.MiddleCenter,padding=new RectOffset(),normal={textColor=enabled?new Color32(255,243,208,255):new Color32(120,128,120,255)}};
            GUI.Label(r,text,style);
            return enabled&&GUI.Button(r,GUIContent.none,GUIStyle.none);
        }
        public static void Curtain(float closed,float width=320,float height=180)
        {
            // Solid stepped edges, not translucent blur/gradients over the art.
            float h=Mathf.Ceil(Mathf.Clamp01(closed)*height*.5f);
            if(h<=0)return;
            Box(new Rect(0,0,width,h),Ink);Box(new Rect(0,height-h,width,h),Ink);
        }
        public static void CameraPose(Camera camera,Vector3 home,Vector3 focus,float approach,bool close)
        {
            // At 1x the fixed sky fills exactly320px; panning it would reveal empty edges.
            var p=close?Vector3.Lerp(home,focus,Mathf.SmoothStep(0,1,approach)):home;
            camera.transform.position=new Vector3(Mathf.Round(p.x*32)/32,Mathf.Round(p.y*32)/32,home.z);
            var pixel=camera.GetComponent<PixelPerfectCamera>();
            // Integer 1x/2x cut-in preserves hard pixel edges during cinematics.
            if(pixel)pixel.assetsPPU=close?64:32;
        }
    }
}
