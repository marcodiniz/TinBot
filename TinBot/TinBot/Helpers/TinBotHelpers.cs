using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Sensors;
using TinBot.Commands;
using TinBot.Portable;

namespace TinBot.Helpers
{
    public static class TinBotHelpers
    {
        public static Commands.Commands Commands { get; set; }

        public static ObservableCollection<TinBotAction> SavedActions { get; } = new ObservableCollection<TinBotAction>();

        static TinBotHelpers()
        {
            SavedActions.Add(new MovementAcion(ETinBotServo.ServoHeadY, 180, 100, 10,"HeadDown"));
            SavedActions.Add(new MovementAcion(ETinBotServo.ServoHeadY, 0, 100, 10,"HeadUp"));

            SavedActions.Add(new FaceAction(ETinBotFaces.Normal, "Normal"));
            SavedActions.Add(new FaceAction(ETinBotFaces.Angry, "Angry"));
            SavedActions.Add(new FaceAction(ETinBotFaces.Blink, "Blink"));
            SavedActions.Add(new FaceAction(ETinBotFaces.BlinkDouble, "BlinkDouble"));
            SavedActions.Add(new FaceAction(ETinBotFaces.UniBlink, "UniBlink"));
            SavedActions.Add(new FaceAction(ETinBotFaces.Happy, "Happy"));
            SavedActions.Add(new FaceAction(ETinBotFaces.HappyGreen, "HappyGreen"));
            SavedActions.Add(new FaceAction(ETinBotFaces.Sad, "Sad"));

            SavedActions.Add(new SpeakAction(
                "<mark name='Happy'/> Olá! Meu nome é timboóti, Minha função é ajudar o time. <mark name='Blink'/> " +
                "Eu estou aqui para dar recados, monitorar os indicadores, <mark name='BlinkDouble'/> alertar o time quando for preciso. " +
                "<mark name='Blink'/> Há, eu também monitoro bíldis, " +
                "e se alguém fizer besteira eu aponto o dedo <emphasis> na cara <mark name='UniBlink'/> </emphasis>, rárrarrárrárra",
                "introduce"));
            SavedActions.Add(new SpeakAction("<emphasis>boooooooom dia time!</emphasis>", "goodmorning1"));
            SavedActions.Add(new SpeakAction("bom dia time!", "goodmorning2"));
            SavedActions.Add(new SpeakAction("<emphasis>bom dia galera!</emphasis>", "goodmorning3"));
            SavedActions.Add(new SpeakAction("<prosody rate='fast'>bom diiia galera!</prosody>", "goodmorning4"));

        }


        public static string WrapWithSSML(string text)
        {
            return
                "<speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' " +
                "xsi:schemaLocation='http://www.w3.org/2001/10/synthesis  http://www.w3.org/TR/speech-synthesis/synthesis.xsd' xml:lang='pt-BR'>" +
                "<prosody volume='110' pitch='900Hz'>" +
                "<mark name='start_talking'/>" +
                text +
                "<mark name='end_talking'/>" +
                "</prosody></speak>";
        }

        public static List<TEnum> Values<TEnum>()
 where TEnum : struct, IComparable, IFormattable, IConvertible
        {
            var enumType = typeof(TEnum);

            return Enum.GetValues(enumType).Cast<TEnum>().ToList();
        }
    }
}
