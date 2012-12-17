using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SanzaiGuokr.GuokrObjects;
using SanzaiGuokr.Messages;
using SanzaiGuokr.Model;

namespace SanzaiGuokr.ViewModel
{
    public class ReadArticleViewModel : ViewModelBase
    {
        public ReadArticleViewModel()
        {
            if (IsInDesignMode)
            {
                the_article = ViewModelLocator.MainStatic.latest_articles[0];
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
            else
            {
                // Code runs "for real": Connect to service, etc...
            }

            Messenger.Default.Register<GoToReadArticle>(this, (action) =>
                {
                    the_article = action.article;
                });
            Messenger.Default.Register<GoToReadPost>(this, (action) =>
                {
                    the_article = action.article;
                });
        }

        #region the_channel
        /// <summary>
        /// The <see cref="the_channel" /> property's name.
        /// </summary>
        public const string the_channelPropertyName = "the_channel";

        private channel _ch = null;

        /// <summary>
        /// Gets the the_channel property.
        /// TODO Update documentation:
        /// Changes to that property's value raise the PropertyChanged event. 
        /// This property's value is broadcasted by the Messenger's default instance when it changes.
        /// </summary>
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

        #region the_article

        public const string the_articlePropertyName = "the_article";
        private article_base _art;
        public article_base the_article
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

    }
}