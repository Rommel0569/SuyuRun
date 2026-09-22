using UnityEngine;

namespace SuyuRun
{
    public sealed partial class RunnerPrototype
    {
        // Replaces the old vector pelican flocks and this project's own Phase 5 pixel clouds with
        // the approved ART_BIBLE.md capa 2 (costa_capa2_nubes.png + costa_capa2_aves.png),
        // per the user's explicit request to use all the bible's pixel art and animations,
        // including the birds. Reuses PixelCostaSkyLayer as-is (already built and approved for
        // this exact job) rather than re-deriving its cloud/flock logic.
        PixelCostaSkyLayer bibleSkyLayer;

        void BuildCoastalSky()
        {
            var clouds=Resources.Load<Texture2D>("costa_capa2_nubes");
            var birds=Resources.Load<Texture2D>("costa_capa2_aves");
            if(!clouds||!birds){Debug.LogWarning("SuyuRun: costa_capa2_nubes/aves not found in Resources; sky birds/clouds skipped.",this);return;}
            var go=new GameObject("ART_BIBLE clouds and birds (capa 2)");
            go.transform.SetParent(world,false);
            // Raised so the birds glide near the mountain peaks instead of near their base -
            // reads more natural, per the user's note.
            go.transform.localPosition=new Vector3(0,5.5f,0);
            bibleSkyLayer=go.AddComponent<PixelCostaSkyLayer>();
            bibleSkyLayer.SetTextures(clouds,birds);
        }

        void UpdateCoastalSky()
        {
            if(!bibleSkyLayer)return;
            bibleSkyLayer.SetPaused(mode==Mode.Paused);
            bibleSkyLayer.SetTravelPixels(distance*32f);
        }
    }
}
