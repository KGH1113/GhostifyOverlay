using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace DonQuixoteOverlay
{
    [Serializable]
    public sealed class OverlaySettings
    {
        public OverlaySettings()
        {
            Enabled = true;
            Group1Enabled = true;
            Group2Enabled = true;
            Group3Enabled = true;
            Group4Enabled = true;
            Group5Enabled = true;
            ShowProgress = true;
            ShowAccuracy = false;
            ShowXAccuracy = true;
            ShowMusicTime = true;
            ShowMapTime = false;
            ShowProgressBar = true;
            ShowCombo = true;
            ShowBpm = true;
            ShowPitch = true;
            ShowTheoreticalKps = false;
            ShowJudgmentCounts = true;
            ShowDetailedJudgments = true;
            ShowAttempts = true;
            ShowSongInfo = true;
            ShowTimingRanges = true;
            ShowFps = false;
            Font = "Gmarket Sans";
            LineSpacing = 0f;
            ValueColor = new Color(255/255f, 255/255f, 255/255f, 255/255f);
            ProgressBarColor = new Color(255/255f, 201/255f, 57/255f, 255/255f);
            AttemptsColor = new Color(255/255f, 255/255f, 255/255f, 255/255f);
        }

        public bool Group6Enabled = true, ShowBestProgress = true;
        public List<CustomOverlayText> CustomTexts = new List<CustomOverlayText>();
        public Dictionary<string, float> BestProgress = new Dictionary<string, float>();
        public bool Enabled = true;
        public bool Group1Enabled = true, Group2Enabled = true, Group3Enabled = true, Group4Enabled = true, Group5Enabled = true;
        public bool ShowProgress = true;
        public bool ShowAccuracy;
        public bool ShowXAccuracy = true;
        public bool ShowMusicTime = true;
        public bool ShowMapTime;
        public bool ShowProgressBar = true;
        public bool ShowCombo = true;
        public bool ShowBpm = true;
        public bool ShowPitch = true;
        public bool ShowTheoreticalKps;
        public bool ShowJudgmentCounts = true;
        public bool ShowDetailedJudgments = true;
        public bool ShowAttempts = true;
        public bool ShowSongInfo = true;
        public bool ShowTimingRanges = true;
        public bool ShowFps = true;
        public string Font = "Gmarket Sans";
        public float LineSpacing;
        [JsonConverter(typeof(KeyViewerContents.KeyViewerColorConverter))]
        public Color ValueColor = Color.white;
        [JsonConverter(typeof(KeyViewerContents.KeyViewerColorConverter))]
        public Color ProgressBarColor = DQColors.KeyAccent;
        [JsonConverter(typeof(KeyViewerContents.KeyViewerColorConverter))]
        public Color AttemptsColor = Color.white;
        public Dictionary<string, int> FullAttempts = new Dictionary<string, int>();
        public Dictionary<string, int> FullProgressAttempts = new Dictionary<string, int>();
    }
}
