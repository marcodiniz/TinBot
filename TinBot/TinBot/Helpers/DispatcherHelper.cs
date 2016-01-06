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

        public static async Task ExecuteOnMainThread(Action action)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.High, () => action());
        }

    }
}
