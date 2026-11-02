using System.Collections.Generic;
using TMPro;
using UnityEngine;
namespace DonQuixoteOverlay
{
    public sealed partial class OverlayController
    {
        private readonly List<TextMeshProUGUI> _customTexts = new List<TextMeshProUGUI>();
        private TextMeshProUGUI _bestProgress;
        private void RefreshExtraTexts(bool playing)
        {
            var settings = Main.Settings.Overlay;
            if (_bestProgress == null) {
                _bestProgress = Text("BestProgress", _attempts.transform, Vector2.zero, new Vector2(400, 50), TextAlignmentOptions.TopLeft, 23);
                _bestProgress.rectTransform.anchorMin = _bestProgress.rectTransform.anchorMax = Vector2.zero;
            }
            _bestProgress.gameObject.SetActive(playing && settings.Group5Enabled && settings.ShowBestProgress);
            _bestProgress.rectTransform.anchoredPosition = new Vector2(0, -(23f + settings.LineSpacing) * .5f);
            _bestProgress.color = settings.AttemptsColor;
            _bestProgress.text = "Best " + AttemptTracker.BestProgress.ToString("0.00%");
            var items = settings.CustomTexts;
            while (_customTexts.Count > items.Count) {
                var last = _customTexts[_customTexts.Count - 1];
                _overlayTexts.Remove(last); Destroy(last.gameObject); _customTexts.RemoveAt(_customTexts.Count - 1);
            }
            while (_customTexts.Count < items.Count) {
                var text = Text("CustomText", _contentRoot, Vector2.zero, new Vector2(1200, 200), TextAlignmentOptions.TopLeft, 28);
                text.richText = false; text.enableAutoSizing = false; text.raycastTarget = false;
                _customTexts.Add(text);
            }
            for (int i = 0; i < items.Count; i++) {
                var item = items[i]; var text = _customTexts[i];
                text.gameObject.SetActive(playing && settings.Group6Enabled && item.Enabled);
                text.text = item.Text; text.color = item.Color; text.fontSize = item.Size;
                text.lineSpacing = settings.LineSpacing;
                text.rectTransform.anchoredPosition = new Vector2(item.X, -item.Y);
            }
        }
    }
}
