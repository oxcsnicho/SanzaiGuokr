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
                    head_48 = "http://img1.guokr.com/gkimage/n5/m9/8v/n5m98v.jpg",
                    nickname = "nicholas",
                    content = "1235"
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
        }

        #region the_article

        /// <summary>
        /// The <see cref="the_article" /> property's name.
        /// </summary>
        public const string the_articlePropertyName = "the_article";

        private article _art;
        /// <summary>
        /// Gets the the_article property.
        /// TODO Update documentation:
        /// Changes to that property's value raise the PropertyChanged event. 
        /// This property's value is broadcasted by the Messenger's default instance when it changes.
        /// </summary>
        public article the_article
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
