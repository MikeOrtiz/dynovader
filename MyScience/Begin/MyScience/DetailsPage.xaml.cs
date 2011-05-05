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
namespace MyScience
{
    public partial class DetailsPage : PhoneApplicationPage
    {
        private double lat;
        private double lng;
        private TextBlock LatBlock, LngBlock;

        public DetailsPage()
        {
            InitializeComponent();
            OnLoaded();
            LatBlock = new TextBlock();
            LngBlock = new TextBlock();
        }

        private void DetailsPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (NavigationContext.QueryString.ContainsKey("selectedItem"))
            {
                App.currentIndex = Convert.ToInt32(NavigationContext.QueryString["selectedItem"]);
                Project currentApp = App.applist[App.currentIndex];
                //PageTitle.Text = currentApp.Name;
                ProjectPivot.Title = currentApp.Name;
                InfoPanel.Children.Clear();
                var DescriptionBlock = new TextBlock();
                DescriptionBlock.Text = currentApp.Description;
                DescriptionBlock.TextWrapping = TextWrapping.Wrap;
                LatBlock.Text = "Lat: " + lat.ToString();
                LngBlock.Text = "Lng:" + lng.ToString();
                InfoPanel.Children.Add(DescriptionBlock);
                InfoPanel.Children.Add(LatBlock);
                InfoPanel.Children.Add(LngBlock);

               
                //map1.Center = mapCenter;
                //map1.ZoomLevel = zoom;
               

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

                //var PhoBlock = new TextBlock { Text = "Please take a photo:" };
                //ImageBrush photo = new ImageBrush { ImageSource = new BitmapImage(new Uri("/Images/BillGates.jpg", UriKind.Relative)), Stretch =Stretch.Fill };
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
                var newButton = new Button { Name = "SubmitButton", Content = "Submit" };
                newButton.Click += new RoutedEventHandler(newButton_Click);
                DynamicPanel.Children.Add(newButton);

                if (NetworkInterface.GetIsNetworkAvailable())
                {
                    Service1Client client = new Service1Client();
                    client.GetProjectDataCompleted += new EventHandler<GetProjectDataCompletedEventArgs>(client_GetProjectDataCompleted);
                    client.GetProjectDataAsync(App.applist[App.currentIndex].ID);
                }
                else
                {
                    newButton.IsEnabled = false;
                    TextBlock warningBlock = new TextBlock { Text = "No network, other people's submissions couldn't be fetched" };
                    InfoPanel.Children.Add(warningBlock);
                }

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
            }
        }

        void saveButton_Click(object sender, RoutedEventArgs e)
        {
            Submission newsubmission = getSubmission();
            
            if (newsubmission == null) return;
            App.toBeSubmit.Add(newsubmission);
            Image photo = DynamicPanel.Children.OfType<Image>().First() as Image;
            WriteableBitmap image = (WriteableBitmap)photo.Source;

            try
            {
                IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication();
                StreamWriter writeFile;

                if (!myIsolatedStorage.DirectoryExists("MyScience/ToBeSubmit"))
                {
                    myIsolatedStorage.CreateDirectory("MyScience/ToBeSubmit");
                }
                if (myIsolatedStorage.FileExists("MyScience/ToBeSubmit/"+newsubmission.ImageName + ".txt"))
                {
                    myIsolatedStorage.DeleteFile("MyScience/ToBeSubmit/"+newsubmission.ImageName + ".txt");
                }
                writeFile = new StreamWriter(new IsolatedStorageFileStream("MyScience/ToBeSubmit/"+ newsubmission.ImageName+".txt", FileMode.CreateNew, myIsolatedStorage));
                writeFile.WriteLine(newsubmission.ProjectID);
                writeFile.WriteLine(newsubmission.ProjectName);
                writeFile.WriteLine(newsubmission.Data);
                writeFile.WriteLine(newsubmission.Location);
                writeFile.WriteLine(newsubmission.Time);
                writeFile.WriteLine(newsubmission.ImageName);
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

                Popup messagePopup = new Popup();
                TextBlock message = new TextBlock();
                message.Text = "Image Saved!\n";
                messagePopup.Child = message;
                messagePopup.IsOpen = true;
                DynamicPanel.Children.Add(messagePopup);
            }
            catch (Exception ex)
            {
                // do something with exception
            }
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
                dataCount.Text = "Current Data in Total: " + projectData.Count;
                InfoPanel.Children.Add(dataCount);

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
                    ImageName = App.currentUser.ID.ToString() + "-" + time.ToFileTime().ToString()
                };
                return newsubmission;
            }
            else
            {
                Popup messagePopup = new Popup();
                TextBlock message = new TextBlock();
                message.Text = "Oops, forgot to submit a pic!\n";
                messagePopup.Child = message;
                messagePopup.IsOpen = true;
                DynamicPanel.Children.Add(messagePopup);
                return null;
            }
        }

        void newButton_Click(object sender, RoutedEventArgs e)
        {
            Submission newsubmission = getSubmission();
            if(newsubmission != null) {
                Image photo = DynamicPanel.Children.OfType<Image>().First() as Image;
                WriteableBitmap image = (WriteableBitmap)photo.Source;
                MemoryStream ms = new MemoryStream();
                image.SaveJpeg(ms, image.PixelWidth, image.PixelHeight, 0, 100);
                byte[] imageData = ms.ToArray();
                newsubmission.ImageData = imageData;

                //Low Res Pic submission
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
                Popup messagePopup = new Popup();
                TextBlock message = new TextBlock();
                message.Text = "Oops, forgot to submit a pic!\n";
                messagePopup.Child = message;
                messagePopup.IsOpen = true;
                DynamicPanel.Children.Add(messagePopup);
            }
            //client.UpdateScoreAsync(App.currentUser.ID, 1);//for now, add one point for each submission
        } 

        void client_SubmitDataCompleted(object sender, SubmitDataCompletedEventArgs e)
        {
            String url = e.Result.ToString();
            Popup messagePopup = new Popup();
            TextBlock message = new TextBlock();
            message.Text = "Congratulation! Data Submitted Successfully!\n" + url;
            messagePopup.Child = message;
            messagePopup.IsOpen = true;
            DynamicPanel.Children.Add(messagePopup);
            //throw new NotImplementedException();
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
            var useEmulation = false;//TODO change to false

            var observable = useEmulation ? App.CreateGeoPositionEmulator() : App.CreateObservableGeoPositionWatcher();

            observable
                .ObserveOnDispatcher()
                .Subscribe(OnPositionChanged);
        }

        private void OnPositionChanged(GeoCoordinate location)
        {
            lat = location.Latitude;
            lng = location.Longitude;
            LatBlock.Text = "Lat: " + lat.ToString();
            LngBlock.Text = "Lng:" + lng.ToString();
            
        }
    }
}