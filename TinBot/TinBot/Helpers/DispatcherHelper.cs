using System;
using Windows.UI.Core;

namespace TinBot.Helpers
{
    public static class DispatcherHelper
    {
        public static CoreDispatcher Dispatcher { get; set; }

        public static async void ExecuteOnMainThread(Action action)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => action());
        }

    }
}
