using UnityEngine;
namespace SuyuRun
{
    // Item data extracted from RunnerPrototype; coordinates remain owned by each course.
    public class RunnerPickup
    {
        public Transform visual;
        public float x,y,width,height;
        public int kind;
        public bool taken,locked;
        public int cultureIndex=-1;
        public bool TryCollect(Vector2 position,Vector2 halfSize)
        {
            if(taken||locked||Mathf.Abs(x-position.x)>width*.5f+halfSize.x||Mathf.Abs(y-position.y)>height*.5f+halfSize.y)return false;
            taken=true;return true;
        }
    }
}
