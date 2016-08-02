using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Microsoft.Maker.RemoteWiring;
using TinBot.Helpers;
using static System.Math;
using static TinBot.Helpers.DispatcherHelper;

namespace TinBot.Operations
{
    public class ServoController
    {

        public ServoController(RemoteDevice device, byte pin, bool inverse = false, int delay = 5)
        {
            _inverse = inverse;
            Device = device;
            Pin = pin;
            _delay = delay;

            Deattach();
        }

        private readonly bool _inverse;
        public RemoteDevice Device { get; }
        public byte Pin { get; }
        public decimal TargetPosition { get; private set; }
        public decimal CurrentPosition { get; private set; } = 90;
        public decimal Speed { get; set; } = 10;
        public bool IsAttached { get; set; } = false;

        private int _delay;

        private int _direction;

        public ServoController()
        {
        }

        public async Task Attach()
        {
            await ExecuteOnMainThread(() => Device.SafePinMode(Pin, PinMode.SERVO));
            IsAttached = true;
            ushort p = (ushort)(_inverse ? 180 - CurrentPosition: CurrentPosition);
            await Device.AnalogWriteAwaitable(Pin, p);
        }

        public async Task Deattach()
        {
            await ExecuteOnMainThread(() => Device.SafePinMode(Pin, PinMode.INPUT));
            IsAttached = false;
        }

        public async Task Move(int targetPosition, int speed = 10, int acceleration = 1)
        {
            if (!IsAttached)
                await Attach();

            if (speed == 0 | acceleration == 0)
                speed = acceleration = 1;

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
                currentSpeed = Math.Min(currentSpeed + acceleration / 10m, speed);
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


            await Task.Run(() =>
            {
                foreach (var position in positions)
                {
                    ushort p = (ushort)(_inverse ? 180 - position : position);
                    //var execute = Execut eOnMainThread(() => Device.analogWrite(Pin, p));
                    Device.AnalogWriteAwaitable(Pin, p);
                    //Device.analogWrite(Pin, p);
                    Task.Delay(_delay).Wait();
                    //await Task.Delay(_delay);
                    //await Task.WhenAll(execute, delay);
                }
            });

            var extraTime = (Abs(CurrentPosition - targetPosition) - positions.Count);
            for (int i = 0; i < (extraTime > 0 ? 15 - extraTime : 0); i++)
            {
                await Task.Delay(_delay);
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
