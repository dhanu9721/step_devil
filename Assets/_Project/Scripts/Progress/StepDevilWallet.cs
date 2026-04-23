using System;
using UnityEngine;

namespace StepDevil
{
    /// <summary>
    /// Persistent wallet for coins, diamonds, and lives.
    /// Lives = 3 free daily lives (reset each day) + bonus lives (from rewards/purchases, permanent).
    /// </summary>
    public static class StepDevilWallet
    {
        const string KeyCoins         = "sd_wallet_coins";
        const string KeyDiamonds      = "sd_wallet_diamonds";
        const string KeyBonusLives    = "sd_wallet_bonus_lives";
        const string KeyDailyDate     = "sd_wallet_daily_date";
        const string KeyDailyLives    = "sd_wallet_daily_lives";

        public const int DailyLivesMax         = 3;
        public const int CostCoinsPerLife      = 30;
        public const int CostDiamondsFor3Lives = 2;

        static string TodayKey => DateTime.Now.ToString("yyyy-MM-dd");

        public static int Coins    => PlayerPrefs.GetInt(KeyCoins,    0);
        public static int Diamonds => PlayerPrefs.GetInt(KeyDiamonds, 0);
        public static int BonusLives => PlayerPrefs.GetInt(KeyBonusLives, 0);

        public static int DailyLivesRemaining
        {
            get
            {
                EnsureDailyRefreshed();
                return PlayerPrefs.GetInt(KeyDailyLives, DailyLivesMax);
            }
        }

        public static int TotalLives => DailyLivesRemaining + BonusLives;

        static void EnsureDailyRefreshed()
        {
            if (PlayerPrefs.GetString(KeyDailyDate, "") != TodayKey)
            {
                PlayerPrefs.SetString(KeyDailyDate, TodayKey);
                PlayerPrefs.SetInt(KeyDailyLives, DailyLivesMax);
                PlayerPrefs.Save();
            }
        }

        public static void AddCoins(int amount)
        {
            PlayerPrefs.SetInt(KeyCoins, Mathf.Max(0, Coins + amount));
            PlayerPrefs.Save();
        }

        public static bool SpendCoins(int amount)
        {
            if (Coins < amount) return false;
            PlayerPrefs.SetInt(KeyCoins, Coins - amount);
            PlayerPrefs.Save();
            return true;
        }

        public static void AddDiamonds(int amount)
        {
            PlayerPrefs.SetInt(KeyDiamonds, Mathf.Max(0, Diamonds + amount));
            PlayerPrefs.Save();
        }

        public static bool SpendDiamonds(int amount)
        {
            if (Diamonds < amount) return false;
            PlayerPrefs.SetInt(KeyDiamonds, Diamonds - amount);
            PlayerPrefs.Save();
            return true;
        }

        public static void AddBonusLives(int amount)
        {
            PlayerPrefs.SetInt(KeyBonusLives, BonusLives + amount);
            PlayerPrefs.Save();
        }

        /// <summary>Spends one life (daily first, then bonus). Returns false if none remain.</summary>
        public static bool SpendLife()
        {
            EnsureDailyRefreshed();
            var daily = PlayerPrefs.GetInt(KeyDailyLives, DailyLivesMax);
            if (daily > 0)
            {
                PlayerPrefs.SetInt(KeyDailyLives, daily - 1);
                PlayerPrefs.Save();
                return true;
            }
            var bonus = BonusLives;
            if (bonus > 0)
            {
                PlayerPrefs.SetInt(KeyBonusLives, bonus - 1);
                PlayerPrefs.Save();
                return true;
            }
            return false;
        }

        /// <summary>Buy 1 bonus life for CostCoinsPerLife coins. Returns false if insufficient coins.</summary>
        public static bool BuyLifeWithCoins()
        {
            if (!SpendCoins(CostCoinsPerLife)) return false;
            AddBonusLives(1);
            return true;
        }

        /// <summary>Buy 3 bonus lives for CostDiamondsFor3Lives diamonds. Returns false if insufficient diamonds.</summary>
        public static bool Buy3LivesWithDiamonds()
        {
            if (!SpendDiamonds(CostDiamondsFor3Lives)) return false;
            AddBonusLives(3);
            return true;
        }

        // ── DEBUG / TESTING ONLY — remove before release ──────────────────

        /// <summary>[DEBUG] Resets daily lives back to DailyLivesMax as if it's a new day.</summary>
        public static void DEBUG_RestoreDailyLives()
        {
            PlayerPrefs.SetString(KeyDailyDate, TodayKey);
            PlayerPrefs.SetInt(KeyDailyLives, DailyLivesMax);
            PlayerPrefs.Save();
        }
    }
}
