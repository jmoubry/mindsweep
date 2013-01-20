using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.IO.IsolatedStorage;

namespace Mindsweep
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            //Shows the rate reminder message, according to the settings of the RateReminder.
            (App.Current as App).rateReminder.Notify();

            this.DataContext = App.ViewModel;

#if DEBUG
            App.ViewModel.Token = App.RtmDebugToken;
#endif
            
            FlurryWP7SDK.Api.LogEvent("Main");
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (App.ViewModel.IsLoggedIn && !App.ViewModel.IsSynced)
                App.ViewModel.Sync();
        }

        private void LogIn_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Login.xaml", UriKind.RelativeOrAbsolute));
        }

        private void LogOut_Click(object sender, EventArgs e)
        {
            App.ViewModel.Token = null;
        }

        private void Sync_Click(object sender, EventArgs e)
        {
            App.ViewModel.Sync();
        }

        private void Search_Click(object sender, EventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/Search.xaml", UriKind.RelativeOrAbsolute));
        }

        private void Add_Click(object sender, EventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/AddTask.xaml", UriKind.RelativeOrAbsolute));
        }

        private void DeleteDB_Click(object sender, EventArgs e)
        {
            App.ViewModel.DeleteDB();
        }

        private void About_Click(object sender, EventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/About.xaml", UriKind.RelativeOrAbsolute));
        }

        private void GoToInbox(object sender, GestureEventArgs e)
        {
            var inbox = App.ViewModel.AllProjects.Where(p => p.Name == "Inbox" && p.Locked).FirstOrDefault();

            if (inbox == null)
                MessageBox.Show("Error accessing your Inbox. Try again later.");
            else
                this.NavigationService.Navigate(new Uri("/ProjectView.xaml?id=" + inbox.Id, UriKind.RelativeOrAbsolute));
        }

        private void GoToNextActions(object sender, GestureEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/NextActions.xaml", UriKind.RelativeOrAbsolute));
        }

        private void GoToProjects(object sender, GestureEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/Projects.xaml", UriKind.RelativeOrAbsolute));
        }

        private void GoToContexts(object sender, GestureEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/Contexts.xaml", UriKind.RelativeOrAbsolute));
        }

        private void GoToFlagged(object sender, GestureEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/Flagged.xaml", UriKind.RelativeOrAbsolute));
        }
    }
}
