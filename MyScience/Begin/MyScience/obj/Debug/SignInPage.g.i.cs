﻿#pragma checksum "D:\My Documents\Stanford\Senior\Spring\CS 210B\dynovader\MyScience\Begin\MyScience\SignInPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "F75722030F0AFA2C0613CF8C336D1BF4"
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

