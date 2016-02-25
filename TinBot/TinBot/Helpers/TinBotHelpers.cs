using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Sensors;
using TinBot.Operations;
using TinBot.Portable;

namespace TinBot.Helpers
{
    public static class TinBotHelpers
    {
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
