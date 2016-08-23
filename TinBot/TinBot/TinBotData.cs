using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;
using Windows.Storage.Streams;
using Newtonsoft.Json;
using TinBot.Portable;
using static TinBot.Helpers.DispatcherHelper;

namespace TinBot
{
    public static class TinBotData
    {
        public static ObservableCollection<TinBotAction> ActionsQueue { get; set; } = new ObservableCollection<TinBotAction>();

        public static Timer LibTimer { get; set; }
        public static Timer QueueTimer { get; set; }

        public static event EventHandler ActionRequestArrived;

        private static ApplicationDataContainer _settings;

        //private static AuthenticationHeaderValue _authHeader =>
        //    AuthenticationHeaderValue.Parse("basic " +
        //                                    Convert.ToBase64String(
        //                                        Encoding.ASCII.GetBytes($"{ApiUser}:{ApiPassword}")));

        public static ActionsContainer ActionsLib { get; set; }

        public static string ApiLibraryUrl
        {
            get { return _settings.Values["ApiLibraryUrl"]?.ToString() ?? ""; }
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
            Setup();
        }

        private static async Task Setup()
        {
            LibTimer = new Timer(s => SyncActionLibrary(), null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(60));
            QueueTimer = new Timer(s => SyncQueue(), null, TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(10));
            _settings = ApplicationData.Current.LocalSettings;

            var result = await ReadSetting<ActionsContainer>("ActionsLib");
            ActionsLib = result ?? new ActionsContainer();
            var rest = ActionsLib["rest"];
            if (rest != null)
                await ExecuteOnMainThread(() => ActionsQueue.Add(ActionsLib["rest"]));
        }

        private static void SyncQueue()
        {
            if (!ActionsLib.AllActions().Any())
                return;

            try
            {
                var client = new HttpClient();
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Chrome/22.0.1229.94");
                //client.DefaultRequestHeaders.Authorization = _authHeader;
                var response = client.GetAsync(ApiQueueUrl, HttpCompletionOption.ResponseContentRead).Result;
                if (response.IsSuccessStatusCode)
                {
                    var result =
                        (List<ActionContainer>)
                            JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result,
                                typeof(List<ActionContainer>));


                    foreach (var actionsContainer in result)
                    {
                        var arrived = actionsContainer.GetAction();
                        if (arrived != null)
                        {
                            ExecuteOnMainThread(() => ActionsQueue.Add(arrived)).Wait();
                        }
                    }

                    if (result.Any())
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
            //client.DefaultRequestHeaders.Authorization = _authHeader;
            client.Timeout = TimeSpan.FromSeconds(30);
            try
            {
                var response = client.GetAsync(ApiLibraryUrl).Result;

                if (response.IsSuccessStatusCode)
                {
                    var str = response.Content.ReadAsStringAsync().Result;
                    ActionsLib = JsonConvert.DeserializeObject<ActionsContainer>(str);
                    SaveSetting("ActionsLib", ActionsLib).GetAwaiter();
                }
            }
            catch (Exception ex)
            {
            }
        }

        internal static async Task<bool> SaveSetting(string key, object value)
        {
            var file =
                await ApplicationData.Current.LocalFolder.CreateFileAsync(key, CreationCollisionOption.ReplaceExisting);
            using (Stream fileStream = await file.OpenStreamForWriteAsync())
            {
                var str = value is string ? (string)value : JsonConvert.SerializeObject(value);
                var bytes = Encoding.ASCII.GetBytes(str);
                await fileStream.WriteAsync(bytes, 0, bytes.Length);
                await fileStream.FlushAsync();
            }
            return true;
        }

        internal static async Task<T> ReadSetting<T>(string key) where T : class
        {
            T rr;

            try
            {
                StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync(key);
                using (IInputStream inStream = await file.OpenSequentialReadAsync())
                {
                    var stream = new StreamReader(inStream.AsStreamForRead());
                    rr = JsonConvert.DeserializeObject<T>(stream.ReadToEnd());
                }
            }
            catch (FileNotFoundException ex)
            {
                return null;
            }
            return rr;
        }
    }
}
