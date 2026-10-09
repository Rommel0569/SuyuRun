using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
namespace SuyuRun.Editor
{
    public static class LayerPolishAudit
    {
        [Serializable] sealed class Spec {public int frames,frameWidth,frameHeight,logicalWidth,logicalHeight,pixelsPerUnit;public string original;}
        [MenuItem("SuyuRun/Refactor/Verificar lote de capas")]
        public static void Run()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            var report=new StringBuilder("Layer polish Unity resource/world-scale audit\n");int count=0,poses=0;
            const string folder="Assets/SuyuRun/Art/RefactorRuntime/Resources/RefactorBank/LayerPolish";
            foreach(var path in Directory.GetFiles(folder,"*.json"))
            {
                var s=JsonUtility.FromJson<Spec>(File.ReadAllText(path));string name=Path.GetFileNameWithoutExtension(path);
                var texture=Resources.Load<Texture2D>("RefactorBank/LayerPolish/"+name);
                if(!texture||texture.width!=s.frameWidth*s.frames||texture.height!=s.frameHeight||texture.filterMode!=FilterMode.Point)throw new Exception("Texture registration "+name);
                var original=AssetDatabase.LoadAssetAtPath<Texture2D>(s.original);if(!original)throw new Exception("Missing original "+name);
                var legacy=Sprite.Create(original,new Rect(0,0,s.logicalWidth,s.logicalHeight),new Vector2(0,1),32,0,SpriteMeshType.FullRect);
                try
                {
                    for(int i=0;i<s.frames;i++)
                    {
                        var frame=RefactorEnvironmentArt.Frame(name,legacy);
                        if(frame==legacy)frame=RefactorMaterialArt.Frame(name,legacy,0,i);
                        if(!frame||frame==legacy||frame.texture!=texture||Vector2.Distance(frame.bounds.size,new Vector2(s.logicalWidth/32f,s.logicalHeight/32f))>.001f)throw new Exception("Runtime selection/scale "+name+"/"+i);
                        poses++;
                    }
                    report.AppendLine("PASS "+name+": "+s.frames+" frames, original world size");count++;
                }
                finally{UnityEngine.Object.DestroyImmediate(legacy);}
            }
            foreach(var state in new[]{"huaraca_cargar","huaraca_girar","huaraca_lanzar"})
            {
                var sheet=TaytaArt.Get("sierra",state);
                if(sheet==null||!sheet.frames[0].texture.name.EndsWith("_v3"))throw new Exception("Huaraca not selected "+state);
                foreach(var frame in sheet.frames)if(!RefactorActorArt.IsRegistered(frame))throw new Exception("Huaraca unregistered "+state);
                report.AppendLine("PASS "+state+": "+sheet.frames.Length+" fixed-scale poses");
            }
            report.AppendLine("PASS "+count+" layers / "+poses+" layer frames. Not a visual Play-through or artistic acceptance.");
            Directory.CreateDirectory("Artifacts/VisualRefactor/LayerPolish");File.WriteAllText("Artifacts/VisualRefactor/LayerPolish/UNITY_AUDIT.txt",report.ToString());Debug.Log(report.ToString());
        }
    }
}
