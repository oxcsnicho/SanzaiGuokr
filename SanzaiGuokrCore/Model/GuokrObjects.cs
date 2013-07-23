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
using System.Data.Linq.Mapping;

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
        public string access_token { get; set; }
        public string refresh_token { get; set; }
        public DateTime expire_dt { get; set; }
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
        public static implicit operator GuokrApiV2.Author(GuokrUser a)
        {
            GuokrApiV2.Author b = new GuokrApiV2.Author();
            b.nickname = a.nickname;
            b.url = a.uri;
            return b;
        }
        public static implicit operator GuokrUser(GuokrApiV2.Author b)
        {
            GuokrUser a = new GuokrUser();
            a.nickname = b.nickname;
            a.uri = b.url;
            return a;
        }

    }
    public class GuokrPost : article_base<GuokrPost>
    {
        public int reply_count
        {
            get
            {
                return CommentCount;
            }
            set
            {
                CommentCount = value;
            }
        }
        public GuokrUser posted_by
        {
            get
            {
                return Author;
            }
            set
            {
                Author = value;
            }
        }
        public string replied_dt { get; set; }
        private string _p;
        private GuokrGroup g = null;
        public GuokrGroup group
        {
            get
            {
                return g;
            }
            set
            {
                if (g == value)
                    return;
                g = value;
                RaisePropertyChanged("group");
            }
        }
        public override string parent_name
        {
            get
            {
                if (group != null)
                    return group.name;
                else
                    return "";
            }
        }
        protected override string GetUrlFromId()
        {
            if (id == 0)
                throw new ArgumentOutOfRangeException();
            return string.Format("http://www.guokr.com/post/{0}/", id);
        }
        public GuokrUser replied_by { get; set; }

        protected override void _readArticle(article_base a)
        {
            if (a.GetType() == typeof(GuokrPost))
                Messenger.Default.Send<GoToReadPost>(new GoToReadPost() { article = (GuokrPost)a });
        }
        protected override void PostLoadArticle()
        {
            // todo
        }
        protected override async Task _loadArticle()
        {
            HtmlContent = await GuokrApi.GetPostContentString(this);
        }

        private bool _iu = false;
        public bool IsUpdated
        {
            get
            {
                return _iu;
            }
            set
            {
                if (_iu != value)
                {
                    _iu = value;
                    RaisePropertyChanged("IsUpdated");
                }
            }
        }
    }
    public class GuokrObjectWithId
    {
        public Int64 id { get; set; }

        public string object_name
        {
            get
            {
                if (this.GetType() == typeof(article) || this.GetType() == typeof(recommend_article))
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
    public class GuokrArticleInfo
    {
        public string obj_type { get; set; }
        public int recommend_cout { get; set; }
        public string title { get; set; }
        public int reply_count { get; set; }
        public string ukey_author { get; set; }
    }
}
