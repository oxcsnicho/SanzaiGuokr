/*
 created by oxcsnicho
 */

using System;
using System.Linq;
using System.Net;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using RestSharp;
using SanzaiWeibo;
using Microsoft.Phone.Controls;
using System.Windows;
using Microsoft.Phone.Shell;
using System.Windows.Threading;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media;
using System.Text.RegularExpressions;
using System.Windows.Documents;
using RestSharp.Deserializers;
using SanzaiGuokr.ViewModel;

namespace WeiboApi
{

    public class SinaApi
    {
        public RestClient restClient = new RestClient();
        protected string base_url = "http://api.t.sina.com.cn/";
        static ApplicationSettingsViewModel ApplicationSettings = ViewModelLocator.ApplicationSettingsStatic;

        #region constants

        // singleton for the oauth base
        protected SinaApi()
        {
            restClient.BaseUrl = base_url;
            /*
                        var resources = new ResourceDictionary() { Source = new Uri("/Resources/ApplicationResource.resx", UriKind.Relative) };
                        app_key = (string)resources["app_key"];
                        app_secret = (string)resources["app_secret"];
             * */
            app_key = "1313825017";
            app_secret = "f1966c10f54df2efaff97b04ee82bf1a";

            if(ViewModelLocator.ApplicationSettingsStatic.WeiboAccountLoginStatus)
            {
                try
                {
                    oauth_token = ApplicationSettings.WeiboAccountAuthToken;
                    access_token = ApplicationSettings.WeiboAccountAccessToken;
                }
                catch
                { }
            }
        }
        private static SinaApi _oauthbase = null;
        public static SinaApi base_oauth
        {
            get
            {
                if (_oauthbase == null)
                    _oauthbase = new SinaApi();
                return _oauthbase;
            }
        }

        protected const string OAuthVersion = "1.0";
        protected const string SignatureMethodName = "HMAC-SHA1";

        // List of know and used oauth parameters' names
        protected const string OAuthConsumerKeyKey = "oauth_consumer_key"; // APP KEY
        protected const string OAuthCallbackKey = "oauth_callback";
        protected const string OAuthVersionKey = "oauth_version";
        protected const string OAuthSignatureMethodKey = "oauth_signature_method";
        protected const string OAuthSignatureKey = "oauth_signature";
        protected const string OAuthTimestampKey = "oauth_timestamp";
        protected const string OAuthNonceKey = "oauth_nonce";
        protected const string OAuthTokenKey = "oauth_token";
        protected const string oAauthVerifier = "oauth_verifier";
        protected const string OAuthTokenSecretKey = "oauth_token_secret";

        protected Random random = new Random();

        public string app_key;
        public string app_secret;
        protected string oauth_token;

        protected string oauth_token_secret;
        protected string oAuthTokenSecret
        {
            set
            {
                oauth_token_secret = value;
            }
        }

        protected string oauth_verifier;
        protected string access_token;


        #endregion

        #region utils

        internal void stuff_oauth_parameters(RestRequest r)
        {
            var ts = GenerateTimeStamp();
            var nonce = GenerateNonce();

            r.AddParameter("oauth_consumer_key", app_key);
            r.AddParameter("oauth_token", oauth_token);
            r.AddParameter("oauth_signature_method", SignatureMethodName);
            r.AddParameter("oauth_timestamp", ts);
            r.AddParameter("oauth_nonce", nonce);
            r.AddParameter("oauth_version", OAuthVersion);

        }

        internal void sign(ApiBase api, RestRequest r)
        {
            r.AddParameter("oauth_signature", GenerateSignature(api, r.Parameters));
        }
        /// <summary>
        /// Provides an internal structure to sort the query parameter
        /// </summary>
        public class QueryParameter
        {
            private string name = null;
            private string value = null;

            public QueryParameter(string name, string value)
            {
                this.name = name;
                this.value = value;
            }

            public QueryParameter(Parameter p)
            {
                this.name = p.Name;
                this.value = (string)p.Value;
            }

            public static implicit operator QueryParameter(Parameter p)
            {
                QueryParameter qp = new QueryParameter(p);
                return qp;
            }

            public string Name
            {
                get { return name; }
            }

            public string Value
            {
                get { return value; }
            }
        }
        // ??? how to specify implicit conversion

        /// <summary>
        /// Comparer class used to perform the sorting of the query parameters
        /// </summary>
        protected class ParameterComparer : IComparer<Parameter>
        {

            #region IComparer<QueryParameter> Members

            public int Compare(Parameter x, Parameter y)
            {
                if (x.Name == null || x.Value == null)
                    return -1;
                else if (y.Name == null || y.Value == null)
                    return 1;
                if (x.Name == y.Name)
                {
                    if (x.Value == y.Value)
                        return 0;
                    else
                        return string.Compare(x.Value.ToString(), y.Value.ToString()); // dangerous: just comparing the ToString value
                }
                else
                {
                    return string.Compare(x.Name, y.Name);
                }
            }

            #endregion
        }

        /// <summary>
        /// Internal function to cut out all non oauth query string parameters (all parameters not begining with "oauth_")
        /// </summary>
        /// <param name="parameters">The query string part of the Url</param>
        /// <returns>A list of QueryParameter each containing the parameter name and value</returns>
        public List<QueryParameter> GetQueryParameters(string parameters)
        {
            if (parameters.StartsWith("?"))
            {
                parameters = parameters.Remove(0, 1);
            }

            List<QueryParameter> result = new List<QueryParameter>();

            if (!string.IsNullOrEmpty(parameters))
            {
                string[] p = parameters.Split('&');
                foreach (string s in p)
                {
                    if (!string.IsNullOrEmpty(s))
                    {
                        if (s.IndexOf('=') > -1)
                        {
                            string[] temp = s.Split('=');
                            result.Add(new QueryParameter(temp[0], temp[1]));
                        }
                        else
                        {
                            result.Add(new QueryParameter(s, string.Empty));
                        }
                    }
                }
            }

            return result;
        }

        public string UrlEncode(string s)
        {
            char[] temp = System.Net.HttpUtility.UrlEncode(s).ToCharArray();

            // do upper case for encoding
            for (int i = 0; i < temp.Length - 2; i++)
            {
                if (temp[i] == '%')
                {
                    temp[i + 1] = char.ToUpper(temp[i + 1]);
                    temp[i + 2] = char.ToUpper(temp[i + 2]);
                }

            }

            // do sina wierd logic -- from android sdk
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < temp.Length; i++)
            {
                switch (temp[i])
                {
                    case '+': sb.Append("%20"); break;
                    case '*': sb.Append("%2A"); break;
                    case '!': sb.Append("%21"); break;
                    case '\'': sb.Append("%27"); break;
                    case '(': sb.Append("%28"); break;
                    case ')': sb.Append("%29"); break;
                    case '%':
                        if (i < temp.Length - 1 && temp[i + 1] == '7' && temp[i + 2] == 'E')
                        {
                            sb.Append('~');
                            i += 2;
                        }
                        else
                        {
                            sb.Append(temp[i]);
                        }
                        break;
                    default:
                        sb.Append(temp[i]);
                        break;
                }
            }
            return sb.ToString();
        }

        public string GenerateSignature(ApiBase api, List<QueryParameter> ps)
        {
            var p = new List<Parameter>();
            foreach (var pp in ps)
            {
                ps.Add(new Parameter() { Name = pp.Name, Value = pp.Value });
            }
            return GenerateSignature(api, ps);
        }
        // ??? how to write this better

        public string GenerateSignature(ApiBase api, List<Parameter> ps)
        {
            // sort the things
            ps.Sort(new ParameterComparer());
            // put them together, & conjuncted
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < ps.Count; i++)
            {
                sb.AppendFormat("{0}={1}", ps[i].Name, UrlEncode(ps[i].Value.ToString()));
                if (i < ps.Count - 1)
                    sb.Append("&");
            }
            // encoding
            string _part2_encoded = UrlEncode(sb.ToString());
            // insert the heading
            string _base_string = api.Method.ToString() + "&" + UrlEncode(base_url + api.path_name) + "&" + _part2_encoded;
            var _databuffer = StringToByte(_base_string);

            // do encryption
            HMACSHA1 hcmasha1 = new HMACSHA1();
            string Key = app_secret + "&";
            if (access_token != null)
                Key += access_token;
            else if (oauth_token_secret != null)
                Key += oauth_token_secret;
            // nothing is appended for request_token

            byte[] _byte_key = new byte[Key.Length];
            var test = StringToByte(Key);
            for (int i = 0; i < Key.Length; i++)
                _byte_key[i] = test[i];
            hcmasha1.Key = _byte_key;

            //_Api_tako.Add("generateSignature: key=" + ByteToString(_byte_key) + "; data = " + ByteToString(_databuffer));

            var _hashed_buffer = hcmasha1.ComputeHash(_databuffer);
            // base 64 encoding
            string ret = Convert.ToBase64String(_hashed_buffer);

            // return the string
            return ret;
        }

        private string ByteToString(byte[] b)
        {
            return System.Text.Encoding.UTF8.GetString(b, 0, b.Length);
        }
        private byte[] StringToByte(string p)
        {
            return System.Text.UTF8Encoding.UTF8.GetBytes(p);
        }

        /// <summary>
        /// Generate the timestamp for the signature        
        /// </summary>
        /// <returns></returns>
        public string GenerateTimeStamp()
        {
            // Default implementation of UNIX time of the current UTC time
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString();
        }
        /// <summary>
        /// Generate a nonce
        /// </summary>
        /// <returns></returns>
        public string GenerateNonce()
        {
            // Just a simple implementation of a random number between 123400 and 9999999
            return random.Next(123400, 9999999).ToString();
        }

        #endregion

        #region events
        // events for api call complete
        public event EventHandler<ApiResultEventArgs> call_complete;
        public class ApiResultEventArgs : EventArgs
        {
            private Api _api;
            public Api Api
            {
                get { return _api; }
                set { _api = value; }
            }

            private bool _success;
            public bool is_success
            {
                get { return _success; }
                set { _success = value; }
            }

            private string content;

            public string Content
            {
                get { return content; }
                set { content = value; }
            }
            private Error _error = new Error();
            public Error Error
            {
                get { return _error; }
                set { _error = value; }
            }

            public ApiResultEventArgs() { }
            public ApiResultEventArgs(bool b, Api api)
            {
                _success = b;
                Api = api;
            }
        }
        public class ApiResultEventArgs<T> : EventArgs where T : new()
        {
            private Api<T> _api;
            public Api<T> Api
            {
                get { return _api; }
                set { _api = value; }
            }

            private bool _success;
            public bool is_success
            {
                get { return _success; }
                set { _success = value; }
            }

            private Error _error = new Error();
            public Error Error
            {
                get { return _error; }
                set { _error = value; }
            }

            public ApiResultEventArgs() { }
            public ApiResultEventArgs(bool b, T obj, Api<T> api)
            {
                _success = b;
                _object = obj;
                Api = api;
                Error = null;
            }
            private T _object;
            public T data
            {
                get { return _object; }
                set { _object = value; }
            }
        }

        #endregion

        #region trace
        public Trace _trace = Trace.get_global_trace();
        public List<string> _Api_trace = new List<string>();

        protected void _trace_record_parameters(ApiBase api, List<Parameter> ps)
        {
            _trace.LogParameters(api, "request", ps);
        }

        protected void LogTrace(ApiBase api, string p, string ps)
        {
            _trace.Log(api, p, ps);
        }
        protected void LogTrace(ApiBase api, string p, List<Parameter> ps)
        {
            _trace.LogParameters(api, p, ps);
        }

        #endregion

        #region login

        // events for login complete
        public event login_complete_handler login_complete;
        public delegate void login_complete_handler(object sender);

        private void OnCallComplete(object sender, ApiResultEventArgs e)
        {
            call_complete(sender, e);
        }


        internal void logout()
        {
            oauth_token = null;
            oauth_token_secret = null;
            access_token = null;
            is_logged_in = false;

            ApplicationSettings.SetupWeiboAccount(false, "", "");
        }
        protected void get_request_token()
        {
            // method specifically for request token
            Api api = new Api()
            {
                name = "request_token",
                path_name = "oauth/request_token",
                Method = Method.GET
            };

            var r = new RestRequest(api.path_name, api.Method);

            var ts = GenerateTimeStamp();
            var nonce = GenerateNonce();
            r.AddParameter("oauth_consumer_key", app_key);
            r.AddParameter("oauth_signature_method", SignatureMethodName);
            r.AddParameter("oauth_timestamp", ts);
            r.AddParameter("oauth_nonce", nonce);
            r.AddParameter("oauth_version", OAuthVersion);

            r.AddParameter("oauth_signature", GenerateSignature(api, r.Parameters));
            //r.RequestFormat = DataFormat.json;

            _trace_record_parameters(api, r.Parameters);
            restClient.ExecuteAsync(r, (response) =>
            {
                _Api_trace.Add(api.name + "-result: " + response.Content);

                if (response.Content == "")
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() => MessageBox.Show("网络异常，请稍后再试"));
                    return;
                }

                // returned values are in name/value pair form
                var _parameters = GetQueryParameters(response.Content);
                oauth_token = _parameters.FirstOrDefault(x => x.Name == "oauth_token").Value;
                oauth_token_secret = _parameters.FirstOrDefault(x => x.Name == "oauth_token_secret").Value;
                // need to notify that things are done

                // ??? synchronize between actions?
                authorize(userLogin, passwd);
            });
        }
        protected void authorize(string userLogin, string passwd)
        {
            Api api = new Api()
            {
                name = "authorize",
                path_name = "oauth/authorize",
                Method = Method.GET
            };

            var r = new RestRequest(api.path_name, api.Method);
            r.AddParameter("oauth_token", oauth_token);
            r.AddParameter("oauth_callback", "json"); // directly get the response
            //r.AddParameter("display", "mobile");
            r.AddParameter("userId", userLogin);
            r.AddParameter("passwd", passwd);
            //r.RequestFormat = DataFormat.json;
            //            r.RootElement = "oauth";

            _trace_record_parameters(api, r.Parameters);
            restClient.ExecuteAsync<oauth>(r, (response) =>
            {
                LogTrace(api, "result", response.Content);
                // returned things are in json
                var result = response.Data;
                oauth_token = result.oauth_token;
                oauth_verifier = result.oauth_verifier;

                //?? synchronize problem: any way to write this better
                get_access_token();
            });
        }
        protected void get_access_token()
        {
            Api api = new Api()
            {
                name = "access_token",
                path_name = "oauth/access_token",
                Method = Method.GET
            };

            var r = new RestRequest(api.path_name, api.Method);
            var ts = GenerateTimeStamp();
            var nonce = GenerateNonce();
            r.AddParameter("oauth_consumer_key", app_key);
            r.AddParameter("oauth_token", oauth_token);
            r.AddParameter("oauth_signature_method", SignatureMethodName);
            r.AddParameter("oauth_timestamp", ts);
            r.AddParameter("oauth_nonce", nonce);
            r.AddParameter("oauth_version", OAuthVersion);
            r.AddParameter("oauth_verifier", oauth_verifier);
            r.AddParameter("oauth_signature", GenerateSignature(api, r.Parameters));

            _trace_record_parameters(api, r.Parameters);
            restClient.ExecuteAsync<oauth>(r, (response) =>
            {
                LogTrace(api, "result", response.Content);
                var _parameters = GetQueryParameters(response.Content);
                OnLoginComplete(this, _parameters);

            });
        }
        private string userLogin;
        private string passwd;
        private string userId;
        public void login(string your_userLogin, string your_passwd)
        {
            logout();
            userLogin = your_userLogin;
            passwd = your_passwd;

            get_request_token();
        }
        bool is_logged_in = false;
        public bool IsLoggedIn
        {
            get { return is_logged_in; }
        }
        private void OnLoginComplete(object sender, List<QueryParameter> _parameters)
        {
            userId = _parameters.FirstOrDefault(x => x.Name == "user_id").Value;

            oauth_token = _parameters.FirstOrDefault(x => x.Name == "oauth_token").Value; // the original oauth token is overwritten!! by design, as specified by the api document
            access_token = _parameters.FirstOrDefault(x => x.Name == "oauth_token_secret").Value;

            if (is_logged_in == false)
            {
                is_logged_in = true;
                ApplicationSettings.SetupWeiboAccount(IsLoggedIn, oauth_token, access_token);
            }
            login_complete(this);

        }


        #endregion

        #region thread lock
        public class thread_lock
        {
            public static bool can_show=true;
            public static object lock_object = new object();
        }
        #endregion

        #region Api Base

        public class ApiBase
        {
            public string name { get; set; }
            public string path_name { get; set; }
            public Method Method { get; set; }
            public override string ToString()
            {
                return name;
            }

            public RestRequest r;
            public ApiCallContext context = new ApiCallContext();
            protected RestClient restClient = SinaApi.base_oauth.restClient;
            protected SinaApi _obase = SinaApi.base_oauth;
            protected Trace _trace = SinaApi.base_oauth._trace;

            public Action<RestRequest> stuff_parameters;
            public Action<RestRequest> stuff_data;

            public ApiBase()
            {
                stuff_parameters = (r) =>
                {
                    foreach (var item in context.parameters)
                    {
                        if (item.Name.Length>=4 && item.Name.Substring(0, 4) == "oauth")
                            r.AddParameter(item.Name, item.Value, ParameterType.Cookie);
                        else
                            r.AddParameter(item.Name, item.Value, ParameterType.GetOrPost);
                    }
                };
                stuff_data = (r) => { };
            }
        }
        public class ApiCallContext
        {
            public Int64 since_id = 0;
            public Int64 max_id = 0;
            public Int64 id = 0;

            public int count = 0;
            public int page = 0;
            public int is_comment = 0;
            public int with_new_status = 0;

            public string status;

            public string filename;

            public List<Parameter> parameters = new List<Parameter>();
            public void AddParameter(string name, object value)
            {
                parameters.Add(new Parameter() { Name = name, Value = value });
            }
            public byte[] data;
        }
        public class Api : ApiBase
        {
            public Action<RestResponse> callback;
            public ApiResultEventArgs e = new ApiResultEventArgs();
            public event EventHandler<ApiResultEventArgs> call_complete;

            public Api()
            {
                Method = Method.GET;
                e.Api = this;
                callback = (response) =>
                {
                    e.is_success = true;
                    e.Content = response.Content;
                };
            }

            public void call()
            {
                var r = new RestRequest(this.path_name, this.Method);
                _obase.stuff_oauth_parameters(r);

                stuff_parameters(r);

                _obase.sign(this, r);

                stuff_data(r);

                _trace.LogParameters(this, "request", r.Parameters);

                BackgroundWorker bw = new BackgroundWorker();
                bw.DoWork += new DoWorkEventHandler((sss, eeee) =>
                    {
                        restClient.ExecuteAsync(r, (response) =>
                        {

                            Deployment.Current.Dispatcher.BeginInvoke(() =>
                                {
                                    _trace.Log(this, "result", response.Content);
                                    if (response.ResponseStatus == ResponseStatus.Error)
                                    {
                                        e.is_success = false;
                                        e.Error.error = "网络异常，连不上...";
                                    }
                                    else if (response.ResponseStatus == ResponseStatus.TimedOut)
                                    {
                                        e.is_success = false;
                                        e.Error.error = "网络超时，请稍后手动重试";
                                    }
                                    else if (response.ResponseStatus == ResponseStatus.Aborted)
                                    {
                                        e.is_success = false;
                                        e.Error.error = "网络中断。huh?";
                                    }
                                    else
                                    {
                                        e.is_success = true;
                                        callback(response);
                                    }

                                    if (e.is_success == false)
                                    {
                                        lock (thread_lock.lock_object)
                                        {
                                        if (thread_lock.can_show)
                                        {
                                            //MessageBox.Show(e.Error.error);
                                            thread_lock.can_show = false;
                                            DispatcherTimer dt = new DispatcherTimer();
                                            dt.Interval = TimeSpan.FromSeconds(2);
                                            dt.Tick += new EventHandler((ssss, eeeee) =>
                                            {
                                                dt.Stop();
                                                thread_lock.can_show = true;
                                            });
                                            dt.Start();
                                        }
                                        }
                                    }
                                    

                                    if (call_complete != null)
                                        call_complete(this, e);
                                });
                        });
                    });
                bw.RunWorkerAsync();
                bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler((sss, eee) =>
                {
                });
            }

        }
        public class Api<T> : ApiBase where T : new()
        {
            public Action<RestResponse<T>> callback;
            public ApiResultEventArgs<T> e = new ApiResultEventArgs<T>();
            public event EventHandler<ApiResultEventArgs<T>> call_complete;

            public Api()
            {
                Method = Method.GET;

                e.Api = this;
                callback = (response) =>
                {
                    // nothing is done here
                };
            }

            public void call()
            {
                var r = new RestRequest(this.path_name, this.Method);
                _obase.stuff_oauth_parameters(r);

                stuff_parameters(r);

                _obase.sign(this, r);

                stuff_data(r);

                _trace.LogParameters(this, "request", r.Parameters);

                BackgroundWorker bw = new BackgroundWorker();
                bw.DoWork += new DoWorkEventHandler((sss, eeee) =>
                    {
                        restClient.ExecuteAsync<T>(r, (response, handle) =>
                        {
                            _trace.Log(this, "result", response.Content);
                            Deployment.Current.Dispatcher.BeginInvoke(() =>
                                {
                                    if (response.ResponseStatus == ResponseStatus.Error)
                                    {
                                        e.is_success = false;
                                        e.data = default(T);
                                        e.Error.error = "网络异常，连不上...";
                                    }
                                    else if (response.ResponseStatus == ResponseStatus.TimedOut)
                                    {
                                        e.is_success = false;
                                        e.data = default(T);
                                        e.Error.error = "网络超时，请稍后手动重试";
                                    }
                                    else if (response.ResponseStatus == ResponseStatus.Aborted)
                                    {
                                        e.is_success = false;
                                        e.data = default(T);
                                        e.Error.error = "网络中断。huh?";
                                    }
                                    else if (response.ContentLength < 256
                                        && response.Content.Contains("error_code")
                                        && response.Content.Substring(0, 10).Contains("request"))
                                    {
                                        e.is_success = false;
                                        e.data = default(T);
                                        var d = new JsonDeserializer();
                                        e.Error = d.Deserialize<Error>(response.Content);
                                    }
                                    else
                                    {
                                        e.is_success = true;
                                        e.data = response.Data;
                                        callback(response);
                                    }


                                    if (e.is_success == false)
                                    {
                                        lock (thread_lock.lock_object)
                                        {
                                        if (thread_lock.can_show)
                                        {
                                            //MessageBox.Show(e.Error.error);
                                            thread_lock.can_show = false;
                                            DispatcherTimer dt = new DispatcherTimer();
                                            dt.Interval = TimeSpan.FromSeconds(2);
                                            dt.Tick += new EventHandler((ssss, eeeee) =>
                                            {
                                                dt.Stop();
                                                thread_lock.can_show = true;
                                            });
                                            dt.Start();
                                        }
                                        }
                                    }

                                    if (call_complete != null)
                                        call_complete(this, e);
                                });
                        });
                    });
                bw.RunWorkerAsync();
            }
        }

        #endregion

        #region API calls

        // get logged on user data; set UserManager.You
        public class Api_VerifyCredentials : Api<user>
        {
            public Api_VerifyCredentials()
            {
                name = "verify_credentials";
                path_name = "account/verify_credentials.json";
                Method = Method.GET;
                callback = (response) =>
                {
                    ApplicationSettings.WeiboAccountProfile = response.Data;
                };
            }
        }

        // post a weibo
        public class Api_Update : Api<status>
        {
            public Api_Update()
            {
                name = "update";
                path_name = "statuses/update.json";
                Method = Method.POST;
            }
            public void call(string your_message)
            {
                var api = this;
                api.context.AddParameter("status", your_message);
                base.call();
            }
        };

        // post weibo with picture
        public class Api_Upload : Api<status>
        {
            public Api_Upload()
            {
                name = "upload";
                path_name = "statuses/upload.json";
                Method = Method.POST;
            }
            public void call(string status, byte[] pic, string filename = "picture.jpg")
            {
                var api = this;
                
                if (status == null || status == "")
                    status = "发个图";
                api.context.AddParameter("status", status);

                stuff_data = (request) =>
                {
                    request.AddFile("pic", pic, filename);
                };

                base.call();
            }
        };
        #endregion

        #region unused APIs

        /*
        // get all weibo of a specified user
        public class Api_UserTimeline : Api<List<status>>
        {
            public Api_UserTimeline()
            {
                name = "user_timeline";
                path_name = "statuses/user_timeline.json";
                Method = Method.GET;
            }
            public void call(Int64 user_id=0, long since_id = 0, long max_id = 0, int count = 20, int page = 1)
            {
                var api = this;
                if (user_id != 0)
                    api.context.AddParameter("user_id", user_id);
                if (since_id != 0)
                    api.context.AddParameter("since_id", since_id);
                if (max_id != 0)
                    api.context.AddParameter("max_id", max_id);

                api.context.AddParameter("count", count);
                base.call();
            }
        };
        
        // get all your weibo
        public class Api_HomeTimeline : Api<List<status>>
        {
            public Api_HomeTimeline()
            {
                name = "home_timeline";
                path_name = "statuses/home_timeline.json";
                Method = Method.GET;
            }
            public void call(long since_id = 0, long max_id = 0, int count = 20, int page = 1)
            {
                var api = this;
                if (since_id != 0)
                    api.context.AddParameter("since_id", since_id);
                if (max_id != 0)
                    api.context.AddParameter("max_id", max_id);

                api.context.AddParameter("count", count);
                base.call();

                var unread = new Api_Unread();
                unread.call();
            }
        };

        // get all weibo that mentions you
        public class Api_Mentions : Api<List<status>>
        {
            public Api_Mentions()
            {
                name = "mentions";
                path_name = "statuses/mentions.json";
                Method = Method.GET;
            }
            public void call(Int64 since_id = 0, Int64 max_id = 0, int count = 20, int page = 1)
            {
                var api = this;
                if (since_id != 0)
                    api.context.AddParameter("since_id", since_id);
                if (max_id != 0)
                    api.context.AddParameter("max_id", max_id);

                api.context.AddParameter("count", count);
                base.call();
            }
        };

        // repost a weibo
        public class Api_Repost : Api<status>
        {
            public Api_Repost()
            {
                name = "repost";
                path_name = "statuses/repost.json";
                Method = Method.POST;
            }
            public void call(Int64 id, string your_message = "", int is_comment = 0)
            {
                var api = this;
                api.context.AddParameter("id", id);
                if (your_message != "")
                    api.context.AddParameter("status", your_message);
                if (is_comment != 0)
                    api.context.AddParameter("is_comment", is_comment);
                base.call();
            }
        };

        // get unread counts
        
        public class Api_Unread : Api<count>
        {
            public Api_Unread()
            {
                name = "unread";
                path_name = "statuses/unread.json";
                Method = Method.GET;
                callback = (response) =>
                    {
                        try
                        {
                            UserManager.UserProfile.UnreadCount = response.Data as count;
                            e.data = response.Data;
                            e.is_success = true;
                        }
                        catch (Exception)
                        {
                            e.data = response.Data;
                            e.is_success = false;
                        }
                    };
            }
            /// <summary>
            /// return unread count
            /// </summary>
            /// <param name="since_id">if with_net_status=1, return whether there are new status after since_id</param>
            /// <param name="with_new_status">whether to return count of new_status</param>
            public void call(Int64 since_id = 0, int with_new_status = 0)
            {
                var api = this;
                if (with_new_status != 0)
                    api.context.AddParameter("with_new_status", with_new_status);
                if (since_id != 0)
                    api.context.AddParameter("since_id", since_id);

                base.call();
            }
        };

        // get reset counts
        public class Api_ResetCount : Api
        {
            public Api_ResetCount()
            {
                name = "reset_count";
                path_name = "statuses/reset_count.json";
                Method = Method.POST;
            }

            public void call(int type)
            {
                var api = this;
                api.context.AddParameter("type", type);
                base.call();
            }

        };

        // get a weibo given ID
        public class Api_Status_Show : Api<status>
        {
            public Api_Status_Show()
            {
                name = "status_show";
                path_name = "statuses/show";
                Method = Method.GET;
            }
            public void call(Int64 id)
            {
                var api = this;
                api.path_name = string.Format("{0}/{1}.json", api.path_name, id);
                api.context.AddParameter("id", id);
                base.call();
            }
        };

        // delete a weibo 
        public class Api_StatusDestory : Api<status>
        {
            public Api_StatusDestory()
            {
                name = "status_destory";
                path_name = "statuses/destroy";
                Method = Method.DELETE;
            }
            public void call(Int64 id)
            {
                var api = this;
                api.path_name = string.Format("{0}/{1}.json", api.path_name, id);
                // api.context.AddParameter("id", id);
                base.call();
            }
        };


        public class Api_CommentsToMe : Api<List<comment>>
        {
            public Api_CommentsToMe()
            {
                name = "comments_to_me";
                path_name = "statuses/comments_to_me.json";
                Method = Method.GET;

                callback = (response) =>
                {
                    foreach (var item in response.Data as List<comment>)
                    {
                        item.normalize();
                    }
                    e.is_success = true;
                    e.data = response.Data;
                };
            }
            public void call(long since_id = 0, long max_id = 0, int count = 20, int page = 1)
            {
                var api = this;
                if (since_id != 0)
                    api.context.AddParameter("since_id", since_id);
                if (max_id != 0)
                    api.context.AddParameter("max_id", max_id);

                api.context.AddParameter("count", count);
                base.call();
            }
        }

        // get all comments of a given weibo
        public class Api_Comments : Api<List<comment>>
        {
            public Api_Comments()
            {
                name = "comments";
                path_name = "statuses/comments.json";
                Method = Method.GET;
            }
            public void call(Int64 id, int count = 20, int page = 1)
            {
                var api = this;
                api.context.AddParameter("id", id);
                api.context.AddParameter("count", count);
                api.context.AddParameter("page", page);
                base.call();
            }
        };

        // batch get comment/rt count of a list of weibo
        public class Api_counts : Api<List<count>>
        {
            public Api_counts()
            {
                name = "counts";
                path_name = "statuses/counts.json";
            }
            public void call(List<Int64> ids)
            {
                if (ids.Count == 0)
                    return;
                var api = this;
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < ids.Count; i++)
                {
                    sb.Append(ids[i].ToString());
                    if (i < ids.Count - 1)
                        sb.Append(',');
                }
                api.context.AddParameter("ids", sb.ToString());
                base.call();
            }
        }

        public class Api_User : Api<user>
        {
            public Api_User()
            {
                callback = (response) =>
                    {
                        e.data = response.Data as user;
                        e.data.normalize();
                        e.is_success = true;
                    };
            }
        }
        // get a user given ID or name
        public class Api_UserShow : Api_User
        {
            public Api_UserShow()
            {
                name = "user_show";
                path_name = "users/show.json";
                Method = Method.GET;
            }
            public void call(Int64 id)
            {
                var api = this;
                api.context.AddParameter("user_id", id);
                base.call();
            }
            public void call(string id)
            {
                var api = this;
                api.context.AddParameter("screen_name", id);
                base.call();
            }
        };

        // follow some guy
        public class Api_FriendshipCreate : Api_User
        {
            public Api_FriendshipCreate()
            {
                name = "friendship_create";
                path_name = "friendships/create.json";
                Method = Method.POST;
            }
            public void call(Int64 id)
            {
                var api = this;
                api.context.AddParameter("user_id", id);
                base.call();
            }
            public void call(string id)
            {
                var api = this;
                api.context.AddParameter("screen_name", id);
                base.call();
            }
        };

        // unfollow some guy
        public class Api_FriendshipDestory : Api_User
        {
            public Api_FriendshipDestory()
            {
                name = "friendship_destroy";
                path_name = "friendships/destroy.json";
                Method = Method.DELETE;
            }
            public void call(Int64 id)
            {
                var api = this;
                api.context.AddParameter("user_id", id);
                base.call();
            }
            public void call(string id)
            {
                var api = this;
                api.context.AddParameter("screen_name", id);
                base.call();
            }
        };

        // unfollow some guy
        public class Api_Friendships_Show : Api<relationship>
        {
            public Api_Friendships_Show()
            {
                name = "friendships_show";
                path_name = "friendships/show.json";
                Method = Method.GET;
                callback = (response) =>
                {
                    var rel = response.Data as relationship;
                    var u = UserManager.Manager.get(rel.target.id);
                    u.following = rel.source.following;
                    u.followed_by = rel.source.followed_by;
                };
            }
            public void call(Int64 id)
            {
                var api = this;
                api.context.AddParameter("target_id", id);
                base.call();
            }
            public void call(string id)
            {
                var api = this;
                api.context.AddParameter("target_screen_name", id);
                base.call();
            }
        };

        // get the list of followings
        public class Api_Statuses_Followings : Api<Api_Statuses_Followings_repsonse>
        {
            public Api_Statuses_Followings()
            {
                name = "friends";
                path_name = "statuses/friends.xml"; // this is the only place that we use xml; json does not work
                Method = Method.GET;
            }
            public void call(Int64 id, int cursor = -1, int count = 20)
            {
                var api = this;
                api.context.AddParameter("user_id", id);
                api.context.AddParameter("cursor", cursor);
                api.context.AddParameter("count", count);
                base.call();

                // to support repeated calling
                // 1. understand the returned cursor
                // 2. repeated calling out
                // 3. accumulate the return
            }
        };

        // get the list of followers
        public class Api_Statuses_Followers : Api<List<user>>
        {
            public Api_Statuses_Followers()
            {
                name = "followers";
                path_name = "statuses/followers.xml"; // also xml here
                Method = Method.GET;
            }
            public void call(Int64 id,int cursor = -1, int count =20)
            {
                var api = this;
                api.context.AddParameter("user_id", id);
                api.context.AddParameter("cursor", cursor);
                api.context.AddParameter("count", count);
                base.call();

                // to support repeated calling
                // 1. understand the returned cursor
                // 2. repeated calling out
                // 3. accumulate the return
            }
        };

        // comment a weibo
        public class Api_Statuses_Comment : Api<comment>
        {
            public Api_Statuses_Comment ()
            {
                name = "statuses_comment";
                path_name = "statuses/comment.json";
                Method = Method.POST;
            }
            public void call(Int64 id, Int64 cid=0, string your_message = "", int without_mention = 0)
            {
                var api = this;
                api.context.AddParameter("comment", your_message);
                api.context.AddParameter("id", id);
                api.context.AddParameter("without_mention", without_mention);
                if (cid != 0)
                    api.context.AddParameter("cid", cid);
                base.call();
            }
        };

        // bilateral friends // does not work
        public class Api_2_friends_bilateral : Api<friends_bilateral_response>
        {
            public Api_2_friends_bilateral()
            {
                name = "friends_bilateral";
                path_name = "2/friendships/friends/bilateral/ids.json";
                Method = Method.GET;
            }
            public void call(Int64 uid, int count = 2000, int page = 1, int sort = 0)
            {
                var api = this;
                api.context.AddParameter("uid", uid);
                api.context.AddParameter("count", count);
                api.context.AddParameter("page", page);
                api.context.AddParameter("sort", sort);
                base.call();
            }
        };
*/
        #endregion
    }
}