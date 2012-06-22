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

namespace SanzaiGuokr
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            //LoadTheme();

            InitializeComponent();
        }

        private void PhoneApplicationPage_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            Messenger.Default.Register<GoToReadArticle>(this, (a)=>_GoToReadArticle(a));
            Messenger.Default.Register<channel>(this, (a)=>_GoToViewChannel(a));
            Messenger.Default.Register<ChannelLoadFailureMessage>(this, (a) => _ChannelLoadFailure(a));
            
            SystemTray.IsVisible = true;
        }

        private void LoadTheme()
        {
            if (DateTime.Now > DateTime.Now.Date.AddHours(23) ||
                DateTime.Now < DateTime.Now.Date.AddHours(6))
            {
                var res = MessageBox.Show("夜深了，来点小情调吧？", "夜深了...", MessageBoxButton.OKCancel);
                switch (res)
                {
                    case MessageBoxResult.Cancel:
                    case MessageBoxResult.No:
                    case MessageBoxResult.None:
                        break;

                    case MessageBoxResult.OK:
                    case MessageBoxResult.Yes:
                    default:

                        var uri = new Uri("/SanzaiGuokr;component/Styles/Night.xaml", UriKind.Relative);
                        var the_theme = new ResourceDictionary { Source = uri };
                        Application.Current.Resources.MergedDictionaries.RemoveAt(0); // dangerous: assuming day.xaml being [0]
                        Application.Current.Resources.MergedDictionaries.Add(the_theme);
                        break;
                }
            }
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
            NavigationService.Navigate(new Uri("/ReadArticle.xaml",UriKind.Relative));

            if (a.article.order == a.article.parent_list.ArticleList.Count - 1)
                a.article.parent_list.load_more();
        }

        private void suggestion_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/about.xaml", UriKind.Relative));
        }

        private void loadlatest_Click(object sender, EventArgs e)
        {
            MessageBox.Show("想加载最新文章，麻烦退出程序后重新进入。︵(￣︶￣)︵");
        }

        private void togglefontsize_Click(object sender, EventArgs e)
        {
            var _as = ViewModelLocator.ApplicationSettingsStatic;

            _as.FontSizeSetting = _as.FontSizeSetting == ApplicationSettingsViewModel.FontSizeSettingLarge ? ApplicationSettingsViewModel.FontSizeSettingNormal : ApplicationSettingsViewModel.FontSizeSettingLarge;
            
        }
    }
}
