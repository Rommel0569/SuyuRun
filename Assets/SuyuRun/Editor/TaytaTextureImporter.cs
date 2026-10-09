using UnityEditor;
using UnityEngine;
namespace SuyuRun.Editor
{
    public sealed class TaytaTextureImporter:AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            if(!assetPath.StartsWith("Assets/SuyuRun/Art/Tayta/")&&!assetPath.Contains("selva_museo_") )return;
            var t=(TextureImporter)assetImporter;
            t.filterMode=FilterMode.Point;t.textureCompression=TextureImporterCompression.Uncompressed;
            t.mipmapEnabled=false;t.alphaIsTransparency=true;t.npotScale=TextureImporterNPOTScale.None;t.maxTextureSize=4096;
        }
    }
}
