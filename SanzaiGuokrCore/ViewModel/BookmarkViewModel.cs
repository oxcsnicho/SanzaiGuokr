using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using SanzaiGuokr.Model;
using System.Threading.Tasks;

namespace SanzaiGuokr.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm/getstarted
    /// </para>
    /// </summary>
    public class BookmarkViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the BookmarkViewModel class.
        /// </summary>
        public BookmarkViewModel()
        {
            if (IsInDesignMode)
            {
                BookmarkList.ArticleList.Add(new article()
                {
                    title = "test article title for bookmark",
                    Abstract = "this is a test article that has been created for bookmark"
                });
            }
            else
            {
                //if (BookmarkList.ArticleList.Count == 0)
                //    TaskEx.Run(() => BookmarkList.load_more());
            }
        }

        /// <summary>
        /// The <see cref="BookmarkList" /> property's name.
        /// </summary>
        public const string BookmarkListPropertyName = "BookmarkList";

        private bookmark_article_list _BMlist = null;
        public bookmark_article_list BookmarkList
        {
            get
            {
                if (_BMlist == null)
                    _BMlist = new bookmark_article_list();
                return _BMlist;
            }
        }

        ////public override void Cleanup()
        ////{
        ////    // Clean own resources if needed

        ////    base.Cleanup();
        ////}
    }
}