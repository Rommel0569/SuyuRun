namespace SuyuRun
{
    public sealed class RunnerAirClock
    {
        bool flying,descending;int lastJump;float began,last;
        public float Age(bool grounded,int jumps,float downwardVelocity,float now)
        {
            if(grounded||now<last){flying=false;descending=false;lastJump=0;began=now;}
            else
            {
                bool falling=downwardVelocity>=0;
                if(!flying||jumps>lastJump||falling!=descending)began=now;
                flying=true;descending=falling;lastJump=jumps;
            }
            last=now;return now-began;
        }
    }
    // Local time for one-shot poses; a shared distance clock keeps run/fire legs in phase.
    public sealed class RunnerPoseClock
    {
        string previous;
        float began,last;
        public float Age(string state,float now)
        {
            if(previous!=state||now<last){previous=state;began=now;}
            last=now;
            return now-began;
        }
    }
}
