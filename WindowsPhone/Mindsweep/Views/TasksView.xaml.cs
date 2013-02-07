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
using Mindsweep.Helpers;

namespace Mindsweep.Views
{
    public partial class TasksView : PhoneApplicationPage
    {
        public TasksView()
        {
            InitializeComponent();

            this.DataContext = App.ViewModel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (NavigationContext.QueryString.ContainsKey("id"))
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
                    TaskListBox.ItemsSource = App.ViewModel.AllTasks.Where(t => t.TaskSeries.Project.Id == proj.Id).Where(Exp.IsOpen).OrderBy(t => t.Due).ThenBy(t => t.Priority).ThenBy(t => t.TaskSeries.Name);
                }
            }
            else
            {
                string tag = NavigationContext.QueryString["tag"];

                Title.Text = tag;

                var tasksForTag = App.ViewModel.AllTasks.Where(Exp.IsOpen).Where(Exp.HasTags).Where(t => t.TaskSeries.Tags.Split(',').Contains(tag)).OrderBy(t => t.Due).ThenBy(t => t.Priority).ThenBy(t => t.TaskSeries.Name).ToList();

                TaskListBox.ItemsSource = tasksForTag;
            }

            FlurryWP7SDK.Api.LogEvent("Project");
        }


        private void Add_Click(object sender, EventArgs e)
        {
        }

        private void Edit_Click(object sender, EventArgs e)
        {
        }

        private void Delete_Click(object sender, EventArgs e)
        {
            App.ViewModel.ConfirmAndDelete(TaskListBox.CheckedItems.Cast<Task>().ToList(), this);

            TaskListBox.IsCheckModeActive = false;
        }

        private void Complete_Click(object sender, EventArgs e)
        {
            foreach (var item in TaskListBox.CheckedItems)
                App.ViewModel.Complete(item as Task);

            TaskListBox.IsCheckModeActive = false;
        }

        private void Postpone_Click(object sender, EventArgs e)
        {
            foreach (var item in TaskListBox.CheckedItems.ToList())
                App.ViewModel.Postpone(item as Task);

            TaskListBox.IsCheckModeActive = false;
        }
    }
}