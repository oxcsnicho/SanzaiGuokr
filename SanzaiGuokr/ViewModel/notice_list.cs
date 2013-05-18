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
using SanzaiGuokr.GuokrApiV2;
using System.Collections.Generic;
using System.Threading.Tasks;
using SanzaiGuokr.Model;
using System.Text.RegularExpressions;

namespace SanzaiGuokr.ViewModel
{
    public class notice_list : object_list_base<GuokrNotice, List<GuokrNotice>>
    {
        public notice_list()
        {
        }
        protected override bool LoadMoreArticlesCanExecute()
        {
            return ArticleList.Count <= 0;
        }
        protected override async Task<List<GuokrNotice>> get_data()
        {
            return await GuokrApi.GetNoticeV2();
        }
        protected override bool load_more_item_filter(GuokrNotice item)
        {
            var m = Regex.Match(item.url, @"\d+");
            if (m.Success)
                the_ids.Add(Convert.ToInt64(m.Groups[0].Value));
            return base.load_more_item_filter(item);
        }
        private List<long> the_ids = new List<long>();
        public bool IsRelied(long id)
        {
            return the_ids.Contains(id);
        }
    }
}
