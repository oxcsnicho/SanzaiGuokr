using System;
using System.Windows;
using System.Windows.Threading;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using SanzaiGuokr.Messages;
using SanzaiGuokr.Model;
using SanzaiGuokr.ViewModel;

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
            Messenger.Default.Register<GoToReadArticleComment>(this, (a) => _GoToReadArticleComment(a));
            Messenger.Default.Register<channel>(this, (a) => _GoToViewChannel(a));
            Messenger.Default.Register<ChannelLoadFailureMessage>(this, (a) => _ChannelLoadFailure(a));
            Messenger.Default.Register<ViewImageMessage>(this, (a) =>
            {
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
                        MessageBox.Show("哥们，转发总得先登录一下吧");
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
                ApplicationBar.IsVisible = true;
                e.Cancel = true;
            }
            base.OnBackKeyPress(e);
        }

        PivotItem FocusedPivotItem;
        private void main_pivot_LoadedPivotItem(object sender, PivotItemEventArgs e)
        {
            FocusedPivotItem = e.Item;
            ResetUnloadTimer();

            if (e.Item == latest_articles_pano)
                return;

            if (e.Item.Content == null)
            {
                if (e.Item == channels_pano)
                    e.Item.Content = new ChannelsUserControl();
                if (e.Item.Name == "mrguokr_pano")
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
                            && i != FocusedPivotItem)
                        {
                            i.Content = null;
#if DEBUG
                            text += i.Name + " ";
#endif
                        }
#if DEBUG
                        if (!string.IsNullOrEmpty(text))
                            SetPIText("[Debug] Content unloaded, "+text);
#endif
                    }
                    t.Stop();
                };
            return t;
        }
        #endregion
    }
}
