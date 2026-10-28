using System;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace SuyuRun.Editor
{
    // Offline energy landmarks, not a claim of musical phrase/beat transcription.
    public static class FinalMovieScoreBuild
    {
        public static void Build()
        {
            foreach(string guid in AssetDatabase.FindAssets("t:Texture2D",new[]{"Assets/SuyuRun/Art/RefactorRuntime/Resources/FinalMovieV4"}))
            {
                string path=AssetDatabase.GUIDToAssetPath(guid);var importer=AssetImporter.GetAtPath(path) as TextureImporter;
                if(importer&&(importer.maxTextureSize!=16384||importer.npotScale!=TextureImporterNPOTScale.None||importer.textureCompression!=TextureImporterCompression.Uncompressed))
                {
                    importer.maxTextureSize=16384;importer.npotScale=TextureImporterNPOTScale.None;
                    importer.filterMode=FilterMode.Point;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.mipmapEnabled=false;
                    importer.SaveAndReimport();
                }
            }
            var clip=Resources.Load<AudioClip>("FinalMovie/final_music");
            if(!clip)throw new InvalidOperationException("Final soundtrack missing");
            clip.LoadAudioData();
            int count=Math.Min(clip.samples,clip.frequency*31),channels=clip.channels;
            var data=new float[count*channels];
            if(!clip.GetData(data,0))throw new InvalidOperationException("Cannot read PCM soundtrack for offline score");
            int block=Math.Max(1,clip.frequency/50),n=count/block;
            var rms=new float[n];
            for(int w=0;w<n;w++)
            {
                double sum=0;int start=w*block*channels;
                for(int j=0;j<block*channels;j++)sum+=data[start+j]*data[start+j];
                rms[w]=(float)Math.Sqrt(sum/(block*channels));
            }
            var result=new FinalMovieScore();var nominal=new[]{2.5f,6,10,14.5f};
            for(int k=0;k<4;k++)
            {
                float best=-1;int chosen=Mathf.RoundToInt(nominal[k]*clip.frequency/block);
                for(int w=Math.Max(2,chosen-9);w<Math.Min(n-1,chosen+10);w++)
                {
                    float onset=Mathf.Max(0,rms[w]-rms[w-2]);
                    float metric=onset+.04f*rms[w]-.001f*Mathf.Abs(w-chosen);
                    if(metric>best){best=metric;result.cuts[k]=w*block/(float)clip.frequency;}
                }
            }
            result.Validate();result.method="Offline 20ms PCM energy-rise landmarks near storyboard cuts; ear validation not claimed";
            string folder="Assets/SuyuRun/Art/RefactorRuntime/Resources/FinalMovieV4";
            File.WriteAllText(folder+"/score.json",JsonUtility.ToJson(result,true));
            Directory.CreateDirectory("Artifacts/FinalMovieV4");
            File.WriteAllText("Artifacts/FinalMovieV4/MUSIC_LANDMARKS.json",JsonUtility.ToJson(result,true));
            AssetDatabase.ImportAsset(folder+"/score.json");
            Debug.Log("Final V4 score: "+string.Join(", ",result.cuts));
        }
    }
}
