using System;
using System.Net;
using System.Threading.Tasks;
using RestSharp;
using SanzaiGuokr.Model;
using SanzaiGuokr.Util;
using SanzaiGuokr.ViewModel;
using System.Collections.Generic;

namespace SanzaiGuokr.SinaApiV2
{
    public class SinaApiV2 : ApiClassBase
    {
        private static RestClient _c;
        public static RestClient Client
        {
            get
            {
                if (_c == null)
                    _c = new RestClient(SinaApiConfig.SinaBaseUrl);
                return _c;
            }
        }
        private static RestClient _uc;
        public static RestClient UploadClient
        {
            get
            {
                if (_uc == null)
                    _uc = new RestClient(SinaApiConfig.SinaBaseUrl);
                return _uc;
            }
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

        public static async Task<List<WeiboApi.status>> MrGuokrHomeTimeline()
        {
            var req = GetRequest();
            req.Resource = "2/statuses/home_timeline.json";
            req.Method = Method.GET;
            req.Parameters.Add(new Parameter() { Name = "access_token", Value = ViewModelLocator.ApplicationSettingsStatic.MrGuokrSinaLogin.access_token, Type = ParameterType.GetOrPost });
            req.Parameters.Add(new Parameter() { Name = "count", Value = 30, Type = ParameterType.GetOrPost });
            req.Parameters.Add(new Parameter() { Name = "Accept-Encoding", Value = "gzip", Type = ParameterType.HttpHeader });

            var r = await CallAPI<WeiboResponse>(req, ViewModelLocator.ApplicationSettingsStatic.MrGuokrSinaLogin);
            return r.Statuses;
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

        private static Task<TResponse> CallAPI<TResponse>(RestRequest req, SinaLogin login = null) where TResponse : new()
        {
            return CallAPI<TResponse>(Client, req, login);
        }

        private static async Task<TResponse> CallAPI<TResponse>(RestClient c, RestRequest req, SinaLogin login = null) where TResponse : new()
        {
            if (login == null)
                login = ViewModelLocator.ApplicationSettingsStatic.WeiboAccountSinaLogin;
            if (login != null && login.IsValid)
            {
                var token = ViewModelLocator.ApplicationSettingsStatic.WeiboAccountAccessToken;
                req.AddParameter(new Parameter() { Name = "access_token", Value = token, Type = ParameterType.GetOrPost });
                var response = await RestSharpAsync.RestSharpExecuteAsyncTask<TResponse>(c, req);
                if (response == null)
                    throw new SinaWeiboException() { error = "No Response" };
                ProcessError<SinaWeiboException>(response);
                if (response.Data == null)
                    throw new SinaWeiboException() { error = response.Content };
                if (response.StatusCode != HttpStatusCode.OK)
                    throw new SinaWeiboException() { error = "Status = " + response.StatusCode.ToString() };
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
        public string username { get; set; }

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
        // sanzaiguokr #1
        public static string app_key = "1313825017";
        public static string app_secret = "f1966c10f54df2efaff97b04ee82bf1a";

        // sanzaiguokr #2
        //public static string app_key = "1985727276";
        //public static string app_secret = "222e579aad8738eebe5878cbc2cb6a98";

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
        public override string GetErrorMessage()
        {
            return error;
        }
    }


}
