using System;
using System.Diagnostics;
using System.Net;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using SanzaiGuokr.Model;
using GalaSoft.MvvmLight.Messaging;
using SanzaiGuokr.Messages;
using System.Windows;
using System.Threading.Tasks;

namespace SanzaiGuokr.GuokrObject
{
    public class GuokrCookie
    {
        public CookieContainer CookieContainer
        { get; set; }

        public bool IsValid
        {
            get
            {
                if (CookieContainer == null)
                    return false;
                var cookies = CookieContainer.GetCookies(new Uri("http://m.guokr.com", UriKind.Absolute));
                if (cookies == null || cookies.Count == 0)
                    return false;
                return !cookies["is_logined"].Expired;
            }
        }
#if false
        public static explicit operator GuokrCookie(List<RestResponseCookie> value)
        {
            GuokrCookie g =new GuokrCookie();
            g.Cookies = value;
            return g;
        }
#endif
    }

    public class GuokrUserLogin : GuokrUser
    {
        public string username { get; set; }
        public string password { get; set; }
        public string ukey { get; set; }
    }
    public class GuokrUserHeaderInfo : GuokrUserLogin
    {
        public string home_url { get; set; }
        public int unread_message_count { get; set; }
    }
    public class PostReplyResponse
    {
        public string date_create { get; set; }
        public string home_url { get; set; }
        public string nickname { get; set; }
        public int reply_id { get; set; }
        public string value { get; set; }
    }
    public class GuokrUserToken
    {
        public string token { get; set; }
    }
    public class GuokrAuth
    {
        public static void encodePassword(string username, string password, string token, out string encodedPassword, out string userToken)
        {
            if (username == null)
                username = "";
            if (password == null)
                password = "";

            var g = username;
            var e = password;
            var o = token;
            g = g.ToLower();
            e = k("YiQunSongShu" + e);
            e = k(o + e);
            g = k(o + g);

            encodedPassword = e;
            userToken = g;
        }
        static string k(string s)
        {
            byte[] data = new byte[s.Length];
            for (int i = 0; i < s.Length; i++)
            {
                if ((int)s[i] > 127)
                    throw new NotImplementedException();
                data[i] = (byte)s[i];
            }

            var k = new SHA1Managed();
            byte[] res = k.ComputeHash(data);
            string t = "";
            for (int i = 0; i < res.Length; i++)
                t += ((int)res[i]).ToString("x2");
            return t;
        }

    }
    public class GuokrGroup
    {
        public string name { get; set; }
        public int id { get; set; }
        public string path
        {
            get
            {
                return "/group/posts/" + id.ToString() + "/";
            }
            set
            {
                try
                {
                    var match = Regex.Match(value, @"/group/posts/(\d+)/");
                    if (match.Success && match.Groups.Count > 0)
                    {
                        id = Convert.ToInt32(match.Groups[1].Value);
                    }
                }
                catch
                {
                    Debug.WriteLine("set group path failed. " + value);
                }
            }
        }
    }
    public class GuokrUser
    {
        public int id { get; set; }
        public string uri { get; set; }
        public string nickname { get; set; }
    }
    public class GuokrPost : article_base<GuokrPost>
    {
        private int rpl_cnt;
        public int reply_count
        {
            get
            {
                return rpl_cnt;
            }
            set
            {
                rpl_cnt = value;
                CommentCount = value;
            }
        }
        public GuokrUser posted_by { get; set; }
        public string replied_dt { get; set; }
        private string _p;
        public string path
        {
            get
            {
                return _p;
            }
            set
            {
                _p = value;
                var m = Regex.Match(_p,@"(\d+)");
                if (m.Success == true && m.Groups.Count > 1)
                    id = Convert.ToInt64(m.Groups[1].Value);
            }
        }
        public GuokrGroup group { get; set; }
        public GuokrUser replied_by { get; set; }

        protected override void _readArticle(article_base a)
        {
            if (a.GetType() == typeof(GuokrPost))
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                    Messenger.Default.Send<GoToReadPost>(new GoToReadPost() { article = (GuokrPost)a })
                );
        }
        protected override void PostLoadArticle()
        {
            // todo
        }
        protected override async Task _loadArticle()
        {
           HtmlContent = await GuokrApi.GetPostContentString(this);
        }
    }
    public class GuokrObjectWithId
    {
        public Int64 id { get; set; }
        public string object_name
        {
            get
            {
                if (this.GetType() == typeof(article))
                {
                    return "article";
                }
                else if (this.GetType() == typeof(GuokrPost))
                {
                    return "post";
                }
                else return "unknown";
            }
        }
    }
}
