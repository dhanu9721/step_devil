namespace StepDevil
{
    /// <summary>
    /// Optional named clips on each <see cref="SDSpriteAnimator"/> under <see cref="StepDevilBlipController"/>.
    /// If a name has no matching clip, <see cref="SDSpriteAnimator.PlayNamedOrDefault"/> falls back to default frames.
    /// </summary>
    public static class StepDevilBlipAnimationNames
    {
        public const string Idle = "Idle";
        public const string Correct = "Correct";
        public const string Wrong = "Wrong";
        public const string TimeUp = "TimeUp";
    }
}
