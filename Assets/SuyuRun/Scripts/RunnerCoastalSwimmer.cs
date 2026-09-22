using UnityEngine;

namespace SuyuRun
{
    public sealed partial class RunnerPrototype
    {
        // Phase 4 of the Costa redesign: a small swimming figure out on the water, a stylized
        // tribute to Jose Olaya (see CULTURAL_SOURCES.md for the institutional source - this is
        // an original silhouette, not a documented likeness; no such record exists for him).
        // Same technique as the boats' rower: a three-frame stroke cycle swapped via
        // MaterialPropertyBlock, driven by the same phase that already floats/bobs the figure
        // (ParallaxPiece.onFloatPhase), plus a small foam splash that flashes during the pull.
        MeshRenderer swimmerSplashRenderer;
        Mesh swimmerSplashMesh;

        void BuildCoastalSwimmer()
        {
            var root=new GameObject("Coastal swimmer - stylized tribute").transform;
            root.gameObject.AddComponent<UnityEngine.Rendering.SortingGroup>().sortingOrder=-19;

            var bodyGo=new GameObject("Swimmer body");bodyGo.transform.SetParent(root,false);
            bodyGo.AddComponent<MeshFilter>().sharedMesh=TexturedQuadMesh(0,0,.9f,.5f,1);
            var bodyRenderer=bodyGo.AddComponent<MeshRenderer>();bodyRenderer.sharedMaterial=SpriteMaterial("Swimmer/SwimmerReach");bodyRenderer.sortingOrder=1;
            var bodyBlock=new MaterialPropertyBlock();
            var reach=Resources.Load<Sprite>("Swimmer/SwimmerReach").texture;
            var pull=Resources.Load<Sprite>("Swimmer/SwimmerPull").texture;
            var glide=Resources.Load<Sprite>("Swimmer/SwimmerGlide").texture;

            var splashGo=new GameObject("Splash");splashGo.transform.SetParent(root,false);
            splashGo.transform.localPosition=new Vector3(.42f,.12f,0);
            swimmerSplashMesh=TexturedQuadMesh(0,0,.38f,.28f,1);
            splashGo.AddComponent<MeshFilter>().sharedMesh=swimmerSplashMesh;
            swimmerSplashRenderer=splashGo.AddComponent<MeshRenderer>();swimmerSplashRenderer.sharedMaterial=SpriteMaterial("Swimmer/Splash");swimmerSplashRenderer.sortingOrder=2;

            var piece=Layer(root,35,-.42f,.1f,140f,true);
            piece.onFloatPhase=phase=>
            {
                float cycle=Mathf.Repeat(phase/(Mathf.PI*2),1f);
                var tex=cycle<1f/3f?reach:cycle<2f/3f?pull:glide;
                bodyBlock.SetTexture(MainTexId,tex);
                bodyRenderer.SetPropertyBlock(bodyBlock);
                // Flashes brightest mid-pull, when the leading hand would be entering the water.
                float splashAlpha=Mathf.Clamp01(1f-Mathf.Abs(cycle-.5f)*4f);
                var colors=new Color[4];for(int i=0;i<4;i++)colors[i]=new Color(1,1,1,splashAlpha);
                swimmerSplashMesh.colors=colors;
            };
        }
    }
}
