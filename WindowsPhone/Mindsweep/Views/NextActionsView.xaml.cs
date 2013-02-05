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
using Telerik.Windows.Controls;

namespace Mindsweep.Views
{
    public partial class NextActionsView : PhoneApplicationPage
    {
        public NextActionsView()
        {
            InitializeComponent();

            this.DataContext = App.ViewModel;

            foreach (PivotItem item in NextActionsPivot.Items.ToList())
                if (item.Visibility == Visibility.Collapsed)
                    NextActionsPivot.Items.Remove(item);

            FlurryWP7SDK.Api.LogEvent("Next Actions");
        }

        private void Add_Click(object sender, EventArgs e)
        {
        }

        private void Delete_Click(object sender, EventArgs e)
        {
           // ConfirmAndDelete
        }

        private void Complete_Click(object sender, EventArgs e)
        {
            PivotItem pivotItem = (PivotItem)NextActionsPivot.SelectedItem;
            RadDataBoundListBox list = pivotItem.Content as RadDataBoundListBox;

            foreach (var item in list.CheckedItems)
                App.ViewModel.Complete(item as Task);

            list.IsCheckModeActive = false;
        }

        private void Postpone_Click(object sender, EventArgs e)
        {
            PivotItem pivotItem = (PivotItem)NextActionsPivot.SelectedItem;
            RadDataBoundListBox list = pivotItem.Content as RadDataBoundListBox;

            foreach (var item in list.CheckedItems.ToList())
                App.ViewModel.Postpone(item as Task);

            list.IsCheckModeActive = false;
        }
    }
}