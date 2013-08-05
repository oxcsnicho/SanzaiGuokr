﻿using System;
using System.Windows;
using System.Windows.Threading;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using SanzaiGuokr.Messages;
using SanzaiGuokr.Model;
using SanzaiGuokr.ViewModel;
using System.Text;
using SanzaiGuokr.Util;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Tasks;
using System.Windows.Data;
using SanzaiGuokrV8;

namespace SanzaiGuokr
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            Messenger.Default.Register<GoToReadArticle>(this, (a) => _GoToReadArticle(a));
            Messenger.Default.Register<GoToReadPost>(this, (a) => _GoToReadPost(a));
            Messenger.Default.Register<GoToReadArticleComment>(this, (a) => _GoToReadArticleComment(a));
            Messenger.Default.Register<channel>(this, (a) => _GoToViewChannel(a));
            // Messenger.Default.Register<ChannelLoadFailureMessage>(this, (a) => _ChannelLoadFailure(a));
            Messenger.Default.Register<ViewImageMessage>(this, (a) =>
            {
#if DEBUG
                MessageBox.Show(a.med_uri);
#else
                DebugLogging.Append("ViewImage", a.med_uri, "");
#endif
                if (a.med_uri == null)
                    return;
                popup.IsOpen = true;
                imagePopupViewer.SourceUri = new Uri(a.med_uri, UriKind.Absolute);
                ApplicationBar.IsVisible = false;
            });
            Messenger.Default.Register<ReposeAWeibo>(this, (a) =>
                {
                    if (ViewModelLocator.ApplicationSettingsStatic.WeiboAccountLoginStatus)
                    {
                        ViewModelLocator.MainStatic.the_weibo = a.Status;
                        NavigationService.Navigate(new Uri("/WeiboRepost.xaml", UriKind.Relative));
                    }
                    else
                    {
                        MessageBox.Show("哥们，转发总得先登录一下吧\n\n o(# ￣▽￣)==O))￣▽￣\")o ");
                        NavigationService.Navigate(new Uri("/WeiboLoginPage2.xaml", UriKind.Relative));
                    }
                });

        }

        private void PhoneApplicationPage_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            SystemTray.IsVisible = true;

            imagePopupViewer.Tap += (ss, ee) =>
                {
                    popup.IsOpen = false;
                    ApplicationBar.IsVisible = true;
                };

#if false
            if (!ViewModelLocator.ApplicationSettingsStatic.IsGroupEnabledSettingBool
                && main_pivot.Items.Contains(group_pano))
            {
                main_pivot.HidePivotItem(group_pano);
            }
            else
            {
                main_pivot.UnHidePivotItem(group_pano);
            }
#endif

#if DEBUG
            foreach (var item in ApplicationBar.MenuItems)
            {
                var p = (item as ApplicationBarMenuItem);
                if (p != null && p.Text.Contains("好评"))
                {
                    p.Click -= recommend_Click;
                    p.Click += debug_Click;
                    p.Text = "debug";
                }
            }
#endif
        }

#if false
        private void _ChannelLoadFailure(ChannelLoadFailureMessage a)
        {
            //LatestArticleList.ListFooterTemplate = Application.Current.Resources["FailedFooterTamplate"] as DataTemplate;
        }
#endif

        private void _GoToViewChannel(channel a)
        {
            NavigationService.Navigate(new Uri("/ViewChannel.xaml", UriKind.Relative));
        }

        private void _GoToReadArticle(GoToReadArticle a)
        {
            if (NavigationService.CurrentSource.OriginalString != "/ReadArticle.xaml")
                NavigationService.Navigate(new Uri("/ReadArticle.xaml", UriKind.Relative));

            if (a.article.parent_list != null && a.article.order == a.article.parent_list.ArticleList.Count - 1)
                Task.Run(() => a.article.parent_list.load_more());
        }
        private void _GoToReadPost(GoToReadPost a)
        {
            if (NavigationService.CurrentSource.OriginalString != "/ReadPost.xaml")
                NavigationService.Navigate(new Uri("/ReadPost.xaml", UriKind.Relative));

#if false
            if (a.article.group != null)
                FlurryWP8SDK.Api.LogEvent("ViewPost", parameters: new List<FlurryWP8SDK.Models.Parameter> {
            new FlurryWP8SDK.Models.Parameter("group", a.article.group.name)
        });
#endif

            if (a.article.parent_list != null && a.article.order == a.article.parent_list.ArticleList.Count - 1)
                Task.Run(() => a.article.parent_list.load_more());
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
            MessageBox.Show("想加载最新文章/小组，麻烦退出程序后重新进入。\n\n ︵(￣︶￣)︵");
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
                ApplicationBar.IsVisible = true;
                e.Cancel = true;
                return;
            }
            // submission
            if (!ViewModelLocator.ApplicationSettingsStatic.HasReviewed && DateTime.Now > new DateTime(2013, 8, 11))
            {
                if (ViewModelLocator.ApplicationSettingsStatic.AnonymousUserId.Length > 0)
                {
                    char a = ViewModelLocator.ApplicationSettingsStatic.AnonymousUserId[0];
                    if (a == 'v' || a == 'V')
                    {
                        var t = MessageBox.Show("憋了这么久节操，是时候掉一点了！\n\n~~~掉节操求好评啊，亲！~~~\n\n ○(┬﹏┬)○ \n\n（发过也可以再发哦~）\n（好评顶上一千发裸照哦~）", "广告时间", MessageBoxButton.OKCancel);
                        if (t == MessageBoxResult.OK || t == MessageBoxResult.Yes)
                        {
                            DateTime ttt = DateTime.Now;
                            var tt = new MarketplaceReviewTask();
                            tt.Show();
                            if (DateTime.Now > ttt.AddSeconds(15))
                                ViewModelLocator.ApplicationSettingsStatic.HasReviewed = true;
                        }
                    }
                }
            }
            base.OnBackKeyPress(e);
        }

        PivotItem FocusedPivotItem;

        private void main_pivot_LoadedPivotItem(object sender, PivotItemEventArgs e)
        {
            if (FocusedPivotItem != null)
                App.ReportUsage(FocusedPivotItem.Name);

            FocusedPivotItem = e.Item;

            ResetUnloadTimer();

            if (e.Item == latest_articles_pano)
                HubTileService.UnfreezeGroup("hubs");
            else
                HubTileService.FreezeGroup("hubs");

            if (e.Item == latest_articles_pano
                || e.Item == group_pano)
                return;

            if (e.Item.Content == null)
            {
                if (e.Item == channels_pano)
                    e.Item.Content = new ChannelsUserControl();
                if (e.Item == mrguokr_pano)
                    e.Item.Content = new MrGuokrUserControl();

                e.Item.DataContext = this.DataContext;
            }
        }

        private void main_pivot_UnloadedPivotItem(object sender, PivotItemEventArgs e)
        {
        }

        #region UnloadTimer
        DispatcherTimer timer = new DispatcherTimer();
        int UnloadTimerIntervalSeconds = 10;
        void ResetUnloadTimer()
        {
            if (timer != null)
                timer.Stop();

            timer = GetNewTimer();
            timer.Start();
        }
        DispatcherTimer GetNewTimer()
        {
            DispatcherTimer t = new DispatcherTimer();
            t.Interval = new TimeSpan(0, 0, 0, UnloadTimerIntervalSeconds);
            t.Tick += (ss, ee) =>
                {
                    string text = "";
                    foreach (var item in main_pivot.Items)
                    {
                        var i = item as PivotItem;
                        if (i != null && i != latest_articles_pano
                            && i != group_pano
                            && i != FocusedPivotItem)
                        {
                            i.Content = null;
#if DEBUG
                            text += i.Name + " ";
#endif
                        }
#if DEBUG
                        if (!string.IsNullOrEmpty(text))
                            SetPIText("[Debug] Content unloaded, " + text);
#endif
                    }
                    t.Stop();
                };
            return t;
        }
        #endregion

#if false
        int page = 0;
        private async void group_Click(object sender, EventArgs e)
        {
            if (!GuokrApi.IsVerified)
                await GuokrApi.VerifyAccountV3();
            try
            {
                var posts = await GuokrApi.GetLatestPostsV2(page);
                StringBuilder sb = new StringBuilder();
                foreach (var item in posts)
                    sb.Append(item.title + "\n");

                MessageBox.Show(sb.ToString());
                var postDetail = await GuokrApi.GetPostContent(posts[0]);
                MessageBox.Show(postDetail.InnerText);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            page++;
        }
#endif

        private void viewNotices(object sender, System.Windows.RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/ViewNotices.xaml", UriKind.Relative));
        }

        private void test(object sender, System.Windows.ExceptionRoutedEventArgs e)
        {
            //var bi = new BitmapImage();
        }

        private void debug_Click(object sender, EventArgs e)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private void recommend_Click(object sender, EventArgs e)
        {
            var t = new MarketplaceReviewTask();
            t.Show();
        }

        private void random_gate_Click(object sender, EventArgs e)
        {
            if (ViewModelLocator.MainStatic.RandomGate.CanExecute(null))
                ViewModelLocator.MainStatic.RandomGate.Execute(null);
        }

        private void bookmark_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/ViewBookmark.xaml", UriKind.Relative));
        }
    }
}
