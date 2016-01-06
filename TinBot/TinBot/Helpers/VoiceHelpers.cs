using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinBot.Helpers
{
    public static class VoiceHelpers
    {
        public static Dictionary<string,string> Phrases { get; } = new Dictionary<string, string>()
        {
            ["introduce"]= "<mark name='feliz'/> Olá! Meu nome é timboóti, Minha função é ajudar o time. <mark name='piscando'/> " +
                           "Eu estou aqui para dar recados, monitorar os indicadores, <mark name='piscando_duplo'/> alertar o time quando for preciso. " +
                           "<mark name='piscando'/> Há, eu também monitoro bíldis, " +
                           "e se alguém fizer besteira eu aponto o dedo <emphasis> na cara</emphasis> <mark name='piscadela'/>, rárrarrárrárra",
            ["goodmorning1"]= "<emphasis>boooooooom dia time!</emphasis>",
            ["goodmorning2"]= "bom dia time!",
            ["goodmorning3"]= "<emphasis>bom dia galera!</emphasis>",
            ["goodmorning4"]= "<prosody rate='fast'>bom diiia galera!</prosody>",
            ["marcaoneia"]= "<mark name='normal'/><break time='10s' />Olá,<mark name='feliz'/> Marcão e Néia! Tudo bom com vocês?. <mark name='piscando_duplo'/> " +
                           "boa noite aí <mark name='piscadela'/>",
        };

        static VoiceHelpers()
        {
            
        }

        public static string WrapWithSSML(string text)
        {
            return
                "<speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' " +
                "xsi:schemaLocation='http://www.w3.org/2001/10/synthesis  http://www.w3.org/TR/speech-synthesis/synthesis.xsd' xml:lang='pt-BR'>" +
                "<prosody volume='110' pitch='900Hz'>" +
                "<mark name='start_talking'/>" +
                text+
                "<mark name='end_talking'/>" +
                "</prosody></speak>";
        }
    }
}
