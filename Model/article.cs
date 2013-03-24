using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using RestSharp;
using SanzaiGuokr.GuokrObject;
using SanzaiGuokr.Messages;
using SanzaiGuokr.Util;
using SanzaiGuokr.ViewModel;
using HtmlAgilityPack;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Net;
using System.Data.Linq.Mapping;

namespace SanzaiGuokr.Model
{
    public class article_base : GuokrObjectWithId, INotifyPropertyChanged
    {
        #region url
        public string m_url
        {
            get
            {
                return url.Length > 21 ? url.Substring(21) : "";
            }
            set
            {
                //url = value;
            }
        }
        public string url { get; set; }
        private string _wwwurl;
        public string wwwurl
        {
            get
            {
                return _wwwurl;
            }
            set
            {
                _wwwurl = value;
                var m = Regex.Match(value, @"\d+");
                if (m.Success)
                {
                    id = Convert.ToInt64(m.Groups[0].Value);
                    url = "http://api.guokr.com/minisite/article/" + m.Groups[0].Value + ".json";
                }
            }
        }
        public Uri uri
        {
            get
            {
                return new Uri(url, UriKind.Absolute);
            }
        }
        #endregion

        #region title
        public override string ToString()
        {
            return title;
        }
        public string title { get; set; }
        #endregion

        #region comment list
        private comment_list _cm;
        public comment_list CommentList
        {
            get
            {
                if (_cm == null)
                    _cm = new comment_list(this);

#if false
                if (_cm.ArticleList.Count == 0)
                    _cm.load_more();
#endif

                return _cm;
            }
            private set
            { }
        }
        private int _cmcnt = -1;
        private string CommentCountPropertyName = "CommentCount";
        public int CommentCount
        {
            get
            {
                if (_cmcnt == -1)
                    refresh_comment_count();
                return _cmcnt;
            }
            set
            {
                _cmcnt = value;
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        RaisePropertyChanged(CommentCountPropertyName);
                        RaisePropertyChanged(CommentCountFormattedPropertyName);
                        ReadThisArticleComment.RaiseCanExecuteChanged();
                    });
            }
        }
        private string CommentCountFormattedPropertyName = "CommentCountFormatted";
        public virtual string CommentCountFormatted
        {
            get
            {
                return string.Format("评论({0})", CommentCount == -1 ? 0 : CommentCount);
            }
        }
        public async Task refresh_comment_count()
        {
            await TaskEx.Run(async () =>
                {
                    try
                    {
                        await GuokrApi.GetArticleInfo(this);
                    }
                    catch { }
                }
            );
        }
        private RelayCommand _rcc = null;
        public RelayCommand RefreshCommentCount
        {
            get
            {
                if (_rcc == null)
                {
                    _rcc = new RelayCommand(() => refresh_comment_count());
                }
                return _rcc;
            }
        }
        private RelayCommand _rtac = null;
        public RelayCommand ReadThisArticleComment
        {
            get
            {
                if (_rtac == null)
                    _rtac = new RelayCommand(() =>
                        {
                            Messenger.Default.Send<GoToReadArticleComment>(new GoToReadArticleComment() { article = this });
                        }, CanReadComment);
                return _rtac;
            }
        }
        bool CanReadComment()
        {
            return CommentCount >= 0;
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string name)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs(name));
                });
        }

        #region command: readArticle
        protected virtual void _readArticle(article_base a)
        {
            throw new NotImplementedException();
        }

        public RelayCommand ReadThisArticle
        {
            get
            {
                return new RelayCommand(() =>
                    {
                        _readArticle(this);
                    });
            }
        }
        #endregion
        #region loading content
        public enum ArticleStatus
        {
            NotLoaded,
            Loading,
            Loaded
        };

        private static string StatusPropertyName = "Status";
        private ArticleStatus _status = ArticleStatus.NotLoaded;
        public ArticleStatus Status
        {
            get
            {
                return _status;
            }
            private set
            {
                _status = value;
                RaisePropertyChanged(StatusPropertyName);
            }
        }

        private static string HtmlDocContentPropertyName = "HtmlDocContent";
        private HtmlDocument _htmlDoc;
        public HtmlDocument HtmlDocContent
        {
            get
            {
                if (_htmlDoc == null)
                {
                    if (!string.IsNullOrEmpty(HtmlContent))
                    {
                        _htmlDoc = new HtmlDocument();
                        _htmlDoc.LoadHtml(HtmlContent);
                    }
                }
                return _htmlDoc;
            }
            private set
            {
                _htmlDoc = value;
                RaisePropertyChanged(HtmlDocContentPropertyName);
            }
        }

        private static string HtmlContentPropertyName = "HtmlContent";
        private string _html;
        public string HtmlContent
        {
            get
            {
                if (string.IsNullOrEmpty(_html))
                    if (Status == ArticleStatus.NotLoaded)
                        LoadArticle();

                return _html;
            }
            set
            {
                _html = value;
                RaisePropertyChanged(HtmlContentPropertyName);
                RaisePropertyChanged(HtmlDocContentPropertyName);
            }
        }

        public async Task LoadArticle()
        {
            if (Status != ArticleStatus.NotLoaded)
                return;

            Status = ArticleStatus.Loading;
            try
            {
                await _loadArticle();
                Status = ArticleStatus.Loaded;
            }
            catch (Exception e)
            {
#if DEBUG
                Deployment.Current.Dispatcher.BeginInvoke(() => MessageBox.Show(e.Message));
#endif
                Status = ArticleStatus.NotLoaded;
            }
        }
        protected virtual async Task _loadArticle()
        {
            Common.RestSharpLoadDataFromUri(uri, (response) =>
                {
                    if (response.ResponseStatus != ResponseStatus.Completed
                        || response.Content.Length == 0)
                    {
                        Status = ArticleStatus.NotLoaded;
                        HtmlContent = Common.ErrorHtml;
                    }
                    else
                    {
                        Status = ArticleStatus.Loaded;
                        HtmlContent = response.Content;
                        PostLoadArticle();
                    }
                });
        }

        protected virtual async void PostLoadArticle()
        {
        }
        #endregion

    }
    public class article_base<T> : article_base
        where T : article_base<T>
    {
        #region command: GoToNext, GoToPrevious
        public object_list_base<T, List<T>> parent_list { get; set; }
        private int _order = -1;
        public int order
        {
            get
            {
                if (_order == -1)
                {
                    int _o = parent_list.ArticleList.IndexOf((T)this);
                    if (_o >= 0)
                        _order = _o;
                    else { }
                }
                else { }
                return _order;
            }
        }
        bool CanGoToNext()
        {
            return parent_list != null && order < parent_list.ArticleList.Count - 1;
            //return true;
        }
        private RelayCommand _rna = null;
        public RelayCommand ReadNextArticle
        {
            get
            {
                if (_rna == null)
                    _rna = new RelayCommand(() =>
                {
                    var next = parent_list.ArticleList[order + 1];
                    /*
                    if (order == parent_list.ArticleList.Count - 1)
                    {
                        parent_list.ArticleList.Add(parent_list.GetDefaultArticleOfThisList());
                        parent_list.load_more();
                    }
                     */
                    _readArticle(next);
                },
                CanGoToNext);

                return _rna;
            }
        }

        bool CanGoToPrevious()
        {
            return parent_list != null && order > 0;
        }
        private RelayCommand _rpa = null;
        public RelayCommand ReadPreviousArticle
        {
            get
            {
                if (_rpa == null)
                    _rpa = new RelayCommand(() =>
                {
                    var previous = parent_list.ArticleList[order - 1];
                    _readArticle(previous);
                },
                CanGoToPrevious);

                return _rpa;
            }
        }
        #endregion

    }

    public class article : article_base<article>
    {
        #region abstract
        private string _abs;
        public string Abstract
        {
            get
            {
                return _abs;
            }
            set
            {
                _abs = value.TrimEnd(new char[] { '\n', ' ', '\t', '\r' });
            }
        }
        public string ShortAbstract
        {
            get
            {
                if (title.Length < 15)
                    if (Abstract.Length > 25)
                        return Abstract.Substring(0, 25) + "...";
                    else
                        return Abstract;
                else
                    if (Abstract.Length > 10)
                        return Abstract.Substring(0, 10) + "...";
                    else
                        return Abstract;
            }
        }
        #endregion
        public string minisite_name { get; set; }
        public string pic { get; set; }
        public string small_pic
        {
            get
            {
                if (!string.IsNullOrEmpty(pic) && pic.Contains("/image/"))
                {
                    var s = pic.Replace("/image/", "/thumbnail/");
                    s = s.Insert(s.Length - 4, "_130x100");
                    return s;
                }
                else
                    return pic;
            }
        }
        public Uri small_pic_url
        {
            get
            {
                if (!string.IsNullOrEmpty(small_pic))
                    return new Uri(small_pic, UriKind.Absolute);
                return null;
            }
        }
        private BitmapImage _imgsrc;
        public BitmapImage ImgSrc
        {
            get
            {
                if (_imgsrc == null)
                {
                    _imgsrc = new BitmapImage();
                    _imgsrc.CreateOptions = BitmapCreateOptions.BackgroundCreation | BitmapCreateOptions.DelayCreation;
#if false
                    _imgsrc.UriSource = HeadUri;
#endif
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
                    wc.OpenReadAsync(small_pic_url);
                }
                return _imgsrc;
            }
            set
            {
                _imgsrc = value;
                RaisePropertyChanged("ImgSrc");
            }
        }
        protected override void _readArticle(article_base a)
        {
            if (a.GetType() == typeof(article))
                Messenger.Default.Send<GoToReadArticle>(new GoToReadArticle() { article = (article)a });
        }

        public string FullUrl
        {
            get
            {
                return "http://www.guokr.com/article/" + id.ToString() + "/";
            }
        }

        #region new article content
        protected override async Task _loadArticle()
        {
            try
            {
                HtmlContent = await GuokrApi.GetArticleV2(this);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        #endregion

        #region sha
        private RelayCommand _sha;
        public RelayCommand ShaCommand
        {
            get
            {
                if (_sha == null)
                {
                    _sha = new RelayCommand(() =>
                        {
                            Deployment.Current.Dispatcher.BeginInvoke(async () =>
                                {
                                    var t = MessageBox.Show("准备秒杀之，确定？\n\n ψ(╰_╯)σ‧‧☆杀!", "确定秒杀", MessageBoxButton.OKCancel);
                                    if (t == MessageBoxResult.OK || t == MessageBoxResult.Yes)
                                    {
                                        try
                                        {
                                            await GuokrApi.PostCommentV2(this, "杀！");
                                            await GuokrApi.GetArticleInfo(this);
                                        }
                                        catch (Exception ex)
                                        {
                                            MessageBox.Show("杀败了。。momo。。（你没登录吧！还是我出八阿哥了！）");
                                        }
                                        if (this.CommentCount <= 1)
                                            MessageBox.Show("杀掉了～");
                                        else
                                            MessageBox.Show("杀败了。。momo。。手速不够快啊\n（删帖去吧～长按帖子可以删除哦）\n\n ╮(╯▽╰)╭");
                                    }
                                });
                        });
                }
                return _sha;
            }
        }
        bool canExecuteSha()
        {
            return CommentCount == 0;
        }
        #endregion

        #region reply count
        protected override async void PostLoadArticle()
        {
#if false
            string search_string = @"reply_count=(\d+)";
            var res = Regex.Match(HtmlContent, search_string).Groups;
            if (res.Count >= 0)
                CommentCount = Convert.ToInt32(res[1].Value);
#endif
        }
        #endregion
    }

}
