using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        }

        private readonly bool _inverse;
        public RemoteDevice Device { get; }
        public byte Pin { get; }
        public decimal TargetPosition { get; private set; }
        public decimal CurrentPosition { get; private set; } = 90;
        public decimal Speed { get; set; } = 10;

        private const int _delay = 10;

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

        public async Task Move(int targetPosition, int speed = 10, int acceleration = 1)
        {
            TargetPosition = targetPosition;
            Speed = speed;
            var currentSpeed = 0m;

            var posStart = CurrentPosition;
            var posEnd = TargetPosition;

            var shouldInvertDirection = (posEnd < posStart);
            if (shouldInvertDirection)
            {
                posEnd = CurrentPosition;
                posStart = targetPosition;
            }

            var positionsAcceleration = new List<decimal>() { posStart };
            var positionsDeacceleration = new List<decimal>();
            var positions = new List<decimal>();

            while (currentSpeed < speed)
            {
                currentSpeed = Math.Min(currentSpeed + acceleration/10m, speed);
                var step = currentSpeed;

                positionsAcceleration.Add(step + positionsAcceleration.Last());
            }
            foreach (var p in positionsAcceleration)
            {
                positionsDeacceleration.Insert(0, posEnd + (posStart - p));
            }

            while (positionsAcceleration.Last() > positionsDeacceleration.First())
            {
                positionsAcceleration.RemoveAt(positionsDeacceleration.Count - 1);
                positionsDeacceleration.RemoveAt(0);
            }

            positions.AddRange(positionsAcceleration);
            while (positions.Last() + speed < positionsDeacceleration.First())
            {
                positions.Add(positions.Last() + speed);
            }
            positions.AddRange(positionsDeacceleration);

            if (shouldInvertDirection)
                positions.Reverse();


            foreach (var position in positions)
            {
                ushort p = (ushort)(_inverse ? 180 - position : position);
                var execute = ExecuteOnMainThread(() => Device.analogWrite(Pin, p));
                var delay = Task.Delay(_delay);
                await Task.WhenAll(execute, delay);
            }

            CurrentPosition = targetPosition;
        }

        //private void TimerOnTick(object sender, object o)
        //{
        //    if (CurrentPosition == TargetPosition)
        //    {
        //        _timer.Stop();
        //        return;
        //    }
            
        //    //deacceleration
        //    if (CurrentPosition + CurrentSpeed >= TargetPosition)
        //    {
        //        CurrentSpeed = Max(1, CurrentSpeed - Acceleration / 10);
        //    }
        //    else if (CurrentSpeed < Speed) //acceleration
        //    {
        //        CurrentSpeed = Min(Speed, CurrentSpeed + Acceleration / 10);
        //    }

        //    CurrentPosition = (CurrentPosition + CurrentSpeed * _direction);
        //    CurrentPosition = _direction > 0 ? Min(CurrentPosition, TargetPosition) : Max(CurrentPosition, TargetPosition);
        //    if (CurrentPosition > 180)
        //        CurrentPosition = 180;
        //    if (CurrentPosition < 0)
        //        CurrentPosition = 0;

            
        //}
    }
}
