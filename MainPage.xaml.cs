﻿using Microsoft.Phone.Controls;
using GalaSoft.MvvmLight.Messaging;
using SanzaiGuokr.Messages;
using System;
using System.Windows;
using System.Windows.Media.Imaging;
using SanzaiGuokr.Model;
using Microsoft.Phone.Tasks;
using Microsoft.Phone.Shell;
using System.Reflection;
using SanzaiGuokr.ViewModel;
using System.Windows.Threading;
using Microsoft.Phone.Info;

namespace SanzaiGuokr
{
    public partial class MainPage : PhoneApplicationPage
    {
        ProgressIndicator pi;

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            if (!IsAdminLiveId)
            {
                adminTab.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void PhoneApplicationPage_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            Messenger.Default.Register<GoToReadArticle>(this, (a) => _GoToReadArticle(a));
            Messenger.Default.Register<GoToReadArticleComment>(this, (a) => _GoToReadArticleComment(a));
            Messenger.Default.Register<channel>(this, (a) => _GoToViewChannel(a));
            Messenger.Default.Register<ChannelLoadFailureMessage>(this, (a) => _ChannelLoadFailure(a));
            Messenger.Default.Register<ViewImageMessage>(this, (a) =>
            {
                popup.IsOpen = true;
                imagePopupViewer.SourceUri = new Uri(a.med_uri, UriKind.Absolute);
            });

            SystemTray.IsVisible = true;
            pi = new ProgressIndicator();
            SystemTray.SetProgressIndicator(this, pi);

            if (ViewModelLocator.ApplicationSettingsStatic.ColorThemeStatus == ApplicationSettingsViewModel.ColorThemeMode.NIGHT)
            {
                pi.Text = "夜深了，调暗灯光...";
                pi.IsVisible = true;
                DispatcherTimer dt = new DispatcherTimer();
                dt.Interval = TimeSpan.FromSeconds(3);
                dt.Tick += new EventHandler((ss, ee) =>
                {
                    pi.IsVisible = false;
                });
                dt.Start();
            }

            imagePopupViewer.Tap += (ss, ee) => popup.IsOpen = false;
        }

        private void _ChannelLoadFailure(ChannelLoadFailureMessage a)
        {
            LatestArticleList.ListFooterTemplate = Application.Current.Resources["FailedFooterTamplate"] as DataTemplate;
        }

        private void _GoToViewChannel(channel a)
        {
            NavigationService.Navigate(new Uri("/ViewChannel.xaml", UriKind.Relative));
        }

        private void _GoToReadArticle(GoToReadArticle a)
        {
            NavigationService.Navigate(new Uri("/ReadArticle.xaml", UriKind.Relative));

            if (a.article.order == a.article.parent_list.ArticleList.Count - 1)
                a.article.parent_list.load_more();
        }
        private void _GoToReadArticleComment(GoToReadArticleComment a)
        {
            NavigationService.Navigate(new Uri("/ViewComments.xaml", UriKind.Relative));
        }

        private void suggestion_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/about.xaml", UriKind.Relative));
        }

        private void loadlatest_Click(object sender, EventArgs e)
        {
            MessageBox.Show("想加载最新文章，麻烦退出程序后重新进入。︵(￣︶￣)︵");
        }

        private void settings_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Settings.xaml", UriKind.Relative));
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (popup.IsOpen)
            {
                popup.IsOpen = false;
                e.Cancel = true;
            }
            base.OnBackKeyPress(e);
        }

        private void refreshMrGuokrToken_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/WeiboLoginPage2.xaml?mrguokr=true", UriKind.Relative));
        }

        #region AdminLiveId
        const string IsAdminLiveIdPropertyName = "IsAdminLiveId";
        bool IsAdminLiveId
        {
            get
            {
                try
                {
                    string anid = UserExtendedProperties.GetValue("ANID") as string;
                    string anonymousUserId = anid.Substring(2, 32); // in case anid is null, exception will be thrown which is desired
                    return anonymousUserId == "1D8D874F9703EB1C4FC0E2F5FFFFFFFF" ||
                        anonymousUserId == "35E1A346BCD794F5F4EC941DFFFFFFFF";
                }
                catch
                {
                    return true;
                }
            }
        }
        #endregion
    }
}
