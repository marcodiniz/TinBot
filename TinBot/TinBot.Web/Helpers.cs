using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TinBot.Web
{
    public static class Helpers
    {
        private static Random _rnd = new Random((int) DateTime.Now.Ticks);

        public static T DeepCopy<T>(this T obj) where T:class 
        {
            if (obj == null)
                return null;

            var str = JsonConvert.SerializeObject(obj);
            return JsonConvert.DeserializeObject<T>(str);
        }

        public static T GetRandom<T>(this IEnumerable<T> collection)
        {
            var list = collection.ToList();
            var i = new Random(_rnd.Next()).Next(0, list.Count);
            Console.WriteLine(typeof(T).Name);
            Console.WriteLine("count "+ list.Count);
            Console.WriteLine(i);
            return list[i];
        }
    }
}
