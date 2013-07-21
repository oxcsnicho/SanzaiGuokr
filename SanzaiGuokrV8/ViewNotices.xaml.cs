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
    public partial class ViewNotices : PhoneApplicationPage
    {
        public ViewNotices()
        {
            InitializeComponent();
        }

        private void PhoneApplicationPage_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            //Messenger.Default.Register<ChannelLoadFailureMessage>(this, (a) => _ChannelLoadFailure(a));
            ViewModelLocator.MainStatic.NoticeList.ArticleList.Clear();
            Task.Run(() =>
            {
                ViewModelLocator.MainStatic.NoticeList.load_more();
            });
        }

        private void ApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            var t = MessageBox.Show("确认清除所有消息？\n\n消息历史记录可以在电脑版果壳网站上找到\n（http://www.guokr.com/user/notice/）", "清除消息", MessageBoxButton.OKCancel);
            if (t == MessageBoxResult.OK || t == MessageBoxResult.Yes)
                ViewModelLocator.MainStatic.ResetRNNumber.Execute(null);
        }
#if false
        private void _ChannelLoadFailure(ChannelLoadFailureMessage a)
        {
            //ArticleList.ListFooterTemplate = Application.Current.Resources["FailedFooterTamplate"] as DataTemplate;
        }
#endif
    }
}