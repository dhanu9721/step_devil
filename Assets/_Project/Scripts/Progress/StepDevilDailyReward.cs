using System;
using UnityEngine;

namespace StepDevil
{
    public enum DailyRewardType { Coins, Diamonds, Life }
    public enum DayStatus { Claimed, Available, Locked }

    public struct DailyRewardDef
    {
        public string Emoji;
        public string Label;
        public int Amount;
        public DailyRewardType Type;
    }

    /// <summary>
    /// 7-day rotating reward calendar. Claim once per day.
    /// Missing a day resets the streak back to day 1.
    /// After completing all 7 days the cycle restarts.
    /// </summary>
    public static class StepDevilDailyReward
    {
        public static readonly DailyRewardDef[] DayRewards =
        {
            new DailyRewardDef { Emoji = "\U0001FA99", Label = "10 Coins",   Amount = 10, Type = DailyRewardType.Coins    },
            new DailyRewardDef { Emoji = "\U0001FA99", Label = "20 Coins",   Amount = 20, Type = DailyRewardType.Coins    },
            new DailyRewardDef { Emoji = "\U0001F48E", Label = "1 Diamond",  Amount = 1,  Type = DailyRewardType.Diamonds },
            new DailyRewardDef { Emoji = "\u2764\uFE0F", Label = "1 Life",   Amount = 1,  Type = DailyRewardType.Life     },
            new DailyRewardDef { Emoji = "\U0001FA99", Label = "50 Coins",   Amount = 50, Type = DailyRewardType.Coins    },
            new DailyRewardDef { Emoji = "\U0001F48E", Label = "2 Diamonds", Amount = 2,  Type = DailyRewardType.Diamonds },
            new DailyRewardDef { Emoji = "\u2764\uFE0F", Label = "2 Lives",  Amount = 2,  Type = DailyRewardType.Life     },
        };

        const string KeyDay      = "sd_dr_day";       // 0-6: index of the next reward to claim
        const string KeyLastDate = "sd_dr_last_date"; // date string of last successful claim

        static string TodayKey => DateTime.Now.ToString("yyyy-MM-dd");

        /// <summary>0-based index of today's claimable reward.</summary>
        public static int CurrentDayIndex => GetCurrentDayIndex();

        public static bool IsTodayClaimed => PlayerPrefs.GetString(KeyLastDate, "") == TodayKey;

        public static bool CanClaimToday => !IsTodayClaimed;

        public static DailyRewardDef TodayReward => DayRewards[GetCurrentDayIndex()];

        /// <summary>Returns the reward definition and claim status for each of the 7 days in order.</summary>
        public static (DailyRewardDef def, DayStatus status) GetDayInfo(int dayIndex)
        {
            var current = GetCurrentDayIndex();
            DayStatus status;
            if (dayIndex < current)
                status = DayStatus.Claimed;                          // already collected on a previous day
            else if (dayIndex == current && !IsTodayClaimed)
                status = DayStatus.Available;                        // ready to claim right now
            else
                status = DayStatus.Locked;                           // future day (or today already claimed)
            return (DayRewards[dayIndex], status);
        }

        /// <summary>
        /// Claims today's reward, grants it to the wallet, and advances the day counter.
        /// Returns false if already claimed today.
        /// </summary>
        public static bool ClaimToday()
        {
            if (IsTodayClaimed) return false;

            // If streak broken (missed more than 1 day), reset to day 0
            var last = PlayerPrefs.GetString(KeyLastDate, "");
            if (!string.IsNullOrEmpty(last) && DateTime.TryParse(last, out var lastDate))
            {
                if ((DateTime.Now.Date - lastDate.Date).Days > 1)
                    PlayerPrefs.SetInt(KeyDay, 0);
            }

            var day = PlayerPrefs.GetInt(KeyDay, 0) % 7;
            var def = DayRewards[day];

            switch (def.Type)
            {
                case DailyRewardType.Coins:    StepDevilWallet.AddCoins(def.Amount);      break;
                case DailyRewardType.Diamonds: StepDevilWallet.AddDiamonds(def.Amount);   break;
                case DailyRewardType.Life:     StepDevilWallet.AddBonusLives(def.Amount); break;
            }

            PlayerPrefs.SetString(KeyLastDate, TodayKey);
            PlayerPrefs.SetInt(KeyDay, (day + 1) % 7);
            PlayerPrefs.Save();
            return true;
        }

        static int GetCurrentDayIndex()
        {
            // If streak broken, the claimable day resets to 0
            var last = PlayerPrefs.GetString(KeyLastDate, "");
            if (!string.IsNullOrEmpty(last) && DateTime.TryParse(last, out var lastDate))
            {
                if ((DateTime.Now.Date - lastDate.Date).Days > 1)
                    return 0;
            }
            return PlayerPrefs.GetInt(KeyDay, 0) % 7;
        }
    }
}
