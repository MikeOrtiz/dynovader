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
using Microsoft.Phone.Info;
using Microsoft.Phone.Controls;
using MyScience.MyScienceService;

namespace MyScience
{
    public partial class SignIn : PhoneApplicationPage
    {
        public SignIn()
        {
            InitializeComponent();
        }

        private void SignInPage_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void registerButton_Click(object sender, RoutedEventArgs e)
        {
            /* Get phoneid */
            byte[] result = null;
            object uniqueId;
            if (DeviceExtendedProperties.TryGetValue("DeviceUniqueId", out uniqueId))
                result = (byte[])uniqueId;
            String phoneID = BitConverter.ToString(result);

            /*Parse the fields list into Json String*/
            //String data = GetJsonString(fields);
            MyScienceServiceClient client = new MyScienceServiceClient();
            client.RegisterUserCompleted += new EventHandler<RegisterUserCompletedEventArgs>(client_RegisterUserCompleted);
            client.RegisterUserAsync(0, phoneID, registerNameBox.Text);
        }

        private void signInButton_Click(object sender, RoutedEventArgs e)
        {
            byte[] result = null;
            object uniqueId;
            if (DeviceExtendedProperties.TryGetValue("DeviceUniqueId", out uniqueId))
                result = (byte[]) uniqueId;
            String phoneID = BitConverter.ToString(result);
            
            Service1Client client = new Service1Client();
            client.GetUserProfileCompleted += new EventHandler<GetUserProfileCompletedEventArgs>(client_GetUserProfileCompleted);
            client.GetUserProfileAsync(userNameBox.Text, phoneID);
        }

        //for now, just accepts a correct user, and moves to main page
        void client_GetUserProfileCompleted(object sender, GetUserProfileCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                List<User> users = e.Result.ToList<User>();
                if (users.Count() == 1)
                {
                    App.currentUser = users[0];
                    App.userVerified = true;
                    tryAgainBlock.Text = "";
                    NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
                }
                else
                {
                    //tell the user to retry
                    tryAgainBlock.Text = "Username not found...";
                }
            }
        }

        //for now, just accepts a correct user, and moves to main page
        void client_RegisterUserCompleted(object sender, RegisterUserCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                App.currentUser = new User();
                App.currentUser.ID = e.Result.ID;
                App.currentUser.Name = e.Result.name;
                App.currentUser.Score = (int) e.Result.score;
                App.userVerified = true;
                tryAgainBlock.Text = "";
                NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
            }
            else
            {
                //tell the user to retry
                registerAgainBlock.Text = "Please try a different user name";
            }
        }
    }
}