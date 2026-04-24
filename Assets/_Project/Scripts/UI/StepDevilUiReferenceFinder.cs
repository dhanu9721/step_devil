using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StepDevil
{
    /// <summary>
    /// Fills <see cref="StepDevilUiReferences"/> from a hierarchy that follows <c>Assets/Scenes/SampleScene.unity</c>:
    /// <c>StepDevilGame → StepDevilCanvas → Root</c>, with screen panels as <b>direct children</b> of Root (Title, LevelMap, World, …).
    /// </summary>
    public static class StepDevilUiReferenceFinder
    {
        public static bool TryFind(Transform stepDevilRoot, out StepDevilUiReferences r, out string failureDetail)
        {
            failureDetail = null;
            r = new StepDevilUiReferences();
            var canvasTf = stepDevilRoot.Find("StepDevilCanvas");
            if (canvasTf == null)
            {
                var c = stepDevilRoot.GetComponentInChildren<Canvas>(true);
                if (c == null)
                {
                    failureDetail =
                        "No Canvas under StepDevilGame. Add a child named StepDevilCanvas with a Canvas, or any Canvas under this object.";
                    return false;
                }

                canvasTf = c.transform;
            }

            r.Canvas = canvasTf.GetComponent<Canvas>();
            var root = canvasTf.Find("Root") as RectTransform;
            if (root == null)
            {
                failureDetail =
                    "No RectTransform named 'Root' under the canvas. Expected hierarchy: StepDevilCanvas/Root with screen panels as direct children (Title, LevelMap, …).";
                return false;
            }

            r.Root = root;

            r.FlashOverlay = FindImageUnderRoot(root, "Flash")
                             ?? FindImageUnderRoot(root, "FlashOverlay");

            r.TitleScreen = FindScreenRoot(root, "Title")?.gameObject;
            if (r.TitleScreen != null)
            {
                var titleTf = r.TitleScreen.transform;
                var devilEmoji = FindDeep(titleTf, "devilEmoji");
                r.TitleDevilAnim = devilEmoji != null ? devilEmoji.GetComponent<SDSpriteAnimator>() : null;
                r.TitlePlayButton = FindButtonByNameSubstring(titleTf, "PLAY");

                // Optional scene-authored wallet: look for a TMP named any of these under Title.
                // When found, the injected bottom action bar keeps its row EMPTY so the
                // scene version isn't duplicated (which was reflowing user-authored layout).
                r.TitleCoinsText = FindTmpFirst(titleTf,
                    "TitleCoins", "TitleCoinsText", "CoinsLabel", "TitleCoinsLbl");
                r.TitleDiamondsText = FindTmpFirst(titleTf,
                    "TitleDiamonds", "TitleDiamondsText", "DiamondsLabel",
                    "TitleDiamondsLbl", "TitleGems", "TitleGemsLbl");

                // Optional action-bar buttons. Find Rewards FIRST so the generic "DAILY"
                // fallback for Challenge can't accidentally snap to a Rewards button.
                r.DailyRewardsButton = FindButtonByNameSubstring(titleTf, "DAILYREWARDS")
                                       ?? FindButtonByNameSubstring(titleTf, "REWARDS")
                                       ?? FindButtonByLabelText(titleTf, "DAILY REWARDS", "REWARDS");
                r.DailyChallengeButton = FindButtonByNameSubstring(titleTf, "DAILYCHALLENGE")
                                         ?? FindButtonByNameSubstring(titleTf, "CHALLENGE")
                                         ?? FindButtonByNameSubstring(titleTf, "DAILY")
                                         ?? FindButtonByLabelText(titleTf, "DAILY CHALLENGE", "CHALLENGE")
                                         ?? FindButtonByLabelTextExcluding(titleTf, r.DailyRewardsButton,
                                             "DAILY");
                r.SpinButton = FindButtonByNameSubstring(titleTf, "SPIN")
                               ?? FindButtonByLabelText(titleTf, "SPIN");
                r.StoreButton = FindButtonByNameSubstring(titleTf, "STORE")
                                ?? FindButtonByLabelText(titleTf, "STORE", "SHOP");
                r.SettingsButton = FindButtonByNameSubstring(titleTf, "SETTING")
                                   ?? FindButtonByLabelText(titleTf, "SETTINGS", "SETTING");
                r.NoAdsButton = FindButtonByNameSubstring(titleTf, "NOADS")
                                ?? FindButtonByNameSubstring(titleTf, "NO_ADS")
                                ?? FindButtonByNameSubstring(titleTf, "NOAD")
                                ?? FindButtonByLabelText(titleTf, "NO ADS", "NOADS");
                r.DebugButton = FindButtonByNameSubstring(titleTf, "DEBUG")
                                ?? FindButtonByLabelText(titleTf, "DEBUG", "RESET");
            }

            r.LevelMapScreen = FindScreenRoot(root, "LevelMap")?.gameObject;
            if (r.LevelMapScreen != null)
                r.LevelMapView = r.LevelMapScreen.GetComponent<StepDevilLevelMapView>();

            r.WorldScreen = FindScreenRoot(root, "World")?.gameObject;
            if (r.WorldScreen != null)
            {
                var ws = r.WorldScreen.transform;
                r.WorldNum = FindTmp(ws, "WNum");
                r.WorldIcon = FindDeep<SDSpriteAnimator>(ws, "WIcon");
                r.WorldName = FindTmp(ws, "WName");
                var rule = FindDeep(ws, "Rule");
                r.WorldRule = rule != null ? FindTmp(rule, "RuleText") : null;
                r.WorldGoButton = FindButtonByNameSubstring(ws, "LET");
            }

            r.GameScreen = FindScreenRoot(root, "Game")?.gameObject;
            if (r.GameScreen != null)
            {
                var g = r.GameScreen.transform;
                // Optional scene-authored back button. Tries name substrings in order,
                // then falls back to matching the button's label text (useful when the
                // author named the GameObject "<" or "←" like the code-built version does).
                r.GameBackButton = FindButtonByNameSubstring(g, "BACK")
                                   ?? FindButtonByNameSubstring(g, "LEAVE")
                                   ?? FindButtonByNameSubstring(g, "EXIT")
                                   ?? FindButtonByNameSubstring(g, "RETURN")
                                   ?? FindButtonByNameSubstring(g, "QUIT")
                                   ?? FindButtonByLabelText(g, "<", "←", "BACK", "LEAVE", "EXIT");
                r.LivesText = FindTmpInGameHudRow(g, "Lives", "LivesText");
                r.LevelNum = FindTmp(g, "LvlNum");
                r.CoinsText = FindTmpInGameHudRow(g, "Coins", "CoinsText");
                r.WorldHint = FindTmp(g, "Whint");
                r.PathDotsRoot = FindDeep(g, "DotsRow") as RectTransform;
                var blipRoot = FindDeep(g, "BlipRoot");
                if (blipRoot != null)
                    r.BlipController = blipRoot.GetComponent<StepDevilBlipController>();
                r.BlipText = FindTmp(g, "Blip");
                var devilAnimTf = FindDeep(g, "devilAnim");
                if (devilAnimTf != null)
                    r.DevilAnim = devilAnimTf.GetComponent<SDSpriteAnimator>();
                r.MirrorBanner = FindTmp(g, "Mirror");
                r.StonesRoot = FindDeep(g, "Stones") as RectTransform;
                r.TimerLabel = FindTmp(g, "TLbl");
                var track = FindDeep(g, "Track");
                if (track != null)
                    r.TimerFill = FindDeep<Image>(track, "Fill");
            }

            r.ResultScreen = FindScreenRoot(root, "Result")?.gameObject;
            if (r.ResultScreen != null)
            {
                var rs = r.ResultScreen.transform;
                r.ResultIcon = FindDeep<SDSpriteAnimator>(rs, "RIcon");
                r.ResultTitle = FindTmp(rs, "RTitle");
                r.ResultSub = FindTmp(rs, "RSub");
                r.ResultLives = FindTmp(rs, "RLives");
                r.ResultRetry = FindButtonByNameSubstring(rs, "TRY");
            }

            r.TruthScreen = FindScreenRoot(root, "Truth")?.gameObject;
            if (r.TruthScreen != null)
            {
                var ts = r.TruthScreen.transform;
                r.TruthSum = FindTmp(ts, "Sum");
                var truthScroll = FindDeep(ts, "TruthScroll");
                r.TruthScroll = truthScroll != null ? truthScroll.GetComponent<ScrollRect>() : null;
                var viewport = truthScroll != null ? FindDeep(truthScroll, "Viewport") : null;
                r.TruthRowsRoot = viewport != null ? FindDeep(viewport, "Content") as RectTransform : null;
                r.TruthContinueButton = FindButtonByNameSubstring(ts, "CONTINUE");
            }

            r.GameOverScreen = FindScreenRoot(root, "GameOver")?.gameObject;
            if (r.GameOverScreen != null)
            {
                var gos = r.GameOverScreen.transform;
                r.GameOverLevelText = FindTmp(gos, "GOLvl");
                r.GameOverCoinsText = FindTmp(gos, "GOCoins");
                r.GameOverPlayAgainButton = FindButtonByNameSubstring(gos, "PLAY");
            }

            r.CompleteScreen = FindScreenRoot(root, "Complete")?.gameObject;
            if (r.CompleteScreen != null)
            {
                var cs = r.CompleteScreen.transform;
                r.ConfettiRoot = FindDeep(cs, "Confetti") as RectTransform;
                AssignCompleteStatsFromStatsGrid(cs, ref r);
                r.CompletePlayAgainButton = FindButtonByNameSubstring(cs, "PLAY");
            }

            if (!r.IsAssigned)
            {
                var sb = new StringBuilder();
                sb.Append("Missing or misnamed UI elements under Root (see StepDevilUiReferences / SampleScene layout): ");
                var first = true;
                foreach (var name in r.EnumerateMissingSlotNames())
                {
                    if (!first)
                        sb.Append(", ");
                    first = false;
                    sb.Append(name);
                }

                failureDetail = sb.ToString();
                return false;
            }

            return true;
        }

        static void AssignCompleteStatsFromStatsGrid(Transform completeScreen, ref StepDevilUiReferences r)
        {
            var stats = FindDeep(completeScreen, "Stats");
            if (stats == null)
                return;

            var vals = new List<TextMeshProUGUI>();
            for (var i = 0; i < stats.childCount; i++)
            {
                var cell = stats.GetChild(i);
                var v = FindTmp(cell, "Val");
                if (v != null)
                    vals.Add(v);
            }

            if (vals.Count >= 3)
            {
                r.CompleteCoins = vals[0];
                r.CompleteLies = vals[1];
                r.CompleteFalls = vals[2];
            }
        }

        /// <summary>SampleScene keeps each screen as a direct child of Root — match that first so nested duplicate names do not win.</summary>
        static RectTransform FindScreenRoot(RectTransform root, string screenName)
        {
            for (var i = 0; i < root.childCount; i++)
            {
                var c = root.GetChild(i);
                if (c.name == screenName)
                    return c as RectTransform;
            }

            return FindDeep(root, screenName) as RectTransform;
        }

        static Image FindImageUnderRoot(RectTransform root, string objectName)
        {
            var t = FindDeep(root, objectName);
            return t != null ? t.GetComponent<Image>() : null;
        }

        static TextMeshProUGUI FindTmp(Transform t, string childName)
        {
            var ch = FindDeep(t, childName);
            return ch != null ? ch.GetComponent<TextMeshProUGUI>() : null;
        }

        /// <summary>Tries each object name in order (e.g. SampleScene <c>Lives</c> vs inspector-renamed <c>LivesText</c>).</summary>
        static TextMeshProUGUI FindTmpFirst(Transform t, params string[] objectNames)
        {
            if (t == null || objectNames == null)
                return null;
            foreach (var name in objectNames)
            {
                var tmp = FindTmp(t, name);
                if (tmp != null)
                    return tmp;
            }

            return null;
        }

        /// <summary>
        /// Code-built HUD uses <see cref="StepDevilGame.UiBuild"/> <c>CreateIconLabel</c>: row GameObject is named e.g. <c>Coins</c>,
        /// but <see cref="TextMeshProUGUI"/> lives on child <c>Lbl</c>. SampleScene may put TMP on the row root instead.
        /// </summary>
        static TextMeshProUGUI FindTmpInGameHudRow(Transform gameScreen, params string[] rowObjectNames)
        {
            if (gameScreen == null || rowObjectNames == null)
                return null;
            foreach (var rowName in rowObjectNames)
            {
                if (string.IsNullOrEmpty(rowName))
                    continue;
                var row = FindDeep(gameScreen, rowName);
                if (row == null)
                    continue;
                var onRow = row.GetComponent<TextMeshProUGUI>();
                if (onRow != null)
                    return onRow;
                var lbl = FindDeep(row, "Lbl");
                if (lbl != null)
                {
                    var onLbl = lbl.GetComponent<TextMeshProUGUI>();
                    if (onLbl != null)
                        return onLbl;
                }

                var first = row.GetComponentInChildren<TextMeshProUGUI>(true);
                if (first != null)
                    return first;
            }

            return null;
        }

        /// <summary>First <see cref="Button"/> whose <see cref="Object.name"/> contains <paramref name="substring"/> (case-insensitive). Matches SampleScene labels with emoji/prefixes.</summary>
        static Button FindButtonByNameSubstring(Transform scope, string substring)
        {
            if (scope == null || string.IsNullOrEmpty(substring))
                return null;
            var key = substring.ToUpperInvariant();
            foreach (var b in scope.GetComponentsInChildren<Button>(true))
            {
                if (b.name.ToUpperInvariant().IndexOf(key, System.StringComparison.Ordinal) >= 0)
                    return b;
            }

            return null;
        }

        /// <summary>First <see cref="Button"/> whose TextMeshProUGUI child (label) exactly or
        /// contains one of the given strings (case-insensitive, trimmed). Useful when the
        /// Button GameObject is unnamed / has a generic name but its label gives away intent.</summary>
        static Button FindButtonByLabelText(Transform scope, params string[] labels)
            => FindButtonByLabelTextExcluding(scope, null, labels);

        /// <summary>Same as <see cref="FindButtonByLabelText"/> but skips <paramref name="exclude"/> —
        /// lets a generic fallback keyword ("DAILY") reach the Challenge button without
        /// accidentally binding to the Rewards button that was already matched.</summary>
        static Button FindButtonByLabelTextExcluding(Transform scope, Button exclude, params string[] labels)
        {
            if (scope == null || labels == null || labels.Length == 0)
                return null;
            foreach (var b in scope.GetComponentsInChildren<Button>(true))
            {
                if (exclude != null && b == exclude) continue;
                var tmp = b.GetComponentInChildren<TextMeshProUGUI>(true);
                if (tmp == null) continue;
                var text = (tmp.text ?? "").Trim().ToUpperInvariant();
                if (text.Length == 0) continue;
                foreach (var l in labels)
                {
                    if (string.IsNullOrEmpty(l)) continue;
                    var key = l.Trim().ToUpperInvariant();
                    if (text == key || text.IndexOf(key, System.StringComparison.Ordinal) >= 0)
                        return b;
                }
            }
            return null;
        }

        static Transform FindDeep(Transform t, string name)
        {
            if (t == null)
                return null;
            if (t.name == name)
                return t;
            foreach (Transform c in t)
            {
                var x = FindDeep(c, name);
                if (x != null)
                    return x;
            }

            return null;
        }

        static T FindDeep<T>(Transform t, string childName) where T : Component
        {
            var ch = FindDeep(t, childName);
            return ch != null ? ch.GetComponent<T>() : null;
        }
    }
}
