using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace SuyuRun
{
    // Three forty-second music chapters. A lesson repeats inside its checkpoint until played correctly.
    public sealed class TutorialStageDirector : MonoBehaviour
    {
        [SerializeField] AudioClip tutorialMusic;
        [SerializeField] Font pixelFont;
        [SerializeField] Camera tutorialCamera;
        [SerializeField] AudioSource musicSource;

        const float Ground = 144f;
        const float HeroX = 72f;
        const float ReloadSeconds = 1.2f;
        readonly string[] prompts = {
            "MANTEN S O ABAJO PARA AGACHARTE",
            "ESPACIO: SALTA EL OBSTACULO",
            "PULSA ESPACIO OTRA VEZ EN EL AIRE",
            "SALTA EL PRIMERO Y AGACHATE BAJO EL SEGUNDO",
            "E O CLIC DERECHO: DISPARA AL BLANCO",
            "SALTA Y DISPARA AL BLANCO AEREO",
            "SALTA Y HAZ CLIC IZQUIERDO EN EL NUDO",
            "SUELTA CON ESPACIO Y CRUZA EL ACANTILADO",
            "ESQUIVA EL OBSTACULO Y DISPARA AL BLANCO",
            "ESPANTAPAJAROS: AGACHATE Y ACIERTA 3 VECES"
        };
        readonly string[] successes = {
            "AGACHADO", "SALTO", "DOBLE SALTO", "ESQUIVE", "DISPARO",
            "BLANCO AEREO", "CUERDA", "ACANTILADO", "COMBINACION", "TUTORIAL COMPLETADO"
        };

        AudioSource music;
        Font font;
        Texture2D pixel, sky, cliffs, sea, dunes, ground, tree, gap, dummy, barrier, flyingTarget;
        Texture2D uiPrompt, uiSuccess, uiTop;
        bool ownsCamera, grounded = true, airJump = true, crouched, rope, releasedRope;
        bool firstPassed, secondPassed, targetHit, bossDodged, complete, successFlash, started;
        int task, phase, bossHits, failures;
        float chapterTime, travel, feet = Ground, vertical, shotCooldown, shotPulse, noticeUntil;
        float ropeAngle, ropeAnchorX, attackTimer, hitPulse;
        string notice = "";

        // The first chapter is deliberately gentle, but still needs to feel like a run.
        const float IntroSpeed = 150f;
        const float RunSpeed = 200f;
        float Speed { get { return phase == 0 ? IntroSpeed : phase == 1 ? RunSpeed : 0f; } }

        void Awake()
        {
            if (!tutorialCamera)
            {
                var cameraObject = new GameObject("Tutorial Camera + Audio Listener");
                tutorialCamera = cameraObject.AddComponent<Camera>();
                ownsCamera = true;
            }
            tutorialCamera.orthographic = true;
            tutorialCamera.orthographicSize = 180f / 64f;
            tutorialCamera.transform.position = new Vector3(5, 180f / 64f, -10);
            tutorialCamera.backgroundColor = new Color32(33, 28, 45, 255);
            if (!tutorialCamera.GetComponent<AudioListener>()) tutorialCamera.gameObject.AddComponent<AudioListener>();
            AudioListener.pause = false;

            font = pixelFont ? pixelFont : Resources.Load<Font>("PressStart2P-Regular");
            pixel = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            pixel.SetPixel(0, 0, Color.white);
            pixel.Apply();
            pixel.filterMode = FilterMode.Point;

            sky = Art("sky"); cliffs = Art("cliffs"); sea = Art("sea");
            dunes = Art("dunes"); ground = Art("ground"); tree = Art("dry_tree_rope");
            gap = Art("cliff_gap"); dummy = Art("training_dummy");
            barrier = Art("low_barrier"); flyingTarget = Art("flying_target");
            uiPrompt = Art("ui_prompt"); uiSuccess = Art("ui_success"); uiTop = Art("ui_top");

            music = musicSource ? musicSource : gameObject.AddComponent<AudioSource>();
            music.playOnAwake = false;
            music.spatialBlend = 0;
            music.loop = false;
            music.mute = false;
            music.clip = tutorialMusic;
            music.volume = .34f * Mathf.Clamp01(PlayerPrefs.GetFloat("SuyuRun.Settings.MusicVol", 1f));
            if (!tutorialMusic)
            {
                Debug.LogError("Tutorial: tutorial.mp3 no esta asignado en Level_Tutorial.");
                enabled = false;
                return;
            }
            if (tutorialMusic.loadState == AudioDataLoadState.Unloaded) tutorialMusic.LoadAudioData();
            RestartChapter(0, false);
        }

        Texture2D Art(string name)
        {
            var result = Resources.Load<Texture2D>("Tutorial/" + name);
            if (!result) Debug.LogError("Tutorial: falta el arte Tutorial/" + name);
            else { result.filterMode = FilterMode.Point; result.wrapMode = TextureWrapMode.Clamp; }
            return result;
        }

        void OnDestroy()
        {
            if (pixel) Destroy(pixel);
            if (ownsCamera && tutorialCamera) Destroy(tutorialCamera.gameObject);
        }

        void RestartChapter(int next, bool retry)
        {
            phase = next; chapterTime = 0; travel = 0; feet = Ground; vertical = 0;
            grounded = true; airJump = true; crouched = false; rope = false; releasedRope = false;
            firstPassed = false; secondPassed = false; targetHit = false; attackTimer = 0;
            // Keep the soundtrack continuous across checkpoints and chapter changes.
            // Only the first chapter starts playback; a failed lesson never seeks backwards.
            if (next != 0 || started) { if (!music.isPlaying) music.Play(); }
            else Show("PULSA ESPACIO PARA COMENZAR", 999f);
            if (retry) Show("VUELVE A INTENTAR DESDE ESTE PUNTO", 2.2f);
            else Show(next == 0 ? "PRACTICA BASICA · VELOCIDAD 1" : next == 1 ?
                "PRACTICA AVANZADA · VELOCIDAD 2" : "PRUEBA FINAL · ESPANTAPAJAROS", 2.5f);
        }

        void Show(string message, float seconds, bool success = false)
        {
            notice = message;
            noticeUntil = Time.unscaledTime + seconds;
            successFlash = success;
        }

        void Advance()
        {
            Show("BIEN: " + successes[task], 2.1f, true);
            int previous = task++;
            if (previous != 6) travel = 0; // Keep the rope and its position for the cliff lesson.
            firstPassed = false; secondPassed = false; targetHit = false;
        }

        void Retry()
        {
            failures++;
            if (task == 7) task = 6; // Retry the rope and cliff together within checkpoint two.
            if (task == 9) bossHits = Mathf.Max(0, bossHits - 1);
            travel = 0; feet = Ground; vertical = 0; grounded = true; airJump = true;
            rope = false; releasedRope = false; firstPassed = false; secondPassed = false;
            targetHit = false; attackTimer = 0;
            chapterTime = Mathf.Max(0, chapterTime - 2f);
            Show("CASI · REPITE ESTA PRACTICA", 2.2f);
        }

        void Finish()
        {
            complete = true;
            music.Stop();
            PlayerPrefs.SetInt("SuyuRun.Progress.TutorialComplete", 1);
            PlayerPrefs.Save();
            Show("BIEN: TUTORIAL COMPLETADO", 10f, true);
        }

        void Update()
        {
            if (complete || !music || !tutorialMusic) return;
            var keys = Keyboard.current;
            var mouse = Mouse.current;
            bool jump = keys != null && keys.spaceKey.wasPressedThisFrame;
            bool duck = keys != null && (keys.sKey.isPressed || keys.downArrowKey.isPressed);
            bool fire = (keys != null && keys.eKey.wasPressedThisFrame) ||
                        (mouse != null && mouse.rightButton.wasPressedThisFrame);
            bool click = mouse != null && mouse.leftButton.wasPressedThisFrame;
            if (!started)
            {
                if (keys == null || !keys.spaceKey.wasPressedThisFrame) return;
                started = true;
                Show("COMIENZA LA PRACTICA", 1.2f, true);
                music.Play();
                return;
            }
            Vector2 pointer = mouse != null ? mouse.position.ReadValue() : Vector2.zero;
            pointer = new Vector2(pointer.x * 320f / Mathf.Max(1, Screen.width),
                (Screen.height - pointer.y) * 180f / Mathf.Max(1, Screen.height));

            float dt = Mathf.Min(Time.deltaTime, .04f);
            chapterTime += dt;
            if (task >= 1 && task < 9 && !rope) travel += Speed * dt;
            shotCooldown = Mathf.Max(0, shotCooldown - dt);
            shotPulse = Mathf.Max(0, shotPulse - dt);
            hitPulse = Mathf.Max(0, hitPulse - dt);
            attackTimer += dt;
            crouched = duck && !rope;

            bool doubleJump = false;
            if (rope)
            {
                ropeAngle += dt * 3f;
                feet = Ground - 16f - Mathf.Abs(Mathf.Sin(ropeAngle)) * 8f;
                if (jump || (mouse != null && mouse.leftButton.wasReleasedThisFrame))
                {
                    rope = false; releasedRope = true; grounded = false;
                    vertical = -195f; airJump = true;
                }
            }
            else
            {
                if (jump)
                {
                    if (grounded) { vertical = -195f; grounded = false; }
                    else if (airJump) { vertical = -175f; airJump = false; doubleJump = true; }
                }
                if (!grounded)
                {
                    vertical += 420f * dt;
                    feet += vertical * dt;
                    if (feet >= Ground) { feet = Ground; vertical = 0; grounded = true; airJump = true; }
                }
            }

            bool shot = false;
            if (fire)
            {
                if (shotCooldown <= 0)
                {
                    shotCooldown = ReloadSeconds;
                    shotPulse = .17f;
                    shot = true;
                }
                else Show("ARMA RECARGANDO · ESPERA LA BARRA", 1f);
            }

            switch (task)
            {
                case 0:
                    if (crouched) Advance();
                    break;
                case 1:
                {
                    float x = 260 - travel;
                    if (x < HeroX + 12 && x + 27 > HeroX - 10 && feet > Ground - 18) Retry();
                    else if (x + 27 < HeroX - 10) Advance();
                    break;
                }
                case 2:
                    if (doubleJump) Advance();
                    break;
                case 3:
                {
                    float low = 260 - travel, high = 365 - travel;
                    if (!firstPassed && low < HeroX + 12 && low + 27 > HeroX - 10 && feet > Ground - 18) { Retry(); break; }
                    if (low + 27 < HeroX - 10) firstPassed = true;
                    if (firstPassed && high < HeroX + 12 && high + 30 > HeroX - 10 && (!crouched || !grounded)) { Retry(); break; }
                    if (firstPassed && high + 30 < HeroX - 10) Advance();
                    break;
                }
                case 4:
                case 5:
                {
                    float x = 245 - travel;
                    if (shot && x > HeroX + 18 && x < 250 && (task == 4 || !grounded))
                    {
                        hitPulse = .3f;
                        Advance();
                    }
                    else if (x < HeroX - 20) { travel = 0; Show("BLANCO PERDIDO · VUELVE A INTENTAR", 1.6f); }
                    break;
                }
                case 6:
                {
                    float x = 220 - travel;
                    float knotX = x + 59f;
                    float knotY = Ground - (tree ? tree.height : 144) + 79f;
                    if (click && !grounded && Mathf.Abs(pointer.x - knotX) < 16 &&
                        Mathf.Abs(pointer.y - knotY) < 16 && Mathf.Abs(knotX - HeroX) < 72)
                    {
                        rope = true; ropeAnchorX = knotX; ropeAngle = 0; Advance();
                    }
                    else if (x < -72) { travel = 0; Show("ACERCATE, SALTA Y TOCA EL NUDO", 1.6f); }
                    break;
                }
                case 7:
                {
                    float x = 130 - travel;
                    if (!rope && x < HeroX + 10 && x + 46 > HeroX - 8 && feet > Ground - 12)
                    { Retry(); break; }
                    if (releasedRope && x + 46 < HeroX - 8) Advance();
                    break;
                }
                case 8:
                {
                    float low = 250 - travel, target = 340 - travel;
                    if (!firstPassed && low < HeroX + 12 && low + 27 > HeroX - 10 && feet > Ground - 18) { Retry(); break; }
                    if (low + 27 < HeroX - 10) firstPassed = true;
                    if (shot && target > HeroX + 18 && target < 250) { targetHit = true; hitPulse = .3f; }
                    if (target < HeroX - 20 && !targetHit) { Retry(); break; }
                    if (firstPassed && targetHit) Advance();
                    break;
                }
                case 9:
                {
                    if (shot) { bossHits++; hitPulse = .32f; }
                    // The scarecrow gives a long warning, then throws a high disc.
                    if (attackTimer >= 4.1f && attackTimer < 5.1f)
                    {
                        float discX = 238 - (attackTimer - 4.1f) * 300f;
                        if (discX < HeroX + 12 && discX > HeroX - 12)
                        {
                            if (!crouched && grounded) { Retry(); break; }
                            bossDodged = true;
                        }
                    }
                    if (attackTimer >= 5.1f) attackTimer = 0;
                    if (bossHits >= 3 && bossDodged) Advance();
                    break;
                }
            }

            int gate = phase == 0 ? 4 : phase == 1 ? 9 : 10;
            if (chapterTime >= 40f)
            {
                if (task >= gate)
                {
                    if (phase == 2) Finish();
                    else RestartChapter(phase + 1, false);
                }
                else
                {
                    chapterTime = 0;
                    Show("REPITE ESTE TRAMO PARA COMPLETAR LA PRACTICA", 2.2f);
                }
            }
            if (!music.isPlaying && !complete) music.Play();
            music.volume = .34f * Mathf.Clamp01(PlayerPrefs.GetFloat("SuyuRun.Settings.MusicVol", 1f));
            if (keys != null && keys.escapeKey.wasPressedThisFrame) SceneManager.LoadScene("Level_Costa");
        }

        void Draw(Texture2D texture, float x, float y, float width, float height)
        {
            if (texture) GUI.DrawTexture(new Rect(Mathf.Round(x), Mathf.Round(y), width, height), texture, ScaleMode.StretchToFill, true);
        }

        void Layer(Texture2D texture, float factor, float y, float height)
        {
            if (!texture) return;
            float offset = Mathf.Floor(Mathf.Repeat(travel * factor + (factor == 0 ? 160f : 0), texture.width));
            Draw(texture, -offset, y, texture.width, height);
            Draw(texture, texture.width - offset, y, texture.width, height);
        }

        void Fill(float x, float y, float width, float height, Color color)
        {
            Color previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(new Rect(x, y, width, height), pixel);
            GUI.color = previous;
        }

        void Text(string value, Rect area, int size, Color color, TextAnchor align = TextAnchor.MiddleCenter)
        {
            var style = new GUIStyle(GUI.skin.label) { font = font, fontSize = size, alignment = align, wordWrap = true };
            style.normal.textColor = new Color32(35, 28, 46, 255);
            GUI.Label(new Rect(area.x + 1, area.y + 1, area.width, area.height), value, style);
            style.normal.textColor = color;
            GUI.Label(area, value, style);
        }

        void Scenery()
        {
            Layer(sky, 0, 0, 180);
            Layer(cliffs, .16f, 0, 180);
            Layer(sea, .3f, 0, 180);
            Layer(dunes, .55f, 0, 180);
            Layer(ground, 1f, Ground, 36);

            if (task == 1 || task == 3 || task == 8)
            {
                float x = (task == 1 ? 260 : task == 3 ? 260 : 250) - travel;
                Draw(barrier, x, Ground - 18, 32, 18);
                if (task == 3) Draw(barrier, 365 - travel, 96, 32, 18);
            }
            if (task == 4 || task == 5 || task == 8)
            {
                float x = (task == 8 ? 340 : 245) - travel;
                Draw(flyingTarget, x, task == 5 ? 71 : 112, 28, 24);
            }
            if (task == 6)
                Draw(tree, 220 - travel, Ground - (tree ? tree.height : 144), 78, tree ? tree.height : 144);
            if (task == 7)
            {
                Draw(tree, ropeAnchorX - 59, Ground - (tree ? tree.height : 144), 78, tree ? tree.height : 144);
                Draw(gap, 130 - travel, Ground, 72, 36);
            }
            if (task >= 9)
            {
                Draw(dummy, 229, Ground - 70 + Mathf.Round(Mathf.Sin(Time.time * 2) * 2), 44, 70);
                if (attackTimer > 3.2f && attackTimer < 4.1f)
                    Text("!", new Rect(235, 47, 32, 24), 14, new Color32(255, 220, 124, 255));
                if (attackTimer >= 4.1f && attackTimer < 5.1f)
                    Draw(flyingTarget, 238 - (attackTimer - 4.1f) * 300f, 91, 28, 24);
            }
        }

        void Hero()
        {
            float drawX = HeroX - 20;
            if (rope)
            {
                // The tether follows the real anchor; the player is only lifted after a valid grab.
                float handX = HeroX + 5, handY = feet - 27;
                float distance = Vector2.Distance(new Vector2(ropeAnchorX, 79), new Vector2(handX, handY));
                if (distance > 1)
                {
                    var old = GUI.matrix;
                    GUIUtility.RotateAroundPivot(Mathf.Atan2(handY - 79, handX - ropeAnchorX) * Mathf.Rad2Deg - 90,
                        new Vector2(ropeAnchorX, 79));
                    Fill(ropeAnchorX - 1, 79, 2, distance, new Color32(242, 196, 116, 255));
                    GUI.matrix = old;
                }
            }
            var frame = TaytaArt.Frame("costa", crouched ? "slide" : grounded ? "run" : "jump", Time.time);
            if (!frame) return;
            frame.texture.filterMode = FilterMode.Point;
            float height = crouched ? 29 : 47;
            var source = frame.rect;
            GUI.DrawTextureWithTexCoords(new Rect(drawX, feet - height, 42, height), frame.texture,
                new Rect(source.x / frame.texture.width, source.y / frame.texture.height,
                    source.width / frame.texture.width, source.height / frame.texture.height));
            if (shotPulse > 0)
            {
                float shotY = feet - (crouched ? 22 : 29);
                float shotX = task == 9 ? 230 : task == 8 ? 340 - travel : 245 - travel;
                Fill(HeroX + 16, shotY, Mathf.Clamp(shotX - HeroX - 16, 0, 165), 2,
                    new Color32(255, 224, 144, 255));
            }
        }

        void Hud()
        {
            Color dark = new Color32(35, 28, 46, 255);
            Color cream = new Color32(255, 235, 179, 255);
            Color gold = new Color32(250, 197, 104, 255);
            Draw(uiTop, 8, 7, 304, 24);
            Text("TUTORIAL  " + (phase + 1) + "/3", new Rect(19, 10, 112, 17), 7, cream, TextAnchor.MiddleLeft);
            Text(Mathf.FloorToInt(chapterTime) + "/40 S", new Rect(242, 10, 61, 17), 7, cream);
            Fill(133, 17, 101, 5, dark);
            Fill(135, 18, 97 * Mathf.Clamp01(chapterTime / 40f), 3, gold);

            Draw(uiPrompt, 52, 35, 216, 18);
            string instruction = !started ? "PULSA ESPACIO PARA COMENZAR" : complete ? "TUTORIAL COMPLETADO" : task >= 10 ?
                "BIEN · MANTENTE HASTA EL FIN DEL TRAMO" : task == 4 && shotCooldown > 0 ?
                "RECARGA: ESPERA 1.2 SEGUNDOS ENTRE DISPAROS" : prompts[task];
            Text(instruction, new Rect(58, 36, 204, 15), 4, dark);

            Draw(uiTop, 155, 156, 157, 20);
            Text("ARMA", new Rect(162, 159, 39, 13), 6, cream, TextAnchor.MiddleLeft);
            Fill(205, 163, 94, 5, dark);
            Fill(207, 164, 90 * (1 - shotCooldown / ReloadSeconds), 3, gold);
            if (task == 9)
                Text("BLANCOS " + bossHits + "/3  ·  ESQUIVE " + (bossDodged ? "SI" : "NO"),
                    new Rect(127, 134, 182, 16), 6, cream, TextAnchor.MiddleRight);
            if (music.volume <= .001f)
                Text("MUSICA EN 0% · REVISA AJUSTES", new Rect(8, 144, 200, 12), 5, cream, TextAnchor.MiddleLeft);
            if (Time.unscaledTime < noticeUntil)
            {
                Draw(successFlash ? uiSuccess : uiPrompt, successFlash ? 64 : 58, 61,
                    successFlash ? 192 : 204, 15);
                Text(notice, new Rect(successFlash ? 70 : 64, 64,
                    successFlash ? 180 : 192, 11), 4, successFlash ? new Color32(26, 86, 58, 255) : dark);
            }
            if (complete)
            {
                Draw(uiSuccess, 64, 61, 192, 15);
                Text("LISTO PARA COSTA", new Rect(70, 64, 180, 11), 5, new Color32(26, 86, 58, 255));
                Draw(uiPrompt, 94, 112, 132, 30);
                Text("IR A COSTA", new Rect(99, 115, 122, 23), 7, dark);
                if (GUI.Button(new Rect(94, 112, 132, 30), GUIContent.none, GUIStyle.none))
                    SceneManager.LoadScene("Level_Costa");
            }
        }

        void OnGUI()
        {
            var old = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(new Vector3(Screen.width / 320f, Screen.height / 180f, 1));
            Scenery();
            Hero();
            Hud();
            GUI.matrix = old;
        }
    }
}
