using UnityEngine;

namespace SuyuRun
{
    public sealed partial class RunnerPrototype
    {
        bool ownsSong;
        AudioSource menuMusic;
        void SetupMenuMusic()
        {
            if(legacyPrototype)return;
            var intro=Resources.Load<AudioClip>("Inicio");if(!intro)return;
            menuMusic=gameObject.AddComponent<AudioSource>();menuMusic.clip=intro;menuMusic.loop=true;menuMusic.volume=.28f;menuMusic.Play();
        }
        void ReturnToMenu()
        {
            music.Stop();mode=Mode.Menu;burst=0;
            if(menuMusic&&!menuMusic.isPlaying)menuMusic.Play();
        }
        void LoadMusic()
        {
            ownsSong=!course.HasRecordedMusic;
            song=ownsSong?Compose():course.soundtrack.clip;
            music.clip=song;
            music.loop=ownsSong; // Only the authored placeholder is a seamless loop.
        }
        void SeekMusic(float levelSeconds)
        {
            float position=course.HasRecordedMusic?course.soundtrack.clipStartSeconds+levelSeconds:Mathf.Repeat(levelSeconds,song.length);
            music.time=Mathf.Clamp(position,0,Mathf.Max(0,song.length-.01f));
        }
        void StartMusic(float levelSeconds)
        {
            music.Stop();SeekMusic(levelSeconds);
            double scheduled=AudioSettings.dspTime+.1;
            started=scheduled-levelSeconds;
            music.PlayScheduled(scheduled);
        }
        float MusicalBeat => course.SecondsToBeat(distance/Speed);
    }
}
