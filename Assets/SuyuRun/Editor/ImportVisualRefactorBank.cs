using System.IO;
using UnityEditor;
using UnityEngine;

namespace SuyuRun.Editor
{
    // Explicit staging import only; never changes existing resources or scenes.
    public static class ImportVisualRefactorBank
    {
        [MenuItem("SuyuRun/Refactor/Importar banco a staging")]
        public static void Import()
        {
            string root=Directory.GetParent(Application.dataPath).FullName;
            string source=Path.Combine(root,"Artifacts/VisualRefactor/Bank");
            if(!Directory.Exists(source)){Debug.LogError("No existe el banco de refactor.");return;}
            foreach(string file in Directory.GetFiles(source,"*.json",SearchOption.AllDirectories))
            {
                string text=File.ReadAllText(file);
                if(!text.Contains("\"frameWidth\""))continue;
                string png=Path.ChangeExtension(file,".png");
                if(!File.Exists(png))continue;
                string relative=file.Substring(source.Length+1).Replace('\\','/');
                string asset="Assets/SuyuRun/Art/RefactorStaging/"+relative;
                string target=Path.Combine(root,asset);
                Directory.CreateDirectory(Path.GetDirectoryName(target));
                File.Copy(file,target,true);
                File.Copy(png,Path.ChangeExtension(target,".png"),true);
                string texturePath=Path.ChangeExtension(asset,".png");
                AssetDatabase.ImportAsset(texturePath,ImportAssetOptions.ForceUpdate);
                var importer=(TextureImporter)AssetImporter.GetAtPath(texturePath);
                importer.textureType=TextureImporterType.Sprite;
                importer.spriteImportMode=SpriteImportMode.Single;
                importer.filterMode=FilterMode.Point;
                importer.textureCompression=TextureImporterCompression.Uncompressed;
                importer.mipmapEnabled=false;
                importer.wrapMode=TextureWrapMode.Clamp;
                importer.alphaIsTransparency=true;
                importer.maxTextureSize=4096;
                importer.spritePixelsPerUnit=64;
                var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);
                settings.spriteAlignment=(int)SpriteAlignment.Custom;
                settings.spritePivot=new Vector2(0,1);importer.SetTextureSettings(settings);
                importer.SaveAndReimport();
            }
            AssetDatabase.Refresh();
            Debug.Log("Banco importado a staging. No activado en gameplay; verificar registro corporal y eventos.");
        }
    }
}
