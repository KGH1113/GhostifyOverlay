using System;
using System.Linq;
using TMPro;
using UnityEngine;
namespace DonQuixoteOverlay
{
    public sealed partial class SettingsWindow
    {
        private string _customTextId;
        private readonly CustomOverlayText _emptyCustom = new CustomOverlayText { Text = "" };
        private CustomOverlayText SelectedCustom => Main.Settings.Overlay.CustomTexts.FirstOrDefault(t => t.Id == _customTextId) ?? _emptyCustom;
        private void BuildCustomTextSettings()
        {
            Heading("6 · 커스텀 텍스트");
            Toggle("커스텀 텍스트 그룹 표시", () => Main.Settings.Overlay.Group6Enabled, v => Main.Settings.Overlay.Group6Enabled = v);
            Button("텍스트 추가", () => TextDialog("텍스트 추가", "", "화면에 표시할 텍스트를 입력하세요. 최대 50개입니다.", 2048, value => {
                if (Main.Settings.Overlay.CustomTexts.Count >= 50) throw new ArgumentException("최대 50개까지 추가할 수 있습니다.");
                var item = new CustomOverlayText { Text = value }; Main.Settings.Overlay.CustomTexts.Add(item); _customTextId = item.Id; Main.RequestSave();
            }));
            Button("편집할 텍스트 선택", () => {
                var items = Main.Settings.Overlay.CustomTexts.ToArray();
                Choose("텍스트 선택", items.Select((t, i) => (i + 1) + " · " + t.Text.Replace("\n", " ")).ToArray(), i => _customTextId = items[i].Id);
            });
            LiveHint(() => ReferenceEquals(SelectedCustom, _emptyCustom) ? "텍스트를 추가하거나 목록에서 선택하세요." : "편집 중: " + SelectedCustom.Text);
            Toggle("선택한 텍스트 표시", () => SelectedCustom.Enabled, v => SelectedCustom.Enabled = v);
            Label("텍스트 내용 · Enter로 줄바꿈, 입력을 마치면 자동 저장", 44);
            var input = DarkNeonUi.Input("Custom text", Row(96).transform, Vector2.zero, Vector2.one, "내용", false);
            input.lineType = TMP_InputField.LineType.MultiLineNewline; input.characterLimit = 2048;
            input.onEndEdit.AddListener(value => { SelectedCustom.Text = value; Main.RequestSave(); });
            _refresh.Add(() => { if (!input.isFocused) input.SetTextWithoutNotify(SelectedCustom.Text); });
            ColorRow("텍스트 색상", () => SelectedCustom.Color, v => SelectedCustom.Color = v);
            ValueSlider("글자 크기", 8, 160, () => SelectedCustom.Size, v => SelectedCustom.Size = v);
            ValueSlider("가로 위치 X", -1920, 3840, () => SelectedCustom.X, v => SelectedCustom.X = v);
            ValueSlider("세로 위치 Y", -1080, 2160, () => SelectedCustom.Y, v => SelectedCustom.Y = v);
            Label("1920×1080 기준 · X는 오른쪽으로, Y는 아래로 이동합니다. 숫자칸에 직접 입력할 수 있습니다.", 60);
            Button("선택한 텍스트 삭제", () => {
                var item = SelectedCustom;
                if (ReferenceEquals(item, _emptyCustom)) return;
                Confirm("텍스트 삭제", "선택한 커스텀 텍스트를 삭제할까요?", () => {
                    Main.Settings.Overlay.CustomTexts.Remove(item); _customTextId = null; Main.RequestSave();
                    foreach (var refresh in _refresh) refresh();
                });
            });
        }
    }
}
