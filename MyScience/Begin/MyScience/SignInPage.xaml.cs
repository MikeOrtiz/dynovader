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
        public SignIn()
        {
            InitializeComponent();
            bool hasNetwork = NetworkInterface.GetIsNetworkAvailable();
            if (!hasNetwork)
            {
                registerButton.IsEnabled = false;
            }
        }

        private void SignInPage_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void registerButton_Click(object sender, RoutedEventArgs e)
        {
            if (alreadyClicked)
                return;
            alreadyClicked = true;
            /* Get phoneid */
            byte[] result = null;
            object uniqueId;
            if (DeviceExtendedProperties.TryGetValue("DeviceUniqueId", out uniqueId))
                result = (byte[])uniqueId;
            String phoneID = BitConverter.ToString(result);

            /*Parse the fields list into Json String*/
            WriteableBitmap image = (WriteableBitmap)userImage.Source;

            if (image != null)
            {
                MemoryStream ms = new MemoryStream();
                image.SaveJpeg(ms, image.PixelWidth, image.PixelHeight, 0, 100);
                byte[] imageData = ms.ToArray();
                Service1Client client = new Service1Client();
                client.RegisterUserWithImageCompleted += new EventHandler<RegisterUserWithImageCompletedEventArgs>(client_RegisterUserWithImageCompleted);
                client.RegisterUserWithImageAsync(0, phoneID, registerNameBox.Text, "JPEG", imageData);
            }
            else
            {
                Service1Client client = new Service1Client();
                client.RegisterUserCompleted += new EventHandler<RegisterUserCompletedEventArgs>(client_RegisterUserCompleted);
                client.RegisterUserAsync(0, phoneID, registerNameBox.Text);
            }
        }

        private void signInButton_Click(object sender, RoutedEventArgs e)
        {
            if (alreadyClicked)
                return;
            alreadyClicked = true;
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
            } else {
                String txtDirectory = "MyScience/UserProfile/";
                using (IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (!myIsolatedStorage.DirectoryExists(txtDirectory) ||
                        !myIsolatedStorage.FileExists(txtDirectory+userNameBox.Text+".txt")) {
                        tryAgainBlock.Text = "No network and no user cache found.";
                    }else {

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

        //for now, just accepts a correct user, and moves to main page
        void client_RegisterUserCompleted(object sender, RegisterUserCompletedEventArgs e)
        {
            alreadyClicked = false;
            if (e.Result != null)
            {
                App.currentUser = e.Result;
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

        //for now, just accepts a correct user, and moves to main page
        void client_RegisterUserWithImageCompleted(object sender, RegisterUserWithImageCompletedEventArgs e)
        {
            alreadyClicked = false;
            if (e.Result != null)
            {
                App.currentUser = e.Result;
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

        private void choosePhotoButton_Click(object sender, RoutedEventArgs e)
        {
            var photoChooserTask = new PhotoChooserTask();
            photoChooserTask.Completed += new EventHandler<PhotoResult>(photoChooserTask_Completed);
            try
            {
                photoChooserTask.Show();
            }
            catch (Exception ex)
            {
            }
        }

        private void takePhotoButton_Click(object sender, RoutedEventArgs e)
        {
            var cameraCaptureTask = new CameraCaptureTask();
            cameraCaptureTask.Completed += new EventHandler<PhotoResult>(cameraCaptureTask_Completed);
            try
            {
                cameraCaptureTask.Show();
            }
            catch (Exception ex)
            {
            }
        }

        void photoChooserTask_Completed(object sender, PhotoResult e)
        {
            if (e.TaskResult == TaskResult.OK)
            {
                WriteableBitmap image = new WriteableBitmap(600, 800);
                image.LoadJpeg(e.ChosenPhoto);
                userImage.Source = image;
                canvas1.Background = null;
            }
        }

        void cameraCaptureTask_Completed(object sender, PhotoResult e)
        {
            if (e.TaskResult == TaskResult.OK)
            {
                WriteableBitmap image = new WriteableBitmap(600, 800);
                image.LoadJpeg(e.ChosenPhoto);
                userImage.Source = image;
                canvas1.Background = null;
            }
        }

        private void registerNameBox_ManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            registerNameBox.Text = "";
        }

        private void userNameBox_ManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            userNameBox.Text = "";
        }

    }
}