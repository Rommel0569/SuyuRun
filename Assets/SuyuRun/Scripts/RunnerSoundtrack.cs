using System;
using UnityEngine;

namespace SuyuRun
{
    [CreateAssetMenu(menuName="SuyuRun/Soundtrack and beat grid")]
    public sealed class RunnerSoundtrack : ScriptableObject
    {
        public AudioClip clip;
        public string trackTitle="Pendiente de archivo autorizado";
        [TextArea] public string creditAndPermission;
        [Min(0)] public float clipStartSeconds;
        [Range(30,240)] public float bpm=120;
        [Tooltip("Seconds from the selected excerpt's start to its first beat.")]
        public float firstBeatSeconds;
        [Tooltip("Optional measured beat times, in seconds from the excerpt start. For live tempo changes. Leave empty for constant BPM.")]
        public float[] beatTimes=Array.Empty<float>();

        public float BeatToSeconds(float beat)
        {
            if(beatTimes==null||beatTimes.Length<2)return firstBeatSeconds+beat*60f/bpm;
            int index=Mathf.Clamp(Mathf.FloorToInt(beat),0,beatTimes.Length-2);
            return Mathf.LerpUnclamped(beatTimes[index],beatTimes[index+1],beat-index);
        }
        public float SecondsToBeat(float seconds)
        {
            if(beatTimes==null||beatTimes.Length<2)return (seconds-firstBeatSeconds)*bpm/60f;
            int lo=0,hi=beatTimes.Length-1;
            while(hi-lo>1){int mid=(hi+lo)/2;if(beatTimes[mid]<=seconds)lo=mid;else hi=mid;}
            return lo+(seconds-beatTimes[lo])/(beatTimes[lo+1]-beatTimes[lo]);
        }
        public void ValidateForDuration(float duration)
        {
            if(bpm<=0||float.IsNaN(bpm)||clipStartSeconds<0||float.IsNaN(firstBeatSeconds))throw new InvalidOperationException("Invalid soundtrack timing.");
            if(beatTimes!=null)
                for(int i=0;i<beatTimes.Length;i++)if(float.IsNaN(beatTimes[i])||float.IsInfinity(beatTimes[i])||(i>0&&beatTimes[i]<=beatTimes[i-1]))throw new InvalidOperationException("Beat timestamps must be finite and strictly increasing.");
            if(clip&&clipStartSeconds+duration>clip.length+.01f)throw new InvalidOperationException("Soundtrack is shorter than this course excerpt. Adjust duration or clip start; do not loop an unmeasured song.");
        }
    }
}
