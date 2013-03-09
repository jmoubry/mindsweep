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
using System.Windows.Data;
using Microsoft.Phone.Tasks;

namespace Mindsweep.Views
{
    public partial class TaskView : PhoneApplicationPage
    {
        public TaskView()
        {
            InitializeComponent();
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (NavigationContext.QueryString.ContainsKey("taskid")
                && NavigationContext.QueryString.ContainsKey("taskseriesid"))
            {
                string taskId = NavigationContext.QueryString["taskid"];
                string taskseriesId = NavigationContext.QueryString["taskseriesid"];

                Task task = App.ViewModel.AllTasks.Where(t => t.Id == taskId && t.TaskSeries.Id == taskseriesId).FirstOrDefault();

                if (task != null)
                    DataContext = task;
            }


            FlurryWP7SDK.Api.LogEvent("Task");
        }

        private void Add_Click(object sender, EventArgs e)
        {
        }

        private void Edit_Click(object sender, EventArgs e)
        {
        }

        private void Delete_Click(object sender, EventArgs e)
        {
            App.ViewModel.ConfirmAndDelete(new List<Task>() { DataContext as Task }, this);
        }

        private void Complete_Click(object sender, EventArgs e)
        {
            App.ViewModel.Complete(DataContext as Task);
        }

        private void Postpone_Click(object sender, EventArgs e)
        {
            App.ViewModel.Postpone(DataContext as Task);
        }

        private void URL_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            WebBrowserTask webBrowserTask = new WebBrowserTask();
            webBrowserTask.Uri = new Uri((sender as TextBlock).Text, UriKind.Absolute);
            webBrowserTask.Show();
        }
    }
}