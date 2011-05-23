﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;
using Delay;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Net.NetworkInformation;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using MyScience.MyScienceService;
using System.Threading;
using System.Windows.Controls.Primitives;

namespace MyScience
{
    public partial class MainPage : PhoneApplicationPage
    {
        private PopupMessageControl msg;
        private LogoutMessageControl logoutmsg;
        private bool projectloaded=false;
        private bool submissionloaded=false;
        // Constructor
        public MainPage()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(MainPage_Loaded);

            msg = new PopupMessageControl(); //TODO test this
            logoutmsg = new LogoutMessageControl();
        }

        // Handle selection changed on ListBox
        private void MainListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // If selected index is -1 (no selection) do nothing
            if (MainListBox.SelectedIndex == -1)
                return;

            // Navigate to the new page
            NavigationService.Navigate(new Uri("/DetailsPage.xaml?selectedItem=" + MainListBox.SelectedIndex, UriKind.Relative));

            // Reset selected index to -1 (no selection)
            MainListBox.SelectedIndex = -1;
        }

        private void ToBeSubmitBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // If selected index is -1 (no selection) do nothing
            if (ToBeSubmitBox.SelectedIndex == -1)
                return;

            // Navigate to the new page
            NavigationService.Navigate(new Uri("/SubmissionPage.xaml?selectedItem=" + ToBeSubmitBox.SelectedIndex, UriKind.Relative));

            // Reset selected index to -1 (no selection)
            ToBeSubmitBox.SelectedIndex = -1;
        }

        // Load data for the ViewModel Items
        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            projectloaded = false;
            submissionloaded = false;

            
            if (NetworkInterface.GetIsNetworkAvailable() && App.firstAccess)
            {
                Service1Client client = new Service1Client();
                /* Get list of projects */
                turnOnProgressBar(ProjectProgressBar);
                client.GetProjectsCompleted += new EventHandler<GetProjectsCompletedEventArgs>(client_GetProjectsCompleted);
                client.GetProjectsAsync();
                /* Get Hall of Fame user list */
                turnOnProgressBar(FameProgreeBar);
                client.GetTopScorersCompleted += new EventHandler<GetTopScorersCompletedEventArgs>(client_GetTopScorersCompleted);
                client.GetTopScorersAsync();
                /* Get User's past submissions */
                turnOnProgressBar(DataProgreeBar);
                client.GetUserSubmissionCompleted += new EventHandler<GetUserSubmissionCompletedEventArgs>(client_GetUserSubmissionCompleted);
                client.GetUserSubmissionAsync(App.currentUser.ID);
                /* Get user profile image */
                turnOnProgressBar(ProfileProgressBar);
                client.GetUserImageCompleted += new EventHandler<GetUserImageCompletedEventArgs>(client_GetUserImageCompleted);
                client.GetUserImageAsync(App.currentUser.Name, "JPEG");
                /* Load tobe submitted list */
                App.loadToBeSubmit();
                App.firstAccess = false;   
            }
            else
            {
                /* Load all */
                App.loadAppAll();
                userPic.Source = App.userProfileImage;
                projectloaded = true;
                submissionloaded = true;
                displayUserProjects();
            }
            /* Modify userprofile panorama */
            updatePageControls();
        }

        private void updatePageControls()
        {
            if (App.applist != null && App.applist.Count != 0)
            {
                MainListBox.ItemsSource = App.applist;
            }
            if (App.topscorerslist != null && App.topscorerslist.Count != 0)
            {
                HallOfFameBox.ItemsSource = App.topscorerslist;
            }
            if (App.sentSubmissions.Count != 0)
            {
                PictureWall.ItemsSource = App.sentSubmissions;
            }
            if (App.toBeSubmit.Count != 0)
            {
                ToBeSubmitBox.ItemsSource = App.toBeSubmit;
            }
            //change visibility
            this.MainListBox.Visibility = System.Windows.Visibility.Visible;
            this.HallOfFameBox.Visibility = System.Windows.Visibility.Visible;
            this.PictureWall.Visibility = System.Windows.Visibility.Visible;
            this.ToBeSubmitBox.Visibility = System.Windows.Visibility.Visible;
            /* Modify userprofile panorama */
            //userPic.Source = App.userProfileImage; this is dealt with in the client download function call
            userName.Text = App.currentUser.Name;
            score.Text = "Score: " + App.currentUser.Score.ToString();
            scientistLevel.Text = App.currentUser.Score < 50 ? "Newb" : "Aspiring Scientist";

        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }


        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        #region progressbar

        private void turnOnProgressBar(PerformanceProgressBar bar) {
            bar.IsIndeterminate=true;
            bar.Visibility = System.Windows.Visibility.Visible;
        }

        private void turnOffProgressBar(PerformanceProgressBar bar)
        {
            bar.IsIndeterminate = false;
            bar.Visibility = System.Windows.Visibility.Visible;
        }

        #endregion


        private List<int> getUserProjects(List<Submission> submissions) {
              List<int> result = new List<int>();
             
                  for (int i = 0; i < submissions.Count; i++)
                  {
                      if (!result.Contains(submissions[i].ProjectID))
                      {
                          result.Add(submissions[i].ProjectID);
                      }
                  }
            
              return result;
        }

        private void displayUserProjects()
        {
            if (projectloaded && submissionloaded)
            {
                List<Project> userprojects = new List<Project>();
                for (int i = 0; i < App.applist.Count; i++)
                {
                    if (App.userProject.Contains(App.applist[i].ID))
                        userprojects.Add(App.applist[i]);
                }
                ProjectListBox.ItemsSource = userprojects;
                ProjectListBox.Visibility = System.Windows.Visibility.Visible;
            }

        }


        #region client_calls

        void client_GetUserSubmissionCompleted(object sender, GetUserSubmissionCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                //SubmissionListBox.ItemsSource = e.Result;
                PictureWall.ItemsSource = e.Result;
                List<Submission> submissions = e.Result.ToList<Submission>();
                App.userProject = getUserProjects(submissions);
                submissionloaded = true;
                displayUserProjects();
                try
                {
                    IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication();
                    StreamWriter writeFile;
                    String txtDirectory = "MyScience/Submissions/" + App.currentUser.ID;
                    if (!myIsolatedStorage.DirectoryExists(txtDirectory))
                    {
                        myIsolatedStorage.CreateDirectory(txtDirectory);
                    }
                    foreach (Submission submn in submissions)
                    {
                        String filename = submn.LowResImageName.Substring(submn.LowResImageName.LastIndexOf('/') + 1);
                        if (myIsolatedStorage.FileExists(txtDirectory+"/" + filename + ".txt"))
                        {
                            myIsolatedStorage.DeleteFile(txtDirectory+"/"+ filename + ".txt");
                        }
                        writeFile = new StreamWriter(new IsolatedStorageFileStream(txtDirectory+"/" + filename + ".txt", FileMode.CreateNew, myIsolatedStorage));
                        writeFile.WriteLine(submn.ProjectID);
                        writeFile.WriteLine(submn.ProjectName);
                        writeFile.WriteLine(submn.Data);
                        writeFile.WriteLine(submn.Location);
                        writeFile.WriteLine(submn.Time);
                        writeFile.WriteLine(submn.ImageName);
                        writeFile.WriteLine(submn.LowResImageName);
                        writeFile.WriteLine(filename);//TODO remove this or not
                        writeFile.Close();
                    }
                }
                catch (Exception ex)
                {
                    //do something here
                }

                App.sentSubmissions = e.Result.ToList<Submission>();
                App.saveSubmissions();
            }
            turnOffProgressBar(DataProgreeBar);
        }

        void client_GetProjectsCompleted(object sender, GetProjectsCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                this.MainListBox.ItemsSource = e.Result;
                App.applist = e.Result.ToList<Project>();

                projectloaded = true;
                displayUserProjects();
                /* Write file to isolated storage */
                try 
                {
                    IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication();
                    StreamWriter writeFile;
                    String txtDirectory = "MyScience/Projects";
                    if (!myIsolatedStorage.DirectoryExists(txtDirectory))
                    {
                        myIsolatedStorage.CreateDirectory(txtDirectory);
                    }
                    foreach (Project project in App.applist)
                    {
                        String filename = project.ID.ToString();
                        if (myIsolatedStorage.FileExists(txtDirectory + "/" + filename + ".txt"))
                        {
                            myIsolatedStorage.DeleteFile(txtDirectory + "/" + filename + ".txt");
                        }

                        writeFile = new StreamWriter(new IsolatedStorageFileStream(txtDirectory + "/" + filename + ".txt", FileMode.CreateNew, myIsolatedStorage));
                        writeFile.WriteLine(project.ID);
                        writeFile.WriteLine(project.Name);
                        writeFile.WriteLine(project.Owner);
                        writeFile.WriteLine(project.Description);
                        writeFile.WriteLine(project.Form);
                        writeFile.Close();
                    }
                }
                catch (Exception ex) { }
                App.saveProjects();
            }
            turnOffProgressBar(ProjectProgressBar);
        }

        /*After fetching the list of top scorers from sql azure, bind the result with the listbox*/
        void client_GetTopScorersCompleted(object sender, GetTopScorersCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                App.topscorerslist = e.Result.ToList<TopScorer>();
                for (int i = 0; i < App.topscorerslist.Count; i++)
                {
                    App.topscorerslist[i].title = App.topscorerslist[i].Score > 50 ? "Aspiring Scientist":"Newb";
                    App.topscorerslist[i].ImageName = "http://myscience.blob.core.windows.net/userimages/"+App.topscorerslist[i].Name + ".jpg";
                }
                this.HallOfFameBox.ItemsSource = App.topscorerslist;
                App.saveTopScorers();
            }
            turnOffProgressBar(FameProgreeBar);
        }

        /* Fetch user's new profile image. Also writes the image to the siolated storage */
        void client_GetUserImageCompleted(object sender, GetUserImageCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                WriteableBitmap image = new WriteableBitmap(160, 120);
                MemoryStream ms = new MemoryStream(e.Result);
                image.LoadJpeg(ms);
                //Image userImage = DynamicPanel.Children.OfType<Image>().First() as Image;
                userPic.Source = image;
                userPic.Height = image.PixelHeight;
                userPic.Width = image.PixelWidth;
               
                IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication();
                if (!myIsolatedStorage.DirectoryExists("MyScience/Images"))
                {
                    myIsolatedStorage.CreateDirectory("MyScience/Images");
                }
                if (myIsolatedStorage.FileExists("MyScience/Images/" + App.currentUser.Name + ".jpg"))
                {
                    turnOffProgressBar(ProfileProgressBar);
                    return;
                    //myIsolatedStorage.DeleteFile("MyScience/Images/" + App.currentUser.Name + ".jpg");
                }
                IsolatedStorageFileStream fileStream = myIsolatedStorage.CreateFile("MyScience/Images/" + App.currentUser.Name + ".jpg");
                image.SaveJpeg(fileStream, image.PixelWidth, image.PixelHeight, 0, 100);
                fileStream.Close();
            }
            turnOffProgressBar(ProfileProgressBar);
        }

        #endregion

        private void userPic_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                if (e != null)
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
            }
        }

        void photoChooserTask_Completed(object sender, PhotoResult e)
        {
            if (e.TaskResult == TaskResult.OK)
            {
                App.firstAccess = true;
                WriteableBitmap image = new WriteableBitmap(160, 120);
                image.LoadJpeg(e.ChosenPhoto);
                userPic.Source = image;
                userPic.Height = image.PixelHeight;
                userPic.Width = image.PixelWidth;
                MemoryStream ms = new MemoryStream();
                image.SaveJpeg(ms, image.PixelWidth, image.PixelHeight, 0, 100);
                byte[] imageData = ms.ToArray();

                //upload new user pic
                Service1Client client = new Service1Client();
                client.UploadUserImageCompleted += new EventHandler<UploadUserImageCompletedEventArgs>(client_UploadUserImageCompleted);
                client.UploadUserImageAsync(App.currentUser.Name, "JPEG", imageData);
            }
        }

        void client_UploadUserImageCompleted(object sender, UploadUserImageCompletedEventArgs e)
        {

        }

        private void Image_Opened(object sender, RoutedEventArgs e)
        {
            var image = sender as Image;
            String filename = image.Name.Substring(image.Name.LastIndexOf('/') + 1);
            //IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication();
            //if (!myIsolatedStorage.DirectoryExists("MyScience/Images"))
            //{
            //    myIsolatedStorage.CreateDirectory("MyScience/Images");
            //}
            //if (myIsolatedStorage.FileExists("MyScience/Images/" + filename + ".jpg"))
            //{
            //    myIsolatedStorage.DeleteFile("MyScience/Images/" + filename + ".jpg");
            //}
            WriteableBitmap photo = new WriteableBitmap((BitmapImage)image.Source);
            //IsolatedStorageFileStream fileStream = myIsolatedStorage.CreateFile("MyScience/Images/" + filename + ".jpg");
            //photo.SaveJpeg(fileStream, photo.PixelWidth, photo.PixelHeight, 0, 100);
            //fileStream.Close();

            ThreadPool.QueueUserWorkItem(new WaitCallback(saveImageInCache), new TaskInfo(photo, filename));
        }

        private void Image_Failed(object sender, RoutedEventArgs e)
        {
            var image = sender as Image;
            ImageSource source = new BitmapImage(new Uri("./Images/unknownuser.jpg", UriKind.Relative));
            image.Source = source;
        }

        private void saveImageInCache(Object stateInfo)
        {
            TaskInfo info = (TaskInfo)stateInfo;
            WriteableBitmap photo = info.photo;
            String filename = info.filename;
            if (!filename.EndsWith(".jpg")) filename += ".jpg";
           
            IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication();
            if (!myIsolatedStorage.DirectoryExists("MyScience/Images"))
            {
                myIsolatedStorage.CreateDirectory("MyScience/Images");
            }
            if (myIsolatedStorage.FileExists("MyScience/Images/" + filename ))
            {
                myIsolatedStorage.DeleteFile("MyScience/Images/" + filename);
            }
            
            IsolatedStorageFileStream fileStream = myIsolatedStorage.CreateFile("MyScience/Images/" + filename );
            photo.SaveJpeg(fileStream, photo.PixelWidth, photo.PixelHeight, 0, 100);
            fileStream.Close();
        }

        #region Refresh buttons

        private void refreshproject_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                turnOnProgressBar(ProjectProgressBar);
                projectloaded = false;
                Service1Client client = new Service1Client();
                /* Get list of projects */
                client.GetProjectsCompleted += new EventHandler<GetProjectsCompletedEventArgs>(client_GetProjectsCompleted);
                client.GetProjectsAsync();
            }
            else
            {
                displayPopup();
            }
        }


        private void refreshprofile_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                turnOnProgressBar(ProfileProgressBar);
                Service1Client client = new Service1Client();
                /* Get user profile image */
                client.GetUserImageCompleted += new EventHandler<GetUserImageCompletedEventArgs>(client_GetUserImageCompleted);
                client.GetUserImageAsync(App.currentUser.Name, "JPEG");
            }
            else
            {
                displayPopup();
            }
        }

        private void refreshyourdata_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                turnOnProgressBar(DataProgreeBar);
                submissionloaded = false;
                Service1Client client = new Service1Client();
                /* Get User's past submissions */
                client.GetUserSubmissionCompleted += new EventHandler<GetUserSubmissionCompletedEventArgs>(client_GetUserSubmissionCompleted);
                client.GetUserSubmissionAsync(App.currentUser.ID);
            }
            else
            {
                displayPopup();
            }
        }

        private void refreshyourdata2_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                turnOnProgressBar(DataProgreeBar);
                Service1Client client = new Service1Client();
                /* Get User's past submissions */
                client.GetUserSubmissionCompleted += new EventHandler<GetUserSubmissionCompletedEventArgs>(client_GetUserSubmissionCompleted);
                client.GetUserSubmissionAsync(App.currentUser.ID);
            }
            else
            {
                displayPopup();
            }
        }

        private void refreshhalloffame_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                turnOnProgressBar(FameProgreeBar);
                Service1Client client = new Service1Client();
                /* Get User's past submissions */
                client.GetTopScorersCompleted += new EventHandler<GetTopScorersCompletedEventArgs>(client_GetTopScorersCompleted);
                client.GetTopScorersAsync();
            }
            else
            {
                displayPopup();
            }
        }

        #endregion

        public void displayPopup()
        {
            msg.msgcontent.Text = "We're having a connectivity problem. This maybe because your cellular data connections are turned off. Please try again later.";
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
        }

        public void displayLogoutPopup()
        {
            //logoutmsg.logoutmsgcontent.Text = "We're having a connectivity problem. This maybe because your cellular data connections are turned off. Please try again later.";
            App.popup.Child = logoutmsg;
            App.popup.Margin = new Thickness(0);
            App.popup.Height = logoutmsg.Height;
            App.popup.Width = logoutmsg.Width;
            App.popup.HorizontalAlignment = HorizontalAlignment.Center;
            App.popup.VerticalAlignment = VerticalAlignment.Center;
            App.popup.HorizontalOffset = 0;
            App.popup.VerticalOffset = 0;
            App.popup.MinHeight = logoutmsg.Height;
            App.popup.MinWidth = logoutmsg.Width;
            App.popup.IsOpen = true;
        }

        private void logoutButton_Click(object sender, RoutedEventArgs e)
        {
            displayLogoutPopup();
            //appReset();
            //NavigationService.Navigate(new Uri("/home.xaml", UriKind.Relative));
        }

        private void appReset()
        {
            App.currentUser = null;
            App.userProfileImage = null;
            App.firstAccess = true;
            App.userVerified = false;
        }
    }
}