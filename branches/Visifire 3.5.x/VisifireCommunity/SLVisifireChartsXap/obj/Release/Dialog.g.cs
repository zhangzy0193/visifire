﻿#pragma checksum "E:\Visifire 3.5.x\WP7\VisifireCommunity\SLVisifireChartsXap\..\..\Common\SLVisifireChartsXap\Dialog.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "656DE3F30969208C1233FB3C71C004F8"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

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


namespace SLVisifireChartsXap {
    
    
    public partial class Dialog : System.Windows.Controls.UserControl {
        
        internal System.Windows.Media.Animation.Storyboard DialogInStoryBoard;
        
        internal System.Windows.Media.Animation.Storyboard DialogOutStoryBoard;
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
        internal System.Windows.Controls.Canvas canvas;
        
        internal System.Windows.Controls.Button CloseButton;
        
        internal System.Windows.Controls.TextBlock Info;
        
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
            System.Windows.Application.LoadComponent(this, new System.Uri("/SLVisifireChartsXap;component/Dialog.xaml", System.UriKind.Relative));
            this.DialogInStoryBoard = ((System.Windows.Media.Animation.Storyboard)(this.FindName("DialogInStoryBoard")));
            this.DialogOutStoryBoard = ((System.Windows.Media.Animation.Storyboard)(this.FindName("DialogOutStoryBoard")));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
            this.canvas = ((System.Windows.Controls.Canvas)(this.FindName("canvas")));
            this.CloseButton = ((System.Windows.Controls.Button)(this.FindName("CloseButton")));
            this.Info = ((System.Windows.Controls.TextBlock)(this.FindName("Info")));
        }
    }
}

