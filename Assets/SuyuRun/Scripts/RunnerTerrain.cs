using UnityEngine;

namespace SuyuRun
{
    // Pure course queries shared by the controller and Editor regression checks.
    public static class RunnerTerrain
    {
        public const float Floor = -2.6f;
        public static bool HasFloor(RunnerCourse course,float x)
        {
            foreach(var gap in course.gaps)if(x>=gap.x&&x<gap.x+gap.width)return false;
            return true;
        }
        public static float TopAt(RunnerCourse course,float x)
        {
            float top=HasFloor(course,x)?Floor:float.NegativeInfinity;
            foreach(var p in course.platforms)if(x>=p.x&&x<=p.x+p.width)top=Mathf.Max(top,Floor+p.elevation);
            return top;
        }
        public static bool Landing(RunnerCourse course,float x,float oldFeet,float newFeet,out float top)
        {
            top=float.NegativeInfinity;
            if(newFeet>oldFeet)return false;
            if(HasFloor(course,x)&&oldFeet>=Floor-.025f&&newFeet<=Floor)top=Floor;
            foreach(var p in course.platforms)
            {
                float y=Floor+p.elevation;
                if(x+.25f>=p.x&&x-.25f<=p.x+p.width&&oldFeet>=y-.025f&&newFeet<=y)top=Mathf.Max(top,y);
            }
            return !float.IsNegativeInfinity(top);
        }
        public static bool HitsWall(RunnerCourse course,float oldX,float x,float feet)
        {
            foreach(var p in course.platforms)
                if(oldX+.28f<=p.x&&x+.28f>p.x&&feet<Floor+p.elevation-.04f)return true;
            return false;
        }
        public static bool SafeCheckpoint(RunnerCourse course,float x)
        {
            for(float offset=-1;offset<=6;offset+=.5f)
                if(!HasFloor(course,x+offset)||TopAt(course,x+offset)>Floor+.01f)return false;
            foreach(var e in course.encounters)if(Mathf.Abs(course.BeatToSeconds(e.beat)*course.speed-x)<6)return false;
            return true;
        }
    }
}
