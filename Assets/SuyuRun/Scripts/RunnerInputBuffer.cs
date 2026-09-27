namespace SuyuRun
{
    // Render-frame presses survive frames without a physics/DSP step and early landings.
    public sealed class RunnerInputBuffer
    {
        public const double Grace=.18;
        double expires=double.NegativeInfinity;
        public void Press(double now){expires=now+Grace;}
        public bool Pending(double now)=>now<=expires;
        public bool Consume(double now)
        {bool pending=Pending(now);Clear();return pending;}
        public void Clear(){expires=double.NegativeInfinity;}
    }
}
