﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using RestSharp;
using System.Text.RegularExpressions;
using System.Windows.Navigation;
using SanzaiGuokr.ViewModel;
using System.ComponentModel;
using SanzaiGuokr.Util;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SanzaiGuokr.Messages;
using System.Text;
using System.Windows.Threading;
using SanzaiGuokrCore.Util;
using System.Threading.Tasks;

namespace webbrowsertest
{
    public partial class MyWebBrowser : UserControl
    {
        public MyWebBrowser()
        {
            InitializeComponent();
            InternalWB.LoadCompleted += new System.Windows.Navigation.LoadCompletedEventHandler((ss, ee) =>
            {
                if (LoadCompleted != null)
                    LoadCompleted(ss, ee);

                Task.Run(async () =>
                    {
                        await Task.Delay(300);
                        Deployment.Current.Dispatcher.BeginInvoke(() => Opacity = 1);
                    });
            });
            InternalWB.NavigationFailed += new System.Windows.Navigation.NavigationFailedEventHandler((ss, ee) =>
            {
                if (NavigationFailed != null)
                    NavigationFailed(ss, ee);
            });
            InternalWB.Navigated += new EventHandler<NavigationEventArgs>((ss, ee) =>
            {
                if (Navigated != null)
                    Navigated(ss, ee);

                Task.Run(async () =>
                    {
                        await Task.Delay(5000);
                        Deployment.Current.Dispatcher.BeginInvoke(() => Opacity = 1);
                    });
            });
            InternalWB.ScriptNotify += new EventHandler<NotifyEventArgs>((ss, ee) =>
            {
                if (ScriptNotify != null)
                    ScriptNotify(ss, ee);
            });
        }

        public void ClearContent()
        {
            InternalWB.NavigateToString("");
        }
        #region HtmlMode
        public enum HtmlModeType
        {
            FullHtml,
            HtmlFragment,
            JsonHtmlFragment,
            Div
        };
        public static readonly DependencyProperty HtmlModeProperty =
            DependencyProperty.Register(
            "SourceHtml", typeof(HtmlModeType), typeof(MyWebBrowser),
            new PropertyMetadata(default(HtmlModeType), new PropertyChangedCallback(HtmlModeChanged))
            );
        public HtmlModeType HtmlMode
        {
            get
            {
                return (HtmlModeType)GetValue(HtmlModeProperty);
            }
            set
            {
                SetValue(HtmlModeProperty, value);
            }

        }
        static void HtmlModeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
        }
        #endregion

        #region SourceHtml
        public static readonly DependencyProperty SourceHtmlProperty =
            DependencyProperty.Register(
            "SourceHtml", typeof(string), typeof(MyWebBrowser),
            new PropertyMetadata(default(string), new PropertyChangedCallback(SourceHtmlChanged))
            );
        public string SourceHtml
        {
            get
            {
                return (string)GetValue(SourceHtmlProperty);
            }
            set
            {
                SetValue(SourceHtmlProperty, value);
            }
        }
        static void SourceHtmlChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var current = sender as MyWebBrowser;
            var value = e.NewValue as string;
            if (current == null || value == null)
                return;

            if (current.StartNavigating != null)
                current.StartNavigating(current, new NavigatingEventArgs());

            // async await
#if DEBUG
            Messenger.Default.Send<MyWebBrowserStatusChanged>(new MyWebBrowserStatusChanged() { NewStatus = "Preparing" });
#endif
            current.PrepareControl(value.Length);
            current.MassageAndShowHTML(value);
        }
        #endregion
#if false
        #region SourceUri
        public static readonly DependencyProperty SourceUriProperty = 
            DependencyProperty.Register(
            "SourceUri", typeof(Uri), typeof(MyWebBrowser), 
            new PropertyMetadata(default(Uri),new PropertyChangedCallback(SourceUriChanged))
            );

        public Uri SourceUri
        {
            get
            {
                return (Uri)GetValue(SourceUriProperty);
            }
            set
            {
                SetValue(SourceUriProperty, value);
            }
        }

        static void SourceUriChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var current = sender as MyWebBrowser;
            var value = e.NewValue as Uri;
            if (current == null || value == null)
                return;

            current.StartNavigate();
        }

                private void PrepareControl()
        {
            // fix bug #1 in a brutal way
            var d = DataContext as ReadArticleViewModel;
            if (d != null)
            {
                d.LoadingIndicator = true;
            }

            if (StartNavigating != null)
            {
                StartNavigating(this, new NavigatingEventArgs());
            }
            Opacity = 0;
            LoadCompleted += new LoadCompletedEventHandler((ss, ee) =>
                {
                    Opacity = 1;
                });
        }

        private void StartNavigate()
        {
            PrepareControl();
            LoadDataAndShowHTML();
        }

        private void LoadDataAndShowHTML()
        {
            Common.RestSharpLoadDataFromUri(SourceUri, (response) =>
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    if (response.ResponseStatus != ResponseStatus.Completed
                        || response.Content.Length == 0)
                    {
                        if (NavigationFailed != null)
                            NavigationFailed(this, null);

                        Messenger.Default.Send<MyWebBrowserStatusChanged>(new MyWebBrowserStatusChanged() { NewStatus = "Rendering" });
                        InternalWB.NavigateToString(Common.ErrorHtml);
                    }
                    else
                        Deployment.Current.Dispatcher.BeginInvoke(() => MassageAndShowHTML(WebForegroundColor,
                            WebBackgroundColor, FontSize, response.Content));
                });
            });
        }

        public void MassageAndShowHTML(string html_doc)
        {
            MassageAndShowHTML(WebForegroundColor, WebBackgroundColor, WebFontSize, html_doc);
        }
        public void MassageAndShowHTML(Color WebForegroundColor, Color WebBackgroundColor, double WebFontSize, string html_doc) // can be changed to async method
        {
            var bw = new BackgroundWorker();
            var mode = HtmlMode; // must needed because going down we are not in the UI thread anymore
            bw.DoWork += new DoWorkEventHandler((ss, ee) =>
            {
                string foreground = WebForegroundColor.ToString().Substring(3).ToLowerInvariant();
                string base_url = "http://www.guokr.com";
                string stylesheet = string.Format("<style type=\"text/css\"> "
                       + "body {{ background-color: #{0};font-size: {2}px }}" //body styles
                       + "p.document-figcaption{{ font-size: {3}px;font-style:italic;text-align:center}}" // img caption styles
                        + ".article>article,.article > article h1, .article > article h2, .article > article h3 {{color:#{4} }}" //foreground color 1
                        + "a, .fake_a {{color:#{5}}}"//foreground color 2
                        + ".article > article > .title {{padding-top:0px}}" //title gap
                       + "</style>",
                    WebBackgroundColor.ToString().Substring(3), foreground, FontSizeTweak(WebFontSize).ToString(), //body style parameters
                    (FontSizeTweak(WebFontSize) - 1).ToString(), //img caption style parameters
                    foreground, foreground // foreground color
                    );

                switch (mode)
                {
                    case HtmlModeType.HtmlFragment:
                        html_doc = @"<!DOCTYPE HTML>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <title>http://www.guokr.com/?reply_count=55</title>
    <link rel=""stylesheet"" href=""http://www.guokr.com/skin/mobile_app.css?gt"">
    <meta name=""viewport"" content = ""width = device-width, initial-scale = 1, minimum-scale = 1, maximum-scale = 1"" />
" + stylesheet + @"<body><div class=""cmts"" id=""comments"">" + html_doc + "</div></body></html>";
                        break;

                    case HtmlModeType.FullHtml:
                        var index_of_stylesheet = html_doc.IndexOf("/skin/mobile_app.css", StringComparison.InvariantCultureIgnoreCase);
                        var index_of_head_ending = html_doc.IndexOf("</head>", StringComparison.InvariantCultureIgnoreCase);

                        html_doc = html_doc.Substring(0, index_of_stylesheet)
                            + base_url
                            + html_doc.Substring(index_of_stylesheet, index_of_head_ending - index_of_stylesheet)
                            + stylesheet
                            + html_doc.Substring(index_of_head_ending, html_doc.Length - index_of_head_ending);
                        break;
                }

                html_doc = ConvertExtendedASCII(html_doc);

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        try
                        {
                            Messenger.Default.Send<MyWebBrowserStatusChanged>(new MyWebBrowserStatusChanged() { NewStatus = "Rendering" });
                            InternalWB.NavigateToString(html_doc);
                        }
                        catch
                        {
                        }
                    });

            });
            bw.RunWorkerAsync();
        }
        public static double FontSizeTweak(double a)
        {
            return Math.Ceiling((a + 1.328) / 1.333) - 3;
        }
        public static string ConvertExtendedASCII(string HTML)
        {
            string retVal = "";
            char[] s = HTML.ToCharArray();

            foreach (char c in s)
            {
                if (Convert.ToInt32(c) > 127)
                    retVal += "&#" + Convert.ToInt32(c) + ";";
                else
                    retVal += c;
            }

            return retVal;
        }   
        private void StartNavigate()
        {
            PrepareControl();
            LoadDataAndShowHTML();
        }

        #endregion
#endif

        #region LoadHTML

        private void PrepareControl(int length)
        {
            // fix bug #1 in a brutal way
            var d = DataContext as ReadArticleViewModel;
            if (d != null)
            {
                d.LoadingIndicator = true;
            }
            Opacity = 0;

            if (StartNavigating != null)
            {
                StartNavigating(this, new NavigatingEventArgs());
            }
#if false
            var dt = new DispatcherTimer();
            //MessageBox.Show("length=" + length);
            dt.Interval = TimeSpan.FromSeconds(1 + (length < 8000 ? length / 8000 : 1));
            dt.Tick += (ss, ee) =>
                {
                    Opacity = 1;
                    dt.Stop();
                };
            dt.Start();
#endif
        }

        public void MassageAndShowHTML(string html_doc)
        {

            MassageAndShowHTML(WebForegroundColor, WebBackgroundColor, WebFontSize, html_doc);
        }
        public void MassageAndShowHTML(Color WebForegroundColor, Color WebBackgroundColor, double WebFontSize, string html_doc) // can be changed to async method
        {
            var mode = HtmlMode; // must needed because going down we are not in the UI thread anymore
#if false
            var bw = new BackgroundWorker();
            bw.DoWork += new DoWorkEventHandler((ss, ee) =>
            {
#endif
            string foreground = WebForegroundColor.ToString().Substring(3).ToLowerInvariant();
            string base_url = "http://www.guokr.com";
            string stylesheet = string.Format("<style type=\"text/css\"> \n"
                   + "body, .post-detail {{ background-color: #{0};font-size: {2}px !important; margin-top:0px; word-wrap: break-word; }}\n" //body styles
                   + "p.document-figcaption{{ font-size: {3}px;font-style:italic;text-align:center;}}\n" // img caption styles
                    + ".ui-content, .article>article,.article > article h1, .article > article h2, .article > article h3, .post, #articleTitle, td {{color:#{4}; }}\n" //foreground color 1
                    + "a, .fake_a {{color:#{5};}}\n"//foreground color 2
                    + "div[style] {{background-color: #{6} !important;}}\n" // div background for later
                    + ".article > article > .title, .article-head {{padding-top:0px;}}\n" //title gap
                    + " .post-detail {{ font-size: 116%; }}\n" // post detail
                    + " .post {{ margin-top: 0; }}\n" // post top margin
                    + ".article-head > h3 {{font-size: 150%; margin-top:2px;}} h1 {{font-size: 125%;}}\n" // title size
                    + "img[style] {{width: 200px !important; height: auto !important; margin: auto !important; display: block !important; text-align: center !important; }}\n"//img style
                    + "embed {{width: 250px !important; height: 150px !important;}}\n" // embed width
                    + "iframe {{width: 250px !important; height: 150px !important;}}\n" // iframe width
                    + ".post-detail span {{ color: #{7} !important; }}\n" // post detail
                    + "img {{width: 200px !important; height: auto !important; display: block !important; margin: auto !important; text-align: center !important; }}\n"//img style
                    + "#articleAuthorImg {{ width: 60 !important; height: 60 !important; margin-left: 0 !important; text-align: left !important; }} \n" // fix for author img
                //+ "ul {{ margin-left: -15px !important; padding-left: -15px !important; }}\n" //li style //does not work
                   + "</style>\n",
                WebBackgroundColor.ToString().Substring(3), foreground, FontSizeTweak(WebFontSize).ToString(), //body style parameters
                (FontSizeTweak(WebFontSize) - 1).ToString(), //img caption style parameters
                foreground, foreground, // foreground color
        WebBackgroundColor.ToString().Substring(3), // background for boxes
    foreground // foreground for .post-detail>span
                );
            string script = @"<script>
var b = document.getElementsByTagName(""img"");
for (i=0;i<b.length;i++)
{
if(b[i].id !=""articleAuthorImg"")
b[i].onclick=function () { window.external.notify(""img|""+this.src); return false; };
}
var e = document.getElementsByTagName(""object"");
for (i=0;i<e.length;i++)
{
var par = e[i].parentElement;
var rep = document.createElement(""p"");
rep.innerHTML = e[i].innerHTML;
par.replaceChild(rep, e[i]);
}
var c = document.getElementsByTagName(""embed"");
for (i=0;i<c.length;i++)
{
var par = c[i].parentElement;
var rep = document.createElement(""a"");
rep.href = c[i].src;
rep.innerText = ""此处有视频，点击播放"";
par.replaceChild(rep, c[i]);
}
var d = document.getElementsByTagName(""iframe"");
for (i=0;i<d.length;i++)
{
var par = d[i].parentElement;
var rep = document.createElement(""a"");
rep.href = d[i].src;
rep.innerText = ""此处有视频，点击播放"";
par.replaceChild(rep, d[i]);
}
var a = document.getElementsByTagName(""a"");
for (i=0;i<a.length;i++)
{
a[i].onclick=function () { window.external.notify(""a|""+this.href); return false; };
}
                    </script>
";
#if false
b[i].onerror=function () { this.src = thumbnailToImage(this.src);};
function thumbnailToImage(c){
    var a=c;
    -1!=a.indexOf(""/thumbnail/"") && (a = a.replace(""thumbnail"",""image"").replace(/\_[0-9]*x\./,"".""));
    return a;
}
function imageToThumbnail(c){
    var a=c;
    if(-1!==a.indexOf(""/thumbnail/""))
    {
        a=a.replace(/\_[0-9]*x\./, ""_200x."");
    }
    else if(-1!==a.indexOf(""/image/""))
    {
-1!==a.indexOf(""image"") && (a=a.replace(""image"", ""thumbnail""));
-1!==a.indexOf("".jpg"") && (a=a.replace("".jpg"", ""_200x.jpg""));
-1!==a.indexOf("".png"") && (a=a.replace("".png"", ""_200x.png""));
-1!==a.indexOf("".gif"") && (a=a.replace("".gif"", ""_200x.gif""));
}
    return a;
}
#endif
            string copyright = @"
<p class=""copyright"">
                
                本文版权属于果壳网（<a title=""果壳网"" href=""http://www.guokr.com/"">guokr.com</a>），转载请注明出处。商业使用请<a title=""联系果壳"" target=""_blank"" href=""/contact/"">联系果壳</a>
                
                </p>";

            if (string.IsNullOrWhiteSpace(html_doc))
                html_doc =
                    @"<html lang=""zh-CN"">
                                        <head>
                                            <meta charset=""UTF-8"" />
                                        <meta name=""viewport"" content = ""width = device-width, user-scale=no, initial-scale = 1, minimum-scale = 1, maximum-scale = 1"" /> "
                    + stylesheet + @"
                                        </head>
                                        <body>
                                        <div data-role=""content"" id=""articleContent"" class=""ui-content"" role=""main"">"
                    + @"刷新失败..."
                    + "</div>" + script + "</body></html>";
            else
                switch (mode)
                {
                    case HtmlModeType.JsonHtmlFragment:
                        html_doc =
                            @"<html lang=""zh-CN"">
                                            <head>
                                                <meta charset=""UTF-8"" />
                                            <meta name=""viewport"" content = ""width = device-width, user-scale=no, initial-scale = 1, minimum-scale = 1, maximum-scale = 1"" /> "
                            + stylesheet + @"
                                            </head>
                                            <body>
                                            <div data-role=""content"" id=""articleContent"" class=""ui-content"" role=""main"">"
                            + html_doc + copyright + "</div>" + script + "</body></html>";
                        break;
                    case HtmlModeType.HtmlFragment:
                        html_doc = @"<!DOCTYPE HTML>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <title>http://www.guokr.com/?reply_count=55</title>
    <link rel=""stylesheet"" href=""http://www.guokr.com/skin/mobile_app.css?gt"">
    <meta name=""viewport"" content = ""width = device-width, initial-scale = 1, minimum-scale = 1, maximum-scale = 1"" />
" + stylesheet + @"<body><div class=""cmts"" id=""comments"">" + html_doc + "</div></body></html>";
                        break;

                    case HtmlModeType.FullHtml:
                        var index_of_stylesheet = html_doc.IndexOf("/skin/mobile_app.css", StringComparison.InvariantCultureIgnoreCase);
                        var index_of_head_ending = html_doc.IndexOf("</head>", StringComparison.InvariantCultureIgnoreCase);

                        html_doc = html_doc.Substring(0, index_of_stylesheet)
                            + base_url
                            + html_doc.Substring(index_of_stylesheet, index_of_head_ending - index_of_stylesheet)
                            + stylesheet
                            + html_doc.Substring(index_of_head_ending, html_doc.Length - index_of_head_ending);
                        break;
                    case HtmlModeType.Div:
                        html_doc =
                            @"
<html>
    <head>
        <meta charset=""UTF-8"">
        <meta name=""viewport"" content = ""width = device-width, user-scale=no, initial-scale = 1, minimum-scale = 1, maximum-scale = 1"" />
" + stylesheet + @"</head><body>" + html_doc + script + "</body></html>";
                        break;
                }

#if DEBUG
                Messenger.Default.Send<MyWebBrowserStatusChanged>(new MyWebBrowserStatusChanged() { NewStatus = "Converting" });
#endif
            // html_doc = (html_doc.Substring(html_doc.IndexOf("<html", StringComparison.InvariantCultureIgnoreCase)));

#if false
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
#endif
            var regex = @"img1.guokr.com/(thumbnail/[\w-_]*_\d{0,3}x\d{0,3}|image/[\w-_]*).(jpg|png|gif)";
            html_doc = Regex.Replace(html_doc, regex,
                (m) =>
                {
                    var gi = new GuokrImageInfo(m.ToString());
                    return gi.ToThumbnail(200);
                });
            try
            {
#if DEBUG
                        Messenger.Default.Send<MyWebBrowserStatusChanged>(new MyWebBrowserStatusChanged() { NewStatus = "Rendering" });
#endif
                InternalWB.NavigateToString(html_doc);
            }
            catch
            {
            }
#if false
                });

            });
            bw.RunWorkerAsync();
#endif
        }
        public static double FontSizeTweak(double a)
        {
            return Math.Ceiling((a + 1.328) / 1.333) - 3;
        }
        public static string ConvertExtendedASCII(string text)
        {
            var answer = new StringBuilder();
            char[] s = text.ToCharArray();

            foreach (char c in s)
            {
                if (Convert.ToInt32(c) > 127)
                    answer.Append("&#" + Convert.ToInt32(c) + ";");
                else
                    answer.Append(c);
            }

            return answer.ToString();
        }
        #endregion
        #region StartingNavigating, LoadComplete, Failed
        public event EventHandler<NavigatingEventArgs> StartNavigating;
        public event LoadCompletedEventHandler LoadCompleted;
        public event NavigationFailedEventHandler NavigationFailed;
        public event EventHandler<NavigationEventArgs> Navigated;
        public event EventHandler<NotifyEventArgs> ScriptNotify;
        #endregion

        #region WebBackgroundColor, WebForegroundColor, WebFontSize
        const string WebBackgroundColorPropertyName = "WebBackgroundColor";
        public static readonly DependencyProperty WebBackgroundColorProperty =
            DependencyProperty.Register(
            WebBackgroundColorPropertyName, typeof(Color), typeof(MyWebBrowser), null
            );
        public Color WebBackgroundColor
        {
            get
            {
                return (Color)GetValue(WebBackgroundColorProperty);
            }
            set
            {
                SetValue(WebBackgroundColorProperty, value);
            }
        }
        const string WebForegroundColorPropertyName = "WebForegroundColor";
        public static readonly DependencyProperty WebForegroundColorProperty =
            DependencyProperty.Register(
            WebForegroundColorPropertyName, typeof(Color), typeof(MyWebBrowser), null
            );
        public Color WebForegroundColor
        {
            get
            {
                return (Color)GetValue(WebForegroundColorProperty);
            }
            set
            {
                SetValue(WebForegroundColorProperty, value);
            }
        }
        const string WebFontSizePropertyName = "WebFontSize";
        public static readonly DependencyProperty WebFontSizeProperty =
            DependencyProperty.Register(
            WebFontSizePropertyName, typeof(double), typeof(MyWebBrowser), null);

        public double WebFontSize
        {
            get
            {
                return (double)GetValue(WebFontSizeProperty);
            }
            set
            {
                SetValue(WebFontSizeProperty, value);
            }
        }
        #endregion

        #region command (refresh, go back, etc)
        private RelayCommand _refresh = null;
        public RelayCommand Refresh
        {
            get
            {
                if (_refresh == null)
                    _refresh = new RelayCommand(() =>
                    {
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                            {
                                InternalWB.InvokeScript("eval", "history.go(0)");
                            });
                    });
                return _refresh;
            }
        }
        #endregion
    }
}
