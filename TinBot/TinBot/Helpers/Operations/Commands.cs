using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
using static TinBot.Helpers.DispatcherHelper;
using static TinBot.TinBotData;

namespace TinBot.Operations
{
    public class Commands
    {
        public MediaElement MediaElement { get; }
        public SpeechSynthesizer Synth { get; } = new SpeechSynthesizer();
        public Ear Ear { get; set; }

        public Dictionary<ETinBotFaces, Storyboard> Faces { get; }
        public Dictionary<ETinBotToggle, int> Toggles { get; }
        public Dictionary<Storyboard, int> FacesPauseTime { get; }

        private readonly List<Storyboard> _animationQueue = new List<Storyboard>();
        private int _actionsStackCount = 0;
        private readonly Body _body;

        public Commands(MediaElement mediaElement, Body body, Ear ear,
            Dictionary<Storyboard, int> facesPauseTime, Dictionary<ETinBotFaces, Storyboard> faces,
            Dictionary<ETinBotToggle, int> toggles)
        {
            MediaElement = mediaElement;
            FacesPauseTime = facesPauseTime;
            Faces = faces;
            _body = body;
            Ear = ear;
            Toggles = toggles;

            var voice = SpeechSynthesizer.AllVoices.FirstOrDefault(v => v.DisplayName.Contains("Daniel"));
            Synth.Voice = voice;

            SetupStandByBehaviors();

            ActionRequestArrived += (s, a) => ProcessActionsQueue();

            Ear.ActionRequested += (sender, action) =>
            {
                ExecuteOnMainThread(() => ActionsQueue.Add(action)).Wait();
                ProcessActionsQueue();
            };
        }

        private void SetupStandByBehaviors()
        {
            PhoneState.OnStandByOn += async (sender, args) =>
            {
                Ear.StartListen();
                await _body.DeAttachServos();
            };

            PhoneState.OnStandByOff += async (sender, args) =>
            {
                Ear.StopListen();
                await _body.AttachServos();
            };
        }

        private void ProcessActionsQueue()
        {
            //if (!_body.IsReady) return;

            var _lock = "lock";
            lock (_lock)
            {
                if (_actionsStackCount >= 1) return;

                if (!ActionsQueue.Any())
                {
                    PhoneState.SetStandByOn();
                    return;
                }

                if (PhoneState.IsStandByOn)
                {
                    PhoneState.SetStandByOff();
                    Task.Delay(1000).Wait();
                }

                var nextAction = ActionsQueue[0];
                ExecuteOnMainThread(() => ActionsQueue.RemoveAt(0)).Wait();

                if (nextAction != null)
                {
                    _actionsStackCount++;

                    ExecuteAction(nextAction).Wait();
                    ExecuteAction(new SavedAction("rest")).Wait();

                    _actionsStackCount--;
                }

                ProcessActionsQueue();
            }
        }

        public async Task ExecuteAction(TinBotAction action)
        {
            if (action == null || !_body.IsReady)
                return;

            try
            {
                for (int i = 0; i < Math.Max(1, action.Repeat); i++)
                {
                    switch (action.Type)
                    {
                        case EActionType.Speak:
                            await Speak(((SpeakAction)action).Text);
                            break;
                        case EActionType.Face:
                            var sb = Faces[((FaceAction)action).TinBotFaces];
                            await ExecuteOnMainThread(() => PlayAndPause(sb));
                            break;
                        case EActionType.Toggle:
                            if (_body.IsReady)
                            {
                                await ExecuteToggle((ToggleAction)action);
                            }
                            break;
                        case EActionType.Move:
                            if (_body.IsReady)
                            {
                                var moveAction = (MovementAcion)action;
                                if (_body.Servos.ContainsKey(moveAction.Servo))
                                    await Move(moveAction);
                            }
                            break;
                        case EActionType.Saved:
                            var found = ActionsLib.AllActions().FirstOrDefault(
                                x => x.Name.Equals(((SavedAction) action).ActionName, StringComparison.OrdinalIgnoreCase));
                            if (found != null)
                                await ExecuteAction(found);
                            break;
                        case EActionType.Sequence:
                            var sequenceAction = (SequenceAction)action;
                            foreach (var paralellAction in sequenceAction.Sequence)
                            {
                                List<Task> waiters = new List<Task>();
                                foreach (var actionContainer in paralellAction)
                                {
                                    var a = actionContainer.GetAction();
                                    var t = ExecuteAction(a);
                                    if (!a.IgnoreBlockingInSeries)
                                        waiters.Add(t);
                                }
                                await Task.WhenAll(waiters);
                            }
                            break;
                    }
                    await Task.Delay(action.ExtraWaitTime);
                }
            }
            catch (Exception ex)
            {
                return;
            }
        }

        private async Task Move(MovementAcion moveAction)
        {
            var servo = _body.Servos[moveAction.Servo];
            await servo.Move(moveAction.TargetPosition, moveAction.Speed,
                moveAction.Acceleratton);
        }

        private async Task ExecuteToggle(ToggleAction action)
        {
            if (!Toggles.ContainsKey(action.TinBotToggle))
                return;

            for (int i = 0; i < action.Repeat; i++)
            {
                if (action.TimeOn > 0)
                {
                    var t = _body.SerialOut.SetValue(Toggles[action.TinBotToggle], true);
                    await Task.WhenAll(t, Task.Delay(action.TimeOn));
                }
                if (action.TimeOff > 0)
                {
                    var t = _body.SerialOut.SetValue(Toggles[action.TinBotToggle], false);
                    await Task.WhenAll(t, Task.Delay(action.TimeOff));
                }
            }

        }

        public async Task Speak(string text)
        {
            var tts = TinBotHelpers.WrapWithSSML(text);
            var ssStream = await Synth.SynthesizeSsmlToStreamAsync(tts);

            await ExecuteOnMainThread(() =>
            {
                MediaElement.MarkerReached -= MediaElementOnMarkerReached;
                MediaElement.MarkerReached += MediaElementOnMarkerReached;

                MediaElement.Markers.Clear();
                ssStream.Markers.ToList()
                    .ForEach(x => MediaElement.Markers.Add(new TimelineMarker { Text = x.Text, Time = x.Time }));

                MediaElement.SetSource(ssStream, ssStream.ContentType);
                MediaElement.Play();
            });

            await Task.Delay(500);

            var playing = true;
            while (playing)
            {
                await ExecuteOnMainThread(() =>
                {
                    playing = MediaElement.CurrentState == MediaElementState.Playing ||
                              MediaElement.CurrentState == MediaElementState.Buffering ||
                              MediaElement.CurrentState == MediaElementState.Opening;
                });

                await Task.Delay(500);
            }
        }

        private void MediaElementOnMarkerReached(object sender, TimelineMarkerRoutedEventArgs e)
        {
            foreach (var t in e.Marker.Text.Split(','))
            {
                ExecuteAction(new SavedAction(t));
            }
        }

        private void PlayAndPause(Storyboard sb)
        {
            sb.Completed -= PlayNext;
            sb.Completed += PlayNext;

            if (!_animationQueue.Any())
            {
                _animationQueue.Add(sb);
                PlayNext();
            }
            else
            {
                _animationQueue.RemoveRange(1, _animationQueue.Count - 1);
                _animationQueue.Add(sb);
                if (_animationQueue[0].GetCurrentTime().TotalMilliseconds - FacesPauseTime[_animationQueue[0]] < 1)
                    _animationQueue[0].Resume();
            }
        }

        private void PlayNext(object sender = null, object o = null)
        {
            if (sender != null)
                _animationQueue.Remove(sender as Storyboard);

            {
                if (_animationQueue.Count > 0)
                {
                    var storyBoard = _animationQueue[0];
                    var dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
                    var pause = FacesPauseTime[storyBoard];
                    if (pause > 0)
                    {
                        new Timer(state =>
                        {
                            if (_animationQueue.Count < 2)
                                DispatcherHelper.ExecuteOnMainThread(() => storyBoard.Pause()).Wait();
                        }, null, pause, Timeout.Infinite);
                    }
                    storyBoard.Begin();
                }
            }
        }
    }
}
