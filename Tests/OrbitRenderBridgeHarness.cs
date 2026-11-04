using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using GhostifyOverlay.Integration.OrbitRender;
using DonQuixoteOverlay;
using DonQuixoteOverlay.KeyViewerContents;
using UnityEngine;

// Compiles the production bridge against inert collaborators. The fake optional
// API is loaded only after the missing-mod case, exercising real reflection.
public static class OrbitRenderBridgeHarness
{
    private static readonly List<string> Results = new List<string>();
    private static object Provider => OrbitRenderTestApi.Provider;

    public static string[] Run()
    {
        Results.Clear();
        OrbitRenderBridge.Shutdown();
        SetupPresentation();
        Main.Enabled = false;
        OrbitRenderBridge.Tick();
        Check(OrbitRenderTestApi.RegisterCalls == 0, "disabled mod does not register an overlay");
        Main.Enabled = true;
        OrbitRenderBridge.Tick();
        long ticks;
        Check(!OrbitRenderBridge.IsRendering && !OrbitRenderBridge.TryGetReplayTicks(out ticks), "missing OrbitRender leaves normal input and time available");

        Type api = LoadOptionalApi();
        api.GetField("ApiVersion").SetValue(null, 0);
        OrbitRenderBridge.Shutdown();
        OrbitRenderBridge.Tick();
        Check(OrbitRenderTestApi.RegisterCalls == 0, "an incompatible optional API is ignored");
        api.GetField("ApiVersion").SetValue(null, 1);
        OrbitRenderTestApi.Rendering = true;
        OrbitRenderBridge.Shutdown();
        OrbitRenderBridge.Tick();
        Check(OrbitRenderTestApi.RegisterCalls == 0 && OrbitRenderBridge.IsRendering && OrbitRenderBridge.IsReplayActive
            && KeyViewer.Instance.Replay && AttemptTracker.Rendering,
            "late loading protects counters and suspends live input without joining a partial render");
        int observedBegins = AttemptTracker.Begins;
        OrbitRenderBridge.Tick();
        Check(AttemptTracker.Begins == observedBegins, "observing an active session does not replace its protection snapshot each frame");
        OrbitRenderBridge.Shutdown();
        Check(!OrbitRenderBridge.IsRendering && !KeyViewer.Instance.Replay && !AttemptTracker.Rendering
            && OrbitRenderTestApi.UnregisterCalls == 0, "disabling a late observer restores local state without unregistering a nonexistent provider");
        OrbitRenderBridge.Tick();
        Check(OrbitRenderBridge.IsRendering && KeyViewer.Instance.Replay && OrbitRenderTestApi.RegisterCalls == 0,
            "re-enabling during the same render reinstates protection without claiming missed input");

        OrbitRenderTestApi.Rendering = false;
        OrbitRenderTestApi.RejectRegistration = true;
        OrbitRenderBridge.Tick();
        Check(!OrbitRenderBridge.IsRendering && !KeyViewer.Instance.Replay && !AttemptTracker.Rendering,
            "the first idle update ends late-session protection before registration");
        Check(OrbitRenderTestApi.RegisterCalls == 1 && Provider == null, "refused registration does not claim ownership");
        OrbitRenderBridge.Tick();
        Check(OrbitRenderTestApi.RegisterCalls == 1, "failed registration is throttled instead of retried each frame");
        OrbitRenderTestApi.RejectRegistration = false;
        Thread.Sleep(1100);
        OrbitRenderBridge.Tick();
        Check(OrbitRenderTestApi.RegisterCalls == 2 && Provider != null && OrbitRenderTestApi.LastId == "GhostifyOverlay", "an idle compatible API accepts the deferred provider");
        OrbitRenderBridge.Tick();
        Check(OrbitRenderTestApi.RegisterCalls == 2, "an already registered provider is not duplicated");

        SetupPresentation();
        Begin(false);
        Check(OrbitRenderBridge.IsRendering && !OrbitRenderBridge.IsReplayActive && AttemptTracker.Rendering, "normal rendering protects attempts without entering replay input mode");
        Check(!SettingsWindow.Instance.Visible && !LayoutEditorController.Active && ChatterRuntime.RenderSuspended, "render preparation closes control UI and suspends chatter filtering");
        Check(Canvases().SequenceEqual(new[] { OverlayController.Instance.CaptureCanvas }), "normal rendering provides the HUD without the key viewer");
        Invoke("OnReplayClock", 1d, 10000000L);
        Invoke("OnReplayInput", "A", true, 10000000L);
        Check(!OrbitRenderBridge.TryGetReplayTicks(out ticks) && KeyViewer.Instance.Inputs.Count == 0, "normal renders ignore replay-only callbacks");
        End();
        Check(!OrbitRenderBridge.IsRendering && !AttemptTracker.Rendering && !ChatterRuntime.RenderSuspended, "normal render completion releases every session guard");

        Begin(true);
        Check(OrbitRenderBridge.IsReplayActive && KeyViewer.Instance.Replay, "replay session switches the viewer to recorded input");
        Check(Canvases().SequenceEqual(new[] { OverlayController.Instance.CaptureCanvas, KeyViewer.Instance.KeyViewerObject.Canvas }), "replay capture provides both existing canvases");
        int begins = AttemptTracker.Begins;
        Invoke("OnRenderSessionBegin", true);
        Check(AttemptTracker.Begins == begins, "duplicate session begin does not replace the owned snapshot");
        Invoke("OnReplayClock", 2d, 20000000L);
        Check(OrbitRenderBridge.TryGetReplayTicks(out ticks) && ticks == 20000000L, "presentation clock uses supplied TimeSpan ticks");
        Invoke("OnReplayInput", "None", true, 0L);
        Invoke("OnReplayInput", "not-a-key", true, 0L);
        Check(KeyViewer.Instance.Inputs.Count == 0, "invalid and unbound None key names are ignored");
        Invoke("OnReplayClock", 0d, 0L);
        Invoke("OnReplayInput", "a", true, -30L);
        Invoke("OnReplayInput", "A", false, -20L);
        Invoke("OnReplayInput", "KeypadEnter", true, 15L);
        Invoke("OnReplayClock", 2d, 20000000L);
        Check(KeyViewer.Instance.Inputs.Count == 3 && KeyViewer.Instance.Inputs[0].Code == KeyCode.A
            && KeyViewer.Instance.Inputs[0].Ticks == -30 && KeyViewer.Instance.Inputs[1].Ticks == -20
            && !KeyViewer.Instance.Inputs[1].Down && KeyViewer.Instance.Inputs[2].Code == KeyCode.KeypadEnter,
            "recorded edges preserve event timestamps, case-insensitive names and keypad identity");
        Check(OrbitRenderBridge.TryGetReplayTicks(out ticks) && ticks == 20000000L, "frame clock is restored after earlier input-event times");
        Invoke("OnBeforeOverlayCapture", true);
        Check(KeyViewer.Instance.Refreshes == 1, "capture refreshes the replay presentation at the supplied frame clock");
        End();
        Check(!KeyViewer.Instance.Replay && !KeyViewer.Instance.EndSawReplay && KeyViewer.Instance.EndSawRendering,
            "live input resumes after the replay flag clears while the recording guard remains owned");
        Check(!OrbitRenderBridge.TryGetReplayTicks(out ticks) && ticks == 0 && !OrbitRenderBridge.IsRendering,
            "end callback cleans up even though OrbitRender already cleared its rendering flag");
        int ends = AttemptTracker.Ends;
        Invoke("OnRenderSessionEnd");
        Check(AttemptTracker.Ends == ends, "duplicate session end is harmless");
        int inputs = KeyViewer.Instance.Inputs.Count;
        Invoke("OnReplayInput", "A", true, 42L);
        Check(KeyViewer.Instance.Inputs.Count == inputs, "late replay input cannot reach live mode");

        KeyViewer.Instance.FailBegin = true;
        ExpectFailure(() => Begin(true));
        Check(!OrbitRenderBridge.IsRendering && !OrbitRenderBridge.IsReplayActive && !AttemptTracker.Rendering
            && !ChatterRuntime.RenderSuspended, "a viewer preparation failure restores attempts, input and chatter ownership");
        KeyViewer.Instance.FailBegin = false;
        Begin(true);
        KeyViewer.Instance.FailEnd = true;
        ExpectFailure(End);
        Check(!OrbitRenderBridge.IsRendering && !OrbitRenderBridge.IsReplayActive && !AttemptTracker.Rendering
            && !ChatterRuntime.RenderSuspended, "a viewer cleanup failure still releases remaining session guards");
        KeyViewer.Instance.FailEnd = false;

        Begin(true);
        OrbitRenderTestApi.ThrowUnregister = true;
        OrbitRenderBridge.Shutdown();
        Check(OrbitRenderTestApi.UnregisterCalls == 1 && !OrbitRenderBridge.IsRendering && !KeyViewer.Instance.Replay
            && !AttemptTracker.Rendering && !ChatterRuntime.RenderSuspended,
            "unregister failure during an active render still restores local state");
        Check(Main.Messages.Count > 0, "optional API failures are reported without escaping shutdown");
        OrbitRenderTestApi.ThrowUnregister = false;
        OrbitRenderTestApi.Rendering = false;
        OrbitRenderBridge.Tick();
        Check(OrbitRenderTestApi.RegisterCalls == 3, "a stopped bridge can register again for a later session");
        OrbitRenderTestApi.Provider = null;
        Thread.Sleep(1100);
        OrbitRenderBridge.Tick();
        Check(OrbitRenderTestApi.RegisterCalls == 4 && Provider != null, "a lost provider registration is recovered while idle");
        OrbitRenderBridge.Shutdown();
        return Results.ToArray();
    }

    private static void SetupPresentation()
    {
        OverlayController.Instance = new OverlayController { CaptureCanvas = new Canvas() };
        KeyViewer.Instance = new KeyViewer { KeyViewerObject = new GameObject { Canvas = new Canvas() } };
        SettingsWindow.Instance = new SettingsWindow { Visible = true };
        LayoutEditorController.Active = true;
    }
    private static void Begin(bool replay)
    {
        OrbitRenderTestApi.Rendering = true;
        Invoke("OnRenderSessionBegin", replay);
    }
    private static void End()
    {
        // Matches OverlayApi.EndSession: its public flags clear before callbacks.
        OrbitRenderTestApi.Rendering = false;
        Invoke("OnRenderSessionEnd");
    }
    private static Canvas[] Canvases() => ((IEnumerable<Canvas>)Invoke("GetOverlayCanvases")).ToArray();
    private static object Invoke(string name, params object[] arguments)
    {
        try { return Provider.GetType().GetMethod(name, BindingFlags.Public | BindingFlags.Instance).Invoke(Provider, arguments); }
        catch (TargetInvocationException error) { throw error.InnerException; }
    }
    private static void ExpectFailure(Action action)
    {
        try { action(); }
        catch (InvalidOperationException) { return; }
        throw new Exception("Expected the injected collaborator failure.");
    }
    private static void Check(bool condition, string description)
    {
        if (!condition) throw new Exception("FAIL " + description);
        Results.Add("PASS " + description);
    }
    private static Type LoadOptionalApi()
    {
        var assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("OrbitRender.TestDouble"), AssemblyBuilderAccess.Run);
        var type = assembly.DefineDynamicModule("Overlay").DefineType("OrbitRender.OverlayApi", TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.Sealed);
        type.DefineField("ApiVersion", typeof(int), FieldAttributes.Public | FieldAttributes.Static);
        Forward(type, "Register", typeof(bool), typeof(string), typeof(object));
        Forward(type, "Unregister", typeof(void), typeof(string));
        Forward(type, "IsRegistered", typeof(bool), typeof(string));
        var property = type.DefineProperty("IsRendering", PropertyAttributes.None, typeof(bool), Type.EmptyTypes);
        property.SetGetMethod(Forward(type, "get_IsRendering", typeof(bool)));
        return type.CreateType();
    }
    private static MethodBuilder Forward(TypeBuilder type, string name, Type result, params Type[] parameters)
    {
        var method = type.DefineMethod(name, MethodAttributes.Public | MethodAttributes.Static, result, parameters);
        var body = method.GetILGenerator();
        for (int i = 0; i < parameters.Length; i++) body.Emit(OpCodes.Ldarg, i);
        body.Emit(OpCodes.Call, typeof(OrbitRenderTestApi).GetMethod(name));
        body.Emit(OpCodes.Ret);
        return method;
    }
}

public static class OrbitRenderTestApi
{
    public static object Provider;
    public static string LastId;
    public static bool Rendering, RejectRegistration, ThrowUnregister;
    public static int RegisterCalls, UnregisterCalls;
    public static bool get_IsRendering() => Rendering;
    public static bool Register(string id, object provider)
    {
        RegisterCalls++;
        if (RejectRegistration) return false;
        LastId = id;
        Provider = provider;
        return true;
    }
    public static bool IsRegistered(string id) => Provider != null && id == LastId;
    public static void Unregister(string id)
    {
        UnregisterCalls++;
        if (ThrowUnregister) throw new InvalidOperationException("API unavailable during unregister");
        if (id == LastId) Provider = null;
    }
}

namespace UnityEngine
{
    public class Object { public static implicit operator bool(Object value) => value != null; }
    public sealed class Canvas : Object { }
    public sealed class GameObject : Object
    {
        public Canvas Canvas;
        public T GetComponent<T>() where T : class => Canvas as T;
    }
    public enum KeyCode { None = 0, Return = 13, A = 97, KeypadEnter = 271 }
}
namespace DonQuixoteOverlay
{
    internal static class Main
    {
        internal static bool Enabled;
        internal static readonly List<string> Messages = new List<string>();
        internal static void Log(string message) => Messages.Add(message);
    }
    internal static class AttemptTracker
    {
        internal static bool Rendering;
        internal static int Begins, Ends;
        internal static void BeginRenderSession() { Rendering = true; Begins++; }
        internal static void EndRenderSession() { Rendering = false; Ends++; }
    }
    internal static class ChatterRuntime
    {
        internal static bool RenderSuspended;
        internal static void Reset() { }
    }
    internal static class LayoutEditorController
    {
        internal static bool Active;
        internal static void CancelActive() { Active = false; }
    }
    internal sealed class SettingsWindow
    {
        internal static SettingsWindow Instance;
        internal bool Visible;
        internal void Toggle() { Visible = !Visible; }
    }
    internal sealed class OverlayController
    {
        internal static OverlayController Instance;
        internal Canvas CaptureCanvas;
    }
}
namespace DonQuixoteOverlay.KeyViewerContents
{
    internal sealed class KeyViewer
    {
        internal static KeyViewer Instance;
        internal GameObject KeyViewerObject;
        internal bool Replay, FailBegin, FailEnd, EndSawReplay, EndSawRendering;
        internal int Refreshes;
        internal readonly List<InputEdge> Inputs = new List<InputEdge>();
        internal void SetReplayInput(bool replay)
        {
            Replay = replay;
            if (!replay)
            {
                EndSawReplay = OrbitRenderBridge.IsReplayActive;
                EndSawRendering = OrbitRenderBridge.IsRendering;
            }
            if (replay ? FailBegin : FailEnd) throw new InvalidOperationException("injected viewer lifecycle failure");
        }
        internal void ApplyReplayKey(KeyCode code, bool down, long ticks) => Inputs.Add(new InputEdge { Code = code, Down = down, Ticks = ticks });
        internal void RefreshReplayFrame() { Refreshes++; }
        internal sealed class InputEdge { internal KeyCode Code; internal bool Down; internal long Ticks; }
    }
}
