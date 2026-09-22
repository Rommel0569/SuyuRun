using UnityEngine;

namespace SuyuRun
{
    /// <summary>Demo driver for the assembled pixel-art Costa scene: advances a travel-distance
    /// value over time and feeds it to every parallax layer, so Play mode shows the whole stack
    /// scrolling together instead of sitting static. This is a visual integration check, not the
    /// real runner controller - actual player-driven distance still lives in RunnerPrototype for
    /// the existing vector/mesh Level_Costa scene.</summary>
    public sealed class CostaPixelSceneDriver : MonoBehaviour
    {
        [SerializeField] float pixelsPerSecond = 60f;
        [SerializeField] PixelParallaxLayer[] layers;
        [SerializeField] PixelCostaSkyLayer skyLayer;
        float travel;

        void Update()
        {
            travel += pixelsPerSecond * Time.deltaTime;
            foreach (var layer in layers) if (layer) layer.SetTravelPixels(travel);
            if (skyLayer) skyLayer.SetTravelPixels(travel);
        }
    }
}
