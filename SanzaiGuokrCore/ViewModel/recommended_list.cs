using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using SanzaiGuokr.Model;
using System.Collections.Generic;

namespace SanzaiGuokr.ViewModel
{
    public class recommended_list : object_list_base<recommend_article, List<recommend_article>>
    {
        protected override async System.Threading.Tasks.Task<List<recommend_article>> get_data()
        {
            return await GuokrApi.GetRecommendedArticlesV2();
        }

        public recommended_list()
        {
            ArticleList.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(ArticleListCollectionChanged);
        }
        void ArticleListCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                foreach (recommend_article item in e.NewItems)
                {
                    item.parent_list = this;
                }
        }
        protected override bool load_more_item_filter(recommend_article item)
        {
            /* remember to change at submission */
            if (item.minisite_name == "性 情"
                && DateTime.Now < new DateTime(2013, 7, 11))
                return true;
            return false;
        }
        protected override bool LoadMoreArticlesCanExecute()
        {
            return ArticleList.Count <= 0;
        }
    }
}
