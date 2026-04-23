using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StepDevil
{
    // ══════════════════════════════════════════════════════════════
    //  SDAnimationLibrary — every animation from STEP_DEVIL_game.html
    //  ported to Unity using the SDTween engine.
    //
    //  All methods are static. Call them from any MonoBehaviour.
    //  Every method returns its SDTweenHandle so callers can
    //  chain .OnCompleteCallback(), .Kill(), etc.
    // ══════════════════════════════════════════════════════════════
    public static class SDAnimationLibrary
    {
        // ─────────────────────────────────────────────────────────
        //  1. DEVIL BOB — title screen icon floats up and down
        //     CSS: translateY(0) → translateY(-8px) → 0, 2s infinite
        // ─────────────────────────────────────────────────────────
        public static SDTweenHandle DevilBob(RectTransform rt, float amplitude = 8f, float duration = 2f)
        {
            var startY = rt.anchoredPosition.y;
            return SDTween.To(duration, t =>
            {
                if (!rt) return;
                // t goes 0→1 with InOutQuad; pingPong reverses, giving smooth bob
                var pos = rt.anchoredPosition;
                pos.y = startY + amplitude * t;
                rt.anchoredPosition = pos;
            }, rt)
            .SetEase(SDEase.InOutQuad)
            .SetLoops(-1, pingPong: true);
        }

        // ─────────────────────────────────────────────────────────
        //  2. BLIP JUMP — character jumps toward chosen stone
        //     CSS: .jl  translateX(-65) translateY(-16) rotate(-20deg)
        //          .jr  translateX(65)  translateY(-16) rotate(20deg)
        //          .jc  translateY(-24) scale(.95)
        // ─────────────────────────────────────────────────────────
        public static SDTweenHandle BlipJumpLeft(RectTransform rt, float dur = 0.35f)
        {
            SDTween.Kill(rt);
            var from = rt.anchoredPosition;
            var to = new Vector2(-65f, -16f);
            return SDTween.To(dur, t =>
            {
                if (!rt) return;
                rt.anchoredPosition = Vector2.LerpUnclamped(from, to, t);
                rt.localEulerAngles = new Vector3(0, 0, Mathf.LerpUnclamped(0, -20f, t));
            }, rt).SetEase(SDEase.OutBack);
        }

        public static SDTweenHandle BlipJumpRight(RectTransform rt, float dur = 0.35f)
        {
            SDTween.Kill(rt);
            var from = rt.anchoredPosition;
            var to = new Vector2(65f, -16f);
            return SDTween.To(dur, t =>
            {
                if (!rt) return;
                rt.anchoredPosition = Vector2.LerpUnclamped(from, to, t);
                rt.localEulerAngles = new Vector3(0, 0, Mathf.LerpUnclamped(0, 20f, t));
            }, rt).SetEase(SDEase.OutBack);
        }

        public static SDTweenHandle BlipJumpCenter(RectTransform rt, float dur = 0.35f)
        {
            SDTween.Kill(rt);
            var from = rt.anchoredPosition;
            var to = new Vector2(0f, -24f);
            var fromScale = rt.localScale;
            return SDTween.To(dur, t =>
            {
                if (!rt) return;
                rt.anchoredPosition = Vector2.LerpUnclamped(from, to, t);
                rt.localScale = Vector3.LerpUnclamped(fromScale, Vector3.one * 0.95f, t);
            }, rt).SetEase(SDEase.OutBack);
        }

        /// <summary>Jump to arbitrary position (for 2-stone or 3-stone forks).</summary>
        public static SDTweenHandle BlipJump(RectTransform rt, Vector2 targetPos, float targetRotZ = 0f, float dur = 0.35f)
        {
            SDTween.Kill(rt);
            var fromPos = rt.anchoredPosition;
            var fromRot = rt.localEulerAngles.z;
            return SDTween.To(dur, t =>
            {
                if (!rt) return;
                rt.anchoredPosition = Vector2.LerpUnclamped(fromPos, targetPos, t);
                rt.localEulerAngles = new Vector3(0, 0, Mathf.LerpUnclamped(fromRot, targetRotZ, t));
            }, rt).SetEase(SDEase.OutBack);
        }

        // ─────────────────────────────────────────────────────────
        //  3. BLIP BOUNCE — safe landing
        //     CSS: scale(1.25) → translateY(-18) scale(.88) →
        //          translateY(4) scale(1.08) → scale(1), 0.5s
        // ─────────────────────────────────────────────────────────
        public static SDTweenHandle BlipBounce(RectTransform rt, float dur = 0.5f)
        {
            SDTween.Kill(rt);
            var basePos = rt.anchoredPosition;
            return SDTween.To(dur, t =>
            {
                if (!rt) return;
                float s, yOff;
                if (t < 0.0f) { s = 1f; yOff = 0f; }
                else if (t < 0.25f)
                {
                    // 0→0.25: scale 1→1.25
                    var lt = t / 0.25f;
                    s = Mathf.Lerp(1f, 1.25f, lt);
                    yOff = 0f;
                }
                else if (t < 0.5f)
                {
                    // 0.25→0.5: scale 1.25→0.88, y → -18
                    var lt = (t - 0.25f) / 0.25f;
                    s = Mathf.Lerp(1.25f, 0.88f, lt);
                    yOff = Mathf.Lerp(0f, -18f, lt);
                }
                else if (t < 0.8f)
                {
                    // 0.5→0.8: scale 0.88→1.08, y → +4
                    var lt = (t - 0.5f) / 0.3f;
                    s = Mathf.Lerp(0.88f, 1.08f, lt);
                    yOff = Mathf.Lerp(-18f, 4f, lt);
                }
                else
                {
                    // 0.8→1.0: scale 1.08→1, y → 0
                    var lt = (t - 0.8f) / 0.2f;
                    s = Mathf.Lerp(1.08f, 1f, lt);
                    yOff = Mathf.Lerp(4f, 0f, lt);
                }

                rt.localScale = Vector3.one * s;
                var p = basePos;
                p.y += yOff;
                rt.anchoredPosition = p;
            }, rt).SetEase(SDEase.Linear); // keyframes are manually timed
        }

        // ─────────────────────────────────────────────────────────
        //  4. BLIP FALL — character falls into void
        //     CSS: translateY(120) rotate(180deg) scale(.4) opacity 0
        // ─────────────────────────────────────────────────────────
        public static SDTweenHandle BlipFall(RectTransform rt, CanvasGroup cg = null, float dur = 0.6f)
        {
            SDTween.Kill(rt);
            var fromPos = rt.anchoredPosition;
            var toPos = fromPos + new Vector2(0, -120f); // fall downward in Unity coords
            return SDTween.To(dur, t =>
            {
                if (!rt) return;
                rt.anchoredPosition = Vector2.LerpUnclamped(fromPos, toPos, t);
                rt.localEulerAngles = new Vector3(0, 0, Mathf.LerpUnclamped(0, 180f, t));
                rt.localScale = Vector3.one * Mathf.LerpUnclamped(1f, 0.4f, t);
                if (cg) cg.alpha = 1f - t;
            }, rt).SetEase(SDEase.InCubic);
        }

        // ─────────────────────────────────────────────────────────
        //  5. BLIP LAND — scale up on safe landing
        //     CSS: scale(1.25)
        // ─────────────────────────────────────────────────────────
        public static SDTweenHandle BlipLand(RectTransform rt, float dur = 0.2f)
        {
            return SDTween.Scale(rt, Vector3.one * 1.25f, dur).SetEase(SDEase.OutBack);
        }

        // ─────────────────────────────────────────────────────────
        //  6. MIRROR BLINK — warning text blinks
        //     CSS: opacity 1 → 0.4, 1s infinite
        // ─────────────────────────────────────────────────────────
        public static SDTweenHandle MirrorBlink(TextMeshProUGUI txt, float dur = 1f)
        {
            SDTween.Kill(txt);
            return SDTween.To(dur, t =>
            {
                if (!txt) return;
                var c = txt.color;
                c.a = Mathf.LerpUnclamped(1f, 0.4f, t);
                txt.color = c;
            }, txt)
            .SetEase(SDEase.InOutQuad)
            .SetLoops(-1, pingPong: true);
        }

        public static SDTweenHandle MirrorBlink(CanvasGroup cg, float dur = 1f)
        {
            SDTween.Kill(cg);
            return SDTween.Fade(cg, 0.4f, dur)
                .SetEase(SDEase.InOutQuad)
                .SetLoops(-1, pingPong: true);
        }

        // ─────────────────────────────────────────────────────────
        //  7. STONE POP SAFE — safe stone reveal pop
        //     CSS: scale(1) → scale(1.07) → scale(1), 0.4s
        // ─────────────────────────────────────────────────────────
        public static SDTweenHandle StonePopSafe(RectTransform rt, float dur = 0.4f)
        {
            SDTween.Kill(rt);
            return SDTween.To(dur, t =>
            {
                if (!rt) return;
                float s;
                if (t < 0.5f)
                    s = Mathf.Lerp(1f, 1.07f, t / 0.5f);
                else
                    s = Mathf.Lerp(1.07f, 1f, (t - 0.5f) / 0.5f);
                rt.localScale = Vector3.one * s;
            }, rt).SetEase(SDEase.Linear);
        }

        // ─────────────────────────────────────────────────────────
        //  8. STONE SHAKE VOID — void stone reveal shake
        //     CSS: translateX(0) → -7 → 7 → 0, 0.4s
        // ─────────────────────────────────────────────────────────
        public static SDTweenHandle StoneShakeVoid(RectTransform rt, float intensity = 7f, float dur = 0.4f)
        {
            SDTween.Kill(rt);
            var baseX = rt.anchoredPosition.x;
            return SDTween.To(dur, t =>
            {
                if (!rt) return;
                float offX;
                if (t < 0.25f)
                    offX = Mathf.Lerp(0, -intensity, t / 0.25f);
                else if (t < 0.75f)
                    offX = Mathf.Lerp(-intensity, intensity, (t - 0.25f) / 0.5f);
                else
                    offX = Mathf.Lerp(intensity, 0, (t - 0.75f) / 0.25f);

                var pos = rt.anchoredPosition;
                pos.x = baseX + offX;
                rt.anchoredPosition = pos;
            }, rt).SetEase(SDEase.Linear);
        }

        // ─────────────────────────────────────────────────────────
        //  9. FLASH SCREEN — full screen color overlay flash
        //     CSS: opacity 0 → 1 → 0 over 0.48s
        // ─────────────────────────────────────────────────────────
        public static SDTweenHandle FlashScreen(Image overlay, Color color, float dur = 0.48f)
        {
            SDTween.Kill(overlay);
            overlay.color = color;
            overlay.transform.SetAsLastSibling();
            return SDTween.To(dur, t =>
            {
                if (!overlay) return;
                float a;
                if (t < 0.3f)
                    a = Mathf.Lerp(0f, color.a, t / 0.3f);
                else
                    a = Mathf.Lerp(color.a, 0f, (t - 0.3f) / 0.7f);

                var c = color;
                c.a = a;
                overlay.color = c;
            }, overlay)
            .SetEase(SDEase.Linear)
            .OnCompleteCallback(() =>
            {
                if (overlay)
                {
                    overlay.color = new Color(0, 0, 0, 0);
                    overlay.transform.SetAsFirstSibling();
                }
            });
        }

        // ─────────────────────────────────────────────────────────
        // 10. FLOAT TEXT — floating +10, FLIPPED!, etc.
        //     CSS: translate up, scale 1 → 1.4, opacity 1 → 0, 0.9s
        // ─────────────────────────────────────────────────────────
        public static SDTweenHandle FloatTextUp(RectTransform rt, float distance = 100f, float dur = 0.9f)
        {
            var startPos = rt.anchoredPosition;
            var startScale = rt.localScale;
            var cg = rt.GetComponent<CanvasGroup>();
            if (cg == null) cg = rt.gameObject.AddComponent<CanvasGroup>();

            return SDTween.To(dur, t =>
            {
                if (!rt) return;
                var pos = startPos;
                pos.y += distance * t;
                rt.anchoredPosition = pos;
                rt.localScale = startScale * Mathf.LerpUnclamped(1f, 1.4f, t);
                if (cg) cg.alpha = 1f - t;
            }, rt)
            .SetEase(SDEase.OutCubic)
            .OnCompleteCallback(() =>
            {
                if (rt) UnityEngine.Object.Destroy(rt.gameObject);
            });
        }

        // ─────────────────────────────────────────────────────────
        // 11. CONFETTI FALL — particle dots falling & spinning
        //     CSS: translateY(-10 → 730) rotate(0 → 540deg) opacity 1→0, 3s
        // ─────────────────────────────────────────────────────────
        public static SDTweenHandle ConfettiFall(RectTransform rt, float fallDistance = 730f, float dur = 3f)
        {
            var startY = rt.anchoredPosition.y;
            return SDTween.To(dur, t =>
            {
                if (!rt) return;
                var pos = rt.anchoredPosition;
                pos.y = startY - fallDistance * t;
                rt.anchoredPosition = pos;
                rt.localEulerAngles = new Vector3(0, 0, 540f * t);

                var img = rt.GetComponent<Image>();
                if (img)
                {
                    var c = img.color;
                    c.a = 1f - t;
                    img.color = c;
                }
            }, rt).SetEase(SDEase.Linear);
        }

        // ─────────────────────────────────────────────────────────
        // 12. TIMER BAR — smooth fill decrease
        //     CSS: width transition 0.08s linear
        // ─────────────────────────────────────────────────────────
        public static SDTweenHandle TimerBarDecrease(Image fillImg, float from, float to, float dur)
        {
            SDTween.Kill(fillImg);
            return SDTween.FillAmount(fillImg, to, dur).SetEase(SDEase.Linear);
        }

        // ─────────────────────────────────────────────────────────
        // 13. PATH DOT PULSE — active dot scales up with glow
        //     CSS: scale(1.4) with box-shadow glow
        // ─────────────────────────────────────────────────────────
        public static SDTweenHandle PathDotPulse(RectTransform rt, float dur = 0.6f)
        {
            return SDTween.Scale(rt, Vector3.one * 1.4f, dur)
                .SetEase(SDEase.OutElastic);
        }

        public static SDTweenHandle PathDotShrink(RectTransform rt, float dur = 0.3f)
        {
            return SDTween.Scale(rt, Vector3.one, dur).SetEase(SDEase.OutQuad);
        }

        // ─────────────────────────────────────────────────────────
        // 14. BUTTON PRESS — tap feedback
        //     CSS: scale(.95) on :active
        // ─────────────────────────────────────────────────────────
        public static SDTweenHandle ButtonPress(RectTransform rt, float dur = 0.1f)
        {
            SDTween.Kill(rt);
            rt.localScale = Vector3.one * 0.95f;
            return SDTween.Scale(rt, Vector3.one, dur).SetEase(SDEase.OutBack);
        }

        // ─────────────────────────────────────────────────────────
        // 15. STONE SELECTED — chosen stone scales up
        //     CSS: transform: scale(1.06)
        // ─────────────────────────────────────────────────────────
        public static SDTweenHandle StoneSelect(RectTransform rt, float dur = 0.15f)
        {
            return SDTween.Scale(rt, Vector3.one * 1.06f, dur).SetEase(SDEase.OutBack);
        }

        // ─────────────────────────────────────────────────────────
        // 16. SCREEN FADE IN — screen transition (fade + slide up)
        // ─────────────────────────────────────────────────────────
        public static SDTweenHandle ScreenFadeIn(CanvasGroup cg, RectTransform rt = null, float dur = 0.35f)
        {
            cg.alpha = 0f;
            SDTween.Kill(cg);
            var handle = SDTween.Fade(cg, 1f, dur).SetEase(SDEase.OutCubic);

            if (rt != null)
            {
                var targetPos = rt.anchoredPosition;
                rt.anchoredPosition = targetPos + new Vector2(0, -30f);
                SDTween.MoveAnchoredPos(rt, targetPos, dur).SetEase(SDEase.OutCubic);
            }

            return handle;
        }

        public static SDTweenHandle ScreenFadeOut(CanvasGroup cg, float dur = 0.2f)
        {
            SDTween.Kill(cg);
            return SDTween.Fade(cg, 0f, dur).SetEase(SDEase.InQuad);
        }

        // ─────────────────────────────────────────────────────────
        // 17. STONE FADE OTHERS — dim non-chosen stones
        //     CSS: opacity .28
        // ─────────────────────────────────────────────────────────
        public static SDTweenHandle FadeCanvasGroup(CanvasGroup cg, float toAlpha, float dur = 0.3f)
        {
            return SDTween.Fade(cg, toAlpha, dur).SetEase(SDEase.OutQuad);
        }

        // ─────────────────────────────────────────────────────────
        // 18. COLOR TRANSITION — smooth stone color reveal
        // ─────────────────────────────────────────────────────────
        public static SDTweenHandle ColorReveal(Image img, Color toColor, float dur = 0.3f)
        {
            return SDTween.ColorTo(img, toColor, dur).SetEase(SDEase.OutQuad);
        }

        // ─────────────────────────────────────────────────────────
        // 19. BLIP RESET — single tween for pos + scale + rotation (avoids multi-tween jitter)
        // ─────────────────────────────────────────────────────────
        public static SDTweenHandle BlipReset(RectTransform rt, float dur = 0.38f)
        {
            SDTween.Kill(rt);
            var fromPos = rt.anchoredPosition;
            var fromScale = rt.localScale;
            var fromEuler = rt.localEulerAngles;
            return SDTween.To(dur, t =>
            {
                if (!rt)
                    return;
                rt.anchoredPosition = Vector2.LerpUnclamped(fromPos, Vector2.zero, t);
                rt.localScale = Vector3.LerpUnclamped(fromScale, Vector3.one, t);
                // Shortest arc to 0° — Vector3 lerp on euler Z can spin ~360° (e.g. 340° → 0) after left jumps.
                var x = Mathf.LerpUnclamped(fromEuler.x, 0f, t);
                var y = Mathf.LerpUnclamped(fromEuler.y, 0f, t);
                var z = Mathf.LerpAngle(fromEuler.z, 0f, t);
                rt.localEulerAngles = new Vector3(x, y, z);
            }, rt).SetEase(SDEase.OutCubic);
        }
    }
}
