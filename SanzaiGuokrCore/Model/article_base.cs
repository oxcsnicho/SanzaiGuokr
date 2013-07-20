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
        #region author
        public GuokrUser Author { get; set; }
        public string posted_dt { get; set; }
        public DateTime PostedDateTime
        {
            get
            {
                return DateTime.Parse(posted_dt);
            }
        }
        #endregion

        #region url
        private void SetIdFromString(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                var match = Regex.Match(value, @"\d+");
                if (match.Success && !string.IsNullOrEmpty(match.Value))
                    id = Convert.ToInt64(match.Value);
            }
        }

        public string m_url
        {
            get
            {
                return uri.AbsolutePath;
            }
            set
            {
                SetIdFromString(value);
            }
        }
        protected virtual string GetUrlFromId()
        {
            if (id == 0)
                throw new ArgumentOutOfRangeException();
            return "http://www.guokr.com/apis/minisite/article/" + id.ToString() + ".json";
        }
        private string _url;
        public string url
        {
            get
            {
                if (!string.IsNullOrEmpty(_url))
                    return _url;
                else if (id > 0)
                    return GetUrlFromId();
                else
                    return "";
            }
            set
            {
                _url = value;
                SetIdFromString(_url);
            }
        }
        private string _wwwurl;
        public string wwwurl
        {
            get
            {
                return "http://www.guokr.com/" + object_name + "/" + id.ToString() + "/";
            }
            set
            {
                _wwwurl = value;
                SetIdFromString(_wwwurl);
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

#if WEAKREFERENCE
        private WeakReference<comment_list> _cmref;
        public comment_list CommentList
        {
            get
            {
                if (_cmref == null)
                    _cmref = new WeakReference<comment_list>(null);

                comment_list _cm = null;
                if (!_cmref.TryGetTarget(out _cm) || _cm == null)
                {
                    _cm = new comment_list(this);
                    _cmref.SetTarget(new comment_list(this));
                }
                return _cm;
            }
            private set
            {
                _cmref.SetTarget(value);
            }
        }
#else
        private comment_list _cm;
        public comment_list CommentList
        {
            get
            {
                if (_cm == null)
                    _cm = new comment_list(this);
                return _cm;
            }
            private set
            { }
        }
#endif
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
                        RaisePropertyChanged("SubTitle");
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
#if WP8
            await Task.Run(async () =>
#else
            await TaskEx.Run(async () =>
#endif
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
#if WP8
                await Task.Run(async () => await _loadArticle());
#else
                await TaskEx.Run(async () => await _loadArticle());
#endif
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

        #region parent name
        public virtual string parent_name
        {
            get
            {
                return "parent_name not defined";
            }
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
            set
            {
                _rna = value;
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


}
