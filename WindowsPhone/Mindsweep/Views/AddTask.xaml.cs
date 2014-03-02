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
using System.Windows.Input;

namespace Mindsweep.Views
{
    public partial class AddTask : PhoneApplicationPage
    {
        public AddTask()
        {
            InitializeComponent();

            this.DataContext = new Task() { TaskSeries = new TaskSeries() };

            FlurryWP7SDK.Api.LogEvent("Add Task");
        }

        private void Ok_Click(object sender, EventArgs e)
        {
            object focusObj = FocusManager.GetFocusedElement();
            if (focusObj != null && focusObj is TextBox)
            {
                var binding = (focusObj as TextBox).GetBindingExpression(TextBox.TextProperty);
                binding.UpdateSource();
            }

            App.ViewModel.Add(DataContext as Task);

            // Return to the main page.
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }
    }
}