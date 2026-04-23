using UnityEngine;

namespace StepDevil
{
    /// <summary>Winding path for level nodes (same idea as LiarGame’s map).</summary>
    public static class StepDevilLevelMapLayout
    {
        public static Vector2 GetNodeAnchoredPosition(int indexZeroBased, StepDevilMapStyle s)
        {
            return new Vector2(
                Mathf.Sin(indexZeroBased * s.PathWaveStep) * s.LevelPathAmplitude,
                -indexZeroBased * s.LevelPathSpacing - s.PathVerticalStart);
        }

        public static float GetContentHeight(int levelCount, StepDevilMapStyle s) =>
            levelCount * s.LevelPathSpacing + s.PathContentPadding;
    }

    [System.Serializable]
    public struct StepDevilMapStyle
    {
        public float LevelPathSpacing;
        public float LevelPathAmplitude;
        public float PathWaveStep;
        public float PathVerticalStart;
        public float PathContentPadding;
        public float PathContentWidth;
        public float PathSegmentThickness;
        public Vector2 LevelNodeSize;

        public static StepDevilMapStyle Default => new StepDevilMapStyle
        {
            LevelPathSpacing = 104f,
            LevelPathAmplitude = 72f,
            PathWaveStep = 0.62f,
            PathVerticalStart = 44f,
            PathContentPadding = 200f,
            PathContentWidth = 360f,
            PathSegmentThickness = 6f,
            LevelNodeSize = new Vector2(52f, 52f)
        };
    }
}
