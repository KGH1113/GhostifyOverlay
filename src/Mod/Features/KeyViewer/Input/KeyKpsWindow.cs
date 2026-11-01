// Derived from JipperResourcePack by Jongyeol. Copyright (c) 2024, Jongyeol.
// BSD-3-Clause: see Licenses/JipperResourcePack-BSD-3-Clause.txt and THIRD-PARTY-NOTICES.md.
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using SkyHook;
using UnityEngine;

namespace DonQuixoteOverlay.KeyViewerContents
{
    public static class KeyKpsWindow
    {
        public static int Trim(ConcurrentQueue<long> presses, long now)
        {
            while (presses.TryPeek(out long first) && now - first > TimeSpan.TicksPerSecond)
                presses.TryDequeue(out _);
            return presses.Count;
        }
    }
}
