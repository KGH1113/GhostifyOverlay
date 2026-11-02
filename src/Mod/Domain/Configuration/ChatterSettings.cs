using System;
namespace DonQuixoteOverlay
{
    [Serializable]
    public sealed class ChatterSettings
    {
        public ChatterSettings()
        {
            Enabled = true;
            IntervalMilliseconds = 70;
        }

        public bool Enabled;
        public int IntervalMilliseconds = 100;
    }
}
