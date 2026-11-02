using System;
using Newtonsoft.Json;
using UnityEngine;
namespace DonQuixoteOverlay
{
    [Serializable]
    public sealed class CustomOverlayText
    {
        public string Id = Guid.NewGuid().ToString("N");
        public string Text = "Custom Text";
        public bool Enabled = true;
        public float X = 960, Y = 400, Size = 28;
        [JsonConverter(typeof(KeyViewerContents.KeyViewerColorConverter))]
        public Color Color = Color.white;
        internal void Normalize()
        {
            if (string.IsNullOrEmpty(Id)) Id = Guid.NewGuid().ToString("N");
            Text = Text ?? "";
            if (Text.Length > 2048) Text = Text.Substring(0, 2048);
            X = SettingsNormalization.Bounded(X, -1920, 3840, 960);
            Y = SettingsNormalization.Bounded(Y, -1080, 2160, 400);
            Size = SettingsNormalization.Bounded(Size, 8, 160, 28);
            Color.r = SettingsNormalization.Bounded(Color.r, 0, 1, 1);
            Color.g = SettingsNormalization.Bounded(Color.g, 0, 1, 1);
            Color.b = SettingsNormalization.Bounded(Color.b, 0, 1, 1);
            Color.a = SettingsNormalization.Bounded(Color.a, 0, 1, 1);
        }
    }
}
