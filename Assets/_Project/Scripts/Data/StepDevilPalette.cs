using UnityEngine;

namespace StepDevil
{
    public static class StepDevilPalette
    {
        public static readonly Color32 Bg = new Color32(0x0D, 0x0D, 0x1A, 0xFF);
        public static readonly Color32 Accent = new Color32(0xFF, 0x4D, 0x6D, 0xFF);
        public static readonly Color32 Safe = new Color32(0x06, 0xD6, 0xA0, 0xFF);
        public static readonly Color32 Danger = new Color32(0xEF, 0x23, 0x3C, 0xFF);
        public static readonly Color32 Gold = new Color32(0xFF, 0xD1, 0x66, 0xFF);
        public static readonly Color32 Purple = new Color32(0x9B, 0x5D, 0xE5, 0xFF);
        public static readonly Color32 Grey = new Color32(0x8D, 0x99, 0xAE, 0xFF);
        public static readonly Color32 Blue = new Color32(0x43, 0x61, 0xEE, 0xFF);
        public static readonly Color32 Orange = new Color32(0xFF, 0x9A, 0x3C, 0xFF);
        public static readonly Color32 TextMuted = new Color32(0xAA, 0xAA, 0xAA, 0xFF);
        public static readonly Color32 NeutralDark = new Color32(0x7F, 0x8C, 0x8D, 0xFF);

        public static Color32 StoneAccent(string colorKey)
        {
            if (string.IsNullOrEmpty(colorKey))
                return Grey;
            switch (colorKey.ToLowerInvariant())
            {
                case "green": return Safe;
                case "red": return Danger;
                case "gold": return Orange;
                case "yellow": return Gold;
                case "purple": return Purple;
                case "blue": return Blue;
                case "grey":
                case "gray": return Grey;
                default: return Grey;
            }
        }
    }
}
