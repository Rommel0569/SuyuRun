using UnityEngine;
using UnityEngine.InputSystem;
namespace SuyuRun
{
    public sealed partial class SierraStageDirector
    {
        enum Interlude { None, Arrival, Guide, Trap, Rewind }
        Interlude interlude;
        double interludeAt;
        int escapePresses;
        bool guideSeen;
        float rewindFrom;
        string narrativeLine;
        int escapedTraps,paidRevives;
        float NarrativeTime=>(float)((paused?pauseDsp:AudioSettings.dspTime)-interludeAt);
        void UpdateSierraCamera()
        {
            Vector3 home=new Vector3(5,180f/64,-10);
            float approach=0;bool close=false;float focusX=HeroX;
            double now=paused?pauseDsp:AudioSettings.dspTime;
            if(interlude==Interlude.Arrival)
            {
                float age=(float)(now-interludeAt);
                approach=1-Mathf.SmoothStep(0,1,(age-1.5f)/.85f);close=age<1.65f;
                focusX=88;
            }
            else if(complete)
            {
                float age=(float)(now-endingAt);
                approach=age<.6f?age/.6f:1-Mathf.Clamp01((age-2.1f)/.7f);
                close=age>.55f&&age<2.15f;focusX=160;
            }
            var focus=new Vector3(focusX/32,(180-Surface(seconds*Speed+focusX,act)+36)/32,-10);
            RunnerPresentation.CameraPose(view,home,focus,approach,close);
        }
        public string NarrativeReport=>$"Escapes={escapedTraps}; PaidRevives={paidRevives}; Guide={guideSeen}";
        public void DiagnosticTrap()
        {if(DiagnosticAutoplay&&interlude==Interlude.None)BeginTrap();}
        public void DiagnosticRevive()
        {if(DiagnosticAutoplay&&checkpointTime>0&&coins>=RunnerProgress.ReviveCost){Fail();Revive();}}

        void BeginInterlude(Interlude next)
        {
            interlude=next;interludeAt=AudioSettings.dspTime;music.Pause();
        }
        void BeginTrap()
        {
            escapePresses=0;BeginInterlude(Interlude.Trap);
            EffectAt("salpicadura",seconds*Speed+HeroX-16,feet-20);
        }
        bool UpdateInterlude(Keyboard keyboard,double now)
        {
            if(interlude==Interlude.None&&!guideSeen&&seconds>duration/3+2&&!dead&&!complete&&!paused)
            {guideSeen=true;BeginInterlude(Interlude.Guide);}
            if(interlude==Interlude.None)return false;
            float t=(float)(now-interludeAt);
            if(interlude==Interlude.Arrival)
            {
                narrativeLine=t<1.2f?"FRANCISCO: ¡El portal me trajo a los Andes!":"¡Sigamos el camino!";
                if(t>=2.5f)EndInterlude(now);
            }
            else if(interlude==Interlude.Guide)
            {
                narrativeLine=t<1.8f?"GUARDIAN: ¡Las aves atacan las cuerdas!":"FRANCISCO: ¡Las detendré con la huaraca!";
                if(t>=3.5f||(keyboard!=null&&keyboard.enterKey.wasPressedThisFrame))EndInterlude(now);
            }
            else if(interlude==Interlude.Trap)
            {
                if(keyboard!=null&&keyboard.spaceKey.wasPressedThisFrame)escapePresses++;
                if(DiagnosticAutoplay)escapePresses=Mathf.FloorToInt(t*5);
                narrativeLine="¡BARRO! ESPACIO 3 VECES: "+Mathf.Min(3,escapePresses)+"/3";
                if(escapePresses>=3){escapedTraps++;damageAt=seconds;EndInterlude(now);}
                else if(t>2.2f){health--;damageAt=seconds;EndInterlude(now);if(health<=0)Fail();}
            }
            else if(interlude==Interlude.Rewind)
            {
                narrativeLine="REGRESANDO AL PUNTO DE RETORNO...";
                seconds=Mathf.Lerp(rewindFrom,checkpointTime,Mathf.SmoothStep(0,1,t/.8f));act=ActAt(seconds);
                feet=Surface(seconds*Speed+HeroX,act);
                if(t>=.8f){interlude=Interlude.None;Restart(true);}
            }
            return true;
        }
        void EndInterlude(double now)
        {
            interlude=Interlude.None;narrativeLine="";epoch=now-seconds;previous=now;
            if(!dead&&!complete)
            {
                music.Stop();music.time=soundtrack.clipStartSeconds+seconds;
                epoch=now+.1-seconds;previous=now+.1;music.PlayScheduled(now+.1);
                runnerFadeAt=now+.1;music.volume=0;
            }
        }
        void RenderNarrative()
        {
            float t=NarrativeTime;
            if(interlude==Interlude.Arrival)
            {
                Draw("portal_ceremonial",20,Surface(48,act)-106,14,t);
                RunnerPortalIngress.Frame(pool[used-1],15);
                if(t<1.4f)Draw("fx_teleport_anillo",16,Surface(48,act)-21,19,t);
                if(t>1.45f&&t<2.3f)Draw("efecto_sorpresa",HeroX-16,feet-75,19,t-1.45f);
            }
            if(interlude==Interlude.Guide)
            {
                Draw(t<1.5f?"guardian_huaraca":"guardian_idle",176,Surface(seconds*Speed+200,act)-47,16,t);
                FaceLast(RunnerFacing.TowardLeft(200,HeroX));
                if(t>1.8f&&t<2.6f)Draw("efecto_sorpresa",HeroX-16,feet-75,19,t-1.8f);
            }
            if(interlude==Interlude.Trap)Draw("fx_salpicadura",HeroX-16,feet-13,16,Mathf.Repeat(t,.6f));
        }
    }
}
