using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
namespace SuyuRun.Editor
{
    [InitializeOnLoad] public static class PixelViewportAudit
    {
        static PixelViewportAudit(){EditorApplication.update+=Poll;}
        static void Poll()
        {
            if(!EditorApplication.isCompiling&&!EditorApplication.isUpdating&&File.Exists("Artifacts/VisualRefactor/REQUEST_DOOR_AUDIT.txt"))
            {File.Delete("Artifacts/VisualRefactor/REQUEST_DOOR_AUDIT.txt");RunDoor();}
            const string request="Artifacts/VisualRefactor/REQUEST_PIXEL_VIEWPORT.txt";
            if(EditorApplication.isCompiling||EditorApplication.isUpdating||!File.Exists(request))return;
            File.Delete(request);Run();
        }
        public static void RunCompletion()
        {
            const string key="SuyuRun.Prototype.Bank";
            bool existed=PlayerPrefs.HasKey(key);int original=PlayerPrefs.GetInt(key,0);
            try
            {
                PlayerPrefs.SetInt(key,3);int coins=2;
                if(!RunnerProgress.TryRevive(ref coins)||coins!=0||PlayerPrefs.GetInt(key)!=0)throw new Exception("Mixed wallet revive failed");
                coins=4;if(RunnerProgress.TryRevive(ref coins)||coins!=4)throw new Exception("Insufficient funds changed wallet");
                coins=8;if(!RunnerProgress.TryRevive(ref coins)||coins!=3)throw new Exception("Run wallet revive failed");
                File.WriteAllText("Artifacts/VisualRefactor/revive_audit.txt","PASS mixed/run wallets and insufficient funds; original save restored.\n");
            }
            finally{if(existed)PlayerPrefs.SetInt(key,original);else PlayerPrefs.DeleteKey(key);PlayerPrefs.Save();}
            RunDoor();Run();GlobalEngineeringAudit.Run();HudLayoutAudit.Run();HumanGaitAudit.Run();RefactorResourceAudit.Run();RefactorMaterialAudit.Run();
        }
        public static void RunDoor()
        {
            AssetDatabase.ImportAsset("Assets/SuyuRun/Art/Shaders/PortalThreshold.shader",ImportAssetOptions.ForceSynchronousImport);
            var shader=Shader.Find("SuyuRun/PortalThreshold");
            if(!shader||ShaderUtil.ShaderHasError(shader))throw new Exception("Door threshold shader missing/invalid");
            var root=new GameObject("Temporary doorway QA");var target=new RenderTexture(640,360,24);target.Create();var previous=RenderTexture.active;
            Material art=new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"));
            try
            {
                var cam=new GameObject("Door QA camera").AddComponent<Camera>();cam.transform.SetParent(root.transform);cam.enabled=false;
                cam.orthographic=true;cam.orthographicSize=2;cam.transform.position=new Vector3(0,1.3f,-10);cam.cullingMask=1<<31;cam.backgroundColor=new Color32(47,39,48,255);
                var frames=Resources.LoadAll<Sprite>("costa_puerta_dimensional");if(frames.Length==0)throw new Exception("Missing door source");
                var portal=new GameObject("QA door").AddComponent<SpriteRenderer>();portal.transform.SetParent(root.transform);portal.gameObject.layer=31;
                portal.sprite=RefactorMaterialArt.Frame("costa_puerta_dimensional",frames[0],0,0);portal.transform.position=new Vector3(-1,3,0);portal.sharedMaterial=art;portal.sortingOrder=1;
                RunnerPortalIngress.Frame(portal,3);foreach(var sr in portal.GetComponentsInChildren<SpriteRenderer>())sr.gameObject.layer=31;
                var hero=new GameObject("QA Tayta").AddComponent<SpriteRenderer>();hero.transform.SetParent(root.transform);hero.gameObject.layer=31;hero.sortingOrder=3;hero.sharedMaterial=art;
                foreach(float phase in new[]{0f,.25f,.5f,.75f,1f})
                {
                    hero.sprite=TaytaArt.Frame("costa","run",phase*.5f);hero.transform.localScale=Vector3.one;hero.transform.position=new Vector3(-.75f,1.5f,0);
                    RunnerPortalIngress.Apply(hero,new Vector3(0,1.25f,0),phase);
                    if(hero.transform.localScale!=Vector3.one||hero.color.a!=1)throw new Exception("Door altered scale/alpha");
                    UnityEngine.Rendering.RenderPipeline.SubmitRenderRequest(cam,new UniversalRenderPipeline.SingleCameraRequest{destination=target});RenderTexture.active=target;
                    var image=new Texture2D(640,360,TextureFormat.RGBA32,false);image.ReadPixels(new Rect(0,0,640,360),0,0);image.Apply();
                    File.WriteAllBytes("Artifacts/VisualRefactor/door_phase_"+Mathf.RoundToInt(phase*100)+".png",image.EncodeToPNG());UnityEngine.Object.DestroyImmediate(image);
                }
                File.WriteAllText("Artifacts/VisualRefactor/door_audit.txt","PASS shader and five rendered phases; fixed body scale/alpha.\n");
            }
            finally{RenderTexture.active=previous;target.Release();UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(root);UnityEngine.Object.DestroyImmediate(art);}
        }
        [MenuItem("SuyuRun/QA/Verificar referencia de render adaptativa")]
        public static void Run()
        {
            var report=new StringBuilder("Adaptive pixel reference: same world framing, no undersized-target warning at tested sizes.\n");
            var go=new GameObject("Temporary pixel viewport audit");
            var camera=go.AddComponent<Camera>();camera.enabled=false;
            var pixel=go.AddComponent<PixelPerfectCamera>();
            try
            {
                foreach(var size in new[]{new Vector2Int(160,90),new Vector2Int(320,180),new Vector2Int(549,309),new Vector2Int(640,360),new Vector2Int(1280,720),new Vector2Int(3440,1440)})
                foreach(bool close in new[]{false,true})
                {
                    var target=new RenderTexture(size.x,size.y,0);camera.targetTexture=target;
                    try
                    {
                        RunnerPixelViewport.Configure(camera,close);
                        if(pixel.refResolutionX>size.x||pixel.refResolutionY>size.y||pixel.refResolutionX%2!=0||pixel.refResolutionY%2!=0||Mathf.Abs(pixel.refResolutionY/(float)pixel.assetsPPU-5.625f/(close?2:1))>.001f)
                            throw new Exception("Pixel reference changes framing or exceeds target: "+size);
                        report.AppendLine("PASS "+size+" close="+close+": "+pixel.refResolutionX+"x"+pixel.refResolutionY+", PPU="+pixel.assetsPPU);
                    }
                    finally{camera.targetTexture=null;UnityEngine.Object.DestroyImmediate(target);}
                }
            }
            finally{UnityEngine.Object.DestroyImmediate(go);}
            File.WriteAllText("Artifacts/VisualRefactor/pixel_viewport_audit.txt",report.ToString());
        }
    }
}
