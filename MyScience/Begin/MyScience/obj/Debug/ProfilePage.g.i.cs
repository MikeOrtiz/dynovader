﻿#pragma checksum "C:\Users\Lu Li\Documents\cs210\MyScience\Begin\MyScience\ProfilePage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "6BD00E7B6C9B2DCFDC4CCD933DC53262"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.225
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Microsoft.Phone.Controls;
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
    
    
    public partial class ProfilePage : Microsoft.Phone.Controls.PhoneApplicationPage {
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
        internal System.Windows.Controls.StackPanel TitlePanel;
        
        internal System.Windows.Controls.TextBlock ApplicationTitle;
        
        internal System.Windows.Controls.TextBlock PageTitle;
        
        internal System.Windows.Controls.Grid ContentPanel;
        
        internal System.Windows.Controls.Grid UserInfoPlusPic;
        
        internal System.Windows.Controls.Image userPic;
        
        internal System.Windows.Controls.Grid UserInfoGrid;
        
        internal System.Windows.Controls.TextBlock userName;
        
        internal System.Windows.Controls.TextBlock dob;
        
        internal System.Windows.Controls.Grid ActiveProjectsGrid;
        
        internal System.Windows.Controls.TextBlock UserProjectsText;
        
        internal System.Windows.Controls.ListBox ProjectListBox;
        
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
            System.Windows.Application.LoadComponent(this, new System.Uri("/MyScience;component/ProfilePage.xaml", System.UriKind.Relative));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
            this.TitlePanel = ((System.Windows.Controls.StackPanel)(this.FindName("TitlePanel")));
            this.ApplicationTitle = ((System.Windows.Controls.TextBlock)(this.FindName("ApplicationTitle")));
            this.PageTitle = ((System.Windows.Controls.TextBlock)(this.FindName("PageTitle")));
            this.ContentPanel = ((System.Windows.Controls.Grid)(this.FindName("ContentPanel")));
            this.UserInfoPlusPic = ((System.Windows.Controls.Grid)(this.FindName("UserInfoPlusPic")));
            this.userPic = ((System.Windows.Controls.Image)(this.FindName("userPic")));
            this.UserInfoGrid = ((System.Windows.Controls.Grid)(this.FindName("UserInfoGrid")));
            this.userName = ((System.Windows.Controls.TextBlock)(this.FindName("userName")));
            this.dob = ((System.Windows.Controls.TextBlock)(this.FindName("dob")));
            this.ActiveProjectsGrid = ((System.Windows.Controls.Grid)(this.FindName("ActiveProjectsGrid")));
            this.UserProjectsText = ((System.Windows.Controls.TextBlock)(this.FindName("UserProjectsText")));
            this.ProjectListBox = ((System.Windows.Controls.ListBox)(this.FindName("ProjectListBox")));
        }
    }
}

