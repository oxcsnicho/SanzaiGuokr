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
	NOTLOADED,
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
                return ArticleList.Count < 10 ? 4 : 7;
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
                && DateTime.Now < new DateTime(2013, 7, 11))
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

    public class search_result_article_list : article_list
    {
        public string query { get; set; }
        public GuokrApi.SearchSortOrder sortOrder { get; set; }
        public int page { get; set; }
        protected override async Task<List<article>> get_data()
        {
            return await GuokrApi.SearchArticle(query, sortOrder, page++);
        }
    }

    public class bookmark_article_list : article_list
    {
        public bookmark_article_list() : base() { }

        public BookmarkDataContext Bookmarks
        {
            get
            {
                return BookmarkDataContext.Current;
            }
        }

        protected override async Task<List<article>> get_data()
        {
#if WP8
            return await Task.Run(() => Bookmarks.ReturnBookmarks(offset: ArticleList.Count, count: PageSize));
#else
            return await TaskEx.Run(() => Bookmarks.ReturnBookmarks(offset: ArticleList.Count, count : PageSize));
#endif

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
        protected override bool LoadMoreArticlesCanExecute()
        {
            return ArticleList.Count <= 0;
        }
        protected override async System.Threading.Tasks.Task<List<GuokrPost>> get_data()
        {
            return await GuokrApi.GetLatestPostsV3();
        }
        protected override bool load_more_item_filter(GuokrPost item)
        {
            item.IsUpdated = false;
            // change at submission
            if (DateTime.Now < new DateTime(2013, 7, 11)
                && item.group.name == "性 情")
                return true;
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
#if WP8
                        Task.Run(() => load_more(true));
                        Task.Run(() => GuokrApi.GetRNNumber());
#else
                        TaskEx.Run(() => load_more(true));
                        TaskEx.Run(() => GuokrApi.GetRNNumber());
#endif

                    }, RefreshListCanExecute);
                return _rl;
            }
        }
    }

    public class GuokrPost_list2 : object_list_base<GuokrPost, List<GuokrPost>>
    {
        private int PageSize = 20;
        private int _page = 0;
        public int Page
        {
            get
            {
                return _page;
            }
            set
            {
                _page = value;
                RaisePropertyChanged("Page");
                RaisePropertyChanged("FormattedPage");
            }
        }
        public string FormattedPage
        {
            get
            {
                if (Status == StatusType.FAILED)
                    return "加载失败";
                else if (Status == StatusType.NOTLOADED)
                    return "";
                else
                    return "第" + Page + "页";
            }
        }

        public GuokrPost_list2(int pagesize=20)
        {
            PageSize = pagesize;
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
        protected override bool LoadMoreArticlesCanExecute()
        {
            return ArticleList.Count <= 0;
        }
        protected override async System.Threading.Tasks.Task<List<GuokrPost>> get_data()
        {
            return await GuokrApi.GetLatestPostsV3(offset: PageSize * Page);
        }
        protected override async void post_load_more()
        {
            Page += 1;
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                PreviousPage.RaiseCanExecuteChanged();
                NextPage.RaiseCanExecuteChanged();
            });
        }
        protected bool PreviousPageCanExecute()
        {
            return Status!= StatusType.INPROGRESS && Page > 1;
        }
        private RelayCommand _pp;
        public RelayCommand PreviousPage
        {
            get
            {
                if (_pp == null)
                    _pp = new RelayCommand(() =>
                    {
#if WP8
                        Page -= 2;
                        Task.Run(() => load_more(true));
                        Task.Run(() => GuokrApi.GetRNNumber());
                        GoogleAnalytics.EasyTracker.GetTracker().SendEvent("RelayCommand", "PreviousPage", "GuokrPost_list2", 1);
#else
                        Page-=2;
                        TaskEx.Run(() => load_more(true));
                        TaskEx.Run(() => GuokrApi.GetRNNumber());
                        GoogleAnalytics.EasyTracker.GetTracker().SendEvent("RelayCommand", "PreviousPage", "GuokrPost_list2", 1);
#endif

                    }, PreviousPageCanExecute);
                return _pp;
            }
        }
        protected bool NextPageCanExecute()
        {
            return Status != StatusType.INPROGRESS;
        }
        private RelayCommand _np;
        public RelayCommand NextPage
        {
            get
            {
                if (_np == null)
                    _np = new RelayCommand(() =>
                    {
#if WP8
                        Task.Run(() => load_more(true));
                        Task.Run(() => GuokrApi.GetRNNumber());
                        GoogleAnalytics.EasyTracker.GetTracker().SendEvent("RelayCommand", "NextPage", "GuokrPost_list2", 1);
#else
                        TaskEx.Run(() => load_more(true));
                        TaskEx.Run(() => GuokrApi.GetRNNumber());
                        GoogleAnalytics.EasyTracker.GetTracker().SendEvent("RelayCommand", "NextPage", "GuokrPost_list2", 1);
#endif

                    }, NextPageCanExecute);
                return _np;
            }
        }
    }

}
