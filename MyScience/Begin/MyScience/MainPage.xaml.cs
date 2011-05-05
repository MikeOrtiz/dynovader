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
using System.Xml;
using Delay;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using MyScience.MyScienceService;
using System.IO.IsolatedStorage;
using System.Windows.Data;
using System.Globalization;

namespace MyScience
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();
            DataContext = App.ViewModel;
            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
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

        private void ProjectListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // If selected index is -1 (no selection) do nothing
            if (ProjectListBox.SelectedIndex == -1)
                return;

            // Navigate to the new page
            NavigationService.Navigate(new Uri("/DetailsPage.xaml?selectedItem=" + ProjectListBox.SelectedIndex, UriKind.Relative));

            // Reset selected index to -1 (no selection)
            ProjectListBox.SelectedIndex = -1;
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
            if (!App.userVerified)
                NavigationService.Navigate(new Uri("/SignInPage.xaml", UriKind.Relative));
            else
            {
            
                if (!App.ViewModel.IsDataLoaded)
                {
                    App.ViewModel.LoadData();
                }

                Service1Client client = new Service1Client();
                client.GetProjectsCompleted += new EventHandler<GetProjectsCompletedEventArgs>(client_GetProjectsCompleted);
                client.GetProjectsAsync();
                userName.Text = App.currentUser.Name;
                score.Text = "Score: " + App.currentUser.Score.ToString();
                scientistLevel.Text = App.currentUser.Score < 5 ? "Newb" : "Aspiring Scientist";

                client.GetTopScorersCompleted += new EventHandler<GetTopScorersCompletedEventArgs>(client_GetTopScorersCompleted);
                client.GetTopScorersAsync();
                //this.Loaded += new RoutedEventHandler(TopScorers_Loaded);

                client.GetUserSubmissionCompleted += new EventHandler<GetUserSubmissionCompletedEventArgs>(client_GetUserSubmissionCompleted);
                client.GetUserSubmissionAsync(App.currentUser.ID);

                client.GetUserImageCompleted += new EventHandler<GetUserImageCompletedEventArgs>(client_GetUserImageCompleted);
                client.GetUserImageAsync(App.currentUser.Name, "JPEG");

                loadToBeSubmitPage();

            }
        }

        void client_GetUserSubmissionCompleted(object sender, GetUserSubmissionCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                SubmissionListBox.ItemsSource = e.Result;
                PictureWall.ItemsSource = e.Result;
            }
        }

        private void TopScorers_Loaded(object sender, RoutedEventArgs e)
        {
            Service1Client client = new Service1Client();
            client.GetTopScorersCompleted += new EventHandler<GetTopScorersCompletedEventArgs>(client_GetTopScorersCompleted);
            client.GetTopScorersAsync();
        }

        void client_GetProjectsCompleted(object sender, GetProjectsCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                this.MainListBox.ItemsSource = e.Result;
                App.applist = e.Result.ToList<Project>();
            }
            this.MainListBox.Visibility = System.Windows.Visibility.Visible;
        }

        /*After fetching the list of top scorers from sql azure, bind the result with the listbox*/
        void client_GetTopScorersCompleted(object sender, GetTopScorersCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                this.HallOfFameBox.ItemsSource = e.Result;
                App.topscorerslist = e.Result.ToList<TopScorer>();
            }
            this.HallOfFameBox.Visibility = System.Windows.Visibility.Visible;
        }

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
            }
        }

        private void userPic_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
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

        void photoChooserTask_Completed(object sender, PhotoResult e)
        {
            if (e.TaskResult == TaskResult.OK)
            {
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

        private void loadToBeSubmitPage()
        {
            loadToBeSubmit();
            //ToBeSubmitInfo.Text = App.toBeSubmit.Count.ToString() + " submissions to be uploaded";
            if (App.toBeSubmit.Count != 0)
            {
                ToBeSubmitBox.ItemsSource = App.toBeSubmit;
                ToBeSubmitBox.Visibility = System.Windows.Visibility.Visible;
            }
        }

        /* Load all the submissions that haven't been uploaded yet */
        private void loadToBeSubmit()
        {
            String txtDirectory = "MyScience/ToBeSubmit/";
            using (IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!myIsolatedStorage.DirectoryExists(txtDirectory)) return;

                String[] txtfiles = myIsolatedStorage.GetFileNames(txtDirectory+ "*.txt");
                foreach (String txtfile in txtfiles) {
                    var fileStream = myIsolatedStorage.OpenFile(txtDirectory + txtfile, FileMode.Open, FileAccess.Read);
                    using (StreamReader reader = new StreamReader(fileStream))
                    {
                        Submission submn = new Submission();
                        submn.ID = 0;
                        submn.ProjectID = Convert.ToInt32(reader.ReadLine());
                        submn.ProjectName = reader.ReadLine();
                        submn.UserID = App.currentUser.ID;
                        submn.Data = reader.ReadLine();
                        submn.Location = reader.ReadLine();
                        submn.Time = Convert.ToDateTime(reader.ReadLine());
                        submn.ImageName = reader.ReadLine();
                        App.toBeSubmit.Add(submn);
                    }
                }
            }
        }
    }

   

  
}