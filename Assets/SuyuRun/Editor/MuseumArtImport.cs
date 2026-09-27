using UnityEditor;
namespace SuyuRun.Editor
{
    public sealed class MuseumArtImport:AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            if(!assetPath.Contains("/Art/SharedMenu/")&&!assetPath.EndsWith("sierra_camelidos_pastando.png"))return;
            var t=(TextureImporter)assetImporter;t.textureType=TextureImporterType.Default;
            t.filterMode=UnityEngine.FilterMode.Point;t.mipmapEnabled=false;t.alphaIsTransparency=true;
            t.textureCompression=TextureImporterCompression.Uncompressed;t.npotScale=TextureImporterNPOTScale.None;
            if(assetPath.EndsWith("inicio_final_marco.png"))t.maxTextureSize=4096;
        }
        void OnPreprocessAudio()
        {
            if(!assetPath.EndsWith("/Museum/MUSEO.mp3"))return;
            var t=(AudioImporter)assetImporter;var settings=t.defaultSampleSettings;
            settings.loadType=UnityEngine.AudioClipLoadType.Streaming;t.defaultSampleSettings=settings;
        }
    }
}
