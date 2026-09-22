using System.Collections.Generic;
using UnityEngine;

namespace SuyuRun
{
    public sealed partial class RunnerPrototype
    {
        sealed class StructureVisual { public Transform root; public float x, width; }
        readonly List<StructureVisual> structures=new List<StructureVisual>();
        Transform panorama,impulseVisual;
        Sprite panoramaSprite;
        Material panoramaMaterial;
        float panoramaY;
        readonly Color stone=new Color(.68f,.38f,.22f), shadowStone=new Color(.33f,.24f,.25f), sand=new Color(1,.81f,.46f);

        void BuildIllustratedCoast()
        {
            cam.backgroundColor=new Color(.06f,.67f,.9f);
            // ART_BIBLE.md sky (costa_capa1_cielo.png), replacing the old AI panorama crop, per the
            // user's request to bring the bible's approved pixel art into the real playable level.
            // Factor 0.0 per the bible ("cielo... la excepcion explicita al tile; no repetir el sol
            // como capa movil"): completely fixed on screen, never scrolls. A version that drifted
            // with distance (like the old panorama, kept static-but-slow to avoid duplicating its
            // one unrepeatable landmark) eventually scrolled off a ~1240-unit course entirely,
            // leaving the plain camera background color showing - exactly the bug the user caught.
            // Bottom edge is still handed off to the procedural ocean's top edge.
            var texture=Resources.Load<Texture2D>("costa_capa1_cielo");
            if(texture)
            {
                panorama=new GameObject("ART_BIBLE sky (costa_capa1_cielo)").transform;
                panorama.SetParent(world,false);
                panoramaSprite=Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),new Vector2(.5f,.5f),100);
                panoramaMaterial=new Material(Shader.Find("Sprites/Default"));
                var renderer=panorama.gameObject.AddComponent<SpriteRenderer>();renderer.sprite=panoramaSprite;renderer.sharedMaterial=panoramaMaterial;renderer.sortingOrder=-40;
                const float targetHeight=9f;
                float scale=targetHeight/(texture.height/100f);
                panorama.localScale=Vector3.one*scale;
                float halfHeight=targetHeight*.5f;
                panoramaY=.35f+halfHeight;
                panorama.localPosition=new Vector3(0,panoramaY,0);
            }
            else
            {
                Rect("Pacific water",world,0,-1.3f,40,4,new Color(.1f,.71f,.76f),-35);
                for(int i=0;i<8;i++)Shape("Distant coastal cliff",world,new Vector2(-17+i*5,-.2f),new Vector2(9,6),new Color(.9f,.64f,.37f),3,90,-30);
            }
            // Independent near layer: original coastal shrubs, reeds and low dune silhouettes.
            for(int i=0;i<8;i++)
            {
                var root=new GameObject("Parallax coastal vegetation").transform;root.SetParent(world,false);root.position=new Vector3(-24+i*6,Ground-.3f,0);
                for(int j=0;j<3;j++)Shape("Agave leaf",root,new Vector2((j-1)*.16f,.25f),new Vector2(.22f,.8f+(i%3)*.14f),new Color(.1f,.46f,.45f),3,65+j*25,-9);
                scenery.Add(root);sceneryBase.Add(root.position);
            }
            float start=-30;
            foreach(var gap in course.gaps){BuildStructure(start,gap.x-start,0,false);start=gap.x+gap.width;}
            BuildStructure(start,Duration*Speed+30-start,0,false);
            foreach(var p in course.platforms)BuildStructure(p.x,p.width,p.elevation,true,p.style);
            foreach(var gap in course.gaps)
            {
                var root=new GameObject("Gap warning and current").transform;
                for(int i=0;i<3;i++)Shape("Warning chevron",root,new Vector2(-.8f+i*.23f,Ground+.07f),new Vector2(.25f,.18f),new Color(.9f,.3f,.2f),3,0,2);
                Rect("Deep water",root,gap.width*.5f,Ground-2.6f,gap.width,.5f,new Color(.04f,.4f,.56f),-6);
                structures.Add(new StructureVisual{root=root,x=gap.x,width=gap.width});
            }
            var finish=new GameObject("Finish adobe gateway").transform;
            Rect("Left pillar",finish,0,Ground+1.8f,.55f,3.6f,stone,1);
            Rect("Right pillar",finish,3.4f,Ground+1.8f,.55f,3.6f,stone,1);
            Rect("Gateway cap",finish,1.7f,Ground+3.6f,4.3f,.4f,sand,2);
            for(int i=0;i<5;i++)Shape("Finish hanging pennant",finish,new Vector2(.4f+i*.65f,Ground+3.1f),new Vector2(.4f,.6f),i%2==0?teal:gold,3,270,2);
            structures.Add(new StructureVisual{root=finish,x=Duration*Speed-3,width=5});
            BuildCoastalDancer();
            BuildCoastalSwimmer();
            BuildParallaxLayers();
        }

        void BuildStructure(float x,float width,float elevation,bool raised,StructureStyle style=StructureStyle.Terrace)
        {
            // Vector relief walls/panels/facade stairs retired per the user's explicit request:
            // landing structures must be pixel art matching ART_BIBLE.md, not procedural flat-color
            // mesh with a texture patch stuck on top. The whole facade is now the approved seamless
            // ground tile (costa_capa7_suelo) end to end, plus the bible's own pillar accent on
            // raised platforms (BuildPlatformPillars, unchanged). `style` stays on course data for a
            // possible future pixel-art variant per style, but no longer changes the rendering -
            // BuildArcadeFacade/BuildSanctuaryFacade (its old vector-only variants) were removed.
            var root=new GameObject(raised?"Terraced adobe landing structure":"Coastal foundation").transform;
            root.gameObject.AddComponent<UnityEngine.Rendering.SortingGroup>().sortingOrder=raised?0:-3;
            float top=Ground+elevation,bottom=Ground-3.3f;
            structures.Add(new StructureVisual{root=root,x=x,width=width});
            BuildFacadeTexture(root,width,top,bottom,raised);
            // Kept as a plain accent line, not baked art: it is the gameplay affordance the in-run
            // hint text already refers to ("El borde claro es tu apoyo"), marking exactly where the
            // collision top sits, independent of whichever tile texture is under it.
            if(raised)Rect("Readable landing lip",root,width*.5f,top-.015625f,width,.03125f,cream,3);
        }
        Material bibleGroundMaterial;
        void BuildFacadeTexture(Transform root,float width,float top,float bottom,bool raised)
        {
            // The whole structure body is this ART_BIBLE.md-approved seamless ground tile
            // (costa_capa7_suelo) now, not just a narrow inset patch over vector relief walls, per
            // the user's request that landing structures be pixel art end to end.
            if(!bibleGroundMaterial)bibleGroundMaterial=SpriteMaterial("costa_capa7_suelo");
            float fw=width,fh=1f;
            var go=new GameObject("Pixel-art facade (ART_BIBLE ground tile)");go.transform.SetParent(root,false);
            go.transform.localPosition=new Vector3(width*.5f,top-.5f,0);
            go.AddComponent<MeshFilter>().sharedMesh=TexturedQuadMesh(0,0,fw,fh,Mathf.Max(1,fw),Mathf.Max(1,fh));
            var renderer=go.AddComponent<MeshRenderer>();renderer.sharedMaterial=bibleGroundMaterial;renderer.sortingOrder=-1;
            // One curb only: do not repeat the textile frieze down the foundation.
            if(top-1>bottom)Rect("Sand below curb",root,width*.5f,(top-1+bottom)*.5f,width,top-1-bottom,new Color32(243,214,162,255),-2);
        }
        Material pilarMaterial;
        void BuildPlatformPillars(Transform root,float width,float top,float bottom)
        {
            // Decorative, non-tiled accents from costa_plataformas.png (pilar), placed once near
            // each raised platform's edge instead of stretched/tiled as a texture - it is a whole
            // illustrated piece, not a seamless pattern.
            var pilar=NamedSprite("costa_plataformas","pilar");
            if(!pilar||width<2f)return;
            if(!pilarMaterial){pilarMaterial=new Material(Shader.Find("Sprites/Default"));pilarMaterial.mainTexture=pilar.texture;ownedSpriteMaterials.Add(pilarMaterial);}
            float pw=pilar.rect.width/32f,ph=pilar.rect.height/32f;
            var uv=SubRectUv(pilar);
            var go=new GameObject("Pillar accent (ART_BIBLE)");go.transform.SetParent(root,false);
            go.transform.localPosition=new Vector3(width-pw*.6f,bottom+ph*.5f,0);
            go.AddComponent<MeshFilter>().sharedMesh=SubRectQuadMesh(pw,ph,uv);
            var renderer=go.AddComponent<MeshRenderer>();renderer.sharedMaterial=pilarMaterial;renderer.sortingOrder=4;
        }

        void BakeStructure(Transform root)
        {
            // One mesh per depth band instead of a draw call per brick or relief.
            var bands=new Dictionary<int,List<CombineInstance>>();
            var filters=root.GetComponentsInChildren<MeshFilter>();
            foreach(var filter in filters)
            {
                int order=filter.GetComponent<MeshRenderer>().sortingOrder;
                if(!bands.TryGetValue(order,out var band)){band=new List<CombineInstance>();bands.Add(order,band);}
                band.Add(new CombineInstance{mesh=filter.sharedMesh,transform=root.worldToLocalMatrix*filter.transform.localToWorldMatrix});
            }
            foreach(var band in bands)
            {
                var mesh=new Mesh{name="Baked architectural depth "+band.Key};mesh.CombineMeshes(band.Value.ToArray(),true,true);ownedMeshes.Add(mesh);
                var go=new GameObject(mesh.name);go.transform.SetParent(root,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;
                var renderer=go.AddComponent<MeshRenderer>();renderer.sharedMaterial=material;renderer.sortingOrder=band.Key;
            }
            foreach(var filter in filters){filter.gameObject.SetActive(false);Destroy(filter.gameObject);}
        }

        Transform Polygon(string name,Transform parent,Vector2[] points,Color color,int order)
        {
            var root=new GameObject(name);root.transform.SetParent(parent,false);
            var vertices=new Vector3[points.Length];var colors=new Color[points.Length];var triangles=new int[(points.Length-2)*3];
            for(int i=0;i<points.Length;i++){vertices[i]=points[i];colors[i]=color;}
            for(int i=0;i<points.Length-2;i++){triangles[i*3]=0;triangles[i*3+1]=i+1;triangles[i*3+2]=i+2;}
            var mesh=new Mesh{name=name,vertices=vertices,triangles=triangles,colors=colors};mesh.RecalculateBounds();
            ownedMeshes.Add(mesh);
            root.AddComponent<MeshFilter>().sharedMesh=mesh;var r=root.AddComponent<MeshRenderer>();r.sharedMaterial=material;r.sortingOrder=order;
            return root.transform;
        }

        void UpdateCoastalArt()
        {
            // Fixed on screen (bible factor 0.0) - no distance term, so it can never scroll away
            // and expose the background color no matter how long the course runs.
            if(panorama)panorama.localPosition=new Vector3(0,panoramaY,0);
            UpdateParallaxLayers();
            foreach(var s in structures){s.root.position=new Vector3(s.x-distance-3,0,0);s.root.gameObject.SetActive(s.x+s.width>distance-15&&s.x<distance+20);}
            if(!impulseVisual&&hero)
            {
                impulseVisual=new GameObject("Air impulse orbit").transform;impulseVisual.SetParent(hero,false);impulseVisual.localPosition=new Vector3(0,.65f,0);
                for(int i=0;i<4;i++){float a=i*Mathf.PI*.5f;Shape("Orbit spark",impulseVisual,new Vector2(Mathf.Cos(a)*.7f,Mathf.Sin(a)*.7f),Vector2.one*.18f,new Color(.55f,.3f,1),4,0,17);}
            }
            foreach(var orb in items)if(orb.kind==4&&!orb.taken){orb.visual.localScale=Vector3.one*(1+Mathf.Sin(Time.time*5)*.1f);}
            UpdateCoastalDancer();
            AnimateBirds();
        }
    }
}
