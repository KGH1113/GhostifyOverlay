using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using DonQuixoteOverlay.KeyViewerContents;
using GhostifyOverlay.Integration.OrbitRender;
using SkyHook;
using UnityEngine;

// The production input/replay/counting code is compiled unchanged. Only Unity,
// SkyHook, presentation and storage are inert, so no game or keyboard is needed.
public static class KeyViewerReplayHarness
{
    public static string[] Run()
    {
        var results = new List<string>();
        void Check(bool condition, string message)
        {
            if (!condition) throw new Exception("FAIL " + message);
            results.Add("PASS " + message);
        }

        OrbitRenderBridge.IsReplayActive = false;
        DonQuixoteOverlay.Main.Enabled = true;
        KeyCountData.Load("unused-test-directory");
        var saved = KeyCountData.Instance;
        saved.Count[0] = 7;
        saved.TotalCount = 7;
        var original = (long[])saved.Count.Clone();
        var viewer = new KeyViewer();
        viewer.InitializeHarness();
        Check(SkyHookManager.KeyUpdated.Count == 1 && DonQuixoteOverlay.AsyncInputHook.Acquires == 1,
            "live input acquires one SkyHook listener");
        viewer.StartHarness();
        Check(SkyHookManager.KeyUpdated.Count == 1 && DonQuixoteOverlay.AsyncInputHook.Acquires == 1,
            "repeated live startup cannot duplicate a listener or hook lease");

        SkyHookManager.KeyUpdated.Invoke(new SkyHookEvent(KeyLabel.A, 65, true, 10));
        OrbitRenderBridge.IsReplayActive = true;
        viewer.SetReplayInput(true);
        Check(SkyHookManager.KeyUpdated.Count == 0 && DonQuixoteOverlay.AsyncInputHook.Releases == 1 && viewer.Queued == 0,
            "replay entry releases live input and discards queued pre-render events");
        Check(!ReferenceEquals(viewer.Counts, saved) && viewer.Counts.Count.SequenceEqual(original) && viewer.Counts.TotalCount == 7,
            "replay starts with a separate copy of the saved counters");
        viewer.StartHarness();
        viewer.ObserveRawEvent(new SkyHookEvent(KeyLabel.A, 65, true, 11));
        SkyHookManager.KeyUpdated.Invoke(new SkyHookEvent(KeyLabel.A, 65, true, 11));
        Input.Down.Add(KeyCode.A);
        DonQuixoteOverlay.AsyncInputHook.IsRunning = false;
        viewer.PumpHarness();
        Check(viewer.Queued == 0 && Input.Reads == 0 && viewer.Counts.TotalCount == 7 && SkyHookManager.KeyUpdated.Count == 0,
            "replay bypasses raw, normal and frame fallback live input without duplicates");
        Input.Down.Clear();

        viewer.ApplyReplayKey(KeyCode.A, true, -2000000);
        var firstRain = viewer.Keys[0].LastRain;
        var firstGhost = viewer.Keys[0].LastGhostRain;
        Check(viewer.Counts.TotalCount == 9 && viewer.Counts.Count[0] == 8 && viewer.Counts.Count[1] == 1,
            "recorded Unity key reaches both symbolic and native-key display bindings exactly once");
        Check(firstRain.StartTime == -2000000 && firstGhost.StartTime == -2000000,
            "negative countdown timestamps remain intact for normal and ghost rain");
        viewer.ApplyReplayKey(KeyCode.A, true, -1500000);
        Check(viewer.Counts.TotalCount == 9 && viewer.Presses == 2,
            "duplicate recorded down does not inflate counts or KPS");
        viewer.ApplyReplayKey(KeyCode.A, false, -1000000);
        Check(firstRain.FinishSize && firstGhost.FinishSize && Math.Abs(firstRain.FinalSizeY - 100f / 3f) < .001f,
            "recorded release uses its event time to finish hand and ghost rain");
        viewer.ApplyReplayKey(KeyCode.A, true, 0);
        viewer.ApplyReplayKey(KeyCode.A, false, 0);
        Check(viewer.Counts.TotalCount == 11 && viewer.Presses == 4 && !viewer.Held(0),
            "multiple same-frame press/release edges survive with zero-duration taps");
        viewer.ApplyReplayKey(KeyCode.B, true, 1);
        viewer.ApplyReplayKey(KeyCode.B, false, 2);
        Check(viewer.Counts.Count[KeyViewer.HandOutIndex] == 1 && viewer.Counts.TotalCount == 12,
            "recorded foot input counts without creating another hand event");
        viewer.ApplyReplayKey(KeyCode.Return, true, 3);
        viewer.ApplyReplayKey(KeyCode.Return, false, 4);
        viewer.ApplyReplayKey(KeyCode.KeypadEnter, true, 5);
        viewer.ApplyReplayKey(KeyCode.KeypadEnter, false, 6);
        Check(viewer.Counts.Count[2] == 1 && viewer.Counts.Count[3] == 1 && viewer.Counts.TotalCount == 14,
            "main Enter and numpad Enter stay separate replay bindings");
        Check(saved.Count.SequenceEqual(original) && saved.TotalCount == 7,
            "rendered hand, foot and ghost activity never mutates the saved counter object");

        viewer.ApplyReplayKey(KeyCode.A, true, 10);
        viewer.BeginGameplayRun();
        Check(viewer.Held(0), "game start callbacks cannot erase the recorded held-key state");
        viewer.SetHiddenAndSuspended();
        viewer.RefreshReplayFrame();
        Check(viewer.KeyViewerObject.Active && !viewer.Suspended && viewer.Keys[0].Pressed,
            "capture refresh presents recorded held keys even when the live viewer was hidden or unfocused");
        viewer.Clock = 2 * TimeSpan.TicksPerSecond;
        viewer.RefreshReplayFrame();
        Check(viewer.Kps.Value.text == "0", "capture refresh expires KPS according to the replay clock");

        OrbitRenderBridge.IsReplayActive = false;
        viewer.SetReplayInput(false);
        Check(ReferenceEquals(viewer.Counts, saved) && saved.Count.SequenceEqual(original) && saved.TotalCount == 7,
            "replay exit restores the original cumulative counters");
        Check(SkyHookManager.KeyUpdated.Count == 1 && DonQuixoteOverlay.AsyncInputHook.Acquires == 2 && !viewer.Held(0) && viewer.Presses == 0,
            "replay exit clears transient state and resumes exactly one live input lease");
        Check(viewer.Total.Value.text == "7" && viewer.Keys[0].Value.text == "7",
            "replay exit immediately restores displayed saved totals");
        viewer.SetReplayInput(false);
        Check(DonQuixoteOverlay.AsyncInputHook.Acquires == 2 && SkyHookManager.KeyUpdated.Count == 1,
            "repeated replay cleanup does not duplicate live listeners");
        saved.Flush(0, true);
        Check(GhostifyOverlay.Infrastructure.Persistence.JsonSettingsRepository<KeyCountData>.Saves == 0,
            "rendering does not mark saved counts dirty or cause persistence on a later flush");

        var down = new SkyHookEvent(KeyLabel.A, 65, true, 100);
        viewer.ObserveRawEvent(down);
        SkyHookManager.KeyUpdated.Invoke(down);
        viewer.PumpHarness();
        Check(saved.TotalCount == 9 && viewer.Keys[0].LastGhostRain != null,
            "resumed live normal/raw routing still counts each binding once and draws ghost rain");
        saved.Flush(1, true);
        Check(GhostifyOverlay.Infrastructure.Persistence.JsonSettingsRepository<KeyCountData>.Saves == 1,
            "a new live press persists normally after rendering");
        var up = new SkyHookEvent(KeyLabel.A, 65, false, 101);
        viewer.ObserveRawEvent(up);
        SkyHookManager.KeyUpdated.Invoke(up);
        viewer.PumpHarness();
        viewer.ResetTransient(false);
        DonQuixoteOverlay.AsyncInputHook.IsRunning = false;
        Input.Down.Add(KeyCode.B);
        viewer.PumpHarness();
        viewer.PumpHarness();
        Input.Down.Clear();
        Check(saved.TotalCount == 10,
            "frame fallback resumes after replay and repeated frame reads do not duplicate a press");
        long after = saved.TotalCount;
        viewer.ApplyReplayKey(KeyCode.B, true, 102);
        Check(saved.TotalCount == after, "late recorded callbacks cannot enter the restored live counter");
        viewer.StopHarness();
        return results.ToArray();
    }
}

namespace GhostifyOverlay.Integration.OrbitRender
{
    internal static class OrbitRenderBridge { internal static bool IsReplayActive; }
}
namespace UnityEngine
{
    public enum KeyCode { None = 0, A = 97, B = 98, Return = 13, KeypadEnter = 271, Escape = 27, Mouse0 = 323 }
    public sealed class GameObject { public bool Active = true; public void SetActive(bool active) => Active = active; }
    public static class Time { public static int frameCount; }
    public static class Input
    {
        public static readonly HashSet<KeyCode> Down = new(), Up = new();
        public static int Reads;
        public static bool GetKey(KeyCode key) { Reads++; return false; }
        public static bool GetKeyDown(KeyCode key) { Reads++; return Down.Contains(key); }
        public static bool GetKeyUp(KeyCode key) { Reads++; return Up.Contains(key); }
    }
}
namespace SkyHook
{
    public enum KeyLabel { Unknown, A, B, Enter, NumpadEnter }
    public enum EventType { KeyPressed, KeyReleased }
    public readonly struct SkyHookEvent(KeyLabel label, ushort key, bool down, long ticks)
    {
        public readonly KeyLabel Label = label;
        public readonly ushort Key = key;
        public readonly EventType Type = down ? EventType.KeyPressed : EventType.KeyReleased;
        public long GetTimeInTicks() => ticks;
    }
    public static class SkyHookKeyMapper
    {
        public static KeyLabel UnityKeyToSkyHookKey(KeyCode key) => key switch {
            KeyCode.A => KeyLabel.A, KeyCode.B => KeyLabel.B, KeyCode.Return => KeyLabel.Enter, KeyCode.KeypadEnter => KeyLabel.NumpadEnter, _ => KeyLabel.Unknown };
        public static ushort KeyLabelToNativeKeyCode(KeyLabel key) => key switch { KeyLabel.A => 65, KeyLabel.B => 66, KeyLabel.Enter or KeyLabel.NumpadEnter => 13, _ => 0 };
        public static KeyCode SkyHookKeyToUnityKey(KeyLabel label) => label switch {
            KeyLabel.A => KeyCode.A, KeyLabel.B => KeyCode.B, KeyLabel.Enter => KeyCode.Return, KeyLabel.NumpadEnter => KeyCode.KeypadEnter, _ => KeyCode.None };
    }
    public sealed class EventHub
    {
        private readonly List<Action<SkyHookEvent>> _listeners = new();
        public int Count => _listeners.Count;
        public void AddListener(Action<SkyHookEvent> listener) => _listeners.Add(listener);
        public void RemoveListener(Action<SkyHookEvent> listener) => _listeners.Remove(listener);
        public void Invoke(SkyHookEvent value) { foreach (var listener in _listeners.ToArray()) listener(value); }
    }
    public static class SkyHookManager { public static readonly EventHub KeyUpdated = new(); }
}
namespace DonQuixoteOverlay
{
    internal static class Main { internal static bool Enabled = true; internal static void Log(string value) { } }
    internal static class PatchRegistry { internal static bool IsAvailable(string name) => true; }
    internal static class RuntimeStatus { internal static void Set(string id, string label, string detail) { } }
    internal static class AsyncInputHook
    {
        internal static bool IsRunning = true;
        internal static int Acquires, Releases;
        internal static void Acquire() { Acquires++; IsRunning = true; }
        internal static void Release() { Releases++; IsRunning = false; }
    }
    internal static class JsonFileStore { internal static readonly object Storage = new(); }
    internal static class NativeInputSync
    {
        internal static ushort NativeKey(KeyCode code) => SkyHookKeyMapper.KeyLabelToNativeKeyCode(SkyHookKeyMapper.UnityKeyToSkyHookKey(code));
    }
}
namespace GhostifyOverlay.Application.Persistence
{
    public interface ISettingsRepository<T> { T Load(); void Save(T value); }
}
namespace GhostifyOverlay.Infrastructure.Persistence
{
    public sealed class JsonSettingsRepository<T> : GhostifyOverlay.Application.Persistence.ISettingsRepository<T>
    {
        private readonly Func<T> _create;
        public static int Saves;
        public JsonSettingsRepository(string path, object storage, Func<string, T> deserialize, Func<T> create, Action<string> log, bool persistDefaults, Action<T> onRecovered) => _create = create;
        public T Load() => _create();
        public void Save(T value) => Saves++;
    }
}
namespace DonQuixoteOverlay.KeyViewerContents
{
    public enum KeyviewerStyle { Key16 }
    public sealed class KeyViewerSetting
    {
        public bool Enabled = true, useRain = true, useGhostRain = true;
        public KeyviewerStyle KeyViewerStyle;
        public float rainSpeed = 100;
    }
    public sealed class TextValue { public string text; }
    public sealed class Key
    {
        public int Color;
        public bool Pressed;
        public TextValue Value = new();
        public RawRain LastRain, LastGhostRain;
        public void UpdateRequestKey(bool pressed) => Pressed = pressed;
        public void UpdateKey() { }
    }
    public sealed class RainManager
    {
        public readonly ConcurrentQueue<RawRain> RawRainQueue = new();
        private readonly List<RawRain> _active = new();
        public void Tick() { while (RawRainQueue.TryDequeue(out var rain)) _active.Add(rain); }
        public void Clear()
        {
            Tick();
            foreach (var rain in _active) RawRain.AddPool(rain);
            _active.Clear();
        }
    }
    public static class KeyViewerMetrics { public static float RainWidth(int color) => 50; }
    public partial class KeyViewer
    {
        public const int HandOutIndex = 20, FootOutIndex = 36, GhostOutIndex = 56;
        public static readonly KeyViewerSetting Settings = new();
        public static RainManager RainManager;
        public Key[] Keys;
        public Key Kps = new(), Total = new();
        public GameObject KeyViewerObject = new();
        private bool _built, _visible = true, _focused = true;
        internal bool CaptureRequested;
        internal int CapturedKeyCode, CaptureAfterFrame;
        internal long CaptureStartTicks;
        public long Clock;
        private static KeyViewer _instance;
        public static long CurrentTicks => _instance.Clock;
        public KeyCountData Counts => CurrentCounts;
        public int Queued => _eventQueue.Count;
        public int Presses => _pressTimes.Count;
        public bool Suspended => _suspended;
        public bool Held(int slot) => _keyState[slot];
        private static KeyCode[] GetKeyCode() => [KeyCode.A, (KeyCode)(0x1000 + 65), KeyCode.Return, KeyCode.KeypadEnter];
        private static KeyCode[] GetFootKeyCode() => [KeyCode.B];
        private static KeyCode[] GetGhostKeyCode() => [KeyCode.A];
        private static int CountIndex(int index, KeyviewerStyle style) => index;
        public void InitializeHarness()
        {
            _instance = this;
            _built = true;
            Keys = Enumerable.Range(0, FootOutIndex).Select(_ => new Key()).ToArray();
            _pressTimes = new();
            RainManager = new();
            RebuildKeyBinding();
            StartEventListener();
        }
        public void SetHiddenAndSuspended() { _visible = false; _focused = false; _suspended = true; KeyViewerObject.SetActive(false); }
        public void StartHarness() => StartEventListener();
        public void StopHarness() => StopEventListener();
        public void PumpHarness() => PumpInput();
    }
}
