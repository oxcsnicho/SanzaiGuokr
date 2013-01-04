﻿using System;
using System.Collections.Generic;
using RestSharp;
using SanzaiGuokr.ViewModel;
using SanzaiGuokr.GuokrObject;
using System.Threading.Tasks;

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

    public class GuokrPost_list : object_list_base<GuokrPost, List<GuokrPost>>
    {
        public GuokrPost_list()
        {
            ArticleList.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(ArticleListCollectionChanged);
        }
        void ArticleListCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                foreach (GuokrPost item in e.NewItems)
                {
                    item.parent_list = this;
                }
        }
        int page = 0;
        protected override bool LoadMoreArticlesCanExecute()
        {
            return ArticleList.Count<=0;
        }
        protected override async System.Threading.Tasks.Task<List<GuokrPost>> get_data()
        {
            var t = await GuokrApi.GetLatestPosts(page);
            return t;
        }
        protected override bool load_more_item_filter(GuokrPost item)
        {
            return false;
            /* remember to change at submission */
            if (item.group.name == "性 情"
                && DateTime.Now < new DateTime(2012, 11, 28))
                return true;
            return false;
        }
        protected override void post_load_more()
        {
#if DEBUG
            TaskEx.Run(async () =>
                {
                    foreach (var item in ArticleList)
                    {
                        if(item != null)
                            await item.LoadArticle();
                    }
                });
#endif
        }
    }
}
