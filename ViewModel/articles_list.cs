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
using System.Collections.ObjectModel;
using RestSharp;
using System.Collections.Generic;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight;
using System.ComponentModel;
using GalaSoft.MvvmLight.Messaging;
using SanzaiGuokr.ViewModel;
using SanzaiGuokr.Messages;
using SanzaiGuokr.Util;

namespace SanzaiGuokr.Model
{

    public enum StatusType
    {
        SUCCESS,
        INPROGRESS,
        FAILED,
        ENDED
    }

    public class article_list : object_list_base<article, List<article>>
    {
        protected RestClient restClient = GuokrApi.Client;
        public article_list()
        {
            ArticleList.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(ArticleListCollectionChanged);
        }
        protected override async System.Threading.Tasks.Task<List<article>> get_data()
        {
            return await GuokrApi.GetLatestArticles(ArticleList.Count);
        }

        void ArticleListCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                foreach (article item in e.NewItems)
                {
                    item.parent_list = this;
                }
        }
        public string Name { get; set; }

        protected override bool load_more_item_filter(article item)
        {
            /* remember to change at submission */
            if (item.minisite_name == "性 情"
                && DateTime.Now < new DateTime(2012, 11, 28))
                return true;
            return false;
        }
        protected override void post_load_more()
        {
            if (ArticleList.Count > 0)
            {
                article last_last = ArticleList[ArticleList.Count - 1];
                last_last.ReadNextArticle.RaiseCanExecuteChanged();
            }
        }
    }

    public class minisite_article_list : article_list
    {
        public minisite_article_list(int id)
        {
            minisite_id = id;
        }
        public int minisite_id { get; set; }

        protected override async System.Threading.Tasks.Task<List<article>> get_data()
        {
            return await GuokrApi.GetMinisiteArticles(minisite_id, ArticleList.Count);
        }
    }
}
