using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Mindsweep.Model;
using Telerik.Windows.Controls;

namespace Mindsweep
{
    public partial class Projects : PhoneApplicationPage
    {
        public Projects()
        {
            InitializeComponent();

            this.DataContext = App.ViewModel;

            FlurryWP7SDK.Api.LogEvent("Projects");
        }

        private void Add_Click(object sender, EventArgs e)
        {

        }

        private void Filter_Click(object sender, EventArgs e)
        {

        }

        private void RadDataBoundListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender == null)
                return;

            Project proj = (sender as RadDataBoundListBox).SelectedItem as Project;

            // De-select so that it may be re-selected.
            (sender as RadDataBoundListBox).SelectedItem = null;

            if (proj == null)
                MessageBox.Show("Error loading project. Please try again later.");
            else
                this.NavigationService.Navigate(new Uri("/ProjectView.xaml?id=" + proj.Id, UriKind.RelativeOrAbsolute));
        }
    }
}