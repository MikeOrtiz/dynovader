﻿#pragma checksum "C:\Users\Lu Li\Documents\dynovader\MyScience\Begin\MyScience\SettingPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "F12A39894834A1368B76D3A672E18CC9"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using System.Windows.Shapes;
using System.Windows.Threading;


namespace MyScience {
    
    
    public partial class SettingPage : Microsoft.Phone.Controls.PhoneApplicationPage {
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
        internal System.Windows.Controls.StackPanel TitlePanel;
        
        internal System.Windows.Controls.TextBlock ApplicationTitle;
        
        internal System.Windows.Controls.TextBlock PageTitle;
        
        internal System.Windows.Controls.Grid ContentPanel;
        
        internal Microsoft.Phone.Shell.ApplicationBarIconButton homeButton;
        
        internal Microsoft.Phone.Shell.ApplicationBarIconButton profileButton;
        
        internal Microsoft.Phone.Shell.ApplicationBarIconButton rankButton;
        
        internal Microsoft.Phone.Shell.ApplicationBarIconButton settingsButton;
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Windows.Application.LoadComponent(this, new System.Uri("/MyScience;component/SettingPage.xaml", System.UriKind.Relative));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
            this.TitlePanel = ((System.Windows.Controls.StackPanel)(this.FindName("TitlePanel")));
            this.ApplicationTitle = ((System.Windows.Controls.TextBlock)(this.FindName("ApplicationTitle")));
            this.PageTitle = ((System.Windows.Controls.TextBlock)(this.FindName("PageTitle")));
            this.ContentPanel = ((System.Windows.Controls.Grid)(this.FindName("ContentPanel")));
            this.homeButton = ((Microsoft.Phone.Shell.ApplicationBarIconButton)(this.FindName("homeButton")));
            this.profileButton = ((Microsoft.Phone.Shell.ApplicationBarIconButton)(this.FindName("profileButton")));
            this.rankButton = ((Microsoft.Phone.Shell.ApplicationBarIconButton)(this.FindName("rankButton")));
            this.settingsButton = ((Microsoft.Phone.Shell.ApplicationBarIconButton)(this.FindName("settingsButton")));
        }
    }
}

