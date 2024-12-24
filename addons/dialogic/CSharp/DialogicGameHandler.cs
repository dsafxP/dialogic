using System;
using Godot;
using Godot.Collections;

namespace Dialogic;

public class DialogicGameHandler : IDisposable
{
    private readonly Node _dialogic = null;

    private bool _disposed = false;

    // Cache signal connections to allow disconnection during disposal
    private readonly Callable _dialogicPausedCallable;
    private readonly Callable _dialogicResumedCallable;
    private readonly Callable _stateChangedCallable;
    private readonly Callable _timelineEndedCallable;
    private readonly Callable _timelineStartedCallable;
    private readonly Callable _eventHandledCallable;
    private readonly Callable _signalEventCallable;
    private readonly Callable _textSignalCallable;

    // Signals
    public event Action DialogicPaused;
    public event Action DialogicResumed;
    public event Action<int> StateChanged;
    public event Action TimelineEnded;
    public event Action TimelineStarted;
    public event Action<Resource> EventHandled;
    public event Action<string> SignalEvent;
    public event Action<string> TextSignal;

    // Properties
    public string CurrentTimeline => _dialogic?.Get(PropertyNames.CurrentTimeline).AsString();
    public States CurrentState => (States)(_dialogic?.Get(PropertyNames.CurrentState).AsInt32() ?? 0);
    public bool Paused => _dialogic?.Get(PropertyNames.Paused).AsBool() ?? false;

    public DialogicGameHandler(Node node)
    {
        _dialogic = node.GetNode("/root/Dialogic") ?? throw new ArgumentNullException(nameof(node), "Dialogic singleton not found.");

        // Initialize callables
        _dialogicPausedCallable = Callable.From(() => DialogicPaused?.Invoke());
        _dialogicResumedCallable = Callable.From(() => DialogicResumed?.Invoke());
        _stateChangedCallable = Callable.From<int>((state) => StateChanged?.Invoke(state));
        _timelineEndedCallable = Callable.From(() => TimelineEnded?.Invoke());
        _timelineStartedCallable = Callable.From(() => TimelineStarted?.Invoke());
        _eventHandledCallable = Callable.From<Resource>((res) => EventHandled?.Invoke(res));
        _signalEventCallable = Callable.From<string>((args) => SignalEvent?.Invoke(args));
        _textSignalCallable = Callable.From<string>((args) => TextSignal?.Invoke(args));

        // Connect signals
        ConnectSignal(SignalNames.DialogicPaused, _dialogicPausedCallable);
        ConnectSignal(SignalNames.DialogicResumed, _dialogicResumedCallable);
        ConnectSignal(SignalNames.StateChanged, _stateChangedCallable);
        ConnectSignal(SignalNames.TimelineEnded, _timelineEndedCallable);
        ConnectSignal(SignalNames.TimelineStarted, _timelineStartedCallable);
        ConnectSignal(SignalNames.EventHandled, _eventHandledCallable);
        ConnectSignal(SignalNames.SignalEvent, _signalEventCallable);
        ConnectSignal(SignalNames.TextSignal, _textSignalCallable);
    }

    private void ConnectSignal(string signalName, Callable callable)
    {
        if (!_dialogic.IsConnected(signalName, callable))
        {
            _dialogic.Connect(signalName, callable);
        }
    }

    private void DisconnectSignal(string signalName, Callable callable)
    {
        if (_dialogic.IsConnected(signalName, callable))
        {
            _dialogic.Disconnect(signalName, callable);
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        GC.SuppressFinalize(this);

        // Disconnect signals
        DisconnectSignal(SignalNames.DialogicPaused, _dialogicPausedCallable);
        DisconnectSignal(SignalNames.DialogicResumed, _dialogicResumedCallable);
        DisconnectSignal(SignalNames.StateChanged, _stateChangedCallable);
        DisconnectSignal(SignalNames.TimelineEnded, _timelineEndedCallable);
        DisconnectSignal(SignalNames.TimelineStarted, _timelineStartedCallable);
        DisconnectSignal(SignalNames.EventHandled, _eventHandledCallable);
        DisconnectSignal(SignalNames.SignalEvent, _signalEventCallable);
        DisconnectSignal(SignalNames.TextSignal, _textSignalCallable);

        _disposed = true;
    }

    ~DialogicGameHandler()
    {
        Dispose();
    }

    // Methods
    public Node Start(string timeline) => (Node)_dialogic?.Call(MethodNames.Start, timeline);

    public Node Start(Resource timeline) => (Node)_dialogic?.Call(MethodNames.Start, timeline);

    // Clear the dialogic state
    public void Clear(ClearFlags flags = ClearFlags.FullClear) => _dialogic?.Call(MethodNames.Clear, (int)flags);

    // End the current timeline
    public void EndTimeline() => _dialogic?.Call(MethodNames.EndTimeline);

    // Handle the next event in the timeline
    public void HandleNextEvent() => _dialogic?.Call(MethodNames.HandleNextEvent);

    // Handle a specific event in the timeline
    public void HandleEvent(int eventIndex) => _dialogic?.Call(MethodNames.HandleEvent, eventIndex);

    // Get the full state of the dialogic system
    public Dictionary GetFullState() => (Dictionary)_dialogic?.Call(MethodNames.GetFullState);

    // Load a saved state into the dialogic system
    public void LoadFullState(Dictionary stateInfo) => _dialogic?.Call(MethodNames.LoadFullState, stateInfo);

    // Flags for clearing the state
    public enum ClearFlags
    {
        FullClear,
        KeepVariables,
        TimelineInfoOnly
    }

    // States enum
    public enum States
    {
        Idle,
        ShowingText,
        Animating,
        AwaitingChoice,
        Waiting
    }

    public static class MethodNames
    {
        public const string Start = "start";
        public const string Clear = "clear";
        public const string EndTimeline = "end_timeline";
        public const string HandleNextEvent = "handle_next_event";
        public const string HandleEvent = "handle_event";
        public const string GetFullState = "get_full_state";
        public const string LoadFullState = "load_full_state";
        public const string HasSubsystem = "has_subsystem";
        public const string GetSubsystem = "get_subsystem";
    }

    public static class PropertyNames
    {
        public const string CurrentTimeline = "current_timeline";
        public const string CurrentState = "current_state";
        public const string Paused = "paused";
    }

    public static class SignalNames
    {
        public const string DialogicPaused = "dialogic_paused";
        public const string DialogicResumed = "dialogic_resumed";
        public const string StateChanged = "state_changed";
        public const string TimelineEnded = "timeline_ended";
        public const string TimelineStarted = "timeline_started";
        public const string EventHandled = "event_handled";
        public const string SignalEvent = "signal_event";
        public const string TextSignal = "text_signal";
    }
}
