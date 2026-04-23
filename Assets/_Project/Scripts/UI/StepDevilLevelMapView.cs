using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StepDevil
{
    /// <summary>Scrollable winding path of level nodes (LiarGame-style). Wire ScrollRect, Back, optional header in the inspector.</summary>
    [DisallowMultipleComponent]
    [ExecuteAlways]
    public sealed class StepDevilLevelMapView : MonoBehaviour
    {
        [Header("Required")]
        [SerializeField] ScrollRect _scrollRect;
        [SerializeField] Button _backButton;

        [Header("Optional")]
        [SerializeField] TextMeshProUGUI _headerTitle;
        [SerializeField] TMP_FontAsset _overrideFont;
        [SerializeField] StepDevilMapStyle _style = StepDevilMapStyle.Default;

        [Header("Editor")]
        [Tooltip("Build the path in the Scene view when not playing (for layout preview).")]
        [SerializeField] bool _previewInEditor;

        Action<int> _onLevelSelected;
        Action _onBack;
        RectTransform[] _nodes;
        bool _built;
        TMP_FontAsset _font;

        public ScrollRect ScrollRect => _scrollRect;

        /// <summary>Used when the map UI is built from code instead of the inspector.</summary>
        public void SetRuntimeRefs(ScrollRect scroll, Button back, TextMeshProUGUI headerTitle = null)
        {
            _scrollRect = scroll;
            _backButton = back;
            _headerTitle = headerTitle;
        }

        public void SetOverrideFont(TMP_FontAsset font)
        {
            _overrideFont = font;
        }

        /// <summary>When UI is built from code, pin layout to the same values as SampleScene / <see cref="StepDevilMapStyle.Default"/>.</summary>
        public void SetMapStyle(StepDevilMapStyle style)
        {
            _style = style;
        }

        public void Initialize(Action onBack, Action<int> onLevelSelected)
        {
            _onBack = onBack;
            _onLevelSelected = onLevelSelected;
            EnsureFont();

            if (_backButton != null)
            {
                _backButton.onClick.RemoveAllListeners();
                _backButton.onClick.AddListener(() => _onBack?.Invoke());
            }

            if (_headerTitle != null)
            {
                _headerTitle.text = "LEVELS";
                if (_font != null)
                    _headerTitle.font = _font;
            }

            if (_scrollRect == null || _scrollRect.content == null)
            {
                Debug.LogError("[StepDevilLevelMapView] Assign Scroll Rect with Content.", this);
                return;
            }

            if (!_built || !Application.isPlaying)
                BuildMap();
        }

        void OnEnable()
        {
            if (!Application.isPlaying && _previewInEditor && _scrollRect != null && _scrollRect.content != null)
            {
                _built = false;
                BuildMap();
            }
        }

        void EnsureFont()
        {
            if (_overrideFont != null)
            {
                _font = _overrideFont;
                return;
            }

            _font = TMP_Settings.defaultFontAsset;
        }

        public void RefreshUnlocks()
        {
            if (_nodes == null)
                return;
            var max = StepDevilProgress.GetMaxUnlockedLevel();
            for (var i = 0; i < _nodes.Length; i++)
                ApplyNodeVisual(_nodes[i], i + 1, i + 1 <= max);
        }

        public void ScrollToLevel(int oneBasedLevel)
        {
            if (_scrollRect == null)
                return;
            oneBasedLevel = Mathf.Clamp(oneBasedLevel, 1, StepDevilDatabase.LevelCount);
            var n = Mathf.Max(1, StepDevilDatabase.LevelCount - 1);
            var t = (oneBasedLevel - 1) / (float)n;
            _scrollRect.verticalNormalizedPosition = Mathf.Clamp01(1f - t);
        }

        void BuildMap()
        {
            var mapContent = _scrollRect.content;
            mapContent.anchorMin = new Vector2(0.5f, 1f);
            mapContent.anchorMax = new Vector2(0.5f, 1f);
            mapContent.pivot = new Vector2(0.5f, 1f);
            mapContent.anchoredPosition = Vector2.zero;

            var n = StepDevilDatabase.LevelCount;
            mapContent.sizeDelta = new Vector2(_style.PathContentWidth, StepDevilLevelMapLayout.GetContentHeight(n, _style));

            _scrollRect.horizontal = false;
            _scrollRect.vertical = true;
            _scrollRect.movementType = ScrollRect.MovementType.Clamped;

            for (var i = mapContent.childCount - 1; i >= 0; i--)
            {
                var ch = mapContent.GetChild(i).gameObject;
                if (Application.isPlaying)
                    Destroy(ch);
                else
                    DestroyImmediate(ch);
            }

            _nodes = new RectTransform[n];
            for (var i = 0; i < n; i++)
            {
                var level = i + 1;
                var p0 = StepDevilLevelMapLayout.GetNodeAnchoredPosition(i, _style);
                var p1 = StepDevilLevelMapLayout.GetNodeAnchoredPosition(i + 1, _style);
                if (i < n - 1)
                    AddPathSegment(mapContent, p0, p1);

                var nodeRt = CreateNodeShell(mapContent, level, p0);
                _nodes[i] = nodeRt;
                ApplyNodeVisual(nodeRt, level, StepDevilProgress.IsLevelUnlocked(level));
            }

            _built = true;
        }

        void ApplyNodeVisual(RectTransform rt, int level, bool unlocked)
        {
            ClearNode(rt);

            if (unlocked)
            {
                var img = GetOrAddImage(rt);
                img.sprite = StepDevilUiPrimitives.WhiteSprite;
                img.color = StepDevilPalette.Safe;
                img.raycastTarget = true;

                var btn = GetOrAddButton(rt);
                btn.targetGraphic = img;
                btn.onClick.RemoveAllListeners();
                var captured = level;
                btn.onClick.AddListener(() =>
                {
                    if (StepDevilProgress.IsLevelUnlocked(captured))
                        _onLevelSelected?.Invoke(captured);
                });

                var labelGo = new GameObject("Num", typeof(RectTransform));
                labelGo.transform.SetParent(rt, false);
                var lrt = labelGo.GetComponent<RectTransform>();
                lrt.anchorMin = lrt.anchorMax = new Vector2(0.5f, 0.5f);
                lrt.pivot = new Vector2(0.5f, 0.5f);
                lrt.sizeDelta = rt.sizeDelta;
                lrt.anchoredPosition = Vector2.zero;
                var tx = labelGo.AddComponent<TextMeshProUGUI>();
                StepDevilTmpUtil.ApplyDefaultFont(tx, _font);
                tx.text = level.ToString();
                tx.fontSize = 20;
                tx.alignment = StepDevilTmpUtil.TextAnchorToTmp(TextAnchor.MiddleCenter);
                tx.color = StepDevilPalette.Bg;
                tx.raycastTarget = false;
                tx.fontStyle = FontStyles.Bold;
            }
            else
            {
                var img = GetOrAddImage(rt);
                img.sprite = StepDevilUiPrimitives.WhiteSprite;
                img.color = StepDevilPalette.Grey;
                img.raycastTarget = false;

                var lockGo = new GameObject("Lock", typeof(RectTransform));
                lockGo.transform.SetParent(rt, false);
                var lrt = lockGo.GetComponent<RectTransform>();
                lrt.anchorMin = lrt.anchorMax = new Vector2(0.5f, 0.5f);
                lrt.pivot = new Vector2(0.5f, 0.5f);
                lrt.sizeDelta = rt.sizeDelta;
                lrt.anchoredPosition = Vector2.zero;
                var tx = lockGo.AddComponent<TextMeshProUGUI>();
                StepDevilTmpUtil.ApplyDefaultFont(tx, _font);
                tx.text = "\u2014";
                tx.fontSize = 22;
                tx.alignment = StepDevilTmpUtil.TextAnchorToTmp(TextAnchor.MiddleCenter);
                tx.color = new Color(0.35f, 0.38f, 0.42f);
                tx.raycastTarget = false;
            }
        }

        static Image GetOrAddImage(RectTransform rt)
        {
            var img = rt.GetComponent<Image>();
            return img != null ? img : rt.gameObject.AddComponent<Image>();
        }

        static Button GetOrAddButton(RectTransform rt)
        {
            var btn = rt.GetComponent<Button>();
            return btn != null ? btn : rt.gameObject.AddComponent<Button>();
        }

        static void StretchFull(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        static void ClearNode(RectTransform rt)
        {
            for (var i = rt.childCount - 1; i >= 0; i--)
                DestroyImmediate(rt.GetChild(i).gameObject);

            var comps = rt.GetComponents<Component>();
            for (var i = comps.Length - 1; i >= 0; i--)
            {
                if (comps[i] is RectTransform)
                    continue;
                DestroyImmediate(comps[i]);
            }
        }

        void AddPathSegment(RectTransform parent, Vector2 a, Vector2 b)
        {
            var d = b - a;
            var len = Mathf.Max(8f, d.magnitude);
            var ang = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;
            var mid = (a + b) * 0.5f;
            var line = StepDevilUiPrimitives.CreateImage(parent, "Path", StepDevilPalette.NeutralDark, new Vector2(len, _style.PathSegmentThickness));
            var lineRt = line.rectTransform;
            lineRt.anchorMin = lineRt.anchorMax = new Vector2(0.5f, 1f);
            lineRt.pivot = new Vector2(0.5f, 0.5f);
            lineRt.anchoredPosition = mid;
            lineRt.localRotation = Quaternion.Euler(0f, 0f, ang);
        }

        RectTransform CreateNodeShell(RectTransform parent, int level, Vector2 pos)
        {
            var go = new GameObject("Node" + level, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = _style.LevelNodeSize;
            rt.anchoredPosition = pos;
            return rt;
        }
    }
}
