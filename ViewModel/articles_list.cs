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

namespace SanzaiGuokr.Model
{

    public enum StatusType
    {
        SUCCESS,
        INPROGRESS,
        FAILED
    }

    public class article_list : object_list_base<article>
    {
        public article_list()
        {
            ArticleList.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(ArticleListCollectionChanged);
            req_resource = "api/content/latest_article_list/";
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

        protected override void PrepareRestParameters(RestRequest req)
        {
            req.Parameters.Add(new Parameter() { Name = "count", Value = 8, Type = ParameterType.GetOrPost });
            req.Parameters.Add(new Parameter() { Name = "offset", Value = ArticleList.Count, Type = ParameterType.GetOrPost });
        }
        protected override bool load_more_item_filter(article item)
        {
            /* remember to change at submission */
            if (item.minisite_name == "性 情"
                && DateTime.Now < new DateTime(2012, 7, 20))
                return true;
            return false;
        }
        protected override void load_more_post_cleanup()
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
        public int minisite_id { get; set; }
        public minisite_article_list(int _id)
        {
            minisite_id = _id;
            req_resource = "api/content/minisite_article_list/";
        }

        protected override void PrepareRestParameters(RestRequest req)
        {
            req.AddParameter(new Parameter() { Name = "minisite_id", Value = minisite_id, Type = ParameterType.GetOrPost });
            base.PrepareRestParameters(req);
        }
    }
}
