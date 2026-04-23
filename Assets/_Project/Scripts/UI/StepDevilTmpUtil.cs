using TMPro;
using UnityEngine;

namespace StepDevil
{
    /// <summary>Maps legacy Text anchors to TMP alignment.</summary>
    public static class StepDevilTmpUtil
    {
        public static TextAlignmentOptions TextAnchorToTmp(TextAnchor a)
        {
            return a switch
            {
                TextAnchor.UpperLeft => TextAlignmentOptions.TopLeft,
                TextAnchor.UpperCenter => TextAlignmentOptions.Top,
                TextAnchor.UpperRight => TextAlignmentOptions.TopRight,
                TextAnchor.MiddleLeft => TextAlignmentOptions.Left,
                TextAnchor.MiddleCenter => TextAlignmentOptions.Center,
                TextAnchor.MiddleRight => TextAlignmentOptions.Right,
                TextAnchor.LowerLeft => TextAlignmentOptions.BottomLeft,
                TextAnchor.LowerCenter => TextAlignmentOptions.Bottom,
                TextAnchor.LowerRight => TextAlignmentOptions.BottomRight,
                _ => TextAlignmentOptions.Center
            };
        }

        public static void ApplyDefaultFont(TextMeshProUGUI tmp, TMP_FontAsset font)
        {
            if (tmp == null)
                return;
            if (font != null)
                tmp.font = font;
            else if (TMP_Settings.defaultFontAsset != null)
                tmp.font = TMP_Settings.defaultFontAsset;
        }

        /// <summary>
        /// Optional: add a TMP font asset at <c>Resources/StepDevil/EmojiFallback SDF</c> (Font Asset Creator, e.g. Segoe UI Emoji)
        /// to restore emoji without replacing UI strings.
        /// </summary>
        public static void TryRegisterEmojiFallback(TMP_FontAsset primary)
        {
            if (primary == null)
                return;
            var fb = Resources.Load<TMP_FontAsset>("StepDevil/EmojiFallback SDF");
            if (fb == null)
                return;
            if (primary.fallbackFontAssetTable != null && !primary.fallbackFontAssetTable.Contains(fb))
                primary.fallbackFontAssetTable.Add(fb);
        }
    }
}
