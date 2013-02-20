using System;
using System.Collections.Generic;
using RestSharp;
using SanzaiGuokr.ViewModel;
using SanzaiGuokr.GuokrObject;
using System.Threading.Tasks;
using System.Windows;
using System.Threading;
using GalaSoft.MvvmLight.Command;

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
                && DateTime.Now < new DateTime(2013, 2, 23))
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
            var t = await GuokrApi.GetLatestPostsV2(page);
            return t;
        }
        protected override bool load_more_item_filter(GuokrPost item)
        {
            item.IsUpdated = false;
            if (map.Count == 0)
                return false;
            else if (!map.ContainsKey(item.title) || item.reply_count > map[item.title])
                item.IsUpdated = true;
            return false;
        }
        protected override void post_load_more()
        {
#if false
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
        protected bool RefreshListCanExecute()
        {
            return Status == StatusType.SUCCESS && ArticleList.Count > 0;
        }
        protected Dictionary<string, int> map = null;
        protected override async Task pre_load_more()
        {
            if (map == null)
                map = new Dictionary<string, int>();
            map.Clear();
            if (ArticleList.Count > 0)
                foreach (var item in ArticleList)
                {
                    map[item.title] = item.reply_count;
                }
        }
        private RelayCommand _rl;
        public RelayCommand RefreshList
        {
            get
            {
                if (_rl == null)
                    _rl = new RelayCommand(() =>
                    {
                        TaskEx.Run(() => load_more(true));
                    }, RefreshListCanExecute);
                return _rl;
            }
        }
    }
}
