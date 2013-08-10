using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using GalaSoft.MvvmLight.Messaging;
using SanzaiGuokr.Messages;

namespace SanzaiGuokrV8
{
    public partial class MainUserControl : UserControl
    {
        public MainUserControl()
        {
            InitializeComponent();

            LatestArticleList.ViewportReachedBottom += LatestArticleList_ViewportReachedBottom;
        }

        void LatestArticleList_ViewportReachedBottom(object sender, RoutedEventArgs e)
        {
                Messenger.Default.Send<MinimizeApplicationBar>(new MinimizeApplicationBar() { Invert = false });
        }

    }
}
