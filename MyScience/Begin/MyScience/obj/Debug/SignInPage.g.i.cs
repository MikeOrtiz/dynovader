﻿#pragma checksum "C:\Users\Lu Li\Documents\cs210\MyScience\Begin\MyScience\SignInPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "2747C9426F6D7756E93B110441E38A93"
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
    
    
    public partial class SignIn : Microsoft.Phone.Controls.PhoneApplicationPage {
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
        internal System.Windows.Controls.StackPanel TitlePanel;
        
        internal System.Windows.Controls.TextBlock ApplicationTitle;
        
        internal System.Windows.Controls.TextBlock PageTitle;
        
        internal System.Windows.Controls.Grid ContentPanel;
        
        internal System.Windows.Controls.Canvas SignInCanvas;
        
        internal System.Windows.Controls.TextBlock userNameBlock;
        
        internal System.Windows.Controls.TextBox userNameBox;
        
        internal System.Windows.Controls.Button signInButton;
        
        internal System.Windows.Controls.TextBlock tryAgainBlock;
        
        internal System.Windows.Controls.Canvas RegisterCanvas;
        
        internal System.Windows.Controls.TextBlock registerNameBlock;
        
        internal System.Windows.Controls.TextBox registerNameBox;
        
        internal System.Windows.Controls.Button registerButton;
        
        internal System.Windows.Controls.TextBlock registerAgainBlock;
        
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
            System.Windows.Application.LoadComponent(this, new System.Uri("/MyScience;component/SignInPage.xaml", System.UriKind.Relative));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
            this.TitlePanel = ((System.Windows.Controls.StackPanel)(this.FindName("TitlePanel")));
            this.ApplicationTitle = ((System.Windows.Controls.TextBlock)(this.FindName("ApplicationTitle")));
            this.PageTitle = ((System.Windows.Controls.TextBlock)(this.FindName("PageTitle")));
            this.ContentPanel = ((System.Windows.Controls.Grid)(this.FindName("ContentPanel")));
            this.SignInCanvas = ((System.Windows.Controls.Canvas)(this.FindName("SignInCanvas")));
            this.userNameBlock = ((System.Windows.Controls.TextBlock)(this.FindName("userNameBlock")));
            this.userNameBox = ((System.Windows.Controls.TextBox)(this.FindName("userNameBox")));
            this.signInButton = ((System.Windows.Controls.Button)(this.FindName("signInButton")));
            this.tryAgainBlock = ((System.Windows.Controls.TextBlock)(this.FindName("tryAgainBlock")));
            this.RegisterCanvas = ((System.Windows.Controls.Canvas)(this.FindName("RegisterCanvas")));
            this.registerNameBlock = ((System.Windows.Controls.TextBlock)(this.FindName("registerNameBlock")));
            this.registerNameBox = ((System.Windows.Controls.TextBox)(this.FindName("registerNameBox")));
            this.registerButton = ((System.Windows.Controls.Button)(this.FindName("registerButton")));
            this.registerAgainBlock = ((System.Windows.Controls.TextBlock)(this.FindName("registerAgainBlock")));
        }
    }
}
