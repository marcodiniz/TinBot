using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.SpeechSynthesis;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Microsoft.Maker.RemoteWiring;
using Microsoft.Maker.Serial;
using TinBot.Commands;
using TinBot.Helpers;
using static TinBot.Helpers.DispatcherHelper;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TinBot
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private UsbSerial _usb;
        private BluetoothSerial _bluetooth;
        private RemoteDevice _uno;
        private Dictionary<Storyboard, int> _animationPauseTime;
        private VoiceInformation _voice;
        private Commands.Commands _commands;

        private ServoController _servoMao;
        private ServoController _servoRightArm;
        private ServoController _servoLeftArm;
        private ServoController _servoTorso;
        private ServoController _servoHeadY;

        public MainPage()
        {
            this.InitializeComponent();

            DispatcherHelper.Dispatcher = Windows.UI.Core.CoreWindow.GetForCurrentThread().Dispatcher;

            _commands = new Commands.Commands(media,
                new Dictionary<Storyboard, int>()
                {
                    [bravo] = 200,
                    [feliz] = 100,
                    [feliz_verde] = 100,
                    [normal] = 0,
                    [piscadela] = 0,
                    [piscando] = 0,
                    [piscando_duplo] = 0,
                    [triste] = 200
                },
                new Dictionary<string, Storyboard>()
                {
                    ["bravo"] = bravo,
                    ["feliz"] = feliz,
                    ["feliz_verde"] = feliz_verde,
                    ["normal"] = normal,
                    ["piscadela"] = piscadela,
                    ["piscando"] = piscando,
                    ["piscando_duplo"] = piscando_duplo,
                    ["triste"] = triste
                });


            var keepScreenOnRequest = new Windows.System.Display.DisplayRequest();
            keepScreenOnRequest.RequestActive();


            _commands.ExecuteAction(TinBotAction.MakeSpeakAction("Apresente-se", "marcaoneia"));

            var view = ApplicationView.GetForCurrentView();
            //view.TryEnterFullScreenMode();

            var voices = SpeechSynthesizer.AllVoices;
            _voice = voices.FirstOrDefault(v => v.DisplayName.Contains("Daniel"));

            _usb = new Microsoft.Maker.Serial.UsbSerial("VID_1A86", "PID_7523");
            //_bluetooth = new BluetoothSerial("TinBot");

            //_bluetooth.ConnectionEstablished += () => Label.Text = "Conectado! - ";
            //_bluetooth.ConnectionFailed += message => Label.Text = "Faiou: " + message;
            _usb.ConnectionEstablished += () => Label.Text = "Conectado! - ";
            _usb.ConnectionFailed += message => Label.Text = "Faiou: " + message;
            //_bluetooth.ConnectionLost += message => Label.Text = "Lost: " + message;
            //_bluetooth.ConnectionLost += message => _bluetooth.begin(57600, SerialConfig.SERIAL_8N1);

            _uno = new RemoteDevice(_usb);
            
            _uno.DeviceConnectionLost += message => ExecuteOnMainThread(() => Label.Text = "Uno Lost " + message);
            _uno.DeviceConnectionFailed += message => ExecuteOnMainThread(() => Label.Text = "Uno Failed " + message);
            _uno.DeviceReady += () => ExecuteOnMainThread(() =>
            {
                UnoReady = true;
                Label.Text = "Uno Ready ";
            
            });

            _uno.StringMessageReceived += message => ExecuteOnMainThread(() => txtString.Text = message);
            _uno.SysexMessageReceived += (command, message) =>
            {
                var str = message.ReadString(255);
                this.txtSys.Text = str;
            };

            _usb.begin(9600, SerialConfig.SERIAL_8N1);

            this.Unloaded += (sender, args) => _bluetooth.Dispose();
        }

        public bool UnoReady { get; set; }

        private void BtnAcende_Click(object sender, RoutedEventArgs e)
        {
            //_uno.analogWrite(13, 500);
            _uno.digitalWrite(13, PinState.HIGH);
        }

        private void Btnapaga_Click(object sender, RoutedEventArgs e)
        {
            _uno.digitalWrite(13, PinState.LOW);
        }

        private void Btnteste_Click(object sender, RoutedEventArgs e)
        {
            _commands.ExecuteAction(TinBotAction.MakeSpeakAction("Apresente-se", "marcaoneia"));
        }

        private void Btnteste_Click2(object sender, RoutedEventArgs e)
        {
            Label.Text = "...";
            ;           // _bluetooth.begin(9600, SerialConfig.SERIAL_8N1);
            //  normal.Completed += (o, o1) => bravo.Begin();
            //PlayAndPause(bravo);
        }



        private async void btnVoz1_Click(object sender, RoutedEventArgs e)
        {
            _commands.ExecuteAction(TinBotAction.MakeSpeakAction("Apresente-se", "introduce"));
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _bluetooth.Dispose();
        }

        private void sldHand_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (UnoReady)
            {
                ushort speed =10;
                ushort acc = 1000;
                _servoMao.Move((ushort)sldHand.Value, speed, acc);
            }
        }

        private void sldArm_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (UnoReady)
            {
                ushort speed =20;
                ushort acc = 15;
                _servoLeftArm.Move((ushort)sldArm.Value, speed, acc);
            }
        }

        private void sldRightArnm_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (UnoReady)
            {
                ushort speed = 20;
                ushort acc = 15;
                _servoRightArm.Move((ushort)sldRightArnm.Value, speed, acc);
            }
        }

        private void sldTorso_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (UnoReady)
            {
                ushort speed = 10;
                ushort acc = 7;
                _servoTorso.Move((ushort)sldTorso.Value, speed, acc);
            }
        }

        private void sldHeadY_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (UnoReady)
            {
                ushort speed = 10;
                ushort acc = 7;
                _servoHeadY.Move((ushort)sldHeadY.Value, speed, acc);
            }
        }
    }

}
