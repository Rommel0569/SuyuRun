using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
namespace SuyuRun.Editor
{
    public static class SierraReadabilityAudit
    {
        [MenuItem("SUYURUN/Final/Verificar color Sierra y escala inicial %#&m")]
        public static void Run()
        {
            TaytaCompatibilityAudit.Run();
            var shader=Resources.Load<Shader>("SierraTaytaMatte");
            if(!shader||!shader.isSupported)throw new Exception("Sierra matte shader unavailable");
            foreach(var candidate in new[]{shader,Shader.Find("SuyuRun/PortalThreshold")})
                foreach(var message in ShaderUtil.GetShaderMessages(candidate))
                    if(message.severity==UnityEditor.Rendering.ShaderCompilerMessageSeverity.Error)throw new Exception(message.message);
            var preview=EditorSceneManager.NewPreviewScene();
            var camera=new GameObject("Readability camera").AddComponent<Camera>();
            SceneManager.MoveGameObjectToScene(camera.gameObject,preview);
            camera.scene=preview;
            camera.orthographic=true;camera.orthographicSize=2;camera.transform.position=new Vector3(0,0,-10);
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.18f,.23f,.32f);
            var source=TaytaArt.Get("sierra","run").frames[2];
            for(int i=0;i<2;i++)
            {
                var actor=new GameObject(i==0?"Before":"Matte").AddComponent<SpriteRenderer>();
                SceneManager.MoveGameObjectToScene(actor.gameObject,preview);
                actor.sprite=source;
                actor.transform.localScale=Vector3.one*(3/source.bounds.size.y);
                actor.transform.position=new Vector3(i==0?-1.7f:1.7f,0,0)-source.bounds.center*(3/source.bounds.size.y);
                if(i==1)SierraTaytaAppearance.Apply(actor);
                else actor.sharedMaterial=new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"));
            }
            var target=new RenderTexture(960,540,24);target.Create();
            RenderPipeline.SubmitRenderRequest(camera,new UniversalRenderPipeline.SingleCameraRequest{destination=target});
            RenderTexture.active=target;var image=new Texture2D(960,540,TextureFormat.RGBA32,false);
            image.ReadPixels(new Rect(0,0,960,540),0,0);image.Apply();RenderTexture.active=null;
            Directory.CreateDirectory("Artifacts/SierraReadability");
            File.WriteAllBytes("Artifacts/SierraReadability/before_after.png",image.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(image);target.Release();UnityEngine.Object.DestroyImmediate(target);
            File.WriteAllText("Artifacts/SierraReadability/result.txt","PASS: previous animation bank selected; both shaders supported without import errors; real URP sprite comparison rendered. Left original, right matte.\n");
            EditorSceneManager.ClosePreviewScene(preview);
            if(Application.isBatchMode)FinalMoviePlayAudit.Run();
            else FinalMoviePlayAudit.Isolated();
        }
    }
}
