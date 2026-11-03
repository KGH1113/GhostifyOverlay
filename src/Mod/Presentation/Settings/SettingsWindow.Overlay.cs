using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DonQuixoteOverlay
{
    public sealed partial class SettingsWindow
    {
        private void BuildOverlayGroups()
        {
            foreach (var entry in OverlayGroups.All)
            {
                var group = entry;
                AddCategory(group.Title, () => {
                    Heading(group.Title);
                    Toggle("이 그룹 표시", () => OverlayGroups.Get(Main.Settings.Overlay, group.EnabledField), v => OverlayGroups.Set(Main.Settings.Overlay, group.EnabledField, v));
                    OverlayMemberGrid(group);
                    if (group.EnabledField == "Group5Enabled") BuildCurrentMapResetControls();
                    LiveHint(() => !Main.Settings.Overlay.Enabled ? "오버레이 전체 표시가 꺼져 있습니다." : !OverlayGroups.Get(Main.Settings.Overlay, group.EnabledField) ? "이 그룹은 숨겨져 있습니다. 개별 항목 선택은 유지됩니다." : "그룹 전체와 개별 항목을 따로 켜고 끌 수 있습니다.");
                    if (group.EnabledField == "Group3Enabled") Label("곡 정보가 표시되면 게임 기본 곡 정보는 자동으로 숨깁니다.\nADOFAI Tweaks의 곡 정보 숨김 설정을 우선합니다.", 64);
                    if (group.EnabledField == "Group4Enabled") LiveHint(() => NativeJudgments.DetailedTextVisible(true) ? "세부 누적 판정은 게임의 세부 판정 표시 설정을 따릅니다." : "게임의 세부 판정 표시가 꺼져 있어 세부 누적 판정도 숨겨집니다.", 64);
                });
            }
        }
        private void BuildCurrentMapResetControls()
        {
            Heading("현재 맵 기록 초기화");
            Label("현재 불러온 맵의 기록만 초기화합니다. 다른 맵의 기록은 유지됩니다.", 52);
            var attempts = Button("현재 맵 시도 횟수 초기화", () => ConfirmCurrentMapReset(true));
            var best = Button("현재 맵 최고 진행도 초기화", () => ConfirmCurrentMapReset(false));
            _refresh.Add(() => { bool ready = !string.IsNullOrEmpty(AttemptTracker.CurrentRecordKey); attempts.interactable = best.interactable = ready; });
            LiveHint(() => string.IsNullOrEmpty(AttemptTracker.CurrentRecordKey) ? "맵을 불러온 상태에서 사용할 수 있습니다." : "시도 횟수와 최고 진행도는 각각 따로 초기화됩니다.");
        }
        private void ConfirmCurrentMapReset(bool attempts)
        {
            string key = AttemptTracker.CaptureRecordKey();
            if (string.IsNullOrEmpty(key)) { ShowMessage("초기화할 맵 없음", "맵을 불러온 뒤 다시 시도하세요."); return; }
            Confirm(attempts ? "현재 맵 시도 횟수 초기화" : "현재 맵 최고 진행도 초기화",
                attempts ? "이 맵의 Attempt·Prac Attempt·Full Attempt·Full Prac Attempt를 0으로 초기화할까요?\n최고 진행도와 다른 맵 기록은 유지됩니다." :
                "이 맵의 최고 진행도를 0%로 초기화할까요?\n시도 횟수와 다른 맵 기록은 유지됩니다. 현재 진행 중인 시도는 제외하고 다음 시도부터 새로 기록합니다.",
                () => AttemptTracker.ResetCurrentMap(key, attempts));
        }
        private void OverlayMemberGrid(OverlayGroup group)
        {
            const int rows = 3;
            var row = Row(rows * 52);
            for (int i = 0; i < group.Fields.Length; i++)
            {
                string field = group.Fields[i]; int column = i % 2, line = i / 2;
                Button button = null;
                button = DarkNeonUi.ToggleCard(group.Labels[i], row.transform, new Vector2(column * .5f, 1f - (line + 1f) / rows), new Vector2((column + 1) * .5f, 1f - line / (float)rows), OverlayGroups.Get(Main.Settings.Overlay, field), () => {
                    OverlayGroups.Set(Main.Settings.Overlay, field, !OverlayGroups.Get(Main.Settings.Overlay, field));
                    button.GetComponent<DQToggleVisual>().SetValue(OverlayGroups.Get(Main.Settings.Overlay, field)); Main.RequestSave();
                });
                var rect = (RectTransform)button.transform; rect.offsetMin = new Vector2(column == 0 ? 0 : 4, 4); rect.offsetMax = new Vector2(column == 0 ? -4 : 0, -4);
                _refresh.Add(() => button.GetComponent<DQToggleVisual>().SetValue(OverlayGroups.Get(Main.Settings.Overlay, field)));
            }
        }
    }
}
