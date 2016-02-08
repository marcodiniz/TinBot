
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace TinBot.Portable
{
    public class ActionsContainer
    {
        public List<MovementAcion> MovementAcions { get; set; } = new List<MovementAcion>();
        public List<FaceAction> FaceActions { get; set; } = new List<FaceAction>();
        public List<SpeakAction> SpeakActions { get; set; } = new List<SpeakAction>();
        public List<SavedAction> SavedActions { get; set; } = new List<SavedAction>();
        public List<SequenceAction> SequenceActions { get; set; } = new List<SequenceAction>();

        public List<TinBotAction> AllActions()
        {
            return MovementAcions.Cast<TinBotAction>()
                .Union(FaceActions)
                .Union(SpeakActions)
                .Union(SavedActions)
                .Union(SequenceActions)
                .ToList();
        }
    }

    public enum EActionType
    {
        Speak,
        Face,
        Move,
        Saved,
        Sequence
    }

    public abstract class TinBotAction
    {
     public EActionType Type { get; set; }

        public string Name { get; set; }

        protected TinBotAction()
        {
            
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

        public SpeakAction()
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

        public MovementAcion()
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

        public FaceAction()
        {
            
        }
        public FaceAction(ETinBotFaces tinBotFaces, string name = null) : base(EActionType.Face, name)
        {
            TinBotFaces = tinBotFaces;
        }
    }

    public class SavedAction : TinBotAction
    {
        public EActionType ActionType { get; set; }
        public string ActionName { get; set; }

        public SavedAction()
        {
            
        }
        public SavedAction(string actionName, EActionType actionType = EActionType.Saved) : base(EActionType.Saved, null)
        {
            ActionType = actionType;
            ActionName = actionName;
        }
    }

    public class SequenceAction : TinBotAction
    {
        public List<List<TinBotAction>> Sequence { get; set; } = new List<List<TinBotAction>>();

        public SequenceAction()
        {
            
        }
       
        public SequenceAction(string name) : base(EActionType.Sequence, name)
        {
        }
    }
}
