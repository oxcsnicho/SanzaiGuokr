using System;
using System.Collections.Generic;
using RestSharp;
using SanzaiGuokr.ViewModel;
using SanzaiGuokr.GuokrObject;
using System.Threading.Tasks;
using System.Windows;
using System.Threading;

namespace SanzaiGuokr.Model
{

    public enum StatusType
    {
        SUCCESS,
        INPROGRESS,
        FAILED,
        UNDERCONSTRUCTION,
        ENDED
    }

    public class article_list : object_list_base<article, List<article>>
    {
        protected RestClient restClient = GuokrApi.WwwClient;
        public article_list()
        {
            ArticleList.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(ArticleListCollectionChanged);
        }
        protected int PageSize
        {
            get
            {
                return ArticleList.Count < 10 ? 4 : 8;
            }
        }
        protected override async System.Threading.Tasks.Task<List<article>> get_data()
        {
            return await GuokrApi.GetLatestArticlesV2(PageSize, ArticleList.Count);
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
                && DateTime.Now < new DateTime(2013, 2, 15))
                return true;
            return false;
        }
        protected override async void post_load_more()
        {
            if (last_last != null)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() => last_last.ReadNextArticle.RaiseCanExecuteChanged());
            }
#if false
            foreach (var item in ArticleList)
            {
                await item.refresh_comment_count();
            }
#endif
        }
    }

    public class minisite_article_list : article_list
    {
        public minisite_article_list(string key)
        {
            minisite_key = key;
        }
        public string minisite_key { get; set; }

        protected override async System.Threading.Tasks.Task<List<article>> get_data()
        {
            return await GuokrApi.GetLatestArticlesV2(minisite_key: this.minisite_key, offset: ArticleList.Count);
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
            return ArticleList.Count <= 0;
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
                && DateTime.Now < new DateTime(2013, 2, 15))
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
                        if (item != null)
                            await item.LoadArticle();
                    }
                });
#endif
        }
    }
}
