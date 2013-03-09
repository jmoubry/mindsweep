using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Mindsweep.Helpers;
using System.Xml.Linq;

namespace Mindsweep.Views
{
    public partial class Login : PhoneApplicationPage
    {
        WebClient client;

        public Login()
        {
            InitializeComponent();

            client = new WebClient();
            client.DownloadStringCompleted += client_DownloadStringCompleted;
        }

        private string _frob;

        void client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            // TODO: handle errors
            XDocument d = XDocument.Parse(e.Result);

            (e.UserState as Action<XDocument>)(d);
        }

        public void GetFrob()
        {
            client.DownloadStringAsync(RTM.SignRequest(RTM.URI_GET_FROB), new Action<XDocument>(LogIn));
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            GetFrob();
        }

        public void LogIn(XDocument xml)
        {
            _frob = xml.Descendants("frob").Select(f => f.Value).FirstOrDefault();

            if (string.IsNullOrEmpty(_frob))
                ;// TODO: handle error
            else
            {
                wbLogin.Navigate(RTM.SignRequest(RTM.URI_AUTH + "&frob=" + _frob));
            }
        }

        public void Done(XDocument xml)
        {
            // TODO: on an api error, check token to see if expired.

            App.ViewModel.Login(xml.Descendants("token").Select(t => t.Value).FirstOrDefault());

            var user = xml.Descendants("user").FirstOrDefault();

            if (user != null)
            {
                App.ViewModel.User = new ViewModels.MainViewModel.UserInfo() 
                {
                    Id = user.Attribute("id").Value,
                    Username = user.Attribute("username").Value,
                    FullName = user.Attribute("fullname").Value 
                };
            }

            this.NavigationService.Navigate(new Uri("/Views/MainPage.xaml", UriKind.RelativeOrAbsolute));
        }

        private void Retry_Click(object sender, RoutedEventArgs e)
        {
            GetFrob();
        }


        private void Done_Click(object sender, RoutedEventArgs e)
        {
            client.DownloadStringAsync(RTM.SignRequest(RTM.URI_GET_TOKEN + "&frob=" + _frob), new Action<XDocument>(Done));
        }

        private void wbLogin_Navigating(object sender, NavigatingEventArgs e)
        {
            
        }

    }
}