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
using Microsoft.Phone.Shell;

namespace MyScience
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();
            /*ApplicationBar = new ApplicationBar();
            ApplicationBar.IsVisible = true;
            ApplicationBar.IsMenuEnabled = true;*/

            //ApplicationBarIconButton homeButton = new ApplicationBarIconButton(new Uri("/Images/email.png", UriKind.Relative));
            //homeButton.Text = "Start";
            //homeButton.Click += new EventHandler(homeButton_Click);

            //ApplicationBarIconButton profileButton = new ApplicationBarIconButton(new Uri("/Images/question.png", UriKind.Relative));
            //profileButton.Text = "Stop";
            //profileButton.Click += new EventHandler(profileButton_Click);

            //ApplicationBar.Buttons.Add(homeButton);
            //ApplicationBar.Buttons.Add(profileButton);

            //createMenuItem();

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

        private void appList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }


        
    }
}