using UnityEngine;

namespace SuyuRun
{
    /// <summary>Isolated layer 2. Origin is the top-left pixel of a 320x180 view.
    /// Assign the two layer-2 textures in Inspector after art approval.
    /// No references to, or changes in, the old illustrated runner.</summary>
    public sealed class PixelCostaSkyLayer : MonoBehaviour
    {
        [SerializeField] Texture2D clouds;
        [SerializeField] Texture2D birds;
        [SerializeField] int sortingOrder = -38;
        const float Ppu = 32;
        readonly SpriteRenderer[] cloudTiles = new SpriteRenderer[2];
        readonly SpriteRenderer[] flockBirds = new SpriteRenderer[20];
        readonly Sprite[,] frames = new Sprite[3,6];
        Sprite cloudSprite;
        double epoch, pausedAt, pauseDuration;
        float travelPixels;
        bool ready, paused;
        // The bird sweep below was written for an isolated 320px-wide Pixel Perfect Camera demo,
        // where local x=0 is the screen's LEFT edge. Level_Costa's real camera is much wider
        // (~614px visible at 32 Ppu) and centered on this layer's parent, so that same x=0 sat near
        // screen CENTER there - birds entered already halfway across instead of from the left.
        // Derive the actual visible half-width from whichever camera is live so birds always spawn
        // fully off the true left edge and exit fully past the true right edge.
        float halfSpanPx = 160f;
        bool referenceWindow;
        public void UseReferenceWindow() { referenceWindow=true; }

        public void SetTravelPixels(float pixels) { travelPixels = pixels; }
        public void SetTextures(Texture2D cloudsTexture,Texture2D birdsTexture) { clouds=cloudsTexture; birds=birdsTexture; }
        public void SetPaused(bool value)
        {
            if(value==paused)return;
            if(value)pausedAt=AudioSettings.dspTime;
            else pauseDuration+=AudioSettings.dspTime-pausedAt;
            paused=value;
        }

        void Start()
        {
            if(!clouds||!birds){Debug.LogWarning("Layer 2 needs its approved cloud and bird textures.",this);return;}
            if(clouds.width!=640||clouds.height!=180||birds.width!=96||birds.height!=48)
            {Debug.LogError("Unexpected Costa layer-2 texture dimensions.",this);return;}
            var originalCloud=Sprite.Create(clouds,new Rect(0,0,640,180),new Vector2(0,1),Ppu,0,SpriteMeshType.FullRect);
            cloudSprite=RefactorMaterialArt.Frame("costa_capa2_nubes",originalCloud,0);
            if(cloudSprite!=originalCloud)Destroy(originalCloud);
            for(int i=0;i<2;i++)cloudTiles[i]=Make("Cloud tile "+i,cloudSprite,sortingOrder);
            for(int size=0;size<3;size++)for(int f=0;f<6;f++)
            {
                var legacy=Sprite.Create(birds,new Rect(f*16,(2-size)*16,16,16),new Vector2(0,1),Ppu,0,SpriteMeshType.FullRect);
                frames[size,f]=RefactorMaterialArt.Frame("costa_capa2_aves",legacy,0,size*6+f);
                if(frames[size,f]!=legacy)Destroy(legacy);
            }
            for(int i=0;i<20;i++)flockBirds[i]=Make("Decorative flock bird "+i,frames[1,2],sortingOrder+1);
            var cam=FindFirstObjectByType<Camera>();
            if(cam)halfSpanPx=cam.orthographicSize*cam.aspect*Ppu+32f;
            epoch=AudioSettings.dspTime;pauseDuration=0;ready=true;
        }

        SpriteRenderer Make(string label,Sprite sprite,int order)
        {
            var child=new GameObject(label);child.transform.SetParent(transform,false);
            var renderer=child.AddComponent<SpriteRenderer>();renderer.sprite=sprite;renderer.sortingOrder=order;return renderer;
        }

        void Update()
        {
            if(!ready)return;
            float t=(float)((paused?pausedAt:AudioSettings.dspTime)-epoch-pauseDuration);
            float offset=GameScale.SnapVisual(Mathf.Repeat(travelPixels*.05f+t*.4f,640),true);
            for(int i=0;i<2;i++)if(cloudTiles[i])cloudTiles[i].transform.localPosition=new Vector3((i*640-offset)/Ppu,0,0);
            for(int group=0;group<4;group++)for(int bird=0;bird<5;bird++)
            {
                float phase=Mathf.Repeat(t+group*6,24);int step=(bird+1)/2;
                int side=bird%2==1?1:-1;bool recede=group%2==1;
                int size=recede?Mathf.Min(2,(int)(phase/7)):1;
                float x=GameScale.SnapVisual(-halfSpanPx+phase*(halfSpanPx*2f/24f)-step*(12-size*3),true);
                if(referenceWindow)x=GameScale.SnapVisual(-62+phase*21-step*(12-size*3),true);
                float y=GameScale.SnapVisual(48+group%3*15+side*step*(4-size)-phase*(recede?.7f:0)+Mathf.Sin(t*.8f+group),true);
                float cycle=Mathf.Repeat(t+bird*.13f+group*.4f,3);
                int frame=cycle<.75f?(int)(cycle*8)%6:2;
                var renderer=flockBirds[group*5+bird];
                if(!renderer)continue;
                renderer.sprite=frames[size,frame];renderer.transform.localPosition=new Vector3(x/Ppu,-y/Ppu,0);
                // Recycling takes place offscreen; no random spawn gaps or visible teleport.
                renderer.enabled=referenceWindow?x>-20&&x<340:x>-halfSpanPx-20&&x<halfSpanPx+20;
            }
        }

        void OnDestroy()
        {
            // Refactor sprites belong to the shared resource bank, not this
            // scene. Sierra/menu birds can still be using them after Costa exits.
            if(cloudSprite&&!cloudSprite.name.StartsWith("RefactorMaterial/"))Destroy(cloudSprite);
            foreach(var sprite in frames)if(sprite&&!sprite.name.StartsWith("RefactorMaterial/"))Destroy(sprite);
        }
    }
}
