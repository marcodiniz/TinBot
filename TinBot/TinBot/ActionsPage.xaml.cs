using System;
using System.Collections.Generic;
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
        public IObservable<MovementAcion> MovementAcion { get; private set; }
        public List<ETinBotServo> TinBotServos => TinBotHelpers.Values<ETinBotServo>();

        public ActionsPage()
        {
            this.InitializeComponent();

            listBox.DataContext = TinBotHelpers.SavedActions;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private void ListBox_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var action = listBox.SelectedItem as TinBotAction;
            TinBotHelpers.Commands.ExecuteAction(action);
        }


        private void ListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var action = (TinBotAction)listBox.SelectedItem;
            switch (action.Type)
            {
                case EActionType.Speak:
                    break;
                case EActionType.Face:
                    break;
                case EActionType.Move:
                     PivotItemMovment.DataContext =  (MovementAcion) action;
                    break;
                case EActionType.Saved:
                    break;
                case EActionType.Sequence:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
