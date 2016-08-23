using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Microsoft.Maker.RemoteWiring;
using Microsoft.Maker.Serial;
using TinBot.Helpers;
using TinBot.Portable;
using static TinBot.Helpers.DispatcherHelper;

namespace TinBot.Operations
{
    public class Body
    {
        private const byte _phoneChargePin = 13;

        private readonly UsbSerial _usbSerial;
        private readonly BluetoothSerial _bluetoothSerial;
        public Dictionary<ETinBotServo, ServoController> Servos { get; set; }

        public SerialOutController SerialOut { get; set; }

        public RemoteDevice Arduino { get; private set; }
        public bool IsReady { get; private set; }

        public event EventHandler<string> ConnectionNotify;

        private readonly DispatcherTimer _checkConnectiontimer = new DispatcherTimer();
        private int _failConnectionCount = 0;

        public Body(BluetoothSerial bluetoothSerial)
        {
            _bluetoothSerial = bluetoothSerial;
            _bluetoothSerial.ConnectionEstablished += () => ConnectionNotify?.Invoke(this, "Bluetooth estabelecido");
            _bluetoothSerial.ConnectionFailed += async message =>
             {
                 ConnectionNotify?.Invoke(this, "Bluetooth falhou: " + message);
                 await Task.Delay(5000);
                 Setup();
             };
            _bluetoothSerial.ConnectionLost += message =>
            {
                ConnectionNotify?.Invoke(this, "Bluetooth perdido: " + message);
                Setup();
            };

            _checkConnectiontimer.Interval = TimeSpan.FromSeconds(30);
            _checkConnectiontimer.Tick += CheckConnectiontimerOnTick;
        }

        public Body(UsbSerial usbSerial)
        {
            _usbSerial = usbSerial;
            _usbSerial.ConnectionEstablished += () => ConnectionNotify?.Invoke(this, "USB estabelecido");
            _usbSerial.ConnectionFailed += message =>
             {
                 ConnectionNotify?.Invoke(this, "USB falhou: " + message);
                 Setup();
             };
            _usbSerial.ConnectionLost += message =>
            {
                ConnectionNotify?.Invoke(this, "USB perdido: " + message);
            };

            _checkConnectiontimer.Interval = TimeSpan.FromSeconds(90);
            _checkConnectiontimer.Tick += CheckConnectiontimerOnTick;
        }

        public async Task Setup()
        {
            await ExecuteOnMainThread(() => _checkConnectiontimer.Stop());

            IsReady = false;

            await Task.Delay(2000);
            ConnectionNotify?.Invoke(this, "Tentando se Conectar");
            await Task.Delay(2000);

            if (_bluetoothSerial != null)
            {
                Arduino?.Dispose();
                Arduino = new RemoteDevice(_bluetoothSerial);
                await Task.Delay(1000);
                await ExecuteOnMainThread(() => _bluetoothSerial.begin(57600, SerialConfig.SERIAL_8N1));
            }
            else
            {
                Arduino?.Dispose();
                Arduino = new RemoteDevice(_usbSerial);
                await Task.Delay(1000);
                await ExecuteOnMainThread(() => _usbSerial.begin(57600, SerialConfig.SERIAL_8N1));
            }

            SerialOut = new SerialOutController(Arduino, 12, 8, 7);

            Arduino.DeviceConnectionLost += message =>
            {
                ConnectionNotify?.Invoke(this, "Conexão Arduino Perdida: " + message);
            };

            Arduino.DeviceConnectionFailed += async message =>
            {
                ConnectionNotify?.Invoke(this, "Conexão Arduino Falhou: " + message);
                await Task.Delay(5000);
            };

            Arduino.DeviceReady += async () => await ArduinoOnDeviceReady();

            Servos = new Dictionary<ETinBotServo, ServoController>()
            {
                [ETinBotServo.ServoHand] = new ServoController(Arduino, 09),
                [ETinBotServo.ServoHeadX] = new ServoController(Arduino, 05),
                [ETinBotServo.ServoHeadY] = new ServoController(Arduino, 06),
                [ETinBotServo.ServoLeftArm] = new ServoController(Arduino, 03, true),
                [ETinBotServo.ServoRightArm] = new ServoController(Arduino, 11),
                [ETinBotServo.ServoTorso] = new ServoController(Arduino, 10)
            };

            await ExecuteOnMainThread(() => _checkConnectiontimer.Start());
        }

        private void CheckConnectiontimerOnTick(object sender, object o)
        {
            if (Arduino == null)
                return;

            Arduino.pinMode("A0", PinMode.ANALOG);
            Arduino.pinMode("A1", PinMode.INPUT);
            Arduino.pinMode("A2", PinMode.OUTPUT);
            var a0 = Arduino.analogRead("A0");
            var a1 = Arduino.digitalRead(1);
            var a2 = Arduino.digitalRead(2);
            if ((a0 < 100 || a0 > 1000))
            {
                if (_failConnectionCount++ > 3)
                {
                    Setup();
                    _failConnectionCount = 0;
                }
            }
            else
                _failConnectionCount = 0;
        }

        private async Task ArduinoOnDeviceReady()
        {
            ConnectionNotify?.Invoke(this, "Arduino Ready");

            await SerialOut.Reset(false);
            await Task.Delay(300);
            await AttachServos();

            Arduino.SafePinMode(_phoneChargePin, PinMode.OUTPUT);

            IsReady = true;
        }

        public async Task AttachServos()
        {
            if (IsReady)
                foreach (var servo in Servos.Values.Where(x => !x.IsAttached))
                {
                    Task.Delay(100).Wait();
                    await servo.Attach();
                }
        }

        public async Task DeAttachServos()
        {
            if (IsReady)
                foreach (var servo in Servos.Values.Where(x => x.IsAttached))
                {
                    Task.Delay(50).Wait();
                    await servo.Deattach();
                }
        }

        public async Task StartPhoneCharge()
        {
            await Arduino.DigitalWriteAwaitable(_phoneChargePin, true);
        }

        public async Task StopPhoneCharge()
        {
            await Arduino.DigitalWriteAwaitable(_phoneChargePin, false);
        }
    }
}
