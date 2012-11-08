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

namespace SanzaiGuokr
{
    public partial class Settings : PhoneApplicationPage
    {
        public Settings()
        {
            InitializeComponent();
        }

        private void ClearButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var button = sender as Button;
            if (button == null)
                return;

            var appsettings = ViewModel.ViewModelLocator.ApplicationSettingsStatic;
            bool status;
            string uri;
            Action a;
            if (button.Name == "ClearButton")
            {
                status = appsettings.WeiboAccountLoginStatus;
                uri = "/WeiboLoginPage2.xaml";
                a = () =>
                    {
                        appsettings.WeiboAccountSinaLogin = null;
                        appsettings.WeiboAccountProfile = null;
                    };
            }
            else
            {
                status = appsettings.GuokrAccountLoginStatus;
                uri = "/GuokrLoginPage.xaml";
                a = () =>
                    {
                        appsettings.GuokrAccountCookie = null;
                        appsettings.GuokrAccountProfile = null;
                    };
            }
            if (status)
            {
                var res = MessageBox.Show("确认登出? 这会清除你的登录数据 (立即生效)", "确认登出", MessageBoxButton.OKCancel);
                switch (res)
                {
                    case MessageBoxResult.Cancel:
                    case MessageBoxResult.No:
                    case MessageBoxResult.None:
                        break;
                    case MessageBoxResult.OK:
                    case MessageBoxResult.Yes:
                        a.Invoke();
                        break;
                    default:
                        break;
                }
            }
            else
            {
                NavigationService.Navigate(new Uri(uri, UriKind.Relative));
            }
        }

        private void refreshMrGuokrToken_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/WeiboLoginPage2.xaml?mrguokr=true", UriKind.Relative));
        }

    }
}