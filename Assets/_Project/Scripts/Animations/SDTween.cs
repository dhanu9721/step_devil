using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace StepDevil
{
    // ══════════════════════════════════════════════════════════════
    //  SDTween — lightweight tween engine (DOTween-like API)
    //  Zero dependencies. Attach to any scene via SDTween.Init().
    // ══════════════════════════════════════════════════════════════

    /// <summary>Easing functions matching common CSS / DOTween curves.</summary>
    public enum SDEase
    {
        Linear,
        InQuad, OutQuad, InOutQuad,
        InCubic, OutCubic, InOutCubic,
        InBack, OutBack, InOutBack,
        InElastic, OutElastic,
        InBounce, OutBounce,
    }

    /// <summary>A single running tween. Returned by every SDTween.To / shorthand so you can chain options.</summary>
    public sealed class SDTweenHandle
    {
        internal int Id;
        internal float Duration;
        internal float Elapsed;
        internal float Delay;
        internal bool UseUnscaledTime = true;
        internal bool IsComplete;
        internal bool IsPaused;
        internal SDEase Ease = SDEase.OutQuad;
        internal int LoopCount = 1;        // -1 = infinite
        internal bool PingPong;
        internal bool Forward = true;
        internal int LoopsDone;
        internal Action<float> OnUpdate;    // receives normalized 0..1
        internal Action OnComplete;
        internal Action OnKill;
        internal object Target;             // for kill-by-target

        // ── Fluent API ───────────────────────────────────────────

        public SDTweenHandle SetEase(SDEase ease) { Ease = ease; return this; }
        public SDTweenHandle SetDelay(float d) { Delay = d; return this; }
        public SDTweenHandle SetLoops(int count, bool pingPong = false) { LoopCount = count; PingPong = pingPong; return this; }
        public SDTweenHandle SetUpdate(bool useUnscaledTime) { UseUnscaledTime = useUnscaledTime; return this; }
        public SDTweenHandle OnCompleteCallback(Action cb) { OnComplete = cb; return this; }
        public SDTweenHandle OnKillCallback(Action cb) { OnKill = cb; return this; }

        public void Kill()
        {
            if (IsComplete) return;
            IsComplete = true;
            OnKill?.Invoke();
        }

        public void Pause() => IsPaused = true;
        public void Resume() => IsPaused = false;
    }

    /// <summary>Main tween manager — auto-creates itself on first use.</summary>
    public sealed class SDTween : MonoBehaviour
    {
        static SDTween _instance;
        static int _nextId;
        readonly List<SDTweenHandle> _active = new();
        readonly List<SDTweenHandle> _toAdd = new();

        // ── Bootstrapping ────────────────────────────────────────

        static SDTween Instance
        {
            get
            {
                if (_instance == null) Init();
                return _instance;
            }
        }

        public static void Init()
        {
            if (_instance != null) return;
            var go = new GameObject("[SDTween]");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<SDTween>();
        }

        // ── Core factory ─────────────────────────────────────────

        /// <summary>Generic tween: calls <paramref name="onUpdate"/> each frame with eased 0..1.</summary>
        public static SDTweenHandle To(float duration, Action<float> onUpdate, object target = null)
        {
            var h = new SDTweenHandle
            {
                Id = ++_nextId,
                Duration = Mathf.Max(duration, 0.001f),
                OnUpdate = onUpdate,
                Target = target
            };
            Instance._toAdd.Add(h);
            return h;
        }

        // ── Kill helpers ─────────────────────────────────────────

        /// <summary>Kill all tweens targeting <paramref name="target"/>.</summary>
        public static void Kill(object target)
        {
            if (_instance == null) return;
            foreach (var h in _instance._active)
                if (!h.IsComplete && h.Target == target) h.Kill();
            foreach (var h in _instance._toAdd)
                if (!h.IsComplete && h.Target == target) h.Kill();
        }

        public static void KillAll()
        {
            if (_instance == null) return;
            foreach (var h in _instance._active) h.Kill();
            foreach (var h in _instance._toAdd) h.Kill();
        }

        // ── Shorthand: RectTransform ─────────────────────────────

        public static SDTweenHandle MoveAnchoredPos(RectTransform rt, Vector2 to, float dur)
        {
            var from = rt.anchoredPosition;
            return To(dur, t => { if (rt) rt.anchoredPosition = Vector2.LerpUnclamped(from, to, t); }, rt);
        }

        public static SDTweenHandle Scale(Transform tr, Vector3 to, float dur)
        {
            var from = tr.localScale;
            return To(dur, t => { if (tr) tr.localScale = Vector3.LerpUnclamped(from, to, t); }, tr);
        }

        public static SDTweenHandle ScaleX(Transform tr, float to, float dur)
        {
            var from = tr.localScale;
            var target = new Vector3(to, from.y, from.z);
            return To(dur, t => { if (tr) tr.localScale = Vector3.LerpUnclamped(from, target, t); }, tr);
        }

        public static SDTweenHandle Rotate(Transform tr, Vector3 toEuler, float dur)
        {
            var from = tr.localEulerAngles;
            return To(dur, t => { if (tr) tr.localEulerAngles = Vector3.LerpUnclamped(from, toEuler, t); }, tr);
        }

        public static SDTweenHandle LocalMove(Transform tr, Vector3 to, float dur)
        {
            var from = tr.localPosition;
            return To(dur, t => { if (tr) tr.localPosition = Vector3.LerpUnclamped(from, to, t); }, tr);
        }

        // ── Shorthand: UI ────────────────────────────────────────

        public static SDTweenHandle Fade(CanvasGroup cg, float to, float dur)
        {
            var from = cg.alpha;
            return To(dur, t => { if (cg) cg.alpha = Mathf.LerpUnclamped(from, to, t); }, cg);
        }

        public static SDTweenHandle FadeImage(Image img, float toAlpha, float dur)
        {
            var from = img.color;
            var to = new Color(from.r, from.g, from.b, toAlpha);
            return To(dur, t => { if (img) img.color = Color.LerpUnclamped(from, to, t); }, img);
        }

        public static SDTweenHandle ColorTo(Image img, Color to, float dur)
        {
            var from = img.color;
            return To(dur, t => { if (img) img.color = Color.LerpUnclamped(from, to, t); }, img);
        }

        public static SDTweenHandle FadeText(TextMeshProUGUI txt, float toAlpha, float dur)
        {
            var from = txt.color;
            var to = new Color(from.r, from.g, from.b, toAlpha);
            return To(dur, t => { if (txt) txt.color = Color.LerpUnclamped(from, to, t); }, txt);
        }

        public static SDTweenHandle FillAmount(Image img, float to, float dur)
        {
            var from = img.fillAmount;
            return To(dur, t => { if (img) img.fillAmount = Mathf.LerpUnclamped(from, to, t); }, img);
        }

        // ── Shorthand: sequences via delay chaining ──────────────

        /// <summary>Calls action after delay (unscaled time). Use for sequencing.</summary>
        public static SDTweenHandle DelayedCall(float delay, Action callback)
        {
            return To(0.001f, _ => { }).SetDelay(delay).OnCompleteCallback(callback);
        }

        // ── Update loop ──────────────────────────────────────────

        void LateUpdate()
        {
            if (_toAdd.Count > 0)
            {
                _active.AddRange(_toAdd);
                _toAdd.Clear();
            }

            for (var i = _active.Count - 1; i >= 0; i--)
            {
                var h = _active[i];
                if (h.IsComplete) { _active.RemoveAt(i); continue; }
                if (h.IsPaused) continue;

                var dt = h.UseUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

                if (h.Delay > 0f)
                {
                    h.Delay -= dt;
                    continue;
                }

                h.Elapsed += dt;
                var rawT = Mathf.Clamp01(h.Elapsed / h.Duration);
                var t = h.Forward ? rawT : 1f - rawT;
                var eased = Evaluate(h.Ease, t);

                try { h.OnUpdate?.Invoke(eased); }
                catch (MissingReferenceException) { h.Kill(); _active.RemoveAt(i); continue; }

                if (rawT >= 1f)
                {
                    h.LoopsDone++;
                    if (h.LoopCount == -1 || h.LoopsDone < h.LoopCount)
                    {
                        h.Elapsed = 0f;
                        if (h.PingPong) h.Forward = !h.Forward;
                    }
                    else
                    {
                        h.IsComplete = true;
                        try { h.OnComplete?.Invoke(); }
                        catch (Exception e) { Debug.LogException(e); }
                        _active.RemoveAt(i);
                    }
                }
            }
        }

        // ── Easing math ──────────────────────────────────────────

        public static float Evaluate(SDEase ease, float t)
        {
            switch (ease)
            {
                case SDEase.Linear: return t;
                case SDEase.InQuad: return t * t;
                case SDEase.OutQuad: return t * (2f - t);
                case SDEase.InOutQuad: return t < 0.5f ? 2f * t * t : -1f + (4f - 2f * t) * t;
                case SDEase.InCubic: return t * t * t;
                case SDEase.OutCubic: { var u = t - 1f; return u * u * u + 1f; }
                case SDEase.InOutCubic: return t < 0.5f ? 4f * t * t * t : (t - 1f) * (2f * t - 2f) * (2f * t - 2f) + 1f;
                case SDEase.InBack: { const float s = 1.70158f; return t * t * ((s + 1f) * t - s); }
                case SDEase.OutBack: { const float s = 1.70158f; var u = t - 1f; return u * u * ((s + 1f) * u + s) + 1f; }
                case SDEase.InOutBack:
                {
                    const float s = 1.70158f * 1.525f;
                    var u = t * 2f;
                    if (u < 1f) return 0.5f * (u * u * ((s + 1f) * u - s));
                    u -= 2f;
                    return 0.5f * (u * u * ((s + 1f) * u + s) + 2f);
                }
                case SDEase.InElastic:
                {
                    if (t <= 0f) return 0f;
                    if (t >= 1f) return 1f;
                    return -Mathf.Pow(2f, 10f * (t - 1f)) * Mathf.Sin((t - 1.1f) * 5f * Mathf.PI);
                }
                case SDEase.OutElastic:
                {
                    if (t <= 0f) return 0f;
                    if (t >= 1f) return 1f;
                    return Mathf.Pow(2f, -10f * t) * Mathf.Sin((t - 0.1f) * 5f * Mathf.PI) + 1f;
                }
                case SDEase.InBounce: return 1f - EvalOutBounce(1f - t);
                case SDEase.OutBounce: return EvalOutBounce(t);
                default: return t;
            }
        }

        static float EvalOutBounce(float t)
        {
            if (t < 1f / 2.75f) return 7.5625f * t * t;
            if (t < 2f / 2.75f) { t -= 1.5f / 2.75f; return 7.5625f * t * t + 0.75f; }
            if (t < 2.5f / 2.75f) { t -= 2.25f / 2.75f; return 7.5625f * t * t + 0.9375f; }
            t -= 2.625f / 2.75f;
            return 7.5625f * t * t + 0.984375f;
        }
    }
}
