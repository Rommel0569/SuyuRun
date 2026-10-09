using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
namespace SuyuRun.Editor
{
    public static class RefactorMaterialAudit
    {
        [Serializable] sealed class Spec {public int frames,logicalWidth,logicalHeight;public string original;}
        public static void Run()
        {
            var report=new StringBuilder("Native material runtime registration audit\n");int count=0;
            foreach(var path in Directory.GetFiles("Assets/SuyuRun/Art/RefactorRuntime/Resources/RefactorBank/Materials","*.json"))
            {
                string name=Path.GetFileNameWithoutExtension(path);var s=JsonUtility.FromJson<Spec>(File.ReadAllText(path));
                var importer=AssetImporter.GetAtPath(Path.ChangeExtension(path,".png")) as TextureImporter;
                if(importer&&importer.maxTextureSize<16384){importer.maxTextureSize=16384;importer.SaveAndReimport();}
                var original=AssetDatabase.LoadAssetAtPath<Texture2D>(s.original);
                if(!original)throw new Exception("Missing material original "+name);
                var legacy=Sprite.Create(original,new Rect(0,0,s.logicalWidth,s.logicalHeight),new Vector2(0,1),32,0,SpriteMeshType.FullRect);
                try
                {
                    for(int i=0;i<s.frames;i++)
                    {
                        var frame=RefactorMaterialArt.Frame(name,legacy,0,i);
                        if(!frame||frame==legacy||RefactorActorArt.Size(frame)!=new Vector2(s.logicalWidth,s.logicalHeight)||Vector2.Distance(frame.bounds.size,new Vector2(s.logicalWidth/32f,s.logicalHeight/32f))>.001f||frame.texture.filterMode!=FilterMode.Point)
                            throw new Exception("Invalid material frame "+name+"/"+i);
                        if(name=="costa_capa7_suelo"&&frame.texture.wrapMode!=TextureWrapMode.Repeat)throw new Exception("Costa facade cannot repeat");
                    }
                    report.AppendLine("PASS "+name+": "+s.frames+" frames, Point, original world size");count++;
                }
                finally{UnityEngine.Object.DestroyImmediate(legacy);}
            }
            report.AppendLine("Sheets verified: "+count+". Geometry/resource audit, not artistic approval.");
            File.WriteAllText("Artifacts/VisualRefactor/material_runtime_audit.txt",report.ToString());
        }
    }
}
