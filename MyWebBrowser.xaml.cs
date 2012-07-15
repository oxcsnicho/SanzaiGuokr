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
        }


        #region SourceUri
        public static readonly DependencyProperty SourceUriProperty = 
            DependencyProperty.Register(
            "SourceUri", typeof(Uri), typeof(MyWebBrowser), null
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
                if (StartNavigating != null)
                {
                    StartNavigating(this, new NavigatingEventArgs() {
                   //     Uri = (Uri)GetValue(SourceUriProperty)
                    });
                }
                var c = new RestClient(value.Scheme+"://"+value.Host);
                var r = new RestRequest();
                r.Resource = value.AbsolutePath + value.Query;
#if false
                r.Resource = value.AbsolutePath;
                foreach (var q in value.Query.Split(new char[] { '?', '&' }))
                {
                    if (string.IsNullOrEmpty(q))
                        continue;
                    var qq = q.Split(new char[] { '=' });
                    if (qq.Length < 2)
                        continue;
                    r.AddParameter(qq[0], qq[1]);
                }
#endif

                c.ExecuteAsync(r, (response) =>
                    {
                        Deployment.Current.Dispatcher.BeginInvoke(() => InternalWB.NavigateToString(MassageHTML(response.Content)));
                    });
            }
        }
        public string MassageHTML(string html_doc)
        {
            var index_of_stylesheet = html_doc.IndexOf("/skin/mobile_app.css", StringComparison.InvariantCultureIgnoreCase);
            var index_of_head_ending = html_doc.IndexOf("</head>",StringComparison.InvariantCultureIgnoreCase);

            string base_url = "http://www.guokr.com";
            string stylesheet = string.Format("<style type=\"text/css\"> "
           + "body {{ background-color: #{0};foreground-color: {1};font-size: {2}px }}" //body styles
           + "p.document-figcaption{{ font-size: {3}px;font-style:italic;text-align:center}}" // img caption styles
           +"</style>",
                WebBackgroundColor.ToString().Substring(3), WebForegroundColor.ToString(), WebFontSize.ToString(), //body style parameters
                (WebFontSize-1).ToString() //img caption style parameters
                );

            html_doc = html_doc.Substring(0, index_of_stylesheet)
                + base_url
                + html_doc.Substring(index_of_stylesheet, index_of_head_ending - index_of_stylesheet)
                + stylesheet
                + html_doc.Substring(index_of_head_ending, html_doc.Length - index_of_head_ending);
            return ConvertExtendedASCII(html_doc);
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
