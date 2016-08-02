using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Microsoft.Maker.RemoteWiring;
using TinBot.Helpers;
using static TinBot.Helpers.DispatcherHelper;

namespace TinBot.Operations
{
    public class SerialOutController
    {
        private readonly RemoteDevice _device;
        private readonly byte _dataPin;
        private readonly byte _clockPin;
        private readonly byte _latchPin;
        private readonly byte _numOuputs;

        private List<bool> _data;

        private readonly int _delay = 10;

        public SerialOutController(RemoteDevice device, byte dataPin, byte clockPin, byte latchPin, byte numOuputs = 8)
        {
            _device = device;
            _dataPin = dataPin;
            _clockPin = clockPin;
            _latchPin = latchPin;
            _numOuputs = numOuputs;

            _data = new List<bool>(numOuputs);
            for (int i = 0; i < numOuputs; i++)
                _data.Add(false);

            device.pinMode(dataPin, PinMode.OUTPUT);
        }

        public async Task Reset(bool value = false)
        {
            await Task.Delay(_delay);
            for (int i = 0; i < _numOuputs; i++)
            {
                _data[i] = value;
            }
            await WriteAllToOutputs();
        }

        public async Task DataIn(bool value)
        {
            _data.RemoveAt(_numOuputs - 1);
            _data.Insert(0, value);

            await _device.DigitalWriteAwaitable(_dataPin, value, _delay);
            await _device.DigitalPulseAwaitable(_clockPin, _delay);
        }

        public async Task Latch()
        {
            await _device.DigitalPulseAwaitable(_latchPin, _delay);
        }

        public async Task WriteAllToOutputs()
        {
            await Task.Run(() =>
            {
                for (int i = _numOuputs - 1; i >= 0; i--)
                {
                    _device.DigitalWriteAwaitable(_dataPin, _data[i], _delay).Wait();
                    _device.DigitalPulseAwaitable(_clockPin, _delay).Wait();
                }
                Latch().Wait();
            });
        }

        public bool GetValue(int index) => _data[index];

        public async Task SetValue(int index, bool value, bool imediatlyWriteOutputs = true)
        {
            _data[index] = value;
            if (imediatlyWriteOutputs)
                await WriteAllToOutputs();
        }
    }
}
