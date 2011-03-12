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
using Microsoft.Phone.Shell;
using System.Xml;
using System.IO;

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


        void settingsButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/SettingPage.xaml", UriKind.Relative));
        }

        void rankButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/RankPage.xaml", UriKind.Relative));
        }

        void profileButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/ProfilePage.xaml", UriKind.Relative));
        }

        void homeButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
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

        // Load data for the ViewModel Items
        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!App.ViewModel.IsDataLoaded)
            {
                App.ViewModel.LoadData();
            }

            String text = "<?xml version=\"1.0\"?>"
                   + "<applist>"
                   + "<application>"
                   + "<id>1</id>"
                   + "<name>Creek Watch</name>"
                   + "<description>Creek Watch is an iPhone application that enables you to help monitor the health of your local watershed."
                   + "Whenever you pass by a waterway, spend a few seconds using the Creek Watch application to snap a picture and report how much water and trash you see."
                   + "We aggregate the data and share it with water control boards to help them track pollution and manage water resources.</description>"
                   + "<form> {\"type\":\"Question\",\"label\":\"question1\"}</form>"
                   + "</application>"
                //       + "<application>"
                //       + "<id>2</id>"
                //       + "<name>iNaturalist</name>"
                //       + "<description>Welcome!to , where you can record what you see in nature, meet other nature lovers,"
                //       + "and learn about the natural world.</description>"
                //       + "</application>"
                //       + "<application>"
                //       + "<id>3</id>"
                //       + "<name>The Sleep Cycle</name>"
                //       + "<description> The Sleep Cycle alarm clock is a bio-alarm clock that analyzes your sleep patterns and wakes you when you are in the lightest sleep phase."
                //       + "Waking up in the lightest sleep phase feels like waking without an alarm clock - it is a natural way to wake up where you feel rested and relaxed.</description>"
                //       + "</application>"
                   + "</applist>";

            XmlReader reader = XmlReader.Create(new MemoryStream(System.Text.UnicodeEncoding.Unicode.GetBytes(text)));
            App.applist = parseXML(reader);
            MainListBox.ItemsSource = App.applist;

            /*Get applist from remote server, not working now*/
            //String address = "http://128.12.62.142/dynovader/json.php?action=projectlist";
            //getAppList(address);
        }

        /*Download the applist from the website*/
        public void getAppList(String websiteURL)
        {
            WebClient phone = new WebClient();
            phone.DownloadStringAsync(new Uri(websiteURL));
            phone.DownloadStringCompleted += new DownloadStringCompletedEventHandler(phone_DownloadStringCompleted);
        }

        /*When the download finished, try to parse the xml file and create a list of applications*/
        void phone_DownloadStringCompleted(object sneder, DownloadStringCompletedEventArgs e)
        {
            lock (this)
            {
                /*Get the content of the downloaded file*/
                string result = e.Result;

                /*Parse it as xml format and create a list of applications and bind it to the listbox*/
                XmlReader reader = XmlReader.Create(new MemoryStream(System.Text.UnicodeEncoding.Unicode.GetBytes(result)));
                App.applist = parseXML(reader);
                MainListBox.ItemsSource =App.applist;
            }

        }

        /*Parsing the dowloaded xml file of applist*/
        List<Project> parseXML(XmlReader reader)
        {
            List<Project> appList = new List<Project>();
            int id = 0;
            String name = "";
            String description = "";
            String form = "";
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "id")
                {
                    reader.Read();
                    id = Convert.ToInt32(reader.Value);
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "name")
                {
                    reader.Read();
                    name = reader.Value;
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "description")
                {
                    reader.Read();
                    description = reader.Value;
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "form")
                {
                    reader.Read();
                    form = reader.Value;
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "application")
                {
                    Project app = new Project(id, name, description, form);
                    appList.Add(app);
                }

            }
            return appList;
        }
    }
}