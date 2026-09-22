using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.U2D;

namespace SuyuRun.Editor
{
    // Assembles every approved Assets/SuyuRun/Art/PixelCosta/ asset into a working, visible
    // scene: Pixel Perfect Camera at the bible's 320x180 reference resolution, all 7 parallax
    // layers, ground, a sample of platforms/hero/obstacles/collectibles, and a demo scroll
    // driver so Play actually shows the stack moving together. Built in Prototype_Costa.unity,
    // NOT Level_Costa.unity - the existing vector/mesh runner keeps working untouched there
    // while this integration is verified; switching Level_Costa over to this is a separate,
    // deliberate later step (see PROGRESS.md).
    public static class AssembleCostaPixelLevel
    {
        const string ScenePath = "Assets/SuyuRun/Scenes/Prototype_Costa.unity";
        const string Art = "Assets/SuyuRun/Art/PixelCosta/";
        const float Ppu = 32f;
        const float ViewTop = 180f / 2f / Ppu; // world Y of the viewport's top edge (camera at origin)

        [MenuItem("SuyuRun/Pixel Art/Ensamblar nivel Costa pixel art")]
        public static void Assemble()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath);
            var old = GameObject.Find("Pixel Costa Level");
            if (old) Object.DestroyImmediate(old);

            // Prototype_Costa.unity already contained a legacy "SuyuRun Prototype" (RunnerPrototype,
            // the old vector/mesh illustrated runner) scaffold. Disabled, not deleted - it stays
            // available if needed, but it would otherwise render on top of the new pixel-art scene
            // through the same camera.
            var legacy = GameObject.Find("SuyuRun Prototype");
            if (legacy && legacy.activeSelf) legacy.SetActive(false);

            var cam = Camera.main;
            if (!cam)
            {
                var camGo = new GameObject("Main Camera");
                cam = camGo.AddComponent<Camera>();
                camGo.AddComponent<AudioListener>();
                camGo.tag = "MainCamera";
            }
            cam.orthographic = true;
            // World X=0..10 is the 320px-wide viewport (top-left origin convention, see
            // PixelCostaSkyLayer), so the camera must be centered on X=5, not X=0.
            cam.transform.position = new Vector3(320f / Ppu / 2f, 0, -10);
            cam.backgroundColor = new Color(.13f, .11f, .18f);
            var ppc = cam.GetComponent<PixelPerfectCamera>();
            if (!ppc) ppc = cam.gameObject.AddComponent<PixelPerfectCamera>();
            ppc.assetsPPU = (int)Ppu;
            ppc.refResolutionX = 320;
            ppc.refResolutionY = 180;
            ppc.cropFrameX = false;
            ppc.cropFrameY = false;
            ppc.pixelSnapping = true;

            var root = new GameObject("Pixel Costa Level").transform;
            root.position = new Vector3(0, ViewTop, 0);

            var sky = Sprite("costa_capa1_cielo.png", "");
            var skyGo = new GameObject("Cielo (estatico, sin tile)");
            skyGo.transform.SetParent(root, false);
            skyGo.transform.localPosition = new Vector3(-(640f - 320f) / 2f / Ppu, 0, 0);
            var skyRenderer = skyGo.AddComponent<SpriteRenderer>();
            skyRenderer.sprite = sky; skyRenderer.sortingOrder = -40;

            var skyLayerGo = new GameObject("Capa 2 - nubes y aves");
            skyLayerGo.transform.SetParent(root, false);
            var skyLayer = skyLayerGo.AddComponent<PixelCostaSkyLayer>();
            var so = new SerializedObject(skyLayer);
            so.FindProperty("clouds").objectReferenceValue = Texture("costa_capa2_nubes.png");
            so.FindProperty("birds").objectReferenceValue = Texture("costa_capa2_aves.png");
            so.ApplyModifiedPropertiesWithoutUndo();

            var paracas = AddLayer(root, "Capa 3 - Paracas y Candelabro", Sprite("costa_capa3_paracas.png", ""), .15f, -36, 0);
            var huacas = AddLayer(root, "Capa 5 - huacas y muros", Sprite("costa_capa5_huacas.png", ""), .50f, -29, 0);
            var algarrobos = AddLayer(root, "Capa 6 - algarrobos, rocas, lobos", Sprite("costa_capa6_algarrobos.png", ""), .75f, -22, 0);
            var suelo = AddLayer(root, "Capa 7 - suelo jugable", Sprite("costa_capa7_suelo.png", ""), 1.0f, -20, 148);

            var marGo = new GameObject("Capa 4 - mar (4 frames animados)");
            marGo.transform.SetParent(root, false);
            var marAnim = marGo.AddComponent<FrameAnimator>();
            var marRenderer = marGo.AddComponent<SpriteRenderer>();
            marRenderer.sortingOrder = -33;
            var marFrames = new[] { Sprite("costa_capa4_mar.png", "mar_0"), Sprite("costa_capa4_mar.png", "mar_1"), Sprite("costa_capa4_mar.png", "mar_2"), Sprite("costa_capa4_mar.png", "mar_3") };
            marAnim.SetFrames(marFrames, 4f, true);
            // A 4-frame flipbook needs only one visible copy (it's already a full 640-wide panel,
            // same as capa1/3/5/6) so it's centered like the sky rather than tiled/scrolled via
            // PixelParallaxLayer, which assumes a single static sprite per layer.
            marGo.transform.localPosition = new Vector3(-(640f - 320f) / 2f / Ppu, 0, 0);

            // Local Y (relative to root, which already carries the world Y offset) of the ground
            // tile's top edge - do not also subtract root.position.y here, these objects are
            // parented under root so that offset already applies once via the transform hierarchy.
            float groundTopY = -148f / Ppu;

            var block = PlaceSprite(root, "Bloque plataforma", "costa_plataformas.png", "bloque", -18, new Vector2(2.5f, groundTopY + 32f / Ppu));
            var ramp = PlaceSprite(root, "Rampa", "costa_plataformas.png", "rampa", -18, new Vector2(4.6f, groundTopY + 32f / Ppu));
            var pillar = PlaceSprite(root, "Pilar", "costa_plataformas.png", "pilar", -18, new Vector2(6.5f, groundTopY + 48f / Ppu));
            AddCollider(block, 48, 32); AddCollider(ramp, 32, 32); AddCollider(pillar, 16, 48);

            var heroGo = new GameObject("Heroe (idle)");
            heroGo.transform.SetParent(root, false);
            heroGo.transform.localPosition = new Vector2(1.2f, groundTopY + 48f / Ppu);
            var heroRenderer = heroGo.AddComponent<SpriteRenderer>();
            heroRenderer.sortingOrder = -10;
            var heroAnim = heroGo.AddComponent<FrameAnimator>();
            heroAnim.SetFrames(new[] { Sprite("costa_heroe.png", "idle_0"), Sprite("costa_heroe.png", "idle_1"), Sprite("costa_heroe.png", "idle_2"), Sprite("costa_heroe.png", "idle_3") }, 6f, true);

            var farolGo = PlaceSprite(root, "Farol", "costa_efectos.png", "farol_1", -9, new Vector2(6.9f, groundTopY + 16f / Ppu));

            var hex = new GameObject("Hexagono obstaculo");
            hex.transform.SetParent(root, false);
            hex.transform.localPosition = new Vector2(9f, groundTopY + 16f / Ppu);
            var hexRenderer = hex.AddComponent<SpriteRenderer>(); hexRenderer.sortingOrder = -10;
            var hexAnim = hex.AddComponent<FrameAnimator>();
            hexAnim.SetFrames(new[] { Sprite("costa_obstaculos.png", "hexagono_0"), Sprite("costa_obstaculos.png", "hexagono_1"), Sprite("costa_obstaculos.png", "hexagono_2"), Sprite("costa_obstaculos.png", "hexagono_3") }, 6f, true);

            var moneda = PlaceSprite(root, "Moneda coleccionable", "costa_coleccionables.png", "moneda_0", -10, new Vector2(3.5f, groundTopY + 24f / Ppu));
            var huaco = PlaceSprite(root, "Huaco coleccionable", "costa_coleccionables.png", "huaco_0", -10, new Vector2(5.8f, groundTopY + 24f / Ppu));

            var driverGo = new GameObject("Costa Pixel Scene Driver");
            driverGo.transform.SetParent(root, false);
            var driver = driverGo.AddComponent<CostaPixelSceneDriver>();
            var driverSo = new SerializedObject(driver);
            driverSo.FindProperty("layers").arraySize = 4;
            driverSo.FindProperty("layers").GetArrayElementAtIndex(0).objectReferenceValue = paracas;
            driverSo.FindProperty("layers").GetArrayElementAtIndex(1).objectReferenceValue = huacas;
            driverSo.FindProperty("layers").GetArrayElementAtIndex(2).objectReferenceValue = algarrobos;
            driverSo.FindProperty("layers").GetArrayElementAtIndex(3).objectReferenceValue = suelo;
            driverSo.FindProperty("skyLayer").objectReferenceValue = skyLayer;
            driverSo.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("SuyuRun: nivel Costa pixel art ensamblado en Prototype_Costa.unity. Abrir esa escena y pulsar Play para verlo.");
        }

        static Sprite Sprite(string file, string spriteName)
        {
            var path = Art + file;
            var all = AssetDatabase.LoadAllAssetsAtPath(path).OfType<UnityEngine.Sprite>().ToArray();
            if (all.Length == 0) { Debug.LogError("No sprite loaded from " + path); return null; }
            if (string.IsNullOrEmpty(spriteName)) return all[0];
            var found = all.FirstOrDefault(s => s.name == spriteName);
            if (!found) Debug.LogError($"Sprite '{spriteName}' not found in {path}");
            return found;
        }
        static Texture2D Texture(string file) => AssetDatabase.LoadAssetAtPath<Texture2D>(Art + file);

        static PixelParallaxLayer AddLayer(Transform root, string name, UnityEngine.Sprite sprite, float factor, int order, float yPixels)
        {
            var go = new GameObject(name);
            go.transform.SetParent(root, false);
            var layer = go.AddComponent<PixelParallaxLayer>();
            var so = new SerializedObject(layer);
            so.FindProperty("sprite").objectReferenceValue = sprite;
            so.FindProperty("parallaxFactor").floatValue = factor;
            so.FindProperty("sortingOrder").intValue = order;
            so.FindProperty("yPixels").floatValue = yPixels;
            so.ApplyModifiedPropertiesWithoutUndo();
            return layer;
        }

        static GameObject PlaceSprite(Transform root, string name, string file, string spriteName, int order, Vector2 position)
        {
            var go = new GameObject(name);
            go.transform.SetParent(root, false);
            go.transform.localPosition = position;
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = Sprite(file, spriteName);
            renderer.sortingOrder = order;
            return go;
        }

        static void AddCollider(GameObject go, float widthPixels, float heightPixels)
        {
            var box = go.AddComponent<BoxCollider2D>();
            box.size = new Vector2(widthPixels / Ppu, heightPixels / Ppu);
            box.offset = new Vector2(widthPixels / Ppu / 2f, -heightPixels / Ppu / 2f); // top-left pivot -> box centered below-right of origin
        }
    }
}
