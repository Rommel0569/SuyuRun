using UnityEngine;
using UnityEngine.Rendering;

namespace SuyuRun
{
    public sealed partial class RunnerPrototype
    {
        // Scenic silhouettes use muted colors and stay behind every gameplay object.
        // Fishing scenery is an artistic setting, not an archaeological reconstruction.
        void BuildCoastalProps()
        {
            for(int i=0;i<6;i++)
            {
                var root=new GameObject("Coastal vignette "+i).transform;
                root.gameObject.AddComponent<SortingGroup>().sortingOrder=-18;
                // Pottery/vessel vignette removed: against the new pixel-art background it read as
                // an out-of-place floating line of jars, per the user's explicit callout.
                switch(i%2)
                {
                    case 0: BuildFishingPier(root);break;
                    default: BuildReedBundle(root);break;
                }
                BakeStructure(root);
                root.localScale=Vector3.one*.8f;
                Layer(root,-60+i*25,-2.2f,.21f,150);
            }
            BuildCoastalSky();
        }

        void BuildFishingPier(Transform root)
        {
            Color wood=new Color(.46f,.36f,.29f),light=new Color(.72f,.57f,.38f);
            for(int i=0;i<3;i++)
            {
                Rect("Pier pile",root,-.85f+i*.85f,-.23f,.14f,1.05f,wood,0);
                Rect("Pile binding",root,-.85f+i*.85f,.17f,.2f,.09f,light,2);
            }
            Rect("Pier underside",root,0,.11f,2.4f,.18f,wood,1);
            for(int i=0;i<9;i++)Rect("Sunlit deck plank",root,-1.04f+i*.26f,.24f,.24f,.13f,light,2);
            // Basket with visible weave, entirely behind the playable platform lip.
            Shape("Woven fishing basket",root,new Vector2(.5f,.52f),new Vector2(.55f,.5f),light,8,22.5f,3);
            for(int i=0;i<3;i++)Rect("Basket weave",root,.5f,.4f+i*.11f,.42f,.025f,wood,4);
            Shape("Basket opening",root,new Vector2(.5f,.74f),new Vector2(.4f,.08f),wood,12,0,4);
            Rect("Pier mooring post",root,-.75f,.55f,.075f,.65f,wood,3);
            Rect("Mooring rope",root,-.61f,.55f,.035f,.46f,light,4).localRotation=Quaternion.Euler(0,0,30);
        }

        void BuildReedBundle(Transform root)
        {
            for(int i=0;i<7;i++)
            {
                float x=(i-3)*.09f, h=.85f+(i%3)*.15f;
                Rect("Dry totora stalk",root,x,h*.5f,.065f,h,new Color(.65f+i%2*.12f,.57f,.32f),1).localRotation=Quaternion.Euler(0,0,(i-3)*-4);
            }
            Rect("Totora bundle binding",root,0,.32f,.65f,.065f,shadowStone,2);
            Rect("Totora bundle upper binding",root,0,.65f,.48f,.045f,shadowStone,2);
            for(int i=0;i<3;i++)
            {
                float x=.65f+i*.23f;
                Shape("Shell fan",root,new Vector2(x,.04f),new Vector2(.22f,.16f),new Color(.97f,.84f,.65f),7,90,3);
                Rect("Shell ridge",root,x,.05f,.012f,.11f,new Color(.73f,.55f,.41f),4);
            }
        }
    }
}
