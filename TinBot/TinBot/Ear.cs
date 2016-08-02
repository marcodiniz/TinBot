using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Globalization;
using Windows.Media.SpeechRecognition;
using TinBot.Operations;
using TinBot.Portable;

namespace TinBot
{
    public class Ear
    {
        //public List<SpeechRecognitionListConstraint> Constraints { get; set; } = new List<SpeechRecognitionListConstraint>();

        public SpeechRecognizer Recognizer { get; set; }

        public event EventHandler<TinBotAction> ActionRequested;

        public Ear()
        {
            Recognizer = new SpeechRecognizer(new Language("pt-BR"));

            Recognizer.ContinuousRecognitionSession.ResultGenerated += ContinuousRecognitionSessionOnResultGenerated;
        }


        public void StartListen()
        {
            var l = "locker";
            try
            {
                StopListen();
                UpdateConstraints().Wait();
                Recognizer.ContinuousRecognitionSession.StartAsync().AsTask().Wait();
            }
            catch (Exception e)
            {
            }
        }


        public void StopListen()
        {
            try
            {
                if (Recognizer.State != SpeechRecognizerState.Idle)
                    Recognizer.ContinuousRecognitionSession.StopAsync().AsTask().Wait();
            }
            catch (Exception e)
            {
            }
        }

        public async Task Pause()
        {
            if (Recognizer.State != SpeechRecognizerState.Idle && Recognizer.State != SpeechRecognizerState.Paused)
                await Recognizer.ContinuousRecognitionSession.PauseAsync();
        }

        public async Task Resume()
        {
            if (Recognizer.State == SpeechRecognizerState.Idle)
                StartListen();
            if (Recognizer.State == SpeechRecognizerState.Paused)
            {
                await UpdateConstraints();
                Recognizer.ContinuousRecognitionSession.Resume();
            }
        }

        private async Task UpdateConstraints()
        {
            Recognizer.Constraints.Clear();

            foreach (var listenKey in TinBotData.ActionsLib.ListenKeys)
            {
                var phrases = listenKey.ListenFor.Split(';').SelectMany(x => new[]
                {
                    "Timbóti, " + x,
                    "Tim bóti, " + x,
                    "Tim boti, " + x,
                    "Tim bót, " + x,
                    "Tim bótí, " + x,
                    x + "Timbóti",
                    x + "Tim bóti",
                    x + "Tim boti",
                    x + "Tim bót",
                    x + "Tim bótí",
                });
                Recognizer.Constraints.Add(new SpeechRecognitionListConstraint(phrases, listenKey.Do));
            }

            await Recognizer.CompileConstraintsAsync();
        }

        private void ContinuousRecognitionSessionOnResultGenerated(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionResultGeneratedEventArgs args)
        {
            var result = args.Result;
            if (result.Status == SpeechRecognitionResultStatus.Success &&
                result.Confidence == SpeechRecognitionConfidence.High)
            {
                var tags = result.Constraint.Tag.Split(';');
                var actionName = tags[new Random().Next(tags.Length)];
                var action = TinBotData.ActionsLib[actionName];

                if (action != null)
                    ActionRequested?.Invoke(this, action);
            }
            else
            {
                ActionRequested?.Invoke(this, new SavedAction("LPulseRed"));
            }
        }
    }
}
