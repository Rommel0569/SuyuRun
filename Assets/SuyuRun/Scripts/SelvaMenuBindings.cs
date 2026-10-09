using UnityEngine;
using UnityEngine.SceneManagement;
namespace SuyuRun
{
    public sealed partial class SelvaStageDirector
    {
        bool menuPreview;int previewSample;
        void ApplyMenuSettings()
        {
            bank=PlayerPrefs.GetInt("SuyuRun.Prototype.Bank",0);
            int nextMax=5+Mathf.Clamp(PlayerPrefs.GetInt("SuyuRun.Upgrade.Vida",0),0,3);
            int extra=Mathf.Max(0,nextMax-maxHealth);maxHealth=nextMax;
            health=Mathf.Min(maxHealth,health+extra);
            if(mini!=null){mini.maxHealth=maxHealth;mini.health=health;}
            float volume=PlayerPrefs.GetFloat("SuyuRun.Settings.MusicVol",1);
            music.volume=.34f*musicFade*volume;outgoingMusic.volume=.34f*(1-musicFade)*volume;
        }
        void PreviewMenuMusic()
        {
            if(menuPreview){StopMenuPreview();return;}
            previewSample=music.timeSamples;menuPreview=true;music.UnPause();
            if(!music.isPlaying)music.Play();
        }
        void StopMenuPreview()
        {
            if(!menuPreview)return;music.Pause();music.timeSamples=previewSample;menuPreview=false;
        }
        void ReturnFromSelva(){StopMenuPreview();SceneManager.LoadScene("Level_Costa");}
        void DrawSelvaSharedMenu()
        {
            int previousTab=menuState.tab;
            SharedGameMenu.Draw(menuState,new SharedGameMenu.Context{
                regionName="SELVA",sierra=true,paused=paused,dead=dead,complete=complete,
                diagnostic=DiagnosticAutoplay,coins=coins+(mini?.coins??0),score=score,bank=bank,
                canRevive=dead&&RunnerProgress.CanRevive(coins+(mini?.coins??0)),revive=ReviveInPlace,
                detail=complete?"PATRIMONIO AMAZONICO CONSERVADO":dead?"Reapareces al inicio de este bioma. Conservas las monedas y las piezas del museo.":"La musica y el recorrido estan en pausa.",
                resume=()=>{StopMenuPreview();ApplyMenuSettings();TogglePause();},
                retry=()=>{StopMenuPreview();Retry();},menu=ReturnFromSelva,
                collected=SharedGameMenu.IsCollected,settingsChanged=ApplyMenuSettings,
                previewMusic=PreviewMenuMusic,
                calibrationInfo="LATENCIA COMPARTIDA · AJUSTA CON - / +"
            });
            if(previousTab!=menuState.tab)StopMenuPreview();
        }
    }
}
