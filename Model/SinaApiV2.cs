using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using RestSharp;
using System.Threading.Tasks;
using SanzaiGuokr.Util;
using SanzaiGuokr.ViewModel;
using SanzaiGuokr.Model;
using System.Collections.Generic;

namespace SanzaiGuokr.SinaApiV2
{
    public class SinaApiV2
    {
        private static RestClient _c;
        public static RestClient Client
        {
            get {
                if (_c == null)
                    _c = new RestClient(SinaApiConfig.SinaBaseUrl);
                return _c; }
        }
        private static RestClient _uc;
        public static RestClient UploadClient
        {
            get {
                if (_uc == null)
                    _uc = new RestClient(SinaApiConfig.SinaBaseUrl);
                return _uc; }
        }
        static RestRequest GetRequest()
        {
            return new RestRequest()
            {
                RequestFormat = DataFormat.Json,
                OnBeforeDeserialization = resp =>
                {
                    resp.ContentType = "application/json";
                }
            };
        }

        public static async Task<List<WeiboApi.status>> HomeTimeline()
        {
            var req = GetRequest();
            req.Resource = "2/statuses/home_timeline.json";
            req.Method = Method.GET;
            req.Parameters.Add(new Parameter() { Name = "access_token", Value = ViewModelLocator.ApplicationSettingsStatic.MrGuokrSinaLogin.access_token, Type=ParameterType.GetOrPost });
            //req.Parameters.Add(new Parameter() { Name = "screen_name", Value = "果壳网", Type = ParameterType.GetOrPost });
            req.Parameters.Add(new Parameter() { Name = "count", Value = 30, Type = ParameterType.GetOrPost });
            //req.Parameters.Add(new Parameter() { Name = "trim_user", Value = 1, Type = ParameterType.GetOrPost });
            return await CallAPI<List<WeiboApi.status>>(req);
        }

        public static async Task<WeiboApi.status> PostWeibo(string s)
        {
            var req = GetRequest();
            req.Resource = "/2/statuses/update.json";
            req.Method = Method.POST;
            req.AddParameter(new Parameter() { Name = "status", Value = s, Type = ParameterType.GetOrPost });
            return await CallAPI<WeiboApi.status>(req);
        }

        public static async Task<WeiboApi.status> PostWeibo(string s, byte[] pic)
        {
            var req = GetRequest();
            req.Resource = "/2/statuses/upload.json";
            req.Method = Method.POST;
            req.AddParameter(new Parameter() { Name = "status", Value = s, Type = ParameterType.GetOrPost });
            req.AddFile("pic", pic, "filename");
            return await CallAPI<WeiboApi.status>(req);
        }

        public static async Task<WeiboApi.status> RepostWeibo(string s, WeiboApi.status a, bool is_comment = false)
        {
            var req = GetRequest();
            req.Resource = "/2/statuses/repost.json";
            req.Method = Method.POST;
            req.AddParameter(new Parameter() { Name = "status", Value = s, Type = ParameterType.GetOrPost });
            req.AddParameter(new Parameter() { Name = "id", Value = a.id, Type = ParameterType.GetOrPost });
            if (is_comment)
                req.AddParameter(new Parameter() { Name = "is_comment", Value = 1, Type = ParameterType.GetOrPost });
            return await CallAPI<WeiboApi.status>(req);
        }

        private static Task<TResponse> CallAPI<TResponse>(RestRequest req) where TResponse : new()
        {
            return CallAPI<TResponse>(Client, req);
        }

        private static async Task<TResponse> CallAPI<TResponse>(RestClient c, RestRequest req) where TResponse : new()
        {
            if(ViewModelLocator.ApplicationSettingsStatic.WeiboAccountLoginStatus)
            {
                var token = ViewModelLocator.ApplicationSettingsStatic.WeiboAccountAccessToken;
                req.AddParameter(new Parameter() { Name = "access_token", Value = token, Type = ParameterType.GetOrPost });
                var response = await RestSharpAsync.RestSharpExecuteAsyncTask<TResponse>(c, req);
                if (response == null)
                    throw new SinaWeiboException() { error = "No Response" };
                if (response.Data == null)
                    throw new SinaWeiboException() { error = response.Content };
                if(response.StatusCode != HttpStatusCode.OK)
                    throw new SinaWeiboException() { error = "Status = "+response.StatusCode.ToString() };
                return response.Data;
            }
            else
            {
                throw new SinaWeiboException() { error = "未登录" };
            }
        }
    }
    public class SinaLogin
    {
        public string access_token { get; set; }
        public long expires_in { get; set; }
        public long uid { get; set; }
        public long request_time_utc { get; set; }

        public SinaLogin()
        {
            access_token = "";
        }

        public bool IsValid
        {
            get
            {
                return access_token != "" && DateTime.Now <= RequestDateTime.AddSeconds(expires_in);
            }
        }

        public DateTime RequestDateTime
        {
            get
            {
                return DateTime.FromFileTimeUtc(request_time_utc).ToLocalTime();
            }
        }

    }
    public class SinaApiConfig
    {
        public static string app_key = "1313825017";
        public static string app_secret = "f1966c10f54df2efaff97b04ee82bf1a";
        public static string StanfordLocation = "http://ccrma.stanford.edu/~darkowen/temp/temp";
        public static string StanfordLocationBaseUrl = "http://ccrma.stanford.edu";
        public static string StanfordLocationResource = "/~darkowen/temp/temp";
        public static string SinaBaseUrl = "https://api.weibo.com";
        public static string SinaUploadBaseUrl = "https://upload.api.weibo.com";

        SinaLogin _login;
        private SinaApiConfig(SinaLogin l)
        {
            _login = l;
        }

    }
    public class SinaWeiboException : MyException
    {
        public int error_code { get; set; }
        public string error { get; set; }
        public string request { get; set; }

        public override int GetErrorCode()
        {
            return error_code;
        }
    }


}
