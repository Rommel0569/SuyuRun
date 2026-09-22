using UnityEngine;

namespace SuyuRun
{
    /// <summary>General scrolling background layer for Costa's pixel-art parallax stack (any
    /// approved scene layer or the ground tile). Origin is the top-left pixel of a 320x180 view,
    /// matching PixelCostaSkyLayer's convention. Enough tile copies are spawned to always cover
    /// the viewport regardless of the sprite's width, so this works both for a 640px scene panel
    /// (2 copies, like PixelCostaSkyLayer's clouds) and a 32px ground tile (many copies).</summary>
    public sealed class PixelParallaxLayer : MonoBehaviour
    {
        [SerializeField] Sprite sprite;
        [SerializeField] float parallaxFactor = .5f;
        [SerializeField] int sortingOrder;
        [SerializeField] float yPixels;
        const float Ppu = 32f;
        const float ViewportPixels = 320f;
        SpriteRenderer[] tiles;
        float tileWidth, bandWidth;
        float travelPixels;

        public void SetTravelPixels(float pixels) { travelPixels = pixels; }

        void Start()
        {
            if (!sprite) { Debug.LogWarning("PixelParallaxLayer needs a sprite.", this); return; }
            tileWidth = sprite.rect.width;
            int copies = Mathf.Max(2, Mathf.CeilToInt(ViewportPixels / tileWidth) + 2);
            bandWidth = tileWidth * copies;
            tiles = new SpriteRenderer[copies];
            for (int i = 0; i < copies; i++)
            {
                var go = new GameObject("Tile " + i);
                go.transform.SetParent(transform, false);
                var renderer = go.AddComponent<SpriteRenderer>();
                renderer.sprite = sprite;
                renderer.sortingOrder = sortingOrder;
                tiles[i] = renderer;
            }
        }

        void Update()
        {
            if (tiles == null) return;
            float offset = travelPixels * parallaxFactor;
            for (int i = 0; i < tiles.Length; i++)
            {
                float x = Mathf.Repeat(i * tileWidth - offset, bandWidth) - tileWidth;
                tiles[i].transform.localPosition = new Vector3(x / Ppu, -yPixels / Ppu, 0);
            }
        }
    }
}
