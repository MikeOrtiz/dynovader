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
        public delegate void activatePage();
        private registerUser registerUserFn;
        private activatePage activatePageFn;

        public GeoLocationMessageControl(registerUser callBack, activatePage callBack2)
        {
            InitializeComponent();
            registerUserFn = callBack;
            activatePageFn = callBack2;
        }

        private void continue_Click(object sender, RoutedEventArgs e)
        {
            //((App)Application.Current).registerUser();
            registerUserFn();
            activatePageFn();
            //LayoutRoot.IsHitTestVisible = true;
            App.popup.IsOpen = false;
        }

        private void stay_Click(object sender, RoutedEventArgs e)
        {
            //LayoutRoot.IsHitTestVisible = true;
            activatePageFn();
            App.popup.IsOpen = false;
        }
    }
}
