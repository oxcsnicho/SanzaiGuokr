using System;
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

namespace webbrowsertest
{
    public partial class MyWebBrowser : UserControl
    {
        public MyWebBrowser()
        {
            InitializeComponent();
            InternalWB.LoadCompleted+=new System.Windows.Navigation.LoadCompletedEventHandler((ss,ee)=>
            {
                if (LoadCompleted != null)
                    LoadCompleted(ss, ee);
            });
            InternalWB.NavigationFailed += new System.Windows.Navigation.NavigationFailedEventHandler((ss, ee) =>
            {
                if (NavigationFailed != null)
                    NavigationFailed(ss, ee);
            });
            InternalWB.Navigated+=new EventHandler<NavigationEventArgs>((ss,ee)=>
            {
                if (Navigated!=null)
                    Navigated(ss,ee);
            });
        }
        #region HtmlMode
        public enum HtmlModeType
        {
            FullHtml,
            HtmlFragment,
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
            current.PrepareControl();
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
            var dt = new DispatcherTimer();
            dt.Interval = TimeSpan.FromSeconds(1);
            dt.Tick += (ss, ee) =>
                {
                    Opacity = 1;
                    dt.Stop();
                };
            dt.Start();
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
                    case HtmlModeType.Div:
                        html_doc =
                            @"
<html>
	<head>
		<meta charset=""UTF-8"">
        <meta name=""viewport"" content = ""width = device-width, initial-scale = 0.5, minimum-scale = 0.5, maximum-scale = 1"" />
		<link rel=""stylesheet"" href=""http://www.guokr.com/skin/h.css?ssa"">
		<link rel=""stylesheet"" href=""http://www.guokr.com/skin/group.css?ss9"">
    <link rel=""stylesheet"" href=""http://www.guokr.com/skin/mobile_app.css?gt"">
		<style type=""text/css"">a.bshareDiv,#bsPanel,#bsMorePanel,#bshareF{border:none;background:none;padding:0;margin:0;font:12px Helvetica,Calibri,Tahoma,Arial,宋体,sans-serif;line-height:14px;}#bsPanel div,#bsMorePanel div,#bshareF div{display:block;}.bsRlogo .bsPopupAwd,.bsRlogoSel .bsPopupAwd,.bsLogo .bsPopupAwd,.bsLogoSel .bsPopupAwd{ line-height:16px!important;}a.bshareDiv div,#bsFloatTab div{*display:inline;zoom:1;display:inline-block;}a.bshareDiv img,a.bshareDiv div,a.bshareDiv span,a.bshareDiv a,#bshareF table,#bshareF tr,#bshareF td{text-decoration:none;background:none;margin:0;padding:0;border:none;line-height:1.2}a.bshareDiv span{display:inline;float:none;}div.buzzButton{cursor:pointer;font-weight:bold;}.buzzButton .shareCount a{color:#333}.bsStyle1 .shareCount a{color:#fff}span.bshareText{white-space:nowrap;}span.bshareText:hover{text-decoration:underline;}a.bshareDiv .bsPromo,div.bshare-custom .bsPromo{display:none;position:absolute;z-index:100;}a.bshareDiv .bsPromo.bsPromo1,div.bshare-custom .bsPromo.bsPromo1{width:51px;height:18px;top:-18px;left:0;line-height:16px;font-size:12px !important;font-weight:normal !important;color:#fff;text-align:center;background:url(http://static.bshare.cn/frame/images/bshare_box_sprite2.gif) no-repeat 0 -606px;}div.bshare-custom .bsPromo.bsPromo2{background:url(http://static.bshare.cn/frame/images/bshare_promo_sprite.gif) no-repeat;cursor:pointer;}</style>
" + stylesheet + @"</head><body>" + html_doc + "</body></html>";
                        break;
                }

#if DEBUG
            Messenger.Default.Send<MyWebBrowserStatusChanged>(new MyWebBrowserStatusChanged() { NewStatus = "Converting" });
#endif
                html_doc = ConvertExtendedASCII(html_doc);

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
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
                });

            });
            bw.RunWorkerAsync();
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
