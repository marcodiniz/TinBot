using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media.Audio;
using Windows.Media.SpeechSynthesis;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using TinBot.Helpers;
using Buffer = Windows.Storage.Streams.Buffer;

namespace TinBot.Commands
{
    public class Commands
    {
        public MediaElement MediaElement { get; }
        public SpeechSynthesizer Synth { get; } = new SpeechSynthesizer();
        public Dictionary<string, Storyboard> Faces { get; }
        public Dictionary<Storyboard, int> FacesPauseTime { get; }

        private readonly List<Storyboard> _animationQeue = new List<Storyboard>();

        public Commands(MediaElement mediaElement, Dictionary<Storyboard, int> facesPauseTime, Dictionary<string, Storyboard> faces)
        {
            MediaElement = mediaElement;
            FacesPauseTime = facesPauseTime;
            Faces = faces;

            var voice = SpeechSynthesizer.AllVoices.FirstOrDefault(v => v.DisplayName.Contains("Daniel"));
            Synth.Voice = voice;
            mediaElement.MarkerReached += MediaElementOnMarkerReached;
        }

        public void ExecuteAction(TinBotAction action)
        {
            if (action.Type == EActionType.Speak)
            {
                var str = action.SpeakText ?? VoiceHelpers.Phrases[action.SpeakKey];
                Speak(str);
            }
        }

        public async Task Speak(string text)
        {
            var tts = VoiceHelpers.WrapWithSSML(text);
            var ssStream = await Synth.SynthesizeSsmlToStreamAsync(tts);
            MediaElement.Markers.Clear();
            ssStream.Markers.ToList().ForEach(x=>MediaElement.Markers.Add(new TimelineMarker() {Text = x.Text,Time = x.Time}));


            MediaElement.SetSource(ssStream, ssStream.ContentType);
            MediaElement.Play();
        }

        private void MediaElementOnMarkerReached(object sender, TimelineMarkerRoutedEventArgs e)
        {
            //check if the requested action is a Face Animaton
            if (Faces.ContainsKey(e.Marker.Text))
            {
                var sb = Faces[e.Marker.Text];
                PlayAndPause(sb);
            }
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
                _animationQeue.Skip(1).ToList().ForEach(x => _animationQeue.Remove(x));
                _animationQeue.Add(sb);
                if (_animationQeue[0].GetCurrentTime().TotalMilliseconds - FacesPauseTime[_animationQeue[0]] < 1)
                    _animationQeue[0].Resume();
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
                    var pause = FacesPauseTime[storyBoard];
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
    }
}
