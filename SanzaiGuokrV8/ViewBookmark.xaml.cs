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

namespace SanzaiGuokr
{
    public partial class ViewBookmark : PhoneApplicationPage
    {
        public ViewBookmark()
        {
            InitializeComponent();
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

#if false
        private void PhoneApplicationPage_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            Messenger.Default.Register<ChannelLoadFailureMessage>(this, (a) => _ChannelLoadFailure(a));
        }
        private void _ChannelLoadFailure(ChannelLoadFailureMessage a)
        {
            ArticleList.ListFooterTemplate = Application.Current.Resources["FailedFooterTamplate"] as DataTemplate;
        }
#endif
    }
}