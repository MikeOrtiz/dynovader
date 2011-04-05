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

using MyScience.MyScienceService;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Windows.Controls.Primitives;

namespace MyScience
{
    

    public partial class DetailsPage : PhoneApplicationPage
    {
        private double lat;
        private double lng;
        private double blah;

        public DetailsPage()
        {
            InitializeComponent();
            OnLoaded();
            blah = 0.22;
        }

        //void settingsButton_Click(object sender, EventArgs e)
        //{
        //    NavigationService.Navigate(new Uri("/SettingPage.xaml", UriKind.Relative));
        //}

        //void rankButton_Click(object sender, EventArgs e)
        //{
        //    NavigationService.Navigate(new Uri("/RankPage.xaml", UriKind.Relative));
        //}

        //void profileButton_Click(object sender, EventArgs e)
        //{
        //    NavigationService.Navigate(new Uri("/ProfilePage.xaml", UriKind.Relative));
        //}

        //void homeButton_Click(object sender, EventArgs e)
        //{
        //    NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
        //}

        private void DetailsPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (NavigationContext.QueryString.ContainsKey("selectedItem"))
            {
                App.currentIndex = Convert.ToInt32(NavigationContext.QueryString["selectedItem"]);
                Project currentApp = App.applist[App.currentIndex];
                PageTitle.Text = currentApp.Name;
                DynamicPanel.Children.Clear();
                var DescriptionBlock = new TextBlock();
                DescriptionBlock.Text = currentApp.Description;
                var LatBlock = new TextBlock();
                LatBlock.Text = "Lat: " + lat.ToString();
                var LngBlock = new TextBlock();
                LngBlock.Text = "Lng:" + lng.ToString();
                DynamicPanel.Children.Add(DescriptionBlock);
                DynamicPanel.Children.Add(LatBlock);
                DynamicPanel.Children.Add(LngBlock);

                List<Field> fields = GetFormField(currentApp.Form);
                /*When submission page loaded, it will generate controls dynamically*/
                //DynamicPanel.Children.Clear();
                for (int i = 0; i < fields.Count; i++)
                {
                    switch (fields[i].type)
                    {
                        case "Question":
                            var newTextBlock = new TextBlock { Name = "Question" + i.ToString(), Text = fields[i].label };
                            var newTextBox = new TextBox { Name = "Answer" + i.ToString() };
                            DynamicPanel.Children.Add(newTextBlock);
                            DynamicPanel.Children.Add(newTextBox);
                            break;
                    }

                }
                //add button and event handler here
                var newButton = new Button { Name = "SubmitButton", Content = "Submit" };
                newButton.Click += new RoutedEventHandler(newButton_Click);
                DynamicPanel.Children.Add(newButton);
            }
        }

        void newButton_Click(object sender, RoutedEventArgs e)
        {
            Project app = App.applist[App.currentIndex];
            List<Field> fields = GetFormField(app.Form);
            /*Get the values of all the fields*/
            int cur = 3;
            for (int i = 0; i < fields.Count; i++)
            {
                switch (fields[i].type)
                {
                    case "Question":
                        TextBox newTextBox = DynamicPanel.Children[cur + 1] as TextBox;
                        fields[i].value = newTextBox.Text;
                        cur += 2;
                        break;
                }

            }
            /*Parse the fields list into Json String*/
            String data = GetJsonString(fields);
            MyScienceServiceClient client = new MyScienceServiceClient();
            client.SubmitDataCompleted += new EventHandler<SubmitDataCompletedEventArgs>(client_SubmitDataCompleted);
            client.SubmitDataAsync(0, App.applist[App.currentIndex].ID, App.currentUser.ID, data, lat.ToString()+","+lng.ToString(),1);

            //client.UpdateScoreAsync(App.currentUser.ID, 1);//for now, add one point for each submission
        }

        void client_SubmitDataCompleted(object sender, SubmitDataCompletedEventArgs e)
        {
            
            Popup messagePopup = new Popup();
            TextBlock message = new TextBlock();
            message.Text = "Congratulation! Data Submitted Successfully!";
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

            //Map.Center = location;
        }


        //private void SubmitButton_Click(object sender, RoutedEventArgs e)
        //{

        //    //WebClient client = new WebClient();

        //    ////client.UploadStringAsync(new Uri("http://128.12.62.142/dynovader/json.php?action=submit&projectid=7&latitude=37.430299&longitude=-122.173349"),"GET");
        //    //client.UploadStringAsync(new Uri("http://128.12.62.142/dynovader/json.php?action=submit&projectid=" + App.applist[App.currentIndex].ID + "&latitude=37.430299&longitude=-122.173349"), "GET");

        //    //client.UploadStringCompleted += (s, ev) =>
        //    //{
        //    //    String result = ev.Result;
        //    //};

        //   NavigationService.Navigate(new Uri("/SubmissionPage.xaml", UriKind.Relative));

        //}


    }
}