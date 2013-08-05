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
using SanzaiGuokr.Messages;
using GalaSoft.MvvmLight.Messaging;
using SanzaiGuokr.ViewModel;
using System.Threading.Tasks;
using Microsoft.Phone.Shell;
using SanzaiGuokr.Util;

namespace SanzaiGuokr
{
    public partial class ViewBookmark : PhoneApplicationPage
    {
        public ViewBookmark()
        {
            InitializeComponent();
#if PIPROFILING
            var pi = new ProgressIndicator();
            pi.IsVisible = true;
            SystemTray.SetProgressIndicator(this, pi);
            Messenger.Default.Register<SetProgressIndicator>(this, (a) => Common.ProcessProgressIndicator(SystemTray.GetProgressIndicator(this), a));
#endif
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            switch (e.NavigationMode)
            {
                case System.Windows.Navigation.NavigationMode.Back:
                    break;
                case System.Windows.Navigation.NavigationMode.Forward:
                    break;
                case System.Windows.Navigation.NavigationMode.New:
                    ViewModelLocator.BookmarkStatic.BookmarkList.Bookmarks.SubmitChanges();
                    ViewModelLocator.BookmarkStatic.BookmarkList.ArticleList.Clear();
                    ViewModelLocator.BookmarkStatic.BookmarkList.LoadMoreArticles.Execute(null);
                    break;
                case System.Windows.Navigation.NavigationMode.Refresh:
                    break;
                case System.Windows.Navigation.NavigationMode.Reset:
                    break;
                default:
                    break;
            }
            base.OnNavigatedTo(e);
        }

        private void PhoneApplicationPage_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            SystemTray.IsVisible = true;
        }
#if false
        private void _ChannelLoadFailure(ChannelLoadFailureMessage a)
        {
            ArticleList.ListFooterTemplate = Application.Current.Resources["FailedFooterTamplate"] as DataTemplate;
        }
#endif
    }
}