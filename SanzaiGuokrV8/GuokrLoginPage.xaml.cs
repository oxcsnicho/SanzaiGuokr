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
using SanzaiGuokr.GuokrApiV2;

namespace SanzaiGuokr
{

    public partial class GuokrLoginPage : PhoneApplicationPage
    {
        const int client_id = 32380;
        Uri login_uri = new Uri("https://account.guokr.com/oauth2/authorize/?"
                    + "response_type=code"
                    + "&client_id=" + client_id
                    + "&redirect_uri=http://www.guokr.com/mobile-loading.html"
                    + "&display=mobile"
            //+ "&state=123123"
            //+ "&suppress_prompt=true"
                    );
        // Constructor
        public GuokrLoginPage()
        {
            InitializeComponent();
            Loaded += new RoutedEventHandler(GuokrLoginPage_Loaded);

#if false
            usernameBox.Text = "oxcsnicho@gmail.com";
            passwordBox.Password = "nicholas";
            usernameBox.ItemsSource = candidates;
#endif
        }

        void GuokrLoginPage_Loaded(object sender, RoutedEventArgs e)
        {
            login_wb.Navigate(login_uri);
            VisualStateManager.GoToState(this, "Normal", false);

            login_wb.Navigating += login_wb_Navigating;
            login_wb.LoadCompleted += (ss, ee) => progressBar.Visibility = System.Windows.Visibility.Collapsed;
            login_wb.Navigating += (ss, ee) => progressBar.Visibility = System.Windows.Visibility.Visible;
        }

        async void login_wb_Navigating(object sender, NavigatingEventArgs e)
        {
            try
            {
                if (e.Uri.Host == "www.guokr.com" && e.Uri.AbsolutePath == "/mobile-loading.html")
                {
                    VisualStateManager.GoToState(this, "Disabled", false);
                    var code = Common.ParseQueryString(e.Uri.Query)["code"];

                    var c = new RestClient("https://account.guokr.com");
                    var r = new RestRequest(Method.POST);
                    r.Resource = "/oauth2/token/";
                    r.AddParameter(new Parameter() { Name = "grant_type", Value = "authorization_code", Type = ParameterType.GetOrPost });
                    r.AddParameter(new Parameter() { Name = "client_id", Value = "32380", Type = ParameterType.GetOrPost });
                    r.AddParameter(new Parameter() { Name = "redirect_uri", Value = "http://www.guokr.com/mobile-loading.html", Type = ParameterType.GetOrPost });
                    r.AddParameter(new Parameter() { Name = "client_secret", Value = "9b4565d2b40ad9c3d61e42437d1e257d736795ab", Type = ParameterType.GetOrPost });
                    r.AddParameter(new Parameter() { Name = "code", Value = code, Type = ParameterType.GetOrPost });

                    var resp = await RestSharpAsync.RestSharpExecuteAsyncTask<GuokrOauthTokenResponse>(c, r);
                    if (resp.StatusCode.HasFlag(HttpStatusCode.BadRequest) || resp.StatusCode.HasFlag(HttpStatusCode.InternalServerError))
                        throw new WebException();

                    ViewModelLocator.ApplicationSettingsStatic.GuokrAccountProfile = new GuokrUserLogin()
                    {
                        access_token = resp.Data.access_token,
                        nickname = resp.Data.nickname,
                        refresh_token = resp.Data.refresh_token,
                        expire_dt = DateTime.Now.AddSeconds(resp.Data.expires_in),
                        ukey = resp.Data.ukey
                    };
                    MessageBox.Show(ViewModelLocator.ApplicationSettingsStatic.GuokrAccountName + " 登录成功");
                    VisualStateManager.GoToState(this, "Normal", false);
                    if (NavigationService.CanGoBack)
                        NavigationService.GoBack();
                }
                else if (e.Uri.Host == "account.guokr.com" && e.Uri.AbsolutePath == "/sign_in/" && !e.Uri.Query.Contains("display=mobile"))
                {
                    e.Cancel = true;
                    login_wb.Navigate(new Uri("https://account.guokr.com/sign_in/" + e.Uri.Query + "&display=mobile"));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("貌似有点问题，重来一次吧～");
                Debug.WriteLine(ex.InnerException.ToString());
            }
        }

#if false
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
            if (!usernameBox.Text.Contains("@"))
            {
                MessageBox.Show("登录名应使用邮箱帐号");
                return;
            }

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
#endif

    }

}