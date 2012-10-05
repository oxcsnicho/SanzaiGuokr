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
            current.PrepareControl();
            current.MassageAndShowHTML(value);
        }
        #endregion
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
            InternalWB.Opacity = 0;
            InternalWB.LoadCompleted += new LoadCompletedEventHandler((ss, ee) => InternalWB.Opacity = 1);
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
            bw.DoWork += new DoWorkEventHandler((ss, ee) =>
            {
                var index_of_stylesheet = html_doc.IndexOf("/skin/mobile_app.css", StringComparison.InvariantCultureIgnoreCase);
                var index_of_head_ending = html_doc.IndexOf("</head>", StringComparison.InvariantCultureIgnoreCase);

                string foreground = WebForegroundColor.ToString().Substring(3).ToLowerInvariant();
                string base_url = "http://www.guokr.com";
                string stylesheet = string.Format("<style type=\"text/css\"> "
                       + "body {{ background-color: #{0};font-size: {2}px }}" //body styles
                       + "p.document-figcaption{{ font-size: {3}px;font-style:italic;text-align:center}}" // img caption styles
                        + ".article>article,.article > article h1, .article > article h2, .article > article h3 {{color:#{4} }}" //foreground color 1
                        + "a, .fake_a {{color:#{5}}}"//foreground color 2
                       + "</style>",
                    WebBackgroundColor.ToString().Substring(3), foreground, FontSizeTweak(WebFontSize).ToString(), //body style parameters
                    (FontSizeTweak(WebFontSize) - 1).ToString(), //img caption style parameters
                    foreground, foreground // foreground color
                    );

                html_doc = html_doc.Substring(0, index_of_stylesheet)
                    + base_url
                    + html_doc.Substring(index_of_stylesheet, index_of_head_ending - index_of_stylesheet)
                    + stylesheet
                    + html_doc.Substring(index_of_head_ending, html_doc.Length - index_of_head_ending);

                html_doc = ConvertExtendedASCII(html_doc);

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        try
                        {
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

    }
}
