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
using Microsoft.Phone.Info;
using System.Text.RegularExpressions;
using FlurryWP7SDK;
using System.Collections.Generic;
using SanzaiGuokr.ViewModel;
using Microsoft.Phone.Net.NetworkInformation;
using System.Threading.Tasks;

namespace SanzaiGuokr.Util
{
    public class Common
    {
        public static void CheckNetworkStatus()
        {
            TaskEx.Run(() =>
            {
                DeviceNetworkInformation.ResolveHostNameAsync(
                        new DnsEndPoint("guokr.com", 80),
                        new NameResolutionCallback(nrr =>
                        {
                            if (nrr == null)
                                return;
                            var info = nrr.NetworkInterface;
                            if (info == null)
                                return;
                            var type = info.InterfaceType;

                            if (type == NetworkInterfaceType.Wireless80211
                                || type == NetworkInterfaceType.Ethernet)
                                Deployment.Current.Dispatcher.BeginInvoke(() =>
                                    ViewModelLocator.ApplicationSettingsStatic.NetworkStatus = ApplicationSettingsViewModel.NetworkType.WIFI);
                        }), null);
            });
        }
        public static void InitializeFlurry()
        {
            var ass = ViewModelLocator.ApplicationSettingsStatic;
            FlurryWP7SDK.Api.StartSession("6676FNCYNHJ2Z8CK6VZG");
            FlurryWP7SDK.Api.SetUserId(ViewModelLocator.ApplicationSettingsStatic.AnonymousUserId);
            FlurryWP7SDK.Api.SetSessionContinueSeconds(10);
            FlurryWP7SDK.Api.LogEvent("ApplicationSettings", new List<FlurryWP7SDK.Models.Parameter> {
                new FlurryWP7SDK.Models.Parameter("FontSizeSettingEnum", ass.FontSizeSettingEnum.ToString()),
                new FlurryWP7SDK.Models.Parameter("AlwaysEnableDarkTheme", ass.AlwaysEnableDarkTheme.ToString()),
                new FlurryWP7SDK.Models.Parameter("IsGroupEnabledSettingBool", ass.IsGroupEnabledSettingBool.ToString())
            });
        }
        static string lastname;
        static DateTime lasttime = DateTime.Now;
        public static void ReportUsage(string name = "")
        {
            if (string.IsNullOrEmpty(name))
                name = lastname;

            var diff = DateTime.Now - lasttime;
#if !DEBUG
            if (diff > TimeSpan.FromSeconds(3))
#endif
#if FALSE
            AnalyticsTracker tracker = new AnalyticsTracker();
                tracker.Track("PivotSwitch", name, "AT*" + diff.TotalSeconds.ToString());
#endif
                Api.LogEvent("PivotSwitch", new List<FlurryWP7SDK.Models.Parameter> {
                new FlurryWP7SDK.Models.Parameter("AwaitTime", diff.TotalSeconds.ToString())
            });
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
            string model = "";
            try
            {
                string manufacturer = DeviceStatus.DeviceManufacturer;
                if (manufacturer.Equals("NOKIA"))
                {
                    model = manufacturer + " ";
                    string name = DeviceStatus.DeviceName;
                    if (name.Contains("_"))
                        name = name.Substring(0, name.IndexOf('_'));
                    switch (name)
                    {
                        case "RM-846":
                            model += "Lumia 620";
                            break;
                        case "RM-878":
                            model += "Lumia 810";
                            break;
                        case "RM-824":
                        case "RM-825":
                        case "RM-826":
                            model += "Lumia 820";
                            break;
                        case "RM-845":
                            model += "Lumia 822";
                            break;
                        case "RM-820":
                        case "RM-821":
                        case "RM-822":
                            model += "Lumia 920";
                            break;
                        case "RM-867":
                            model += "Lumia 920T";
                            break;
                        default:
                            if (name.Contains("Nokia"))
                            {
                                Regex.Replace(name, "Nokia", "Lumia");
                                model += name;
                            }
                            else
                                throw new Exception();
                            break;
                    }
                }
                else if (manufacturer.Equals("HTC"))
                {
                    model = manufacturer + " ";
                    string name = DeviceStatus.DeviceName;
                    switch (name)
                    {
                        case "C620e":
                        case "C620d":
                        case "C620t":
                            model += "8X";
                            break;
                        case "A620d":
                        case "A620e":
                            model += "8S";
                            break;
                        /*
                        case "X310e":
                            model += "Titan";
                            break;
                        case "mwp6985":
                            model += "Trophy";
                            break;
                        case "T8788":
                            model += "Surround";
                            break;
                        */
                        default:
                            if (name.Contains("Mozart"))
                                model += "Mozart";
                            else if (name.Contains("8X"))
                                model += "8X";
                            /*
                            else if (name.Contains("HD7"))
                                model += "HD7";
                            else if (name.Contains("Trophy"))
                                model += "Trophy";
                            else if (name.IndexOf("Radar", StringComparison.OrdinalIgnoreCase) != -1)
                                model += "Radar";
                            else if (name.Contains("X310e"))
                                model += "Titan";
                            */
                            else
                                throw new Exception();
                            break;
                    }
                    return model;
                }
                else if (manufacturer.Equals("Samsung"))
                {
                    model = DeviceStatus.DeviceManufacturer + " ";
                    string name = DeviceStatus.DeviceName;
                    switch (name)
                    {
                        case "SGH-i917":
                        case "SGH-i917.":
                        case "SGH-I917":
                        case "I917":
                        case "SGH-i677":
                        case "SGH-I677":
                        case "SGH-i917R":
                        case "SGH-I937":
                        case "Focus i917":
                            model += "Focus";
                            break;
                        /*
                        case "GT-I8350":
                            model += "Omnia W";
                            break;
                        case "GT-S7530E":
                            model += "Omnia M";
                            break;
                        */
                        case "OMNIA7":
                        case "Omina 7":
                        case "Omnia 7":
                        case "GT-i8700":
                            model += "Omnia 7";
                            break;
                        default:
                            throw new Exception();
                            break;
                    }
                    return model;
                }
                else
                {
                    throw new Exception();
                }
            }
            catch
            {
                model = "山寨果壳.wp";
            }
            return model;
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
            try
            {
                var s = BBParser.ToHtml(code);
                return s;
            }
            catch
            {
                return "";
            }
        }
        public static string HumanReadableTime(DateTime dt_created_at)
        {
            if (dt_created_at == default(DateTime))
                return "???";
            var timediff = GuokrApi.ServerNow.ToUniversalTime() - dt_created_at.ToUniversalTime();
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
