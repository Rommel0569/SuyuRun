using UnityEngine;

namespace SuyuRun
{
    // Costa's ending: reach the dimensional door at the finish, get the arms-reveal animation,
    // a surprise "?!", then the door's vortex pulls the hero in before the run counts as Complete.
    // Art approved under ART_BIBLE.md: costa_puerta_dimensional, costa_heroe_logro,
    // costa_efecto_sorpresa, costa_efecto_absorcion.
    public sealed partial class RunnerPrototype
    {
        Sprite[] heroLogro, sorpresaFrames, absorcionFrames;
        Transform portalAnchor, sorpresaAnchor, absorcionAnchor;
        FrameAnimator sorpresaAnimator, absorcionAnimator;
        float portalX;
        int endingPhase;
        float endingTimer;
        const float ArmsPhase = 1.4f, SurprisePhase = .8f, AbsorbPhase = 1.3f, GonePhase = .4f;

        void BuildCoastalEnding()
        {
            heroLogro = HeroFrames(Resources.LoadAll<Sprite>("costa_heroe_logro"), "logro", 4);
            // Every other world item lands at the hero's fixed screen x (-3) exactly when
            // it.x==distance, because the shared scroll formula (it.x-distance-3) already bakes in
            // that -3. Subtracting an extra 3 here made the door register 3 units EARLIER than the
            // hero could ever reach it - by the time distance caught up to Duration*Speed (the
            // ending's own trigger), the door had already scrolled a beat past the hero, so the
            // absorb sequence played over empty ground instead of the doorway.
            portalX = Duration * Speed;

            var puertaFrames = HeroFrames(Resources.LoadAll<Sprite>("costa_puerta_dimensional"), "puerta", 2);
            portalAnchor = WorldSprite("Dimensional door (ART_BIBLE costa_puerta_dimensional)", puertaFrames, 2f, true, 3, out _);

            sorpresaFrames = HeroFrames(Resources.LoadAll<Sprite>("costa_efecto_sorpresa"), "sorpresa", 3);
            sorpresaAnchor = WorldSprite("Surprise mark", sorpresaFrames, 6f, false, 19, out sorpresaAnimator);
            sorpresaAnchor.gameObject.SetActive(false);

            absorcionFrames = HeroFrames(Resources.LoadAll<Sprite>("costa_efecto_absorcion"), "absorcion", 4);
            absorcionAnchor = WorldSprite("Absorption vortex", absorcionFrames, 8f, true, 18, out absorcionAnimator);
            absorcionAnchor.gameObject.SetActive(false);
        }

        // Anchors a top-left-pivot sprite (ART_BIBLE.md convention) so its bottom-center sits at the
        // anchor transform's position, like BuildHero does for the hero itself - lets these play as
        // ordinary world objects (position set every frame) without PixelSprite's square-only sizing.
        Transform WorldSprite(string name, Sprite[] frames, float fps, bool loop, int order, out FrameAnimator animator)
        {
            var anchor = new GameObject(name).transform;
            var spriteGo = new GameObject(name + " sprite");
            spriteGo.transform.SetParent(anchor, false);
            float w = 0, h = 0;
            if (frames != null && frames.Length > 0 && frames[0])
            {
                w = frames[0].rect.width / frames[0].pixelsPerUnit;
                h = frames[0].rect.height / frames[0].pixelsPerUnit;
            }
            spriteGo.transform.localPosition = new Vector3(-w * .5f, h, 0);
            var sr = spriteGo.AddComponent<SpriteRenderer>();
            sr.sortingOrder = order;
            if (frames != null && frames.Length > 0) sr.sprite = frames[0];
            animator = spriteGo.AddComponent<FrameAnimator>();
            animator.SetFrames(frames, fps, loop);
            return anchor;
        }

        void UpdateCoastalEnding()
        {
            if (portalAnchor) portalAnchor.position = new Vector3(portalX - distance - 3, Ground, 0);
            if (portalAnchor) portalAnchor.gameObject.SetActive(mode != Mode.Menu && mode != Mode.Museum && Mathf.Abs(portalX - distance) < 28);

            if (mode != Mode.Ending)
            {
                if (sorpresaAnchor && sorpresaAnchor.gameObject.activeSelf) sorpresaAnchor.gameObject.SetActive(false);
                if (absorcionAnchor && absorcionAnchor.gameObject.activeSelf) absorcionAnchor.gameObject.SetActive(false);
                return;
            }

            endingTimer += Time.deltaTime;
            if (sorpresaAnchor) sorpresaAnchor.position = new Vector3(-3, feet + 1.9f, 0);
            if (absorcionAnchor) absorcionAnchor.position = new Vector3(-3, feet + .8f, 0);

            switch (endingPhase)
            {
                case 0: // arms reveal plays (see UpdateHeroSprite)
                    if (endingTimer >= ArmsPhase)
                    {
                        endingPhase = 1; endingTimer = 0;
                        if (sorpresaAnimator) sorpresaAnimator.SetFrames(sorpresaFrames, 6f, false);
                        if (sorpresaAnchor) sorpresaAnchor.gameObject.SetActive(true);
                        feedback = "¡TIENES MANOS!"; feedbackUntil = Time.unscaledTime + 1; Beep(1400, .12f);
                    }
                    break;
                case 1: // "?!" pop
                    if (endingTimer >= SurprisePhase)
                    {
                        endingPhase = 2; endingTimer = 0;
                        if (sorpresaAnchor) sorpresaAnchor.gameObject.SetActive(false);
                        if (absorcionAnchor) absorcionAnchor.gameObject.SetActive(true);
                        feedback = "EL PORTAL TE ABSORBE..."; feedbackUntil = Time.unscaledTime + 1.2f; Beep(220, .3f);
                    }
                    break;
                case 2: // absorbed - hero shrinks toward the door
                    if (heroRenderer)
                    {
                        float t = Mathf.Clamp01(endingTimer / AbsorbPhase);
                        heroRenderer.transform.localScale = Vector3.one * Mathf.Max(0, 1 - t);
                    }
                    if (endingTimer >= AbsorbPhase)
                    {
                        endingPhase = 3; endingTimer = 0;
                        if (absorcionAnchor) absorcionAnchor.gameObject.SetActive(false);
                        if (heroRenderer) heroRenderer.enabled = false;
                    }
                    break;
                case 3: // brief pause on the empty doorway before the results screen
                    if (endingTimer >= GonePhase)
                    {
                        mode = Mode.Complete;
                        bank += coins; best = Mathf.Max(best, score);
                        if (!AutomatedSmokeTest) { PlayerPrefs.SetInt("SuyuRun.Prototype.Bank", bank); PlayerPrefs.SetInt("SuyuRun.Prototype.Best", best); PlayerPrefs.Save(); }
                    }
                    break;
            }
        }
    }
}
