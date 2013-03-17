using System;
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
using MC.Phone.Analytics;

namespace SanzaiGuokr
{
    public partial class MainPage : PhoneApplicationPage
    {
        ProgressIndicator pi;

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            Messenger.Default.Register<GoToReadArticle>(this, (a) => _GoToReadArticle(a));
            Messenger.Default.Register<GoToReadPost>(this, (a) => _GoToReadPost(a));
            Messenger.Default.Register<GoToReadArticleComment>(this, (a) => _GoToReadArticleComment(a));
            Messenger.Default.Register<channel>(this, (a) => _GoToViewChannel(a));
            Messenger.Default.Register<ChannelLoadFailureMessage>(this, (a) => _ChannelLoadFailure(a));
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
            pi = new ProgressIndicator();
            SystemTray.SetProgressIndicator(this, pi);

            if (ViewModelLocator.ApplicationSettingsStatic.ColorThemeStatus == ApplicationSettingsViewModel.ColorThemeMode.NIGHT)
                SetPIText("夜深了，调暗灯光..");

            imagePopupViewer.Tap += (ss, ee) =>
                {
                    popup.IsOpen = false;
                    ApplicationBar.IsVisible = true;
                };

            if (!ViewModelLocator.ApplicationSettingsStatic.IsGroupEnabledSettingBool
                && main_pivot.Items.Contains(group_pano))
            {
                main_pivot.HidePivotItem(group_pano);
            }
            else
            {
                main_pivot.UnHidePivotItem(group_pano);
            }
        }

        void SetPIText(string text)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    pi.Text = text;
                    pi.IsVisible = true;
                    DispatcherTimer dt = new DispatcherTimer();
                    dt.Interval = TimeSpan.FromSeconds(3);
                    dt.Tick += new EventHandler((ss, ee) =>
                    {
                        pi.IsVisible = false;
                        dt.Stop();
                    });
                    dt.Start();
                });
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

            if (a.article.parent_list != null && a.article.order == a.article.parent_list.ArticleList.Count - 1)
                TaskEx.Run(() => a.article.parent_list.load_more());
        }
        private void _GoToReadPost(GoToReadPost a)
        {
            NavigationService.Navigate(new Uri("/ReadPost.xaml", UriKind.Relative));

            if (a.article.parent_list != null && a.article.order == a.article.parent_list.ArticleList.Count - 1)
                TaskEx.Run(() => a.article.parent_list.load_more());
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
            }
            base.OnBackKeyPress(e);
        }

        PivotItem FocusedPivotItem;

        private void main_pivot_LoadedPivotItem(object sender, PivotItemEventArgs e)
        {
            if (FocusedPivotItem != null)
                Common.ReportUsage(FocusedPivotItem.Name);

            FocusedPivotItem = e.Item;

            ResetUnloadTimer();

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

        int page = 0;
        private async void group_Click(object sender, EventArgs e)
        {
            if (!GuokrApi.IsVerified)
                await GuokrApi.VerifyAccountV2("oxcsnicho@gmail.com", "nicholas");
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

        private void viewNotices(object sender, System.Windows.RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/ViewNotices.xaml", UriKind.Relative));
        }
    }
}
