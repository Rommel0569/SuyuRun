using UnityEngine;
namespace SuyuRun
{
    // Side-view doorway: fixed body scale, a physical step past the threshold,
    // and pixel clipping at the far jamb instead of shrinking/fading the body.
    public static class RunnerPortalIngress
    {
        public static void Apply(SpriteRenderer hero,Vector3 chamber,float progress)
        {
            if(!hero)return;
            var mask=hero.GetComponent<PortalDoorMask>();
            if(!mask)mask=hero.gameObject.AddComponent<PortalDoorMask>();
            mask.Apply(hero,chamber,progress);
        }
        public static void Frame(SpriteRenderer portal,int heroOrder)
        {
            if(!portal)return;
            var frame=portal.GetComponent<PortalDoorFrame>();
            if(!frame)frame=portal.gameObject.AddComponent<PortalDoorFrame>();
            frame.Set(portal,heroOrder+1);
        }
    }
    public sealed class PortalDoorMask:MonoBehaviour
    {
        Material original,door;
        MaterialPropertyBlock properties;
        public void Apply(SpriteRenderer hero,Vector3 chamber,float progress)
        {
            if(progress<=0){if(original)hero.sharedMaterial=original;return;}
            if(!door)
            {
                original=hero.sharedMaterial;
                var shader=Shader.Find("SuyuRun/PortalThreshold");if(!shader)return;
                door=new Material(shader);
            }
            hero.sharedMaterial=door;
            float t=Mathf.Clamp01(progress);
            hero.transform.position+=Vector3.right*GameScale.SnapVisual(t*56,true)/32;
            if(properties==null)properties=new MaterialPropertyBlock();
            hero.GetPropertyBlock(properties);properties.SetFloat("_DoorPlaneX",chamber.x+24/32f);hero.SetPropertyBlock(properties);
        }
        void OnDestroy(){if(door)Destroy(door);}
    }
    public static class RunnerShotAudio
    {
        static AudioClip clip;
        static readonly System.Collections.Generic.Dictionary<int,AudioSource> sources=new System.Collections.Generic.Dictionary<int,AudioSource>();
        public static void Play(GameObject host)
        {
            if(!clip)clip=Resources.Load<AudioClip>("Sfx/shot");if(!clip)return;
            int id=host.GetInstanceID();
            if(!sources.TryGetValue(id,out var source)||!source){source=host.AddComponent<AudioSource>();source.playOnAwake=false;source.spatialBlend=0;sources[id]=source;}
            source.PlayOneShot(clip,.09f);
        }
    }
    // Reuses the approved texture: back/opening stays behind Tayta and only
    // the right jamb and lintel are drawn in front. No replacement artwork.
    public sealed class PortalDoorFrame:MonoBehaviour
    {
        SpriteRenderer source,right,top;Sprite current;int order;
        public void Set(SpriteRenderer renderer,int foregroundOrder)
        {source=renderer;order=foregroundOrder;UpdateFrame();}
        void LateUpdate(){UpdateFrame();}
        void UpdateFrame()
        {
            if(!source||!source.sprite)return;
            string n=source.sprite.name;
            bool isDoor=n.Contains("portal")||n.Contains("puerta")||n.Contains("tunel");
            if(!isDoor){if(right)right.enabled=false;if(top)top.enabled=false;return;}
            if(current!=source.sprite)
            {
                if(right&&right.sprite)Destroy(right.sprite);if(top&&top.sprite)Destroy(top.sprite);
                current=source.sprite;
                var r=current.rect;float w=Mathf.Round(r.width*.20f),h=Mathf.Round(r.height*.18f);
                Make(ref right,"Door foreground jamb",new Rect(r.xMax-w,r.y,w,r.height),(r.width-w-current.pivot.x)/current.pixelsPerUnit,-current.pivot.y/current.pixelsPerUnit);
                Make(ref top,"Door foreground lintel",new Rect(r.x,r.yMax-h,r.width,h),-current.pivot.x/current.pixelsPerUnit,(r.height-h-current.pivot.y)/current.pixelsPerUnit);
            }
            foreach(var sr in new[]{right,top}){sr.enabled=source.enabled;sr.sortingOrder=order;sr.color=source.color;}
        }
        void Make(ref SpriteRenderer sr,string label,Rect rect,float x,float y)
        {
            if(!sr){var go=new GameObject(label);go.transform.SetParent(transform,false);sr=go.AddComponent<SpriteRenderer>();}
            sr.sharedMaterial=source.sharedMaterial;
            sr.sprite=Sprite.Create(current.texture,rect,Vector2.zero,current.pixelsPerUnit,0,SpriteMeshType.FullRect);
            sr.transform.localPosition=new Vector3(x,y,0);
        }
        void OnDestroy(){if(right&&right.sprite)Destroy(right.sprite);if(top&&top.sprite)Destroy(top.sprite);}
    }
}
