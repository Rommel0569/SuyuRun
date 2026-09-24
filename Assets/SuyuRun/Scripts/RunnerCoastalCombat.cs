using System.Collections.Generic;
using UnityEngine;

namespace SuyuRun
{
    public sealed partial class RunnerPrototype
    {
        sealed class Bird
        {
            public Transform root;public float x,y,amplitude=.13f;public bool defeated;public int culture=-1;
            // Steering state (Reynolds-style Seek + Wander), see SteerBird below.
            public float curY,vy,wanderAngle;
        }
        const float BirdSeekRange=5.5f;   // world units of x-distance at which a bird notices the hero and gives chase
        const float BirdMaxSpeed=3.2f;    // vertical steering speed cap, units/s
        const float BirdMaxForce=10f;     // vertical acceleration cap applied to reach BirdMaxSpeed, units/s^2
        const float BirdWanderJitter=3.4f;// how fast the wander angle drifts, rad/s
        const float BirdWanderRadius=.42f;// how far the wander target strays from the bird's rest height
        sealed class MemoryDrop {public Item source,reward;}
        readonly List<Bird> birds=new List<Bird>();
        readonly List<MemoryDrop> memoryDrops=new List<MemoryDrop>();
        int birdsDefeated;

        void BuildCoastalEncounters()
        {
            // Demo request: show Seek+Wander right away instead of waiting for the first crate.
            CreateBird(6f,-1);
            CreateBird(10f,-1);
            var boxes=items.FindAll(it=>it.kind==2);int index=0;
            foreach(var box in boxes)
            {
                box.visual.name="Memory crate - break with light burst";
                Rect("Crate dark frame",box.visual,0,0,.78f,.96f,new Color(.27f,.12f,.15f),3);
                for(int i=-1;i<=1;i+=2)Rect("Crate riveted edge",box.visual,i*.3f,0,.075f,.85f,gold,6);
                Rect("Crate middle seal",box.visual,0,0,.22f,.24f,cream,7);
                var reward=CreateMemory(box.x+.65f,box.y+.3f,index%4);reward.locked=true;
                memoryDrops.Add(new MemoryDrop{source=box,reward=reward});
                CreateBird(box.x+5.5f,index%3==2?2:-1);
                CreateBird(box.x+9.5f,-1);
                index++;
            }
            // Portrait memories also exist on the route, not only behind combat.
            foreach(var pair in new[]{new Vector2(22,2),new Vector2(152,3)})
            {
                float x=course.BeatToSeconds(pair.x)*Speed;float top=RunnerTerrain.TopAt(course,x);
                if(!float.IsNegativeInfinity(top))CreateMemory(x,top+.95f,(int)pair.y);
            }
            // Patrol birds fill the long flat "just walking" stretches between hazards so there's
            // always something to react to (shoot or jump). Their bob is sized to stay under the
            // hero's jump apex (~2.36 above Ground) and dip into the standing shot's height band
            // (Ground+.55 +-.6), so both a jump and a burst are always viable, never mandatory.
            // The stretch right after the start and the final one before the route's end are left
            // clear (arrival space).
            var markers=new List<float>{0f};
            foreach(var e in course.encounters)markers.Add(course.BeatToSeconds(e.beat)*Speed);
            foreach(var p in course.platforms){markers.Add(p.x);markers.Add(p.x+p.width);}
            foreach(var g in course.gaps){markers.Add(g.x);markers.Add(g.x+g.width);}
            markers.Sort();
            float last=0;
            foreach(var m in markers)
            {
                if(m-last>20f&&last>5f)CreateBird(last+(m-last)*.5f,-1,.7f);
                last=Mathf.Max(last,m);
            }
        }
        Item CreateMemory(float x,float y,int culture)
        {
            var root=BuildCultureVisual(culture,null);root.localScale=Vector3.one*.82f;
            var it=new Item{visual=root,x=x,y=y,width=.62f,height=.9f,kind=6,cultureIndex=culture};items.Add(it);return it;
        }
        void ReleaseMemory(Item source)
        {
            foreach(var drop in memoryDrops)if(drop.source==source)
            {drop.reward.locked=false;feedback="CAJA ABIERTA · +3 MONEDAS · recoge la memoria";feedbackUntil=Time.unscaledTime+2;}
        }
        void CreateBird(float x,int culture,float amplitude=.13f)
        {
            // ART_BIBLE.md "pájaro atacante" (6-frame full flap), replacing the old flat-color
            // vector gallinazo per the user's request that obstacles/enemies be pixel art.
            var root=new GameObject("Pixel obstacle (ART_BIBLE costa_obstaculos pajaro)").transform;
            PixelSprite("Bird sprite",root,ObstacleFrames("pajaro",6),1.1f,8f,7);
            float y=Ground+.7f;
            birds.Add(new Bird{root=root,x=x,y=y,curY=y,amplitude=amplitude,culture=culture,wanderAngle=Random.Range(0f,Mathf.PI*2)});
        }
        void ResetCoastalEncounters(bool resume)
        {
            if(!resume){birdsDefeated=0;foreach(var bird in birds)bird.defeated=false;}
            foreach(var drop in memoryDrops)drop.reward.locked=!drop.source.taken;
        }
        void UpdateCoastalCombat(float dt,float previous)
        {
            if(mode!=Mode.Running)return;
            foreach(var bird in birds)
            {
                if(bird.defeated)continue;
                float sx=bird.x-distance-3;SteerBird(bird,sx,dt);float y=bird.curY;
                for(int i=shots.Count-1;i>=0;i--)
                {
                    var shot=shots[i];if(Mathf.Abs(shot.position.x-sx)>.65f||Mathf.Abs(shot.position.y-y)>.6f)continue;
                    bird.defeated=true;birdsDefeated++;coins+=2;score+=125;Destroy(shot.gameObject);shots.RemoveAt(i);
                    feedback="AVE VENCIDA · +2 MONEDAS";feedbackUntil=Time.unscaledTime+1.2f;Beep(410,.09f);
                    if(bird.culture>=0)DiscoverCulture(bird.culture);
                    break;
                }
                if(!bird.defeated&&bird.x-.45f<distance+.3f&&bird.x+.45f>previous-.3f&&feet<y+.2f&&feet+(slide>0?HeroSlideHeight:HeroStandHeight)>y-.2f)
                {lastFailure="bird at "+bird.x;mode=Mode.Dead;music.Pause();feedback="Salta o usa E ante las aves";feedbackUntil=Time.unscaledTime+3;}
            }
        }
        void AnimateBirds()
        {
            // Wing flap now comes from the pajaro sprite sheet's own frames (FrameAnimator on the
            // child set up in CreateBird), not a per-frame wing-transform rotation.
            foreach(var bird in birds)
            {
                bird.root.gameObject.SetActive(mode!=Mode.Museum&&!bird.defeated&&Mathf.Abs(bird.x-distance)<24);
                bird.root.position=new Vector3(bird.x-distance-3,bird.curY,0);
            }
        }
        // Reynolds-style Seek + Wander confined to the bird's vertical patrol band. The horizontal
        // position keeps coming from the scripted world-scroll (bird.x-distance), which is what
        // lets level markers place encounters precisely; only the altitude is steered, which is
        // what the hero actually has to read and react to (jump, slide or shoot).
        void SteerBird(Bird bird,float sx,float dt)
        {
            float band=Mathf.Max(bird.amplitude,.32f)*3f;
            float targetY;
            if(Mathf.Abs(sx)<BirdSeekRange)
            {
                // Seek: head straight for the hero's current foot height so standing still stops working.
                targetY=Mathf.Clamp(feet,bird.y-band,bird.y+band);
            }
            else
            {
                // Wander: a target drifting around a slowly, randomly turning angle (classic Reynolds wander).
                bird.wanderAngle+=Random.Range(-BirdWanderJitter,BirdWanderJitter)*dt;
                targetY=bird.y+Mathf.Sin(bird.wanderAngle)*Mathf.Min(BirdWanderRadius,band);
            }
            float desiredVelocity=Mathf.Clamp(targetY-bird.curY,-1f,1f)*BirdMaxSpeed;
            float steering=Mathf.Clamp(desiredVelocity-bird.vy,-BirdMaxForce*dt,BirdMaxForce*dt);
            bird.vy=Mathf.Clamp(bird.vy+steering,-BirdMaxSpeed,BirdMaxSpeed);
            bird.curY=Mathf.Clamp(bird.curY+bird.vy*dt,bird.y-band,bird.y+band);
        }
    }
}
