using System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Microsoft.Maker.RemoteWiring;
using static System.Math;
using static TinBot.Helpers.DispatcherHelper;

namespace TinBot.Commands
{
    public class ServoController
    {

        public ServoController(RemoteDevice device, byte pin, bool inverse = false)
        {
            _inverse = inverse;
            Device = device;
            Pin = pin;

            _timer.Interval = TimeSpan.FromMilliseconds(5);
            _timer.Tick += TimerOnTick;

        }

        private readonly bool _inverse;
        public RemoteDevice Device { get; }
        public byte Pin { get; }
        public decimal TargetPosition { get; private set; }
        public decimal CurrentPosition { get; private set; } = 90;
        public decimal Speed { get; set; } = 10;
        public decimal CurrentSpeed { get; set; } = 1;
        public decimal Acceleration { get; set; }

        private readonly DispatcherTimer _timer = new DispatcherTimer();
        private int _direction;

        public void Attach()
        {
            Device.pinMode(Pin, PinMode.SERVO);
        }

        public void Deattach()
        {
            Device.pinMode(Pin, PinMode.INPUT);
        }

        public void Move(int targetPosition, int speed = 10, int acceleration = 1)
        {
            _timer.Stop();

            Acceleration = acceleration;
            TargetPosition = targetPosition;
            Speed = speed;
            CurrentSpeed = 0;

            _direction = (targetPosition >= CurrentPosition) ? 1 : -1;

            _timer.Start();
        }

        private void TimerOnTick(object sender, object o)
        {
            if (CurrentPosition == TargetPosition)
            {
                _timer.Stop();
                return;
            }
            
            //deacceleration
            if (CurrentPosition + CurrentSpeed >= TargetPosition)
            {
                CurrentSpeed = Max(1, CurrentSpeed - Acceleration / 10);
            }
            else if (CurrentSpeed < Speed) //acceleration
            {
                CurrentSpeed = Min(Speed, CurrentSpeed + Acceleration / 10);
            }

            CurrentPosition = (CurrentPosition + CurrentSpeed * _direction);
            CurrentPosition = _direction > 0 ? Min(CurrentPosition, TargetPosition) : Max(CurrentPosition, TargetPosition);
            if (CurrentPosition > 180)
                CurrentPosition = 180;
            if (CurrentPosition < 0)
                CurrentPosition = 0;

            ushort position = (ushort)(_inverse ? 180 - CurrentPosition : CurrentPosition);
            ExecuteOnMainThread(() => Device.analogWrite(Pin, position));
        }
    }
}
