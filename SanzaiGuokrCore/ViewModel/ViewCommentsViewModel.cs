using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using SanzaiGuokr.GuokrObjects;
using SanzaiGuokr.Messages;
using SanzaiGuokr.Model;

namespace SanzaiGuokr.ViewModel
{
    public class ViewCommentsViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the ViewCommentsViewModel class.
        /// </summary>
        public ViewCommentsViewModel()
        {
            if (IsInDesignMode)
            {
                this.the_article = new article();
                this.the_article.CommentList.ArticleList.Add(new comment()
                {
                    head_48 = "http://img1.guokr.com/thumbnail/S2j_UySh7oYg8BN7epTyv9zs9_dJ0M5brUQGMjrKDm4wAAAAMAAAAFBO_48x48.png",
                    nickname = "nicholas",
                    content = "This is a very long piece of text. This is a very long piece of text. This is a very long piece of text. This is a very long piece of text. ",
                    date_create = "2013-07-27T23:36:21.527751+08:00",
                    liking_count = 1
                });
                this.the_article.CommentList.ArticleList.Add(new comment()
                {
                    head_48 = "http://img1.guokr.com/thumbnail/5lr4VactYHyMfr8dwccVs1-7_E0DCl2xcnjnKkhrbSagAAAAoAAAAEpQ_48x48.jpg",
                    nickname = "yangyang",
                    content = "This is a very short piece of shit.",
                    date_create = "2013-07-27T23:36:21.527751+08:00",
                    liking_count = 0
                });
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
            Messenger.Default.Register<GoToReadArticleComment>(this, (action) =>
            {
                the_article = action.article;
            });
        }

        #region the_article

        /// <summary>
        /// The <see cref="the_article" /> property's name.
        /// </summary>
        public const string the_articlePropertyName = "the_article";

        private article_base _art;
        /// <summary>
        /// Gets the the_article property.
        /// TODO Update documentation:
        /// Changes to that property's value raise the PropertyChanged event. 
        /// This property's value is broadcasted by the Messenger's default instance when it changes.
        /// </summary>
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
            }
        }

        #endregion


        ////public override void Cleanup()
        ////{
        ////    // Clean own resources if needed

        ////    base.Cleanup();
        ////}
    }
}
