using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Microsoft.Maker.RemoteWiring;
using Microsoft.Maker.Serial;
using TinBot.Portable;
using static TinBot.Helpers.DispatcherHelper;

namespace TinBot.Operations
{
    public class Body
    {
        private readonly UsbSerial _usbSerial;
        private readonly BluetoothSerial _bluetoothSerial;
        private Action _connect;
        public ServoController ServoHand { get; private set; }
        public ServoController ServoRightArm { get; private set; }
        public ServoController ServoLeftArm { get; private set; }
        public ServoController ServoTorso { get; private set; }
        public ServoController ServoHeadY { get; private set; }
        public ServoController ServoHeadX { get; private set; }
        public Dictionary<ETinBotServo, ServoController> Servos { get; set; }

        public SerialOutController SerialOut { get; set; }

        public RemoteDevice Arduino { get; private set; }
        public bool IsReady { get; private set; }

        public event EventHandler<string> ConnectionNotify;

        private DispatcherTimer _checkConnectiontimer = new DispatcherTimer();
        private int failConnectionCount = 0;


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

            _checkConnectiontimer.Interval = TimeSpan.FromSeconds(30);
            _checkConnectiontimer.Tick += CheckConnectiontimerOnTick;
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
                if (failConnectionCount++ > 3)
                {
                    Setup();
                    failConnectionCount = 0;
                }
            }
            else
                failConnectionCount = 0;
        }


        public async void Setup()
        {
            await ExecuteOnMainThread(() => _checkConnectiontimer.Stop());

            IsReady = false;

            await Task.Delay(2000);
            ConnectionNotify?.Invoke(this, "Tentando se Conectar");
            await Task.Delay(2000);

            if (_bluetoothSerial != null)
            {
                Arduino = new RemoteDevice(_bluetoothSerial);
                await Task.Delay(1000);
                _bluetoothSerial.begin(57600, SerialConfig.SERIAL_8N1);
            }
            else
            {
                Arduino = new RemoteDevice(_usbSerial);
                await Task.Delay(1000);
                _usbSerial.begin(57600, SerialConfig.SERIAL_8N1);
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

            ServoHand = new ServoController(Arduino, 09);
            ServoRightArm = new ServoController(Arduino, 05);
            ServoLeftArm = new ServoController(Arduino, 06, true);
            ServoTorso = new ServoController(Arduino, 03, delay: 10);
            ServoHeadY = new ServoController(Arduino, 11);
            ServoHeadX = new ServoController(Arduino, 10);

            Servos = new Dictionary<ETinBotServo, ServoController>()
            {
                [ETinBotServo.ServoHand] = ServoHand,
                [ETinBotServo.ServoHeadX] = ServoHeadX,
                [ETinBotServo.ServoHeadY] = ServoHeadY,
                [ETinBotServo.ServoLeftArm] = ServoLeftArm,
                [ETinBotServo.ServoRightArm] = ServoRightArm,
                [ETinBotServo.ServoTorso] = ServoTorso,
            };

            await ExecuteOnMainThread(() => _checkConnectiontimer.Start());
        }

        private async Task ArduinoOnDeviceReady()
        {
            ConnectionNotify?.Invoke(this, "Arduino Ready");

            await Task.Delay(700);
            await ServoHeadY.Attach();
            await Task.Delay(700);
            await ServoHeadX.Attach();
            await Task.Delay(700);
            await ServoHand.Attach();
            await Task.Delay(700);
            await ServoRightArm.Attach();
            await Task.Delay(700);
            await ServoLeftArm.Attach();
            await Task.Delay(700);
            await ServoTorso.Attach();
            await Task.Delay(300);
            await SerialOut.Reset(false);

            IsReady = true;
        }
    }
}
