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
using TinBot.Portable;
using Buffer = Windows.Storage.Streams.Buffer;

namespace TinBot.Commands
{
    public class Commands
    {
        public MediaElement MediaElement { get; }
        public SpeechSynthesizer Synth { get; } = new SpeechSynthesizer();
        public Dictionary<ETinBotFaces, Storyboard> Faces { get; }
        public Dictionary<ETinBotServo, ServoController> Servos { get; }
        public Dictionary<Storyboard, int> FacesPauseTime { get; }

        private readonly List<Storyboard> _animationQeue = new List<Storyboard>();

        public Commands(MediaElement mediaElement, Dictionary<Storyboard, int> facesPauseTime,
            Dictionary<ETinBotFaces, Storyboard> faces, Dictionary<ETinBotServo, ServoController> servos)
        {
            MediaElement = mediaElement;
            FacesPauseTime = facesPauseTime;
            Faces = faces;
            Servos = servos;

            var voice = SpeechSynthesizer.AllVoices.FirstOrDefault(v => v.DisplayName.Contains("Daniel"));
            Synth.Voice = voice;
            mediaElement.MarkerReached += MediaElementOnMarkerReached;
        }

        public async Task ExecuteAction(TinBotAction action)
        {
            switch (action.Type)
            {
                case EActionType.Speak:
                    await Speak(((SpeakAction)action).Text);
                    break;
                case EActionType.Face:
                    var sb = Faces[((FaceAction)action).TinBotFaces];
                    PlayAndPause(sb);
                    break;
                case EActionType.Move:
                    var moveAction = (MovementAcion)action;
                    await
                        Servos[moveAction.Servo].Move(moveAction.TargetPosition, moveAction.Speed,
                            moveAction.Acceleratton);
                    break;
                case EActionType.Saved:
                    var foundAction =
                        TinBotHelpers.SavedActions.FirstOrDefault(
                            x => ((SavedAction)action).ActionName.Equals(x.Name, StringComparison.OrdinalIgnoreCase));
                    if (foundAction != null)
                        await ExecuteAction(foundAction);
                    break;
                case EActionType.Sequence:
                    var sequenceAction = (SequenceAction)action;
                    foreach (var paralellAction in sequenceAction.Sequence)
                    {
                        var awaiters = paralellAction.Select(ExecuteAction).ToArray();
                        Task.WaitAll(awaiters);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public async Task Speak(string text)
        {
            var tts = TinBotHelpers.WrapWithSSML(text);
            var ssStream = await Synth.SynthesizeSsmlToStreamAsync(tts);
            MediaElement.Markers.Clear();
            ssStream.Markers.ToList().ForEach(x => MediaElement.Markers.Add(new TimelineMarker { Text = x.Text, Time = x.Time }));

            MediaElement.SetSource(ssStream, ssStream.ContentType);
            MediaElement.Play();
        }

        private void MediaElementOnMarkerReached(object sender, TimelineMarkerRoutedEventArgs e)
        {
                ExecuteAction(new SavedAction(e.Marker.Text));
        }

        private void PlayAndPause(Storyboard sb)
        {
            sb.Completed -= PlayNext;
            sb.Completed += PlayNext;

            if (!_animationQeue.Any())
            {
                _animationQeue.Add(sb);
                PlayNext();
            }
            else
            {
                _animationQeue.RemoveRange(1, _animationQeue.Count - 1);
                _animationQeue.Add(sb);
                if (_animationQeue[0].GetCurrentTime().TotalMilliseconds - FacesPauseTime[_animationQeue[0]] < 1)
                    _animationQeue[0].Resume();
            }
        }

        private void PlayNext(object sender = null, object o = null)
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
                                DispatcherHelper.ExecuteOnMainThread(() => storyBoard.Pause()).Wait();
                        }, null, pause, Timeout.Infinite);
                    }
                    storyBoard.Begin();
                }
            }
        }
    }
}
