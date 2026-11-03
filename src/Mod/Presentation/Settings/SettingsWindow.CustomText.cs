using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace DonQuixoteOverlay
{
    public sealed partial class SettingsWindow
    {
        private GameObject _customRows;
        private readonly List<GameObject> _customCards = new List<GameObject>();
        private readonly List<Action> _customRefresh = new List<Action>();
        private void BuildCustomTextSettings()
        {
            Heading("6 · 커스텀 텍스트");
            Toggle("커스텀 텍스트 그룹 표시", () => Main.Settings.Overlay.Group6Enabled, v => Main.Settings.Overlay.Group6Enabled = v);
            _customRows = Row(1);
            RebuildCustomRows();
            Button("+", () => {
                if (Main.Settings.Overlay.CustomTexts.Count >= 50) return;
                Main.Settings.Overlay.CustomTexts.Add(new CustomOverlayText { Text = "text" });
                Main.RequestSave(); RebuildCustomRows();
            });
        }
        private void RebuildCustomRows()
        {
            ClosePalette();
            foreach (var refresh in _customRefresh) _refresh.Remove(refresh);
            _customRefresh.Clear();
            foreach (var child in _customCards) { child.SetActive(false); Destroy(child); }
            _customCards.Clear();
            var items = Main.Settings.Overlay.CustomTexts;
            float height = Math.Max(1, items.Count * 76);
            _customRows.GetComponent<LayoutElement>().preferredHeight = height;
            for (int i = 0; i < items.Count; i++) {
                var item = items[i];
                var card = DarkNeonUi.Rect("Custom text row", _customRows.transform, new Vector2(0, 1), Vector2.one);
                _customCards.Add(card);
                var rect = (RectTransform)card.transform; rect.pivot = new Vector2(.5f, 1);
                rect.sizeDelta = new Vector2(0, 66); rect.anchoredPosition = new Vector2(0, -i * 76);
                DarkNeonUi.ControlSurface(card, DQColors.Card, DQRadii.Control, true);
                Button toggle = null;
                toggle = DarkNeonUi.ToggleCard("", card.transform, new Vector2(.015f,.15f), new Vector2(.11f,.85f), item.Enabled, () => {
                    item.Enabled = !item.Enabled; toggle.GetComponent<DQToggleVisual>().SetValue(item.Enabled); Main.RequestSave();
                });
                var input = DarkNeonUi.Input("Custom text", card.transform, new Vector2(.125f,.13f), new Vector2(.55f,.87f), "text", false);
                input.lineType = TMP_InputField.LineType.MultiLineNewline; input.characterLimit = 2048; input.SetTextWithoutNotify(item.Text);
                input.onEndEdit.AddListener(value => { item.Text = value; Main.RequestSave(); });
                DarkNeonUi.Button("위치·크기 조절", card.transform, new Vector2(.57f,.17f), new Vector2(.83f,.83f), UiButtonKind.Secondary, () => {
                    ClosePalette(); StopCapture(); SetVisible(false);
                    Main.Root.GetComponent<LayoutEditorController>().BeginCustomFromSettings(item, () => SetVisible(true));
                });
                var swatch = DarkNeonUi.Button("", card.transform, new Vector2(.85f,.23f), new Vector2(.91f,.77f), UiButtonKind.Secondary, () => {
                    ClosePalette(); StopCapture();
                    _palette = ColorPalettePanel.Open(_canvas.transform, "텍스트 색상", item.Color, color => {
                        item.Color = color; Main.RequestSave(); foreach (var refresh in _customRefresh) refresh();
                    });
                });
                swatch.GetComponent<Image>().color = Color.white;
                Action refreshSwatch = () => { var colors = swatch.colors; colors.normalColor = colors.highlightedColor = colors.pressedColor = item.Color; swatch.colors = colors; };
                _refresh.Add(refreshSwatch); _customRefresh.Add(refreshSwatch); refreshSwatch();
                DarkNeonUi.Button("×", card.transform, new Vector2(.93f,.17f), new Vector2(.99f,.83f), UiButtonKind.Secondary, () => {
                    Confirm("텍스트 삭제", "선택한 텍스트를 삭제할까요?", () => { Main.Settings.Overlay.CustomTexts.Remove(item); Main.RequestSave(); RebuildCustomRows(); });
                });
            }
        }
    }
}
