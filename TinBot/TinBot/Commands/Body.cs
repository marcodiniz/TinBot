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
        private ServoController _servoHand;
        private ServoController _servoRightArm;
        private ServoController _servoLeftArm;
        private ServoController _servoTorso;
        private ServoController _servoHeadY;
        private ServoController _servoHeadX;

        public RemoteDevice Arduino { get; }

        public Body(BluetoothSerial bluetoothSerial)
        {
            _bluetoothSerial = bluetoothSerial;
            Arduino = new RemoteDevice(bluetoothSerial);
            _connect = () => bluetoothSerial.begin(9600, SerialConfig.SERIAL_8N1);

            Setup();
        }

        public Body(UsbSerial usbSerial)
        {
            _usbSerial = usbSerial;
            Arduino = new RemoteDevice(usbSerial);
            _connect = () => _usbSerial.begin(9600, SerialConfig.SERIAL_8N1);

            Setup();
        }

        private void Setup()
        {
            Arduino.DeviceConnectionLost += message =>
            {
                Task.Delay(5000);
                Connect();
            };

            Arduino.DeviceReady += ArduinoOnDeviceReady;

            _servoHand = new ServoController(Arduino, 05);
            _servoRightArm = new ServoController(Arduino, 06);
            _servoLeftArm = new ServoController(Arduino, 09, true);
            _servoTorso = new ServoController(Arduino, 03);
            _servoHeadY = new ServoController(Arduino, 10);
            _servoHeadX = new ServoController(Arduino, 11);

        }

        private void ArduinoOnDeviceReady()
        {
            _servoHand.Attach();
            _servoRightArm.Attach();
            _servoLeftArm.Attach();
            _servoTorso.Attach();
            _servoHeadY.Attach();
            _servoHeadX.Attach();
        }

        public void Connect()
        {
            _connect();
        }

    }
}
