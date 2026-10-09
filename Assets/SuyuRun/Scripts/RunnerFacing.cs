using UnityEngine;
namespace SuyuRun
{
    public static class RunnerFacing
    {
        public static bool TowardLeft(float actorX,float targetX)=>targetX<actorX;
        public static bool FlipFor(Sprite sprite,bool left)=>sprite&&(sprite.name.StartsWith("RefactorPresentation/stone/")||sprite.name.StartsWith("RefactorPresentation/Actors/pebble/"))?!left:left;
        // Flip around the visible canvas centre, not the imported top-left pivot.
        // This preserves registration with the unchanged hitbox for every bank.
        public static void Apply(SpriteRenderer renderer,bool left)
        {
            // Stone attack drawings have a native left-facing silhouette.
            // Other banks (including Tayta and Inca) are authored facing right.
            if(renderer)left=FlipFor(renderer.sprite,left);
            if(!renderer||!renderer.sprite||renderer.flipX==left)return;
            Vector3 center=renderer.bounds.center;
            renderer.flipX=left;
            renderer.transform.position+=center-renderer.bounds.center;
        }
    }
}
