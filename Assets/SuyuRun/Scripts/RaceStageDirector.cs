using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace SuyuRun
{
    // Minijuego "Rutas" (Lab 06 - Waypoints), vista cenital.
    // Construccion: el jugador coloca las palmeras (waypoints) que forman el circuito.
    // Fase 1: vuelta en solitario contra el reloj. Fase 2: carrera contra 1 rival sobre el mismo circuito.
    // Cada bote sigue los waypoints con la logica de FollowWP del lab: si distancia al waypoint < umbral -> siguiente;
    // al final vuelve al 0; gira con Slerp; avanza con speed. Aqui el jugador solo controla la velocidad y,
    // como en el lab, speed y rotSpeed estan ligados: a mas velocidad, menos giro (mas rapido = giro mas amplio).
    // Cada palmera tiene una zona de control: si el jugador se desborda sin entrar en ella es game over y puede
    // reiniciar desde el ultimo checkpoint tocado (con penalizacion de tiempo).
    public sealed class RaceStageDirector : MonoBehaviour
    {
        [SerializeField] AudioClip music;
        [SerializeField] Font pixelFont;

        const string MenuScene = "Level_Costa";
        const int MinPalms = 3, MaxPalms = 6, SoloLaps = 1, RaceLaps = 3;
        const float MinSpacing = 1.9f;
        const float ZoneRadius = .75f;         // zona de control = umbral de llegada (el "3" del FollowWP original)
        const float PlayerMin = 1.2f, PlayerMax = 4.6f, PlayerAccel = 3.6f, PlayerDrag = 4.2f;
        const float RotSlow = 3.4f, RotFast = 1.2f;   // rotSpeed del jugador a velocidad minima / maxima
        const float RiskSpeed = 3.0f;          // por encima de esto el giro ya no alcanza a cerrar sobre la zona
        const float RetryPenalty = 5f;         // segundos que suma cada reinicio desde un checkpoint
        const float RivalSpeed = 3.5f, RivalRot = 3.2f;
        static readonly Rect Water = new Rect(1f, 1f, 14f, 7f);
        static readonly Rect PalmArea = new Rect(1.5f, 1.4f, 13f, 5.8f);

        enum Phase { Build, Countdown, Race, Interlude, GameOver, Finished }

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
        sealed class Palm { public Vector2 pos; public SpriteRenderer renderer; }
        sealed class Boat
        {
            public string name; public bool player;
            public Transform root; public SpriteRenderer renderer;
            public Vector2 pos; public Quaternion rot;
            public float speed, baseSpeed, rotSpeed, progress, finishTime, minDist, legTime;
            public int cur, lap, reached, cpHits, retries;
            public bool finished, hasSafe;
        }

        readonly Dictionary<string, Sheet> sheets = new Dictionary<string, Sheet>();
        readonly List<Palm> palms = new List<Palm>();
        readonly List<Boat> boats = new List<Boat>();
        readonly List<LineRenderer> rings = new List<LineRenderer>();
        readonly List<SpriteRenderer> sparkles = new List<SpriteRenderer>();
        readonly List<float> sparklePhase = new List<float>();
        Material spriteMaterial;
        Sprite whiteSprite, treeSprite, sparkleSprite;
        Texture2D whiteTexture;
        Transform stage;
        Camera cam;
        Vector3 camBase;
        Font font;
        AudioSource sfx;
        LineRenderer route;
        SpriteRenderer ghost;
        Sheet boatSheet;

        Phase phase = Phase.Build;
        int stageIndex;                         // 0 = vuelta en solitario, 1 = carrera contra el rival
        bool paused;
        float raceTime, countdownT, bannerUntil, panelAt = -1, boatAge, interludeAt, soloTime;
        int lastCountBeep = 4, soloRetries;
        bool resuming;
        string banner = "";
        Vector2 startPoint, startDir;

        int LapsThisStage { get { return stageIndex == 0 ? SoloLaps : RaceLaps; } }

        void Awake()
        {
            SetupCamera();
            font = pixelFont ? pixelFont : Resources.Load<Font>("PressStart2P-Regular");
            spriteMaterial = new Material(Shader.Find("Sprites/Default"));
            whiteTexture = new Texture2D(4, 4, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color[16];
            for (int i = 0; i < px.Length; i++) px[i] = Color.white;
            whiteTexture.SetPixels(px); whiteTexture.Apply();
            whiteSprite = Sprite.Create(whiteTexture, new Rect(0, 0, 4, 4), new Vector2(.5f, .5f), 4);
            stage = new GameObject("Race Stage Art").transform;
            stage.SetParent(transform, false);
            sfx = gameObject.AddComponent<AudioSource>();
            sfx.playOnAwake = false;
            if (music)
            {
                var m = gameObject.AddComponent<AudioSource>();
                m.clip = music; m.loop = true; m.playOnAwake = false;
                m.volume = .3f * Mathf.Clamp01(PlayerPrefs.GetFloat("SuyuRun.Settings.MusicVol", 1f));
                m.Play();
            }
            BuildMap();
            BuildRouteAndGhost();
            boatSheet = LoadSheet("RefactorBank/Actors/boat/walk", 100f, new Vector2(.5f, .16f));
            Say("COLOCA LAS PALMERAS DEL CIRCUITO", 4f);
        }

        void OnDestroy()
        {
            if (spriteMaterial) Destroy(spriteMaterial);
            if (whiteTexture) Destroy(whiteTexture);
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
            cam.orthographicSize = Mathf.Max(4.5f, 8f / aspect);
            camBase = new Vector3(8f, 4.5f, -10f);
            cam.transform.position = camBase;
        }

        Sheet LoadSheet(string path, float ppu, Vector2 pivot)
        {
            string key = path + "|" + ppu + "|" + pivot;
            if (sheets.TryGetValue(key, out var cached)) return cached;
            var texture = Resources.Load<Texture2D>(path);
            var meta = Resources.Load<TextAsset>(path);
            if (!texture || !meta) { Debug.LogError("Rutas: falta arte " + path); return null; }
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

        SpriteRenderer AddRenderer(string name, Sprite sprite, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(stage, false);
            var r = go.AddComponent<SpriteRenderer>();
            r.sharedMaterial = spriteMaterial; r.sprite = sprite; r.sortingOrder = order;
            return r;
        }

        SpriteRenderer AddRect(string name, Rect area, Color color, int order)
        {
            var r = AddRenderer(name, whiteSprite, order);
            r.color = color;
            r.transform.position = new Vector3(area.center.x, area.center.y, 0);
            r.transform.localScale = new Vector3(area.width, area.height, 1);
            return r;
        }

        void BuildMap()
        {
            var sandTex = Resources.Load<Texture2D>("Ground/Sand");
            if (sandTex)
            {
                sandTex.filterMode = FilterMode.Point; sandTex.wrapMode = TextureWrapMode.Repeat;
                var sand = Sprite.Create(sandTex, new Rect(0, 0, sandTex.width, sandTex.height), new Vector2(.5f, .5f), 128, 0, SpriteMeshType.FullRect);
                var r = AddRenderer("Sand", sand, -100);
                r.drawMode = SpriteDrawMode.Tiled; r.size = new Vector2(30, 18);
                r.transform.position = new Vector3(8, 4.5f, 0);
            }
            else AddRect("Sand", new Rect(-7, -4.5f, 30, 18), new Color32(255, 210, 120, 255), -100);
            AddRect("Shore foam", new Rect(Water.x - .18f, Water.y - .18f, Water.width + .36f, Water.height + .36f), new Color32(255, 240, 200, 255), -95);
            AddRect("Water", Water, new Color32(36, 172, 182, 255), -90);

            var sparkleTex = Resources.Load<Texture2D>("Sea/SeaSparkle");
            if (sparkleTex)
            {
                sparkleTex.filterMode = FilterMode.Point; sparkleTex.wrapMode = TextureWrapMode.Clamp;
                sparkleSprite = Sprite.Create(sparkleTex, new Rect(0, 0, sparkleTex.width, sparkleTex.height), new Vector2(.5f, .5f), 128);
                for (int i = 0; i < 26; i++)
                {
                    var s = AddRenderer("Sparkle", sparkleSprite, -85);
                    s.transform.position = new Vector3(UnityEngine.Random.Range(Water.xMin + .4f, Water.xMax - .4f), UnityEngine.Random.Range(Water.yMin + .4f, Water.yMax - .4f), 0);
                    s.transform.localScale = Vector3.one * UnityEngine.Random.Range(.5f, 1.1f);
                    sparkles.Add(s); sparklePhase.Add(UnityEngine.Random.Range(0f, 6.28f));
                }
            }

            var treeTex = Resources.Load<Texture2D>("Tutorial/dry_tree_rope");
            if (treeTex)
            {
                treeTex.filterMode = FilterMode.Point; treeTex.wrapMode = TextureWrapMode.Clamp;
                treeSprite = Sprite.Create(treeTex, new Rect(0, 0, treeTex.width, treeTex.height), new Vector2(.42f, .03f), 128);
            }
            else Debug.LogError("Rutas: falta el arte Tutorial/dry_tree_rope");
        }

        void BuildRouteAndGhost()
        {
            var go = new GameObject("Route line");
            go.transform.SetParent(stage, false);
            route = go.AddComponent<LineRenderer>();
            route.sharedMaterial = spriteMaterial;
            route.startColor = route.endColor = new Color32(255, 235, 179, 220);
            route.startWidth = route.endWidth = .07f;
            route.sortingOrder = 5; route.useWorldSpace = true; route.loop = true;
            route.numCornerVertices = 2; route.positionCount = 0;
            ghost = AddRenderer("Palm ghost", treeSprite, 60);
            ghost.enabled = false;
        }

        // Zonas de control celestes alrededor de cada palmera (se muestran al empezar la carrera).
        void BuildRings()
        {
            foreach (var palm in palms)
            {
                var go = new GameObject("Checkpoint zone");
                go.transform.SetParent(stage, false);
                var l = go.AddComponent<LineRenderer>();
                l.sharedMaterial = spriteMaterial; l.useWorldSpace = true; l.loop = true;
                l.sortingOrder = 4; l.startWidth = l.endWidth = .06f;
                l.startColor = l.endColor = new Color(.7f, .95f, 1f, .5f);
                const int segments = 40;
                l.positionCount = segments;
                for (int k = 0; k < segments; k++)
                {
                    float a = k * Mathf.PI * 2f / segments;
                    l.SetPosition(k, new Vector3(palm.pos.x + Mathf.Cos(a) * ZoneRadius, palm.pos.y + Mathf.Sin(a) * ZoneRadius, 0));
                }
                rings.Add(l);
            }
        }

        // ---------- bucle ----------
        // Si el estado quedo a medias (p. ej. recompilacion de scripts con el juego en marcha), vuelve a la construccion.
        void HealState()
        {
            if (phase != Phase.Build && boats.Count < 1) { phase = Phase.Build; paused = false; }
        }

        void Update()
        {
            HealState();
            var keys = Keyboard.current; var mouse = Mouse.current;
            if (keys != null && keys.escapeKey.wasPressedThisFrame)
            {
                if (phase == Phase.Finished) { SceneManager.LoadScene(MenuScene); return; }
                paused = !paused;
            }
            float dt = paused ? 0f : Time.deltaTime;

            for (int i = 0; i < sparkles.Count; i++)
                sparkles[i].color = new Color(1, 1, 1, .25f + .45f * (.5f + .5f * Mathf.Sin(Time.time * 1.6f + sparklePhase[i])));

            if (!paused)
            {
                switch (phase)
                {
                    case Phase.Build: UpdateBuild(mouse, keys); break;
                    case Phase.Countdown: UpdateCountdown(dt); break;
                    case Phase.Race: UpdateRace(dt, keys, mouse); break;
                    case Phase.Interlude: UpdateInterlude(); break;
                }
            }
            if (phase != Phase.Build) { AnimateBoats(dt); PulseRings(); }
        }

        void LateUpdate() { FitCamera(); }

        Vector2 PointerWorld(Mouse mouse)
        {
            Vector2 p = mouse.position.ReadValue();
            Vector3 w = cam.ScreenToWorldPoint(new Vector3(p.x, p.y, -cam.transform.position.z));
            return new Vector2(w.x, w.y);
        }

        bool PointerOverUi(Mouse mouse)
        {
            Vector2 p = mouse.position.ReadValue();
            float vy = (Screen.height - p.y) / Screen.height * 720f;
            return vy < 56f || vy > 640f;
        }

        // ---------- construir ----------
        bool CanPlace(Vector2 p, out string why)
        {
            why = "";
            if (palms.Count >= MaxPalms) { why = "MAXIMO " + MaxPalms + " PALMERAS"; return false; }
            if (!PalmArea.Contains(p)) { why = "COLOCALA DENTRO DEL MAR"; return false; }
            foreach (var palm in palms)
                if (Vector2.Distance(palm.pos, p) < MinSpacing) { why = "MUY CERCA DE OTRA PALMERA"; return false; }
            return true;
        }

        void UpdateBuild(Mouse mouse, Keyboard keys)
        {
            if (mouse != null)
            {
                Vector2 world = PointerWorld(mouse);
                bool overUi = PointerOverUi(mouse);
                bool valid = CanPlace(world, out string why);
                ghost.enabled = !overUi && Water.Contains(world) && palms.Count < MaxPalms;
                ghost.transform.position = new Vector3(world.x, world.y, 0);
                ghost.color = valid ? new Color(1, 1, 1, .55f) : new Color(1, .4f, .4f, .55f);
                if (mouse.leftButton.wasPressedThisFrame && !overUi)
                {
                    if (valid) AddPalm(world); else Say(why, 1.4f);
                }
                if (mouse.rightButton.wasPressedThisFrame) RemoveLastPalm();
            }
            if (keys != null)
            {
                if (keys.zKey.wasPressedThisFrame || keys.backspaceKey.wasPressedThisFrame) RemoveLastPalm();
                if (keys.enterKey.wasPressedThisFrame) TryStart();
            }
        }

        void AddPalm(Vector2 p)
        {
            var r = AddRenderer("Palm " + (palms.Count + 1), treeSprite, 10 + Mathf.RoundToInt((9f - p.y) * 2f));
            r.transform.position = new Vector3(p.x, p.y, 0);
            palms.Add(new Palm { pos = p, renderer = r });
            if (palms.Count == 1) r.color = new Color(1f, .86f, .45f);
            RefreshRoute();
            Beep(660, .06f);
        }

        void RemoveLastPalm()
        {
            if (palms.Count == 0) return;
            Destroy(palms[palms.Count - 1].renderer.gameObject);
            palms.RemoveAt(palms.Count - 1);
            RefreshRoute();
            Beep(300, .06f);
        }

        void ClearPalms()
        {
            while (palms.Count > 0) RemoveLastPalm();
        }

        void RefreshRoute()
        {
            route.positionCount = palms.Count;
            for (int i = 0; i < palms.Count; i++) route.SetPosition(i, new Vector3(palms[i].pos.x, palms[i].pos.y, 0));
            route.loop = palms.Count >= 3;
        }

        void TryStart()
        {
            if (palms.Count < MinPalms) { Say("NECESITAS AL MENOS " + MinPalms + " PALMERAS", 1.6f); return; }
            ghost.enabled = false;
            Vector2 w0 = palms[0].pos, w1 = palms[1].pos;
            startDir = (w1 - w0).normalized;
            startPoint = w0 - startDir * 1.6f;
            BuildRings();
            boats.Add(CreateBoat("TU", true, Color.white));
            stageIndex = 0;
            BeginCountdown();
            Say("FASE 1 · UNA VUELTA TU SOLO", 3f);
        }

        // ---------- carrera ----------
        Boat CreateBoat(string name, bool player, Color tint)
        {
            var r = AddRenderer("Boat " + name, boatSheet.frames[0], 30);
            r.color = tint;
            var trail = r.gameObject.AddComponent<TrailRenderer>();
            trail.sharedMaterial = spriteMaterial; trail.time = .7f; trail.minVertexDistance = .1f;
            trail.startWidth = .22f; trail.endWidth = 0f; trail.sortingOrder = 28;
            trail.startColor = new Color(1, 1, 1, .7f); trail.endColor = new Color(1, 1, 1, 0);
            return new Boat { name = name, player = player, root = r.transform, renderer = r, rotSpeed = player ? RotSlow : RivalRot, baseSpeed = RivalSpeed * UnityEngine.Random.Range(.97f, 1.03f) };
        }

        void ResetBoat(Boat b, float lane)
        {
            Vector2 perp = new Vector2(-startDir.y, startDir.x);
            Vector2 start = startPoint + perp * lane;
            start.x = Mathf.Clamp(start.x, Water.xMin + .3f, Water.xMax - .3f);
            start.y = Mathf.Clamp(start.y, Water.yMin + .3f, Water.yMax - .3f);
            Vector2 toNext = Wp(1) - start;
            b.pos = start;
            b.rot = Quaternion.Euler(0, 0, Mathf.Atan2(toNext.y, toNext.x) * Mathf.Rad2Deg);
            b.speed = PlayerMin; b.cur = 1; b.lap = 0; b.reached = 0; b.cpHits = 0; b.retries = 0; b.hasSafe = false;
            b.progress = 0; b.finished = false; b.finishTime = 0; b.legTime = 0; b.minDist = float.MaxValue;
            ApplyVisual(b);
            var trail = b.root.GetComponent<TrailRenderer>();
            if (trail) trail.Clear();
        }

        void BeginCountdown()
        {
            phase = Phase.Countdown; countdownT = 0; lastCountBeep = 4; boatAge = 0;
            if (boats.Count == 1) ResetBoat(boats[0], 0f);
            else { ResetBoat(boats[0], .55f); ResetBoat(boats[1], -.55f); }
        }

        void UpdateCountdown(float dt)
        {
            countdownT += dt;
            int shown = 3 - Mathf.FloorToInt(countdownT);
            if (shown >= 1 && shown < lastCountBeep) { lastCountBeep = shown; Beep(440, .12f); }
            if (countdownT >= 3.6f)
            {
                phase = Phase.Race;
                if (!resuming) raceTime = 0;
                resuming = false;
                Say("¡YA!", 1f); Beep(880, .25f);
            }
        }

        Vector2 Wp(int i) { return palms[i].pos; }

        // Procesa un checkpoint (tocado o fallado) y avanza al siguiente waypoint, como el currentWP++ del FollowWP.
        void PassCheckpoint(Boat b)
        {
            int idx = b.cur;
            b.cur = (b.cur + 1) % palms.Count;
            b.reached++; b.legTime = 0; b.minDist = float.MaxValue;
            b.cpHits++; b.hasSafe = true;
            if (b.player) Beep(900 + b.cpHits * 110, .07f);
            if (idx == 0)
            {
                b.lap++;
                if (b.lap >= LapsThisStage) { Finish(b); return; }
                b.cpHits = 0;
                if (b.player) Say("VUELTA " + (b.lap + 1) + "/" + LapsThisStage, 1.4f);
            }
        }

        // Logica de FollowWP (lab 06) adaptada a 2D.
        void FollowWaypoints(Boat b, float dt)
        {
            Vector2 target = Wp(b.cur);
            float dist = Vector2.Distance(b.pos, target);
            b.legTime += dt;
            if (dist < ZoneRadius) { PassCheckpoint(b); if (b.finished) return; }
            else if (b.player)
            {
                // Game over: llego cerca de la zona pero se alejo sin entrar, o lleva demasiado tiempo en este tramo.
                b.minDist = Mathf.Min(b.minDist, dist);
                if ((b.minDist < 2f && dist - b.minDist > .6f) || b.legTime > 7f) { FailRun(b); return; }
            }
            else if (b.legTime > 7f) { PassCheckpoint(b); if (b.finished) return; }

            Vector2 dir = Wp(b.cur) - b.pos;
            Quaternion look = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
            float error = Quaternion.Angle(b.rot, look);
            b.rot = Quaternion.Slerp(b.rot, look, b.rotSpeed * dt);
            float speed = b.speed;
            if (!b.player)
            {
                // Rival: frena al girar y recibe un pequeno "rubber band" (+-7%).
                float catchUp = 1f + Mathf.Clamp(boats[0].progress - b.progress, -1.5f, 1.5f) * .05f;
                speed = b.baseSpeed * catchUp * Mathf.Lerp(1f, .35f, Mathf.Clamp01(error / 90f));
            }
            b.pos += (Vector2)(b.rot * Vector3.right) * speed * dt;
            b.pos.x = Mathf.Clamp(b.pos.x, Water.xMin + .2f, Water.xMax - .2f);
            b.pos.y = Mathf.Clamp(b.pos.y, Water.yMin + .2f, Water.yMax - .2f);
        }

        void Finish(Boat b)
        {
            b.finished = true; b.finishTime = raceTime;
            if (!b.player) return;
            Beep(988, .3f);
            if (stageIndex == 0)
            {
                soloTime = b.finishTime + b.retries * RetryPenalty; soloRetries = b.retries;
                phase = Phase.Interlude; interludeAt = Time.time + 4f;
                Say("FASE 1 COMPLETA · " + FormatTime(soloTime), 3.8f);
            }
            else if (!boats[1].finished) Say("¡TERMINASTE! ESPERANDO AL RIVAL...", 4f);
        }

        // El jugador se desbordo de una zona de control: game over con opcion de reiniciar desde el ultimo checkpoint.
        void FailRun(Boat b)
        {
            phase = Phase.GameOver;
            bannerUntil = 0;
            Beep(150, .4f, .35f);
        }

        void RetryFromCheckpoint()
        {
            var me = boats[0];
            int n = palms.Count;
            Vector2 target = Wp(me.cur);
            if (me.hasSafe)
            {
                Vector2 prev = Wp((me.cur - 1 + n) % n);
                me.pos = prev + (target - prev).normalized * .3f;
            }
            else
            {
                Vector2 perp = new Vector2(-startDir.y, startDir.x);
                me.pos = startPoint + perp * (boats.Count == 1 ? 0f : .55f);
            }
            Vector2 toNext = target - me.pos;
            me.rot = Quaternion.Euler(0, 0, Mathf.Atan2(toNext.y, toNext.x) * Mathf.Rad2Deg);
            me.speed = PlayerMin; me.legTime = 0; me.minDist = float.MaxValue;
            me.retries++;
            ApplyVisual(me);
            var trail = me.root.GetComponent<TrailRenderer>();
            if (trail) trail.Clear();
            resuming = true; phase = Phase.Countdown; countdownT = 2.0f; lastCountBeep = 4;
            Say("REINICIO +" + RetryPenalty.ToString("0") + " S", 2f);
        }

        void UpdateInterlude()
        {
            if (Time.time < interludeAt) return;
            stageIndex = 1;
            boats.Add(CreateBoat("ROJO", false, new Color(1f, .55f, .55f)));
            BeginCountdown();
            Say("FASE 2 · CARRERA CONTRA ROJO · " + RaceLaps + " VUELTAS", 3f);
        }

        void UpdateRace(float dt, Keyboard keys, Mouse mouse)
        {
            raceTime += dt;
            var me = boats[0];
            if (!me.finished)
            {
                bool gas = (keys != null && keys.spaceKey.isPressed) || (mouse != null && mouse.leftButton.isPressed);
                me.speed = Mathf.Clamp(me.speed + (gas ? PlayerAccel : -PlayerDrag) * dt, PlayerMin, PlayerMax);
                // Lab 06: a mayor velocidad menos rotSpeed efectivo -> giro mas amplio; a menor velocidad, giro preciso.
                me.rotSpeed = Mathf.Lerp(RotSlow, RotFast, Mathf.InverseLerp(PlayerMin, PlayerMax, me.speed));
                if (me.legTime > 4f && me.speed > 2.4f && Time.time > bannerUntil)
                    Say("¡SUELTA ESPACIO PARA FRENAR Y GIRAR!", 2f);
            }
            foreach (var b in boats)
            {
                if (b.finished) continue;
                FollowWaypoints(b, dt);
                if (phase != Phase.Race) break;
                if (b.finished) continue;
                float leg = Mathf.Max(.1f, Vector2.Distance(Wp((b.cur - 1 + palms.Count) % palms.Count), Wp(b.cur)));
                b.progress = b.reached + 1f - Mathf.Clamp01(Vector2.Distance(b.pos, Wp(b.cur)) / leg);
            }
            if (phase == Phase.Race && stageIndex == 1 && boats[0].finished && boats[1].finished)
            {
                phase = Phase.Finished; panelAt = Time.time + 1.0f;
                bool won = PlayerEffectiveTime() < boats[1].finishTime;
                Say(won ? "¡GANASTE LA CARRERA!" : "EL RIVAL TE GANO", 1.4f);
                Beep(won ? 988 : 330, .35f);
            }
        }

        float PlayerEffectiveTime() { return boats[0].finishTime + boats[0].retries * RetryPenalty; }

        void PulseRings()
        {
            int active = boats.Count > 0 ? boats[0].cur : -1;
            for (int i = 0; i < rings.Count; i++)
            {
                bool isActive = i == active && phase == Phase.Race;
                float a = isActive ? .65f + .3f * Mathf.Sin(Time.time * 6f) : .4f;
                rings[i].startColor = rings[i].endColor = new Color(.7f, .95f, 1f, a);
                rings[i].startWidth = rings[i].endWidth = isActive ? .1f : .06f;
            }
        }

        void AnimateBoats(float dt)
        {
            if (phase == Phase.Race) boatAge += dt;
            foreach (var b in boats)
            {
                b.renderer.sprite = boatSheet.At(phase == Phase.Race && !b.finished ? boatAge * (b.player ? b.speed / 3f : 1f) : 0f);
                ApplyVisual(b);
            }
        }

        // El sprite es de perfil: si el rumbo apunta a la izquierda se voltea en X para que el remero no quede de cabeza.
        void ApplyVisual(Boat b)
        {
            float angle = b.rot.eulerAngles.z;
            bool left = Mathf.Cos(angle * Mathf.Deg2Rad) < 0f;
            b.root.position = new Vector3(b.pos.x, b.pos.y, 0);
            b.root.rotation = Quaternion.Euler(0, 0, left ? angle - 180f : angle);
            b.renderer.flipX = left;
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

        Vector2 ToVirtual(Vector2 world)
        {
            Vector3 s = cam.WorldToScreenPoint(new Vector3(world.x, world.y, 0));
            return new Vector2(s.x / Screen.width * 1280f, (Screen.height - s.y) / Screen.height * 720f);
        }

        bool Button(Rect r, string label, bool enabled, Color cream, Color gold, Color dark)
        {
            bool hover = enabled && r.Contains(Event.current.mousePosition);
            Fill(r, !enabled ? new Color32(80, 70, 90, 255) : hover ? gold : new Color32(110, 66, 38, 255));
            Fill(new Rect(r.x + 4, r.y + 4, r.width - 8, r.height - 8), dark);
            Text(label, r, 12, !enabled ? new Color32(140, 130, 150, 255) : hover ? gold : cream, TextAnchor.MiddleCenter);
            return GUI.Button(r, GUIContent.none, GUIStyle.none);
        }

        void OnGUI()
        {
            HealState();
            var old = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(new Vector3(Screen.width / 1280f, Screen.height / 720f, 1));
            Color cream = new Color32(255, 235, 179, 255), gold = new Color32(250, 197, 104, 255), dark = new Color32(35, 28, 46, 255);
            Color cyan = new Color32(170, 240, 255, 255);

            Fill(new Rect(0, 0, 1280, 56), new Color(dark.r, dark.g, dark.b, .85f));
            Fill(new Rect(0, 54, 1280, 3), gold);

            if (phase == Phase.Build)
            {
                Text("PALMERAS " + palms.Count + "/" + MaxPalms, new Rect(24, 0, 400, 56), 16, gold, TextAnchor.MiddleLeft);
                Text("CONSTRUYE EL CIRCUITO", new Rect(380, 0, 520, 56), 16, cream, TextAnchor.MiddleCenter);
            }
            else
            {
                var me = boats[0];
                string stageLabel = stageIndex == 0 ? "FASE 1 · SOLO" : "FASE 2 · VS ROJO";
                Text(stageLabel + " · VUELTA " + Mathf.Min(me.lap + 1, LapsThisStage) + "/" + LapsThisStage, new Rect(24, 0, 470, 56), 12, cream, TextAnchor.MiddleLeft);
                Text("CHECKPOINT " + me.cpHits + "/" + palms.Count, new Rect(470, 0, 340, 56), 17, cyan, TextAnchor.MiddleCenter);
                float shownTime = me.finished ? PlayerEffectiveTime() : raceTime + me.retries * RetryPenalty;
                string penalty = me.retries > 0 ? " (+" + (me.retries * RetryPenalty).ToString("0") + "S)" : "";
                Text("TIEMPO " + FormatTime(shownTime) + penalty, new Rect(800, 0, 456, 56), 13, me.retries > 0 ? new Color32(255, 150, 130, 255) : cream, TextAnchor.MiddleRight);
            }

            DrawLabels(cream, gold, dark);

            Fill(new Rect(0, 640, 1280, 80), new Color(dark.r, dark.g, dark.b, .85f));
            Fill(new Rect(0, 640, 1280, 3), gold);
            if (phase == Phase.Build) DrawBuildBar(cream, gold, dark);
            else DrawRaceBar(cream, gold, dark);

            if (phase == Phase.Countdown)
            {
                int shown = 3 - Mathf.FloorToInt(countdownT);
                if (shown >= 1) Text(shown.ToString(), new Rect(0, 200, 1280, 200), 96, gold, TextAnchor.MiddleCenter);
            }
            if (Time.time < bannerUntil)
            {
                Fill(new Rect(250, 92, 780, 60), new Color(dark.r, dark.g, dark.b, .9f));
                Fill(new Rect(250, 92, 780, 3), gold);
                Text(banner, new Rect(250, 92, 780, 60), 16, cream, TextAnchor.MiddleCenter);
            }
            if (phase == Phase.Finished && panelAt > 0 && Time.time >= panelAt) DrawResults(cream, gold, dark);
            if (phase == Phase.GameOver) DrawGameOver(cream, gold, dark);
            if (paused) DrawPause(cream, gold, dark);
            GUI.matrix = old;
        }

        void DrawLabels(Color cream, Color gold, Color dark)
        {
            for (int i = 0; i < palms.Count; i++)
            {
                Vector2 v = ToVirtual(palms[i].pos + new Vector2(0, 1.25f));
                Text(i == 0 ? "1 META" : (i + 1).ToString(), new Rect(v.x - 60, v.y - 14, 120, 28), 14, i == 0 ? gold : cream, TextAnchor.MiddleCenter);
            }
            if (phase == Phase.Build) return;
            foreach (var b in boats)
            {
                Vector2 v = ToVirtual(b.pos + new Vector2(0, .75f));
                Text(b.name, new Rect(v.x - 50, v.y - 12, 100, 24), 11, b.player ? gold : cream, TextAnchor.MiddleCenter);
            }
        }

        void DrawBuildBar(Color cream, Color gold, Color dark)
        {
            if (Button(new Rect(60, 652, 260, 44), "BORRAR TODO", palms.Count > 0, cream, gold, dark)) ClearPalms();
            Text("CLIC: PALMERA · CLIC DERECHO / Z: DESHACER", new Rect(340, 650, 600, 24), 10, cream, TextAnchor.MiddleCenter);
            Text("ENTER: EMPEZAR · ESC: PAUSA", new Rect(340, 676, 600, 24), 10, cream, TextAnchor.MiddleCenter);
            if (Button(new Rect(960, 652, 260, 44), "EMPEZAR CARRERA", true, cream, gold, dark)) TryStart();
        }

        void DrawRaceBar(Color cream, Color gold, Color dark)
        {
            var me = boats[0];
            Text("ESPACIO / CLIC ACELERA · MENOS VELOCIDAD = GIRO MAS PRECISO", new Rect(240, 648, 800, 22), 9, cream, TextAnchor.MiddleCenter);
            Rect bar = new Rect(440, 676, 400, 24);
            Fill(bar, new Color32(20, 16, 28, 255));
            float t = Mathf.InverseLerp(PlayerMin, PlayerMax, me.speed);
            float risk = Mathf.InverseLerp(PlayerMin, PlayerMax, RiskSpeed);
            Fill(new Rect(bar.x + bar.width * risk, bar.y, bar.width * (1 - risk), bar.height), new Color(.8f, .2f, .2f, .35f));
            Fill(new Rect(bar.x + 2, bar.y + 2, (bar.width - 4) * t, bar.height - 4), me.speed > RiskSpeed ? new Color32(220, 70, 60, 255) : gold);
            Fill(new Rect(bar.x + bar.width * risk - 1, bar.y - 3, 3, bar.height + 6), cream);
            Text("RIESGO", new Rect(bar.x + bar.width * risk, bar.y - 20, 200, 18), 8, new Color32(240, 120, 110, 255), TextAnchor.MiddleLeft);
        }

        void DrawResults(Color cream, Color gold, Color dark)
        {
            var me = boats[0]; var rival = boats[1];
            bool won = PlayerEffectiveTime() < rival.finishTime;
            Fill(new Rect(0, 0, 1280, 720), new Color(0, 0, 0, .55f));
            Fill(new Rect(340, 130, 600, 460), new Color(dark.r, dark.g, dark.b, .97f));
            Fill(new Rect(340, 130, 600, 5), gold);
            Fill(new Rect(340, 585, 600, 5), gold);
            Text(won ? "¡GANASTE LA CARRERA!" : "EL RIVAL TE GANO", new Rect(340, 155, 600, 60), 22, won ? gold : new Color32(240, 120, 110, 255), TextAnchor.MiddleCenter);
            Text("FASE 1 · SOLO", new Rect(340, 235, 600, 28), 12, cream, TextAnchor.MiddleCenter);
            Text(FormatTime(soloTime) + (soloRetries > 0 ? "  (" + soloRetries + " REINICIOS)" : "  SIN REINICIOS"), new Rect(340, 263, 600, 36), 18, cream, TextAnchor.MiddleCenter);
            Text("FASE 2 · TU", new Rect(340, 315, 600, 28), 12, cream, TextAnchor.MiddleCenter);
            Text(FormatTime(PlayerEffectiveTime()) + (me.retries > 0 ? "  (" + me.retries + " REINICIOS)" : "  SIN REINICIOS"), new Rect(340, 343, 600, 36), 18, won ? gold : cream, TextAnchor.MiddleCenter);
            Text("FASE 2 · ROJO", new Rect(340, 395, 600, 28), 12, cream, TextAnchor.MiddleCenter);
            Text(FormatTime(rival.finishTime), new Rect(340, 423, 600, 36), 18, won ? cream : gold, TextAnchor.MiddleCenter);
            Text(palms.Count + " PALMERAS · REINICIO = +" + RetryPenalty.ToString("0") + " S", new Rect(340, 475, 600, 24), 9, cream, TextAnchor.MiddleCenter);
            if (Button(new Rect(370, 515, 240, 52), "OTRO CIRCUITO", true, cream, gold, dark))
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            if (Button(new Rect(670, 515, 240, 52), "VOLVER AL MENU", true, cream, gold, dark))
                SceneManager.LoadScene(MenuScene);
        }

        void DrawGameOver(Color cream, Color gold, Color dark)
        {
            var me = boats[0];
            Color red = new Color32(240, 120, 110, 255);
            Fill(new Rect(0, 0, 1280, 720), new Color(0, 0, 0, .6f));
            Fill(new Rect(340, 170, 600, 380), new Color(dark.r, dark.g, dark.b, .97f));
            Fill(new Rect(340, 170, 600, 5), red);
            Fill(new Rect(340, 545, 600, 5), red);
            Text("GAME OVER", new Rect(340, 195, 600, 50), 26, red, TextAnchor.MiddleCenter);
            Text("¡TE DESBORDASTE DEL PUNTO DE CONTROL!", new Rect(340, 255, 600, 36), 14, cream, TextAnchor.MiddleCenter);
            Text("CHECKPOINT " + me.cpHits + "/" + palms.Count + " · VUELTA " + Mathf.Min(me.lap + 1, LapsThisStage) + "/" + LapsThisStage, new Rect(340, 295, 600, 28), 11, new Color32(170, 240, 255, 255), TextAnchor.MiddleCenter);
            Text("REINICIAR SUMA +" + RetryPenalty.ToString("0") + " S A TU TIEMPO", new Rect(340, 325, 600, 24), 9, cream, TextAnchor.MiddleCenter);
            if (Button(new Rect(380, 375, 520, 56), "REINICIAR DESDE EL PUNTO DE CONTROL", true, cream, gold, dark)) RetryFromCheckpoint();
            if (Button(new Rect(380, 450, 520, 56), "VOLVER AL MENU", true, cream, gold, dark)) SceneManager.LoadScene(MenuScene);
        }

        void DrawPause(Color cream, Color gold, Color dark)
        {
            Fill(new Rect(0, 0, 1280, 720), new Color(0, 0, 0, .6f));
            Fill(new Rect(440, 200, 400, 300), new Color(dark.r, dark.g, dark.b, .97f));
            Fill(new Rect(440, 200, 400, 5), gold);
            Text("PAUSA", new Rect(440, 220, 400, 60), 24, gold, TextAnchor.MiddleCenter);
            if (Button(new Rect(480, 310, 320, 56), "CONTINUAR", true, cream, gold, dark)) paused = false;
            if (Button(new Rect(480, 390, 320, 56), "VOLVER AL MENU", true, cream, gold, dark)) SceneManager.LoadScene(MenuScene);
        }
    }
}
