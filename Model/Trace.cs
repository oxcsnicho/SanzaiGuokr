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
using System.Collections.Generic;
using System.Text;
using RestSharp;
using System.Diagnostics;

namespace WeiboApi
{
    public class Trace
    {
        protected static Trace _global_tako;
        public static Trace get_global_trace()
        {
            if(Trace._global_tako == null)
                _global_tako = new Trace();
            return _global_tako;
        }

        public List<string> _api_tako = new List<string>();

        public void Log(SinaApi.ApiBase api, string tag, string p_2)
        {
            Log(api.ToString(), tag, p_2);
        }

        public void Log(object sender, string tag, string p_2)
        {
            string str = (sender.ToString() + "[" + tag + "]: " + p_2);
            _api_tako.Add(str);
            if(str.Length > 128)
                str = str.Substring(0, 128);
            Debug.WriteLine(str);
        }

        public void LogParameters(SinaApi.ApiBase api, string tag, List<Parameter> ps)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var t in ps)
            {
                if (t.Name.Length < 6 ||
                    t.Name.Substring(0, 6) != "oauth_")
                sb.AppendFormat("{0}={1}&", t.Name, t.Value);
            }
            if (sb.Length > 0)
                sb.Remove(sb.Length - 1, 1);
            Log(api.name, tag, sb.ToString());
        }
    }
}
