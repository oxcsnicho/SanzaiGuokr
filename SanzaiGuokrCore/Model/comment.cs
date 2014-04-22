using System;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using HtmlAgilityPack;
using SanzaiGuokr.Messages;
using SanzaiGuokr.Model;
using SanzaiGuokr.ViewModel;
using Microsoft.Phone.Tasks;
using SanzaiGuokr.Util;

namespace SanzaiGuokr.GuokrObjects
{
    public class comment : INotifyPropertyChanged
    {
        public override string ToString()
        {
            return string.IsNullOrEmpty(content) ? contentHtml : content;
        }
        public int liking_count { get; set; }
        public string LikingCountFormatted
        {
            get
            {
                return string.Format("顶[{0}]", liking_count);
            }
        }
        private RelayCommand _lc = null;
        public RelayCommand LikeCommand
        {
            get
            {
                if (_lc == null)
                {
                    _lc = new RelayCommand(async () =>
                    {
                        try
                        {
                            await GuokrApi.LikeComment(this);
                            liking_count++;
                            RaisePropertyChanged("liking_count");
                            RaisePropertyChanged("LikingCountFormatted");
                        }
                        catch
                        {
                            MessageBox.Show("你貌似已经顶过一下了亲 (要么就是网不通 =,=)");
                        }
                    }, canExecuteLikeCommand);
                }
                return _lc;
            }
        }
        public bool CanLikeComment
        {
            get
            {
                return ViewModelLocator.ApplicationSettingsStatic.GuokrAccountLoginStatus
                    && this.parent_object_name == "article"
                    && ukey != ViewModelLocator.ApplicationSettingsStatic.GuokrAccountProfile.ukey;
            }
        }
	bool canExecuteLikeCommand()
        {
            return !has_liked;
        }
        public long reply_id { get; set; }
        public string nickname { get; set; }
        public string ukey { get; set; }
        public bool title_authorized { get; set; }
        private string h48;
        public string head_48
        {
            get
            {
                return h48;
            }
            set
            {
                h48 = value;
                if (h48.Contains("/thumbnail"))
                    userPicUrl = h48.Replace("/thumbnail/", "/image/").Replace("_48x48", "");
                else
                    userPicUrl = h48;
            }
        }
        public string date_create { get; set; }
        private string _c;
        public string content
        {
            get
            {
                return _c;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    return;
                _c = Common.TransfromBBCodeToHtml(value).TrimEnd(new char[] {'\n', '\r', ' ', '\t'});
                if (contentHtmlDoc == null) ;
            }
        }
        public int floor { get; set; }

        public Uri UserUri
        {
            get
            {
                if (!string.IsNullOrEmpty(userUrl))
                    return new Uri(userUrl, UriKind.Absolute);
                else
                    return null;
            }
        }
        private Uri _headUri = null;
        public Uri HeadUri
        {
            get
            {
                if (_headUri == null && !string.IsNullOrEmpty(head_48))
                    _headUri = new Uri(head_48, UriKind.Absolute);
                return _headUri;
            }
        }
#if false
        private BitmapImage _imgsrc;
        public BitmapImage ImgSrc
        {
            // todo: not required anymore
            get
            {
                if (_imgsrc == null)
                {
                    _imgsrc = new BitmapImage();
                    _imgsrc.CreateOptions = BitmapCreateOptions.BackgroundCreation | BitmapCreateOptions.DelayCreation;
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
#endif
        public string contentHtml { get; set; }
        private HtmlDocument _chd;
        public HtmlDocument contentHtmlDoc
        {
            get
            {
                if (_chd == null)
                {
                    string s = content;
                    if (string.IsNullOrEmpty(content))
                        s = contentHtml;
                    if (s == null)
                        s = "";
                    _chd = new HtmlDocument();
                    _chd.LoadHtml(s);
                }
                return _chd;
            }
        }
        public string ReferenceContent
        {
            get
            {
                var res = from item in contentHtmlDoc.DocumentNode.ChildNodes
                          where (item.NodeType == HtmlNodeType.Text
                                || item.NodeType == HtmlNodeType.Element && item.Name == "span")
                                && !string.IsNullOrWhiteSpace(item.InnerText)
                          select item.InnerText.Trim(new char[] { '\n', ' ', '\t', '\r' });
                if (res.Count() > 0)
                {
                    var s = res.Aggregate((total, next) => total = total + (next == "" ? "" : " " + next));
                    return s.Length > 100 ? s.Substring(0, 100) + "..." : s;
                }
                else
                    return "";
            }
        }
#if false
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
#endif
        public string FormattedFloor
        {
            get
            {
                return string.Format("{0}楼", floor);
            }
        }
        private DateTime dt_created;
        public DateTime DtDateCreated
        {
            get
            {
                if (dt_created == default(DateTime))
                {
                    var dt = DateTime.Parse(date_create);
                    dt_created = DateTime.SpecifyKind(dt.AddHours(-8), DateTimeKind.Utc);
                }
                return dt_created;
            }
        }

        public string FormattedDateCreated
        {
            get
            {
                return Common.HumanReadableTime(DtDateCreated);
            }
        }
        public string userUrl { get; set; }
        public string userPicUrl { get; set; }

        private RelayCommand _vap;
        public RelayCommand ViewAuthorPicture
        {
            get
            {
                if (_vap == null)
                {
                    _vap = new RelayCommand(() =>
                        {
                            Messenger.Default.Send<ViewImageMessage>(new ViewImageMessage()
                            {
                                small_uri = userPicUrl,
                                med_uri = userPicUrl,
                                large_uri = userPicUrl
                            });
                        });
                }
                return _vap;
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
                Deployment.Current.Dispatcher.BeginInvoke(() => PropertyChanged(this, new PropertyChangedEventArgs(name)));
        }
        #endregion

        #region commands

        private RelayCommand _deletecmd;
        bool canDeleteComment()
        {
            return ViewModelLocator.ApplicationSettingsStatic.GuokrAccountLoginStatus
                && ukey == ViewModelLocator.ApplicationSettingsStatic.GuokrAccountProfile.ukey;
        }
        public RelayCommand DeleteCommentCommand
        {
            get
            {
                if (_deletecmd == null)
                {
                    _deletecmd = new RelayCommand(() => GuokrApi.DeleteCommentV2(this), canDeleteComment);
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

        private RelayCommand _viewuser;
        public RelayCommand ViewUser
        {
            get
            {
                if (_viewuser == null)
                {
                    _viewuser = new RelayCommand(() =>
                        {
                            var t = new WebBrowserTask();
                            t.Uri = UserUri;
                            t.Show();
                        });
                }
                return _viewuser;
            }
        }
        #endregion

        public bool IsReplied
        {
            get
            {
                return ViewModelLocator.MainStatic.NoticeList.IsRelied(reply_id);
            }
        }

        public string url { get; set; }
        public string parent_object_name
        {
            get
            {
                if (url.Contains("article"))
                    return "article";
                else if (url.Contains("post") || url.Contains("blog"))
                    return "post";
                else
                    return "unknown";
            }
        }

        public bool has_liked { get; set; }
    }
}
