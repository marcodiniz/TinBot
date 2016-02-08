using System.Collections.Generic;
using Microsoft.AspNet.Mvc;
using Newtonsoft.Json;
using TinBot.Portable;
using TinBot.Web.ViewModels;

namespace TinBot.Web
{
    public static class Exts
    {

        public static List<MovementAcion> MovementAcions { get; set; } = new List<MovementAcion>();
        public static List<FaceAction> FaceActions { get; set; } =new List<FaceAction>();
        public static List<SpeakAction> SpeakActions{ get; set; } = new List<SpeakAction>();
        public static List<SavedAction> SavedActions { get; set; } = new List<SavedAction>();
        public static List<SequenceAction> SequenceActions { get; set; } = new List<SequenceAction>();

        static Exts()
        {
            MovementAcions.Add(new MovementAcion(ETinBotServo.ServoHeadY, 180, 100, 10,"HeadDown"));
            MovementAcions.Add(new MovementAcion(ETinBotServo.ServoHeadY, 0, 100, 10,"HeadUp"));

            FaceActions.Add(new FaceAction(ETinBotFaces.Normal, "Normal"));
            FaceActions.Add(new FaceAction(ETinBotFaces.Angry, "Angry"));
            FaceActions.Add(new FaceAction(ETinBotFaces.Blink, "Blink"));
            FaceActions.Add(new FaceAction(ETinBotFaces.BlinkDouble, "BlinkDouble"));
            FaceActions.Add(new FaceAction(ETinBotFaces.UniBlink, "UniBlink"));
            FaceActions.Add(new FaceAction(ETinBotFaces.Happy, "Happy"));
            FaceActions.Add(new FaceAction(ETinBotFaces.HappyGreen, "HappyGreen"));
            FaceActions.Add(new FaceAction(ETinBotFaces.Sad, "Sad"));

            SpeakActions.Add(new SpeakAction(
                "<mark name='Happy'/> Olá! Meu nome é timboóti, Minha função é ajudar o time. <mark name='Blink'/> " +
                "Eu estou aqui para dar recados, monitorar os indicadores, <mark name='BlinkDouble'/> alertar o time quando for preciso. " +
                "<mark name='Blink'/> Há, eu também monitoro bíldis, " +
                "e se alguém fizer besteira eu aponto o dedo <emphasis> na cara <mark name='UniBlink'/> </emphasis>, rárrarrárrárra",
                "introduce"));
            SpeakActions.Add(new SpeakAction("<emphasis>boooooooom dia time!</emphasis>", "goodmorning1"));
            SpeakActions.Add(new SpeakAction("bom dia time!", "goodmorning2"));
            SpeakActions.Add(new SpeakAction("<emphasis>bom dia galera!</emphasis>", "goodmorning3"));
            SpeakActions.Add(new SpeakAction("<prosody rate='fast'>bom diiia galera!</prosody>", "goodmorning4"));

        }

        public static ItemVM<string> FromTinBotAction(this TinBotAction a)
        {
            return new ItemVM<string>
            {
                Description = a.ToString(),
                Item = JsonConvert.SerializeObject(a, Formatting.Indented)
            };
        }

        public static void Dump(this Controller ctrl, object obj)
        {
            var dict =(Dictionary<string, string>) ctrl.ViewBag.Dict ?? new Dictionary<string, string>();

            dict.Add((obj??"null").ToString(),JsonConvert.SerializeObject(obj,Formatting.Indented));

            ctrl.ViewBag.Dict = dict;
        }
    }
}
