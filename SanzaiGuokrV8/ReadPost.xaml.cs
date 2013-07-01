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
using SanzaiGuokr.GuokrObject;

namespace SanzaiGuokr
{
    public partial class ReadPost : PhoneApplicationPage
    {
        public ReadPost()
        {
            InitializeComponent();

            NavigationInTransition nvs = new NavigationInTransition();
            NavigationTransition n = new NavigationTransition();
        }

        private GuokrPost a
        {
            get
            {
                var dc = (this.DataContext as ReadPostViewModel);
                if (dc == null) return null;

                return dc.the_article;
            }
        }

        private async void email_share_Click(object sender, EventArgs e)
        {
#if false
            EmailComposeTask t = new EmailComposeTask();

            if (a == null) return;
            var html = a.HtmlContent;
            var s = await Task.Run(() =>
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
#endif
        }

        private void weibo_share_Click(object sender, EventArgs e)
        {
#if false
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
#endif
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            a.refresh_comment_count();
        }

        private void wbScriptNotify(object sender, Microsoft.Phone.Controls.NotifyEventArgs e)
        {
        	MessageBox.Show(e.Value);
        }

        private void GestureListener_Flick(object sender, FlickGestureEventArgs e)
        {
            if (e.Direction == System.Windows.Controls.Orientation.Horizontal
                && e.Angle > 160 && e.Angle < 200)
                (this.DataContext as ReadPostViewModel).the_article.ReadThisArticleComment.Execute(null);
            else if (e.Direction == System.Windows.Controls.Orientation.Horizontal
                && (e.Angle > 350 || e.Angle < 40)
                && NavigationService.CanGoBack)
                NavigationService.GoBack();
        }

    }
}