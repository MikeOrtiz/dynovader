﻿using System;
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
using System.Device.Location;
using Microsoft.Phone.Reactive;
using Microsoft.Phone.Controls.Maps;

using MyScience.MyScienceService;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Windows.Controls.Primitives;
using Microsoft.Phone.Tasks;
using System.Windows.Media.Imaging;
using System.IO.IsolatedStorage;
using Microsoft.Phone.Net.NetworkInformation;
using System.Threading;
namespace MyScience
{
    public partial class DetailsPage : PhoneApplicationPage
    {
        private double lat;
        private double lng;
        private TextBlock LatBlock, LngBlock;
        private TextBlock submissionStatMsg;
        private PerformanceProgressBar progressbar;
        private PopupMessageControl msg;
        private GeoCoordinateWatcher geoCoordinateWatcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High);

        public DetailsPage()
        {
            InitializeComponent();
            OnLoaded();
            LatBlock = new TextBlock();
            LngBlock = new TextBlock();
            //submissionStatMsg = new Popup();
            submissionStatMsg = new TextBlock();
            submissionStatMsg.Visibility = System.Windows.Visibility.Collapsed;
            progressbar = new PerformanceProgressBar();
            progressbar.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0x6C, 0x16));

            //popup message content
            msg = new PopupMessageControl(activatePage);
        }

        private void DetailsPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (NavigationContext.QueryString.ContainsKey("selectedItem"))
            {
                App.currentIndex = Convert.ToInt32(NavigationContext.QueryString["selectedItem"]);
                Project currentApp = App.applist[App.currentIndex];
                //PageTitle.Text = currentApp.Name;
                ProjectPivot.Title = currentApp.Name;
                BackgroundContent.Text = currentApp.Description;
                int numlines = currentApp.Description.Length / 35 + 1; //TODO fix this, estimated number of lines really sketchy stuff going on 
                BackgroundContent.MaxHeight = numlines * 36; //estimated line height based on pixels
                BackgroundContent.Height = numlines * 36;

                List<Field> fields = GetFormField(currentApp.Form);
                /*When submission page l oaded, it will generate controls dynamically*/
                DynamicPanel.Children.Clear();
                for (int i = 0; i < fields.Count; i++)
                {
                    switch (fields[i].type)
                    {
                        case "Question":
                            //TODO:add a numerical checker for number answers
                            var newTextBlock = new TextBlock { Name = "Question" + i.ToString(), Text = fields[i].label };
                            var newTextBox = new TextBox { Name = "Answer" + i.ToString() };
                            DynamicPanel.Children.Add(newTextBlock);
                            DynamicPanel.Children.Add(newTextBox);
                            break;
                        //case "Photo":
                            //TODO: A button with a camera icon on top of it, when user finishes photo taking,
                            //      substitute the camera icon with the photo
                            //var cameraButton = new Button { Name = "CameraButton", Content = "Take Photo", Width = DynamicPanel.Width / 4 };
                            //cameraButton.Click += new RoutedEventHandler(cameraButton_Click);
                            //var photo = new Image { Name = "Picture" };
                            //DynamicPanel.Children.Add(cameraButton);
                            //DynamicPanel.Children.Add(photo);
                           // break;
                        case "RadioButton":
                            //TODO: type is RadioButton, label is question, value is options
                            //      In value, different options are seperated by "|"
                            var RBTextBlock = new TextBlock { Name = "Question" + i.ToString(), Text = fields[i].label };
                            DynamicPanel.Children.Add(RBTextBlock);
                            string[] Options = fields[i].value.Split('|');
                            RadioButton[] RadioButtons = new RadioButton[Options.Length];
                            for (int j = 0; j < Options.Length; j++)
                            {
                                RadioButtons[j] = new RadioButton { Content = Options[j] };
                                DynamicPanel.Children.Add(RadioButtons[j]);
                            }
                            break;
                        case "CheckBox":
                            //TODO: same as RadioButton
                             var CBTextBlock = new TextBlock { Name = "Question" + i.ToString(), Text = fields[i].label };
                            DynamicPanel.Children.Add(CBTextBlock);
                            string[] Choices = fields[i].value.Split('|');
                            CheckBox[] CheckBoxes = new CheckBox[Choices.Length];
                            for (int j = 0; j < Choices.Length; j++)
                            {
                                CheckBoxes[j] = new CheckBox { Content = Choices[j] };
                                DynamicPanel.Children.Add(CheckBoxes[j]);
                            }
                            break;
                        case "SliderBar":
                            //TODO: same as RadioButton except value is the max and min values
                            var SBTextBlock = new TextBlock { Name = "Question" + i.ToString(), Text = fields[i].label };
                            String[] Values = fields[i].value.Split('|');
                            var SliderBar = new Slider { Minimum = double.Parse(Values[0]), Maximum = double.Parse(Values[1]) };
                            DynamicPanel.Children.Add(SBTextBlock);
                            DynamicPanel.Children.Add(SliderBar);
                            break;
                    }
                }

                var cameraButton = new Button { Name = "CameraButton", Content = "Take a photo" };
                cameraButton.Click += new RoutedEventHandler(cameraButton_Click);
                var photo = new Image { Name = "Picture", Height = 80, Width = 80 };
                DynamicPanel.Children.Add(cameraButton);


                var albumButton = new Button { Name = "AlbumButton", Content = "Choose Photo", Width = DynamicPanel.Width };
                albumButton.Click += new RoutedEventHandler(albumButton_Click);
                DynamicPanel.Children.Add(albumButton);
                DynamicPanel.Children.Add(photo);

                var saveButton = new Button { Name = "SaveButton", Content = "Save Only" };
                saveButton.Click += new RoutedEventHandler(saveButton_Click);
                DynamicPanel.Children.Add(saveButton);

                //add button and event handler here
                var submitButton = new Button { Name = "SubmitButton", Content = "Submit" };
                submitButton.Click += new RoutedEventHandler(newButton_Click);
                DynamicPanel.Children.Add(submitButton);
                if (!NetworkInterface.GetIsNetworkAvailable())
                    submitButton.IsEnabled = false;
                //Last to be added to Dynamic Panel
                DynamicPanel.Children.Add(submissionStatMsg);
                progressbar.IsIndeterminate = false;
                //progressbar.Visibility = System.Windows.Visibility.Visible;
                DynamicPanel.Children.Add(progressbar);

                GeoCoordinate mapCenter;
                int zoom = 15;
                if (lat == 0 && lng == 0)
                {
                    mapCenter = new GeoCoordinate(37.434999, -122.182989);
                }
                else
                {
                    mapCenter = new GeoCoordinate(lat, lng);
                }
                map1.SetView(mapCenter, zoom);

                if (NetworkInterface.GetIsNetworkAvailable())
                {
                    Service1Client client = new Service1Client();
                    client.GetProjectDataCompleted += new EventHandler<GetProjectDataCompletedEventArgs>(client_GetProjectDataCompleted);
                    //client.GetProjectDataAsync(App.applist[App.currentIndex].ID);
                    ThreadPool.QueueUserWorkItem(new WaitCallback(GetProjectDatainBackground), client);
                }
                else
                {
                    submitButton.IsEnabled = false;
                    //TextBlock warningBlock = new TextBlock { Text = "No network, other people's submissions couldn't be fetched" };
                    //InfoPanel.Children.Add(warningBlock);
                }
            }
        }

        private void GetProjectDatainBackground(Object state)
        {
            Service1Client client = (Service1Client)state;
            client.GetProjectDataAsync(App.applist[App.currentIndex].ID);
        }

        void saveButton_Click(object sender, RoutedEventArgs e)
        {
            var saveButton = DynamicPanel.Children.OfType<Button>().First() as Button;
            var submitButton = DynamicPanel.Children.OfType<Button>().ElementAt(1) as Button;
            saveButton.IsEnabled = false;
            submitButton.IsEnabled = false;
            Submission newsubmission = getSubmission();

            if (newsubmission == null)
            {
                saveButton.IsEnabled = true;
                submitButton.IsEnabled = true;
                return;
            }
            //App.toBeSubmit.Add(newsubmission);
            Image photo = DynamicPanel.Children.OfType<Image>().First() as Image;
            WriteableBitmap image = (WriteableBitmap)photo.Source;
            String txtDirectory = "MyScience/ToBeSubmit/" + App.currentUser.ID;
            try
            {
                IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication();
                StreamWriter writeFile;

                if (!myIsolatedStorage.DirectoryExists(txtDirectory))
                {
                    myIsolatedStorage.CreateDirectory(txtDirectory);
                }
                txtDirectory += "/";
                if (myIsolatedStorage.FileExists(txtDirectory+newsubmission.ImageName + ".txt"))
                {
                    myIsolatedStorage.DeleteFile(txtDirectory+newsubmission.ImageName + ".txt");
                }
                writeFile = new StreamWriter(new IsolatedStorageFileStream(txtDirectory+ newsubmission.ImageName+".txt", FileMode.CreateNew, myIsolatedStorage));
                writeFile.WriteLine(newsubmission.ProjectID);
                writeFile.WriteLine(newsubmission.ProjectName);
                writeFile.WriteLine(newsubmission.Data);
                writeFile.WriteLine(newsubmission.Location);
                writeFile.WriteLine(newsubmission.Time);
                writeFile.WriteLine(newsubmission.ImageName);
                writeFile.WriteLine(newsubmission.LowResImageName);
                writeFile.Close();

                if(!myIsolatedStorage.DirectoryExists("MyScience/Images"))
                { 
                    myIsolatedStorage.CreateDirectory("MyScience/Images");
                }
                if (myIsolatedStorage.FileExists("MyScience/Images/" + newsubmission.ImageName + ".jpg"))
                {
                    myIsolatedStorage.DeleteFile("MyScience/Images/" + newsubmission.ImageName + ".jpg");
                }

                IsolatedStorageFileStream fileStream = myIsolatedStorage.CreateFile("MyScience/Images/" + newsubmission.ImageName + ".jpg");
                image.SaveJpeg(fileStream, image.PixelWidth, image.PixelHeight, 0, 100);
                fileStream.Close();
                displayPopup(popupTitle1, popupContent2);
                submissionStatMsg.Text = "Submission Saved!\n";
                //saveButton.IsEnabled = true;
                //submitButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                // do something with exception
            }
            saveButton.IsEnabled = true;
            submitButton.IsEnabled = true;
        }

        void cameraButton_Click(object sender, RoutedEventArgs e)
        {
            var cameraTask = new CameraCaptureTask();
            cameraTask.Completed += new EventHandler<PhotoResult>(cameraTask_Completed);
            try
            {
                cameraTask.Show();
            }
            catch (Exception ex)
            {
            }
        }

        void albumButton_Click(object sender, RoutedEventArgs e)
        {
            var albumTask = new PhotoChooserTask();
            albumTask.Completed += new EventHandler<PhotoResult>(photoChooserTask_Completed);
            try
            {
                albumTask.Show();
            }
            catch (Exception ex)
            {
            }
        }

        void cameraTask_Completed(object sender, PhotoResult e)
        {
            if (e.TaskResult == TaskResult.OK)
            {
                //WriteableBitmap image = new WriteableBitmap(1920, 2560);
                WriteableBitmap image = new WriteableBitmap(2560, 1920);
                image.LoadJpeg(e.ChosenPhoto);
                Image photo = DynamicPanel.Children.OfType<Image>().First() as Image;
                photo.Source = image;
                photo.Height = 210;
                photo.Width = 280;
            }
        }

        void photoChooserTask_Completed(object sender, PhotoResult e)
        {
            if (e.TaskResult == TaskResult.OK)
            {
                WriteableBitmap image = new WriteableBitmap(2560, 1920);
                //image.SetSource(e.ChosenPhoto);
                image.LoadJpeg(e.ChosenPhoto);

                Image photo = DynamicPanel.Children.OfType<Image>().First() as Image;
                photo.Source = image;
                photo.Height = 210;
                photo.Width = 280;
            }
        }

        void client_GetProjectDataCompleted(object sender, GetProjectDataCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                TextBlock dataCount = new TextBlock();
                List<Submission> projectData = e.Result.ToList<Submission>();

                List<GeoCoordinate> datapoints = new List<GeoCoordinate>();
                for (int i = 0; i < projectData.Count; i++)
                {
                    String[] location = projectData[i].Location.Split(',');
                    if (location[0] != "0" || location[1] != "0")
                    {
                        Pushpin pin = new Pushpin();
                        GeoCoordinate latlng = new GeoCoordinate(Convert.ToDouble(location[0]), Convert.ToDouble(location[1]));
                        pin.Location = latlng;
                        datapoints.Add(latlng);
                        Image photo = new Image { Height = 50, Width = 50 };
                        System.Windows.Data.Binding tmpBinding = new System.Windows.Data.Binding();
                        tmpBinding.Source = new Uri(projectData[i].ImageName);
                        photo.SetBinding(Image.SourceProperty, tmpBinding);
                        //photo.Source = image;
                        pin.Content =photo;
                        map1.Children.Add(pin);
                    }
                }
                map1.SetView(LocationRect.CreateLocationRect(datapoints));
            } 
        }

        Submission getSubmission()
        {
            Project app = App.applist[App.currentIndex];
            List<Field> fields = GetFormField(app.Form);
            /*Get the values of all the fields*/
            int cur = 0;
            for (int i = 0; i < fields.Count; i++)
            {
                switch (fields[i].type)
                {
                    case "Question":
                        TextBox newTextBox = DynamicPanel.Children[cur + 1] as TextBox;
                        fields[i].value = newTextBox.Text;
                        cur += 2;
                        break;
                    case "RadioButton":
                        String[] Options = fields[i].value.Split('|');
                        cur = cur + 1;
                        for (int j = 0; j < Options.Length; j++)
                        {
                            RadioButton RButton = DynamicPanel.Children[cur] as RadioButton;
                            if (RButton.IsChecked == true)
                            {
                                fields[i].value = RButton.Content.ToString();
                            }
                            cur++;
                        }
                        break;
                    case "CheckBox":
                        String[] Choices = fields[i].value.Split('|');
                        fields[i].value = "";
                        cur = cur + 1;
                        for (int j = 0; j < Choices.Length; j++)
                        {
                            CheckBox CBox = DynamicPanel.Children[cur] as CheckBox;
                            if (CBox.IsChecked == true)
                            {
                                if (fields[i].value == "")
                                {
                                    fields[i].value = CBox.Content.ToString();
                                }
                                else
                                {
                                    fields[i].value = fields[i].value + "|" + CBox.Content.ToString();
                                }
                            }
                            cur++;
                        }
                        break;
                    case "SliderBar":
                        cur = cur + 1;
                        Slider SliderBar = DynamicPanel.Children[cur] as Slider;
                        fields[i].value = SliderBar.Value.ToString();
                        break;
                }

            }
            Image photo = DynamicPanel.Children.OfType<Image>().First() as Image;
            WriteableBitmap image = (WriteableBitmap)photo.Source;

            if (!App.locationServicesOn)
            {
                //TextBlock message = new TextBlock();
                displayPopup(popupTitle1, popupContent5);
                return null;
            }
            if (image != null)
            {

                /*Parse the fields list into Json String*/
                String data = GetJsonString(fields);
                DateTime time = DateTime.Now;
                Submission newsubmission = new Submission
                {
                    ID = 0,
                    ProjectID = App.applist[App.currentIndex].ID,
                    ProjectName = App.applist[App.currentIndex].Name,
                    UserID = App.currentUser.ID,
                    Data = data,
                    Location = lat.ToString() + "," + lng.ToString(),
                    Time = time,
                    ImageName = App.currentUser.ID.ToString() + "-" + time.ToFileTime().ToString(),
                    LowResImageName = App.currentUser.ID.ToString() + "-" + time.ToFileTime().ToString()
                };
                return newsubmission;
            }
            else
            {
                TextBlock message = new TextBlock();
                displayPopup(popupTitle1, popupContent3);
                return null;
            }
        }

        void newButton_Click(object sender, RoutedEventArgs e)
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                displayPopup(popupTitle2, popupContent1);
                return;
            }
            submissionStatMsg.Text = "Submitting...";
            progressbar.IsIndeterminate = true;
            progressbar.Visibility = System.Windows.Visibility.Visible;

            var takePhotoButton = DynamicPanel.Children.OfType<Button>().First() as Button;
            var choosePhotoButton = DynamicPanel.Children.OfType<Button>().ElementAt(1) as Button;
            var saveButton = DynamicPanel.Children.OfType<Button>().ElementAt(2) as Button;
            var submitButton = DynamicPanel.Children.OfType<Button>().ElementAt(3) as Button;

            takePhotoButton.IsEnabled = false;
            choosePhotoButton.IsEnabled = false;
            saveButton.IsEnabled = false;
            submitButton.IsEnabled = false;
            Submission newsubmission = getSubmission();
            if(newsubmission != null) {
                Image photo = DynamicPanel.Children.OfType<Image>().First() as Image;
                WriteableBitmap image = (WriteableBitmap)photo.Source;
                MemoryStream ms = new MemoryStream();
                image.SaveJpeg(ms, image.PixelWidth, image.PixelHeight, 0, 100);
                byte[] imageData = ms.ToArray();
                newsubmission.ImageData = imageData;

                //Low Res Pic submission   comment for now
                MemoryStream lowresms = new MemoryStream();
                image.SaveJpeg(lowresms, 80, 60, 0, 80);
                byte[] lowResImageData = lowresms.ToArray();
                newsubmission.LowResImageData = lowResImageData;

                Service1Client client = new Service1Client();
                client.SubmitDataCompleted += new EventHandler<SubmitDataCompletedEventArgs>(client_SubmitDataCompleted);
                client.SubmitDataAsync(newsubmission);
            }
            else
            {
                takePhotoButton.IsEnabled = true;
                choosePhotoButton.IsEnabled = true;
                saveButton.IsEnabled = true;
                submitButton.IsEnabled = true;
                TextBlock message = new TextBlock();
                //displayPopup(popupTitle1, popupContent3);
                submissionStatMsg.Text = "Oops, forgot to submit a pic!\n";
                progressbar.IsIndeterminate = false;
                progressbar.Visibility = System.Windows.Visibility.Visible;
            }
        } 

        void client_SubmitDataCompleted(object sender, SubmitDataCompletedEventArgs e)
        {
            var takePhotoButton = DynamicPanel.Children.OfType<Button>().First() as Button;
            var choosePhotoButton = DynamicPanel.Children.OfType<Button>().ElementAt(1) as Button;
            var saveButton = DynamicPanel.Children.OfType<Button>().ElementAt(2) as Button;
            var submitButton = DynamicPanel.Children.OfType<Button>().ElementAt(3) as Button;
            takePhotoButton.IsEnabled = true;
            choosePhotoButton.IsEnabled = true;
            saveButton.IsEnabled = true;
            submitButton.IsEnabled = true;
            String url = e.Result.ToString();
            progressbar.IsIndeterminate = false;
            progressbar.Visibility = System.Windows.Visibility.Collapsed;
            displayPopup(popupTitle1, popupContent4);
        }

        /*Parse the fields list into Json String*/
        private String GetJsonString(List<Field> fields)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(List<Field>));
            MemoryStream ms = new MemoryStream();
            for (int i = 0; i < fields.Count; i++)
            {
                serializer.WriteObject(ms, fields[i]);
            }
            byte[] stream = ms.ToArray();
            ms.Close();
            return Encoding.UTF8.GetString(stream, 0, stream.Length);
        }

        /*parsing Json to get fields required*/
        private List<Field> GetFormField(String form)
        {
            //List<Field> fields = new List<Field>();
            byte[] byteArray = Encoding.Unicode.GetBytes(form);
            MemoryStream stream = new MemoryStream(byteArray);
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(List<Field>));
            stream.Position = 0;
            //while (stream.Position < stream.Length)
            //{
            //    fields.Add((Field)(ser.ReadObject(stream)));
            //}
            var fields = ser.ReadObject(stream);
            stream.Close();
            return (List<Field>)fields;
        }

        private void OnLoaded(/*object sender, RoutedEventArgs e*/)
        {
            WatcherStart();
        }

        #region GeoCoordinateWatcher code

        private void WatcherStart()
        {
            geoCoordinateWatcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High);
            geoCoordinateWatcher.MovementThreshold = 20;

            // Add event handlers for StatusChanged and PositionChanged events
            geoCoordinateWatcher.StatusChanged += new EventHandler<GeoPositionStatusChangedEventArgs>(watcher_StatusChanged);
            geoCoordinateWatcher.PositionChanged += new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(watcher_PositionChanged);

            // Start data acquisition
            geoCoordinateWatcher.Start();
        }

        /// <summary>
        /// Handler for the StatusChanged event. This invokes MyStatusChanged on the UI thread and
        /// passes the GeoPositionStatusChangedEventArgs
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void watcher_StatusChanged(object sender, GeoPositionStatusChangedEventArgs e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() => MyStatusChanged(e));
        }
        /// <summary>
        /// Custom method called from the StatusChanged event handler
        /// </summary>
        /// <param name="e"></param>
        void MyStatusChanged(GeoPositionStatusChangedEventArgs e)
        {
            var saveButton = DynamicPanel.Children.OfType<Button>().ElementAt(2) as Button;
            var submitButton = DynamicPanel.Children.OfType<Button>().ElementAt(3) as Button;
            switch (e.Status)
            {
                case GeoPositionStatus.Disabled:
                    // The location service is disabled or unsupported.
                    // Alert the user
                    saveButton.IsEnabled = false;
                    submitButton.IsEnabled = false;
                    displayPopup(popupTitle1, popupContent5);
                    break;
                case GeoPositionStatus.Initializing:
                    saveButton.IsEnabled = false;
                    submitButton.IsEnabled = false;
                    break;
                case GeoPositionStatus.NoData:
                    saveButton.IsEnabled = false;
                    submitButton.IsEnabled = false;
                    //displayPopup(popupTitle1, popupContent6);
                    break;
                case GeoPositionStatus.Ready:
                    saveButton.IsEnabled = true;
                    submitButton.IsEnabled = true;
                    break;
            }
        }

        /// <summary>
        /// Handler for the PositionChanged event. This invokes MyStatusChanged on the UI thread and
        /// passes the GeoPositionStatusChangedEventArgs
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void watcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() => MyPositionChanged(e));
        }

        /// <summary>
        /// Custom method called from the PositionChanged event handler
        /// </summary>
        /// <param name="e"></param>
        void MyPositionChanged(GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            // Update the TextBlocks to show the current location
            lat = e.Position.Location.Latitude;
            lng = e.Position.Location.Longitude;
            LatBlock.Text = lat.ToString("0.000");
            LngBlock.Text = lng.ToString("0.000");
        }

        #endregion

        private static string popupTitle1 = "myscience";
        private static string popupTitle2 = "myscience error";
        private static string popupContent1 = "We're having a connectivity problem. This maybe because your cellular data connections are turned off. Please try again later.";
        private static string popupContent2 = "Submission Saved!";
        private static string popupContent3 = "Oops, forgot to submit a pic!";
        private static string popupContent4 = "Congratulation! Data Submitted Successfully!";
        private static string popupContent5 = "Location services maybe turned off. Please turn on location services in order to provide location info with your data submission. myScience will anonymize location data and will make it accessible for academic research purposes.";
        private static string popupContent6 = "Unable to accurately triangulate location. Please wait and try again later.";

        public void displayPopup(string title, string content)
        {
            msg.msgtitle.Text = title;
            msg.msgcontent.Text = content;
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