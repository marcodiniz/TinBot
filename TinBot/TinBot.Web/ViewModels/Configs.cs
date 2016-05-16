using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TinBot.Web.ViewModels
{
    public class NamesMapping
    {
        public string From { get; set; }
        public string To{ get; set; }
    }

    public class Configs
    {
        public List<NamesMapping> UserNamesToUserName { get; set; } = new List<NamesMapping>();
        public List<NamesMapping> UserNameToDisplayName{ get; set; } = new List<NamesMapping>();

        public int IPT { get; set; }
        public int IPTBadLimit { get; set; }
        public int IPTGoodLimit { get; set; }

        public int IET { get; set; }
        public int IETBadLimit { get; set; }
        public int IETGoodLimit { get; set; }

        public string GetRealUserName(string userName)
        {
            if (string.IsNullOrEmpty(userName))
                return null;
            return UserNamesToUserName.FirstOrDefault(x => x.From.ToUpper().Split(';').Contains(userName.ToUpper()))?.To;
        }

        public string GetDisplayName(string userName)
        {
            if (string.IsNullOrEmpty(userName))
                return null;

            Console.WriteLine(userName);
            var realUserName = GetRealUserName(userName);
            var displays=
                UserNameToDisplayName.FirstOrDefault(x => x.From.Equals(realUserName, StringComparison.Ordinal))?.To.Split(';');

            if (displays!=null && displays.Any())
                return displays[new Random().Next(displays.Length)];

            return null;
        }
    }
}
