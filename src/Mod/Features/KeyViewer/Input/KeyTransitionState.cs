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
    // Pure state/clock decisions are shared by the runtime and the offline regression suite.
    public sealed class KeyTransitionState
    {
        private readonly bool[] _state = new bool[KeyViewer.GhostOutIndex];
        private readonly bool[] _startHeld = new bool[KeyViewer.GhostOutIndex];
        public bool Transition(int index, bool pressed)
        {
            if (_startHeld[index])
            {
                if (!pressed)
                    _startHeld[index] = false;
                return false;
            }

            if (_state[index] == pressed)
                return false;
            _state[index] = pressed;
            return true;
        }

        public bool Synchronize(int index, bool pressed)
        {
            if (!pressed)
                _startHeld[index] = false;
            return _state[index] = pressed && !_startHeld[index];
        }

        public void SuppressStartHeld(int index, bool held)
        {
            _startHeld[index] = held;
            _state[index] = false;
        }

        public void ClearStates() => Array.Clear(_state, 0, _state.Length);
        public void Clear()
        {
            ClearStates();
            Array.Clear(_startHeld, 0, _startHeld.Length);
        }
    }
}
