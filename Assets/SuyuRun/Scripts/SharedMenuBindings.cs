using UnityEngine;
namespace SuyuRun
{
    public sealed partial class RunnerPrototype
    {
        readonly SharedGameMenu.State sharedMenu=new SharedGameMenu.State();
        Mode sharedPreviousMode=(Mode)(-1);
        bool SharedWindowVisible=>!legacyPrototype&&(mode==Mode.Paused||mode==Mode.Dead||mode==Mode.Complete||mode==Mode.Museum||mode==Mode.Settings||mode==Mode.Shop);
        bool SharedMuseumVisible=>SharedWindowVisible&&(sharedPreviousMode!=mode?mode==Mode.Museum:sharedMenu.tab==1);
        bool DrawSharedWindow()
        {
            if(!SharedWindowVisible){sharedPreviousMode=(Mode)(-1);return false;}
            if(sharedPreviousMode!=mode)
            {sharedMenu.tab=mode==Mode.Museum?1:mode==Mode.Settings?2:mode==Mode.Shop?3:0;sharedPreviousMode=mode;}
            Matrix4x4 saved=GUI.matrix;GUI.matrix=saved*Matrix4x4.Scale(Vector3.one*4);
            SharedGameMenu.Draw(sharedMenu,new SharedGameMenu.Context{
                sierra=false,paused=mode==Mode.Paused,dead=mode==Mode.Dead,complete=mode==Mode.Complete,
                diagnostic=AutomatedSmokeTest,coins=coins,score=score,bank=bank,canRevive=checkpoint>0&&RunnerProgress.CanRevive(coins),
                detail=mode==Mode.Complete?"SIERRA DESBLOQUEADA":mode==Mode.Dead?"Salta, desliza o dispara segun el obstaculo.":"La musica y el recorrido estan en pausa.",
                resume=()=>{LoadUpgrades();TogglePause();},retry=()=>Begin(),menu=ReturnToMenu,next=StartCoastTravel,
                revive=()=>{if(RunnerProgress.TryRevive(ref coins))Begin(true);},
                settingsChanged=()=>{LoadSettings();LoadUpgrades();bank=PlayerPrefs.GetInt("SuyuRun.Prototype.Bank");},
                collected=(region,index)=>PlayerPrefs.GetInt("SuyuRun.Museum."+(region==0?"Costa."+CoastalCulture.Entries[index].id:"Sierra."+SharedGameMenu.SierraIds[index]),0)==1||(region==0&&discovered[index])
            });
            GUI.matrix=saved;return true;
        }
    }
    public sealed partial class SierraStageDirector
    {
        readonly SharedGameMenu.State sharedMenu=new SharedGameMenu.State{region=1};
        bool DrawSharedWindow()
        {
            bool caveFailed=cavePhase!=CavePhase.None&&cave!=null&&cave.dead;
            if(!(paused||dead||caveFailed||complete))return false;
            if(complete&&AudioSettings.dspTime-endingAt<3)return false;
            sharedMenu.tab=serviceTab;
            SharedGameMenu.Draw(sharedMenu,new SharedGameMenu.Context{
                sierra=true,paused=paused,dead=dead||caveFailed,complete=complete,diagnostic=DiagnosticAutoplay,
                coins=coins+(cave?.coins??0),score=score,bank=bank,canRevive=(caveFailed||dead&&checkpointTime>0)&&RunnerProgress.CanRevive(coins+(cave?.coins??0)),
                detail=complete?"TRAJE PROTECTOR OBTENIDO · SELVA DESBLOQUEADA":caveFailed?"Reintenta la cueva sin perder el recorrido.":dead?"Usa las marcas para preparar el salto.":"La musica y el recorrido estan en pausa.",
                resume=ToggleSierraPause,retry=()=>{if(caveFailed)RetryCave();else Restart();},revive=Revive,
                menu=()=>UnityEngine.SceneManagement.SceneManager.LoadScene("Level_Costa"),next=()=>RunnerProgress.OpenSelva(),
                settingsChanged=()=>{liveMusicVolume=PlayerPrefs.GetFloat("SuyuRun.Settings.MusicVol",1);music.volume=.34f*liveMusicVolume;latency=PlayerPrefs.GetFloat("SuyuRun.Settings.Latency",0);bank=PlayerPrefs.GetInt("SuyuRun.Prototype.Bank");},
                collected=(region,index)=>region==1?HasMemory(index):PlayerPrefs.GetInt("SuyuRun.Museum.Costa."+CoastalCulture.Entries[index].id,0)==1,
                calibrationInfo="TAPS "+tapErrors.Count+" · "+Mathf.RoundToInt(tapBpm)+" BPM · PULSA T",
                previewMusic=()=>{if(music.isPlaying)music.Stop();else {music.time=soundtrack.clipStartSeconds+Mathf.Min(seconds,duration-10);music.Play();}},
                applyCalibration=tapErrors.Count<8?null:(System.Action)(()=>{float sum=0;foreach(float v in tapErrors)sum+=v;SetLatency(sum/tapErrors.Count);}),
                resetCalibration=()=>{tapErrors.Clear();tapIntervals.Clear();lastTap=-1;}
            });
            if(serviceTab!=sharedMenu.tab){serviceTab=sharedMenu.tab;music.Stop();}
            return true;
        }
    }
}
