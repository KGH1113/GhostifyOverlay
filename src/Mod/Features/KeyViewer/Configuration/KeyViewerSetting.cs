// Derived from JipperResourcePack by Jongyeol. Copyright (c) 2024, Jongyeol.
// BSD-3-Clause: see Licenses/JipperResourcePack-BSD-3-Clause.txt and THIRD-PARTY-NOTICES.md.
using UnityEngine;

namespace DonQuixoteOverlay.KeyViewerContents
{
    [System.Serializable]
    public class KeyViewerSetting
    {
        public KeyViewerSetting()
        {
            KeyViewerStyle = (KeyviewerStyle)1;
            FootKeyViewerStyle = (FootKeyviewerStyle)5;
            key10 = new KeyCode[] { (KeyCode)9, (KeyCode)49, (KeyCode)50, (KeyCode)101, (KeyCode)281, (KeyCode)257, (KeyCode)267, (KeyCode)265, (KeyCode)32, (KeyCode)274 };
            key10Text = new string[] { null, null, null, null, "Pg↓", "Num", "/", "9", "␣", "↓" };
            GhostKey10 = new KeyCode[] { (KeyCode)0, (KeyCode)0, (KeyCode)119, (KeyCode)114, (KeyCode)279, (KeyCode)263, (KeyCode)0, (KeyCode)0, (KeyCode)0, (KeyCode)0 };
            key12 = new KeyCode[] { (KeyCode)9, (KeyCode)49, (KeyCode)50, (KeyCode)101, (KeyCode)281, (KeyCode)257, (KeyCode)267, (KeyCode)265, (KeyCode)32, (KeyCode)99, (KeyCode)274, (KeyCode)273 };
            key12Text = new string[] { null, null, null, null, "Pg↓", "Num", "/", "9", "␣", null, "↓", "↑" };
            GhostKey12 = new KeyCode[] { (KeyCode)0, (KeyCode)113, (KeyCode)119, (KeyCode)114, (KeyCode)279, (KeyCode)263, (KeyCode)264, (KeyCode)0, (KeyCode)0, (KeyCode)0, (KeyCode)0, (KeyCode)0 };
            key16 = new KeyCode[] { (KeyCode)9, (KeyCode)49, (KeyCode)50, (KeyCode)101, (KeyCode)281, (KeyCode)257, (KeyCode)267, (KeyCode)265, (KeyCode)32, (KeyCode)99, (KeyCode)274, (KeyCode)273, (KeyCode)301, (KeyCode)306, (KeyCode)266, (KeyCode)259 };
            key16Text = new string[] { null, null, null, null, "Pg↓", "Num", "/", "9", "␣", null, "↓", "↑", null, null, ".", "3" };
            GhostKey16 = new KeyCode[] { (KeyCode)0, (KeyCode)0, (KeyCode)119, (KeyCode)114, (KeyCode)279, (KeyCode)263, (KeyCode)0, (KeyCode)0, (KeyCode)0, (KeyCode)0, (KeyCode)0, (KeyCode)0, (KeyCode)0, (KeyCode)0, (KeyCode)0, (KeyCode)0 };
            footkey2 = new KeyCode[] { (KeyCode)289, (KeyCode)282 };
            footkey4 = new KeyCode[] { (KeyCode)289, (KeyCode)283, (KeyCode)288, (KeyCode)282 };
            footkey8 = new KeyCode[] { (KeyCode)289, (KeyCode)285, (KeyCode)288, (KeyCode)284, (KeyCode)287, (KeyCode)283, (KeyCode)286, (KeyCode)282 };
            footkey16 = new KeyCode[] { (KeyCode)289, (KeyCode)285, (KeyCode)288, (KeyCode)284, (KeyCode)287, (KeyCode)283, (KeyCode)286, (KeyCode)282, (KeyCode)105, (KeyCode)107, (KeyCode)117, (KeyCode)106, (KeyCode)121, (KeyCode)104, (KeyCode)116, (KeyCode)103 };
            footkey2Text = new string[] { null, null };
            footkey4Text = new string[] { null, null, null, null };
            footkey8Text = new string[] { null, null, null, null, null, null, null, null };
            footkey16Text = new string[] { null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null };
            YLocation = 200f;
            Size = 0.99999994f;
            KeyGap = 3f;
            useRain = true;
            useGhostRain = true;
            rainSpeed = 100f;
            rainHeight = 200f;
            Enabled = true;
            ViewerOnlyGameplay = true;
            SyncNativeInputKeys = true;
            XLocation = 2f;
            Background = new Color(0/255f, 0/255f, 0/255f, 255/255f);
            BackgroundClicked = new Color(255/255f, 255/255f, 255/255f, 255/255f);
            Outline = new Color(255/255f, 201/255f, 57/255f, 255/255f);
            OutlineClicked = new Color(255/255f, 201/255f, 57/255f, 255/255f);
            Text = new Color(255/255f, 255/255f, 255/255f, 255/255f);
            TextClicked = new Color(0/255f, 0/255f, 0/255f, 255/255f);
            RainColor = new Color(255/255f, 201/255f, 57/255f, 255/255f);
            RainColor2 = new Color(255/255f, 255/255f, 255/255f, 255/255f);
            GhostRainColor = new Color(14/255f, 180/255f, 252/255f, 255/255f);
            KpsColors.Background = new Color(0/255f, 0/255f, 0/255f, 255/255f);
            KpsColors.Outline = new Color(255/255f, 201/255f, 57/255f, 255/255f);
            KpsColors.Text = new Color(255/255f, 255/255f, 255/255f, 255/255f);
            KpsColors.Value = new Color(255/255f, 255/255f, 255/255f, 255/255f);
            TotalColors.Background = new Color(0/255f, 0/255f, 0/255f, 255/255f);
            TotalColors.Outline = new Color(255/255f, 201/255f, 57/255f, 255/255f);
            TotalColors.Text = new Color(255/255f, 255/255f, 255/255f, 255/255f);
            TotalColors.Value = new Color(255/255f, 255/255f, 255/255f, 255/255f);
        }

        public KeyCode[] key4 = [KeyCode.Alpha2, KeyCode.E, KeyCode.Keypad1, KeyCode.KeypadDivide];
        public string[] key4Text = new string[4];
        public KeyCode[] GhostKey4 = [KeyCode.W, KeyCode.R, KeyCode.Keypad7, KeyCode.Keypad8];
        public KeyCode[] key8 = [KeyCode.Tab, KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.E, KeyCode.PageDown, KeyCode.Keypad1, KeyCode.KeypadDivide, KeyCode.Keypad9];
        public string[] key8Text = new string[8];
        public KeyCode[] GhostKey8 = [KeyCode.None, KeyCode.None, KeyCode.W, KeyCode.R, KeyCode.End, KeyCode.Keypad7, KeyCode.None, KeyCode.None];
        public int SchemaVersion = 5;
        public KeyviewerStyle KeyViewerStyle = KeyviewerStyle.Key10;
        public FootKeyviewerStyle FootKeyViewerStyle = FootKeyviewerStyle.None;
        // ReSharper disable InconsistentNaming
        // ReSharper disable FieldCanBeMadeReadOnly.Global
        public KeyCode[] key10 = [KeyCode.Tab, KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.E, KeyCode.PageDown, KeyCode.Keypad1, KeyCode.KeypadDivide, KeyCode.Keypad9, KeyCode.Space, KeyCode.DownArrow];
        public string[] key10Text = new string[10];
        public KeyCode[] GhostKey10 = [KeyCode.None, KeyCode.None, KeyCode.W, KeyCode.R, KeyCode.End, KeyCode.Keypad7, KeyCode.None, KeyCode.None, KeyCode.None, KeyCode.None];
        public KeyCode[] key12 = [KeyCode.Tab, KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.E, KeyCode.PageDown, KeyCode.Keypad1, KeyCode.KeypadDivide, KeyCode.Keypad9, KeyCode.Space, KeyCode.C, KeyCode.DownArrow, KeyCode.UpArrow];
        public string[] key12Text = new string[12];
        public KeyCode[] GhostKey12 = [KeyCode.None, KeyCode.Q, KeyCode.W, KeyCode.R, KeyCode.End, KeyCode.Keypad7, KeyCode.Keypad8, KeyCode.None, KeyCode.None, KeyCode.None, KeyCode.None, KeyCode.None];
        public KeyCode[] key16 = [KeyCode.Tab, KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.E, KeyCode.PageDown, KeyCode.Keypad1, KeyCode.KeypadDivide, KeyCode.Keypad9, KeyCode.Space, KeyCode.C, KeyCode.DownArrow, KeyCode.UpArrow, KeyCode.CapsLock, KeyCode.LeftControl, KeyCode.KeypadPeriod, KeyCode.Keypad3];
        public string[] key16Text = [null, null, null, null, "Pg↓", "1", "/", "9", "␣", null, "↓", "↑", null, null, ".", "3"];
        public KeyCode[] GhostKey16 = [KeyCode.None, KeyCode.None, KeyCode.W, KeyCode.R, KeyCode.End, KeyCode.Keypad7, KeyCode.None, KeyCode.None, KeyCode.None, KeyCode.None, KeyCode.None, KeyCode.None, KeyCode.None, KeyCode.None, KeyCode.None, KeyCode.None];
        public KeyCode[] footkey2 = [KeyCode.F8, KeyCode.F1];
        public KeyCode[] footkey4 = [KeyCode.F8, KeyCode.F2, KeyCode.F7, KeyCode.F1];
        public KeyCode[] footkey8 = [KeyCode.F8, KeyCode.F4, KeyCode.F7, KeyCode.F3, KeyCode.F6, KeyCode.F2, KeyCode.F5, KeyCode.F1];
        public KeyCode[] footkey16 = [KeyCode.F8, KeyCode.F4, KeyCode.F7, KeyCode.F3, KeyCode.F6, KeyCode.F2, KeyCode.F5, KeyCode.F1, KeyCode.I, KeyCode.K, KeyCode.U, KeyCode.J, KeyCode.Y, KeyCode.H, KeyCode.T, KeyCode.G];
        public string[] footkey2Text = new string[2];
        public string[] footkey4Text = new string[4];
        public string[] footkey8Text = new string[8];
        public string[] footkey16Text = new string[16];
        public float YLocation = 200;
        public float Size = .99999994f;
        public float KeyGap = 3;
        public bool useRain = true;
        public bool useGhostRain = true;
        public float rainSpeed = 100;
        public float rainHeight = 200;
        // ReSharper restore InconsistentNaming
        public bool Enabled = true;
        public bool ViewerOnlyGameplay = true;
        public bool SyncNativeInputKeys = true;
        public float XLocation = 2;
        [Newtonsoft.Json.JsonConverter(typeof(KeyViewerColorConverter))]
        public Color Background = UnityEngine.Color.black;
        [Newtonsoft.Json.JsonConverter(typeof(KeyViewerColorConverter))]
        public Color BackgroundClicked = UnityEngine.Color.white;
        [Newtonsoft.Json.JsonConverter(typeof(KeyViewerColorConverter))]
        public Color Outline = DQColors.KeyAccent;
        [Newtonsoft.Json.JsonConverter(typeof(KeyViewerColorConverter))]
        public Color OutlineClicked = DQColors.KeyAccent;
        [Newtonsoft.Json.JsonConverter(typeof(KeyViewerColorConverter))]
        public Color Text = UnityEngine.Color.white;
        [Newtonsoft.Json.JsonConverter(typeof(KeyViewerColorConverter))]
        public Color TextClicked = UnityEngine.Color.black;
        [Newtonsoft.Json.JsonConverter(typeof(KeyViewerColorConverter))]
        public Color RainColor = DQColors.KeyAccent;
        [Newtonsoft.Json.JsonConverter(typeof(KeyViewerColorConverter))]
        public Color RainColor2 = UnityEngine.Color.white;
        [Newtonsoft.Json.JsonConverter(typeof(KeyViewerColorConverter))]
        public Color GhostRainColor = new Color(14f / 255f, 180f / 255f, 252f / 255f, 1f);
        public KeyViewerCounterPalette KpsColors = new();
        public KeyViewerCounterPalette TotalColors = new();
    }
}
