using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StepDevil
{
    /// <summary>Controls how CreateText sizes itself inside its parent.</summary>
    public enum UiTextMode
    {
        /// <summary>Stretch-to-fill the parent RectTransform.</summary>
        FillParent,
        /// <summary>Attach a LayoutElement with preferredWidth/preferredHeight for use inside a LayoutGroup.</summary>
        LayoutVerticalBlock,
    }

    public sealed partial class StepDevilGame
    {
        // ─────────────────────────────────────────────────────────────────────
        //  Positioning helpers — NO layout groups needed anywhere.
        //  All Y values are measured from the PARENT's TOP edge (positive = down).
        //  Reference canvas = 390 × 844.
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>Top-Centre pivot. y = distance from parent top.</summary>
        static void TC(RectTransform rt, float y, float w, float h, float cx = 0f)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(w, h);
            rt.anchoredPosition = new Vector2(cx, -y);
        }

        /// <summary>Top-Left pivot.</summary>
        static void TL(RectTransform rt, float x, float y, float w, float h)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.sizeDelta = new Vector2(w, h);
            rt.anchoredPosition = new Vector2(x, -y);
        }

        /// <summary>Top-Right pivot. x = distance from right edge.</summary>
        static void TR(RectTransform rt, float x, float y, float w, float h)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.sizeDelta = new Vector2(w, h);
            rt.anchoredPosition = new Vector2(-x, -y);
        }

        /// <summary>Bottom-Centre pivot. y = distance from parent bottom.</summary>
        static void BC(RectTransform rt, float y, float w, float h, float cx = 0f)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.sizeDelta = new Vector2(w, h);
            rt.anchoredPosition = new Vector2(cx, y);
        }

        /// <summary>Middle-Left pivot. x = distance from left edge.</summary>
        static void ML(RectTransform rt, float x, float w, float h)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 0.5f);
            rt.pivot = new Vector2(0f, 0.5f);
            rt.sizeDelta = new Vector2(w, h);
            rt.anchoredPosition = new Vector2(x, 0f);
        }

        /// <summary>Middle-Right pivot. x = distance from right edge.</summary>
        static void MR(RectTransform rt, float x, float w, float h)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(1f, 0.5f);
            rt.pivot = new Vector2(1f, 0.5f);
            rt.sizeDelta = new Vector2(w, h);
            rt.anchoredPosition = new Vector2(-x, 0f);
        }

        /// <summary>Middle-Centre pivot. cx/cy = offsets from parent centre.</summary>
        static void MC(RectTransform rt, float cx, float cy, float w, float h)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(w, h);
            rt.anchoredPosition = new Vector2(cx, cy);
        }

        /// <summary>Full-width strip anchored to parent TOP. y = distance from top, h = height.</summary>
        static void FWT(RectTransform rt, float y, float h)
        {
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(0f, h);
            rt.anchoredPosition = new Vector2(0f, -y);
        }

        /// <summary>Full-width strip anchored to parent BOTTOM. y = distance from bottom, h = height.</summary>
        static void FWB(RectTransform rt, float y, float h)
        {
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.sizeDelta = new Vector2(0f, h);
            rt.anchoredPosition = new Vector2(0f, y);
        }

        /// <summary>Stretch to fill parent with optional insets (l/t/r/b).</summary>
        static void SI(RectTransform rt, float l = 0f, float t = 0f, float r = 0f, float b = 0f)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(l, b);
            rt.offsetMax = new Vector2(-r, -t);
        }

        /// <summary>Row anchored to parent top, stretched horizontally with side margin on each side.
        /// Use this for list items stacked under a scroll content rect.</summary>
        static void RowTopStretch(RectTransform rt, float y, float h, float sideInset)
        {
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot     = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(-sideInset * 2f, h);
            rt.anchoredPosition = new Vector2(0f, -y);
        }

        /// <summary>SI overload for Transform — every UI GameObject's Transform is actually a RectTransform.</summary>
        static void SI(Transform t, float l = 0f, float top = 0f, float r = 0f, float b = 0f)
            => SI((RectTransform)t, l, top, r, b);

        static void StretchFull(RectTransform rt) => SI(rt);

        // ─────────────────────────────────────────────────────────────────────
        //  BuildUi — entry point
        // ─────────────────────────────────────────────────────────────────────

        void BuildUi()
        {
            var go = new GameObject("StepDevilCanvas");
            go.transform.SetParent(transform, false);
            _canvas = go.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(390f, 844f);
            scaler.matchWidthOrHeight = 0.5f;
            go.AddComponent<GraphicRaycaster>();

            _rootRt = MkNode(go.transform, "Root");
            SI(_rootRt);
            var rootBg = _rootRt.gameObject.AddComponent<Image>();
            rootBg.color = StepDevilPalette.Bg;
            rootBg.raycastTarget = false;

            // ── Screens ───────────────────────────────────────────────────────
            var screensGo = MkNode(_rootRt, "Screens");
            SI(screensGo);

            _titleGo        = BuildTitleScreen(screensGo.gameObject.transform);
            _levelMapGo     = BuildLevelMapScreen(screensGo.gameObject.transform);
            _worldGo        = BuildWorldScreen(screensGo.gameObject.transform);
            _gameGo         = BuildGameScreen(screensGo.gameObject.transform);
            _resultGo       = BuildResultScreen(screensGo.gameObject.transform);
            _truthGo        = BuildTruthScreen(screensGo.gameObject.transform);
            _chestGo        = BuildChestScreen(screensGo.gameObject.transform);
            _gameOverGo     = BuildGameOverScreen(screensGo.gameObject.transform);
            _completeGo     = BuildCompleteScreen(screensGo.gameObject.transform);
            _spinWheelGo    = BuildSpinWheelScreen(screensGo.gameObject.transform);
            _dailyRewardsGo = BuildDailyRewardsScreen(screensGo.gameObject.transform);
            _noLivesGo      = BuildNoLivesScreen(screensGo.gameObject.transform);
            _storeGo        = BuildStoreScreen(screensGo.gameObject.transform);

            // ── Popups (always on top) ────────────────────────────────────────
            var popupsGo = MkNode(_rootRt, "Popups");
            SI(popupsGo);

            _settingsPopupGo = BuildSettingsPopup(popupsGo.gameObject.transform);
            _leavePopupGo    = BuildLeavePopup(popupsGo.gameObject.transform);
            _noAdsPopupGo    = BuildNoAdsPopup(popupsGo.gameObject.transform);
            _noLivesPopupGo  = BuildNoLivesPopup(popupsGo.gameObject.transform);
            _settingsPopupGo.SetActive(false);
            _leavePopupGo.SetActive(false);
            _noAdsPopupGo.SetActive(false);
            _noLivesPopupGo.SetActive(false);

            // ── Flash overlay (last = always on top) ─────────────────────────
            _flashOverlay = CreateImage(_rootRt, "Flash", new Color(1f, 1f, 1f, 0f));
            SI(_flashOverlay.rectTransform);
            _flashOverlay.raycastTarget = false;

            // Initial visibility
            _levelMapGo.SetActive(false);
            _worldGo.SetActive(false);
            _gameGo.SetActive(false);
            _resultGo.SetActive(false);
            _truthGo.SetActive(false);
            _chestGo.SetActive(false);
            _gameOverGo.SetActive(false);
            _completeGo.SetActive(false);
            _spinWheelGo.SetActive(false);
            _dailyRewardsGo.SetActive(false);
            _noLivesGo.SetActive(false);
            _storeGo.SetActive(false);
        }

        // ─────────────────────────────────────────────────────────────────────
        //  TITLE SCREEN
        // ─────────────────────────────────────────────────────────────────────

        GameObject BuildTitleScreen(Transform parent)
        {
            var panel = MkPanel(parent, "Title", StepDevilPalette.Bg);

            // Background gradient overlay
            var grad = MkPanel(panel, "Grad", new Color32(26, 10, 46, 45));
            SI(grad);

            // ── Devil emoji / animated icon ───────────────────────────────────
            var devilGo = new GameObject("devilEmoji", typeof(RectTransform));
            devilGo.transform.SetParent(panel, false);
            var devilRt = devilGo.GetComponent<RectTransform>();
            TC(devilRt, 60f, 200f, 88f);
            var devilImg = devilGo.AddComponent<Image>();
            devilImg.color = Color.white;
            devilImg.preserveAspect = true;
            devilImg.raycastTarget = false;
            EnsureUiImageHasSprite(devilImg);
            devilGo.AddComponent<SDSpriteAnimator>();

            // ── Title text ────────────────────────────────────────────────────
            var title = MkTxt(panel, "Title", "STEP DEVIL", 44, StepDevilPalette.Accent,
                TextAnchor.MiddleCenter, bold: true, wrap: false);
            TC(title.rectTransform, 156f, 380f, 54f);

            // ── Tag line ──────────────────────────────────────────────────────
            var tag = MkTxt(panel, "Tag",
                "Every step tells you something.\nEvery step might be <color=#FF4D6D>lying.</color>",
                13, StepDevilPalette.Grey, TextAnchor.MiddleCenter, wrap: true);
            tag.fontStyle = FontStyles.Italic;
            tag.richText = true;
            TC(tag.rectTransform, 218f, 320f, 60f);

            // ── Chips row (3 fixed-size chips, no layout group) ───────────────
            var chipsGo = new GameObject("Chips", typeof(RectTransform));
            chipsGo.transform.SetParent(panel, false);
            var chipsRt = chipsGo.GetComponent<RectTransform>();
            TC(chipsRt, 286f, 360f, 36f);
            // Place chips manually: 3 chips × 110px wide, 8px gap → total = 346px, center offset = -116
            CreateChip(chipsRt, "Colour signals", StepDevilPalette.Safe,   -119f);
            CreateChip(chipsRt, "Label signals",  StepDevilPalette.Accent,    0f);
            CreateChip(chipsRt, "Icon signals",   StepDevilPalette.Gold,    119f);

            // ── How-to text ───────────────────────────────────────────────────
            var howto = MkTxt(panel, "Howto",
                "Tap <b>LEFT</b> or <b>RIGHT</b> stone to step on it\n" +
                "Read the signals — they might be <color=#FF4D6D>LYING</color> to you\n" +
                "3 lives \u00B7 50 levels \u00B7 2 worlds\n" +
                "<color=#888888><size=10>Left / Right arrow keys also work on desktop</size></color>",
                12, StepDevilPalette.TextMuted, TextAnchor.MiddleCenter, wrap: true);
            howto.richText = true;
            TC(howto.rectTransform, 330f, 320f, 96f);

            // ── PLAY button ───────────────────────────────────────────────────
            _titlePlayButton = CreateButton(panel, "PLAY", StepDevilPalette.Accent, OpenLevelMap);
            var playLbl = _titlePlayButton.GetComponentInChildren<TextMeshProUGUI>();
            if (playLbl != null) playLbl.fontSize = 18f;
            TC(_titlePlayButton.GetComponent<RectTransform>(), 434f, 200f, 52f);

            // ── Action bar (anchored to bottom) ───────────────────────────────
            BuildTitleBottomBar(panel);

            return panel.gameObject;
        }

        /// <summary>Icon + number pill for the Title-screen wallet row. Auto-sized
        /// (ContentSizeFitter) so the icon always hugs its label and the pill reads
        /// as a single unit, never a 120px placeholder with the content clumped to one side.</summary>
        void BuildWalletPill(RectTransform parent, string name, Color32 tint,
            out TextMeshProUGUI labelOut, System.Func<string> initialText)
        {
            var pillGo = new GameObject(name, typeof(RectTransform));
            pillGo.transform.SetParent(parent, false);
            var pillRt = pillGo.GetComponent<RectTransform>();
            pillRt.anchorMin = pillRt.anchorMax = new Vector2(0.5f, 0.5f);
            pillRt.pivot     = new Vector2(0.5f, 0.5f);
            pillRt.sizeDelta = new Vector2(0f, 24f); // width driven by ContentSizeFitter

            var pillH = pillGo.AddComponent<HorizontalLayoutGroup>();
            pillH.childAlignment         = TextAnchor.MiddleCenter;
            pillH.spacing                = 6f;
            pillH.padding                = new RectOffset(0, 0, 0, 0);
            pillH.childForceExpandWidth  = false;
            pillH.childForceExpandHeight = false;
            pillH.childControlWidth      = true;
            pillH.childControlHeight     = true;

            var pillFitter = pillGo.AddComponent<ContentSizeFitter>();
            pillFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            pillFitter.verticalFit   = ContentSizeFitter.FitMode.Unconstrained;

            // Icon sits first.
            var icoImg = new GameObject("Ico", typeof(RectTransform)).AddComponent<Image>();
            icoImg.transform.SetParent(pillRt, false);
            icoImg.color = tint;
            icoImg.preserveAspect = true;
            icoImg.raycastTarget = false;
            EnsureUiImageHasSprite(icoImg);
            var icoLe = icoImg.gameObject.AddComponent<LayoutElement>();
            icoLe.preferredWidth = 16f;
            icoLe.preferredHeight = 16f;
            icoLe.minWidth = 16f;

            // Label sits second.
            labelOut = MkTxt(pillRt, "Lbl", initialText?.Invoke() ?? "0", 12,
                tint, TextAnchor.MiddleLeft, bold: true, wrap: false);
            var lblLe = labelOut.gameObject.AddComponent<LayoutElement>();
            lblLe.minWidth = 20f;
            lblLe.preferredHeight = 22f;
        }

        void BuildTitleBottomBar(Transform parent)
        {
            const float barH = 140f;

            var barGo = new GameObject("ActionBar", typeof(RectTransform));
            barGo.transform.SetParent(parent, false);
            var barRt = barGo.GetComponent<RectTransform>();
            FWB(barRt, 0f, barH);
            barGo.AddComponent<Image>().color = new Color32(10, 8, 22, 230);

            // Wallet row (coins + diamonds) — absolute top of bar, centered.
            //
            // Layout: WalletRow (HLG, center-aligned) ──┬── Coins pill  (HLG: icon + text)
            //                                           └── Diamonds pill (HLG: icon + text)
            //
            // Each pill auto-sizes via a ContentSizeFitter so the icon always sits
            // right next to its number, and the two pills sit symmetrically around
            // the centre with a fixed gap between them.
            var walletGo = new GameObject("WalletRow", typeof(RectTransform));
            walletGo.transform.SetParent(barGo.transform, false);
            var walletRt = walletGo.GetComponent<RectTransform>();
            TC(walletRt, 8f, 280f, 26f);
            var walletH = walletGo.AddComponent<HorizontalLayoutGroup>();
            walletH.childAlignment      = TextAnchor.MiddleCenter;
            walletH.spacing             = 36f;   // equal gap between the two pills
            walletH.childForceExpandWidth  = false;
            walletH.childForceExpandHeight = false;
            walletH.childControlWidth      = true;
            walletH.childControlHeight     = true;

            BuildWalletPill(walletRt, "Coins",
                StepDevilPalette.Gold, out _titleCoinsText,
                () => StepDevilWallet.Coins.ToString());
            BuildWalletPill(walletRt, "Diamonds",
                new Color32(100, 220, 255, 255), out _titleDiamondsText,
                () => StepDevilWallet.Diamonds.ToString());

            // Horizontal scroll strip
            var scrollGo = new GameObject("ActionScroll", typeof(RectTransform));
            scrollGo.transform.SetParent(barGo.transform, false);
            var scrollRt = scrollGo.GetComponent<RectTransform>();
            FWB(scrollRt, 0f, 98f);

            var scroll = scrollGo.AddComponent<ScrollRect>();
            scroll.horizontal = true;
            scroll.vertical   = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 40f;
            scroll.inertia = true;
            scroll.decelerationRate = 0.135f;

            var viewportGo = new GameObject("Viewport", typeof(RectTransform));
            viewportGo.transform.SetParent(scrollGo.transform, false);
            SI(viewportGo.GetComponent<RectTransform>());
            viewportGo.AddComponent<Image>().color = Color.white;
            viewportGo.AddComponent<Mask>().showMaskGraphic = false;

            var contentGo = new GameObject("Content", typeof(RectTransform));
            contentGo.transform.SetParent(viewportGo.transform, false);
            var contentRt = contentGo.GetComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0f, 0f);
            contentRt.anchorMax = new Vector2(0f, 1f);
            contentRt.pivot     = new Vector2(0f, 0.5f);
            contentRt.sizeDelta = Vector2.zero;
            contentRt.anchoredPosition = Vector2.zero;
            // Scroll content uses HLG + CSF — correct usage (scroll list)
            var csf = contentGo.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            csf.verticalFit   = ContentSizeFitter.FitMode.Unconstrained;
            var hlay = contentGo.AddComponent<HorizontalLayoutGroup>();
            hlay.childAlignment = TextAnchor.MiddleLeft;
            hlay.spacing = 8f;
            hlay.padding = new RectOffset(14, 14, 4, 4);
            hlay.childForceExpandWidth  = false;
            hlay.childForceExpandHeight = true;

            scroll.viewport = viewportGo.GetComponent<RectTransform>();
            scroll.content  = contentRt;

            // Tiles
            var dailyDone = StepDevilDailyChallenge.IsCompletedToday();
            _dailyChallengeButton = CreateActionTile(contentRt,
                "daily", "DAILY",
                dailyDone ? StepDevilPalette.Grey : StepDevilPalette.Gold,
                OnDailyChallengePressed);
            _dailyChallengeIconImg = _dailyChallengeButton.GetComponentInChildren<Image>();
            _dailyChallengeButton.interactable = !dailyDone;

            var drDone = StepDevilDailyReward.IsTodayClaimed;
            _dailyRewardsButton = CreateActionTile(contentRt, "rewards", "REWARDS",
                drDone ? StepDevilPalette.Grey : new Color32(100, 60, 200, 255), OpenDailyRewards);

            var canSpin = StepDevilSpinWheel.CanSpinToday();
            _spinButton = CreateActionTile(contentRt, "spin", "SPIN",
                canSpin ? StepDevilPalette.Gold : StepDevilPalette.Grey, OpenSpinWheel);

            CreateActionTile(contentRt, "debug", "DEBUG", new Color32(160, 40, 40, 255), () =>
            {
                PlayerPrefs.DeleteAll();
                PlayerPrefs.Save();
                _levelIndex = 0;
                _lives  = StepDevilWallet.TotalLives;
                _coins  = StepDevilWallet.Coins;
                _totalLies = 0;
                _totalFalls = 0;
                _totalLevelsCleared = 0;
                UpdateLives();
                UpdateCoins();
                RefreshTitleWalletBar();
                RefreshDailyButton();
                RefreshSpinButton();
                RefreshDailyRewardsButton();
            });

            CreateActionTile(contentRt, "noads",    "NO ADS",   new Color32(40, 80, 160, 255), OpenNoAdsPopup);
            CreateActionTile(contentRt, "store",    "STORE",    new Color32(40, 130, 80, 255), OpenStore);
            CreateActionTile(contentRt, "settings", "SETTINGS", new Color32(80, 55, 140, 255), OpenSettingsPopup);
        }

        // ─────────────────────────────────────────────────────────────────────
        //  LEVEL MAP SCREEN
        // ─────────────────────────────────────────────────────────────────────

        GameObject BuildLevelMapScreen(Transform parent)
        {
            var panel = MkPanel(parent, "LevelMap", StepDevilPalette.Bg);

            const float headerH = 52f;
            const float topPad  = 8f;

            // Header bar
            var headerGo = new GameObject("MapHeader", typeof(RectTransform));
            headerGo.transform.SetParent(panel, false);
            var header = headerGo.GetComponent<RectTransform>();
            FWT(header, topPad, headerH);
            headerGo.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.04f);

            // Title (full-width, centred)
            var mapTitle = MkTxt(headerGo.transform, "MapTitle", "LEVELS",
                22, Color.white, TextAnchor.MiddleCenter, bold: true, wrap: false);
            SI(mapTitle.rectTransform);

            // Back button (top-left)
            var backBtn = CreateButton(headerGo.transform, "<",
                new Color(1f, 1f, 1f, 0.10f), OnBackFromLevelMap);
            var backRt = backBtn.GetComponent<RectTransform>();
            ML(backRt, 8f, 44f, 44f);
            var backLbl = backBtn.GetComponentInChildren<TextMeshProUGUI>();
            if (backLbl != null) { backLbl.fontSize = 26f; backLbl.color = Color.white; }
            backBtn.transform.SetAsLastSibling();

            // Lives badge (top-right)
            var liveBadgeGo = new GameObject("LivesBadge", typeof(RectTransform));
            liveBadgeGo.transform.SetParent(headerGo.transform, false);
            var liveBadgeRt = liveBadgeGo.GetComponent<RectTransform>();
            MR(liveBadgeRt, 8f, 72f, 36f);
            liveBadgeGo.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.07f);
            // Heart icon (left of badge)
            var heartIco = MkImgSlot(liveBadgeGo.transform, "HeartIco", new Color32(255, 80, 80, 255), 14f, 14f);
            ML(heartIco.rectTransform, 6f, 14f, 14f);
            // Lives count text
            _levelMapLivesText = MkTxt(liveBadgeGo.transform, "LivesVal",
                StepDevilWallet.TotalLives.ToString(), 15, Color.white, TextAnchor.MiddleCenter, bold: true);
            _levelMapLivesText.richText = true;
            _levelMapLivesText.enableWordWrapping = false;
            _levelMapLivesText.overflowMode = TextOverflowModes.Ellipsis;
            SI(_levelMapLivesText.rectTransform, 26f, 0f, 6f, 0f);
            liveBadgeGo.transform.SetAsLastSibling();

            // Scroll area below header
            var scrollHolder = new GameObject("ScrollHolder", typeof(RectTransform));
            scrollHolder.transform.SetParent(panel, false);
            var scrollHolderRt = scrollHolder.GetComponent<RectTransform>();
            SI(scrollHolderRt, 0f, topPad + headerH, 0f, 0f);

            var scrollGo = new GameObject("ScrollView", typeof(RectTransform));
            scrollGo.transform.SetParent(scrollHolder.transform, false);
            var scrollRt = scrollGo.GetComponent<RectTransform>();
            SI(scrollRt);

            var sr = scrollGo.AddComponent<ScrollRect>();
            sr.horizontal = false;
            sr.vertical   = true;
            sr.movementType = ScrollRect.MovementType.Clamped;

            var viewport = CreateImage(scrollGo.transform, "Viewport", new Color(0, 0, 0, 0));
            viewport.raycastTarget = true;
            SI(viewport.rectTransform);
            viewport.maskable = true;
            viewport.gameObject.AddComponent<RectMask2D>();

            var content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(viewport.transform, false);
            var contentRt = content.GetComponent<RectTransform>();
            contentRt.anchorMin = Vector2.zero;
            contentRt.anchorMax = Vector2.one;
            contentRt.offsetMin = Vector2.zero;
            contentRt.offsetMax = Vector2.zero;

            sr.content  = contentRt;
            sr.viewport = viewport.rectTransform;

            _levelMapView = panel.gameObject.AddComponent<StepDevilLevelMapView>();
            _levelMapView.SetMapStyle(StepDevilMapStyle.Default);
            _levelMapView.SetRuntimeRefs(sr, backBtn, mapTitle);
            if (_tmpFont != null)
                _levelMapView.SetOverrideFont(_tmpFont);

            return panel.gameObject;
        }

        // ─────────────────────────────────────────────────────────────────────
        //  WORLD SCREEN
        // ─────────────────────────────────────────────────────────────────────

        GameObject BuildWorldScreen(Transform parent)
        {
            var panel = MkPanel(parent, "World", StepDevilPalette.Bg);
            var rad = MkPanel(panel, "Rad", new Color32(155, 93, 229, 28));
            SI(rad);

            // World number label
            _worldNum = MkTxt(panel, "WNum", "WORLD 1", 12, StepDevilPalette.Grey,
                TextAnchor.MiddleCenter);
            TC(_worldNum.rectTransform, 70f, 334f, 22f);

            // World icon / animation
            _worldIcon = MkAnim(panel, "WIcon", 88f, 88f);
            TC(_worldIcon.GetComponent<RectTransform>(), 100f, 88f, 88f);

            // World name
            _worldName = MkTxt(panel, "WName", "", 30, Color.white,
                TextAnchor.MiddleCenter, bold: true, wrap: false);
            TC(_worldName.rectTransform, 196f, 334f, 44f);

            // Rule card with accent left border
            var ruleGo = new GameObject("Rule", typeof(RectTransform));
            ruleGo.transform.SetParent(panel, false);
            var ruleRt = ruleGo.GetComponent<RectTransform>();
            TC(ruleRt, 248f, 300f, 200f);
            var ruleBg = ruleGo.AddComponent<Image>();
            ruleBg.color = new Color(1f, 1f, 1f, 0.05f);
            ruleBg.raycastTarget = false;
            EnsureUiImageHasSprite(ruleBg);
            // Left accent bar
            var accentGo = new GameObject("AccentBorder", typeof(RectTransform));
            accentGo.transform.SetParent(ruleRt, false);
            var accentRt = accentGo.GetComponent<RectTransform>();
            accentRt.anchorMin = new Vector2(0f, 0f);
            accentRt.anchorMax = new Vector2(0f, 1f);
            accentRt.pivot = new Vector2(0f, 0.5f);
            accentRt.sizeDelta = new Vector2(3f, 0f);
            accentRt.anchoredPosition = Vector2.zero;
            accentGo.AddComponent<Image>().color = StepDevilPalette.Accent;
            // Rule text (inset from borders)
            _worldRule = MkTxt(ruleRt, "RuleText", "", 13,
                new Color32(0xCC, 0xCC, 0xCC, 0xFF), TextAnchor.UpperLeft, wrap: true);
            _worldRule.richText = true;
            _worldRule.overflowMode = TextOverflowModes.Ellipsis;
            SI(_worldRule.rectTransform, 19f, 12f, 16f, 12f);

            // LET'S GO button
            _worldGoBtn = CreateButton(panel, "LET'S GO", StepDevilPalette.Accent, BeginLevel);
            TC(_worldGoBtn.GetComponent<RectTransform>(), 456f, 220f, 48f);

            return panel.gameObject;
        }

        // ─────────────────────────────────────────────────────────────────────
        //  GAME SCREEN
        // ─────────────────────────────────────────────────────────────────────

        GameObject BuildGameScreen(Transform parent)
        {
            var panel = MkPanel(parent, "Game", StepDevilPalette.Bg);

            // ── Timer bar (bottom) ────────────────────────────────────────────
            var timerGo = new GameObject("TimerBar", typeof(RectTransform));
            timerGo.transform.SetParent(panel, false);
            var timerRt = timerGo.GetComponent<RectTransform>();
            FWB(timerRt, 0f, 48f);
            timerGo.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.35f);

            _timerLabel = MkTxt(timerGo.transform, "TLbl", "TAP A STONE", 9,
                new Color(1f, 1f, 1f, 0.45f), TextAnchor.MiddleCenter, wrap: false);
            TC(_timerLabel.rectTransform, 4f, 340f, 14f);

            var trackGo = new GameObject("Track", typeof(RectTransform));
            trackGo.transform.SetParent(timerGo.transform, false);
            var trackRt = trackGo.GetComponent<RectTransform>();
            FWB(trackRt, 4f, 8f);
            var trackImg = trackGo.AddComponent<Image>();
            trackImg.color = new Color(1f, 1f, 1f, 0.12f);
            EnsureUiImageHasSprite(trackImg);

            _timerFill = CreateImage(trackGo.transform, "Fill", StepDevilPalette.Safe);
            SI(_timerFill.rectTransform);
            _timerFill.type = Image.Type.Filled;
            _timerFill.fillMethod = Image.FillMethod.Horizontal;
            _timerFill.fillOrigin = (int)Image.OriginHorizontal.Left;
            _timerFill.fillAmount = 1f;
            EnsureUiImageHasSprite(_timerFill);

            // ── Header (top, fixed height) ────────────────────────────────────
            var headerGo = new GameObject("Header", typeof(RectTransform));
            headerGo.transform.SetParent(panel, false);
            var header = headerGo.GetComponent<RectTransform>();
            FWT(header, 10f, 62f);
            headerGo.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.04f);

            // Back button
            var backBtn = CreateButton(headerGo.transform, "<",
                new Color(1f, 1f, 1f, 0.10f), OnBackFromGame);
            ML(backBtn.GetComponent<RectTransform>(), 6f, 44f, 44f);
            var backLbl = backBtn.GetComponentInChildren<TextMeshProUGUI>();
            if (backLbl != null) { backLbl.fontSize = 26f; backLbl.color = Color.white; }

            // Centre level badge
            var midGo = new GameObject("Mid", typeof(RectTransform));
            midGo.transform.SetParent(headerGo.transform, false);
            SI(midGo.GetComponent<RectTransform>(), 56f, 6f, 110f, 6f);
            midGo.AddComponent<Image>().color = new Color(0.6f, 0.2f, 1f, 0.18f);

            _levelNum = MkTxt(midGo.transform, "LvlNum", "1", 22, Color.white,
                TextAnchor.MiddleCenter, bold: true, wrap: false);
            TC(_levelNum.rectTransform, 4f, 140f, 28f);

            var lvlSub = MkTxt(midGo.transform, "LvlSub", "LEVEL", 8, StepDevilPalette.Grey,
                TextAnchor.MiddleCenter, wrap: false);
            BC(lvlSub.rectTransform, 4f, 140f, 14f);

            // Right panel: coins + lives
            var rightGo = new GameObject("Right", typeof(RectTransform));
            rightGo.transform.SetParent(headerGo.transform, false);
            var rightRt = rightGo.GetComponent<RectTransform>();
            MR(rightRt, 6f, 104f, 50f);

            // Coins row (top of right panel)
            var coinsRowGo = new GameObject("Coins", typeof(RectTransform));
            coinsRowGo.transform.SetParent(rightGo.transform, false);
            TC(coinsRowGo.GetComponent<RectTransform>(), 0f, 104f, 24f);
            var coinsIco = MkImgSlot(coinsRowGo.transform, "Ico", StepDevilPalette.Gold, 14f, 14f);
            ML(coinsIco.rectTransform, 0f, 14f, 14f);
            _coinsText = MkTxt(coinsRowGo.transform, "Lbl", "0", 12, StepDevilPalette.Gold,
                TextAnchor.MiddleLeft, bold: true);
            SI(_coinsText.rectTransform, 18f, 0f, 0f, 0f);

            // Lives row (bottom of right panel)
            var livesRowGo = new GameObject("Lives", typeof(RectTransform));
            livesRowGo.transform.SetParent(rightGo.transform, false);
            BC(livesRowGo.GetComponent<RectTransform>(), 0f, 104f, 24f);
            var heartIco = MkImgSlot(livesRowGo.transform, "Ico", new Color32(255, 80, 80, 255), 14f, 14f);
            ML(heartIco.rectTransform, 0f, 14f, 14f);
            _livesText = MkTxt(livesRowGo.transform, "Lbl", "", 14, Color.white,
                TextAnchor.MiddleLeft, bold: false);
            SI(_livesText.rectTransform, 18f, 0f, 0f, 0f);

            // ── Hint bar ──────────────────────────────────────────────────────
            var hintBarGo = new GameObject("Hint", typeof(RectTransform));
            hintBarGo.transform.SetParent(panel, false);
            FWT(hintBarGo.GetComponent<RectTransform>(), 72f, 26f);
            hintBarGo.AddComponent<Image>().color = new Color(1f, 0.15f, 0.3f, 0.08f);
            _worldHint = MkTxt(hintBarGo.transform, "Whint", "", 9,
                new Color(1f, 1f, 1f, 0.55f), TextAnchor.MiddleCenter, wrap: false);
            _worldHint.richText = true;
            _worldHint.overflowMode = TextOverflowModes.Ellipsis;
            SI(_worldHint.rectTransform, 10f, 0f, 10f, 0f);

            // ── Devil tip bar (hidden by default) ─────────────────────────────
            var tipBarGo = new GameObject("DevilTip", typeof(RectTransform));
            tipBarGo.transform.SetParent(panel, false);
            FWT(tipBarGo.GetComponent<RectTransform>(), 98f, 22f);
            tipBarGo.AddComponent<Image>().color = new Color(0.45f, 0.08f, 0.75f, 0.12f);
            _devilTipText = MkTxt(tipBarGo.transform, "Tip", "", 8,
                new Color32(210, 160, 255, 200), TextAnchor.MiddleCenter, wrap: false);
            _devilTipText.fontStyle = FontStyles.Italic;
            _devilTipText.overflowMode = TextOverflowModes.Ellipsis;
            SI(_devilTipText.rectTransform, 10f, 0f, 10f, 0f);
            tipBarGo.SetActive(false);

            // ── Progress / dots ───────────────────────────────────────────────
            var pathGo = new GameObject("Path", typeof(RectTransform));
            pathGo.transform.SetParent(panel, false);
            FWT(pathGo.GetComponent<RectTransform>(), 120f, 42f);

            var pathLbl = MkTxt(pathGo.transform, "PathLbl", "PROGRESS", 8,
                new Color(1f, 1f, 1f, 0.3f), TextAnchor.MiddleCenter, wrap: false);
            FWT(pathLbl.rectTransform, 0f, 18f);

            var dotsRowGo = new GameObject("DotsRow", typeof(RectTransform));
            dotsRowGo.transform.SetParent(pathGo.transform, false);
            FWB(dotsRowGo.GetComponent<RectTransform>(), 0f, 22f);
            _pathDotsRoot = dotsRowGo.GetComponent<RectTransform>();

            // ── Mirror banner (hidden by default) ─────────────────────────────
            var mirrorGo = new GameObject("Mirror", typeof(RectTransform));
            mirrorGo.transform.SetParent(panel, false);
            FWT(mirrorGo.GetComponent<RectTransform>(), 162f, 22f);
            _mirrorBanner = MkTxt(mirrorGo.transform, "Lbl",
                "! LEFT \u2194 RIGHT FLIPPED!", 10, StepDevilPalette.Purple,
                TextAnchor.MiddleCenter, wrap: false);
            SI(_mirrorBanner.rectTransform);
            _mirrorBanner.gameObject.SetActive(false);

            // ── Blip character ────────────────────────────────────────────────
            var charAreaGo = new GameObject("Char", typeof(RectTransform));
            charAreaGo.transform.SetParent(panel, false);
            TC(charAreaGo.GetComponent<RectTransform>(), 192f, 340f, 80f);

            var blipRootGo = new GameObject("BlipRoot", typeof(RectTransform));
            blipRootGo.transform.SetParent(charAreaGo.transform, false);
            var blipRootRt = blipRootGo.GetComponent<RectTransform>();
            MC(blipRootRt, 0f, 0f, 340f, 76f);

            var idle    = CreateBlipAnimatorSlot(blipRootGo.transform, "Blip_Idle");
            var correct = CreateBlipAnimatorSlot(blipRootGo.transform, "Blip_Correct");
            var wrong   = CreateBlipAnimatorSlot(blipRootGo.transform, "Blip_Wrong");
            var timeUp  = CreateBlipAnimatorSlot(blipRootGo.transform, "Blip_TimeUp");
            correct.gameObject.SetActive(false);
            wrong.gameObject.SetActive(false);
            timeUp.gameObject.SetActive(false);

            var blipCtrl = blipRootGo.AddComponent<StepDevilBlipController>();
            blipCtrl.Configure(idle, correct, wrong, timeUp);
            _blipVisuals = blipCtrl;
            _blipRt = blipCtrl.Root;
            _blipText = null;

            // ── Stones area (fills space between blip and timer bar) ──────────
            // HorizontalLayoutGroup here is required: CreateStone in Round.cs spawns 2–4 stones
            // with LayoutElement(preferredWidth=108), and they need to be laid out side-by-side.
            var stonesGo = new GameObject("Stones", typeof(RectTransform));
            stonesGo.transform.SetParent(panel, false);
            SI(stonesGo.GetComponent<RectTransform>(), 10f, 280f, 10f, 48f);
            var stonesHlg = stonesGo.AddComponent<HorizontalLayoutGroup>();
            stonesHlg.childAlignment = TextAnchor.MiddleCenter;
            stonesHlg.spacing = 12f;
            stonesHlg.padding = new RectOffset(4, 4, 4, 4);
            stonesHlg.childControlWidth      = true;
            stonesHlg.childControlHeight     = true;
            stonesHlg.childForceExpandWidth  = false;
            stonesHlg.childForceExpandHeight = false;
            _stonesRoot = stonesGo.GetComponent<RectTransform>();

            return panel.gameObject;
        }

        // ─────────────────────────────────────────────────────────────────────
        //  RESULT SCREEN
        // ─────────────────────────────────────────────────────────────────────

        GameObject BuildResultScreen(Transform parent)
        {
            var panel = MkPanel(parent, "Result", StepDevilPalette.Bg);
            var rad = MkPanel(panel, "Rad", new Color32(239, 35, 60, 18));
            SI(rad);

            _resultIcon = MkAnim(panel, "RIcon", 96f, 96f);
            TC(_resultIcon.GetComponent<RectTransform>(), 100f, 96f, 96f);

            _resultTitle = MkTxt(panel, "RTitle", "YOU FELL!", 30, Color.white,
                TextAnchor.MiddleCenter, bold: true, wrap: false);
            TC(_resultTitle.rectTransform, 204f, 342f, 44f);

            _resultSub = MkTxt(panel, "RSub", "", 13, StepDevilPalette.Grey,
                TextAnchor.MiddleCenter, wrap: true);
            TC(_resultSub.rectTransform, 254f, 342f, 72f);

            _resultLives = MkTxt(panel, "RLives", "", 24, Color.white,
                TextAnchor.MiddleCenter, wrap: false);
            TC(_resultLives.rectTransform, 332f, 342f, 36f);

            _resultRetry = CreateButton(panel, "TRY AGAIN", StepDevilPalette.Accent, RetryLevel);
            TC(_resultRetry.GetComponent<RectTransform>(), 374f, 220f, 52f);
            var rLbl = _resultRetry.GetComponentInChildren<TextMeshProUGUI>();
            if (rLbl != null) rLbl.fontSize = 16f;

            return panel.gameObject;
        }

        // ─────────────────────────────────────────────────────────────────────
        //  TRUTH SCREEN
        // ─────────────────────────────────────────────────────────────────────

        GameObject BuildTruthScreen(Transform parent)
        {
            var panel = MkPanel(parent, "Truth", StepDevilPalette.Bg);

            // Header: icon + label
            var headGo = new GameObject("Head", typeof(RectTransform));
            headGo.transform.SetParent(panel, false);
            TC(headGo.GetComponent<RectTransform>(), 28f, 362f, 34f);
            var headIco = MkImgSlot(headGo.transform, "Ico", Color.white, 22f, 22f);
            ML(headIco.rectTransform, 0f, 22f, 22f);
            var headLbl = MkTxt(headGo.transform, "Lbl", "THE TRUTH REVEALED",
                20, Color.white, TextAnchor.MiddleCenter, bold: true);
            SI(headLbl.rectTransform);

            var sub = MkTxt(panel, "Sub", "Here's what the devil was hiding...",
                11, StepDevilPalette.Grey, TextAnchor.MiddleCenter, wrap: true);
            TC(sub.rectTransform, 70f, 362f, 40f);

            // Scroll view — content uses VLG + CSF (scroll list — correct usage)
            var scrollGo = new GameObject("TruthScroll", typeof(RectTransform));
            scrollGo.transform.SetParent(panel, false);
            var scrollRt = scrollGo.GetComponent<RectTransform>();
            SI(scrollRt, 0f, 118f, 0f, 108f);

            _truthScroll = scrollGo.AddComponent<ScrollRect>();
            _truthScroll.horizontal = false;
            _truthScroll.movementType = ScrollRect.MovementType.Clamped;

            var viewport = CreateImage(scrollGo.transform, "Viewport", new Color(0, 0, 0, 0));
            viewport.raycastTarget = true;
            SI(viewport.rectTransform);
            viewport.maskable = true;
            viewport.gameObject.AddComponent<RectMask2D>();

            var contentGo = new GameObject("Content", typeof(RectTransform));
            contentGo.transform.SetParent(viewport.transform, false);
            var contentRt = contentGo.GetComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0f, 1f);
            contentRt.anchorMax = new Vector2(1f, 1f);
            contentRt.pivot     = new Vector2(0.5f, 1f);
            contentRt.sizeDelta = Vector2.zero;
            contentRt.anchoredPosition = Vector2.zero;
            var csf = contentGo.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            csf.verticalFit   = ContentSizeFitter.FitMode.PreferredSize;
            var vl = contentGo.AddComponent<VerticalLayoutGroup>();
            vl.spacing = 7f;
            vl.childAlignment = TextAnchor.UpperCenter;
            vl.childForceExpandWidth  = true;
            vl.childForceExpandHeight = false;
            vl.childControlWidth  = true;
            vl.childControlHeight = true;
            _truthScroll.content  = contentRt;
            _truthScroll.viewport = viewport.rectTransform;
            _truthRowsRoot = contentRt;

            // Summary text (above bottom buttons)
            _truthSum = MkTxt(panel, "Sum", "", 12, Color.white,
                TextAnchor.MiddleCenter, wrap: true);
            _truthSum.richText = true;
            BC(_truthSum.rectTransform, 60f, 362f, 44f);

            // CONTINUE button
            _truthContinueButton = CreateButton(panel, "CONTINUE", StepDevilPalette.Safe, AfterTruth);
            BC(_truthContinueButton.GetComponent<RectTransform>(), 10f, 280f, 48f);

            return panel.gameObject;
        }

        // ─────────────────────────────────────────────────────────────────────
        //  GAME OVER SCREEN
        // ─────────────────────────────────────────────────────────────────────

        GameObject BuildGameOverScreen(Transform parent)
        {
            var panel = MkPanel(parent, "GameOver", StepDevilPalette.Bg);
            var rad = MkPanel(panel, "Rad", new Color32(239, 35, 60, 22));
            SI(rad);

            var goIcon = MkAnim(panel, "GOIcon", 96f, 96f);
            TC(goIcon.GetComponent<RectTransform>(), 110f, 96f, 96f);

            var goTxt = MkTxt(panel, "GO", "GAME OVER", 32, StepDevilPalette.Danger,
                TextAnchor.MiddleCenter, bold: true, wrap: false);
            TC(goTxt.rectTransform, 214f, 342f, 48f);

            var goSub = MkTxt(panel, "GOSub", "The devil wins... this time.", 13,
                StepDevilPalette.Grey, TextAnchor.MiddleCenter, wrap: true);
            goSub.richText = true;
            TC(goSub.rectTransform, 268f, 342f, 48f);

            _goLevel = MkTxt(panel, "GOLvl", "", 13, Color.white,
                TextAnchor.MiddleCenter, wrap: true);
            _goLevel.richText = true;
            TC(_goLevel.rectTransform, 322f, 342f, 56f);

            _goCoins = MkTxt(panel, "GOCoins", "", 20, StepDevilPalette.Gold,
                TextAnchor.MiddleCenter, bold: true, wrap: false);
            TC(_goCoins.rectTransform, 384f, 342f, 36f);

            _gameOverPlayAgainButton = CreateButton(panel, "PLAY AGAIN",
                StepDevilPalette.Accent, StartGame);
            TC(_gameOverPlayAgainButton.GetComponent<RectTransform>(), 426f, 200f, 52f);

            return panel.gameObject;
        }

        // ─────────────────────────────────────────────────────────────────────
        //  COMPLETE SCREEN
        // ─────────────────────────────────────────────────────────────────────

        GameObject BuildCompleteScreen(Transform parent)
        {
            var panel = MkPanel(parent, "Complete", StepDevilPalette.Bg);
            var rad = MkPanel(panel, "Rad", new Color32(6, 214, 160, 18));
            SI(rad);

            // Confetti root (particle effects spawned here at runtime)
            var confettiGo = new GameObject("Confetti", typeof(RectTransform));
            confettiGo.transform.SetParent(panel, false);
            TC(confettiGo.GetComponent<RectTransform>(), 50f, 342f, 4f);
            _confettiRoot = confettiGo.GetComponent<RectTransform>();

            var winIcon = MkAnim(panel, "WinIcon", 88f, 88f);
            TC(winIcon.GetComponent<RectTransform>(), 60f, 88f, 88f);

            var winTxt = MkTxt(panel, "Win", "YOU WIN!", 36, StepDevilPalette.Safe,
                TextAnchor.MiddleCenter, bold: true, wrap: false);
            TC(winTxt.rectTransform, 156f, 342f, 50f);

            var winSub = MkTxt(panel, "WinSub",
                "You outsmarted the devil across all 12 levels!", 13,
                StepDevilPalette.Grey, TextAnchor.MiddleCenter, wrap: true);
            TC(winSub.rectTransform, 212f, 342f, 40f);

            // Stats 2×2 grid (absolute, no GridLayoutGroup)
            var statsGo = new GameObject("Stats", typeof(RectTransform));
            statsGo.transform.SetParent(panel, false);
            TC(statsGo.GetComponent<RectTransform>(), 260f, 342f, 148f);

            const float boxW = 161f, boxH = 64f, gapX = 10f, gapY = 10f;
            _completeCoins  = StatBox(statsGo.transform, TL_box(0,     0,    boxW, boxH), "0",  "Devil Coins");
            _completeLies   = StatBox(statsGo.transform, TL_box(boxW+gapX, 0, boxW, boxH), "0",  "Lies Caught");
            _completeFalls  = StatBox(statsGo.transform, TL_box(0,     boxH+gapY, boxW, boxH), "0",  "Falls");
                              StatBox(statsGo.transform, TL_box(boxW+gapX, boxH+gapY, boxW, boxH), "12", "Levels Done");

            _completePlayAgainButton = CreateButton(panel, "PLAY AGAIN",
                StepDevilPalette.Accent, StartGame);
            TC(_completePlayAgainButton.GetComponent<RectTransform>(), 416f, 200f, 52f);
            var cLbl = _completePlayAgainButton.GetComponentInChildren<TextMeshProUGUI>();
            if (cLbl != null) cLbl.fontSize = 18f;

            return panel.gameObject;
        }

        /// <summary>Returns a Rect for TL positioning inside a container.</summary>
        static (float x, float y, float w, float h) TL_box(float x, float y, float w, float h)
            => (x, y, w, h);

        TextMeshProUGUI StatBox(Transform parent, (float x, float y, float w, float h) r,
            string val, string lbl)
        {
            var box = new GameObject("Stat", typeof(RectTransform));
            box.transform.SetParent(parent, false);
            var rt = box.GetComponent<RectTransform>();
            TL(rt, r.x, r.y, r.w, r.h);
            box.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.05f);

            var t = MkTxt(box.transform, "Val", val, 30, Color.white,
                TextAnchor.MiddleCenter, bold: true, wrap: false);
            TC(t.rectTransform, 6f, r.w - 8f, 36f);

            var l = MkTxt(box.transform, "Lbl", lbl, 10, StepDevilPalette.Grey,
                TextAnchor.MiddleCenter, wrap: false);
            BC(l.rectTransform, 6f, r.w - 8f, 18f);

            return t;
        }

        // ─────────────────────────────────────────────────────────────────────
        //  SPIN WHEEL SCREEN
        // ─────────────────────────────────────────────────────────────────────

        GameObject BuildSpinWheelScreen(Transform parent)
        {
            var panel = MkPanel(parent, "SpinWheel", StepDevilPalette.Bg);
            var grad = MkPanel(panel, "Grad", new Color32(50, 30, 0, 80));
            SI(grad);

            // Title
            var spinTitle = new GameObject("Title", typeof(RectTransform));
            spinTitle.transform.SetParent(panel, false);
            TC(spinTitle.GetComponent<RectTransform>(), 60f, 300f, 36f);
            var spinTitleIco = MkImgSlot(spinTitle.transform, "Ico", StepDevilPalette.Gold, 26f, 26f);
            ML(spinTitleIco.rectTransform, 0f, 26f, 26f);
            var spinTitleLbl = MkTxt(spinTitle.transform, "Lbl", "DAILY SPIN",
                22, StepDevilPalette.Gold, TextAnchor.MiddleCenter, bold: true);
            SI(spinTitleLbl.rectTransform);

            // Wallet row
            var walletGo = new GameObject("WalletRow", typeof(RectTransform));
            walletGo.transform.SetParent(panel, false);
            TC(walletGo.GetComponent<RectTransform>(), 104f, 280f, 24f);

            var swCoinsGo = new GameObject("Coins", typeof(RectTransform));
            swCoinsGo.transform.SetParent(walletGo.transform, false);
            MC(swCoinsGo.GetComponent<RectTransform>(), -68f, 0f, 120f, 22f);
            var swCIco = MkImgSlot(swCoinsGo.transform, "Ico", StepDevilPalette.Gold, 16f, 16f);
            ML(swCIco.rectTransform, 0f, 16f, 16f);
            var swCLbl = MkTxt(swCoinsGo.transform, "Lbl",
                StepDevilWallet.Coins.ToString(), 12, StepDevilPalette.Gold,
                TextAnchor.MiddleLeft, bold: true);
            SI(swCLbl.rectTransform, 21f, 0f, 0f, 0f);

            var swGemsGo = new GameObject("Diamonds", typeof(RectTransform));
            swGemsGo.transform.SetParent(walletGo.transform, false);
            MC(swGemsGo.GetComponent<RectTransform>(), 68f, 0f, 120f, 22f);
            var swGIco = MkImgSlot(swGemsGo.transform, "Ico", new Color32(100, 220, 255, 255), 16f, 16f);
            ML(swGIco.rectTransform, 0f, 16f, 16f);
            var swGLbl = MkTxt(swGemsGo.transform, "Lbl",
                StepDevilWallet.Diamonds.ToString(), 12, new Color32(100, 220, 255, 255),
                TextAnchor.MiddleLeft, bold: true);
            SI(swGLbl.rectTransform, 21f, 0f, 0f, 0f);

            // Wheel frame
            var wheelFrameGo = new GameObject("WheelFrame", typeof(RectTransform));
            wheelFrameGo.transform.SetParent(panel, false);
            TC(wheelFrameGo.GetComponent<RectTransform>(), 136f, 290f, 290f);
            wheelFrameGo.AddComponent<Image>().color = new Color32(20, 14, 42, 255);

            // Rotating wheel container
            var wcGo = new GameObject("WheelContainer", typeof(RectTransform));
            wcGo.transform.SetParent(wheelFrameGo.transform, false);
            _spinWheelContainer = wcGo.GetComponent<RectTransform>();
            MC(_spinWheelContainer, 0f, 0f, 290f, 290f);

            const float radius = 100f;
            for (int i = 0; i < StepDevilSpinWheel.Prizes.Length; i++)
            {
                var prize    = StepDevilSpinWheel.Prizes[i];
                var angleDeg = i * 45f;
                var angleRad = angleDeg * Mathf.Deg2Rad;
                var pos = new Vector2(radius * Mathf.Sin(angleRad), radius * Mathf.Cos(angleRad));

                var itemGo = new GameObject($"Prize{i}", typeof(RectTransform));
                itemGo.transform.SetParent(_spinWheelContainer, false);
                var itemRt = itemGo.GetComponent<RectTransform>();
                MC(itemRt, pos.x, pos.y, 74f, 48f);
                itemRt.localRotation = Quaternion.Euler(0f, 0f, -angleDeg);
                var c = prize.BgColor;
                itemGo.AddComponent<Image>().color = new Color(c.r/255f, c.g/255f, c.b/255f, 0.88f);
                // Icon (top of prize tile, absolutely placed)
                var prizeIco = MkImgSlot(itemRt, "E", Color.white, 22f, 22f);
                TC(prizeIco.rectTransform, 2f, 22f, 22f);
                // Label (bottom)
                var prizeLbl = MkTxt(itemRt, "L", prize.Label, 8, Color.white,
                    TextAnchor.MiddleCenter, wrap: false);
                BC(prizeLbl.rectTransform, 2f, 70f, 12f);
            }

            // Center circle
            var centerGo = new GameObject("CenterCircle", typeof(RectTransform));
            centerGo.transform.SetParent(wheelFrameGo.transform, false);
            MC(centerGo.GetComponent<RectTransform>(), 0f, 0f, 72f, 72f);
            centerGo.AddComponent<Image>().color = new Color32(14, 8, 30, 255);
            var ctrImg = MkImgSlot(centerGo.transform, "Ctr", Color.white, 48f, 48f);
            MC(ctrImg.rectTransform, 0f, 0f, 48f, 48f);

            // Pointer
            var ptrGo = new GameObject("Pointer", typeof(RectTransform));
            ptrGo.transform.SetParent(wheelFrameGo.transform, false);
            var ptrRt = ptrGo.GetComponent<RectTransform>();
            ptrRt.anchorMin = ptrRt.anchorMax = new Vector2(0.5f, 1f);
            ptrRt.pivot = new Vector2(0.5f, 1f);
            ptrRt.sizeDelta = new Vector2(24f, 28f);
            ptrRt.anchoredPosition = new Vector2(0f, -2f);
            var ptrImg = MkImgSlot(ptrGo.transform, "Arrow", StepDevilPalette.Gold, 24f, 28f);
            SI(ptrImg.rectTransform);

            // Result text
            _spinResultText = MkTxt(panel, "Result", "Tap SPIN to win!", 13,
                StepDevilPalette.Grey, TextAnchor.MiddleCenter, wrap: true);
            TC(_spinResultText.rectTransform, 434f, 280f, 28f);

            // SPIN button
            var canSpin = StepDevilSpinWheel.CanSpinToday();
            _spinActionButton = CreateButton(panel,
                canSpin ? "SPIN!" : "ALREADY CLAIMED",
                canSpin ? StepDevilPalette.Gold : StepDevilPalette.Grey,
                OnSpinButtonPressed);
            _spinActionButton.interactable = canSpin;
            _spinActionLabel = _spinActionButton.GetComponentInChildren<TextMeshProUGUI>();
            if (_spinActionLabel != null) _spinActionLabel.fontSize = 17f;
            TC(_spinActionButton.GetComponent<RectTransform>(), 468f, 240f, 56f);

            // CLOSE button
            var closeBtn = CreateButton(panel, "CLOSE", StepDevilPalette.Grey, OnCloseSpinWheel);
            var closeLbl = closeBtn.GetComponentInChildren<TextMeshProUGUI>();
            if (closeLbl != null) closeLbl.fontSize = 13f;
            TC(closeBtn.GetComponent<RectTransform>(), 532f, 160f, 38f);

            return panel.gameObject;
        }

        // ─────────────────────────────────────────────────────────────────────
        //  DAILY REWARDS SCREEN
        // ─────────────────────────────────────────────────────────────────────

        GameObject BuildDailyRewardsScreen(Transform parent)
        {
            var panel = MkPanel(parent, "DailyRewards", StepDevilPalette.Bg);
            var grad = MkPanel(panel, "Grad", new Color32(20, 0, 60, 90));
            SI(grad);

            var drTitle = new GameObject("Title", typeof(RectTransform));
            drTitle.transform.SetParent(panel, false);
            TC(drTitle.GetComponent<RectTransform>(), 70f, 300f, 36f);
            var drIco = MkImgSlot(drTitle.transform, "Ico", new Color32(180, 100, 255, 255), 26f, 26f);
            ML(drIco.rectTransform, 0f, 26f, 26f);
            var drLbl = MkTxt(drTitle.transform, "Lbl", "DAILY REWARDS",
                22, new Color32(180, 100, 255, 255), TextAnchor.MiddleCenter, bold: true);
            SI(drLbl.rectTransform);

            var drSub = MkTxt(panel, "Sub",
                "Claim your reward each day — don't miss a streak!", 11,
                StepDevilPalette.Grey, TextAnchor.MiddleCenter, wrap: true);
            TC(drSub.rectTransform, 114f, 300f, 40f);

            var daysGo = new GameObject("DaysRoot", typeof(RectTransform));
            daysGo.transform.SetParent(panel, false);
            _dailyRewardsDaysRoot = daysGo.GetComponent<RectTransform>();
            TC(_dailyRewardsDaysRoot, 162f, 342f, 220f);
            // Without a layout group here, BuildDayRow's two child rows both anchor at
            // the same position and overlap. VLG stacks them vertically, driven by the
            // 100-px LayoutElement.preferredHeight set on each row.
            var daysV = daysGo.AddComponent<VerticalLayoutGroup>();
            daysV.spacing = 12f;
            daysV.childAlignment = TextAnchor.UpperCenter;
            daysV.childForceExpandWidth = true;
            daysV.childForceExpandHeight = false;
            daysV.childControlWidth = true;
            daysV.childControlHeight = false;
            daysV.padding = new RectOffset(0, 0, 4, 4);

            var drClose = CreateButton(panel, "CLOSE", StepDevilPalette.Grey, OnCloseDailyRewards);
            var drCloseLbl = drClose.GetComponentInChildren<TextMeshProUGUI>();
            if (drCloseLbl != null) drCloseLbl.fontSize = 13f;
            TC(drClose.GetComponent<RectTransform>(), 390f, 160f, 42f);

            return panel.gameObject;
        }

        // ─────────────────────────────────────────────────────────────────────
        //  NO LIVES SCREEN
        // ─────────────────────────────────────────────────────────────────────

        GameObject BuildNoLivesScreen(Transform parent)
        {
            var panel = MkPanel(parent, "NoLives", StepDevilPalette.Bg);
            var grad = MkPanel(panel, "Grad", new Color32(239, 35, 60, 22));
            SI(grad);

            var nlIcon = MkAnim(panel, "NLIcon", 96f, 96f);
            TC(nlIcon.GetComponent<RectTransform>(), 70f, 96f, 96f);

            var nlTitle = MkTxt(panel, "Title", "OUT OF LIVES", 30, StepDevilPalette.Danger,
                TextAnchor.MiddleCenter, bold: true, wrap: false);
            TC(nlTitle.rectTransform, 174f, 320f, 48f);

            _noLivesInfoText = MkTxt(panel, "Info", "", 12, StepDevilPalette.Grey,
                TextAnchor.MiddleCenter, wrap: true);
            TC(_noLivesInfoText.rectTransform, 228f, 300f, 44f);

            _noLivesWalletText = MkTxt(panel, "Wallet", "", 14, StepDevilPalette.Gold,
                TextAnchor.MiddleCenter, bold: true, wrap: false);
            TC(_noLivesWalletText.rectTransform, 278f, 300f, 26f);

            var sep = MkTxt(panel, "Sep", "Buy more lives:", 11, StepDevilPalette.TextMuted,
                TextAnchor.MiddleCenter, wrap: false);
            TC(sep.rectTransform, 310f, 300f, 20f);

            _noLivesBuyCoinsBtn = CreateButton(panel,
                $"BUY 1 LIFE  \u2014  {StepDevilWallet.CostCoinsPerLife} coins",
                StepDevilPalette.Gold, OnBuyLifeWithCoins);
            TC(_noLivesBuyCoinsBtn.GetComponent<RectTransform>(), 338f, 340f, 52f);
            var nlCLbl = _noLivesBuyCoinsBtn.GetComponentInChildren<TextMeshProUGUI>();
            if (nlCLbl != null) nlCLbl.fontSize = 15f;

            _noLivesBuyDiamondsBtn = CreateButton(panel,
                $"BUY 3 LIVES  \u2014  {StepDevilWallet.CostDiamondsFor3Lives} gems",
                new Color32(0, 180, 220, 255), OnBuy3LivesWithDiamonds);
            TC(_noLivesBuyDiamondsBtn.GetComponent<RectTransform>(), 398f, 340f, 52f);
            var nlDLbl = _noLivesBuyDiamondsBtn.GetComponentInChildren<TextMeshProUGUI>();
            if (nlDLbl != null) nlDLbl.fontSize = 15f;

            _noLivesRetryBtn = CreateButton(panel, "RETRY LEVEL", StepDevilPalette.Accent,
                OnRetryFromNoLives);
            TC(_noLivesRetryBtn.GetComponent<RectTransform>(), 458f, 340f, 52f);
            var nlRLbl = _noLivesRetryBtn.GetComponentInChildren<TextMeshProUGUI>();
            if (nlRLbl != null) nlRLbl.fontSize = 15f;

            var nlClose = CreateButton(panel, "GO TO MENU", StepDevilPalette.Grey, OnCloseNoLives);
            TC(nlClose.GetComponent<RectTransform>(), 518f, 280f, 42f);
            var nlCl2 = nlClose.GetComponentInChildren<TextMeshProUGUI>();
            if (nlCl2 != null) nlCl2.fontSize = 13f;

            return panel.gameObject;
        }

        // ─────────────────────────────────────────────────────────────────────
        //  CHEST SCREEN
        // ─────────────────────────────────────────────────────────────────────

        GameObject BuildChestScreen(Transform parent)
        {
            var panel = MkPanel(parent, "Chest", StepDevilPalette.Bg);
            var grad = MkPanel(panel, "Grad", new Color32(26, 10, 46, 80));
            SI(grad);

            var hdr = MkTxt(panel, "Header", "REWARD CHEST", 13, StepDevilPalette.Gold,
                TextAnchor.MiddleCenter, bold: true, wrap: false);
            TC(hdr.rectTransform, 80f, 320f, 22f);

            // Chest emoji / animated image
            var emojiGo = new GameObject("ChestEmoji", typeof(RectTransform));
            emojiGo.transform.SetParent(panel, false);
            TC(emojiGo.GetComponent<RectTransform>(), 110f, 120f, 120f);
            _chestEmoji = emojiGo.AddComponent<Image>();
            _chestEmoji.color = Color.white;
            _chestEmoji.preserveAspect = true;
            _chestEmoji.raycastTarget = false;
            EnsureUiImageHasSprite(_chestEmoji);
            emojiGo.AddComponent<SDSpriteAnimator>();

            _chestTitle = MkTxt(panel, "Title", "You earned!", 22, Color.white,
                TextAnchor.MiddleCenter, bold: true, wrap: false);
            TC(_chestTitle.rectTransform, 238f, 320f, 36f);

            // Reward card
            var cardGo = new GameObject("Card", typeof(RectTransform));
            cardGo.transform.SetParent(panel, false);
            TC(cardGo.GetComponent<RectTransform>(), 282f, 280f, 90f);
            cardGo.AddComponent<Image>().color = new Color32(255, 209, 102, 30);

            _chestRewardValue = MkTxt(cardGo.transform, "Value", "+25", 42,
                StepDevilPalette.Gold, TextAnchor.MiddleCenter, bold: true, wrap: false);
            TC(_chestRewardValue.rectTransform, 6f, 256f, 52f);

            _chestRewardLabel = MkTxt(cardGo.transform, "Label", "COINS", 14,
                StepDevilPalette.TextMuted, TextAnchor.MiddleCenter, wrap: false);
            BC(_chestRewardLabel.rectTransform, 6f, 256f, 22f);

            _chestClaimButton = CreateButton(panel, "CLAIM!", StepDevilPalette.Gold, OnChestClaim);
            TC(_chestClaimButton.GetComponent<RectTransform>(), 380f, 220f, 56f);
            var claimLbl = _chestClaimButton.GetComponentInChildren<TextMeshProUGUI>();
            if (claimLbl != null) claimLbl.fontSize = 18f;

            return panel.gameObject;
        }

        // ─────────────────────────────────────────────────────────────────────
        //  STORE SCREEN
        // ─────────────────────────────────────────────────────────────────────

        GameObject BuildStoreScreen(Transform parent)
        {
            var panel = MkPanel(parent, "Store", StepDevilPalette.Bg);

            const float headerH = 56f;
            const float topPad  = 8f;

            // Header
            var headerGo = new GameObject("Header", typeof(RectTransform));
            headerGo.transform.SetParent(panel, false);
            FWT(headerGo.GetComponent<RectTransform>(), topPad, headerH);
            headerGo.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.04f);

            // Title
            var storeTitle = new GameObject("Title", typeof(RectTransform));
            storeTitle.transform.SetParent(headerGo.transform, false);
            SI(storeTitle.GetComponent<RectTransform>());
            var stIco = MkImgSlot(storeTitle.transform, "Ico", Color.white, 22f, 22f);
            MC(stIco.rectTransform, -60f, 0f, 22f, 22f);
            var stLbl = MkTxt(storeTitle.transform, "Lbl", "STORE",
                22, Color.white, TextAnchor.MiddleCenter, bold: true);
            SI(stLbl.rectTransform);

            // Back button
            var stBack = CreateButton(headerGo.transform, "<",
                new Color(1f, 1f, 1f, 0.10f), OnBackFromStore);
            ML(stBack.GetComponent<RectTransform>(), 8f, 44f, 44f);
            var stBackLbl = stBack.GetComponentInChildren<TextMeshProUGUI>();
            if (stBackLbl != null) { stBackLbl.fontSize = 26f; stBackLbl.color = Color.white; }
            stBack.transform.SetAsLastSibling();

            // Scroll area
            var scrollHolder = new GameObject("ScrollHolder", typeof(RectTransform));
            scrollHolder.transform.SetParent(panel, false);
            SI(scrollHolder.GetComponent<RectTransform>(), 0f, topPad + headerH, 0f, 0f);

            var scrollGo = new GameObject("ScrollView", typeof(RectTransform));
            scrollGo.transform.SetParent(scrollHolder.transform, false);
            SI(scrollGo.GetComponent<RectTransform>());

            var sr = scrollGo.AddComponent<ScrollRect>();
            sr.horizontal = false;
            sr.vertical   = true;
            sr.movementType = ScrollRect.MovementType.Elastic;
            sr.elasticity = 0.1f;
            sr.inertia = true;
            sr.decelerationRate = 0.135f;
            sr.scrollSensitivity = 40f;

            var viewportGo = new GameObject("Viewport", typeof(RectTransform));
            viewportGo.transform.SetParent(scrollGo.transform, false);
            SI(viewportGo.GetComponent<RectTransform>());
            viewportGo.AddComponent<Image>().color = Color.white;
            viewportGo.AddComponent<Mask>().showMaskGraphic = false;

            // Content rect — stretches to viewport width; height is set manually below.
            // NO layout components on this or its children. Children use anchored positions.
            var contentGo = new GameObject("Content", typeof(RectTransform));
            contentGo.transform.SetParent(viewportGo.transform, false);
            var contentRt = contentGo.GetComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0f, 1f);
            contentRt.anchorMax = new Vector2(1f, 1f);
            contentRt.pivot     = new Vector2(0.5f, 1f);
            contentRt.anchoredPosition = Vector2.zero;

            sr.viewport = viewportGo.GetComponent<RectTransform>();
            sr.content  = contentRt;

            // Stack items with a manual running y offset (no layout groups).
            const float padTop    = 16f;
            const float padBottom = 32f;
            const float gap       = 8f;
            const float side      = 16f;
            float y = padTop;

            y += BuildStoreSectionHeader(contentRt, "Coins", y, side) + gap;
            y += BuildStoreItem(contentRt, "100 Coins",   "coins_100",  "\u20B910",  y, side) + gap;
            y += BuildStoreItem(contentRt, "500 Coins",   "coins_500",  "\u20B940",  y, side) + gap;
            y += BuildStoreItem(contentRt, "1,200 Coins", "coins_1200", "\u20B980",  y, side) + gap;
            y += BuildStoreItem(contentRt, "3,000 Coins", "coins_3000", "\u20B9180", y, side) + gap;

            y += BuildStoreSectionHeader(contentRt, "Gems", y, side) + gap;
            y += BuildStoreItem(contentRt, "5 Diamonds",  "gems_5",  "\u20B910", y, side) + gap;
            y += BuildStoreItem(contentRt, "20 Diamonds", "gems_20", "\u20B930", y, side) + gap;
            y += BuildStoreItem(contentRt, "60 Diamonds", "gems_60", "\u20B980", y, side) + gap;

            y += BuildStoreSectionHeader(contentRt, "Bundles", y, side) + gap;
            y += BuildStoreBundleItem(contentRt, "Starter Pack",
                "500 Coins + 10 Diamonds + 5 Lives", "bundle_starter", "\u20B950",
                new Color32(255, 200, 0, 255), y, side) + gap;
            y += BuildStoreBundleItem(contentRt, "Pro Pack",
                "2,000 Coins + 50 Diamonds + No Ads", "bundle_pro", "\u20B9150",
                new Color32(180, 100, 255, 255), y, side);

            // Content height = total stacked extent. ScrollRect uses this for scroll range.
            contentRt.sizeDelta = new Vector2(0f, y + padBottom);

            return panel.gameObject;
        }

        float BuildStoreSectionHeader(RectTransform contentRt, string title, float y, float sideInset)
        {
            const float h = 36f;
            var go = new GameObject("Section_" + title, typeof(RectTransform));
            go.transform.SetParent(contentRt, false);
            RowTopStretch(go.GetComponent<RectTransform>(), y, h, sideInset);
            go.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.05f);

            var ico = MkImgSlot(go.transform, "SecIco",
                new Color(0.75f, 0.75f, 0.75f, 1f), 18f, 18f);
            ML(ico.rectTransform, 12f, 18f, 18f);

            var lbl = MkTxt(go.transform, "Lbl", title, 13,
                new Color(0.75f, 0.75f, 0.75f, 1f), TextAnchor.MiddleLeft, bold: true);
            ML(lbl.rectTransform, 36f, 200f, 22f);

            return h;
        }

        float BuildStoreItem(RectTransform contentRt, string name, string itemId,
            string price, float y, float sideInset)
        {
            const float h        = 66f;
            const float iconSize = 44f;
            const float iconLeft = 14f;
            const float textLeft = iconLeft + iconSize + 10f;
            const float buyW     = 72f;
            const float buyRight = 14f;
            const float textRightPad = textLeft + buyW + buyRight;

            var go = new GameObject("Item_" + name, typeof(RectTransform));
            go.transform.SetParent(contentRt, false);
            RowTopStretch(go.GetComponent<RectTransform>(), y, h, sideInset);
            go.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.04f);
            go.AddComponent<Outline>().effectColor = new Color(1f, 1f, 1f, 0.08f);

            var iconImg = MkImgSlot(go.transform, "Icon", Color.white, iconSize, iconSize);
            ML(iconImg.rectTransform, iconLeft, iconSize, iconSize);

            var nLbl = MkTxt(go.transform, "Name", name, 14, Color.white,
                TextAnchor.MiddleLeft, bold: true);
            var nRt = nLbl.rectTransform;
            nRt.anchorMin = new Vector2(0f, 1f);
            nRt.anchorMax = new Vector2(1f, 1f);
            nRt.pivot     = new Vector2(0f, 1f);
            nRt.sizeDelta = new Vector2(-textRightPad, 22f);
            nRt.anchoredPosition = new Vector2(textLeft, -14f);

            var dLbl = MkTxt(go.transform, "Desc", "One-time purchase", 10,
                new Color(0.55f, 0.55f, 0.55f, 1f), TextAnchor.MiddleLeft);
            var dRt = dLbl.rectTransform;
            dRt.anchorMin = new Vector2(0f, 1f);
            dRt.anchorMax = new Vector2(1f, 1f);
            dRt.pivot     = new Vector2(0f, 1f);
            dRt.sizeDelta = new Vector2(-textRightPad, 16f);
            dRt.anchoredPosition = new Vector2(textLeft, -38f);

            var buyBtn = CreateButton(go.transform, price + "\nBUY",
                StepDevilPalette.Safe, () => OnBuyItem(itemId, name, 0));
            MR(buyBtn.GetComponent<RectTransform>(), buyRight, buyW, 46f);
            var bLbl = buyBtn.GetComponentInChildren<TextMeshProUGUI>();
            if (bLbl != null) { bLbl.fontSize = 11f; bLbl.enableWordWrapping = true; }

            return h;
        }

        float BuildStoreBundleItem(RectTransform contentRt, string name, string desc, string itemId,
            string price, Color32 accent, float y, float sideInset)
        {
            const float h         = 78f;
            const float badgeSize = 40f;
            const float badgeLeft = 14f;
            const float textLeft  = badgeLeft + badgeSize + 10f;
            const float buyW      = 72f;
            const float buyRight  = 14f;
            const float textRightPad = textLeft + buyW + buyRight;

            var go = new GameObject("Bundle_" + name, typeof(RectTransform));
            go.transform.SetParent(contentRt, false);
            RowTopStretch(go.GetComponent<RectTransform>(), y, h, sideInset);
            go.AddComponent<Image>().color =
                new Color(accent.r/255f, accent.g/255f, accent.b/255f, 0.12f);
            var ol = go.AddComponent<Outline>();
            ol.effectColor = new Color(accent.r/255f, accent.g/255f, accent.b/255f, 0.5f);
            ol.effectDistance = new Vector2(2f, 2f);

            var badge = MkImgSlot(go.transform, "Badge", accent, badgeSize, badgeSize);
            ML(badge.rectTransform, badgeLeft, badgeSize, badgeSize);

            var cnLbl = MkTxt(go.transform, "Name", name, 15, accent,
                TextAnchor.MiddleLeft, bold: true);
            var nRt = cnLbl.rectTransform;
            nRt.anchorMin = new Vector2(0f, 1f);
            nRt.anchorMax = new Vector2(1f, 1f);
            nRt.pivot     = new Vector2(0f, 1f);
            nRt.sizeDelta = new Vector2(-textRightPad, 24f);
            nRt.anchoredPosition = new Vector2(textLeft, -14f);

            var cdLbl = MkTxt(go.transform, "Desc", desc, 10,
                new Color(0.7f, 0.7f, 0.7f, 1f), TextAnchor.MiddleLeft, wrap: true);
            var dRt = cdLbl.rectTransform;
            dRt.anchorMin = new Vector2(0f, 1f);
            dRt.anchorMax = new Vector2(1f, 1f);
            dRt.pivot     = new Vector2(0f, 1f);
            dRt.sizeDelta = new Vector2(-textRightPad, 30f);
            dRt.anchoredPosition = new Vector2(textLeft, -40f);

            var buyBtn = CreateButton(go.transform, price + "\nBUY",
                new Color(accent.r/255f, accent.g/255f, accent.b/255f, 0.8f),
                () => OnBuyItem(itemId, name, 0));
            MR(buyBtn.GetComponent<RectTransform>(), buyRight, buyW, 52f);
            var bLbl = buyBtn.GetComponentInChildren<TextMeshProUGUI>();
            if (bLbl != null) { bLbl.fontSize = 11f; bLbl.enableWordWrapping = true; }

            return h;
        }

        // ─────────────────────────────────────────────────────────────────────
        //  POPUPS
        // ─────────────────────────────────────────────────────────────────────

        GameObject BuildNoLivesPopup(Transform parent)
        {
            var popupGo = MkPopupBackdrop(parent, "NoLivesPopup", new Color32(255, 90, 100, 140));
            var card = MkPopupCard(popupGo.transform, 320f, 370f,
                new Color32(18, 10, 36, 252), new Color(0.9f, 0.1f, 0.2f, 0.55f));

            // Header row
            var hdrRt = PopupHeaderRow(card, "OUT OF LIVES",
                new Color32(255, 90, 100, 255), CloseNoLivesPopup, out _nlpWalletText);
            // (wallet text is repurposed as the header label — override)
            // Actually we need separate wallet text, so create proper header:

            // Re-do: header container (absolute inside card)
            card.gameObject.GetComponent<RectTransform>(); // already have card RT via MkPopupCard
            var cardRt = card;

            FWT(hdrRt, 18f, 36f); // re-position header

            // Divider
            PopupDivider(cardRt, 54f);

            // Sub text
            var subTxt = MkTxt(cardRt, "Sub", "You need lives to play.", 12,
                StepDevilPalette.TextMuted, TextAnchor.MiddleCenter);
            TC(subTxt.rectTransform, 63f, 280f, 20f);

            // Wallet
            _nlpWalletText = MkTxt(cardRt, "Wallet", "", 15, StepDevilPalette.Gold,
                TextAnchor.MiddleCenter, bold: true);
            TC(_nlpWalletText.rectTransform, 91f, 280f, 26f);

            // BUY 1 LIFE
            _nlpBuyCoinsBtn = CreateButton(cardRt,
                $"BUY 1 LIFE  \u2014  {StepDevilWallet.CostCoinsPerLife} coins",
                StepDevilPalette.Gold, OnNoLivesBuyCoins);
            TC(_nlpBuyCoinsBtn.GetComponent<RectTransform>(), 133f, 280f, 52f);
            var nlcLbl = _nlpBuyCoinsBtn.GetComponentInChildren<TextMeshProUGUI>();
            if (nlcLbl != null) nlcLbl.fontSize = 14f;

            // BUY 3 LIVES
            _nlpBuyDiamondsBtn = CreateButton(cardRt,
                $"BUY 3 LIVES  \u2014  {StepDevilWallet.CostDiamondsFor3Lives} gems",
                new Color32(0, 180, 220, 255), OnNoLivesBuyDiamonds);
            TC(_nlpBuyDiamondsBtn.GetComponent<RectTransform>(), 193f, 280f, 52f);
            var nldLbl = _nlpBuyDiamondsBtn.GetComponentInChildren<TextMeshProUGUI>();
            if (nldLbl != null) nldLbl.fontSize = 14f;

            // WATCH AD
            var adsBtn = CreateButton(cardRt,
                "WATCH AD  \u2014  +50 coins",
                new Color32(60, 100, 180, 255), OnNoLivesWatchAds);
            TC(adsBtn.GetComponent<RectTransform>(), 253f, 280f, 48f);
            var adsLbl = adsBtn.GetComponentInChildren<TextMeshProUGUI>();
            if (adsLbl != null) adsLbl.fontSize = 13f;

            // Note
            var noteTxt = MkTxt(cardRt, "Note",
                "After watching, convert coins\u2009\u2192\u2009life above.", 11,
                StepDevilPalette.TextMuted, TextAnchor.MiddleCenter, wrap: true);
            TC(noteTxt.rectTransform, 309f, 280f, 36f);

            return popupGo;
        }

        GameObject BuildSettingsPopup(Transform parent)
        {
            var popupGo = MkPopupBackdrop(parent, "SettingsPopup", new Color32(0, 0, 0, 166));
            var card = MkPopupCard(popupGo.transform, 300f, 260f,
                new Color32(18, 12, 40, 252), new Color(0.55f, 0.2f, 1f, 0.55f));

            var cardRt = card;

            // Header
            var hdrRt = PopupHeaderRow(card, "SETTINGS",
                Color.white, CloseSettingsPopup, out _);
            FWT(hdrRt, 18f, 36f);

            PopupDivider(cardRt, 54f);

            // Toggle rows
            _soundToggleLbl = PopupToggleRow(cardRt, "Sound", 63f, ToggleSound);
            _musicToggleLbl = PopupToggleRow(cardRt, "Music", 117f, ToggleMusic);

            // Quit button
            var quitBtn = CreateButton(cardRt, "QUIT GAME",
                new Color(0.75f, 0.1f, 0.15f, 0.88f), QuitGame);
            TC(quitBtn.GetComponent<RectTransform>(), 181f, 260f, 46f);
            var qLbl = quitBtn.GetComponentInChildren<TextMeshProUGUI>();
            if (qLbl != null) { qLbl.fontSize = 14f; qLbl.color = Color.white; }

            return popupGo;
        }

        GameObject BuildLeavePopup(Transform parent)
        {
            var popupGo = MkPopupBackdrop(parent, "LeavePopup", new Color32(0, 0, 0, 184));
            var card = MkPopupCard(popupGo.transform, 300f, 210f,
                new Color32(18, 12, 40, 252), new Color(1f, 0.25f, 0.3f, 0.55f));

            var cardRt = card;

            // Title icon+label (centered)
            var titleGo = new GameObject("Title", typeof(RectTransform));
            titleGo.transform.SetParent(cardRt, false);
            TC(titleGo.GetComponent<RectTransform>(), 22f, 252f, 36f);
            var tIco = MkImgSlot(titleGo.transform, "Ico", StepDevilPalette.Accent, 20f, 20f);
            ML(tIco.rectTransform, 0f, 20f, 20f);
            var tLbl = MkTxt(titleGo.transform, "Lbl", "Leave Level?",
                18, StepDevilPalette.Accent, TextAnchor.MiddleCenter, bold: true);
            SI(tLbl.rectTransform);

            PopupDivider(cardRt, 66f);

            var bodyTxt = MkTxt(cardRt, "Body",
                "Progress in this level\nwill be lost.", 13,
                new Color(0.78f, 0.78f, 0.78f, 1f), TextAnchor.MiddleCenter, wrap: true);
            TC(bodyTxt.rectTransform, 75f, 252f, 44f);

            // Button row
            var btnRowGo = new GameObject("Buttons", typeof(RectTransform));
            btnRowGo.transform.SetParent(cardRt, false);
            FWB(btnRowGo.GetComponent<RectTransform>(), 0f, 46f);

            var noBtn = CreateButton(btnRowGo.transform, "NO, STAY",
                new Color(0.15f, 0.55f, 0.2f, 0.35f), OnLeaveNo);
            TL(noBtn.GetComponent<RectTransform>(), 0f, 0f, 144f, 46f);
            var noLbl = noBtn.GetComponentInChildren<TextMeshProUGUI>();
            if (noLbl != null) { noLbl.fontSize = 13f; }

            var yesBtn = CreateButton(btnRowGo.transform, "YES, LEAVE",
                new Color(0.78f, 0.12f, 0.18f, 0.88f), OnLeaveYes);
            TR(yesBtn.GetComponent<RectTransform>(), 0f, 0f, 144f, 46f);
            var yesLbl = yesBtn.GetComponentInChildren<TextMeshProUGUI>();
            if (yesLbl != null) { yesLbl.fontSize = 13f; }

            return popupGo;
        }

        GameObject BuildNoAdsPopup(Transform parent)
        {
            var popupGo = MkPopupBackdrop(parent, "NoAdsPopup", new Color32(0, 0, 0, 174));
            var card = MkPopupCard(popupGo.transform, 300f, 370f,
                new Color32(14, 10, 30, 252), new Color(0.25f, 0.45f, 0.9f, 0.55f));

            var cardRt = card;

            var hdrRt = PopupHeaderRow(card, "NO ADS", Color.white, CloseNoAdsPopup, out _);
            FWT(hdrRt, 16f, 36f);

            PopupDivider(cardRt, 52f);

            // Art area
            var artGo = new GameObject("Art", typeof(RectTransform));
            artGo.transform.SetParent(cardRt, false);
            FWT(artGo.GetComponent<RectTransform>(), 53f, 130f);
            artGo.AddComponent<Image>().color = new Color(0.04f, 0.06f, 0.18f, 1f);
            artGo.AddComponent<Outline>().effectColor = new Color(0.3f, 0.5f, 1f, 0.25f);
            var artIcon = MkAnim(artGo.transform, "ArtIcon", 60f, 60f);
            TC(artIcon.GetComponent<RectTransform>(), 8f, 60f, 60f);
            var artHead = MkTxt(artGo.transform, "Head", "Ad-Free Gaming", 16,
                new Color(0.5f, 0.75f, 1f, 1f), TextAnchor.MiddleCenter, bold: true);
            TC(artHead.rectTransform, 72f, 260f, 22f);
            var artSub = MkTxt(artGo.transform, "Sub",
                "No interruptions · Pure focus\nSupport the developer \u2665",
                11, new Color(0.65f, 0.65f, 0.65f, 1f), TextAnchor.MiddleCenter, wrap: true);
            TC(artSub.rectTransform, 96f, 260f, 36f);

            // Feature bullets
            string[] feats = {
                "\u2022  Remove all banner & video ads",
                "\u2022  Lifetime access — one purchase",
                "\u2022  Works on all your devices"
            };
            for (int i = 0; i < feats.Length; i++)
            {
                var feat = MkTxt(cardRt, "Feat", feats[i], 12,
                    new Color(0.8f, 0.8f, 0.8f, 1f), TextAnchor.MiddleLeft);
                TL(feat.rectTransform, 20f, 191f + i * 26f, 256f, 18f);
            }

            // BUY NOW
            var buyBtn = CreateButton(cardRt, "BUY NOW  \u2014  \u20B999",
                new Color(0.18f, 0.42f, 0.9f, 0.92f), () => OnBuyItem("no_ads", "Remove Ads", 99));
            TC(buyBtn.GetComponent<RectTransform>(), 279f, 260f, 48f);
            var buyLbl = buyBtn.GetComponentInChildren<TextMeshProUGUI>();
            if (buyLbl != null) { buyLbl.fontSize = 14f; }

            return popupGo;
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Popup helpers (no layout groups)
        // ─────────────────────────────────────────────────────────────────────

        GameObject MkPopupBackdrop(Transform parent, string name, Color32 bgColor)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            SI(go.GetComponent<RectTransform>());
            go.AddComponent<Image>().color = bgColor;
            return go;
        }

        RectTransform MkPopupCard(Transform parent, float w, float h,
            Color32 bg, Color outline)
        {
            var cardGo = new GameObject("Card", typeof(RectTransform));
            cardGo.transform.SetParent(parent, false);
            var cardRt = cardGo.GetComponent<RectTransform>();
            MC(cardRt, 0f, 0f, w, h);
            cardGo.AddComponent<Image>().color = bg;
            var ol = cardGo.AddComponent<Outline>();
            ol.effectColor    = outline;
            ol.effectDistance = new Vector2(2f, 2f);
            return cardRt;
        }

        /// <summary>Creates a header row (full-width) with icon+title on the left and X button on the right.
        /// Returns the header RT so the caller can position it with FWT/TC.
        /// outLbl receives the toggle/secondary label if needed — pass _ to discard.</summary>
        RectTransform PopupHeaderRow(RectTransform cardRt, string title, Color32 titleColor,
            Action onClose, out TextMeshProUGUI outLbl)
        {
            var hdrGo = new GameObject("Header", typeof(RectTransform));
            hdrGo.transform.SetParent(cardRt, false);
            var hdrRt = hdrGo.GetComponent<RectTransform>();
            // Caller positions via FWT after this returns.

            // Title icon+label (leaves 44px on right for close btn)
            var titleContainer = new GameObject("TitleArea", typeof(RectTransform));
            titleContainer.transform.SetParent(hdrGo.transform, false);
            SI(titleContainer.GetComponent<RectTransform>(), 0f, 0f, 44f, 0f);
            var tIco = MkImgSlot(titleContainer.transform, "Ico", titleColor, 20f, 20f);
            ML(tIco.rectTransform, 0f, 20f, 20f);
            outLbl = MkTxt(titleContainer.transform, "Lbl", title,
                16, titleColor, TextAnchor.MiddleLeft, bold: true);
            SI(outLbl.rectTransform, 26f, 0f, 0f, 0f);

            // X close button (right)
            var closeBtn = CreateButton(hdrGo.transform, "X",
                new Color(1f, 1f, 1f, 0.12f), onClose);
            MR(closeBtn.GetComponent<RectTransform>(), 0f, 36f, 36f);
            var cLbl = closeBtn.GetComponentInChildren<TextMeshProUGUI>();
            if (cLbl != null) { cLbl.fontSize = 16f; cLbl.color = new Color(1f, 1f, 1f, 0.8f); }

            return hdrRt;
        }

        void PopupDivider(RectTransform cardRt, float y)
        {
            var go = new GameObject("Divider", typeof(RectTransform));
            go.transform.SetParent(cardRt, false);
            FWT(go.GetComponent<RectTransform>(), y, 1f);
            go.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.1f);
        }

        /// <summary>A full-width toggle row at absolute y inside a popup card.
        /// Returns the ON/OFF button label TMP for later updates.</summary>
        TextMeshProUGUI PopupToggleRow(RectTransform cardRt, string label, float y, Action onToggle)
        {
            var rowGo = new GameObject("Row_" + label, typeof(RectTransform));
            rowGo.transform.SetParent(cardRt, false);
            FWT(rowGo.GetComponent<RectTransform>(), y, 46f);
            rowGo.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.04f);

            var icoImg = MkImgSlot(rowGo.transform, "Ico", Color.white, 20f, 20f);
            ML(icoImg.rectTransform, 14f, 20f, 20f);

            var lblTxt = MkTxt(rowGo.transform, "Lbl", label, 14, Color.white,
                TextAnchor.MiddleLeft);
            SI(lblTxt.rectTransform, 42f, 0f, 72f, 0f);

            var toggleBtn = CreateButton(rowGo.transform, "ON",
                new Color(0.18f, 0.72f, 0.28f, 0.3f), onToggle);
            MR(toggleBtn.GetComponent<RectTransform>(), 6f, 64f, 34f);
            var tLbl = toggleBtn.GetComponentInChildren<TextMeshProUGUI>();
            if (tLbl != null) { tLbl.fontSize = 13f; }
            return tLbl;
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Shared factory methods — no LayoutElement attached
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>Create a TextMeshProUGUI. Caller must position the RectTransform.</summary>
        TextMeshProUGUI MkTxt(Transform parent, string name, string txt, int sz, Color32 col,
            TextAnchor align, bool bold = false, bool wrap = false)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var t = go.AddComponent<TextMeshProUGUI>();
            StepDevilTmpUtil.ApplyDefaultFont(t, _tmpFont);
            t.text = txt;
            t.fontSize = sz;
            t.color = col;
            t.alignment = StepDevilTmpUtil.TextAnchorToTmp(align);
            t.enableWordWrapping = wrap;
            t.textWrappingMode = wrap ? TextWrappingModes.Normal : TextWrappingModes.NoWrap;
            t.overflowMode = TextOverflowModes.Overflow;
            t.enableAutoSizing = false;
            t.raycastTarget = false;
            if (bold) t.fontStyle = FontStyles.Bold;
            go.GetComponent<RectTransform>().localScale = Vector3.one;
            return t;
        }

        /// <summary>Creates a Button with label text filling it. Caller positions the RectTransform.</summary>
        Button CreateButton(Transform parent, string label, Color32 bg, Action onClick)
        {
            var go = new GameObject(label, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(200f, 48f); // default; caller overrides via TC/TL/etc.
            var img = go.AddComponent<Image>();
            img.color = bg;
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(() => onClick?.Invoke());
            var lbl = MkTxt(go.transform, "Lbl", label, 14, Color.white,
                TextAnchor.MiddleCenter, bold: true, wrap: false);
            SI(lbl.rectTransform);
            lbl.raycastTarget = false;
            return btn;
        }

        /// <summary>Image + SDSpriteAnimator slot. Caller positions the RectTransform.</summary>
        SDSpriteAnimator MkAnim(Transform parent, string name, float w, float h)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            go.GetComponent<RectTransform>().sizeDelta = new Vector2(w, h);
            var img = go.AddComponent<Image>();
            img.color = Color.white;
            img.preserveAspect = true;
            img.raycastTarget = false;
            EnsureUiImageHasSprite(img);
            return go.AddComponent<SDSpriteAnimator>();
        }

        /// <summary>Plain Image slot (no animator). Caller positions the RectTransform.</summary>
        static Image MkImgSlot(Transform parent, string name, Color32 tint, float w, float h)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            go.GetComponent<RectTransform>().sizeDelta = new Vector2(w, h);
            var img = go.AddComponent<Image>();
            img.color = tint;
            img.preserveAspect = true;
            img.raycastTarget = false;
            EnsureUiImageHasSprite(img);
            return img;
        }

        /// <summary>Blip animation slot — stretches to fill BlipRoot.</summary>
        SDSpriteAnimator CreateBlipAnimatorSlot(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            SI(go.GetComponent<RectTransform>());
            var img = go.AddComponent<Image>();
            img.color = Color.white;
            img.preserveAspect = true;
            img.raycastTarget = false;
            EnsureUiImageHasSprite(img);
            return go.AddComponent<SDSpriteAnimator>();
        }

        /// <summary>Full-colour chip for the title row.</summary>
        void CreateChip(RectTransform parent, string txt, Color32 border, float cx)
        {
            var go = new GameObject("Chip", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            MC(rt, cx, 0f, 110f, 32f);
            go.AddComponent<Image>().color =
                new Color(border.r/255f, border.g/255f, border.b/255f, 0.15f);
            var ol = go.AddComponent<Outline>();
            ol.effectColor = new Color(border.r/255f, border.g/255f, border.b/255f, 0.5f);
            ol.effectDistance = new Vector2(1f, 1f);
            // Icon (left)
            var icoGo = new GameObject("ChipIco", typeof(RectTransform));
            icoGo.transform.SetParent(rt, false);
            var icoRt = icoGo.GetComponent<RectTransform>();
            ML(icoRt, 6f, 14f, 14f);
            var icoImg = icoGo.AddComponent<Image>();
            icoImg.color = border;
            icoImg.preserveAspect = true;
            icoImg.raycastTarget = false;
            EnsureUiImageHasSprite(icoImg);
            // Text (fills remaining space)
            var t = MkTxt(rt, "T", txt, 10, border, TextAnchor.MiddleCenter, bold: true);
            SI(t.rectTransform, 24f, 0f, 0f, 0f);
            t.enableWordWrapping = false;
        }

        /// <summary>Action tile for the horizontal scroll bar.</summary>
        Button CreateActionTile(Transform parent, string icon, string label,
            Color32 bgColor, Action onClick)
        {
            var tileGo = new GameObject(label, typeof(RectTransform));
            tileGo.transform.SetParent(parent, false);
            var rt = tileGo.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(82f, 82f);
            var img = tileGo.AddComponent<Image>();
            img.color = bgColor;
            var btn = tileGo.AddComponent<Button>();
            btn.targetGraphic = img;
            if (onClick != null) btn.onClick.AddListener(() => onClick());
            // LayoutElement so the scroll HLG knows this tile's size
            var le = tileGo.AddComponent<LayoutElement>();
            le.preferredWidth  = 82f;
            le.preferredHeight = 82f;
            // Icon (top-centre of tile)
            var iconGo = new GameObject("Icon", typeof(RectTransform));
            iconGo.transform.SetParent(rt, false);
            TC(iconGo.GetComponent<RectTransform>(), 10f, 28f, 28f);
            var iconImg = iconGo.AddComponent<Image>();
            iconImg.color = Color.white;
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;
            EnsureUiImageHasSprite(iconImg);
            iconGo.AddComponent<SDSpriteAnimator>();
            // Label (bottom-centre of tile)
            var lblT = MkTxt(rt, "Lbl", label, 8, Color.white,
                TextAnchor.MiddleCenter, bold: true);
            BC(lblT.rectTransform, 6f, 74f, 14f);
            lblT.enableWordWrapping = false;
            return btn;
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Low-level constructors
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>Creates a plain RectTransform node (no Image, no layout component).</summary>
        static RectTransform MkNode(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.GetComponent<RectTransform>();
        }

        /// <summary>Creates a full-screen coloured panel. Caller applies SI/FWT/etc to position.</summary>
        static Transform MkPanel(Transform parent, string name, Color32 bg)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            SI(rt); // default: stretch to fill parent
            if (bg.a > 0)
            {
                var img = go.AddComponent<Image>();
                img.color = bg;
                img.raycastTarget = false;
            }
            return go.transform;
        }

        static Image CreateImage(Transform parent, string name, Color32 color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            EnsureUiImageHasSprite(img);
            return img;
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Layout-aware helpers used by Round.cs (truth rows, stone widgets, daily
        //  reward cards, floating text). They attach LayoutElements so parents
        //  using HorizontalLayoutGroup / VerticalLayoutGroup can size them.
        // ─────────────────────────────────────────────────────────────────────

        RectTransform CreatePanel(Transform parent, string name, Color bg,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
            if (bg.a > 0f)
            {
                var img = go.AddComponent<Image>();
                img.color = bg;
                img.raycastTarget = false;
            }
            return rt;
        }

        TextMeshProUGUI CreateText(Transform parent, string name, string txt, int sz, Color col,
            TextAnchor align, bool bold, bool wrap, TMP_FontAsset font, UiTextMode mode,
            float w = 0f, float h = 0f)
        {
            var t = MkTxt(parent, name, txt, sz, col, align, bold, wrap);
            if (font != null) t.font = font;
            if (mode == UiTextMode.FillParent)
            {
                SI(t.rectTransform);
            }
            else
            {
                var le = t.gameObject.AddComponent<LayoutElement>();
                if (w > 0f) le.preferredWidth  = w;
                if (h > 0f) le.preferredHeight = h;
            }
            return t;
        }

        Image CreateImgSlot(Transform parent, string name, Color tint, float w, float h)
        {
            var img = MkImgSlot(parent, name, tint, w, h);
            var le = img.gameObject.AddComponent<LayoutElement>();
            le.preferredWidth  = w;
            le.preferredHeight = h;
            return img;
        }

        SDSpriteAnimator CreateAnimSlot(Transform parent, string name, float w, float h)
        {
            var anim = MkAnim(parent, name, w, h);
            var le = anim.gameObject.AddComponent<LayoutElement>();
            le.preferredWidth  = w;
            le.preferredHeight = h;
            return anim;
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Reward celebration overlay (lazy, created once)
        // ─────────────────────────────────────────────────────────────────────

        void EnsureRewardCelebrationOverlay()
        {
            if (_rewardCelebrationGo != null) return;
            if (_rootRt == null) return;

            var overlayGo = new GameObject("RewardCelebration", typeof(RectTransform));
            overlayGo.transform.SetParent(_rootRt, false);
            overlayGo.transform.SetAsLastSibling();
            SI(overlayGo.GetComponent<RectTransform>());

            var bg = overlayGo.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0f);
            bg.raycastTarget = true;

            var cg = overlayGo.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            cg.interactable = false;
            cg.blocksRaycasts = false;

            var animGo = new GameObject("CelebrationAnim", typeof(RectTransform));
            animGo.transform.SetParent(overlayGo.transform, false);
            MC(animGo.GetComponent<RectTransform>(), 0f, 0f, 300f, 300f);
            var animImg = animGo.AddComponent<Image>();
            animImg.color = Color.white;
            animImg.preserveAspect = true;
            animImg.raycastTarget = false;
            var anim = animGo.AddComponent<SDSpriteAnimator>();

            _rewardCelebrationGo   = overlayGo;
            _rewardCelebrationAnim = anim;
            _rewardCelebrationCg   = cg;

            if (_rewardCelebrationPreset != null && _rewardCelebrationPreset.HasContent)
                _rewardCelebrationAnim.ImportPreset(_rewardCelebrationPreset);
        }
    }
}
