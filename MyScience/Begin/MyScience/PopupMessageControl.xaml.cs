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
    public partial class PopupMessageControl : UserControl
    {
        public delegate void activatePage();
        private activatePage activatePageFn;

        public PopupMessageControl(activatePage callBack)
        {
            InitializeComponent();
            activatePageFn = callBack;
        }

        private void close_Click(object sender, RoutedEventArgs e)
        {
            App.popup.IsOpen = false;
            //LayoutRoot.IsHitTestVisible = true;
            activatePageFn();
        }
    }
}
