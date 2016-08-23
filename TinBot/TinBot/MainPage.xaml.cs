using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.SpeechRecognition;
using Windows.Media.SpeechSynthesis;
using Windows.UI;
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
using TinBot.Operations;
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
        private readonly Operations.Commands _commands;
        private readonly Body _body;
        public Ear Ear { get; set; } = new Ear();

        public MainPage()
        {
            this.InitializeComponent();
            var keepScreenOnRequest = new Windows.System.Display.DisplayRequest();
            keepScreenOnRequest.RequestActive();
            //ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
            DispatcherHelper.Dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
            DispatcherHelper.SyncContext = TaskScheduler.FromCurrentSynchronizationContext();

            _usb = new UsbSerial("VID_1A86", "PID_7523");
            _bluetooth = new BluetoothSerial("TinBot");
            _body = new Body(_usb);
            //_body = new Body(_bluetooth);
            _commands = new Operations.Commands(media, _body, Ear,
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

                new Dictionary<ETinBotToggle, int>()
                {
                    [ETinBotToggle.Green] = 1,
                    [ETinBotToggle.Red] = 2,
                    [ETinBotToggle.Blue] = 3,
                    [ETinBotToggle.Laser] = 4
                }
                );

            _body.ConnectionNotify += (sender, s) => ExecuteOnMainThread(() => Label.Text = s);
            _body.Setup();
        }


        public bool UnoReady { get; set; }



        private void Btnteste_Click(object sender, RoutedEventArgs e)
        {
            _commands.ExecuteAction(new SavedAction("introduce"));
        }

        private void Olho2OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs doubleTappedRoutedEventArgs)
        {
            _body.Setup();
        }

        private void Olho1OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs doubleTappedRoutedEventArgs)
        {
            Frame.Navigate(typeof(ActionsPage));
        }

        private void Label_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            Label.Background = new SolidColorBrush(Color.FromArgb(255,255,255,255));
        }

        private void Label_Holding(object sender, HoldingRoutedEventArgs e)
        {
            Label.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
        }
    }

}
