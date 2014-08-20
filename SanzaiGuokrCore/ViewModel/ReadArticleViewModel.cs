using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SanzaiGuokr.GuokrObjects;
using SanzaiGuokr.Messages;
using SanzaiGuokr.Model;
using SanzaiGuokr.GuokrObject;

namespace SanzaiGuokr.ViewModel
{
    public class ReadArticleViewModel_Base<T> : ViewModelBase
        where T : article_base, new()
    {
        public ReadArticleViewModel_Base()
        {
            if (IsInDesignMode)
            {
            }
            else
            {
            }
        }
        protected void CreateStaticComments()
        {
            if (the_article != null)
            {
                for (int i = 0; i < 10; i++)
                {
                    the_article.CommentList.ArticleList.Add(new comment()
                    {
                        nickname = "jswxdzc",
                        content = "杀！好牛啊！赞一个",
                        head_48 = "http://img1.guokr.com/gkimage/sh/x9/uu/shx9uu.jpg"
                    });
                }
            }
        }

        #region the_channel
        public const string the_channelPropertyName = "the_channel";

        private channel _ch = null;
        public channel the_channel
        {
            get
            {
                return _ch;
            }

            set
            {
                if (_ch == value)
                {
                    return;
                }

                var oldValue = _ch;
                _ch = value;

                // Update bindings, no broadcast
                RaisePropertyChanged(the_channelPropertyName);
            }
        }
        #endregion
        #region progress indicator

        private RelayCommand _setli;
        public RelayCommand SetLoadingIndicator
        {
            get
            {
                if (_setli == null)
                    _setli = new RelayCommand(() =>
                {
                    LoadingIndicator = true;
                });

                return _setli;
            }
        }
        private RelayCommand _resetli;
        public RelayCommand ResetLoadingIndicator
        {
            get
            {
                if (_resetli == null)
                    _resetli = new RelayCommand(() =>
                {
                    LoadingIndicator = false;
                });

                return _resetli;
            }
        }

        public const string LoadingIndicatorPropertyName = "LoadingIndicator";

        private bool _li = false;

        public bool LoadingIndicator
        {
            get
            {
                return _li;
            }

            set
            {
                if (_li == value)
                {
                    return;
                }

                var oldValue = _li;
                _li = value;

                // Update bindings, no broadcast
                RaisePropertyChanged(LoadingIndicatorPropertyName);
            }
        }

        #endregion

        #region the_article

        public const string the_articlePropertyName = "the_article";
        private T _art;
        public T the_article
        {
            get
            {
                return _art;
            }

            set
            {
                if (_art == value)
                {
                    return;
                }

                var oldValue = _art;
                _art = value;

                // Update bindings, no broadcast
                RaisePropertyChanged(the_articlePropertyName);
                SetLoadingIndicator.Execute(null);
            }
        }

        #endregion

    }

    public class ReadArticleViewModel : ReadArticleViewModel_Base<article>
    {
        public ReadArticleViewModel()
        {
            if (IsInDesignMode)
            {
                the_article = ViewModelLocator.MainStatic.latest_articles[0];
                CreateStaticComments();
            }
            else
            {
                Messenger.Default.Register<GoToReadArticle>(this, (action) =>
                    {
                        the_article = action.article;
                        the_article.LoadArticle();
                    });
            }
        }

        public BookmarkDataContext Context
        {
            get
            {
                return BookmarkDataContext.Current;
            }
        }

        private RelayCommand bmart;
        public RelayCommand BookmarkArticle
        {
            get
            {
                if (bmart == null)
                {
                    bmart = new RelayCommand(() =>
                {
                    Context.InsertBookmarkIfNotExist(the_article);
                    GoogleAnalytics.EasyTracker.GetTracker().SendEvent("RelayCommand", "BookmarkArticle", the_article.object_name, 1);
                });
                }
                return bmart;
            }
        }
    }

    public class ReadPostViewModel : ReadArticleViewModel_Base<GuokrPost>
    {
        public ReadPostViewModel()
        {
            if (IsInDesignMode)
            {
                the_article = ViewModelLocator.MainStatic.latest_posts[0];
                CreateStaticComments();
            }
            else
            {
                Messenger.Default.Register<GoToReadPost>(this, (action) =>
                    {
                        the_article = action.article;
                    });
            }
        }
    }

}