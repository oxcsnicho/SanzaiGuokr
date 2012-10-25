using System;
using GalaSoft.MvvmLight.Command;
using SanzaiGuokr.ViewModel;
using System.Windows;
using GalaSoft.MvvmLight.Messaging;
using SanzaiGuokr.Messages;
using System.ComponentModel;
using GalaSoft.MvvmLight;
using SanzaiGuokr.Util;
using RestSharp;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace SanzaiGuokr.Model
{
    public class article : ViewModelBase
    {
        public override string ToString()
        {
            return title;
        }
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
        public int id { get; set; }
        private string _m_url;
        public string m_url
        {
            get
            {
                return _m_url;
            }
            set
            {
                _m_url = value;
            }
        }
        public string minisite_name { get; set; }
        public string pic { get; set; }
        public string title { get; set; }

        public string url
        {
            get
            {
                return "http://m.guokr.com" + m_url;
            }
        }
        public Uri uri
        {
            get
            {
                return new Uri(url, UriKind.Absolute);
            }
        }

        public RelayCommand ReadThisArticle
        {
            get
            {
                return new RelayCommand(() =>
                    {
                        Messenger.Default.Send<GoToReadArticle>(new GoToReadArticle() { article = this });
                    });
            }
        }

        public article_list parent_list { get; set; }
        private int _order = -1;
        public int order
        {
            get
            {
                if(_order == -1)
                {
                    int _o = parent_list.ArticleList.IndexOf(this);
                    if (_o >= 0)
                        _order = _o;
                    else { }
                }
                else {}
                return _order;
            }
        }
        bool CanGoToNext()
        {
            return order < parent_list.ArticleList.Count - 1;
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
                    article next = null;
                    /*
                    if (order == parent_list.ArticleList.Count - 1)
                    {
                        parent_list.ArticleList.Add(parent_list.GetDefaultArticleOfThisList());
                        parent_list.load_more();
                    }
                     */
                    next = parent_list.ArticleList[order + 1];
                    Messenger.Default.Send<GoToReadArticle>(new GoToReadArticle() { article = next });
                },
                CanGoToNext);

                return _rna;
            }
        }

        bool CanGoToPrevious()
        {
            return order > 0;
        }
        private RelayCommand _rpa = null;
        public RelayCommand ReadPreviousArticle
        {
            get
            {
                if(_rpa == null)
                    _rpa = new RelayCommand(() =>
                {
                    article previous = null;
                    if (CanGoToPrevious())
                        previous = parent_list.ArticleList[order - 1];
                    Messenger.Default.Send<GoToReadArticle>(new GoToReadArticle() { article = previous });
                },
                CanGoToPrevious);

                return _rpa;
            }
        }

        public string FullUrl
        {
            get
            {
                return "http://www.guokr.com/article/" + id.ToString() + "/";
            }
        }

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

        private static string HtmlContentPropertyName = "HtmlContent";
        private string _html;
        public string HtmlContent
        {
            get
            {
                if (Status == ArticleStatus.NotLoaded)
                    LoadArticle();

                return _html;
            }
            private set
            {
                _html = value;
                RaisePropertyChanged(HtmlContentPropertyName);
            }
        }

        public bool LoadArticle()
        {
            if (Status == ArticleStatus.Loading ||
                Status == ArticleStatus.Loading)
                return true;

            Status = ArticleStatus.Loading;

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
                        string search_string = @"reply_count=(\d+)";
                        var res = Regex.Match(HtmlContent, search_string).Groups;
                        if (res.Count >= 0)
                            CommentCount = Convert.ToInt32(res[1].Value);
                    }
                });

            return false;
        }
        private new void RaisePropertyChanged(string name) 
        {
            Deployment.Current.Dispatcher.BeginInvoke(() => base.RaisePropertyChanged(name));
        }

        private comment_list _cm;
        public comment_list CommentList
        {
            get
            {
                if (_cm == null)
                    _cm = new comment_list(this);

                if (_cm.ArticleList.Count == 0)
                    _cm.load_more();

                return _cm;
            }
            private set
            { }
        }
        private int _cmcnt=0;
        private string CommentCountPropertyName = "CommentCount";
        public int CommentCount
        {
            get
            {
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
        public string CommentCountFormatted
        {
            get
            {
                return string.Format("评论({0})", CommentCount);
            }
        }

        private RelayCommand _rtac = null;
        public RelayCommand ReadThisArticleComment
        {
            get
            {
                if(_rtac == null)
                _rtac = new RelayCommand(() =>
                    {
                        Messenger.Default.Send<GoToReadArticleComment>(new GoToReadArticleComment() { article = this });
                    }, CanReadComment);
                return _rtac;
            }
        }
        bool CanReadComment()
        {
            return CommentCount > 0;
        }
    }

}
