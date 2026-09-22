using UnityEngine;

namespace SuyuRun
{
    public sealed partial class RunnerPrototype
    {
        Transform danceIsland;
        readonly Transform[] repeatedDanceIslands=new Transform[3];
        Sprite[] dancerFrames;
        SpriteRenderer dancerRenderer;
        readonly SpriteRenderer[] repeatedDancerRenderers=new SpriteRenderer[3];

        void BuildCoastalDancer()
        {
            // Small environmental performer, not a collectible, hazard or caricature.
            danceIsland=new GameObject("Coastal dance vignette (ART_BIBLE pixel art)").transform;
            danceIsland.SetParent(world,false);
            danceIsland.gameObject.AddComponent<UnityEngine.Rendering.SortingGroup>().sortingOrder=-20;
            // Rock under the performer is now the same approved pixel-art block used for playable
            // platforms (costa_plataformas.png "bloque"), per the user's request that it be pixel
            // art too - reusing an already-approved asset, not a new one.
            var bloque=NamedSprite("costa_plataformas","bloque");
            if(bloque)
            {
                var rockGo=new GameObject("Rock (ART_BIBLE bloque)");rockGo.transform.SetParent(danceIsland,false);
                float rw=bloque.rect.width/32f,rh=bloque.rect.height/32f;
                rockGo.transform.localPosition=new Vector3(-rw*.5f,0,0);
                var rockMat=new Material(Shader.Find("Sprites/Default"));rockMat.mainTexture=bloque.texture;ownedSpriteMaterials.Add(rockMat);
                rockGo.AddComponent<MeshFilter>().sharedMesh=SubRectQuadMesh(rw,rh,SubRectUv(bloque));
                var rockRenderer=rockGo.AddComponent<MeshRenderer>();rockRenderer.sharedMaterial=rockMat;rockRenderer.sortingOrder=0;
            }
            // Dancer figure: new small pixel-art sprite (costa_bailarin.png, pending the user's
            // approval) - a zambo chinchano dancing festejo. Per the user's follow-up, the original
            // vector rig's LIVELY MOVEMENT was already right and had to be kept (even improved), not
            // flattened into a simple pose swap: this is a 6-frame zapateo footwork cycle (kick-
            // stomp-recover on each side), stepped through by the same musical phase that used to
            // drive the limb IK, not a free-running fps loop.
            // Resources.LoadAll doesn't guarantee sheet order, so filter by exact frame name like
            // HeroFrames/ObstacleFrames do, instead of trusting array position.
            var bailarinSheet=Resources.LoadAll<Sprite>("costa_bailarin");
            dancerFrames=new Sprite[6];
            if(bailarinSheet!=null)foreach(var s in bailarinSheet)for(int i=0;i<6;i++)if(s.name=="baile_"+i)dancerFrames[i]=s;
            var dancerGo=new GameObject("Dancer (ART_BIBLE costa_bailarin)");dancerGo.transform.SetParent(danceIsland,false);
            float dw=.75f,dh=1.25f; // 24x40 native at PPU 32
            dancerGo.transform.localPosition=new Vector3(-dw*.5f,.05f+dh,0);
            dancerRenderer=dancerGo.AddComponent<SpriteRenderer>();dancerRenderer.sortingOrder=1;
            if(dancerFrames!=null&&dancerFrames.Length>0)dancerRenderer.sprite=dancerFrames[0];

            for(int i=0;i<repeatedDanceIslands.Length;i++)
            {
                repeatedDanceIslands[i]=Instantiate(danceIsland,world);repeatedDanceIslands[i].name="Repeated coastal dance vignette "+i;
                repeatedDancerRenderers[i]=repeatedDanceIslands[i].GetComponentInChildren<SpriteRenderer>();
            }
            UpdateCoastalDancer();
        }

        void UpdateCoastalDancer()
        {
            if(!danceIsland)return;
            // Same musical timeline as the runner; pausing/dying/checkpoint-seeking freezes/restores
            // which frame is showing, same as before.
            float beat=mode==Mode.Menu||mode==Mode.Museum?waterTime*course.EffectiveBpm/60f:MusicalBeat;
            if(dancerFrames!=null&&dancerFrames.Length>=6)
            {
                // Same 2-beat swing period the old limb IK used (Mathf.Sin(beat*Mathf.PI)), now
                // stepping through the 6-frame kick-stomp-recover cycle instead of posing joints.
                float phase=Mathf.Repeat(beat*.5f,1f);
                int index=Mathf.Clamp(Mathf.FloorToInt(phase*dancerFrames.Length),0,dancerFrames.Length-1);
                var sprite=dancerFrames[index];
                if(sprite){dancerRenderer.sprite=sprite;foreach(var r in repeatedDancerRenderers)if(r)r.sprite=sprite;}
            }
            danceIsland.localPosition=new Vector3(Mathf.Repeat(5-distance*.16f+36,72)-36,-1.27f,0);
            for(int i=0;i<repeatedDanceIslands.Length;i++)
                repeatedDanceIslands[i].localPosition=new Vector3(Mathf.Repeat(5+(i+1)*18-distance*.16f+36,72)-36,-1.27f,0);
        }
    }
}
