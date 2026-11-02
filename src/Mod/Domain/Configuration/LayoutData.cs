using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace DonQuixoteOverlay
{
    [Serializable]
    public sealed class LayoutData
    {
        public LayoutData()
        {
            ProgressBarX = 0f;
            ProgressBarY = 0f;
            ProgressBarScale = 1f;
            TopLeftX = 0f;
            TopLeftY = 5f;
            TopRightX = 0f;
            TopRightY = 5f;
            AttemptsX = -1300f;
            AttemptsY = -10f;
            JudgmentsX = 0f;
            JudgmentsY = -20f;
            ComboX = 0f;
            ComboY = -25f;
            DetailedPerfectX = 0f;
            DetailedPerfectY = -15f;
            TopLeftScale = 0.8999999f;
            TopRightScale = 0.95f;
            AttemptsScale = 0.799999952f;
            JudgmentsScale = 0.799999952f;
            ComboScale = 0.5499999f;
            DetailedPerfectScale = 0.799999952f;
            SongInfoX = 0f;
            SongInfoY = 0f;
            SongInfoScale = 1f;
            TimingRangesX = 0f;
            TimingRangesY = -10.0000019f;
            TimingRangesScale = 1f;
        }

        public int SchemaVersion = 5;
        public float ProgressBarX, ProgressBarY;
        public float ProgressBarScale = 1f;
        public float TopLeftX;
        public float TopLeftY = 5f;
        public float TopRightX;
        public float TopRightY = 5f;
        public float AttemptsX = -1300f;
        public float AttemptsY = -10f;
        public float JudgmentsX;
        public float JudgmentsY = -20f;
        public float ComboX;
        public float ComboY = -20f;
        public float DetailedPerfectX;
        public float DetailedPerfectY = -15f;
        public float TopLeftScale = .9499999f;
        public float TopRightScale = 1f;
        public float AttemptsScale = .799999952f;
        public float JudgmentsScale = .799999952f;
        public float ComboScale = .5999999f;
        public float DetailedPerfectScale = .799999952f;
        public float SongInfoX;
        public float SongInfoY;
        public float SongInfoScale = 1f;
        public float TimingRangesX;
        public float TimingRangesY = -10.0000019f;
        public float TimingRangesScale = 1f;
    }
}
