using System;
using GalaSoft.MvvmLight.Command;
using SanzaiGuokr.ViewModel;
using System.Windows;
using GalaSoft.MvvmLight.Messaging;
using SanzaiGuokr.Messages;
using System.ComponentModel;
using GalaSoft.MvvmLight;

namespace SanzaiGuokr.Model
{
    public class article
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


    }
}
