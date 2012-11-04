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

namespace SanzaiGuokr.SinaApiV2
{
    public class SinaLogin
    {
        public string access_token { get; set; }
        public long expires_in { get; set; }
        public long uid { get; set; }
        public long request_time_utc { get; set; }

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
        public static string app_key = "1313825017";
        public static string app_secret = "f1966c10f54df2efaff97b04ee82bf1a";
        public static string StanfordLocation = "http://ccrma.stanford.edu/~darkowen/temp/temp";

        SinaLogin _login;
        private SinaApiConfig(SinaLogin l)
        {
            _login = l;
        }

    }


}
