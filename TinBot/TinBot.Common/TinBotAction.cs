using System.Collections.Generic;

namespace TinBot.Common
{
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
     public EActionType Type { get; }

        public string Name { get; }

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
        public ETinBotServo Servo { get; }
        public int TargetPosition { get; }
        public int Speed { get; }
        public int Acceleratton { get;}

        public MovementAcion(ETinBotServo servo, int targetPosition, int speed = 10, int acceleratton = 2, string name = null) : base(EActionType.Move, name)
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
        public ETinBotFaces TinBotFaces { get; }

        public FaceAction(ETinBotFaces tinBotFaces, string name = null) : base(EActionType.Face, name)
        {
            TinBotFaces = tinBotFaces;
        }
    }

    public class SavedAction : TinBotAction
    {
        public EActionType ActionType { get; }
        public string ActionName { get; }

        public SavedAction(string actionName, EActionType actionType = EActionType.Saved) : base(EActionType.Saved, null)
        {
            ActionType = actionType;
            ActionName = actionName;
        }
    }

    public class SequenceAction : TinBotAction
    {
        public List<List<TinBotAction>> Sequence { get; } = new List<List<TinBotAction>>();

        public SequenceAction(string name) : base(EActionType.Sequence, name)
        {
        }
    }
}
