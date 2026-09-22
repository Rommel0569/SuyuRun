using System;
using UnityEngine;

namespace SuyuRun
{
    public enum EncounterKind { Jump, Slide, Breakable }
    [Serializable]
    public struct Encounter
    {
        [Min(0)] public float beat;
        public EncounterKind kind;
        [Range(1,3)] public int count;
    }

    public enum StructureStyle { Terrace, RecessedArcade, SteppedSanctuary }
    [Serializable] public struct CoursePlatform { public float x, width, elevation; public StructureStyle style; }
    [Serializable] public struct CourseGap { public float x, width; }
    [Serializable] public struct CourseOrb { public float x, elevation; }

    [CreateAssetMenu(menuName="SuyuRun/Runner course")]
    public sealed class RunnerCourse : ScriptableObject
    {
        public string region="COSTA";
        [Min(1)] public float speed=7;
        [Range(40,200)] public float bpm=120;
        [Min(8)] public float duration=64;
        [Min(4)] public float checkpointSeconds=16;
        public Encounter[] encounters=Array.Empty<Encounter>();
        public CoursePlatform[] platforms=Array.Empty<CoursePlatform>();
        public CourseGap[] gaps=Array.Empty<CourseGap>();
        public CourseOrb[] orbs=Array.Empty<CourseOrb>();
        public float[] jumpPads=Array.Empty<float>();
        public RunnerSoundtrack soundtrack;
        public bool HasRecordedMusic => soundtrack&&soundtrack.clip;
        public float EffectiveBpm => HasRecordedMusic?soundtrack.bpm:bpm;
        public float BeatToSeconds(float beat)=>HasRecordedMusic?soundtrack.BeatToSeconds(beat):beat*60f/bpm;
        public float SecondsToBeat(float seconds)=>HasRecordedMusic?soundtrack.SecondsToBeat(seconds):seconds*bpm/60f;

        public void ValidateCourse()
        {
            if(speed<=0||bpm<=0||duration<=0||checkpointSeconds<=0)throw new InvalidOperationException("Course timing must be positive.");
            if(soundtrack)soundtrack.ValidateForDuration(duration);
            float previous=-1;
            foreach(var item in encounters)
            {
                if(item.beat<=previous||BeatToSeconds(item.beat)<0||BeatToSeconds(item.beat)>=duration)throw new InvalidOperationException("Encounters must be ordered, unique and inside the course.");
                previous=item.beat;
            }
            foreach(var p in platforms)if(p.width<=0||p.elevation<=0||p.x<0||p.x+p.width>speed*duration)throw new InvalidOperationException("Invalid platform bounds.");
            float end=-1;
            foreach(var g in gaps){if(g.width<=0||g.x<end||g.x+g.width>speed*duration)throw new InvalidOperationException("Invalid or overlapping gaps.");end=g.x+g.width;}
            foreach(var o in orbs)if(o.x<0||o.x>speed*duration||o.elevation<0)throw new InvalidOperationException("Invalid air impulse point.");
            foreach(float x in jumpPads)if(x<0||x>speed*duration||!RunnerTerrain.HasFloor(this,x)||RunnerTerrain.TopAt(this,x)>RunnerTerrain.Floor+.01f)throw new InvalidOperationException("Jump pad must stand on clear floor.");
        }
        public static RunnerCourse Prototype()
        {
            var c=CreateInstance<RunnerCourse>();
            int[] beats={12,20,28,36,44,52,60,68,76,84,92,100,108,116};
            int[] types={0,0,1,0,2,1,0,1,2,0,1,2,0,0};
            c.encounters=new Encounter[beats.Length];
            for(int i=0;i<beats.Length;i++)c.encounters[i]=new Encounter{beat=beats[i],kind=(EncounterKind)types[i]};
            return c;
        }
        public static RunnerCourse VerticalPrototype()
        {
            var c=Prototype();
            float[] beats={12,82/3.5f,28,40,48,60,76,302/3.5f,92,112,120};
            int[] types={0,0,1,2,0,1,2,0,1,2,0};
            c.encounters=new Encounter[beats.Length];
            for(int i=0;i<beats.Length;i++)c.encounters[i]=new Encounter{beat=beats[i],kind=(EncounterKind)types[i],count=i==0||i==10?2:1};
            c.platforms=new[]{
                new CoursePlatform{x=60,width=12,elevation=1},new CoursePlatform{x=76,width=11,elevation=2.2f,style=StructureStyle.SteppedSanctuary},
                new CoursePlatform{x=180,width=10,elevation=1.2f,style=StructureStyle.RecessedArcade},new CoursePlatform{x=194,width=10,elevation=2.3f,style=StructureStyle.SteppedSanctuary},
                new CoursePlatform{x=280,width=12,elevation=1,style=StructureStyle.RecessedArcade},new CoursePlatform{x=296,width=12,elevation=2,style=StructureStyle.SteppedSanctuary},
                new CoursePlatform{x=360,width=13,elevation=1.3f,style=StructureStyle.RecessedArcade}};
            c.gaps=new[]{new CourseGap{x=120,width=7.5f},new CourseGap{x=238,width=7.5f},new CourseGap{x=340,width=7.5f}};
            c.orbs=new[]{new CourseOrb{x=123,elevation=2},new CourseOrb{x=241,elevation=2},new CourseOrb{x=343,elevation=2}};
            c.jumpPads=new[]{176f};
            return c;
        }
    }
}
