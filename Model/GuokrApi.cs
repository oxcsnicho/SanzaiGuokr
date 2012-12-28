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
        OK2,//2
        OK3,//3
        OK4,//4
        OK5,//5
        OK6,//6
        CommentDoesNotExist,//7
        OK8,//8
        OK9,//9
        CommentTooFrequent,//10
        VerificationFailed,
        CallFailure
    }
    public class GuokrApi : ApiClassBase
    {

        public const string GuokrBaseUrl = "http://www.guokr.com";
        private static RestClient _client = new RestClient(GuokrBaseUrl) { CookieContainer = new CookieContainer() };
        public static RestClient Client
        {
            get
            {
                return _client;
            }
        }

        public static bool IsVerified
        {
            get
            {
                return (Client != null && Client.CookieContainer != null && Client.CookieContainer.Count > 0);
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

        public static async Task VerifyAccount(string username, string password)
        {
#if DEBUG
            try
            {
#endif
                string token;
                // get the token
                {
                    var req = NewJsonRequest();
                    req.Resource = "/api/userinfo/get_token/";
                    req.Method = Method.POST;
                    req.Parameters.Add(new Parameter() { Name = "username", Value = username, Type = ParameterType.GetOrPost });
                    var response = await RestSharpAsync.RestSharpExecuteAsyncTask<GuokrUserToken>(Client, req);
                    ProcessError(response);
                    token = response.Data.token;
                }

                // encode password
                string userToken = "";
                string encodedPassword = "";
                GuokrAuth.encodePassword(username, password, token, out encodedPassword, out userToken);

                // login and get cookie
                {
                    var req = NewJsonRequest();
                    req.Resource = "/api/userinfo/login/";
                    req.Method = Method.POST;
                    req.Parameters.Add(new Parameter() { Name = "username", Value = username, Type = ParameterType.GetOrPost });
                    req.Parameters.Add(new Parameter() { Name = "sspassword", Value = encodedPassword, Type = ParameterType.GetOrPost });
                    req.Parameters.Add(new Parameter() { Name = "susertoken", Value = userToken, Type = ParameterType.GetOrPost });
                    req.Parameters.Add(new Parameter() { Name = "remember", Value = true, Type = ParameterType.GetOrPost });

                    var response = await RestSharpAsync.RestSharpExecuteAsyncTask<GuokrUserLogin>(Client, req);
                    ProcessError(response);

                    ViewModelLocator.ApplicationSettingsStatic.GuokrAccountProfile = new GuokrUserLogin()
                    {
                        nickname = response.Data.nickname,
                        ukey = response.Data.ukey,
                        username = username,
                        password = password
                    };
                }
#if DEBUG
            }
            catch (Exception e)
            {
                throw e;
            }
#endif
        }
        public static async Task PostComment(article_base a, string comment)
        {
            if (!IsVerified)
            {
                var aps = ViewModelLocator.ApplicationSettingsStatic;
                if (aps.GuokrAccountLoginStatus)
                    await VerifyAccount(aps.GuokrAccountProfile.username, aps.GuokrAccountProfile.password);
                else
                    throw new GuokrException() { errnum = GuokrErrorCode.LoginRequired };
            }
            var client = Client;
            var req = NewJsonRequest();
            req.Resource = "/api/reply/new/";
            req.Method = Method.POST;

            comment += "\n来自" + @"[url href=http://windowsphone.com/s?appid=bd089a5a-b561-4155-b21b-30b9844e7ee7]山寨果壳.wp[/url]";

            req.AddParameter(new Parameter() { Name = "obj_type", Value = a.object_name, Type = ParameterType.GetOrPost });
            req.AddParameter(new Parameter() { Name = "obj_id", Value = a.id, Type = ParameterType.GetOrPost });
            req.AddParameter(new Parameter() { Name = "content", Value = comment, Type = ParameterType.GetOrPost });

            var response = await RestSharpAsync.RestSharpExecuteAsyncTask<PostReplyResponse>(client, req);
            ProcessError(response);
        }

        public static async Task DeleteComment(comment c)
        {
            if (!IsVerified)
            {
                var aps = ViewModelLocator.ApplicationSettingsStatic;
                if (aps.GuokrAccountLoginStatus)
                    await VerifyAccount(aps.GuokrAccountProfile.username, aps.GuokrAccountProfile.password);
                else
                    throw new GuokrException() { errnum = GuokrErrorCode.LoginRequired };
            }
            var client = Client;
            var req = NewJsonRequest();
            req.Resource = "/api/reply/delete/";
            req.Method = Method.POST;

            req.AddParameter(new Parameter() { Name = "reply_id", Value = c.reply_id, Type = ParameterType.GetOrPost });

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

            Dictionary<string, string> kvp = new Dictionary<string, string>();
            kvp.Add("ul", @"//ul[@class=""titles cb""]");
            kvp.Add("title", @"//h3/a[@href!=""/blog/""]");
            kvp.Add("reply_count", @"//span[@class=""titles-r-grey""]");
            kvp.Add("group", @"//span[@class=""titles-b-l""]/a[1]");
            kvp.Add("posted_by", @"//span[@class=""titles-b-l""]/a[2]");
            kvp.Add("replied_dt", @"//span[@class=""titles-b-r""]");

            return await _getPosts(Client, req, kvp);
        }
        static internal async Task<List<GuokrPost>> _getPosts(RestClient client, RestRequest req,
            Dictionary<string, string> xpath, GuokrGroup group = null)
        {
            if (!IsVerified)
            {
                var aps = ViewModelLocator.ApplicationSettingsStatic;
                if (aps.GuokrAccountLoginStatus)
                    await VerifyAccount(aps.GuokrAccountProfile.username, aps.GuokrAccountProfile.password);
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

                    var titles = ul.SelectNodes(ul.XPath + xpath["title"]);
                    var reply_counts = ul.SelectNodes(ul.XPath + xpath["reply_count"]);
                    var posted_bys = ul.SelectNodes(ul.XPath + xpath["posted_by"]);
                    var replied_dts = ul.SelectNodes(ul.XPath + xpath["replied_dt"]);
                    var groups = ul.SelectNodes(ul.XPath + xpath["group"]);

                    if (titles.Count <= 1
                        || titles.Count != reply_counts.Count
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

            var resp = await RestSharpAsync.RestSharpExecuteAsyncTask(Client, req);
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


        public static async Task<List<article>> GetLatestArticles(int offset = 0)
        {
            var req = NewJsonRequest();
            req.Resource = "api/content/latest_article_list/";
            req.Method = Method.POST;

            req.Parameters.Add(new Parameter() { Name = "count", Value = 3, Type = ParameterType.GetOrPost });
            req.Parameters.Add(new Parameter() { Name = "offset", Value = offset, Type = ParameterType.GetOrPost });

            var resp = await RestSharpAsync.RestSharpExecuteAsyncTask<List<article>>(Client, req);
            ProcessError(resp);
            return resp.Data;
        }
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
        public static async Task<List<comment>> GetComments(GuokrObjectWithId obj, int offset = 0)
        {
            var req = NewJsonRequest();
            req.Resource = "api/reply/list/";
            req.Method = Method.GET;

            req.Parameters.Add(new Parameter() { Name = "obj_id", Value = obj.id, Type = ParameterType.GetOrPost });
            req.Parameters.Add(new Parameter() { Name = "obj_type", Value = obj.object_name, Type = ParameterType.GetOrPost });
            req.Parameters.Add(new Parameter() { Name = "count", Value = 10, Type = ParameterType.GetOrPost });
            req.Parameters.Add(new Parameter() { Name = "offset", Value = offset, Type = ParameterType.GetOrPost });

            var resp = await RestSharpAsync.RestSharpExecuteAsyncTask<List<comment>>(Client, req);
            ProcessError(resp);
            return resp.Data;
        }
    }
}
