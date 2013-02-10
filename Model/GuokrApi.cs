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
        private static RestClient _wwwclient = new RestClient(GuokrBaseUrlWww) { CookieContainer = new CookieContainer() };
        public static RestClient WwwClient
        {
            get
            {
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

        }
        public static async Task PostCommentV2(article_base a, string comment)
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
            req.Method = Method.POST;

            comment += "\n来自" + @"[url href=http://windowsphone.com/s?appid=bd089a5a-b561-4155-b21b-30b9844e7ee7]山寨果壳.wp[/url]";

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
            req.Resource = "/article/" + a.id + "/";
            req.Method = Method.POST;

            comment += "\n来自" + @"[url=http://windowsphone.com/s?appid=bd089a5a-b561-4155-b21b-30b9844e7ee7]山寨果壳.wp[/url]";

            string t = await GetCSRFTokenV2(req.Resource);
            req.AddParameter(new Parameter() { Name = "reply", Value = comment, Type = ParameterType.GetOrPost });
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
        public static async Task<List<GuokrPost>> GetLatestPosts(int page = 0)
        {
            var req = new RestRequest();
            req.Method = Method.GET;
            req.Resource = "/group/latest/";
            if (page != 0)
                req.Parameters.Add(new Parameter() { Name = "page", Value = page + 1, Type = ParameterType.GetOrPost });
            req.Parameters.Add(new Parameter() { Name = "Accept-Encoding", Value = "gzip", Type = ParameterType.HttpHeader });

            Dictionary<string, string> kvp = new Dictionary<string, string>();
            kvp.Add("ul", @"//ul[@class=""titles cb""]");
            kvp.Add("title", @"//h3/a[@href!=""/blog/""]");
            kvp.Add("reply_count", @"//span[@class=""titles-r-grey""]");
            kvp.Add("group", @"//span[@class=""titles-b-l""]/a[1]");
            kvp.Add("posted_by", @"//span[@class=""titles-b-l""]/a[2]");
            kvp.Add("replied_dt", @"//span[@class=""titles-b-r""]");

            return await _getPosts(WwwClient, req, kvp);
        }
        static internal async Task<List<GuokrPost>> _getPosts(RestClient client, RestRequest req,
            Dictionary<string, string> xpath, GuokrGroup group = null)
        {
            if (!IsVerified)
            {
                var aps = ViewModelLocator.ApplicationSettingsStatic;
                if (aps.GuokrAccountLoginStatus)
                    await VerifyAccountV2(aps.GuokrAccountProfile.username, aps.GuokrAccountProfile.password);
                else
                    throw new GuokrException() { errnum = GuokrErrorCode.LoginRequired };
            }
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
                                p.title = title.InnerText;
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
            req.Resource = p.path;
            req.Method = Method.GET;

            var resp = await RestSharpAsync.RestSharpExecuteAsyncTask(WwwClient, req);
            ProcessError(resp);

            var doc = new HtmlDocument();
            doc.LoadHtml(resp.Content);
            var n = doc.DocumentNode.SelectSingleNode(@"//div[@class=""post""]");
            if (p.path.Contains("post"))
            {
                n.SelectSingleNode(@"//div[@id=""share""]").Remove();
                var m = n.SelectSingleNode(@"//div[@class=""post-pic""]");
                string s = m.InnerHtml;
                n.SelectSingleNode(@"//div[@class=""post-info""]").PrependChild(HtmlNode.CreateNode(@"<p class=""fl"">" + s + @"</p>"));
                m.Remove();
            }

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
                req.Resource = "group/post_reply.json";
                req.Parameters.Add(new Parameter() { Name = "post_id", Value = obj.id, Type = ParameterType.GetOrPost });
            }
            else
            {
                throw new GuokrException() { errnum = GuokrErrorCode.InternalError, errmsg = "object_name = " + obj.object_name + " is not supported" };
            }

            req.Parameters.Add(new Parameter() { Name = "limit", Value = limit, Type = ParameterType.GetOrPost });
            req.Parameters.Add(new Parameter() { Name = "offset", Value = offset, Type = ParameterType.GetOrPost });
            req.Parameters.Add(new Parameter() { Name = "_", Value = DateTime.Now.Second % 7, Type = ParameterType.GetOrPost });

            var resp = await RestSharpAsync.RestSharpExecuteAsyncTask<GetArticleCommentsResponse>(ApiClient, req);
            ProcessError(resp);
            if (resp.Data != null)
                return resp.Data.ToCommentList(offset);
            else
                throw new GuokrException() { errnum = GuokrErrorCode.CallFailure, errmsg = resp.Content };
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
    }
}
