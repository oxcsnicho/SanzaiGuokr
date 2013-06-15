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
                    title = "需要担心自来水中的氯吗？",
                    Abstract = "最近，网上盛传一条信息，宣称蒸食物一定要先开着盖子把水烧开，否则自来水的氯再经过加热后，氯从自来水中挥发出来被包覆在食物上。自来水中为何有氯？这些氯对人又是否有害呢？",
                    IsBookmarked = true
                });
                BookmarkList.ArticleList.Add(new article()
                {
                    title = "要担心唇膏/唇彩里的重金属吗？",
                    Abstract = "上月，加州大学伯克利分校公共卫生学院的研究人员撰写的一份报告[1]引发大家对于唇膏唇彩中重金属的担忧，更有甚者，将二者与“有毒”“致癌”划上等号。唇部彩妆中的重金属需要担心吗？",
                    IsBookmarked = false
                });
                BookmarkList.ArticleList.Add(new article()
                {
                    title = "花露水中有“农药”是怎么回事？",
                    Abstract = "夏天用花露水驱蚊是人们常用的方法，然而花露水里含有“农药”的信息同时也引起人们很多恐慌。花露水中有农药是怎么回事？海外代购的产品会更加安全吗？",
                    IsBookmarked = true
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