﻿using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using GalaSoft.MvvmLight.Command;
using SanzaiGuokr.Model;
using SanzaiGuokr.ViewModel;
using GalaSoft.MvvmLight.Messaging;
using SanzaiGuokr.Messages;
using HtmlAgilityPack;
using System.Linq;

namespace SanzaiGuokr.GuokrObjects
{
    public class comment : INotifyPropertyChanged
    {
        public long reply_id { get; set; }
        public string nickname { get; set; }
        public bool title_authorized { get; set; }
        public string head_48 { get; set; }
        public string date_create { get; set; }
        public string content { get; set; }
        public int floor { get; set; }

        private Uri _headUri = null;
        public Uri HeadUri
        {
            get
            {
                if (_headUri == null)
                    _headUri = new Uri(head_48, UriKind.Absolute);
                return _headUri;
            }
        }
        private BitmapImage _imgsrc;
        public BitmapImage ImgSrc
        {
            get
            {
                if (_imgsrc == null)
                {
                    _imgsrc = new BitmapImage(HeadUri);
                    WebClient wc = new WebClient();
                    wc.Headers["Referer"] = "http://www.guokr.com";
                    wc.OpenReadCompleted += (s, e) =>
                        {
                            try
                            {
                                _imgsrc.SetSource(e.Result);
                                RaisePropertyChanged("ImgSrc");
                            }
                            catch
                            {

                            }
                        };
                    wc.OpenReadAsync(HeadUri);
                    //                    _imgsrc.ImageFailed+=new EventHandler<ExceptionRoutedEventArgs>(_imgsrc_ImageFailed);
                }
                return _imgsrc;
            }
            private set
            {
                _imgsrc = value;
                RaisePropertyChanged("ImgSrc");
            }
        }
        public string ReferenceContent
        {
            get
            {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(content);
                var res = from item in doc.DocumentNode.ChildNodes
                          where item.NodeType == HtmlNodeType.Text && !string.IsNullOrWhiteSpace(item.InnerText)
                          select item.InnerText;
                return res.Aggregate((total, next) => total = total + "\n" + next);
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
        public string FormattedFloor
        {
            get
            {
                return string.Format("{0}楼", floor);
            }
        }
        public DateTime FormattedDateCreated
        {
            get
            {
                var dt = DateTime.ParseExact(date_create, "yyyy-dd-mm hh:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                return dt;
            }
        }

#if false
        void _imgsrc_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            var test = 1;
            test++;
        }
#endif

        #region INotifyPropertyChange
        public event PropertyChangedEventHandler PropertyChanged;
        void RaisePropertyChanged(string name)
        {
            if (PropertyChanged != null)
                Deployment.Current.Dispatcher.BeginInvoke(() => PropertyChanged(this, new PropertyChangedEventArgs("name")));
        }
        #endregion

        #region commands

        private RelayCommand _deletecmd;
        bool canDeleteComment()
        {
            return ViewModelLocator.ApplicationSettingsStatic.GuokrAccountLoginStatus
                && nickname == ViewModelLocator.ApplicationSettingsStatic.GuokrAccountProfile.nickname;
        }
        public RelayCommand DeleteCommentCommand
        {
            get
            {
                if (_deletecmd == null)
                {
                    _deletecmd = new RelayCommand(() => GuokrApi.DeleteComment(this), canDeleteComment);
                }
                return _deletecmd;
            }
            set { _deletecmd = value; }
        }
        public bool CanDelete
        {
            get
            {
                return canDeleteComment();
            }
        }

        private RelayCommand _referencecmd;

        public RelayCommand ReferenceCommentCommand
        {
            get
            {
                if (_referencecmd == null)
                {
                    _referencecmd = new RelayCommand(() => Messenger.Default.Send<ReferenceCommentMessage>(new ReferenceCommentMessage() { comment = this }));
                }
                return _referencecmd;
            }
            set { _referencecmd = value; }
        }

        #endregion

    }
}
