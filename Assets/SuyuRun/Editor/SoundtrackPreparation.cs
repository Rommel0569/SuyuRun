using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SuyuRun.Editor
{
    public static class SoundtrackPreparation
    {
        const string ClipPath="Assets/SuyuRun/Audio/El Alcatraz.mp3";
        [Serializable] sealed class AudioInfo { public int frequency,channels,samples; public float duration; }
        [Serializable] sealed class RhythmAnalysis { public float bpm,firstBeatSeconds,clipStartSeconds; public float[] beatTimes; }
        [MenuItem("SuyuRun/Audio/Registrar música elegida")]
        public static void RegisterSelectedMusic()
        {
            const string path="Assets/SuyuRun/Data/Resources/SelectedMusic.asset";
            AssetDatabase.Refresh();var selection=AssetDatabase.LoadAssetAtPath<SelectedMusic>(path);
            if(!selection){selection=ScriptableObject.CreateInstance<SelectedMusic>();AssetDatabase.CreateAsset(selection,path);}
            selection.costa=AssetDatabase.LoadAssetAtPath<AudioClip>(ClipPath);
            selection.sierra=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/SuyuRun/Audio/sierra.mp3");
            selection.selva=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/SuyuRun/Audio/selva.mp3");
            selection.inicio=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/SuyuRun/Audio/Resources/Inicio.mp3");
            if(!selection.sierra||!selection.selva||!selection.inicio)throw new Exception("A selected music file is not imported yet.");
            EditorUtility.SetDirty(selection);AssetDatabase.SaveAssets();
            Directory.CreateDirectory("Artifacts/Audio");File.WriteAllText("Artifacts/Audio/selected-music.txt",$"Costa: {selection.costa.length:F2}s\nSierra: {selection.sierra.length:F2}s\nSelva: {selection.selva.length:F2}s\nInicio: {selection.inicio.length:F2}s\nUser files imported; Sierra/Selva beatmaps still pending.");
        }
        [MenuItem("SuyuRun/Audio/Exportar muestras para sincronización")]
        public static void ExportSamples()
        {
            AssetDatabase.ImportAsset(ClipPath,ImportAssetOptions.ForceSynchronousImport);
            var importer=(AudioImporter)AssetImporter.GetAtPath(ClipPath);
            if(!importer)throw new Exception("Missing user-provided MP3.");
            var settings=importer.defaultSampleSettings;settings.loadType=AudioClipLoadType.DecompressOnLoad;settings.quality=1;
            importer.defaultSampleSettings=settings;importer.SaveAndReimport();
            var clip=AssetDatabase.LoadAssetAtPath<AudioClip>(ClipPath);clip.LoadAudioData();
            var pcm=new float[clip.samples*clip.channels];
            if(!clip.GetData(pcm,0))throw new Exception("Unable to decode clip samples.");
            Directory.CreateDirectory("Artifacts/Audio");
            using(var file=new BinaryWriter(File.Create("Artifacts/Audio/alcatraz.f32")))foreach(float sample in pcm)file.Write(sample);
            File.WriteAllText("Artifacts/Audio/alcatraz-info.json",JsonUtility.ToJson(new AudioInfo{frequency=clip.frequency,channels=clip.channels,samples=clip.samples,duration=clip.length},true));
            Debug.Log("SuyuRun: audio samples exported, "+clip.length+" seconds.");
        }
        [MenuItem("SuyuRun/Audio/Integrar Alcatraz y recorrido completo")]
        public static void ApplyAnalysis()
        {
            const string trackPath="Assets/SuyuRun/Data/Resources/AlcatrazSoundtrack.asset";
            const string coursePath="Assets/SuyuRun/Data/Resources/CostaAlcatraz.asset";
            if(AssetDatabase.LoadAssetAtPath<RunnerCourse>(coursePath))
                throw new Exception("CostaAlcatraz already exists. Edit its data deliberately; do not overwrite authored changes with the initial setup.");
            var analysis=JsonUtility.FromJson<RhythmAnalysis>(File.ReadAllText("Artifacts/Audio/alcatraz-analysis.json"));
            var track=AssetDatabase.LoadAssetAtPath<RunnerSoundtrack>(trackPath);
            if(!track){track=ScriptableObject.CreateInstance<RunnerSoundtrack>();AssetDatabase.CreateAsset(track,trackPath);}
            track.clip=AssetDatabase.LoadAssetAtPath<AudioClip>(ClipPath);
            track.trackTitle="El Alcatraz";track.bpm=analysis.bpm;track.firstBeatSeconds=analysis.firstBeatSeconds;
            track.clipStartSeconds=analysis.clipStartSeconds;track.beatTimes=analysis.beatTimes;
            track.creditAndPermission="MP3 proporcionado por el usuario. Intérprete, autoría y permiso de redistribución pendientes de documentar antes de publicar. Pulsos estimados automáticamente; revisión auditiva pendiente.";
            track.ValidateForDuration(track.clip.length);
            var basis=RunnerCourse.VerticalPrototype();
            var c=ScriptableObject.CreateInstance<RunnerCourse>();c.region="COSTA · EL ALCATRAZ";
            c.soundtrack=track;c.duration=track.clip.length-.03f;c.bpm=analysis.bpm;
            var encounters=new List<Encounter>();var platforms=new List<CoursePlatform>();var gaps=new List<CourseGap>();var orbs=new List<CourseOrb>();var pads=new List<float>();
            float Map(float x)=>track.BeatToSeconds(x/3.5f)*c.speed;
            // Three explicitly arranged phrases of the coastal prototype; no random generation.
            for(int section=0;section<3;section++)
            {
                float shift=section*448;
                foreach(var item in basis.encounters)
                {
                    var e=item;e.beat+=section*128;
                    if(c.BeatToSeconds(e.beat)<c.duration-.3f)encounters.Add(e);
                }
                for(int i=0;i<basis.platforms.Length;i++)
                {
                    var p=basis.platforms[i];float left=Map(p.x+shift),right=Map(p.x+p.width+shift);
                    p.x=left;p.width=right-left;
                    if(section==1)p.style=(StructureStyle)((i+1)%3);
                    if(section==2)p.style=(StructureStyle)((i+2)%3);
                    platforms.Add(p);
                }
                foreach(var item in basis.gaps){var g=item;g.x=Map(item.x+shift);g.width=Map(item.x+item.width+shift)-g.x;gaps.Add(g);}
                foreach(var item in basis.orbs){var o=item;o.x=Map(item.x+shift);orbs.Add(o);}
                foreach(float x in basis.jumpPads)pads.Add(Map(x+shift));
            }
            c.encounters=encounters.ToArray();c.platforms=platforms.ToArray();c.gaps=gaps.ToArray();c.orbs=orbs.ToArray();c.jumpPads=pads.ToArray();
            c.ValidateCourse();AssetDatabase.CreateAsset(c,coursePath);EditorUtility.SetDirty(track);AssetDatabase.SaveAssets();
            UnityEngine.Object.DestroyImmediate(basis);
            File.WriteAllText("Artifacts/Audio/integration.txt",$"Imported user MP3; full course {c.duration:F3}s; estimated BPM {track.bpm}; {track.beatTimes.Length} beat markers; {c.platforms.Length} platforms; {c.orbs.Length} orbs. Compilation/data validation only; listening and play balance still pending.");
            Debug.Log("SuyuRun: El Alcatraz integrated with full-length coastal course.");
        }
    }
}
