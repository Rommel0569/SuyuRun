using UnityEditor;
using UnityEngine;
namespace SuyuRun.Editor
{
    public sealed class RefactorRuntimeImport:AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            if(!assetPath.Contains("/RefactorRuntime/Resources/RefactorBank/")&&!assetPath.Contains("/RefactorRuntime/Resources/HudStatus/")&&!assetPath.Contains("/RefactorRuntime/Resources/UnifiedHud/"))return;
            var importer=(TextureImporter)assetImporter;
            importer.textureType=TextureImporterType.Sprite;
            importer.spriteImportMode=SpriteImportMode.Single;
            importer.filterMode=FilterMode.Point;
            importer.textureCompression=TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled=false;importer.wrapMode=assetPath.EndsWith("/costa_capa7_suelo.png")?TextureWrapMode.Repeat:TextureWrapMode.Clamp;
            importer.npotScale=TextureImporterNPOTScale.None;
            importer.alphaIsTransparency=true;importer.maxTextureSize=16384;
            // The foreground river is CPU-masked around platforms at native resolution.
            importer.isReadable=assetPath.EndsWith("/selva_agua_superficie.png");
            importer.spritePixelsPerUnit=assetPath.Contains("/AtmosphereClean/")?128:64;
        }
    }
}
