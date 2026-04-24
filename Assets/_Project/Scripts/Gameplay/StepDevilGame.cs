using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StepDevil
{
    public sealed partial class StepDevilGame : MonoBehaviour
    {
        enum ScreenId
        {
            Title,
            LevelMap,
            World,
            Game,
            Result,
            Truth,
            Chest,
            GameOver,
            Complete,
            SpinWheel,
            DailyRewards,
            NoLives,
            Store
        }

        enum ChestRewardType { Coins, ExtraLife }

        struct HistoryEntry
        {
            public StepDevilStoneDef Stone;
            public bool Ok;
            public int Lies;
            public bool ColorLie;
            public bool LabelLie;
            public bool IconLie;
        }

        struct StoneWidgets
        {
            public GameObject Root;
            public Button Button;
            public Image Bg;
            public Image TopBar;
            public SDSpriteAnimator Icon;  // Image placeholder — assign sprites in the Inspector
            public TextMeshProUGUI Label;
            public TextMeshProUGUI Hint;
            public TextMeshProUGUI Tag;
            public int Index;
        }

        const float ChoiceAnimSeconds = 0.35f;
        const float RevealDelaySeconds = 0.55f;
        const float NextForkDelaySeconds = 0.35f;
        const float ForkChoiceSeconds = 12f;

        [Header("UI source")]
        [Tooltip("When true, use the Canvas/Root already in the scene (your Inspector edits persist). When false, Awake destroys children and rebuilds UI from code — every SDSpriteAnimator under this object is snapshotted by hierarchy path before destroy (title devilEmoji, blips, etc.) and restored after rebuild; you can still override blips with the arrays below.")]
        [SerializeField] bool _useHierarchyFromScene = true;

        [Tooltip("When ON, every ShowScreen() re-runs defensive fixups: reorders Game-screen children, overwrites VLG/HLG flags, snaps Timer/Mirror/Blip TMPs to hard-coded anchors, and forces min widths on every TMP. Keep this on if your scene layout renders broken at runtime. Turn it OFF to trust your Inspector-authored positions, anchors and sizes as-is.")]
        [SerializeField] bool _overrideSceneLayoutAtRuntime = true;

        [Header("Scene override slots (optional)")]
        [Tooltip("Drag your scene-authored in-game Back button here. Used when the auto-finder can't locate it by name (e.g. you named it something unusual like '<'). Takes priority over the finder.")]
        [SerializeField] Button _sceneGameBackButtonOverride;
        [Tooltip("Drag your scene-authored Daily Challenge button (on the Title / ActionBar). Leave empty to let the finder locate it by name/label.")]
        [SerializeField] Button _sceneDailyChallengeButtonOverride;
        [Tooltip("Drag your scene-authored Daily Rewards button. Leave empty to let the finder locate it by name/label.")]
        [SerializeField] Button _sceneDailyRewardsButtonOverride;
        [Tooltip("Drag your scene-authored Spin Wheel button. Leave empty to let the finder locate it by name/label.")]
        [SerializeField] Button _sceneSpinButtonOverride;
        [Tooltip("Drag your scene-authored Store button. Leave empty to let the finder locate it by name/label.")]
        [SerializeField] Button _sceneStoreButtonOverride;
        [Tooltip("Drag your scene-authored Settings button. Leave empty to let the finder locate it by name/label.")]
        [SerializeField] Button _sceneSettingsButtonOverride;
        [Tooltip("Drag your scene-authored No Ads button. Leave empty to let the finder locate it by name/label.")]
        [SerializeField] Button _sceneNoAdsButtonOverride;

        [SerializeField] TMP_FontAsset _tmpFont;

        [Header("Blip sprites (code-built UI only)")]
        [Tooltip("When Use Hierarchy From Scene is off, optional override: if non-empty, replaces that blip’s sprites (otherwise sprites are restored from the scene hierarchy before destroy).")]
        [SerializeField] Sprite[] _blipIdleFrames;
        [SerializeField] Sprite[] _blipCorrectFrames;
        [SerializeField] Sprite[] _blipWrongFrames;
        [SerializeField] Sprite[] _blipTimeUpFrames;

        [Header("Reward Celebration")]
        [Tooltip("Drag coin / confetti / diamond sprites here. Played as a looping animation overlay whenever a reward is claimed.")]
        [SerializeField] SDSpriteAnimatorPreset _rewardCelebrationPreset;

        Canvas _canvas;
        RectTransform _rootRt;
        Image _flashOverlay;

        GameObject _titleGo;
        GameObject _levelMapGo;
        GameObject _worldGo;
        GameObject _gameGo;
        GameObject _resultGo;
        GameObject _truthGo;
        GameObject _gameOverGo;
        GameObject _completeGo;

        Button _titlePlayButton;
        Button _gameBackButton;
        Button _dailyChallengeButton;
        Image _dailyChallengeIconImg;
        SDSpriteAnimator _titleDevilAnim;
        StepDevilLevelMapView _levelMapView;
        TextMeshProUGUI _levelMapLivesText;

        TextMeshProUGUI _worldNum;
        SDSpriteAnimator _worldIcon;
        TextMeshProUGUI _worldName;
        TextMeshProUGUI _worldRule;
        Button _worldGoBtn;

        TextMeshProUGUI _livesText;
        TextMeshProUGUI _levelNum;
        TextMeshProUGUI _coinsText;
        TextMeshProUGUI _worldHint;

        RectTransform _pathDotsRoot;
        readonly List<Image> _pathDots = new List<Image>();

        TextMeshProUGUI _blipText;
        RectTransform _blipRt;
        StepDevilBlipController _blipVisuals;
        SDSpriteAnimator _devilAnim;
        TextMeshProUGUI _mirrorBanner;
        RectTransform _stonesRoot;
        readonly List<StoneWidgets> _stonePool = new List<StoneWidgets>();

        TextMeshProUGUI _timerLabel;
        Image _timerFill;
        TextMeshProUGUI _devilTipText;

        SDSpriteAnimator _resultIcon;
        TextMeshProUGUI _resultTitle;
        TextMeshProUGUI _resultSub;
        TextMeshProUGUI _resultLives;
        Button _resultRetry;

        ScrollRect _truthScroll;
        RectTransform _truthRowsRoot;
        TextMeshProUGUI _truthSum;
        Button _truthContinueButton;

        TextMeshProUGUI _goLevel;
        TextMeshProUGUI _goCoins;
        Button _gameOverPlayAgainButton;

        RectTransform _confettiRoot;
        TextMeshProUGUI _completeCoins;
        TextMeshProUGUI _completeLies;
        TextMeshProUGUI _completeFalls;
        Button _completePlayAgainButton;

        // Chest
        GameObject _chestGo;
        Image _chestEmoji;
        TextMeshProUGUI _chestTitle;
        TextMeshProUGUI _chestRewardLabel;
        TextMeshProUGUI _chestRewardValue;
        Button _chestClaimButton;
        ChestRewardType _pendingReward;
        int _pendingRewardAmount;

        readonly List<HistoryEntry> _history = new List<HistoryEntry>();

        int _levelIndex;
        int _forkIndex;
        int _lives = 3;
        int _coins;
        bool _locked;
        bool _mirror;
        int _mirrorCd;
        float _timerStart;
        int _totalLies;
        int _totalFalls;
        int _totalLevelsCleared;
        bool _isDailyChallenge;
        bool _pendingReturnToTitle; // set when daily challenge triggers a chest — claim should go to Title not next level
        bool _spinWheelIsSpinning;
        bool _spinWheelPendingClaim;

        // Title screen wallet bar + action buttons
        TextMeshProUGUI _titleCoinsText;
        TextMeshProUGUI _titleDiamondsText;
        Button _spinButton;
        Button _dailyRewardsButton;
        Button _storeButton;
        Button _settingsButton;
        Button _noAdsButton;

        // Spin Wheel screen
        GameObject _spinWheelGo;
        RectTransform _spinWheelContainer;
        TextMeshProUGUI _spinResultText;
        Button _spinActionButton;
        TextMeshProUGUI _spinActionLabel;

        // Daily Rewards screen
        GameObject _dailyRewardsGo;
        RectTransform _dailyRewardsDaysRoot;

        // No Lives screen
        GameObject _noLivesGo;
        TextMeshProUGUI _noLivesInfoText;
        TextMeshProUGUI _noLivesWalletText;
        Button _noLivesBuyCoinsBtn;
        Button _noLivesBuyDiamondsBtn;
        Button _noLivesRetryBtn;

        // Store screen
        GameObject _storeGo;

        // No Ads popup
        GameObject _noAdsPopupGo;

        // No Lives popup (shown from level map when TotalLives == 0)
        GameObject _noLivesPopupGo;
        TextMeshProUGUI _nlpWalletText;
        Button _nlpBuyCoinsBtn;
        Button _nlpBuyDiamondsBtn;
        int _pendingLevelFromMap = -1;

        // Reward celebration overlay
        GameObject _rewardCelebrationGo;
        SDSpriteAnimator _rewardCelebrationAnim;
        CanvasGroup _rewardCelebrationCg;

        // Popups
        GameObject _settingsPopupGo;
        GameObject _leavePopupGo;
        TextMeshProUGUI _soundToggleLbl;
        TextMeshProUGUI _musicToggleLbl;
        bool _gamePaused;
        float _pauseElapsed;
        const string KeySoundEnabled = "sd_sound_on";
        const string KeyMusicEnabled = "sd_music_on";

        // Currently running round state-machine coroutine (Choice/Reveal or ForkTimerExpired).
        // Tracked so we can abort it cleanly when the player leaves gameplay mid-animation.
        Coroutine _roundCo;

        void Awake()
        {
            if (_useHierarchyFromScene)
                BindFromHierarchy();
            else
            {
                var capturedAnimators = CaptureAllSpriteAnimatorPresets();
                for (var i = transform.childCount - 1; i >= 0; i--)
                    Destroy(transform.GetChild(i).gameObject);
                BuildUi();
                ApplyCodeBuiltAnimationData(capturedAnimators);
            }

            if (_titleDevilAnim == null && _titleGo != null)
                _titleDevilAnim = FindTitleDevilEmojiTransform(_titleGo.transform)?.GetComponent<SDSpriteAnimator>();

            // Build new code-only screens that are never in the scene hierarchy
            if (_rootRt != null && _spinWheelGo == null)
            {
                _spinWheelGo    = BuildSpinWheelScreen(_rootRt);    _spinWheelGo.SetActive(false);
                _dailyRewardsGo = BuildDailyRewardsScreen(_rootRt); _dailyRewardsGo.SetActive(false);
                _noLivesGo      = BuildNoLivesScreen(_rootRt);       _noLivesGo.SetActive(false);
            }

            // Build No Lives popup if not yet created (scene-hierarchy path)
            if (_rootRt != null && _noLivesPopupGo == null)
            {
                _noLivesPopupGo = BuildNoLivesPopup(_rootRt);
                _noLivesPopupGo.SetActive(false);
            }

            // Popups / screens that BuildUi() creates in the code-built path but the
            // scene-hierarchy path skips. Without these, Back/Store/Settings taps silently
            // no-op because their target GameObject is null.
            if (_rootRt != null && _settingsPopupGo == null)
            {
                _settingsPopupGo = BuildSettingsPopup(_rootRt);
                _settingsPopupGo.SetActive(false);
            }
            if (_rootRt != null && _leavePopupGo == null)
            {
                _leavePopupGo = BuildLeavePopup(_rootRt);
                _leavePopupGo.SetActive(false);
            }
            if (_rootRt != null && _noAdsPopupGo == null)
            {
                _noAdsPopupGo = BuildNoAdsPopup(_rootRt);
                _noAdsPopupGo.SetActive(false);
            }
            if (_rootRt != null && _storeGo == null)
            {
                _storeGo = BuildStoreScreen(_rootRt);
                _storeGo.SetActive(false);
            }

            // Inject title extras (wallet bar + spin + rewards buttons) only if neither
            // the code-built bar nor a scene-authored ActionBar already exists. Without
            // this check, ANY call path that leaves _spinButton null (e.g. scene mode)
            // would duplicate the bar every time Awake runs.
            var sceneHasActionBar = _titleGo != null && _titleGo.transform.Find("ActionBar") != null;
            if (_spinButton == null && _titleGo != null && !sceneHasActionBar)
                InjectTitleExtras();

            // Inject daily challenge button if not already created (scene-based UI path)
            if (_dailyChallengeButton == null && _titleGo != null)
                InjectDailyChallengeButton();

            WireBuiltUiButtons();
            RefreshTitleWalletBar();
            RefreshDailyRewardsButton();
            // Sync scene-authored action buttons to current game state. Code-built
            // buttons already get the right tint at creation, but scene-bound ones
            // inherit whatever colour the author set.
            RefreshDailyButton();
            RefreshSpinButton();
            ShowScreen(ScreenId.Title);
        }

        void OnDisable()
        {
            // Game object is being disabled or destroyed (scene change, domain reload, app quit).
            // Stop every coroutine owned by this MonoBehaviour and kill any tween we still
            // hold references to so callbacks never fire on invalid targets.
            StopAllCoroutines();
            _roundCo = null;

            if (_blipRt != null)                SDTween.Kill(_blipRt);
            if (_mirrorBanner != null)          SDTween.Kill(_mirrorBanner);
            if (_flashOverlay != null)          SDTween.Kill(_flashOverlay);
            if (_spinWheelContainer != null)    SDTween.Kill(_spinWheelContainer);
            if (_rewardCelebrationCg != null)   SDTween.Kill(_rewardCelebrationCg);
            if (_chestEmoji != null)            SDTween.Kill(_chestEmoji.rectTransform);
        }

        /// <summary>Cleanly abort the active round state machine (choice/reveal/timer-expired) and reset gameplay locks.</summary>
        void AbortActiveRound()
        {
            if (_roundCo != null)
            {
                StopCoroutine(_roundCo);
                _roundCo = null;
            }
            if (_blipRt != null) SDTween.Kill(_blipRt);
            _locked = false;
        }

        // App-pause handling: when the player backgrounds the app mid-fork, the fork
        // timer must NOT keep ticking — otherwise they return to a guaranteed "Time's up!"
        // We freeze _timerStart across the pause just like the Leave popup does.
        bool _backgroundedDuringFork;

        void OnApplicationPause(bool paused)
        {
            HandleAppInterruption(paused);
        }

        void OnApplicationFocus(bool hasFocus)
        {
            HandleAppInterruption(!hasFocus);
        }

        void HandleAppInterruption(bool interrupted)
        {
            if (_gameGo == null || !_gameGo.activeInHierarchy) return;

            if (interrupted)
            {
                if (_gamePaused || _backgroundedDuringFork) return;
                _pauseElapsed = Time.unscaledTime - _timerStart;
                _gamePaused = true;
                _backgroundedDuringFork = true;
            }
            else if (_backgroundedDuringFork)
            {
                _backgroundedDuringFork = false;
                _gamePaused = false;
                _timerStart = Time.unscaledTime - _pauseElapsed;
            }
        }

        void Update()
        {
            if (_gameGo == null || !_gameGo.activeInHierarchy || _locked || _timerFill == null || _gamePaused)
                return;
            var elapsed = Time.unscaledTime - _timerStart;
            var remaining01 = 1f - Mathf.Clamp01(elapsed / ForkChoiceSeconds);
            _timerFill.fillAmount = remaining01;
            SetTimerBarColorForRemainingFraction(remaining01);
            if (elapsed >= ForkChoiceSeconds)
                OnForkChoiceTimeExpired();
        }

        /// <summary>Green when plenty of time left, yellow in the middle third, red in the last third.</summary>
        void SetTimerBarColorForRemainingFraction(float remaining01)
        {
            if (_timerFill == null)
                return;
            remaining01 = Mathf.Clamp01(remaining01);
            var g = (Color)StepDevilPalette.Safe;
            var y = (Color)StepDevilPalette.Gold;
            var r = (Color)StepDevilPalette.Danger;
            if (remaining01 > 2f / 3f)
                _timerFill.color = Color.Lerp(y, g, (remaining01 - 2f / 3f) / (1f / 3f));
            else if (remaining01 > 1f / 3f)
                _timerFill.color = y;
            else
                _timerFill.color = Color.Lerp(r, y, remaining01 / (1f / 3f));
        }

        /// <summary>Path under <see cref="StepDevilGame"/> transform, e.g. <c>StepDevilCanvas/Root/Title/Content/devilEmoji</c>.</summary>
        static string BuildRelativePath(Transform root, Transform descendant)
        {
            if (root == null || descendant == null || !descendant.IsChildOf(root))
                return null;
            if (descendant == root)
                return "";
            var parts = new List<string>();
            var t = descendant;
            while (t != null && t != root)
            {
                parts.Add(t.name);
                t = t.parent;
            }

            if (t != root)
                return null;
            parts.Reverse();
            return string.Join("/", parts);
        }

        /// <summary>Title <c>devilEmoji</c> lives under <c>Content</c> in code-built UI — a single-name <see cref="Transform.Find(string)"/> only searches direct children.</summary>
        internal static Transform FindTitleDevilEmojiTransform(Transform titleScreenRoot)
        {
            if (titleScreenRoot == null)
                return null;
            var direct = titleScreenRoot.Find("Content/devilEmoji") ?? titleScreenRoot.Find("devilEmoji");
            if (direct != null)
                return direct;
            foreach (var tr in titleScreenRoot.GetComponentsInChildren<Transform>(true))
            {
                if (tr.name == "devilEmoji")
                    return tr;
            }

            return null;
        }

        /// <summary>Snapshot every <see cref="SDSpriteAnimator"/> under this object before code-built UI is destroyed.</summary>
        List<(string Path, SDSpriteAnimatorPreset Preset)> CaptureAllSpriteAnimatorPresets()
        {
            var list = new List<(string Path, SDSpriteAnimatorPreset Preset)>();
            var anims = GetComponentsInChildren<SDSpriteAnimator>(true);
            for (var i = 0; i < anims.Length; i++)
            {
                var anim = anims[i];
                var path = BuildRelativePath(transform, anim.transform);
                if (string.IsNullOrEmpty(path))
                    continue;
                var preset = anim.ExportPreset();
                if (preset == null || !preset.HasContent)
                    continue;
                list.Add((path, preset));
            }

            return list;
        }

        void ApplySpriteAnimatorPresetsByPath(IReadOnlyList<(string Path, SDSpriteAnimatorPreset Preset)> captured)
        {
            for (var i = 0; i < captured.Count; i++)
            {
                var entry = captured[i];
                var t = transform.Find(entry.Path);
                if (t == null)
                    continue;
                var anim = t.GetComponent<SDSpriteAnimator>();
                if (anim == null)
                    continue;
                anim.ImportPreset(entry.Preset);
                anim.Play();
            }
        }

        /// <summary>Restore all UI sprite animations from the pre-destroy hierarchy, then optional blip array overrides.</summary>
        void ApplyCodeBuiltAnimationData(IReadOnlyList<(string Path, SDSpriteAnimatorPreset Preset)> capturedAnimators)
        {
            if (_useHierarchyFromScene)
                return;
            ApplySpriteAnimatorPresetsByPath(capturedAnimators);
            if (_blipVisuals != null)
            {
                TryApplyBlipSerializedOverride(_blipVisuals.IdleAnimator, _blipIdleFrames);
                TryApplyBlipSerializedOverride(_blipVisuals.CorrectAnimator, _blipCorrectFrames);
                TryApplyBlipSerializedOverride(_blipVisuals.WrongAnimator, _blipWrongFrames);
                TryApplyBlipSerializedOverride(_blipVisuals.TimeUpAnimator, _blipTimeUpFrames);
                _blipVisuals.ShowIdle();
            }

            if (_titleGo != null)
                _titleDevilAnim = FindTitleDevilEmojiTransform(_titleGo.transform)?.GetComponent<SDSpriteAnimator>();
        }

        static void TryApplyBlipSerializedOverride(SDSpriteAnimator anim, Sprite[] serializedOverride)
        {
            if (anim == null || serializedOverride == null || serializedOverride.Length == 0)
                return;
            anim.SetDefaultFrames(serializedOverride);
        }

        void WireBuiltUiButtons()
        {
            if (_titlePlayButton != null)
            {
                _titlePlayButton.onClick.RemoveAllListeners();
                _titlePlayButton.onClick.AddListener(OpenLevelMap);
            }

            // Scene-authored Game back button — only reachable through this binding,
            // since the code-built path wires its own back button inline in BuildGameScreen.
            if (_gameBackButton != null)
            {
                _gameBackButton.onClick.RemoveAllListeners();
                _gameBackButton.onClick.AddListener(OnBackFromGame);
            }

            if (_dailyChallengeButton != null)
            {
                _dailyChallengeButton.onClick.RemoveAllListeners();
                _dailyChallengeButton.onClick.AddListener(OnDailyChallengePressed);
            }

            if (_spinButton != null)
            {
                _spinButton.onClick.RemoveAllListeners();
                _spinButton.onClick.AddListener(OpenSpinWheel);
            }

            if (_dailyRewardsButton != null)
            {
                _dailyRewardsButton.onClick.RemoveAllListeners();
                _dailyRewardsButton.onClick.AddListener(OpenDailyRewards);
            }

            // Scene-authored ActionBar buttons. The code-built injector wires its own copies
            // inline via CreateActionTile, so these listener hooks only fire for
            // scene/override-bound buttons and are idempotent either way.
            if (_storeButton != null)
            {
                _storeButton.onClick.RemoveAllListeners();
                _storeButton.onClick.AddListener(OpenStore);
            }
            if (_settingsButton != null)
            {
                _settingsButton.onClick.RemoveAllListeners();
                _settingsButton.onClick.AddListener(OpenSettingsPopup);
            }
            if (_noAdsButton != null)
            {
                _noAdsButton.onClick.RemoveAllListeners();
                _noAdsButton.onClick.AddListener(OpenNoAdsPopup);
            }

            if (_spinActionButton != null)
            {
                _spinActionButton.onClick.RemoveAllListeners();
                _spinActionButton.onClick.AddListener(OnSpinButtonPressed);
            }

            if (_chestClaimButton != null)
            {
                _chestClaimButton.onClick.RemoveAllListeners();
                _chestClaimButton.onClick.AddListener(OnChestClaim);
            }

            if (_noLivesBuyCoinsBtn != null)
            {
                _noLivesBuyCoinsBtn.onClick.RemoveAllListeners();
                _noLivesBuyCoinsBtn.onClick.AddListener(OnBuyLifeWithCoins);
            }

            if (_noLivesBuyDiamondsBtn != null)
            {
                _noLivesBuyDiamondsBtn.onClick.RemoveAllListeners();
                _noLivesBuyDiamondsBtn.onClick.AddListener(OnBuy3LivesWithDiamonds);
            }

            if (_noLivesRetryBtn != null)
            {
                _noLivesRetryBtn.onClick.RemoveAllListeners();
                _noLivesRetryBtn.onClick.AddListener(OnRetryFromNoLives);
            }

            if (_worldGoBtn != null)
            {
                _worldGoBtn.onClick.RemoveAllListeners();
                _worldGoBtn.onClick.AddListener(BeginLevel);
            }

            if (_levelMapView != null)
                _levelMapView.Initialize(OnBackFromLevelMap, OnLevelSelectedFromMap);

            if (_truthContinueButton != null)
            {
                _truthContinueButton.onClick.RemoveAllListeners();
                _truthContinueButton.onClick.AddListener(AfterTruth);
            }

            if (_gameOverPlayAgainButton != null)
            {
                _gameOverPlayAgainButton.onClick.RemoveAllListeners();
                _gameOverPlayAgainButton.onClick.AddListener(OnGameOverPlayAgain);
            }

            if (_completePlayAgainButton != null)
            {
                _completePlayAgainButton.onClick.RemoveAllListeners();
                _completePlayAgainButton.onClick.AddListener(StartGame);
            }

            if (_resultRetry != null)
            {
                _resultRetry.onClick.RemoveAllListeners();
                _resultRetry.onClick.AddListener(RetryLevel);
            }
        }

        void BindFromHierarchy()
        {
            if (!StepDevilUiReferenceFinder.TryFind(transform, out var r, out var bindFailure))
            {
                Debug.LogError("[StepDevilGame] Could not bind UI from hierarchy. " + bindFailure, this);
                return;
            }

            _canvas = r.Canvas;
            _rootRt = r.Root;
            _flashOverlay = r.FlashOverlay;
            _titleGo = r.TitleScreen;
            _titleDevilAnim = r.TitleDevilAnim;
            _titlePlayButton = r.TitlePlayButton;
            // Optional scene-authored Title wallet. If bound here, InjectTitleExtras
            // below skips its duplicate wallet row to avoid layout reflow.
            if (r.TitleCoinsText != null)    _titleCoinsText = r.TitleCoinsText;
            if (r.TitleDiamondsText != null) _titleDiamondsText = r.TitleDiamondsText;

            // ActionBar buttons: Inspector override wins, else the auto-finder result.
            // Any button that binds here suppresses its matching code-built/injected version.
            _dailyChallengeButton = _sceneDailyChallengeButtonOverride ?? r.DailyChallengeButton;
            _dailyRewardsButton   = _sceneDailyRewardsButtonOverride   ?? r.DailyRewardsButton;
            _spinButton           = _sceneSpinButtonOverride           ?? r.SpinButton;
            _storeButton          = _sceneStoreButtonOverride          ?? r.StoreButton;
            _settingsButton       = _sceneSettingsButtonOverride       ?? r.SettingsButton;
            _noAdsButton          = _sceneNoAdsButtonOverride          ?? r.NoAdsButton;

            _levelMapGo = r.LevelMapScreen;
            _levelMapView = r.LevelMapView;
            _worldGo = r.WorldScreen;
            _worldNum = r.WorldNum;
            _worldIcon = r.WorldIcon;
            _worldName = r.WorldName;
            _worldRule = r.WorldRule;
            _worldGoBtn = r.WorldGoButton;
            _gameGo = r.GameScreen;
            // Inspector override wins — falls back to the auto-finder result, which in
            // turn may be null if the scene button has an unusual name. Either way,
            // WireBuiltUiButtons null-guards before wiring.
            _gameBackButton = _sceneGameBackButtonOverride != null
                ? _sceneGameBackButtonOverride
                : r.GameBackButton;
            _livesText = r.LivesText;
            _levelNum = r.LevelNum;
            _coinsText = r.CoinsText;
            _worldHint = r.WorldHint;
            _pathDotsRoot = r.PathDotsRoot;
            _blipVisuals = r.BlipController;
            _blipText = r.BlipText;
            _devilAnim = r.DevilAnim;
            _mirrorBanner = r.MirrorBanner;
            _stonesRoot = r.StonesRoot;
            _timerLabel = r.TimerLabel;
            _timerFill = r.TimerFill;
            _resultGo = r.ResultScreen;
            _resultIcon = r.ResultIcon;
            _resultTitle = r.ResultTitle;
            _resultSub = r.ResultSub;
            _resultLives = r.ResultLives;
            _resultRetry = r.ResultRetry;
            _truthGo = r.TruthScreen;
            _truthRowsRoot = r.TruthRowsRoot;
            _truthSum = r.TruthSum;
            _truthScroll = r.TruthScroll;
            _truthContinueButton = r.TruthContinueButton;
            _gameOverGo = r.GameOverScreen;
            _goLevel = r.GameOverLevelText;
            _goCoins = r.GameOverCoinsText;
            _gameOverPlayAgainButton = r.GameOverPlayAgainButton;
            _completeGo = r.CompleteScreen;
            _completeCoins = r.CompleteCoins;
            _completeLies = r.CompleteLies;
            _completeFalls = r.CompleteFalls;
            _confettiRoot = r.ConfettiRoot;
            _completePlayAgainButton = r.CompletePlayAgainButton;

            if (_blipVisuals != null)
                _blipRt = _blipVisuals.Root;
            else if (_blipText != null)
                _blipRt = _blipText.rectTransform;

            if (_timerFill == null && _gameGo != null)
            {
                var fillTf = _gameGo.transform.Find("Timer/Track/Fill");
                if (fillTf != null)
                    _timerFill = fillTf.GetComponent<Image>();
            }
        }

        void InjectDailyChallengeButton()
        {
            // Find the play button's parent layout group
            if (_titlePlayButton == null) return;
            var parent = _titlePlayButton.transform.parent;
            if (parent == null) return;

            var dailyDone = StepDevilDailyChallenge.IsCompletedToday();
            var btnColor = dailyDone ? StepDevilPalette.Grey : StepDevilPalette.Gold;
            _dailyChallengeButton = CreateButton(parent, dailyDone ? "DAILY DONE" : "DAILY CHALLENGE", btnColor, OnDailyChallengePressed);
            var dcTmp = _dailyChallengeButton.GetComponentInChildren<TextMeshProUGUI>();
            if (dcTmp != null) dcTmp.fontSize = 14f;
            var dle = _dailyChallengeButton.gameObject.AddComponent<LayoutElement>();
            dle.preferredWidth = 200f;
            dle.preferredHeight = 42f;
            // Place it right after the play button
            _dailyChallengeButton.transform.SetSiblingIndex(_titlePlayButton.transform.GetSiblingIndex() + 1);

            if (!dailyDone)
                _dailyChallengeButton.interactable = true;
            else
                _dailyChallengeButton.interactable = false;
        }

        void OnDailyChallengePressed()
        {
            if (StepDevilDailyChallenge.IsCompletedToday())
                return; // already done today

            _isDailyChallenge = true;
            _levelIndex = StepDevilDailyChallenge.GetTodayLevelIndex();
            _forkIndex = 0;
            _lives = StepDevilWallet.TotalLives;
            _coins = StepDevilWallet.Coins;
            _totalLies = 0;
            _totalFalls = 0;
            _totalLevelsCleared = 0;
            _mirror = false;
            _mirrorCd = 0;
            _history.Clear();

            // Show world screen so player sees the rules for this level's world
            PopulateWorldScreen();
            ShowScreen(ScreenId.World);
        }

        void RefreshDailyButton()
        {
            if (_dailyChallengeButton == null)
                return;
            var done = StepDevilDailyChallenge.IsCompletedToday();
            // Icon is now an Image placeholder — swap sprites in Inspector; only tint the button here.
            var img = _dailyChallengeButton.GetComponent<UnityEngine.UI.Image>();
            if (img != null)
                img.color = done ? (Color)StepDevilPalette.Grey : (Color)StepDevilPalette.Gold;
            _dailyChallengeButton.interactable = !done;
        }

        void RefreshTitleWalletBar()
        {
            if (_titleCoinsText != null)
                _titleCoinsText.text = StepDevilWallet.Coins.ToString();
            if (_titleDiamondsText != null)
                _titleDiamondsText.text = StepDevilWallet.Diamonds.ToString();
        }

        void RefreshLevelMapHeader()
        {
            if (_levelMapLivesText == null) return;
            var total = StepDevilWallet.TotalLives;
            _levelMapLivesText.text = total > 0
                ? total.ToString()
                : "<color=#FF4D6D>0</color>";
        }

        void RefreshSpinButton()
        {
            if (_spinButton == null) return;
            var canSpin = StepDevilSpinWheel.CanSpinToday();
            var img = _spinButton.GetComponent<Image>();
            if (img != null) img.color = canSpin ? (Color)StepDevilPalette.Gold : (Color)StepDevilPalette.Grey;
            // Icon is now an Image placeholder — swap sprites in Inspector.
            _spinButton.interactable = canSpin;
        }

        /// <summary>Keeps the Title-screen "REWARDS" tile in sync with
        /// <see cref="StepDevilDailyReward.IsTodayClaimed"/>. Needed on launch, on return
        /// from the daily-rewards screen, and after midnight rollovers — parallels
        /// RefreshDailyButton / RefreshSpinButton.</summary>
        void RefreshDailyRewardsButton()
        {
            if (_dailyRewardsButton == null) return;
            var claimed = StepDevilDailyReward.IsTodayClaimed;
            var img = _dailyRewardsButton.GetComponent<Image>();
            if (img != null)
                img.color = claimed ? (Color)StepDevilPalette.Grey : new Color32(100, 60, 200, 255);
            _dailyRewardsButton.interactable = !claimed;
        }

        void OpenSpinWheel()
        {
            _spinWheelIsSpinning = false;
            _spinWheelPendingClaim = false;
            PopulateSpinScreen();
            ShowScreen(ScreenId.SpinWheel);
        }

        void OpenDailyRewards()
        {
            PopulateDailyRewardsScreen();
            ShowScreen(ScreenId.DailyRewards);
        }

        void InjectTitleExtras()
        {
            // For the scene-based UI path: build the bottom action bar onto the title screen root.
            if (_titleGo != null)
                BuildTitleBottomBar(_titleGo.transform);
        }

        // [DEBUG] Remove before release
        void InjectDebugRestoreLivesButton()
        {
            if (_titlePlayButton == null) return;
            var parent = _titlePlayButton.transform.parent;
            if (parent == null) return;

            var btn = CreateButton(parent, "[DEBUG] Restore Daily Lives", new Color32(180, 50, 50, 255), () =>
            {
                StepDevilWallet.DEBUG_RestoreDailyLives();
                _lives = StepDevilWallet.TotalLives;
                UpdateLives();
                RefreshTitleWalletBar();
            });
            var lbl = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (lbl != null) lbl.fontSize = 10f;
            btn.gameObject.AddComponent<LayoutElement>().preferredHeight = 30f;
        }

        void OpenLevelMap()
        {
            _isDailyChallenge = false;
            if (_levelMapView != null)
            {
                _levelMapView.RefreshUnlocks();
                _levelMapView.ScrollToLevel(StepDevilProgress.GetMaxUnlockedLevel());
            }

            ShowScreen(ScreenId.LevelMap);
        }

        void OnBackFromLevelMap()
        {
            ShowScreen(ScreenId.Title);
        }

        void OnBackFromGame()
        {
            OpenLeavePopup();
        }

        // ── Settings popup ───────────────────────────────────────────────────
        void OpenSettingsPopup()
        {
            RefreshSettingsToggleLabels();
            if (_settingsPopupGo != null) _settingsPopupGo.SetActive(true);
        }

        void CloseSettingsPopup()
        {
            if (_settingsPopupGo != null) _settingsPopupGo.SetActive(false);
        }

        void ToggleSound()
        {
            var on = PlayerPrefs.GetInt(KeySoundEnabled, 1) == 1;
            PlayerPrefs.SetInt(KeySoundEnabled, on ? 0 : 1);
            PlayerPrefs.Save();
            RefreshSettingsToggleLabels();
        }

        void ToggleMusic()
        {
            var on = PlayerPrefs.GetInt(KeyMusicEnabled, 1) == 1;
            PlayerPrefs.SetInt(KeyMusicEnabled, on ? 0 : 1);
            PlayerPrefs.Save();
            RefreshSettingsToggleLabels();
        }

        void RefreshSettingsToggleLabels()
        {
            if (_soundToggleLbl != null)
                _soundToggleLbl.text = PlayerPrefs.GetInt(KeySoundEnabled, 1) == 1 ? "ON" : "OFF";
            if (_musicToggleLbl != null)
                _musicToggleLbl.text = PlayerPrefs.GetInt(KeyMusicEnabled, 1) == 1 ? "ON" : "OFF";
        }

        void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // ── Store screen ─────────────────────────────────────────────────────
        void OpenStore()
        {
            ShowScreen(ScreenId.Store);
        }

        void OnBackFromStore()
        {
            ShowScreen(ScreenId.Title);
        }

        void OnBuyItem(string itemId, string displayName, int price)
        {
            // Placeholder: In production connect to IAP here.
            Debug.Log($"[Store] Buy requested: {displayName} — ₹{price} (id={itemId})");
        }

        // ── No Ads popup ─────────────────────────────────────────────────────
        void OpenNoAdsPopup()
        {
            if (_noAdsPopupGo != null) _noAdsPopupGo.SetActive(true);
        }

        void CloseNoAdsPopup()
        {
            if (_noAdsPopupGo != null) _noAdsPopupGo.SetActive(false);
        }

        // ── Leave Level popup ────────────────────────────────────────────────
        void OpenLeavePopup()
        {
            if (_leavePopupGo == null) return;
            _pauseElapsed = Time.unscaledTime - _timerStart;
            _gamePaused = true;
            _locked = true;
            _leavePopupGo.SetActive(true);
        }

        void CloseLeavePopup()
        {
            if (_leavePopupGo != null) _leavePopupGo.SetActive(false);
            _gamePaused = false;
            _locked = false;
            _timerStart = Time.unscaledTime - _pauseElapsed;
        }

        void OnLeaveYes()
        {
            if (_leavePopupGo != null) _leavePopupGo.SetActive(false);
            _gamePaused = false;
            AbortActiveRound();

            // Return to wherever the player entered gameplay from. Daily Challenge starts
            // on the Title screen, so abandoning a daily attempt should bounce straight back
            // there — not to the campaign Level Map the player never opened.
            if (_isDailyChallenge)
            {
                _isDailyChallenge = false;
                RefreshDailyButton();
                RefreshTitleWalletBar();
                ShowScreen(ScreenId.Title);
            }
            else
            {
                ShowScreen(ScreenId.LevelMap);
            }
        }

        void OnLeaveNo()
        {
            CloseLeavePopup();
        }

        void OnLevelSelectedFromMap(int oneBasedLevel)
        {
            if (!StepDevilProgress.IsLevelUnlocked(oneBasedLevel))
                return;

            // If the player has no lives, show the No Lives popup instead of starting
            if (StepDevilWallet.TotalLives <= 0)
            {
                OpenNoLivesPopup(oneBasedLevel);
                return;
            }

            _isDailyChallenge = false;
            _levelIndex = Mathf.Clamp(oneBasedLevel - 1, 0, StepDevilDatabase.LevelCount - 1);
            _forkIndex = 0;
            _history.Clear();
            _mirror = false;
            _mirrorCd = 0;
            PopulateWorldScreen();
            ShowScreen(ScreenId.World);
        }

        // ── No Lives Popup (level-map entry gate) ─────────────────────────

        void OpenNoLivesPopup(int oneBasedLevel)
        {
            _pendingLevelFromMap = oneBasedLevel;
            RefreshNoLivesPopup();
            if (_noLivesPopupGo != null) _noLivesPopupGo.SetActive(true);
        }

        void CloseNoLivesPopup()
        {
            _pendingLevelFromMap = -1;
            if (_noLivesPopupGo != null) _noLivesPopupGo.SetActive(false);
        }

        void RefreshNoLivesPopup()
        {
            if (_nlpWalletText != null)
                _nlpWalletText.text = $"{StepDevilWallet.Coins} coins  ·  {StepDevilWallet.Diamonds} gems";

            var hasCoins = StepDevilWallet.Coins >= StepDevilWallet.CostCoinsPerLife;
            if (_nlpBuyCoinsBtn != null)
            {
                _nlpBuyCoinsBtn.interactable = hasCoins;
                var img = _nlpBuyCoinsBtn.GetComponent<Image>();
                if (img != null) img.color = hasCoins ? (Color)StepDevilPalette.Gold : (Color)StepDevilPalette.Grey;
            }

            var hasGems = StepDevilWallet.Diamonds >= StepDevilWallet.CostDiamondsFor3Lives;
            if (_nlpBuyDiamondsBtn != null)
            {
                _nlpBuyDiamondsBtn.interactable = hasGems;
                var img = _nlpBuyDiamondsBtn.GetComponent<Image>();
                if (img != null) img.color = hasGems ? new Color32(0, 180, 220, 255) : (Color)StepDevilPalette.Grey;
            }
        }

        void OnNoLivesBuyCoins()
        {
            if (!StepDevilWallet.BuyLifeWithCoins()) return;
            RefreshTitleWalletBar();
            RefreshNoLivesPopup();
            // Auto-proceed: the player now has at least 1 life
            if (StepDevilWallet.TotalLives > 0 && _pendingLevelFromMap > 0)
            {
                var lvl = _pendingLevelFromMap;
                CloseNoLivesPopup();
                OnLevelSelectedFromMap(lvl); // TotalLives > 0, so won't re-open popup
            }
        }

        void OnNoLivesBuyDiamonds()
        {
            if (!StepDevilWallet.Buy3LivesWithDiamonds()) return;
            RefreshTitleWalletBar();
            RefreshNoLivesPopup();
            // Auto-proceed
            if (StepDevilWallet.TotalLives > 0 && _pendingLevelFromMap > 0)
            {
                var lvl = _pendingLevelFromMap;
                CloseNoLivesPopup();
                OnLevelSelectedFromMap(lvl);
            }
        }

        void OnNoLivesWatchAds()
        {
            // Simulate ad → reward coins; popup stays open so player can convert
            StepDevilWallet.AddCoins(50);
            RefreshTitleWalletBar();
            RefreshNoLivesPopup();
        }

        void PopulateWorldScreen()
        {
            var lv = StepDevilDatabase.GetLevel(_levelIndex);
            var w = StepDevilDatabase.Worlds[lv.WorldIndex];
            if (_worldNum != null)
                _worldNum.text = $"WORLD {w.Number}";
            // _worldIcon is now an Image/SDSpriteAnimator placeholder — assign sprites in the Inspector.
            if (_worldName != null)
                _worldName.text = w.Name;
            if (_worldRule != null)
                _worldRule.text = w.RuleHtml;
        }

        void BeginLevel()
        {
            EnterGameplay();
        }

        void StartLevel()
        {
            EnterGameplay();
        }

        void EnterGameplay()
        {
            ShowScreen(ScreenId.Game);
            var lv = StepDevilDatabase.GetLevel(_levelIndex);
            if (_forkIndex < 0 || _forkIndex >= lv.ForkCount)
                _forkIndex = 0;
            // Daily Challenge hides the campaign level number — the level was picked
            // for the player and its sequence number is meaningless to them here.
            if (_levelNum != null)
                _levelNum.text = _isDailyChallenge ? "DAILY" : lv.Id.ToString();
            var w = StepDevilDatabase.Worlds[lv.WorldIndex];
            if (_worldHint != null)
                _worldHint.text = w.Hint;
            _lives = StepDevilWallet.TotalLives;
            _coins = StepDevilWallet.Coins;
            ResetBlip();
            BuildPathDots();
            UpdateLives();
            UpdateCoins();
            PresentFork();
        }

        void OnGameOverPlayAgain()
        {
            if (_isDailyChallenge)
            {
                // Daily: just go back to title, don't reset main game
                _isDailyChallenge = false;
                RefreshDailyButton();
                ShowScreen(ScreenId.Title);
                return;
            }
            StartGame();
        }

        void StartGame()
        {
            _isDailyChallenge = false;
            _pendingReturnToTitle = false;
            _levelIndex = 0;
            _forkIndex = 0;
            _lives = StepDevilWallet.TotalLives;
            _coins = StepDevilWallet.Coins;
            _totalLies = 0;
            _totalFalls = 0;
            _totalLevelsCleared = 0;
            _mirror = false;
            _mirrorCd = 0;
            _history.Clear();
            RefreshDailyButton();
            RefreshTitleWalletBar();
            ShowScreen(ScreenId.Title);
        }

        void RefreshSceneLayoutForScreen(ScreenId id)
        {
            GameObject screen = null;
            switch (id)
            {
                case ScreenId.Title: screen = _titleGo; break;
                case ScreenId.LevelMap: screen = _levelMapGo; break;
                case ScreenId.World: screen = _worldGo; break;
                case ScreenId.Game: screen = _gameGo; break;
                case ScreenId.Result: screen = _resultGo; break;
                case ScreenId.Truth: screen = _truthGo; break;
                case ScreenId.GameOver: screen = _gameOverGo; break;
                case ScreenId.Complete: screen = _completeGo; break;
                case ScreenId.SpinWheel: screen = _spinWheelGo; break;
                case ScreenId.DailyRewards: screen = _dailyRewardsGo; break;
                case ScreenId.NoLives: screen = _noLivesGo; break;
                case ScreenId.Store:   screen = _storeGo;   break;
            }

            if (screen == null)
                return;

            var rt = screen.GetComponent<RectTransform>();
            if (rt == null)
                return;

            if (id == ScreenId.Game && _overrideSceneLayoutAtRuntime)
                EnsureGameScreenLayout();

            Canvas.ForceUpdateCanvases();
            if (_rootRt != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(_rootRt);
            LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
            if (id == ScreenId.LevelMap && _levelMapView != null && _levelMapView.ScrollRect != null && _levelMapView.ScrollRect.content != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(_levelMapView.ScrollRect.content);
            if (id == ScreenId.LevelMap)
                RefreshLevelMapHeader();
            if (id == ScreenId.Truth && _truthScroll != null && _truthScroll.content != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(_truthScroll.content);

            // Defensive TMP width / anchor rewrites — only when the opt-in flag is on.
            // When OFF, your Inspector-authored positions are trusted verbatim.
            if (!_overrideSceneLayoutAtRuntime)
                return;
            if (id == ScreenId.Truth)
                SanitizeTruthScreenTmpWidths();
            SanitizeStackScreenTmpWidths(screen, id == ScreenId.Game ? 358f : 300f);
            if (id == ScreenId.Game)
                ApplyGameScreenTextLayoutGuards();
        }

        void SanitizeTruthScreenTmpWidths()
        {
            const float w = 362f;
            EnsureTmpMinWidth(_truthSum, w);
            if (_truthGo == null)
                return;
            var head = _truthGo.transform.Find("Head");
            if (head != null)
                EnsureTmpMinWidth(head.GetComponent<TextMeshProUGUI>(), w);
            var sub = _truthGo.transform.Find("Sub");
            if (sub != null)
                EnsureTmpMinWidth(sub.GetComponent<TextMeshProUGUI>(), w);
        }


        void SanitizeStackScreenTmpWidths(GameObject screen, float minWidth)
        {
            if (screen == null)
                return;
            var tmps = screen.GetComponentsInChildren<TextMeshProUGUI>(true);
            for (var i = 0; i < tmps.Length; i++)
                EnsureTmpMinWidth(tmps[i], minWidth);
        }
        static void EnsureTmpMinWidth(TextMeshProUGUI tmp, float screenMinWidth)
        {
            if (tmp == null)
                return;
            var rt = tmp.rectTransform;
            var le = tmp.GetComponent<LayoutElement>();
            var targetW = screenMinWidth;
            if (le != null && le.preferredWidth > 1f)
                targetW = Mathf.Min(screenMinWidth, Mathf.Max(le.preferredWidth, le.minWidth));

            // One-character-per-line happens when width is basically 0. Do not upsize small chips/labels that are already >= usable width.
            if (rt.rect.width >= Mathf.Max(24f, targetW * 0.85f))
                return;

            var h = rt.sizeDelta.y > 4f ? rt.sizeDelta.y : Mathf.Max(48f, tmp.fontSize * 2f);
            var anchored = Mathf.Approximately(rt.anchorMin.x, rt.anchorMax.x) && Mathf.Approximately(rt.anchorMin.y, rt.anchorMax.y);
            if (anchored)
                rt.sizeDelta = new Vector2(Mathf.Max(rt.sizeDelta.x, targetW), Mathf.Max(rt.sizeDelta.y, h));
            if (le == null)
                le = tmp.gameObject.AddComponent<LayoutElement>();
            le.minWidth = Mathf.Max(le.minWidth, targetW);
            le.preferredWidth = Mathf.Max(le.preferredWidth, targetW);
            le.minHeight = Mathf.Max(le.minHeight, h);
        }


        /// <summary>Forces Game screen HUD TMP to explicit layout cells. Scene stretch TMP at 0 width wraps one glyph per line.</summary>
        void ApplyGameScreenTextLayoutGuards()
        {
            // _worldHint and _devilTipText are managed by their parent HorizontalLayoutGroup — skip ApplyTmpAsLayoutBlock for them.
            ApplyTmpAsLayoutBlock(_timerLabel,   340f, 14f);
            ApplyTmpAsLayoutBlock(_mirrorBanner, 358f, 22f);
            if (_blipText != null)
                ApplyTmpAsLayoutBlock(_blipText, 340f, 76f);
        }

        /// <summary>
        /// Scene-built Game panels often use VLG with control off, Char flexible height, and Timer anchored to the bottom — all of which
        /// stack controls on top of each other and shrink the timer track to a few pixels. This normalizes layout for code- and scene-built UI.
        /// </summary>
        void EnsureGameScreenLayout()
        {
            if (_gameGo == null)
                return;

            if (_timerFill == null)
            {
                var fillTf = _gameGo.transform.Find("TimerBar/Track/Fill")
                          ?? _gameGo.transform.Find("Body/Timer/Track/Fill")
                          ?? _gameGo.transform.Find("Timer/Track/Fill");
                if (fillTf != null)
                    _timerFill = fillTf.GetComponent<Image>();
            }

            var root = _gameGo.transform;
            var sectionOrder = new[] { "Header", "Hint", "Path", "Timer", "Char", "Stones" };
            for (var i = 0; i < sectionOrder.Length; i++)
            {
                var t = root.Find(sectionOrder[i]);
                if (t != null)
                    t.SetSiblingIndex(i);
            }

            var vlg = _gameGo.GetComponent<VerticalLayoutGroup>();
            if (vlg != null)
            {
                vlg.spacing = 10f;
                vlg.padding = new RectOffset(16, 16, 12, 16);
                vlg.childControlWidth = true;
                vlg.childControlHeight = true;
                vlg.childForceExpandWidth = true;
                vlg.childForceExpandHeight = false;
            }

            var charTf = root.Find("Char");
            if (charTf != null)
            {
                var le = charTf.GetComponent<LayoutElement>();
                if (le != null)
                {
                    le.flexibleHeight = 0f;
                    le.preferredHeight = Mathf.Max(le.preferredHeight, 100f);
                    le.minHeight = Mathf.Max(le.minHeight, 88f);
                }
            }

            if (_stonesRoot != null)
            {
                var hlg = _stonesRoot.GetComponent<HorizontalLayoutGroup>();
                if (hlg != null)
                {
                    hlg.childForceExpandWidth = false;
                    hlg.childAlignment = TextAnchor.MiddleCenter;
                }

                var sle = _stonesRoot.GetComponent<LayoutElement>();
                if (sle != null && sle.preferredHeight < 170f)
                    sle.preferredHeight = 196f;
            }

            var timerTf = root.Find("Timer");
            if (timerTf != null)
            {
                var timerRt = timerTf as RectTransform;
                if (timerRt != null)
                {
                    timerRt.anchorMin = new Vector2(0f, 1f);
                    timerRt.anchorMax = new Vector2(1f, 1f);
                    timerRt.pivot = new Vector2(0.5f, 1f);
                    timerRt.sizeDelta = new Vector2(0f, 56f);
                    timerRt.anchoredPosition = Vector2.zero;
                }

                var tle = timerTf.GetComponent<LayoutElement>();
                if (tle != null)
                {
                    tle.minWidth = 320f;
                    tle.preferredHeight = 56f;
                    tle.flexibleWidth = 1f;
                }
            }

            if (_timerLabel != null)
            {
                var le = _timerLabel.GetComponent<LayoutElement>();
                if (le != null)
                    le.enabled = true;
            }

            if (_timerFill != null)
            {
                var trackRt = _timerFill.transform.parent as RectTransform;
                if (trackRt != null)
                {
                    trackRt.anchorMin = new Vector2(0f, 0.5f);
                    trackRt.anchorMax = new Vector2(1f, 0.5f);
                    trackRt.pivot = new Vector2(0.5f, 0.5f);
                    trackRt.sizeDelta = new Vector2(0f, 12f);
                    trackRt.anchoredPosition = Vector2.zero;
                    var trackLe = trackRt.GetComponent<LayoutElement>();
                    if (trackLe != null)
                    {
                        trackLe.preferredHeight = 12f;
                        trackLe.minHeight = 8f;
                        trackLe.flexibleWidth = 1f;
                    }

                    EnsureUiImageHasSprite(trackRt.GetComponent<Image>());
                }

                var fr = _timerFill.rectTransform;
                fr.anchorMin = Vector2.zero;
                fr.anchorMax = Vector2.one;
                fr.offsetMin = Vector2.zero;
                fr.offsetMax = Vector2.zero;
                _timerFill.type = Image.Type.Filled;
                _timerFill.fillMethod = Image.FillMethod.Horizontal;
                _timerFill.fillOrigin = (int)Image.OriginHorizontal.Left;
                _timerFill.preserveAspect = false;
                EnsureUiImageHasSprite(_timerFill);
            }

            var timerPanel = root.Find("Timer");
            if (timerPanel != null)
            {
                var tv = timerPanel.GetComponent<VerticalLayoutGroup>();
                if (tv != null)
                {
                    tv.childControlWidth = true;
                    tv.childControlHeight = true;
                    tv.childForceExpandHeight = false;
                    tv.spacing = 6f;
                }
            }
        }

        /// <summary>Unity UI Images with no sprite often draw nothing (including Filled); assign a 1×1 white quad.</summary>
        static void EnsureUiImageHasSprite(Image img)
        {
            if (img == null || img.sprite != null)
                return;
            var tex = Texture2D.whiteTexture;
            img.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
        }

        static void ApplyTmpAsLayoutBlock(TextMeshProUGUI t, float w, float h)
        {
            if (t == null)
                return;
            var rt = t.rectTransform;
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(0f, 0f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(w, h);
            rt.anchoredPosition = Vector2.zero;
            var le = t.GetComponent<LayoutElement>();
            if (le == null)
                le = t.gameObject.AddComponent<LayoutElement>();
            le.minWidth = le.preferredWidth = w;
            le.minHeight = le.preferredHeight = h;
        }

#if UNITY_EDITOR
        public void EditorOnlyRebuildUi()
        {
            var capturedAnimators = !_useHierarchyFromScene ? CaptureAllSpriteAnimatorPresets() : null;

            for (var i = transform.childCount - 1; i >= 0; i--)
                DestroyImmediate(transform.GetChild(i).gameObject);

            BuildUi();

            if (!_useHierarchyFromScene)
                ApplyCodeBuiltAnimationData(capturedAnimators ?? new List<(string Path, SDSpriteAnimatorPreset Preset)>());
            else if (_titleGo != null)
                _titleDevilAnim = FindTitleDevilEmojiTransform(_titleGo.transform)?.GetComponent<SDSpriteAnimator>();

            if (Application.isPlaying)
            {
                WireBuiltUiButtons();
                ShowScreen(ScreenId.Title);
            }
        }
#endif
    }
}
