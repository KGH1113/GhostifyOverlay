using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DonQuixoteOverlay.KeyViewerContents;

namespace DonQuixoteOverlay
{
    public sealed class LayoutEditorController : MonoBehaviour
    {
        // Keep the previous 1–9 shortcuts; 0 adds the progress bar.
        private static readonly OverlayPart[] Parts =
        {
            OverlayPart.TopLeft,
            OverlayPart.TopRight,
            OverlayPart.Attempts,
            OverlayPart.Judgments,
            OverlayPart.Combo,
            OverlayPart.DetailedPerfect,
            OverlayPart.TopLeft,
            OverlayPart.SongInfo,
            OverlayPart.TimingRanges,
            OverlayPart.ProgressBar
        };
        private static readonly string[] Names =
        {
            "좌상단",
            "우상단",
            "시도 횟수",
            "누적 판정",
            "콤보",
            "누적 세부 판정",
            "키뷰어",
            "곡 정보",
            "판정 범위",
            "진행 막대"
        };
        private readonly LayoutKeyRepeat _horizontal = new LayoutKeyRepeat(), _vertical = new LayoutKeyRepeat();
        private LayoutEditSnapshot _before;
        private int _part, _shownPart = -1, _beginFrame;
        private bool _scaling;
        private GameObject _hintCanvas;
        private TMP_Text _hint;
        private Action _returnToSettings;
        internal static bool IsActive { get; private set; }
        internal int SelectedIndex => _part;
        private CustomOverlayText _custom, _customDraft;
        private TMP_Text _customPreview;
        internal RectTransform CustomPreview => _custom != null ? _customPreview?.rectTransform : null;
        internal static bool IsEditingCustom(CustomOverlayText item) => IsActive && Main.Root != null && Main.Root.GetComponent<LayoutEditorController>()?._custom == item;
        internal void BeginCustomFromSettings(CustomOverlayText item, Action returnToSettings)
        {
            if (IsActive || item == null) return;
            Begin(0);
            _custom = item;
            _customDraft = new CustomOverlayText { Id = item.Id, Text = item.Text, Color = item.Color, X = item.X, Y = item.Y, Size = item.Size };
            _returnToSettings = returnToSettings;
            if (_customPreview == null) {
                _customPreview = DarkNeonUi.Text("", _hintCanvas.transform, new Vector2(0, 1), new Vector2(0, 1), 28, item.Color, TextAlignmentOptions.TopLeft, false);
                _customPreview.rectTransform.pivot = new Vector2(0, 1);
                _customPreview.richText = false; _customPreview.raycastTarget = false;
                _customPreview.enableAutoSizing = false;
                _customPreview.overflowMode = TextOverflowModes.Overflow;
            }
            _customPreview.gameObject.SetActive(true);
            _shownPart = -1; RefreshHint(); RefreshCustomPreview();
        }
        private void RefreshCustomPreview()
        {
            if (_customDraft == null || _customPreview == null) return;
            _customPreview.text = string.IsNullOrEmpty(_customDraft.Text) ? "텍스트" : _customDraft.Text;
            _customPreview.color = _customDraft.Color; _customPreview.fontSize = _customDraft.Size;
            _customPreview.rectTransform.anchoredPosition = new Vector2(_customDraft.X, -_customDraft.Y);
            _customPreview.rectTransform.sizeDelta = _customPreview.GetPreferredValues(_customPreview.text);
        }

        internal static void CancelActive()
        {
            if (Main.Root != null)
                Main.Root.GetComponent<LayoutEditorController>()?.Cancel(false);
        }

        private void OnEnable()
        {
            SceneManager.activeSceneChanged += OnSceneChanged;
        }

        private void OnDisable()
        {
            SceneManager.activeSceneChanged -= OnSceneChanged;
            Cancel(false);
        }

        private void OnDestroy()
        {
            Cancel(false);
        }

        private void OnSceneChanged(Scene a, Scene b)
        {
            Cancel(false);
        }

        public void Begin(int part = 0)
        {
            if (IsActive)
                return;
            EnsureHint();
            _before = new LayoutEditSnapshot(LayoutStore.Current, KeyViewerStore.Settings);
            _part = Math.Max(0, Math.Min(Parts.Length - 1, part));
            _shownPart = -1;
            _beginFrame = Time.frameCount;
            _scaling = false;
            _horizontal.Reset();
            _vertical.Reset();
            IsActive = true;
            _hintCanvas.SetActive(true);
            RefreshHint();
        }

        internal void BeginFromSettings(int part, Action returnToSettings)
        {
            Begin(part);
            _returnToSettings = returnToSettings;
        }

        private void Update()
        {
            if (!IsActive || Time.frameCount <= _beginFrame)
                return;
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cancel();
                return;
            }

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                try
                {
                    if (_custom != null) {
                        float oldX = _custom.X, oldY = _custom.Y, oldSize = _custom.Size;
                        _custom.X = _customDraft.X; _custom.Y = _customDraft.Y; _custom.Size = _customDraft.Size;
                        try { SettingsStore.Save(Main.Settings); }
                        catch { _custom.X = oldX; _custom.Y = oldY; _custom.Size = oldSize; throw; }
                        End(); return;
                    }
                    LayoutStore.Save();
                    KeyViewerStore.Save();
                    End();
                }
                catch (Exception ex)
                {
                    Main.Log("Layout edit save: " + ex.Message);
                }

                return;
            }

            int selected = _part;
            for (int i = 0; i < 10; i++)
            {
                int digit = (i + 1) % 10;
                if (Input.GetKeyDown((KeyCode)((int)KeyCode.Alpha0 + digit)) || Input.GetKeyDown((KeyCode)((int)KeyCode.Keypad0 + digit)))
                    selected = i;
            }

            if (Input.GetKeyDown(KeyCode.Tab))
                selected = (selected + 1) % Parts.Length;
            if (_custom != null) selected = _part;
            bool scaling = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            if (selected != _part || scaling != _scaling)
            {
                _horizontal.Reset();
                _vertical.Reset();
            }

            _part = selected;
            _scaling = scaling;
            RefreshHint();
            double now = Time.unscaledTimeAsDouble;
            int x = _horizontal.Poll((Input.GetKey(KeyCode.RightArrow) ? 1 : 0) - (Input.GetKey(KeyCode.LeftArrow) ? 1 : 0), now);
            int y = _vertical.Poll((Input.GetKey(KeyCode.UpArrow) ? 1 : 0) - (Input.GetKey(KeyCode.DownArrow) ? 1 : 0), now);
            if (scaling)
            {
                if (y != 0)
                    Scale(y * .05f);
                return;
            }

            if (x == 0 && y == 0)
                return;
            float step = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ? 20f : 5f;
            if (_custom != null) {
                _customDraft.X += x * step; _customDraft.Y -= y * step;
                _customDraft.Normalize(); RefreshCustomPreview(); return;
            }
            if (_part == 6)
            {
                var keys = KeyViewerStore.Settings;
                keys.XLocation += x * step;
                keys.YLocation += y * step;
                KeyViewerStore.Normalize(keys);
                KeyViewer.Instance?.ApplyPosition();
            }
            else
            {
                var layout = LayoutStore.Current;
                var part = Parts[_part];
                OverlayTransforms.Set(layout, part, "X", OverlayTransforms.Get(layout, part, "X") + x * step);
                OverlayTransforms.Set(layout, part, "Y", OverlayTransforms.Get(layout, part, "Y") + y * step);
            }
        }

        private void Scale(float delta)
        {
            if (_custom != null) {
                _customDraft.Size = Mathf.Clamp(_customDraft.Size + delta * 20f, 8, 160);
                RefreshCustomPreview(); return;
            }
            if (_part == 6)
            {
                var keys = KeyViewerStore.Settings;
                keys.Size = Mathf.Clamp(keys.Size + delta, .45f, 2f);
                KeyViewer.Instance?.ApplyPosition();
            }
            else
            {
                var layout = LayoutStore.Current;
                var part = Parts[_part];
                OverlayTransforms.Set(layout, part, "Scale", OverlayTransforms.Get(layout, part, "Scale") + delta);
            }
        }

        private void End(bool returnToSettings = true)
        {
            IsActive = false;
            _custom = _customDraft = null;
            if (_customPreview != null) _customPreview.gameObject.SetActive(false);
            _before = null;
            _horizontal.Reset();
            _vertical.Reset();
            if (_hintCanvas != null)
                _hintCanvas.SetActive(false);
            var callback = _returnToSettings;
            _returnToSettings = null;
            if (returnToSettings) callback?.Invoke();
        }

        private void Cancel(bool returnToSettings = true)
        {
            if (_before == null)
                return;
            _before.Restore(LayoutStore.Current, KeyViewerStore.Settings);
            KeyViewer.Instance?.ApplyPosition();
            End(returnToSettings);
        }

        private void EnsureHint()
        {
            if (_hintCanvas != null)
                return;
            _hintCanvas = new GameObject("GhostifyOverlay.LayoutHint", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            _hintCanvas.transform.SetParent(transform, false);
            _hintCanvas.AddComponent<LayoutSelectionFrame>().Owner = this;
            var canvas = _hintCanvas.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10001;
            var scaler = _hintCanvas.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = .5f;
            var panel = DarkNeonUi.Rect("Hint", _hintCanvas.transform, new Vector2(.5f, 1), new Vector2(.5f, 1));
            var rect = panel.GetComponent<RectTransform>();
            rect.pivot = new Vector2(.5f, 1);
            rect.anchoredPosition = new Vector2(0, -22);
            rect.sizeDelta = new Vector2(1440, 108);
            DarkNeonUi.ControlSurface(panel, DQColors.Window, DQRadii.Panel, true).raycastTarget = false;
            _hint = DarkNeonUi.Text("", panel.transform, new Vector2(.02f, 0), new Vector2(.98f, 1), 19, DQColors.Text, TextAlignmentOptions.Center, true);
            _hint.raycastTarget = false;
        }

        private void RefreshHint()
        {
            if (_shownPart == _part || _hint == null)
                return;
            _shownPart = _part;
            if (_custom != null) {
                _hint.text = "커스텀 텍스트 위치·크기 편집\n방향키 이동 (길게 누르면 연속 이동) · Shift 빠르게 · Ctrl+↑↓ 글자 크기\nEnter 저장 · Esc 취소";
                return;
            }
            _hint.text = "위치·크기 편집: " + Names[_part] + "\n1 좌상단 · 2 우상단 · 3 시도 횟수 · 4 누적 판정 · 5 콤보 · 6 세부 판정 · 7 키뷰어 · 8 곡 정보 · 9 판정 범위 · 0 진행 막대" + "\nTab 다음 항목 · 방향키 이동 (길게 누르면 연속 이동) · Ctrl+↑↓ 크기 · Shift 빠르게 · Enter 저장 · Esc 취소";
        }
    }
}
