using System;

namespace StepDevil
{
    [Serializable]
    public struct StepDevilStoneDef
    {
        public StepDevilStoneType Type;
        public string ColorKey;
        public string Label;
        public string Icon;

        public static StepDevilStoneDef Honest(StepDevilStoneType type)
        {
            return new StepDevilStoneDef
            {
                Type = type,
                ColorKey = TrueColorKey(type),
                Label = TrueLabel(type),
                Icon = TrueIcon(type)
            };
        }

        public static StepDevilStoneDef WithOverrides(StepDevilStoneType type, string colorKey, string label, string icon)
        {
            return new StepDevilStoneDef
            {
                Type = type,
                ColorKey = colorKey ?? TrueColorKey(type),
                Label = label ?? TrueLabel(type),
                Icon = icon ?? TrueIcon(type)
            };
        }

        public static string TrueColorKey(StepDevilStoneType type)
        {
            return type switch
            {
                StepDevilStoneType.Solid => "green",
                StepDevilStoneType.Void => "red",
                StepDevilStoneType.Bonus => "gold",
                StepDevilStoneType.Spring => "yellow",
                StepDevilStoneType.Mirror => "purple",
                _ => "grey"
            };
        }

        public static string TrueLabel(StepDevilStoneType type)
        {
            return type switch
            {
                StepDevilStoneType.Solid => "SAFE",
                StepDevilStoneType.Void => "DANGER",
                StepDevilStoneType.Bonus => "BONUS",
                StepDevilStoneType.Spring => "BOUNCE",
                StepDevilStoneType.Mirror => "MIRROR",
                _ => "?"
            };
        }

        public static string TrueIcon(StepDevilStoneType type)
        {
            return type switch
            {
                StepDevilStoneType.Solid => "\u2713",
                StepDevilStoneType.Void => "\u2717",
                StepDevilStoneType.Bonus => "\u2605",
                StepDevilStoneType.Spring => "\u2191",
                StepDevilStoneType.Mirror => "\u21BB",
                _ => "?"
            };
        }

        public bool ColorLie => !string.Equals(ColorKey, TrueColorKey(Type), StringComparison.OrdinalIgnoreCase);
        public bool LabelLie => !string.Equals(Label, TrueLabel(Type), StringComparison.Ordinal);
        public bool IconLie => !string.Equals(Icon, TrueIcon(Type), StringComparison.Ordinal);
        public int LieCount => (ColorLie ? 1 : 0) + (LabelLie ? 1 : 0) + (IconLie ? 1 : 0);
        public bool IsSafe => Type != StepDevilStoneType.Void;
    }
}
