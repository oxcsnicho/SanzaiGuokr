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
            string token;
            var c = new RestClient("http://m.guokr.com");

            try
            {
                {
                    // get the token
                    var req = new RestRequest();
                    req.Resource = "/api/userinfo/get_token/";
                    req.Method = Method.POST;
                    req.RequestFormat = DataFormat.Json;
                    req.OnBeforeDeserialization = resp =>
                    {
                        resp.ContentType = "application/json";
                    };
                    req.Parameters.Add(new Parameter() { Name = "username", Value = usernameBox.Text, Type = ParameterType.GetOrPost });
                    var response = await RestSharpAsync<GuokrUserToken>.RestSharpExecuteAsyncTask(c, req);
                    if (response.Data == null)
                        throw new WebException();
                    token = response.Data.token;
                }

                // encode password
                string userToken = "";
                string encodedPassword = "";
                GuokrAuth.encodePassword(usernameBox.Text, passwordBox.Password, token, out encodedPassword, out userToken);

                {
                    // login and get cookie
                    var req = new RestRequest();
                    req.Resource = "/api/userinfo/login/";
                    req.Method = Method.POST;
                    req.RequestFormat = DataFormat.Json;
                    req.OnBeforeDeserialization = resp =>
                    {
                        resp.ContentType = "application/json";
                    };
                    req.Parameters.Add(new Parameter() { Name = "username", Value = usernameBox.Text, Type = ParameterType.GetOrPost });
                    req.Parameters.Add(new Parameter() { Name = "sspassword", Value = encodedPassword, Type = ParameterType.GetOrPost });
                    req.Parameters.Add(new Parameter() { Name = "susertoken", Value = userToken, Type = ParameterType.GetOrPost });
                    req.Parameters.Add(new Parameter() { Name = "remember", Value = true, Type = ParameterType.GetOrPost });
                    var response = await RestSharpAsync<GuokrUserInfo>.RestSharpExecuteAsyncTask(c, req);
                    if (response.Data == null)
                        throw new WebException();
                    ViewModelLocator.ApplicationSettingsStatic.GuokrAccountProfile = response.Data;

                    // refactor this part
                    var g = new GuokrCookie();
                    g.Cookies = (List<RestResponseCookie>)response.Cookies;
                    ViewModelLocator.ApplicationSettingsStatic.GuokrAccountCookie = g;
                }

                MessageBox.Show(ViewModelLocator.ApplicationSettingsStatic.GuokrAccountName + " 登录成功");
                VisualStateManager.GoToState(this, "Normal", false);
                if (NavigationService.CanGoBack)
                    NavigationService.GoBack();
            }
            catch (Exception)
            {
                MessageBox.Show("有问题");
            }
        }
    }

}