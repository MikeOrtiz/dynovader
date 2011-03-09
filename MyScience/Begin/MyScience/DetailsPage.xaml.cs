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
using Microsoft.Phone.Info;
using System.Text;
using System.Runtime.Serialization;


namespace MyScience
{
    

    public partial class DetailsPage : PhoneApplicationPage
    {
        public DetailsPage()
        {
            InitializeComponent();
        }

        void settingsButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/SettingPage.xaml", UriKind.Relative));
        }

        void rankButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/RankPage.xaml", UriKind.Relative));
        }

        void profileButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/ProfilePage.xaml", UriKind.Relative));
        }

        void homeButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
        }

        private void DetailsPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (NavigationContext.QueryString.ContainsKey("selectedItem"))
            {
                App.currentIndex = Convert.ToInt32(NavigationContext.QueryString["selectedItem"]);
                Project currentApp = App.applist[App.currentIndex];
                PageTitle.Text = currentApp.Name;
                DescriptionBlock.Text = currentApp.Description;
                // Set the phone ID
                byte[] result = null;
                object uniqueId;
                if (DeviceExtendedProperties.TryGetValue("DeviceUniqueId", out uniqueId))
                    result = (byte[]) uniqueId;
                idWrapper.Text = "ID: "+BitConverter.ToString(result);
                // Set the 

            }
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {

            WebClient client = new WebClient();

            //client.UploadStringAsync(new Uri("http://128.12.62.142/dynovader/json.php?action=submit&projectid=7&latitude=37.430299&longitude=-122.173349"),"GET");
            client.UploadStringAsync(new Uri("http://128.12.62.142/dynovader/json.php?action=submit&projectid=" + App.applist[App.currentIndex].ID + "&latitude=37.430299&longitude=-122.173349"), "GET");

            client.UploadStringCompleted += (s, ev) =>
            {
                String result = ev.Result;
            };

            //NavigationService.Navigate(new Uri("/SubmissionPage.xaml", UriKind.Relative));

        }


    }
}