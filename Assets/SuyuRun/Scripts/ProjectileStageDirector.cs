using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace SuyuRun
{
    // Minijuego "Proyectiles": heroe fijo a la izquierda, objetivos a la derecha.
    // Fisica del lab 05 (Prediccion de trayectoria): p(t) = p0 + v*t + g*t^2/2.
    // La misma ecuacion dibuja la guia (dots, como Tuto_DrawTrajectory) y mueve la bola,
    // asi que la prediccion es exacta. Arrastre hacia atras: v = -(arrastre) * Power.
    public sealed class ProjectileStageDirector : MonoBehaviour
    {
        [SerializeField] AudioClip music;
        [SerializeField] Font pixelFont;

        const string MenuScene = "Level_Costa";
        const float GroundY = 34f / 32f;
        static readonly Vector2 HeroFeet = new Vector2(1.35f, GroundY);
        static readonly Vector2 Anchor = new Vector2(1.95f, 1.65f);
        const float Gravity = 12f;
        const float MaxPull = 2.2f;
        const float Power = 5.4f;
        const float MinPull = .3f;
        const float BallRadius = .19f;
        const int DotCount = 6;
        const float DotStep = .075f;
        const float ReloadSeconds = .35f;

        [Serializable] sealed class SheetSpec { public int frames, frameWidth, frameHeight; public float fps; public bool loop; }
        sealed class Sheet
        {
            public Sprite[] frames; public float fps; public bool loop;
            public Sprite At(float age)
            {
                int i = Mathf.FloorToInt(Mathf.Max(0, age) * fps);
                return frames[loop ? i % frames.Length : Mathf.Min(i, frames.Length - 1)];
            }
        }
        sealed class Target
        {
            public bool bird, moving, dead, faceRight;
            public Vector2 pos, basePos, amp, hitOffset;
            public float speed, phase, radius, age, deadAge;
            public Transform root; public SpriteRenderer renderer; public Sheet idle, death;
        }
        sealed class Fx { public Transform root; public SpriteRenderer renderer; public Sheet sheet; public float age; }

        readonly Dictionary<string, Sheet> sheets = new Dictionary<string, Sheet>();
        readonly List<Target> targets = new List<Target>();
        readonly List<Fx> effects = new List<Fx>();
        readonly List<GameObject> owned = new List<GameObject>();
        Material spriteMaterial;
        Sprite circle, ballSprite;
        Transform stage;
        Camera cam;
        Vector3 camBase;
        Font font;
        AudioSource musicSource, sfx;

        SpriteRenderer heroRenderer, ballRenderer, cloudRenderer;
        SpriteRenderer[] dots;
        LineRenderer band;
        Sheet heroIdle, heroShoot;

        bool aiming, ballFlying, started, complete;
        Vector2 aimStart, pull, ballPos, launchPos, launchVelocity;
        float flightTime, reloadAt, elapsed, shake, heroShootUntil, heroAge, bannerUntil, nextPhaseAt = -1, panelAt = -1, ballSpin;
        int phase, shots, hits;
        string banner = "";

        void Awake()
        {
            SetupCamera();
            font = pixelFont ? pixelFont : Resources.Load<Font>("PressStart2P-Regular");
            spriteMaterial = new Material(Shader.Find("Sprites/Default"));
            circle = MakeCircle(16);
            stage = new GameObject("Projectile Stage Art").transform;
            stage.SetParent(transform, false);

            BuildBackground();
            BuildHero();
            BuildSling();
            BuildBall();
            BuildDots();
            sfx = gameObject.AddComponent<AudioSource>();
            sfx.playOnAwake = false;
            if (music)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.clip = music; musicSource.loop = true; musicSource.playOnAwake = false;
                musicSource.volume = .3f * Mathf.Clamp01(PlayerPrefs.GetFloat("SuyuRun.Settings.MusicVol", 1f));
                musicSource.Play();
            }
            SpawnPhase(0);
            Say("FASE 1 · DERRIBA LOS 3 OBJETIVOS", 3f);
        }

        void OnDestroy()
        {
            if (spriteMaterial) Destroy(spriteMaterial);
            if (circle) Destroy(circle.texture);
            if (circle) Destroy(circle);
        }

        // ---------- setup ----------
        void SetupCamera()
        {
            cam = Camera.main;
            if (!cam)
            {
                var go = new GameObject("Main Camera") { tag = "MainCamera" };
                cam = go.AddComponent<Camera>();
            }
            if (!cam.GetComponent<AudioListener>()) cam.gameObject.AddComponent<AudioListener>();
            AudioListener.pause = false;
            cam.orthographic = true;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color32(33, 28, 45, 255);
            FitCamera();
        }

        void FitCamera()
        {
            float aspect = Mathf.Max(.5f, (float)Screen.width / Mathf.Max(1, Screen.height));
            cam.orthographicSize = Mathf.Max(2.8125f, 5f / aspect);
            camBase = new Vector3(5f, cam.orthographicSize, -10f);
        }

        Sheet LoadSheet(string path, float ppu, Vector2 pivot)
        {
            string key = path + "|" + ppu + "|" + pivot;
            if (sheets.TryGetValue(key, out var cached)) return cached;
            var texture = Resources.Load<Texture2D>(path);
            var meta = Resources.Load<TextAsset>(path);
            if (!texture || !meta) { Debug.LogError("Proyectiles: falta arte " + path); return null; }
            texture.filterMode = FilterMode.Point; texture.wrapMode = TextureWrapMode.Clamp;
            var spec = JsonUtility.FromJson<SheetSpec>(meta.text);
            int count = Mathf.Max(1, spec.frames);
            int w = spec.frameWidth > 0 ? spec.frameWidth : texture.width / count;
            int h = spec.frameHeight > 0 ? spec.frameHeight : texture.height;
            var frames = new Sprite[count];
            for (int i = 0; i < count; i++)
                frames[i] = Sprite.Create(texture, new Rect(i * w, 0, w, h), pivot, ppu, 0, SpriteMeshType.FullRect);
            var sheet = new Sheet { frames = frames, fps = spec.fps > 0 ? spec.fps : 8, loop = spec.loop };
            sheets[key] = sheet;
            return sheet;
        }

        SpriteRenderer AddRenderer(string name, Transform parent, Sprite sprite, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var r = go.AddComponent<SpriteRenderer>();
            r.sharedMaterial = spriteMaterial; r.sprite = sprite; r.sortingOrder = order;
            return r;
        }

        LineRenderer AddLine(string name, Color color, float width, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(stage, false);
            var l = go.AddComponent<LineRenderer>();
            l.sharedMaterial = spriteMaterial; l.startColor = l.endColor = color;
            l.startWidth = l.endWidth = width; l.sortingOrder = order; l.useWorldSpace = true;
            l.numCapVertices = 2; l.numCornerVertices = 2;
            return l;
        }

        static Sprite MakeCircle(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            float r = size / 2f - .5f;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                    tex.SetPixel(x, y, Vector2.Distance(new Vector2(x, y), new Vector2(r, r)) <= r ? Color.white : Color.clear);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(.5f, .5f), size);
        }

        void BuildBackground()
        {
            Layer("costa_capa1_cielo", 5f, -100, out _);
            Layer("costa_capa2_nubes", 5f, -95, out cloudRenderer);
            Layer("costa_capa3_paracas", 5f, -90, out _);
            Layer("costa_capa5_huacas", 3.2f, -80, out _);
            var tile = Resources.Load<Texture2D>("costa_capa7_suelo");
            if (!tile) return;
            tile.filterMode = FilterMode.Point; tile.wrapMode = TextureWrapMode.Clamp;
            var sprite = Sprite.Create(tile, new Rect(0, 0, tile.width, tile.height), new Vector2(.5f, 1f), 32);
            for (int row = 0; row < 2; row++)
                for (int i = -1; i <= 11; i++)
                {
                    var r = AddRenderer("Ground", stage, sprite, -50);
                    r.transform.position = new Vector3(i + .5f, GroundY - row, 0);
                }
        }

        void Layer(string resource, float centerX, int order, out SpriteRenderer renderer)
        {
            renderer = null;
            var tex = Resources.Load<Texture2D>(resource);
            if (!tex) return;
            tex.filterMode = FilterMode.Point; tex.wrapMode = TextureWrapMode.Clamp;
            var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5f, 0f), 32);
            renderer = AddRenderer(resource, stage, sprite, order);
            renderer.transform.position = new Vector3(centerX, 0, 0);
        }

        void BuildHero()
        {
            var pivot = new Vector2(.5f, 4f / 128f);
            heroIdle = LoadSheet("RefactorBank/TaytaCosta/idle", 94f, pivot);
            heroShoot = LoadSheet("RefactorBank/TaytaCosta/burst", 94f, pivot);
            if (heroIdle == null) return;
            heroRenderer = AddRenderer("Hero", stage, heroIdle.frames[0], 20);
            heroRenderer.transform.position = new Vector3(HeroFeet.x, HeroFeet.y, 0);
        }

        void BuildSling()
        {
            var wood = new Color32(110, 66, 38, 255);
            var stem = AddLine("Sling fork", wood, .09f, 24);
            stem.positionCount = 3;
            stem.SetPositions(new[] { new Vector3(1.95f, 1.15f, 0), new Vector3(1.95f, 1.43f, 0), new Vector3(1.8f, 1.67f, 0) });
            var prong = AddLine("Sling fork right", wood, .09f, 24);
            prong.positionCount = 2;
            prong.SetPositions(new[] { new Vector3(1.95f, 1.43f, 0), new Vector3(2.1f, 1.67f, 0) });
            band = AddLine("Sling band", new Color32(226, 184, 120, 255), .06f, 25);
            band.positionCount = 3;
        }

        void BuildBall()
        {
            var tex = Resources.Load<Texture2D>("SierraV2/sierra_piedra");
            if (tex)
            {
                tex.filterMode = FilterMode.Point; tex.wrapMode = TextureWrapMode.Clamp;
                ballSprite = Sprite.Create(tex, new Rect(0, 0, 8, 8), new Vector2(.5f, .5f), 20);
            }
            else ballSprite = circle;
            ballRenderer = AddRenderer("Ball", stage, ballSprite, 30);
            ballPos = Anchor;
        }

        void BuildDots()
        {
            dots = new SpriteRenderer[DotCount];
            for (int i = 0; i < DotCount; i++)
            {
                float k = 1f - i / (float)DotCount;
                dots[i] = AddRenderer("Trajectory dot", stage, circle, 35);
                dots[i].transform.localScale = Vector3.one * Mathf.Lerp(.07f, .2f, k);
                dots[i].color = new Color(1f, .86f, .5f, Mathf.Lerp(.35f, 1f, k));
                dots[i].enabled = false;
            }
        }

        // ---------- fases ----------
        void SpawnPhase(int index)
        {
            phase = index;
            foreach (var t in targets) if (t.root) Destroy(t.root.gameObject);
            targets.Clear();
            if (index == 0)
            {
                AddTarget(false, new Vector2(6.2f, GroundY), false, Vector2.zero, 0, 0);
                AddTarget(true, new Vector2(7.9f, 3.4f), false, Vector2.zero, 0, 0);
                AddTarget(false, new Vector2(9.0f, GroundY), false, Vector2.zero, 0, 0);
            }
            else
            {
                AddTarget(true, new Vector2(6.3f, 4.0f), true, new Vector2(.9f, .25f), 2.6f, 0f);
                AddTarget(false, new Vector2(8.0f, GroundY), true, new Vector2(.8f, 0f), 2.3f, 1.5f);
                AddTarget(true, new Vector2(9.0f, 2.6f), true, new Vector2(.25f, .7f), 2.9f, 3f);
            }
        }

        void AddTarget(bool bird, Vector2 pos, bool moving, Vector2 amp, float speed, float phaseOffset)
        {
            var t = new Target { bird = bird, pos = pos, basePos = pos, moving = moving, amp = amp, speed = speed, phase = phaseOffset, faceRight = false };
            if (bird)
            {
                var pivot = new Vector2(.5f, .5f);
                t.idle = LoadSheet("RefactorBank/Actors/coastbird/flight", 144f, pivot);
                t.death = LoadSheet("RefactorBank/Actors/coastbird/death", 144f, pivot);
                t.radius = .25f; t.hitOffset = Vector2.zero;
            }
            else
            {
                var pivot = new Vector2(.5f, 4f / 128f);
                t.idle = LoadSheet(moving ? "RefactorBank/Actors/hexagon/walk" : "RefactorBank/Actors/hexagon/idle", 144f, pivot);
                t.death = LoadSheet("RefactorBank/Actors/hexagon/death", 144f, pivot);
                t.radius = .24f; t.hitOffset = new Vector2(0, .22f);
            }
            if (t.idle == null || t.death == null) return;
            t.renderer = AddRenderer(bird ? "Target bird" : "Target hexagon", stage, t.idle.frames[0], 10);
            t.root = t.renderer.transform;
            t.root.position = pos;
            targets.Add(t);
        }

        Vector2 Center(Target t) { return t.pos + t.hitOffset; }

        int Alive() { int n = 0; foreach (var t in targets) if (!t.dead) n++; return n; }

        // ---------- bucle ----------
        void Update()
        {
            float dt = Time.deltaTime;
            var keys = Keyboard.current;
            if (keys != null && keys.escapeKey.wasPressedThisFrame) { SceneManager.LoadScene(MenuScene); return; }
            if (started && !complete) elapsed += dt;

            HandleAim(Mouse.current);
            UpdateBall(dt);
            UpdateTargets(dt);
            UpdateEffects(dt);
            UpdateFlow();
            UpdateHero(dt);
            UpdateSlingVisuals();
            if (cloudRenderer) cloudRenderer.transform.position = new Vector3(5f + Mathf.PingPong(Time.time * .12f, 4f) - 2f, 0, 0);
        }

        void LateUpdate()
        {
            FitCamera();
            shake = Mathf.MoveTowards(shake, 0, Time.deltaTime * 3f);
            cam.transform.position = camBase + (Vector3)(UnityEngine.Random.insideUnitCircle * shake * .08f);
        }

        Vector2 PointerWorld(Mouse mouse)
        {
            Vector2 p = mouse.position.ReadValue();
            Vector3 w = cam.ScreenToWorldPoint(new Vector3(p.x, p.y, -cam.transform.position.z));
            return new Vector2(w.x, w.y);
        }

        bool CanAim { get { return !ballFlying && Time.time >= reloadAt && !complete; } }

        void HandleAim(Mouse mouse)
        {
            if (mouse == null) return;
            if (!aiming)
            {
                if (CanAim && mouse.leftButton.wasPressedThisFrame)
                {
                    aiming = true; started = true;
                    aimStart = PointerWorld(mouse); pull = Vector2.zero; ballPos = Anchor;
                }
                return;
            }
            pull = Vector2.ClampMagnitude(PointerWorld(mouse) - aimStart, MaxPull);
            ballPos = PulledBall(pull);
            if (mouse.rightButton.wasPressedThisFrame) { CancelAim(); return; }
            if (mouse.leftButton.wasReleasedThisFrame)
            {
                if (pull.magnitude >= MinPull) Launch(); else CancelAim();
            }
        }

        // Solo visual: la bola se estira un poco, pero la fisica siempre parte de Anchor
        // (si no, arrastrar hacia abajo para lanzar hacia arriba la dejaba bajo el suelo).
        static Vector2 PulledBall(Vector2 pullVector)
        {
            Vector2 p = Anchor + pullVector * .3f;
            p.y = Mathf.Max(p.y, GroundY + BallRadius);
            return p;
        }

        void CancelAim()
        {
            aiming = false; pull = Vector2.zero; ballPos = Anchor;
        }

        void Launch()
        {
            aiming = false; ballFlying = true; shots++;
            launchPos = Anchor; launchVelocity = -pull * Power; flightTime = 0;
            heroShootUntil = Time.time + .35f;
            Beep(520, .08f);
        }

        // p = p0 + v*t + g*t^2/2 (ecuacion del lab); 4 subpasos para no atravesar objetivos.
        void UpdateBall(float dt)
        {
            if (ballFlying)
            {
                const int steps = 4;
                float h = dt / steps;
                for (int i = 0; i < steps && ballFlying; i++)
                {
                    flightTime += h;
                    ballPos = launchPos + launchVelocity * flightTime + .5f * new Vector2(0, -Gravity) * flightTime * flightTime;
                    CheckBall();
                }
                ballSpin += dt * 540f;
            }
            ballRenderer.enabled = ballFlying || aiming || Time.time >= reloadAt;
            if (!ballFlying && !aiming) ballPos = Anchor;
            ballRenderer.transform.position = new Vector3(ballPos.x, ballPos.y, 0);
            ballRenderer.transform.rotation = Quaternion.Euler(0, 0, ballFlying ? ballSpin : 0);
        }

        void CheckBall()
        {
            foreach (var t in targets)
            {
                if (t.dead) continue;
                if (Vector2.Distance(ballPos, Center(t)) < BallRadius + t.radius) { Hit(t); EndFlight(); return; }
            }
            if (ballPos.y - BallRadius <= GroundY)
            {
                SpawnFx("RefactorBank/Effects/impacto_piedra", new Vector2(ballPos.x, GroundY + .2f), 96f, 38);
                Beep(160, .1f, .25f);
                EndFlight();
                return;
            }
            if (ballPos.x > 11.5f || ballPos.x < -1.5f || flightTime > 8f) EndFlight();
        }

        void EndFlight()
        {
            ballFlying = false; reloadAt = Time.time + ReloadSeconds; ballPos = Anchor;
        }

        void Hit(Target t)
        {
            t.dead = true; t.deadAge = 0; hits++;
            SpawnFx("RefactorBank/Effects/impacto_arma", Center(t), 64f, 40);
            shake = 1f;
            Beep(380, .12f);
            if (Alive() > 0) return;
            if (phase == 0) { nextPhaseAt = Time.time + 1.3f; Say("FASE 1 SUPERADA", 1.3f); }
            else { complete = true; panelAt = Time.time + 1.4f; Say("¡TODOS DERRIBADOS!", 1.4f); Beep(880, .25f); }
        }

        void UpdateTargets(float dt)
        {
            foreach (var t in targets)
            {
                t.age += dt;
                if (!t.dead)
                {
                    if (t.moving)
                    {
                        float a = t.age * t.speed + t.phase;
                        t.pos = t.basePos + new Vector2(Mathf.Sin(a) * t.amp.x, Mathf.Sin(a * 1.3f) * t.amp.y);
                        t.faceRight = Mathf.Cos(a) * t.amp.x >= 0;
                    }
                    float bob = t.bird && !t.moving ? Mathf.Sin(t.age * 3f) * .06f : 0f;
                    t.root.position = new Vector3(t.pos.x, t.pos.y + bob, 0);
                    t.renderer.sprite = t.idle.At(t.age);
                    t.renderer.flipX = t.bird && !t.faceRight;
                }
                else
                {
                    t.deadAge += dt;
                    t.renderer.sprite = t.death.At(t.deadAge);
                    float alpha = Mathf.Clamp01(1f - (t.deadAge - .45f) / .4f);
                    t.renderer.color = new Color(1, 1, 1, alpha);
                    if (alpha <= 0) t.renderer.enabled = false;
                }
            }
        }

        void SpawnFx(string path, Vector2 pos, float ppu, int order)
        {
            var sheet = LoadSheet(path, ppu, new Vector2(.5f, .5f));
            if (sheet == null) return;
            var r = AddRenderer("Fx", stage, sheet.frames[0], order);
            r.transform.position = new Vector3(pos.x, pos.y, 0);
            effects.Add(new Fx { root = r.transform, renderer = r, sheet = sheet });
        }

        void UpdateEffects(float dt)
        {
            for (int i = effects.Count - 1; i >= 0; i--)
            {
                var fx = effects[i];
                fx.age += dt;
                if (fx.age * fx.sheet.fps >= fx.sheet.frames.Length) { Destroy(fx.root.gameObject); effects.RemoveAt(i); continue; }
                fx.renderer.sprite = fx.sheet.At(fx.age);
            }
        }

        void UpdateFlow()
        {
            if (nextPhaseAt > 0 && Time.time >= nextPhaseAt)
            {
                nextPhaseAt = -1;
                SpawnPhase(1);
                Say("FASE 2 · ¡AHORA SE MUEVEN!", 3f);
            }
        }

        void UpdateHero(float dt)
        {
            if (heroRenderer == null) return;
            heroAge += dt;
            bool shooting = Time.time < heroShootUntil && heroShoot != null;
            heroRenderer.sprite = shooting ? heroShoot.At(.35f - (heroShootUntil - Time.time)) : heroIdle.At(heroAge);
        }

        void UpdateSlingVisuals()
        {
            Vector3 left = new Vector3(1.8f, 1.67f, 0), right = new Vector3(2.1f, 1.67f, 0);
            Vector3 mid = new Vector3(Anchor.x, Anchor.y, 0);
            if (aiming) mid = new Vector3(ballPos.x, ballPos.y, 0);
            band.SetPositions(new[] { left, mid, right });

            for (int i = 0; i < DotCount; i++)
            {
                if (!aiming) { dots[i].enabled = false; continue; }
                float t = (i + 1) * DotStep;
                Vector2 p = Anchor + (-pull * Power) * t + .5f * new Vector2(0, -Gravity) * t * t;
                dots[i].enabled = p.y > GroundY;
                dots[i].transform.position = new Vector3(p.x, p.y, 0);
            }
        }

        void Say(string message, float seconds) { banner = message; bannerUntil = Time.time + seconds; }

        void Beep(float freq, float seconds, float gain = .3f)
        {
            if (!sfx) return;
            int rate = 44100, n = Mathf.CeilToInt(rate * seconds);
            var data = new float[n];
            for (int i = 0; i < n; i++) data[i] = Mathf.Sin(2 * Mathf.PI * freq * i / rate) * (1f - i / (float)n) * gain;
            var clip = AudioClip.Create("beep", n, 1, rate, false);
            clip.SetData(data, 0);
            sfx.PlayOneShot(clip, Mathf.Clamp01(PlayerPrefs.GetFloat("SuyuRun.Settings.EffectsVol", 1f)));
            Destroy(clip, seconds + 1f);
        }

        // ---------- HUD (1280x720 virtual) ----------
        static string FormatTime(float t)
        {
            return Mathf.FloorToInt(t / 60f).ToString("00") + ":" + (t % 60f).ToString("00.00");
        }

        void Fill(Rect r, Color c)
        {
            var old = GUI.color; GUI.color = c; GUI.DrawTexture(r, Texture2D.whiteTexture); GUI.color = old;
        }

        // El skin por defecto cambia el color del label al pasar el mouse; se fija en todos los estados.
        static void SetTextColor(GUIStyle style, Color c)
        {
            style.normal.textColor = style.hover.textColor = style.active.textColor = style.focused.textColor = c;
            style.onNormal.textColor = style.onHover.textColor = style.onActive.textColor = style.onFocused.textColor = c;
        }

        void Text(string s, Rect r, int size, Color color, TextAnchor anchor)
        {
            var style = new GUIStyle(GUI.skin.label) { font = font, fontSize = size, alignment = anchor, wordWrap = false };
            SetTextColor(style, new Color(0, 0, 0, .75f));
            GUI.Label(new Rect(r.x + 2, r.y + 2, r.width, r.height), s, style);
            SetTextColor(style, color);
            GUI.Label(r, s, style);
        }

        void OnGUI()
        {
            var old = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(new Vector3(Screen.width / 1280f, Screen.height / 720f, 1));
            Color cream = new Color32(255, 235, 179, 255), gold = new Color32(250, 197, 104, 255), dark = new Color32(35, 28, 46, 255);

            Fill(new Rect(0, 0, 1280, 56), new Color(dark.r, dark.g, dark.b, .85f));
            Fill(new Rect(0, 54, 1280, 3), gold);
            Text("FASE " + (phase + 1) + "/2   TIROS " + shots, new Rect(24, 0, 430, 56), 16, cream, TextAnchor.MiddleLeft);
            Text("OBJETIVOS " + (3 - Alive()) + "/3", new Rect(440, 0, 400, 56), 18, gold, TextAnchor.MiddleCenter);
            Text("TIEMPO " + FormatTime(elapsed), new Rect(830, 0, 426, 56), 16, cream, TextAnchor.MiddleRight);

            Fill(new Rect(0, 684, 1280, 36), new Color(dark.r, dark.g, dark.b, .8f));
            Text(started ? "ARRASTRA HACIA ATRAS PARA APUNTAR · SUELTA PARA LANZAR · CLIC DERECHO CANCELA · ESC MENU"
                         : "ARRASTRA CON EL MOUSE HACIA ATRAS Y SUELTA PARA LANZAR · EL TIEMPO EMPIEZA AL APUNTAR",
                 new Rect(0, 684, 1280, 36), 11, cream, TextAnchor.MiddleCenter);

            if (Time.time < bannerUntil)
            {
                Fill(new Rect(250, 92, 780, 60), new Color(dark.r, dark.g, dark.b, .9f));
                Fill(new Rect(250, 92, 780, 3), gold);
                Text(banner, new Rect(250, 92, 780, 60), 20, cream, TextAnchor.MiddleCenter);
            }

            if (complete && panelAt > 0 && Time.time >= panelAt) DrawResults(cream, gold, dark);
            GUI.matrix = old;
        }

        void DrawResults(Color cream, Color gold, Color dark)
        {
            Fill(new Rect(0, 0, 1280, 720), new Color(0, 0, 0, .55f));
            Fill(new Rect(340, 150, 600, 420), new Color(dark.r, dark.g, dark.b, .97f));
            Fill(new Rect(340, 150, 600, 5), gold);
            Fill(new Rect(340, 565, 600, 5), gold);
            Text("¡MINIJUEGO COMPLETADO!", new Rect(340, 175, 600, 60), 22, gold, TextAnchor.MiddleCenter);
            Text("TIEMPO", new Rect(340, 260, 600, 36), 16, cream, TextAnchor.MiddleCenter);
            Text(FormatTime(elapsed), new Rect(340, 296, 600, 56), 36, cream, TextAnchor.MiddleCenter);
            Text("TIROS " + shots + "   PRECISION " + Mathf.RoundToInt(100f * 6f / Mathf.Max(6, shots)) + "%",
                 new Rect(340, 370, 600, 40), 14, cream, TextAnchor.MiddleCenter);

            if (ResultButton(new Rect(370, 470, 240, 56), "REINTENTAR", cream, gold, dark))
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            if (ResultButton(new Rect(670, 470, 240, 56), "VOLVER AL MENU", cream, gold, dark))
                SceneManager.LoadScene(MenuScene);
        }

        bool ResultButton(Rect r, string label, Color cream, Color gold, Color dark)
        {
            bool hover = r.Contains(Event.current.mousePosition);
            Fill(r, hover ? gold : new Color32(110, 66, 38, 255));
            Fill(new Rect(r.x + 4, r.y + 4, r.width - 8, r.height - 8), dark);
            Text(label, r, 13, hover ? gold : cream, TextAnchor.MiddleCenter);
            return GUI.Button(r, GUIContent.none, GUIStyle.none);
        }
    }
}
