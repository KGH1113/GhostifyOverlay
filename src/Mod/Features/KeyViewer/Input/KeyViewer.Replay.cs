using System;
using GhostifyOverlay.Integration.OrbitRender;
using SkyHook;
using UnityEngine;

namespace DonQuixoteOverlay.KeyViewerContents
{
    public partial class KeyViewer
    {
        // A render may display increasing counts, but must never mutate the saved counters.
        private KeyCountData _replayCounts;
        private KeyCountData CurrentCounts => _replayCounts ?? KeyCountData.Instance;

        internal void SetReplayInput(bool replay)
        {
            if (replay)
            {
                if (_replayCounts == null)
                {
                    _replayCounts = new KeyCountData();
                    Array.Copy(KeyCountData.Instance.Count, _replayCounts.Count, _replayCounts.Count.Length);
                    _replayCounts.TotalCount = KeyCountData.Instance.TotalCount;
                }
                if (_listening) StopEventListener();
                ResetTransient(false);
                _state.Clear();
                RefreshReplayFrame();
                return;
            }
            if (_replayCounts == null) return;
            ResetTransient(false);
            _state.Clear();
            _replayCounts = null;
            _lastTotalCount = -1;
            for (int i = 0; i < _shownCounts.Length; i++) _shownCounts[i] = -1;
            if (_built)
            {
                UpdateCounters();
                if (Main.Enabled && Settings.Enabled && !_listening) StartEventListener();
            }
        }

        // OverlayApi callbacks run on Unity's main thread. Bypass live event routing,
        // including its ghost-only split, so each recorded transition is applied once.
        internal void ApplyReplayKey(KeyCode code, bool pressed, long ticks)
        {
            if (!OrbitRenderBridge.IsReplayActive || !_built || _replayCounts == null) return;
            WorkUnity(code, pressed, ticks);
            KeyLabel label = SkyHookKeyMapper.UnityKeyToSkyHookKey(code);
            if (label != KeyLabel.Unknown)
            {
                ushort native = NativeInputSync.NativeKey(code);
                if (native != ushort.MaxValue && _keyBinding.NativeMap.TryGetValue(native, out var indexes))
                    Work(indexes, pressed, ticks, false, false);
            }
        }

        internal void RefreshReplayFrame()
        {
            if (!_built || _replayCounts == null) return;
            _suspended = false;
            _visible = true;
            KeyViewerObject.SetActive(true);
            if (Settings.useRain) RainManager.Tick();
            foreach (var key in Keys) key?.UpdateKey();
            UpdateCounters();
        }
    }
}
