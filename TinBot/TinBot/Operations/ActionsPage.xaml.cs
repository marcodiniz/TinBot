using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using TinBot.Helpers;
using TinBot.Portable;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace TinBot
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ActionsPage : Page
    {
        public ObservableCollection<TinBotAction> ActionsQueue => TinBotData.ActionsQueue;

        public ActionsPage()
        {
            this.InitializeComponent();


            this.Loaded += (sender, args) =>
            {
                listBox.DataContext = TinBotData.ActionsQueue;
                _txtLibraryUrl.Text = TinBotData.ApiLibraryUrl;
                _txtQueueUrl.Text = TinBotData.ApiQueueUrl;
                _txtUser.Text = TinBotData.ApiUser;
                _txtPassword.Password = TinBotData.ApiPassword;
            };
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private void _btnSave_Click(object sender, RoutedEventArgs e)
        {
            TinBotData.ApiLibraryUrl = _txtLibraryUrl.Text;
            TinBotData.ApiQueueUrl= _txtQueueUrl.Text;
            TinBotData.ApiUser= _txtUser.Text;
            TinBotData.ApiPassword = _txtPassword.Password;
        }
    }
}
