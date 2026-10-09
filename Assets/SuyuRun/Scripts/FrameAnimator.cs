using UnityEngine;

namespace SuyuRun
{
    /// <summary>Plays a fixed set of sliced sprites at a constant frame rate, no interpolation -
    /// used for the pixel-art sea (4 frames/1s per ART_APPROVALS.md), collectible shine, effect
    /// loops, and the hero's per-state animation. Swap Frames + call Play() to change state.</summary>
    public sealed class FrameAnimator : MonoBehaviour
    {
        [SerializeField] Sprite[] frames;
        [SerializeField] float fps = 4f;
        [SerializeField] bool loop = true;
        SpriteRenderer target;
        float t;
        int index;

        public void SetFrames(Sprite[] value, float framesPerSecond, bool loopValue)
        {
            frames = value; fps = framesPerSecond; loop = loopValue; t = 0; index = 0;
            if (target && frames != null && frames.Length > 0) target.sprite = frames[0];
        }

        void Awake() { target = GetComponent<SpriteRenderer>(); }

        void Update()
        {
            if (!target || frames == null || frames.Length == 0 || fps <= 0) return;
            t += Time.deltaTime;
            float frameTime = 1f / fps;
            if (t >= frameTime)
            {
                int steps=Mathf.FloorToInt(t/frameTime);
                t-=steps*frameTime;
                index=loop?(index+steps)%frames.Length:Mathf.Min(index+steps,frames.Length-1);
            }
            target.sprite = frames[index];
        }
    }
}
