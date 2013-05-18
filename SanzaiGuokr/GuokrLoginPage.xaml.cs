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
using System.Collections.ObjectModel;
using WeiboApi;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using RestSharp;
using RestSharp.Serializers;
using RestSharp.Deserializers;
using System.Diagnostics;
using SanzaiGuokr.SinaApiV2;
using SanzaiGuokr.ViewModel;
using Microsoft.Phone.Tasks;
using System.Text;
using SanzaiGuokr.Util;
using SanzaiGuokr.GuokrObject;
using SanzaiGuokr.Model;

namespace SanzaiGuokr
{

    public partial class GuokrLoginPage : PhoneApplicationPage
    {
        // Constructor
        public GuokrLoginPage()
        {
            InitializeComponent();
            Loaded += new RoutedEventHandler(GuokrLoginPage_Loaded);

#if DEBUG
            usernameBox.Text = "oxcsnicho@gmail.com";
            passwordBox.Password = "nicholas";
#endif
            usernameBox.ItemsSource = candidates;
        }

        void GuokrLoginPage_Loaded(object sender, RoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "Normal", false);
        }

        ObservableCollection<string> candidates = new ObservableCollection<string>();
        void add_candidate(string text)
        {
            clear_candidates();
            candidates.Add(text + "hotmail.com");
            candidates.Add(text + "gmail.com");
            candidates.Add(text + "qq.com");
        }
        void clear_candidates()
        {
            candidates.Clear();
        }
        private void usernameBox_TextChanged(object sender, RoutedEventArgs e)
        {
            string text = usernameBox.Text;
            if (text.Length == 0)
                return;
            else if (!text.Contains('@'))
            {
                clear_candidates();
                usernameBox.IsDropDownOpen = false;
            }
            else if (text[text.Length - 1] == '@')
            {
                add_candidate(text);
                usernameBox.IsDropDownOpen = true;
            }
        }

        private async void confirmButton_Click(object sender, RoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "Disabled", false);

            try
            {
                await GuokrApi.VerifyAccountV2(usernameBox.Text, passwordBox.Password);
                MessageBox.Show(ViewModelLocator.ApplicationSettingsStatic.GuokrAccountName + " 登录成功");
                VisualStateManager.GoToState(this, "Normal", false);
                if (NavigationService.CanGoBack)
                    NavigationService.GoBack();
            }
            catch (GuokrException ge)
            {
                if (ge.errnum == GuokrErrorCode.VerificationFailed)
                {
                    MessageBox.Show("用户名密码不对哦，注意大小写\n╮(▔▽▔\")╭");
                    VisualStateManager.GoToState(this, "Normal", false);
                }
                else
                {
                    MessageBox.Show("囧了。错误代码："+ge.errmsg);
                    VisualStateManager.GoToState(this, "Normal", false);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("有问题");
                Debug.WriteLine(ex.InnerException.Message);
            }
        }
    }

}