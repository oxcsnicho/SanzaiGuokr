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
using SanzaiGuokr.Util;
using SanzaiGuokr.ViewModel;

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
                a = () => appsettings.WeiboAccountSinaLogin = null;
            }
            else
            {
                status = appsettings.GuokrAccountLoginStatus;
                uri = "/GuokrLoginPage.xaml";
                a = () => appsettings.GuokrAccountProfile = null;
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

        private void DebugModeOptions_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModelLocator.ApplicationSettingsStatic.DebugMode)
            {
                MessageBox.Show(
                @"Debug模式用于收集使用信息，发给尼姑，供其修bug用：） 使用方法：
1. 打开debug模式
2. 程序会开始监控。这时开始去干你想干的事情，信息会被记录
3. 完成后，回到这里关闭debug模式
4. 把信息发给尼姑", "Debug模式使用方法", MessageBoxButton.OK);
            }
            else
            {
                var res = MessageBox.Show("确认发送收集的信息？", "", MessageBoxButton.OKCancel);
                if (res == MessageBoxResult.Yes || res == MessageBoxResult.OK)
                {
                    EmailComposeTask t = new EmailComposeTask();
                    t.To = "sanzaiweibo@gmail.com";
                    t.Subject = "山寨果壳错误报告 - " + DateTime.Now.ToString() + " - " + ViewModelLocator.ApplicationSettingsStatic.AnonymousUserId;
                    t.Body = "请在此输入问题描述:\n\n\n" + DebugLogging.Flush();
                    t.Show();
                }
                DebugLogging.Clear();
            }

        }

        private void EnableGroup_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModelLocator.ApplicationSettingsStatic.IsGroupEnabledSettingBool)
            {
                if (!ViewModelLocator.ApplicationSettingsStatic.GuokrAccountLoginStatus)
                {
                    MessageBox.Show(
                    @"注意哦，要看小组的话需要
1. 登录果壳
2. 在果壳网站上加好小组
不然会load不出来哦~", "小组使用方法", MessageBoxButton.OK);

                    NavigationService.Navigate(new Uri("/GuokrLoginPage.xaml", UriKind.Relative));
                }
            }

        }

    }
}