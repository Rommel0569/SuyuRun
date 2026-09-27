namespace SuyuRun
{
    // Shared semi-implicit step from Costa. Both controllers consume DSP time in substeps.
    public static class RunnerMotion
    {
        public const float MaxStep=1f/120f;
        public static void Integrate(ref float position,ref float velocity,float acceleration,float dt)
        {velocity+=acceleration*dt;position+=velocity*dt;}
    }
}
