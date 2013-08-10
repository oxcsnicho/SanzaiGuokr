using System;
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
using System.Threading.Tasks;

namespace SanzaiGuokr
{
    public partial class SearchArticle : PhoneApplicationPage
    {
        public SearchArticle()
        {
            InitializeComponent();

            Loaded += new RoutedEventHandler(SearchArticle_Loaded);

#if PIPROFILING
            var pi = new ProgressIndicator();
            pi.IsVisible = true;
            SystemTray.SetProgressIndicator(this, pi);
            Messenger.Default.Register<SetProgressIndicator>(this, (a) => Common.ProcessProgressIndicator(SystemTray.GetProgressIndicator(this), a));
#endif
        }

        void SearchArticle_Loaded(object sender, RoutedEventArgs e)
        {
            SystemTray.IsVisible = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("debug");
        }

        private async void sendButton_Click(object sender, RoutedEventArgs e)
        {
            var dc = this.DataContext as SearchArticleViewModel;
            if (dc == null)
                throw new ArgumentException("Cannot get datacontext");

            if (string.IsNullOrWhiteSpace(commentBox.Text))
                return;

            VisualStateManager.GoToState(this, "Disabled", false);

            dc.SearchResultList.page = 0;
            await Task.Run(async () => await dc.SearchResultList.load_more(true));

#if false
            string comment = commentBox.Text;
            if (string.IsNullOrWhiteSpace(comment))
                return;
            var dc = this.DataContext as SearchArticleViewModel;
            if (dc == null)
            {
                MessageBox.Show("bug");
                return;
            }



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
#endif

            VisualStateManager.GoToState(this, "Normal", false);

        }
        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
        }

        private void GestureListener_Flick(object sender, FlickGestureEventArgs e)
        {
        }
    }
}