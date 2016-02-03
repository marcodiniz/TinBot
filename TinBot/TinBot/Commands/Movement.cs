using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinBot.Commands
{
    public enum TinBotServos
    {
        None,
        ServoHand,
        ServoRightArm,
        ServoLeftArm,
        ServoTorso,
        ServoHeadY, 
        ServoHeadX 
    }

    public class Movement
    {
        public Movement(TinBotServos servo, int targetPosition, int speed = 10, int acceleratton = 2, bool blockeable = true)
        {
            Servo = servo;
            TargetPosition = targetPosition;
            Speed = speed;
            Acceleratton = acceleratton;
            Blockeable = blockeable;
        }

        public Movement Next { get; private set; } = null;
        public bool Blockeable { get; private set; }
        public TinBotServos Servo { get; private set; }
        public Body Body { get; private set; } = null;

        public int TargetPosition { get; private set; }
        public int Speed { get; private set; }
        public int Acceleratton{ get; private set; }

        
        
    }
}
