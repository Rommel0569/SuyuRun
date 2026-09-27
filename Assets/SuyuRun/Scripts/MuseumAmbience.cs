using UnityEngine;
namespace SuyuRun
{
    // Scene-owned ambience, independent of the level's MIDI clock.
    public sealed class MuseumAmbience : MonoBehaviour
    {
        AudioSource source;
        public float Mix { get; private set; }
        public void Tick(bool visible,float volume)
        {
            if(!source)
            {
                source=gameObject.AddComponent<AudioSource>();source.playOnAwake=false;
                source.clip=Resources.Load<AudioClip>("Museum/MUSEO");source.loop=true;source.volume=0;
            }
            if(visible&&source.clip&&!source.isPlaying)source.Play();
            Mix=Mathf.MoveTowards(Mix,visible?1:0,Time.unscaledDeltaTime/1.2f);
            source.volume=.28f*Mathf.Clamp01(volume)*Mix;
            if(!visible&&Mix<=0&&source.isPlaying)source.Stop();
        }
    }
}
