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
using Microsoft.Maker.Serial;
using Microsoft.Maker.RemoteWiring;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TinBot
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private UsbSerial _usb;
        private RemoteDevice _uno;
        private List<Storyboard> _animationQeue = new List<Storyboard>();
        private Dictionary<Storyboard, int> _animationPauseTime;
        private VoiceInformation _voice;
        private Commands.Commands _commands;

        public MainPage()
        {
            this.InitializeComponent();

            _commands = new Commands.Commands(media,
                new Dictionary<string, int>()
                {
                    ["bravo"] = 200,
                    ["feliz"] = 100,
                    ["feliz_verde"] = 100,
                    ["normal"] = 0,
                    ["piscadela"] = 0,
                    ["piscando"] = 0,
                    ["piscando_duplo"] = 0,
                    ["triste"] = 200
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
            var view = ApplicationView.GetForCurrentView();
            //view.TryEnterFullScreenMode();

            var voices = SpeechSynthesizer.AllVoices;
            _voice = voices.FirstOrDefault(v => v.DisplayName.Contains("Daniel"));

            _usb = new Microsoft.Maker.Serial.UsbSerial("VID_2341", "PID_0043");
            _uno = new RemoteDevice(_usb);

            _usb.ConnectionEstablished += () => Label.Text = "Conectado! - ";
            _usb.ConnectionFailed += message => Label.Text = "Faiou: " + message;
            _usb.ConnectionLost += message => Label.Text = "Lost: " + message;

            //_uno.DeviceReady += () => Label.Text = "Uno Ready!";
            _uno.DeviceConnectionLost += message => Label.Text = "Uno Lost " + message;
            //_uno.DeviceConnectionFailed += message => Label.Text = "uno failed " + message;

            _usb.begin(9600, SerialConfig.SERIAL_8N1);

        }

        private void BtnAcende_Click(object sender, RoutedEventArgs e)
        {
            _uno.analogWrite(11, 500);
            //_uno.digitalWrite(13,PinState.HIGH);
        }

        private void Btnapaga_Click(object sender, RoutedEventArgs e)
        {
            _uno.digitalWrite(11, PinState.LOW);
        }

        private void Btnteste_Click(object sender, RoutedEventArgs e)
        {
            PlayAndPause(feliz);
            //PlayAndPause(piscadela);
        }

        private void Btnteste_Click2(object sender, RoutedEventArgs e)
        {
            //  normal.Completed += (o, o1) => bravo.Begin();
            PlayAndPause(bravo);
        }

        private void PlayAndPause(Storyboard sb)
        {
            sb.Completed -= PlayNext;
            sb.Completed += PlayNext;

            if (_animationQeue.Count == 0)
            {
                _animationQeue.Add(sb);
                PlayNext(null, null);
            }
            else
            {
                //if (sb != _animationQeue[0])
                {
                    _animationQeue.Add(sb);
                    if (_animationQeue[0].GetCurrentTime().TotalMilliseconds - _animationPauseTime[_animationQeue[0]] < 1)
                        _animationQeue[0].Resume();
                }
            }
        }

        private void PlayNext(object sender, object o)
        {
            if (sender != null)
                _animationQeue.Remove(sender as Storyboard);

            {
                if (_animationQeue.Count > 0)
                {
                    var storyBoard = _animationQeue[0];
                    var dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
                    var pause = _animationPauseTime[storyBoard];
                    if (pause > 0)
                    {
                        new Timer(state =>
                        {
                            if (_animationQeue.Count < 2)
                                dispatcher.RunAsync(CoreDispatcherPriority.High, () => storyBoard.Pause());
                        }, null, pause, Timeout.Infinite);
                    }
                    storyBoard.Begin();
                }
            }

        }

        private async void btnVoz1_Click(object sender, RoutedEventArgs e)
        {
            MediaElement mediaElement = this.media;
            mediaElement.Volume = mediaElement.Volume * 2;
            // The object for controlling the speech synthesis engine (voice).
            var synth = new Windows.Media.SpeechSynthesis.SpeechSynthesizer();
            synth.Voice = _voice;
            // Generate the audio stream from plain text.

        }
    }

}
