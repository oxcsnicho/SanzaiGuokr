using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using SanzaiGuokr.GuokrObjects;
using SanzaiGuokr.Messages;
using SanzaiGuokr.Model;

namespace SanzaiGuokr.ViewModel
{
    public class SearchArticleViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the ViewCommentsViewModel class.
        /// </summary>
        public SearchArticleViewModel()
        {
            if (IsInDesignMode)
            {
                SearchResultList.ArticleList.Add(new article()
                {
                    Abstract = "在民间传言里，大雁从不独活，一只死去，另一只也会郁郁而亡；天鹅十分重视夫妻感情，甚至会为死去的配偶守节；鸳鸯则是永恒的爱情象征，操守好得让人“只羡鸳鸯不羡仙”。不过，在鸟类爱...",
                    title = "忠贞有假？民间传言里鸟类爱情的真相",
                    minisite_name = "自然控",
                    id = 101524,
                    posted_dt = "2012-02-27"
                });
                SearchResultList.ArticleList.Add(new article()
                {
                    Abstract = "怎样做才能在夜晚的骑行中拉风又安全？昂贵的单车？最潮的发型？这些都不如一套LED信号单车骑士服给力。漫漫黑夜，留下一道道炫光背影的你，怎能不羡煞旁人。 DIYer...",
                    title = "暗夜骑士的炫光信号服",
                    minisite_name = "DIY",
                    id = 48208,
                    posted_dt = "2011-06-27"
                });
                SearchResultList.ArticleList.Add(new article()
                {
                    Abstract = "两类野菜还真都出身于蔬菜名门，其后台家族在菜市场上的风头与名动天下的十字花家族和菊家族差不了多少。不过，不同于之前那睹名不知身世故的荠菜和蒲公英，这次介绍的这两位菜蔬大族中的乡...",
                    title = "野菜八卦之豪族浪子——野豌豆和水芹",
                    minisite_name = "自然控",
                    id = 93278,
                    posted_dt = "2012-02-03"
                });
            }
            else
            {
                // Code runs "for real": Connect to service, etc...
            }
        }

        #region articlelist
        private search_result_article_list _al;

        public search_result_article_list SearchResultList
        {
            get
            {
                if (_al == null)
                    _al = new search_result_article_list();
                return _al;
            }
        }

        #endregion

    }
}
