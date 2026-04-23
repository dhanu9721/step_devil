using UnityEngine;

namespace StepDevil
{
    /// <summary>Persisted max unlocked level (1-based). Level 1 always playable.</summary>
    public static class StepDevilProgress
    {
        const string Key = "stepdevil_max_level";

        public static int GetMaxUnlockedLevel()
        {
            return Mathf.Clamp(PlayerPrefs.GetInt(Key, 1), 1, StepDevilDatabase.LevelCount);
        }

        public static bool IsLevelUnlocked(int oneBasedLevel)
        {
            return oneBasedLevel >= 1 && oneBasedLevel <= GetMaxUnlockedLevel();
        }

        public static void OnLevelCompleted(int completedOneBasedLevel)
        {
            if (completedOneBasedLevel < 1 || completedOneBasedLevel > StepDevilDatabase.LevelCount)
                return;
            var next = Mathf.Min(completedOneBasedLevel + 1, StepDevilDatabase.LevelCount);
            var cur = GetMaxUnlockedLevel();
            if (next > cur)
                PlayerPrefs.SetInt(Key, next);
            PlayerPrefs.Save();
        }

        public static void ResetProgress()
        {
            PlayerPrefs.DeleteKey(Key);
            PlayerPrefs.Save();
        }
    }
}
