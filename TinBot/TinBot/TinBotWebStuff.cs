using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Data.Json;
using Newtonsoft.Json;
using TinBot.Portable;

namespace TinBot
{
    public static class TinBotWebStuff
    {
        public static string ApiLibraryUrl { get; set; }
        public static string ApiQueueUrl { get; set; }

        public static ActionsContainer ActionsLib { get; set; }
        public static Queue<TinBotAction> ActionsQueue { get; set; }

        public static Timer LibTimer { get; set; }
        public static Timer QueueTimer { get; set; }

        static TinBotWebStuff()
        {
            ApiLibraryUrl = "http://localhost:5000/api/Library";
            ApiQueueUrl = "http://localhost:5000/api/queue";
            ActionsQueue = new Queue<TinBotAction>();
            LibTimer = new Timer(s => SyncActionLibrary(), null, TimeSpan.Zero, TimeSpan.FromMinutes(10));
            QueueTimer = new Timer(s => SyncQueueLibrary(), null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
        }

        private static void SyncQueueLibrary()
        {
            var client = new HttpClient();
            var response = client.GetAsync(ApiQueueUrl).Result;
            if (response.IsSuccessStatusCode)
            {
                var result =
                    (List<ActionsContainer>)
                        JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result,
                            typeof (List<ActionsContainer>));

                foreach (var actionsContainer in result)
                {
                    ActionsQueue.Enqueue(actionsContainer.AllActions().FirstOrDefault());
                }
            }
        }

        public static void SyncActionLibrary()
        {
            var client = new HttpClient();
            var response = client.GetAsync(ApiLibraryUrl).Result;
            if (response.IsSuccessStatusCode)
            {
                ActionsLib =
                    (ActionsContainer)
                        JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result,
                            typeof (ActionsContainer));
            }
        }
    }
}
