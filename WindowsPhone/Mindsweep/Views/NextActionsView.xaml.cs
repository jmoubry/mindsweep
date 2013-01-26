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
    public partial class NextActionsView : PhoneApplicationPage
    {
        public NextActionsView()
        {
            InitializeComponent();

            this.DataContext = App.ViewModel;

            FlurryWP7SDK.Api.LogEvent("Next Actions");
        }

        private void Add_Click(object sender, EventArgs e)
        {

        }
    }
}