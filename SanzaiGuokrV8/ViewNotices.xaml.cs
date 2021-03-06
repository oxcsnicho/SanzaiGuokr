﻿using System;
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
using Microsoft.Phone.Controls;
using SanzaiGuokr.Messages;
using GalaSoft.MvvmLight.Messaging;
using SanzaiGuokr.ViewModel;
using System.Threading.Tasks;
using Microsoft.Phone.Shell;
using SanzaiGuokr.Util;

namespace SanzaiGuokr
{
    public partial class ViewNotices : PhoneApplicationPage
    {
        public ViewNotices()
        {
            InitializeComponent();
#if PIPROFILING
            var pi = new ProgressIndicator();
            pi.IsVisible = true;
            SystemTray.SetProgressIndicator(this, pi);
            Messenger.Default.Register<SetProgressIndicator>(this, (a) => Common.ProcessProgressIndicator(SystemTray.GetProgressIndicator(this), a));
#endif
        }

        private void PhoneApplicationPage_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            SystemTray.IsVisible = true;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            switch (e.NavigationMode)
            {
                case System.Windows.Navigation.NavigationMode.Back:
                    break;
                case System.Windows.Navigation.NavigationMode.Forward:
                    break;
                case System.Windows.Navigation.NavigationMode.New:
                    //Messenger.Default.Register<ChannelLoadFailureMessage>(this, (a) => _ChannelLoadFailure(a));
                    ViewModelLocator.MainStatic.NoticeList.ArticleList.Clear();
                    Task.Run(() =>
                    {
                        ViewModelLocator.MainStatic.NoticeList.load_more();
                    });
                    break;
                case System.Windows.Navigation.NavigationMode.Refresh:
                    break;
                case System.Windows.Navigation.NavigationMode.Reset:
                    break;
                default:
                    break;
            }
            base.OnNavigatedTo(e);
        }

        private void ApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            var t = MessageBox.Show("确认清除所有消息？\n\n消息历史记录可以在电脑版果壳网站上找到\n（http://www.guokr.com/user/notice/）", "清除消息", MessageBoxButton.OKCancel);
            if (t == MessageBoxResult.OK || t == MessageBoxResult.Yes)
                ViewModelLocator.MainStatic.ResetRNNumber.Execute(null);
        }
#if false
        private void _ChannelLoadFailure(ChannelLoadFailureMessage a)
        {
            //ArticleList.ListFooterTemplate = Application.Current.Resources["FailedFooterTamplate"] as DataTemplate;
        }
#endif
    }
}