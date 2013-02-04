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
using RestSharp;
using SanzaiGuokr.GuokrObject;
using SanzaiGuokr.Util;
using GalaSoft.MvvmLight.Messaging;
using SanzaiGuokr.Messages;
using System.Windows.Threading;

namespace SanzaiGuokr
{
    public partial class ViewComments : PhoneApplicationPage
    {
        ProgressIndicator pi;
        public ViewComments()
        {
            InitializeComponent();

            NavigationInTransition nvs = new NavigationInTransition();
            NavigationTransition n = new NavigationTransition();
            Loaded += new RoutedEventHandler(ViewComments_Loaded);

#if DEBUG
            debugTextBox.Visibility = System.Windows.Visibility.Visible;
            debugButton.Visibility = System.Windows.Visibility.Visible;
#endif

            Messenger.Default.Register<ReferenceCommentMessage>(this, (a) =>
                commentBox.Text += "[blockquote]" + "引用@" + a.comment.nickname + " 的话：\n" + a.comment.ReferenceContent + "[/blockquote]");
            Messenger.Default.Register<DeleteCommentComplete>(this, (a) =>
                {
                    if (a.Exception != null)
                        MessageBox.Show("删除失败。。" + a.Exception.errmsg);
                    else
                    {
                        var vm = DataContext as ViewCommentsViewModel;
                        if (vm == null)
                            return;
                        if(vm.the_article.CommentList.ArticleList.Remove(a.comment))
                            MessageBox.Show("删除成功！\n\n <(￣oo,￣)/ ");
#if false
                        if (pi != null)
                        {
                            pi.Text = "删除完成！";
                            pi.IsVisible = true;
                            var dt = new DispatcherTimer();
                            dt.Interval = TimeSpan.FromSeconds(3);
                            dt.Tick += (ss, ee) =>
                            {
                                pi.IsVisible = false;
                                dt.Stop();
                            };
                            dt.Start();
                        }
#endif
                    }
                });
        }

        void ViewComments_Loaded(object sender, RoutedEventArgs e)
        {
            pi = new ProgressIndicator();
            SystemTray.SetProgressIndicator(this, pi);
        }

        private void email_share_Click(object sender, EventArgs e)
        {
            EmailComposeTask t = new EmailComposeTask();
            var a = (this.DataContext as ReadArticleViewModel).the_article;
            t.Subject = "[果壳] " + a.title;
            t.Body = string.Format("标准链接： {0}\n移动版链接： {1}\n\n{2} - {3}\n\n{4}\n\n{5}",
                "http://www.guokr.com/article/" + a.id.ToString() + "/",
                a.url,
                a.title.TrimEnd(new char[] { ' ', '\n', '\t', '\r' }), a.minisite_name,
                a.Abstract,
                "Generated by 山寨果壳");
            t.Show();
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("debug");
        }

        private async void sendButton_Click(object sender, RoutedEventArgs e)
        {
            string comment = commentBox.Text;
            if (string.IsNullOrWhiteSpace(comment))
                return;
            var dc = this.DataContext as ViewCommentsViewModel;
            if (dc == null)
            {
                MessageBox.Show("bug");
                return;
            }


            VisualStateManager.GoToState(this, "Disabled", false);

            try
            {
                await GuokrApi.PostCommentV2(dc.the_article, comment);
                MessageBox.Show("发送成功");
                commentBox.Text = "";

#if false
                var response = await RestSharpAsync<PostReplyResponse>.RestSharpExecuteAsyncTask(client, req);
                if (response.StatusCode == HttpStatusCode.OK)
                    MessageBox.Show("发送成功");
                else
                    MessageBox.Show("发送失败");
#endif
            }
            catch (GuokrException gex)
            {
                switch (gex.errnum)
                {
                    case GuokrErrorCode.LoginRequired:
                        MessageBox.Show("童鞋，先登录吧？\n\n （＃－.－）/");
                        NavigationService.Navigate(new Uri("/GuokrLoginPage.xaml", UriKind.Relative));
                        break;
                    case GuokrErrorCode.VerificationFailed:
                        MessageBox.Show("咦？你改密码了？重新登录一次吧～\n\n  <(￣︶￣)/");
                        NavigationService.Navigate(new Uri("/GuokrLoginPage.xaml", UriKind.Relative));
                        break;
                    case GuokrErrorCode.OK:
                        break;
                    case GuokrErrorCode.CommentTooFrequent:
                        MessageBox.Show("手速太快了哥们。。\n\n ))))m -_-)/" + gex.errmsg);
                        break;
                    case GuokrErrorCode.CallFailure:
                        MessageBox.Show("连接败了，大概是个bug。。\n\n \\(￣▽￣)♂" + gex.errmsg);
                        break;
                    default:
                        MessageBox.Show("发送失败，大概有bug。。\n\n \\(￣▽￣)♂" + gex.errmsg);
                        // 错误日志
                        break;
                }
            }
            catch
            {
                MessageBox.Show("发送失败，大概有bug。。");
                // 错误日志
            }

            VisualStateManager.GoToState(this, "Normal", false);

        }
        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (ViewModelLocator.MainStatic.ImagePopupOpened)
            {
                ViewModelLocator.MainStatic.ImagePopupOpened = false;
                e.Cancel = true;
            }
            base.OnBackKeyPress(e);
        }
    }
}