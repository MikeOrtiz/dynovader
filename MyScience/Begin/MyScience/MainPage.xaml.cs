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
using Microsoft.Phone.Net.NetworkInformation;
using System.ComponentModel;

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
                if (NetworkInterface.GetIsNetworkAvailable() && App.firstAccess)
                {
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

                    App.firstAccess = false;
                }
                else
                {
                //String txtDirectory = "MyScience/Submissions/"+App.currentUser.ID+"/";
                //using (IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
                //{
                //    if (!myIsolatedStorage.DirectoryExists(txtDirectory)) return;

                //    String[] txtfiles = myIsolatedStorage.GetFileNames(txtDirectory + "*.txt");
                //    foreach (String txtfile in txtfiles)
                //    {
                //        myIsolatedStorage.DeleteFile(txtDirectory + txtfile);
                //    }
                //}
                    loadProjectPage();
                    if(App.applist != null && App.applist.Count != 0) MainListBox.ItemsSource = App.applist;
                    loadTopScorers();
                    if(App.topscorerslist != null && App.topscorerslist.Count != 0) HallOfFameBox.ItemsSource = App.topscorerslist;
                    loadUserProfilePic();
                    userName.Text = App.currentUser.Name;
                    score.Text = "Score: " + App.currentUser.Score.ToString();
                    scientistLevel.Text = App.currentUser.Score < 50 ? "Newb" : "Aspiring Scientist";
                    List<Submission> submissions = loadCachedSubmission();
                    if (submissions.Count != 0)
                    {
                        SubmissionListBox.ItemsSource = submissions;
                        PictureWall.ItemsSource = submissions;
                    }
                    loadToBeSubmitPage();
                }
            }
        }

        void client_GetUserSubmissionCompleted(object sender, GetUserSubmissionCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                SubmissionListBox.ItemsSource = e.Result;
                PictureWall.ItemsSource = e.Result;
                List<Submission> submissions = e.Result.ToList<Submission>();
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
                        writeFile.WriteLine(filename);
                        writeFile.Close();
                    }
                }
                catch (Exception ex)
                {
                    //do something here
                }
            }
            
        }

        //private void TopScorers_Loaded(object sender, RoutedEventArgs e)
        //{
        //    Service1Client client = new Service1Client();
        //    client.GetTopScorersCompleted += new EventHandler<GetTopScorersCompletedEventArgs>(client_GetTopScorersCompleted);
        //    client.GetTopScorersAsync();
        //}

        void client_GetProjectsCompleted(object sender, GetProjectsCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                this.MainListBox.ItemsSource = e.Result;
                App.applist = e.Result.ToList<Project>();
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
                        String filename =project.ID.ToString();
                        if (myIsolatedStorage.FileExists(txtDirectory+"/" +filename+ ".txt"))
                        {
                            myIsolatedStorage.DeleteFile(txtDirectory+"/" +filename + ".txt");
                        }
                        
                        writeFile = new StreamWriter(new IsolatedStorageFileStream(txtDirectory+"/"+filename  + ".txt", FileMode.CreateNew, myIsolatedStorage));
                        writeFile.WriteLine(project.ID);
                        writeFile.WriteLine(project.Name);
                        writeFile.WriteLine(project.Owner);
                        writeFile.WriteLine(project.Description);
                        writeFile.WriteLine(project.Form);
                        writeFile.Close();
                    }
                }
                catch (Exception ex)
                {
                    //do something
                }

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
                try
                {
                    IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication();
                    StreamWriter writeFile;
                    String txtDirectory = "MyScience/TopScorers";
                    if (!myIsolatedStorage.DirectoryExists(txtDirectory))
                    {
                        myIsolatedStorage.CreateDirectory(txtDirectory);
                    }
                    foreach (TopScorer ts in App.topscorerslist)
                    {
                        if (myIsolatedStorage.FileExists(txtDirectory+"/" + ts.ID + ".txt"))
                        {
                            myIsolatedStorage.DeleteFile(txtDirectory+"/" + ts.ID + ".txt");
                        }
                        writeFile = new StreamWriter(new IsolatedStorageFileStream(txtDirectory +"/"+ ts.ID + ".txt", FileMode.CreateNew, myIsolatedStorage));
                        writeFile.WriteLine(ts.ID);
                        writeFile.WriteLine(ts.Name);
                        writeFile.WriteLine(ts.Score);
                        writeFile.Close();
                    }
                }
                catch (Exception ex)
                {
                    //do somthing
                }

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
                IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication();
                if (!myIsolatedStorage.DirectoryExists("MyScience/Images"))
                {
                    myIsolatedStorage.CreateDirectory("MyScience/Images");
                }
                if (myIsolatedStorage.FileExists("MyScience/Images/" + App.currentUser.Name + ".jpg"))
                {
                    myIsolatedStorage.DeleteFile("MyScience/Images/" + App.currentUser.Name + ".jpg");
                }
                IsolatedStorageFileStream fileStream = myIsolatedStorage.CreateFile("MyScience/Images/" + App.currentUser.Name + ".jpg");
                image.SaveJpeg(fileStream, image.PixelWidth, image.PixelHeight, 0, 100);
                fileStream.Close();
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
                ToBeSubmitBox.ItemsSource = null;
                ToBeSubmitBox.ItemsSource = App.toBeSubmit;
                ToBeSubmitBox.Visibility = System.Windows.Visibility.Visible;
            }
        }

        /* Load all the submissions that haven't been uploaded yet */
        private void loadToBeSubmit()
        {
            String txtDirectory = "MyScience/ToBeSubmit/"+App.currentUser.ID;
            loadSubmission(txtDirectory, App.toBeSubmit); //TODO might be bug, since persistence across sessions does not happen
        }

        private void loadProjectPage() 
        {
            App.applist = new List<Project>();
            String txtDirectory = "MyScience/Projects/";
            using (IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                
                if (!myIsolatedStorage.DirectoryExists(txtDirectory)) return;
                //myIsolatedStorage.DeleteFile("MyScience/Projects/More Creek Watch25.txt");
                String[] txtfiles = myIsolatedStorage.GetFileNames(txtDirectory + "*.txt");
                foreach (String txtfile in txtfiles)
                {
                    var fileStream = myIsolatedStorage.OpenFile(txtDirectory + txtfile, FileMode.Open, FileAccess.Read);
                    using (StreamReader reader = new StreamReader(fileStream))
                    {
                        Project project = new Project();
                        project.ID = Convert.ToInt32(reader.ReadLine());
                        project.Name = reader.ReadLine();
                        project.Owner = Convert.ToInt32(reader.ReadLine());
                        project.Description = reader.ReadLine();
                        project.Form = reader.ReadLine();
                        App.applist.Add(project);
                    }
                }
            }
        }

        private void loadTopScorers()
        {
            App.topscorerslist = new List<TopScorer>();
            String txtDirectory = "MyScience/TopScorers/";
            using (IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!myIsolatedStorage.DirectoryExists(txtDirectory)) return;

                String[] txtfiles = myIsolatedStorage.GetFileNames(txtDirectory + "*.txt");
                foreach (String txtfile in txtfiles)
                {
                    //myIsolatedStorage.DeleteFile(txtDirectory + txtfile);
                    var fileStream = myIsolatedStorage.OpenFile(txtDirectory + txtfile, FileMode.Open, FileAccess.Read);
                    using (StreamReader reader = new StreamReader(fileStream))
                    {
                        TopScorer ts = new TopScorer();
                        ts.ID = Convert.ToInt32(reader.ReadLine());
                        ts.Name = reader.ReadLine();
                        ts.Score = Convert.ToInt32(reader.ReadLine());
                        App.topscorerslist.Add(ts);
                    }
                }
            }
        }

        private void loadUserProfilePic()
        {

            String filename = App.currentUser.Name + ".jpg";
            BitmapImage image = new BitmapImage();
            using (IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
            {

                using (IsolatedStorageFileStream fileStream = myIsolatedStorage.OpenFile("MyScience/Images/" + filename, FileMode.Open, FileAccess.Read))
                {
                    image.SetSource(fileStream);
                }
               
            }
            userPic.Source = image;
        }

        private void Image_Opened(object sender, RoutedEventArgs e)
        {
            var bw = new BackgroundWorker();
            bw.DoWork += (s, a) =>
            {
                 if (!NetworkInterface.GetIsNetworkAvailable()) return;
            Image image = sender as Image;

            Dispatch(image);
            };
            bw.RunWorkerAsync();
        }

        private void Dispatch(Image image) {
            
            Dispatcher.BeginInvoke(() => {
                String filename = image.Name.Substring(image.Name.LastIndexOf('/') + 1);
            IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication();
            if (!myIsolatedStorage.DirectoryExists("MyScience/Images"))
            {
                myIsolatedStorage.CreateDirectory("MyScience/Images");
            }
            if (myIsolatedStorage.FileExists("MyScience/Images/" + filename + ".jpg"))
            {
                myIsolatedStorage.DeleteFile("MyScience/Images/" + filename + ".jpg");
            }
            //BitmapImage photo = (BitmapImage)image.Source;
            WriteableBitmap photo = new WriteableBitmap((BitmapImage)image.Source);
            IsolatedStorageFileStream fileStream = myIsolatedStorage.CreateFile("MyScience/Images/" + filename + ".jpg");
            photo.SaveJpeg(fileStream, photo.PixelWidth, photo.PixelHeight, 0, 100);
            fileStream.Close();
            });
        }
        

        private void loadSubmission(String txtDirectory, List<Submission> sublist)
        {
            using (IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!myIsolatedStorage.DirectoryExists(txtDirectory)) return;

                txtDirectory += "/";
                String[] txtfiles = myIsolatedStorage.GetFileNames(txtDirectory + "*.txt");
                sublist.Clear();
                foreach (String txtfile in txtfiles)
                {
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
                        submn.LowResImageName = reader.ReadLine();
                        sublist.Add(submn);
                    }
                }
            }
        }

        private List<Submission> loadCachedSubmission()
        {
            String txtDirectory = "MyScience/Submissions/"+App.currentUser.ID;
            List<Submission> sublist = new List<Submission>();
            loadSubmission(txtDirectory, sublist);
            return sublist;
        }

    }

   

  
}