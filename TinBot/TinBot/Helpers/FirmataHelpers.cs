using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maker.RemoteWiring;
using static TinBot.Helpers.DispatcherHelper;


namespace TinBot.Helpers
{
    public static class FirmataHelpers
    {
        public static object _lock = new object();

        public static PinState ToPinState(this bool value)
        {
            return value ? PinState.HIGH : PinState.LOW;
        }

        public static async Task DigitalPulseAwaitable(this RemoteDevice device, byte port, int duration = 1)
        {
            await device.DigitalWriteAwaitable(port, true, duration);
            await Task.Delay(duration);
            await device.DigitalWriteAwaitable(port, false, duration);
            await Task.Delay(duration);
        }

        public static void SafePinMode(this RemoteDevice device, byte pin, PinMode pinMode)
        {
            lock (_lock)
            {
                device.pinMode(pin,pinMode);
            }
        }

        public static void SafePinMode(this RemoteDevice device, string pin, PinMode pinMode)
        {
            lock (_lock)
            {
                device.pinMode(pin, pinMode);
            }
        }

        public static async Task DigitalWriteAwaitable(this RemoteDevice device, byte port, bool value, int releaseTime = 1)
        {
            //await ExecuteOnMainThread(() => device.digitalWrite(port, value.ToPinState()));
            lock (_lock)
            {
                device.digitalWrite(port, value.ToPinState());
            }
        }

        public static async Task AnalogWriteAwaitable(this RemoteDevice device, byte port, ushort value, int releaseTime = 1)
        {
            lock (_lock)
            {
                try
                {
                    device.analogWrite(port, value);
                }
                catch (Exception ex)
                {
                } 
            }
        }


    }
}
