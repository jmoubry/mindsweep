using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace Mindsweep
{
    public partial class Projects : PhoneApplicationPage
    {
        public Projects()
        {
            InitializeComponent();

            this.DataContext = App.ViewModel;

            FlurryWP7SDK.Api.LogEvent("Main");
        }

        private void Add_Click(object sender, EventArgs e)
        {

        }

        private void Filter_Click(object sender, EventArgs e)
        {

        }
    }
}