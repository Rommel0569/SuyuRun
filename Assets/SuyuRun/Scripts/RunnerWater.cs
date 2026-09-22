using System.Collections.Generic;
using UnityEngine;

namespace SuyuRun
{
    public sealed partial class RunnerPrototype
    {
        // Phase 2 of the Costa redesign: the flat-color procedural ocean was replaced by a
        // pixel-art sea (Tools/gen_costa_sea_sprites.py -> Assets/SuyuRun/Art/Resources/Sea/),
        // animated by scrolling each layer's UVs, with a tide driven by
        // Assets/costa_music_map.json (staged read-only via CostaMusicMapSetup) sampled against
        // AudioSettings.dspTime while the level is running.
        sealed class SeaLayer
        {
            public Mesh mesh;
            public float uRepeat, scrollSpeed, scrollOffset;
            public Color baseColor;
            public float alphaFromTide, alphaFromBass, alphaBase;
        }
        sealed class Sparkle { public Transform root; public Mesh mesh; public float phase; }

        readonly List<SeaLayer> seaLayers = new List<SeaLayer>();
        readonly List<Sparkle> sparkles = new List<Sparkle>();
        Transform seaRoot;
        CostaMusicMap musicMap;
        float tide, tideVelocity;
        // Sampled while running, for the smoke-test report - lets the user confirm the tide
        // actually moved and tracked the music map instead of staying flat or failing silently.
        float smokeTideMin = float.MaxValue, smokeTideMax = float.MinValue, smokeEnergyMax;
        float waterTime;

        Material SpriteMaterial(string resourcePath)
        {
            var sprite = Resources.Load<Sprite>(resourcePath);
            var mat = new Material(Shader.Find("Sprites/Default"));
            if (sprite) mat.mainTexture = sprite.texture;
            else Debug.LogWarning("SuyuRun: missing sea sprite " + resourcePath);
            ownedSpriteMaterials.Add(mat);
            return mat;
        }

        // For a PixelCosta sheet imported with multiple named sprites (see PixelCostaImport.cs) -
        // loads the whole texture but samples it through the mesh's own UVs, so the caller still
        // needs to compute UVs for the sub-rect itself (see BuildFacadeTexture for an example).
        Sprite NamedSprite(string resourceName, string spriteName)
        {
            var all = Resources.LoadAll<Sprite>(resourceName);
            foreach (var s in all) if (s.name == spriteName) return s;
            Debug.LogWarning($"SuyuRun: sprite '{spriteName}' not found in Resources/{resourceName}");
            return null;
        }

        // Normalized UV bounds (u0,v0,width,height) of a named sub-sprite within its sheet, for a
        // single non-tiled instance (a whole illustrated piece, not a repeating pattern).
        static Rect SubRectUv(Sprite sprite)
        {
            var tex = sprite.texture;
            return new Rect(sprite.rect.x / tex.width, sprite.rect.y / tex.height, sprite.rect.width / tex.width, sprite.rect.height / tex.height);
        }
        Mesh SubRectQuadMesh(float w, float h, Rect uv)
        {
            var mesh = new Mesh { name = "Sub-rect quad" };
            mesh.vertices = new[] { new Vector3(-w * .5f, 0, 0), new Vector3(w * .5f, 0, 0), new Vector3(w * .5f, h, 0), new Vector3(-w * .5f, h, 0) };
            mesh.uv = new[] { new Vector2(uv.x, uv.y), new Vector2(uv.x + uv.width, uv.y), new Vector2(uv.x + uv.width, uv.y + uv.height), new Vector2(uv.x, uv.y + uv.height) };
            mesh.triangles = new[] { 0, 1, 2, 0, 2, 3 };
            var white = new Color[4]; for (int i = 0; i < 4; i++) white[i] = Color.white; mesh.colors = white;
            mesh.RecalculateBounds(); ownedMeshes.Add(mesh);
            return mesh;
        }

        // Smooth transitions for wrapped parallax pieces (mountains, clouds): fades a piece to
        // transparent over the last fraction of its half-period, right before Mathf.Repeat snaps
        // it back to the other edge. That snap always happens off-screen (periods are far wider
        // than the visible camera window), so this stops it from ever being a visible pop even if
        // the piece briefly grazes the edge of view, and reads as the piece dissolving in the haze.
        static float WrapFade(float wrappedX, float period, float edgeFraction = .12f)
        {
            float edge = period * .5f;
            return Mathf.Clamp01((edge - Mathf.Abs(wrappedX)) / (edge * edgeFraction));
        }
        static void SetMeshAlpha(Mesh mesh, float alpha)
        {
            var colors = new Color[4]; for (int i = 0; i < 4; i++) colors[i] = new Color(1, 1, 1, alpha);
            mesh.colors = colors;
        }

        Mesh TexturedQuadMesh(float x, float y, float w, float h, float uRepeat, float vRepeat = 1)
        {
            var mesh = new Mesh { name = "Sea layer quad" };
            mesh.vertices = new[] { new Vector3(x - w * .5f, y - h * .5f, 0), new Vector3(x + w * .5f, y - h * .5f, 0), new Vector3(x + w * .5f, y + h * .5f, 0), new Vector3(x - w * .5f, y + h * .5f, 0) };
            mesh.uv = new[] { new Vector2(0, 0), new Vector2(uRepeat, 0), new Vector2(uRepeat, vRepeat), new Vector2(0, vRepeat) };
            mesh.triangles = new[] { 0, 1, 2, 0, 2, 3 };
            var white = new Color[4]; for (int i = 0; i < 4; i++) white[i] = Color.white; mesh.colors = white;
            mesh.RecalculateBounds(); ownedMeshes.Add(mesh);
            return mesh;
        }

        SeaLayer BuildSeaLayer(string name, string texturePath, float y, float h, float uRepeat, int order, float scrollSpeed, Color tint, float alphaFromTide, float alphaFromBass)
        {
            var go = new GameObject(name); go.transform.SetParent(seaRoot, false);
            var mesh = TexturedQuadMesh(0, y, 48, h, uRepeat);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = go.AddComponent<MeshRenderer>(); renderer.sharedMaterial = SpriteMaterial(texturePath); renderer.sortingOrder = order;
            var layer = new SeaLayer { mesh = mesh, uRepeat = uRepeat, scrollSpeed = scrollSpeed, baseColor = tint, alphaBase = tint.a, alphaFromTide = alphaFromTide, alphaFromBass = alphaFromBass };
            seaLayers.Add(layer);
            return layer;
        }

        void BuildSea()
        {
            seaRoot = new GameObject("Costa animated pixel-art sea").transform; seaRoot.SetParent(world, false);
            musicMap = CostaMusicMap.Load();
            if (musicMap == null) Debug.LogWarning("SuyuRun: costa_music_map.json not staged yet - run SuyuRun > Audio > Sincronizar mapa musical de Costa. The sea falls back to idle motion.");

            // Fully uniform sea per the user's explicit follow-up ("sigue estando con el degradado,
            // debe ser uniforme"): keeping even ONE copy of the animated costa_capa4_mar band still
            // showed a gradient, because that crop shades itself from a lighter tone at its own top
            // to a darker one at its own bottom - the banding was never just about how many times it
            // repeated. The whole sea is now a single flat color end to end (the same palette shade,
            // #27737A, that band's own top tone used), so there is no texture left to shade. Life
            // comes only from the foam/sparkle layers on top and the tide's vertical bob, not from
            // the water's fill color.
            Rect("Sea body", seaRoot, 0, -3.3f, 48, 7.4f, new Color(39 / 255f, 115 / 255f, 122 / 255f), -34);
            Rect("Soft distant horizon", seaRoot, 0, .34f, 48, .035f, new Color(.64f, .87f, .89f, .65f), -32);
            BuildSeaLayer("Foam line", "Sea/SeaFoam", .3f, .22f, 32, -30, .6f, new Color(1, 1, 1, .8f), alphaFromTide: .15f, alphaFromBass: .5f);
            BuildSparkles();
        }

        void BuildSparkles()
        {
            var material = SpriteMaterial("Sea/SeaSparkle");
            for (int i = 0; i < 16; i++)
            {
                var root = new GameObject("Sea reflection sparkle " + i).transform;
                var mesh = TexturedQuadMesh(0, 0, .16f, .16f, 1);
                var go = root.gameObject;
                go.AddComponent<MeshFilter>().sharedMesh = mesh;
                var renderer = go.AddComponent<MeshRenderer>(); renderer.sharedMaterial = material; renderer.sortingOrder = -22;
                Layer(root, -22 + i * 2.9f, .05f - (i % 5) * .1f, .09f, 50f);
                sparkles.Add(new Sparkle { root = root, mesh = mesh, phase = i * 1.83f });
            }
        }

        void UpdateWater()
        {
            // Calm coastal water has its own slow cadence, not a musical strobe.
            // Pause freezes the water as well as gameplay.
            if (mode != Mode.Paused) waterTime += Mathf.Min(Time.deltaTime, .05f);

            float songSeconds = mode == Mode.Running ? (float)(AudioSettings.dspTime - started) : mode == Mode.Paused ? pausedAt : -1f;
            float targetEnergy, targetBass;
            if (musicMap != null && songSeconds >= 0)
            {
                targetEnergy = musicMap.EnergyAt(songSeconds);
                targetBass = musicMap.BassAt(songSeconds);
            }
            else
            {
                // No live song position (menu/museum/dead/complete, or the map failed to load):
                // a gentle idle swell keeps the sea alive instead of freezing it.
                targetEnergy = .35f + Mathf.Sin(waterTime * .18f) * .15f;
                targetBass = .3f + Mathf.Sin(waterTime * .14f + 1.1f) * .12f;
            }
            // Smoothed so the tide reads as "sube y baja suave" rather than stepping every 0.25s sample.
            tide = Mathf.SmoothDamp(tide, targetEnergy, ref tideVelocity, 1.4f);

            if (mode == Mode.Running)
            {
                smokeTideMin = Mathf.Min(smokeTideMin, tide);
                smokeTideMax = Mathf.Max(smokeTideMax, tide);
                smokeEnergyMax = Mathf.Max(smokeEnergyMax, targetEnergy);
            }

            float dt = Mathf.Min(Time.deltaTime, .05f);
            if (mode == Mode.Paused) dt = 0;
            float bob = (tide - .5f) * .34f; // small vertical swell, stays well clear of Ground
            seaRoot.localPosition = new Vector3(0, bob, 0);

            foreach (var layer in seaLayers)
            {
                layer.scrollOffset += layer.scrollSpeed * (.55f + tide * 1.1f) * dt;
                float o = layer.scrollOffset;
                layer.mesh.uv = new[] { new Vector2(o, 0), new Vector2(layer.uRepeat + o, 0), new Vector2(layer.uRepeat + o, 1), new Vector2(o, 1) };
                float alpha = Mathf.Clamp01(layer.alphaBase + layer.alphaFromTide * (tide - .5f) + layer.alphaFromBass * targetBass);
                var color = layer.baseColor; color.a = alpha;
                var colors = new Color[4]; for (int i = 0; i < 4; i++) colors[i] = color; layer.mesh.colors = colors;
            }

            foreach (var s in sparkles)
            {
                float blink = Mathf.Clamp01(Mathf.Sin(waterTime * 1.4f + s.phase) * 1.6f - .4f);
                float alpha = blink * (.4f + tide * .8f);
                var colors = new Color[4]; for (int i = 0; i < 4; i++) colors[i] = new Color(1, 1, 1, alpha); s.mesh.colors = colors;
            }
        }

        // Reported by RunnerPrototype.SmokeReport so a run's evidence file shows the tide actually
        // moved (min < max) and tracked the music map's energy instead of sitting idle or failing.
        string SeaSmokeReport => legacyPrototype
            ? "SeaSync=not-applicable-legacy"
            : musicMap == null
                ? "SeaSync=missing-costa_music_map"
                : $"SeaTideMin={smokeTideMin:F3} SeaTideMax={smokeTideMax:F3} SeaEnergyMax={smokeEnergyMax:F3}";
    }
}
