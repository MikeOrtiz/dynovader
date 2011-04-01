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

namespace MyScience
{
    public partial class ProfilePage : PhoneApplicationPage
    {
        private User currentUser;

        public ProfilePage(User user)
        {
            currentUser = user;
            InitializeComponent();
            DataContext = App.ViewModel;
        }

        public ProfilePage()
        {
            InitializeComponent();
            DataContext = App.ViewModel;
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
        private void ProfilePage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!App.ViewModel.IsDataLoaded)
            {
                App.ViewModel.LoadData();
            }
        }
    }
}