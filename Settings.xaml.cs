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
            var appsettings = ViewModel.ViewModelLocator.ApplicationSettingsStatic;
            if (appsettings.WeiboAccountLoginStatus)
            {
                var res = MessageBox.Show("确认登出?", "确认登出? 这会清除你的微薄登录数据 (立即生效)", MessageBoxButton.OKCancel);
                switch (res)
                {
                    case MessageBoxResult.Cancel:
                    case MessageBoxResult.No:
                    case MessageBoxResult.None:
                        break;
                    case MessageBoxResult.OK:
                    case MessageBoxResult.Yes:
                        appsettings.SetupWeiboAccount(false, "", "");
                        break;
                    default:
                        break;
                }
            }
            else
            {
                NavigationService.Navigate(new Uri("/WeiboLoginPage.xaml", UriKind.Relative));
            }
        }
    }
}