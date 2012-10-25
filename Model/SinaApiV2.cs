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
        public DateTime RequestDateTime { get; set; }

        public SinaLogin()
        {
            access_token = "";
            RequestDateTime = DateTime.Now;
        }

        public bool IsValid
        {
            get
            {
                return access_token != "" && DateTime.Now <= RequestDateTime.AddSeconds(expires_in);
            }
        }
    }
    public class SinaApiV2
    {
        SinaLogin _login;
        public SinaApiV2(SinaLogin l)
        {
            _login = l;
        }

    }
}
