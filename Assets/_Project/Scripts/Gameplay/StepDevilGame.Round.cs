using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StepDevil
{
    public sealed partial class StepDevilGame
    {
        void ResetBlip()
        {
            if (_blipVisuals != null)
                _blipVisuals.ShowIdle();
            else if (_blipText != null)
                _blipText.text = ":|";
            SDTween.Kill(_blipRt);
            _blipRt.anchoredPosition = Vector2.zero;
            _blipRt.localScale = Vector3.one;
            _blipRt.localEulerAngles = Vector3.zero;
            var cg = _blipRt.GetComponent<CanvasGroup>();
            if (cg != null) cg.alpha = 1f;

            if (_devilAnim != null)
            {
                _devilAnim.StopAndReset();
                _devilAnim.gameObject.SetActive(false);
            }
        }

        /// <summary>Idle sprite + eased return to center (used between forks — avoids snap jitter).</summary>
        IEnumerator ResetBlipSmooth()
        {
            if (_blipVisuals != null)
                _blipVisuals.ShowIdle();
            else if (_blipText != null)
                _blipText.text = ":|";

            if (_devilAnim != null)
            {
                _devilAnim.StopAndReset();
                _devilAnim.gameObject.SetActive(false);
            }

            var cg = _blipRt.GetComponent<CanvasGroup>();
            if (cg != null)
                cg.alpha = 1f;

            const float dur = 0.38f;
            SDTween.Kill(_blipRt);
            var h = SDAnimationLibrary.BlipReset(_blipRt, dur);
            var finished = false;
            h.OnCompleteCallback(() => finished = true);
            while (!finished)
                yield return null;

            _blipRt.anchoredPosition = Vector2.zero;
            _blipRt.localScale = Vector3.one;
            _blipRt.localEulerAngles = Vector3.zero;
        }

        void BuildPathDots()
        {
            foreach (Transform c in _pathDotsRoot)
                Destroy(c.gameObject);
            _pathDots.Clear();

            var n = StepDevilDatabase.GetLevel(_levelIndex).ForkCount;
            for (var i = 0; i < n; i++)
            {
                var dot = CreateImage(_pathDotsRoot, "Dot", new Color(1f, 1f, 1f, 0.12f));
                var rt = dot.rectTransform;
                rt.sizeDelta = new Vector2(10f, 10f);
                dot.maskable = false;
                _pathDots.Add(dot);
            }

            RefreshPathDots();
        }

        void RefreshPathDots()
        {
            for (var i = 0; i < _pathDots.Count; i++)
            {
                Color32 c;
                if (i < _history.Count)
                    c = _history[i].Ok ? StepDevilPalette.Safe : StepDevilPalette.Danger;
                else if (i == _forkIndex && _history.Count == _forkIndex)
                    c = StepDevilPalette.Accent;
                else
                    c = new Color32(0xFF, 0xFF, 0xFF, 0x12);

                var active = i == _forkIndex && _history.Count == _forkIndex;
                SDAnimationLibrary.ColorReveal(_pathDots[i], c, 0.25f);
                if (active)
                    SDAnimationLibrary.PathDotPulse(_pathDots[i].rectTransform);
                else
                    SDAnimationLibrary.PathDotShrink(_pathDots[i].rectTransform);
            }
        }

        void PresentFork()
        {
            _locked = false;
            _timerStart = Time.unscaledTime;
            if (_timerFill != null)
            {
                _timerFill.fillAmount = 1f;
                SetTimerBarColorForRemainingFraction(1f);
            }

            foreach (Transform c in _stonesRoot)
                Destroy(c.gameObject);
            _stonePool.Clear();

            var lv = StepDevilDatabase.GetLevel(_levelIndex);
            var fork = lv.Forks[_forkIndex];
            for (var i = 0; i < fork.Length; i++)
            {
                var stone = fork[i];
                var w = CreateStone(stone, i);
                _stonePool.Add(w);
            }

            // Fake psychological hint — misleads the player
            if (_devilTipText != null)
            {
                var worldIndex = StepDevilDatabase.GetLevel(_levelIndex).WorldIndex;
                var tip = SDFakeHintBank.Pick(worldIndex, _forkIndex);
                var hasTip = !string.IsNullOrEmpty(tip);
                _devilTipText.text = hasTip ? tip : "";
                // tipBar is the immediate parent — show/hide it so the VLG reclaims the space
                _devilTipText.transform.parent.gameObject.SetActive(hasTip);
            }

            _mirrorBanner.gameObject.SetActive(_mirror);
            if (_mirror)
            {
                _mirrorBanner.text = $"! LEFT <-> RIGHT FLIPPED! ({_mirrorCd} left)";
                var c = _mirrorBanner.color;
                c.a = 1f;
                _mirrorBanner.color = c;
            }
            else
            {
                SDTween.Kill(_mirrorBanner);
            }

            RefreshPathDots();
            EnsureGameScreenLayout();
            if (_gameGo != null)
            {
                var grt = _gameGo.GetComponent<RectTransform>();
                if (grt != null)
                    LayoutRebuilder.ForceRebuildLayoutImmediate(grt);
            }

            if (_stonesRoot != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(_stonesRoot);
        }

        StoneWidgets CreateStone(StepDevilStoneDef stone, int index)
        {
            var go = new GameObject($"Stone_{index}", typeof(RectTransform));
            go.transform.SetParent(_stonesRoot, false);
            var le = go.AddComponent<LayoutElement>();
            le.flexibleWidth = 0f;
            le.preferredWidth = 108f;
            le.minWidth = 72f;
            le.minHeight = 156f;
            le.preferredHeight = 180f;

            var bg = go.AddComponent<Image>();
            var accent = StepDevilPalette.StoneAccent(stone.ColorKey);
            bg.color = new Color(accent.r / 255f, accent.g / 255f, accent.b / 255f, 0.08f);
            var outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(accent.r / 255f, accent.g / 255f, accent.b / 255f, 0.9f);
            outline.effectDistance = new Vector2(2f, 2f);

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = bg;
            var captured = index;
            btn.onClick.AddListener(() => OnStoneTapped(captured));

            var inner = new GameObject("Content", typeof(RectTransform));
            inner.transform.SetParent(go.transform, false);
            var innerRt = inner.GetComponent<RectTransform>();
            StretchFull(innerRt);
            var innerLe = inner.AddComponent<LayoutElement>();
            innerLe.flexibleWidth = 1f;
            innerLe.flexibleHeight = 1f;
            var vlg = inner.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(8, 8, 6, 8);
            vlg.spacing = 3f;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            var topGo = new GameObject("TopBar", typeof(RectTransform));
            topGo.transform.SetParent(inner.transform, false);
            var topRt = topGo.GetComponent<RectTransform>();
            topRt.anchorMin = new Vector2(0f, 1f);
            topRt.anchorMax = new Vector2(1f, 1f);
            topRt.pivot = new Vector2(0.5f, 1f);
            topRt.sizeDelta = new Vector2(0f, 4f);
            var topImg = topGo.AddComponent<Image>();
            topImg.color = accent;
            topImg.raycastTarget = false;
            var topBarLe = topGo.AddComponent<LayoutElement>();
            topBarLe.preferredHeight = 4f;
            topBarLe.flexibleWidth = 1f;

            // Icon is an Image+SDSpriteAnimator placeholder — assign sprites from the Inspector.
            var iconSlot = CreateAnimSlot(inner.transform, "Icon", 40f, 40f);
            iconSlot.GetComponent<Image>().color = accent;
            var iconSlotLe = iconSlot.gameObject.GetComponent<LayoutElement>();
            if (iconSlotLe != null) { iconSlotLe.preferredWidth = 40f; iconSlotLe.minWidth = 40f; iconSlotLe.preferredHeight = 40f; iconSlotLe.minHeight = 40f; }
            var icon = iconSlot;
            var label = CreateText(inner.transform, "Label", stone.Label, 11, accent, TextAnchor.MiddleCenter, true, true, null, UiTextMode.LayoutVerticalBlock, 168f,
                44f);
            var hint = CreateText(inner.transform, "Hint", "", 9, new Color(1f, 1f, 1f, 0.35f), TextAnchor.MiddleCenter, false, true, null,
                UiTextMode.LayoutVerticalBlock, 168f, 36f);

            var tag = CreateText(go.transform, "Tag", "", 9, Color.black, TextAnchor.UpperRight, false, true, null, UiTextMode.FillParent);
            var tagLe = tag.GetComponent<LayoutElement>();
            if (tagLe != null)
                Destroy(tagLe);
            tag.rectTransform.anchorMin = new Vector2(1, 1);
            tag.rectTransform.anchorMax = new Vector2(1, 1);
            tag.rectTransform.pivot = new Vector2(1, 1);
            tag.rectTransform.sizeDelta = new Vector2(72f, 22f);
            tag.rectTransform.anchoredPosition = new Vector2(-4, -4);
            tag.gameObject.SetActive(false);

            return new StoneWidgets
            {
                Root = go,
                Button = btn,
                Bg = bg,
                TopBar = topImg,
                Icon = icon,
                Label = label,
                Hint = hint,
                Tag = tag,
                Index = index
            };
        }

        void OnStoneTapped(int visualIndex)
        {
            if (_locked)
                return;
            _locked = true;

            var lv = StepDevilDatabase.GetLevel(_levelIndex);
            var fork = lv.Forks[_forkIndex];
            var actual = visualIndex;
            if (_mirror)
            {
                if (fork.Length == 2)
                    actual = 1 - visualIndex;
                else if (fork.Length == 3)
                {
                    if (visualIndex == 0)
                        actual = 2;
                    else if (visualIndex == 2)
                        actual = 0;
                }
            }

            StartCoroutine(ChoiceRoutine(actual, visualIndex, fork));
        }

        IEnumerator ChoiceRoutine(int actual, int visual, StepDevilStoneDef[] fork)
        {
            var stone = fork[actual];

            // Animate blip jumping toward chosen stone
            if (fork.Length == 2)
            {
                if (actual == 0)
                    SDAnimationLibrary.BlipJumpLeft(_blipRt, ChoiceAnimSeconds);
                else
                    SDAnimationLibrary.BlipJumpRight(_blipRt, ChoiceAnimSeconds);
            }
            else
            {
                if (actual == 0)
                    SDAnimationLibrary.BlipJumpLeft(_blipRt, ChoiceAnimSeconds);
                else if (actual == 2)
                    SDAnimationLibrary.BlipJumpRight(_blipRt, ChoiceAnimSeconds);
                else
                    SDAnimationLibrary.BlipJumpCenter(_blipRt, ChoiceAnimSeconds);
            }

            // Animate selected stone scaling up
            if (visual >= 0 && visual < _stonePool.Count)
            {
                var rt = _stonePool[visual].Root.transform as RectTransform;
                SDAnimationLibrary.StoneSelect(rt);
            }

            yield return new WaitForSecondsRealtime(ChoiceAnimSeconds);

            // Legacy: optional separate devilAnim under Game. Prefer multi-sprite blip (RevealRoutine).
            if (_blipVisuals == null && _devilAnim != null)
            {
                _devilAnim.gameObject.SetActive(true);
                _devilAnim.PlayNamedOrDefault(StepDevilEmojiAnimationNames.Happy);
            }

            yield return StartCoroutine(RevealRoutine(stone, actual, visual, fork));
        }

        IEnumerator RevealRoutine(StepDevilStoneDef stone, int actual, int visual, StepDevilStoneDef[] fork)
        {
            var safe = stone.IsSafe;
            var cLie = stone.ColorLie;
            var lLie = stone.LabelLie;
            var iLie = stone.IconLie;
            var lies = stone.LieCount;
            _totalLies += lies;

            if (visual >= 0 && visual < _stonePool.Count)
            {
                var w = _stonePool[visual];
                var stoneRt = w.Root.transform as RectTransform;
                w.Tag.gameObject.SetActive(true);
                w.Hint.color = lies > 0 ? StepDevilPalette.Danger : StepDevilPalette.Safe;
                w.Hint.text = lies > 0 ? $"{lies} LIE{(lies > 1 ? "S" : "")}!" : "HONEST";

                if (safe)
                {
                    // Smooth color transition to safe
                    var safeColor = new Color(StepDevilPalette.Safe.r / 255f, StepDevilPalette.Safe.g / 255f, StepDevilPalette.Safe.b / 255f, 0.25f);
                    SDAnimationLibrary.ColorReveal(w.Bg, safeColor);
                    w.Tag.text = stone.Type switch
                    {
                        StepDevilStoneType.Bonus => "BONUS!",
                        StepDevilStoneType.Spring => "SPRING!",
                        StepDevilStoneType.Mirror => "MIRROR!",
                        _ => "SOLID!"
                    };
                    w.Tag.color = Color.black;

                    // Correct stone: success blip sprites (or clear TMP).
                    if (_blipVisuals != null)
                        _blipVisuals.ShowCorrect();
                    else if (_blipText != null)
                        _blipText.text = "";

                    // Stone pop animation (matches CSS popSafe)
                    SDAnimationLibrary.StonePopSafe(stoneRt);

                    Flash(new Color(StepDevilPalette.Safe.r / 255f, StepDevilPalette.Safe.g / 255f, StepDevilPalette.Safe.b / 255f, 0.15f));
                    if (stone.Type == StepDevilStoneType.Bonus)
                    {
                        StepDevilWallet.AddCoins(10);
                        _coins = StepDevilWallet.Coins;
                        UpdateCoins();
                        SpawnFloatText("+10 $", StepDevilPalette.Gold);
                    }

                    if (stone.Type == StepDevilStoneType.Mirror)
                    {
                        _mirror = true;
                        _mirrorCd = 2;
                        SpawnFloatText("! FLIPPED!", StepDevilPalette.Purple);
                    }
                    else if (_mirror)
                    {
                        _mirrorCd--;
                        if (_mirrorCd <= 0)
                            _mirror = false;
                    }
                }
                else
                {
                    // Smooth color transition to danger
                    var dangerColor = new Color(StepDevilPalette.Danger.r / 255f, StepDevilPalette.Danger.g / 255f, StepDevilPalette.Danger.b / 255f, 0.25f);
                    SDAnimationLibrary.ColorReveal(w.Bg, dangerColor);
                    w.Tag.text = "VOID!";
                    w.Tag.color = Color.white;

                    // Wrong stone: wrong blip + fall (matches CSS .fall)
                    if (_blipVisuals != null)
                        _blipVisuals.ShowWrong();
                    else if (_blipText != null)
                        _blipText.text = ":(";
                    var blipCg = _blipRt.GetComponent<CanvasGroup>();
                    if (blipCg == null) blipCg = _blipRt.gameObject.AddComponent<CanvasGroup>();
                    SDAnimationLibrary.BlipFall(_blipRt, blipCg);

                    // Stone shake animation (matches CSS shakeVoid)
                    SDAnimationLibrary.StoneShakeVoid(stoneRt);

                    Flash(new Color(StepDevilPalette.Danger.r / 255f, StepDevilPalette.Danger.g / 255f, StepDevilPalette.Danger.b / 255f, 0.2f));
                    StepDevilWallet.SpendLife();
                    _lives = StepDevilWallet.TotalLives;
                    _totalFalls++;
                    if (_mirror)
                    {
                        _mirrorCd--;
                        if (_mirrorCd <= 0)
                            _mirror = false;
                    }
                }
            }

            // Fade non-chosen stones (smooth transition instead of instant)
            for (var i = 0; i < _stonePool.Count; i++)
            {
                if (i != visual)
                {
                    var cg = _stonePool[i].Root.GetComponent<CanvasGroup>();
                    if (cg == null)
                        cg = _stonePool[i].Root.AddComponent<CanvasGroup>();
                    SDAnimationLibrary.FadeCanvasGroup(cg, 0.28f, 0.3f);
                }
            }

            _history.Add(new HistoryEntry
            {
                Stone = stone,
                Ok = safe,
                Lies = lies,
                ColorLie = cLie,
                LabelLie = lLie,
                IconLie = iLie
            });

            if (safe)
                _forkIndex++;

            RefreshPathDots();
            yield return new WaitForSecondsRealtime(RevealDelaySeconds);

            if (!safe && _lives <= 0)
            {
                if (_isDailyChallenge)
                    ShowGameOver();
                else
                    ShowNoLives();
                yield break;
            }

            if (!safe)
            {
                ShowForkFail();
                yield break;
            }

            if (_forkIndex >= StepDevilDatabase.GetLevel(_levelIndex).ForkCount)
            {
                yield return ResetBlipSmooth();
                ShowLevelWin();
                yield break;
            }

            yield return ResetBlipSmooth();
            yield return new WaitForSecondsRealtime(NextForkDelaySeconds);
            PresentFork();
        }

        void ShowForkFail()
        {
            if (_resultIcon != null) _resultIcon.Play(); // sprite animation — assign frames in the Inspector
            _resultTitle.text = "YOU FELL!";
            _resultSub.text =
                "The signals were lying to you. Don't trust what looks obvious — look for signals that AGREE.";
            _resultLives.text = _lives <= 0 ? "<color=#FF4D6D>0</color>" : _lives.ToString();
            _resultRetry.gameObject.SetActive(true);
            ShowScreen(ScreenId.Result);
        }

        void OnForkChoiceTimeExpired()
        {
            if (_locked)
                return;
            _locked = true;
            StartCoroutine(ForkTimerExpiredRoutine());
        }

        IEnumerator ForkTimerExpiredRoutine()
        {
            if (_blipVisuals != null)
                _blipVisuals.ShowTimeUp();
            else if (_blipText != null)
                _blipText.text = ":(";
            var blipCg = _blipRt.GetComponent<CanvasGroup>();
            if (blipCg == null)
                blipCg = _blipRt.gameObject.AddComponent<CanvasGroup>();
            SDAnimationLibrary.BlipFall(_blipRt, blipCg);
            Flash(new Color(StepDevilPalette.Danger.r / 255f, StepDevilPalette.Danger.g / 255f, StepDevilPalette.Danger.b / 255f, 0.2f));
            StepDevilWallet.SpendLife();
            _lives = StepDevilWallet.TotalLives;
            _totalFalls++;
            UpdateLives();
            yield return new WaitForSecondsRealtime(RevealDelaySeconds);

            if (_lives <= 0)
            {
                if (_isDailyChallenge)
                    ShowGameOver();
                else
                    ShowNoLives();
                yield break;
            }

            if (_resultIcon != null) _resultIcon.Play(); // sprite animation — assign frames in the Inspector
            _resultTitle.text = "TIME'S UP!";
            _resultSub.text = "You didn't step in time — same as a fall. Try again!";
            _resultLives.text = _lives <= 0 ? "<color=#FF4D6D>0</color>" : _lives.ToString();
            _resultRetry.gameObject.SetActive(true);
            ShowScreen(ScreenId.Result);
        }

        void ShowLevelWin()
        {
            if (_isDailyChallenge)
                StepDevilDailyChallenge.MarkCompleted();
            else
                StepDevilProgress.OnLevelCompleted(StepDevilDatabase.GetLevel(_levelIndex).Id);
            _totalLevelsCleared++;
            StepDevilWallet.AddCoins(5);
            _coins = StepDevilWallet.Coins;
            UpdateCoins();
            PopulateTruthScreen();
            ShowScreen(ScreenId.Truth);
        }

        void PopulateTruthScreen()
        {
            foreach (Transform c in _truthRowsRoot)
                Destroy(c.gameObject);

            var lv = StepDevilDatabase.GetLevel(_levelIndex);
            var bonusCoins = 0;
            var falls = 0;
            var liesSum = 0;

            for (var i = 0; i < _history.Count; i++)
            {
                var h = _history[i];
                liesSum += h.Lies;
                if (!h.Ok)
                    falls++;
                if (h.Stone.Type == StepDevilStoneType.Bonus)
                    bonusCoins += 10;

                // --- Row container (horizontal: Step | Mid | Result) ---
                var row = CreatePanel(_truthRowsRoot, "Row", new Color(1f, 1f, 1f, 0.04f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.one);
                var rowLe = row.gameObject.AddComponent<LayoutElement>();
                rowLe.minHeight = 64f;
                rowLe.preferredHeight = 72f;
                var hlay = row.gameObject.AddComponent<HorizontalLayoutGroup>();
                hlay.childAlignment = TextAnchor.MiddleCenter;
                hlay.padding = new RectOffset(12, 12, 8, 8);
                hlay.spacing = 10f;
                hlay.childControlWidth = true;
                hlay.childControlHeight = true;
                hlay.childForceExpandWidth = false;
                hlay.childForceExpandHeight = true;

                // Left: Step label
                var stepLe = CreateText(row, "Step", $"Step {i + 1}", 11, StepDevilPalette.Grey, TextAnchor.MiddleLeft, false, true, null,
                    UiTextMode.LayoutVerticalBlock, 50f, 28f);
                var stepLeComp = stepLe.gameObject.GetComponent<LayoutElement>();
                if (stepLeComp == null) stepLeComp = stepLe.gameObject.AddComponent<LayoutElement>();
                stepLeComp.preferredWidth = 50f;
                stepLeComp.minWidth = 50f;

                // Middle: type + signal chips (vertical stack)
                var mid = CreatePanel(row, "Mid", new Color(0, 0, 0, 0), Vector2.zero, Vector2.one, Vector2.zero, Vector2.one);
                var midLe = mid.gameObject.AddComponent<LayoutElement>();
                midLe.flexibleWidth = 1f;
                midLe.minWidth = 120f;
                var midV = mid.gameObject.AddComponent<VerticalLayoutGroup>();
                midV.spacing = 4f;
                midV.childAlignment = TextAnchor.MiddleLeft;
                midV.childControlWidth = true;
                midV.childControlHeight = true;
                midV.childForceExpandWidth = true;
                midV.childForceExpandHeight = false;
                var typeTxt =
                    $"TRUE: {h.Stone.Type}";
                CreateText(mid, "Type", typeTxt, 12, Color.white, TextAnchor.MiddleLeft, true, true, null, UiTextMode.LayoutVerticalBlock, 160f,
                    22f);

                // Signal chips row
                var sigRow = CreatePanel(mid, "Sigs", new Color(0, 0, 0, 0), Vector2.zero, Vector2.one, Vector2.zero, Vector2.one);
                var sigLe = sigRow.gameObject.AddComponent<LayoutElement>();
                sigLe.preferredHeight = 20f;
                var shr = sigRow.gameObject.AddComponent<HorizontalLayoutGroup>();
                shr.spacing = 4f;
                shr.childForceExpandWidth = false;
                shr.childForceExpandHeight = false;
                shr.childControlWidth = true;
                shr.childControlHeight = true;
                SigChip(sigRow, "Color", h.ColorLie);
                SigChip(sigRow, "Label", h.LabelLie);
                SigChip(sigRow, "Icon", h.IconLie);

                // Right: OK/XX result
                var right = CreatePanel(row, "R", new Color(0, 0, 0, 0), Vector2.zero, Vector2.one, Vector2.zero, Vector2.one);
                var rightLe = right.gameObject.AddComponent<LayoutElement>();
                rightLe.preferredWidth = 52f;
                rightLe.minWidth = 52f;
                var rv = right.gameObject.AddComponent<VerticalLayoutGroup>();
                rv.childAlignment = TextAnchor.MiddleCenter;
                rv.childControlWidth = true;
                rv.childControlHeight = true;
                rv.childForceExpandWidth = true;
                rv.childForceExpandHeight = false;
                rv.spacing = 2f;
                CreateText(right, "O", h.Ok ? "OK" : "XX", 20, h.Ok ? StepDevilPalette.Safe : StepDevilPalette.Danger, TextAnchor.MiddleCenter, true, true, null,
                    UiTextMode.LayoutVerticalBlock, 48f, 24f);
                CreateText(right, "Ot", h.Ok ? "SAFE" : "FELL", 9, h.Ok ? StepDevilPalette.Safe : StepDevilPalette.Danger, TextAnchor.MiddleCenter,
                    true, true, null, UiTextMode.LayoutVerticalBlock, 48f, 16f);
            }

            if (_truthScroll != null && _truthScroll.content != null)
            {
                var content = _truthScroll.content;
                var cle = content.GetComponent<LayoutElement>();
                if (cle == null)
                    cle = content.gameObject.AddComponent<LayoutElement>();
                cle.minWidth = Mathf.Max(cle.minWidth, 340f);
            }

            if (_truthSum != null)
            {
                var sle = _truthSum.GetComponent<LayoutElement>();
                if (sle == null)
                    sle = _truthSum.gameObject.AddComponent<LayoutElement>();
                const float sumW = 362f;
                sle.minWidth = Mathf.Max(sle.minWidth, sumW);
                sle.preferredWidth = Mathf.Max(sle.preferredWidth, sumW);
            }

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(_truthRowsRoot);

            _truthSum.richText = true;
            _truthSum.text =
                $"<b>Level {lv.Id} — \"{lv.Name}\" Complete!</b>\n" +
                $"<color=#{ColorUtility.ToHtmlStringRGB(StepDevilPalette.Gold)}>+{5 + bonusCoins} $</color>   " +
                $"<color=#{ColorUtility.ToHtmlStringRGB(StepDevilPalette.Danger)}>{falls} fall{(falls != 1 ? "s" : "")}</color>   " +
                $"<color=#888888>{liesSum} lie{(liesSum != 1 ? "s" : "")} found</color>";

            if (_truthGo != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(_truthGo.GetComponent<RectTransform>());
            SanitizeTruthScreenTmpWidths();
            if (_truthGo != null)
                SanitizeStackScreenTmpWidths(_truthGo, 300f);
            if (_truthSum != null)
                _truthSum.ForceMeshUpdate(true);
        }

        void SigChip(RectTransform parent, string name, bool lie)
        {
            var bgColor = lie ? new Color(1f, 0.1f, 0.15f, 0.18f) : new Color(0.1f, 0.8f, 0.5f, 0.18f);
            var go = CreatePanel(parent, "Sig", bgColor, Vector2.zero, Vector2.one, Vector2.zero, Vector2.one);
            var chipLe = go.gameObject.AddComponent<LayoutElement>();
            chipLe.preferredHeight = 18f;
            chipLe.preferredWidth = 56f;
            chipLe.minWidth = 48f;
            var chipV = go.gameObject.AddComponent<VerticalLayoutGroup>();
            chipV.childAlignment = TextAnchor.MiddleCenter;
            chipV.childControlWidth = true;
            chipV.childControlHeight = true;
            chipV.childForceExpandWidth = true;
            chipV.childForceExpandHeight = true;
            chipV.padding = new RectOffset(2, 2, 1, 1);
            var t = lie ? StepDevilPalette.Danger : StepDevilPalette.Safe;
            CreateText(go, "T", $"{name} {(lie ? "LIE" : "OK")}", 8, t, TextAnchor.MiddleCenter, true, true, null, UiTextMode.LayoutVerticalBlock, 52f,
                16f);
        }

        void AfterTruth()
        {
            if (_isDailyChallenge)
            {
                // Daily challenge done — check chest before returning
                _isDailyChallenge = false;
                if (_totalLevelsCleared % 3 == 0)
                {
                    _pendingReturnToTitle = true;
                    ShowChest();
                    return;
                }
                RefreshDailyButton();
                RefreshTitleWalletBar();
                ShowScreen(ScreenId.Title);
                return;
            }

            // Every 3 levels cleared — show the chest first
            if (_totalLevelsCleared % 3 == 0)
            {
                ShowChest();
                return;
            }

            AdvanceToNextLevel();
        }

        void AdvanceToNextLevel()
        {
            var prevWorldIndex = StepDevilDatabase.GetLevel(_levelIndex).WorldIndex;
            _levelIndex++;
            if (_levelIndex >= StepDevilDatabase.LevelCount)
            {
                ShowComplete();
                return;
            }

            _forkIndex = 0;
            _history.Clear();
            _mirror = false;
            _mirrorCd = 0;

            var nextLevel = StepDevilDatabase.GetLevel(_levelIndex);
            if (nextLevel.WorldIndex != prevWorldIndex && nextLevel.Id >= StepDevilProgress.GetMaxUnlockedLevel())
            {
                PopulateWorldScreen();
                ShowScreen(ScreenId.World);
            }
            else
            {
                StartLevel();
            }
        }

        void ShowChest()
        {
            // Pick reward: alternate between coins and extra life based on cleared count
            var milestone = _totalLevelsCleared / 3; // 1, 2, 3, 4...
            if (milestone % 3 == 0)
            {
                // Every 3rd chest (3, 6, 9 levels) → extra life
                _pendingReward = ChestRewardType.ExtraLife;
                _pendingRewardAmount = 1;
            }
            else
            {
                // Otherwise coins — increases with milestone
                _pendingReward = ChestRewardType.Coins;
                _pendingRewardAmount = milestone % 2 == 0 ? 50 : 25;
            }

            PopulateChestScreen();
            ShowScreen(ScreenId.Chest);
            PlayChestOpenAnimation();
        }

        void PopulateChestScreen()
        {
            // Re-enable claim button (may have been disabled by previous OnChestClaim)
            if (_chestClaimButton != null) _chestClaimButton.interactable = true;

            // _chestEmoji is now an Image placeholder — assign chest sprite in the Inspector.

            if (_pendingReward == ChestRewardType.ExtraLife)
            {
                if (_chestRewardValue != null) _chestRewardValue.text = "+1";
                if (_chestRewardLabel != null) _chestRewardLabel.text = "EXTRA LIFE";
                if (_chestTitle != null) _chestTitle.text = "Extra life earned!";
            }
            else
            {
                if (_chestRewardValue != null) _chestRewardValue.text = $"+{_pendingRewardAmount}";
                if (_chestRewardLabel != null) _chestRewardLabel.text = "COINS";
                if (_chestTitle != null) _chestTitle.text = "You earned coins!";
            }
        }

        void PlayChestOpenAnimation()
        {
            if (_chestEmoji == null) return;
            var rt = _chestEmoji.rectTransform;
            // Bounce-open animation — assign open/closed chest sprites to the Image in the Inspector
            rt.localScale = Vector3.one * 0.5f;
            SDTween.To(0.4f, t =>
            {
                if (!rt) return;
                rt.localScale = Vector3.one * Mathf.LerpUnclamped(0.5f, 1f, t);
            }, rt)
            .SetEase(SDEase.OutBack)
            .OnCompleteCallback(() =>
            {
                // Sprite swap: assign your "open chest" sprite to _chestEmoji.sprite at runtime or in the Inspector.
                SDTween.To(0.2f, t =>
                {
                    if (!rt) return;
                    rt.localScale = Vector3.one * Mathf.LerpUnclamped(1.15f, 1f, t);
                }, rt).SetEase(SDEase.OutQuad);
            });
        }

        void OnChestClaim()
        {
            // Prevent double-tap: disable the button immediately
            if (_chestClaimButton != null) _chestClaimButton.interactable = false;

            // Apply the reward immediately (wallet changes happen now)
            if (_pendingReward == ChestRewardType.ExtraLife)
            {
                StepDevilWallet.AddBonusLives(_pendingRewardAmount);
                _lives = StepDevilWallet.TotalLives;
                UpdateLives();
            }
            else
            {
                StepDevilWallet.AddCoins(_pendingRewardAmount);
                _coins = StepDevilWallet.Coins;
                UpdateCoins();
            }
            RefreshTitleWalletBar();

            // Capture navigation intent before the async callback fires
            var returnToTitle = _pendingReturnToTitle;
            _pendingReturnToTitle = false;

            // Navigate ONLY after the celebration animation has fully finished —
            // previously AdvanceToNextLevel() ran immediately, starting gameplay
            // while the overlay animation was still playing on top.
            ShowRewardCelebration(() =>
            {
                if (returnToTitle)
                {
                    RefreshDailyButton();
                    ShowScreen(ScreenId.Title);
                }
                else if (_levelIndex < StepDevilDatabase.LevelCount)
                    AdvanceToNextLevel();
                else
                    ShowScreen(ScreenId.Title);
            });
        }

        void RetryLevel()
        {
            _forkIndex = 0;
            _mirror = false;
            _mirrorCd = 0;
            _history.Clear();
            BeginLevel();
        }

        // ── No Lives Screen ────────────────────────────────────────────────

        void ShowNoLives()
        {
            _lives = StepDevilWallet.TotalLives; // already 0, but sync
            PopulateNoLivesScreen();
            ShowScreen(ScreenId.NoLives);
        }

        void PopulateNoLivesScreen()
        {
            if (_noLivesInfoText != null)
                _noLivesInfoText.text = $"Daily lives: {StepDevilWallet.DailyLivesRemaining}/{StepDevilWallet.DailyLivesMax}  \u00B7  Bonus lives: {StepDevilWallet.BonusLives}";
            if (_noLivesWalletText != null)
                _noLivesWalletText.text = $"{StepDevilWallet.Coins} coins  ·  {StepDevilWallet.Diamonds} gems";

            var hasCoins = StepDevilWallet.Coins >= StepDevilWallet.CostCoinsPerLife;
            if (_noLivesBuyCoinsBtn != null)
            {
                _noLivesBuyCoinsBtn.interactable = hasCoins;
                var img = _noLivesBuyCoinsBtn.GetComponent<UnityEngine.UI.Image>();
                if (img != null) img.color = hasCoins ? (Color)StepDevilPalette.Gold : (Color)StepDevilPalette.Grey;
            }

            var hasGems = StepDevilWallet.Diamonds >= StepDevilWallet.CostDiamondsFor3Lives;
            if (_noLivesBuyDiamondsBtn != null)
            {
                _noLivesBuyDiamondsBtn.interactable = hasGems;
                var img = _noLivesBuyDiamondsBtn.GetComponent<UnityEngine.UI.Image>();
                if (img != null) img.color = hasGems ? new Color32(0, 180, 220, 255) : (Color)StepDevilPalette.Grey;
            }

            var hasLives = StepDevilWallet.TotalLives > 0;
            if (_noLivesRetryBtn != null)
            {
                _noLivesRetryBtn.interactable = hasLives;
                var img = _noLivesRetryBtn.GetComponent<UnityEngine.UI.Image>();
                if (img != null) img.color = hasLives ? (Color)StepDevilPalette.Accent : (Color)StepDevilPalette.Grey;
            }
        }

        void OnBuyLifeWithCoins()
        {
            if (!StepDevilWallet.BuyLifeWithCoins()) return;
            _lives = StepDevilWallet.TotalLives;
            _coins = StepDevilWallet.Coins;
            UpdateLives();
            UpdateCoins();
            RefreshTitleWalletBar();
            PopulateNoLivesScreen();
        }

        void OnBuy3LivesWithDiamonds()
        {
            if (!StepDevilWallet.Buy3LivesWithDiamonds()) return;
            _lives = StepDevilWallet.TotalLives;
            _coins = StepDevilWallet.Coins;
            UpdateLives();
            UpdateCoins();
            RefreshTitleWalletBar();
            PopulateNoLivesScreen();
        }

        void OnRetryFromNoLives()
        {
            _lives = StepDevilWallet.TotalLives;
            if (_lives <= 0) { PopulateNoLivesScreen(); return; }
            UpdateLives();
            RetryLevel();
        }

        void OnCloseNoLives()
        {
            RefreshTitleWalletBar();
            ShowScreen(ScreenId.Title);
        }

        // ── Spin Wheel ─────────────────────────────────────────────────────

        void PopulateSpinScreen()
        {
            // Update wallet labels inside the spin screen
            if (_spinWheelGo == null) return;
            var walletRow = _spinWheelGo.transform.Find("Content/WalletRow");
            if (walletRow != null)
            {
                // "Coins" and "Diamonds" are now icon+text containers; text is in child "Lbl"
                var coinsT = walletRow.Find("Coins/Lbl")?.GetComponent<TextMeshProUGUI>();
                var gemsT  = walletRow.Find("Diamonds/Lbl")?.GetComponent<TextMeshProUGUI>();
                if (coinsT != null) coinsT.text = StepDevilWallet.Coins.ToString();
                if (gemsT  != null) gemsT.text  = StepDevilWallet.Diamonds.ToString();
            }

            var canSpin = StepDevilSpinWheel.CanSpinToday();
            if (_spinActionLabel != null)
                _spinActionLabel.text = canSpin ? "SPIN!" : "ALREADY CLAIMED";
            if (_spinActionButton != null)
            {
                _spinActionButton.interactable = canSpin && !_spinWheelIsSpinning;
                var img = _spinActionButton.GetComponent<UnityEngine.UI.Image>();
                if (img != null) img.color = canSpin ? (Color)StepDevilPalette.Gold : (Color)StepDevilPalette.Grey;
            }
            if (_spinResultText != null)
                _spinResultText.text = canSpin ? "Tap SPIN to win!" : "Come back tomorrow!";

            // Reset wheel rotation
            if (_spinWheelContainer != null)
                _spinWheelContainer.localRotation = Quaternion.identity;
        }

        void OnSpinButtonPressed()
        {
            if (_spinWheelIsSpinning) return;

            if (_spinWheelPendingClaim)
            {
                ClaimSpinPrize();
                return;
            }

            if (!StepDevilSpinWheel.CanSpinToday()) return;

            _spinWheelIsSpinning = true;
            _spinWheelPendingClaim = false;

            if (_spinActionButton != null) _spinActionButton.interactable = false;

            var prizeIdx = StepDevilSpinWheel.PickPrize();
            var targetAngle = -(360f * 5f - prizeIdx * 45f);

            SDTween.To(3.0f, t =>
            {
                if (_spinWheelContainer == null) return;
                var angle = Mathf.LerpUnclamped(0f, targetAngle, t);
                _spinWheelContainer.localRotation = Quaternion.Euler(0f, 0f, angle);
            }, _spinWheelContainer)
            .SetEase(SDEase.OutCubic)
            .OnCompleteCallback(OnSpinAnimationComplete);
        }

        void OnSpinAnimationComplete()
        {
            _spinWheelIsSpinning = false;
            _spinWheelPendingClaim = true;

            var prize = StepDevilSpinWheel.Prizes[StepDevilSpinWheel.TodayPrizeIndex()];
            if (_spinResultText != null)
                _spinResultText.text = $"You won: {prize.Label}!";
            if (_spinActionLabel != null)
                _spinActionLabel.text = "CLAIM!";
            if (_spinActionButton != null)
            {
                _spinActionButton.interactable = true;
                var img = _spinActionButton.GetComponent<UnityEngine.UI.Image>();
                if (img != null) img.color = (Color)StepDevilPalette.Gold;
            }
        }

        void ClaimSpinPrize()
        {
            _spinWheelPendingClaim = false;
            ShowRewardCelebration();
            var prize = StepDevilSpinWheel.ClaimSpin();

            _coins = StepDevilWallet.Coins;
            UpdateCoins();
            RefreshTitleWalletBar();
            RefreshSpinButton();

            if (_spinResultText != null)
                _spinResultText.text = $"Claimed: {prize.Label}!";
            if (_spinActionLabel != null)
                _spinActionLabel.text = "CLAIMED";
            if (_spinActionButton != null)
            {
                _spinActionButton.interactable = false;
                var img = _spinActionButton.GetComponent<UnityEngine.UI.Image>();
                if (img != null) img.color = (Color)StepDevilPalette.Grey;
            }

            // Update wallet row in spin screen
            if (_spinWheelGo != null)
            {
                var walletRow = _spinWheelGo.transform.Find("Content/WalletRow");
                if (walletRow != null)
                {
                    var coinsT = walletRow.Find("Coins/Lbl")?.GetComponent<TextMeshProUGUI>();
                    var gemsT  = walletRow.Find("Diamonds/Lbl")?.GetComponent<TextMeshProUGUI>();
                    if (coinsT != null) coinsT.text = StepDevilWallet.Coins.ToString();
                    if (gemsT  != null) gemsT.text  = StepDevilWallet.Diamonds.ToString();
                }
            }
        }

        void OnCloseSpinWheel()
        {
            // If mid-spin animation, let it finish but we'll navigate away
            _spinWheelIsSpinning = false;
            _spinWheelPendingClaim = false;
            RefreshSpinButton();
            RefreshTitleWalletBar();
            ShowScreen(ScreenId.Title);
        }

        // ── Daily Rewards Screen ───────────────────────────────────────────

        void PopulateDailyRewardsScreen()
        {
            if (_dailyRewardsDaysRoot == null) return;

            foreach (Transform c in _dailyRewardsDaysRoot)
                Destroy(c.gameObject);

            // Build two rows: days 0-3 (row 1) and days 4-6 (row 2)
            BuildDayRow(_dailyRewardsDaysRoot, 0, 4);
            BuildDayRow(_dailyRewardsDaysRoot, 4, 7);
        }

        void BuildDayRow(RectTransform parent, int from, int to)
        {
            var rowGo = new GameObject($"Row{from}", typeof(RectTransform));
            rowGo.transform.SetParent(parent, false);
            var rowRt = rowGo.GetComponent<RectTransform>();
            rowRt.anchorMin = rowRt.anchorMax = new Vector2(0.5f, 0f);
            rowRt.sizeDelta = new Vector2(342f, 100f);
            var hlay = rowGo.AddComponent<HorizontalLayoutGroup>();
            hlay.childAlignment = TextAnchor.MiddleCenter;
            hlay.spacing = 7f;
            hlay.childForceExpandWidth = false;
            hlay.childForceExpandHeight = true;
            rowGo.AddComponent<LayoutElement>().preferredHeight = 100f;

            for (int i = from; i < to; i++)
                BuildDayCard(rowRt, i);
        }

        void BuildDayCard(RectTransform parent, int dayIndex)
        {
            var (def, status) = StepDevilDailyReward.GetDayInfo(dayIndex);

            Color32 bgColor;
            Color32 textColor;
            switch (status)
            {
                case DayStatus.Claimed:   bgColor = new Color32(40, 40, 60, 255);   textColor = StepDevilPalette.Grey; break;
                case DayStatus.Available: bgColor = new Color32(60, 40, 100, 255);  textColor = Color.white; break;
                default:                  bgColor = new Color32(22, 18, 36, 255);   textColor = new Color32(60, 60, 80, 255); break;
            }

            var cardGo = new GameObject($"Day{dayIndex + 1}", typeof(RectTransform));
            cardGo.transform.SetParent(parent, false);
            var cardRt = cardGo.GetComponent<RectTransform>();
            cardRt.anchorMin = cardRt.anchorMax = new Vector2(0f, 0f);
            cardRt.sizeDelta = new Vector2(78f, 96f);
            var cardImg = cardGo.AddComponent<Image>();
            cardImg.color = bgColor;
            var cardV = cardGo.AddComponent<VerticalLayoutGroup>();
            cardV.childAlignment = TextAnchor.MiddleCenter;
            cardV.spacing = 2f;
            cardV.padding = new RectOffset(3, 3, 6, 4);
            cardV.childForceExpandWidth = true;
            cardV.childForceExpandHeight = false;
            cardGo.AddComponent<LayoutElement>().preferredWidth = 78f;

            // Day label
            var dayTxt = CreateText(cardRt, "Day", $"DAY {dayIndex + 1}", 8, textColor,
                TextAnchor.MiddleCenter, true, false, null, UiTextMode.LayoutVerticalBlock, 72f, 14f);

            // Reward icon: Image placeholder — assign reward sprite in Inspector
            Color iconTint = status == DayStatus.Locked ? new Color(0.3f, 0.3f, 0.4f) : Color.white;
            var rewardImg = CreateImgSlot(cardRt, "RewardIco", iconTint, 28f, 28f);
            rewardImg.gameObject.GetComponent<LayoutElement>().preferredHeight = 30f;

            // Amount label
            CreateText(cardRt, "Amt", status == DayStatus.Locked ? "???" : def.Label, 8, textColor,
                TextAnchor.MiddleCenter, false, false, null, UiTextMode.LayoutVerticalBlock, 72f, 14f);

            // Status button / badge
            if (status == DayStatus.Available)
            {
                var claimBtn = CreateButton(cardRt, "CLAIM", new Color32(100, 60, 200, 255), OnClaimDailyReward);
                var claimLbl = claimBtn.GetComponentInChildren<TextMeshProUGUI>();
                if (claimLbl != null) claimLbl.fontSize = 9f;
                claimBtn.gameObject.AddComponent<LayoutElement>().preferredHeight = 22f;
            }
            else
            {
                var badgeTxt = CreateText(cardRt, "Badge",
                    status == DayStatus.Claimed ? "DONE" : "LOCKED",
                    8, textColor, TextAnchor.MiddleCenter, false, false, null, UiTextMode.LayoutVerticalBlock, 72f, 18f);
            }
        }

        void OnClaimDailyReward()
        {
            if (!StepDevilDailyReward.ClaimToday()) return;

            ShowRewardCelebration();

            _coins = StepDevilWallet.Coins;
            UpdateCoins();
            RefreshTitleWalletBar();

            // Update the daily rewards tile — icon is now an Image placeholder; just grey the button
            if (_dailyRewardsButton != null)
            {
                var img = _dailyRewardsButton.GetComponent<UnityEngine.UI.Image>();
                if (img != null) img.color = (Color)StepDevilPalette.Grey;
            }

            PopulateDailyRewardsScreen();
        }

        void OnCloseDailyRewards()
        {
            RefreshTitleWalletBar();
            ShowScreen(ScreenId.Title);
        }

        void ShowGameOver()
        {
            var lv = StepDevilDatabase.GetLevel(_levelIndex);
            _goLevel.text = $"You made it to Level <b>{lv.Id}</b>";
            _goLevel.richText = true;
            _goCoins.text = $"{_coins} coins collected";
            ShowScreen(ScreenId.GameOver);
        }

        void ShowComplete()
        {
            _completeCoins.text = _coins.ToString();
            _completeLies.text = _totalLies.ToString();
            _completeFalls.text = _totalFalls.ToString();
            StartCoroutine(ConfettiRoutine());
            ShowScreen(ScreenId.Complete);
        }

        IEnumerator ConfettiRoutine()
        {
            foreach (Transform c in _confettiRoot)
                Destroy(c.gameObject);
            var cols = new[]
            {
                StepDevilPalette.Accent, StepDevilPalette.Safe, StepDevilPalette.Gold, StepDevilPalette.Blue, StepDevilPalette.Purple,
                StepDevilPalette.Orange
            };
            for (var i = 0; i < 35; i++)
            {
                var img = CreateImage(_confettiRoot, "c", cols[i % cols.Length]);
                var rt = img.rectTransform;
                rt.anchorMin = rt.anchorMax = new Vector2(UnityEngine.Random.value, 1f);
                rt.sizeDelta = new Vector2(8f, 8f);
                rt.anchoredPosition = new Vector2(0, 10f);

                // Animate each confetti dot falling (matches CSS cfall)
                var delay = UnityEngine.Random.value * 2f;
                var dur = 2f + UnityEngine.Random.value * 2.5f;
                SDAnimationLibrary.ConfettiFall(rt, 730f, dur)
                    .SetDelay(delay)
                    .OnCompleteCallback(() => { if (rt) Destroy(rt.gameObject); });
            }

            yield break;
        }

        void ShowScreen(ScreenId id)
        {
            ActivateScreen(_titleGo, id == ScreenId.Title);
            ActivateScreen(_levelMapGo, id == ScreenId.LevelMap);
            ActivateScreen(_worldGo, id == ScreenId.World);
            ActivateScreen(_gameGo, id == ScreenId.Game);
            ActivateScreen(_resultGo, id == ScreenId.Result);
            ActivateScreen(_truthGo, id == ScreenId.Truth);
            ActivateScreen(_chestGo, id == ScreenId.Chest);
            ActivateScreen(_gameOverGo, id == ScreenId.GameOver);
            ActivateScreen(_completeGo, id == ScreenId.Complete);
            ActivateScreen(_spinWheelGo, id == ScreenId.SpinWheel);
            ActivateScreen(_dailyRewardsGo, id == ScreenId.DailyRewards);
            ActivateScreen(_noLivesGo, id == ScreenId.NoLives);
            ActivateScreen(_storeGo,   id == ScreenId.Store);

            if (id == ScreenId.Game)
                _timerStart = Time.unscaledTime;

            // Scene-authored rows often lay out at 0 width while inactive; fix after the target screen is active.
            RefreshSceneLayoutForScreen(id);
            if (id == ScreenId.Game && _stonesRoot != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(_stonesRoot);

            // Start devil bob on title screen emoji
            if (id == ScreenId.Title)
                StartDevilBob();
            if (id == ScreenId.Truth)
                TrySpawnHappyDevilPopup();
        }

        /// <summary>Activates a screen with fade-in transition.</summary>
        void ActivateScreen(GameObject go, bool show)
        {
            if (go == null) return;
            if (show && !go.activeSelf)
            {
                go.SetActive(true);
                var cg = go.GetComponent<CanvasGroup>();
                if (cg == null) cg = go.AddComponent<CanvasGroup>();
                SDAnimationLibrary.ScreenFadeIn(cg);
            }
            else if (!show && go.activeSelf)
            {
                go.SetActive(false);
            }
        }

        void StartDevilBob()
        {
            if (_titleGo == null) return;
            var anim = _titleDevilAnim;
            if (anim == null)
                anim = FindTitleDevilEmojiTransform(_titleGo.transform)?.GetComponent<SDSpriteAnimator>();

            if (anim == null)
                return;

            var rt = anim.transform as RectTransform;
            if (rt != null)
                SDTween.Kill(rt);

            anim.gameObject.SetActive(true);
            anim.PlayNamedOrDefault(StepDevilEmojiAnimationNames.Idle);
        }

        void UpdateLives()
        {
            if (_livesText == null) return;
            _livesText.text = _lives <= 0 ? "<color=#FF4D6D>0</color>" : _lives.ToString();
        }

        void UpdateCoins()
        {
            if (_coinsText == null) return;
            _coinsText.text = _coins.ToString();
        }

        void Flash(Color c)
        {
            if (_flashOverlay == null)
                return;
            // Tween-based flash (matches CSS flashOv .fgreen/.fred)
            SDAnimationLibrary.FlashScreen(_flashOverlay, c);
        }

        /// <summary>Floating feedback: prefer Happy devil sprite FX (same art as gameplay devil); fallback to TMP if no frames.</summary>
        void SpawnFloatText(string msg, Color32 color)
        {
            if (TrySpawnHappyDevilPopup())
                return;

            var t = CreateText(_rootRt, "Float", msg, 22, color, TextAnchor.MiddleCenter, true, true, null, UiTextMode.FillParent);
            var le = t.GetComponent<LayoutElement>();
            if (le != null)
                Destroy(le);
            t.transform.SetAsLastSibling();
            var rt = t.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.55f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(380f, 48f);
            rt.anchoredPosition = Vector2.zero;
            SDAnimationLibrary.FloatTextUp(rt, 100f, 0.9f);
        }

        bool TrySpawnHappyDevilPopup()
        {
            var src = _blipVisuals != null && _blipVisuals.CorrectAnimator != null
                ? _blipVisuals.CorrectAnimator
                : (_devilAnim != null ? _devilAnim : _titleDevilAnim);
            if (src == null || !src.TryGetFramesForHappyOrDefault(out var frames, out var fps, out var loop, out var pingPong))
                return false;

            var go = new GameObject("HappyDevilFx", typeof(RectTransform));
            go.transform.SetParent(_rootRt, false);
            go.transform.SetAsLastSibling();
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.55f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(112f, 112f);
            rt.anchoredPosition = Vector2.zero;
            var img = go.AddComponent<Image>();
            img.color = Color.white;
            img.preserveAspect = true;
            var anim = go.AddComponent<SDSpriteAnimator>();
            anim.Play(frames, fps > 0f ? fps : 12f, loop, pingPong);
            SDAnimationLibrary.FloatTextUp(rt, 100f, 0.9f);
            return true;
        }

        // ── Reward Celebration Overlay ─────────────────────────────────────

        /// <summary>
        /// Shows the reward-celebration sprite animation overlay for ~2.5 s then fades it out.
        /// Call this from any "Claim" handler.
        /// </summary>
        /// <param name="onComplete">Called after the overlay has fully faded out. Use this to
        /// start gameplay or navigate — guarantees the animation never plays over the next screen.</param>
        void ShowRewardCelebration(System.Action onComplete = null)
        {
            EnsureRewardCelebrationOverlay();
            if (_rewardCelebrationGo == null)
            {
                // No overlay available — invoke immediately so the caller never gets stuck
                onComplete?.Invoke();
                return;
            }

            // Re-apply preset in case sprites were assigned after the overlay was first created
            if (_rewardCelebrationPreset != null && _rewardCelebrationPreset.HasContent && _rewardCelebrationAnim != null)
                _rewardCelebrationAnim.ImportPreset(_rewardCelebrationPreset);

            if (_rewardCelebrationAnim != null)
                _rewardCelebrationAnim.Play();

            var cg = _rewardCelebrationCg;
            SDTween.Kill(cg);
            cg.alpha = 0f;
            cg.blocksRaycasts = true; // block taps during celebration

            // Fade in → hold 2 s → fade out → invoke onComplete
            SDTween.Fade(cg, 1f, 0.25f)
                .OnCompleteCallback(() =>
                {
                    SDTween.DelayedCall(2f, () => HideRewardCelebration(onComplete));
                });
        }

        void HideRewardCelebration(System.Action onComplete = null)
        {
            if (_rewardCelebrationCg == null)
            {
                onComplete?.Invoke();
                return;
            }
            var cg = _rewardCelebrationCg;
            SDTween.Kill(cg);
            SDTween.Fade(cg, 0f, 0.3f)
                .OnCompleteCallback(() =>
                {
                    if (_rewardCelebrationAnim != null) _rewardCelebrationAnim.Stop();
                    cg.blocksRaycasts = false;
                    onComplete?.Invoke(); // navigate / start gameplay only now
                });
        }
    }
}
