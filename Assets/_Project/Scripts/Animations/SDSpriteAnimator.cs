using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace StepDevil
{
    /// <summary>Snapshot of <see cref="SDSpriteAnimator"/> inspector data for copying before UI rebuild (code-built path destroys GameObjects).</summary>
    [Serializable]
    public sealed class SDSpriteAnimatorPreset
    {
        public Sprite[] DefaultFrames;
        public SDSpriteAnimator.SpriteAnimation[] Animations;
        public float Fps = 12f;
        public bool Loop = true;
        public bool PingPong;

        public bool HasContent
        {
            get
            {
                if (DefaultFrames != null && DefaultFrames.Length > 0)
                    return true;
                if (Animations == null)
                    return false;
                for (var i = 0; i < Animations.Length; i++)
                {
                    var a = Animations[i];
                    if (a.Frames != null && a.Frames.Length > 0)
                        return true;
                }

                return false;
            }
        }
    }

    // ══════════════════════════════════════════════════════════════
    //  SDSpriteAnimator — plays sprite sequences on UI Image
    //  like a GIF. Supports multiple named animations, looping,
    //  ping-pong, callbacks, and speed control.
    //
    //  Usage:
    //    1. Add to any GameObject with a UI Image component.
    //    2. Assign sprite frames in the Inspector (or via code).
    //    3. Plays automatically on Start, or call Play("name").
    //
    //  For SpriteRenderer (non-UI), attach to a GameObject with
    //  a SpriteRenderer — it auto-detects which one to use.
    // ══════════════════════════════════════════════════════════════

    [RequireComponent(typeof(RectTransform))]
    public sealed class SDSpriteAnimator : MonoBehaviour
    {
        // ── Inspector ────────────────────────────────────────────

        [Header("Default Animation")]
        [Tooltip("Sprites played in order, like GIF frames.")]
        [SerializeField] Sprite[] _frames;

        [Tooltip("Frames per second (e.g. 12 = typical GIF speed).")]
        [SerializeField] float _fps = 12f;

        [Tooltip("Loop forever.")]
        [SerializeField] bool _loop = true;

        [Tooltip("Bounce back and forth (1-2-3-2-1 instead of 1-2-3-1-2-3).")]
        [SerializeField] bool _pingPong;

        [Tooltip("Start playing automatically on Awake.")]
        [SerializeField] bool _playOnAwake = true;

        [Tooltip("Use unscaled time (ignores Time.timeScale).")]
        [SerializeField] bool _useUnscaledTime = true;

        [Header("Multiple Animations (optional)")]
        [SerializeField] SpriteAnimation[] _animations;

        // ── Public properties ────────────────────────────────────

        /// <summary>Current FPS. Change at runtime to speed up / slow down.</summary>
        public float Fps { get => _fps; set => _fps = Mathf.Max(0.1f, value); }

        /// <summary>Is the animator currently playing?</summary>
        public bool IsPlaying => _playing;

        /// <summary>Name of the current animation (empty string for default).</summary>
        public string CurrentAnimation => _currentAnimName;

        /// <summary>Current frame index.</summary>
        public int CurrentFrame => _frameIndex;

        /// <summary>Fired when an animation completes (non-looping) or finishes one full cycle (looping).</summary>
        public event Action<string> OnAnimationComplete;

        /// <summary>Fired every frame change. Passes the frame index.</summary>
        public event Action<int> OnFrameChanged;

        // ── Runtime state ────────────────────────────────────────

        Image _image;
        SpriteRenderer _spriteRenderer;

        Sprite[] _activeFrames;
        float _activeFps;
        bool _activeLoop;
        bool _activePingPong;

        int _frameIndex;
        float _timer;
        bool _playing;
        bool _forward = true;
        string _currentAnimName = "";

        // animation lookup built from _animations array
        Dictionary<string, SpriteAnimation> _animDict;

        // ── Lifecycle ────────────────────────────────────────────

        void Awake()
        {
            _image = GetComponent<Image>();
            _spriteRenderer = GetComponent<SpriteRenderer>();

            BuildAnimDict();

            if (_playOnAwake && _frames != null && _frames.Length > 0)
                Play();
        }

        void Update()
        {
            if (!_playing || _activeFrames == null || _activeFrames.Length == 0)
                return;

            var dt = _useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            _timer += dt;

            var interval = 1f / _activeFps;
            if (_timer < interval)
                return;

            _timer -= interval;
            Advance();
        }

        // ── Public API ───────────────────────────────────────────

        /// <summary>Unity's 1×1 white used by UI fallbacks — do not treat as authored art.</summary>
        static bool IsPlaceholderUiSprite(Sprite s)
        {
            return s != null && ReferenceEquals(s.texture, Texture2D.whiteTexture);
        }

        /// <summary>
        /// Replace the default <see cref="_frames"/> at runtime (e.g. from <see cref="StepDevilGame"/> when UI is code-built).
        /// Rebuilds named-clip lookup; does not start playback unless <paramref name="playNow"/> is true.
        /// </summary>
        public void SetDefaultFrames(Sprite[] frames, bool playNow = false)
        {
            _frames = frames ?? Array.Empty<Sprite>();
            BuildAnimDict();
            StopAndReset();
            if (playNow && _frames.Length > 0)
                Play();
        }

        /// <summary>Copy default frames and optional named clips (for migrating Inspector setup before objects are destroyed).</summary>
        public SDSpriteAnimatorPreset ExportPreset()
        {
            var p = new SDSpriteAnimatorPreset { Fps = _fps, Loop = _loop, PingPong = _pingPong };
            if (_frames != null && _frames.Length > 0)
                p.DefaultFrames = (Sprite[])_frames.Clone();
            if (_animations != null && _animations.Length > 0)
            {
                p.Animations = new SpriteAnimation[_animations.Length];
                for (var i = 0; i < _animations.Length; i++)
                {
                    var a = _animations[i];
                    p.Animations[i] = new SpriteAnimation
                    {
                        Name = a.Name,
                        Frames = a.Frames != null ? (Sprite[])a.Frames.Clone() : null,
                        Fps = a.Fps,
                        Loop = a.Loop,
                        PingPong = a.PingPong
                    };
                }
            }

            if (p.HasContent)
                return p;

            // Dragging onto the UI Image only fills Image.sprite — _frames may stay empty; still copy that for rebuild.
            var img = GetComponent<Image>();
            if (img != null && img.sprite != null && !IsPlaceholderUiSprite(img.sprite))
            {
                p.DefaultFrames = new[] { img.sprite };
                return p;
            }

            var sr = GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != null && !IsPlaceholderUiSprite(sr.sprite))
            {
                p.DefaultFrames = new[] { sr.sprite };
                return p;
            }

            return null;
        }

        /// <summary>Restore frames and named clips from <see cref="ExportPreset"/>.</summary>
        public void ImportPreset(SDSpriteAnimatorPreset p)
        {
            if (p == null || !p.HasContent)
                return;
            _fps = p.Fps;
            _loop = p.Loop;
            _pingPong = p.PingPong;
            _frames = p.DefaultFrames != null && p.DefaultFrames.Length > 0
                ? (Sprite[])p.DefaultFrames.Clone()
                : Array.Empty<Sprite>();
            if (p.Animations != null && p.Animations.Length > 0)
            {
                _animations = new SpriteAnimation[p.Animations.Length];
                for (var i = 0; i < p.Animations.Length; i++)
                {
                    var a = p.Animations[i];
                    _animations[i] = new SpriteAnimation
                    {
                        Name = a.Name,
                        Frames = a.Frames != null ? (Sprite[])a.Frames.Clone() : null,
                        Fps = a.Fps,
                        Loop = a.Loop,
                        PingPong = a.PingPong
                    };
                }
            }
            else
                _animations = null;

            BuildAnimDict();
            StopAndReset();
        }

        /// <summary>Play the default animation (from _frames).</summary>
        public void Play()
        {
            _activeFrames = _frames;
            _activeFps = _fps;
            _activeLoop = _loop;
            _activePingPong = _pingPong;
            _currentAnimName = "";
            StartPlayback();
        }

        /// <summary>
        /// Prefer a named clip from <see cref="_animations"/>; if missing or empty, fall back to the default <see cref="_frames"/> sequence.
        /// </summary>
        public void PlayNamedOrDefault(string animationName)
        {
            BuildAnimDict();
            if (_animDict != null &&
                _animDict.TryGetValue(animationName, out var anim) &&
                anim.Frames != null &&
                anim.Frames.Length > 0)
            {
                Play(animationName);
                return;
            }

            Play();
        }

        /// <summary>
        /// Resolve the <see cref="StepDevilEmojiAnimationNames.Happy"/> clip, or fall back to default <see cref="_frames"/>, for spawning duplicate FX.
        /// </summary>
        public bool TryGetFramesForHappyOrDefault(out Sprite[] frames, out float clipFps, out bool loop, out bool pingPong)
        {
            BuildAnimDict();
            if (_animDict != null &&
                _animDict.TryGetValue(StepDevilEmojiAnimationNames.Happy, out var anim) &&
                anim.Frames != null &&
                anim.Frames.Length > 0)
            {
                frames = anim.Frames;
                clipFps = anim.Fps > 0 ? anim.Fps : _fps;
                loop = anim.Loop;
                pingPong = anim.PingPong;
                return true;
            }

            if (_frames != null && _frames.Length > 0)
            {
                frames = _frames;
                clipFps = _fps;
                loop = _loop;
                pingPong = _pingPong;
                return true;
            }

            frames = null;
            clipFps = _fps;
            loop = true;
            pingPong = false;
            return false;
        }

        /// <summary>Play a named animation from the _animations list.</summary>
        public void Play(string animationName)
        {
            if (_animDict == null) BuildAnimDict();

            if (_animDict != null && _animDict.TryGetValue(animationName, out var anim))
            {
                _activeFrames = anim.Frames;
                _activeFps = anim.Fps > 0 ? anim.Fps : _fps;
                _activeLoop = anim.Loop;
                _activePingPong = anim.PingPong;
                _currentAnimName = animationName;
                StartPlayback();
            }
            else
            {
                Debug.LogWarning($"[SDSpriteAnimator] Animation '{animationName}' not found on {gameObject.name}");
            }
        }

        /// <summary>Play an ad-hoc set of frames (no Inspector setup needed).</summary>
        public void Play(Sprite[] frames, float fps = 12f, bool loop = true, bool pingPong = false)
        {
            _activeFrames = frames;
            _activeFps = fps;
            _activeLoop = loop;
            _activePingPong = pingPong;
            _currentAnimName = "(runtime)";
            StartPlayback();
        }

        /// <summary>Stop playback. Keeps current frame visible.</summary>
        public void Stop()
        {
            _playing = false;
        }

        /// <summary>Stop and reset to frame 0.</summary>
        public void StopAndReset()
        {
            _playing = false;
            _frameIndex = 0;
            _forward = true;
            if (_activeFrames != null && _activeFrames.Length > 0)
                ApplyFrame(0);
        }

        /// <summary>Pause (can Resume later).</summary>
        public void Pause() => _playing = false;

        /// <summary>Resume after Pause.</summary>
        public void Resume() => _playing = true;

        /// <summary>Jump to a specific frame.</summary>
        public void SetFrame(int index)
        {
            if (_activeFrames == null || _activeFrames.Length == 0) return;
            _frameIndex = Mathf.Clamp(index, 0, _activeFrames.Length - 1);
            ApplyFrame(_frameIndex);
        }

        /// <summary>Set a single static sprite (stops animation).</summary>
        public void SetStatic(Sprite sprite)
        {
            _playing = false;
            if (_image) _image.sprite = sprite;
            if (_spriteRenderer) _spriteRenderer.sprite = sprite;
        }

        // ── Internals ────────────────────────────────────────────

        void StartPlayback()
        {
            if (_activeFrames == null || _activeFrames.Length == 0) return;
            _frameIndex = 0;
            _timer = 0f;
            _forward = true;
            _playing = true;
            ApplyFrame(0);
        }

        void Advance()
        {
            var count = _activeFrames.Length;
            if (count <= 1) return;

            if (_activePingPong)
            {
                if (_forward)
                {
                    _frameIndex++;
                    if (_frameIndex >= count - 1)
                    {
                        _frameIndex = count - 1;
                        _forward = false;
                        if (!_activeLoop && _frameIndex == count - 1)
                            CheckNonLoopEnd();
                    }
                }
                else
                {
                    _frameIndex--;
                    if (_frameIndex <= 0)
                    {
                        _frameIndex = 0;
                        _forward = true;
                        OnAnimationComplete?.Invoke(_currentAnimName);
                        if (!_activeLoop)
                        {
                            _playing = false;
                            return;
                        }
                    }
                }
            }
            else
            {
                _frameIndex++;
                if (_frameIndex >= count)
                {
                    OnAnimationComplete?.Invoke(_currentAnimName);
                    if (_activeLoop)
                        _frameIndex = 0;
                    else
                    {
                        _frameIndex = count - 1;
                        _playing = false;
                        return;
                    }
                }
            }

            ApplyFrame(_frameIndex);
        }

        void CheckNonLoopEnd()
        {
            // For ping-pong non-loop: fires at the peak, actual stop happens at frame 0
        }

        void ApplyFrame(int index)
        {
            if (_activeFrames == null || index < 0 || index >= _activeFrames.Length)
                return;

            var sprite = _activeFrames[index];
            if (sprite == null) return;

            if (_image) _image.sprite = sprite;
            if (_spriteRenderer) _spriteRenderer.sprite = sprite;

            OnFrameChanged?.Invoke(index);
        }

        void BuildAnimDict()
        {
            if (_animations == null || _animations.Length == 0) return;
            _animDict = new Dictionary<string, SpriteAnimation>(_animations.Length);
            foreach (var a in _animations)
            {
                if (!string.IsNullOrEmpty(a.Name) && a.Frames != null && a.Frames.Length > 0)
                    _animDict[a.Name] = a;
            }
        }

        // ── Nested types ─────────────────────────────────────────

        [Serializable]
        public sealed class SpriteAnimation
        {
            [Tooltip("Name used to play this animation via Play(\"name\").")]
            public string Name;

            [Tooltip("Sprite frames for this animation.")]
            public Sprite[] Frames;

            [Tooltip("Frames per second. 0 = use the default FPS.")]
            public float Fps;

            public bool Loop = true;
            public bool PingPong;
        }
    }
}
