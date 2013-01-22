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

namespace Mindsweep.Views
{
    public partial class ProjectView : PhoneApplicationPage
    {
        public ProjectView()
        {
            InitializeComponent();

            this.DataContext = App.ViewModel;

            
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            string projectId = NavigationContext.QueryString["id"];

            Project proj = App.ViewModel.AllProjects.Where(p => p.Id == projectId).FirstOrDefault();

            if (proj == null)
            {
                TaskListBox.ItemsSource = null;
            }
            else
            {
                Title.Text = proj.Name;
                TaskListBox.ItemsSource = proj.TaskSeries.Where(t => t.Tasks.Any(tt => !tt.Completed.HasValue && !tt.Deleted.HasValue));
            }

            FlurryWP7SDK.Api.LogEvent("Project");
        }

        private void Add_Click(object sender, EventArgs e)
        {

        }
    }
}