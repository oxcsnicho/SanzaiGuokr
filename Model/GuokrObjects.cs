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
using System.Security.Cryptography;
using System.Collections.Generic;
using RestSharp;

namespace SanzaiGuokr.GuokrObject
{
    public class GuokrCookie
    {
        public List<RestResponseCookie> Cookies { get; set; }

        public bool IsValid
        {
            get
            {
                return Cookies == null || Cookies.Count <= 0 ? false : !Cookies[0].Expired;
            }
        }
        public static explicit operator GuokrCookie(List<RestResponseCookie> value)
        {
            GuokrCookie g =new GuokrCookie();
            g.Cookies = value;
            return g;
        }
    }

    public class GuokrUserInfo
    {
        public string nickname { get; set; }
        public string ukey { get; set; }
    }
    public class GuokrUserHeaderInfo : GuokrUserInfo
    {
        public string home_url { get; set; }
        public int unread_message_count { get; set; }
    }
    public class PostReplyResponse
    {
        public string date_created { get; set; }
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
}
