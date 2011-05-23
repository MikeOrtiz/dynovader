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
using System.Windows.Navigation;

namespace MyScience
{
    public partial class LogoutMessageControl : UserControl
    {
        public LogoutMessageControl()
        {
            InitializeComponent();
        }

        private void logout_Click(object sender, RoutedEventArgs e)
        {
            ((App)Application.Current).appReset();
            App.popup.IsOpen = false;
        }

        private void stay_Click(object sender, RoutedEventArgs e)
        {
            App.popup.IsOpen = false;
        }
    }
}
