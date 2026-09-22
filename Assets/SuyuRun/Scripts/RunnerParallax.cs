using System.Collections.Generic;
using UnityEngine;

namespace SuyuRun
{
    public sealed partial class RunnerPrototype
    {
        sealed class ParallaxPiece {public Transform root;public float x,y,speed,period;public bool floating;public System.Action<float> onFloatPhase;public System.Action<float> onWrapFade;}
        readonly List<ParallaxPiece> parallaxPieces=new List<ParallaxPiece>();
        ParallaxPiece Layer(Transform root,float x,float y,float speed,float period,bool floating=false)
        {root.SetParent(world,false);var piece=new ParallaxPiece{root=root,x=x,y=y,speed=speed,period=period,floating=floating};parallaxPieces.Add(piece);return piece;}
        void BuildParallaxLayers()
        {
            BuildSea();
            // The panorama itself is placed once in BuildIllustratedCoast and updated directly in
            // UpdateCoastalArt (slow, non-wrapping parallax) - it is a single perspective illustration
            // of one bay, so tiling/mirroring copies of it side by side duplicated the ruin, headland
            // and shoreline in an obviously repeated, mirrored way. Not added to the wrapping layer list.
            // The old Phase 5 procedural mountains (Sky/Mountain0-3, this project's own pre-bible
            // silhouettes) were retired here per the user's explicit request that the level look
            // "tal cual" the ART_BIBLE.md reference composite: they sat on screen at the same time
            // as the bible's own Paracas/Candelabro band below, two unrelated mountain silhouettes
            // overlapping and reading as clutter instead of the single clean skyline in the reference.
            // ART_BIBLE.md mid-ground scene bands (Paracas/Candelabro, huacas, algarrobos/rocas/
            // lobos), brought into the real playable level per the user's request. Unlike the
            // panorama sky (a single unrepeatable perspective illustration, see phase 1), these were
            // validated as seamless left=right, so tiling them continuously (period equal to
            // their own width, no gaps) is exactly what they were designed for.
            // depthScale/depthAlpha shrink and soften each band (furthest = most), an atmospheric-
            // perspective cue so the water's edge doesn't read as flat/"dry" cutouts. Each of these
            // also gets 2-3 separate passes at different lift/scale/speed instead of one single
            // strip, so the huacas/algarrobos read as several rows sitting at different distances
            // (varied phase per pass, via the differing order/period below) rather than one flat
            // line of repeated silhouettes.
            // Toned down from the first pass at this (fewer rows, lower alpha overall - it was
            // reading as cluttered) and every instance now fades at its own wrap seam instead of
            // just relying on the two-copy handoff, which wasn't fully hiding the reset once the
            // period got small (the depth-scaled, smaller-tiled rows wrap much more often than the
            // original single band did).
            BuildBibleBandLayer("costa_capa3_paracas",24f,.55f,.13f,-28,depthScale:.6f,depthAlpha:.55f);
            BuildBibleBandLayer("costa_capa5_huacas",24f,.45f,.35f,-27,depthScale:.5f,depthAlpha:.45f,phaseOffset:.33f);
            BuildBibleBandLayer("costa_capa5_huacas",24f,.15f,.50f,-26,depthScale:.7f,depthAlpha:.7f);
            BuildBibleBandLayer("costa_capa6_algarrobos",24f,0f,.75f,-24,depthScale:.8f,depthAlpha:.8f);
            // Boats removed per the user's explicit request: six of them at different speeds/depths
            // visibly crossed paths mid-sea, reading as cluttered ornaments rather than a clean
            // composition ("quita lo que esta en medio del mar... que se vea limpio, profesional").
            BuildCoastalProps();
        }
        // Sea's own top edge (see BuildSea/BuildOpenOcean's y=.35/.41-ish convention) - the line
        // these silhouettes' bases should sit on, not sink into.
        const float SeaHorizonY=.4f;
        void BuildBibleBandLayer(string resourceName,float worldWidth,float liftAboveHorizon,float factor,int order,float depthScale=1f,float depthAlpha=1f,float phaseOffset=0f)
        {
            var sprite=Resources.Load<Sprite>(resourceName);
            if(!sprite){Debug.LogWarning("SuyuRun: missing ART_BIBLE layer "+resourceName);return;}
            worldWidth*=depthScale;
            float worldHeight=worldWidth*(sprite.rect.height/sprite.rect.width);
            // Fixed: these images' artwork reaches their own bottom edge (huacas' bases "llegan al
            // borde inferior" per ART_APPROVALS.md), so centering them (the old behaviour) sank a
            // good third of each image into the sea - land silhouettes visibly poking up through
            // the water. Bottom-align instead: the quad's bottom edge sits on the sea's own top
            // edge (SeaHorizonY), with an optional small lift so a layer can sit a bit further back
            // "above" the shoreline than another.
            float y=SeaHorizonY+liftAboveHorizon+worldHeight*.5f;
            // Two copies, not one: a single instance wrapping within its own period pops/teleports
            // the instant Mathf.Repeat resets it, because there is nothing else on screen to hand
            // off to (unlike the mountains/boats, whose period is far wider than the viewport so
            // that reset always happens off-screen). Two copies side by side, each the width of one
            // tile and wrapping over a band twice that width, always have one of them covering the
            // seam - the same "conveyor belt" technique already used for the sea/sky/ground tiling
            // elsewhere, just expressed through this game's own Layer() system instead of a
            // dedicated component. The validated seamless edges are what make this look
            // continuous rather than like two visibly repeated copies.
            for(int i=0;i<2;i++)
            {
                var go=new GameObject("ART_BIBLE band ("+resourceName+") "+i);
                var mesh=TexturedQuadMesh(0,0,worldWidth,worldHeight,1);
                // Slightly smaller and a touch translucent: simple atmospheric-perspective cue
                // (things further off look smaller and hazier) so the land/water border doesn't
                // read as a flat, "dry" cutout glued right at the shoreline.
                if(depthAlpha<1f)SetMeshAlpha(mesh,depthAlpha);
                go.AddComponent<MeshFilter>().sharedMesh=mesh;
                var renderer=go.AddComponent<MeshRenderer>();renderer.sharedMaterial=SpriteMaterial(resourceName);renderer.sortingOrder=order;
                var piece=Layer(go.transform,i*worldWidth+phaseOffset*worldWidth*2,y,factor,worldWidth*2);
                // Extra safety net on top of the two-copy handoff: fades this copy out right before
                // its own wrap resets it. The smaller, depth-scaled rows wrap much more often than
                // the original single band, so relying on the handoff alone wasn't fully hiding it.
                piece.onWrapFade+=fade=>SetMeshAlpha(mesh,fade*depthAlpha);
            }
        }
        // Still used by the swimmer's stroke-frame swap (RunnerCoastalSwimmer.cs) even after the
        // boats/rowers that used to share it were removed.
        static readonly int MainTexId=Shader.PropertyToID("_MainTex");
        void BuildAdobeOutpost(float x,float y,float speed,float period)
        {
            var root=new GameObject("Distant adobe outpost and fishing net").transform;
            root.gameObject.AddComponent<UnityEngine.Rendering.SortingGroup>().sortingOrder=-24;
            Rect("Adobe storehouse",root,0,.55f,1.5f,1.1f,new Color(.75f,.48f,.31f),0);
            Rect("Outpost parapet",root,0,1.11f,1.8f,.16f,sand,1);
            Rect("Shadowed doorway",root,.2f,.35f,.4f,.7f,shadowStone,1);
            for(int i=0;i<3;i++)Rect("Adobe window",root,-.53f+i*.45f,.86f,.2f,.18f,shadowStone,1);
            Rect("Net post",root,1.3f,.55f,.05f,1.1f,shadowStone,1);Rect("Net post",root,2.5f,.55f,.05f,1.1f,shadowStone,1);
            for(int i=0;i<5;i++){Rect("Fishing net vertical",root,1.4f+i*.25f,.65f,.015f,.7f,cream,1);Rect("Fishing net horizontal",root,1.95f,.32f+i*.17f,1.2f,.012f,cream,1);}
            Layer(root,x,y,speed,period);
        }
        void UpdateParallaxLayers()
        {
            foreach(var piece in parallaxPieces)
            {
                float x=Mathf.Repeat(piece.x-distance*piece.speed+piece.period*.5f,piece.period)-piece.period*.5f;
                piece.root.localPosition=new Vector3(x,piece.y,0);
                if(piece.floating)
                {
                    float phase=waterTime*1.25f+piece.x*.3f;
                    piece.root.localPosition+=Vector3.up*(Mathf.Sin(phase)*.035f);
                    piece.root.localRotation=Quaternion.Euler(0,0,Mathf.Sin(phase+.7f)*1.2f);
                    piece.onFloatPhase?.Invoke(phase);
                }
                piece.onWrapFade?.Invoke(WrapFade(x,piece.period));
            }
            UpdateWater();
            UpdateCoastalSky();
        }
    }
}
