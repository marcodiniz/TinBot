using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maker.RemoteWiring;
using Microsoft.Maker.Serial;

namespace TinBot.Commands
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

        public SerialOutController SerialOut { get; set; }

        public RemoteDevice Arduino { get; }

        public Body(BluetoothSerial bluetoothSerial)
        {
            _bluetoothSerial = bluetoothSerial;
            Arduino = new RemoteDevice(bluetoothSerial);
            _connect = () => bluetoothSerial.begin(9600, SerialConfig.SERIAL_8N1);

            SerialOut = new SerialOutController(Arduino, 12, 8, 7);

            Setup();
        }

        public Body(UsbSerial usbSerial)
        {
            _usbSerial = usbSerial;
            Arduino = new RemoteDevice(usbSerial);
            _connect = () => _usbSerial.begin(9600, SerialConfig.SERIAL_8N1);

            SerialOut = new SerialOutController(Arduino, 12, 8, 7);

            Setup();
        }

        private void Setup()
        {
            Arduino.DeviceConnectionLost += message =>
            {
                Task.Delay(5000);
                Connect();
            };

            Arduino.DeviceReady += async () => await ArduinoOnDeviceReady();

            ServoHand = new ServoController(Arduino, 09);
            ServoRightArm = new ServoController(Arduino, 05);
            ServoLeftArm = new ServoController(Arduino, 06, true);
            ServoTorso = new ServoController(Arduino, 03);
            ServoHeadY = new ServoController(Arduino, 11);
            ServoHeadX = new ServoController(Arduino, 10);

        }

        private async Task ArduinoOnDeviceReady()
        {
            ServoHand.Attach();
            ServoRightArm.Attach();
            ServoLeftArm.Attach();
            ServoTorso.Attach();
            ServoHeadY.Attach();
            ServoHeadX.Attach();

            await Task.Delay(1000);
            await SerialOut.Reset(false);
        }

        public void Connect()
        {
            _connect();
        }

    }
}
