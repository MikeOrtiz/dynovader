using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Info;
using Microsoft.Phone.Tasks;
using MyScience.MyScienceService;
using System.IO.IsolatedStorage;
using Microsoft.Phone.Net.NetworkInformation;


namespace MyScience
{
    public partial class SignIn : PhoneApplicationPage
    {
        private bool alreadyClicked = false;
        private GeoLocationMessageControl locpermitmsg;
        private PopupMessageControl msg;
        public SignIn()
        {
            InitializeComponent();
            bool hasNetwork = NetworkInterface.GetIsNetworkAvailable();
            if (!hasNetwork)
            {
                registerButton.IsEnabled = false;
            }
            locpermitmsg = new GeoLocationMessageControl(registerUser, activatePage);
            msg = new PopupMessageControl(registerUser);
        }

        private void SignInPage_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void registerButton_Click(object sender, RoutedEventArgs e)
        {
            tryAgainBlock.Text = "";
            App.registerName = registerNameBox.Text;
            //displayLocationPermissionPopup();
            displayPopup();
        }


        private void signInButton_Click(object sender, RoutedEventArgs e)
        {
            /* process button */
            if (alreadyClicked)
                return;
            alreadyClicked = true;
            registerAgainBlock.Text = "";
            byte[] result = null;
            object uniqueId;
            if (DeviceExtendedProperties.TryGetValue("DeviceUniqueId", out uniqueId))
                result = (byte[])uniqueId;
            String phoneID = BitConverter.ToString(result);

            if (NetworkInterface.GetIsNetworkAvailable())
            {
                Service1Client client = new Service1Client();
                client.GetUserProfileCompleted += new EventHandler<GetUserProfileCompletedEventArgs>(client_GetUserProfileCompleted);
                client.GetUserProfileAsync(userNameBox.Text, phoneID);
            }
            else
            {
                String txtDirectory = "MyScience/UserProfile/";
                using (IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (!myIsolatedStorage.DirectoryExists(txtDirectory) ||
                        !myIsolatedStorage.FileExists(txtDirectory + userNameBox.Text + ".txt"))
                    {
                        tryAgainBlock.Text = "No network and no user cache found.";
                    }
                    else
                    {

                        String[] txtfiles = myIsolatedStorage.GetFileNames(txtDirectory + userNameBox.Text + ".txt");

                        var fileStream = myIsolatedStorage.OpenFile(txtDirectory + userNameBox.Text + ".txt", FileMode.Open, FileAccess.Read);
                        using (StreamReader reader = new StreamReader(fileStream))
                        {
                            User user = new User();
                            user.ID = Convert.ToInt32(reader.ReadLine());
                            user.Name = reader.ReadLine();
                            user.Score = Convert.ToInt32(reader.ReadLine());
                            App.currentUser = user;
                        }
                        alreadyClicked = false;
                        App.userVerified = true;
                        tryAgainBlock.Text = "";
                        NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
                    }
                }
            }
        }

        //for now, just accepts a correct user, and moves to main page
        void client_GetUserProfileCompleted(object sender, GetUserProfileCompletedEventArgs e)
        {
            alreadyClicked = false;
            if (e.Result != null)
            {
                List<User> users = e.Result.ToList<User>();
                if (users.Count() == 1)
                {
                    App.currentUser = users[0];
                    App.userVerified = true;
                    tryAgainBlock.Text = "";
                    try
                    {
                        IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication();
                        StreamWriter writeFile;

                        if (!myIsolatedStorage.DirectoryExists("MyScience/UserProfile"))
                        {
                            myIsolatedStorage.CreateDirectory("MyScience/UserProfile");
                        }
                        if (myIsolatedStorage.FileExists("MyScience/UserProfile/" + App.currentUser.Name + ".txt"))
                        {
                            myIsolatedStorage.DeleteFile("MyScience/UserProfile/" + App.currentUser.Name+ ".txt");
                        }
                        writeFile = new StreamWriter(new IsolatedStorageFileStream("MyScience/UserProfile/" + App.currentUser.Name + ".txt", FileMode.CreateNew, myIsolatedStorage));
                        writeFile.WriteLine(App.currentUser.ID);
                        writeFile.WriteLine(App.currentUser.Name);
                        writeFile.WriteLine(App.currentUser.Score);
                        writeFile.Close();
                    }
                    catch (Exception ex)
                    {
                        // do something with exception
                    }
                    NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
                }
                else
                {
                    //tell the user to retry
                    tryAgainBlock.Text = "Username not found...";
                }
            }
        }

        public void registerUser()
        {
            LayoutRoot.IsHitTestVisible = true; //make sure phone touchscreen is active
            /* Get phoneid */
            byte[] result = null;
            object uniqueId;
            if (DeviceExtendedProperties.TryGetValue("DeviceUniqueId", out uniqueId))
                result = (byte[])uniqueId;
            String phoneID = BitConverter.ToString(result);

            Service1Client client = new Service1Client();
            client.RegisterUserCompleted += new EventHandler<RegisterUserCompletedEventArgs>(client_RegisterUserCompleted);
            client.RegisterUserAsync(0, phoneID, registerNameBox.Text);
        }

        //for now, just accepts a correct user, and moves to main page
        void client_RegisterUserCompleted(object sender, RegisterUserCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                App.currentUser = e.Result;
                App.userVerified = true;
                tryAgainBlock.Text = "";
                NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
            }
            else
            {
                //displayPopup();
                //tell the user to retry
                registerAgainBlock.Text = "Please try a different user name";
                //todo do a popup here
            }
        }

        public void displayLocationPermissionPopup()
        {
            //logoutmsg.logoutmsgcontent.Text = "We're having a connectivity problem. This maybe because your cellular data connections are turned off. Please try again later.";
            App.popup.Child = locpermitmsg;
            App.popup.Margin = new Thickness(0);
            App.popup.Height = locpermitmsg.Height;
            App.popup.Width = locpermitmsg.Width;
            App.popup.HorizontalAlignment = HorizontalAlignment.Center;
            App.popup.VerticalAlignment = VerticalAlignment.Center;
            App.popup.HorizontalOffset = 0;
            App.popup.VerticalOffset = 0;
            App.popup.MinHeight = locpermitmsg.Height;
            App.popup.MinWidth = locpermitmsg.Width;
            App.popup.IsOpen = true;
            LayoutRoot.IsHitTestVisible = false;
        }

        public void displayPopup()
        {
            msg.msgcontent.Text = "Welcome to myScience! Thank you for contributing to scientific research around the world!";
            msg.msgtitle.Text = "myScience";
            App.popup.Child = msg;
            App.popup.Margin = new Thickness(0);
            App.popup.Height = msg.Height;
            App.popup.Width = msg.Width;
            App.popup.HorizontalAlignment = HorizontalAlignment.Center;
            App.popup.VerticalAlignment = VerticalAlignment.Center;
            App.popup.HorizontalOffset = 0;
            App.popup.VerticalOffset = 0;
            App.popup.MinHeight = msg.Height;
            App.popup.MinWidth = msg.Width;
            App.popup.IsOpen = true;
            LayoutRoot.IsHitTestVisible = false;
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            //Override back key press if a context or dialog is open
            if (App.popup.IsOpen)
            {
                e.Cancel = true;
                App.popup.IsOpen = false;
                LayoutRoot.IsHitTestVisible = true;
                //OnBackKeyPress(e);
            }
            base.OnBackKeyPress(e);
        }

        public void activatePage()
        {
            LayoutRoot.IsHitTestVisible = true;
        }
    }
}