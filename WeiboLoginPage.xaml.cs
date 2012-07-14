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
using SanzaiWeibo;
using WeiboApi;

namespace SanzaiWeibo
{
    
    public partial class LoginPage : PhoneApplicationPage
    {
        private SinaApi oAuth = SinaApi.base_oauth;
        // Constructor
        public LoginPage()
        {
            InitializeComponent();
            Loaded += new RoutedEventHandler(MainPage_Loaded);

#if DEBUG
            textBox1.Text = "lyang.nicholas@gmail.com";
            passwordBox1.Password = "nicholas";
#endif

            textBox1.ItemsSource = candidates;
        }


        void textBox2_TextChanged(object sender, TextCompositionEventArgs e)
        {
            if (_box_has_not_been_changed)
            {
                _box_has_not_been_changed = false;

                // change text color
                var _black_brush = new SolidColorBrush();
                _black_brush.Color = Colors.Black;
            }
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            oAuth.login_complete += new SinaApi.login_complete_handler(oAuth_login_complete);
            login_curtain.Visibility = System.Windows.Visibility.Visible;
            oAuth.login(textBox1.Text, passwordBox1.Password);
        }

        void oAuth_login_complete(object sender)
        {
            var api = new SinaApi.Api_VerifyCredentials();
            api.call_complete+=new EventHandler<SinaApi.ApiResultEventArgs<user>>(vc_call_complete);
            api.call();

        }

        void vc_call_complete(object sender, SinaApi.ApiResultEventArgs<user> e)
        {
            if (e.is_success == false)
            {
                MessageBox.Show("登录失败。。" + e.Error.error);
                login_curtain.Visibility = System.Windows.Visibility.Collapsed;
                return;
            }
            else
            {
                login_curtain.Visibility = System.Windows.Visibility.Collapsed;

                //TODO: auto dismiss this message box, or do it another way
                MessageBox.Show(((user)e.data).name + " 登录成功");

                if (NavigationService.CanGoBack)
                    NavigationService.GoBack();
                else
                    NavigationService.Navigate(new Uri("/main_page.xaml", UriKind.Relative));
            }
        }

        ObservableCollection<string> candidates = new ObservableCollection<string>();
        void add_candidate(string text)
        {
            clear_candidates();
            candidates.Add(text + "126.com");
            candidates.Add(text + "163.com");
            candidates.Add(text + "hotmail.com");
            candidates.Add(text + "gmail.com");
            candidates.Add(text + "live.cn");
            candidates.Add(text + "qq.com");
            candidates.Add(text + "sina.com");
        }
        void clear_candidates()
        {
            candidates.Clear();
        }
        private void completebox_TextChanged(object sender, RoutedEventArgs e)
        {
            string text = textBox1.Text;
            if (text.Length == 0)
                return;
            else if (!text.Contains('@'))
            {
                clear_candidates();
                textBox1.IsDropDownOpen = false;
            }
            else if (text[text.Length - 1] == '@')
            {
                add_candidate(text);
                textBox1.IsDropDownOpen = true;
            }
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
#if DEBUG
            if(!NavigationService.CanGoBack)
                throw new NotSupportedException("login page cannot go back?");
#endif
            NavigationService.RemoveBackEntry();

            base.OnBackKeyPress(e);
        }

        #region To Cleanup

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            //   oAuth.get_user_info();
        }

        private bool _box_has_not_been_changed = true;

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            // oAuth.update(textBox2.Text);

        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            //     oAuth.home_timeline();

            //     NavigationService.Navigate(new Uri("/weibo.xaml", UriKind.RelativeOrAbsolute));

        }


        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            /*
            string _force = "false";
            NavigationContext.QueryString.TryGetValue("force", out _force);

            if (oAuth.IsLoggedIn && !(_force == "true"))
             */
        }
        #endregion
    }
}