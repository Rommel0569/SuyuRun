using UnityEngine;

namespace SuyuRun
{
    // Ajustes screen (ART_BIBLE.md section 4 lists it in the start panel's button row). The inicio
    // art only has 5 baked button frames and TIENDA already took the slot the bible drew as
    // AJUSTES, so this is reached via a small link in the title screen's empty sky area instead of
    // a sixth pixel-art button - flagged, not silently dropped. Volume is the concrete, working
    // part; full latency/tap-tempo calibration (bible section 14.4) is a bigger feature left for
    // later, noted on-screen rather than faked.
    public sealed partial class RunnerPrototype
    {
        const float BaseMusicVolume=.34f, BaseMenuMusicVolume=.28f, BaseEffectsVolume=.23f;
        float musicVolumeMul=1f, effectsVolumeMul=1f;
        void LoadSettings()
        {
            musicVolumeMul=PlayerPrefs.GetFloat("SuyuRun.Settings.MusicVol",1f);
            effectsVolumeMul=PlayerPrefs.GetFloat("SuyuRun.Settings.EffectsVol",1f);
            ApplyVolumeSettings();
        }
        void ApplyVolumeSettings()
        {
            if(music)music.volume=BaseMusicVolume*musicVolumeMul;
            if(menuMusic)menuMusic.volume=BaseMenuMusicVolume*musicVolumeMul;
            if(effects)effects.volume=BaseEffectsVolume*effectsVolumeMul;
        }
        void DrawSettings()
        {
            GUI.Label(new Rect(40,20,1200,60),"AJUSTES",title);
            Rect panelRect=new Rect(40,120,600,300);
            Panel(panelRect,new Color(.025f,.065f,.11f,.9f));
            GUI.Label(new Rect(panelRect.x+24,panelRect.y+20,500,30),"MÚSICA",text);
            float newMusic=GUI.HorizontalSlider(new Rect(panelRect.x+24,panelRect.y+56,480,24),musicVolumeMul,0,1);
            GUI.Label(new Rect(panelRect.x+24,panelRect.y+120,500,30),"EFECTOS",text);
            float newEffects=GUI.HorizontalSlider(new Rect(panelRect.x+24,panelRect.y+156,480,24),effectsVolumeMul,0,1);
            if(!Mathf.Approximately(newMusic,musicVolumeMul)||!Mathf.Approximately(newEffects,effectsVolumeMul))
            {
                musicVolumeMul=newMusic;effectsVolumeMul=newEffects;
                ApplyVolumeSettings();
                PlayerPrefs.SetFloat("SuyuRun.Settings.MusicVol",musicVolumeMul);
                PlayerPrefs.SetFloat("SuyuRun.Settings.EffectsVol",effectsVolumeMul);
                PlayerPrefs.Save();
            }
            GUI.Label(new Rect(panelRect.x+24,panelRect.y+220,540,70),"Calibración de latencia y modo tap-tempo: todavía no implementado, queda pendiente.",small);
            if(GUI.Button(new Rect(40,440,260,44),"VOLVER AL INICIO",button))ReturnToMenu();
        }
    }
}
