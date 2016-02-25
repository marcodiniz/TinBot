
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;

namespace TinBot.Portable
{
    public class ActionContainer
    {
        public MovementAcion MovementAcion { get; set; }
        public FaceAction FaceAction { get; set; }
        public SpeakAction SpeakAction { get; set; }
        public SavedAction SavedAction { get; set; }
        public SequenceAction SequenceAction { get; set; }
        public ToggleAction ToggleAction { get; set; }

        public ActionContainer()
        {

        }

        public ActionContainer(TinBotAction action)
        {
            MovementAcion = action as MovementAcion;
            FaceAction = action as FaceAction;
            SpeakAction = action as SpeakAction;
            SavedAction = action as SavedAction;
            SequenceAction = action as SequenceAction;
            ToggleAction = action as ToggleAction;
        }

        public TinBotAction GetAction()
        {
            return MovementAcion ?? FaceAction ?? SpeakAction ?? SavedAction ?? SequenceAction ?? ToggleAction 
                as TinBotAction;
        }
    }

    public class ActionsContainer
    {
        public List<MovementAcion> MovementAcions { get; set; } = new List<MovementAcion>();
        public List<FaceAction> FaceActions { get; set; } = new List<FaceAction>();
        public List<SpeakAction> SpeakActions { get; set; } = new List<SpeakAction>();
        public List<SavedAction> SavedActions { get; set; } = new List<SavedAction>();
        public List<SequenceAction> SequenceActions { get; set; } = new List<SequenceAction>();
        public List<ToggleAction> ToggleActions{ get; set; } = new List<ToggleAction>();

        public List<ListenKey>  ListenKeys { get; set; } = new List<ListenKey>();

        public List<TinBotAction> AllActions()
        {
            return MovementAcions.Cast<TinBotAction>()
                .Union(FaceActions)
                .Union(SpeakActions)
                .Union(SavedActions)
                .Union(SequenceActions)
                .Union(ToggleActions)
                .ToList();
        }

        public TinBotAction this[string name]
        {
            get { return AllActions().FirstOrDefault(x => x.Name.Equals(name,StringComparison.OrdinalIgnoreCase)); }
        }
    }

    public enum EActionType
    {
        None=0,
        Face = 1,
        Move,
        Saved,
        Sequence,
        Toggle,
        Speak
    }

    public abstract class TinBotAction
    {
        public EActionType Type { get; set; }
        public string Name { get; set; }
        public bool IgnoreBlockingInSeries { get; set; }
        public int ExtraWaitTime { get; set; }
        public int Repeat { get; set; } = 1;

        protected TinBotAction(EActionType type)
        {
            Type = type;
        }

        protected TinBotAction(EActionType type, string name)
        {
            Type = type;
            Name = name;
        }

        public override string ToString()
        {
            return $"{Type} - {Name}";
        }

    }

    public class SpeakAction : TinBotAction
    {
        public string Text { get; set; }

        public SpeakAction() : base(EActionType.Speak)
        {

        }

        public SpeakAction(string text, string name = null) : base(EActionType.Speak, name)
        {
            Text = text;
        }
    }

    public enum ETinBotServo
    {
        None,
        ServoHand,
        ServoRightArm,
        ServoLeftArm,
        ServoTorso,
        ServoHeadY,
        ServoHeadX
    }

    public class MovementAcion : TinBotAction
    {
        public ETinBotServo Servo { get; set; }
        public int TargetPosition { get; set; }
        public int Speed { get; set; }
        public int Acceleratton { get; set; }

        public MovementAcion() : base(EActionType.Move)
        {

        }
        public MovementAcion(ETinBotServo servo = ETinBotServo.None, int targetPosition = 90, int speed = 10, int acceleratton = 2, string name = null) : base(EActionType.Move, name)
        {
            Acceleratton = acceleratton;
            Speed = speed;
            TargetPosition = targetPosition;
            Servo = servo;
        }
    }

    public enum ETinBotFaces
    {
        Angry,
        Happy,
        HappyGreen,
        Normal,
        Blink,
        UniBlink,
        BlinkDouble,
        Sad
    }

    public class FaceAction : TinBotAction
    {
        public ETinBotFaces TinBotFaces { get; set; }

        public FaceAction() : base(EActionType.Face)
        {

        }
        public FaceAction(ETinBotFaces tinBotFaces, string name = null) : base(EActionType.Face, name)
        {
            TinBotFaces = tinBotFaces;
        }
    }

    public class SavedAction : TinBotAction
    {
        public string ActionName { get; set; }

        public SavedAction() : base(EActionType.Saved)
        {

        }
        public SavedAction(string actionName, int repeat = 1) : base(EActionType.Saved, null)
        {
            Repeat = repeat;
            ActionName = actionName;
        }
    }

    public class SequenceAction : TinBotAction
    {
        public List<List<ActionContainer>> Sequence { get; set; } = new List<List<ActionContainer>>();

        public SequenceAction() : base(EActionType.Sequence)
        {

        }

        public SequenceAction(string name) : base(EActionType.Sequence, name)
        {
        }

        public void AddPararellActions(params TinBotAction[] actions)
        {
            Sequence.Add(actions.Select(x => new ActionContainer(x)).ToList());
        }
    }

    public enum ETinBotToggle
    {
        None,
        Blue,
        Red,
        Green,
        Laser
    }

    public class ToggleAction : TinBotAction
    {
        public ETinBotToggle TinBotToggle { get; set; }
        public int TimeOn { get; set; }
        public int TimeOff { get; set; }
        public int Repeat { get; set; }

        public ToggleAction() : base(EActionType.Toggle)
        {
        }

        public ToggleAction(ETinBotToggle tinBotToggle, int timeOn, int timeOff, int repeat=0, string name=null) : base(EActionType.Toggle, name)
        {
            TinBotToggle = tinBotToggle;
            TimeOn = timeOn;
            TimeOff = timeOff;
            Repeat = repeat;
        }
    }

    public class ListenKey
    {
        public string ListenFor { get; set; }
        public string Do { get; set; }
    }

}
