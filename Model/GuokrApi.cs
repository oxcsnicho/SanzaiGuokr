using System;
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

namespace SanzaiGuokr.GuokrApiV2
{
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
    }
    public class RecommendedArticleInfo
    {
        public int ordinal { get; set; }
        public string parent_name { get; set; }
        public string title { get; set; }
        public string url { get; set; }
        public string image { get; set; }
        public string summary { get; set; }
        public string parent_url { get; set; }
        public string resource_url { get; set; }
        public string type { get; set; }
    }
    public class RecommendedArticlesResponse : GuokrResponse
    {
        public List<RecommendedArticleInfo> result { get; set; }

        public List<article> ToArticleList()
        {
            if (result != null)
            {
                var res = from item in this.result
			  where item.type == "article" // only consume articles
                          select new article()
                          {
                              minisite_name = item.parent_name,
                              url = item.resource_url,
                              id = Convert.ToInt64(Regex.Match(item.url, @"\d+").Value),
                              Abstract = item.summary,
                              pic = item.image,
                              title = item.title,
                          };
                return res.ToList();
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
                              minisite_name = item.minisite.name,
                              url = item.resource_url,
                              id = item.id,
                              Abstract = item.summary,
                              pic = string.IsNullOrWhiteSpace(item.image) ? item.small_image : item.image,
                              title = item.title
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
        public int id { get; set; }
        public Author author { get; set; }
    }

    public class GuokrRnNum
    {
        public int r { get; set; }
        public int n { get; set; }
        public int SumValue
        {
            get
            {
                return r + n;
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
                return !(r == 0 && n == 0);
            }
        }
    }
    public class GuokrRnResponse : GuokrResponse
    {
        public GuokrRnNum result { get; set; }
    }
    public class GuokrNotice
    {
        public string content { get; set; }
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

        private RelayCommand _vi;
        public RelayCommand ViewItem
        {
            get
            {
                if (_vi == null)
                    _vi = new RelayCommand(async () =>
                    {
                        var a = await GuokrApi.GetGuokrObjectFromReply(url);
#if DEBUG
                        if (a.object_name == "article")
                            Deployment.Current.Dispatcher.BeginInvoke(() => MessageBox.Show((a as article).url));
                        else if (a.object_name == "post")
                            Deployment.Current.Dispatcher.BeginInvoke(() => MessageBox.Show((a as GuokrPost).path));
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
                            await GuokrApi.GetPostDetail(b);
                            Messenger.Default.Send<GoToReadPost>(new GoToReadPost() { article = b });
                        }

                    });
                return _vi;
            }
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
                        ukey = i.author.ukey
                    };
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

        public override int GetErrorCode()
        {
            return (int)errnum;
        }
        public override string GetErrorMessage()
        {
            return errmsg;
        }
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

        public static bool IsVerified
        {
            get
            {
                return (WwwClient != null && WwwClient.CookieContainer != null && WwwClient.CookieContainer.Count > 0);
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

            GetRNNumber();
        }

        static string comefrome = "\n来自" + @"[url=http://windowsphone.com/s?appid=bd089a5a-b561-4155-b21b-30b9844e7ee7]"
            + Common.DeviceName()
                + "[/url]";
        public static async Task PostCommentV2(article_base a, string comment)
        {
            if (a.object_name == "post")
            {
                await PostCommentV3(a, comment);
                return;
            }
            else if (a.object_name != "article")
                throw new NotImplementedException();
            if (!IsVerified)
            {
                var aps = ViewModelLocator.ApplicationSettingsStatic;
                if (aps.GuokrAccountLoginStatus)
                    await VerifyAccountV2(aps.GuokrAccountProfile.username, aps.GuokrAccountProfile.password);
                else
                    throw new GuokrException() { errnum = GuokrErrorCode.LoginRequired };
            }
            var client = WwwClient;
            var req = NewJsonRequest();
            req.Resource = "/apis/minisite/article_reply.json";
            req.Method = Method.POST;

            comment += comefrome;

            if (a.object_name == "article")
                req.AddParameter(new Parameter() { Name = "article_id", Value = a.id, Type = ParameterType.GetOrPost });
            else
                throw new NotImplementedException();
            req.AddParameter(new Parameter() { Name = "content", Value = comment, Type = ParameterType.GetOrPost });
            req.AddParameter(new Parameter() { Name = "access_token", Value = ViewModelLocator.ApplicationSettingsStatic.GuokrAccountProfile.access_token, Type = ParameterType.GetOrPost });

            var response = await RestSharpAsync.RestSharpExecuteAsyncTask<PostReplyResponse>(client, req);
            ProcessError(response);
        }
        public static async Task PostCommentV3(article_base a, string comment)
        {
            if (!IsVerified)
            {
                var aps = ViewModelLocator.ApplicationSettingsStatic;
                if (aps.GuokrAccountLoginStatus)
                    await VerifyAccountV2(aps.GuokrAccountProfile.username, aps.GuokrAccountProfile.password);
                else
                    throw new GuokrException() { errnum = GuokrErrorCode.LoginRequired };
            }
            var client = WwwClient;
            var req = NewJsonRequest();
            req.Resource = "/" + a.object_name + "/" + a.id;
            req.Method = Method.POST;

            comment += comefrome;

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
            if (!IsVerified)
            {
                var aps = ViewModelLocator.ApplicationSettingsStatic;
                if (aps.GuokrAccountLoginStatus)
                    await VerifyAccountV2(aps.GuokrAccountProfile.username, aps.GuokrAccountProfile.password);
                else
                    throw new GuokrException() { errnum = GuokrErrorCode.LoginRequired };
            }
            var client = WwwClient;
            var req = NewJsonRequest();
            req.Resource = "/apis/minisite/article_reply.json";
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
        public static async Task<List<GuokrPost>> GetLatestPostsV2(int page = 0)
        {
            var req = new RestRequest();
            req.Method = Method.GET;

            var aps = ViewModelLocator.ApplicationSettingsStatic;
            if (aps.GuokrAccountLoginStatus)
            {
                req.Resource = "/group/user/recent_replies/";
                if (!IsVerified)
                    await VerifyAccountV2(aps.GuokrAccountProfile.username, aps.GuokrAccountProfile.password);
                else
                    throw new GuokrException() { errnum = GuokrErrorCode.LoginRequired };
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

            await TaskEx.Run(() =>
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
                                p.path = GetAttribute(title, "href");
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

        public static async Task<HtmlNode> GetPostContent(GuokrPost p)
        {
            var req = new RestRequest();
            req.Resource = new Uri(p.path).AbsolutePath;
            req.Method = Method.GET;

            buf.SetBufToInProgress(req.Resource);

            var resp = await RestSharpAsync.RestSharpExecuteAsyncTask(WwwClient, req);
            ProcessError(resp);

            var doc = new HtmlDocument();
            doc.LoadHtml(resp.Content);
            var n = doc.DocumentNode.SelectSingleNode(@"//div[@class=""post""]");
            if (p.path.Contains("post"))
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

            TaskEx.Run(() => ParsePostComments(req.Resource, doc));

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
        #endregion

        #region post comments
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
                        p.date_create = DateTime.Now.ToString();
                    else if (p.date_create.Contains("今天"))
                        p.date_create = p.date_create.Replace("今天", DateTime.Today.ToShortDateString());
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
                        p.date_create = (DateTime.Now - ts).ToString();
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
        #endregion

        public static async Task<string> GetArticleV2(article a)
        {
            var req = NewJsonRequest();
            req.Resource = a.m_url;
            req.Method = Method.GET;

            var client = ApiClient;
            var resp = await RestSharpAsync.RestSharpExecuteAsyncTask<GetArticleResponse>(client, req);
            ProcessError(resp);

            string html = "";
            if (resp.Data != null)
            {
                html = @"<div class=""article-head""><h3>"
                    + resp.Data.result.title
                    + "</h3><p>"
                    + resp.Data.result.author.nickname + "<br>发表于 " + resp.Data.result.DatePublished.ToString("yyyy-MM-dd hh:mm:ss")
                    + "</p></div>"
                    + @"<div class=""article-content"">"
                    + resp.Data.result.content
                    + "</div>";
                a.CommentCount = resp.Data.result.replies_count;
            }

            return html;
        }
        public static async Task<List<article>> GetRecommendedArticlesV2()
        {
            var req = NewJsonRequest();
            req.Resource = "flowingboard/item/home_recommend.json";
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
        public static async Task<List<article>> GetLatestArticlesV2(int pagesize = 4, int offset = 0, string minisite_key = "")
        {
            var req = NewJsonRequestCallback();
            req.Resource = "apis/minisite/article.js";
            req.Method = Method.GET;

            req.Parameters.Add(new Parameter() { Name = "limit", Value = pagesize, Type = ParameterType.GetOrPost });
            req.Parameters.Add(new Parameter() { Name = "offset", Value = offset, Type = ParameterType.GetOrPost });
            req.Parameters.Add(new Parameter() { Name = "retrieve_type", Value = "by_minisite", Type = ParameterType.GetOrPost });
            if (!string.IsNullOrEmpty(minisite_key))
                req.Parameters.Add(new Parameter() { Name = "minisite_key", Value = minisite_key, Type = ParameterType.GetOrPost });

            var resp = await RestSharpAsync.RestSharpExecuteAsyncTask<LatestArticlesResponse>(WwwClient, req);
            ProcessError(resp);
#if false
            foreach (var item in resp.Data)
            {
                await GuokrApi.GetArticleInfo(item);
            }
#endif

            GetRNNumber();

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
                return await GetCommentsV3(obj, offset, limit);
            }
            else
            {
                throw new GuokrException() { errnum = GuokrErrorCode.InternalError, errmsg = "object_name = " + obj.object_name + " is not supported" };
            }

            req.Parameters.Add(new Parameter() { Name = "limit", Value = limit, Type = ParameterType.GetOrPost });
            req.Parameters.Add(new Parameter() { Name = "offset", Value = offset, Type = ParameterType.GetOrPost });

            var resp = await RestSharpAsync.RestSharpExecuteAsyncTask<GetArticleCommentsResponse>(ApiClient, req);
            ProcessError(resp);
            if (resp.Data != null)
                return resp.Data.ToCommentList(offset);
            else
                throw new GuokrException() { errnum = GuokrErrorCode.CallFailure, errmsg = resp.Content };
        }

        public static async Task GetPostDetail(GuokrPost p)
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
                throw new GuokrException() { errnum = GuokrErrorCode.CallFailure, errmsg = "Data is null" };
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

            var req = NewJsonRequest();
            req.Resource = "apis/community/rn_num.json";
            req.Method = Method.GET;
            req.AddParameter(new Parameter() { Name = "access_token", Value = ViewModelLocator.ApplicationSettingsStatic.GuokrAccountProfile.access_token, Type = ParameterType.GetOrPost });

            var resp = await RestSharpAsync.RestSharpExecuteAsyncTask<GuokrRnResponse>(WwwClient, req);
            ProcessError(resp);
            if (resp.Data.result != null)
                ViewModelLocator.ApplicationSettingsStatic.GuokrRnNumber = resp.Data.result;
        }

        public static async Task<List<GuokrNotice>> GetNoticeV2()
        {
            if (!ViewModelLocator.ApplicationSettingsStatic.GuokrAccountLoginStatus)
                return null;

            var req = NewJsonRequest();
            req.Resource = "apis/community/notice.json";
            req.Method = Method.GET;
            req.AddParameter(new Parameter() { Name = "access_token", Value = ViewModelLocator.ApplicationSettingsStatic.GuokrAccountProfile.access_token, Type = ParameterType.GetOrPost });

            var resp = await RestSharpAsync.RestSharpExecuteAsyncTask<GuokrNoticeResponse>(WwwClient, req);
            ProcessError(resp);
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
                a = new GuokrPost() { path = response.AbsolutePath };
            else if (response.AbsolutePath.Contains("/article/"))
                a = new article() { wwwurl = response.DnsSafeHost + response.AbsolutePath };
            else
                throw new NotImplementedException();

            return a;
        }

    }

}
