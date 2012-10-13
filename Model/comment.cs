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

namespace SanzaiGuokr.Model
{
    public class comment
    {
        public string nickname { get; set; }
        public bool title_authorized { get; set; }
        public string head_48 { get; set; }
        public DateTime date_create { get; set; }
        public string content { get; set; }

        private Uri _headUri = null;
        public Uri HeadUri
        {
            get
            {
                if (_headUri == null)
                    _headUri = new Uri(head_48);
                return _headUri;
            }
        }
        public string contentHTML
        {
            get
            {
                var bgc = (Color)Application.Current.Resources["DefaultBackgroundColor"];
                return @"<!DOCTYPE HTML>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <title>http://www.guokr.com/?reply_count=55</title>
    <link rel=""stylesheet"" href=""http://www.guokr.com/skin/mobile_app.css?gt"">
    <meta name=""viewport"" content = ""width = device-width, initial-scale = 1, minimum-scale = 1, maximum-scale = 1"" />
" + @"<body><div class=""cmts"" id=""comments"">" + content + "</div></body></html>"; 
            }
        }
    }
}
