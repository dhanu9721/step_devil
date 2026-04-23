using UnityEngine;

namespace StepDevil
{
    /// <summary>
    /// Swaps between separate blip GameObjects (each with <see cref="SDSpriteAnimator"/>) for idle / correct / wrong / time-up.
    /// Assign sprites on each child in the Inspector, or use named animation clips matching <see cref="StepDevilBlipAnimationNames"/>.
    /// </summary>
    public sealed class StepDevilBlipController : MonoBehaviour
    {
        [Header("One active at a time — drag sprites onto each SDSpriteAnimator")]
        [SerializeField] SDSpriteAnimator _idle;
        [SerializeField] SDSpriteAnimator _correct;
        [SerializeField] SDSpriteAnimator _wrong;
        [SerializeField] SDSpriteAnimator _timeUp;

        public RectTransform Root => (RectTransform)transform;

        /// <summary>Used for floating “Happy” FX fallback when gameplay devil sprite is absent.</summary>
        public SDSpriteAnimator CorrectAnimator => _correct;

        public SDSpriteAnimator IdleAnimator => _idle;
        public SDSpriteAnimator WrongAnimator => _wrong;
        public SDSpriteAnimator TimeUpAnimator => _timeUp;

        /// <summary>Runtime wiring (code-built UI).</summary>
        public void Configure(SDSpriteAnimator idle, SDSpriteAnimator correct, SDSpriteAnimator wrong, SDSpriteAnimator timeUp)
        {
            _idle = idle;
            _correct = correct;
            _wrong = wrong;
            _timeUp = timeUp;
        }

        public void ShowIdle()
        {
            if (_idle == null)
                return;
            ActivateOnly(_idle);
            PlayNamed(_idle, StepDevilBlipAnimationNames.Idle);
        }

        public void ShowCorrect()
        {
            if (_correct == null)
            {
                ShowIdle();
                return;
            }

            ActivateOnly(_correct);
            PlayNamed(_correct, StepDevilBlipAnimationNames.Correct);
        }

        public void ShowWrong()
        {
            if (_wrong == null)
            {
                ShowIdle();
                return;
            }

            ActivateOnly(_wrong);
            PlayNamed(_wrong, StepDevilBlipAnimationNames.Wrong);
        }

        public void ShowTimeUp()
        {
            if (_timeUp == null)
            {
                ShowWrong();
                return;
            }

            ActivateOnly(_timeUp);
            PlayNamed(_timeUp, StepDevilBlipAnimationNames.TimeUp);
        }

        void ActivateOnly(SDSpriteAnimator primary)
        {
            if (primary == null)
                return;
            if (_idle != null)
                _idle.gameObject.SetActive(primary == _idle);
            if (_correct != null)
                _correct.gameObject.SetActive(primary == _correct);
            if (_wrong != null)
                _wrong.gameObject.SetActive(primary == _wrong);
            if (_timeUp != null)
                _timeUp.gameObject.SetActive(primary == _timeUp);
        }

        static void PlayNamed(SDSpriteAnimator anim, string clipName)
        {
            if (anim == null)
                return;
            anim.gameObject.SetActive(true);
            anim.PlayNamedOrDefault(clipName);
        }
    }
}
