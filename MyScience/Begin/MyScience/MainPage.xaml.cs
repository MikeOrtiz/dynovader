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

namespace MyScience
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();
            ApplicationBar = new ApplicationBar();
            ApplicationBar.IsVisible = true;
            ApplicationBar.IsMenuEnabled = true;

            //ApplicationBarIconButton homeButton = new ApplicationBarIconButton(new Uri("/Images/email.png", UriKind.Relative));
            //homeButton.Text = "Start";
            //homeButton.Click += new EventHandler(homeButton_Click);

            //ApplicationBarIconButton profileButton = new ApplicationBarIconButton(new Uri("/Images/question.png", UriKind.Relative));
            //profileButton.Text = "Stop";
            //profileButton.Click += new EventHandler(profileButton_Click);

            //ApplicationBar.Buttons.Add(homeButton);
            //ApplicationBar.Buttons.Add(profileButton);

            createMenuItem();

            DataContext = App.ViewModel;
            this.Loaded += new RoutedEventHandler(MainPage_Loaded);

        }

        void createMenuItem()
        {
            string[] imagePath = new string[] { "Images/home.png", "Images/people.png", "Images/rank.png", "Images/setting.png" };
            string[] buttonLabel = new string[] { "Home", "Profile", "Rank", "Settings" };
            ApplicationBarIconButton[] barButtons = new ApplicationBarIconButton[4];
            for (int i = 0; i < barButtons.Length; i++)
            {
                barButtons[i] = new ApplicationBarIconButton(new Uri(imagePath[i], UriKind.Relative));
                barButtons[i].Text = buttonLabel[i];
                ApplicationBar.Buttons.Add(barButtons[i]);
                //this is a comment!!!!
            }
            barButtons[0].Click += new EventHandler(homeButton_Click);
            barButtons[1].Click += new EventHandler(profileButton_Click);
            barButtons[2].Click += new EventHandler(rankButton_Click);
            barButtons[3].Click += new EventHandler(settingsButton_Click);
            //ApplicationBarMenuItem[] menuItems = new ApplicationBarMenuItem[4];
            //for (int i = 0; i < menuItems.Length; i++)
            //{
            //    menuItems[i] = new ApplicationBarMenuItem("MenuItem " + i);
            //}
            //menuItems[0].Click += new EventHandler(menuItem0_Click);
            //menuItems[1].Click += new EventHandler(menuItem1_Click);
            //menuItems[2].Click += new EventHandler(menuItem2_Click);
            //menuItems[3].Click += new EventHandler(menuItem3_Click);

            
        }

        void settingsButton_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        
        void infoButton_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        void rankButton_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        void menuItem3_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        void menuItem2_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        void menuItem1_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        void menuItem0_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        void profileButton_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        void homeButton_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void appList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void listBox2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

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
        }
  
    }
}