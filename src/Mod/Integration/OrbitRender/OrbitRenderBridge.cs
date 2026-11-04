using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using DonQuixoteOverlay;
using DonQuixoteOverlay.KeyViewerContents;
using UnityEngine;

namespace GhostifyOverlay.Integration.OrbitRender
{
    // Optional, name-based OverlayApi v1 integration. No OrbitRender types cross
    // this boundary and no assembly reference is required to run without it.
    internal static class OrbitRenderBridge
    {
        private const string ProviderId = "GhostifyOverlay";
        private static readonly Provider Callbacks = new Provider();
        private static MethodInfo _register, _unregister, _isRegistered;
        private static Func<bool> _rendering;
        private static long _nextResolve, _replayTicks;
        private static bool _registered, _observingSession;
        private static volatile bool _replayActive;
        internal static bool IsRendering { get; private set; }
        // Also holds input in isolation when enabled halfway through a render;
        // no missed replay events are synthesized in that waiting state.
        internal static bool IsReplayActive => _replayActive;

        internal static bool TryGetReplayTicks(out long ticks)
        {
            ticks = Interlocked.Read(ref _replayTicks);
            return _replayActive;
        }

        // Called by the existing UMM update callback on the main thread.
        internal static void Tick()
        {
            if (!Main.Enabled) return;
            long now = Stopwatch.GetTimestamp();
            if (_observingSession)
            {
                // A provider registered after BeginSession would not receive the
                // missed input or belong to the already captured canvas set. Wait
                // safely for that render to finish instead. The cached getter is
                // allocation-free and lets the first live frame resume normally.
                try
                {
                    if (_rendering()) return;
                    EndSession();
                    _nextResolve = 0;
                }
                catch (Exception error)
                {
                    if (now >= _nextResolve)
                    {
                        _nextResolve = now + Stopwatch.Frequency;
                        Main.Log("OrbitRender session status: " + error.GetBaseException().Message);
                    }
                    return;
                }
            }
            else if (IsRendering) return;
            if (now < _nextResolve) return;
            _nextResolve = now + Stopwatch.Frequency;
            try
            {
                if (_register == null && !Resolve()) return;
                if (_rendering())
                {
                    _observingSession = true;
                    // No callback stream is available for this session. Isolate
                    // counters and suspend live input until the next real session.
                    BeginSession(true);
                    return;
                }
                if (_registered && _isRegistered.Invoke(null, new object[] { ProviderId }) is true) return;
                _registered = _register.Invoke(null, new object[] { ProviderId, Callbacks }) is true;
            }
            catch (Exception error)
            {
                Main.Log("OrbitRender overlay registration: " + error.GetBaseException().Message);
            }
        }

        private static bool Resolve()
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.Static;
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type;
                try { type = assembly.GetType("OrbitRender.OverlayApi", false); }
                catch (Exception) { continue; }
                if (type == null || !(type.GetField("ApiVersion", flags)?.GetValue(null) is int version) || version < 1) continue;
                var register = type.GetMethod("Register", flags, null, new[] { typeof(string), typeof(object) }, null);
                var unregister = type.GetMethod("Unregister", flags, null, new[] { typeof(string) }, null);
                var registered = type.GetMethod("IsRegistered", flags, null, new[] { typeof(string) }, null);
                var rendering = type.GetProperty("IsRendering", flags);
                if (register?.ReturnType != typeof(bool) || unregister == null || registered?.ReturnType != typeof(bool) || rendering?.PropertyType != typeof(bool) || rendering.GetGetMethod() == null) continue;
                var isRendering = (Func<bool>)Delegate.CreateDelegate(typeof(Func<bool>), rendering.GetGetMethod());
                _register = register;
                _unregister = unregister;
                _isRegistered = registered;
                _rendering = isRendering;
                return true;
            }
            return false;
        }

        internal static void Shutdown()
        {
            try
            {
                if (_registered) _unregister?.Invoke(null, new object[] { ProviderId });
            }
            catch (Exception error) { Main.Log("OrbitRender overlay unregister: " + error.GetBaseException().Message); }
            finally
            {
                try { EndSession(); }
                finally
                {
                    _registered = false;
                    _observingSession = false;
                    _nextResolve = 0;
                }
            }
        }

        private static void EndSession()
        {
            if (!IsRendering) return;
            // Unregister does not send an end callback. Keep our guard active until
            // all owned state is restored, including when shutdown happens mid-render.
            _replayActive = false;
            try { KeyViewer.Instance?.SetReplayInput(false); }
            finally
            {
                try { AttemptTracker.EndRenderSession(); }
                finally
                {
                    ChatterRuntime.RenderSuspended = false;
                    ChatterRuntime.Reset();
                    _replayActive = false;
                    Interlocked.Exchange(ref _replayTicks, 0);
                    IsRendering = false;
                    _observingSession = false;
                }
            }
        }

        private static void BeginSession(bool replay)
        {
            if (IsRendering) return;
            AttemptTracker.BeginRenderSession();
            IsRendering = true;
            _replayActive = replay;
            Interlocked.Exchange(ref _replayTicks, 0);
            try
            {
                LayoutEditorController.CancelActive();
                if (SettingsWindow.Instance != null && SettingsWindow.Instance.Visible) SettingsWindow.Instance.Toggle();
                ChatterRuntime.RenderSuspended = true;
                ChatterRuntime.Reset();
                if (replay) KeyViewer.Instance?.SetReplayInput(true);
            }
            catch
            {
                EndSession();
                throw;
            }
        }

        private sealed class Provider
        {
            public IEnumerable<Canvas> GetOverlayCanvases()
            {
                if (_observingSession) yield break;
                var overlay = OverlayController.Instance;
                if (overlay != null && overlay.CaptureCanvas != null) yield return overlay.CaptureCanvas;
                var viewer = KeyViewer.Instance;
                if (!_replayActive || viewer == null || !viewer.KeyViewerObject) yield break;
                var canvas = viewer.KeyViewerObject.GetComponent<Canvas>();
                if (canvas != null) yield return canvas;
            }

            public void OnRenderSessionBegin(bool replay) => BeginSession(replay);

            public void OnRenderSessionEnd() => EndSession();

            public void OnReplayClock(double videoSeconds, long ticks)
            {
                if (_replayActive && !_observingSession) Interlocked.Exchange(ref _replayTicks, ticks);
            }

            public void OnReplayInput(string keyCode, bool down, long ticks)
            {
                if (_replayActive && !_observingSession && Enum.TryParse(keyCode, true, out KeyCode code) && code != KeyCode.None)
                    KeyViewer.Instance?.ApplyReplayKey(code, down, ticks);
            }

            public void OnBeforeOverlayCapture(bool replay)
            {
                if (_replayActive && !_observingSession) KeyViewer.Instance?.RefreshReplayFrame();
            }
        }
    }
}
