using System;
using System.Collections.Generic;
using Godot;

namespace Dialogic;

/// <summary>
/// A builder class for constructing `DialogicTimeline` resources
/// </summary>
public class DialogicTimeline
{
    public List<Event> Events
    {
        get;
        private set;
    } = new();

    /// <summary>
    /// Combines all currently built events into a single timeline and returns the timeline resource.
    /// </summary>
    /// <returns>a resource instance of the timeline. Since it's a GDScript class, that's the best we can do</returns>
    public Resource GetResource
    {
        get
        {
            Resource timelineRes = new();

            timelineRes.SetScript(GD.Load<Script>("res://addons/dialogic/Resources/timeline.gd"));

            bool flagUsesEvents = false;

            Godot.Collections.Array eventsArr = new();

            foreach (Event @event in Events)
            {
                if (@event is EventRaw eventRaw)
                {
                    eventsArr.Add(eventRaw.rawTextCode); // Only case that isn't stored as a resource type
                }
                else
                {
                    flagUsesEvents = true;
                    var eventRes = new Resource();
                    eventRes.SetScript(GetScriptFor(@event));
                    @event.Apply(ref eventRes);
                    eventsArr.Add(eventRes);
                }
            }

            timelineRes.Set(PropertyNames.Events, eventsArr);

            if (flagUsesEvents)
            {
                // if we are passing events, this tells the Timeline that it doesn't need to parse text into events on its own
                timelineRes.Set(PropertyNames.EventsProcessed, true);
            }

            return timelineRes;
        }
    }

    private static Script GetScriptFor(Event @event)
    {
        Script resource = @event switch
        {
            EventText => GD.Load<Script>("res://addons/dialogic/Modules/Text/event_text.gd"),
            EventWaitInput => GD.Load<Script>("res://addons/dialogic/Modules/WaitInput/event_wait_input.gd"),
            EventWait => GD.Load<Script>("res://addons/dialogic/Modules/Wait/event_wait.gd"),
            EventVoice => GD.Load<Script>("res://addons/dialogic/Modules/Voice/event_voice.gd"),
            EventVariable => GD.Load<Script>("res://addons/dialogic/Modules/Variable/event_variable.gd"),
            EventTextInput => GD.Load<Script>("res://addons/dialogic/Modules/TextInput/event_text_input.gd"),
            EventSignal => GD.Load<Script>("res://addons/dialogic/Modules/Signal/event_signal.gd"),
            EventSetting => GD.Load<Script>("res://addons/dialogic/Modules/Settings/event_setting.gd"),
            EventSave => GD.Load<Script>("res://addons/dialogic/Modules/Save/event_save.gd"),
            EventReturn => GD.Load<Script>("res://addons/dialogic/Modules/Jump/event_return.gd"),
            EventLabel => GD.Load<Script>("res://addons/dialogic/Modules/Jump/event_label.gd"),
            EventJump => GD.Load<Script>("res://addons/dialogic/Modules/Jump/event_jump.gd"),
            EventHistory => GD.Load<Script>("res://addons/dialogic/Modules/History/event_history.gd"),
            EventGlossary => GD.Load<Script>("res://addons/dialogic/Modules/Glossary/event_glossary.gd"),
            EventEndBranch => GD.Load<Script>("res://addons/dialogic/Modules/Core/event_end_branch.gd"),
            EventEnd => GD.Load<Script>("res://addons/dialogic/Modules/Core/event_end_branch.gd"),
            EventCondition => GD.Load<Script>("res://addons/dialogic/Modules/Condition/event_condition.gd"),
            EventComment => GD.Load<Script>("res://addons/dialogic/Modules/Comment/event_comment.gd"),
            EventChoice => GD.Load<Script>("res://addons/dialogic/Modules/Choice/event_choice.gd"),
            EventPosition => GD.Load<Script>("res://addons/dialogic/Modules/Character/event_position.gd"),
            EventCharacter => GD.Load<Script>("res://addons/dialogic/Modules/Character/event_character.gd"),
            EventCallNode => GD.Load<Script>("res://addons/dialogic/Modules/CallNode/event_call_node.gd"),
            EventBackground => GD.Load<Script>("res://addons/dialogic/Modules/Background/event_background.gd"),
            EventSound => GD.Load<Script>("res://addons/dialogic/Modules/Audio/event_sound.gd"),
            EventMusic => GD.Load<Script>("res://addons/dialogic/Modules/Audio/event_music.gd"),
            _ => throw new NotImplementedException("Unexpected event type!")
        };
        return resource;
    }

    public abstract record Event()
    {
        public abstract void Apply(ref Resource resource);
    }

    public sealed record EventRaw(string rawTextCode) : Event
    {
        public override void Apply(ref Resource resource) { }
    }

    public sealed record EventText(string text = "", Resource character = null, string portrait = "") : Event
    {
        public override void Apply(ref Resource resource)
        {
            resource.Set(PropertyNames.Text, text);
            resource.Set(PropertyNames.Character, character);
            resource.Set(PropertyNames.Portrait, portrait);
        }
    }

    public sealed record EventWaitInput(bool hideTextBox = true) : Event
    {
        public override void Apply(ref Resource resource) => resource.Set(PropertyNames.HideTextbox, hideTextBox);
    }

    public sealed record EventWait(float time = 1, bool hideText = true) : Event
    {
        public override void Apply(ref Resource resource)
        {
            resource.Set(PropertyNames.Time, time);
            resource.Set(PropertyNames.HideText, hideText);
        }
    }

    public sealed record EventVoice(string filePath = "", float volume = 0, string audioBus = "Master") : Event
    {
        public override void Apply(ref Resource resource)
        {
            resource.Set(PropertyNames.FilePath, filePath);
            resource.Set(PropertyNames.Volume, volume);
            resource.Set(PropertyNames.AudioBus, audioBus);
        }
    }

    public sealed record EventVariable(string name = "", EventVariable.Operations operation = EventVariable.Operations.SET, Variant value = new(),
        float randomMin = 0, float randomMax = 100) : Event
    {
        public override void Apply(ref Resource resource)
        {
            resource.Set(PropertyNames.Name, name);
            resource.Set(PropertyNames.Operation, (int)operation);
            resource.Set(PropertyNames.Value, value);
            resource.Set(PropertyNames.RandomMin, randomMin);
            resource.Set(PropertyNames.RandomMax, randomMax);
        }

        public enum Operations
        {
            SET,
            ADD,
            SUBTRACT,
            MULTIPLY,
            DIVIDE
        };
    }

    public sealed record EventTextInput(string textPrompt = "Please enter some text:", string variable = "", string placeholder = "",
        string @default = "", bool allowEmpty = false) : Event
    {
        public override void Apply(ref Resource resource)
        {
            resource.Set(PropertyNames.Text, textPrompt);
            resource.Set(PropertyNames.Variable, variable);
            resource.Set(PropertyNames.Placeholder, placeholder);
            resource.Set(PropertyNames.Default, @default);
            resource.Set(PropertyNames.AllowEmpty, allowEmpty);
        }
    }

    public sealed record EventSignal(string argument = "") : Event
    {
        public override void Apply(ref Resource resource) => resource.Set(PropertyNames.Argument, argument);
    }

    public sealed record EventSetting(string name = "", Variant value = new(),
        EventSetting.Modes mode = EventSetting.Modes.SET) : Event
    {
        public override void Apply(ref Resource resource)
        {
            resource.Set(PropertyNames.Name, name);
            resource.Set(PropertyNames.Value, value);
            resource.Set(PropertyNames.Mode, (int)mode);
        }

        public enum Modes
        {
            SET,
            RESET,
            RESET_ALL
        }
    }
    public sealed record EventSave(string saveSlot = "") : Event
    {
        public override void Apply(ref Resource resource) => resource.Set(PropertyNames.SlotName, saveSlot);
    }

    public sealed record EventReturn() : Event
    {
        public override void Apply(ref Resource resource) { }
    }

    public sealed record EventLabel(string Name = "") : Event
    {
        public override void Apply(ref Resource resource) => resource.Set(PropertyNames.Name, Name);
    }

    public sealed record EventJump(string LabelName = "", Resource TimelineTarget = null) : Event
    {
        public override void Apply(ref Resource resource)
        {
            resource.Set(PropertyNames.Timeline, TimelineTarget);
            resource.Set(PropertyNames.LabelName, LabelName);
        }
    }

    public sealed record EventHistory(EventHistory.Actions Action = EventHistory.Actions.PAUSE) : Event
    {
        public override void Apply(ref Resource resource) => resource.Set(PropertyNames.Action, (int)Action);

        public enum Actions
        {
            CLEAR,
            PAUSE,
            RESUME
        }
    }

    public sealed record EventGlossary() : Event
    {
        public override void Apply(ref Resource resource) { }
    }

    public sealed record EventEndBranch() : Event
    {
        public override void Apply(ref Resource resource) { }
    }

    public sealed record EventEnd() : Event
    {
        public override void Apply(ref Resource resource) { }
    }

    public sealed record EventCondition(EventCondition.ConditionType conditionType = EventCondition.ConditionType.IF, string condition = "") : Event
    {
        public override void Apply(ref Resource resource)
        {
            resource.Set(PropertyNames.ConditionType, (int)conditionType);
            resource.Set(PropertyNames.Condition, condition);
        }

        public enum ConditionType
        {
            IF,
            ELIF,
            ELSE
        }
    }

    public sealed record EventComment(string text = "") : Event
    {
        public override void Apply(ref Resource resource) => resource.Set(PropertyNames.Text, text);
    }

    public sealed record EventChoice(string text = "", string condition = "",
        EventChoice.ElseActions elseAction = EventChoice.ElseActions.DEFAULT,
        string disabledText = "") : Event
    {
        public override void Apply(ref Resource resource)
        {
            resource.Set(PropertyNames.Text, text);
            resource.Set(PropertyNames.Condition, condition);
            resource.Set(PropertyNames.ElseAction, (int)elseAction);
            resource.Set(PropertyNames.DisabledText, disabledText);
        }

        public enum ElseActions
        {
            HIDE,
            DISABLE,
            DEFAULT
        }
    }

    public sealed record EventPosition(EventPosition.Actions action, int position,
        Vector2 vector, float movementTime) : Event
    {
        public override void Apply(ref Resource resource)
        {
            resource.Set(PropertyNames.Action, (int)action);
            resource.Set(PropertyNames.Position, position);
            resource.Set(PropertyNames.Vector, vector);
            resource.Set(PropertyNames.MovementTime, movementTime);
        }

        public enum Actions
        {
            SET_RELATIVE,
            SET_ABSOLUTE,
            RESET,
            RESET_ALL
        }
    }

    public sealed record EventCharacter(EventCharacter.Actions action = EventCharacter.Actions.JOIN,
        Resource character = null, string portrait = "",
        int position = 1, string animationName = "", float animationLength = .5f,
        int animationRepeats = 1, bool animationWait = false, float positionMoveTime = 0,
        int zIndex = 0, bool mirror = false, string extraData = "") : Event
    {
        public override void Apply(ref Resource resource)
        {
            resource.Set(PropertyNames.Action, (int)action);
            resource.Set(PropertyNames.Character, character);
            resource.Set(PropertyNames.Portrait, portrait);
            resource.Set(PropertyNames.Position, position);
            resource.Set(PropertyNames.AnimationName, animationName);
            resource.Set(PropertyNames.AnimationLength, animationLength);
            resource.Set(PropertyNames.AnimationRepeats, animationRepeats);
            resource.Set(PropertyNames.AnimationWait, animationWait);
            resource.Set(PropertyNames.PositionMoveTime, positionMoveTime);
            resource.Set(PropertyNames.ZIndex, zIndex);
            resource.Set(PropertyNames.Mirrored, mirror);
            resource.Set(PropertyNames.ExtraData, extraData);
        }

        public enum Actions
        {
            JOIN,
            LEAVE,
            UPDATE
        }
    }
    public sealed record EventCallNode(string path, string method, Godot.Collections.Array args,
        bool wait, bool inline, string inlineSignalArgument, bool inlineSignalUse) : Event
    {
        public override void Apply(ref Resource resource)
        {
            resource.Set(PropertyNames.Path, path);
            resource.Set(PropertyNames.Method, method);
            resource.Set(PropertyNames.Arguments, args);
            resource.Set(PropertyNames.Wait, wait);
            resource.Set(PropertyNames.Inline, inline);
            resource.Set(PropertyNames.InlineSignalArgument, inlineSignalArgument);
            resource.Set(PropertyNames.InlineSignalUse, inlineSignalUse);
        }
    }

    public sealed record EventBackground(string scene = "", string argument = "", string transition = "", float fade = 0) : Event
    {
        public override void Apply(ref Resource resource)
        {
            resource.Set(PropertyNames.Scene, scene);
            resource.Set(PropertyNames.Argument, argument);
            resource.Set(PropertyNames.Fade, fade);
            resource.Set(PropertyNames.Transition, transition);
        }
    }

    public sealed record EventSound(string filePath, float volume, string audioBus, bool loop) : Event
    {
        public override void Apply(ref Resource resource)
        {
            resource.Set(PropertyNames.FilePath, filePath);
            resource.Set(PropertyNames.Volume, volume);
            resource.Set(PropertyNames.AudioBus, audioBus);
            resource.Set(PropertyNames.Loop, loop);
        }
    }

    public sealed record EventMusic(string filePath = "", float volume = 0,
        string audioBus = "", bool loop = false) : Event
    {
        public override void Apply(ref Resource resource)
        {
            resource.Set(PropertyNames.FilePath, filePath);
            resource.Set(PropertyNames.Volume, volume);
            resource.Set(PropertyNames.AudioBus, audioBus);
            resource.Set(PropertyNames.Loop, loop);
        }
    }

    public static class PropertyNames
    {
        public const string Events = "events";
        public const string EventsProcessed = "events_processed";
        public const string Text = "text";
        public const string Character = "character";
        public const string Portrait = "portrait";
        public const string HideTextbox = "hide_textbox";
        public const string Time = "time";
        public const string HideText = "hide_text";
        public const string FilePath = "file_path";
        public const string Volume = "volume";
        public const string AudioBus = "audio_bus";
        public const string Name = "name";
        public const string Operation = "operation";
        public const string Value = "value";
        public const string RandomMin = "random_min";
        public const string RandomMax = "random_max";
        public const string Placeholder = "placeholder";
        public const string Default = "default";
        public const string AllowEmpty = "allow_empty";
        public const string Argument = "argument";
        public const string Mode = "mode";
        public const string SlotName = "slot_name";
        public const string LabelName = "label_name";
        public const string Timeline = "timeline";
        public const string ConditionType = "condition_type";
        public const string Condition = "condition";
        public const string ElseAction = "else_action";
        public const string DisabledText = "disabled_text";
        public const string Position = "position";
        public const string Vector = "vector";
        public const string MovementTime = "movement_time";
        public const string AnimationName = "animation_name";
        public const string AnimationLength = "animation_length";
        public const string AnimationRepeats = "animation_repeats";
        public const string AnimationWait = "animation_wait";
        public const string PositionMoveTime = "position_move_time";
        public const string ZIndex = "z_index";
        public const string Mirrored = "mirrored";
        public const string ExtraData = "extra_data";
        public const string Path = "path";
        public const string Method = "method";
        public const string Arguments = "arguments";
        public const string Wait = "wait";
        public const string Inline = "inline";
        public const string InlineSignalArgument = "inline_signal_argument";
        public const string InlineSignalUse = "inline_signal_use";
        public const string Scene = "scene";
        public const string ArgumentFade = "argument_fade";
        public const string Loop = "loop";
        public const string FadeLength = "fade_length";
        public const string Variable = "variable";
        public const string Action = "action";
        public const string Fade = "fade";
        public const string Transition = "transition";
    }

    public static class MethodNames
    {
        public const string Apply = "Apply";
        public const string SetScript = "SetScript";
        public const string Set = "Set";
    }

    public static class SignalNames
    {
        public const string Signal = "signal";
        public const string CallNode = "call_node";
    }
}
