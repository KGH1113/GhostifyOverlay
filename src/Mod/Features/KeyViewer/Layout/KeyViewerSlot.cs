// Derived from JipperResourcePack by Jongyeol. Copyright (c) 2024, Jongyeol.
// BSD-3-Clause: see Licenses/JipperResourcePack-BSD-3-Clause.txt and THIRD-PARTY-NOTICES.md.
using System.Collections.Generic;

namespace DonQuixoteOverlay.KeyViewerContents
{
    internal readonly struct KeyViewerSlot
    {
        internal readonly int Index, RainRow, RainSource;
        internal readonly float X, Y, Width, Height;
        internal readonly bool Slim;
        internal KeyViewerSlot(int index, float x, float y, float width, int row, int source = -1, bool slim = false)
        {
            Index = index;
            X = x;
            Y = y;
            Width = width;
            RainRow = row;
            RainSource = source;
            Slim = slim;
            Height = slim ? 30 : 50;
        }
    }
}
