using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StepDevil
{
    /// <summary>Drag-and-drop wiring for <see cref="StepDevilGame"/> when UI is built in the scene (see Step Devil menu → Create Full UI).</summary>
    [System.Serializable]
    public sealed class StepDevilUiReferences
    {
        public Canvas Canvas;
        public RectTransform Root;
        public Image FlashOverlay;

        public GameObject TitleScreen;
        /// <summary>Title screen devil (e.g. <c>devilEmoji</c> with <see cref="SDSpriteAnimator"/>).</summary>
        public SDSpriteAnimator TitleDevilAnim;
        public Button TitlePlayButton;

        /// <summary>Optional. Title-screen coin label — if set, the code-built bottom action bar skips its own wallet row.</summary>
        public TextMeshProUGUI TitleCoinsText;
        /// <summary>Optional. Title-screen diamond label — if set, the code-built bottom action bar skips its own wallet row.</summary>
        public TextMeshProUGUI TitleDiamondsText;

        public GameObject LevelMapScreen;
        public StepDevilLevelMapView LevelMapView;

        public GameObject WorldScreen;
        public TextMeshProUGUI WorldNum;
        public SDSpriteAnimator WorldIcon;
        public TextMeshProUGUI WorldName;
        public TextMeshProUGUI WorldRule;
        public Button WorldGoButton;

        public GameObject GameScreen;
        /// <summary>Optional. In-game Back button. If set, it's wired to open the Leave popup.</summary>
        public Button GameBackButton;
        public TextMeshProUGUI LivesText;
        public TextMeshProUGUI LevelNum;
        public TextMeshProUGUI CoinsText;
        public TextMeshProUGUI WorldHint;
        public RectTransform PathDotsRoot;
        public SDSpriteAnimator DevilAnim;
        public TextMeshProUGUI MirrorBanner;
        public RectTransform StonesRoot;
        /// <summary>Optional multi-sprite blip (Char/BlipRoot). When set, <see cref="BlipText"/> can be null.</summary>
        public StepDevilBlipController BlipController;
        public TextMeshProUGUI BlipText;
        public TextMeshProUGUI TimerLabel;
        public Image TimerFill;

        public GameObject ResultScreen;
        public SDSpriteAnimator ResultIcon;
        public TextMeshProUGUI ResultTitle;
        public TextMeshProUGUI ResultSub;
        public TextMeshProUGUI ResultLives;
        public Button ResultRetry;

        public GameObject TruthScreen;
        public RectTransform TruthRowsRoot;
        public TextMeshProUGUI TruthSum;
        public ScrollRect TruthScroll;
        public Button TruthContinueButton;

        public GameObject GameOverScreen;
        public TextMeshProUGUI GameOverLevelText;
        public TextMeshProUGUI GameOverCoinsText;
        public Button GameOverPlayAgainButton;

        public GameObject CompleteScreen;
        public TextMeshProUGUI CompleteCoins;
        public TextMeshProUGUI CompleteLies;
        public TextMeshProUGUI CompleteFalls;
        public RectTransform ConfettiRoot;
        public Button CompletePlayAgainButton;

        public bool IsAssigned
        {
            get
            {
                foreach (var _ in EnumerateMissingSlotNames())
                    return false;
                return true;
            }
        }

        /// <summary>Human-readable slot names that are still null (for editor validation and logs).</summary>
        public IEnumerable<string> EnumerateMissingSlotNames()
        {
            if (Canvas == null) yield return nameof(Canvas);
            if (Root == null) yield return nameof(Root);
            if (FlashOverlay == null) yield return nameof(FlashOverlay);
            if (TitleScreen == null) yield return nameof(TitleScreen);
            if (TitlePlayButton == null) yield return nameof(TitlePlayButton);
            if (LevelMapScreen == null) yield return nameof(LevelMapScreen);
            if (LevelMapView == null) yield return nameof(LevelMapView);
            if (WorldScreen == null) yield return nameof(WorldScreen);
            if (WorldNum == null) yield return nameof(WorldNum);
            // WorldIcon is optional (SDSpriteAnimator) — no missing-slot check
            if (WorldName == null) yield return nameof(WorldName);
            if (WorldRule == null) yield return nameof(WorldRule);
            if (WorldGoButton == null) yield return nameof(WorldGoButton);
            if (GameScreen == null) yield return nameof(GameScreen);
            if (LivesText == null) yield return nameof(LivesText);
            if (LevelNum == null) yield return nameof(LevelNum);
            if (CoinsText == null) yield return nameof(CoinsText);
            if (WorldHint == null) yield return nameof(WorldHint);
            if (PathDotsRoot == null) yield return nameof(PathDotsRoot);
            if (BlipText == null && BlipController == null) yield return nameof(BlipText);
            // DevilAnim is optional — no missing-slot check
            // MirrorBanner is optional — if absent, the flip logic still works, the
            // player just won't see the "LEFT <-> RIGHT FLIPPED" warning banner.
            if (StonesRoot == null) yield return nameof(StonesRoot);
            if (TimerLabel == null) yield return nameof(TimerLabel);
            if (TimerFill == null) yield return nameof(TimerFill);
            if (ResultScreen == null) yield return nameof(ResultScreen);
            // ResultIcon is optional (SDSpriteAnimator) — no missing-slot check
            if (ResultTitle == null) yield return nameof(ResultTitle);
            if (ResultSub == null) yield return nameof(ResultSub);
            if (ResultLives == null) yield return nameof(ResultLives);
            if (ResultRetry == null) yield return nameof(ResultRetry);
            if (TruthScreen == null) yield return nameof(TruthScreen);
            if (TruthRowsRoot == null) yield return nameof(TruthRowsRoot);
            if (TruthSum == null) yield return nameof(TruthSum);
            if (TruthScroll == null) yield return nameof(TruthScroll);
            if (TruthContinueButton == null) yield return nameof(TruthContinueButton);
            if (GameOverScreen == null) yield return nameof(GameOverScreen);
            if (GameOverLevelText == null) yield return nameof(GameOverLevelText);
            if (GameOverCoinsText == null) yield return nameof(GameOverCoinsText);
            if (GameOverPlayAgainButton == null) yield return nameof(GameOverPlayAgainButton);
            if (CompleteScreen == null) yield return nameof(CompleteScreen);
            if (CompleteCoins == null) yield return nameof(CompleteCoins);
            if (CompleteLies == null) yield return nameof(CompleteLies);
            if (CompleteFalls == null) yield return nameof(CompleteFalls);
            if (ConfettiRoot == null) yield return nameof(ConfettiRoot);
            if (CompletePlayAgainButton == null) yield return nameof(CompletePlayAgainButton);
        }
    }
}
