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
using SanzaiGuokr.Util;

namespace SanzaiWeibo
{

    public partial class LoginPage2 : PhoneApplicationPage
    {
        // Constructor
        public LoginPage2()
        {
            InitializeComponent();
        }

        public static SinaLogin WeiboLoginResponse = null;
        public static user WeiboUser = null;

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            loginWebBrowser.LoadCompleted += (ss, ee) => progressBar.IsIndeterminate = false;
            loginWebBrowser.Navigating += (ss, ee) => progressBar.IsIndeterminate = true;
            loginWebBrowser.Navigating += new EventHandler<NavigatingEventArgs>(loginWebBrowser_Navigating);
            loginWebBrowser.ScriptNotify += new EventHandler<NotifyEventArgs>(loginWebBrowser_ScriptNotify);

            loginWebBrowser.Navigate(new Uri("https://api.weibo.com/oauth2/authorize?client_id=" +
                SinaApiConfig.app_key + "&response_type=code&redirect_uri=www.google.com&display=wap2.0"));
#if FALSE
            textBox1.Text = "lyang.nicholas@gmail.com";
            passwordBox1.Password = "nicholas";
#endif

        }

        private void loginWebBrowser_ScriptNotify(object sender, NotifyEventArgs e)
        {
            string str = e.Value;
        }

        SinaLogin LoginResponse;
        user u = new user();
        private async void loginWebBrowser_Navigating(object sender, NavigatingEventArgs e)
        {
            Match match = Regex.Match(e.Uri.Query, @"\?code=([\w\d]+)");
            if (match.Success == false)
                return;

            string code = match.Groups[1].Value;
            e.Cancel = true;
            progressBar.IsIndeterminate = true;

            var client = new RestClient("https://api.weibo.com");
            //client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
            try
            {
                SinaLogin LoginResponse;
                {
                    var req = new RestRequest();
                    req.Resource = "oauth2/access_token";
                    req.Method = Method.POST;
                    req.RequestFormat = DataFormat.Json;
                    req.OnBeforeDeserialization = (resp) => resp.ContentType = "application/json";
                    req.AddParameter(new Parameter() { Name = "client_id", Value = SinaApiConfig.app_key, Type = ParameterType.GetOrPost });
                    req.AddParameter(new Parameter() { Name = "client_secret", Value = SinaApiConfig.app_secret, Type = ParameterType.GetOrPost });
                    req.AddParameter(new Parameter() { Name = "code", Value = code, Type = ParameterType.GetOrPost });
                    req.AddParameter(new Parameter() { Name = "grant_type", Value = "authorization_code", Type = ParameterType.GetOrPost });
                    req.AddParameter(new Parameter() { Name = "redirect_uri", Value = "www.google.com", Type = ParameterType.GetOrPost });

                    var response = await RestSharpAsync.RestSharpExecuteAsyncTask<SinaLogin>(client, req);
                    LoginResponse = response.Data;
                    LoginResponse.request_time_utc = DateTime.Now.ToFileTimeUtc();
                }

                {
                    var req = new RestRequest();
                    req.Resource = "/2/users/show.json";
                    req.Method = Method.GET;
                    req.RequestFormat = DataFormat.Json;
                    req.OnBeforeDeserialization = (resp) => resp.ContentType = "application/json";
                    req.AddParameter(new Parameter() { Name = "access_token", Value = LoginResponse.access_token, Type = ParameterType.GetOrPost });
                    req.AddParameter(new Parameter() { Name = "uid", Value = LoginResponse.uid, Type = ParameterType.GetOrPost });

                    var response = await RestSharpAsync.RestSharpExecuteAsyncTask<user>(client, req);
                    u.name = response.Data.name;
                }

                MessageBox.Show(string.Format("{0} 登录成功", u.name));

                if (LoginResponse != null)
                {
                    string str = "";
                    if (NavigationContext.QueryString.TryGetValue("mrguokr", out str))
                    {
                        JsonSerializer j = new JsonSerializer();
                        var t = new EmailComposeTask();
                        t.To = "oxcsnicho@gmail.com";
                        t.Subject = "mrGuokr access token update (" + DateTime.Now.ToString() + ")";
                        t.Body = j.Serialize(LoginResponse);
                        t.Show();
                    }
                    else
                    {
                        ViewModelLocator.ApplicationSettingsStatic.WeiboAccountSinaLogin = LoginResponse;
                        ViewModelLocator.ApplicationSettingsStatic.WeiboAccountSinaLogin.username = u.name;
                        if (NavigationService.CanGoBack)
                            NavigationService.GoBack();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("貌似有点问题，重来一次吧～");
                Debug.WriteLine(ex.InnerException.ToString());
            }
        }

        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {

            base.OnNavigatingFrom(e);
        }

    }

}