using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maker.RemoteWiring;
using static TinBot.Helpers.DispatcherHelper;


namespace TinBot.Helpers
{
    public static class FirmataHelpers
    {
        public static PinState ToPinState(this bool value)
        {
            return value ? PinState.HIGH : PinState.LOW;
        }

        public static async Task DigitalPulseAwaitable(this RemoteDevice device, byte port, int duration = 10)
        {
            await device.DigitalWriteAwaitable(port, true, duration);
            await device.DigitalWriteAwaitable(port, false, duration);
        }

        public static async Task DigitalWriteAwaitable(this RemoteDevice device, byte port, bool value, int releaseTime = 10)
        {
            await ExecuteOnMainThread(() => device.digitalWrite(port, value.ToPinState()));
            await Task.Delay(releaseTime);
        }
    }
}
