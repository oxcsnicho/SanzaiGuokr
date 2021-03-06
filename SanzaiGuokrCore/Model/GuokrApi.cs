﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Messaging;
using HtmlAgilityPack;
using RestSharp;
using SanzaiGuokr.GuokrObject;
using SanzaiGuokr.GuokrObjects;
using SanzaiGuokr.Messages;
using SanzaiGuokr.Util;
using SanzaiGuokr.ViewModel;
using System.Windows;
using SanzaiGuokr.GuokrApiV2;
using System.Linq;
using SanzaiGuokr.Model;
using System.Collections;
using GalaSoft.MvvmLight.Command;
using System.IO.IsolatedStorage;
using System.Text;
using System.IO;
using Windows.Storage;
using System.Diagnostics;

namespace SanzaiGuokr.GuokrApiV2
{
    public class Icon
    {
        public string large { get; set; }
        public string small { get; set; }
    }

    public class GuokrOauthTokenResponse
    {
        public string access_token { get; set; }
        public int expires_in { get; set; }
        public string nickname { get; set; }
        public string refresh_token { get; set; }
        public string ukey { get; set; }
    }

    public class Minisite
    {
        public string name { get; set; }
        public string url { get; set; }
        public string introduction { get; set; }
        public string key { get; set; }
        public string date_created { get; set; }
        public string icon { get; set; }
    }

    public class Avatar
    {
        public string large { get; set; }
        public string small { get; set; }
        public string normal { get; set; }
    }

    public class Author
    {
        public int followers_count { get; set; }
        public string ukey { get; set; }
        public bool is_exists { get; set; }
        public string title { get; set; }
        public string url { get; set; }
        public string gender { get; set; }
        public string resource_url { get; set; }
        public string nickname { get; set; }
        public bool is_title_authorized { get; set; }
        public Avatar avatar { get; set; }
    }

    public class ArticleInfo
    {
        public Minisite minisite { get; set; }
        public string image { get; set; }
        public string date_published { get; set; }
        public int id { get; set; }
        public string title { get; set; }
        public string url { get; set; }
        public string image_description { get; set; }
        public Author author { get; set; }
        public string small_image { get; set; }
        public string summary { get; set; }
        public string resource_url { get; set; }
        public int replies_count { get; set; }
    }
    public class RecommendedArticleInfo
    {
        public int ordinal { get; set; }
        //public string parent_name { get; set; }
        public string title { get; set; }
        public string url { get; set; }
        public string image { get; set; }
        //public string summary { get; set; }
        //public string parent_url { get; set; }
        //public string resource_url { get; set; }
        //public string type { get; set; }
    }
    public class RecommendedArticlesResponse : GuokrResponse
    {
        public List<RecommendedArticleInfo> result { get; set; }

        public List<recommend_article> ToArticleList()
        {
            var r = new Random(DateTime.Now.Second);
            //int nr_to_show = 3;
            //var start = r.Next(5 / nr_to_show) * nr_to_show;
            //var start = 0;
            if (result != null)
            {
                var res = from item in this.result
                          where item.url.Contains("article") && !item.url.Contains("zone") && !string.IsNullOrEmpty(item.image)
                          //&& item.ordinal >= start && item.ordinal < start + nr_to_show
                          select new recommend_article()
                          {
                              //minisite_name = item.parent_name,
                              url = item.url,
                              //id = Convert.ToInt64(Regex.Match(item.url, @"\d+").Value),
                              //Abstract = item.summary,
                              pic = item.image,
                              title = item.title
                          };
                return res.OrderBy(elem => Guid.NewGuid()).Take(3).ToList();
            }
            else
                return null;
        }
    }

    public class LatestArticlesResponse : GuokrResponse
    {
        public int limit { get; set; }
        public List<ArticleInfo> result { get; set; }
        public int offset { get; set; }
        public int total { get; set; }

        public List<article> ToArticleList()
        {
            if (result != null)
            {
                var res = from item in this.result
                          select new article()
                          {
                              Author = item.author,
                              minisite_name = (item.minisite != null ? item.minisite.name : "科学人"),
                              url = item.resource_url,
                              id = item.id,
                              Abstract = item.summary,
                              pic = string.IsNullOrWhiteSpace(item.image) ? item.small_image : item.image,
                              title = item.title,
                              posted_dt = item.date_published,
                              CommentCount = item.replies_count
                          };
                return res.ToList();
            }
            else
                return null;
        }
    }
    public class ArticleDetail
    {
        public Minisite minisite { get; set; }
        public List<string> tags { get; set; }
        public string image { get; set; }
        public bool is_replyable { get; set; }
        public string date_published { get; set; }
        public ArticleInfo prev_article { get; set; }
        public int replies_count { get; set; }
        public int id { get; set; }
        public int recommends_count { get; set; }
        public string copyright { get; set; }
        public string title { get; set; }
        public string url { get; set; }
        public string image_description { get; set; }
        public Author author { get; set; }
        public string small_image { get; set; }
        public string summary { get; set; }
        public string content { get; set; }
        public string resource_url { get; set; }
        public ArticleInfo next_article { get; set; }

        private DateTime _date_published;
        public DateTime DatePublished
        {
            get
            {
                if (_date_published == default(DateTime))
                {
                    _date_published = DateTime.Parse(date_published);
                }
                return _date_published;
            }
        }
    }

    public class PostDetail
    {
        public string content { get; set; }
        public Author author { get; set; }
        public Group group { get; set; }
        public bool is_digest { get; set; }
        public string title { get; set; }
        public string url { get; set; }
        public string ukey_author { get; set; }
        public bool is_replyable { get; set; }
        public string summary { get; set; }
        public string date_last_replied { get; set; }
        public string html { get; set; }
        public bool is_stick { get; set; }
        public string date_created { get; set; }
        public int replies_count { get; set; }
        public string resource_url { get; set; }
        public int group_id { get; set; }
        public int recommends_count { get; set; }
        public int id { get; set; }
    }
    public class GetPostDetailResponse : GuokrResponse
    {
        public PostDetail result { get; set; }
    }
    public class GetArticleResponse : GuokrResponse
    {
        public ArticleDetail result { get; set; }
    }

    public class CommentInfo
    {
        public string content { get; set; }
        public string date_created { get; set; }
        public string resource_url { get; set; }
        public string url { get; set; }
        public int id { get; set; }
        public int liking_count { get; set; }
        public int likings_count { get; set; }
        public Author author { get; set; }
        public bool current_user_has_liked { get; set; }
    }

    public class GuokrRnNum
    {
        public int r { get; set; }
        public int n { get; set; }
        public int SumValue
        {
            get
            {
                return n;
            }
        }
        public int TotalValue
        {
            get
            {
                return r * 100 + n;
            }
        }
        public bool is_nonempty
        {
            get
            {
                return SumValue > 0;
            }
        }
    }
    public class GuokrRnResponse : GuokrResponse
    {
        public GuokrRnNum result { get; set; }
    }
    public class GuokrNotice
    {
        private string c = "";
        public string content
        {
            get
            {
                return c;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                    c = value.Replace('\n', ' ');
            }
        }
        public bool is_read { get; set; }
        public bool is_unread
        {
            get
            {
                return !is_read;
            }
        }
        public string date_last_updated { get; set; }
        public string ukey { get; set; }
        public string url { get; set; }
        public int id { get; set; }
        string GetType(string url)
        {
            if (string.IsNullOrEmpty(url))
                return "";

            if (url.Contains("article"))
                return "article";
            else if (url.Contains("post"))
                return "post";
            else
                return "unknown";
        }

        public StatusType status { get; set; }
        private RelayCommand _vi;
        public RelayCommand ViewItem
        {
            get
            {
                if (_vi == null)
                    _vi = new RelayCommand(async () =>
                    {
                        status = StatusType.INPROGRESS;
                        _vi.RaiseCanExecuteChanged();
                        var a = await GuokrApi.GetGuokrObjectFromReply(url);
#if DEBUG
                        Deployment.Current.Dispatcher.BeginInvoke(() => MessageBox.Show((a as article_base).url));
#endif
                        var title = content.Split(new char[] { '《', '》' })[1];
                        if (a.object_name == "article")
                        {
                            var b = a as article;
                            b.title = title;
                            Messenger.Default.Send<GoToReadArticle>(new GoToReadArticle() { article = b });
                        }
                        else if (a.object_name == "post")
                        {
                            var b = a as GuokrPost;
                            b.title = title;
                            try
                            {
                                await GuokrApi.GetPostDetail(b);
                                Messenger.Default.Send<GoToReadPost>(new GoToReadPost() { article = b });
                            }
                            catch (GuokrException e)
                            {
                                if (e.error_code == GuokrErrorCodeV2.ResultNotFound)
                                {
                                    Messenger.Default.Send<DisplayMessageBox>(new DisplayMessageBox() { message = "404 目标失联。。。被和谐了？" });
                                }
                            }
                        }
                        status = StatusType.SUCCESS;

                    }, CanViewItem);
                return _vi;
            }
        }
        public bool CanViewItem()
        {
            if (status == StatusType.INPROGRESS)
                return false;
            var patterns = new string[] { @".*《.*》.*提到了你.*", @"你的帖子.*" };
            foreach (var pattern in patterns)
            {
                if (Regex.Match(content, pattern).Success)
                    return true;
            }
            return false;
        }

        void GetThings(IAsyncResult result)
        {
            HttpWebRequest request = (HttpWebRequest)result.AsyncState;
            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(result);

            var path = response.ResponseUri.AbsolutePath;
            var type = GetType(path);

            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                if (type == "post")
                {

                    //                    Messenger.Default.Send<GoToReadPost>(new GoToReadPost() { article = new GuokrPost(){ id....}})
                }
            });

        }

    }
    public class GuokrNoticeResponse : GuokrResponse
    {
        public List<GuokrNotice> result { get; set; }
    }

    public class GetArticleCommentsResponse : GuokrResponse
    {
        public int limit { get; set; }
        public List<CommentInfo> result { get; set; }
        public int offset { get; set; }
        public int total { get; set; }

        public List<comment> ToCommentList(int offset = 0)
        {
            int floor = offset;
            var q = from i in result
                    let f = ++floor
                    select new comment()
                    {
                        content = i.content,
                        date_create = i.date_created,
                        floor = f,
                        nickname = i.author.nickname,
                        head_48 = i.author.avatar.small,
                        reply_id = i.id,
                        title_authorized = i.author.is_title_authorized,
                        userPicUrl = i.author.avatar.large,
                        userUrl = i.author.url,
                        ukey = i.author.ukey,
                        liking_count = i.liking_count + i.likings_count,
                        url = i.url,
                        has_liked = i.current_user_has_liked
                    };
            return q.ToList();
        }
    }

    public class Group
    {
        public bool is_publicly_readable { get; set; }
        public int members_count { get; set; }
        public string name { get; set; }
        public string url { get; set; }
        public string introduction_summary { get; set; }
        public string resource_url { get; set; }
        public int id { get; set; }
        public bool is_application_required { get; set; }
        public Icon icon { get; set; }
    }

    public class PostInfo
    {
        public int replies_count { get; set; }
        public Group group { get; set; }
        public bool is_digest { get; set; }
        public string title { get; set; }
        public string url { get; set; }
        public bool is_virgin { get; set; }
        public Author author { get; set; }
        public bool is_replyable { get; set; }
        public string summary { get; set; }
        public string date_last_replied { get; set; }
        public bool is_stick { get; set; }
        public string ukey_author { get; set; }
        public string date_created { get; set; }
        public string resource_url { get; set; }
        public int group_id { get; set; }
        public int id { get; set; }
    }
    public class LatestPostResponse : GuokrResponse
    {
        public int limit { get; set; }
        public List<PostInfo> result { get; set; }
        public int offset { get; set; }
        public int total { get; set; }

        public List<GuokrPost> ToPostList()
        {
            var q = result.Select((i) => new GuokrPost()
                    {
                        title = i.title.Trim(new char[] { '\n', ' ', '\t' }),
                        posted_by = new GuokrUser()
                        {
                            nickname = i.author.nickname,
                            uri = i.author.url
                        },
                        group = new GuokrGroup()
                        {
                            id = i.group_id,
                            name = i.group.name,
                            path = i.group.url
                        },
                        reply_count = i.replies_count,
                        replied_dt = i.date_last_replied,
                        wwwurl = i.url
                    });
            return q.ToList();
        }
    }
    public class GuokrResponse
    {
        private string _n;
        public string now
        {
            get
            {
                return _n;
            }
            set
            {
                _n = value;
                GuokrApi.ServerNow = DateTime.Parse(value);
            }
        }
        public bool ok { get; set; }
    }
}

namespace SanzaiGuokr.Model
{
    public class GuokrException : MyException
    {
        public GuokrErrorCode errnum { get; set; }
        public string errmsg { get; set; }
        public GuokrErrorCodeV2 error_code { get; set; }
        public string error { get; set; }

        public override int GetErrorCode()
        {
            if ((int)errnum > 0)
                return (int)errnum;
            else
                return (int)error_code;
        }
        public override string GetErrorMessage()
        {
            if (!string.IsNullOrEmpty(errmsg))
                return errmsg;
            else
                return error;
        }
    }
    public enum GuokrErrorCodeV2
    {
        IllegalAccessToken = 200004,
        AccessTokenIsNotProvided = 200014,
        ResultNotFound = 200015
    }
    public enum GuokrErrorCode
    {
        OK,//0
        LoginRequired,//1
        InternalError,//2
        OK3,//3
        OK4,//4
        OK5,//5
        OK6,//6
        CommentDoesNotExist,//7
        OK8,//8
        OK9,//9
        CommentTooFrequent,//10
        VerificationFailed,
        VerificationInternalError,
        UnderConstruction,
        CallFailure
    }
    public class GuokrApi : ApiClassBase
    {
        private static DateTime _sn;
        public static DateTime ServerNow
        {
            get
            {
                return _sn;
            }
            set
            {
                _sn = value;
                ServerTimeDiff = ServerNow - DateTime.Now;
            }
        }
        public static TimeSpan ServerTimeDiff { get; set; }

        public const string GuokrBaseUrlApi = "http://apis.guokr.com";
        private static RestClient _apiclient = new RestClient(GuokrBaseUrlApi) { CookieContainer = new CookieContainer() };
        public static RestClient ApiClient
        {
            get
            {
                return _apiclient;
            }
        }

        public const string GuokrBaseUrlWww = "http://www.guokr.com";
        private static RestClient _wwwclient = null;
        public static RestClient WwwClient
        {
            get
            {
                if (_wwwclient == null)
                    _wwwclient = new RestClient(GuokrBaseUrlWww) { CookieContainer = new CookieContainer() };
                return _wwwclient;
            }
        }

        static void ProcessError(IRestResponse resp)
        {
            ApiClassBase.ProcessError<GuokrException>(resp);
            //TODO: enable this using reflection
#if false
            if(resp.Data == null)
                throw new GuokrException() { errnum = GuokrErrorCode.CallFailure, errmsg = response.ErrorMessage };
#endif
        }

        public static async Task VerifyAccountV2(string username, string password)
        {
            var client = new RestClient("https://account.guokr.com") { CookieContainer = new CookieContainer() };
            string token;
            // get csrf_token
            {
                var req = new RestRequest();
                req.Parameters.Add(new Parameter() { Name = "Accept-Encoding", Value = "gzip", Type = ParameterType.HttpHeader });
                req.Resource = "/sign_in/";
                req.Method = Method.GET;
                var response = await RestSharpAsync.RestSharpExecuteAsyncTask(client, req);
                var match = Regex.Match(response.Content, @"input.*csrf_token.*(\d{14}##\w{40})");
                if (match.Success)
                    token = match.Groups[1].Value;
                else
                    throw new GuokrException() { errnum = GuokrErrorCode.VerificationInternalError, errmsg = "get csrf_token failed" };
            }

#if false
                // encode password
                string userToken = "";
                string encodedPassword = "";
                GuokrAuth.encodePassword(username, password, token, out encodedPassword, out userToken);
#endif

            // get session in cookies
            {
                var req = new RestRequest();
                req.Parameters.Add(new Parameter() { Name = "Accept-Encoding", Value = "gzip", Type = ParameterType.HttpHeader });
                req.Resource = "/sign_in/";
                req.Method = Method.POST;
                req.Parameters.Add(new Parameter() { Name = "email", Value = username, Type = ParameterType.GetOrPost });
                req.Parameters.Add(new Parameter() { Name = "password", Value = password, Type = ParameterType.GetOrPost });
                req.Parameters.Add(new Parameter() { Name = "csrf_token", Value = token, Type = ParameterType.GetOrPost });

                var response = await RestSharpAsync.RestSharpExecuteAsyncTask(client, req);
#if DEBUG
                DebugLogging.Append("GuokrLogin", response);
#endif
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                    throw new GuokrException() { errnum = GuokrErrorCode.VerificationFailed, errmsg = "user name password not accepted" };
            }

            // get access token
            string access_token = "";
            {
                var req = new RestRequest();
                req.Parameters.Add(new Parameter() { Name = "Accept-Encoding", Value = "gzip", Type = ParameterType.HttpHeader });
                req.Resource = "/oauth2/authorize/";
                req.Method = Method.GET;
                req.Parameters.Add(new Parameter() { Name = "client_id", Value = 32353, Type = ParameterType.GetOrPost });
                req.Parameters.Add(new Parameter() { Name = "redirect_uri", Value = "http://www.guokr.com", Type = ParameterType.GetOrPost });
                req.Parameters.Add(new Parameter() { Name = "response_type", Value = "cookie", Type = ParameterType.GetOrPost });
                req.Parameters.Add(new Parameter() { Name = "suppress_prompt", Value = 1, Type = ParameterType.GetOrPost });

                var response = await RestSharpAsync.RestSharpExecuteAsyncTask(client, req);
#if DEBUG
                DebugLogging.Append("GuokrLogin", response);
#endif
                if (response.StatusCode != HttpStatusCode.OK)
                    throw new GuokrException() { errnum = GuokrErrorCode.VerificationInternalError, errmsg = "cannot get access token" };

                try
                {
                    var cookies = client.CookieContainer.GetCookies(new Uri("https://account.guokr.com", UriKind.Absolute));
                    var cookie = cookies["_32353_access_token"];
                    var ukey = cookies["_32353_ukey"].Value;
                    access_token = cookie.Value;
                    if (access_token == "")
                        throw new Exception();
                    ViewModelLocator.ApplicationSettingsStatic.GuokrAccountProfile = new GuokrUserLogin()
                    {
                        access_token = access_token,
                        username = username,
                        ukey = ukey,
                        password = password
                    };
                    WwwClient.CookieContainer = client.CookieContainer;
                }
                catch
                {
                    throw new GuokrException() { errnum = GuokrErrorCode.VerificationInternalError, errmsg = "cannot get access token from cookie" };
                }
            }
        }

        public static async Task VerifyAccountV3AndRefreshIfNecessary(bool RequireStrongAuth = false)
        {
            var aps = ViewModelLocator.ApplicationSettingsStatic;
            if (aps.GuokrAccountLoginStatus)
            {
                if (aps.GuokrAccountProfile.expire_dt < DateTime.Now.AddDays(3))
                    await RefreshToken();
            }
            else
                if (RequireStrongAuth)
                    throw new GuokrException() { errnum = GuokrErrorCode.LoginRequired, error = "哥们，还没登录吧？" };

#if DEBUG
            Debug.WriteLine("access_token expires in " + (aps.GuokrAccountProfile.expire_dt - DateTime.Now).TotalHours.ToString() + "h");
#endif
        }
        public static async Task RefreshToken()
        {
            var client = new RestClient("https://account.guokr.com");
            var req = NewJsonRequest();
            req.Method = Method.POST;
            req.Resource = "/oauth2/token/";

            var aps = ViewModelLocator.ApplicationSettingsStatic.GuokrAccountProfile;
            req.AddParameter(new Parameter() { Name = "grant_type", Value = "refresh_token", Type = ParameterType.GetOrPost });
            req.AddParameter(new Parameter() { Name = "client_id", Value = 32380, Type = ParameterType.GetOrPost });
            req.AddParameter(new Parameter() { Name = "client_secret", Value = "9b4565d2b40ad9c3d61e42437d1e257d736795ab", Type = ParameterType.GetOrPost });
            req.AddParameter(new Parameter() { Name = "refresh_token", Value = aps.refresh_token, Type = ParameterType.GetOrPost });

            var response = await RestSharpAsync.RestSharpExecuteAsyncTask<GuokrOauthTokenResponse>(client, req);
            ProcessError(response);
            if (response.Data != null)
            {
                aps.refresh_token = response.Data.refresh_token;
                aps.access_token = response.Data.access_token;
                aps.expire_dt = DateTime.Now.AddSeconds(response.Data.expires_in);
                aps.nickname = response.Data.nickname;
                aps.ukey = response.Data.ukey;
            }
        }

        public static async Task PostCommentV2(article_base a, string comment)
        {
            await VerifyAccountV3AndRefreshIfNecessary(RequireStrongAuth: true);

            var client = ApiClient;
            var req = NewJsonRequest();
            req.Method = Method.POST;

            comment += ViewModelLocator.ApplicationSettingsStatic.CodedSignatureString;

            if (a.object_name == "article")
            {
                req.Resource = "/minisite/article_reply.json";
                req.AddParameter(new Parameter() { Name = "article_id", Value = a.id, Type = ParameterType.GetOrPost });
            }
            else if (a.object_name == "post")
            {
                req.Resource = "/group/post_reply.json";
                req.AddParameter(new Parameter() { Name = "post_id", Value = a.id, Type = ParameterType.GetOrPost });
            }
            else
                throw new NotImplementedException();

            req.AddParameter(new Parameter() { Name = "content", Value = comment, Type = ParameterType.GetOrPost });
            req.AddParameter(new Parameter() { Name = "access_token", Value = ViewModelLocator.ApplicationSettingsStatic.GuokrAccountProfile.access_token, Type = ParameterType.GetOrPost });

            var response = await RestSharpAsync.RestSharpExecuteAsyncTask<PostReplyResponse>(client, req);
            ProcessError(response);
        }
        public static async Task PostCommentV3(article_base a, string comment)
        {
            await VerifyAccountV3AndRefreshIfNecessary();
            var client = WwwClient;
            var req = NewJsonRequest();
            req.Resource = "/" + a.object_name + "/" + a.id;
            req.Method = Method.POST;

            comment += ViewModelLocator.ApplicationSettingsStatic.CodedSignatureString;

            string t = await GetCSRFTokenV2(req.Resource);
            req.Resource += "/reply/";
            req.AddParameter(new Parameter() { Name = "content", Value = comment, Type = ParameterType.GetOrPost });
            req.AddParameter(new Parameter() { Name = "csrf_token", Value = t, Type = ParameterType.GetOrPost });

            var response = await RestSharpAsync.RestSharpExecuteAsyncTask(client, req);
            ProcessError(response);
            if (response.StatusCode != HttpStatusCode.OK)
                throw new GuokrException() { errnum = GuokrErrorCode.CallFailure, errmsg = response.Content.Substring(0, 256) };
        }
        private static async Task<string> GetCSRFTokenV2(string path)
        {
            string token;
            var client = WwwClient;
            var req = new RestRequest();
            req.Resource = path;
            req.Method = Method.GET;
            var response = await RestSharpAsync.RestSharpExecuteAsyncTask(client, req);
            var match = Regex.Match(response.Content, @"input.*csrf_token.*(\d{14}##\w{40})");
            if (match.Success)
                token = match.Groups[1].Value;
            else
                throw new GuokrException() { errnum = GuokrErrorCode.VerificationInternalError, errmsg = "get csrf_token failed" };

            return token;
        }

        public static async Task DeleteCommentV2(comment c)
        {
            await VerifyAccountV3AndRefreshIfNecessary();
            var client = ApiClient;
            var req = NewJsonRequest();

            if (c.parent_object_name == "article")
                req.Resource = "minisite/article_reply.json";
            else if (c.parent_object_name == "post")
                req.Resource = "group/post_reply.json";
            req.Method = Method.DELETE;

            req.AddParameter(new Parameter() { Name = "reply_id", Value = c.reply_id, Type = ParameterType.GetOrPost });
            req.AddParameter(new Parameter() { Name = "access_token", Value = ViewModelLocator.ApplicationSettingsStatic.GuokrAccountProfile.access_token, Type = ParameterType.GetOrPost });

            var response = await RestSharpAsync.RestSharpExecuteAsyncTask(client, req);
            try
            {
                ProcessError(response);
                Messenger.Default.Send<DeleteCommentComplete>(new DeleteCommentComplete() { comment = c });
            }
            catch (GuokrException e)
            {
                Messenger.Default.Send<DeleteCommentComplete>(new DeleteCommentComplete() { comment = c, Exception = e });
            }
        }

        #region groups (html parsing based)
#if false
        public static async Task<IEnumerable<GuokrPost>> GetGroupPosts(GuokrGroup g, int page = 0)
        {
            var client = new RestClient("http://www.guokr.com");
            var req = new RestRequest();
            req.Method = Method.GET;
            req.Resource = g.path;
            if (page != 0)
                req.Parameters.Add(new Parameter() { Name = "page", Value = page, Type = ParameterType.GetOrPost });

            Dictionary<string, string> kvp = new Dictionary<string, string>();
            kvp.Add("ulclass", "titles");
            kvp.Add("liname", "h2");
            return await _getPosts(client, req, kvp);
        }
#endif

        public static async Task<List<GuokrPost>> GetLatestPostsV3(int pagesize = 20, int offset = 0)
        {
            var req = NewJsonRequest();
            req.Method = Method.GET;
            req.Resource = "group/post.json";

            var aps = ViewModelLocator.ApplicationSettingsStatic;
            try
            {
                if (aps.GuokrAccountLoginStatus)
                {
                    await VerifyAccountV3AndRefreshIfNecessary();

                    req.Parameters.Add(new Parameter() { Name = "access_token", Value = aps.GuokrAccountProfile.access_token, Type = ParameterType.GetOrPost });
                    req.Parameters.Add(new Parameter() { Name = "retrieve_type", Value = "recent_replies", Type = ParameterType.GetOrPost });
                }
                else
                    throw new Exception();
            }
            catch
            {
                req.Parameters.Add(new Parameter() { Name = "retrieve_type", Value = "hot_post", Type = ParameterType.GetOrPost });
            }

            req.Parameters.Add(new Parameter() { Name = "limit", Value = pagesize, Type = ParameterType.GetOrPost });
            req.Parameters.Add(new Parameter() { Name = "offset", Value = offset, Type = ParameterType.GetOrPost });

            var resp = await RestSharpAsync.RestSharpExecuteAsyncTask<LatestPostResponse>(ApiClient, req);
            ProcessError(resp);

            return resp.Data.ToPostList();
        }

#if false
        public static async Task<List<GuokrPost>> GetLatestPostsV2(int page = 0)
        {
            var req = new RestRequest();
            req.Method = Method.GET;

            var aps = ViewModelLocator.ApplicationSettingsStatic;
            if (aps.GuokrAccountLoginStatus)
            {
                req.Resource = "/group/user/recent_replies/";
                if (!IsVerified)
                    await VerifyAccountV3();
            }
            else
                req.Resource = "/group/hot_posts/";

            if (page != 0)
                req.Parameters.Add(new Parameter() { Name = "page", Value = page + 1, Type = ParameterType.GetOrPost });
            req.Parameters.Add(new Parameter() { Name = "Accept-Encoding", Value = "gzip", Type = ParameterType.HttpHeader });
            req.Parameters.Add(new Parameter() { Name = "_", Value = (DateTime.Now.Second + DateTime.Now.Minute * 60) / 7, Type = ParameterType.GetOrPost });

            Dictionary<string, string> kvp = new Dictionary<string, string>();
            kvp.Add("ul", @"//ul[@class=""titles""]");
            kvp.Add("title", @"/li/h3[@class=""titles-txt""]/a[@href!=""/blog/""]");
            kvp.Add("reply_count", @"//div[@class=""titles-r-grey""]");
            kvp.Add("group", @"/li/p/span[1]/a");
            kvp.Add("posted_by", @"/li/p/span[3]/a");
            kvp.Add("replied_dt", @"//span[@class=""titles-b-r""]");

            return await _getPosts(WwwClient, req, kvp);
        }
        static internal async Task<List<GuokrPost>> _getPosts(RestClient client, RestRequest req,
            Dictionary<string, string> xpath, GuokrGroup group = null)
        {
            var resp = await RestSharpAsync.RestSharpExecuteAsyncTask(client, req);

            List<GuokrPost> ress = new List<GuokrPost>();

#if WP8
            await Task.Run(() =>
#else
            await TaskEx.Run(() =>
#endif
                {
                    var html = resp.Content;
                    var doc = new HtmlDocument();
                    doc.LoadHtml(html);
                    var ul = doc.DocumentNode.SelectSingleNode(xpath["ul"]);

                    if (!(xpath.ContainsKey("title") && xpath.ContainsKey("reply_count")
                        && xpath.ContainsKey("posted_by")
                        && xpath.ContainsKey("replied_dt") && xpath.ContainsKey("group")
                        ))
                        throw new NotImplementedException("xpath expressions not sufficient");

                    if (ul == null)
                        return;

                    var titles = ul.SelectNodes(ul.XPath + xpath["title"]);
                    var reply_counts = ul.SelectNodes(ul.XPath + xpath["reply_count"]);
                    var posted_bys = ul.SelectNodes(ul.XPath + xpath["posted_by"]);
                    var replied_dts = ul.SelectNodes(ul.XPath + xpath["replied_dt"]);
                    var groups = ul.SelectNodes(ul.XPath + xpath["group"]);

                    if (titles == null || titles.Count <= 1)
                        return;

                    if (titles.Count != reply_counts.Count
                        || titles.Count != posted_bys.Count
                        || titles.Count != replied_dts.Count
                        || titles.Count != groups.Count)
                        throw new ArgumentOutOfRangeException();

                    for (int i = 0; i < titles.Count; i++)
                    {
                        var p = new GuokrPost();
                        ress.Add(p);
                        try
                        {
                            if (xpath.ContainsKey("title"))
                            {
                                var title = titles[i];
                                p.title = HtmlEntity.DeEntitize(title.InnerText);
                                p.wwwurl = GetAttribute(title, "href");
                            }

                            if (xpath.ContainsKey("reply_count"))
                                p.reply_count = Convert.ToInt32(reply_counts[i].InnerText);

                            if (group != null)
                                p.group = group;
                            else if (xpath.ContainsKey("group"))
                            {
                                p.group = new GuokrGroup();
                                var grouplink = groups[i];
                                p.group.name = grouplink.InnerText;
                                p.group.path = grouplink.Attributes["href"].Value;
                            }
                            else
                                p.group = null;

                            if (xpath.ContainsKey("posted_by"))
                            {
                                p.posted_by = new GuokrUser();
                                var n = posted_bys[i];
                                p.posted_by.nickname = n.InnerText;
                                p.posted_by.uri = GetAttribute(n, "href");
                            }

                            if (xpath.ContainsKey("replied_dt"))
                            {
                                var dt = replied_dts[i].InnerText;
                                var match = Regex.Match(dt, @"\d{4}-\d{1,2}-\d{1,2} \d{2}:\d{2}:\d{2}");
                                p.replied_dt = match.Success && match.Groups.Count > 0 ? match.Groups[1].Value : "";
                            }
                        }
                        catch (Exception e)
                        {
                            DebugLogging.Append("exception", e.Message, "");
                        }

                    }

                });

            return ress;
        }
#endif

#if false
        public static async Task<HtmlNode> GetPostContent(GuokrPost p)
        {
            var req = new RestRequest();
            req.Resource = p.m_url;
            req.Method = Method.GET;

            buf.SetBufToInProgress(req.Resource);

            var resp = await RestSharpAsync.RestSharpExecuteAsyncTask(WwwClient, req);
            ProcessError(resp);

            var doc = new HtmlDocument();
            doc.LoadHtml(resp.Content);
            var n = doc.DocumentNode.SelectSingleNode(@"//div[@class=""post""]");
            if (p.wwwurl.Contains("post"))
            {
                //n.SelectSingleNode(@"//div[@id=""share""]").Remove();
                var k = n.SelectSingleNode(@"//div[@class=""gpack post-txt""]");
                n.RemoveChild(k);
                n.AppendChildren(k.ChildNodes);
                var m = n.SelectSingleNode(@"//div[@class=""post-pic""]");
                string s = m.InnerHtml;
                n.SelectSingleNode(@"//div[@class=""post-info""]").PrependChild(HtmlNode.CreateNode(@"<p class=""fl"">" + s + @"</p>"));
                m.Remove();
            }

            if (string.IsNullOrEmpty(p.parent_name))
            {
                var s = doc.DocumentNode.SelectSingleNode(@"//div[@class=""gbreadcrumb""]/ul/li[2]/a");
                if (s != null)
                {
                    p.group = new GuokrGroup()
                    {
                        name = s.InnerText.Trim(new char[] { '\n', ' ', '\t' }),
                        path = s.GetAttributeValue("href", "")
                    };
                }
            }

#if WP8
            Task.Run(() => ParsePostComments(req.Resource, doc));
#else
            TaskEx.Run(() => ParsePostComments(req.Resource, doc));
#endif

            return n;
        }
        public static async Task<string> GetPostContentString(GuokrPost p)
        {
            var t = await GetPostContent(p);
            if (t == null)
                return "";
            return t.OuterHtml;
        }

        static string GetClass(HtmlNode n)
        {
            return GetAttribute(n, "class");
        }
        static string GetAttribute(HtmlNode n, string attrname)
        {
            if (n.Attributes[attrname] != null)
                return n.Attributes[attrname].Value;
            else
                return "";
        }
#endif
        #endregion

        #region post comments
#if false
        public static async Task<List<comment>> GetCommentsV3(GuokrObjectWithId obj, int offset = 0, int limit = 10)
        {
            int pagecount = 50;
            int page = offset / pagecount;
            offset %= pagecount;

            string path = "/post/" + obj.id.ToString() + "/";
            if (page > 0)
                path += "?page=" + (page + 1).ToString();

            bool needRefresh = false;
            if (buf.GetStatus(path) == BufferStatus.Completed && offset >= buf.GetBufLength(path))
                needRefresh = true;
            if (needRefresh)
            {
                var post = obj as GuokrPost;
                await GetPostDetail(post);
                if (post.reply_count / pagecount < page
                    || offset + page * pagecount >= post.reply_count)
                    return new List<comment>();
                buf.RefreshBuf(path);
            }

#if DEBUG
            Deployment.Current.Dispatcher.BeginInvoke(() =>
                        MessageBox.Show(string.Format("offset, {0}; limit, {1}; page, {2}; buf.getbuflength, {3}; needRefresh, {4}",
                offset,
                limit,
                page,
                buf.GetBufLength(path),
                needRefresh.ToString())));
#endif

            switch (buf.GetStatus(path))
            {
                case BufferStatus.NotAvailable:
                case BufferStatus.Failed:
                    buf.SetBufToInProgress(path);

                    var req = new RestRequest();
                    req.Resource = path;
                    req.Method = Method.GET;
                    if (needRefresh)
                    {
                        if (req.Resource.Contains("?"))
                            req.Resource += "&";
                        else
                            req.Resource += "?";
                        req.Resource += "_=" + (DateTime.Now.Second + DateTime.Now.Minute * 60).ToString();
                    }

                    var resp = await RestSharpAsync.RestSharpExecuteAsyncTask(WwwClient, req);
                    ProcessError(resp);
                    if (resp.StatusCode != HttpStatusCode.OK)
                        throw new GuokrException() { errmsg = "Can't retrieve html for " + path, errnum = GuokrErrorCode.CallFailure };

                    var doc = new HtmlDocument();
                    doc.LoadHtml(resp.Content);
                    ParsePostComments(path, doc);

                    return await buf.SafeGetBufRange(path, offset, limit);
                case BufferStatus.InProgress:
                case BufferStatus.Completed:
                    return await buf.SafeGetBufRange(path, offset, limit);
                default:
                    throw new NotImplementedException();
            }
        }
        private static MyBuffer<string, comment> _b;
        private static MyBuffer<string, comment> buf
        {
            get
            {
                if (_b == null)
                    _b = new MyBuffer<string, comment>();
                return _b;
            }
        }
        private static void ParsePostComments(string path, HtmlDocument doc)
        {
            if (!buf.ContainsKey(path) || buf.GetStatus(path) != BufferStatus.InProgress)
                return;

            buf.PutBuffer(path, InternalParsePostComments(doc));
        }
        private static List<comment> InternalParsePostComments(HtmlDocument htmlDocument)
        {
            var xpath = new Dictionary<string, string>() {
            { "ul", @"//ul[@class=""cmts-list""]"},
            { "content" , @"//div[contains(@class,""cmt-content"")]"},
            { "date_create" , @"//span[@class=""cmt-info""]"},
            { "floor" , @"//span[@class=""cmt-floor""]"},
            { "head_48" , @"//div[contains(@class,""cmt-img"")]//img"},
            { "home_url" , @"//div[contains(@class,""cmt-img"")]/a"},
            { "nickname" , @"//a[@class=""cmt-author cmtAuthor""]"},
            { "reply_id" , @"//li"} };

            var ul = htmlDocument.DocumentNode.SelectSingleNode(xpath["ul"]);
            if (ul == null)
                return new List<comment>();

            var contents = ul.SelectNodes(ul.XPath + xpath["content"]);

            if (contents == null || contents.Count == 0)
                return new List<comment>();

            var date_creates = ul.SelectNodes(ul.XPath + xpath["date_create"]);
            var floors = ul.SelectNodes(ul.XPath + xpath["floor"]);
            var head_48s = ul.SelectNodes(ul.XPath + xpath["head_48"]);
            var home_urls = ul.SelectNodes(ul.XPath + xpath["home_url"]);
            var nicknames = ul.SelectNodes(ul.XPath + xpath["nickname"]);
            var reply_ids = ul.SelectNodes(ul.XPath + xpath["reply_id"]);

            if (contents.Count != date_creates.Count
                || contents.Count != floors.Count
                || contents.Count != head_48s.Count
                || contents.Count != home_urls.Count
                || contents.Count != nicknames.Count
                || contents.Count != reply_ids.Count)
                throw new ArgumentOutOfRangeException();

            List<comment> ress = new List<comment>();
            for (int i = 0; i < contents.Count; i++)
            {
                var p = new comment();
                ress.Add(p);
                try
                {
                    p.contentHtml = contents[i].InnerHtml; // don't do DeEntitize because this will be loaded as HTMLDocument
                    p.date_create = date_creates[i].InnerText;
                    if (p.date_create.Contains("刚刚"))
                        p.date_create = ServerNow.ToString();
                    else if (p.date_create.Contains("今天"))
                        p.date_create = p.date_create.Replace("今天", ServerNow.Date.ToShortDateString());
                    else if (p.date_create.Contains("前"))
                    {
                        string[] a = new string[] { "秒前", "分钟前", "小时前" };
                        int m = 1;
                        foreach (var item in a)
                        {
                            if (p.date_create.Contains(item))
                            {
                                p.date_create = p.date_create.Replace(item, "");
                                break;
                            }
                            else
                                m = m * 60;
                        }
                        if (m > 3600)
                            throw new NotImplementedException();
                        TimeSpan ts = TimeSpan.FromSeconds(m * Convert.ToInt32(p.date_create));
                        p.date_create = (ServerNow - ts).ToString();
                    }
                    p.floor = Convert.ToInt32(floors[i].InnerText.Substring(0, floors[i].InnerText.Length - 1));
                    p.head_48 = head_48s[i].GetAttributeValue("src", "");
                    p.userUrl = home_urls[i].GetAttributeValue("href", "");
                    p.nickname = nicknames[i].InnerText;
                    p.reply_id = Convert.ToInt64(reply_ids[i].GetAttributeValue("id", "0"));
                }
                catch (Exception e)
                {
                    DebugLogging.Append("exception", e.Message, "");
                }

            }
            return ress;
        }
#endif
        #endregion

        public static async Task<string> GetArticleV2(article a, bool OverrideCache = false)
        {
#if DEBUG
            var local = Windows.Storage.ApplicationData.Current.LocalFolder;
            var localFolder = await local.CreateFolderAsync("cache", Windows.Storage.CreationCollisionOption.OpenIfExists);
            var filename = a.id.ToString() + ".htmlcache";
            if (!OverrideCache)
            {
                try
                {
                    using (var fs = await localFolder.OpenStreamForReadAsync(filename))
                    {
                        if (fs.Length <= 0)
                            throw new ArgumentException();
                        using (var sr = new StreamReader(fs))
                        {
                            return await sr.ReadToEndAsync();
                        }
                    }
                }
                catch
                { }
            }
#endif

            var req = NewJsonRequest();
            req.Resource = "/apis/minisite/article/{id}.json";
            req.Method = Method.GET;
            req.AddParameter(new Parameter() { Name = "id", Value = a.id, Type = ParameterType.UrlSegment });

            var resp = await RestSharpAsync.RestSharpExecuteAsyncTask<GetArticleResponse>(WwwClient, req);
            ProcessError(resp);

            string html = "";
            if (resp.Data != null)
            {
                html = @"<div class=""article-head""><h1>"
                    + resp.Data.result.title
                    + "</h1><p style=\"color: #999;\">"
                    + resp.Data.result.author.nickname + " 发表于 " + Common.HumanReadableTime(resp.Data.result.DatePublished)
                    + "</p></div>"
                    + @"<div class=""article-content"">"
                    + resp.Data.result.content
                    + "</div>";
                a.CommentCount = resp.Data.result.replies_count;
                a.minisite_name = (resp.Data.result.minisite == null ? "科学人" : resp.Data.result.minisite.name);
                if (string.IsNullOrEmpty(a.title))
                {
                    a.title = resp.Data.result.title;
                    a.Abstract = resp.Data.result.summary;
                    a.HtmlContent = html;
                    a.pic = string.IsNullOrEmpty(resp.Data.result.image)
                        ? resp.Data.result.small_image
                        : resp.Data.result.image;
                }
            }

#if DEBUG
            var file = await localFolder.CreateFileAsync(filename, CreationCollisionOption.OpenIfExists);
            var bytes = Encoding.UTF8.GetBytes(html);
            using (var stream = await file.OpenStreamForWriteAsync())
            {
                try
                {
                    stream.WriteAsync(bytes, 0, html.Length);
                }
                catch
                {
                    file.DeleteAsync();
                }
            }
#endif

            return html;
        }
        public static async Task<List<recommend_article>> GetRecommendedArticlesV2()
        {
            var req = NewJsonRequest();
            req.Resource = "flowingboard/item/editor_recommend.json";
            req.Method = Method.GET;

#if false
            req.Parameters.Add(new Parameter() { Name = "limit", Value = pagesize, Type = ParameterType.GetOrPost });
            req.Parameters.Add(new Parameter() { Name = "offset", Value = offset, Type = ParameterType.GetOrPost });
            req.Parameters.Add(new Parameter() { Name = "retrieve_type", Value = "by_minisite", Type = ParameterType.GetOrPost });
            if (!string.IsNullOrEmpty(minisite_key))
                req.Parameters.Add(new Parameter() { Name = "minisite_key", Value = minisite_key, Type = ParameterType.GetOrPost });
#endif

            var resp = await RestSharpAsync.RestSharpExecuteAsyncTask<RecommendedArticlesResponse>(ApiClient, req);
            ProcessError(resp);

            return resp.Data.ToArticleList();
        }
        public static async Task<List<article>> GetLatestArticlesV2(int pagesize = 7, int offset = 0, string minisite_key = "")
        {
            var req = NewJsonRequestCallback();
            req.Resource = "apis/minisite/article.js";
            req.Method = Method.GET;

            req.Parameters.Add(new Parameter() { Name = "limit", Value = pagesize, Type = ParameterType.GetOrPost });
            req.Parameters.Add(new Parameter() { Name = "offset", Value = offset, Type = ParameterType.GetOrPost });
            req.Parameters.Add(new Parameter() { Name = "retrieve_type", Value = "by_subject", Type = ParameterType.GetOrPost });
            if (!string.IsNullOrEmpty(minisite_key))
                req.Parameters.Add(new Parameter() { Name = "minisite_key", Value = minisite_key, Type = ParameterType.GetOrPost });

            var resp = await RestSharpAsync.RestSharpExecuteAsyncTask<LatestArticlesResponse>(WwwClient, req);
            ProcessError(resp);
            if (resp != null)
                ViewModelLocator.ApplicationSettingsStatic.MaxArticleNumber = resp.Data.total;
#if false
            foreach (var item in resp.Data)
            {
                await GuokrApi.GetArticleInfo(item);
            }
#endif

            return resp.Data.ToArticleList();
        }
#if false
        public static async Task<List<article>> GetMinisiteArticles(int minisite_id, int offset = 0)
        {
            var req = NewJsonRequest();
            req.Resource = "api/content/minisite_article_list/";
            req.Method = Method.POST;

            req.AddParameter(new Parameter() { Name = "minisite_id", Value = minisite_id, Type = ParameterType.GetOrPost });
            req.Parameters.Add(new Parameter() { Name = "count", Value = 8, Type = ParameterType.GetOrPost });
            req.Parameters.Add(new Parameter() { Name = "offset", Value = offset, Type = ParameterType.GetOrPost });

            var resp = await RestSharpAsync.RestSharpExecuteAsyncTask<List<article>>(Client, req);
            ProcessError(resp);
            return resp.Data;
        }
#endif
        public static async Task<List<comment>> GetCommentsV2(GuokrObjectWithId obj, int offset = 0, int limit = 10)
        {
            var req = NewJsonRequest();
            req.Method = Method.GET;

            if (obj.object_name == "article")
            {
                req.Resource = "minisite/article_reply.json";
                req.Parameters.Add(new Parameter() { Name = "article_id", Value = obj.id, Type = ParameterType.GetOrPost });
            }
            else if (obj.object_name == "post")
            {
#if false
                req.Resource = "group/post_reply.json";
                req.Parameters.Add(new Parameter() { Name = "post_id", Value = obj.id, Type = ParameterType.GetOrPost });
#endif
                req.Resource = "group/post_reply.json";
                req.Parameters.Add(new Parameter() { Name = "post_id", Value = obj.id, Type = ParameterType.GetOrPost });
                req.Parameters.Add(new Parameter() { Name = "retrieve_type", Value = "by_post", Type = ParameterType.GetOrPost });
            }
            else
            {
                throw new GuokrException() { errnum = GuokrErrorCode.InternalError, errmsg = "object_name = " + obj.object_name + " is not supported" };
            }

            req.Parameters.Add(new Parameter() { Name = "limit", Value = limit, Type = ParameterType.GetOrPost });
            req.Parameters.Add(new Parameter() { Name = "offset", Value = offset, Type = ParameterType.GetOrPost });
            if (ViewModelLocator.ApplicationSettingsStatic.GuokrAccountLoginStatus)
                req.AddParameter(new Parameter() { Name = "access_token", Value = ViewModelLocator.ApplicationSettingsStatic.GuokrAccountProfile.access_token, Type = ParameterType.GetOrPost });

            var resp = await RestSharpAsync.RestSharpExecuteAsyncTask<GetArticleCommentsResponse>(ApiClient, req);
            ProcessError(resp);
            if (resp.Data != null)
                return resp.Data.ToCommentList(offset);
            else
                throw new GuokrException() { errnum = GuokrErrorCode.CallFailure, errmsg = resp.Content };
        }

        public static async Task<string> GetPostDetail(GuokrPost p)
        {
            if (p == null)
                throw new ArgumentNullException();
            var req = NewJsonRequest();
            req.Resource = "apis/group/post/{post_id}.json";
            req.Method = Method.GET;
            req.AddParameter(new Parameter() { Name = "post_id", Value = p.id, Type = ParameterType.UrlSegment });

            var resp = await RestSharpAsync.RestSharpExecuteAsyncTask<GetPostDetailResponse>(WwwClient, req);
            ProcessError(resp);
            if (resp.Data != null)
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        p.reply_count = resp.Data.result.replies_count;
                    });
            else
                return "";

            if (string.IsNullOrEmpty(p.parent_name))
            {
                p.group = new GuokrGroup()
                {
                    name = resp.Data.result.group.name,
                    path = resp.Data.result.group.url
                };
            }

            string html = "<div class=\"post\">\n"
                        + "<h1 id=\"articleTitle\">" + resp.Data.result.title + "</h1>\n"
                        + "<div class=\"post-pic\"><img id=\"articleAuthorImg\" style=\"display: block !important;\" src=\"" + resp.Data.result.author.avatar.normal + "\"/></div>\n"
                        + "<div class=\"post-info\">"
                                + "<a id=\"articleAuthor\" href=\"" + resp.Data.result.author.url + "\">" + resp.Data.result.author.nickname + "</a>\n"
                                + "<p>" + Common.HumanReadableTime(DateTime.Parse(resp.Data.result.date_created)) + "</p>\n"
                        + "</div>"
                        + "<div class=\"post-detail\" style=\"margin-top: 10px !important;\"><p/>"
                                + Common.PostBBParser.ToHtml(
                                        Regex.Replace(resp.Data.result.content.Replace("\r", ""),
                                                        @"\x5Btable\x5D.*?\x5B\/table\x5D",
                                                        m=> Regex.Replace(m.Value,
                                                                @"(\x5Btable\x5D.*?\x5btd\x5d)|(\x5B\/td\x5D.*?\x5btd\x5d)|(\x5B\/td\x5D.*?\x5b\/table\x5d)",
                                                                mm => mm.Value.Replace("\n", ""), RegexOptions.Singleline), RegexOptions.Singleline)
                                        .Replace("\n\n", "[br]").Replace("\n", "[br]"))
                        + "</div>"
                + "</div>";

            if (string.IsNullOrEmpty(p.title))
            {
                p.title = resp.Data.result.title;
                p.HtmlContent = html;
                p.CommentCount = resp.Data.result.replies_count;
            }
            return html;
        }

        public static async Task GetArticleInfo(article_base a)
        {
#if false
            var req = NewJsonRequest();
            req.Resource = "api/content/article_info/";
            req.Method = Method.POST;

            req.Parameters.Add(new Parameter() { Name = "obj_id", Value = a.id, Type = ParameterType.GetOrPost });
            var resp = await RestSharpAsync.RestSharpExecuteAsyncTask<List<GuokrArticleInfo>>(WwwClient, req);
            ProcessError(resp);
            if (resp.Data != null && resp.Data.Count > 0)
            {
                //Deployment.Current.Dispatcher.BeginInvoke(() => a.CommentCount = resp.Data[0].reply_count);
                a.CommentCount = resp.Data[0].reply_count;
            }
#endif
        }

        public static async Task GetRNNumber()
        {
            if (!ViewModelLocator.ApplicationSettingsStatic.GuokrAccountLoginStatus)
                return;

            await VerifyAccountV3AndRefreshIfNecessary();

            var req = NewJsonRequest();
            req.Resource = "apis/community/rn_num.json";
            req.Method = Method.GET;
            req.AddParameter(new Parameter() { Name = "access_token", Value = ViewModelLocator.ApplicationSettingsStatic.GuokrAccountProfile.access_token, Type = ParameterType.GetOrPost });

            var resp = await RestSharpAsync.RestSharpExecuteAsyncTask<GuokrRnResponse>(WwwClient, req);
            ProcessError(resp);
            if (resp.Data.result != null)
                ViewModelLocator.MainStatic.GuokrRnNumber = resp.Data.result;
        }

        public static async Task ResetRNNumber(Int64 notice_id = 0)
        {
            if (!ViewModelLocator.ApplicationSettingsStatic.GuokrAccountLoginStatus)
                return;

            var req = NewJsonRequest();
            req.Resource = "apis/community/notice_ignore.json";
            req.Method = Method.PUT;
            req.AddParameter(new Parameter() { Name = "access_token", Value = ViewModelLocator.ApplicationSettingsStatic.GuokrAccountProfile.access_token, Type = ParameterType.GetOrPost });
            if (notice_id != 0)
                req.AddParameter(new Parameter() { Name = "nid", Value = notice_id, Type = ParameterType.GetOrPost });
            else
                req.AddParameter(new Parameter() { Name = "nid", Value = "", Type = ParameterType.GetOrPost });

            // don't parse the response
            var resp = await RestSharpAsync.RestSharpExecuteAsyncTask(WwwClient, req);
            ProcessError(resp);
            if (resp.StatusCode == HttpStatusCode.OK)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    ViewModelLocator.MainStatic.GuokrRnNumber = new GuokrRnNum() { r = 0, n = 0 };
                    ViewModelLocator.MainStatic.NoticeList.ArticleList.Clear();
                });
            }
        }

        public static async Task<List<GuokrNotice>> GetNoticeV2(int offset = 0, int limit = 30)
        {
            if (!ViewModelLocator.ApplicationSettingsStatic.GuokrAccountLoginStatus)
                return null;

            var req = NewJsonRequest();
            req.Resource = "apis/community/notice.json";
            req.Method = Method.GET;
            req.AddParameter(new Parameter() { Name = "access_token", Value = ViewModelLocator.ApplicationSettingsStatic.GuokrAccountProfile.access_token, Type = ParameterType.GetOrPost });

            req.AddParameter(new Parameter() { Name = "limit", Value = limit, Type = ParameterType.GetOrPost });
            if (offset > 0)
                req.AddParameter(new Parameter() { Name = "offset", Value = offset, Type = ParameterType.GetOrPost });

            var resp = await RestSharpAsync.RestSharpExecuteAsyncTask<GuokrNoticeResponse>(WwwClient, req);
            ProcessError(resp);
            if (resp.Data != null)
                ViewModelLocator.MainStatic.GuokrRnNumber.n = resp.Data.result.Count;
            return resp.Data.result;
        }
        public static async Task<Uri> GetRedirectUri(Uri uri)
        {
            var req = new RestRequest();
            req.Resource = uri.AbsolutePath;
            req.Method = Method.HEAD;

            var resp = await RestSharpAsync.RestSharpExecuteAsyncTask(WwwClient, req);
            return resp.ResponseUri;
        }
        public static async Task<GuokrObjectWithId> GetGuokrObjectFromReply(string url)
        {
            var response = await GetRedirectUri(new Uri(url, UriKind.Absolute));

            GuokrObjectWithId a = null;
            if (response.AbsolutePath.Contains("/post/"))
                a = new GuokrPost() { m_url = response.AbsolutePath };
            else if (response.AbsolutePath.Contains("/article/"))
                a = new article() { wwwurl = response.DnsSafeHost + response.AbsolutePath };
            else
                throw new NotImplementedException();

            return a;
        }
#if false
        public static async Task<string> GetRandomArticleUrl()
        {
            var client = new RestClient("http://52guokr.duapp.com");
            client.FollowRedirects = false;
            var req = new RestRequest();
            req.Resource = "/random";
            req.Method = Method.GET;
            var resp = await RestSharpAsync.RestSharpExecuteAsyncTask(client, req);
            if (resp.StatusCode != HttpStatusCode.Found)
                throw new GuokrException() { errnum = GuokrErrorCode.CallFailure, errmsg = "GetRandomArticle failure, incorrect status code obtained from 52guokr.duapp.com: " + resp.StatusCode.ToString() };
            var loc = resp.Headers.FirstOrDefault(l => l.Name == "Location");
            if (loc == null)
                throw new GuokrException() { errnum = GuokrErrorCode.CallFailure, errmsg = "GetRandomArticle failure, no redirection url obtained from 52guokr.duapp.com" };
            return loc.Value.ToString();
        }
        public static async Task<article> GetArticleFromId(int id)
        {
            var req = NewJsonRequest();
            req.Resource = "apis/minisite/article/{post_id}.json";
            req.Method = Method.GET;
            req.Parameters.Add(new Parameter() { Name = "post_id", Value = id, Type = ParameterType.UrlSegment });
            var resp = await RestSharpAsync.RestSharpExecuteAsyncTask<GetArticleResponse>(WwwClient, req);
            ProcessError(resp);
            if (resp.Data != null)
                return resp.Data.result;

        }
#endif
        public static async Task LikeComment(comment c)
        {
            await VerifyAccountV3AndRefreshIfNecessary();

            var req = NewJsonRequest();
            if (c.parent_object_name == "article")
                req.Resource = "minisite/article_reply_liking.json";
            else if (c.parent_object_name == "post")
                req.Resource = "group/post_reply_liking.json";

            req.Method = Method.POST;
            req.AddParameter(new Parameter() { Name = "access_token", Value = ViewModelLocator.ApplicationSettingsStatic.GuokrAccountProfile.access_token, Type = ParameterType.GetOrPost });
            req.AddParameter(new Parameter() { Name = "reply_id", Value = c.reply_id, Type = ParameterType.GetOrPost });

            var resp = await RestSharpAsync.RestSharpExecuteAsyncTask<GuokrResponse>(ApiClient, req);
            ProcessError(resp);

            // TODO: need to improve error handling
            if (resp.Data.ok == false)
                throw new Exception();
        }

        #region search
        public enum SearchSortOrder
        {
            ByRelevance,
            ByTime
        };
        public static async Task<List<article>> SearchArticle(string query, SearchSortOrder order = SearchSortOrder.ByRelevance, int page = 0)
        {
            var req = new RestRequest();
            req.Resource = "/search/article";
            req.Method = Method.GET;
            req.Parameters.Add(new Parameter() { Name = "wd", Value = query, Type = ParameterType.GetOrPost });
            req.Parameters.Add(new Parameter() { Name = "Accept-Encoding", Value = "gzip", Type = ParameterType.HttpHeader });
            req.Parameters.Add(new Parameter() { Name = "sort", Value = (order == SearchSortOrder.ByRelevance ? "" : "date"), Type = ParameterType.GetOrPost });
            if (page > 0)
                req.Parameters.Add(new Parameter() { Name = "page", Value = page + 1, Type = ParameterType.GetOrPost });

            var resp = await RestSharpAsync.RestSharpExecuteAsyncTask(WwwClient, req);
            ProcessError(resp);

            var htmldoc = new HtmlDocument();
            htmldoc.LoadHtml(resp.Content);
            var items = htmldoc.DocumentNode.SelectNodes(@"//li[@class=""items-post""]");
            if (items == null || items.Count == 0)
                return new List<article>();

            var result = items.Select((i) => new article()
            {
                Abstract = "..." + i.SelectSingleNode(i.XPath + @"/p[1]").InnerText + "...",
                title = i.SelectSingleNode(i.XPath + @"/h2/a").InnerText,
                minisite_name = i.SelectSingleNode(i.XPath + @"/p[2]/a").InnerText,
                id = Convert.ToInt64(Regex.Match(i.SelectSingleNode(i.XPath + @"/h2/a").Attributes["href"].Value, @"\d+").Groups[0].Value),
                posted_dt = Regex.Match(i.SelectSingleNode(i.XPath + @"/p[2]/text()[2]").InnerText, @"\d{4}-\d{1,2}-\d{1,2}").Groups[0].Value,
            });
            return result.ToList();
        }
        #endregion

    }

}
