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
using Microsoft.Phone.Shell;
using SanzaiGuokr.Util;

namespace SanzaiGuokr
{
    public partial class ViewChannel : PhoneApplicationPage
    {
        public ViewChannel()
        {
            InitializeComponent();
#if PIPROFILING
            var pi = new ProgressIndicator();
            pi.IsVisible = true;
            SystemTray.SetProgressIndicator(this, pi);
            Messenger.Default.Register<SetProgressIndicator>(this, (a) => Common.ProcessProgressIndicator(SystemTray.GetProgressIndicator(this), a));
#endif

            Loaded += ViewChannel_Loaded;
        }

        void ViewChannel_Loaded(object sender, RoutedEventArgs e)
        {
            SystemTray.IsVisible = true;
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