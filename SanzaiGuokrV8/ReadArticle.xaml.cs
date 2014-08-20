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
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Messaging;
using SanzaiGuokr.Messages;
using SanzaiGuokr.Util;
using SanzaiGuokrCore.Util;
using System.Windows.Data;
using webbrowsertest;
using System.Text.RegularExpressions;
using System.Diagnostics;
using MicroMsg.sdk;
using System.Windows.Resources;
using SanzaiGuokrV8;
using System.IO;

namespace SanzaiGuokr
{
    public partial class ReadArticle : PhoneApplicationPage
    {
        public ReadArticle()
        {
            InitializeComponent();

            NavigationInTransition nvs = new NavigationInTransition();
            NavigationTransition n = new NavigationTransition();

#if PIPROFILING
            var pi = new ProgressIndicator();
            pi.IsVisible = true;
            SystemTray.SetProgressIndicator(this, pi);
            Messenger.Default.Register<SetProgressIndicator>(this, (a) => Common.ProcessProgressIndicator(SystemTray.GetProgressIndicator(this), a));
#endif
        }

        private article a
        {
            get
            {
                var dc = (this.DataContext as ReadArticleViewModel);
                if (dc == null) return null;

                if (dc.the_article == null)
                {
                    string article_id;
                    if (this.NavigationContext.QueryString.TryGetValue("article_id", out article_id))
                    {
                        dc.the_article = new article();
                        dc.the_article.id = Convert.ToInt32(article_id);
                        GuokrApi.GetArticleV2(dc.the_article);
                    }
                    else
                        return null;
                }

                return dc.the_article;
            }
        }

        private async Task<byte[]> readRes(Uri uri)
        {
            //if (uri == null)
                return readRes("guokr_64x64.png");

            Stream stream = await WebClientAsync.OpenReadAsync(uri);
            byte[] data = new byte[stream.Length];
            stream.Read(data, 0, (int)stream.Length);
            return data;
        }
        private byte[] readRes(string path)
        {
            StreamResourceInfo info = App.GetResourceStream(new Uri(path, UriKind.Relative));
            if (info == null) return null;
            Stream stream = info.Stream;
            byte[] data = new byte[stream.Length];
            stream.Read(data, 0, (int)stream.Length);
            return data;
        }

        bool customMessageBoxOpened = false;
        private void weixin_share_click(object sender, System.Windows.RoutedEventArgs e)
        {
            var askk = new CustomMessageBox()
            {
                Caption = "分享方式",
                Message = "发到朋友圈还是微信好友？",
                LeftButtonContent = "微信好友",
                RightButtonContent = "朋友圈"
            };
            customMessageBoxOpened = true;
            askk.Dismissed += async (ss, ee) => 
                {
                    sharePopup.IsOpen = false;

                    int scene = 0;
                    switch (ee.Result)
                    {
                        case CustomMessageBoxResult.RightButton:
                            scene = SendMessageToWX.Req.WXSceneTimeline;
                            break;
                        case CustomMessageBoxResult.LeftButton:
                            scene = SendMessageToWX.Req.WXSceneSession;
                            break;
                        case CustomMessageBoxResult.None:
                        default:
                            return;
                    }

                    string AppID = "wxe92e817bb0352573";
                    var msg = new WXWebpageMessage();
                    if (a == null)
                    {
                        MessageBox.Show("Object a is null");
                        return;
                    }

                    await a.StatusReady();
                    msg.Title = a.title;
                    msg.Description = string.IsNullOrEmpty(a.Abstract) ? "点击链接看详细内容" : a.Abstract;
                    msg.ThumbData = await readRes(a.small_pic_url);
                    msg.WebpageUrl = a.wwwurl;
                    try
                    {
                        SendMessageToWX.Req req = new SendMessageToWX.Req(msg, scene);
                        IWXAPI api = WXAPIFactory.CreateWXAPI(AppID);

                        Console.WriteLine("api.SendReq in");
                        api.SendReq(req);
                        GoogleAnalytics.EasyTracker.GetTracker().SendEvent("Sharing", "Weixin_share", 
                            NavigationService.CurrentSource.ToString().Contains("FileTypeAssociation")?"Restarted":"Started", 1);
                    }
                    catch (WXException ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                };
            askk.Show();
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
            GoogleAnalytics.EasyTracker.GetTracker().SendEvent("Sharing", "email_share", "Default", 1);
        }

        private void weibo_share_click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ViewModelLocator.ApplicationSettingsStatic.WeiboAccountLoginStatus)
            {
                // post new weibo
                if (a == null) return;
                Messenger.Default.Send<CreateAWeibo>(new CreateAWeibo() { Article = a, Type = CreateAWeibo.ShareWeiboType.ShareGuokrArticle });
                NavigationService.Navigate(new Uri("/WeiboCreate.xaml", UriKind.Relative));
                GoogleAnalytics.EasyTracker.GetTracker().SendEvent("Sharing", "weibo_share", "Started", 1);
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
            ViewModelLocator.MainStatic.SetNavigationService(this.NavigationService);
            //weiboshare.Content = ViewModelLocator.ApplicationSettingsStatic.WeiboAccountLoginStatus ? "微博分享" : "微博登录(登录后分享)";
            a.refresh_comment_count();
            SystemTray.IsVisible = true;
            if (NavigationService.BackStack != null && NavigationService.BackStack.Count() > 0)
                if (Common.uriComparePath(NavigationService.BackStack.FirstOrDefault().Source, NavigationService.CurrentSource))
                    NavigationService.RemoveBackEntry();
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
                    if (data.Contains("swf"))
                        if (data.Contains("player.youku.com"))
                        {
                            var match = Regex.Match(data, @"/sid/([a-zA-Z0-9]*?)/");
                            if (match.Success)
                                data = string.Format("http://v.youku.com/v_show/id_{0}.html", match.Groups[1]);
                        }
                        else if (data.Contains("www.tudou.com"))
                        {
                            var match = Regex.Match(data, @"/v/([a-zA-Z0-9_-]*?)/");
                            if (match.Success)
                                data = string.Format("http://www.tudou.com/programs/view/{0}/", match.Groups[1]);
                        }
                        else if (data.Contains("video.sina.com"))
                        {
                            var match = Regex.Match(data, @"vid=([0-9]*).*uid=([0-9]*)/");
                            if (match.Success)
                                data = string.Format("http://video.sina.com.cn/v/b/{0}-{1}.html", match.Groups[1], match.Groups[2]);
                        }
                        else
                            GoogleAnalytics.EasyTracker.GetTracker().SendEvent("OpenSWF", data, "ReadArticle", 1);
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
            if (ViewModelLocator.MainStatic.ImagePopupOpened || customMessageBoxOpened || sharePopup.IsOpen)
            {
                ViewModelLocator.MainStatic.ImagePopupOpened = false;
                sharePopup.IsOpen = false;
                customMessageBoxOpened = false;
                e.Cancel = true;
            }
            else
            {
                Messenger.Default.Unregister(this);
                MYFUCKYOUWP.ClearContent();
            }
            if (!NavigationService.CanGoBack)
            {
                NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
                NavigationService.RemoveBackEntry();
            }
            else
                base.OnBackKeyPress(e);
        }

        private void GestureListener_Flick(object sender, FlickGestureEventArgs e)
        {
#if false
            if (ViewModelLocator.MainStatic.ImagePopupOpened)
                return;

            if (e.Direction == System.Windows.Controls.Orientation.Horizontal
                && e.Angle > 160 && e.Angle < 200)
                (this.DataContext as ReadArticleViewModel).the_article.ReadThisArticleComment.Execute(null);
            else if (e.Direction == System.Windows.Controls.Orientation.Horizontal
                && (e.Angle > 350 || e.Angle < 40)
                && NavigationService.CanGoBack)
                NavigationService.GoBack();
#endif
        }

    }
}