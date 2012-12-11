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
using System.Collections.Generic;

namespace SanzaiGuokr.Model
{
    public class article_base : ViewModelBase
    {
        #region url
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
        #endregion

        #region id
        public int id { get; set; }
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

                if (_cm.ArticleList.Count == 0)
                    _cm.load_more();

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

        private new void RaisePropertyChanged(string name)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() => base.RaisePropertyChanged(name));
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
        #region command: GoToNext, GoToPrevious
        public object_list_base<article_base,List<article_base>> parent_list { get; set; }
        private int _order = -1;
        public int order
        {
            get
            {
                if (_order == -1)
                {
                    int _o = parent_list.ArticleList.IndexOf(this);
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
            return order > 0;
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
            if (Status == ArticleStatus.Loading)
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
                        PostLoadArticle();
                    }
                });

            return false;
        }

        protected virtual void PostLoadArticle()
        {
        }
        #endregion

    }
    public class article : article_base
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
        #endregion
        public string minisite_name { get; set; }
        public string pic { get; set; }


        public string FullUrl
        {
            get
            {
                return "http://www.guokr.com/article/" + id.ToString() + "/";
            }
        }

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
                                    var t = MessageBox.Show("准备秒杀之，确定？", "确定秒杀", MessageBoxButton.OKCancel);
                                    if (t == MessageBoxResult.OK || t == MessageBoxResult.Yes)
                                    {
                                        try
                                        {
                                            await GuokrApi.PostComment(this, "杀！");
                                            MessageBox.Show("杀掉了～");
                                        }
                                        catch (Exception ex)
                                        {
                                            MessageBox.Show("杀败了。。momo。。（你没登录吧！还是我出八阿哥了！）");
                                        }
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
        protected override void PostLoadArticle()
        {
            string search_string = @"reply_count=(\d+)";
            var res = Regex.Match(HtmlContent, search_string).Groups;
            if (res.Count >= 0)
                CommentCount = Convert.ToInt32(res[1].Value);
        }
        #endregion


        public new article_list parent_list { get; set; }
    }

}
