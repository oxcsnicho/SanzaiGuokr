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
using RestSharp;
using HtmlAgilityPack;
using CodeKicker.BBCode;
using SanzaiGuokr.Model;
using MC.Phone.Analytics;

namespace SanzaiGuokr.Util
{
    public class Common
    {
        static string lastname;
        static DateTime lasttime = DateTime.Now;
        public static void ReportUsage(string name = "")
        {
            if (string.IsNullOrEmpty(name))
                name = lastname;
            AnalyticsTracker tracker = new AnalyticsTracker();

	    var diff = DateTime.Now - lasttime;
#if !DEBUG
	    if(diff > TimeSpan.FromSeconds(3))
#endif
            tracker.Track("PivotUsage", name, diff.TotalSeconds.ToString());
#if DEBUG
	    DebugLogging.Append("Usage", name, diff.TotalSeconds.ToString());
#endif
            lastname = name;
            lasttime = DateTime.Now;
        }
        internal static void StopUsage()
        {
            ReportUsage();
        }
        internal static void ResumeUsage()
        {
            lasttime = DateTime.Now;
        }

        public static string DeviceName()
        {
            var r = Microsoft.Phone.Info.DeviceStatus.DeviceName;
            if (r.IndexOf("nokia", StringComparison.OrdinalIgnoreCase) >= 0
            || r.IndexOf("samsung", StringComparison.OrdinalIgnoreCase) >= 0
            || r.IndexOf("htc", StringComparison.OrdinalIgnoreCase) >= 0
            || r.IndexOf("galaxy", StringComparison.OrdinalIgnoreCase) >= 0
            || r.IndexOf("lumia", StringComparison.OrdinalIgnoreCase) >= 0
            || r.IndexOf("omnia", StringComparison.OrdinalIgnoreCase) >= 0)
                return r;
            else
                return "山寨果壳.wp";
        }
        private static BBCodeParser _bbp;
        public static BBCodeParser BBParser
        {
            get
            {
                if (_bbp == default(BBCodeParser))
                    _bbp = new BBCodeParser(new[]
                {
                    new BBTag("bold", "<b>", "</b>"), 
                    //new BBTag("italic", "<span style=\"font-style:italic;\">", "</span>"), 
                    new BBTag("italic", "<i>", "</i>"), 
                    //new BBTag("u", "<span style=\"text-decoration:underline;\">", "</span>"), 
                    //new BBTag("code", "<pre class=\"prettyprint\">", "</pre>"), 
                    new BBTag("image", "<img src=\"${content}\" />", "", false, true), 
                    new BBTag("blockquote", "<blockquote>", "</blockquote>"), 
                    new BBTag("color", "", ""), 
                    //new BBTag("ul", "<ul>", "</ul>"), // problem: nothing maps to <li>
                    //new BBTag("ol", "<ol>", "</ol>"), 
                    //new BBTag("*", "<li>", "</li>", true, false), 
                    new BBTag("url", "<a href=\"${href}\">", "</a>", new BBAttribute("href", ""), new BBAttribute("href", "href")), 
                });
                return _bbp;
            }
        }
        public static string TransformBBCode(string code)
        {
            var s = BBParser.ToHtml(code);
            return s;
        }
        public static string HumanReadableTime(DateTime dt_created_at)
        {
            if (dt_created_at == default(DateTime))
                return "???";
            var timediff = DateTime.Now - dt_created_at + GuokrApi.ServerTimeDiff;
            string res;

            if (timediff < TimeSpan.FromSeconds(60))
                res = ((int)timediff.TotalSeconds).ToString() + "秒前";
            else if (timediff < TimeSpan.FromMinutes(60))
                res = ((int)timediff.TotalMinutes).ToString() + "分钟前";
#if false
            else if (timediff < TimeSpan.FromHours(24))
                res = ((int)timediff.TotalHours).ToString() + "小时前" + dt_created_at.ToLocalTime().ToString("mm");
#endif
            else if (dt_created_at.Date == DateTime.Now.Date)
                res = dt_created_at.ToLocalTime().ToString("HH:mm");
            else if (timediff < TimeSpan.FromDays(180))
                res = dt_created_at.ToLocalTime().ToString("MM/dd HH:mm");
            else
                res = dt_created_at.ToLocalTime().ToString("yyyy/MM/dd HH:mm");

            return res;

        }
        public static string FlattenHtmlConentToText(string HtmlContent)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(HtmlContent);
            return FlattenHtmlToText(doc.DocumentNode);
        }
        public static string FlattenHtmlToText(HtmlNode node)
        {
            if (node == null)
                return "";

            if (node.NodeType == HtmlNodeType.Text)
                return node.InnerText;
            if (node.NodeType == HtmlNodeType.Element)
            {
                if (node.Name == "h1")
                    return "\n" + node.InnerText + "\n";
                if (node.Name == "p" || node.Name == "h2")
                    return node.InnerText + "\n";
                else if (node.Name == "a")
                    return node.InnerText + " (" + node.Attributes["href"].Value + ") ";
                else if (node.Name == "span" || node.Name == "time")
                    return node.InnerText + " ";
                else if (node.Name == "strong")
                    return "*" + node.InnerText + "*";
                else if (node.Name == "img")
                    return "[img: " + node.Attributes["src"].Value + "]\n";
                //else if (node.Name == "h1")
                //    return string.Format("-----\n{0}\n-----", node.InnerText);
            }
            if (node.NodeType == HtmlNodeType.Document
            || node.NodeType == HtmlNodeType.Element &&
            (node.Name == "html" || node.Name == "body" || node.Name == "article" || node.Name == "div" || node.Name == "p"
            || node.Name == "tr" || node.Name == "td" || node.Name == "table"))
            {
                string res = "";
                foreach (var item in node.ChildNodes)
                {
                    res += FlattenHtmlToText(item);
                }
                if (node.Name == "p" || node.Name == "tr" || node.Name == "table")
                    res += "\n";
                else if (node.Name == "td")
                    res += "\t";
                return res;
            }
            return "";
        }
        public static void RestSharpLoadDataFromUri(Uri SourceUri, Action<IRestResponse> callback)
        {
            var value = SourceUri;
            var c = new RestClient(value.Scheme + "://" + value.Host);
            var r = new RestRequest();
            r.Resource = value.AbsolutePath + value.Query;

            c.ExecuteAsync(r, callback);
        }
        public static string DefaultHtml()
        {
            var bgc = (Color)Application.Current.Resources["DefaultBackgroundColor"];

            return @"
<!DOCTYPE HTML>
<html lang=""en"">
<head>
    <meta charset=""gb2312"">
    <title>http://www.guokr.com/?reply_count=34</title>
    <meta name=""viewport"" content = ""width = device-width, initial-scale = 1, minimum-scale = 1, maximum-scale = 1"" />
<style type=""""text/css"""">
<!--
body {
	font-family: tahoma,arial,sans-serif,sans,STHeiti; font-size: 13px; word-wrap: break-word; background-color: "
                + "#" + bgc.ToString().Substring(3) +
            @"; }
.article > article {
	width: 320px; color: rgb(85, 85, 85); overflow: hidden; padding-bottom: 20px;
}
* {
	margin: 0px; padding: 0px;
}
-->
</style>
</head>
<body class=""article"">
    <article>
<p>   </p>
</article>
</body>
</html>
";
        }
        public static string ErrorHtml = @"
<!DOCTYPE HTML>
<html lang=""en"">
<head>
    <meta charset=""gb2312"">
    <title>http://www.guokr.com/?reply_count=34</title>
    <meta name=""viewport"" content = ""width = device-width, initial-scale = 1, minimum-scale = 1, maximum-scale = 1"" />
<style type=""""text/css"""">
<!--
body {
	font-family: tahoma,arial,sans-serif,sans,STHeiti; font-size: 13px; word-wrap: break-word; background-color: rgb(238, 238, 238);
}
.article > article {
	width: 320px; color: rgb(85, 85, 85); overflow: hidden; padding-bottom: 20px;
}
* {
	margin: 0px; padding: 0px;
}
-->
</style>
</head>
<body class=""article"">
    <article>
<p>Unable to connect; please retry later</p>
</article>
</body>
</html>
";

    }
}
