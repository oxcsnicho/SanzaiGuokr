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
using Microsoft.Phone.Tasks;
using SanzaiGuokr.Model;
using SanzaiGuokr.ViewModel;
using Microsoft.Phone.Shell;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Messaging;
using SanzaiGuokr.Messages;

namespace SanzaiGuokr
{
    public partial class ReadPost : PhoneApplicationPage
    {
        public ReadPost()
        {
            InitializeComponent();

            NavigationInTransition nvs = new NavigationInTransition();
            NavigationTransition n = new NavigationTransition();

#if !DEBUG
            debugTextBox.Visibility = System.Windows.Visibility.Collapsed;
            wbDebugTextbox.Visibility = System.Windows.Visibility.Collapsed;
#else
            Messenger.Default.Register<MyWebBrowserStatusChanged>(this, (s) => Deployment.Current.Dispatcher.BeginInvoke(
                () => wbDebugTextbox.Text = s.NewStatus));
#endif

        }

        private article a
        {
            get
            {
                var dc = (this.DataContext as ReadArticleViewModel);
                if (dc == null) return null;

                return dc.the_article;
            }
        }

        private async void email_share_Click(object sender, EventArgs e)
        {
            EmailComposeTask t = new EmailComposeTask();

            if (a == null) return;
            var html = a.HtmlContent;
            var s = await TaskEx.Run(() =>
                {
                    return Util.Common.FlattenHtmlConentToText(html);
                });
            t.Subject = "[果壳] " + a.title;
            t.Body = string.Format("标准链接： {0}\n移动版链接： {1}\n\n{2} - {3}\n\n{4}\n\n{5}",
                "http://www.guokr.com/article/" + a.id.ToString() + "/",
                a.url,
                a.title.TrimEnd(new char[] { ' ', '\n', '\t', '\r' }), a.minisite_name,
                s,
                "Generated by 山寨果壳");
            t.Show();
        }

        private void weibo_share_Click(object sender, EventArgs e)
        {
            if (ViewModelLocator.ApplicationSettingsStatic.WeiboAccountLoginStatus)
            {
                // post new weibo
                if (a == null) return;
                Messenger.Default.Send<CreateAWeibo>(new CreateAWeibo() { Article = a, Type = CreateAWeibo.ShareWeiboType.ShareGuokrArticle });
                NavigationService.Navigate(new Uri("/WeiboCreate.xaml", UriKind.Relative));
            }
            else
            {
                // setup account
                NavigationService.Navigate(new Uri("/WeiboLoginPage2.xaml", UriKind.Relative));
            }
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (ApplicationBarMenuItem item in ApplicationBar.MenuItems)
            {
                if (item.Text == "微博分享")
                    item.Text = ViewModelLocator.ApplicationSettingsStatic.WeiboAccountLoginStatus ? "微博分享" : "微博登录(登录后分享)";
            }
        }

        private void view_comments_Click(object sender, EventArgs e)
        {
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MYFUCKYOUWP.Refresh.Execute(null);

        }

        public object FlattenHtmlContentToText { get; set; }
    }
}