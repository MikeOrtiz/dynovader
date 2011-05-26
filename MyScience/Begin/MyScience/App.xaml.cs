using System;
using System.Collections.Generic;
using System.Device.Location;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Reactive;
using System.Windows.Controls.Primitives;
using Microsoft.Phone.Net.NetworkInformation;
using MyScience.MyScienceService;

namespace MyScience
{
    public partial class App : Application
    {
        public static List<Project> applist = new List<Project>();
        public static List<TopScorer> topscorerslist = new List<TopScorer>();
        public static int currentIndex;
        public static GeoCoordinateWatcher geoCoordinateWatcher = new GeoCoordinateWatcher();
        public static Random random = new Random();
        public static bool userVerified = false;
        public static User currentUser = null;
        public static List<Submission> toBeSubmit = new List<Submission>();
        public static List<Submission> sentSubmissions = new List<Submission>();
        public static int currentSubmissionIndex;
        public static bool firstAccess = true; //repurposing it in general to refresh the main page, i.e. for when a new submission goes through, etc

        public static List<int> userProject = new List<int>();

        public static Popup popup = new Popup();
        public static BitmapImage userProfileImage;


        /// <summary>
        /// Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public PhoneApplicationFrame RootFrame { get; private set; }

        /// <summary>
        /// Constructor for the Application object.
        /// </summary>
        public App()
        {
            // Global handler for uncaught exceptions. 
            UnhandledException += Application_UnhandledException;

            // Show graphics profiling information while debugging.
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // Display the current frame rate counters.
                Application.Current.Host.Settings.EnableFrameRateCounter = true;

                // Show the areas of the app that are being redrawn in each frame.
                //Application.Current.Host.Settings.EnableRedrawRegions = true;

                // Enable non-production analysis visualization mode, 
                // which shows areas of a page that are being GPU accelerated with a colored overlay.
                //Application.Current.Host.Settings.EnableCacheVisualization = true;
            }

            // Standard Silverlight initialization
            InitializeComponent();

            // Phone-specific initialization
            InitializePhoneApplication();

            RootFrame.Navigating += new NavigatingCancelEventHandler(RootFrame_Navigating);
        }

        void RootFrame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            // Only care about MainPage
            if (e.Uri.ToString().Contains("home.xaml") != true)
                return;

            // Cancel current navigation and schedule the real navigation for the next tick
            // (we can't navigate immediately as that will fail; no overlapping navigations
            // are allowed)
            e.Cancel = true;
            RootFrame.Dispatcher.BeginInvoke(delegate
            {
                if (currentUser == null)
                    RootFrame.Navigate(new Uri("/SignInPage.xaml", UriKind.Relative));
                else
                    RootFrame.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
            });
        }

        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
            //if (NetworkInterface.GetIsNetworkAvailable())
            LoadPersistentState();
        }

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
            LoadPersistentState();
            LoadTransState();
        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
            SavePersistentState();
            SaveTransState();
        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
            SavePersistentState();
        }

        // Code to execute if a navigation fails
        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // A navigation has failed; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        // Code to execute on Unhandled Exceptions
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        public static IObservable<GeoCoordinate> CreateObservableGeoPositionWatcher()
        {
            var observable = Observable.FromEvent<GeoPositionChangedEventArgs<GeoCoordinate>>(
                e => geoCoordinateWatcher.PositionChanged += e,
                e => geoCoordinateWatcher.PositionChanged -= e)
                .Select(e => e.EventArgs.Position.Location);

            geoCoordinateWatcher.Start();

            return observable;
        }

        #region Phone application initialization

        // Avoid double-initialization
        private bool phoneApplicationInitialized = false;

        // Do not add any additional code to this method
        private void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized)
                return;

            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            RootFrame = new PhoneApplicationFrame();
            RootFrame.Navigated += CompleteInitializePhoneApplication;

            // Handle navigation failures
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            // Ensure we don't initialize again
            phoneApplicationInitialized = true;
        }

        // Do not add any additional code to this method
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // Set the root visual to allow the application to render
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;

            // Remove this handler since it is no longer needed
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        #endregion

        /* Persistent State */
        private const string AppCurrentUserKey = "CurrentUser";
        /* Transient State */
        private const string AppProjectsList = "ProjectsList";
        private const string AppTopScorersList = "TopScorersList";
        private const string AppCurrentIndex = "CurrentIndex";
        private const string AppUserVerified = "UserVerified";
        private const string AppToBeSubmitList = "ToBeSubmitList";
        private const string AppSentSubmissions = "SentSubmissions";
        private const string AppCurrSubmissionIndex = "CurrSubmissionIndex";
        private const string AppFirstAccess = "FirstAccess";
        private const string AppPopup = "Popup";

        #region load_from_is

        public static void loadAppAll()
        {
            /* Load the list of projects */
            loadProjectPage();
            /* Load list of top scorers */
            loadTopScorers();
            /* Load user profile image */
            loadUserProfilePic();
            /* Load list of to be submitted submissions */
            loadToBeSubmit();
            /* Load list of already sent submissions */
            loadCachedSubmission();
        }

        private void LoadPersistentState()
        {
            IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
            if (settings.Contains(AppCurrentUserKey))
            {
                currentUser = (User)settings[AppCurrentUserKey];
            }
            else
                currentUser = null;
        }

        private void LoadTransState()
        {
            IDictionary<string, object> transState = PhoneApplicationService.Current.State;
            object obj;
            transState.TryGetValue(AppProjectsList, out obj);
            applist = (List<Project>)obj;
            transState.TryGetValue(AppTopScorersList, out obj);
            topscorerslist = (List<TopScorer>)obj;
            transState.TryGetValue(AppCurrentIndex, out obj);
            currentIndex = (int)obj;
            transState.TryGetValue(AppUserVerified, out obj);
            userVerified = (bool)obj;
            transState.TryGetValue(AppToBeSubmitList, out obj);
            toBeSubmit = (List<Submission>)obj;
            transState.TryGetValue(AppSentSubmissions, out obj);
            sentSubmissions = (List<Submission>)obj;
            transState.TryGetValue(AppCurrSubmissionIndex, out obj);
            currentSubmissionIndex = (int)obj;
            transState.TryGetValue(AppFirstAccess, out obj);
            firstAccess = (bool)obj;
            //initialize other app fields, these don't need to be remembered at all
            geoCoordinateWatcher = new GeoCoordinateWatcher();
            random = new Random();
            popup = new Popup();
        }

        public static void loadProjectPage()
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

        public static void loadTopScorers()
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
                        ts.title = ts.Score > 50 ? "Aspiring Scientist" : "Newb";
                        ts.ImageName = ts.Name + ".jpg";
                        App.topscorerslist.Add(ts);
                    }
                }
            }
        }

        /* Load all the submissions that haven't been uploaded yet */
        public static void loadToBeSubmit()
        {
            String txtDirectory = "MyScience/ToBeSubmit/" + App.currentUser.ID;
            loadSubmission(txtDirectory, App.toBeSubmit); 
        }

        public static void loadCachedSubmission()
        {
            String txtDirectory = "MyScience/Submissions/" + App.currentUser.ID;
            loadSubmission(txtDirectory, sentSubmissions);
        }

        public static void loadSubmission(String txtDirectory, List<Submission> sublist)
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

        public static void loadUserProfilePic()
        {
            String filename = App.currentUser.Name + ".jpg";
            userProfileImage = new BitmapImage();
            using (IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!myIsolatedStorage.FileExists("MyScience/Images/" + filename)) return;

                using (IsolatedStorageFileStream fileStream = myIsolatedStorage.OpenFile("MyScience/Images/" + filename, FileMode.Open, FileAccess.Read))
                {
                    userProfileImage.SetSource(fileStream);
                }
            }
        }

        #endregion

        #region save_into_is

        public static void saveAppAll()
        {
            saveSubmissions();
            saveProjects();
            saveTopScorers();
            /* there's no save user image. User image is saved everytime 
             * it's changed. Similarly, user's to-be-sent submissions are
             * written to the isolated storage when user clicks save. */
        }

        private void SavePersistentState()
        {
            if (currentUser != null)
            {
                IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
                settings[AppCurrentUserKey] = currentUser;
                settings.Save();
            }
        }

        private void SaveTransState()
        {
            PhoneApplicationService.Current.State[AppProjectsList] = applist;
            PhoneApplicationService.Current.State[AppTopScorersList] = topscorerslist;
            PhoneApplicationService.Current.State[AppCurrentIndex] = currentIndex;
            PhoneApplicationService.Current.State[AppUserVerified] = userVerified;
            PhoneApplicationService.Current.State[AppToBeSubmitList] = toBeSubmit;
            PhoneApplicationService.Current.State[AppSentSubmissions] = sentSubmissions;
            PhoneApplicationService.Current.State[AppCurrSubmissionIndex] = currentSubmissionIndex;
            PhoneApplicationService.Current.State[AppFirstAccess] = firstAccess;
            //don't save random, geocoordinatewatcher, popup, bitmapimage, just create new copies
        }

        public static void saveSubmissions()
        {
            try
            {
                IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication();
                StreamWriter writeFile;
                String txtDirectory = "MyScience/Submissions/" + App.currentUser.ID;
                if (!myIsolatedStorage.DirectoryExists(txtDirectory))
                {
                    myIsolatedStorage.CreateDirectory(txtDirectory);
                }
                foreach (Submission submn in App.sentSubmissions)
                {
                    String filename = submn.LowResImageName.Substring(submn.LowResImageName.LastIndexOf('/') + 1);
                    if (myIsolatedStorage.FileExists(txtDirectory + "/" + filename + ".txt"))
                    {
                        myIsolatedStorage.DeleteFile(txtDirectory + "/" + filename + ".txt");
                    }
                    writeFile = new StreamWriter(new IsolatedStorageFileStream(txtDirectory + "/" + filename + ".txt", FileMode.CreateNew, myIsolatedStorage));
                    writeFile.WriteLine(submn.ProjectID);
                    writeFile.WriteLine(submn.ProjectName);
                    writeFile.WriteLine(submn.Data);
                    writeFile.WriteLine(submn.Location);
                    writeFile.WriteLine(submn.Time);
                    writeFile.WriteLine(submn.ImageName);
                    writeFile.WriteLine(submn.LowResImageName);
                    //writeFile.WriteLine(filename);//TODO remove this or not
                    writeFile.Close();
                }
            }
            catch (Exception ex) { }
        }


        public static void saveProjects()
        {
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
        }

        public static void saveTopScorers()
        {
            /* Write file to isolated storage */
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
                    if (myIsolatedStorage.FileExists(txtDirectory + "/" + ts.ID + ".txt"))
                    {
                        myIsolatedStorage.DeleteFile(txtDirectory + "/" + ts.ID + ".txt");
                    }
                    writeFile = new StreamWriter(new IsolatedStorageFileStream(txtDirectory + "/" + ts.ID + ".txt", FileMode.CreateNew, myIsolatedStorage));
                    writeFile.WriteLine(ts.ID);
                    writeFile.WriteLine(ts.Name);
                    writeFile.WriteLine(ts.Score);
                    writeFile.Close();
                }
            }
            catch (Exception ex) { }
        }

        #endregion


        public void appReset()
        {
            App.currentUser = null;
            App.userProfileImage = null;
            App.firstAccess = true;
            App.userVerified = false;
            //go back to sign in page
            RootFrame.Navigate(new Uri("/SignInPage.xaml", UriKind.Relative));   
        }
    }
}