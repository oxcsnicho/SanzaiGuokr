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
using SanzaiGuokr.Util;
using SanzaiGuokrCore.Util;

namespace SanzaiGuokr
{
    public partial class ReadArticle : PhoneApplicationPage
    {
        public ReadArticle()
        {
            InitializeComponent();

            NavigationInTransition nvs = new NavigationInTransition();
            NavigationTransition n = new NavigationTransition();
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

        private async void email_share_click(object sender, System.Windows.RoutedEventArgs e)
        {
            EmailComposeTask t = new EmailComposeTask();

            if (a == null) return;
            var html = a.HtmlContent;
            var s = await Task.Run(() =>
                {
                    return Util.Common.FlattenHtmlConentToText(html);
                });
            t.Subject = "[果壳] " + a.title;
            t.Body = string.Format("标准链接： {0}\n移动版链接： {1}\n\n{2} - {3}\n\n{4}\n\n{5}",
                a.wwwurl,
                a.url,
                a.title.TrimEnd(new char[] { ' ', '\n', '\t', '\r' }), a.minisite_name,
                s,
                "Generated by 山寨果壳");
            sharePopup.IsOpen = false;
            t.Show();
        }

        private void weibo_share_click(object sender, System.Windows.RoutedEventArgs e)
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
                MessageBox.Show("哥们，转发总得先登录一下吧\n\n o(# ￣▽￣)==O))￣▽￣\")o ");
                NavigationService.Navigate(new Uri("/WeiboLoginPage2.xaml", UriKind.Relative));
            }
            sharePopup.IsOpen = false;
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            //weiboshare.Content = ViewModelLocator.ApplicationSettingsStatic.WeiboAccountLoginStatus ? "微博分享" : "微博登录(登录后分享)";
            a.refresh_comment_count();
        }

        private void view_comments_Click(object sender, EventArgs e)
        {
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        public object FlattenHtmlContentToText { get; set; }

        private void shareClick(object sender, System.Windows.RoutedEventArgs e)
        {
            sharePopup.IsOpen = !sharePopup.IsOpen;
        }

        private void canvasClick(object sender, System.Windows.Input.GestureEventArgs e)
        {
            sharePopup.IsOpen = false;
        }

        private void wbScriptNotify(object sender, Microsoft.Phone.Controls.NotifyEventArgs e)
        {
            var split = e.Value.IndexOf('|');
            if (split == -1)
                throw new ArgumentException("There is a code bug in ScriptNotify");

            var type = e.Value.Substring(0, split);
            var data = e.Value.Substring(split + 1);
            if (type == "img")
                Messenger.Default.Send<ViewImageMessage>(new ViewImageMessage()
                {
                    small_uri = data,
                    med_uri = (new GuokrImageInfo(data)).ToImage(),
                    large_uri = data
                });
            else if (type == "a")
            {
                if (data.IndexOf("about:") == 0)
                    data = data.Replace("about:", "http://www.guokr.com");
                try
                {
                    var t = new WebBrowserTask();
                    t.Uri = new Uri(data, UriKind.Absolute);
                    t.Show();
                }
                catch (Exception ee)
                {
                    DebugLogging.Append("ScriptNotfiy", e.Value, ee.Message);
                }
            }
        }
        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (ViewModelLocator.MainStatic.ImagePopupOpened)
            {
                ViewModelLocator.MainStatic.ImagePopupOpened = false;
                e.Cancel = true;
            }
            else
            {
                Messenger.Default.Unregister(this);
            }
            base.OnBackKeyPress(e);
        }

        private void GestureListener_Flick(object sender, FlickGestureEventArgs e)
        {
            if (e.Direction == System.Windows.Controls.Orientation.Horizontal
                && e.Angle > 160 && e.Angle < 200)
                (this.DataContext as ReadArticleViewModel).the_article.ReadThisArticleComment.Execute(null);
            else if (e.Direction == System.Windows.Controls.Orientation.Horizontal
                && (e.Angle > 350 || e.Angle < 40)
                && NavigationService.CanGoBack)
                NavigationService.GoBack();
        }
    }
}