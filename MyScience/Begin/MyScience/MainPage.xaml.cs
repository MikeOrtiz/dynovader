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

using MyScience.MyScienceService;

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

                MyScienceServiceClient client = new MyScienceServiceClient();
                client.GetProjectsCompleted += new EventHandler<GetProjectsCompletedEventArgs>(client_GetProjectsCompleted);
                client.GetProjectsAsync();
                userName.Text = App.currentUser.Name;
                score.Text = "Score: " + App.currentUser.Score.ToString();
                scientistLevel.Text = App.currentUser.Score < 5 ? "Newb" : "Aspiring Scientist";

                client.GetTopScorersCompleted += new EventHandler<GetTopScorersCompletedEventArgs>(client_GetTopScorersCompleted);
                client.GetTopScorersAsync();
                //this.Loaded += new RoutedEventHandler(TopScorers_Loaded);
            }
        }

        private void TopScorers_Loaded(object sender, RoutedEventArgs e)
        {
            MyScienceServiceClient client = new MyScienceServiceClient();
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
    }
}