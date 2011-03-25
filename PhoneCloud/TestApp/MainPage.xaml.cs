using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;

using TestApp.MyScienceService;

namespace TestApp
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            MyScienceServiceClient client = new MyScienceServiceClient();
            client.GetProjectsCompleted += new EventHandler<GetProjectsCompletedEventArgs>(client_GetProjectsCompleted);
            client.GetProjectsAsync();
        }

        void client_GetProjectsCompleted(object sender, GetProjectsCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                this.MainListBox.ItemsSource = e.Result;
            }
            this.MainListBox.Visibility = System.Windows.Visibility.Visible;
        }
    }
}