using System;
using System.Threading.Tasks;
using Windows.UI.Core;

namespace TinBot.Helpers
{
    public static class DispatcherHelper
    {
        static DispatcherHelper()
        {
            //Dispatcher = Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher;
        }

        public static CoreDispatcher Dispatcher { get; set; }
        public static TaskScheduler SyncContext { get; set; }

        public static async Task ExecuteOnMainThread(Action action)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.High, () => action());
        }

        public static async Task ExecuteOnSyncContext(Action action)
        {
            await Task.Factory.StartNew(action, Task.Factory.CancellationToken,
             TaskCreationOptions.None, SyncContext);
        }

        public static async Task<T> ExecuteOnSyncContext<T>(Func<T> action)
        {
            return await Task.Factory.StartNew(action, Task.Factory.CancellationToken,
             TaskCreationOptions.None, SyncContext);
        }
    }
}
