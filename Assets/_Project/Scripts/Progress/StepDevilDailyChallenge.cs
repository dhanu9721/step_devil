using System;
using UnityEngine;

namespace StepDevil
{
    /// <summary>
    /// Daily challenge system. Picks one random level per day.
    /// Same level won't repeat within the last 7 days.
    /// Progress is separate from the main campaign.
    /// </summary>
    public static class StepDevilDailyChallenge
    {
        const string KeyToday = "sd_daily_date";
        const string KeyLevel = "sd_daily_level";
        const string KeyDone = "sd_daily_done";
        const string KeyHistory = "sd_daily_history"; // comma-separated last 7 level ids
        const string KeyStreak = "sd_daily_streak";
        const string KeyLastStreakDate = "sd_daily_streak_date";

        // for git test purpose

        /// <summary>Today's date string (yyyy-MM-dd) in local time.</summary>
        static string TodayKey => DateTime.Now.ToString("yyyy-MM-dd");

        /// <summary>Has today's challenge already been completed?</summary>
        public static bool IsCompletedToday()
        {
            return PlayerPrefs.GetString(KeyToday, "") == TodayKey
                   && PlayerPrefs.GetInt(KeyDone, 0) == 1;
        }

        /// <summary>Current daily streak count.</summary>
        public static int GetStreak()
        {
            RefreshStreak();
            return PlayerPrefs.GetInt(KeyStreak, 0);
        }

        /// <summary>Get today's challenge level (1-based id). Generates if needed.</summary>
        public static int GetTodayLevelId()
        {
            var saved = PlayerPrefs.GetString(KeyToday, "");
            if (saved == TodayKey)
                return PlayerPrefs.GetInt(KeyLevel, 1);

            // New day — pick a level
            var levelId = PickLevel();
            PlayerPrefs.SetString(KeyToday, TodayKey);
            PlayerPrefs.SetInt(KeyLevel, levelId);
            PlayerPrefs.SetInt(KeyDone, 0);
            PlayerPrefs.Save();
            return levelId;
        }

        /// <summary>Mark today's challenge as completed.</summary>
        public static void MarkCompleted()
        {
            PlayerPrefs.SetInt(KeyDone, 1);

            // Add to history (keep last 7)
            var levelId = PlayerPrefs.GetInt(KeyLevel, 1);
            var hist = GetRecentHistory();
            var newHist = new int[Mathf.Min(hist.Length + 1, 7)];
            newHist[0] = levelId;
            for (int i = 1; i < newHist.Length && i - 1 < hist.Length; i++)
                newHist[i] = hist[i - 1];
            SaveHistory(newHist);

            // Update streak
            UpdateStreakOnComplete();

            PlayerPrefs.Save();
        }

        /// <summary>Get the 0-based index for today's level.</summary>
        public static int GetTodayLevelIndex()
        {
            var id = GetTodayLevelId();
            // Level ids are 1-based, array is 0-based
            return Mathf.Clamp(id - 1, 0, StepDevilDatabase.LevelCount - 1);
        }

        static int PickLevel()
        {
            var recent = GetRecentHistory();
            // Use date as seed so same day = same level across sessions
            var seed = TodayKey.GetHashCode();
            var rng = new System.Random(seed);

            // Try up to 50 times to find a non-recent level
            for (int attempt = 0; attempt < 50; attempt++)
            {
                var candidate = rng.Next(1, StepDevilDatabase.LevelCount + 1);
                if (!IsInRecent(candidate, recent))
                    return candidate;
            }

            // Fallback: just use whatever the RNG gives
            return rng.Next(1, StepDevilDatabase.LevelCount + 1);
        }

        static bool IsInRecent(int levelId, int[] recent)
        {
            for (int i = 0; i < recent.Length; i++)
            {
                if (recent[i] == levelId)
                    return true;
            }
            return false;
        }

        static int[] GetRecentHistory()
        {
            var raw = PlayerPrefs.GetString(KeyHistory, "");
            if (string.IsNullOrEmpty(raw))
                return Array.Empty<int>();

            var parts = raw.Split(',');
            var result = new int[parts.Length];
            int count = 0;
            for (int i = 0; i < parts.Length; i++)
            {
                if (int.TryParse(parts[i], out var val))
                    result[count++] = val;
            }

            if (count < result.Length)
                Array.Resize(ref result, count);
            return result;
        }

        static void SaveHistory(int[] hist)
        {
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < hist.Length; i++)
            {
                if (i > 0) sb.Append(',');
                sb.Append(hist[i]);
            }
            PlayerPrefs.SetString(KeyHistory, sb.ToString());
        }

        static void RefreshStreak()
        {
            var lastDate = PlayerPrefs.GetString(KeyLastStreakDate, "");
            if (string.IsNullOrEmpty(lastDate))
                return;

            // If last streak date is more than 1 day ago (and today not completed), reset
            if (DateTime.TryParse(lastDate, out var last))
            {
                var diff = (DateTime.Now.Date - last.Date).Days;
                if (diff > 1)
                {
                    // Streak broken
                    PlayerPrefs.SetInt(KeyStreak, 0);
                    PlayerPrefs.SetString(KeyLastStreakDate, "");
                    PlayerPrefs.Save();
                }
            }
        }

        static void UpdateStreakOnComplete()
        {
            var lastDate = PlayerPrefs.GetString(KeyLastStreakDate, "");
            var today = DateTime.Now.Date;
            var streak = PlayerPrefs.GetInt(KeyStreak, 0);

            if (DateTime.TryParse(lastDate, out var last))
            {
                var diff = (today - last.Date).Days;
                if (diff == 1)
                    streak++; // consecutive day
                else if (diff > 1)
                    streak = 1; // streak broken, start fresh
                // diff == 0 means already counted today, don't increment
            }
            else
            {
                streak = 1; // first ever
            }

            PlayerPrefs.SetInt(KeyStreak, streak);
            PlayerPrefs.SetString(KeyLastStreakDate, today.ToString("yyyy-MM-dd"));
        }
    }
}
