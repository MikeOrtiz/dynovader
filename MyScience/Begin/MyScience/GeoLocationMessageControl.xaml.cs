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

namespace MyScience
{
    public partial class GeoLocationMessageControl : UserControl
    {
        public delegate void registerUser();
        private registerUser registerUserFn;

        public GeoLocationMessageControl(registerUser callBack)
        {
            InitializeComponent();
            registerUserFn = callBack;
        }

        private void continue_Click(object sender, RoutedEventArgs e)
        {
            //((App)Application.Current).registerUser();
            registerUserFn();
            App.popup.IsOpen = false;
        }

        private void stay_Click(object sender, RoutedEventArgs e)
        {
            App.popup.IsOpen = false;
        }
    }
}
