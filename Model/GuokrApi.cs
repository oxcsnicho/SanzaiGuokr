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
using SanzaiGuokr.GuokrObject;
using RestSharp;
using SanzaiGuokr.ViewModel;
using SanzaiGuokr.Util;
using System.Threading.Tasks;
using SanzaiGuokr.GuokrObjects;
using GalaSoft.MvvmLight.Messaging;
using SanzaiGuokr.Messages;
using System.Runtime.Serialization;
using System.Collections.Generic;
using HtmlAgilityPack;
using System.Linq;

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

        public const string GuokrBaseUrl = "http://m.guokr.com";
        private static RestClient _client = new RestClient(GuokrBaseUrl) { CookieContainer = new CookieContainer() };
        public static RestClient Client
        {
            get
            {
                return _client;
            }
        }

        public static async Task VerifyAccount(string username, string password)
        {
            try
            {
                string token;
                {
                    // get the token
                    var req = new RestRequest();
                    req.Resource = "/api/userinfo/get_token/";
                    req.Method = Method.POST;
                    req.RequestFormat = DataFormat.Json;
                    req.OnBeforeDeserialization = resp =>
                    {
                        resp.ContentType = "application/json";
                    };
                    req.Parameters.Add(new Parameter() { Name = "username", Value = username, Type = ParameterType.GetOrPost });
                    var response = await RestSharpAsync.RestSharpExecuteAsyncTask<GuokrUserToken>(Client, req);
                    if (response.Data == null)
                        throw new WebException();
                    token = response.Data.token;
                }

                // encode password
                string userToken = "";
                string encodedPassword = "";
                GuokrAuth.encodePassword(username, password, token, out encodedPassword, out userToken);

                {
                    // login and get cookie
                    var req = new RestRequest();
                    req.Resource = "/api/userinfo/login/";
                    req.Method = Method.POST;
                    req.RequestFormat = DataFormat.Json;
                    req.OnBeforeDeserialization = resp =>
                    {
                        resp.ContentType = "application/json";
                    };
                    req.Parameters.Add(new Parameter() { Name = "username", Value = username, Type = ParameterType.GetOrPost });
                    req.Parameters.Add(new Parameter() { Name = "sspassword", Value = encodedPassword, Type = ParameterType.GetOrPost });
                    req.Parameters.Add(new Parameter() { Name = "susertoken", Value = userToken, Type = ParameterType.GetOrPost });
                    req.Parameters.Add(new Parameter() { Name = "remember", Value = true, Type = ParameterType.GetOrPost });

                    var response = await RestSharpAsync.RestSharpExecuteAsyncTask<GuokrUserInfo>(Client, req);
                    ProcessError<GuokrException>(response);
                    if (response.Data == null)
                        throw new GuokrException() { errnum = GuokrErrorCode.CallFailure, errmsg = response.ErrorMessage };

                    ViewModelLocator.ApplicationSettingsStatic.GuokrAccountProfile = new GuokrUserInfo()
                    {
                        nickname = response.Data.nickname,
                        ukey = response.Data.ukey,
                        username = username,
                        password = password
                    };
                }
            }
            catch (Exception e)
            {
                throw e;
            }

        }

        public static bool IsVerified
        {
            get
            {
                return (Client != null && Client.CookieContainer != null && Client.CookieContainer.Count > 0);
            }
        }
        public static async Task PostComment(article a, string comment)
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
            var req = new RestRequest();
            req.Resource = "/api/reply/new/";
            req.Method = Method.POST;
            req.RequestFormat = DataFormat.Json;
            req.OnBeforeDeserialization = resp =>
            {
                resp.ContentType = "application/json";
            };

            comment += "\n来自" + @"[url href=http://windowsphone.com/s?appid=bd089a5a-b561-4155-b21b-30b9844e7ee7]山寨果壳.wp[/url]";

            req.AddParameter(new Parameter() { Name = "obj_type", Value = "article", Type = ParameterType.GetOrPost });
            req.AddParameter(new Parameter() { Name = "obj_id", Value = a.id, Type = ParameterType.GetOrPost });
            req.AddParameter(new Parameter() { Name = "content", Value = comment, Type = ParameterType.GetOrPost });

            var response = await RestSharpAsync.RestSharpExecuteAsyncTask<PostReplyResponse>(client, req);
            ProcessError<GuokrException>(response);
            if (response.Data == null)
                throw new GuokrException() { errnum = GuokrErrorCode.CallFailure, errmsg = response.ErrorMessage };
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
            var req = new RestRequest();
            req.Resource = "/api/reply/delete/";
            req.Method = Method.POST;
            req.RequestFormat = DataFormat.Json;
            req.OnBeforeDeserialization = resp =>
            {
                resp.ContentType = "application/json";
            };

            req.AddParameter(new Parameter() { Name = "reply_id", Value = c.reply_id, Type = ParameterType.GetOrPost });
            var response = await RestSharpAsync.RestSharpExecuteAsyncTask(client, req);
            try
            {
                ProcessError<GuokrException>(response);
                Messenger.Default.Send<DeleteCommentComplete>(new DeleteCommentComplete() { comment = c });
            }
            catch (GuokrException e)
            {
                Messenger.Default.Send<DeleteCommentComplete>(new DeleteCommentComplete() { comment = c, Exception = e });
            }
        }

        public static async Task<IEnumerable<GuokrPost>> GetPosts(GuokrGroup g, int page = 0)
        {
            var client = new RestClient("http://www.guokr.com");
            var req = new RestRequest();
            req.Method = Method.GET;
            req.Resource = g.path;
            if (page != 0)
                req.Parameters.Add(new Parameter() { Name = "page", Value = page, Type = ParameterType.GetOrPost });
            var resp = await RestSharpAsync.RestSharpExecuteAsyncTask(client, req);
            var html = resp.Content;
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            List<GuokrPost> ress = new List<GuokrPost>();
            try
            {
                var uls = doc.DocumentNode.Descendants("ul");
                var ul = uls.FirstOrDefault(i => i.Attributes["class"] != null && i.Attributes["class"].Value == "titles");
#if false
                res = from li in ul.Elements("li").Where(i => GetClass(i) != "titles-h")
                      select new GuokrPost()
                      {
                          title = li.Element("h2").InnerText,
                          path = li.Element("h2").FirstChild.Attributes["href"].Value,
                          reply_count = Convert.ToInt32(li.Element("span").InnerText),
                          posted_by = li.Descendants("span").Where(i => GetClass(i) == "titles-b-l").First().Element("a").InnerText,
                          replied_by = li.Descendants("span").Where(i => GetClass(i) == "titles-b-r").First().Element("a").InnerText,
                          replied_dt = li.Descendants("span").Where(i => GetClass(i) == "titles-b-r").First().ChildNodes.Last(i => i.NodeType == HtmlNodeType.Text).InnerText
                      };
#endif
                foreach (var li in ul.Elements("li"))
                {
                    if (GetClass(li) != "titles-h")
                        ress.Add(new GuokrPost()
                          {
                              title = li.Element("h2").InnerText,
                              path = li.Element("h2").FirstChild.Attributes["href"].Value,
                              reply_count = Convert.ToInt32(li.Element("span").InnerText),
                              posted_by = li.Descendants("span").Where(i => GetClass(i) == "titles-b-l").First().Element("a").InnerText,
                              replied_by = li.Descendants("span").Where(i => GetClass(i) == "titles-b-r").First().Element("a").InnerText,
                              replied_dt = li.Descendants("span").Where(i => GetClass(i) == "titles-b-r").First().ChildNodes
                                  .Last(i => i.NodeType == HtmlNodeType.Text)
                                  .InnerText.Trim(new char[] { ';', 'n', 'b', 's', 'p', '&' })
                          });
                }
            }
            catch (Exception e)
            {
                DebugLogging.Append("exception", e.Message, "");
            }

            return ress;
        }

        static string GetClass(HtmlNode n)
        {
            if (n.Attributes["class"] != null)
                return n.Attributes["class"].Value;
            else
                return "";
        }

    }
}
