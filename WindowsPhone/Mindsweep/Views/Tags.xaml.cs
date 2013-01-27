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
using Mindsweep.ViewModels;
using Mindsweep.Helpers;

namespace Mindsweep.Views
{
    public partial class Tags : PhoneApplicationPage
    {
        public Tags()
        {
            InitializeComponent();

            this.DataContext = App.ViewModel;

            List<string> tags = new List<string>();

            App.ViewModel.AllTasks.Where(Exp.HasTags).Where(Exp.IsOpen).Select(t => t.TaskSeries.Tags.Split(',')).ToList().ForEach(t => tags.AddRange(t));

            TagsListBox.ItemsSource = tags.Distinct().OrderBy(t => t);

            FlurryWP7SDK.Api.LogEvent("Tags");
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

            string tag = (sender as RadDataBoundListBox).SelectedItem as string;

            // De-select so that it may be re-selected.
            (sender as RadDataBoundListBox).SelectedItem = null;

            if (tag == null)
                MessageBox.Show("Error loading tag. Please try again later.");
            else
                this.NavigationService.Navigate(new Uri("/Views/TasksView.xaml?tag=" + tag, UriKind.RelativeOrAbsolute));
        }
    }
}