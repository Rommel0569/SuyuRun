using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
namespace SuyuRun
{
    public sealed partial class SierraStageDirector
    {
        int serviceTab,museumIndex,combo;
        double pauseDsp,feedbackUntil,lastTap=-1;
        string rhythmFeedback="";
        float latency,liveMusicVolume,tapBpm;
        readonly List<float> tapErrors=new List<float>(),tapIntervals=new List<float>();
        readonly Dictionary<int,AudioClip> cueClips=new Dictionary<int,AudioClip>();
        AudioSource sfx;
        MuseumAmbience museumAmbience;
        void UpdateMuseumMusic()
        {
            if(!museumAmbience)museumAmbience=gameObject.AddComponent<MuseumAmbience>();
            museumAmbience.Tick((paused||complete||dead||(cavePhase!=CavePhase.None&&cave!=null&&cave.dead))&&serviceTab==1,liveMusicVolume);
        }
        static readonly string[] MemoryIds={"cabeza_clava","kero","quipu"};
        static readonly string[] MemoryNames={"CABEZA CLAVA","KERO CEREMONIAL","QUIPU"};
        static readonly string[] MemoryOrigins={"CHAVIN · ANCASH","TRADICION ANDINA · INCA","REGISTRO ANDINO · INCA"};
        static readonly string[] MemoryTexts={
            "Escultura tallada en piedra. Las cabezas clavas formaban parte de los muros de Chavin de Huantar. Esta memoria pertenece a Chavin, no a Machu Picchu.",
            "Vaso usado para beber chicha en encuentros y ceremonias. Los keros de madera forman parte de una larga tradicion andina, anterior y posterior al periodo inca.",
            "Conjunto de cordeles anudados. Los quipus incas registraron cantidades mediante nudos y posiciones decimales. Su lectura requeria conocimientos especializados."};
        static readonly string[] MemorySources={
            "https://www.gob.pe/institucion/cultura/noticias/46596-ministerio-de-cultura-celebro-el-v-aniversario-de-la-creacion-del-museo-nacional-de-chavin",
            "https://www.metmuseum.org/art/collection/search/316839",
            "https://library.harvard.edu/about/news/2023-04-12/long-w-2-there-was-quipu-accounting-systems-incan-and-andean-peoples"};
        int MaxHealth=>3+Mathf.Clamp(PlayerPrefs.GetInt("SuyuRun.Upgrade.Vida",0),0,3);
        float MaxBurst=>3+Mathf.Clamp(PlayerPrefs.GetInt("SuyuRun.Upgrade.Rafaga",0),0,3)*.5f;
        void SetupServices()
        {
            liveMusicVolume=PlayerPrefs.GetFloat("SuyuRun.Settings.MusicVol",1);
            music.volume=.34f*liveMusicVolume;
            latency=Mathf.Clamp(PlayerPrefs.GetFloat("SuyuRun.Settings.Latency",0),-.25f,.25f);
            sfx=gameObject.AddComponent<AudioSource>();sfx.playOnAwake=false;
        }
        void Cue(int frequency)
        {
            if(!sfx)return;
            if(!cueClips.TryGetValue(frequency,out var clip))
            {
                const int rate=22050;var samples=new float[2205];
                for(int i=0;i<samples.Length;i++)samples[i]=(Mathf.Repeat(i*frequency/(float)rate,1)<.5f?1:-1)*.12f*(1-i/(float)samples.Length);
                clip=AudioClip.Create("Sierra original cue "+frequency,samples.Length,1,rate,false);clip.SetData(samples,0);cueClips[frequency]=clip;
            }
            sfx.PlayOneShot(clip,PlayerPrefs.GetFloat("SuyuRun.Settings.EffectsVol",1));
        }
        void JudgeAction()
        {
            if(!midiTimeline)return;
            float judged=seconds-latency;
            float beat=midiTimeline.SecondsToBeat(judged);
            float error=Mathf.Abs(judged-midiTimeline.BeatToSeconds(Mathf.Round(beat)));
            if(error<.14f){combo++;score+=error<.075f?25:10;rhythmFeedback=(error<.075f?"PERFECTO":"BIEN")+" · RACHA "+combo;}
            else {combo=0;rhythmFeedback="SIGUE EL PULSO";}
            feedbackUntil=AudioSettings.dspTime+.65;
        }
        void ToggleSierraPause()
        {
            if(cavePhase!=CavePhase.None){ToggleCavePause();return;}
            if(dead||complete)return;
            ClearActionInputs();
            if(!paused){paused=true;pauseDsp=AudioSettings.dspTime;music.Pause();serviceTab=0;}
            else
            {
                double now=AudioSettings.dspTime;paused=false;serviceTab=0;
                if(interlude!=Interlude.None){interludeAt+=now-pauseDsp;music.Stop();}
                else EndInterlude(now);
            }
        }
        bool HasMemory(int index)=>collected.Contains(index)||PlayerPrefs.GetInt("SuyuRun.Museum.Sierra."+MemoryIds[index],0)==1;
        public void DiagnosticPanel(int tab)
        {if(DiagnosticAutoplay){if(!paused&&!complete)ToggleSierraPause();serviceTab=tab;}}
        public void DiagnosticResume(){if(DiagnosticAutoplay&&paused)ToggleSierraPause();}
        void DrawServiceTabs()
        {
            string[] tabs={"RESUMEN","MUSEO","AJUSTES","TIENDA"};
            for(int i=0;i<4;i++)if(RunnerPresentation.Button(new Rect(8+i*78,22,76,10),tabs[i],pixelFont))
            {serviceTab=i;music.Stop();}
        }
        bool DrawServices()
        {
            if(!(paused||complete||dead))return false;
            if(complete&&AudioSettings.dspTime-endingAt<3)return false;
            DrawServiceTabs();
            if(serviceTab==0)return false;
            RunnerPresentation.Box(new Rect(6,34,308,140),RunnerPresentation.Ink);
            RunnerPresentation.Box(new Rect(8,36,304,136),new Color32(76,85,83,255));
            var small=new GUIStyle(label){fontSize=5,wordWrap=true,padding=new RectOffset()};
            if(serviceTab==1)
            {
                for(int i=0;i<3;i++)
                {
                    if(RunnerPresentation.Button(new Rect(14+i*98,40,94,15),MemoryNames[i],pixelFont))museumIndex=i;
                    GUI.color=HasMemory(i)?Color.white:RunnerPresentation.Ink;
                    GUI.color=Color.white;
                    if(!MuseumArt.Draw(new Rect(35+i*98,57,40,32),MemoryIds[i],HasMemory(i)))
                    {GUI.color=HasMemory(i)?Color.white:RunnerPresentation.Ink;RunnerHud.Sprite(new Rect(45+i*98,59,24,24),Frames("coleccionable_"+MemoryIds[i])[0]);GUI.color=Color.white;}
                }
                GUI.Label(new Rect(16,89,288,12),MemoryOrigins[museumIndex],small);
                GUI.Label(new Rect(16,103,288,42),HasMemory(museumIndex)?MemoryTexts[museumIndex]:"Una memoria de "+MemoryOrigins[museumIndex]+". Recogela en Sierra para descubrir su historia completa.",small);
                if(RunnerPresentation.Button(new Rect(16,151,136,13),"CONSULTAR FUENTE",pixelFont))Application.OpenURL(MemorySources[museumIndex]);
                GUI.Label(new Rect(160,152,140,12),"GUARDADO PERMANENTE",small);
            }
            else if(serviceTab==2)
            {
                GUI.Label(new Rect(16,42,280,12),"AUDIO Y CALIBRACION DE JUICIO",small);
                if(RunnerPresentation.Button(new Rect(16,59,28,12),"-",pixelFont))SetSierraVolume(liveMusicVolume-.1f);
                GUI.Label(new Rect(51,62,160,12),"MUSICA "+Mathf.RoundToInt(liveMusicVolume*100)+"%",small);
                if(RunnerPresentation.Button(new Rect(160,59,28,12),"+",pixelFont))SetSierraVolume(liveMusicVolume+.1f);
                if(RunnerPresentation.Button(new Rect(199,59,102,12),"ESCUCHAR / PARAR",pixelFont))
                {if(music.isPlaying)music.Stop();else {music.time=soundtrack.clipStartSeconds+Mathf.Min(seconds,duration-10);music.Play();}}
                if(RunnerPresentation.Button(new Rect(16,78,28,12),"-",pixelFont))SetLatency(latency-.01f);
                GUI.Label(new Rect(51,81,155,12),"COMPENSAR "+Mathf.RoundToInt(latency*1000)+"ms",small);
                if(RunnerPresentation.Button(new Rect(199,78,28,12),"+",pixelFont))SetLatency(latency+.01f);
                if(RunnerPresentation.Button(new Rect(236,78,65,12),"CERO",pixelFont))SetLatency(0);
                GUI.Label(new Rect(16,98,288,25),"Escucha y pulsa T con el ritmo (8 veces). Ajusta el juicio, no modifica el MIDI ni los obstaculos.",small);
                GUI.Label(new Rect(16,127,288,13),"TAPS "+tapErrors.Count+" · TEMPO "+Mathf.RoundToInt(tapBpm)+" BPM",small);
                if(RunnerPresentation.Button(new Rect(16,148,140,14),"APLICAR PROMEDIO",pixelFont,tapErrors.Count>=8))
                {float sum=0;foreach(float v in tapErrors)sum+=v;SetLatency(sum/tapErrors.Count);}
                if(RunnerPresentation.Button(new Rect(164,148,137,14),"REINICIAR TAPS",pixelFont)){tapErrors.Clear();tapIntervals.Clear();lastTap=-1;}
            }
            else
            {
                int wallet=PlayerPrefs.GetInt("SuyuRun.Prototype.Bank");
                GUI.Label(new Rect(16,43,280,12),"BANCO COMPARTIDO: "+wallet,small);
                string[] ids={"Vida","Rafaga","Iman"};string[] names={"VIDA EXTRA","MAS RAFAGA","IMAN DE MONEDAS"};
                for(int i=0;i<3;i++)
                {
                    int level=Mathf.Clamp(PlayerPrefs.GetInt("SuyuRun.Upgrade."+ids[i]),0,3),cost=(level+1)*20;
                    GUI.Label(new Rect(16,65+i*25,162,12),names[i]+" "+level+"/3",small);
                    if(RunnerPresentation.Button(new Rect(186,61+i*25,113,16),level==3?"MAXIMO":cost+" MONEDAS",pixelFont,level<3&&wallet>=cost&&!DiagnosticAutoplay))
                    {
                        if(RunnerProgress.TrySpend(ref wallet,cost))
                        {PlayerPrefs.SetInt("SuyuRun.Prototype.Bank",wallet);PlayerPrefs.SetInt("SuyuRun.Upgrade."+ids[i],level+1);PlayerPrefs.Save();bank=wallet;Cue(900);}
                    }
                }
                GUI.Label(new Rect(16,144,280,22),"Mejoras permanentes. La vida extra se aplica al reiniciar o revivir.",small);
            }
            return true;
        }
        void UpdateServiceInput(Keyboard keyboard)
        {
            if(serviceTab!=2||!(paused||complete||dead)||keyboard==null||!keyboard.tKey.wasPressedThisFrame||!music.isPlaying||!midiTimeline)return;
            double now=AudioSettings.dspTime;float time=music.time-soundtrack.clipStartSeconds;
            float error=time-midiTimeline.BeatToSeconds(Mathf.Round(midiTimeline.SecondsToBeat(time)));
            tapErrors.Add(error);if(tapErrors.Count>16)tapErrors.RemoveAt(0);
            if(lastTap>=0&&now-lastTap>.2&&now-lastTap<2)
            {tapIntervals.Add((float)(now-lastTap));if(tapIntervals.Count>8)tapIntervals.RemoveAt(0);float sum=0;foreach(float v in tapIntervals)sum+=v;tapBpm=60*tapIntervals.Count/sum;}
            lastTap=now;
        }
        void SetSierraVolume(float value){liveMusicVolume=Mathf.Clamp01(value);music.volume=.34f*liveMusicVolume;if(!DiagnosticAutoplay){PlayerPrefs.SetFloat("SuyuRun.Settings.MusicVol",liveMusicVolume);PlayerPrefs.Save();}}
        void SetLatency(float value){latency=Mathf.Clamp(value,-.25f,.25f);if(!DiagnosticAutoplay){PlayerPrefs.SetFloat("SuyuRun.Settings.Latency",latency);PlayerPrefs.Save();}}
    }
}
