using UnityEngine;
namespace SuyuRun
{
    // Logical coordinates remain 320x180: art detail is independent of geometry.
    public static class GameScale
    {
        public const float PixelsPerWorldUnit=32,HeroScale=.85f;
        public const float StandingHeight=34*HeroScale,CrouchHeight=15*HeroScale;
        public const float DuckClearance=20,NativeStride=112;
        public static float GaitClock(float travelled)=>travelled/(NativeStride*2);
        // Higher-resolution sprites advance on their native half-logical-pixel grid.
        public static float SnapVisual(float logical,bool detailed)=>Mathf.Round(logical*(detailed?2:1))/(detailed?2:1);
        public static Rect Body(Vector2 feet,float halfWidth=8,bool crouched=false)
        {
            float h=crouched?CrouchHeight:StandingHeight;
            return new Rect(feet.x-halfWidth*HeroScale,feet.y-h,2*halfWidth*HeroScale,h);
        }
    }
}
