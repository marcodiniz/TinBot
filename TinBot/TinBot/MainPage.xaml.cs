using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
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
using TinBot.Portable;
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
        private readonly Commands.Commands _commands;
        private readonly Body _body;

        public MainPage()
        {
            this.InitializeComponent();
            DispatcherHelper.Dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;

            _usb = new UsbSerial("VID_1A86", "PID_7523");
            _body = new Body(_usb);
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
                new Dictionary<ETinBotFaces, Storyboard>()
                {
                    [ETinBotFaces.Angry] = bravo,
                    [ETinBotFaces.Happy] = feliz,
                    [ETinBotFaces.HappyGreen] = feliz_verde,
                    [ETinBotFaces.Normal] = normal,
                    [ETinBotFaces.UniBlink] = piscadela,
                    [ETinBotFaces.Blink] = piscando,
                    [ETinBotFaces.BlinkDouble] = piscando_duplo,
                    [ETinBotFaces.Sad] = triste
                },
                new Dictionary<ETinBotServo, ServoController>()
                {
                    [ETinBotServo.ServoHand] = _body.ServoHand,  
                    [ETinBotServo.ServoHeadX] = _body.ServoHeadX,  
                    [ETinBotServo.ServoHeadY] = _body.ServoHeadY,  
                    [ETinBotServo.ServoLeftArm] = _body.ServoLeftArm,  
                    [ETinBotServo.ServoRightArm] = _body.ServoRightArm,  
                    [ETinBotServo.ServoTorso] = _body.ServoTorso,  
                }
                );
            TinBotHelpers.Commands = _commands;

             
            var keepScreenOnRequest = new Windows.System.Display.DisplayRequest();
            keepScreenOnRequest.RequestActive();

            //_commands.ExecuteAction(TinBotAction.MakeSpeakAction("Apresente-se", "marcaoneia"));

            var view = ApplicationView.GetForCurrentView();
            //view.TryEnterFullScreenMode();

            var voices = SpeechSynthesizer.AllVoices;
            _voice = voices.FirstOrDefault(v => v.DisplayName.Contains("Daniel"));

            //_bluetooth = new BluetoothSerial("TinBot");

            //_bluetooth.ConnectionEstablished += () => Label.Text = "Conectado! - ";
            //_bluetooth.ConnectionFailed += message => Label.Text = "Faiou: " + message;
            _usb.ConnectionEstablished += () => Label.Text = "Conectado! - ";
            _usb.ConnectionFailed += message => Label.Text = "Faiou: " + message;
            //_bluetooth.ConnectionLost += message => Label.Text = "Lost: " + message;
            //_bluetooth.ConnectionLost += message => _bluetooth.begin(57600, SerialConfig.SERIAL_8N1);

            //_uno = new RemoteDevice(_usb);

            //_uno.DeviceConnectionLost += message => ExecuteOnMainThread(() => Label.Text = "Uno Lost " + message);
            //_uno.DeviceConnectionFailed += message => ExecuteOnMainThread(() => Label.Text = "Uno Failed " + message);
            _body.Arduino.DeviceReady += () => ExecuteOnMainThread(() =>
            {
                UnoReady = true;
                Label.Text = "Uno Ready ";
            });

            //_uno.StringMessageReceived += message => ExecuteOnMainThread(() => txtString.Text = message);
            //_uno.SysexMessageReceived += (command, message) =>
            //{
            //    var str = message.ReadString(255);
            //    this.txtSys.Text = str;
            //};

            _body.Connect();

            }

        public bool UnoReady { get; set; }

        private void BtnAcende_Click(object sender, RoutedEventArgs e)
        {
            _body.Arduino.digitalWrite(13, PinState.HIGH);
            //_body.Arduino.pinMode("A1", PinMode.OUTPUT);
            //_body.Arduino.digitalWrite(1, PinState.HIGH);
        }

        private async void Btnapaga_Click(object sender, RoutedEventArgs e)
        {
            _body.Arduino.digitalWrite(13, PinState.LOW);
 
            await _body.SerialOut.SetValue(0, true,false);
            await _body.SerialOut.SetValue(1, true,false);
            await _body.SerialOut.SetValue(2, true,false);
            await _body.SerialOut.SetValue(3, true);
        }

        private void Btnteste_Click(object sender, RoutedEventArgs e)
        {
            _commands.ExecuteAction(new SavedAction("introduce"));
        }

        private void Btnteste_Click2(object sender, RoutedEventArgs e)
        {
            _commands.ExecuteAction(new SavedAction("goodmorning1"));
        }


        private void btnVoz1_Click(object sender, RoutedEventArgs e)
        {
            _commands.ExecuteAction(new SavedAction("goodmorning2"));
        }

        private void sldHand_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (UnoReady)
            {
                ushort speed = 5;
                ushort acc = 2;
                _body.ServoHand.Move((ushort)sldHand.Value, speed, acc);
            }
        }

        private void sldArm_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (UnoReady)
            {
                _commands.ExecuteAction(new MovementAcion(ETinBotServo.ServoLeftArm, (int) sldArm.Value, 20, 3));
            }
        }

        private void sldRightArnm_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (UnoReady)
            {
                ushort speed = 20;
                ushort acc = 2;
                _body.ServoRightArm.Move((ushort)sldRightArnm.Value, speed, acc);
            }
        }

        private void sldTorso_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (UnoReady)
            {
                ushort speed = 10;
                ushort acc = 7;
                _body.ServoTorso.Move((ushort)sldTorso.Value, speed, acc);
            }
        }

        private void sldHeadY_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (UnoReady)
            {
                ushort speed = 50;
                ushort acc = 7;
                _body.ServoHeadY.Move((ushort)sldHeadY.Value, speed, acc);
            }
        }

        private void sldHeadX_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (UnoReady)
            {
                ushort speed = 50;
                ushort acc = 7;
                _body.ServoHeadX.Move((ushort)sldHeadX.Value, speed, acc);
            }
        }

        private void BtnGoTOActions_OnClick(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof (ActionsPage));
        }
    }

}
