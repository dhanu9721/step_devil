using System;
using UnityEngine;

namespace StepDevil
{
    public enum SpinPrizeType { Coins, Diamonds, Life }

    public struct SpinPrize
    {
        public string Emoji;
        public string Label;
        public int Amount;
        public SpinPrizeType Type;
        public Color32 BgColor;
    }

    /// <summary>
    /// Daily spin wheel — one spin per day.
    /// Prize is seeded by date (same day = same prize across sessions).
    /// </summary>
    public static class StepDevilSpinWheel
    {
        public static readonly SpinPrize[] Prizes =
        {
            new SpinPrize { Emoji = "\U0001FA99", Label = "10 Coins",  Amount = 10,  Type = SpinPrizeType.Coins,    BgColor = new Color32(30,  100, 180, 255) },
            new SpinPrize { Emoji = "\U0001FA99", Label = "25 Coins",  Amount = 25,  Type = SpinPrizeType.Coins,    BgColor = new Color32(30,  130, 80,  255) },
            new SpinPrize { Emoji = "\U0001FA99", Label = "50 Coins",  Amount = 50,  Type = SpinPrizeType.Coins,    BgColor = new Color32(20,  160, 100, 255) },
            new SpinPrize { Emoji = "\U0001FA99", Label = "100 Coins", Amount = 100, Type = SpinPrizeType.Coins,    BgColor = new Color32(40,  180, 120, 255) },
            new SpinPrize { Emoji = "\U0001F48E", Label = "1 Diamond", Amount = 1,   Type = SpinPrizeType.Diamonds, BgColor = new Color32(0,   180, 220, 255) },
            new SpinPrize { Emoji = "\U0001F48E", Label = "2 Diamonds",Amount = 2,   Type = SpinPrizeType.Diamonds, BgColor = new Color32(0,   160, 240, 255) },
            new SpinPrize { Emoji = "\u2764\uFE0F",Label = "1 Life",   Amount = 1,   Type = SpinPrizeType.Life,     BgColor = new Color32(200, 40,  60,  255) },
            new SpinPrize { Emoji = "\U0001F31F", Label = "JACKPOT!",  Amount = 3,   Type = SpinPrizeType.Diamonds, BgColor = new Color32(255, 160, 0,   255) },
        };

        const string KeyDate     = "sd_spin_date";
        const string KeyDone     = "sd_spin_done";
        const string KeyPrizeIdx = "sd_spin_prize_idx";

        static string TodayKey => DateTime.Now.ToString("yyyy-MM-dd");

        public static bool CanSpinToday() =>
            PlayerPrefs.GetString(KeyDate, "") != TodayKey ||
            PlayerPrefs.GetInt(KeyDone, 0) == 0;

        /// <summary>
        /// Picks a random prize, saves it, and returns its index.
        /// Call this when the spin animation begins; the same index is
        /// used by <see cref="ClaimSpin"/> so visual and reward always match.
        /// </summary>
        public static int PickPrize()
        {
            var idx = UnityEngine.Random.Range(0, Prizes.Length);
            PlayerPrefs.SetInt(KeyPrizeIdx, idx);
            PlayerPrefs.Save();
            return idx;
        }

        /// <summary>Returns the prize index chosen by the current spin.</summary>
        public static int TodayPrizeIndex() => PlayerPrefs.GetInt(KeyPrizeIdx, 0);

        /// <summary>Grants the spun prize to the wallet and marks the spin as used. Returns the won prize.</summary>
        public static SpinPrize ClaimSpin()
        {
            var prize = Prizes[TodayPrizeIndex()];

            switch (prize.Type)
            {
                case SpinPrizeType.Coins:    StepDevilWallet.AddCoins(prize.Amount);      break;
                case SpinPrizeType.Diamonds: StepDevilWallet.AddDiamonds(prize.Amount);   break;
                case SpinPrizeType.Life:     StepDevilWallet.AddBonusLives(prize.Amount); break;
            }

            PlayerPrefs.SetString(KeyDate, TodayKey);
            PlayerPrefs.SetInt(KeyDone, 1);
            PlayerPrefs.Save();
            return prize;
        }
    }
}
