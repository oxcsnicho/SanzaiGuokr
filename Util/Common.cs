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

namespace SanzaiGuokr.Util
{
    public class Common
    {
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
        public static void RestSharpLoadDataFromUri(Uri SourceUri, Action<RestResponse> callback)
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
