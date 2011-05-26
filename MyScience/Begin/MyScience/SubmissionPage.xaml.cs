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

using MyScience.MyScienceService;
using System.Windows.Media.Imaging;
using System.IO.IsolatedStorage;
using System.IO;
using System.Text;
using System.Runtime.Serialization.Json;
using System.Windows.Controls.Primitives;
using Microsoft.Phone.Net.NetworkInformation;
using System.Windows.Data;


namespace MyScience
{
    public partial class SubmissionPage : PhoneApplicationPage
    {
        private PopupMessageControl msg;
        private Popup messagePopup;
        private PerformanceProgressBar progressbar;

        public SubmissionPage()
        {
            InitializeComponent();
            messagePopup = new Popup();
            messagePopup.IsOpen = false;
            msg = new PopupMessageControl();
            progressbar = new PerformanceProgressBar();
            progressbar.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0x6C, 0x16));
            App.popup.Child = msg;
            App.popup.Margin = new Thickness(0);
        }

        private void SubmissionPage_Loaded(object sender, RoutedEventArgs e)
        {
           
            if (NavigationContext.QueryString.Contains(new KeyValuePair<string,string>("type","tobesubmit")))
            {
                App.currentSubmissionIndex = Convert.ToInt32(NavigationContext.QueryString["selectedItem"]);
                //if (App.currentSubmissionIndex >= App.toBeSubmit.Count()); TODO problem
                Submission currentSub = App.toBeSubmit[App.currentSubmissionIndex];
                LoadSavedImage(currentSub);
                LoadContent(currentSub);
                LoadButtons();

            }
            else if (NavigationContext.QueryString.Contains(new KeyValuePair<string, string>("type", "submission")))
            {
                SubmissionProgressBar.IsIndeterminate = true;
                String lowResImageName = NavigationContext.QueryString["selectedItem"];
                Submission currentSub = new Submission();
                for (int i = 0; i < App.sentSubmissions.Count; i++)
                {
                    if (App.sentSubmissions[i].LowResImageName == lowResImageName)
                    {
                        LoadSubmittedImage(App.sentSubmissions[i]);
                       LoadContent(App.sentSubmissions[i]);
                        return;
                    }
                }
               
            }


        }

        private void LoadButtons()
        {
            var uploadButton = new Button { Name = "UploadButton", Content = "Submit Now" };
            uploadButton.Click += new RoutedEventHandler(uploadButton_Click);
            DynamicPanel.Children.Add(uploadButton);
            if (!NetworkInterface.GetIsNetworkAvailable())
                uploadButton.IsEnabled = false;

            //Add status message last:
            DynamicPanel.Children.Add(messagePopup);
            DynamicPanel.Children.Add(progressbar);
            progressbar.IsIndeterminate = false;
            progressbar.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void LoadContent(Submission currentSub)
        {
            PageTitle.Text = currentSub.ProjectName;
            

            TimeBlock.Text = "   " + currentSub.Time.ToString();
            LocationBlock.Text = "  " + currentSub.Location;

            List<Field> fields = GetFormField(currentSub.Data);

            for (int i = 0; i < fields.Count; i++)
            {
                switch (fields[i].type)
                {
                    case "Question":
                        //TODO:add a numerical checker for number answers
                        var QBlock = new TextBlock { Name = "Question" + i.ToString(), Text = fields[i].label, FontSize=30 };
                        QBlock.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0x6C, 0x16));
                        var ABlock = new TextBlock { Name = "Answer" + i.ToString(), Text = "    " + fields[i].value, FontSize = 24 };
                        DynamicPanel.Children.Add(QBlock);
                        DynamicPanel.Children.Add(ABlock);
                        break;
                    case "RadioButton":
                        //TODO: type is RadioButton, label is question, value is options
                        //      In value, different options are seperated by "|"
                        var RBTextBlock = new TextBlock { Name = "Question" + i.ToString(), Text = fields[i].label, FontSize=30 };
                        RBTextBlock.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0x6C, 0x16));
                        DynamicPanel.Children.Add(RBTextBlock);
                        string[] Options = fields[i].value.Split('|');
                        for (int j = 0; j < Options.Length; j++)
                        {
                            var RBABlock = new TextBlock { Text = "  " + Options[j], FontSize = 24};
                            DynamicPanel.Children.Add(RBABlock);
                        }
                        break;
                    case "CheckBox":
                        //TODO: same as RadioButton
                        var CBTextBlock = new TextBlock { Name = "Question" + i.ToString(), Text = fields[i].label, FontSize = 30 };
                        CBTextBlock.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0x6C, 0x16));
                        DynamicPanel.Children.Add(CBTextBlock);
                        string[] Choices = fields[i].value.Split('|');
                        for (int j = 0; j < Choices.Length; j++)
                        {
                            var CBABlock = new TextBlock { Text = "  " + Choices[j], FontSize = 24 };
                            DynamicPanel.Children.Add(CBABlock);
                        }
                        break;
                    case "SliderBar":
                        //TODO: same as RadioButton except value is the max and min values
                        var SBTextBlock = new TextBlock { Name = "Question" + i.ToString(), Text = fields[i].label +  fields[i].value, FontSize = 30};
                        SBTextBlock.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0x6C, 0x16));
                        DynamicPanel.Children.Add(SBTextBlock);
                        break;
                }

            }

        }

        private void LoadSavedImage(Submission currentSub)
        {
            String filename = currentSub.ImageName + ".jpg";
            BitmapImage image = new BitmapImage();
            using (IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (IsolatedStorageFileStream fileStream = myIsolatedStorage.OpenFile("MyScience/Images/" + filename, FileMode.Open, FileAccess.Read))
                {
                    image.SetSource(fileStream);
                }
            }
            Photo.Source = image;
        }

        private void LoadSubmittedImage(Submission currentSub)
        {
            String filename = currentSub.ImageName;
            Binding tmpBinding = new Binding();
            tmpBinding.Source = new Uri(filename);
            Photo.SetBinding(Image.SourceProperty, tmpBinding);
            
        }


        void uploadButton_Click(object sender, RoutedEventArgs e)
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                displayPopup(popupTitle2, popupContent1);
                return;
            }
            var uploadButton = DynamicPanel.Children.OfType<Button>().First() as Button;
            uploadButton.IsEnabled = false;
            progressbar.Visibility = System.Windows.Visibility.Visible;
            progressbar.IsIndeterminate = true;
            String filename = App.toBeSubmit[App.currentSubmissionIndex].ImageName + ".jpg";
            WriteableBitmap image = new WriteableBitmap(2560, 1920);
            using (IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (IsolatedStorageFileStream fileStream = myIsolatedStorage.OpenFile("MyScience/Images/" + filename, FileMode.Open, FileAccess.Read))
                {
                    image.SetSource(fileStream);
                }
            }
            MemoryStream ms = new MemoryStream();
            image.SaveJpeg(ms, image.PixelWidth, image.PixelHeight, 0, 100);
            byte[] imageData = ms.ToArray();
            App.toBeSubmit[App.currentSubmissionIndex].ImageData = imageData;

            //Low Res Pic submission 
            MemoryStream lowresms = new MemoryStream();
            image.SaveJpeg(lowresms, 80, 60, 0, 80);
            byte[] lowResImageData = lowresms.ToArray();
            App.toBeSubmit[App.currentSubmissionIndex].LowResImageData = lowResImageData;

            Service1Client client = new Service1Client();
            client.SubmitDataCompleted += new EventHandler<SubmitDataCompletedEventArgs>(client_SubmitDataCompleted);
            client.SubmitDataAsync(App.toBeSubmit[App.currentSubmissionIndex]);
        }

        void client_SubmitDataCompleted(object sender, SubmitDataCompletedEventArgs e)
        {
            String txtFileName = App.toBeSubmit[App.currentSubmissionIndex].ImageName + ".txt";
            String imageFileName = App.toBeSubmit[App.currentSubmissionIndex].ImageName + ".jpg";
            using (IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                myIsolatedStorage.DeleteFile("MyScience/ToBeSubmit/"+App.currentUser.ID+"/" + txtFileName);
                myIsolatedStorage.DeleteFile("MyScience/Images/" +imageFileName);
            }
            //App.toBeSubmit.RemoveAt(App.currentSubmissionIndex);
            //App.firstAccess = true;
            //NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));

            progressbar.IsIndeterminate = false;
            String url = e.Result.ToString();
            displayPopup(popupTitle1, popupContent4);
            //messagePopup.IsOpen = true;
            //var uploadButton = DynamicPanel.Children.OfType<Button>().First() as Button;
            //uploadButton.IsEnabled = true;
        }

        /*parsing Json to get fields required*/
        private List<Field> GetFormField(String form)
        {
            //List<Field> fields = new List<Field>();
            form = form.Replace("\"__type\":\"Field:#MyScience\",", "");
            form = form.Replace("}", "},");
            form = form.Substring(0, form.Length - 1);
            form.Trim();
            form = "[" + form + "]";
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

        private static string popupTitle1 = "myscience";
        private static string popupTitle2 = "myscience error";
        private static string popupContent1 = "We're having a connectivity problem. This maybe because your cellular data connections are turned off. Please try again later.";
        private static string popupContent2 = "Submission Saved!";
        private static string popupContent3 = "Oops, forgot to submit a pic!";
        private static string popupContent4 = "Congratulation! Data Submitted Successfully!";

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
        }

        private void Photo_ImageOpened(object sender, RoutedEventArgs e)
        {
            SubmissionProgressBar.IsIndeterminate = false;

        }
    }
}