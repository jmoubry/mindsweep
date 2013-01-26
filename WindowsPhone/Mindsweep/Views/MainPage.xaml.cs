using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Mindsweep.Helpers;
using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace Mindsweep.Views
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

            App.ViewModel.SyncCompleted += ViewModel_SyncCompleted;

#if DEBUG
            App.ViewModel.Token = App.RtmDebugToken;
#endif
            
            FlurryWP7SDK.Api.LogEvent("Main");
        }

        protected void ViewModel_SyncCompleted(object sender, EventArgs e)
        {
            _HideSystemStatus();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            _SyncIfNeeded();
        }

        private void LogIn_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Views/Login.xaml", UriKind.RelativeOrAbsolute));
        }

        private void LogOut_Click(object sender, EventArgs e)
        {
            App.ViewModel.Token = null;
        }

        private void _Sync()
        {
            if (App.ViewModel.IsLoggedIn)
            {
                ProgressIndicator prog = new ProgressIndicator()
                {
                    IsVisible = true,
                    IsIndeterminate = true,
                    Text = "Syncing..."
                };

                SystemTray.SetProgressIndicator(this, prog);
                
                App.ViewModel.Sync();
            }
        }

        Timer statusUITimer;

        private void _HideSystemStatus(object o = null)
        {
            Dispatcher.BeginInvoke(() =>
            {

                var prog = SystemTray.GetProgressIndicator(this);
                if (prog != null)
                {
                    prog.IsIndeterminate = false;
                    prog.IsVisible = false;
                }
            }
            );
        }

        private void _SyncIfNeeded()
        {
            if (!App.ViewModel.IsSynced)
            {
                _Sync();
            }
            else
            {
                ProgressIndicator prog = new ProgressIndicator()
                {
                    IsVisible = true,
                    IsIndeterminate = false
                };

                TimeSpan lastSynced = DateTime.UtcNow - App.ViewModel.LastSync;

                if (lastSynced.TotalMinutes <= 1)
                    prog.Text = string.Format("Last synced {0} seconds ago", Math.Round(lastSynced.TotalSeconds));
                else
                    prog.Text = string.Format("Last synced {0} minutes ago", Math.Round(lastSynced.TotalMinutes));

                SystemTray.SetProgressIndicator(this, prog);

                statusUITimer = new System.Threading.Timer(_HideSystemStatus, null, 5000, Timeout.Infinite);
            }
        }

        #region UIEvents

        private void Sync_Click(object sender, EventArgs e)
        {
            _Sync();
        }

        private void Search_Click(object sender, EventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/Views/Search.xaml", UriKind.RelativeOrAbsolute));
        }

        private void Add_Click(object sender, EventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/Views/AddTask.xaml", UriKind.RelativeOrAbsolute));
        }

        private void DeleteDB_Click(object sender, EventArgs e)
        {
            App.ViewModel.DeleteDB();
        }

        private void About_Click(object sender, EventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/Views/About.xaml", UriKind.RelativeOrAbsolute));
        }

        private void GoToInbox(object sender, GestureEventArgs e)
        {
            var inbox = App.ViewModel.AllProjects.Where(Exp.IsInbox).FirstOrDefault();

            if (inbox == null)
                MessageBox.Show("Error accessing your Inbox. Try again later.");
            else
                this.NavigationService.Navigate(new Uri("/Views/ProjectView.xaml?id=" + inbox.Id, UriKind.RelativeOrAbsolute));
        }

        private void GoToNextActions(object sender, GestureEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/Views/NextActionsView.xaml", UriKind.RelativeOrAbsolute));
        }

        private void GoToProjects(object sender, GestureEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/Views/Projects.xaml", UriKind.RelativeOrAbsolute));
        }

        private void GoToContexts(object sender, GestureEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/Views/Tags.xaml", UriKind.RelativeOrAbsolute));
        }

        private void GoToFlagged(object sender, GestureEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/Views/Flagged.xaml", UriKind.RelativeOrAbsolute));
        }

        #endregion
    }
}
