using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;
using Newtonsoft.Json;
using TinBot.Portable;
using static TinBot.Helpers.DispatcherHelper;

namespace TinBot
{
    public static class TinBotData
    {

        public static ActionsContainer ActionsLib { get; set; } = new ActionsContainer();
        public static ObservableCollection<TinBotAction> ActionsQueue { get; set; } = new ObservableCollection<TinBotAction>();

        public static Timer LibTimer { get; set; }
        public static Timer QueueTimer { get; set; }

        public static event EventHandler ActionRequestArrived;

        private static bool _initializing = true;
        private static ApplicationDataContainer _settings;
        private static AuthenticationHeaderValue _authHeader => AuthenticationHeaderValue.Parse("basic " +
                                                Convert.ToBase64String(
                                                    Encoding.ASCII.GetBytes($"{ApiUser}:{ApiPassword}")));

        public static string ApiLibraryUrl
        {
            get { return _settings.Values["ApiLibraryUrl"]?.ToString()??""; }
            set { _settings.Values["ApiLibraryUrl"] = value; }
        }

        public static string ApiQueueUrl
        {
            get { return _settings.Values["ApiQueueUrl"]?.ToString() ?? ""; }
            set { _settings.Values["ApiQueueUrl"] = value; }
        }

        public static string ApiUser
        {
            get { return _settings.Values["ApiUser"]?.ToString() ?? ""; }
            set { _settings.Values["ApiUser"] = value; }
        }

        public static string ApiPassword
        {
            get { return _settings.Values["ApiPassword"]?.ToString() ?? ""; }
            set { _settings.Values["ApiPassword"] = value; }
        }

        static TinBotData()
        {
            LibTimer = new Timer(s => SyncActionLibrary(), null, TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(30));
            QueueTimer = new Timer(s => SyncQueue(), null, TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(10));
            _settings = Windows.Storage.ApplicationData.Current.LocalSettings;
        }

        private static void SyncQueue()
        {
            if (_initializing)
                return;

            try
            {
                var client = new HttpClient();
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Chrome/22.0.1229.94");
                client.DefaultRequestHeaders.Authorization = _authHeader;
                var response = client.GetAsync(ApiQueueUrl, HttpCompletionOption.ResponseContentRead).Result;
                if (response.IsSuccessStatusCode)
                {
                    var result =
                        (List<ActionContainer>)
                            JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result,
                                typeof(List<ActionContainer>));

                    if (!result.Any())
                        return;

                    foreach (var actionsContainer in result)
                    {
                        var arrived = actionsContainer.GetAction();
                        if (arrived != null)
                        {
                            ExecuteOnMainThread(() => ActionsQueue.Add(arrived)).Wait();
                        }
                    }
                    client.DeleteAsync(ApiQueueUrl).Wait();
                    ActionRequestArrived?.Invoke(null, EventArgs.Empty);

                }
            }
            catch (Exception ex)
            {
                var a = ex;
            }
        }

        public static void SyncActionLibrary()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Chrome/22.0.1229.94");
            client.DefaultRequestHeaders.Authorization = _authHeader;
            try
            {
                var response = client.GetAsync(ApiLibraryUrl).Result;

                if (response.IsSuccessStatusCode)
                {
                    ActionsLib =
                        (ActionsContainer)
                            JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result,
                                typeof(ActionsContainer));

                    if (_initializing)
                    {
                        Task.Delay(500).Wait();
                        ExecuteOnMainThread(() => ActionsQueue.Add(ActionsLib["Rest"])).Wait();
                        Task.Delay(300).Wait();
                        ActionRequestArrived?.Invoke(null, EventArgs.Empty);
                        _initializing = false;
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
    }
}
