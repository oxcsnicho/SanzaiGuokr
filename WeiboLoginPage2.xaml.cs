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
using SanzaiGuokr.ViewModel;
using SanzaiGuokr.SinaApiV2;

namespace SanzaiWeibo
{
    
    public partial class LoginPage2 : PhoneApplicationPage
    {
        private SinaApi oAuth = SinaApi.base_oauth;
        // Constructor
        public LoginPage2()
        {
            InitializeComponent();
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            loginWebBrowser.LoadCompleted += (ss, ee) => progressBar.IsIndeterminate = false;
            loginWebBrowser.Navigating += (ss, ee) => progressBar.IsIndeterminate = true;
            loginWebBrowser.Navigating += new EventHandler<NavigatingEventArgs>(loginWebBrowser_Navigating);
            loginWebBrowser.ScriptNotify += new EventHandler<NotifyEventArgs>(loginWebBrowser_ScriptNotify);

            loginWebBrowser.Navigate(new Uri("https://api.weibo.com/oauth2/authorize?client_id=" +
                SinaApi.base_oauth.app_key + "&response_type=code&redirect_uri=www.google.com&display=wap2.0"));
#if FALSE
            textBox1.Text = "lyang.nicholas@gmail.com";
            passwordBox1.Password = "nicholas";
#endif

        }

        private  void loginWebBrowser_ScriptNotify(object sender, NotifyEventArgs e)
        {
            string str = e.Value;
        }

        private async void loginWebBrowser_Navigating(object sender, NavigatingEventArgs e)
        {
            Match match = Regex.Match(e.Uri.Query,@"\?code=([\w\d]+)");
            if(match.Success == false)
                return;

            string code = match.Groups[1].Value;
            e.Cancel = true;
            progressBar.IsIndeterminate = true;

            var client = new WebClient();
            //client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
            try
            {
                JsonDeserializer J = new JsonDeserializer();

                string data = await client.UploadStringTaskAsync(String.Format(
                "https://api.weibo.com/oauth2/access_token?client_id={0}&client_secret={1}&grant_type=authorization_code&redirect_uri=www.google.com&code={2}",
                SinaApi.base_oauth.app_key,
                SinaApi.base_oauth.app_secret,
                code), "");
                var response = await TaskEx.Run(() => {
                    var res = J.Deserialize<SinaLogin>(data);
                    ViewModelLocator.ApplicationSettingsStatic.SetupWeiboAccount(true, "", res.access_token);
                    return res;
                });

                data = await client.DownloadStringTaskAsync
                    (string.Format("https://api.weibo.com/2/users/show.json?access_token={0}&uid={1}", response.access_token, response.uid));
                user u = J.Deserialize<user>(data);
                ViewModelLocator.ApplicationSettingsStatic.WeiboAccountProfile = u;

                MessageBox.Show(string.Format("{0} 登录成功", u.name));
                if(NavigationService.CanGoBack)
                    NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show("貌似有点问题，重来一次吧～");
                Debug.WriteLine(ex.InnerException.ToString());
            }
        }

    }
}